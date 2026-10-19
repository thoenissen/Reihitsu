Role: Lead
Commit: 53990ed5df40c0cb8ce6673ce31cefa2089ee59d
Verdict: lane claimed — tests, widened to tooling

# Run: thoenissen/Reihitsu#1069

- Issue: thoenissen/Reihitsu#1069 — add SHARED-DIAG-002 and SHARED-DIAG-003 to `sdd/specs/shared/diagnostics.md`.
- Slug: `name-value-literal-content`
- Branch: `claude/quirky-euler-dsqp4e` (the branch this session is bound to, used instead of `claude/issue-1069-<slug>`; the session may push only there).
- Draft PR: thoenissen/Reihitsu#1099
- Lane: specs and tests for existing behavior. The run adds two requirements to an enforced shared spec and their covering tests; no shipped source changes.

## Roles

| Role | Runs | Reason |
|---|---|---|
| Spec author | yes | writes the two requirements |
| Skeptic | yes | checks the spec diff against the code |
| Test implementer | yes | covering tests |
| Implementer | no | no production change; only runs if a covering test fails against the unchanged code |
| Cleaner | yes | |
| Spec checker | yes | |
| Auditor | yes | added by the widening to tooling |
| Validator | yes | |

## Maintainer clarifications (summarized)

- Source: principle decisions P3 and P5 on thoenissen/Reihitsu#912 (comment 6253795346).
- No change doc: both requirements state intended behavior most rules already follow.
- SHARED-DIAG-002: a rule compares a name by its value — without the `@` prefix, and a qualified name without whitespace or comments.
- SHARED-DIAG-003: a rule treats only the text of a literal or a comment as content that it neither reports nor changes; an interpolation hole and the whitespace before or after a literal are code.
- Rules that violate either requirement are bugs already tracked in thoenissen/Reihitsu#1071, #1074 and #1075; they are not fixed in this run, and a violation found here is not a new open question.
- Each requirement needs a covering test that passes today, taken from a rule that already behaves correctly; tag it with the ID.
- Must not change production code or any other spec, nor touch `sdd/work/coordinator/`. No issues besides open questions without maintainer approval.
- Concurrent runs: #1068, #981, #983, #987, #990. The only shared file is `sdd/specs/requirement-ids.txt` — expect merge conflicts there only.
- The P3 part about casing fixes withholding `@keyword` names is a behavior change tracked elsewhere and is not part of this run.

## Lane widening

`scripts/lane-check.sh tests --head worktree` reported `mismatch — claimed tests, diff needs tooling` after the lead committed two maintainer decision records under `squad/decisions/`, which the script classifies as tooling (known gap, thoenissen/Reihitsu#1054). Before the implementer stage, so the lane widens tests → tooling, as in thoenissen/Reihitsu#1035. The tooling lane adds the auditor; the spec author and skeptic keep running because the run writes a spec.

## Escalations

1. SHARED-DIAG-003 and rules whose subject is comment text — answered: reading B, worded generally (a rule whose subject is the text of a comment or directive, such as RH3201, RH7301, or a documentation rule RH8###, checks that text as its subject; the requirement restricts only rules that check code). Record: `squad/decisions/2026-10-18-shared-diag-003-comment-subject-rules.md`.
2. Using-directive rules RH7201/RH7203/RH7207 compare names by spelling — already filed as bug thoenissen/Reihitsu#1105; no open-questions issue; the case stays out of the spec; the PR lists #1105 under follow-up work. Record: `squad/decisions/2026-10-18-using-rules-name-by-value-filing.md`.
3. Skeptic Q1, does SHARED-DIAG-003 protect a literal's characters or its value — answered: the value. The spelling of a value (escapes, raw-string indentation stripped by the compiler) is the literal's own syntax that a rule about literals may check and a fix may respell while the value stays. Also answered: SHARED-DIAG-003 drops its code-fix clause and covers only what is reported; the fix side belongs to SHARED-FIX-003 (thoenissen/Reihitsu#1068). Record: `squad/decisions/2026-10-18-shared-diag-003-literal-value.md`.

Note: the container restarted after the skeptic; the first spec-author agent cannot be continued, so a fresh spec-author agent does the revision.
4. Auditor findings (pre-existing on `main`, follow-up candidates, no repair round): RH5023 hole blank lines → added to thoenissen/Reihitsu#1075; RH5601 interpolated raw-string closing tab → new bug issue after maintainer approval of its text. Record: `squad/decisions/2026-10-19-audit-findings-1069-tracking.md`.
