Role: skeptic
Commit: c9f143b7b6265130b25e0395fea8f7ac54bccf45
Verdict: #1269: proceed; #1342: proceed (one output question, behind-code comments); #1343: proceed; #1459: proceed wider (RH8305 and RH8307, every declaration directly in the compilation unit)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issues' prescribed outcomes are `agent-filed` proposals and are challenged under *Issue goals*.

Every claim below marked "ran" was settled with `scripts/apply-fix.sh <ID> <dir> --no-install` (LF and CRLF arms) on fixtures in the scratchpad, or, for RH8401 and RH8402, with probe tests in a temporary worktree of this commit (removed afterwards). Fixture texts are given inline where they matter; `⏎` is a line break.

## thoenissen/Reihitsu#1269

### Premise

Real for all three rules; every case is reached by the code fix in the IDE (a user applying the offered action) and by Fix All.

- **RH8201** (ran). `RH8201InheritdocShouldBeUsedCodeFixProvider` replaces the member's leading trivia via `ReplaceNode` and copies every other trivia unchanged, yet the results differ outside the replaced comment:
  - `plain.cs` — one ordinary `/// <summary>⏎/// Runs here⏎/// </summary>` on a four-space-indented override, nothing else around it: LF arm clean; **CRLF arm `[line-ending-drift]`, 1 `\n` written**. Same with an indented `#region B` + blank line in front (`region.cs`): CRLF arm writes 2 `\n`; the region line itself stays.
  - `directives.cs` — `    #if DEBUG⏎    #endif` before the comment and `    #pragma warning disable CS0168` between comment and member: both directives move to column 0 (LF and CRLF).
  - `indent.cs` (#1198 Q1 input, members at 6 and 2 spaces): `Run` — ordinary comment, `/// <inheritdoc/>`, and the member's first line move to column 4, braces stay at 6; `Stop` — `/// <inheritdoc/>` and `[System.Obsolete]` move to 4, the member's first line stays at 2.
  - `specexample.cs` (the RH8201-005 example): LF arm matches the spec; CRLF arm writes 5 `\n`.
  - Since the provider writes no indentation and only the detected line ending (`CreateInheritdocTrivia(endOfLine)`), the changes are applied after the provider returns, in the code action's post-processing. Not run on Windows, so whether the drift there turns into CRLF in LF files is unknown.
- **RH8202** (ran). `nested.cs` (`<summary>S <value>First</value></summary>` + `<remarks><value>Second</value></remarks>`) and `toplevel.cs` (two top-level `<value>`): `fixed (1 action(s), 2 iteration(s))` under LF and CRLF. The analyzer reports only `GetFirstDocumentingTag(…, "value")` — one diagnostic per declaration — so the second element is reported only after the first fix. SHARED-FIX-001 is violated (one diagnostic, two applications).
- **RH8204** (ran). `ExtractInnerContent` parses the element with `XElement.Parse` and writes back `Nodes().ToString(DisableFormatting)`:
  - `a &#32; <see cref='C'/> b` → `a   <see cref="C" /> b` (reference decoded, quotes and self-closing tag rewritten);
  - `a > b` → `a &gt; b`;
  - `the<placeholder> </placeholder>customer` → `thecustomer` (white-space-only content dropped);
  - two-line content in CRLF → `[line-ending-drift]`, 1 `\n`;
  - kept verbatim (pass): plain text, `a &lt; b`, `the <c>x</c>   value`, a `hint="h"` attribute on the placeholder, two-line content under LF.
  - Malformed shapes (#1201 Q1 `B`, `D`) are excluded by the issue (#1241) and were not pursued.

### Mechanism and candidates

- **RH8201**: the fix leaves the layout of the member's rebuilt leading trivia to the code action's post-processing, which lays out the trivia around the inserted `<inheritdoc/>` with its own indentation, directive column, and line ending instead of keeping them. Dispatch: `MemberDeclarationSyntax` found from the diagnostic; the trivia sequence `ReplaceDocumentation` yields. Candidates: every trivia kind that can stand in the member's leading trivia next to the comment — whitespace (indentation), end-of-line, ordinary `//` and `/* */` comments, `#if`/`#elif`/`#else`/`#endif`, `#pragma`, `#region`/`#endregion`, `#nullable`, disabled text, other documentation comments; the attribute lists and the member's first token after the comment; the `/** */` form with and without code behind `*/`.
- **RH8202**: the analyzer reports the first `<value>` of the declaration's comment and the fix removes only the reported element, so one application leaves every further `<value>`. Dispatch: `DirectDocumentationSyntaxChecker.GetFirstDocumentingTag`.
- **RH8204**: the fix replaces the element with an XML re-serialization of its content instead of removing the two tags, so every content whose serialization differs from its source text changes. Dispatch: `XElement.Parse` (success path); the textual fallback `StripPlaceholderTags` is only taken for unparsable content.

### Breaking inputs

Classes: FC-030 (RH8201), FC-007 (RH8202), FC-020/FC-021 (RH8201 directives and comments next to the comment), FC-033/FC-017 (CRLF arm; a LF-only test cannot see the RH8201 and RH8204 drift).

RH8201 matrix (trivia next to the replaced comment × document line ending):

| Member | LF | CRLF |
|---|---|---|
| nothing else, 4-space indentation (`plain.cs`) | holds | **fails** (drift) |
| indented `#region` + blank line before (`region.cs`) | holds | **fails** (drift) |
| indented `#if`/`#endif` before | **fails** (column 0) | **fails** |
| indented `#pragma` between comment and member | **fails** (column 0) | **fails** |
| ordinary comment / comment / attribute list / member at an indentation other than 4 per level (`indent.cs`) | **fails** | **fails** |
| `/** */` with `// Trailing` behind `*/` (spec example) | holds | **fails** (drift) |
| `/** */` with the member behind `*/` on the same line | member moved to its own line (see *Issue goals*) | drift |
| `#nullable`, `#else`, disabled text, `/* */` between comment and member | not run; same mechanism (the trivia sits in the span the post-processing lays out) | not run |

RH8202 matrix (number × placement of `<value>` outside `<code>`/`<example>`): two top-level — **fails**; two nested in different elements — **fails**; one top-level + one nested — not run, same code path; one element — holds (existing tests).

RH8204 matrix (content kind): character reference — **fails**; `>` — **fails**; attribute quotes or self-closing form of a nested element — **fails**; white-space-only — **fails**; multi-line under CRLF — **fails**; `&quot;`/`&apos;`/CDATA/XML comment in the content — not run, same re-serialization path (candidates for the test implementer); plain text, `&lt;`, `&amp;`-free nested `<c>`, attribute on the placeholder, multi-line LF — hold.

Tests the test implementer must cover: each failing member above, each with a CRLF variant (the issue's "Done when" asks for it, and FC-033 makes it necessary for RH8201: the plain case fails only under CRLF).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8201 code fix | must change |
| RH8202 code fix | must change (see *Internal choices*) |
| RH8202 analyzer | must stay (one diagnostic per declaration; see *Internal choices*) |
| RH8204 code fix | must change |
| RH8201/RH8202/RH8204 analyzers | must stay |
| Fix All (BatchFixer) | not involved beyond following the single fix |
| Formatter | not involved (none of these fixes calls it) |
| `Reihitsu.Core` | not involved |
| Rule pages RH8201/RH8202/RH8204 | not involved |
| Specs RH8201-005, RH8202-003, RH8204-003 | coverage sentences must widen (spec author: CRLF, indentation other than 4, indented directives, several `<value>` elements, the content kinds above) |

### Refuse option

- RH8201: withholding the fix for indented directives or odd indentation is cheap, but the CRLF drift occurs on the plainest input (`plain.cs`), which is common code; refusing that would withhold the fix for every CRLF file. Not an option for the drift; for odd indentation and indented `#if`/`#pragma` next to the comment, withholding is a rare-class option, but the same mechanism causes all three, so a fix of the mechanism costs no more.
- RH8202: withholding for a comment with more than one `<value>` is a cheap check on a rare class; removing every `<value>` the rule would report in one application is as cheap (same element-removal helper the fix uses).
- RH8204: withholding for content with references, `>`, nested markup, white-space-only content, or CRLF multi-line content would withhold the fix for common content (`<see cref="…"/>` inside a placeholder is ordinary). Removing only the start and end tag needs no new type.
- Declaring a shape unsupported: not an option — no formatter phase is involved in any of the three fixes.
- Existing helper: `DocumentationCommentCodeFixUtilities` (`src/Reihitsu.Analyzer.CodeFixes/Core/`) is already used by the RH8202 and RH8204 fixes.

### Output questions

_None._

### Internal choices

- RH8202: (A) keep one diagnostic per declaration and let its fix remove every `<value>` the rule would report in that comment; (B) report each `<value>` element with its own diagnostic. SHARED-FIX-001 ("the corrected code is everything the rule reports at the place it checks", the place being the declaration's comment) already demands A with today's diagnostic, so A puts nothing in question; B would change the diagnostics, which no requirement demands, and would itself be an output question. **Recommend A.**
- RH8201, `/** <summary>…</summary> */ public override void Stop()` (member behind `*/` on one line, outside RH8201-005's coverage): RH8201-005 already puts an ordinary comment that followed `*/` on its own line "indented like the member"; the same reading puts the member on the line after `/// <inheritdoc/>` at the column the `/**` had. For four-space indentation that is today's output, so nothing visible changes there. **Recommend that reading**; withholding the fix for this rare shape is the alternative and needs no question either.

### Issue goals

- "`/** */` and the member are joined" listed as a defect: drop it. A `///` comment ends at the line end, so `/// <inheritdoc/>` cannot share its line with the member; moving the member to the next line is required, only its column is in scope (see *Internal choices*). No visible change against today for standard indentation.
- "A CRLF variant of each test passes": keep; it is what exposes the RH8201 and RH8204 drift.
- "Each input has a regression test": keep.

### Architecture findings

_None._ The RH8201 defect is local to one fix (its result is laid out by a component outside the repository instead of by the fix or `ReihitsuFormatter`).

### Follow-up candidates

- RH8202 does not see a `<value>` in a second documentation comment of the same declaration (`GetDocumentationComment` returns the first trivia only); outside RH8202-001's coverage. Not run.

## thoenissen/Reihitsu#1342

### Premise

Real (ran), except that the RH8402 blank line is a different mechanism from the other three. Reached by the code fixes in the IDE and Fix All.

- **RH8305** (`RH8305…CodeFixProvider` formats the whole owning `MemberDeclarationSyntax` with `ReihitsuFormatter.FormatNodeInDocumentAsync`):
  - `void M(){int a=1;⏎a++;}` → body re-laid out (braces, spacing, a blank line inserted between the statements);
  - a reported summary on a class in a namespace (`typesummary.cs`) → every nested member re-laid out: `int A(int a) => a;` becomes a block body, `void B(){…}` re-laid out, `///Text` of a nested member becomes `/// Text`, the blank line between two of its documentation comments removed;
  - `///Text` in the same comment as the reported summary (`sameleading.cs`) → `/// Text`;
  - a blank line between another documentation comment and the reported one in the same leading trivia (`blankbetween.cs`) → removed;
  - an empty type body `{ }` on the documented type → `public class C;` (`innamespace.cs`, `filescoped.cs`, `nestedtop.cs`);
  - `int Z; /// <summary>D</summary>⏎int D;` and `int A; /// <summary>Value</summary>⏎int B;` → comment moved off the code line and a blank line inserted.
  - Holds: a `///Text` in front of a *different* sibling member (`neighbour.cs`) stays — the #1310 Q3 "neighbouring" instance only occurs when that member is inside the formatted declaration.
- **RH8307** (same entry point and target): `void M(int v){int a=1;}` body re-laid out; `int Z; /// <remarks>R⏎/// more</remarks>` and `int A; /// <summary>Total⏎/// sum</summary>` moved off the code line with a blank line inserted; a class summary re-lays out the class's members (`typenested.cs`).
- **RH8309** (reorders the comment itself, then formats the owning declaration): `int A(int a) => a;` → block body; `void I<T>() { }` → braces on their own lines; `int Z; /// <remarks>R</remarks>⏎/// <summary>…` → moved off the code line, blank line inserted; `public class C { }` → `public class C;`.
- **RH8402** (probe test, spec configuration): `// Copyright (c) Old Corp.⏎⏎namespace N;⏎` → header followed directly by `namespace N;`; the blank line is gone. `// Orders⏎⏎namespace N;⏎` (insert path) keeps it. `GetLeadingHeaderSpan` advances its end over every whitespace and end-of-line trivia before the first token, so the replaced span ends at the first token.

### Mechanism and candidates

- **RH8305, RH8307, RH8309**: the fix formats the whole declaration that owns the reported comment, so every formatter phase decides everything in that declaration — its signature, body, nested members, attribute lists, and every trivia in its leading trivia — not only the reported element. Dispatch: the target passed to `FormatNodeInDocumentAsync` (`FindDeclaration`: first `MemberDeclarationSyntax` or `EnumMemberDeclarationSyntax` ancestor of the diagnostic). Candidates: what any phase changes inside a declaration — structural transforms (expression body → block for the method-like kinds, empty type body → `;`, field split, attribute-list split/merge), line breaks, blank lines, horizontal spacing, indentation, documentation-comment normalization of the other lines and comments in the same leading trivia, and the relocation of a behind-code `///` with its blank line; for a type declaration, all of that in every nested member.
- **RH8402**: on the replace path the fix's replaced span runs from the start of the file through every line break before the first token, so the blank lines between the old header run and the code are replaced along with the comments. Dispatch: `GetLeadingHeaderSpan`. Candidates: one or several blank lines after the run; the run followed directly by code (nothing to keep); the run followed by a directive or a documentation comment; leading blank lines before the run.

### Breaking inputs

Classes: FC-030 (all three formatting fixes), FC-025 (expression-bodied kinds the structural transform converts), FC-040 (behind-code `///` is documentation only in `Parse`/`Diagnose` mode), FC-033.

Matrix (fix × what lies outside the reported element in the formatted declaration):

| Outside the element | RH8305 | RH8307 | RH8309 |
|---|---|---|---|
| misformatted body | **fails** | **fails** | not run, same entry point |
| expression-bodied method | **fails** (in a type's nested member) | not run | **fails** |
| `{ }` body on one line | **fails** (`class C;`) | not run | **fails** (`void I<T>() { }`, `class C;`) |
| nested members of a documented type | **fails** | **fails** | not run, same entry point |
| `///Text` in the same leading trivia | **fails** | not run | not run |
| blank line between two doc comments of the declaration | **fails** | not run | **fails** (`blank.cs`, see #1343) |
| comment behind code (`int Z; /// …`) | **fails** | **fails** | **fails** |
| already formatted declaration | holds (existing tests) | holds | holds |

RH8402: one blank line after the run — **fails**; non-copyright comment (insert path) — holds; two blank lines, a directive or a doc comment after the run — not run, same span computation (candidates).

Every member is reached through the IDE fix and Fix All; the test implementer covers each failing cell and one "not run" cell per column with the same entry point.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8305, RH8307, RH8309 code fixes | must change (what they format) |
| RH8402 code fix | must change (the replaced span's end) |
| Analyzers RH8305/RH8307/RH8309/RH8402 | must stay |
| `ReihitsuFormatter` / formatter phases | must stay; FMT-PIPE-010 is right (see #1459). Code fixes reach the formatter's documentation layout only through `ReihitsuFormatter` (FMT-PIPE-009) |
| `ReihitsuFormatter.FindUnsupportedSyntax` call in each fix | must follow whatever node the fix formats after the change (SHARED-FIX-010, FC-043) |
| Rule pages | not involved |
| Specs RH8305-004, RH8307-004, RH8309-004 | coverage sentence "a declaration whose code outside the reported element is already laid out as the formatter lays it out" must go (spec author); RH8402-005's coverage excludes the copyright replace path, so the blank-line behaviour on that path needs a requirement or a widened coverage |

### Refuse option

- Withholding the fix where the declaration is not already laid out as the formatter lays it out would withhold it for code people commonly write, and deciding it needs formatting the declaration and comparing — a prediction, not a cheap check. Not an option (it is escalation 9 territory, not a rare class).
- For the behind-code comment: withholding the RH8305/RH8307/RH8309 fix when the comment's first `///` starts behind a token on its line is a cheap check on a rare class (documentation written behind code). See *Output questions*.
- RH8402: no refuse option worth taking; the blank line is a span boundary.
- Declaring a shape unsupported: not an option; none of these is a shape the formatter mishandles — the formatter does what the fix asked of it.
- Existing helper: `DocumentationCommentCodeFixUtilities` serves RH8304, whose fix already conforms (SHARED-DOC-004 names RH8304 and RH8308 as conforming). Constraint for the implementer: the summary/element layout is owned by the formatter's documentation phase (FMT-DOC-008/009); a copy of it in a fix would be an FC-015 duplicate.

### Output questions

**1 · RH8305 · Summary element must span at least three lines (same case for RH8307 · element text on the line of its closing tag, and RH8309 · element order) — documentation comment written behind code**

Input:
```csharp
class C
{
    int A; /// <summary>Value</summary>
    int B;
}
```

Before (`main`):
```csharp
class C
{
    int A;

    /// <summary>
    /// Value
    /// </summary>
    int B;
}
```

After, A (recommended): no fix offered; the diagnostic stays.

After, B:
```csharp
class C
{
    int A; /// <summary>
    /// Value
    /// </summary>
    int B;
}
```

Why A: documentation behind code is rare, the comment must stay where it is, and laying it out in place would need the formatter's documentation layout without its relocation, which no entry point offers today. Note for the lead: withholding a fix for a rare class puts nothing in question by itself (`README.md`, *What reaches the maintainer*), but the #1342 comment states B as the expected result, so taking A drops that goal; B's continuation-line column follows RH8304's behind-code rule (only the line's own indentation is repeated) and is decided nowhere for these three rules.

### Internal choices

- RH8402: the old header run is replaced as today (#1342 states replacing the run is intended); only the blank lines between the run and the first token stay. Whether the spec author records the replace path in RH8402-005 or a new requirement is internal.

### Issue goals

- RH8305 #1310 Q3 "a neighbouring `///Text` is rewritten": keep, but its premise is narrower than stated — it occurs only for a `///Text` inside the formatted declaration (same leading trivia, or a nested member of a documented type), not for a sibling member.
- "Each input has a regression test": keep.
- Behind-code expected result (#1342 comment): see *Output questions* 1.

### Architecture findings

_None._

### Follow-up candidates

- RH8309 fix on an accessor's comment (`/// <remarks>R</remarks>⏎/// <summary>G</summary>⏎get => 0;`): ran, `no-progress (2 iteration(s))` — the fix edits the property's comment and only expands the accessor summary; #1314 Q4 part 4, not part of #1343 (which takes part 1 only). Parts 2 and 3 (inside a method body, after an attribute list) were not run.

## thoenissen/Reihitsu#1343

### Premise

Real (ran) for all three, with one correction for RH8401.

- **RH8308**: `/// <summary>C</summary> a &amp; b` → `fixed (1 action(s), 3 iteration(s))`. `TryGetFirstMeaningfulTextSpan` ends the span at the end of the first text token with a non-white-space character; Roslyn lexes `&amp;` as its own token inside the same `XmlTextSyntax`, so the span covers `a ` only, then `&amp;`, then `b`. One diagnostic at the start, three applications.
- **RH8309**: two `///` comments of one declaration separated by a blank line, out of order in the second (`blank.cs`: `<summary>` ⏎ blank ⏎ `<remarks>`, `<param>`; `blank2.cs`: `<remarks>` ⏎ blank ⏎ `<summary>`, `<remarks>`, `<param>`) → `fixed (1 action(s), 2 iteration(s))`. The analyzer judges each `DocumentationCommentTriviaSyntax` on its own; the fix reorders `DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration)`, which is the first trivia, and the formatting of the declaration then removes the blank line, so the second application sees one joined comment. Separated by an ordinary `// comment` or by a `/** */` comment instead: `no-progress (1 iteration(s))` — the fix is offered and changes nothing (SHARED-FIX-005, FC-009).
- **RH8401**: `/// first` ⏎ `<U+FEFF>/// second` in a method body (probe test): **both lines are reported from the start** (RH8401-003, one diagnostic per line), and Fix All needs two iterations (`Expected '1' iterations but found '2'`). apply-fix: `fixed (1 action(s), 2 iteration(s))`; the same with U+001A, and with the stray character on the third of four lines (lines 1, 2, 4 converted first). The issue's "reported again" is inaccurate — the line was reported all along — but the defect stands: the fix applied to the second line's diagnostic does not correct that line (SHARED-FIX-001), and Fix All needs two runs (SHARED-FIX-002). Cause: the fix re-scans the comment text with `char.IsWhiteSpace` (`DocumentationCommentUtilities.RewriteDocumentationLine`, `Reihitsu.Core`), which is false for U+FEFF and U+001A, while the lexer treats both as white space in front of the exterior.
- **RH8401 rule page**: `documentation/rules/RH8401.md` has a *Code fix behavior* section whose second paragraph says the line "keeps its three slashes after the fix has run" and must be corrected by hand. Besides being incomplete (a second application converts it), the section lists a corner case and describes implementation, which `AGENTS.md` (*Key conventions*, rule pages) excludes from rule pages whatever the fix does.

### Mechanism and candidates

- **RH8308**: the reported span is the rest of one text token, not the rest of the text, so any text after a closing tag that holds more than one text token on its line is reported and removed piece by piece. Dispatch: `XmlTextSyntax.TextTokens`. Candidates: named entity references, decimal and hexadecimal character references, at the start, in the middle, at the end, several.
- **RH8309**: the fix edits the declaration's first documentation comment, the analyzer reports in any of them, so a diagnostic in any comment but the first is not corrected by its own application. Dispatch: `GetDocumentationComment` (first `DocumentationCommentTriviaSyntax` of the leading trivia). Candidates: separator between the comments (blank line, ordinary comment, `/** */`, directive), which comment is out of order.
- **RH8401**: the fix and the analyzer use two definitions of the white space that may stand before a line's `///`, so a line whose prefix holds a character only the lexer counts as white space is reported but not converted. Dispatch: `RewriteDocumentationLine` versus the lexer's exterior trivia. Candidates: exactly U+FEFF and U+001A (the characters the lexer accepts as white space and `char.IsWhiteSpace` does not), on any line after the first.

### Breaking inputs

Classes: FC-007 (all three), FC-009 (RH8309 with an ordinary or `/** */` comment between), FC-008 (RH8401 Fix All), FC-015 (RH8401: two definitions of white space).

| Rule | Member | Result |
|---|---|---|
| RH8308 | `a &amp;` | **fails** (2 iterations) |
| RH8308 | `&amp; b` | **fails** (2) |
| RH8308 | `a &amp; b` | **fails** (3) |
| RH8308 | `a &#32; b` | **fails** (3) |
| RH8308 | `a &#x41; b` | **fails** (3) |
| RH8308 | `a &lt; &gt; b` | **fails** (4) |
| RH8308 | `a b`; `a <c>x</c> b` (two diagnostics by RH8308-001) | holds |
| RH8309 | blank line between, second out of order | **fails** (2 applications) |
| RH8309 | ordinary comment between | **fails** (no-progress) |
| RH8309 | `/** */` between | **fails** (no-progress) |
| RH8309 | directive between | not run, same dispatch |
| RH8309 | first comment out of order | holds (the fix edits it) |
| RH8401 | U+FEFF on line 2 / on line 3 of 4 | **fails** |
| RH8401 | U+001A on line 2 | **fails** |
| RH8401 | only spaces/tabs before every `///` | holds |

Rarity: an entity after a closing tag, two documentation comments on one declaration, and a U+FEFF/U+001A inside a file are each rare. Each is reached through the IDE fix and Fix All.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8308 analyzer | must change (the span; RH8308-003 says "to the end of the line, or the start of the next XML node", and a reference is character data, not a node) |
| RH8308 code fix | must stay unless the span change requires otherwise (it removes the reported span) |
| RH8309 code fix | must change (withhold or edit the reported comment) |
| RH8309 analyzer | must stay |
| RH8401 code fix / `DocumentationCommentUtilities` (`Reihitsu.Core`) | one of them must change; the helper has other callers (`RewriteDocumentationLines` serves exterior normalization too), so a change there reaches them |
| RH8401 analyzer | must stay |
| `documentation/rules/RH8401.md` | must change (drop the corner-case paragraph; the first paragraph is implementation detail as well) |
| Specs RH8308-001/003, RH8309-004, RH8401-004 | coverage sentences widen or a withheld case is recorded (spec author) |

### Refuse option

The issue's "Done when" admits withholding for each input, so none of the following puts a visible output in question.

- RH8308: withholding does not help — the defect is the span. Widening the span to the end of the text is the cheap change.
- RH8309: withhold the fix when the declaration's leading trivia holds more than one documentation comment (or the diagnostic lies in another comment than the one the fix edits) — a cheap trivia check on a rare class. Sorting across comments would need rules for the separators between them (blank line kept or not, ordinary comment's slot) — more than a cheap check.
- RH8401: withhold the fix when a line of the comment has U+FEFF or U+001A in front of its `///`, or count those two characters as white space in front of the exterior; both are one character test. The second makes the fix agree with the analyzer.
- Declaring a shape unsupported: not an option; no formatter phase is involved.

### Output questions

_None._

### Internal choices

- RH8309 joined comments: (A) withhold for a declaration with more than one documentation comment; (B) sort the joined documentation in one application. **Recommend A**: rare class, cheap check, and B has undecided separator handling that would itself become an output question. Both are inside the issue's "Done when".
- RH8401: (A) convert lines with U+FEFF/U+001A before `///`, (B) withhold for such a comment. **Recommend A**: same cost, removes the disagreement between analyzer and fix, and is what RH8401-004 states for covered input. If the change goes into `DocumentationCommentUtilities`, its other callers have to be checked (scope growth the lead decides).

### Issue goals

- RH8308 "the span runs to the end of the line": keep; RH8308-003 already says so for text without references.
- RH8401 "every line is converted in one application (or the fix is withheld)": keep; restate the premise as "the second line is reported from the start, and its own fix does not convert it".
- "`RH8401.md` matches the behavior": change to "remove the *Code fix behavior* corner-case text" — whatever the fix does, the rule page must not describe it (`AGENTS.md`, rule pages; withholding is covered by `general-notes.md`).
- The comment on #1343 (RH8305 at the start of a file) is the instance of #1459; handled there.

### Architecture findings

- RH8401: the RH8401 analyzer (lexer's `DocumentationCommentExteriorTrivia`) and `DocumentationCommentUtilities.RewriteDocumentationLine` (`char.IsWhiteSpace`) each decide what may stand in front of a line's `///`; they disagree on U+FEFF and U+001A. Input: `/// first⏎<U+FEFF>/// second`. The class is rare (refuse-first applies).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1459

### Premise

Real and wider than the title (ran).

- RH8305, `/// <summary>S</summary>⏎public class C⏎{⏎}` as the whole file: `no-progress (2 iteration(s))`, LF and CRLF. The first application changes only `{ }` into `public class C;` (an FC-030 change), the summary stays one line.
- The same `no-progress` for: a struct, an interface, an enum (2 iterations each), a delegate and a positional record (1 iteration: the action changes nothing at all — SHARED-FIX-005, FC-009); the second of two top-level types; a type after `using System;`; a type after a `// File header` comment.
- RH8307, `/// <summary>Total⏎/// sum</summary>⏎public class C { }` at file level: `no-progress` as well — same class, not named in the issue.
- Holds: a type in a block namespace, in a file-scoped namespace, and a type nested in a file-level type (each still gets the `{ }` → `;` change of #1342). RH8309 at file level reorders correctly (it edits the comment itself), but also turns `{ }` into `;`.
- Cause: `FormatNodeInDocumentAsync` keeps the target's original leading trivia when the target is a direct child of the root (`keepsLeadingTrivia = targetNode.Parent == root || …`, `src/Reihitsu.Formatter/ReihitsuFormatter.cs:208`), as FMT-PIPE-010 requires. RH8305 and RH8307 depend on that entry point rewriting the comment in the target's leading trivia, which it never does for such a target.

### Mechanism and candidates

The RH8305 and RH8307 fixes have the formatter lay out a comment that sits in the leading trivia of the node they pass, and `FormatNodeInDocumentAsync` writes that leading trivia back unchanged whenever the node is a direct child of the compilation unit, so the comment of every file-level declaration stays as it was. Dispatch: `FindDeclaration` (the target) and `FormatNodeInDocumentAsync`'s `keepsLeadingTrivia`. Candidates: every `MemberDeclarationSyntax` that can be a direct child of `CompilationUnitSyntax` — class, struct, interface, enum, delegate, record, record struct, and global statements (the last not run; whether a doc comment there is reported at all was not checked) — for each rule whose fix relies on the formatter to rewrite the comment (RH8305, RH8307), and a target whose first token shares its line with the token before it (the entry point's second keep condition; not reachable for a reported comment that starts its own line).

### Breaking inputs

Classes: FC-007, FC-009, FC-016 (RH8307 is the sibling the issue did not name), FC-030 (the `{ }` → `;` change on the namespace and nested cases).

| Declaration directly in the compilation unit | RH8305 | RH8307 | RH8309 |
|---|---|---|---|
| class (first, second, after `using`, after file header) | **fails** | **fails** (class, first) | holds (but `{ }` → `;`, #1342) |
| struct, interface, enum | **fails** | not run, same path | not run |
| delegate, record | **fails** (no change at all) | not run, same path | not run |
| in block namespace / file-scoped namespace / nested in a file-level type | holds (but `{ }` → `;`) | holds | holds |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8305 and RH8307 code fixes | must change |
| `ReihitsuFormatter.FormatNodeInDocumentAsync` | must stay — FMT-PIPE-010 requires keeping a root child's leading trivia, and its `Why:` (fixes change only their code) holds; the defect is the fixes' dependence on it |
| RH8309 code fix | not involved for this issue (works at file level) |
| Analyzers | must stay |
| Spec RH8305-004 (and RH8307-004) | no coverage exclusion names file-level declarations; the requirement already demands the result |

### Refuse option

Withholding the fix for file-level declarations would withhold it for code people commonly write (types in the global namespace, top-level-statement programs' types), so it is not a rare-class refusal (escalation 9 if anyone proposes it). The same change that makes RH8305/RH8307 format only the reported element (#1342) removes this class, since the class exists only because the fixes format the declaration to reach its leading trivia. Declaring a shape unsupported: not an option.

### Output questions

_None._ The required output in the issue (summary on three lines, `{ }` kept) is what RH8305-004 and SHARED-FIX-006 demand.

### Internal choices

_None._

### Issue goals

- Title "the first type in a file without a namespace": widen to every declaration directly in the compilation unit, and to RH8307. No visible output beyond the requirement's.
- Required output keeps `{ }`: keep; it agrees with #1342 and SHARED-FIX-006 (today's namespace case turns it into `;`).

### Architecture findings

_None._ Note for the lead: FMT-PIPE-010's keep rule and the fixes' choice of target are consistent with the specs on the formatter side; the defect is that RH8305/RH8307 put the code they change into the part of their target the entry point is specified not to touch.

### Follow-up candidates

_None._

## Whole run

- Fixtures (scratchpad, deleted with the session): `fx/`, `fx2/`, `fx3/` under `/tmp/claude-0/-home-user-Reihitsu/49fa1167-c3f4-5924-9935-fba78fc5a582/scratchpad/`. Probe tests ran in a temporary `git worktree` of `c9f143b`, removed afterwards; no repository file other than this handover was changed.
- #1342 and #1459 share one cause for RH8305 and RH8307 (the fix formats the declaration); #1343's RH8309 two-application case also passes through it (the formatting removes the blank line that joins the comments). The lead may treat them as one change.
- One output question for the batch before the test implementer: #1342 *Output questions* 1.
