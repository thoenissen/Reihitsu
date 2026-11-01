# blank-line-sole-owner

| | |
|---|---|
| Date | 2026-11-01 |
| Run | issue #1496 |
| Pull request | #1541 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Block statement after `break;` gets a blank line | A | yes |
| 2 | output question | RH5025 exempts only a block statement that follows another statement | A | yes |
| 3 | output question | Own-line comment, blank line, block statement: blank line above `{` kept | A | yes |
| 4 | output question | Own-line comment in front of a glued comment gets the blank line | A | yes |
| 5 | output question | No blank line inserted above a directive in front of a glued comment | A | yes |
| 6 | output question | Block statement after `}` behind a directive: blank line after the directive | A | yes |
| 7 | output question | Comment glued to a block statement's brace after `}` gets the blank line | A | yes |
| 8 | output question | Directive, blank line, `#endregion` before `}`: no blank line above the directive | A | yes |
| 9 | output question | The same before a collection expression's `]` and a pattern's `)` | A | yes |
| 10 | output question | Comment glued to a complex element initializer's `{`: every complex element alike | A | yes |
| 11 | output question | Own-line comment and blank line above an assigned collection initializer's `{` | A | yes, plus guidance |

With answers 10 and 11 the maintainer gave a general guidance, then clarified it: comments that share a line with code and comments around `=` are unwanted; the formatter must not grow complexity to lay them out; analyzer rules should report them.

## Cost

- Stops: four output-question batches (7, 1, 1, 2 questions) and one clarification of the guidance.
- Repair rounds: three (spec checker; audit 1; audit 2), plus two structural changes (audit 1 and audit 2 architecture findings). The run used every round.
- Roles spawned more than once: spec author nine times, test implementer seven, implementer three, cleaner four, spec checker three, auditor three, skeptic and validator once.
- Most expensive: the blank-line rules for comments glued to a token. The first spec preserved today's output token by token (FMT-BLANK-018's token list); each audit found a comment shape the token list or the comment-position predicates missed. Only after the maintainer's guidance (no formatter complexity for unwanted comments) did one uniform classification replace the list.

## Decisions taken

39 rows. The ones the maintainer may want to see: row 22 (entry 18 counted as covered by answer 1A), row 24 (a comment after `=` gets the normal FMT-BLANK-011 blank line), row 35 (chain link behind a multi-line `*/` counts as own-line, through one shared predicate), and row 38 (a comment-only fix accepted on the plain text-only proof because `--strict-docs` treats public members of an internal type as public API).

## Suggestions

- When an issue says "keep today's output" for a class of inputs the repository considers unwanted (here: comments glued to code), ask the maintainer early whether today's output is worth preserving. The token-by-token preservation cost two repair rounds and was discarded after the guidance.
- The spec author's first scan of existing tests should cover code-fix tests too; two disputed expectations (RH5204, RH5201) were found only by the implementer.
