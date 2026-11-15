Role: skeptic
Commit: 35171e9
Verdict: #966: proceed narrower (the issue's prescribed solution is unsafe); #1289: output question (recommendation: the class stays reported, no production change)

My prompt carried no intended fix or verdict; the issues' own "Expected" sections prescribe solutions, which are challenged below.

## thoenissen/Reihitsu#966

### Premise

Real, reachable, and against an unchanged requirement. Scope as narrowed by the triage comment of 2026-11-15 (the `global::` sort-key part is fixed on `main`): case 1 only.

- Code: `src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs:35-49`. `CanSafelyReorder` is `HasUnsafeTrivia` (a `DirectiveTriviaSyntax` in the leading trivia of a using directive's first token or the trailing trivia of its last token) AND NOT `ContainsPreprocessorDirective`, which runs `(^|\r?\n)\s*#` over `usingDirectives.ToFullString()`. Any line of that text whose first non-blank character is `#` blocks the block — also a line inside a comment.
- Callers that reach it (all share `CanSafelyReorder`): the analyzers RH7201–RH7207 (each returns early on `false`), the code fixes through `UsingDirectiveOrderingCodeFixProviderBase` and the RH7207 provider (both via `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync` and Fix All), and the formatter's using phase (`UsingDirectiveOrderingRewriter.OrganizeUsingDirectives:76`), i.e. the CLI and whole-document formatting.
- Requirement: SHARED-USING-009 blocks a block only for a *preprocessor directive* in the trivia of its directives; comment text is not one. SHARED-DIAG-003 makes a line inside a multi-line comment comment text. No requirement demands today's silence.
- Run (fixtures in the scratchpad, deleted afterwards):
  - `scripts/apply-fix.sh RH7203` over eight inputs that hold a `#`-led line in a comment: `no-diagnostic` under LF and CRLF for every one (list under *Breaking inputs*).
  - The same command over three controls that differ only by dropping the `#` from that line: `fixed (1 action(s), 1 iteration(s))` for every one, LF and CRLF — the only cause is the `#` line.
  - `scripts/format.sh` over all eleven: the `#` inputs keep their order (the formatter changed only blank lines and the final newline), the controls are reordered (`using System;` before `using System.Linq;`).

The issue's claim "the structural check `HasUnsafeTrivia` already catches every real directive" is **false** (see *Breaking inputs*, row group B). Its prescribed solution, "the textual check is dropped; only real directives (`HasUnsafeTrivia`) block reordering", would start reordering blocks with real directives that today are protected by the regex alone.

### Mechanism and candidates

Mechanism: the safety check decides "contains a preprocessor directive" by matching text, not trivia kinds, so it agrees with the syntax tree exactly when no line of the block's full text starts with `#` outside a directive.

Dispatch: `UsingDirectiveOrderingSafety.CanSafelyReorder(SyntaxList<UsingDirectiveSyntax>)`; the blocks it is given are `CompilationUnitSyntax.Usings`, `NamespaceDeclarationSyntax.Usings`, `FileScopedNamespaceDeclarationSyntax.Usings` (`UsingDirectiveOrderingUtilities.GetUsings`). The text it reads is `ToFullString()` of each `UsingDirectiveSyntax`: leading trivia of the first token (including a file header before the first using of the compilation unit), every token, every interior trivia, and the trailing trivia of the `;`.

What can put a line-starting `#` into that text: a `DirectiveTriviaSyntax` (true positive), disabled text (only between directives — always accompanied by one), a `MultiLineCommentTriviaSyntax` (`/* */`) or `MultiLineDocumentationCommentTrivia` (`/** */`) line. `//` and `///` lines start with `/`; using directives hold no literals. So the false-positive set is exactly: a delimited comment, ordinary or documentation, with a line whose first non-blank character is `#`.

### Breaking inputs

Failure classes that reach the change: FC-001 (a narrowed guard inspects a smaller span than the decision it protects — directly applicable to the prescribed solution), FC-005, FC-020 (`#if`/`#pragma`, incl. `#if false`), FC-022 (comment-looking text), FC-033 (CRLF: run every case under both), FC-031 (the regex field's XML summary and `CanSafelyReorder`/`ContainsPreprocessorDirective` summaries describe the old check), FC-013 (analyzer, fix, and formatter must agree — they share the one helper today; a test per surface keeps it so).

Class matrix A — `#`-led comment line, today silenced (fails), must become reorderable/reported. Axes: position of the comment in the block × comment kind × block scope. Every member run with `apply-fix.sh RH7203` (LF and CRLF) unless marked:

| # | Position | Kind | Scope | Today |
|---|---|---|---|---|
| A1 | file header before the first using (leading trivia of first directive) | `/* */` | compilation unit | no-diagnostic, formatter keeps order |
| A2 | between two usings (leading trivia of a later directive) | `/* */`, indented `#` | compilation unit | no-diagnostic, formatter keeps order |
| A3 | starts behind `;` on a using's line and continues (trailing trivia of a directive) | `/* */` | compilation unit | no-diagnostic, formatter keeps order |
| A4 | file header | `/** */` | compilation unit | no-diagnostic, formatter keeps order |
| A5 | between the tokens of a directive (`using /*\n# x\n*/ System;`) | `/* */` | compilation unit | no-diagnostic, formatter keeps order |
| A6 | before the first using of a block-scoped namespace | `/* */` | block namespace | no-diagnostic, formatter keeps order |
| A7 | file-scoped namespace | any | file-scoped namespace | not run; same helper and same text |
| A8 | a commented-out directive block, e.g. `/*\n#if DEBUG\nusing X;\n#endif\n*/` | `/* */` | any | not run; same regex match |

The test implementer should cover A1 (the issue's own input), A2, A3, A4, A5, A6 or A7, A8, each under LF and CRLF, on the formatter and at least one analyzer + its code fix; one test per RH7201–RH7207 analyzer is cheap because each gates on the same call.

Class matrix B — real directive **between the tokens** of a using directive (not in the first token's leading or the last token's trailing trivia). Today blocked only by the regex; `HasUnsafeTrivia` misses it. Established with a throwaway probe referencing `Reihitsu.Core` that printed, per block, `CanSafelyReorder`, the edge-trivia check `HasUnsafeTrivia` performs, and `SyntaxNode.ContainsDirectives` per directive:

| # | Input (shape) | `CanSafelyReorder` | edge directive | directive inside node | `apply-fix.sh RH7203` |
|---|---|---|---|---|---|
| B1 | `using System.Linq;` / `using System` / `#pragma warning disable CS0105` / `.Text;` / `using System;` | False | False | True | no-diagnostic |
| B2 | `using System.Linq;` / `using System` / `#if A` / `.Text` / `#endif` / `;` | False | False | True | no-diagnostic |
| B3 | `using System` / `#if !A` / `.Text;` / `using System.Collections` / `#endif` / `;` / `using System.Buffers;` — `#if` inside one directive, `#endif` inside the next | False | False | True | no-diagnostic |

These must stay not reordered and not reported. Dropping the regex without a replacement that sees directive trivia anywhere inside the directives turns all three into reorderable blocks: B1 moves a `#pragma` relative to the other usings, B3 moves the `#if` and its `#endif` with two different directives. The class is rare (a directive between the tokens of a using), so a test per member is the whole cost. Members B1–B3 are tests the test implementer must write as "stays unchanged / not reported" on the formatter and one analyzer, so the fix cannot narrow the guard (FC-001).

Members that hold today and must keep holding: a directive in the leading trivia of a using (SHARED-USING-009 examples), `#nullable enable` before the first using, a directive after the last using (belongs to the next declaration; existing tests in `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveRequirementTests.cs`).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `UsingDirectiveOrderingSafety` | must change (the one owner of the decision) |
| Analyzers RH7201–RH7207 | must stay (they only call the helper); their output changes for matrix A |
| Code fixes RH7201–RH7207, Fix All | must stay (same helper); now offered for matrix A |
| Formatter using phase (`UsingDirectiveOrderingRewriter`) | must stay; now reorders matrix A |
| RH5114 / `SeparateDirectivesSharingALine` path | not involved (runs for unsafe blocks; unaffected for matrix B, no longer taken for matrix A) |
| Other formatter phases, precheck | not involved |
| Rule pages, `general-notes.md` | not involved (no page states the comment silence) |
| Spec SHARED-USING-009 | wording clarification, see *Internal choices* |
| Tests | Core test of `CanSafelyReorder` (none exists today), formatter regression, analyzer tests |

### Refuse option

Refusing would keep today's behavior: blocks with a `#`-led comment line are silently skipped by the analyzers, their fixes, and the formatter. That is not the refusal `AGENTS.md` *Code-fix coverage* licenses: it is an imprecise guard that gives away safe ground ("None of this licenses an imprecise guard"), and a file header written as a Markdown-like `/* # Title */` block is ordinary enough. The requested change costs no new type or mechanism; it removes a text match in favor of a check of trivia kinds. Narrowing is cheaper than refusing.

Declaring a shape unsupported: not an option. The text shape (a `#`-led comment line) costs no special case once the guard reads trivia kinds — declaring it would refuse whole files to delete nothing. The real-directive shape of matrix B (a directive between a using's tokens) would delete no guard either: the block-level refusal for directives in the edge trivia stays required. "A shape that deletes nothing is not declared."

### Output questions

_None._ Matrix A's new diagnostics, fixes, and formatter reorders are demanded by unchanged SHARED-USING-009 (only a preprocessor directive blocks). Matrix B keeps today's output.

### Internal choices

1. SHARED-USING-009 says "a preprocessor directive … anywhere in the leading or trailing trivia of its directives". Read node-wise (first token's leading, last token's trailing trivia), it would not cover matrix B and would demand reordering B1–B3; read token-wise (the trivia of any token of the directives) it covers them. Only the token-wise reading fits the requirement's own `Why:` (moving a using across `#if`/`#pragma` changes what is compiled), and the node-wise reading would make the run move a `#pragma` and split an `#if`/`#endif` pair across reordered directives — a safety-rail outcome, not an output option. Recommendation: the lead routes a one-sentence clarification to the spec author ("anywhere in the trivia of its directives, also between their tokens"); nothing visible changes against today. If the lead reads the wording as deciding the node-wise reading, it becomes an output question.

### Issue goals

- Goal 1 "The textual check is dropped; only real directives (`HasUnsafeTrivia`) block reordering": change. Dropping the textual check is right; "only `HasUnsafeTrivia`" is a prescribed solution resting on a false premise (matrix B). The goal that survives: only real directives block reordering, wherever in the directives they stand. No visible change against the issue's intent.
- Goal 1, second half ("RH7201–RH7207 report such a block again"): keep.
- Case 2 (`global::`): drop — fixed on `main` by #1735, per the triage comment.

### Architecture findings

_None._ One shared helper owns the decision for all surfaces; the defect is local to it.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1289

### Premise

Reproduces, is reachable, and contradicts the current spec — but no cheap check can decide it, and the issue itself leaves the outcome open.

- Scope as narrowed by the issue body's "Remaining" section and the triage comments: only a `@code` followed by `{` that starts its own line inside a multi-line C# comment or literal within a Razor C# host. The Q1–Q3 instances are fixed (`166e3dd`, #1692); `@functions` moved to #1452.
- Reachability: the analyzer runs on every `.razor` additional file (`RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.OnCompilation`); a Razor project feeds its `.razor` files as additional files. Real caller, real input.
- Code: `FindCodeDirectives` (`…/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs:61-95`) scans the whole file text: it skips a Razor or HTML comment whose opener starts a line, skips `@@`, and reports every `@` that starts a line (after spaces/tabs) and matches `\G@code\s*\{`. It does not know where C# starts or ends, so host kind and C# comment/literal kind are irrelevant by construction.
- Run: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~…RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"` — 53 passed. The issue's own input already has a test: `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported` (its line 7 is the issue's `@code {` inside a raw string in a `@code` host) asserts the diagnostic, and so do `VerifyCodeBlockLineInsideCSharpBlockCommentIsReportedWhileTextInMarkupCommentsIsNot`, `…InsideDocumentationBlockCommentIsReported`, `…InsideVerbatimStringIsReported`, `…InsideInterpolatedVerbatimStringIsReported`, `…InsideInterpolatedRawStringIsReported`, `…InsideBlockCommentOfFunctionsBlockIsReported`, `…InsideBlockCommentAfterClosingBracesInLiteralsIsReported`, `…InsideBlockCommentAfterNestedBracesIsReported`. Each summary says it "pins today's behavior without stating it as intended", and none carries a `Requirement` tag.
- Spec: RH2109-001 says a `@code` "that SHARED-DIAG-003 makes text … MUST NOT be reported"; SHARED-DIAG-003 makes a line inside a multi-line comment or literal its text. So the spec demands "not reported" and the code reports — the spec and the shipped behavior disagree, and the gap is recorded nowhere in `sdd/specs/analyzer/RH2109.md`.
- History (`git show 166e3dd`, its squad note): the run for #1692 built a scanner of where Razor's C# ends and where its comments and literals are; two audits found further missed real directives (braces and comment openers in template markup, builder methods, lambdas, `<text>`, inline markup — today's tests 960–1229 are those members), and the lead refused the class (decision 13) and dropped the C# lexing. That is evidence of the cost, not a guess.

### Mechanism and candidates

Mechanism: RH2109 decides "is text" only for Razor/HTML comments whose opener starts a line and for `@@`/`x@`; it reads no C# extent, so any line inside a multi-line C# comment or literal is read as Razor markup.

Dispatch: one compilation action over `AdditionalFiles` ending in `.razor`; one linear scanner. Candidate axes (from the scanner, not the issue): host (`@code {}`, `@functions {}`, `@{ }`, `@if/@foreach/@while/@switch/@try` blocks, `@( )` explicit expression) × C# extent (`/* */`, `/** */`, verbatim `@""`, interpolated verbatim `$@""`, raw `"""`, interpolated raw `$$"""`) × brace (same line, next line) × indentation (column 0, spaces, tab). Every member is reported today — the scanner ignores the host and the extent kind. Tested today: `@code` and `@functions` hosts, all six extent kinds, same-line brace.

The same mechanism has a converse member (read from the code, and recorded as accepted in #1692's note, not run here): a line inside a multi-line C# comment or literal whose first non-blank text is `@*` or `<!--` is taken as a Razor/HTML comment opener and hides every real `@code {` up to the next `*@`/`-->` — a missed diagnostic. It is outside #1289's wording ("text is reported") but is the same "no C# extent" decision; whatever the spec records for #1289 should name both directions so the gap reads as a decision.

### Breaking inputs

FC-022 (raw, verbatim, interpolated literals; comment-looking text), FC-027 (other hosts), FC-033 (CRLF), FC-034 (the regression test must be the issue's shape: `@code {` starting its own line inside the literal, not a mid-line `@code{`). Under the recommended option nothing changes in production, so the test implementer's work is to turn the existing characterization tests into requirement-tagged tests of the decided result and add the untested host members (`@{ }`, a control-flow block, next-line brace) if the lead wants the decision covered across hosts.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2109 analyzer | must stay (option A) / must change with a Razor-and-C# extent scanner (option B) |
| Code fix | not involved (RH2109 has none) |
| Formatter, CLI | not involved (`.razor` is never formatted) |
| `sdd/specs/analyzer/RH2109.md` | must change under A: RH2109-001 records the class as reported (and the converse miss); today it demands the opposite |
| SHARED-DIAG-003 | must stay; its Razor bullet is satisfied for the comment and escape forms it lists |
| Rule page `documentation/rules/RH2109.md` | not involved (no corner cases on rule pages) |
| Tests | the existing characterization tests become tests of the decided result |

### Refuse option

**Refuse (A):** the class stays reported. Cost: zero production lines, no type, no mechanism; a spec amendment that records the class in RH2109-001 (and the converse miss), and the existing tests rewritten from "pins today's behavior" to the decided result with `Requirement` tags. The class is rare: a line inside a multi-line C# comment or literal of a `.razor` file whose first text is `@code` followed by `{` — commented-out Razor or a literal holding Razor source. When the host is `@code`, the file already carries this rule's diagnostic on the host; the extra one is redundant.

**Decide with a cheap syntactic check (B):** none exists. The analyzer gets the `.razor` file as plain text, not a syntax tree, so the "nodes and trivia the tree already provides" that `AGENTS.md` *Special cases do not grow the source* calls a cheap check are not there. Deciding whether a line lies in a C# comment or literal needs (1) where Razor's C# starts and ends — brace matching across C# and Razor's markup transitions inside code (template markup, `<text>`, `@:` lines, builder methods and lambdas that hold markup) — and (2) a lexer for C# comments and every literal form, including raw and interpolated raw strings with holes. Both are "parser-like rules over text", which that section lists as machinery, and the #1692 attempt shows the cost: two audit rounds of missed real directives before it was refused. Mapping through the Razor source generator's `#line`-mapped output would predict another component's behavior and is unavailable without the Razor SDK. B is not admissible for a rare class under the repository's policy.

**Declare the shape unsupported:** not an option — the formatter precheck never receives `.razor` text, and a declaration refuses formatting, not a diagnostic.

So the policy decides A, but A means amending a requirement that today demands "not reported" for a visible output (a diagnostic). That amendment keeps today's output, yet it changes what an unchanged requirement demands and drops the issue's stated "Required output: not reported" — hence the output question below. If the lead judges that keeping today's output for a rare class puts nothing in question (as for withholding a fix), it can decide A itself and record it; the issue text explicitly left that decision to the run.

### Output questions

**1 · RH2109 · Razor @code blocks should not be used — `@code {` starting a line inside a multi-line C# literal or comment**

Input (`Counter.razor`):
```razor
@code {
    string s = """
@code {
""";
}
```

Before (`main`): reported on line 1 and on line 3.

After, A (recommended): reported on line 1 and on line 3 (unchanged; the spec records that a line inside a multi-line C# comment or literal is read like markup).

After, B: reported on line 1 only.

Why A: deciding B needs a scanner of Razor's C# extents, which this repository refuses for rare inputs and which broke twice in audits when it was last tried; the extra diagnostic only appears on commented-out or quoted Razor source.

### Internal choices

_None._

### Issue goals

- "Required output: not reported": drop under A — it is the open decision, not a goal; this changes no visible output against today.
- "Done when the input above has a regression test with the decided result": the test exists (`VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported`); under A it only needs its summary changed from "without stating it as intended" to the decided result and a `Requirement("RH2109-001")` tag. Keep.

### Architecture findings

_None._

### Follow-up candidates

_None_ beyond the converse member named above, which belongs in the same spec record rather than a separate issue.
