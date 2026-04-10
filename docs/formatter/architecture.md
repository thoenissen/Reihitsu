# Reihitsu Formatter — Technical Architecture Concept

> **Version**: Draft 1.0  
> **Status**: Under development — architecture is being refined collaboratively  
> **Audience**: Developers implementing the formatter

---

## Table of Contents

1. [Overview & Goals](#1-overview--goals)
2. [Problems with the Current Architecture](#2-problems-with-the-current-architecture)
3. [Proposed Architecture: Compute-Then-Apply](#3-proposed-architecture-compute-then-apply)
4. [Pipeline Design](#4-pipeline-design)
5. [Phase 0: Structural Transforms](#5-phase-0-structural-transforms)
6. [Phase 1: Region Formatting](#6-phase-1-region-formatting)
7. [Phase 2: Blank Lines](#7-phase-2-blank-lines)
8. [Phase 3: Line Breaks](#8-phase-3-line-breaks)
9. [Phase 4: Switch Case Braces](#9-phase-4-switch-case-braces)
10. [Phase 5: Horizontal Spacing](#10-phase-5-horizontal-spacing)
11. [Phase 6: Indentation & Alignment](#11-phase-6-indentation--alignment)
12. [Phase 7: Cleanup](#12-phase-7-cleanup)
13. [Formatting Scopes — Solving the Lambda/Initializer Problem](#13-formatting-scopes--solving-the-lambdainitializer-problem)
14. [Handling Continuation-Line Alignment](#14-handling-continuation-line-alignment)
15. [Public Interface](#15-public-interface)
16. [Testing Strategy](#16-testing-strategy)
17. [Open Questions & Design Decisions](#17-open-questions--design-decisions)

---

## 1. Overview & Goals

The Reihitsu Formatter is a non-configurable C# code formatter built on the Roslyn Compiler Platform. It enforces a fixed set of formatting rules (see [Formatting Rules Reference](formatting-rules.md)).

### Design Goals

1. **Correctness**: Formatting must be semantically neutral — no behavioral changes.
2. **Idempotency**: `format(format(input)) == format(input)` — always.
3. **Predictability**: Given a syntax tree, the formatted output is deterministic.
4. **Maintainability**: Rules are isolated, testable, and independently debuggable.
5. **Performance**: Acceptable for real-time use (format-on-save, IDE integration).
6. **Completeness**: Handles all C# constructs, including deeply nested lambdas, initializers, and continuation lines.

### Non-Goals

- User configuration (the formatter is opinionated and non-configurable).
- Semantic analysis (no type information needed — operates purely on syntax trees).

### Future Goals

- **Sorting** (e.g., using directive ordering, member ordering): Not in scope for the initial implementation. When formatting complete documents, member sorting could be a valuable addition. The pipeline architecture supports this — sorting would be a Phase 0 transform (before structural changes) or a separate pre-phase. The scope system and phase isolation should make this straightforward to add later.

---

## 2. Problems with the Current Architecture

### 2.1 The CSharpSyntaxRewriter Double-Formatting Problem

The current formatter uses `CSharpSyntaxRewriter` (visitor pattern) which traverses **depth-first, bottom-up**:

```
Tree:   ArgumentList → Argument → LambdaExpression → BinaryExpression (&&)
Visit:  BinaryExpr ← Lambda ← Argument ← ArgumentList
```

1. `VisitBinaryExpression` positions `&&` correctly relative to its left operand.
2. `VisitArgumentList` later shifts ALL tokens in the argument — including the already-positioned `&&`.
3. Result: `&&` is shifted TWICE — once by its own visitor, once by the parent.

**Root Cause**: In a visitor-based rewriter, each visitor both **reads** token positions (to compute alignment) and **writes** new positions. When an outer visitor processes a subtree that was already rewritten by inner visitors, position information is stale.

### 2.2 The Monolith Problem

`IndentationAndAlignmentRule.cs` is 3700+ lines handling:
- Block indentation
- Argument alignment
- Method chain alignment
- Object initializer layout
- Collection expression layout
- Switch expression layout
- Conditional expression layout
- Logical operator alignment
- Lambda body shifting
- Constructor initializer placement
- Generic constraint placement
- Base type list alignment
- Anonymous object layout

These concerns are tightly coupled through shared mutable state (`_pendingChainAlignmentShifts`, `_parentAssignmentWhitespaceStack`), making it impossible to modify one behavior without risking regressions in others.

### 2.3 Position-Based State Transfer

The current implementation stores computed alignment shifts in dictionaries keyed by `SpanStart`:

```csharp
_pendingChainAlignmentShifts[argumentList.SpanStart] = delta;
// ... later in a different visitor ...
if (_pendingChainAlignmentShifts.TryGetValue(node.SpanStart, out var shift)) ...
```

If any intermediate rewrite changes the tree structure (adds/removes tokens), `SpanStart` values change and the lookup fails silently — producing subtle formatting bugs.

### 2.4 Cascading Realignments

When a method chain is realigned:
1. Each chain link shifts → 2. Continuation tokens inside invocations shift → 3. Pending shifts stored for argument lists → 4. Argument lists shift → 5. Lambda bodies inside arguments shift → 6. Chains inside lambdas may shift again...

There is no single source of truth for "what column should this token be at." Each level computes locally with implicit dependencies on all other levels.

---

## 3. Proposed Architecture: Compute-Then-Apply

### Core Insight

The fundamental problem is that the current architecture **computes** alignment and **applies** it in the same traversal. This means:
- Reads and writes are interleaved.
- Inner computations are invalidated by outer writes.
- Position tracking is fragile.

**Solution**: Separate the process into two distinct phases:

1. **Compute Phase**: Walk the tree (top-down) and determine, for every first-on-line token, its **desired column** and **desired preceding vertical whitespace** (blank lines, line breaks). Store this as a layout model — NOT as token modifications.

2. **Apply Phase**: Walk the token stream once and rewrite leading trivia to match the computed layout model. No alignment computation happens here — it's purely mechanical.

### Why This Solves the Problems

| Problem | How Compute-Then-Apply Solves It |
|---------|----------------------------------|
| Double formatting | Computation happens once per token (top-down). No re-computation. |
| Position staleness | Layout model uses tree-relative references (parent/child relationships), not absolute positions. |
| Cascading shifts | Parent scopes compute child positions directly. No implicit state transfer. |
| Monolith | Computation logic can be split into independent "layout contributors" per construct type. |

---

## 4. Pipeline Design

```
┌────────────────────┐
│  Input SyntaxNode   │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│ Phase 0: Structural │  Expression body → Block body
│ Transforms          │  (methods, constructors, operators, indexers,
│                     │   conversion operators, finalizers, local functions)
└────────┬───────────┘
         │  (Tree structure is now final — except for switch case braces)
         ▼
┌────────────────────┐
│ Phase 1: Region     │  Capitalize region names
│ Formatting          │  Sync #endregion comments
└────────┬───────────┘
         │  (Region trivia is settled)
         ▼
┌────────────────────┐
│ Phase 2: Blank      │  Insert blank lines before statements
│ Lines               │  Insert blank lines before comments
│                     │  Insert blank lines after break
│                     │  Enforce max 1 consecutive blank line
└────────┬───────────┘
         │  (Excessive blank lines removed — accurate line counting)
         ▼
┌────────────────────┐
│ Phase 3: Line       │  Brace placement (Allman style)
│ Breaks              │  Argument wrapping / chain link collapsing
│                     │  Ternary ?/: placement
│                     │  Binary operator position (end→begin)
│                     │  Constructor initializer / generic constraint placement
│                     │  Expression-bodied property → collapse to single line
└────────┬───────────┘
         │  (Line structure is now frozen — we know which tokens start new lines)
         ▼
┌────────────────────┐
│ Phase 4: Switch     │  Detect multi-line cases (using stable line info)
│ Case Braces         │  Add { } to ALL cases if any is multi-line
│                     │  Remove { } if all cases are single-line
│                     │  Place break outside braces
└────────┬───────────┘
         │  (Switch case structure is finalized)
         ▼
┌────────────────────┐
│ Phase 5: Horizontal │  Operator / comma / semicolon spacing
│ Spacing             │  Keyword spacing
│                     │  Parenthesis spacing
└────────┬───────────┘
         │  (Intra-line token positions are settled)
         ▼
┌────────────────────┐
│ Phase 6: Indent-    │  Block indentation (nesting × 4 spaces)
│ ation & Alignment   │  Continuation-line alignment (arguments, chains,
│                     │  binary operators, initializers, ternary, etc.)
│                     │  Comment indentation (match code below)
└────────┬───────────┘
         │  (Leading whitespace is settled)
         ▼
┌────────────────────┐
│ Phase 7: Cleanup    │  Remove trailing whitespace
│                     │  Remove whitespace from blank lines
│                     │  Handle EOF (no trailing newline)
│                     │  Preserve BOM
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  Output SyntaxNode  │
└────────────────────┘
```

### Why This Phase Order?

The pipeline follows a strict dependency chain where each phase's output is a prerequisite for the next:

1. **Structural transforms first** (Phase 0): Changes the tree shape (adds `{ }` blocks, removes `=>`). All subsequent phases need a stable tree structure (with one exception: switch case braces in Phase 4).

2. **Region formatting second** (Phase 1): Modifies directive trivia (capitalizes names, adds endregion comments). This must happen before blank line and line break computation to ensure stable trivia.

3. **Blank lines third** (Phase 2): Normalizes vertical whitespace — collapses excessive blank lines, inserts required blank lines before statements and comments. Runs **before** line breaks so that when Phase 3 establishes the line structure, excessive blank lines don't inflate line counts (critical for Phase 4's multi-line detection in switch cases).

4. **Line breaks fourth** (Phase 3): Determines WHERE line breaks should be — Allman brace placement, argument wrapping, operator positions, etc. After this phase, the set of "first-on-line" tokens is frozen.

5. **Switch case braces fifth** (Phase 4): Adds or removes `{ }` around switch case sections based on multi-line detection. This is a structural change that depends on stable line counts from Phase 3. The phase produces braces with correct line breaks (Allman style). `break` is placed outside the braces.

6. **Horizontal spacing sixth** (Phase 5): Normalizes spaces WITHIN lines (e.g., `a+b` → `a + b`). Since Phase 6's alignment depends on accurate column positions, spacing must run first.

7. **Indentation & alignment seventh** (Phase 6): Computes leading whitespace for every first-on-line token. By now, line structure, blank lines, switch braces, and intra-line spacing are all settled.

8. **Cleanup last** (Phase 7): Final pass to remove trailing whitespace, collapse leftover blank lines, and handle end-of-file. Must be last because earlier phases may introduce trailing whitespace as a side effect.

---

## 5. Phase 0: Structural Transforms

Phase 0 uses the existing `CSharpSyntaxRewriter` approach — which works well for structural transforms since they don't have the double-formatting problem (they change tree structure, not trivia).

### Rules in This Phase

| Rule | Implementation |
|------|---------------|
| Expression-bodied methods → block body | `ExpressionBodiedMethodTransform` |
| Expression-bodied constructors → block body | `ExpressionBodiedConstructorTransform` |
| Expression-bodied operators → block body | `ExpressionBodiedOperatorTransform` |
| Expression-bodied indexers → block body | `ExpressionBodiedIndexerTransform` |
| Expression-bodied conversion operators → block body | `ExpressionBodiedConversionTransform` |
| Expression-bodied finalizers → block body | `ExpressionBodiedFinalizerTransform` |
| Expression-bodied local functions → block body | `ExpressionBodiedLocalFunctionTransform` |

Each transform is a **separate `CSharpSyntaxRewriter`**. This keeps each transform focused, independently testable, and easy to debug. The phase orchestrator runs them sequentially.

### Key Principle

After Phase 0 completes, the **tree structure is frozen**. No subsequent phase may add, remove, or rearrange syntax nodes. All remaining phases operate exclusively on **trivia** (whitespace, comments, preprocessor directives).

---

## 6. Phase 1: Region Formatting

Phase 1 normalizes region directives to ensure consistent naming and endregion comments. This runs before line break computation to ensure all directive trivia is settled.

### Rules in This Phase

| Rule | Implementation |
|------|---------------|
| Region name capitalization | `RegionNameCapitalizationRule` |
| Endregion comment synchronization | `EndregionCommentSyncRule` |

These can be combined into a single `RegionFormattingRule` (similar to the current implementation).

---

## 7. Phase 2: Blank Lines

Phase 2 normalizes vertical whitespace. It runs **before** line breaks so that excessive blank lines don't inflate line counts when Phase 4 detects multi-line switch cases.

### Rules in This Phase

| Rule | What It Does |
|------|-------------|
| Blank line before statements | Inserts blank line before `if`, `try`, `return`, `foreach`, etc. |
| Blank line after break | Inserts blank line after `break` (unless last in block) |
| Blank line before comments | Inserts blank line before comments (unless first in block) |
| Max consecutive blank lines | Collapses 3+ consecutive blank lines to 1 |

### Exceptions (No Blank Line Inserted)

- First statement in a block (preceded by `{`)
- First comment in a block (preceded by `{`)
- `if` preceded by `else` (else-if chains)
- `break` inside switch sections (no leading blank line when after `}` or single statement)
- `yield` following another `yield`

---

## 8. Phase 3: Line Breaks

Phase 3 determines WHERE line breaks should be placed. After this phase, the set of "first-on-line" tokens is frozen.

### Rules in This Phase

| Rule | What It Does |
|------|-------------|
| Brace placement (Allman) | Ensures `{` and `}` are on dedicated lines |
| First member/statement after `{` | Forces new line after opening brace |
| Argument wrapping | Collapses first argument to same line as `(` |
| Chain link collapsing | Collapses first chain link to same line as root |
| Ternary `?`/`:` placement | Places `?` and `:` on new lines for multi-line ternaries |
| Constructor initializer placement | Places `: base()`/`: this()` on new line |
| Generic constraint placement | Places `where` clauses on new lines |
| Expression-bodied property collapse | Collapses multi-line `=>` properties to single line |
| Binary operator position | Moves operators from end-of-line to beginning of next line |

### Binary Operator Position Normalization

When a binary operator (`&&`, `||`, `+`, `-`, `??`, etc.) appears at the **end** of a line rather than the **beginning** of the next line, this rule moves it:

```csharp
// Before (operator at end of line)
if (conditionA &&
    conditionB)

// After (operator at beginning of next line)
if (conditionA
    && conditionB)
```

This normalization is critical because Phase 6's alignment computation assumes operators start continuation lines.

### Implementation Approach

This phase uses `CSharpSyntaxRewriter` to add/remove `EndOfLineTrivia` in leading/trailing trivia. The rewriter makes a single pass over the tree, handling each construct type in its respective `Visit*` method.

**Key Principle**: This phase only adds/removes **line breaks** (`EndOfLineTrivia`). It does NOT set indentation (leading whitespace). That is Phase 6's responsibility.

---

## 9. Phase 4: Switch Case Braces

Phase 4 handles the conditional addition/removal of `{ }` around switch case sections. This is a **structural change** that depends on stable line counts from Phase 3.

### Logic

1. For each `switch` statement, iterate over all `case`/`default` sections.
2. Count the number of lines each section occupies (excluding the `case` label itself and the `break`).
3. If **any** section occupies more than one line → add `{ }` to **all** sections.
4. If **all** sections occupy exactly one line → remove `{ }` from all sections.

### Brace Placement

When braces are added:
- `{` is placed on a new line, indented **+1 level** from the `case` label.
- Content inside braces is indented **+1 level** from the braces.
- `}` is placed on a new line, aligned with `{`.
- `break` is placed **outside** the braces, at the same indent level as `{`.

```csharp
switch (x)
{
    case 1:
        {
            DoA();
            DoB();
        }
        break;

    case 2:
        {
            DoC();
        }
        break;
}
```

### Implementation Note

This phase produces the `BlockSyntax` nodes with correct line break trivia (Allman style) since Phase 3 has already run. The indentation will be set by Phase 6.

---

## 10. Phase 5: Horizontal Spacing

Phase 5 normalizes horizontal spacing within lines. This must run before Phase 6 (Indentation) because spacing changes can shift token column positions, which affects alignment computation.

### Rules in This Phase

| Rule | What It Does |
|------|-------------|
| Operator spacing | Single space around binary/assignment operators |
| Comma spacing | Single space after commas (except array rank specifiers) |
| Semicolon spacing | Single space after semicolons in for-loops |
| Keyword spacing | Single space after `if`, `for`, `while`, etc. |
| Parenthesis spacing | Remove space after `(` and before `)` |
| Multi-space normalization | Collapse multiple spaces to single space |

### Why Before Indentation?

Example: `var x=new Foo { ... }` → After spacing: `var x = new Foo { ... }`

The `new` keyword shifted by 2 columns. If the initializer aligns its braces to the `new` keyword column, Phase 6 needs to see the post-spacing position to compute the correct column.

---

## 11. Phase 6: Indentation & Alignment

This is the core phase that replaces the monolithic `IndentationAndAlignmentRule`. It uses the **compute-then-apply** approach: first compute the desired column for every first-on-line token (via layout contributors), then apply those columns in a single mechanical trivia rewrite.

### 11.1 The Layout Model

The layout model is a mapping from each first-on-line token to its desired indentation column. Since line structure is frozen after Phase 3, the **line number** is a natural and efficient key:

```csharp
internal sealed class LayoutModel
{
    // Key: Line number (0-based)
    // Value: The desired indentation column for the first token on that line
    private readonly Dictionary<int, TokenLayout> _layouts;
}

internal readonly struct TokenLayout
{
    /// Desired column (0-based) for this token's first character.
    public int Column { get; }
}
```

Using line numbers as keys is efficient because lines are frozen after Phase 3 — no phase between 3 and 6 adds or removes line breaks (Phase 4 adds braces WITH their own line breaks, which are stable from that point).

### 11.2 Layout Contributors

Instead of one monolithic class, the computation is split into focused **layout contributors**. Each contributor is responsible for one type of formatting concern:

```csharp
internal interface ILayoutContributor
{
    /// Compute layout instructions for tokens within the given node.
    /// Contributors are called top-down (parent before children).
    /// A contributor may set layout for any descendant token.
    void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model);
}
```

**Proposed contributors** (Phase 6 only — line breaks, blank lines, and spacing are handled by earlier phases):

| Contributor | Responsibility |
|-------------|---------------|
| `ArgumentAlignmentContributor` | Argument/parameter list alignment |
| `MethodChainAlignmentContributor` | Method chain dot alignment |
| `ObjectInitializerContributor` | Object/collection initializer layout |
| `CollectionExpressionContributor` | C# 12+ collection expression layout |
| `BinaryExpressionContributor` | All binary operator alignment (`&&`, `||`, `??`, `+`, `-`, etc.) |
| `ConditionalExpressionContributor` | Ternary `?`/`:` alignment |
| `SwitchExpressionContributor` | Switch expression arm layout |
| `ConstructorInitializerContributor` | `: base()`/`: this()` alignment |
| `GenericConstraintContributor` | `where` clause alignment |
| `BaseTypeListContributor` | Base type/interface list alignment |
| `AnonymousObjectContributor` | Anonymous type layout |
| `CommentIndentationContributor` | Align comments to the code they precede |

### 11.3 Top-Down Traversal

Layout computation uses a **top-down traversal** (parent nodes processed before children), which is the opposite of `CSharpSyntaxRewriter`'s bottom-up approach.

```csharp
internal static class LayoutComputer
{
    public static LayoutModel Compute(SyntaxNode root, FormattingContext context)
    {
        var model = new LayoutModel();
        var rootScope = new FormattingScope(baseIndent: context.BaseIndentLevel);

        // Step A: Compute block indentation for all first-on-line tokens
        ComputeBlockIndentation(root, rootScope, model);

        // Step B: Apply alignment overrides (continuation lines)
        // Each contributor may override the block indentation for specific tokens
        foreach (var contributor in GetAlignmentContributors())
        {
            ApplyContributor(root, contributor, rootScope, model);
        }

        // Step C: Align comments to the code they precede
        AlignComments(root, model);

        return model;
    }
}
```

### 11.4 Priority / Override Rules

When multiple contributors want to set the column for the same token, a priority system resolves conflicts:

1. **Alignment contributors override block indentation**. If `ArgumentAlignmentContributor` says token X should be at column 20, that takes precedence over the block indentation's column 12.
2. **More specific contributors override less specific ones**. A chain inside an argument list: the chain contributor's alignment overrides the argument contributor's alignment.
3. **The layout model tracks the "source" contributor** for debugging.

---

### 11.5 Trivia Rewrite (Apply Step)

After the layout model is computed, a single pass over all tokens applies the indentation:

```csharp
internal static class IndentationRewriter
{
    public static SyntaxNode Apply(SyntaxNode root, LayoutModel model, FormattingContext context)
    {
        return root.ReplaceTokens(
            root.DescendantTokens(),
            (original, _) =>
            {
                if (model.TryGetLayout(original, out var layout) == false)
                {
                    return original;
                }

                var newLeadingTrivia = BuildLeadingTrivia(original, layout, context);

                return original.WithLeadingTrivia(newLeadingTrivia);
            });
    }
}
```

### Key Principle

The trivia rewrite is **purely mechanical**. It does not compute any alignment — it only reads the layout model and produces the corresponding trivia (preserving existing line breaks and blank lines, only replacing the whitespace portion of leading trivia).

---

## 12. Phase 7: Cleanup

The final phase handles concerns that are independent of formatting structure and operate on the already-formatted tree.

### Rules in This Phase

| Rule | Responsibility |
|------|---------------|
| `TrailingWhitespaceRule` | Remove trailing whitespace from all lines |
| `ConsecutiveBlankLineRule` | Collapse multiple consecutive blank lines to at most one |
| `EndOfFileRule` | Ensure file does not end with a trailing newline (SA1518) |
| `BomPreservationRule` | Preserve existing BOM if present |

These rules use the `CSharpSyntaxRewriter` approach, which works well because they operate on **local** trivia (within a single token or adjacent pair) and don't have cascading alignment problems.

---

## 13. Formatting Scopes — Solving the Lambda/Initializer Problem

### The Problem

In the current implementation, lambdas and initializers create new "root" indentation contexts that bypass structural indentation computation. This is handled through stack-based state management, which is fragile and hard to debug.

### The Solution: Formatting Scopes

A **FormattingScope** represents an indentation context. It carries:
- The **base column** for content within this scope
- The **parent scope** (for walking up the scope chain)
- Whether this scope is a **continuation scope** (alignment-based rather than indent-based)

```csharp
internal sealed class FormattingScope
{
    public FormattingScope Parent { get; }
    public int BaseColumn { get; }
    public ScopeKind Kind { get; }
}

internal enum ScopeKind
{
    /// Standard block indentation (namespace, class, method body, etc.)
    Block,

    /// Continuation alignment (arguments, chain, initializer)
    Continuation,

    /// Lambda body — creates a new indentation root
    LambdaBody,

    /// Initializer — creates a new indentation root
    Initializer
}
```

### How Scopes Solve the Lambda Problem

When the layout computer encounters a lambda or initializer, it creates a **new scope**:

```csharp
// Example: Object initializer inside a method
void Method()                                    // Scope: Block(col=8)
{
    var obj = new MyClass                        // Scope: Block(col=8)
              {                                  // Scope: Initializer(col=14)
                  Name = "Test",                 //   → col=14 + 4 = 18
                  Handler = x =>                 // Scope: LambdaBody(col=29)
                            {                    //   → col=29
                                Process(x);      //   → col=29 + 4 = 33
                            }
              };
}
```

**Key Insight**: Each scope computes child positions **relative to itself**, not relative to the original tree structure. This eliminates cascading shifts:
- The initializer scope knows its base column is 14.
- The lambda scope knows its base column is 29.
- Neither needs to know about the other's shifting history.

### Scope Creation Rules

| Syntax Construct | Scope Kind | Base Column |
|------------------|-----------|-------------|
| Namespace body `{ }` | Block | parent + indent |
| Type body `{ }` | Block | parent + indent |
| Method/block body `{ }` | Block | parent + indent |
| Argument list (multi-line) | Continuation | column after `(` |
| Object initializer `{ }` | Initializer | column of `new` |
| Collection initializer `{ }` | Initializer | column of `new` |
| Collection expression `[ ]` | Initializer | column of `[` |
| Lambda block body `{ }` | LambdaBody | column of `{` |
| Lambda expression body | LambdaBody | column of expression start |
| Switch expression body | Continuation | column of `switch` |

---

## 14. Handling Continuation-Line Alignment

### The General Pattern

For any multi-line construct (arguments, chains, logical expressions, etc.), the algorithm is:

1. **Determine if the construct is multi-line** (any descendant token on a different line than the first).
2. **Compute the reference column** (where continuation lines should align to).
3. **Set layout instructions** for each first-on-line token within the construct.

### Example: Method Chain

```
Input tree:
  MemberAccessExpression
    ├─ Expression: items.Where(...)     (line 5, col 8)
    ├─ DotToken: .                      (line 6, col 4)  ← wrong
    └─ Name: Select(...)                (line 6, col 5)

Step 1: Collect chain links → [.Where(), .Select()]
Step 2: Reference column = column of first dot = column_of("items") + len("items") = 13
        Actually: reference = column after root expression's period
Step 3: Set layout:
        .Select → column 13 (align to .Where)
```

### Example: Binary Expression

```
Input tree:
  BinaryExpression (&&)
    ├─ Left: conditionA                 (line 3, col 8)
    ├─ OperatorToken: &&                (line 4, col 4)  ← wrong
    └─ Right: conditionB                (line 4, col 7)

Step 1: Is multi-line? Yes.
Step 2: Reference column = column of Left's first token = 8
Step 3: Set layout:
        && → column 8 (align to conditionA)
        conditionB → column 8 + len("&& ") = 11
```

This same algorithm applies to all binary operators — `&&`, `||`, `??`, `+`, `-`, `|`, etc.

### Handling Nested Continuations

The scope system naturally handles nesting:

```csharp
items.Where(x => x.Name != null        // Chain scope → col 5
                  && x.Value > 0)       // Binary scope within chain → col 18 (aligns to x.Name)
     .Select(x => x.Name)              // Chain scope → col 5
```

The binary expression contributor computes alignment relative to the **current scope's base column**, not the tree's absolute position. The chain contributor has already set the scope for the argument — so the binary expression contributor inherits the correct context.

---

## 15. Public Interface

The public interface of `ReihitsuFormatter` is **preserved unchanged**:

```csharp
public static class ReihitsuFormatter
{
    // Format an entire document
    public static async Task<Document> FormatDocumentAsync(
        Document document,
        CancellationToken cancellationToken = default);

    // Format a syntax tree (no workspace context)
    public static SyntaxTree FormatSyntaxTree(
        SyntaxTree syntaxTree,
        CancellationToken cancellationToken = default);

    // Format a single node (for code fix providers)
    public static SyntaxNode FormatNode(
        SyntaxNode node,
        int indentLevel = -1,
        CancellationToken cancellationToken = default);
}
```

The internal implementation delegates to the new pipeline:

```csharp
public static SyntaxTree FormatSyntaxTree(SyntaxTree syntaxTree, CancellationToken ct)
{
    // ... validation (syntax errors, auto-generated) ...

    var root = syntaxTree.GetRoot(ct);
    var context = new FormattingContext(DetectEndOfLine(root));

    // Phase 0: Structural transforms
    root = StructuralTransformPhase.Execute(root, context, ct);

    // Phase 1: Region formatting
    root = RegionFormattingPhase.Execute(root, context, ct);

    // Phase 2: Blank lines
    root = BlankLinePhase.Execute(root, context, ct);

    // Phase 3: Line breaks
    root = LineBreakPhase.Execute(root, context, ct);

    // Phase 4: Switch case braces
    root = SwitchCaseBracePhase.Execute(root, context, ct);

    // Phase 5: Horizontal spacing
    root = HorizontalSpacingPhase.Execute(root, context, ct);

    // Phase 6: Indentation & alignment (compute-then-apply)
    var layoutModel = LayoutComputer.Compute(root, context);
    root = IndentationRewriter.Apply(root, layoutModel, context);

    // Phase 7: Cleanup
    root = CleanupPhase.Execute(root, context, ct);

    return syntaxTree.WithRootAndOptions(root, syntaxTree.Options);
}
```

---

## 16. Testing Strategy

### 16.1 Existing Tests

Existing formatter tests serve as **integration tests** for the rewrite. However, each test must be individually evaluated to verify that its expected output truly matches the formatting rules defined in this concept. Some existing tests may have been created with incorrect expectations and should be corrected rather than blindly accepted.

### 16.2 Test Categories

| Category | Purpose | Approach |
|----------|---------|----------|
| Unit tests (per contributor) | Test each layout contributor in isolation | Input tree → verify LayoutModel entries |
| Integration tests | Test multi-contributor interactions | Full pipeline input/output comparison |
| Idempotency tests | Verify `format(format(x)) == format(x)` | Apply formatter twice, compare |
| Self-hosting tests | Formatter produces no changes on its own source | Run on Reihitsu solution |
| Regression tests | Prevent specific bug regressions | Targeted input/output pairs |

### 16.3 Test Infrastructure

The existing `FormatterTestsBase` class can be reused. Its `ApplyRule()` and `AssertRuleResult()` methods (which run the full pipeline and verify idempotency) remain valid — only the internal pipeline implementation changes.

---

## 17. Open Questions & Design Decisions

### Q1: LayoutModel Storage — RESOLVED

**Decision**: Use **line number** as the dictionary key. Since line structure is frozen after Phase 3 (no subsequent phase adds or removes line breaks — Phase 4 adds braces WITH their own line breaks, which are then stable), line numbers are stable and unique identifiers for first-on-line tokens. This is simpler and more efficient than annotation-based or span-based approaches.

### Q2: FormatNode API — Requires Investigation

**Problem**: `FormatNode` is used by analyzer code fix providers. The current API takes a `SyntaxNode` and optional `indentLevel`.

**Decision**: The `FormatNode` API does **not** need to maintain backward compatibility. It may require new parameters — for example, requiring a scope-capable syntax node (e.g., a full method body or class body rather than an isolated expression). The exact requirements will be determined by analyzing how `FormatNode` is used in existing code fix providers.

**Action**: Review all `FormatNode` call sites in the analyzer code fix project to determine what context information is available and what the API should look like.

### Q3: Contributor Ordering and Priority

**Problem**: When multiple contributors set layout for the same token, which wins?

**Proposed rule**: Later contributors override earlier ones. Contributors are ordered from least-specific to most-specific:
1. `ArgumentAlignmentContributor`
3. `MethodChainAlignmentContributor`
4. `ObjectInitializerContributor`
5. `BinaryExpressionContributor`
6. ... etc.

**Status**: General approach agreed. Exact ordering to be determined during testing.

### Q4: Multiline Detection

**Problem**: Several alignment rules only apply when a construct is multi-line. How to detect this?

**Approach**: Use `GetLineSpan()` on the construct's `SyntaxTree` to compare line numbers. For detached nodes (no tree), use trivia analysis to count `EndOfLineTrivia`.

### Q5: Incremental Formatting

**Status**: Not needed for v1. The scope-based architecture does not preclude adding incremental formatting later — the scope system naturally supports computing layout for a subtree within its parent scope.

---

## Appendix A: Migration Strategy

### Step 1: Delete All Old Implementation

Remove all existing formatter rule classes, pipeline classes, and internal infrastructure. The new implementation starts from a clean slate. Only the public API surface (`ReihitsuFormatter.cs` signature) is preserved.

### Step 2: Implement Phase by Phase

Build each phase incrementally, starting with Phase 0 and progressing through Phase 7. Each phase is implemented, tested, and validated before moving to the next.

### Step 3: Clarify Behavior Through Questions

When behavior is unclear or formatting rules are ambiguous, **ask the user** for clarification. Do not consult the old source code — the old implementation had known bugs and architectural issues that should not be propagated. The formatting rules document (formatting-rules.md) is the source of truth.

### Step 4: Validate Against Integration Tests

Use existing tests as integration tests, but evaluate each test's expected output against the formatting rules document. Fix any tests whose expectations don't match the defined rules.

---

## Appendix B: File Structure (Proposed)

```
Reihitsu.Formatter/
├── ReihitsuFormatter.cs                  (public API — unchanged)
├── FormattingContext.cs                  (context — preserved)
├── Pipeline/
│   ├── FormattingPipeline.cs             (orchestrates all 8 phases)
│   ├── StructuralTransforms/
│   │   ├── StructuralTransformPhase.cs
│   │   ├── ExpressionBodiedMethodTransform.cs
│   │   ├── ExpressionBodiedConstructorTransform.cs
│   │   ├── ExpressionBodiedOperatorTransform.cs
│   │   ├── ExpressionBodiedIndexerTransform.cs
│   │   ├── ExpressionBodiedConversionTransform.cs
│   │   ├── ExpressionBodiedFinalizerTransform.cs
│   │   └── ExpressionBodiedLocalFunctionTransform.cs
│   ├── RegionFormatting/
│   │   └── RegionFormattingPhase.cs
│   ├── BlankLines/
│   │   ├── BlankLinePhase.cs
│   │   └── StatementBlankLineRule.cs
│   ├── LineBreaks/
│   │   ├── LineBreakPhase.cs
│   │   ├── BracePlacementRule.cs
│   │   └── BinaryOperatorPositionRule.cs
│   ├── SwitchCaseBraces/
│   │   └── SwitchCaseBracePhase.cs
│   ├── HorizontalSpacing/
│   │   ├── HorizontalSpacingPhase.cs
│   │   ├── OperatorSpacingRule.cs
│   │   ├── CommaSpacingRule.cs
│   │   ├── KeywordSpacingRule.cs
│   │   └── ParenthesisSpacingRule.cs
│   ├── Indentation/
│   │   ├── LayoutComputer.cs
│   │   ├── IndentationRewriter.cs
│   │   ├── LayoutModel.cs
│   │   ├── TokenLayout.cs
│   │   ├── FormattingScope.cs
│   │   ├── ScopeKind.cs
│   │   └── Contributors/
│   │       ├── ILayoutContributor.cs
│   │       ├── ArgumentAlignmentContributor.cs
│   │       ├── MethodChainAlignmentContributor.cs
│   │       ├── ObjectInitializerContributor.cs
│   │       ├── CollectionExpressionContributor.cs
│   │       ├── BinaryExpressionContributor.cs
│   │       ├── ConditionalExpressionContributor.cs
│   │       ├── SwitchExpressionContributor.cs
│   │       ├── ConstructorInitializerContributor.cs
│   │       ├── GenericConstraintContributor.cs
│   │       ├── BaseTypeListContributor.cs
│   │       ├── AnonymousObjectContributor.cs
│   │       └── CommentIndentationContributor.cs
│   └── Cleanup/
│       ├── CleanupPhase.cs
│       ├── TrailingWhitespaceRule.cs
│       ├── ConsecutiveBlankLineRule.cs
│       └── EndOfFileRule.cs
```
