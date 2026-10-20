Role: Lead
Commit: 8ed8a45
Verdict: run started

# Run

- Issue: thoenissen/Reihitsu#993 (parent #899, question tracker #912)
- Slug: o1-ordering-specs
- Lane: Specs and tests for existing behavior (one spec per rule RH7001-RH7004, RH7101, RH7102, RH7104-RH7110, with covering tests).
- Roles run: spec author, skeptic, test implementer, cleaner, spec checker, validator. Skipped: implementer, auditor (lane table).

# Maintainer clarifications (summarized)

- Reference SHARED-ORDER-001..007 and the RH2005 spec under Uses; mint no SHARED-ORDER ID.
- May claim only ModifierOrderingUtilities and ModifierOrderingCodeFixProviderBase; must not claim the SHARED-ORDER owner types, DeclarationModifierUtilities, RegionDirectiveUtilities, OrderingDeclarationUtilities, nor edit OrderingDeclarationUtilitiesTests.cs / RegionDirectiveUtilitiesTests.cs.
- Read also: #968, #969, #970, #1134, #1136, #1137, #1143, #1150, #1138. Known instances (lead comments one line each): #968 (RH7110, RH7104), #969 (RH7102, RH7109), #970 (RH7102, RH7107, RH7108, RH7109), #1134 (RH7109), #1143 (RH7004, RH7102, RH7109: do not encode two Fix All passes as a requirement), #1150 (RH7004, RH710x), #1136/#1137 (RH7102/RH7109 follow RH7103).
- Principles P1-P24 of #912 decide cases; a new instance is filed in the spec's open-questions issue marked "Decided by principle Pn (#912)". Decided-but-unimplemented behavior stays out of specs; its test is a characterization test.
- Concurrent squads (#985, #995): do not touch their rules/specs; keep both sides in text conflicts of requirement-ids.txt and sdd/specs/README.md.
- Must not: change production code, fix bugs, write open questions into specs, renumber IDs, mint reserved IDs (SHARED-DIAG-004..008, SHARED-DOC-017, SHARED-USING-007, edits to SHARED-TYPEREGION-002/006, #1138, #1091, #1136 ids), commit sdd/work. Create no issues except "SDD open questions" without approval of exact text.
- Open questions: one issue per spec "SDD open questions: <spec path>", sub-issue of #912; if #912 is at its sub-issue limit, say so in the issue and tell the maintainer.
