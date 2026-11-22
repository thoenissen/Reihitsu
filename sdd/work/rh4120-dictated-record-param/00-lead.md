# Run: rh4120-dictated-record-param

- Issue: thoenissen/Reihitsu#1939 — `agent-filed` (goals are proposals; the visible output was decided by the maintainer, see below)
- Branch: `claude/squad-1939-81sm3x`; draft PR #1947
- Lane: behavior-change — the issue asks for changed analyzer behavior (RH4120 stops reporting a positional record parameter whose generated property implements a property declared in a referenced assembly), which needs a spec change and production code.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

1. #1939 output question (answered on the issue, comment 2026-11-22): **option A** — RH4120 does not report a positional record parameter whose generated property implements a property of another (referenced) assembly; that name is dictated, as for properties under SHARED-CASING-014. Example: `public record Person(string name) : INamed;` with `INamed.name` declared in a referenced assembly is not reported.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Where does the exemption live? | As a bullet of SHARED-CASING-014 referenced from RH4120-001, no new requirement ID | One statement of what makes a name dictated | Spec structure only | 243da8d |
| 2 | Spec author | Parameter of a derived record that inherits a dictated property (`Derived(string name) : Person(name)`) | Stays reported (no property is declared for it) | Today's behavior; only the compiler-declared property decides | Unchanged output | 243da8d |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Spec author done (01, 243da8d): change doc `sdd/changes/2026-11/record-parameter-dictated-name.md` (Draft).
- Open output question for the batch before the test implementer: the override case (`record Item(string title) : MetaRecord` where `MetaRecord.title` is an abstract property of a referenced assembly) — the maintainer's answer names only "implements"; a case the answer does not name is a new question.
- Next: skeptic (02).
