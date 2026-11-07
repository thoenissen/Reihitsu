# spacing-sole-gap-owner

| | |
|---|---|
| Date | 2026-11-01 |
| Run | issue #1493 |
| Pull request | #1548 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Join separator: none unless the two tokens would read as one | B | yes |
| 2 | output question | One space around every `=>` | B | yes |
| 3 | output question | Attribute-target colon: no space before, one behind, every target | B | yes |
| 4 | output question | Conditional `? :` spaced | B | yes |
| 5 | output question | Name colons of arguments, tuple elements, subpatterns | B | yes |
| 6 | output question | One space in front of every `when`, catch filter included | C | yes |
| 7 | output question | One space between a type and every declared name | C | yes |
| 8 | output question | One-line accessor list `{ get; set; }`, attributed lists too | B | yes |
| 9 | output question | Relational pattern `is > 5` | B | yes |
| 10 | output question | Space behind a prefix keyword in front of its operand | B | yes |
| 11 | output question | Space on both sides of `with` | B | yes |
| 12 | output question | Space between an explicit lambda return type and its parameters | B | yes |
| 13 | output question | `delegate* unmanaged` | B | yes |
| 14 | output question | Keyword positions found by the spec author's sweep after batch 1 (`return ref (x);`, lambda tuple return types, function-pointer `ref`) | A | yes |
| 15 | output question | Gap behind a right-carried `/** */` follows the pair's rule | decided on recommendation under the maintainer's standing instruction | yes |

After question 14 the maintainer gave a standing instruction: decide any further output question by the recommendation and report it at the end.

## Cost

- Stops: two (batch of 13 before the test implementer; question 14 before the implementer).
- Repair rounds: three of three. Round 1: change-doc wording (spec checker). Round 2: `DeclaredNameSpacingRule` covered fields and `for`/`fixed`/`using` declarations that spec and comment did not name (auditor). Round 3: the round-2 spec example put a fixed-size buffer in a class (auditor on the repair).
- Roles spawned more than once: spec author seven times (amendment after the skeptic, answers, two correction rounds between test writing and implementation, three repairs), test implementer five, spec checker three, auditor three, cleaner two.
- Most expensive: the gap between the first spec draft and a complete inventory. The skeptic found a dozen pairs the first draft missed; the test implementer and the implementer each still found spec gaps (`ref` positions, FMT-SPACE-014's insertion direction, 17 stale phase-level tests). All were settled before the audit; the first audit found only one medium finding.

## Decisions taken

27. Ones the maintainer may want to see: 5 (the run completes parts of #958, #1514, #1440, #1383, linked with `Refs`), 13 (`ref (int, int)` of locals/fields/return types left as written, question filed as #1550), 20 (bundled fix: the gap behind a rewritten token was skipped in that pass, restoring idempotency of `M(a /** x */ ,b)`), 21 (a whitespace-run case behind a right-carried comment dropped under the maintainer's guidance on comments sharing a line with code).

## Suggestions

- When a rule is keyed by a Roslyn node type (`VariableDeclarationSyntax`, `ParameterSyntax`), the spec author could enumerate the node's parents from the Roslyn model when writing the requirement. Both audit repairs of this run came from an incomplete list of where one node type occurs.
- A spec author's "sweep" after the first batch of answers produced question 14; asking the spec author for that sweep before the first batch would have saved a stop.
