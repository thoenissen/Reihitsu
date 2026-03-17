# Reihitsu Code Formatter — Concept Document

## Table of Contents

1. [Introduction & Motivation](#1-introduction--motivation)
2. [Goals & Non-Goals](#2-goals--non-goals)
3. [Design Principles](#3-design-principles)
4. [Architecture Overview](#4-architecture-overview)
5. [Formatter Engine](#5-formatter-engine)
6. [Formatting Rules Catalog](#6-formatting-rules-catalog)
7. [Integration with Code Fix Providers](#7-integration-with-code-fix-providers)
8. [Visual Studio Extension](#8-visual-studio-extension)
9. [Solution & Project Structure](#9-solution--project-structure)
10. [Testing Strategy](#10-testing-strategy)
11. [Implementation Roadmap](#11-implementation-roadmap)
12. [Open Questions](#12-open-questions)

---

## 1. Introduction & Motivation

### Background

The Reihitsu project currently provides a Roslyn-based analyzer package (`Reihitsu.Analyzer`) that detects and, in many cases, automatically fixes formatting and style violations in C# code. As of version 0.12.1, the project includes 52 diagnostic rules spread across six categories (Clarity, Design, Naming, Formatting, Documentation, Performance), with 21 formatting rules in the `RH03xx` range alone.

Today, formatting corrections are applied **reactively**: the analyzer detects a violation and offers a code fix through Visual Studio's lightbulb UI or via `dotnet format`. While this model works, it has several limitations:

- **Piecemeal application.** Each formatting rule has its own independent code fix provider. Applying all formatting rules to a file requires triggering each fix individually or relying on "Fix All" functionality, which still operates one diagnostic at a time.
- **No holistic formatting pass.** There is no single operation that formats an entire document according to all Reihitsu formatting rules at once. Multiple fixes may interfere with each other's trivia manipulations if not coordinated.
- **No format-on-save.** Developers must manually trigger fixes. There is no way to automatically format code when saving a file.
- **Duplicated logic.** Formatting logic embedded in code fix providers cannot be easily reused outside the analyzer context (e.g., in a CLI tool or a VS extension).

### Vision

This concept proposes a **unified, non-configurable C# code formatter** that:

1. Encapsulates all Reihitsu formatting rules in a single, reusable **formatter engine**.
2. Is consumed by the existing **code fix providers**, replacing their inline formatting logic with calls to the shared engine.
3. Powers a **Visual Studio Extension (VSIX)** that automatically formats code on save.
4. Can optionally be consumed by future tools (CLI formatter, CI integration, etc.).

---

## 2. Goals & Non-Goals

### Goals

| # | Goal | Rationale |
|---|------|-----------|
| G1 | Create a shared formatter engine library that applies all Reihitsu formatting rules to a Roslyn `SyntaxTree` or `Document`. | Centralizes formatting logic; eliminates duplication. |
| G2 | The formatter is **not configurable** by the end user. Formatting rules and their behavior are opinionated and fixed. | Reduces complexity; enforces a single consistent style. |
| G3 | Existing code fix providers delegate to the formatter engine for their formatting logic. | Ensures consistency between one-at-a-time fixes and full-document formatting. |
| G4 | A Visual Studio Extension (VSIX) uses the formatter engine to format documents on save. | Provides seamless developer experience. |
| G5 | The formatter produces **deterministic, idempotent** output — formatting an already-formatted file results in no changes. | Critical for stability and avoiding unnecessary diffs. |
| G6 | The formatter preserves **semantic correctness** — it never changes the meaning of code, only its whitespace, trivia, and structural layout. | Safety guarantee. |
| G7 | The formatter handles **all current RH03xx formatting rules** and is extensible for future rules. | Covers existing scope and supports growth. |

### Non-Goals

| # | Non-Goal | Reason |
|---|----------|--------|
| NG1 | User-configurable formatting options (indent size, brace style, etc.). | By design, the formatter is opinionated and non-configurable. |
| NG2 | Replacing the existing analyzer/diagnostic system. Analyzers continue to report diagnostics; the formatter is a complementary tool. | Analyzers and the formatter serve different purposes (detection vs. correction). |
| NG3 | Formatting non-C# files (XAML, JSON, XML, etc.). | Out of scope for this project. |
| NG4 | A standalone CLI tool (e.g., `reihitsu format`). | May be added later but is not part of this initial concept. |
| NG5 | Formatting code that does not compile or has syntax errors. The formatter requires a valid `SyntaxTree`. | Simplifies implementation and avoids undefined behavior. |

---

## 3. Design Principles

### 3.1 Non-Configurable by Design

The formatter enforces a **single, opinionated style** without any user-facing configuration. This is a deliberate design choice:

- **Consistency across teams.** Every project using Reihitsu follows exactly the same formatting rules.
- **Zero configuration overhead.** No `.editorconfig` entries, no JSON settings files, no discussion about style preferences.
- **Simpler implementation.** No conditional logic paths based on configuration; every rule has exactly one behavior.
- **Predictable output.** Given the same input, the formatter always produces the same output, regardless of environment or user settings.

All formatting parameters (indent size = 4 spaces, brace placement, blank line rules, etc.) are compiled into the formatter as constants.

### 3.2 Idempotency

The formatter must be **idempotent**: applying the formatter to its own output must produce identical output. This guarantees:

- No "format on save" loops where each save triggers further changes.
- Clean diffs — a formatted file shows zero changes when re-formatted.
- Stable CI checks — a "is this formatted?" check always passes after a single formatting pass.

### 3.3 Semantic Preservation

The formatter must **never** alter the semantics of the code. It may only modify:

- **Whitespace trivia** (spaces, tabs, line breaks).
- **Structured trivia** related to layout (`#region` / `#endregion` descriptions are exempt — those are handled by analyzers, not the formatter).
- **Syntax structure** only when it is purely cosmetic (e.g., converting an expression-bodied method to a block-bodied method, which is semantically equivalent).

The formatter must **not** modify:

- Comments (content, though it may adjust their indentation).
- Preprocessor directives (other than adjusting indentation).
- String literals, interpolated strings, or raw string literals.
- Any token that affects compiled output.

### 3.4 Composability

The formatter is built from **individual, composable formatting rules** (called *formatting passes*). Each pass is responsible for one aspect of formatting (e.g., indentation, blank lines, expression layout). Passes are applied in a defined order to avoid conflicts.

---

## 4. Architecture Overview

### 4.1 High-Level Component Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Consumer Layer                               │
│                                                                     │
│  ┌───────────────────┐  ┌──────────────────┐  ┌─────────────────┐  │
│  │   CodeFix         │  │  VS Extension    │  │  Future CLI     │  │
│  │   Providers       │  │  (VSIX)          │  │  Tool           │  │
│  │                   │  │                  │  │                 │  │
│  │  Uses formatter   │  │  Format-on-save  │  │  Batch format   │  │
│  │  for individual   │  │  for active      │  │  entire         │  │
│  │  diagnostics      │  │  document        │  │  projects       │  │
│  └────────┬──────────┘  └────────┬─────────┘  └───────┬─────────┘  │
│           │                      │                     │            │
└───────────┼──────────────────────┼─────────────────────┼────────────┘
            │                      │                     │
            ▼                      ▼                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     Formatter Engine                                │
│                  (Reihitsu.Analyzer.Formatter)                      │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    ReihitsuFormatter                          │   │
│  │                                                              │   │
│  │  + FormatDocumentAsync(Document) : Task<Document>            │   │
│  │  + FormatSyntaxTree(SyntaxTree) : SyntaxTree                 │   │
│  │  + FormatNode(SyntaxNode) : SyntaxNode                       │   │
│  └──────────────────────┬───────────────────────────────────────┘   │
│                         │                                           │
│                         ▼                                           │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │              Formatting Pipeline                              │   │
│  │                                                              │   │
│  │  Pass 1: Structural transforms (expression → block body)     │   │
│  │  Pass 2: Blank line enforcement                              │   │
│  │  Pass 3: Indentation normalization                           │   │
│  │  Pass 4: Multi-line expression layout                        │   │
│  │  Pass 5: Spacing normalization                               │   │
│  │  Pass 6: Final trivia cleanup                                │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │              Individual Formatting Rules                      │   │
│  │                                                              │   │
│  │  IFormattingRule                                              │   │
│  │  + Apply(SyntaxNode, FormattingContext) : SyntaxNode          │   │
│  │                                                              │   │
│  │  ▸ BlankLineBeforeStatementRule                              │   │
│  │  ▸ ObjectInitializerLayoutRule                               │   │
│  │  ▸ MethodChainAlignmentRule                                  │   │
│  │  ▸ LogicalExpressionLayoutRule                               │   │
│  │  ▸ ExpressionBodiedMemberRule                                │   │
│  │  ▸ RegionFormattingRule                                      │   │
│  │  ▸ IndentationRule                                           │   │
│  │  ▸ ... (one per formatting concern)                          │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     Roslyn API Layer                                 │
│                                                                     │
│  Microsoft.CodeAnalysis.CSharp                                      │
│  Microsoft.CodeAnalysis.CSharp.Workspaces                           │
│  SyntaxTree, SyntaxNode, SyntaxToken, SyntaxTrivia                  │
└─────────────────────────────────────────────────────────────────────┘
```

### 4.2 Component Responsibilities

| Component | Responsibility |
|-----------|---------------|
| **Reihitsu.Analyzer.Formatter** | New shared library containing the formatter engine. Targets `netstandard2.0`. |
| **ReihitsuFormatter** | Public entry point. Orchestrates the formatting pipeline. |
| **Formatting Pipeline** | Executes formatting rules in a defined order. Manages the `FormattingContext`. |
| **IFormattingRule** | Interface for individual formatting rules. Each rule handles one formatting concern. |
| **FormattingContext** | Carries state through the pipeline (e.g., current indentation level, document options). |
| **CodeFix Providers** | Existing code fix providers call `ReihitsuFormatter` instead of implementing formatting inline. |
| **VS Extension (VSIX)** | Subscribes to document save events, invokes `ReihitsuFormatter`, and applies the result. |

---

## 5. Formatter Engine

### 5.1 Core API

The formatter engine exposes a minimal, focused API:

```csharp
namespace Reihitsu.Analyzer.Formatter;

/// <summary>
/// Reihitsu code formatter — applies all Reihitsu formatting rules to C# code.
/// This formatter is non-configurable and produces deterministic, idempotent output.
/// </summary>
public static class ReihitsuFormatter
{
    /// <summary>
    /// Formats an entire document by applying all formatting rules.
    /// </summary>
    /// <param name="document">The Roslyn Document to format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new Document with formatting applied.</returns>
    public static Task<Document> FormatDocumentAsync(
        Document document,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats a syntax tree by applying all formatting rules.
    /// Use this overload when no Workspace/Document context is available.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new SyntaxTree with formatting applied.</returns>
    public static SyntaxTree FormatSyntaxTree(
        SyntaxTree syntaxTree,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats a specific syntax node and its descendants.
    /// Useful for code fix providers that need to format only a
    /// newly generated or modified node rather than the full document.
    /// </summary>
    /// <param name="node">The syntax node to format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new SyntaxNode with formatting applied.</returns>
    public static SyntaxNode FormatNode(
        SyntaxNode node,
        CancellationToken cancellationToken = default);
}
```

**Design decisions:**

- **Static class.** Since the formatter is non-configurable, there is no need for instance state. A static class avoids unnecessary allocations and simplifies consumption.
- **Three entry points** cover different use cases:
  - `FormatDocumentAsync` — for VS extension (format-on-save) and full-document formatting.
  - `FormatSyntaxTree` — for scenarios without a workspace context (e.g., testing, CLI tools).
  - `FormatNode` — for code fix providers that generate new syntax nodes and need to format just the generated portion.
- **CancellationToken** on all methods — formatting large files may take non-trivial time; consumers must be able to cancel.

### 5.2 Formatting Pipeline

The formatter applies rules in a carefully ordered pipeline. The ordering prevents conflicts between rules and ensures idempotency.

```
Input SyntaxTree/Node
        │
        ▼
┌─────────────────────────────────┐
│  Phase 1: Structural Transforms │  Highest priority — changes node structure
│                                 │  (e.g., expression body → block body)
│  Rules:                         │
│  - ExpressionBodiedMethodRule   │
│  - ExpressionBodiedCtorRule     │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│  Phase 2: Multi-Line Layout     │  Decides which expressions must span
│                                 │  multiple lines
│  Rules:                         │
│  - ObjectInitializerLayoutRule  │
│  - MethodChainAlignmentRule     │
│  - LogicalExpressionLayoutRule  │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│  Phase 3: Blank Line Mgmt      │  Inserts/removes blank lines between
│                                 │  statements
│  Rules:                         │
│  - BlankLineBeforeStatementsRule│
│  - BlankLineAfterStatementsRule │
│  - BlankLineBeforeCommentsRule  │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│  Phase 4: Indentation           │  Normalizes all indentation to
│                                 │  4 spaces per level
│  Rules:                         │
│  - IndentationRule              │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│  Phase 5: Spacing               │  Normalizes horizontal spacing
│                                 │  (spaces around operators, etc.)
│  Rules:                         │
│  - HorizontalSpacingRule        │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│  Phase 6: Region Formatting     │  Ensures #region/#endregion
│                                 │  consistency and casing
│  Rules:                         │
│  - RegionFormattingRule         │
└───────────┬─────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│  Phase 7: Final Cleanup         │  Trailing whitespace removal,
│                                 │  final newline, consecutive blank
│  Rules:                         │  line reduction
│  - TrailingTriviaCleanupRule    │
└───────────┬─────────────────────┘
            │
            ▼
      Output SyntaxTree/Node
```

**Why this ordering matters:**

1. **Structural transforms first** — converting expression-bodied members to block bodies creates new syntax nodes (braces, return statements) that subsequent phases need to format.
2. **Multi-line layout second** — deciding whether an expression spans multiple lines must happen before indentation is applied.
3. **Blank lines third** — blank line rules depend on the final statement structure but not yet on indentation.
4. **Indentation fourth** — once the structure and line breaks are finalized, indentation can be applied consistently.
5. **Spacing fifth** — horizontal spacing operates within lines and should be applied after vertical layout is settled.
6. **Region formatting sixth** — operates at a higher structural level and depends on surrounding formatting being settled.
7. **Cleanup last** — removes trailing whitespace and normalizes the final state.

### 5.3 Formatting Rules Interface

```csharp
namespace Reihitsu.Analyzer.Formatter.Rules;

/// <summary>
/// Interface for an individual formatting rule.
/// Each rule handles one specific aspect of code formatting.
/// </summary>
internal interface IFormattingRule
{
    /// <summary>
    /// The phase in the formatting pipeline this rule belongs to.
    /// </summary>
    FormattingPhase Phase { get; }

    /// <summary>
    /// Applies this formatting rule to the given syntax node.
    /// </summary>
    /// <param name="node">The syntax node to format.</param>
    /// <param name="context">The formatting context carrying pipeline state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The formatted syntax node.</returns>
    SyntaxNode Apply(SyntaxNode node, FormattingContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Phases of the formatting pipeline, executed in order.
/// </summary>
internal enum FormattingPhase
{
    /// <summary>
    /// Structural transforms (e.g., expression body → block body).
    /// </summary>
    StructuralTransform = 0,

    /// <summary>
    /// Multi-line expression layout decisions.
    /// </summary>
    MultiLineLayout = 1,

    /// <summary>
    /// Blank line insertion and removal.
    /// </summary>
    BlankLineManagement = 2,

    /// <summary>
    /// Indentation normalization.
    /// </summary>
    Indentation = 3,

    /// <summary>
    /// Horizontal spacing normalization.
    /// </summary>
    Spacing = 4,

    /// <summary>
    /// Region formatting.
    /// </summary>
    RegionFormatting = 5,

    /// <summary>
    /// Final cleanup (trailing whitespace, final newline).
    /// </summary>
    Cleanup = 6
}
```

### 5.4 Formatting Context

```csharp
namespace Reihitsu.Analyzer.Formatter;

/// <summary>
/// Carries state through the formatting pipeline.
/// Immutable — each phase receives a fresh context snapshot.
/// </summary>
internal sealed class FormattingContext
{
    /// <summary>
    /// Indentation unit: 4 spaces (non-configurable).
    /// </summary>
    public const int IndentSize = 4;

    /// <summary>
    /// Use spaces for indentation (non-configurable).
    /// </summary>
    public const bool UseTabs = false;

    /// <summary>
    /// End-of-line sequence.
    /// </summary>
    public string EndOfLine { get; }

    /// <summary>
    /// The original document (may be null when formatting a standalone SyntaxTree).
    /// </summary>
    public Document? Document { get; }
}
```

### 5.5 CSharpSyntaxRewriter Pattern

The individual formatting rules are implemented as `CSharpSyntaxRewriter` subclasses. This is the standard Roslyn pattern for transforming syntax trees:

```csharp
namespace Reihitsu.Analyzer.Formatter.Rules;

/// <summary>
/// Base class for formatting rules that operate as syntax rewriters.
/// </summary>
internal abstract class FormattingRuleBase : CSharpSyntaxRewriter, IFormattingRule
{
    /// <inheritdoc/>
    public abstract FormattingPhase Phase { get; }

    /// <inheritdoc/>
    public SyntaxNode Apply(SyntaxNode node, FormattingContext context, CancellationToken cancellationToken)
    {
        Context = context;
        CancellationToken = cancellationToken;

        return Visit(node);
    }

    /// <summary>
    /// The current formatting context.
    /// </summary>
    protected FormattingContext Context { get; private set; }

    /// <summary>
    /// Cancellation token for the current operation.
    /// </summary>
    protected CancellationToken CancellationToken { get; private set; }
}
```

This allows each rule to override specific `Visit*` methods (e.g., `VisitIfStatement`, `VisitMethodDeclaration`) to apply targeted transformations while the base `CSharpSyntaxRewriter` handles recursion through the tree.

### 5.6 Pipeline Executor

```csharp
namespace Reihitsu.Analyzer.Formatter;

/// <summary>
/// Executes the formatting pipeline by running all rules in phase order.
/// </summary>
internal static class FormattingPipeline
{
    /// <summary>
    /// All formatting rules, sorted by phase.
    /// </summary>
    private static readonly IReadOnlyList<IFormattingRule> Rules = LoadRules();

    /// <summary>
    /// Applies the full formatting pipeline to a syntax node.
    /// </summary>
    public static SyntaxNode Execute(
        SyntaxNode node,
        FormattingContext context,
        CancellationToken cancellationToken)
    {
        var current = node;

        foreach (var rule in Rules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            current = rule.Apply(current, context, cancellationToken);
        }

        return current;
    }

    /// <summary>
    /// Loads and orders all formatting rules by phase.
    /// </summary>
    private static IReadOnlyList<IFormattingRule> LoadRules()
    {
        // Rules are instantiated once and reused (they are stateless
        // between calls — state flows through FormattingContext).
        return new IFormattingRule[]
        {
            // Phase 0: Structural transforms
            new ExpressionBodiedMethodRule(),
            new ExpressionBodiedConstructorRule(),

            // Phase 1: Multi-line layout
            new ObjectInitializerLayoutRule(),
            new MethodChainAlignmentRule(),
            new LogicalExpressionLayoutRule(),

            // Phase 2: Blank line management
            new BlankLineBeforeStatementRule(),
            new BlankLineAfterStatementRule(),

            // Phase 3: Indentation
            new IndentationRule(),

            // Phase 4: Spacing
            new HorizontalSpacingRule(),

            // Phase 5: Region formatting
            new RegionFormattingRule(),

            // Phase 6: Cleanup
            new TrailingTriviaCleanupRule(),
        };
    }
}
```

---

## 6. Formatting Rules Catalog

The formatter covers all existing `RH03xx` formatting rules plus additional formatting concerns that are currently not covered by any diagnostic rule. Below is the complete catalog.

### 6.1 Rules Derived from Existing Analyzers

These rules correspond directly to existing Reihitsu diagnostic rules and implement the same formatting behavior:

| Rule | Source Diagnostic | Formatting Behavior |
|------|-------------------|---------------------|
| **ExpressionBodiedMethodRule** | RH0325 | Convert expression-bodied methods to block-bodied methods. |
| **ExpressionBodiedConstructorRule** | RH0326 | Convert expression-bodied constructors to block-bodied constructors. |
| **ObjectInitializerLayoutRule** | RH0302 | Format object initializers as multi-line with brace and property alignment. |
| **MethodChainAlignmentRule** | RH0324 | Align method chain dots vertically on continuation lines. |
| **LogicalExpressionLayoutRule** | RH0329 | Format multi-clause logical expressions across multiple lines with operator alignment. |
| **RegionFormattingRule** | RH0301, RH0328 | Ensure `#region`/`#endregion` descriptions match; region descriptions start with uppercase. |
| **BlankLineBeforeStatementRule** | RH0303–RH0321 | Insert blank lines before `try`, `if`, `while`, `do`, `using`, `foreach`, `for`, `return`, `goto`, `break`, `continue`, `throw`, `switch`, `checked`, `unchecked`, `fixed`, `lock`, and `yield` statements. |
| **BlankLineAfterStatementRule** | RH0313 | Insert blank lines after `break` statements (and other applicable statements). |

### 6.2 New Rules (Not Covered by Existing Diagnostics)

These rules handle formatting concerns that are implicit in the codebase conventions but not currently enforced by diagnostic rules:

| Rule | Formatting Behavior |
|------|---------------------|
| **IndentationRule** | Normalize all indentation to 4 spaces per nesting level. Convert tabs to spaces. Align continuation lines. |
| **HorizontalSpacingRule** | Ensure single spaces around binary operators, after commas, after semicolons in `for` statements. No space before semicolons. No space before `(` in method calls. Single space after keywords (`if`, `for`, `while`, etc.). |
| **TrailingTriviaCleanupRule** | Remove trailing whitespace from all lines. Ensure a single trailing newline at end of file. Collapse consecutive blank lines to a maximum of one. |

### 6.3 Rule Implementation Example

Below is a sketch of how the `BlankLineBeforeStatementRule` would be implemented. This rule consolidates the logic of 17 existing code fix providers into a single rewriter:

```csharp
namespace Reihitsu.Analyzer.Formatter.Rules;

/// <summary>
/// Ensures that certain statements are preceded by a blank line,
/// unless the statement is the first in a block.
/// Consolidates rules RH0303–RH0321.
/// </summary>
internal sealed class BlankLineBeforeStatementRule : FormattingRuleBase
{
    public override FormattingPhase Phase => FormattingPhase.BlankLineManagement;

    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        return EnsureBlankLineBefore(base.VisitIfStatement(node));
    }

    public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
    {
        return EnsureBlankLineBefore(base.VisitTryStatement(node));
    }

    // ... similar overrides for while, do, using, foreach, for,
    //     return, goto, break, continue, throw, switch, checked,
    //     unchecked, fixed, lock, yield statements

    private SyntaxNode EnsureBlankLineBefore(SyntaxNode node)
    {
        if (node == null || IsFirstStatementInBlock(node))
        {
            return node;
        }

        var leadingTrivia = node.GetLeadingTrivia();

        if (HasBlankLineBefore(leadingTrivia))
        {
            return node;
        }

        var newTrivia = leadingTrivia.Insert(0, SyntaxFactory.EndOfLine(Context.EndOfLine));

        return node.WithLeadingTrivia(newTrivia);
    }

    // Helper methods...
}
```

---

## 7. Integration with Code Fix Providers

### 7.1 Current State

Today, each code fix provider implements its own formatting logic. For example, `RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider` rebuilds the entire initializer expression using `SyntaxFactory` calls with hardcoded trivia. The `StatementShouldBePrecededByABlankLineCodeFixProviderBase` inserts `EndOfLine` trivia directly.

### 7.2 Proposed Integration

Code fix providers will delegate formatting to `ReihitsuFormatter.FormatNode()`. This means:

1. The code fix provider **identifies** the node that needs to be fixed (unchanged).
2. Instead of manually manipulating trivia, it calls `ReihitsuFormatter.FormatNode()` on the node (or a replacement node).
3. The formatter applies all relevant rules and returns the formatted node.
4. The code fix provider replaces the original node with the formatted result (unchanged).

#### Before (current approach):

```csharp
// In RH0302 CodeFixProvider
private async Task<Document> ApplyCodeFixAsync(
    Document document,
    ObjectCreationExpressionSyntax node,
    CancellationToken cancellationToken)
{
    var root = await document.GetSyntaxRootAsync(cancellationToken);
    var newKeywordPosition = node.NewKeyword.GetLocation()
                                            .GetLineSpan()
                                            .StartLinePosition;

    // ~50 lines of manual SyntaxFactory calls to rebuild
    // the initializer with correct formatting...
    var correctedInitializer = RebuildInitializerExpression(node, newKeywordPosition);

    root = root.ReplaceNode(node, correctedInitializer);
    return document.WithSyntaxRoot(root);
}
```

#### After (using formatter engine):

```csharp
// In RH0302 CodeFixProvider
private async Task<Document> ApplyCodeFixAsync(
    Document document,
    ObjectCreationExpressionSyntax node,
    CancellationToken cancellationToken)
{
    var root = await document.GetSyntaxRootAsync(cancellationToken);

    // Delegate formatting to the shared engine
    var formattedNode = ReihitsuFormatter.FormatNode(node, cancellationToken);

    root = root.ReplaceNode(node, formattedNode);
    return document.WithSyntaxRoot(root);
}
```

### 7.3 Migration Strategy

The migration from inline formatting to formatter-engine delegation follows a phased approach:

1. **Phase 1:** Implement the formatter engine with all existing formatting rules.
2. **Phase 2:** Add integration tests that compare the formatter output against the existing code fix output for all known test cases. Both must produce identical results.
3. **Phase 3:** Refactor code fix providers one by one to delegate to the formatter. After each refactoring, run the integration tests to ensure no regressions.
4. **Phase 4:** Remove the now-unused inline formatting code from code fix providers. Remove the base class `StatementShouldBePrecededByABlankLineCodeFixProviderBase` once all subclasses have been migrated.

### 7.4 Partial Formatting for Code Fixes

Code fix providers often need to format only a **specific node** (the one being fixed), not the entire document. The `FormatNode()` method supports this use case. It formats the given node and its descendants without touching the rest of the document.

Important considerations:

- **Context-aware formatting.** When formatting a node, the formatter needs to know the node's indentation context (i.e., how deeply nested it is). The `FormatNode` method determines this from the node's position in the syntax tree.
- **Boundary handling.** The formatter must correctly handle the trivia at the boundary between the formatted node and its surrounding unformatted nodes.

---

## 8. Visual Studio Extension

### 8.1 Overview

The Visual Studio Extension (VSIX) provides a **format-on-save** experience. When a developer saves a C# file, the extension automatically applies the Reihitsu formatter to the document before the file is written to disk.

### 8.2 Extension Architecture

```
┌────────────────────────────────────────────────────┐
│          Reihitsu.VisualStudio (VSIX)              │
│                                                    │
│  ┌──────────────────────────────────────────────┐  │
│  │         FormatOnSaveCommandHandler           │  │
│  │                                              │  │
│  │  Subscribes to: IVsRunningDocTableEvents     │  │
│  │  Event: OnBeforeSave                         │  │
│  │                                              │  │
│  │  1. Get active Document from Workspace       │  │
│  │  2. Call ReihitsuFormatter.FormatDocumentAsync│  │
│  │  3. Apply changes to Workspace               │  │
│  └──────────────────────────────────────────────┘  │
│                                                    │
│  ┌──────────────────────────────────────────────┐  │
│  │         FormatDocumentCommandHandler         │  │
│  │                                              │  │
│  │  Provides manual "Format with Reihitsu"      │  │
│  │  command in the editor context menu.          │  │
│  └──────────────────────────────────────────────┘  │
│                                                    │
│  ┌──────────────────────────────────────────────┐  │
│  │         ReihitsuPackage                      │  │
│  │                                              │  │
│  │  VS Package entry point. Registers commands  │  │
│  │  and event handlers.                         │  │
│  └──────────────────────────────────────────────┘  │
│                                                    │
│  References:                                       │
│  ├── Reihitsu.Analyzer.Formatter                   │
│  ├── Microsoft.VisualStudio.SDK                    │
│  └── Community.VisualStudio.Toolkit               │
│                                                    │
└────────────────────────────────────────────────────┘
```

### 8.3 Format-on-Save Implementation

The extension hooks into Visual Studio's document save pipeline using the `IVsRunningDocTableEvents3` interface (or the modern Community.VisualStudio.Toolkit equivalent):

```csharp
using Community.VisualStudio.Toolkit;

using Microsoft.VisualStudio.Shell;

namespace Reihitsu.VisualStudio;

/// <summary>
/// Formats C# documents on save using the Reihitsu formatter.
/// </summary>
[Command(PackageIds.FormatOnSaveCommand)]
internal sealed class FormatOnSaveCommand : BaseCommand<FormatOnSaveCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var docView = await VS.Documents.GetActiveDocumentViewAsync();

        if (docView?.TextBuffer == null)
        {
            return;
        }

        var workspace = docView.TextBuffer.GetWorkspace();
        var documentId = workspace?.GetDocumentIdInCurrentContext(
            docView.TextBuffer.AsTextContainer());

        if (workspace == null || documentId == null)
        {
            return;
        }

        var document = workspace.CurrentSolution.GetDocument(documentId);

        if (document == null)
        {
            return;
        }

        // Apply the Reihitsu formatter
        var formattedDocument = await ReihitsuFormatter.FormatDocumentAsync(
            document,
            CancellationToken.None);

        // Apply changes to the workspace
        workspace.TryApplyChanges(formattedDocument.Project.Solution);
    }
}
```

### 8.4 Running Document Table Events Approach

For format-on-save, the extension subscribes to the Running Document Table (RDT) to detect save events:

```csharp
namespace Reihitsu.VisualStudio;

/// <summary>
/// Listens for document save events and triggers formatting.
/// </summary>
internal sealed class DocumentSaveListener : IVsRunningDocTableEvents3
{
    private readonly RunningDocumentTable _rdt;

    public int OnBeforeSave(uint docCookie)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var documentInfo = _rdt.GetDocumentInfo(docCookie);
        var filePath = documentInfo.Moniker;

        // Only format C# files
        if (filePath?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == false)
        {
            return VSConstants.S_OK;
        }

        // Trigger formatting asynchronously
        _ = FormatDocumentAsync(filePath);

        return VSConstants.S_OK;
    }

    // Other IVsRunningDocTableEvents3 members return VSConstants.S_OK
}
```

### 8.5 Extension Features

| Feature | Description |
|---------|-------------|
| **Format on Save** | Automatically format when saving a `.cs` file. Enabled by default. Can be toggled via a VS menu option. |
| **Format Document Command** | Manual command accessible via Editor Context Menu → "Format with Reihitsu" and via a keyboard shortcut. |
| **Status Bar Feedback** | Shows "Reihitsu: Formatting..." in the status bar during formatting; "Reihitsu: Formatted" on completion. |
| **Error Handling** | If formatting fails (e.g., syntax errors), the save proceeds without formatting. Errors are logged to the Output window. |

### 8.6 Extension Configuration

Even though the formatter itself is non-configurable, the extension provides a minimal options page to control its own behavior:

| Option | Default | Description |
|--------|---------|-------------|
| Enable format on save | `true` | Toggles automatic formatting when saving `.cs` files. |

Note: This is **not** a formatter configuration option. It controls whether the extension triggers formatting on save, not how the formatting behaves.

### 8.7 Supported Visual Studio Versions

| Version | Support |
|---------|---------|
| Visual Studio 2022 (17.x) | ✓ Primary target |
| Visual Studio 2025 (18.x) | ✓ Forward compatibility planned |
| Visual Studio Code | ✗ Out of scope (different extension model) |

### 8.8 Distribution

The extension will be distributed via:

1. **Visual Studio Marketplace** — as a standalone VSIX package.
2. **GitHub Releases** — VSIX attached to Reihitsu releases.

The extension is **independent** from the NuGet analyzer package. A project can use the NuGet analyzer for diagnostics without the VS extension, and vice versa. However, using both together provides the best experience: the analyzer catches violations, and the extension prevents them from being committed.

---

## 9. Solution & Project Structure

### 9.1 New Projects

The following new projects will be added to the `Reihitsu.sln` solution:

```
Reihitsu.sln
├── Reihitsu.Analyzer/
│   ├── Reihitsu.Analyzer/                    (existing — analyzers)
│   ├── Reihitsu.Analyzer.CodeFixes/          (existing — code fixes)
│   ├── Reihitsu.Analyzer.Formatter/          (NEW — formatter engine)
│   ├── Reihitsu.Analyzer.Formatter.Test/     (NEW — formatter tests)
│   ├── Reihitsu.Analyzer.Test/               (existing — analyzer tests)
│   └── Reihitsu.Analyzer.Package/            (existing — NuGet package)
├── Reihitsu.VisualStudio/                    (NEW — VS extension)
│   └── Reihitsu.VisualStudio/
└── concepts/
    └── formatter/
        └── README.md                         (this document)
```

### 9.2 Reihitsu.Analyzer.Formatter Project

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <RootNamespace>Reihitsu.Analyzer.Formatter</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" />
  </ItemGroup>

</Project>
```

**Key decisions:**
- Targets **netstandard2.0** for compatibility with both the analyzer (netstandard2.0) and the VS extension.
- References `Microsoft.CodeAnalysis.CSharp` — the same version used by the analyzer project.
- Does **not** reference `Reihitsu.Analyzer` — the formatter is independent of the diagnostic analyzers.

### 9.3 Project Dependency Graph

```
Reihitsu.Analyzer.Package (NuGet)
├── Reihitsu.Analyzer.CodeFixes
│   ├── Reihitsu.Analyzer
│   └── Reihitsu.Analyzer.Formatter    (NEW dependency)
├── Reihitsu.Analyzer
└── Reihitsu.Analyzer.Formatter         (NEW — bundled in package)

Reihitsu.VisualStudio (VSIX)
└── Reihitsu.Analyzer.Formatter          (NEW dependency)

Reihitsu.Analyzer.Formatter.Test (Tests)
└── Reihitsu.Analyzer.Formatter          (NEW dependency)

Reihitsu.Analyzer.Test (Tests)
├── Reihitsu.Analyzer
└── Reihitsu.Analyzer.CodeFixes
```

**Important:** The formatter library (`Reihitsu.Analyzer.Formatter`) must also be included in the NuGet package, since the code fix providers will depend on it:

```xml
<!-- In Reihitsu.Analyzer.Package.csproj -->
<Target Name="_AddAnalyzersToOutput">
  <ItemGroup>
    <TfmSpecificPackageFile Include="$(OutputPath)\Reihitsu.Analyzer.dll" PackagePath="analyzers/dotnet/cs" />
    <TfmSpecificPackageFile Include="$(OutputPath)\Reihitsu.Analyzer.CodeFixes.dll" PackagePath="analyzers/dotnet/cs" />
    <TfmSpecificPackageFile Include="$(OutputPath)\Reihitsu.Analyzer.Formatter.dll" PackagePath="analyzers/dotnet/cs" />
  </ItemGroup>
</Target>
```

### 9.4 Formatter Project Internal Structure

```
Reihitsu.Analyzer.Formatter/
├── ReihitsuFormatter.cs               # Public API entry point
├── FormattingContext.cs                # Pipeline state
├── FormattingPipeline.cs              # Pipeline executor
├── FormattingConstants.cs             # Non-configurable constants
├── Rules/
│   ├── IFormattingRule.cs             # Rule interface
│   ├── FormattingRuleBase.cs          # CSharpSyntaxRewriter base
│   ├── FormattingPhase.cs             # Phase enum
│   ├── Structural/
│   │   ├── ExpressionBodiedMethodRule.cs
│   │   └── ExpressionBodiedConstructorRule.cs
│   ├── Layout/
│   │   ├── ObjectInitializerLayoutRule.cs
│   │   ├── MethodChainAlignmentRule.cs
│   │   └── LogicalExpressionLayoutRule.cs
│   ├── BlankLines/
│   │   ├── BlankLineBeforeStatementRule.cs
│   │   └── BlankLineAfterStatementRule.cs
│   ├── Indentation/
│   │   └── IndentationRule.cs
│   ├── Spacing/
│   │   └── HorizontalSpacingRule.cs
│   ├── Regions/
│   │   └── RegionFormattingRule.cs
│   └── Cleanup/
│       └── TrailingTriviaCleanupRule.cs
└── Properties/
    └── GlobalUsings.cs
```

---

## 10. Testing Strategy

### 10.1 Test Categories

| Category | Description | Project |
|----------|-------------|---------|
| **Unit Tests** | Test individual formatting rules in isolation. | `Reihitsu.Analyzer.Formatter.Test` |
| **Integration Tests** | Test the full pipeline on complete C# files. | `Reihitsu.Analyzer.Formatter.Test` |
| **Idempotency Tests** | Verify that formatting the output of the formatter produces no changes. | `Reihitsu.Analyzer.Formatter.Test` |
| **Regression Tests** | Compare formatter output against existing code fix test data to ensure identical behavior. | `Reihitsu.Analyzer.Formatter.Test` |
| **Existing Analyzer Tests** | Existing tests continue to pass without modification. | `Reihitsu.Analyzer.Test` |

### 10.2 Test Approach

Following the existing testing conventions in `Reihitsu.Analyzer.Test`, test data files will be stored as `.cs` files in a `Resources/` directory and loaded via `.resx` references:

```
Reihitsu.Analyzer.Formatter.Test/
├── Rules/
│   ├── BlankLines/
│   │   ├── BlankLineBeforeStatementRuleTests.cs
│   │   └── Resources/
│   │       ├── BlankLineBeforeStatement.TestData.cs
│   │       └── BlankLineBeforeStatement.ResultData.cs
│   ├── Layout/
│   │   ├── ObjectInitializerLayoutRuleTests.cs
│   │   └── Resources/
│   │       ├── ObjectInitializerLayout.TestData.cs
│   │       └── ObjectInitializerLayout.ResultData.cs
│   └── ...
├── Pipeline/
│   ├── FormattingPipelineTests.cs         # Full-pipeline tests
│   └── Resources/
│       ├── FullDocument.TestData.cs
│       └── FullDocument.ResultData.cs
└── Idempotency/
    └── IdempotencyTests.cs                # Format-twice-same-result tests
```

### 10.3 Idempotency Testing

Every test data file will be run through the formatter **twice**, and the second pass must produce output identical to the first:

```csharp
[TestMethod]
public void FormatterOutput_IsIdempotent()
{
    var input = TestData.SomeTestDataFile;

    var firstPass = ReihitsuFormatter.FormatSyntaxTree(
        CSharpSyntaxTree.ParseText(input));

    var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass);

    Assert.AreEqual(
        firstPass.GetRoot().ToFullString(),
        secondPass.GetRoot().ToFullString(),
        "Formatter output must be idempotent");
}
```

### 10.4 Regression Testing Against Existing Code Fix Output

To ensure the formatter produces the same output as the existing code fix providers:

```csharp
[TestMethod]
public void FormatterOutput_MatchesExistingCodeFixOutput_RH0302()
{
    var input = ExistingTestData.RH0302TestData;
    var expectedOutput = ExistingTestData.RH0302ResultData;

    var formattedTree = ReihitsuFormatter.FormatSyntaxTree(
        CSharpSyntaxTree.ParseText(input));

    Assert.AreEqual(
        expectedOutput,
        formattedTree.GetRoot().ToFullString());
}
```

---

## 11. Implementation Roadmap

### Phase 1: Foundation (Formatter Engine)

| Step | Task | Estimated Effort |
|------|------|-----------------|
| 1.1 | Create `Reihitsu.Analyzer.Formatter` project with infrastructure (`FormattingContext`, `FormattingPipeline`, `IFormattingRule`, `FormattingRuleBase`). | Small |
| 1.2 | Implement `BlankLineBeforeStatementRule` (covers RH0303–RH0321 — the largest group). | Medium |
| 1.3 | Implement `BlankLineAfterStatementRule` (covers RH0313). | Small |
| 1.4 | Create `Reihitsu.Analyzer.Formatter.Test` project and write tests for blank line rules using existing test data. | Medium |
| 1.5 | Implement idempotency test infrastructure. | Small |

### Phase 2: Layout Rules

| Step | Task | Estimated Effort |
|------|------|-----------------|
| 2.1 | Implement `ObjectInitializerLayoutRule` (covers RH0302). | Medium |
| 2.2 | Implement `MethodChainAlignmentRule` (covers RH0324). | Medium |
| 2.3 | Implement `LogicalExpressionLayoutRule` (covers RH0329). | Medium |
| 2.4 | Implement `ExpressionBodiedMethodRule` (covers RH0325). | Small |
| 2.5 | Implement `ExpressionBodiedConstructorRule` (covers RH0326). | Small |
| 2.6 | Write tests for all layout rules. | Medium |

### Phase 3: Indentation, Spacing, and Cleanup Rules

| Step | Task | Estimated Effort |
|------|------|-----------------|
| 3.1 | Implement `IndentationRule`. | Large |
| 3.2 | Implement `HorizontalSpacingRule`. | Medium |
| 3.3 | Implement `RegionFormattingRule` (covers RH0301, RH0328). | Small |
| 3.4 | Implement `TrailingTriviaCleanupRule`. | Small |
| 3.5 | Write tests and verify idempotency for all rules. | Medium |

### Phase 4: Full-Pipeline Integration

| Step | Task | Estimated Effort |
|------|------|-----------------|
| 4.1 | Implement `ReihitsuFormatter` public API (the three entry points). | Small |
| 4.2 | Write full-document formatting tests. | Medium |
| 4.3 | Run regression tests against existing code fix test data. | Medium |
| 4.4 | Resolve any differences between formatter and existing code fixes. | Medium |

### Phase 5: Code Fix Provider Migration

| Step | Task | Estimated Effort |
|------|------|-----------------|
| 5.1 | Refactor `StatementShouldBePrecededByABlankLineCodeFixProviderBase` subclasses to use formatter. | Medium |
| 5.2 | Refactor `RH0302` code fix provider to use formatter. | Small |
| 5.3 | Refactor `RH0324` code fix provider to use formatter. | Small |
| 5.4 | Refactor `RH0325` code fix provider to use formatter. | Small |
| 5.5 | Refactor `RH0329` code fix provider to use formatter. | Small |
| 5.6 | Remove deprecated inline formatting code. | Small |
| 5.7 | Update NuGet package to include `Reihitsu.Analyzer.Formatter.dll`. | Small |

### Phase 6: Visual Studio Extension

| Step | Task | Estimated Effort |
|------|------|-----------------|
| 6.1 | Create `Reihitsu.VisualStudio` VSIX project. | Small |
| 6.2 | Implement `FormatOnSaveCommand` with RDT event subscription. | Medium |
| 6.3 | Implement manual "Format with Reihitsu" context menu command. | Small |
| 6.4 | Add status bar feedback and error handling. | Small |
| 6.5 | Implement minimal options page (enable/disable format on save). | Small |
| 6.6 | Test in Visual Studio 2022 experimental instance. | Medium |
| 6.7 | Package and publish to VS Marketplace. | Small |

---

## 12. Open Questions

The following questions should be discussed and resolved before or during implementation:

| # | Question | Context |
|---|----------|---------|
| Q1 | **Should the formatter handle files with syntax errors gracefully (e.g., format only valid portions)?** The current concept requires a valid `SyntaxTree`. This means partially written code won't be formatted on save. | The VS extension use case (format-on-save) may encounter files that temporarily have syntax errors during editing. |
| Q2 | **Should the formatter be applied to generated code (`.Designer.cs`, `.g.cs`)?** | Some generated files may be modified by the formatter, causing unnecessary churn if the generator recreates them. |
| Q3 | **How should the formatter handle `#pragma` directives and `#if` / `#endif` conditional compilation?** | These preprocessor directives can affect the structure of the syntax tree and may interact with indentation rules. |
| Q4 | **Should the VS extension provide a "Format Selection" command in addition to full-document formatting?** | Some developers prefer to format only the code they changed. This would use `FormatNode()` on the selected span. |
| Q5 | **Should there be a `dotnet tool` CLI distribution in addition to the VSIX and NuGet package?** | A CLI tool (e.g., `dotnet reihitsu-format`) would enable CI pipeline integration for format checks. This is listed as a non-goal but may be reconsidered. |
| Q6 | **How should the formatter interact with `.editorconfig` files?** The formatter is non-configurable, but `.editorconfig` is widely used. | Some teams may have `.editorconfig` settings that conflict with Reihitsu's formatting. The formatter should ignore `.editorconfig` to maintain its non-configurable nature, but this decision should be documented clearly. |
| Q7 | **Should the formatter preserve comment alignment patterns (e.g., aligned `//` comments at end of lines)?** | End-of-line comments aligned in columns are common in declaration-heavy code. Re-indenting may break this alignment. |
| Q8 | **Should the `FormatNode` API accept an additional `indentLevel` parameter for context?** | When formatting a single node (for code fixes), the formatter needs to determine the correct indentation level. Inferring it from the node's position may not always be accurate for newly generated nodes that aren't yet inserted into a tree. |
