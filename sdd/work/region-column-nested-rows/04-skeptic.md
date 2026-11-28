Role: skeptic
Commit: 2956db2
Verdict: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (*Maintainer answers*, *Confirmed examples*), `03-spec-author.md`, the change doc, and the spec diff `origin/main...2956db2`. I did not re-argue the maintainer's answers. The production code on this branch is still `main`'s, so every "today" output below comes from `scripts/format.sh <dir>` (whole document, through the CLI). I ran each fixture a second time, and the second run reported `Formatted 0 of N file(s)`. Each fixture was a temporary directory under the session scratchpad and is not in the repository.

## Premise

**Reachable.** Every case is reached through the CLI (whole-document formatting): regions inside members, array initializers, `stackalloc`, extension blocks, and top-level statements. The code fixes that format a member reach it too (SHARED-FIX-010).

**The spec states the maintainer's answers.**
- **Region declaration.**
  - Batch 1b 1A: every region directive from a member's first token to its last is refused. The refused list in FMT-UNSUP-023 covers bodies, accessors, lambdas, switch sections, field and property initializers, parameter lists, and the member's attribute arguments.
  - "A nested type is not a member" is the nested-type bullet.
  - Confirmed example 7 (supported between members, also of a nested type) is the second example and change-doc entry 5.
  - Confirmed example 8 (refused in a method body, a field initializer, a parameter list, and attribute arguments) is the first example and change-doc entries 1–4. It also covers the CLI line and the withheld fixes.
- **Locations.** I counted every location against the example text: (12,9) in the spec, and (5,9), (4,19), (4,12), (11,8) in the change doc. All are right.
- **The spec adds positions the answers do not name.** These are the header of a type (03 Q1), top-level statements (03 Q2), and positions between a member's attribute lists or modifiers. The last follows from "first to last token". The others are asked. One position is missing, an extension block (output question 2 below).
- **FMT-BREAK-046 reproduces confirmed examples 1–6 exactly.** I printed the leading-space count of every line of the spec's output block:
  - F: `{` behind `=` (col 14), `1,`/`2` at 18, `};` at 14 — example 6.
  - b: `{` at 19, row `{` at 23, `1, 2` at 27, `},` at 23, `{ 3, 4 }` at 23, `};` at 19 — example 1.
  - e: `new[]`, `{` at 16, `1, 2` at 20, `};` at 16 — example 2.
  - f: `{` at 16, `{ 1, 2 },` at 20, `{` at 20, `3, 4` at 24, `}` at 20, `};` at 16 — example 3.
  - g: as b, with `1,` and `2` each at 27 — example 4.
  - a, h: unchanged — example 5.

  The inputs of the spec example are the confirmed inputs, and batch 1c (`{` stays behind `=`, goes under `new` otherwise) is the second bullet.
- **The text of FMT-BREAK-046 breaks on common code.** Its "written on one line" condition does not say when it is judged. Read literally, it contradicts FMT-BREAK-021 (output question 1 below).

## Mechanism and candidates

_N/A_ as a bug. These are the candidate sets the two new requirements dispatch on.

**FMT-UNSUP-023.** Roslyn always attaches a directive to the leading trivia of the token behind it, so the decision is made per token:
- the input's first token;
- `EndOfFileToken`;
- the `CloseBraceToken` of a `BaseTypeDeclarationSyntax` or a `BaseNamespaceDeclarationSyntax`;
- a token that is the first token of a `MemberDeclarationSyntax` in a type's, a namespace's, or the compilation unit's member list. That set includes `GlobalStatementSyntax`, `EnumMemberDeclarationSyntax`, `DelegateDeclarationSyntax`, namespaces, and `ExtensionBlockDeclarationSyntax`. The token can also be the first token of a `UsingDirectiveSyntax`, an `ExternAliasDirectiveSyntax`, or an `AttributeListSyntax` of the compilation unit.

`ExtensionBlockDeclarationSyntax` is parsed and formatted today (fixture g3 below), and the repository handles it in `MemberLineStartRewriter`, `LineBreakListRewriter`, RH5404, RH8031–RH8033. It derives from `TypeDeclarationSyntax`, but the spec text names it neither as a type nor as a member. I checked the base-type chains by reflection over Microsoft.CodeAnalysis.CSharp 5.12.0 (`dotnet fsi`):
- `ExtensionBlockDeclarationSyntax` → `TypeDeclarationSyntax` → `BaseTypeDeclarationSyntax` → `MemberDeclarationSyntax`;
- `GlobalStatementSyntax` → `MemberDeclarationSyntax`;
- `EnumMemberDeclarationSyntax` → `MemberDeclarationSyntax`.

