Role: auditor
Commit: 7fd0ebdebf47a2115b1c0ac80fa3bb2d8fcbda1f
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `52531f0` (the commit audited in 11), thoenissen/Reihitsu#1289 only. Head `7fd0ebd` is checked out with a clean tree. `origin/main` is `c2866eaf294a9040b995a2b75de7587bdccc5efe` (fetched, the same SHA as the prompt) and is merged into head. `scripts/lane-check.sh bug-fix`: confirmed. Outside `sdd/work/`, the repair diff is `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` (+14/−6) and 13 tests in `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.cs`, which is what handovers 12–15 describe. No spec, rule page, or other surface changed, so the scope did not grow. The verdicts in 11 for #1071, #1268, and #974 stand, because their files are byte-identical to `52531f0`.

Probes: the harness from 11 (`/tmp/claude-0/-home-user-Reihitsu/0f8f32bb-d47f-54fd-bcc1-8f17dd4f4945/scratchpad/probe`, rebuilt to `out2`) calls the private `FindCodeDirectives` from the Release `Reihitsu.Analyzer.dll` built at 20:04. That build postdates `0de22a4` (20:00), the last compiled change. The fixtures are in `/tmp/claude-0/-home-user-Reihitsu/0f8f32bb-d47f-54fd-bcc1-8f17dd4f4945/scratchpad/fx4` and `/tmp/claude-0/-home-user-Reihitsu/0f8f32bb-d47f-54fd-bcc1-8f17dd4f4945/scratchpad/fx5`. `main` reports every `@code\s*\{` in the file, so it reports every directive named below.

## Previous findings

- **#1: closed.** Every member it listed is now reported, and each has a test from 12. The members are `"<!--"` and `"@*"` in a `@{ }` string, `@M("<!--")`, `@("@*")`, `// <!--` in `@if { }`, `user@*x`, an attribute value, and a `<script>` string, plus the later-`-->` variant. A Razor or HTML comment opener now counts only when it starts its line.
- **#2: closed.** The members it listed (`{` and `/*` in `@<p>…</p>` template text, in `@code` and in `@functions`) are now reported, and each has a test.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Analyzer/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` `SkipCSharpBlock` (the new `@<` stop) | FC-027, FC-016 | #2 | PR-introduced | Only a template that starts with `@<` stops the C# lexing. Razor also switches to markup for a tag written directly as a statement in a `@code`/`@functions` block. This is the documented Blazor form for a method or lambda that has a `__builder` parameter. Such markup is still lexed as C#, so a `{` or `/*` in its text keeps the block open to the end of the file, and every later directive is missed. `main` reports them, and RH2109-001 requires them. Counterexample: `@code {` / `    void R(RenderTreeBuilder __builder)` / `    {` / `        <p>{</p>` / `    }` / `}` / `@code {` / `}` → only (1,1) is reported (`main`: (1,1) and (7,1)). Failing members, all probed: `<p>{</p>` in a method body (`a1`); `<p>/* note</p>` (`a2`); a `__builder => { <p>{</p> };` lambda (`a3`); the same in `@functions`, where nothing is reported at all (`a4`); `<text>{</text>` (`a5`); a tag inline after a statement brace, `if (x) { <p>{</p> }` (`e3`). Members that hold: `@<` templates (the repair); markup without these characters (`a7`); `'` in markup text (`e2`); a `@:{` text line (`e1`). A `}` in markup text closes the block early, which is not a regression (`a6`). | The owner re-derives. One cheap check: treat a `<` token that starts its line inside the block and is directly followed by a letter like `@<`, so the outer scan takes the rest of the block. This leaves the inline member `e3` failing. The other choice is for the lead to record this class as a decision with the decision-7 follow-up. Either way, add a test with a real directive after such markup that pins the outcome. |
| 4 | high | same file, `SkipCSharpBlock` (`return token.SpanStart + 2`), together with the line-start opener check in `FindCodeDirectives` | FC-002 | #1 | PR-introduced | After a template `@<`, the outer scan reads the rest of the block's C# as markup. So a `@*` or `<!--` that starts its own line inside a multi-line C# literal or comment there opens a comment. That comment runs to its terminator, or to the end of the file, and swallows a real directive behind the block. The round-1 head (`52531f0`) lexed the whole block and reported the directive, and so does `main`. Decision 12 accepts this miss only for unlexed hosts (`@{ }`, control flow, expressions), not for the rest of a `@code` block. Handover 13 lists it under false-positive fallbacks only, and its miss side is not recorded. Counterexample: `@code {` / `    RenderFragment F => @<p>Hello</p>;` / `    string s = @"` / `@* x` / `";` / `}` / `@code {` / `}` → only (1,1) is reported (`main` and `52531f0`: (1,1) and (7,1)). Failing members (probed): `@*` (`b3`) and `<!--` (`e4`) starting a line in a verbatim string after a template. A multi-line `/* */` comment and a raw string are other members on the same path, traced and not probed. A related markup member (a line-starting `<!--` inside a multi-line JavaScript template literal in `<script>`, `c5`) is also missed. Whether Razor's own tokenizer sees a comment there was not checked. | The shape is rare. Either the lead extends decision 12 to the rest of a block after a template and adds it to the follow-up, or a cheap check keeps the block's remaining C# from opening comments (for example, honor only `@*` at a C# token start until the block's `}`). A test should pin the chosen outcome. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Razor/HTML comment opener outside a lexed block (`StartsWith "@*"`/`"<!--"` && `StartsLine`) | the opener's position, plus spaces and tabs back to `\n`/`\r` or the file start | SHARED-DIAG-003: a Razor or HTML comment in markup, wherever it starts on its line | Disagrees in two directions. A comment opened behind text is now ignored, which gives false positives as on `main` (decision 11, follow-up), e.g. `c2`. A line-starting opener inside unlexed C# opens a comment, which gives misses (decision 12, plus #4) | #4; the rest is accepted |
| Template stop in a lexed block (`StartsWith(text, token.SpanStart, "@<")`) | the start of each C# token, so trivia, string, and char tokens cannot match (`d1`, `d2`, `d3` hold) | Razor's switch from C# to markup inside a code block: `@<tag>`, and a tag at statement start | Covers `@<` only | #3 |
| Directive at line start (`StartsLine`) | unchanged | RH2109-001 (Q3) | unchanged since 11 | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Razor directive and comment scanning, line-start test | `rg -n "@code\|<!--\|ParseTokens\|StartsLine" src --glob '!*Test*'` finds them only in RH2109, where `StartsLine` is reused for the openers rather than copied. The other `ParseTokens` users (`Reihitsu.Core/TokenSeparationUtilities.cs`, `CodeFixes/Core/ExcludedCodeUtilities.cs`) are unrelated | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 9 `Verify…OpenerIn…DoesNotHideDirective…` tests (`@{ }` string ×3 incl. a later real `<!--`, implicit and explicit expressions, `@if` comment, after a letter, attribute value, script string) | a real line-starting directive after a text opener is reported | the directive missed, or an extra diagnostic | `Verify` with `RazorCodeBlockSpan`: exact span and exact count | adequate. 12 shows they fail on `52531f0` and pass with `main`'s analyzer |
| The 4 `Verify…InTemplateMarkupOf{Code,Functions}Block…` tests | a directive after a block that holds a template is reported, and so is the block's own `@code` | a missed second or first directive | same | adequate for templates. No test has markup without `@<` (#3) or a line-starting opener in the block's C# after a template (#4) |

