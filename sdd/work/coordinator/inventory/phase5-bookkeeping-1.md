# Phase 5 — bookkeeping round 1 (2026-10-24)

Source: `phase5-consolidated.md` (buckets A–H), labels/milestones as read from GitHub before the edits. All writes through the GitHub MCP tools. Milestone numbers: 1.2.0 = 5, vNext = 2, Maintenance = 4 (1.4.0 = 6; 1.5.0 exists).

Drift found before editing: the former `1.3.0` issues (#849–#851, #895, #896) and #892–#894, #897 are now on `1.4.0`; the former `1.4.0` issues (#352–#354) are on `1.5.0`. `1.3.0` has no open issues. All of them are bucket D and were treated as "release-planned, keep milestone".

## Task 1 — critical → 1.2.0, `bug`
- Done (31): bucket A #948 #949 #954 #1329 #1389 #1390 #951 #1146 #1075 #1133 #1261 #1134 #1135 #1262 #1332 #1336 #1241 #1344 #967 #1070 #1073 #1286 #1287 #1288 #1257 #1279 #1281 #1282 (28; #1133 and #1135 were already on 1.2.0, label only) plus #1340 #1417 #1422.
- Unchanged and kept on 1.2.0: #946 #888 #1016. #1384 not moved. Verified afterwards: 34 open issues on 1.2.0.
- Errors: none.

## Task 2 — bucket D → vNext, `enhancement`, `bug` removed
- Done (83 labelled `enhancement`): 71 moved to / kept on vNext.
- Kept their milestone (12): #849 #850 #851 #892 #893 #894 #895 #896 #897 (1.4.0), #352 #353 #354 (1.5.0, the renamed former 1.4.0).
- `bug` removed and comment posted (10): #960 #961 #962 #955 #957 #968 #970 #971 #973 #975.
- Errors: none.

## Task 3 — buckets E and F → Maintenance, `maintenance`
- Milestone Maintenance (24): E #1400 #1131 #1291 #1394 #1081 #1136 #1240; F #950 #889 #1244 #1330 #862 #1090 #1032 #867 #877 #898 #1054 #1259 #1427 #1428 #1053 #1132 #1242 (#950 #889 #898 #1054 #877 #862 #867 #1032 #1053 #1132 were already there).
- Skipped: label `maintenance` — it does not exist in the repository, and no available MCP tool can create labels. Consequence for task 5: these 24 issues carry no `maintenance` label. Ten of them keep their old `enhancement` label (#862 #867 #877 #1032 #1053 #1054 #1132 #1242 #1244 #1330); I did not remove it, so that they are not left without any type label. Once `maintenance` exists, swap `enhancement` → `maintenance` on these ten and add it on the other fourteen.

## Task 4 — buckets B, C, H → `bug`
- `bug` on all 78 (B 46, C 18, H 14); no milestone change apart from task 1.
- C comments posted (18): #1381 #1384 #1395 #1398 #1382 #1383 #1416 #1264 #1334 #1337 #1339 #1340 #1345 #1346 #1347 #1268 #1417 #1422.
- Titles: "(behavior change)" removed from #1340, #1395 (now "… (FMT-STRUCT-022)"), #1398 (now "… (FMT-SWITCH-006 over -004)"). No other C title contained it.
- #958 (H) had `enhancement`; removed so that it carries exactly one type label (`bug`).
- Errors: none.

## Task 5 — area labels
- Applied to all 213 issues that stay open (164 issues got a label change in total, including type labels). `analyzer` for rules/fixes, `formatter` for formatter, both for: #862 #888 #965 #966 #1077 #1081 #1083 #1093 #1095 #1098 #1267 #1291 #1354 #1382 #1385 #1388.
- Neither (tooling/squad/SDD process/Playground): #898 #1054 #1427 #1428 #976.
- Existing labels kept even where they look off: #122 and #975 keep `formatter` (VS extension / CLI).
- Type label: every open issue has exactly one of `bug`/`enhancement`, except the 24 E/F issues (see task 3).

## Task 6 — #1034
- Verified: `AttributeTargetUtilities` is listed as code owner in `sdd/specs/shared/attribute-targets.md:7` and `sdd/specs/README.md:20`. Commented and closed (completed).

## Task 7 — #1055
- Covering tests found (tagged `[Requirement("SHARED-DOC-002")]`, extension blocks): `RH8021PrivateMethodsMustBeDocumentedAnalyzerTests.VerifySpecificationExamplePrivateGroupInExtensionBlock`, `RH8020NonPrivateMethodsMustBeDocumentedAnalyzerTests.VerifyNoDiagnosticForImplicitlyPrivateExtensionMemberMethod`, `RH8022NonPrivatePropertiesMustBeDocumentedAnalyzerTests.VerifyGroupOfExtensionBlockProperties`. Commented and closed (completed).

## Task 8 — trim comments
- Posted (7), each claim matches section G (and E/A for #1081/#1281) of the consolidated file: #1281 #1077 #1081 #1076 #1132 #1288 #953.

## Notes
- vNext shows 93 open issues after the edits, but only 72 come from this inventory (71 D + #1239). The rest are issues outside the 215-issue index (e.g. #1452, opened after the index); untouched.
