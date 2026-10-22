# Round 9 created issues and mapping (2026-10-22)

Milestone SDD (9), no labels. Open-issue titles checked (list_issues, 3 pages, 214 issues) before creating; reused #1147, #1276, #958, #895 (same cause), new issues otherwise.
#912 summary: https://github.com/thoenissen/Reihitsu/issues/912#issuecomment-6350039592

## New issues (23)

- #1380 SDD: record principles P32–P37 and the P12 clarification (docs/spec). Follow-up list: https://github.com/thoenissen/Reihitsu/issues/1380#issuecomment-6350003473
- #1381 P32 formatter keeps comment layout when inserting braces — #1368 SW-Q2, #1371 Q-S2. Blocked by #1380. Behavior change.
- #1382 P33 token behind multi-line comment tail; SHARED-INIT-001 — #1362 Q4, #1373 LB-Q2. Blocked by #1380. Behavior change.
- #1383 P34 indexer lists, every assignment op, comment before `]`, attribute-target colon — #1373 LB-Q5, LB-Q9, #1370 Q-I5, #1369 SP-Q2 colon. Blocked by #1380. Behavior change. (LB-Q8 -> #958)
- #1384 P36 documented field declaration not split — #1371 Q-S5. Blocked by #1380. Behavior change.
- #1385 P37 blank line after directives (formatter, RH5002/RH5008 fixes) — #1372 Q-B3, Q-B4. Blocked by #1380. Behavior change.
- #1386 RH5033–RH5049 `///`/`/** */` documenting nothing (P13) — #1362 Q1. Bug.
- #1387 layout rules RH5102, RH5109, RH5111, RH5501–RH5531 conditional directive (P22) — #1375, #1376, #1378, #1379. Bug.
- #1388 RH5405 fix + `FormatNode` keep document EOL (P24) — #1363 Q5, #1366 Q-P1. Bug. Links #1283.
- #1389 formatter BOM + doc text behind code lost — #1364, #1367. Bug (spec).
- #1390 formatter async alias Task, expression-body comments, `with` idempotency — #1371 Q-S1, Q-S3, #1366 Q-P2. Bug (spec).
- #1391 indentation base list / empty list `)` / omitted type arg + lone-CR raw strings — #1370 Q-I3 (item 28), Q-I4, Q-I8, #1365. Bug (spec).
- #1392 FMT-BLANK-008 blank after comment before else — #1372 Q-B1. Bug (spec).
- #1393 RH5104 / RH5417 fixes — items 9, 11 (#1363 Q1, Q3). Bug.
- #1394 shared-spec clarifications — items 8, 10, 12 (#1362 Q3, #1363 Q2, Q4). Docs/spec.
- #1395 field-split comment before later declarator — item 13 (#1371 Q-S4). Behavior change.
- #1396 RH5030 exempts following block statement — item 14 (#1372 Q-B2). Behavior change.
- #1397 line breaks — items 15–18 (#1373 LB-Q3, Q4, Q6, Q7). Behavior change (16 = bug).
- #1398 break-only switch section — item 19 (#1368 SW-Q1). Behavior change.
- #1399 indentation region column + nested array rows — items 20, 21 (#1370 Q-I1, Q-I2). Behavior change.
- #1400 formatter-spec clarifications — items 22–24 (#1370 Q-I6, Q-I7, #1369 SP-Q1). Docs/spec.
- #1401 docs: sdd/README.md tests section + formatter non-goals — item 29.
- #1402 tooling: AGENTS.md P11 pointer — item 30.

## Extended / commented existing issues

- #1147 retitled "RH5110, RH5203, RH5205, RH5206, RH5301–RH5305, RH5308: a line led by a comment is not reported (P12)"; #1362 Q5–Q7 + P12 clarification test note: https://github.com/thoenissen/Reihitsu/issues/1147#issuecomment-6349993400
- #1276 item 27 formatter extension-block braces: https://github.com/thoenissen/Reihitsu/issues/1276#issuecomment-6349994657
- #895 LB-Q10: https://github.com/thoenissen/Reihitsu/issues/895#issuecomment-6349995767
- #958 LB-Q8 (P34 arrows) + SP-Q2 pairs (item 25): https://github.com/thoenissen/Reihitsu/issues/958#issuecomment-6349997387
- #1377 P35 answers SHARED-ATTR-008/-017; milestone SDD set: https://github.com/thoenissen/Reihitsu/issues/1377#issuecomment-6349998510
- #1331 SHARED-FIX-003 "for every input" + P11 closing sentence (item 29): https://github.com/thoenissen/Reihitsu/issues/1331#issuecomment-6349999789
- #963 P32/P36 link: https://github.com/thoenissen/Reihitsu/issues/963#issuecomment-6350000915

## Question → tracking

| Question | Tracked in |
|---|---|
| #1362 Q1 / Q2 / Q3 / Q4 / Q5–Q7 | #1386 / #1380 + #1147 note / #1394 / #1380 + #1382 / #1147 |
| #1363 Q1, Q3 / Q2, Q4 / Q5 / Q6 | #1393 / #1394 / #1388 / #1380 (P35) + #1377 |
| #1364 Q-C1, #1367 Q-D1 | #1389 |
| #1365 Q-R1 | #1391 |
| #1366 Q-P1 / Q-P2 | #1388 / #1390 |
| #1368 SW-Q1 / SW-Q2 | #1398 / #1381 |
| #1369 SP-Q1 / SP-Q2 | #1400 / #958 + #1383 (colon) |
| #1370 Q-I1, Q-I2 / Q-I3, Q-I4, Q-I8 / Q-I5 / Q-I6, Q-I7 | #1399 / #1391 / #1383 / #1400 |
| #1371 Q-S1, Q-S3 / Q-S2 / Q-S4 / Q-S5 | #1390 / #1381 / #1395 / #1384 |
| #1372 Q-B1 / Q-B2 / Q-B3, Q-B4 | #1392 / #1396 / #1385 |
| #1373 LB-Q1 / Q2 / Q3, Q4, Q6, Q7 / Q5, Q9 / Q8 / Q10 | #1276 / #1382 / #1397 / #1383 / #958 / #895 |
| #1374 Q1 | none (closed, no change: narrowed SHARED-DIAG-008 + P31) |
| #1375, #1376, #1378, #1379 | #1387 |

## Question issues

Commented and closed (completed): #1362–#1376, #1378, #1379 (17). All round-9 question issues closed.
Milestone SDD: #1377 (only issue of #1362–#1379 that stays open) and all 23 new issues.
