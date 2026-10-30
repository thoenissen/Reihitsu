Role: test implementer
Commit: 9e460f3
Verdict: tests written — all pass

This is repair round 1, item (c) of `08-auditor.md` finding #1, plus covering tests for the cases the reworded FMT-PIPE-008 `FormatNode` clause (`09-spec-author.md`) names. My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Expected values come from the reworded FMT-PIPE-008 list and the examples in `09-spec-author.md`, not from the code. No production code was touched. Every new test describes existing behavior and passes on the unchanged code.

## Item (c): summaries

All edits are in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterLanguageVersionTests.cs`.

- `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion`: the old summary said "a detached node, which carries the default parse options". It now says "a node parsed without parse options, whose reported syntax tree carries the default parse options". That claims only what holds for its input.
- `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions` and `FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration`: both said "which has no tree of its own", the phrase the auditor called inexact. They now say "without a parent reports a syntax tree with those parse options".

The test names and assertions are unchanged.

## Coverage of the reworded `FormatNode` list

| Case in FMT-PIPE-008 | Covering test |
|---|---|
| A node inside a parsed tree | `FormatNodeFollowsTheTreeVersionForTheSpecEmptyClass`, `FormatNodeKeepsEmptyClassBracedForCSharp11Tree`, `FormatNodeConvertsEmptyClassForCSharp12Tree` (existing) |
| `Parse…` with parse options | `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions`, `FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration` (existing) |
| Root of `CSharpSyntaxTree.Create` with parse options | `FormatNodeFollowsTheParseOptionsOfRootCreatedWithCSharpSyntaxTreeCreate` (new) |
| An edit that produces a new root keeps the edited node's tree options | `…ReturnedByWithoutTrivia`, `…ReturnedByReplaceToken`, `…ReturnedByNormalizeWhitespace`, `…RootReturnedByReplaceNode` (new) |
| `Parse…` without parse options | `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion` (existing, LF only) |
| A node built with `SyntaxFactory` | `FormatNodeFormatsNodeBuiltWithSyntaxFactoryForTheNewestSupportedVersion` (existing), `FormatNodeFormatsCompilationUnitBuiltAroundParsedClassForTheNewestSupportedVersion` (new) |
| A generated `With…` or `WithAdditionalAnnotations` that changes the node | `…ChangedByWithIdentifier…`, `…ChangedByWithAdditionalAnnotations…` (new) |
| A later edit keeps the default parse options | `FormatNodeFormatsEditOfChangedNodeForTheNewestSupportedVersion` (new) |
| A `With…` that changes nothing returns the node itself | `FormatNodeKeepsParseOptionsOfNodeReturnedByUnchangingWithIdentifier` (new; also asserts `AreSame`) |

The `ReplaceNode` test and the `CompilationUnit().AddMembers` test give the contrast the spec author pointed out. The same kind of changed class goes into a compilation unit and gets opposite results.

## Tests

All tests are in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterLanguageVersionTests.cs`. The derived-node tests share the private helper `AssertFormatNodeOfDerivedNode`. Under LF and CRLF it does four things:

1. Parses `class Empty{eol}{{eol}}` as C# 11.
2. Derives the node.
3. Asserts that the derived node's full text still equals the input. This way the output difference can come only from the version.
4. Asserts the exact `FormatNode` output: unchanged when the node keeps C# 11, `class Empty;` when it gets the default options.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FormatNodeFollowsTheParseOptionsOfRootCreatedWithCSharpSyntaxTreeCreate`: C# 11 stays braced, C# 12 converts | FMT-PIPE-008 | node text, LF and CRLF | pass |
| `FormatNodeKeepsParseOptionsOfNodeReturnedByWithoutTrivia` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeKeepsParseOptionsOfNodeReturnedByReplaceToken` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeKeepsParseOptionsOfNodeReturnedByNormalizeWhitespace` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeKeepsParseOptionsOfRootReturnedByReplaceNode` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeKeepsParseOptionsOfNodeReturnedByUnchangingWithIdentifier` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeFormatsNodeChangedByWithIdentifierForTheNewestSupportedVersion` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeFormatsNodeChangedByWithAdditionalAnnotationsForTheNewestSupportedVersion` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeFormatsCompilationUnitBuiltAroundParsedClassForTheNewestSupportedVersion` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeFormatsEditOfChangedNodeForTheNewestSupportedVersion` | FMT-PIPE-008 | `AssertFormatNodeOfDerivedNode` | pass |
| `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion`, `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions`, `FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration`: summary only | FMT-PIPE-008 | unchanged | pass |

## Not covered, and why

- **The remaining named edits.** `ReplaceTrivia`, `RemoveNode`, the `Insert…` methods, `WithLeadingTrivia`, `WithTrailingTrivia`, and `Add…` on a parsed node have no test of their own. They are Roslyn methods, not formatter arms. The formatter reads only `node.SyntaxTree.Options` in `ReihitsuFormatter.FormatNode`, the one call site. Four representative edits pin the "keeps" side and three pin the "default" side. The spec author probed the rest in `09-spec-author.md`.
- **A second `FormatNode` pass.** `09-spec-author.md` says it is stable. The reworded clause does not state it, and the output of `FormatNode` is itself a derived node whose reported tree the spec does not name. Asserting it would pin unspecified behavior.
- **The parsed-without-options case under CRLF.** It stays as `05-test-implementer.md` left it. The built-node tests cover the default-options side under CRLF.
- **Failure classes for production changes, code fixes, and analyzer parity.** None apply, because no production code changed in this round.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~ReihitsuFormatterEntryPointTests|FullyQualifiedName~ReihitsuFormatterLanguageVersionTests"
scripts/test.sh --project architecture
```

Results at 9e460f3 plus these tests: the formatter filter gave 88 passed, 0 failed (78 before, plus 10 new). Architecture gave 89 passed, 0 failed.
