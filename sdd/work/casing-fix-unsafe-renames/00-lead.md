Role: lead
Commit: 61718e2
Verdict: lane claimed — bug fix

# Run: casing-fix-unsafe-renames

Slug: `casing-fix-unsafe-renames`. Branch: `claude/squad-1070-1073-pb445m` (the session's designated branch; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1596.

## Issues

| Issue | `agent-filed` | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1070 | yes — goals are proposals | Bug fix | After the triage comment on the issue, its scope is fix safety only: the casing code fixes are to be withheld where the rename breaks the build or the run-time binding — an override or implementation of a member declared in a referenced assembly (rename gives CS0115 and similar; also the tuple element names such an override must repeat), and an `extern` method bound to a native entry point by its own name (`DllImport`/`LibraryImport` without `EntryPoint`; rename gives EntryPointNotFoundException at run time). The no-report part (new SHARED-CASING-011) moved to thoenissen/Reihitsu#1434 and is NOT part of this run: the diagnostics stay. Observed-versus-expected difference against the existing SHARED-FIX-003. |
| thoenissen/Reihitsu#1073 | yes — goals are proposals | Bug fix | Casing fixes are to be withheld (diagnostic kept) for: a type declared directly in a namespace whose new name collides with one of its members (CS0542/CS0694 — the compile-error check of SHARED-CASING-009 reportedly skips namespace-level types); a member referenced from inactive `#if` code (only active references are renamed); a record whose positional parameter and its explicitly declared member would be split by renaming one side. Observed-versus-expected difference against SHARED-FIX-003 (added by thoenissen/Reihitsu#1068, now closed). |

Instances named by the issues (each a question in the referenced source-audit issue): thoenissen/Reihitsu#1013 Q1/Q2 (RH4103), #1014 Q1 (RH4102), #1023 Q1 (RH4116), #1025 Q1 (RH4114), #1026 Q1 (RH4112), #1027 Q1 (RH4113), #1009 Q1/Q4/Q5 (shared casing spec).

## Lane

Bug fix. Roles run: skeptic (premise check, including reachability), test implementer (reproduction first), implementer, cleaner, spec checker, auditor, validator. Spec author: only if a clarification is needed — skipped unless a role reports a spec gap.

## Maintainer clarifications

None beyond the issue texts. The triage comment on #1070 (by the maintainer's account) narrows #1070 to the fix-safety part.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-1070-1073-pb445m` instead of `claude/issues-1070-1073-<slug>` | The session environment mandates that branch for every push | Branch names are internal | claim commit |

## Follow-up candidates

None yet.
