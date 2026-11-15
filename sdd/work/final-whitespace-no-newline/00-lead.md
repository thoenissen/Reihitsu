# Lead — final-whitespace-no-newline

- Issue: thoenissen/Reihitsu#1747 — `agent-filed` (goals are proposals).
- Slug: `final-whitespace-no-newline`
- Branch: `claude/peaceful-sagan-kchdaf` (the session's mandated branch; replaces the `claude/issue-<N>-<slug>` name)
- PR: thoenissen/Reihitsu#1750 (draft, claim)
- Lane: **bug fix** — the issue reports an observed-versus-expected difference against an existing requirement (FMT-CLEAN-005).
- Roles run: skeptic (premise), test implementer (reproduction), implementer, cleaner, spec checker, auditor, validator. Spec author skipped unless a clarification is needed.

## Maintainer answers

- Earlier answer, not asked in this run: thoenissen/Reihitsu#923, comment 6232535812, Q3 — whitespace behind the last token is removed whether or not a final line break follows; both inputs give the same output. It covers every expected value of `EndOfFileWhitespaceTests`. No output question was asked in this run.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's mandated branch `claude/peaceful-sagan-kchdaf` | The environment permits pushing only there | Internal | claim commit |
| 2 | Skeptic | Premise | Proceed; bug fix lane kept | Reproduced via CLI; the maintainer decided the target output in #923 (Q3) | Covered by a maintainer decision already recorded in `cleanup.md` | – |
| 3 | Skeptic | Issue goal scope (agent-filed) | Widen to every failing member in `01-skeptic.md` (spaces/tab behind the last token, behind trailing block comments, comment-only last line; LF, CRLF, BOM) | Same mechanism; #923 Q3 covers all of them | #923 Q3 decides the output for every member | – |
| 4 | Skeptic | Spec | Spawn spec author for a clarification: drop the end-of-file part of the `cleanup.md` non-goal and widen FMT-CLEAN-005 so it states the removal (keeps the issue's tag) | Bug-fix lane allows the spec author for a clarification; #923 Q1 precedent | Clarifies an output the maintainer already decided | – |
| 5 | Skeptic | Detached roots (FMT-CLEAN-008) | Leave unchanged; change only the end of a file | No caller sees the difference | No caller reaches it | – |
| 7 | Test impl. | Untagged `#endif···` characterization test locks main's behavior that #960 item 1 may change | Keep it: it guards this fix against overreach; the run for #960 item 1 flips it | Asserts main's unchanged output | b0d9c95 |
| 6 | Skeptic | PR links | `Closes #1747`, `Refs #960` | #1747 is item 2 of #960 | Internal | – |

## Follow-up candidates

1. Update thoenissen/Reihitsu#960: item 2 is done by this run; keep item 1 (whitespace at the end of a directive line); its triage comment calls item 2 an "extension", contradicting #923 (01-skeptic.md).
2. FMT-CLEAN-007 says a file with no token besides directives MUST be left unchanged, but the CLI removes a whitespace-only last line behind `#pragma …`; carries the open question whether the spec or the code is wrong (01-skeptic.md).

## Worklist / next step

- Done: skeptic `01-skeptic.md` (proceed, no output questions).
- Done: spec author `02-spec-author.md` (FMT-CLEAN-005 widened, commit 889d042; no output questions). Output-question batch 1: nothing open, no stop.
- Done: test implementer `03-test-implementer.md` (reproduced; 16 failing, 8 guard tests passing; b0d9c95).
- Done: implementer `04-implementer.md` (tests pass; 970fe19, 4103b30).
- Done: cleaner `05-cleaner.md` (clean, 0 warnings; 1578384).
- Done: spec checker `06-spec-checker.md` (consistent; 3a6d91e). Main not ahead; Release build green. Output-question batch 2: nothing open.
- Done: auditor `07-auditor.md` — pass with text findings; audited SHA 2a0087b.
- Open worklist (text-only, proven with verify-text-only): (1) FMT-CLEAN-005 first sentence narrowed → spec author; (2) Maintainer answers recorded (done by lead); hint: `EndOfLineWhitespace` class summary → cleaner. Hint on U+FEFF literal in test skipped (changing literal spelling is not text-only; non-blocking).
- Next: spec author + cleaner text fixes, then verify-text-only, validator, publish.