**FMT-BREAK-046.** The candidates are the parents of an `ArrayInitializerExpression`:
- `ArrayCreationExpressionSyntax`;
- `ImplicitArrayCreationExpressionSyntax`;
- `StackAllocArrayCreationExpressionSyntax`;
- `ImplicitStackAllocArrayCreationExpressionSyntax` (`stackalloc[] { … }`, which the spec does not name);
- `EqualsValueClauseSyntax` (local, field, and property initializer);
- `ArrayInitializerExpression` (a row).

A collection initializer, a complex element initializer, and an object initializer are other kinds and are not candidates.

**Cheapness of FMT-UNSUP-023.** It is a cheap syntactic check under the FMT-UNSUP contract:
- For each active region directive it takes the token, makes three identity checks, and walks the ancestors that begin at that token. Nothing is evaluated outside the input.
- When the token is not the input's first one, a member node that begins at it is a descendant of the input. So the claim of locality (FMT-UNSUP-003) holds.
- No member declaration can occur inside an expression or a statement, so the "first token of a member" exception cannot leak into a nested position (FC-039). The one container that holds statements as members is the compilation unit, through `GlobalStatementSyntax`. A statement in a top-level statement's block starts at no `GlobalStatementSyntax`.

## Breaking inputs

### FMT-BREAK-046 — what "written on one line" is judged on (FC-027, FC-010)

Other rules of the line-break phase add and remove line breaks inside an array initializer's elements:
- FMT-BREAK-021 puts object, collection, and anonymous-object initializers on several lines.
- The brace rules break a switch expression's and a block lambda's braces.
- FMT-BREAK-041 joins the gaps behind a cast or `new`.

Fixtures a/f1.cs, b/g1.cs, and c/h1.cs, through `scripts/format.sh`, on `main`'s code:

```csharp
// input (statements at 8)
var x = new[] { new X { A = 1 } };
var z = new[] { new { A = 1 } };
System.Action[] w = { () => { M(); } };
var a = new[] { x switch { 1 => 2, _ => 3 } };
var c = new[] { (int)
    x };
```

```csharp
// today
var x = new[] { new X
                {
                    A = 1
                } };
var z = new[] { new
                {
                    A = 1
                } };
System.Action[] w = { () =>
                      {
                          M();
                      } };
var a = new[] { x switch
                {
                    1 => 2, _ => 3
                } };
var c = new[] { (int)x };
```

The first four inputs are written on one line, and the formatter writes them over several lines. The fifth is written over two lines, and the formatter joins it onto one. The spec allows three readings:

- **Judged on the input text, with the other rules still applied.** Pass 1 gives today's output, and pass 2 then sees a multi-line initializer and moves its braces. That breaks FMT-PIPE-004 idempotency, so this reading is not an option.
- **Judged on the input text, with "MUST NOT break a line inside it" taken literally.** A one-line array keeps its object initializers on one line, which overrides FMT-BREAK-021. The join case gets braces on lines of their own.
- **Judged on the lines the line-break phase's other rules leave.** The first four get braces on lines of their own, and the fifth stays `new[] { (int)x }`.

The last two give different visible output, so this is output question 1. The class spans:
- every element kind the formatter lays out over several lines: object, collection, anonymous-object, and `with` initializers, block lambdas, and switch expressions;
- every join FMT-BREAK-041 and FMT-BREAK-014/015 make inside an element;
- every array parent above.

No existing test pins the combined case; I found none.

### FMT-BREAK-046 — other candidates (FC-021, FC-020, FC-022, FC-027)

The test implementer must cover each of these. Everything except the first input is "none found".

