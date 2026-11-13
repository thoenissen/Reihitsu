Role: auditor
Commit: 56a5a07900d9a910dfaa7822919a323ee18421f6
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decision 13 of `00-lead.md`, which is the lead's recorded decision and is an input of this audit.

## Scope

This audit covers the repair delta against `5663397` (the commit that added audit 16), for thoenissen/Reihitsu#1289 only. Head `56a5a07` is checked out, and the tree is clean. `origin/main` is `c2866eaf294a9040b995a2b75de7587bdccc5efe`, fetched and the same SHA as the prompt, and it is merged into head. `scripts/lane-check.sh bug-fix` confirmed the lane.

Outside `sdd/work/`, the delta is `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` (+8/−65) and `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.cs`. The test file has 9 turned tests and 7 new tests. Handovers 17–20 describe exactly this delta. No spec, rule page, ledger, or other surface changed, and the scope did not grow: the repair only removes code.

thoenissen/Reihitsu#1071, #1268, and #974 keep their verdict from audit 11. Their files are byte-identical to `52531f0`: `git diff 52531f0 HEAD -- src` touches only the two RH2109 files.

What the repair changed:
- `SkipCSharpBlock` is removed, and with it the C# lexing of `@code` and `@functions` blocks and its `@*` and `@<` stops.
- The pattern matches only `\G@code\s*\{` again.
- A reported directive now advances the scan by one character.

The scan now reports what `main` reports, minus three cases:
- a `@code` that does not start its line (Q3);
- text inside a Razor or HTML comment whose opener starts a line;
- text behind an `@@`.

I traced this by static analysis. I relied on the implementer's result (RH2109 53/53 and the analyzer project 8612/8612) and on the cleaner's Release build and filtered run (225/225), both taken after the last compiled change (`b8b466e`). No finding depended on running anything again.

## Previous findings

- **#1: closed** (it was already closed in 16). The opener logic it concerned (`StartsLine` on `@*` and `<!--`) did not change. Its 9 tests still pass on head.
- **#2: closed** (it was already closed in 16). Block lexing is gone, so `{` and `/*` in template markup no longer keep a block open. The 4 `…InTemplateMarkupOf…` tests still pass.
- **#3: closed.** Every member it listed is now reported:
  - `<p>{</p>` (`a1`), `<p>/* note</p>` (`a2`), and the `__builder` lambda (`a3`);
  - the same in `@functions` (`a4`), in both its `{` and its `/*` variants;
  - `<text>{</text>` (`a5`) and inline markup after a statement brace (`e3`).

  Each member has a test from 17 with a real directive at (7,1). Each test asserts exactly `main`'s report, and 17 verified that against `main`'s analyzer.
- **#4: superseded** by decision 13. Decision 13 extends decision 12's accepted miss to every C# host, `@code` and `@functions` included, and it is listed as follow-up candidate (b). Its members `b3` (`@*`) and `e4` (`<!--`) still miss the later directive, as decided. With block lexing gone, the "after a template" qualifier no longer matters, so the same miss now applies anywhere in a block. This member was not probed, but it is traced: `@code {` / `string s = @"` / `@* x` / `";` / `}` / `@code {` / `}` reports only (1,1). The round-2 head and `main` report (1,1) and (6,1). This is the member decision 13 names, not a new one.

## Findings

_None._ No new finding was found, and none repeats the class of #1–#4.

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | _None._ | – |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Directive (`text[position] == '@' && StartsLine && _codeDirectivePattern.IsMatch(text, position)`) | `\G@code\s*\{` from a `@` that has only spaces and tabs before it back to `\n`, `\r`, or the file start | `main`'s `@code\s*\{` anywhere, narrowed by RH2109-001's line-start clause (Q3) | They agree. The pattern is `main`'s, `\G` anchors it at `startat`, and dropping `@functions` from it removes a keyword that was only matched in order to skip its block (no report was ever made for it) | holds |
| Razor/HTML comment opener (`StartsWith "@*"`/`"<!--"` && `StartsLine`) | unchanged since 13 | SHARED-DIAG-003 Razor bullet | Unchanged. The disagreements are decided: openers behind text → follow-up (c). A line-starting opener inside a multi-line C# literal or comment, now in every host → decisions 12 and 13, follow-up (b) | accepted by decision |
| C# comment and literal text inside `@code`/`@functions` | no longer inspected | RH2109-001 ("a `@code` that SHARED-DIAG-003 makes text MUST NOT be reported") | They disagree for a line-starting `@code {` inside a C# comment or literal. The result equals `main`'s report. This is decisions 8 and 13 and follow-up (a), and #1289 is linked `Refs` | accepted by decision |
| Escape `@@` | unchanged | SHARED-DIAG-003 | unchanged | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Razor directive and comment scanning | `rg -n '@code\|<!--\|"@\*"\|@functions' src --glob '!*Test*' --glob '*.cs'` finds them only in `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer` (`_codeDirectivePattern`, `FindCodeDirectives`). RH2109 no longer calls `SyntaxFactory.ParseTokens` | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 7 new `Verify…InMarkupOf{BuilderMethod,BuilderLambda,…InFunctionsBlock}…`, `…InTextElement…`, `…InInlineMarkup…` (`[Requirement("RH2109-001")]`) | A real line-starting directive after markup that holds `{` or `/*` inside a `@code`/`@functions` block is reported, and so is the block's own `@code` | the (7,1) report missing, a missing (1,1), or an extra diagnostic | `Verify` with `RazorCodeBlockSpan`: exact span and exact count | Adequate. They failed on `e48df40` (17) and pass now. They pin the regression that #3 described if block lexing comes back |
| 9 turned characterizations (`VerifyCodeBlockLineInsideCSharpBlockCommentIsReportedWhileTextInMarkupCommentsIsNot`, `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported`, `…DocumentationBlockComment…IsReported`, `…VerbatimString…IsReported`, `…InterpolatedVerbatimString…IsReported`, `…InterpolatedRawString…IsReported`, `…BlockCommentOfFunctionsBlockIsReported`, `…AfterClosingBracesInLiteralsIsReported`, `…AfterNestedBracesIsReported`) | They pin today's report: a line-starting `@code {` inside a block's C# comment or literal is reported, and the kept parts stay unreported | a line dropped or added | same, exact | Adequate as characterizations. Their tags were removed as the test-implementer rules require for unspecified or contrary behavior. The kept parts they also touch (markup comments, the mid-line `"@code{"`) stay covered by tagged tests: `VerifySharedExampleReportsOnlyTheDirective` (+CRLF), `VerifyCodeBlockInTheMiddleOfALineIsNotReported`, and the multi-line Razor/HTML comment tests |

