Role: auditor
Commit: e20ab4db4a9c4b3604de757c1f146e7a5f660110
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Full audit of head `e20ab4d` against base `origin/main` `53990ed` (`git rev-parse origin/main` after a fetch matches the SHA in my prompt; `git merge-base --is-ancestor` confirms the base is merged). The checkout is the head.
- The diff is what the handovers describe:
  - `sdd/specs/shared/diagnostics.md` (SHARED-DIAG-002, SHARED-DIAG-003, three non-goals) and two ledger lines;
  - three `squad/decisions/2026-10-18-*` records for #1069;
  - 13 new analyzer tests and 16 added `[Requirement("SHARED-DIAG-003")]` tags (RH5601 9, RH5023 5, RH6002 2) in 8 test classes;
  - `sdd/work/name-value-literal-content/`.
- No production file changed. Commits after the test results (`42bb29d..e20ab4d`) touch only `06-cleaner.md` and `07-spec-checker.md`, so the cleaner's build and the test implementer's focused runs still apply (FC-032).
- `scripts/lane-check.sh tooling --head worktree`: "LANE CHECK: confirmed — claimed tooling, diff needs tooling".
- Every spec example block has a test with the same code (checked one by one). Every requirement of thoenissen/Reihitsu#1069 is addressed:
  - its examples appear as prose;
  - the "nor changes" half is out of scope by `squad/decisions/2026-10-18-shared-diag-003-literal-value.md` and the new non-goal that points to `code-fixes.md`.

## Findings

The run's records (`00-lead.md` clarifications, `01`/`04-spec-author.md`, `07-spec-checker.md`) state that the code violating the two new requirements is tracked in thoenissen/Reihitsu#1071, #1074, #1075, and #1105. For two confirmed inputs that is not true. Neither finding asks for a change to this tree; both need tracking that the maintainer approves. This follows the precedent of escalation 2 in `00-lead.md`, the using-directive case later filed as #1105.

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `FormattingTextAnalysisUtilities.GetStringLineIndices` (via `GetNonFormattableLineIndices`), used by `RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer` | FC-001, FC-015, FC-016 | same requirement | SHARED-DIAG-003 says the code inside an interpolated string, a hole's expression included, MUST be checked like any other code. The helper marks every line of an `InterpolatedStringExpressionSyntax.Span` as non-formattable, holes included. Counterexample (`scripts/apply-fix.sh RH5023`, LF and CRLF): `var s = $"""⏎    a{x⏎⏎⏎    + 1}b⏎    """;` gives `no-diagnostic`, while the same two blank lines in `var s = x⏎⏎⏎    + 1;` are reported and fixed. No tracking issue names it: #1074 lists RH5601, RH5602, RH6009, RH6010; #1075 with its comment lists RH5025, RH5027, RH5011, RH5030; no RH5023 open-questions issue exists. The normative text of RH5023-002 ("a line that holds part of a string literal (including … interpolated strings)") reads as covering hole lines, but its Why ("part of the string's value") does not. | The lead brings it to the maintainer: extend #1074 (its title already says "inside interpolation holes is code") or approve a new bug issue. The PR then lists it under follow-up work. The follow-up has to clarify RH5023-002's wording. It also touches a shared helper with other consumers: RH5204, RH5602 (hole case already in #1074), RH6004, RH8301, RH8302, RH8303. No change to this tree. |
| 2 | medium | `RH5601UseTabsCorrectlyAnalyzer.OnSyntaxTree` / `IsStringContentToken` and its code fix | FC-022, FC-013, FC-016 | same requirement | Input: `string s = $"""⏎→→a⏎→→""";`. Under SHARED-DIAG-003 the whitespace in front of the closing `"""` is literal text: it is the text of an interpolated string around its holes, and the raw-string indentation the compiler removes is the literal's own syntax, which only a rule whose subject is a literal checks. RH5601 reports that tab, because the token it finds there is the raw string's end token, which `IsStringContentToken` does not list. Its fix then rewrites the closing line to `····→"""`, and `scripts/apply-fix.sh RH5601` gives `invalid-result` (source that no longer parses), LF and CRLF. The same input without `$` is `no-diagnostic`. A tab inside the interpolated content line `→→a{1}` is not reported. `scripts/format.sh` keeps both tabs (FMT-CLEAN-004), so RH5601 also flags formatter output. This happens on `main` too, independent of this diff, and already breaks RH5601-002 ("the text of an interpolated string"). Untracked: #951 covers the formatter's handling of tab-indented plain raw strings; #1074, #1083 and #1038 do not name this shape. | Escalation 9 (a failure that also exists on `main`): the maintainer decides whether to file a bug or fold it into #1074. The PR then lists it under follow-up work. The run's constraint against production changes keeps the fix out of this PR. No change to this tree. |

