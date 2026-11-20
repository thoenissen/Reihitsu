# Lead — unsupported-link-operator-trivia (CI repair)

- Issue: thoenissen/Reihitsu#894 (`agent-filed`); PR #1881; branch `claude/clever-mccarthy-d581sj`; lane behavior change.
- The run's earlier working docs were deleted before publishing; the run's state is in `squad/notes/2026-11-20-unsupported-link-operator-trivia.md` and the merged change doc `sdd/changes/2026-11/unsupported-link-operator-trivia.md`.
- Maintainer answers: declare the shape (triage on #894); reason text 1A; `.`-to-name shape in a change of its own (2A, #1888).

## CI finding (repair round 2 of 3)

CI job `sonarcloud` on head 9f635a0, test project `Reihitsu.SelfHosting.Test`, test `AnalyzerReportsNoViolationsOnOwnSourceCode("RH2008MethodsShouldNotExceedTheLineLimitAnalyzer")`:

    src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs:84 [RH2008] This member has 66 lines of code, which exceeds the limit of 60.

PR-introduced (the run added a candidate to the precheck). Owner: implementer. No behavior change allowed; the existing tests pin the precheck's behavior, including the order of candidates (FMT-UNSUP-001, the FMT-UNSUP-004/010 overlaps).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 11 | CI | Route the RH2008 self-hosting failure | Repair round 2: implementer → cleaner → auditor (repair only, compiled change after audit) → validator incl. the self-hosting RH2008 test | PR-introduced | Refactor only | — |

## Next

Implementer (01) → cleaner (02) → auditor (03) → validator (04) → delete sdd/work, push.
