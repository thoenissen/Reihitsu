Role: spec checker
Commit: 2a7988c (diff checked: c857b2d...2a7988c, with the repair round 3523e98..2a7988c read separately; the handover cannot name its own SHA)
Verdict: #1423: consistent; #1424: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I derived the check from SHARED-DIAG-010 and the formatter requirements, not from the implementer's wording.

## thoenissen/Reihitsu#1423

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
Unchanged since 03-spec-checker.md: the repair round (`git diff --stat 3523e98..HEAD`) touches no line of `squad/roles/skeptic.md` outside check 3, so the new `## Follow-up candidates` section (`squad/roles/skeptic.md:74`) is as checked before. Checks 1 to 8: ok.

## thoenissen/Reihitsu#1424

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. Change doc: not required (agent-instruction wording; no intended behavior of a shipped surface changes). ok
2. Text matches the specs it relies on:
   - SHARED-DIAG-010 (`sdd/specs/shared/diagnostics.md:534-536`): the exclusion "order or content of code ... reports what its own requirements state, also where the formatter keeps the code" and the `Why:` sentence "The order of members and the content of comments are not the formatter's decision — it keeps them because it does not judge them". FC-014 (`squad/failure-classes.md:60`) and check 3 (`squad/roles/skeptic.md:23`) now limit the exclusion to "order or content that the formatter does not judge, such as the order of members or the content of comments", which restates the spec and drops the earlier universal claim (audit finding 2). Both places give the same reason.
   - Each formatter-judged non-layout example in the FC-014 row is a requirement whose `Why:` names an analyzer rule: using order, FMT-USING-001 (`using-directives.md:23-25`, RH72xx); expression bodies, FMT-STRUCT-004 and FMT-STRUCT-005 (`structural-transforms.md:143,209`, RH3202, RH3203, RH3205 to RH3208); braces of empty types, FMT-STRUCT-017 (`:752`, RH5412 to RH5416); trailing commas, FMT-STRUCT-019 and FMT-STRUCT-020 (`:831,883`, RH5409 to RH5411); field splitting, FMT-STRUCT-021 (`:937`, RH7101).
   - "For the rule's construct" matches FMT-STRUCT-020 ("Object initializers ... MUST keep their trailing comma"): a trailing-comma rule for object initializers is in the class, as the decision's example states.
   - The two excluded examples are consistent with the formatter specs: no formatter requirement reorders members (`rg -i "member order|reorder" sdd/specs/formatter/` finds only using blocks and the documentation-comment non-goal "does not ... reorder ... elements"), and `documentation-comments.md:18` says the formatter does not reflow prose.
   - FC-013 and its clause in check 3 are unchanged (`git diff` shows no FC-013 line changed). FC-013 states which rules may report what the formatter keeps (SHARED-DIAG-010's normative exclusion); FC-014 states whether the formatter produces what a new rule asks for. A using-order rule is outside SHARED-DIAG-010 yet inside FC-014, and 05-implementer.md says so; I found no contradiction in the texts. ok
3. IDs: FC-014 keeps its ID; no requirement ID touched. ok
4. Code owners: no spec touched. ok
5. User documentation: no rule page affected; links `../sdd/specs/shared/diagnostics.md` (failure-classes.md) and `../../sdd/specs/shared/diagnostics.md` (skeptic.md, earlier in check 3) resolve. ok
6. Open questions: none in the handovers 04 to 06 beyond the one the maintainer answered. ok
7. Coverage: the diff does not touch `sdd/specs/`; `scripts/test.sh --project architecture` not required. ok
8. Left behind: `ls sdd/work` shows only `skeptic-followups-fc014`, which the lead deletes before publishing. ok

Decision entry `squad/decisions/2026-10-23-1424-fc014-formatter-owned-concerns.md`: follows the `squad/decisions/README.md` template (Date, Escalation 5, Run, Question, Answer, Reason "none given"); the answer 1B is summarized in English and matches what the repaired texts implement. It supersedes the "narrow to layout concerns" clarification in `00-lead.md`, which is a run-local file deleted before publishing.

### Observations (not findings)
- The FC-014 row's "content of comments" follows SHARED-DIAG-010's own wording. The formatter does rewrite comment layout (documentation comments) and region names, but not the prose or order; no text claims otherwise.
- Check 3 says "layout" without re-defining it; it is tied to SHARED-DIAG-010 earlier in the same check, and the catalog row lists the kinds. Unchanged from the audit's hint.

## Whole run

- `git diff --stat 3523e98..HEAD`: handovers 04 to 06, the decisions entry, `squad/failure-classes.md` (1 line), `squad/roles/skeptic.md` (1 line).
- No C#, spec, or test file changed in the run; nothing under `sdd/` outside this run's `sdd/work/`.
