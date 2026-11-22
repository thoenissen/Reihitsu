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
| 1 | Skeptic | Spec clarification needed? | Yes: the spec author states in the line-break spec (or as an FMT-PIPE-008 example) that below C# 11 no line break is written inside an interpolation hole of a non-verbatim interpolated string, at any nesting depth; no change doc | Line-break requirements are unconditional today; FMT-PIPE-003 already demands compiling output, so this is a clarification | Unchanged FMT-PIPE-003/008 demand compiling output; no new visible choice | (spec author) |
| 2 | Skeptic | Issue goal "output equals input" | Replace by "no line break inside such a hole"; every other requirement (braces, spacing) still applies inside the hole. Tests assert exact output | Agent-filed goal; unchanged requirements decide the other edits | Unchanged requirements decide the output | — |
| 3 | Skeptic | Issue scope "line-break and blank-line phases" | Narrow to the line-break phase; the switch-case brace phase is checked by a fixture (skeptic c28) | Traces show no other phase writes breaks there | — | — |
| 4 | Skeptic | Architecture finding: RH5417's private `IsInsideSingleLineInterpolation` and the formatter fix would state the same rule twice | Structural change in this run: the rule is stated once in `Reihitsu.Core` (next to `StringInterpolationUtilities`), used by the formatter and by RH5417, whose behavior stays unchanged | Narrow fix would add a second statement of the rule; small, within Core which the formatter uses | RH5417 behavior unchanged (refactor) | (implementer) |
| 5 | Skeptic | Declare unsupported? | Not an option: depends on language version, which the precheck cannot read from tokens/trivia; deletes no special case | — | — | — |
| 6 | Test implementer | Multi-statement switch section inside a non-verbatim interpolation below C# 11 (FMT-SWITCH-001/015 vs FMT-BREAK-044) | Rare input class: the switch-case brace phase leaves such a switch as written (no braces inserted) below C# 11; the test pins the exact output | AGENTS.md Code-fix coverage — leaving a construct as written for a rare class; inserting braces on their own lines would be CS8967 | Leaving a construct as written for a rare class puts nothing in question; PR names the class under Review notes | (test implementer, implementer) |

## Follow-up candidates

1. Seven layout rules (RH5301, RH5303, RH5206, RH5103, RH5008, RH5513, RH5104) report inside non-verbatim interpolation holes below C# 11 a line break or blank line that CS8967 forbids (SHARED-DIAG-011); RH5301/RH5303/RH5206 fixes go through the formatter — one analyzer follow-up (01-skeptic.md).
2. Tooling: `scripts/apply-fix.sh` and `scripts/trace.sh` cannot take a language version (01-skeptic.md).

## State

- Last step: test implementer amendment (04-test-implementer.md).
- Next: implementer + spec author (switch-case brace spec states decision 6) in parallel.
