# Lead — analyzer-rule-bug-batch

Branch: `claude/peaceful-wright-5herkt` (the session's designated branch; see decision 1). Draft PR: thoenissen/Reihitsu#1719.
Base: origin/main f81cb5374ed3e0a3009f3b41d758d387b2acc274.

## Issues (prompt order)

All five carry `agent-filed`: their goals are proposals.

| Issue | Scope kept after triage (summarized from the issue and its comments) | Lane reason |
|---|---|---|
| thoenissen/Reihitsu#1074 | SHARED-DIAG-003: whitespace around literals and inside interpolation holes is code. RH5601: a tab before a literal (after a line break, after a `/** */` comment) is not reported. RH5602: trailing whitespace after a literal's end (plain, `$"…"`, verbatim) and inside an interpolation hole is not reported. RH6009/RH6010: initializer braces inside an interpolation hole are not checked. Triage comment: bug-fix lane, no change doc; RH5601-001 and RH5602-001 are aligned to SHARED-DIAG-003 as a clarification in the same PR. | Observed vs. expected against SHARED-DIAG-003 → bug fix |
| thoenissen/Reihitsu#1079 | After the triage split only the RH5603 part remains: for `class C⏎{⏎}⏎···⏎` the fix leaves `}⏎···`, which RH5603 reports again (SHARED-FIX-001). The fix must remove the whole whitespace-only tail in one application, as the formatter does (FMT-CLEAN-005). The RH5601 `///`, RH6004/RH7302 `# region`, RH6005, RH6021 instances moved to #1444 and are NOT in this run. | Fix does not converge → bug fix |
| thoenissen/Reihitsu#1276 | After the triage split only the bug part remains: RH5404 reports a one-line extension block with `Location.None`, and the fix formats a different type; it must be reported at the `extension` keyword (SHARED-DIAG-007) and the fix must not act on a different type (see decision 3). The P11 widening (braces on different lines) and formatter extension-block braces moved to #1446 and are NOT in this run. | SHARED-DIAG-007 violated → bug fix |
| thoenissen/Reihitsu#1613 | RH5202: analyzer and fix extract the closing line of a raw string differently; with lone `\r` line breaks (also U+2028/U+2029/U+0085) the analyzer reports an aligned literal and keeps reporting after the fix (RH5202-003, SHARED-FIX-008). | Observed vs. expected → bug fix |
| thoenissen/Reihitsu#1708 | RH4105: an unnamed extension receiver parameter (`extension(long)`, `extension<T>(T[])`) is reported without a location; it must not be reported (SHARED-DIAG-007). The RH4105 spec gains a sentence and example. | Observed vs. expected → bug fix |

## Lane

Bug fix (all five). Roles: spec author (clarifications: RH5601-001/RH5602-001 alignment to SHARED-DIAG-003 per #1074 triage; RH4105 extension-block sentence per #1708; any other gap a bug exposes) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/peaceful-wright-5herkt` instead of `claude/batch-1074-<slug>` | The environment only permits pushing to the designated branch | Branch name is internal | claim commit |
| 2 | Claim | Scope of #1079 and #1276 | Only the parts the triage comments kept (RH5603 for #1079; RH5404 extension-block location for #1276); the split-off parts belong to #1444/#1446 | Maintainer triage comments on the issues | Follows the issues' recorded split | — |
| 3 | Spec author | #1276: fix on an extension block — format it or withhold it | Withhold the fix for extension blocks (RH5404 spec as written in 7d38931) | The RH5404 rule page already says extension blocks are expanded by hand; formatting the block would leave its own diagnostic (formatter extension braces are #1446), violating SHARED-FIX-001 | Matches the published rule page; today's fix formats a different type, which is the bug | 7d38931 |
| 4 | Spec author | #1074: rewriting RH5601-001/RH5602-001 without a change doc | No change doc | Triage comment on #1074 decided bug-fix lane, clarification to SHARED-DIAG-003 | The behavior is demanded by SHARED-DIAG-003, an unchanged requirement | 7d38931 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: spec author (01, 7d38931). Next: skeptic.