## Failure classes dismissed

The full walk in 11 stands for the parts this repair did not touch. For the repair delta:
- FC-001: the opener guard and the template stop are covered under Guard scope.
- FC-003: both sides are tested: openers in text against line-starting real comments (the existing `…MultiLineRazorComment…`/`…HtmlComment…` tests), and templates against blocks without templates.
- FC-004, FC-005, FC-006, FC-019 to FC-022: no edit of tokens, trivia, or literals; the rule has no code fix.
- FC-007 to FC-014, FC-018, FC-024 to FC-026, FC-028 to FC-030, FC-036, FC-038 to FC-043: no fix, no formatter, no symbol, no layout, no declared shape.
- FC-015: one owner (above).
- FC-017: adequate (above).
- FC-023: no identifier is compared.
- FC-031: both changed summaries and the `<returns>` describe the new code.
- FC-032: the probed Release build postdates the last compiled change.
- FC-033: the line-start check reads `\n` and `\r` as before. CRLF is tested in the existing tests, and the repair adds no line-ending decision.
- FC-034, FC-035: the expected values pin `main`'s output (12 checked them against `main`'s analyzer) as decision 11 requires.
- FC-037: no text-only claim.

Code quality, comments, and scope: the added lines carry no `TODO`, no commented-out code, and no tracker number. No new type. The changes stay inside the run's scope. Cost of special cases: both new conditions are single cheap checks on characters the scanner already reads. They serve Blazor templates and the openers, and nothing serves only a declarable shape. Visible output: every expected value the 13 tests add is `main`'s report, which unchanged RH2109-001 requires (decision 11). No rule page changed.

## Architecture findings

_None._ #3 and #4 are local to one analyzer that approximates Razor's tokenizer. No second component shares the decision.

## Hints

- Lead's follow-up candidates: the fallback false positive for a comment opened behind text that holds a line-starting directive (`c2`: `<div> @*` / `@code {` / `}` / `*@ </div>` reports (2,1)) is confirmed and is already listed.
- Decision 12's own member (`c6`: a `<!--` starting a line inside a verbatim string in `@{ }`) is confirmed as missed, as decision 12 accepts.
