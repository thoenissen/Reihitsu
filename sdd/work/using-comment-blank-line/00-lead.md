Role: lead
Commit: b27f329
Verdict: lane claimed — behavior change

# Run: thoenissen/Reihitsu#1497

- Issue: thoenissen/Reihitsu#1497 — "FMT-USING: the blank line above a comment that leads a using directive of the same group is kept". Labels `enhancement`, `formatter`, `agent-filed` (goals are proposals). Package C4 of thoenissen/Reihitsu#1482.
- Blocker thoenissen/Reihitsu#1496 (C3) is closed, merged with thoenissen/Reihitsu#1541; this run starts on `main` at `b27f329`, which contains it.
- Slug: `using-comment-blank-line`.
- Branch: `claude/great-pasteur-ur6nr8` (the session's designated branch, used instead of `claude/issue-1497-<slug>`). Draft PR: thoenissen/Reihitsu#1547.

## Lane

Behavior change. The issue requests changed behavior of shipped rules (SHARED-USING-008, RH7207 reporting and its code fix, the using phase) and requires a change doc under `sdd/changes/`.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. The issue manager runs before publishing when follow-up candidates exist. No role is skipped.

## Maintainer clarifications

- The issue was revised on 2026-10-28 by maintainer decision (issue comment): the originally filed form (no blank line above a comment that leads a same-group using directive) is reported by RH5020 and left no clean layout. The blank line is now kept: SHARED-USING-008 and RH7207 accept it, the using phase stops removing it, and the formatter's output does not change. This implements thoenissen/Reihitsu#965.
- The issue's "Before the run" section: every statement about today's code is checked against current `main`; a statement an earlier package made true or moot is noted and dropped. The target behavior and "Done when" stay binding; a difference that changes the target goes to the maintainer.

## Maintainer answers

Batch 1 (before the test implementer), answered "as recommended":

1. RH7207 does not report one or more blank lines above a comment that leads a same-group directive (both issue examples) — A, accepted.
2. A fix keeps the blank line above the comment of a directive it moves — A, accepted (change doc change 2).
3. A fix keeps that blank line for a directive that stays in place — A, accepted (change 3).
4. A fix writes exactly one blank line where several stood above such a comment — A, accepted (change 4).
5. A fix keeps the file header's ending blank line above a comment that moves away from the top — A, accepted (change 5).
6. A comment-led directive moved into the middle of its group with no blank line above the comment in the source: the fix writes none (as today); RH5020 keeps reporting it — A.
7. A blank line above an ordinary comment on the directive's line (`using System;⏎⏎/* c */ using System.Linq;`): **B** — RH7207 does not report it, and the RH7201–RH7207 fixes and the formatter keep the blank line. RH5020 still reports the glued comment. This is a formatter output change, a departure from the issue's "the formatter's output does not change", decided by the maintainer.

Batch 2 (repair round 1, before the second audit):

8. RH7201–RH7207 fixes, an own-line comment behind a trailing block comment that spans lines (decision 5) and the trailing whitespace left behind `*/` (decision 6): the maintainer wants no change and no follow-up. The current state is acceptable; a trailing comment that spans lines between using directives is unwanted and gets no special handling. The lead applies the same answer to audit finding 1, whose input is the same comment shape: no change and no follow-up. Decisions 4–6 are withdrawn.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 7 | Repair 1 | Does maintainer answer 8 also cover audit finding 1? | Yes: finding 1 is not repaired and gets no follow-up; its failing tests and those of decisions 5 and 6 are removed; the passing class-member tests, the FMT-USING-005 pin and the Core tests stay | Finding 1's input is the same comment shape the maintainer called unwanted; the architecture finding of the audit had only that shape as its evidence | The fix output stays as on the audited tree for that shape; the answer covers it | dfbeb46 |
| 8 | Repair 1 | Keep the run's passing tests that use a trailing block comment spanning lines (members b and e of finding 1's class, four tests, and the FMT-USING-005 pin `DirectiveWithoutCommentGetsNoLineBreakBehindATrailingCommentSpanningLines`)? | Remove them too | Maintainer answer 8: the shape is unwanted and gets no special handling, so the run pins no output for it | Removing tests changes no output | 8909de6 |
| 1 | Claim | Branch name | Use the session's designated branch `claude/great-pasteur-ur6nr8` instead of `claude/issue-1497-<slug>` | The session environment permits pushes only to that branch | Branch names are internal | claim commit |
| 2 | Test implementer | A comment-led directive with a blank line above it that the reorder makes the block's first directive (`using System.Linq;⏎⏎// About System⏎using System;`): in scope? | Out of scope; `main`'s behavior stays and no test is added | SHARED-USING-011 and FMT-USING-010 cover a directive that continues the group of its predecessor; the block's first directive has none, and FMT-USING-009 already decides the top of the block | No requirement changes for it, so its output stays as on `main` | 8f0adca |
| 3 | Implementer | Architecture finding: whether a blank line stands above a using directive is now measured twice, by RH7207 (`HasBlankLineBefore`, leading trivia only) and by the using phase (`UsingLeadingTriviaBuilder.HasBlankLineAboveLeadingComment`, which also counts the predecessor's line end) | Change the structure in this run: one `Reihitsu.Core` measurement that RH7207 and the using phase both call, before the cleaner | The narrow fix added a second statement of the SHARED-USING-011 measurement; both components are already changed by the run, and `Reihitsu.Core` already holds the shared `IsLedByComment` | Outputs stay as the tests pin them; only which code measures changes | 59340b1 |
| 4 | Auditor | Architecture finding: three places in the using phase decide how many line breaks stand between two rebuilt neighbors (`UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak`, the rewriter's restore branch, and the FMT-USING-010 insertion this run added); audit finding 1 is an input where they disagree | Change the structure in this run: one owner in the using phase for the line breaks between two rebuilt neighbors, applying FMT-USING-002/-010 and the separating line break | The run's own insertion added a third place; a narrow fix for finding 1 would add a fourth branch. The components are the using phase, which the run already changes | Outputs follow unchanged requirements (FMT-USING-010, SHARED-FIX-003); no answered output changes | withdrawn after maintainer answer 8: the shape is unwanted and gets no special handling |
| 5 | Auditor | Pre-existing on `main`: the RH7201–RH7207 fixes move an own-line comment behind a trailing comment that spans lines (`using System.Collections; /* a⏎   b */ using System;⏎// About Linq⏎using System.Linq;`), against SHARED-FIX-003 | Adjacent fix in this run, as part of decision 4, with a test | Same mechanism as decision 4, in the component the run changes; a safety rail the run's structural change touches anyway | SHARED-FIX-003 (unchanged) demands it | withdrawn: no requirement demands it (test implementer, `10-test-implementer.md`); maintainer answer 8 keeps `main`'s output |
| 6 | Auditor | Trailing whitespace after `b */` in the same fix outputs (pre-existing) | Adjacent fix within decision 4 if the single owner removes it; otherwise follow-up | Same shapes, same component | No new layout; trailing whitespace is never wanted output (cleanup) | withdrawn: maintainer answer 8 |

## Repair round 1 (audit `b98cf09`)

- Finding 1 (high, FMT-USING-010, code fix): not repaired, per maintainer answer 8 (the input is a trailing block comment spanning lines between two directives, which gets no special handling); its failing tests are removed, no follow-up.
- Finding 2 (medium, Core tests for `IsLedByComment`/`HasBlankLineAbove`) → test implementer.
- Finding 3 (low, redundant `using System.Linq;` in `UsingLeadingTriviaBuilder.cs`) → cleaner.

## Follow-up candidates

None yet.
