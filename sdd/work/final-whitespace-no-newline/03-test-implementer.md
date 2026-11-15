Role: test implementer
Commit: 8cc1c2aba131d5f922c0fdbf47da9cf9a19fa307 (tests run against this head; the tests are committed on top of it)
Verdict: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

All tests are in `src/Reihitsu.Formatter.Test/Regression/Cleanup/EndOfFileWhitespaceTests.cs`. Every fixture is an explicit string with `\n` escapes (FC-033), never a raw string literal; `AssertRuleResult` runs each one under LF and CRLF, asserts the exact output, a second pass, and phase stability.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpacesBehindTheLastTokenWithoutFinalLineBreakAreRemoved` (reproduction, issue case) | FMT-CLEAN-005 | `AssertRuleResult` | fail |
| `TabBehindTheLastTokenWithoutFinalLineBreakIsRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | fail |
| `SpacesBehindTheLastTokenOfAFileWithAByteOrderMarkAreRemoved` | FMT-CLEAN-005, FMT-CLEAN-009 | `AssertRuleResult` | fail |
| `SpacesBehindABlockCommentOnTheLastTokensLineAreRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | fail |
| `SpacesBehindABlockCommentOnItsOwnLineAreRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | fail |
| `SpacesBehindTheLastOfTwoBlockCommentsOnTheirOwnLinesAreRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | fail |
| `SpacesBehindTheOnlyCommentOfAFileAreRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | fail |
| `OutputWithoutFinalLineBreakEqualsOutputWithFinalLineBreak` (7 rows: members 1, 4, 5, 6, 7, 8, 9) | FMT-CLEAN-005 | `ReihitsuFormatter.FormatSyntaxTree`, LF and CRLF, plus `AssertSecondPassIsStable` | fail (all 7 rows) |
| `FormatDocumentRemovesTheSpacesBehindTheLastToken` (CLI path) | FMT-CLEAN-005 | `ReihitsuFormatter.FormatDocumentAsync`, LF and CRLF | fail |
| `FormatNodeInDocumentOnTheCompilationUnitRemovesTheSpacesBehindTheLastToken` | FMT-CLEAN-005 | `ReihitsuFormatter.FormatNodeInDocumentAsync` on the root, LF and CRLF | fail |
| `SpacesBehindTheLastTokenFollowedByAFinalLineBreakAreRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | pass |
| `WhitespaceOnlyLastLineIsRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | pass |
| `WhitespaceOnlyLastLineBehindABlockCommentIsRemoved` | FMT-CLEAN-005 | `AssertRuleResult` | pass |
| `GapsOnTheLineOfTheLastStatementAreKept` (spec's other side, FC-002) | FMT-CLEAN-005 | `AssertRuleResult` (unchanged) | pass |
| `SpacesInsideASingleLineCommentThatEndsTheFileAreKept` (spec's other side) | FMT-CLEAN-005 | `AssertRuleResult` (unchanged) | pass |
| `WhitespaceOnlyLastLineBehindATrailingDirectiveIsRemovedAndTheDirectiveKeepsItsLineBreak` | FMT-CLEAN-005, FMT-CLEAN-007 | `AssertRuleResult` | pass |
| `WhitespaceBehindAnEndIfThatEndsTheFileIsKept` (characterization, directive structure; disabled text with trailing spaces before it) | FC-020 | `AssertRuleResult` (unchanged) | pass |
| `GapInFrontOfATargetOnTheSameLineIsKept` (characterization of the node-level consumer of `EndOfLineWhitespace.StripBeforeFollowingLineBreak`) | FC-015, FC-001 | `ReihitsuFormatter.FormatNodeInDocumentAsync` on `int b;` in `int a;   int b;`, LF and CRLF | pass |

Every failing test was watched fail on this head for the issue's own difference only: in each, the strings match up to the end of the expected text and the actual output carries the trailing whitespace (`difference: expected [[<end>]]; actual [[<space>]]`), e.g. `"class C\n{\n    int x;\n}\n\n/* end */"` expected versus `"...\n/* end */   "` actual. Totals on this head: 16 failed, 8 passed.

The equivalence test compares against the formatter's own output for the input with a final line break appended — the normative sentence of FMT-CLEAN-005 ("a file without a final line break gives the same output as the same file with one") — so member 6 (`}  /* a */  /* b */  `), whose inner gaps no requirement decides, is tested without fixing those gaps. Both arms of that comparison go through the code under test; the `TrimEnd` assertion next to it keeps it from passing on an output that is wrong on both arms.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `}` + 3 spaces, no final line break (issue) | LF, CRLF | full pipeline, `FormatSyntaxTree`, `FormatDocumentAsync`, `FormatNodeInDocumentAsync` (root) | reproduced | `}···` vs `}` |
| `}` + tab | LF, CRLF | full pipeline | reproduced | `}····` (tab replaced, kept) vs `}` |
| byte order mark + … `}···` | LF, CRLF | full pipeline | reproduced | `}···` vs `}`, byte order mark kept in both |
| `} /* c */···` | LF, CRLF | full pipeline | reproduced | trailing `···` kept vs removed |
| `}\n\n/* end */···` | LF, CRLF | full pipeline | reproduced | trailing `···` kept vs removed |
| `}\n\n/* a */\n/* b */···` | LF, CRLF | full pipeline | reproduced | trailing `···` kept vs removed |
| `/* only */···` | LF, CRLF | full pipeline | reproduced | trailing `···` kept vs removed |
| `}  /* a */  /* b */  ` | LF, CRLF | `FormatSyntaxTree` equivalence | reproduced | output ends `··` without final line break, ends `*/` with one |
| `}···\n`, `}\n···`, `} /* c */\n···` | LF, CRLF | full pipeline | holds | `}` / `} /* c */` |

Counterpart surface: RH5602's report and fix on the formatter output were already observed by the skeptic (`01-skeptic.md`, 6 of 6 fixed, LF and CRLF); no analyzer test is added, because RH5602 is not changed and already behaves as the spec demands.

## Not covered, and why

- **Code-fix path / Fix All (FC-007, FC-008, FC-009, FC-030, FC-043)**: no code fix changes; a code fix formats a non-root target, whose trailing trivia is written back by FMT-PIPE-010 (`ReihitsuFormatterNodeTrailingLineEndTests` covers that), and the one node-level path that runs the cleanup at the end of a file — a compilation-unit target — is tested above.
- **Detached roots, FMT-CLEAN-008 (FC-028)**: lead decision 5 keeps detached roots unchanged; no caller sees the difference. No test added.
- **Directive-line whitespace (#960 item 1), `//` comment text (#959), documentation-comment lines**: out of scope; the `// end···` holding test and the `#endif···` characterization pin today's behavior.
- **Directive-only file with a whitespace-only last line (FMT-CLEAN-007 follow-up candidate)**: open question recorded in `00-lead.md`; not tested so as not to decide it.
- **FC-004, FC-005, FC-019, FC-021**: the change deletes only whitespace trivia that ends the file; comment cases (block comments on the token's line and on their own line, `//` comment) and a directive case are covered above. No join or move is involved.
- **FC-006, FC-022–FC-027, FC-029, FC-036**: no rewrite of syntax, literals, identifiers, or members.
- **FC-012**: no neighbor phase writes whitespace at the end of a file; `AssertRuleResult`'s phase stability check runs every phase alone on each output.
- **FC-013/FC-014**: RH5602 already matches the target output (skeptic).
- **FC-038–FC-043**: no shape is declared unsupported.
- **Self-hosting**: covered by CI's `FormatterSelfHostingTests`, not duplicated.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Cleanup.EndOfFileWhitespaceTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Cleanup"
```
