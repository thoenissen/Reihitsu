Role: lead
Commit: 20adf02
Verdict: post-publish docs repair

## Run

Issues thoenissen/Reihitsu#1329, #1389, #1241, #1344, #1242; PR thoenissen/Reihitsu#1456; branch `claude/dreamy-albattani-e7phpl`; lane bug fix. The earlier work docs were deleted before publishing and are in the branch history (`git show 20adf02^:sdd/work/doc-comment-element-safety/00-lead.md`).

## Maintainer instruction (2026-10-25, summarized)

Revert every change this run made under `documentation/rules/` (RH8107, RH8202, RH8204, RH8301, RH8304, RH8305, RH8307, RH8309) to their state on `main` (f3d514d). Reason given: rule pages keep accumulating notes that make them harder to read; `documentation/rules/general-notes.md` ("A diagnostic without a code fix") already covers diagnostics left without a fix. Specs under `sdd/specs/` are not part of this instruction.
