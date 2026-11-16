# Lead — csharp15-union-with-element

- Issues (prompt order): thoenissen/Reihitsu#1762 (`agent-filed`), thoenissen/Reihitsu#1763 (`agent-filed`). Both goals are proposals.
- Slug: `csharp15-union-with-element`
- Branch: `claude/gifted-brahmagupta-pew7ub` (the session's designated branch; the environment permits pushing only there, so the playbook's `claude/issues-...` name is not used)
- Draft PR: #1782
- Lane: **behavior change**.
  - #1762: new formatter behavior for C# 15 union declarations (braces, members, constraints, empty body, field split, case-type list wrap) — requested changed output with open output questions → behavior change.
  - #1763: new horizontal-spacing behavior for the C# 15 `with(...)` collection-expression element — requested changed output with an open output question → behavior change.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-ups exist. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

Batch 1 (before test implementer), all recommended options chosen:
- Q1 wrapped case-type list: A — keep the primary-constructor (aligned) layout.
- Q2 empty union body: A — `{ }` becomes `;`.
- Q3 union gets class layout (braces/members on own lines, `(` joins name, each `where` on its own line, combined static field split; RH7101 fix offered; RH5404 fix lays out nested union): A — accepted.
- Q4 `with(...)` element: A — no space between `with` and `(` (also `with()`); RH5305 fix output follows.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/gifted-brahmagupta-pew7ub` | Environment allows pushes only there | Internal | claim commit |
| 2 | Skeptic | Architecture finding: type-declaration kind list stated in seven places, already drifted (extension blocks) | Fix narrowly: add `UnionDeclaration` to the lists the run's requirements need; do not generalize to every `TypeDeclarationSyntax`. Structural consolidation + extension-block layout become a follow-up carrying the output question | Generalizing changes extension-block output, which is undecided and outside the run's issues | Union outputs are asked in batch 1; extension blocks keep `main`'s output | — |
| 3 | Skeptic | Test for the C# 14 gate of FMT-STRUCT-017 | No test for the false branch; spec wording kept | Roslyn 5.12 produces no union nodes under C# 14, no caller reaches it | Unreachable | — |
| 4 | Skeptic | Change-doc amendment: RH5404 fix (union inside a type) and RH5305 fix (`with (4)`) outputs change as a consequence | Shown inside batch-1 questions Q3/Q4; spec author records them in the change docs | Direct consequences of Q3/Q4 | Asked | — |
| 5 | Skeptic | Declaring a shape unsupported | Not an option: deletes no special case (skeptic) | AGENTS.md: a shape that deletes nothing is not declared | — | — |
| 6 | Repair 1 | Run cleaner after a spec-text-only repair? | Skip; prove text-only with lane check | Cleaner formats C# and builds; nothing compiled changes | Internal | — |
| 7 | Repair 1 | Add FMT-PIPE-029 tag to RH7101 union tests; `indentation.md` base-list paragraph and RH3202-001 list only class/struct/record/interface | No extra tag (checker did not require it); spec-wording lists folded into follow-up (c) on the duplicated type-kind lists | Outside run requirements; behavior already kind-independent | Wording only | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author.

## Log
- 01-spec-author done (2272e74): two change docs; Q1 (wrapped case-type list), Q2 (empty union body), Q3 (accept class-like union layout incl. RH7101 fix offered), Q4 (`with(` spacing) open for batch 1.
- Follow-up candidates: (a) attribute lists on unions not split — SHARED-ATTR-001 has no union target (01-spec-author); (b) argument list on the line after its name lands at the statement column (01-spec-author).
- 02-skeptic done (984dea0): #1762 proceed, #1763 proceed. Callers: CLI; RH7101, RH5404 fixes (#1762); RH5305 fix (#1763). Test `UnionWithOneLineBracesFormatted` (src/Reihitsu.Cli.Test/EndToEnd/CliLanguageVersionEndToEndTests.cs:41-49) must change. Analyzer tests on unions may hit CS0518/CS0656 with .NET 9 refs (#1766).
- Follow-up candidates: (c) extension-block braces/`where` clauses stay glued on main; type-kind list duplicated in seven places — follow-up carrying output question, rec. class-like layout (02-skeptic); (d) stale RH5404 exclusion comment for unions — input for #1767 (02-skeptic); (a) belongs into #1767 as well.
- Batch-1 answered (1A 2A 3A 4A). 03-spec-author (038aaeb): answers recorded, change docs Accepted (lead accepts). Next: test implementer.
- 04-test-implementer (9dff24e): 36 failing tests (30 formatter, 5 analyzer, 1 CLI). Union code-fix tests use CompilerDiagnostics.None (CS0518/CS0656, #1766). Next: implementer (narrow fix per decision 2).
- 05-implementer (5a6d3e1, handover 547aac1): all 36 pass; full formatter/analyzer/CLI/core/arch green in Debug. 8 prod files, +77/-7. Next: cleaner.
- 06-cleaner (51509e3): clean, Release 0 warnings. Next: spec checker.
- 07-spec-checker (a829d4b): 1 finding #1762 — FMT-PIPE-029 last paragraph lacks "union"; change doc Affected requirements misses FMT-PIPE-029 and RH7101-002. Repair round 1 → spec author (fresh). Cleaner skipped for this round: spec-text-only repair (decision 6). Then spec checker again.
- 08-spec-author (d1af94b): FMT-PIPE-029 names union; change doc lists FMT-PIPE-029, RH7101-002. Next: spec checker (fresh).
- 09-spec-checker (f23bbdb): consistent. main not ahead (1d2d29a); build green. Next: auditor at HEAD.
