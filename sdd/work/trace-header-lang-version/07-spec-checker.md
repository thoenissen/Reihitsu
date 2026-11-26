Role: spec checker
Commit: afff6ff
Verdict: #1924: consistent; #1949: consistent (one optional note, finding 1)

Read: `squad/roles/spec-checker.md`, `squad/README.md`, `00-lead.md`, handovers 01-06, both change docs, `git diff origin/main...HEAD`. My prompt carried no intended fix or hoped-for verdict.

## thoenissen/Reihitsu#1924

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1. ok: `sdd/changes/2026-11/trace-diff-header.md` is Accepted, has `Visible output changes` (none: repository tool), `Decisions`, no task list. A change doc is right: the trace's header output changes intentionally (behavior-change lane).
2. ok: no `sdd/specs/` file is touched; the change doc states the trace has no spec (`reihitsu-format.md` excludes it) and CLI-024 exists and stays unchanged. `scripts/README.md` (*Formatter pipeline trace*) states the new header and matches the change doc.
3. ok: no ID added; `sdd/specs/requirement-ids.txt` untouched.
4. ok: no spec header touched, so no owners changed.
5. ok: no rule page affected.
6. ok: no output questions in the handovers; `Maintainer answers` is empty and nothing is open.
7. n/a: the diff does not touch `sdd/specs/`, so the architecture suite was not run; no requirement is added.
8. ok: the change doc's `Affected requirements` is none; the test implementer's table lists no requirement and the new tests carry no `[Requirement]` tag (checked with `rg`).
9. ok: `sdd/work/` holds only this run's directory (the lead deletes it before publishing).

## thoenissen/Reihitsu#1949

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 5 (optional) | `AGENTS.md:176` | The summary line `scripts/trace.sh <file> [--passes N] [--no-install]` omits `--lang-version`, which `scripts/README.md` and `trace.sh` now list. It is a summary and not wrong, so it is optional for the lead to align. | spec author |

### Checked
1. ok: `sdd/changes/2026-11/tooling-lang-version.md` is Accepted, has `Visible output changes` (none: repository tools) and `Decisions`, no task list. It is needed: new option on both tools (behavior-change lane).
2. ok: no spec exists for either tool (`sdd/README.md` gives `Reihitsu.Tooling` none). The stated usage lines, the three error messages, the warning text, and the `InternalsVisibleTo("Reihitsu.Tooling")` decision match `PipelineTraceCommand.cs`, `CodeFixRunCommand.cs`, and `AssemblyInfo.cs`. `scripts/README.md` and the usage comments of `trace.sh` and `apply-fix.sh` list the option.
3. ok: no ID added, ledger untouched.
4. ok: no spec header touched.
5. ok except finding 1: no rule page affected.
6. ok: no open questions; decisions 1-10 in `00-lead.md` are internal.
7. n/a: `sdd/specs/` untouched, no new requirement, architecture suite not run.
8. ok: the change doc names no requirement and the new tests (`PipelineTraceCommandLanguageVersionTests`, `CodeFixRunCommandLanguageVersionTests`) carry no `[Requirement]` tag, matching the implementer's `Tests` table; the changed `FixtureRunnerTests` call sites alter arguments only (decision 9).
9. ok: see above.

## Whole run

- Lane: the diff holds change docs, text, tooling, production, and tests, so behavior-change holds (change doc present).
- Not run: `scripts/test.sh --project architecture` (no spec change). The validator covers the default test set.
