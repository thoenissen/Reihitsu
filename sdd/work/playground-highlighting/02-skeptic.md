Role: skeptic
Commit: 4546a38
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The maintainer answers 1–6 in `00-lead.md` are treated as binding; nothing below argues against them.

## Premise

Real. thoenissen/Reihitsu#1941 describes the Playground page itself, which every Playground user reaches; it is a caller of the formatter (`PlaygroundFormatter.Format` → `ReihitsuFormatter.FormatSyntaxTree`), not a formatter input question, so *Supported inputs* is met by the page.

Checked against `main`'s code at 4546a38:

- No highlighting: `src/Reihitsu.Playground/Pages/Home.razor` renders two plain `<textarea>` elements.
- Wrapping: neither text area sets `wrap`, and `wwwroot/css/app.css` (`.playground-column textarea`, around line 140) sets no `white-space`, so both use the browser default soft wrap.
- No location for syntax errors: `Home.razor.cs` `FormatInput` sets only the fixed sentence; `PlaygroundFormatter.HasSyntaxErrors` returns a bool and discards the diagnostics.
- The message is `<p class="playground-message">` in red text (`app.css` `.playground-message`).

The spec author's measured values hold. Probe (a throwaway console project in the session scratchpad referencing `src/Reihitsu.Formatter`, `InvariantGlobalization` on, deleted afterwards), `CSharpSyntaxTree.ParseText(src).GetDiagnostics()`:

| Input | Diagnostics (span, line/column, message) |
|---|---|
| `public class Sample{ int x }` | CS1002 `[27..28)` line 1, col 28, on `}`, `; expected` |
| `public class Sample{public int Add(int a,int b){return a+b}` | CS1002 `[58..59)` line 1, col 59, on `}`; CS1513 `[59..59)` line 1, col 60, end of text |
| PLAY-008 example | unsupported, `[41..48)` = `/* c */` |

## Mechanism and candidates

_N/A_ — behavior change, not a bug.

## Breaking inputs

