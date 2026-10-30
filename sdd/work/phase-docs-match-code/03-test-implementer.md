Role: test implementer
Commit: 9ab5415
Verdict: tests written — one covering test fails against the current code (finding against FMT-PIPE-008's detached-node sentence)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The run claims the tooling lane with the implementer skipped, so every test describes existing behavior; a red test is a finding against the spec, reported here and not weakened.

## Finding against the spec

`ReihitsuFormatterLanguageVersionTests.FormatNodeFormatsDetachedNodeParsedWithOlderOptionsForTheNewestSupportedVersion` fails. It is the skeptic's FC-033 candidate (`02-skeptic.md`) for the FMT-PIPE-008 bullet "`FormatNode`: … a detached node belongs to a tree with the default parse options" and for the example sentence "Given to `FormatNode` as a detached node, built without a tree, it gets the C# 12 result, because the default parse options resolve to the newest supported version".

- Input: `SyntaxFactory.ParseCompilationUnit("class Empty\n{\n}", options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11))`, passed to `ReihitsuFormatter.FormatNode` with no `indentLevel`.
- Expected by the spec: `class Empty;`.
- Observed: `class Empty\n{\n}` (unchanged, the C# 11 result).

The node has no tree of its own, yet its `SyntaxTree` does not carry the default parse options: the node comes out formatted for the options it was parsed with. The same call without `options` gives `class Empty;` (`FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion`, green). So "detached node ⇒ default parse options" holds only for a detached node parsed or built without options. The test stays as written; the decision is the spec's.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SwitchCaseBraceRequirementTests.WrapJoinedInsideListElementDoesNotMakeTheSectionMultiLine` (spec example, FMT-BREAK-015 join) | FMT-SWITCH-003 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | pass |
| `SwitchCaseBraceRequirementTests.WrapJoinedAfterAssignmentOperatorDoesNotMakeTheSectionMultiLine` (FMT-BREAK-016 join) | FMT-SWITCH-003 | `AssertRuleResult` | pass |
| `SwitchCaseBraceRequirementTests.WrapKeptInMemberAccessChainMakesTheSectionMultiLine` (kept side, second shape next to the existing `A(1,⏎2)` test) | FMT-SWITCH-003 | `AssertRuleResult` | pass |
| `SwitchCaseBraceRequirementTests.ObjectInitializerExpandedByLineBreaksMakesTheSectionMultiLine` (spec example, FMT-BREAK-021 add) | FMT-SWITCH-003 | `AssertRuleResult` | pass |
| `SwitchCaseBraceRequirementTests.LambdaBlockBodyExpandedByLineBreaksMakesTheSectionMultiLine` (added side, lambda block) | FMT-SWITCH-003 | `AssertRuleResult` | pass |
| `SwitchCaseBraceRequirementTests.ReturnAfterStatementOnTheSameLineMakesTheSectionMultiLine` ("return on a later line", measured after FMT-BREAK splits the statements) | FMT-SWITCH-003 | `AssertRuleResult` | pass |
| `SwitchCaseBraceRequirementTests.ReturnAfterStatementAndCommentOnTheSameLineMakesTheSectionMultiLine` (comment in the split gap) | FMT-SWITCH-003 | `AssertRuleResult` | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentAsyncReturnsDocumentWithMissingSemicolonUnchanged` (spec example) | FMT-PIPE-005 | `Assert.AreSame` on the document, LF and CRLF | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentWithContextAsyncReturnsDocumentWithMissingSemicolonUnchanged` (spec example, target `M`, context `C`) | FMT-PIPE-005 | same | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentWithContextAsyncReturnsDocumentWithMissingSemicolonUnchangedWhenTargetIsContext` (delegation path) | FMT-PIPE-005 | same | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentAsyncReturnsDocumentUnchangedWhenSyntaxErrorIsOutsideTheTarget` | FMT-PIPE-005 | same | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentWithContextAsyncReturnsDocumentUnchangedWhenSyntaxErrorIsOutsideTheTarget` | FMT-PIPE-005 | same | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentAsyncReturnsGeneratedDocumentUnchanged` (spec example) | FMT-PIPE-006 | same | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentWithContextAsyncReturnsGeneratedDocumentUnchanged` (spec example) | FMT-PIPE-006 | same | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentWithContextAsyncReturnsGeneratedDocumentUnchangedWhenTargetIsContext` (delegation path) | FMT-PIPE-006 | same | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentAsyncFormatsTargetWhenMarkerIsInsideTheType` (holding side) | FMT-PIPE-006 | document text, LF and CRLF | pass |
| `ReihitsuFormatterEntryPointTests.FormatNodeInDocumentWithContextAsyncFormatsTargetWhenMarkerFollowsTheFirstToken` (holding side, marker on a type after a `using`) | FMT-PIPE-006 | document text, LF and CRLF | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatDocumentAsyncKeepsGetOnlyAccessorListInCSharp5Project` (spec example) | FMT-PIPE-008 | document text, LF and CRLF | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatDocumentAsyncConvertsGetOnlyPropertyInCSharp6Project` (spec example) | FMT-PIPE-008 | document text, LF and CRLF | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatSyntaxTreeFollowsTheTreeVersionForTheSpecPropertyExample` (spec example, C# 5 and C# 6) | FMT-PIPE-008 | `AssertFormatSyntaxTree` (LF, CRLF, second pass) | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatDocumentAsyncConvertsEmptyClassInCSharp12Project` | FMT-PIPE-008 | document text, LF and CRLF | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatNodeInDocumentAsyncKeepsSpecEmptyClassBracedInCSharp11Project` (node-level spec example) | FMT-PIPE-008 | document text, LF and CRLF | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatNodeInDocumentAsyncConvertsSpecEmptyClassInCSharp12Project` (node-level spec example) | FMT-PIPE-008 | same | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatNodeInDocumentWithContextAsyncKeepsSpecEmptyClassBracedInCSharp11Project` (node-level spec example, compilation unit as context) | FMT-PIPE-008 | same | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatNodeInDocumentWithContextAsyncConvertsSpecEmptyClassInCSharp12Project` (same) | FMT-PIPE-008 | same | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatNodeFollowsTheTreeVersionForTheSpecEmptyClass` (node-level spec example, C# 11 and C# 12 trees) | FMT-PIPE-008 | node text, LF and CRLF | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatNodeConvertsEmptyClassForCSharp12Tree` | FMT-PIPE-008 | node text, LF and CRLF | pass |
| `ReihitsuFormatterLanguageVersionTests.FormatNodeFormatsDetachedNodeParsedWithOlderOptionsForTheNewestSupportedVersion` (skeptic's FC-033 candidate) | FMT-PIPE-008 | node text | **fail** — see *Finding against the spec* |
| existing `ReihitsuFormatterLanguageVersionTests.FormatNodeInDocumentAsyncConvertsNestedEmptyClassInCSharp12Project` — tag added | FMT-PIPE-008 | unchanged | pass |
| existing `ReihitsuFormatterLanguageVersionTests.FormatNodeInDocumentWithContextAsyncConvertsEmptyClassInCSharp12Project` — tag added | FMT-PIPE-008 | unchanged | pass |
| existing `ReihitsuFormatterLanguageVersionTests.FormatNodeKeepsEmptyClassBracedForCSharp11Tree` — tag added | FMT-PIPE-008 | unchanged | pass |

Expected outputs come from the spec examples where one exists. Where none exists (assignment join, chain keep, lambda add, both "later line" cases, the two holding-side node-in-document cases), they are the current output, obtained with `scripts/format.sh` on scratch files for the switch inputs. Each matches the skeptic's observation table in `02-skeptic.md`. In the two "later line" cases, no blank line separates the closing brace of `case 1` from `case 2:`. That is how the formatter lays out a section that ends in a block without `break`. FMT-SWITCH-003 does not decide it, and the tests just record it.

## Not covered, and why

- **Joins of FMT-BREAK-014 (indexer, type arguments) and FMT-BREAK-015 for named arguments and tuple elements.** Each is one more input to the same measurement. `SwitchCaseBraceRewriter.IsMultiLineSection` reads `SyntaxNodeUtilities.IsSingleLine` on the tree `LineBreakPhase` returns, whichever FMT-BREAK rule did the join. I cover two joins from two different FMT-BREAK requirements (015 and 016). Named-argument output was probed and holds (`Foo(message: "x");`, no section braced).
- **Directive in the gap (FC for token gaps and line joins).** A directive inside a section makes FMT-SWITCH-010 leave that section unbraced, so it measures FMT-SWITCH-010, not FMT-SWITCH-003. The run changes no gap or join code. The comment case is covered.
- **FMT-PIPE-008 "tree" versus "project" for `FormatDocumentAsync`.** In an `AdhocWorkspace` the two are the same object (FC-033, `01-spec-author.md`, `02-skeptic.md`). The tests set the project's language version.
- **FMT-PIPE-008 clamping (`Default`, `Latest`, `LatestMajor`, `Preview`).** The tagged `LanguageVersionResolverTests` already cover it, and this run did not reword it.
- **FC-028 (detached versus document-scoped).** Each family is pinned separately. No test asserts that the families agree.
- **Code-fix, Fix All, analyzer parity, naming.** Not involved: no analyzer surface is touched.
- **Failure classes that reach production changes** (column computation across phases, trivia transfer). Not applicable: no production code changes.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~ReihitsuFormatterEntryPointTests|FullyQualifiedName~ReihitsuFormatterLanguageVersionTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterLanguageVersionTests.FormatNodeFormatsDetachedNodeParsedWithOlderOptionsForTheNewestSupportedVersion"
scripts/test.sh --project architecture
```

Results at 9ab5415 plus these tests: formatter filter 75 passed, 1 failed (the detached-node test above). Architecture: 89 passed, 0 failed.
