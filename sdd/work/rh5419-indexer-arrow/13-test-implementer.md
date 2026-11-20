Role: test implementer
Commit: c47a62a (tests written on this head; the commit carrying them follows)
Verdict: tests written

Repair round 2: brings the RH5419 tests in line with the amended RH5419-004 (`12-spec-author.md`, decision 6 in `00-lead.md`, auditor finding #1). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

All in `src/Reihitsu.Analyzer.Test/Formatting/RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzerTests.cs`, each under LF and CRLF.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixIsWithheldForALineCommentBehindThis` (replaces `VerifyFixIsOfferedForALineCommentInFrontOfTheOpeningBracket`; auditor member a) | RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: a fix is offered |
| `VerifyFixIsWithheldForACommentOnALineOfItsOwnInFrontOfTheOpeningBracket` (replaces `VerifyFixIsOfferedForACommentInTheLeadingTriviaOfTheOpeningBracket`; member b) | RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: a fix is offered |
| `VerifyFixIsWithheldForADirectiveBetweenThisAndTheOpeningBracket` (new; member c) | RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: a fix is offered |
| `VerifyFixIsWithheldForALineCommentBehindThisInFrontOfAParameterListOnOneLine` (new; member d) | RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: a fix is offered |
| `VerifyFixIsWithheldForALineCommentBehindAModifier` (new; `public // c` + line break: the measured span starts at the first token behind the attribute lists, so a modifier counts) | RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: a fix is offered |
| `VerifySpecificationExampleOfWithheldFixes` (gains the spec's four indexers `sbyte`, `ushort`, `float`, `double`) | RH5419-001, RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: a fix is offered for the new indexers |
| `VerifySpecificationExampleOfBlockCommentsThatDoNotWithholdTheFix` (gains the documentation-comment indexer `short`, the "leading trivia is not measured" side) | RH5419-003, RH5419-004 | `Verify` (one pass, Fix All) | passes |

Boundary sides already covered and unchanged (FC-002, FC-003): a block comment on the line of `this` (`this /* c */ [sbyte a,⏎ sbyte b]`) and a bare line break between `this` and `[` (`this⏎ [ulong a,⏎ ulong b]`) keep the fix in `VerifyFixPutsIndexersOnOneLine` and the offered spec example, passing today; a documentation comment above a one-line list in `VerifyFixKeepsTheDocumentationCommentAboveTheIndexer`.

Every failing test fails on `No code fix may be offered for the diagnostic at [...]`, not on markup or compilation. RH5419 run: 80 passed, 12 failed (the 6 tests above times 2 line endings). Architecture tests: 198/198.

Existing tests asserting a changed output, `rg "this // \|this$"` over the test projects: only the two flipped tests and the spec-example tests above; the RH5419 formatter tests (`Formatter/Formatting/RH5419…FormatterTests.cs`) assert formatter output, which this round does not change, and stay as they are.

## Not covered, and why

- A line comment behind an attribute list (`[Obsolete] // c⏎int this[int a,⏎ int b] => a;`): RH5419-004 says the attribute lists are not measured, but it gives no example and no output for the fix, and RH5401-004's blank-line guard may also apply. I did not derive an expected output the spec does not show (FC-035).
- Fix All with the new withheld cases: `VerifyFixAllLeavesTheIndexersWhoseFixIsWithheld` already mixes withheld and offered indexers through the same withheld path (`RegisterCodeFixesAsync` registering no action), and Fix All asks the same provider per diagnostic; no new arm.
- FC-004/019/020/021 (trivia loss): the new cases are withheld, so the fix rewrites nothing for them.
- Formatter, CLI, naming, and unsupported-shape classes: no formatter or shape change in this round.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5419"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._
