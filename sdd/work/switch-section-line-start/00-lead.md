Role: lead
Commit: d5483e2
Verdict: lane claimed — behavior change

# Run: switch-section-line-start

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1495 — FMT-BREAK: every switch section and its first statement start their own line | yes — goals are proposals | Behavior change | Requests a new FMT-BREAK requirement and a change of FMT-BLANK-004; the issue requires a change doc |

Parent: thoenissen/Reihitsu#1482 (package C7). Blocker thoenissen/Reihitsu#1487 (B2) is closed, merged through thoenissen/Reihitsu#1510; the issue was written against `735d0dba`, so every statement about today's code is re-checked against current `main`.

Branch: `claude/festive-goldberg-qb8sp9` (the session's designated branch). Draft PR: thoenissen/Reihitsu#1558.

## Roles

Behavior-change lane: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; the issue manager before publishing when follow-up candidates exist. No role skipped.

## Maintainer clarifications

None beyond the issue.

## Maintainer answers

Output-question batch 1 (before the test implementer), all answered with the recommended option:

1. Sections behind `return` / `throw` / `goto case` / `continue` start their own line — A (change-doc visible change 1).
2. A section on a `break`'s line gets its blank line in the first run — A (visible change 2).
3. The first statement of a section moves behind its labels; a `/* */` comment between label and statement stays on the label's line — A (visible change 3; consistent with thoenissen/Reihitsu#1540's recommended option).
4. Labels sharing a line are stacked, one per line — A (FMT-BREAK-034, visible change 4).
5. An empty first statement stays where it is (`case 1:; break;`) — A (FMT-BREAK-035).
6. A `/** */` comment between label and first statement moves with the statement: `case 2:` alone, then `/** d */ A();` on the next line — B.
7. RH5417 (and RH5403, RH3208) fix results follow the new layout — A (visible change 5).

Standing instruction: every later output question of this run is answered with the lead's recommended option; the maintainer wants a summary at the end. Later questions are therefore recorded here with the recommendation taken, not asked.

Later output questions, decided under the standing instruction with the lead's recommendation (from 09-auditor.md):

