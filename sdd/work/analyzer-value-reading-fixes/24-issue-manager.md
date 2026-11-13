Role: issue manager
Commit: see the commit that adds this file
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| `sdd/README.md` says `shared/diagnostics.md` applies without `Uses`, but `SpecStructureTests` requires referenced IDs to be listed | created (internal, docs) | #1693 | agent-filed, maintenance, documentation | — |
| SHARED-DIAG-002 does not name Unicode escapes for every rule; Q1 written only into RH4001/RH4009 | created (clarification) | #1694 | agent-filed, maintenance, analyzer, documentation | — |
| RH4009 nested namespace declarations | not filed: RH4009-002 reports only a namespace declaration "declared at file level" and compares its name, so the spec already decides which declaration is compared; no open question remains | — | — | — |
| RH2109 (a): `@code {` line inside a multi-line C# comment or literal, any host | extended (decisions 13, 15; #1289 stays open, the PR links it with `Refs`) | #1289 | bug, analyzer, agent-filed (unchanged) | — |
| RH2109 (b): line-starting `@*`/`<!--` inside a multi-line C# literal/comment or script text hides a later directive | not filed: dropped by decision 15 (rare input class) | — | — | — |
| RH2109 (c): multi-line Razor/HTML comment whose opener is behind other text | not filed: dropped by decision 15 (rare input class) | — | — | — |

No duplicates found among open and closed issues (checked titles of the newest 200 issues and every `documentation`-labelled issue; #1452 is the `@functions` part and is unrelated).
