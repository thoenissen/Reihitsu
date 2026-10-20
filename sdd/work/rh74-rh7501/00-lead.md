Role: lead
Commit: 8ed8a456be20d9318156beea05a5123eabf04483
Verdict: lane claimed

# Run

- Issue: thoenissen/Reihitsu#995 (RH7401–RH7412, RH7501). Draft PR: thoenissen/Reihitsu#1302. Parent #899, question tracker #912.
- Slug: rh74-rh7501. Branch: claude/affectionate-cori-g925ow (session-designated).
- Lane: specs and tests for existing behavior (one spec per existing rule plus covering tests; no production change). Roles run: spec author, skeptic, test implementer, cleaner, spec checker, validator. Skipped: implementer, auditor (per the lane table).

# Maintainer clarifications (summarized)

- Shared spec: `sdd/specs/shared/member-region-grouping.md`, prefix `SHARED-REGGROUP`, only if more than one spec relies on the behavior. Reference FMT-REGION and SHARED-TYPEREGION IDs only under `Uses`.
- Do not claim `RegionDirectiveUtilities`, `OrderingDeclarationUtilities`, `RegionCategoryUtilities`; do not edit `OrderingDeclarationUtilitiesTests.cs` / `RegionDirectiveUtilitiesTests.cs`. `OverrideMemberUtilities` and `InterfaceImplementationUtilities` may be claimed (used only by RH7401–RH7412).
- Read also issues #1264, #1240, #1260, #1263, #1262 and the type-region shared specs. #1264 (P17) and #1240 (inactive #if covers region directives) are plain instances for the RH74xx bases: the lead comments one line on each; they are not new questions.
- RH7501 relates to FMT-SWITCH-002 and formatter bugs #948–#950: reference, do not refile.
- Decided principles P1–P24 (#912) apply; a case a principle decides is filed in the spec's open-questions issue marked "Decided by principle Pn (#912)". Decided-but-unimplemented behavior stays out of the specs; tests pinning current behavior are characterization tests. New-rule candidates (P11) go in the handover, not into issues.
- Open questions: one "SDD open questions: <spec path>" issue per spec, sub-issue of #912; if #912 is at its sub-issue limit, say so in the issue and tell the maintainer. Create no other issues without approval.
- Must not: change production code or behavior, fix bugs, write open questions into specs, retire/move/renumber existing IDs, mint IDs reserved by #1260, #1138, #1091, #1136 (SHARED-DIAG-004…008, SHARED-DOC-017, SHARED-USING-007, edits to SHARED-TYPEREGION-002/006), put sdd/work/ into the PR.
- Parallel squads (#985 RH8301–RH8501, #993 RH7001–RH7110) own their files; text conflicts in `sdd/specs/requirement-ids.txt` and `sdd/specs/README.md` are resolved by keeping both sides.

# Order

spec author → skeptic → test implementer → cleaner → spec checker → validator. Handovers: `01-spec-author.md`, `02-skeptic.md`, …
