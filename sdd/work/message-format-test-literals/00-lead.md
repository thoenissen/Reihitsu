# Lead — message-format-test-literals

- Issue: thoenissen/Reihitsu#1912 — `agent-filed` (goals are proposals)
- Slug: `message-format-test-literals`
- Branch: `claude/cool-mendel-cmd1vb` (session-designated); draft PR #1967
- Lane: **tests** — the issue changes only test code so it reads diagnostic messages from the analyzer instead of quoting literals; no production code, no visible output.
- Roles run: skeptic (premise and scope against the code), test implementer (performs the test refactoring), cleaner, spec checker, validator. Skipped: spec author (the run adds no spec), implementer and auditor (not in the tests lane).
- Note: the issue names `src/*.Test/`; the test projects live under `tests/`. Its `rg` command finds 23 files under `tests/`, some of them in `Reihitsu.Tooling.Test` and `Reihitsu.ArchitectureTests`, which may not quote analyzer messages at all.

## Maintainer answers

None.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Issue's search path `src` and its `rg`-based "Done when" | Search `tests/`; done when no test quotes or interpolates a diagnostic or code-fix resource text as a literal | The test projects live under `tests/`; the `rg` misses interpolated and self-hosting literals | Tests only | b24eb47 |
| 2 | skeptic | Placeholder messages (RH2008, RH2009, RH2111): literal or formatted `MessageFormat` | Format `MessageFormat` with the test's arguments | Otherwise a wording edit still touches tests, which defeats the issue's purpose | Tests only | — |
| 3 | skeptic | Include `LineCountSelfHostingTests` literals | Include | Same coupling, test path, within the issue's purpose | Tests only | — |
| 4 | skeptic | RH5048/RH5112 `VerifyTitleNamesFirstMemberAccess` title-literal tests | Delete both | Title wording is pinned by `AnalyzerPackageMetadataTests` against the rule page and `AnalyzerResourcesTests` ties the descriptor to its resource; without the literal they test nothing | Tests only | — |
| 5 | skeptic | RH5112 code-fix title literal | Compare with `CodeFixResources.RH5112Title` | Same coupling | Tests only | — |
| 6 | skeptic | Inline replacement or new helper | Inline, existing `MessageFormat`/`CreateMessage`; no new helper | Bases already offer `MessageFormat` | Tests only | — |
| 7 | skeptic | Tooling/Architecture `DiagnosticId, "` matches (4 lines) | Leave unchanged | They are CLI argument arrays, not messages | Tests only | — |

## Follow-up candidates

- An `ArchitectureTests` check that fails when a test file quotes a diagnostic or code-fix resource text as a literal (01-skeptic.md).

## Worklist / next step

- Skeptic done (proceed, goals adjusted; no output questions). Next: test implementer.
