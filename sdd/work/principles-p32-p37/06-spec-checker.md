Role: spec checker
Commit: aa65453
Verdict: consistent

My prompt carried no intended fix, suspected cause, or verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Observations (no action needed)

These are not findings. They are places where the lead may want to know how a decided text was read, or what a test does not pin.

- **P32 bullets without a conforming shape.** FMT-PIPE-013 states three bullets (space between same-line comments, a comment's same-line placement with its statement, blank lines between leading comments). The covering tests pin the space between two comments and the own-line and trailing comments of a wrap. No test pins the blank-line-between-leading-comments bullet or a same-line leading comment, because today's code does not conform (thoenissen/Reihitsu#1381, #963). This is the "tag only conforming shapes" rule of 00-lead.md, clarification 4.
- **P35 reading.** SHARED-FIX-008 adds "a line break the fix keeps MUST stay a lone \\r" to the decided P35 text. The decided text is silent on kept breaks. The addition records today's output (thoenissen/Reihitsu#1363 Q6, "as they do today") and follows skeptic F1. The tagged RH5405 test asserts both the created and the kept breaks. The issue also names RH5417; the spec example and the test use RH5405 only, which the test implementer's handover already states.
- **P36 reading.** The issue says a rewrite that would "move a comment" is not applied. FMT-STRUCT-024 says "put a comment next to other code than the code it was written next to", and states that moving a comment onto its own line in front of the same code is not refused. FMT-STRUCT-002 itself moves such a comment into the block, so the literal wording could not hold. This is the spec author's recorded reading (01-spec-author.md); no maintainer decision covers it.
- **P33 reading.** SHARED-COMMENT-012 limits the rule side to a token that would start its line without the comment, and the formatter side to tokens a formatter requirement asks to start a line. The issue's two examples fall inside both limits. Also a recorded spec-author reading (01-spec-author.md).
- **P37.** Only FMT-BLANK-016 is written; no SHARED-STMT requirement exists. This is the maintainer's answer (b) in `squad/decisions/2026-10-22-1380-p37-shared-side-deferred.md`, so the issue's "and shared/statement-blank-lines.md" part is intentionally deferred to thoenissen/Reihitsu#1385.
- **P12 and P34 narrowings** are in the specs as the maintainer answered: SHARED-COMMENT-011 keeps a rule's own comment exemption (SHARED-CHAIN-004, RH5112 not reporting), and FMT-PIPE-014 keeps the per-construct layouts (FMT-BREAK-014, -021, -022, -026, the switch-arm non-goal) and limits the family statement to the named members. Both match `squad/decisions/2026-10-23-1380-p12-chain-exemption.md` and `squad/decisions/2026-10-23-1380-p34-family-scope.md`.
- **Placement.** The issue says P37 sits "next to FMT-BLANK-006" and P36 "next to FMT-PIPE-003". The new IDs are appended at the end of their specs (FMT-BLANK-016, FMT-STRUCT-024), as the ID rule requires; no ID was inserted out of order.
- **Lane.** 00-lead.md widened the lane from `tests` to `tooling` only because of the decisions entries (thoenissen/Reihitsu#1054); the diff holds no production or tooling code.

## Checked
1. Change doc exactly when required: ok. No change doc is added. The issue fixes the lane as docs/spec with no change doc; every amendment writes down a decided principle or today's behavior (FMT-PIPE-007's lone \\r sentence is the clarification thoenissen/Reihitsu#1081 item 2 decided), and the behavior changes are left to thoenissen/Reihitsu#1381-#1385.
2. Specs match the diff: ok. The diff changes no production code; the requirements follow the format (normative sentence first, Why, examples, `{|RH####:…|}` markup in the new analyzer examples). The decided texts of P32, P33, P34, P35, P36, P37 (formatter side) and the P12 clarification are recorded as the issue states them, with the readings listed above. Scope is exactly as fenced: only SHARED-FIX-008 changed in `shared/code-fixes.md` (and its `Uses` untouched, FMT-PIPE-007 already listed); no SHARED-DIAG ID; no P25-P31 text; `shared/statement-blank-lines.md` untouched; no `src/` file outside test projects; no file outside `sdd/specs`, `sdd/work/principles-p32-p37`, `squad/decisions`, and the tests.
3. IDs: ok. New IDs FMT-BLANK-016, FMT-PIPE-013, FMT-PIPE-014, FMT-STRUCT-024 and SHARED-COMMENT-012 take the next free number and sit at their sorted position in `requirement-ids.txt` (checked: the file is sorted in C order); nothing was removed, renumbered or retired.
4. Code owners: ok. No owner changed; `sdd/specs/README.md` is untouched and needs no change. `SpecConsistencyTests` passes within the architecture run below.
5. User documentation: ok. No rule page is affected (no rule behavior changed).
6. Open questions: ok. 03-spec-author.md lists none; the two escalations are answered in the decisions entries.
7. Requirements covered and specs consistent: ok. `scripts/test.sh --project architecture`: "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72". Every new requirement has tagged tests that pin conforming shapes (FMT-PIPE-013: 5, FMT-PIPE-014: 4, FMT-STRUCT-024: 8, FMT-BLANK-016: 4, SHARED-COMMENT-012: 3, plus the amended FMT-PIPE-007, FMT-BLANK-005, SHARED-FIX-008, SHARED-COMMENT-011 and RH5203-001). I read the tests tagged with every amended requirement (SHARED-COMMENT-011 in RH5112, RH5203, RH5304 and the RH5201/RH5301/RH5303 additions; FMT-STRUCT-002, FMT-STRUCT-006, FMT-SWITCH-007; FMT-BLANK-005; the FMT-PIPE-007 and SHARED-FIX-008 additions). None pins an output that contradicts the amended wording; the existing tests that pin non-conforming shapes (the two ReihitsuFormatterNodeBlankLineTests, the FieldDeclarationSplitTests, the `=> /* inline */ 1;` test) stay untagged.
8. Nothing left behind: ok. `sdd/work/` holds only `principles-p32-p37`, which the lead deletes before publishing.
