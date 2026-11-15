Role: test implementer
Commit: 5703c8c (tests written against this head; src unchanged since 26eefea)
Verdict: tests written

Repair round 1. The prompt carried the lead's worklist (decisions 14, 15, 18) and no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples of FMT-STRUCT-019/020, RH5107-004, FMT-BREAK-012, and FMT-BREAK-038 as written by 14-spec-author. Where a fixture needs a layout no example shows (collection-initializer brace columns, the multi-line `new[]` continuation column), it is the layout head gives the same input with `/* */` in place of `/** */` (element trailing trivia, which the removal already carries), observed with the branch's built CLI on `scratchpad/ti15/a.cs`. The decision-18 rows are characterization: their expected values are the spec text and agree with today's output.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TrailingCommaDocumentationCommentTests.SpecificationExampleCommentsInFrontOfTheEnumTrailingCommaStayBehindTheLastMember` (Parse, Diagnose, None) | FMT-STRUCT-019; FC-040 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | Parse, Diagnose: **fail** — `Large /** largest */` becomes `Large` (comment dropped). None: pass |
| `…DocumentationCommentInFrontOfTheEnumTrailingCommaStaysInFrontOfTheCommasLineComment` (Parse, Diagnose, None) | FMT-STRUCT-019; FC-040 | `AssertRuleResult` | Parse, Diagnose: **fail** — `B /** doc B */ // after` becomes `B  // after`. None: pass |
| `…DocumentationCommentInFrontOfTheArrayInitializerTrailingCommaStaysBehindTheLastElement` (one-line `int[] _a = { 1, 2 /** two */, };` and multi-line `new[] { 1,⏎2 /** two */, }`; Parse, Diagnose, None) | FMT-STRUCT-020; FC-040 | `AssertRuleResult` | Parse, Diagnose: **fail** — `/** two */` dropped. None: pass |
| `…DocumentationCommentInFrontOfTheCollectionInitializerTrailingCommaStaysBehindTheLastElement` (`new List<int> { 1 /** one */, }`; Parse, Diagnose, None) | FMT-STRUCT-020; FC-040 | `AssertRuleResult` | Parse, Diagnose: **fail** — `/** one */` dropped. None: pass |
| `RH5107…AnalyzerTests.VerifySpecificationExampleDocumentationCommentsBehindTheCommaWithholdTheFix` (spec methods `S`, `U`; Parse, Diagnose, None) | RH5107-004; FC-040 | `VerifyNoCodeFixIsOffered(source, DocumentationMode)` | Parse, Diagnose: **fail** — fix offered. None: pass |
| `…VerifyNoCodeFixForEachDocumentationCommentBehindTheComma` (`, /** c */ int b`, `, /** c */⏎int b`, `, /// c⏎int b` × Parse, Diagnose, None) | RH5107-004; FC-040, FC-021 | `VerifyNoCodeFixIsOffered(source, DocumentationMode)` | 6 Parse/Diagnose rows: **fail** — fix offered. 3 None rows: pass |
| `…VerifyNoCodeFixForDocumentationCommentBehindTheCommaWithCarriageReturnLineFeeds` (`/** c */`, `///`; Parse) | RH5107-004; FC-010 | `VerifyNoCodeFixIsOffered(source, DocumentationMode.Parse)` | **fail** — fix offered |
| `…VerifyDocumentationCommentOnTheLineBehindTheCommaKeepsTheFix` (spec `V`) | RH5107-004, RH5107-003; FC-003 | `Verify` | pass |
| `…VerifySingleLineDocumentationCommentOnTheLineBehindTheCommaKeepsTheFix` (`,⏎/// c⏎int b`) | RH5107-004, RH5107-003; FC-003 | `Verify` | pass |
| `LeadingCommaDocumentationCommentTests.DelimitedDocumentationCommentBehindALeadingArgumentCommaLeadsTheArgument` (Parse, Diagnose) | FMT-BREAK-012 | `AssertRuleResult` | pass (characterization, decision 18) |
| `…SingleLineDocumentationCommentBehindALeadingArgumentCommaLeadsTheArgument` (Parse, Diagnose) | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `…DelimitedDocumentationCommentBehindAnArgumentCommaIsPlacedAsForCommaLastCode` (comma-first and comma-last inputs, same expected, Parse) | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `…DelimitedDocumentationCommentSyntaxBehindALeadingArgumentCommaMovesWithTheCommaInDocumentationModeNone` | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `…SingleLineDocumentationCommentSyntaxBehindALeadingArgumentCommaMovesWithTheCommaInDocumentationModeNone` | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `…DelimitedDocumentationCommentBehindALeadingInitializerCommaLeadsTheMember` (Parse, Diagnose) | FMT-BREAK-038 | `AssertRuleResult` | pass |
| `…SingleLineDocumentationCommentBehindALeadingInitializerCommaLeadsTheMember` (Parse, Diagnose) | FMT-BREAK-038 | `AssertRuleResult` | pass |
| `…DelimitedDocumentationCommentBehindAnInitializerCommaIsPlacedAsForCommaLastCode` (object initializer and collection expression, comma-first and comma-last, same expected, Parse) | FMT-BREAK-038 | `AssertRuleResult` | pass |
| `…DelimitedDocumentationCommentSyntaxBehindALeadingInitializerCommaMovesWithTheCommaInDocumentationModeNone` | FMT-BREAK-038 | `AssertRuleResult` | pass |
| `…SingleLineDocumentationCommentSyntaxBehindALeadingInitializerCommaMovesWithTheCommaInDocumentationModeNone` | FMT-BREAK-038 | `AssertRuleResult` | pass |

