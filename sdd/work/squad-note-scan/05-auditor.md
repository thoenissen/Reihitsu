Role: auditor
Commit: b78e195929bec7d4486b64543ced23034bbd0513
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope
Full audit. Head b78e195 (checked out, `git rev-parse HEAD`); base `origin/main` 1102aa52fe204b2c5b423d14ccd697a433466e13, an ancestor of head (`git merge-base --is-ancestor`). The diff is what the handovers describe: one test-project file, `src/Reihitsu.ArchitectureTests/General/SquadNoteReferenceTests.cs` (the `RunWorkFolder` skip, the updated class and method summaries, five fixture tests), plus this run's `sdd/work/squad-note-scan/`. `scripts/lane-check.sh tooling`: "LANE CHECK: confirmed — claimed tooling, diff needs only tests".

Issue thoenissen/Reihitsu#1807 (`agent-filed`), "Done when": the scan passes on a tree whose `sdd/work/` names a note (`ScanIgnoresNoteReferenceInRunHandover`, `ScanIgnoresNoteReferencesAnywhereBelowSddWork`, and the real tree, whose `01-test-implementer.md` deliberately names this run's note), while a permanent file still fails it (`ScanDetectsNoteReferenceInPermanentFilesBesideRunHandover`); a fixture test covers both. Addressed. The validator-side alternative is not taken; `squad/roles/validator.md` is unchanged, as the issue allows.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `squad/notes/README.md:11` | – (one rule stated in two places) | – | PR-introduced | The README states "No file outside `squad/notes/` names a note … `SquadNoteReferenceTests` … enforces this." After the change the test enforces that only for files outside `sdd/work/`, and its class summary (`SquadNoteReferenceTests.cs:15-17`) now states the opposite for that folder: "its handovers may name the run's note". The two statements of the rule disagree, and the README's claim about what the test enforces is no longer exact. Documentation only: it cannot reach compiled behavior, diagnostics, or rule metadata. | Name the exception in the README's "Never referenced" item, e.g. that a run's temporary handovers under `sdd/work/`, deleted before merge, are not scanned. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which files the note-reference scan skips as temporary working docs | Root-relative path with `/` separators, `StartsWith("sdd/work/", Ordinal)` — every scanned file at any depth below the root's `sdd/work/`, nothing else (`sdd/workflow.md`, `sdd/work-archive/`, `src/Sample/work/`, `documentation/sdd/work/` stay scanned; tests pin each) | `WorkingDocumentsTests.WorkingDocumentsDirectoryContainsNoFiles` (`src/Reihitsu.SelfHosting.Test/Repository/WorkingDocumentsTests.cs`): every file of any extension under `Path.Combine(root, "sdd", "work")`, `SearchOption.AllDirectories`, must not exist; run in CI on every pull request (`.github/workflows/SonarCloud.yml` loop includes `Reihitsu.SelfHosting.Test`; the non-Sonar branch runs `dotnet test` over the solution) | Same folder, same depth, counterpart span a superset (all extensions). Every file the scan skips is a file the counterpart forbids at merge, so no permanent file escapes the guard. On a case-insensitive filesystem the counterpart also matches `SDD/Work`, which the ordinal skip still scans: stricter, never a hole. | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| `sdd/work/` holds temporary run docs (`rg -n 'sdd/work\|"sdd", "work"' src scripts .github`) | `SquadNoteReferenceTests.RunWorkFolder` (skip in scan); `WorkingDocumentsTests` (must be empty before merge) — two different decisions over the same folder, the second being what makes the first safe; `LaneClassifier._textPrefixes` (`src/Reihitsu.Tooling/LaneClassifier.cs:15`, classifies all of `sdd/` as text — a different policy); `LaneClassifierTests` data row | single owner per decision |
| Note-reference scan and its exclusions | `SquadNoteReferenceTests` only (`NotesFolder`, `NotesReadme`, `RunWorkFolder`, `_skippedDirectories`) | single owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ScanIgnoresNoteReferenceInRunHandover` | A note path in a run handover under `sdd/work/<slug>/` is not reported | Any finding; was red before the fix (01: 1 finding), so the fixture text does match the pattern (FC-033) | `IsEmpty` on `ScanForNoteReferences` | adequate |
| `ScanIgnoresNoteReferencesAnywhereBelowSddWork` | Skip holds directly in `sdd/work/`, two levels below a run folder, and for `.cs` | Any finding; red before the fix (2 findings) | `IsEmpty` | adequate |
| `ScanDetectsNoteReferenceInPermanentFilesBesideRunHandover` | Permanent files still fail while a handover names a note; the handover is not reported | Count other than 2, or either permanent path missing | `HasCount(2)` + two `Contains` on path prefix | adequate (exact set) |
| `ScanDetectsNoteReferenceInPathsSharingSddWorkPrefix` | Skip requires the folder boundary (`/`) | A prefix without the slash (`"sdd/work"`) would drop both findings | `HasCount(2)` + two `Contains` | adequate |
| `ScanDetectsNoteReferenceInWorkFolderOutsideRepositorySddWork` | Skip is root-anchored, not a folder-name or substring match | A `Contains("sdd/work/")` or a directory-name skip of `work` drops one or both findings | `HasCount(2)` + two `Contains` | adequate |

## Failure classes dismissed
- FC-001, FC-002: the skip's span equals the decision it protects (folder deleted before merge), checked against `WorkingDocumentsTests` above; "root-relative" and "folder boundary" are explicit in the predicate (prefix with `/`, path relative to the scan root).
- FC-003: both sides covered — inside (`sdd/work/` direct and nested), outside sharing the prefix, a `work` folder elsewhere, a nested `sdd/work`.
- FC-015, FC-016: no second copy of the scan; the other repository-wide scans in the default run (`SelfReferentialTrackerReferenceTests` over `*.cs`, the `Sdd/*` tests over `sdd/specs/`, `PhaseOwnershipDocumentationTests` over project `*.cs`) do not read handover Markdown, so no sibling test trips on `sdd/work/` handovers.
- FC-017: see the assertion table; every positive test asserts the exact set.
- FC-031: the class summary and `ScanForNoteReferences` summary describe the new skip; the `RunWorkFolder` summary states its reason. FC-033: reproductions were red before the fix (01). FC-034, FC-035: the fixtures are the reported shape (a handover naming a note), expectations written before the implementation.
- FC-032: no compiled behavior outside the test project changed; the cleaner's Release build (0 warnings) is on the final code.
- FC-037: no comment-only proof claimed.
- FC-044: the guard reads a path, it models nothing.
- FC-004–FC-014, FC-018–FC-030, FC-036, FC-038–FC-043: formatter, analyzer, code-fix, rewrite, and unsupported-shape classes; the change touches none of those surfaces, only a repository guard in `Reihitsu.ArchitectureTests`.
- Letter case of the path (`SDD/Work/`), CRLF content: no caller produces the folder in another case; line endings do not reach the path decision.

## Other checks
- Visible output: none in question; architecture tests are internal (00-lead decision #1).
- Special cases / unsupported shapes: N/A.
- Comments: no `TODO`, no commented-out code, no tracker number in source (`rg -n '#[0-9]|TODO'` over the changed file finds nothing).
- Scope: no edit outside the run's file and `sdd/work/squad-note-scan/`.

## Architecture findings
_None._

## Hints
- The 00-lead decision #1 text says the lane check classifies the path as Test and tooling is a wider claim; the check confirms that. Nothing to do.

## Follow-up candidates
_None._
