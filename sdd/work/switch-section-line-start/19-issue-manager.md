Role: issue manager
Commit: ef60c20
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| 1 `case 1: ;` becomes `case 1:;` | created (formatter feature form, output question) | thoenissen/Reihitsu#1559 | enhancement, formatter, agent-filed | none |
| 2 RH5103 and a statement of another section on the same line | commented (same construct as the open RH5103 issue; behavior change of RH5103) | thoenissen/Reihitsu#1156 | unchanged | none |
| 3 new rule: first statement on its label's line, labels sharing a line | created (analyzer feature form; P11 new-rule candidate) | thoenissen/Reihitsu#1560 | enhancement, analyzer, agent-filed | none |
| 4 `/** */` between label and statement | commented | thoenissen/Reihitsu#1533 | unchanged | none |
| 5 FMT-BLANK-004 sentence to revert | commented (Done-when extension to switch sections) | thoenissen/Reihitsu#1498 | unchanged | existing (#1482) |
| 6 braced-section doubling class | commented (three shapes, narrow fix, structural Done-when) | thoenissen/Reihitsu#1518 | unchanged | none |
| 7 `PlaceTokenOnOwnLine` counts a line break inside a multi-line comment | commented (statement-level instance of the open P33 issue) | thoenissen/Reihitsu#1382 | unchanged | none |
| 8 RH5204 reports formatter output `/** a⏎ b */ case 2:`, fix makes no progress | created (spec bug form, bug-fix lane) | thoenissen/Reihitsu#1561 | bug, analyzer, agent-filed | none |
| 9 RH5020 reports `A();⏎/* own line */ break;`, formatter keeps it | created (formatter bug form as output question A/B) | thoenissen/Reihitsu#1562 | bug, analyzer, formatter, agent-filed | none |
| 10 FMT-BREAK-008 silent on comment in front of a section's block body | created (clarification form) | thoenissen/Reihitsu#1563 | maintenance, formatter, agent-filed | none |
| 11 FMT-BREAK-033 depends on `DocumentationMode` | created (clarification form) | thoenissen/Reihitsu#1564 | maintenance, formatter, agent-filed | none |

No sub-issue links were added: none of the new issues belongs to the formatter-restructuring parent (#1482) or to a parent the candidates named. No candidate was dropped for reachability; every case is reached by whole-document formatting or by an analyzer on formatter output.
