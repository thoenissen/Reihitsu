Role: Lead
Commit: 53990ed5df40c0cb8ce6673ce31cefa2089ee59d
Verdict: lane claimed — tests

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
| Implementer | no | lane runs no implementer |
| Cleaner | yes | |
| Spec checker | yes | |
| Auditor | no | lane runs no auditor |
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