- **Declaration brace written below the `=`.** `int[] b =⏎{ 1,⏎2 };`. Today FMT-BREAK-016 joins the `{` onto the `=` line (fixture a/f2.cs: `int[] b = { 1,⏎2 };`). "Stays on the line of the `=`" relies on that join, so the requirement should cite FMT-BREAK-016.
- **Empty multi-line initializer.** `int[] e = {⏎};`. Today it gives `{⏎          };`. The clause "the first element starts the line behind it" is vacuous there, and the closing brace is already on its own line, so the output stays the same.
- **Comment behind the opening brace, in front of the first element.** `int[] a = { /* c */ 1,⏎2 };`. The comment is trailing trivia of `{`. The spec decides only a comment *in front of* the brace (FC-021).
- **Comments between rows.** A comment between a row's `}` and its `,`, and one behind the `,` in front of the next row: `}, /* c */ { 3, 4 }`.
- **Directives between rows.** `#if X` between rows, and disabled text inside an initializer (FC-020).
- **Tokens behind the closing brace.** `new[] { 1,⏎2 }.Length` and `M(new[] { 1,⏎2 })`. The `.` and the `)` must stay on the brace's line. The spec's list names `,`, `}`, `;`, and `)`, and covers `.` only through "not an element".
- **Verbatim and raw strings.** A verbatim string (03 Q4) and a multi-line raw string as an element. The raw string's content lines move with its opening quotes (FMT-RAW, FC-022).
- **Implicit `stackalloc`.** `stackalloc[] { 1,⏎2 }`. Today, fixture b/g1.cs: `System.Span<int> s = stackalloc [] { 1,⏎                                 2 };`. The spec names `stackalloc` but not the implicit form.
- **Three-dimensional arrays.** `int[,,]` with a multi-line middle row and single-line inner rows.
- **Contexts.** An attribute argument (`[A(new[] { 1,⏎2 })]`) and a property initializer (`int[] P { get; } = { 1,⏎2 };`).

**Existing tests that pin today's multi-line array layout and will change.** Under *Coverage first* these are updated, not duplicated. A line search of the test sources found:
- `TrailingCommaCommentOwnerTests` (lines 35–122);
- `TrailingCommaDocumentationCommentTests` (116, 128);
- `InitializerLeadingCommaTests` (354–370, with `stackalloc`);
- `MethodChainAlignmentTests` (2222, 2236);
- `TabReplacementTests` (163);
- `ArgumentListLineStartTests` (849, 1023, 1079);
- `AttachedPartLineStartTests` (420);
- `LongExpressionLayoutTests` (203, 221);
- `RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasFormatterTests` (71);
- `CliLanguageVersionEndToEndTests` (`TopLevelUnionLocalFormatted`).

Each changed expected value is covered by batch 1b 3A, provided its case is not one that output question 1 decides.

### FMT-UNSUP-023 (FC-038, FC-039, FC-041, FC-043)

- **FC-038 — formatted output is never refused.** I traced the phases that move or rewrite tokens next to a supported region. The second pass must not be refused for any of them. In fixtures b/g2.cs and d/u1.cs, on `main`, each keeps the directive in front of a member's first token or a type's closing brace:
  - `EmptyTypeDeclarationSemicolonTransform`: `class Empty⏎{⏎    #region R⏎    #endregion⏎}` keeps its braces. It is not turned into `class Empty;`, which would put the directive in front of `;`.
  - `EnumTrailingCommaRemovalTransform`: `B,⏎#endregion⏎}` becomes `B⏎⏎#endregion // B⏎}`.
  - `AttributeListSplitTransform`: `#region A⏎[Obsolete, CLSCompliant(false)] void M() { }` puts `#region` above the first `[`.
  - Using ordering: a `#region` around `using System.Text;` ahead of `using System;` is not reordered.

  Field splitting with a region in the first token's leading trivia, which FMT-UNSUP-010 excepts, is not traced; it needs a test.
- **FC-039 — nested positions.**
  - A region inside a lambda inside a field initializer, and one inside a local function.
  - A region inside a method of a nested type at depth 2, which must refuse.
  - A region between the members of a nested type at depth 2, which must not.
  - A region inside a top-level `if` block, which must refuse, and one between top-level statements, which must not (03 Q2).
- **FC-041 — the same node through every entry point.** A method preceded by `#region` gives the same answer as a `FormatNode` input, as a `FormatNodeInDocumentAsync` target, and inside the document. The case where the answers differ is a statement preceded by `#region` inside a method: the whole document refuses it, and a node-level target is excepted. That difference is intended (FMT-UNSUP-003), and it needs one test on each side.
- **FC-043 — the context entry point.** For `FormatNodeInDocumentWithContextAsync`, the precheck's input is the context, so "the input's first token" is the context's first token.
  - With a parameter as target and the method as context, a `#region` in front of the target parameter refuses the context, and the fix is withheld.
  - With an enum member as target and the enum as context (RH5111), a `#region` in front of the member is supported, because it is in front of an enum member's first token.

  Neither case is stated, and both need a test.