Failure classes that reach the change: FC-017 (an assertion weaker than the guarantee), FC-022 (raw and interpolated strings, comment-looking text inside a literal), FC-023 (verbatim identifiers), FC-026 (several identical tokens on one line, so a mark is found by text instead of by offset), FC-033 (the harness normalizes the scenario away: bUnit input vs. the browser's text area value), FC-040 (documentation comments as structured trivia). The formatter classes do not reach it: no formatter, analyzer, or code-fix code changes.

### Error and unsupported marks (PLAY-014) — class matrix

Axes: span width × where the span sits × relation to other marks. Each member was settled with the probe above; all are reachable through the page's "Format" button.

| Member | Input (exact) | Roslyn / formatter result | Today's spec |
|---|---|---|---|
| empty, mid-line, in front of a character | `class C { string s = "abc\n; }` | CS1010 `[21..21)` at `"`, `Newline in constant` | "one character wide at its position" → the `"` — decided |
| empty, mid-line, in front of a character | `class C { /* open` | CS1035 `[10..10)` at `/`; CS1513 `[17..17)` end of text | decided (the `/`; end of text as in the example) |
| **empty, at a line end** (in front of `\n`) | `class C\n{\n    int x\n}\n` | CS1002 `[19..19)`, line 3, col 10; the character at 19 is `\n` | **not decided** — see Output question 3 |
| **empty, at a line end** | `class C\n{\n    void M()\n    {\n        var a = 1\n        var b = 2;\n    }\n}` | CS1002 `[46..46)` line 5, col 18, character `\n` | same |
| empty, at a line end, first line | `using System\nclass C {}` | CS1002 `[12..12)` line 1, col 13, character `\n` | same |
| empty, end of text | `class C {` | CS1513 `[9..9)` | decided by the PLAY-014 example |
| empty, identical duplicates | `class C { void M() { if (true) { }` | CS1513 `[34..34)` twice | listed twice (PLAY-006); two identical marks |
| one character, adjacent to an empty mark | PLAY-006 example 2 | `[58..59)` and `[59..59)` | decided |
| one character, mid-line | `class C { void M() { int x = ; } }` | CS1525 `[29..30)` on `;` | decided |
| several characters | `class C { void M() { foo bar baz; } }` | CS1002 `[29..32)` on `baz` | decided |
| inside comment trivia | PLAY-008 example | `[41..48)` `/* c */` | decided |
| span ending in a line break | `class C\n{\n    int a = 1, b = 2\n#if X\n        , c = 3\n#endif\n        ;\n}` | unsupported (`a directive inside a field declaration with several declarators`), `[31..37)` = `#if X\n` — the directive's span **includes the `\n`** | decided; a test that expects the mark's text to be `#if X` fails |
| same, expression body | `class C\n{\n    int M() =>\n#if X\n        1;\n#else\n        2;\n#endif\n}` | unsupported, `[25..31)` = `#if X\n` | decided |
| after a tab | `class C { void M() { int x = 1\t}` | CS1002 `[31..32)` col 32 (a tab counts one), CS1513 `[32..32)` | decided; the mark is placed by offset, not by column |

For FC-026: the test inputs should hold the marked character more than once on its line (several `}`), so a mark found by text instead of by offset fails. PLAY-006 example 2 holds `}` only once in front of the error; `class C { void M() { int x = 1\t}` or `class C { void M() { if (true) { }` are better.

Existing tests: none pins a mark or the error list. `InputWithSyntaxErrorIsShownUnchangedWithMessage` (`HomePageTests.cs:181`) and `HiddenCodeBesideASyntaxErrorShowsTheSyntaxErrorMessage` (`:347`) assert the message's `TextContent` equals the bare sentence, and must be updated for PLAY-006's list.

### Highlighting (PLAY-010) — token and trivia kinds, from the probe

Confirmed:

- `partial`, `async`, `await` (also in a non-`async` method), `get`, `set`, `where` (constraint), `from`/`in`/`select` (query), `true`, `null` get keyword kinds; `var`, `nameof`, `dynamic`, `value`, `record` (as a type name) are `IdentifierToken`.
- `$"a{1:N2}b{2,5}"`: `$"` `InterpolatedStringStartToken`, `a` and `b` `InterpolatedStringTextToken`, `{` `}` `OpenBraceToken`/`CloseBraceToken`, the format clause `N2` is an **`InterpolatedStringTextToken` inside the hole**, the alignment `,` a `CommaToken`, `"` `InterpolatedStringEndToken`. By kind, `N2` is a string; by "not an interpolation hole … its code is coloured by its own tokens" it is also a string (its own token is a text token). Consistent; worth one test.
- `"""raw"""` is one `SingleLineRawStringLiteralToken`.
- `/// <see cref="int"/> <param name="a">x</param>` is `SingleLineDocumentationCommentTrivia` whose descendant tokens include **`IntKeyword` (`int` in the cref)** and **`IdentifierToken`s** (`see`, `cref`, `param`, `name`, `a`). See *Internal choices*, 1.
- Directives: `#region`, `#pragma warning disable`, `#define` give keyword-kind tokens (`RegionKeyword`, `PragmaKeyword`, `WarningKeyword`, `DisableKeyword`, `DefineKeyword`) and identifier tokens (`CS1`, `X`, `DEBUG`) inside the directive. The PLAY-010 example decides that `#if DEBUG` is a directive, so directive wins over keyword/identifier there.
- Comment behind a directive, by kind:
  - `#endregion // Methods`, `#if DEBUG // c1`, `#else // c2`, `#endif // c3`, `#nullable enable // n`, `#line 5 "f" // l`, `#pragma warning disable CS1 // why` → the `//` part is `SingleLineCommentTrivia`, a comment.
  - `#region X /* c */` and `#error a // b` → **the whole text behind the keyword is `PreprocessingMessageTrivia`**; Roslyn sees no comment there. See *Internal choices*, 2.
- `#endregion // Methods` is the canonical form the region formatting phase writes, so it appears in the output pane for most real inputs that have regions.
- Stray `}` (CS1022) is `SkippedTokensTrivia`; `DescendantTokens()` without `descendIntoTrivia` does not return it, and it holds a token of a punctuation kind (normal colour either way). No probe produced a skipped keyword.

Candidate inputs to cover (none found pinned by an existing test, since no highlighting exists): the two PLAY-010 examples; a doc comment with a keyword in a cref; `#endregion // Methods`; `#pragma warning disable CS1 // why`; `$"a{n:N2}b"` and a raw interpolated string `$$"""{{x}}"""`; `"// not a comment"` (FC-022); `@class` as an identifier (FC-023); text that does not parse (`class C{`, PLAY-011).

### Line endings (FC-033)

In the browser, a text area's value normalizes line breaks to LF, so the page never receives CR LF from the input; bUnit's `Input(...)` passes whatever the test gives. A test with CR LF input is a test of an input no user produces; the existing tests use LF. No member here.

### Layout requirements and bUnit

PLAY-011 ("stay under the characters … while scrolled"), PLAY-012 ("each line … one visual line", horizontal scroll), and PLAY-010's "its own colour in each colour scheme" are properties of layout and CSS, which bUnit does not render: `Reihitsu.Playground.Test` sees markup only, not `app.css`. The architecture test `EveryRequirementOfAnEnforcedSpecIsCovered` requires a covering test for each. A test can cover them only by asserting markup attributes and the stylesheet's rules (reading `wwwroot/css/app.css`), which proves the declaration, not the rendering. Nothing breaks; the lead should expect these three requirements to be covered at that strength, and the validator to check them by hand in a browser.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.Playground` (`Home.razor`, `Home.razor.cs`, `PlaygroundFormatter`, `PlaygroundFormatResult`, `wwwroot/css/app.css`) | must change |
| New highlighting and error-list types in the Playground | must change; they join `Code owners` of `playground/playground.md` and its row in `sdd/specs/README.md:411` |
| `tests/Reihitsu.Playground.Test` | must change: `FormattingRunsWithoutServicesOrJavaScriptInterop` (`:82`, reads the output by `value` and runs with strict JS interop), `PageShowsEditableInputAndReadOnlyOutputWithFormatButtonBelow` (`:104`, asserts the output is a read-only `textarea` with `spellcheck`), and every test reading `#playground-output` by `GetAttribute("value")` (`:93`, `:147`, `:167`, `:172`, `:194`, `:220`, `:243`, `:269`, `:290`, `:316`, `:337`, `:358`) |
| PLAY-001's no-JS-interop guarantee | must stay: the test runs `JSRuntimeMode.Strict`, so any JS interop on render (e.g. for scroll sync) fails it |
| PLAY-003, PLAY-004, PLAY-005, PLAY-007, PLAY-008 and PLAY-009 message texts | must stay |
| `Reihitsu.Formatter`, `Reihitsu.Core`, analyzers, code fixes, Fix All, CLI, rule docs | not involved |
| `deploy-pages.yml` / hosting | not involved |

## Refuse option

No formatter or code-fix input class is in play, so withholding a fix, leaving a construct as written, and declaring a shape unsupported do not apply; nothing here would delete a special case.

The narrower options I checked are all ruled out by the maintainer's answers:

- Highlighting only the output, with no overlay on the input, would remove the alignment risk of PLAY-011 and the parse on every keystroke. Answer 2 requires live input colouring.
- Showing only the first error is ruled out by answer 3 (every error).
- Wrapping with a soft-wrap marker is ruled out by answer 4.

_None_ remains.

Risk the lead should know (not a finding, nothing measured): the Playground is not AOT-compiled (`Reihitsu.Playground.csproj` sets no `RunAOTCompilation`), so PLAY-011 runs a Roslyn parse and a re-render of every coloured span on each keystroke in the interpreted WebAssembly runtime. I did not measure it in a browser and make no claim about its speed. The validator should type into a large input (a few thousand lines) in a browser. PLAY-011's "on every change" as written does not admit coalescing keystrokes; see *Internal choices*, 4.

## Output questions

The spec author's two questions stand. My evidence for each, then one new question.

**1 · Playground · error marks after the input is edited (PLAY-014)** — I agree with **A**. Fact for the lead: option C costs no extra parse, because PLAY-011 already parses on every keystroke; the reason against C is the one given (marks flashing on half-typed code, and a list that disagrees with the marks), not cost. B is what an implementation storing offsets does by default, so whichever answer is chosen needs a test that edits the input after a failed format.

**2 · Playground · colour of `var`, `nameof`, `dynamic`, `value` (PLAY-010)** — I agree with **A**. Probe: `SyntaxFacts.GetContextualKeywordKind` gives `var`, `async`, `nameof`, `record` a contextual kind by text alone, also where they are a parameter name (`void M(int var, int async, int nameof)`), and gives `value` and `dynamic` none. So the "by text" variant does not cover the four names B lists and mis-colours names used as identifiers; it is no better than B.

**3 · Playground · mark for an error at the end of a line (PLAY-014)**

PLAY-014 says an empty span is "marked one character wide at its position", and its only example is at the end of the text. The most common syntax error, a missing `;` at the end of a line in multi-line code, has an empty span whose position holds the line break. The spec does not say what is marked there.

Input:
```csharp
class C
{
    int x
}
```

Before (`main`): no marks; the message is the bare sentence.

After, A (recommended): a one-character-wide red wavy underline directly behind `x`, in the column after the line's last character, on line 3. The text does not change (PLAY-010); the line is only one cell wider. The message lists `line 3, column 10 — CS1002: ; expected`.
```csharp
class C
{
    int x
         ~
}
```

After, B: the line break itself is marked, which shows nothing; the message lists the error, but the input shows no mark for it.

Why A: the end-of-text example already marks the place behind the last character, and a line end is the same case; B leaves the most common error unmarked, against PLAY-014's purpose.

Whatever is chosen, the spec should state the rule for all three positions, with this input as an example:

- in front of a character that is not a line break — that character, as for CS1010 at `"` and CS1035 at `/` above;
- at a line break;
- at the end of the text.

## Internal choices

These are spec wording. In each, the spec's own text or example already decides the visible result; only the literal wording admits a second reading. If the lead reads either as two visible readings, it is an output question.

1. **Precedence inside documentation comments.** The keyword bullet ("a token the parser gives a keyword kind") literally matches `int` in `<see cref="int"/>`, and the identifier bullet matches `see`, `cref`, and `param`. The comment bullet says a documentation comment is a comment. Recommend one sentence: everything inside a documentation comment, its tokens included, is a comment, and everything inside a directive is a directive except a comment trivia behind it. The directive half is already decided by the `#if DEBUG` example. Alternative: XML tokens coloured by kind, which gives multi-coloured doc comments and contradicts the comment bullet.
2. **"Not a comment behind it" in directives.** Roslyn parses `// …` behind `#endregion`, `#if`, `#else`, `#endif`, `#pragma`, `#nullable`, and `#line` as comment trivia. Behind `#region`, `#error`, and `#warning`, it parses the same text as `PreprocessingMessageTrivia`. Recommend wording by kind: "a comment trivia behind it", and adding `#endregion // Methods` to the PLAY-010 example, since the formatter writes that form. Finding `//` inside a `#region` name would be a rule over text, which the change doc's "decided by the kind" rules out.
3. **The highlighted copy under the input and assistive technology.** PLAY-013 cares about assistive technology. A coloured copy of the input under the text area would be read a second time unless it is hidden from assistive technology. Recommend that the spec author add one sentence to PLAY-011 (the copy is hidden from assistive technology). Alternative: leave it to implementation. It costs one attribute either way.
4. **PLAY-011 "on every change".** As written, the colours must follow each keystroke synchronously. Recommend keeping it, and revisiting only if the validator finds typing slow. Coalescing keystrokes is a visible change to typing behavior, which the maintainer's answer 2 ("updates live while typing") covers only loosely.
5. **One owner for line and column.** PLAY-006 now counts "as in PLAY-008", whose count lives in `Home.CreateUnsupportedMessage` (`Home.razor.cs`). This is not a defect yet. It is a place where a second private count would be FC-015, so the auditor should check that one helper serves both.

## Issue goals

The issue is `agent-filed`, but `00-lead.md` records that its plan items 1–4 are the maintainer's answers, so they are binding. Goals I would adjust, none of which changes visible output:

- **"Done when: PLAY-002, PLAY-006, and PLAY-008 … describe the new behavior."** PLAY-008 keeps its text; the box and the mark come from PLAY-013 and PLAY-014. That meets the goal's purpose. No visible difference.
- **The issue's example `line 1, column 26 — CS1002`** names no input; the spec carries the measured values (`column 28`, `column 59`/`60`). No visible difference.
- **The issue's prescribed mechanism** (`<pre>` output, a transparent `<textarea>` over a `<pre>`, `wrap="off"`, `white-space: pre`) is a proposal. The spec rightly states behavior, not markup; the implementer may follow it. No visible difference.

## Architecture findings

_None._

## Follow-up candidates

_None._ One stale reference: the change doc's *Visible output changes* section 1 still says "see the open question in the spec author's handover". The lead replaces it with the answer to Output question 2. Its first highlighting example uses `string s = "text"` where the spec uses `$"a{n}b"`; this is harmless, but the two could be aligned.
