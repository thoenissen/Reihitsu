# Lead — test-helper-hardening

- Issues: thoenissen/Reihitsu#1804 (`agent-filed`), thoenissen/Reihitsu#1838 (`agent-filed`). Goals of both are proposals.
- Slug: `test-helper-hardening`
- Branch: `claude/squad-1804-1838-xgx2xh` (session-designated). Draft PR: #1842.
- Lane: **tests**.
  - #1804: consolidates the pipeline-formatting helper of `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs` and strengthens `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` to whole-text idempotency — test code only, no formatter behavior changes.
  - #1838: adds a "not refused as unsupported" assertion to the formatter-parity helpers of `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`, with an explicit opt-out for tests that use refused inputs on purpose — test code only.
  - Shared mechanism: hardening of the formatter test helpers (one per test project); batched by triage (issue comment on #1838).
- Roles run: skeptic (premise / helpers against the code), test implementer, cleaner, spec checker, validator.
- Roles skipped: spec author (no spec added or changed; runs only if the skeptic finds a spec gap), implementer (no production code), auditor (not in the tests lane).
- Maintainer clarifications: the triage comment on #1838 says the run comes after #1732 declares its shape, so the new assertion would also check the parity tests against that declaration. #1732 is claimed (draft PR #1839) but not merged; the run works against `main` as it stands.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | #1804 names 4 copy classes; there are 8, plus `ExecuteAppliesAllRules` and `ExecutePhaseOrderStructuralBeforeIndentation` in `FormattingPipelineTests` | Cover all 8 classes and both extra tests; helpers that call the public `ReihitsuFormatter` methods or a single phase stay | The issue's "Done when" (no class restates the steps) is only met with all of them; agent-filed scope | Test code only; E2/E3 show every copy passes `ApplyRule`'s checks unchanged | |
| 2 | skeptic | Copies that assert one property only (`DocumentationCommentCollapseGuardTests`, `ElseKeywordClosingBraceAuditFanOutTests`) | Keep their partial assertions; only route the formatting through the shared helper | Turning them into exact-output tests would lock layout no requirement of theirs decides | Test code only | 13b99aa |
| 3 | skeptic | `ExecuteAlreadyFormattedCodeNoChanges`: stale trim comment, and first pass == input never asserted though its doc comment claims it | Compare full texts of both passes, drop the stale comment, assert first pass == input | Matches the test's own documented intent; holds under LF and CRLF (skeptic) | Test code only | 13b99aa |
| 4 | skeptic | Architecture finding: analyzer-test `VerifyFormatterFixCore` is a cross-project copy of the parse-and-run steps; reuse needs `internal` helpers of another test project | Fix narrowly: leave it as is in this run; file the shared-helper question as a follow-up | Reuse across test projects needs a visibility / shared-project decision outside both issues; the run adds no new copy | Test code only | 13b99aa |
| 5 | skeptic | #1838 opt-out parameter has zero callers (no parity test uses a refused input; 485/485 pass with the assertion) | Drop the opt-out parameter; assert non-refusal unconditionally in `VerifyFormatterKeepsReportedShape` and `VerifyFormatter` | A parameter without callers is dead code; a future refused-on-purpose test adds it with its first caller | Test code only | 13b99aa |
| 6 | skeptic | Same refusal gap in Formatter.Test `ApplyRule` (`AssertRuleResult` without expected text, `AssertSecondPassIsStable`, `AssertBlankLinesAbove`) | Follow-up, not an adjacent fix | Draft #1839 (#1732) is in flight and declares a shape many formatter fixtures hold; pulling the check in now couples two concurrent runs | Test code only | 13b99aa |

## Follow-up candidates

- Refusal check for Formatter.Test `ApplyRule` assertions (`AssertRuleResult` without expected text / expected == input, `AssertSecondPassIsStable`, `AssertBlankLinesAbove`) — `01-skeptic.md`, #1838 Follow-up candidates; coordinate with #1732.
- Analyzer-test `VerifyFormatterFixCore` restates the formatter test project's parse-and-run steps (`SecondPassInput`, `PhaseTreeRoundTrip` are `internal` there) — `01-skeptic.md`, #1804 Architecture findings.

## Worklist / next step

- Done: skeptic (`01-skeptic.md`, b9752e5): #1804 proceed wider, #1838 proceed narrower; no output questions.
- Done: test implementer (`02-test-implementer.md`, 13b99aa + 1e7549c): both issues written; formatter 101/101, analyzer 488/488 focused.
- Next: cleaner (`03-cleaner.md`).
