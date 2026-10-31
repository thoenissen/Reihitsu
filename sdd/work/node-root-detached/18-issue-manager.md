Role: issue manager
Commit: 76dd134f
Verdict: filed

Checked on the branch with `scripts/apply-fix.sh` (RH6023, RH5111) and `scripts/format.sh`: after maintainer answer 8 the fix still keeps `.C` in column 10 (`a?.B⏎  .C = 5;`), whole-document formatting gives column 9 and joins `a?[0].C = 5;`. Reachable through the RH5111 and RH6023 code fixes. No duplicate found; the open issue about RH6023/RH6024 rewriting only the operator gaps is related (it would make the RH6023 half moot) but is a different concern. #1521 and #1527 were not touched.

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| RH6023/RH5111 fix output differs from whole-document formatting for an assignment inside a conditional access with a wrapped member | created, as an output question with options A (recommended), B, C | #1529 | agent-filed, enhancement, analyzer, formatter | none |
