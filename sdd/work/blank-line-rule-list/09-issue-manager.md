Role: issue manager
Commit: b8c9d31
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| 1. Documentation comment directly above a region directive (RH8302 against RH5031, FMT-BLANK-010 against FMT-BLANK-013) | commented: the open `agent-filed` issue already decides it (RH8302 does not report a blank line followed by `#region`/`#endregion`, option a, the skeptic's recommended A); the comment adds the formatter side (two rewriters decide the gap by order) and the input | thoenissen/Reihitsu#1348 | bug, analyzer, agent-filed (unchanged) | none |
| 2. SHARED-BLANK-006 stated twice (Core and `BlankLineRegionDirectiveRewriter`) | created | thoenissen/Reihitsu#1574 | agent-filed, maintenance, formatter | thoenissen/Reihitsu#1482 |
| 3. Region directive columns in initializers, switch expressions, and `#endregion` in a switch section | commented: the open `agent-filed` issue covers the initializer, collection-expression, switch-expression and argument-list part; the comment adds the switch-section `#endregion` shape, whose coverage by FMT-INDENT-010 is unchecked | thoenissen/Reihitsu#1399 | enhancement, formatter, agent-filed (unchanged) | none |

No candidate was dropped for reachability: all three are reached through the CLI or whole-document formatting. No closed duplicate was found. The issue search tool was unavailable, so the search was by title over the newest 100 issues plus all open and closed `formatter` and `analyzer` labelled issues; an older issue without those labels could have been missed.

Created or extended: thoenissen/Reihitsu#1574 (created), thoenissen/Reihitsu#1348 and thoenissen/Reihitsu#1399 (commented).
