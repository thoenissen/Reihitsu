# Phase 5 — bookkeeping round P5-2

Source: `state.md` (Round P5-2 DECIDED), `phase5-runs.md`, `phase5-round2-draft.md`, `phase5-splits.md`. All writes through the GitHub MCP tools on 2026-10-24. Every comment and the new issue body end with the Claude Code footer.

Milestone numbers: 1.2.0 = 5, vNext = 2, Maintenance = 4, SDD = 9, Triage = 10, 1.3.0 = 11, **1.2.1 = 12**, **1.2.2 = 13**. The MCP tools cannot list milestones, so 1.2.1 and 1.2.2 were found by setting a number on #1384 and reading it back: #1384 was briefly on Triage (10) and 1.3.0 (11) before 1.2.1 (12), so its timeline shows those two extra milestone events.

## Task 1 — milestones and labels

- Done, milestone 1.2.1 (43): R10 #1384 #1395 #963 #1381 #1398; R11 #1392 #1385 #952 #1391 #1239; R12 #1382 #1383 #958 #1397 #1243 #1093 #1149; R13 #1140 #1144 #1145 #1393; R15 #972 #1096 #1076 #1416; R16 #1150 #1274 #1015 #1143; R17 #1094 #1141 #1142 #1388; R18 #969 #1335 #1263 #1151; R21 #1269 #1342 #1343; R23 #1283 #1284 #1285.
- Done, milestone 1.2.2 (29): R14 #1147 #1152 #1386 #1387; R19 #1264 #1333 #1334 #1339 #1337 #1412; R20 #966 #1105 #1266 #1267; R22 #1345 #1346 #1347 #1348 #1270 #1092 #1153; R24 #1071 #1268 #1084 #1289 #974; R25 #1074 #1079 #1276.
- Labels: read from the open-issue list before the edits. All of the above already had `bug` and no `enhancement` (#958 included), except #1149 (`enhancement`, `formatter`) → set to `bug`, `formatter`. No other label changes.
- R1–R8 milestones not touched.
- Errors: none.

## Task 2 — #1382 split merged back

- Split issue **#1438** ("RH5204 and initializer rules report a token behind a multi-line comment tail") commented and closed as not planned.
- #1382 commented (RH5204 / SHARED-INIT-001 parts back as bugs).

## Task 3 — decision comments (all "Triage P5-2:")

- G1 (16): #1393 (J1), #1146 (J1), #1140 (J2), #1398 (J3), #1074 and #1133 (J4), #1416 and #1261 (J5), #1388 (J6), #1387 and #1337 (J8), #1266 (J9), #1334 (J11 + Q6 / RH7401-001…RH7412-001), #1262 (J12), #1345 (J13), #1417 (J14). The "change doc required / behavior-change lane no longer applies" sentence was added to the 13 issues the draft (G1-A consequence) names as carrying that line: #1393 #1398 #1416 #1334 #1387 #1337 #1266 #1262 #1345 #1417 #1388 #1074 #1133; not to #1146 #1140 #1261. Bodies were not re-read.
- G2 (8): #1284 (J7, SHARED-BODY-003 block body only), #1333 (J10), #1268 and #1071 (J15), #1147 (J16), #1092 and #1287 (J17), #1385 (J20 formatter part).
- Individual: #888, #957 (refers to #1453), #1136, #1149, #966, #1242.
- Errors: none.

## Task 4 — new issue

- Created **#1453** "RH6001 spec: list `fixed` among the reported keywords (clarification)", labels `maintenance`, `analyzer`, milestone Maintenance (verified).

## Task 5

- #1146 milestone left on 1.2.0 (not touched).
- #1074 → 1.2.2, label `bug` (already present; `enhancement` absent); its comment states the whole issue is a bug.