- **Both sides of each listed position (FC-003).** Each of these needs a test on both sides:
  - `[assembly: …]` and `[module: …]` lists;
  - `extern alias`;
  - a using directive inside a namespace;
  - a file-scoped namespace;
  - an enum's `}`;
  - a namespace's `}`;
  - the end of the file;
  - between two attribute lists of a member;
  - between modifiers;
  - in front of a type's `{`;
  - in a base list;
  - in a primary-constructor parameter list.
- **Repository's own code.** A search for `#region` lines indented 8 or more spaces in compiled code under `src/` found one, `CasingCodeFixProviderBase{T}.cs:809`. It stands between the members of a nested type and is supported. The deeply indented regions under `tests/` are inside string literals.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` (FMT-UNSUP-023) | must change |
| `LineBreakPhase` (FMT-BREAK-046) | must change; where the decision runs among its rewriters depends on output question 1 |
| `IndentationPhase` (FMT-INDENT-023, the row's `}` under its `{`) | must change |
| `BlankLinePhase`, `RegionFormattingPhase` | must stay (spec clarifications only) |
| Code fixes | no source change; their results change: withheld by SHARED-FIX-010 for a formatted node that holds the shape, and the new array layout for the fixes that format arrays |
| Analyzers | not involved (03 F4 and F5 are follow-ups) |
| CLI | not involved in code; it reports the refusal through the existing path |
| Rule pages | not involved (no rule page names the formatter) |
| `Reihitsu.Core` | not involved |
| `AGENTS.md` phase table | changed (sixteenth shape); wording note under *Internal choices* |
| Tests | about 217 literals with a region inside a member (03), plus the array tests listed above |

## Refuse option

- **For the region column, the maintainer chose to declare the shape (escalation 10, answered).** The *Deleted* list holds only spec clauses and the special cases the earlier draft would have added. No phase code is deleted. `AGENTS.md` says that a shape which deletes nothing is not declared, but this declaration is the maintainer's explicit answer, so the rule's default is overridden. The PR should say so under `Review notes`, as 03 suggests.
- **For FMT-BREAK-046's rare subclasses, the local refusal "leave the construct as written" is available for a rare class.** The subclasses are a comment in front of a row's opening brace (03 Q3) and a comment between `{` and the first element. Option A of 03 Q3 needs no new mechanism, because it is the existing comment-led path of FMT-INDENT-023 and FMT-INDENT-030. So refusing buys nothing there.
- **Declaring "a comment in front of an array initializer's brace" unsupported** would delete nothing, because the comment-led path stays for complex element initializers. It is not an option.
- **No refuse option exists for output question 1.** The class is common code (an array of object creations).

## Output questions

### 1 · Formatter · an array initializer written on one line whose elements the formatter lays out over several lines

Input (in a method body):
```csharp
var x = new[] { new X { A = 1 } };
var c = new[] { (int)
    x };
```

Before (`main`):
```csharp
        var x = new[] { new X
                        {
                            A = 1
                        } };
        var c = new[] { (int)x };
```

After, A (recommended). Each initializer is judged on the lines the formatter's other line-break rules leave:
```csharp
        var x = new[]
                {
                    new X
                    {
                        A = 1
                    }
                };
        var c = new[] { (int)x };
```

After, B. Each is judged on the text as written, and an initializer written on one line keeps everything inside it on one line, its object initializers included:
```csharp
        var x = new[] { new X { A = 1 } };
        var c = new[]
                {
                    (int)x
                };
```

Why A: it is stable on a second pass. It gives the same braces whether the author wrote the cast on one line or two. And it keeps today's rule that an object initializer is always laid out over several lines. A third reading, judging the written text but still breaking the object initializer, changes the output again on the second pass, so it is no option.

### 2 · Formatter · regions between the members of an extension block

Input (fixture b/g3.cs):
```csharp
static class Ext
{
    extension(int x)
    {
        #region Members
        public int Twice() => x * 2;
        #endregion
    }
}
```

Before (`main`): formatted. The regions stand at column 8, framed by blank lines, and the method gets a block body.

After, A (recommended): as before. An extension block is treated like a nested type, so the regions between its members stay supported, and one in its header `extension(int x)` is refused like one in a type's header.

After, B: the input unchanged.
`error: <file>(5,9): unsupported syntax: a region directive inside a member. The file was left unchanged.`
Code fixes that format the class are no longer offered on it. Declared in this work.

Why A: an extension block holds members as a nested type does, which the maintainer kept supported. The lead may treat batch 1b 1A ("a nested type is not a member") as already covering it. Then no question is needed, only the spec naming it.

### Views on the five questions of 03-spec-author.md

1. **A region in a type's header: A.** Neither the "first to last token of a member" answer nor the nested-type answer reaches a type's header, so the question is real. A keeps one positive list of supported positions. B would need an extra exception, and it keeps today's output, which writes the directives at column 0 inside a base list.
2. **Top-level statements: A.** `GlobalStatementSyntax` is a `MemberDeclarationSyntax`, so A is the same "first token of a member" check with no exception. B would need "all top-level statements are one member", a second rule.
3. **A comment in front of a row's opening brace: A.** It is the existing comment-led path of FMT-INDENT-023 and FMT-INDENT-030, and the class is rare. B needs a line break inserted between a block comment and the brace, which no rule writes today.
4. **A line break inside a string literal: A.** "Any line break in the initializer's text" is the cheapest condition. Under output question 1 A, it is applied to the lines the phase leaves. Add a test with a multi-line raw string as an element (FC-022).
5. **`stackalloc`: A.** Also name `stackalloc[] { … }` (`ImplicitStackAllocArrayCreationExpressionSyntax`) in FMT-BREAK-046. Today it is aligned to the keyword like the explicit form: in fixture b/g1.cs, `2` stands 4 columns right of `stackalloc`.

## Internal choices

- **FMT-INDENT-010's node-level sentence contradicts FMT-PIPE-018.**
  - The sentence says the region directive in front of a node-level input's first token "keeps the block indentation of its position, the statement column inside a block and the label column between the sections of a switch statement".
  - FMT-PIPE-018 says `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` write the target's leading trivia back as it stands in the document, its directives in their columns.
  - So the column written for those entry points is the document's column, not the statement column.
  - Recommendation: reword it to say that this directive is the target's leading trivia, written back by FMT-PIPE-018. Drop the statement and label columns unless a `FormatNode` caller passes such a node. Nothing visible changes. The same applies to the change doc's *Deleted* bullet on FMT-INDENT-010.
- **FMT-BREAK-046's sentence "the opening brace instead stays on the line of the `=`" should cite FMT-BREAK-016,** which joins a brace written below the `=` (fixture a/f2.cs). It should also name `stackalloc[]` (question 5).
- **The `AGENTS.md` summary of the sixteenth shape omits positions.** It says "in front of a member's first token, a type's or namespace's closing brace, the end of the file, or the input's first token". It leaves out using directives, extern aliases, file attribute lists, and an enum's closing brace. Recommendation: "in front of the first token of a member, a using directive, an extern alias, or a file attribute list". Nothing visible changes.
- **FMT-UNSUP-023 should state, for `FormatNodeInDocumentWithContextAsync`, that "the input" is the context** (FC-043, above).
- **The order of FMT-BREAK-046's decision among the line-break rewriters.** Under output question 1 A, the decision must read what the phase's other rewriters leave inside an element. A rewriter that runs after it and adds or removes a line break inside an array initializer would make it a prediction (FMT-PIPE-025). That is the implementer's to order and the auditor's to check. It is not an architecture finding yet.

## Issue goals

- **Item 20 (a region takes the column of what it groups) is replaced by the maintainer's declaration.** Its "Done when" item, "each input has a regression test with the idempotency check", becomes a refusal test for each input. Each such test checks that the input is returned unchanged and reported. The visible change is decided by the maintainer.
- **Item 21 (rows laid out like complex element initializers) is kept, and the maintainer widened it to FMT-BREAK-046.**
- **The issue's description of today's rows is out of date.** The change doc already says so. On `main`, the rows' elements stand one level deeper than the row's line. Only the row's `}` and the shared-brace shapes differ.
- **Both items are decided by the maintainer's answers,** so I would link the issue with `Closes #1399`. That is the lead's decision.

## Architecture findings

_None._

## Follow-up candidates

- 03 F4 (RH7303 does not report the positions FMT-UNSUP-023 refuses outside executable code), F5 (no rule reports the FMT-BREAK-046 layout), and F3 as narrowed (RH7304's `IsWithinElementBody` spec gap): I agree with all three as written in 03.
- No new candidates.