## Visible output

Every expected value that the 16 changed or added tests carry is `main`'s report for that input. 17 checked this against `main`'s analyzer, so no expected value changes an output relative to `main`.

The 7 new tests are demanded by unchanged RH2109-001. The 9 characterizations keep `main`'s output for the class that decision 13 refuses, and the issue goal they leave unmet belongs to an `agent-filed` issue that is linked `Refs`.

No rule page changed.

## Failure classes dismissed

- FC-001, FC-002: covered under Guard scope. The repair removes a guard and adds none.
- FC-003: both sides are tested. Real directives after markup in blocks are tested (7 new tests), and so are the existing tests where a line-starting opener does hide text.
- FC-004, FC-005, FC-006, FC-019 to FC-022: there is no edit of tokens, trivia, or literals, and the rule has no code fix. FC-022's literal-looking text is now deliberately not lexed (decision 13).
- FC-007 to FC-014, FC-018, FC-024 to FC-026, FC-028 to FC-030, FC-036, FC-038 to FC-043: there is no fix, no formatter, no symbol, no layout, and no declared shape.
- FC-015: one owner (above).
- FC-016: #3's sibling shapes (`@functions`, lambda, `<text>`, inline) are all covered by the same removal. The `@functions` variants of `<text>` and inline markup take the identical path, because no keyword selects a different scan any more.
- FC-017: adequate (above).
- FC-023: no identifier is compared.
- FC-027: the multi-line and nested members of #3 (method, lambda, nested `if`) are tested.
- FC-031: `_codeDirectivePattern`'s summary and `FindCodeDirectives`' summary describe the new code. No comment mentions `@functions` or the skipped block any more.
- FC-032: the tests and the build postdate `b8b466e`, the last compiled change.
- FC-033: no line-ending decision was added. `StartsLine` is unchanged, and the CRLF tests exist.
- FC-034: the new tests are the auditor 16 counterexamples themselves.
- FC-035: the expected values come from `main`'s analyzer, not from the patched code (17).
- FC-037: there is no text-only claim.

Other checks:
- Code quality and comments: the round adds no `TODO`, no commented-out code, and no tracker number. The `using Microsoft.CodeAnalysis.CSharp` that became unused was removed, and the remaining helpers (`StartsLine`, `StartsWith`, `SkipPast`) are all used.
- Scope: the delta stays inside the run's scope.
- Cost of special cases: the repair deletes the only special-case machinery (C# block lexing), and nothing that remains serves only a rare or declarable shape.
- Structure: there is no second component.

## Architecture findings

_None._

## Hints

- The scan has no C# extent any more, so a line-starting `@*` or `<!--` inside a multi-line C# literal or comment of a `@code` block now hides a later real directive. `main` reports that directive, and so did the round-2 head (member traced under #4). Decision 13 accepts this explicitly as a rare shape. `squad/README.md` lists withholding a fix and leaving a construct as written as refusals that put nothing in question. It does not list a diagnostic that `main` reports and the run no longer reports. The lead should therefore name this class in the PR's `Review notes` as a deliberate miss for a rare shape, so that the maintainer sees it.
- Follow-up candidate (b) in `00-lead.md` names "a multi-line C# literal or comment". Audit 16's related member `c5`, a line-starting `<!--` inside a multi-line JavaScript template literal in `<script>`, is a non-C# host on the same path. Whether Razor's tokenizer treats it as a comment is unverified. The follow-up text could name "a multi-line literal or comment of C# or script text".
- The summaries of the unchanged tests `VerifyOpeningBracesInsideLiteralsAndCommentsDoNotKeepCodeBlockOpen` (test file, about line 654) and the four `…InTemplateMarkupOf…` tests (about lines 954–1028) explain their guarantee as a code block that is or is not "kept open". The analyzer no longer has that mechanism. The statements remain true as observations and the tests remain useful regression guards, so this is wording only.