Checked and found tracked, so not findings:

- **Tab in a `//` comment on a directive line.** `#if DEBUG // a→b` is reported and fixed today, and the formatter keeps the tab. #1083 (from #1038 Q1) decides it is neither reported nor changed, and cites SHARED-DIAG-003.
- **Tab directly after a literal.** `string s = "a"→;` gives `no-diagnostic`. #1074's summary says only "a tab before a literal", but its source, #1038 Q2, includes the after-literal tab `M("a"→, "b")`. This answers the observation in `07-spec-checker.md`.

## Guard scope

The diff adds no code predicate. The rows below are the boundaries the new requirements draw, set against the code predicates that decide the same thing.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-DIAG-003 subject exemption (comment, directive, literal subjects) | the rule's subject | SHARED-FIX-003 draft (#1068 branch `claude/admiring-euler-a4zxhb` at `77a01eb`): "Text that a diagnostic of the fix's own rule reports may change" | Agree for every decided input. RH3201 reports the comment, RH5202 the whole literal, RH3204 the whole string; the report side and the fix side are now stated once each | ok |
| Literal text vs. code, RH5601 | `root.FindToken(i)` full span (leading and trailing trivia included) plus `FindTrivia(i)` without `findInsideTrivia` | SHARED-DIAG-003 bullets: literal text, format clause, comment text vs. hole expression, alignment clause, surrounding whitespace | Too wide by the literal's trivia (tracked, #1074 via #1038 Q2/Q4). Too narrow for comments inside directive trivia (tracked, #1083). Interpolated raw end token not classified as content | Finding 2 for the last shape |
| Literal text vs. code, line-based rules | `InterpolatedStringExpressionSyntax.Span`, every intersecting line | Same SHARED-DIAG-003 bullets; RH5602-001 explicitly counts hole lines as touched (tracked, #1074) | Too wide: hole lines are exempt | Finding 1 |
| SHARED-DIAG-002 by value | the compared name | RH8101 `parameter.Identifier.ValueText`; RH2003 `GetSimpleTypeName` → `Right.Identifier.ValueText` (rightmost identifier only, so inner trivia never reaches it); RH4105/RH4120 `Identifier.ValueText != "_"` | Agree | ok |
| SHARED-DIAG-002 by value, spelling users | the compared name | `RH4009NamespaceNotAllowedAnalyzer` `Name.ToString()`, `RH4001TypeNameShouldMatchFileNameHelper` `Identifier.Text` (#1071), `UsingDirectiveOrderingUtilities` `Name.ToString()` (#1105), `RH3103UseShorthandForNullableTypesAnalyzer` `Identifier.Text: "nameof"` (a keyword check, see Hints) | Known violators, all tracked | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A name is compared by its value | No shared helper; each rule compares on its own. Rules that compare by value: RH8101, RH2003, RH4105, RH4120, `AttributeTargetUtilities` (`ValueText == "type"`), RH2006, RH2007, RH8501. Rules that compare by spelling: RH4009, RH4001 (#1071), `UsingDirectiveOrderingUtilities` (#1105). Patterns searched: `Identifier.Text`, `Name.ToString()`, `ToString() ==/is`, `Text == "` | No untracked spelling comparison found; the diff adds no owner |
| Literal and comment text is content (analyzer side) | `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken` + `SyntaxTriviaUtilities.IsInsideCommentOrDisabledText`; `FormattingTextAnalysisUtilities.GetStringLineIndices` / `GetNonFormattableLineIndices` (RH5023, RH5204, RH5602, RH6004, RH8301, RH8302, RH8303). Spec owners: SHARED-DIAG-003 (report side), SHARED-FIX-003 draft (fix side), FMT-CLEAN-004 / FMT-PIPE-002 (formatter) | More than one code owner already exists on `main`, and they disagree on holes: RH5601 checks hole code, the line helper exempts it. The diff adds no code owner; the divergence is Finding 1 |

## Assertion adequacy

All new tests use `Verify` with an exact diagnostic set. `Diagnostics(id, message, n)` binds the markup keys `#0`…`#n-1`, each with the rule's message, so an extra or missing report, or a moved span, fails the test.

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8101 `VerifyVerbatimParameterIsMatchedToItsDocumentationByItsValue` | `@class` matches `name="class"`; `@event` without `<param>` is reported | A comparison by `Identifier.Text` adds a diagnostic on `@class` | `Verify` + 1 keyed diagnostic | adequate |
| RH2003 `VerifyTypeNameWrittenWithVerbatimPrefixOrInnerTriviaIsReportedLikeThePlainName` | `@X`, `System . X`, and `System./* c */X` are all reported | `Identifier.Text` drops `@X`. A whole-name spelling comparison drops the two trivia cases | 3 keyed diagnostics | adequate; the trivia cases can fail only against a whole-name comparison |
| RH2003 `VerifyOtherTypeNameWrittenWithVerbatimPrefixOrInnerTriviaIsNotReported` | other exception types are not reported, with or without `@` or trivia | Only a broken semantic check; no change to how the name is compared can make it fail | `Verify` no diagnostics | weak for SHARED-DIAG-002, harmless (Hints) |
| RH6002 `VerifyCommasInHoleAndBetweenLiteralsAreReportedButNotInLiteralOrCommentText` | comma in a hole and comma after `"// c"` reported; ` ,` in literal and comment text not | A rule that skips interpolation holes or literal neighbours loses #0 or #1 | 2 keyed diagnostics | adequate; the negative parts guard against a text-scanning rule |
| RH6002 `VerifyCommasInDocumentationCommentCrefsAreNotReported` | cref commas in `///` are not reported | `DescendantTokens(descendIntoTrivia: true)` would report them | no diagnostics | adequate as a regression guard (the rule does not descend into trivia today) |
| RH6003 `VerifySpaceAfterLiteralIsReportedButNotInsideLiteralText` | space after `"a ;"` reported; ` ;` inside not | Missing #0, or a report inside the literal | 1 keyed diagnostic | adequate |
| RH5601 `VerifyTabsInLiteralAndCommentTextAreNotReportedButTabsInHoleCodeAre` (+ CRLF) | tabs in 6 literal kinds, the format clause, and 4 comment kinds are not reported; tabs in the hole expression and the alignment clause are | Any extra report inside content, or a missing hole report | 2 keyed diagnostics, real tabs via `Replace`, CRLF variant | adequate |
| RH5023 `VerifyEmptyLinesInsideRawStringAndBlockCommentAreNotReported` (+ CRLF) | empty lines in a raw string and a block comment are not blank lines | RH5023 reports the second empty line in either | no diagnostics, CRLF variant | adequate; repeats existing tagged tests, which is harmless |
| RH3201 `EmptyCommentIsReportedBecauseCommentTextIsTheRuleSubject` | the comment-subject rule reports an empty `//` | Missing #0 | 1 keyed diagnostic | adequate |
| RH5202 `VerifyMisalignedRawStringIsReportedBecauseTheLiteralIsTheRuleSubject` (+ CRLF) | misaligned literal reported over its whole span; aligned one not | Missing or extra report | 1 keyed diagnostic, CRLF variant | adequate; both sides present |
| 16 tagged existing tests | each asserts a SHARED-DIAG-003 statement: tabs and blank lines in literal and comment text, the alignment comma as code | — | unchanged | Tags checked line by line. `VerifyBlankLinesInsideInterpolatedStringsAreNotReported` puts its blank lines in the text before `{x}`, not in a hole, so the tag does not contradict the requirement. `VerifyNoDiagnosticsForTabInDisabledText` is correctly left untagged |

## Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020, FC-021 — no edit, join, or move in the diff; the tests are diagnostics-only.
- FC-006, FC-024, FC-025, FC-026, FC-028, FC-029, FC-036 — no rewrite, formatter call, or rename in the diff.
- FC-007, FC-008, FC-009, FC-018, FC-030 — no code fix is changed or asserted.
- FC-010, FC-011, FC-012, FC-014 — no formatter change.
- FC-031 — no production method changed. The new test summaries describe their tests, and the RH3201 summary and method name follow that file's "Verifying …" / no-`Verify`-prefix convention.
- FC-032 — no compiled production change, and every later commit is a handover only.
- FC-033 — CRLF variants exist for the three rules that read line structure (RH5601, RH5023, RH5202). The other new tests assert single-line spans on rules that read no line breaks for those inputs.
- FC-034 — no reproduction in this run.
- FC-035 — the expected values are the spec's examples, run against unchanged code.
- FC-037 — no text-only claim.
- FC-002 — reached; see Hints (the "derives other text" qualifier, contextual keywords).
- FC-003 — reached; both sides tested for RH8101, RH5601, RH5202.
- FC-013, FC-016, FC-022 — reached; see the Findings.
- FC-017 — reached; see Assertion adequacy.
- FC-023 — reached: `@class`, `@event`, `@NotImplementedException`, `@ArgumentException`. Unicode, digit-only and `_` names are not reached, because no comparison changed (`_` in RH4105/RH4120 already uses `ValueText`).
- FC-027 — reached; a hole spanning lines produced Finding 1.
- No rule reads inside numeric or character literals: `rg` finds no use of `NumericLiteralToken`, `NumericLiteralExpression` or `CharacterLiteralExpression` under `Rules/`.

## Hints

- RH2003 `VerifyOtherTypeNameWrittenWithVerbatimPrefixOrInnerTriviaIsNotReported`:
  - The pre-filter rejects `ArgumentException` both by value and by spelling, and the semantic check rejects it too. So the test cannot fail on any change to how the name is compared, and `05-test-implementer.md` calls it FC-003 coverage, which it is not.
  - It is correct and harmless. Dropping its `[Requirement]` tag is optional.
- SHARED-DIAG-002 covers contextual-keyword checks by its wording. There, comparing by spelling is right: `@nameof(…)` is not the operator, which matters for RH3103's `Identifier.Text: "nameof"`. `rg` finds no other keyword check (`var`, `dynamic`, `value`, `await`) and no compiling input that differs today. Both `03-skeptic.md` and `04-spec-author.md` already weighed this.
- The SHARED-DIAG-002 qualifier "or derives other text from it, such as a file name" has no example. The only such rule, RH4001, is broken (#1071), as `04-spec-author.md` records.
- Wording differs between this spec and the concurrent SHARED-FIX-003 draft (`77a01eb`):
  - SHARED-DIAG-003 says "Only the text of a literal or a comment is content".
  - SHARED-FIX-003 says "The text of a comment or a directive is content".
  - They do not contradict in behavior, because SHARED-DIAG-003 restricts only reports and exempts directive-subject rules; the #1083 decision fits both. Whichever run merges second may align the word "content".
- The non-goal "Whether a code fix may change the text of a literal or of a comment — `code-fixes.md`" points to text that exists only once #1068 merges.
- RH5204 also exempts every line of an interpolated string through `GetStringLineIndices`. The layout of hole lines is an FMT-INDENT non-goal (#951), and RH5204 has no spec. A follow-up that narrows the helper for Finding 1 must not change RH5204 without that decision.
