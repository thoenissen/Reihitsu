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
| 7 | Implementer | Four tests expect a space after a `{` the structural phase inserts inside a C# 10 hole (if, local function ×3) | Correct the expected outputs to what unchanged FMT-STRUCT-026, FMT-SPACE-009 and FMT-SPACE-002 give | Rare input class; no new spacing rule for it; output compiles | Unchanged requirements decide the output | (test implementer) |
| 8 | Implementer | `UsingStatementInInterpolationGetsBracesOnItsLineInCSharp10` expects braces on a using body | Correct: expected output is the input unchanged; no requirement braces a using body | Test was wrong | Unchanged FMT-STRUCT-001 decides | (test implementer) |
| 9 | Implementer | Scope: blank-line phase and lambda alignment also changed, new internal type | Accept; widen decision 3 to the blank-line phase; spec author names the blank-line phase where its spec needs it (FMT-BREAK-044 already says "the formatter") | Needed for the fix; new type is the issue's own fix, not escalation 9 | Output decided by FMT-BREAK-044/FMT-PIPE-003 | (spec author) |
| 10 | Implementer | `FormatNodeInDocumentAsync` target itself inside a `$"` hole still breaks | Not in this run; added to follow-up candidate 1 (reached only via RH5301/RH5303/RH5206 fixes) | Belongs with the analyzer follow-up | — | — |
| 11 | Spec author | List `SingleLineInterpolationUtilities` as a code owner of line-breaks.md? | No for now; both phases that call it are already owners; spec checker may say otherwise | Avoid a second owner entry for a helper | Internal | — |

## Follow-up candidates

1. Seven layout rules (RH5301, RH5303, RH5206, RH5103, RH5008, RH5513, RH5104) report inside non-verbatim interpolation holes below C# 11 a line break or blank line that CS8967 forbids (SHARED-DIAG-011); RH5301/RH5303/RH5206 fixes go through the formatter — one analyzer follow-up (01-skeptic.md). Also: `FormatNodeInDocumentAsync` on a target that itself sits inside a `$"` hole still breaks the hole below C# 11, since the enclosing string is out of view (05-implementer.md).
2. Tooling: `scripts/apply-fix.sh` and `scripts/trace.sh` cannot take a language version (01-skeptic.md).

## State

- Last step: test implementer amendment (04-test-implementer.md).
- Spec author done (06-spec-author.md, FMT-SWITCH-017). Open: test needs tag FMT-SWITCH-017 (test implementer, after the implementer).
- Implementer done (05-implementer.md, ef49b5e): 5 disputed tests.
- Test implementer done (07-test-implementer.md): formatter + architecture green.
- Cleaner done (09-cleaner.md, clean).
- Next: spec checker → auditor → validator.
