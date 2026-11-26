# Lead — thoenissen/Reihitsu#1926, repair after publishing

- Issue thoenissen/Reihitsu#1926; PR #1944 (published, ready for review). Slug `unsupported-assignment-arm-comment`; lane behavior-change; branch `claude/squad-1926-97asq4`.
- The run's record is the note `squad/notes/2026-11-26-unsupported-assignment-arm-comment.md`.
- Maintainer request (chat): fix the merge conflicts of PR #1944 with `main`.

## Maintainer answers

- Earlier answers of the run: see the run's note. The change doc `sdd/changes/2026-11/assignment-and-arm-comment-unsupported.md` (Accepted) and FMT-UNSUP-020/021/022 hold them.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | repair | Re-audit after merging main (e014ac6), whose RH2001 change (#1951) touches the same spec and test file as this run's RH2001-007 | No re-audit; the test implementer resolves the one conflict keeping both sides, then cleaner and validator; CI decides | Both sides only withhold the RH2001 fix in more cases; the union is consistent | Each side's outputs were decided on its own PR | — |

## Follow-up candidates

(none)

## State

- A `git merge origin/main` (main at e014ac6) is in progress with one conflict: `tests/Reihitsu.Analyzer.Test/Formatting/RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs`.
- Next: test implementer (01) resolves it and completes the merge; then cleaner (02), validator (03); delete `sdd/work/`; push.
