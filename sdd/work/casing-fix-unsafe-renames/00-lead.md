Role: lead
Commit: 61718e2
Verdict: lane claimed — bug fix; skeptic: #1070 proceed narrower, #1073 proceed

# Run: casing-fix-unsafe-renames

Slug: `casing-fix-unsafe-renames`. Branch: `claude/squad-1070-1073-pb445m` (the session's designated branch; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1596.

## Issues

| Issue | `agent-filed` | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1070 | yes — goals are proposals | Bug fix | After the triage comment on the issue, its scope is fix safety only: the casing code fixes are to be withheld where the rename breaks the build or the run-time binding — an override or implementation of a member declared in a referenced assembly (rename gives CS0115 and similar; also the tuple element names such an override must repeat), and an `extern` method bound to a native entry point by its own name (`DllImport`/`LibraryImport` without `EntryPoint`; rename gives EntryPointNotFoundException at run time). The no-report part (new SHARED-CASING-011) moved to thoenissen/Reihitsu#1434 and is NOT part of this run: the diagnostics stay. Observed-versus-expected difference against the existing SHARED-FIX-003. |
| thoenissen/Reihitsu#1073 | yes — goals are proposals | Bug fix | Casing fixes are to be withheld (diagnostic kept) for: a type declared directly in a namespace whose new name collides with one of its members (CS0542/CS0694 — the compile-error check of SHARED-CASING-009 reportedly skips namespace-level types); a member referenced from inactive `#if` code (only active references are renamed); a record whose positional parameter and its explicitly declared member would be split by renaming one side. Observed-versus-expected difference against SHARED-FIX-003 (added by thoenissen/Reihitsu#1068, now closed). |

Instances named by the issues (each a question in the referenced source-audit issue): thoenissen/Reihitsu#1013 Q1/Q2 (RH4103), #1014 Q1 (RH4102), #1023 Q1 (RH4116), #1025 Q1 (RH4114), #1026 Q1 (RH4112), #1027 Q1 (RH4113), #1009 Q1/Q4/Q5 (shared casing spec).

## Lane

Bug fix. Roles run: skeptic (premise check, including reachability), test implementer (reproduction first), implementer, cleaner, spec checker, auditor, validator. Spec author: runs for clarifications (skeptic named spec gaps; decisions 2, 3, 7, 9), after the skeptic. No change doc: no visible output change beyond what SHARED-FIX-003 / P1 already decide.

## Maintainer clarifications

None beyond the issue texts. The triage comment on #1070 (by the maintainer's account) narrows #1070 to the fix-safety part.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-1070-1073-pb445m` instead of `claude/issues-1070-1073-<slug>` | The session environment mandates that branch for every push | Branch names are internal | claim commit |
| 2 | Skeptic | DllImport/LibraryImport rename: literal SHARED-FIX-003 reading ("the new name does not count") would allow it | Withhold the fix; the spec author clarifies SHARED-FIX-003 so a binding to a native entry point by the method's own name (no `EntryPoint`, or `EntryPoint = nameof(self)`) is not "the new name" | Maintainer principle P1 (#912 principle-decision comment: a fix that would change what the program does at run time is not offered) decides it; the #1070 triage comment keeps it in scope | Covered by a maintainer decision (P1) | — |
| 3 | Skeptic | Architecture finding: SHARED-CASING-009 restates SHARED-FIX-003's compile-error clause narrower (containing declarations only; namespace-level types only for namespace collisions), and the code implements the narrow version | Change the structure in this run: the casing spec stops restating the compile-error rule with its own region and defers to SHARED-FIX-003 (no compile error the code did not have, anywhere the rename reaches, including the renamed type itself and other types of a rename cascade); the casing fix base implements that one statement. SHARED-CASING-007's coverage gap for namespace-level types goes away accordingly | A narrow patch per region would add a third partial statement of the rule; the change stays inside the casing fix base and its specs, which the run changes anyway | Withholding unsafe fixes is decided by the unchanged SHARED-FIX-003 / P1 | — |
| 4 | Skeptic | Region of the compile-error check (containing types of cascade members vs. every changed declaration) | Not prescribed by the spec beyond decision 3; the implementer picks the mechanism, tests must cover the cascade members the skeptic listed | Spec states the rule, not the mechanism | Internal | — |
| 5 | Skeptic | Excluded-code check precision | Withhold when the old or the new name occurs as an identifier in excluded text of any document where the symbol is visible (skeptic option 1) | Option 2 misses a public symbol referenced in another file's excluded branch; false withholds are the safe direction (AGENTS.md, Code-fix coverage) | SHARED-FIX-003 decides withholding; P1 | — |
| 6 | Skeptic | Fix All for a base/derived record pair whose single fixes are both withheld | Follow SHARED-FIX-002: Fix All renames neither | Unchanged requirement decides it | Decided by SHARED-FIX-002 | — |
| 7 | Skeptic | Q5 join direction (rename makes a positional parameter match an inherited member or a private property; synthesized public member disappears) | In scope as a member of #1073 Q5's class; fix withheld; RH4120's fix requirement narrowed by the spec author so it no longer demands such renames | Same mechanism; SHARED-FIX-003 applies to every fix and P1 decided #1009 Q5 | Decided by SHARED-FIX-003 / P1 | — |
| 8 | Skeptic | #1070 goals: no-report goals, SHARED-CASING-011 tag, RH4116 instance (#1023) | Drop the no-report goals (moved to #1434), tag tests SHARED-FIX-003 (and the amended casing requirement), drop RH4116 (no fix exists). Direct metadata overrides already withheld: characterization tests only. #1070 still `Closes` — its narrowed fix-safety purpose is met | `agent-filed` goals are proposals; triage comment narrowed the issue | Dropping removes a requested change; nothing new becomes visible | — |
| 9 | Skeptic | RH4104 (local functions) does not exclude `extern` | Fix-only clarification: the RH4104 fix is withheld for a local extern function bound by its own name; reporting unchanged (reporting is #1434's scope) | Same P1 decision as RH4103 | Decided by P1 | — |

## Follow-up candidates

- RH2001 fix renames only active references; a reference in `#if`-excluded code is left behind (same class as #1073 Q4, other component). Source: 01-skeptic.md (#1073, Follow-up candidates). Issue manager: check #1286 first.