8. A `/** */` comment between two labels on one line (`case 4: /** f */ case 5:`) moves with the following label: `case 4:`, then `/** f */ case 5:` — the same placement answer 6 gives for a first statement.
9. A run of comments that the parser attaches in front of the token on its line and that starts the moved content (`case 2: /** d */ /* c */ A();`, `case 3: /** a */ /** b */ case 4:`) moves with the token as written: `case 2:`, then `/** d */ /* c */ A();`. A comment that trails the label (`case 1: /* c */ A();`) stays on the label's line (answer 3). Why: the documentation comment stays with the token it documents, and the comments keep their order; this is the output the code writes today.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/issue-1495-<slug>` or the session's designated branch | The designated branch `claude/festive-goldberg-qb8sp9` | The session may push only to its designated branch | Branch names are internal | d5483e2 |
| 2 | Spec author | Done-when 2, first half ("`EnsureBlankLineBeforeSection` writes no line break that ends a line") | Dropped; FMT-BLANK-004 states that the blank-line phase writes both line breaks where it requires a blank line in front of a section on the `break`'s line | Impossible with today's phase order without a two-run blank line (FMT-PIPE-004) or a prediction (FMT-PIPE-001); thoenissen/Reihitsu#1498 reorders the phases | The output is identical either way | 06a5ae8 |
| 3 | Spec author | One requirement or three (section / labels / first statement) | Three: FMT-BREAK-033, -034, -035; FMT-BREAK-006 scoped to statements behind statements | One owner and one `Why` per decision; FMT-BREAK-034 can be dropped cleanly on output question 1 | Spec structure only | 06a5ae8 |
| 4 | Skeptic | FMT-BREAK-034's `Why` and the change doc claim stacked labels are what FMT-SWITCH-006 already writes | The claim is false (FMT-SWITCH-006 keeps labels where they stand); the spec author rewords it together with the maintainer's answers | Skeptic reproduced on `main` | Wording only | b304dc9 |
| 5 | Skeptic | Architecture finding: `SwitchCaseBraceRewriter.AddBraces` and the blank-line / line-break phases each write a line break in front of the token behind a braced section | Fix narrowly in this run: the braced shapes the skeptic lists (`case 1: A(); B(); break; case 2:` and `case 1: A(); B(); return 1; case 2:`, LF and CRLF, two runs) get tests and must hold; the structural single-owner change stays with thoenissen/Reihitsu#1518 / thoenissen/Reihitsu#1498 | The structural change is already planned there and needs the phase reorder of #1498; the narrow fix stays inside the components this run changes and their counterpart | FMT-BLANK-004 and FMT-PIPE-004 decide the expected output | e38c060 |
| 6 | Skeptic | Pre-existing on `main`: a braced section on the line of the switch's `}` gets a blank line before `}` in run 1 that run 2 removes | Adjacent fix in this run, with a two-run test | Same mechanism as decision 5, same component, an idempotency defect (FMT-PIPE-004) | An unchanged requirement (FMT-PIPE-004, FMT-BLANK) decides the output | e38c060 |
| 7 | Spec author | Accept the change doc | Accepted (status set by the spec author on the lead's request) after the skeptic and the amendment for the maintainer's answers | All visible output changes answered | Every visible change is covered by a maintainer answer | b304dc9 |
| 8 | Lead | The narrow fix of decisions 5 and 6 | Decisions 5 and 6 stand; the test implementer covers both braced shapes and the `}` shape with two-run LF and CRLF tests | See decisions 5 and 6 | FMT-BLANK-004, FMT-PIPE-004 | e38c060 |
| 9 | Test implementer | RH3208 code-fix output and Fix All not covered by a new test | Accepted | RH3208 reaches a switch only through the same phases the RH5417/RH5403 fix tests run; the switch layout is no diagnostic, so Fix All is not involved | Covered by maintainer answer 7 | 1e548b0 |
| 10 | Implementer | `SectionBehindDirectiveKeepsItsLine` expects the formatter to split a line that is disabled text (`#if DEBUG` without `DEBUG` defined) | The test is wrong; the test implementer makes the branch active so the test checks a section behind a directive, and keeps the disabled-text case unchanged if it adds one | Formatter transformations preserve disabled text (`AGENTS.md`); the test's intent is a section behind a directive | An unchanged rule decides the output | 56ad47f |
| 11 | Implementer | Existing test `DelimitedDocumentationRelocationTests.KeepsDelimitedDocumentationWithStatementBehindCaseLabel` asserts `case 1: /** d */ M(2);` unchanged | The test implementer updates its expected output to `case 1:` then `/** d */ M(2);` | Maintainer answer 6 and FMT-BREAK-035 decide it | Covered by maintainer answer 6 | 56ad47f |
| 12 | Implementer | Architecture finding: `AddBraces` now reads the next token's leading trivia before writing its line break | Keep the narrow fix; structural single owner stays with thoenissen/Reihitsu#1518 / thoenissen/Reihitsu#1498 | The check reads the tree earlier phases produced through the shared line-start fact; it neither predicts a later phase nor restates FMT-BLANK-004, and `ControlFlowBraceTransform` has the same kind of check | Output pinned by tests under FMT-BLANK-004 / FMT-PIPE-004 | e38c060 |
| 13 | Audit 1 | Repair round 1 worklist (09-auditor.md) | F1 (undecided label output): answered as question 8 above; the spec author corrects the change doc's claim that the maintainer decided every entry. F2 (FC-002, spec vs code on which comments move): answered as question 9; the spec author restates FMT-BREAK-033's rule to match, the test implementer pins both inputs and the multi-line `/** a⏎b */ A();` shape. F3 (FC-019/FC-003, PR-introduced): the test implementer adds two-run LF/CRLF tests for a comment between a braced section and the next token. F4 (FC-031 comment wording): the cleaner | All findings are same-requirement or PR-introduced; none repeats an earlier audit class | F1 and F2 decided under the maintainer's standing instruction | pending |

## Follow-up candidates

1. Horizontal spacing: `case 1: ;` becomes `case 1:;` (01-spec-author.md, 02-skeptic.md).
2. RH5103 does not report a statement of another section on the same line (`case 4: break; case 5: M();`); a behavior change of RH5103 (01-spec-author.md, 02-skeptic.md).
3. New-rule candidate: a first statement on its label's line, and labels sharing a line (01-spec-author.md, 02-skeptic.md).
4. Link thoenissen/Reihitsu#1533 to the `/** */`-between-label-and-statement shape (02-skeptic.md).
5. Extend thoenissen/Reihitsu#1498's Done-when to switch sections: FMT-BLANK-004's new sentence is to be reverted there (02-skeptic.md).
6. Extend thoenissen/Reihitsu#1518 with the braced-section doubling class, unless decision 6 fixes it fully in this run (02-skeptic.md).
7. `TokenGapNormalizer.PlaceTokenOnOwnLine` may count a line break inside a multi-line comment, so `A(); /* a⏎b */ B();` may keep `B();` behind the comment; not reproduced (05-implementer.md).
   Confirmed by the auditor on `main` too: in a braced block, `A(); /* c⏎d */ A();` leaves the second `A();` behind the comment; the structural cause is two "starts its line" tests in `TokenGapNormalizer` (09-auditor.md, hint 3).
8. RH5204 reports on the formatter's output `/** a⏎ b */ case 2:` and its fix makes no progress; same on `main` (09-auditor.md, hint 1).
9. RH5020 reports `A();⏎/* own line */ break;`, which the formatter keeps; same on `main` (09-auditor.md, hint 2).