Files:
- `src/Reihitsu.Formatter.Test/Regression/Structural/TrailingCommaDocumentationCommentTests.cs` (new)
- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/LeadingCommaDocumentationCommentTests.cs` (new)
- `src/Reihitsu.Analyzer.Test/Formatting/RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests.cs` (five tests added; none changed)

## Which fail on head, and why
- **8 formatter rows fail** (`TrailingCommaDocumentationCommentTests`, every Parse and Diagnose row; first-pass LF mismatch): the trailing-comma removal drops a `/** */` comment that stands on the last element's line in front of the comma. The failure output is the auditor's finding 1 exactly, e.g. expected `Large /** largest */`, actual `Large`; expected `B /** doc B */ // after`, actual `B  // after`. The None rows pass, because there the comment is the element's trailing trivia.
- **10 RH5107 rows fail** (Parse and Diagnose rows of the spec-example and each-shape tests, both CRLF rows): `No code fix may be offered for the diagnostic at [38..39)` — the fix is registered for a `/** */` or `///` comment behind the comma (finding 2a). The None rows and the two keep-the-fix tests pass.
- **Decision-18 rows (16) pass**, as decision 18 expects: no `src` change for them.

CS1587 note (14-spec-author): under `Diagnose`, `VerifyNoCodeFixIsOffered`'s analyzer step does not trip over CS1587 (warnings are not compared by the verifier), so no markup for it is needed; the failing Diagnose rows fail at the action check, not at verification. `Verify` for `V` runs on the verifier's default parse options and passes.

Risk for the implementer (observation only): once the removal carries the comma's leading trivia, the `/** */` stands in front of `}` or `;` on the last element's line. In Parse and Diagnose it is then the leading trivia of the closing brace on the second pass. `AssertRuleResult` checks the second pass and phase stability, so any relocation by the documentation-comment phase would surface in these tests. FMT-STRUCT-019's example (main's output per 14-spec-author) shows the comment staying.

## Not covered, and why
- Removing transforms other than enum, array initializer, collection initializer: none exist (`rg "PreserveSeparatorTrivia"` → `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`; per 13-auditor's policy table). Lists that keep their trailing comma (object initializer, anonymous object, collection expression, property pattern) run no removal; the auditor checked them unchanged.
- `///` in front of a trailing comma on the element's line: cannot be written (it runs to end of line, so the comma would be inside the comment).
- RH5107 Fix All with a documentation comment: the guard decides per diagnostic, and `VerifyFixAllMovesOnlyTheCommaWithoutACommentBehindIt` already covers the mixed list for a `/* */` comment; the documentation-comment variants reach the same registration call.
- Directive in the gap (rule touching token gaps): `VerifyDirectiveOnTheLineBehindTheCommaKeepsTheFix` exists; a directive in front of a comma on a later line is refused (FMT-UNSUP-009) and covered by the precheck class.
- Decision 17: no test changes (14-spec-author).
- Parameter lists on the formatter side (FMT-BREAK-011): not named by decision 18 or the changed requirements; argument and initializer lists cover FMT-BREAK-012/038.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~TrailingCommaDocumentationCommentTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~LeadingCommaDocumentationCommentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests"
```
