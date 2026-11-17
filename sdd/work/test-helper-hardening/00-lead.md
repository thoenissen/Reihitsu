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

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: skeptic (`01-skeptic.md`).
