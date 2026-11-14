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
| 5 | Skeptic | #1613 architecture finding: analyzer and fix find the raw string's closing line in two ways | Change the structure in this run: one shared way of finding the closing line, used by the RH5202 analyzer and its fix | The two are counterparts the run changes anyway; a narrow fix in the analyzer alone would keep two statements of the rule | Brings the analyzer back to RH5202-001/-003 | — |
| 6 | Skeptic | #1074 RH6009/RH6010: RH5025 already has a private check for interpolation braces | Do not add a third copy; RH6009, RH6010 and RH5025 share one check | FC-015; small, within the run's components | Internal structure only | — |
| 7 | Skeptic | RH5404-003 `Why:` wording; RH5202 aligned-not-reported examples belong to -001 | Spec author amends (continued, once) | Skeptic finding, internal spec wording | Spec wording is internal | — |
| 8 | Skeptic | RH5603 not reporting a file ending in U+2028/U+2029/U+0085 | Dropped, not a follow-up | Rare input class (AGENTS.md, Code-fix coverage) | — | — |
| 9 | Skeptic | #1079: issue says RH5603 reports `}⏎···` again; it is RH5602 | Implement the defect that exists: the fix removes the whole whitespace-only tail; PR states the difference | AGENTS.md, Issue intake | Fix result is decided by the maintainer's answer on #1079 | — |
| 10 | Test implementer | RH5602-001 example does not compile; RH5404 spec misses SHARED-FIX-001 under Uses | Fresh spec author fixes both (04); test implementer then aligns the RH5602 test with the renamed example | Spec defects found by tests, internal | Spec text only | — |
| 11 | Test implementer | Formatter output for `M(/** c */→"abc");` | Dropped, not a follow-up | A comment inside an argument list is a rare input class (RH5038 reports it) | — | — |

## Follow-up candidates

1. RH5603 reports a file that ends in a line break after a directive line or a `///` comment; the formatter keeps such files with or without that line break (FC-013). Carry an output question: A not reported (recommended), B the formatter removes the line break. Source: 02-skeptic.md, #1079.
2. Formatter output for a document whose line breaks are all lone `\r` reportedly contains mixed line endings (confirmed by the test implementer: the RH5202 analyzer reports the formatter's output for such a document; recheck after the implementer whether it remains). Source: 02-skeptic.md, #1613.

## Worklist / next step

- Done: spec author (01, 6ab9935), skeptic (02), test implementer (03, e2d65ed; all five reproduced, 78 failing tests). Fresh spec author fixing two spec defects (04). Next: test implementer alignment of the RH5602 example test, then implementer.
