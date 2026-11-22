# Lead — interpolation-hole-breaks

- Issue: thoenissen/Reihitsu#1566 — formatter breaks an anonymous method in a hole of a regular interpolated string below C# 11 (CS8967). Label `agent-filed`: goals are proposals.
- Slug: `interpolation-hole-breaks`
- Branch: `claude/squad-1566-ocl1p5` (session-designated); draft PR thoenissen/Reihitsu#1945
- Lane: bug fix — the issue reports observed output (does not compile under C# 10) against expected output (formatter output keeps compiling; FMT-PIPE-008 language version).
- Roles: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author only for a clarification, skipped unless a role needs one.
- Maintainer context (issue comments, summarized): reproduces on `main` with `--lang-version 10`; triaged as maintenance because the formatter is used only from C# 12 today, so C# 11-dependent output is not critical now.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: claim, lane claimed.
- Next: skeptic.
