Role: test implementer
Commit: b9efa6c
Verdict: tests written — all pass

This is a follow-up to escalation 4. The maintainer chose option A (`squad/decisions/2026-10-30-phase-docs-match-code-detached-node-version.md`), and the spec author reworded FMT-PIPE-008 in `04-spec-author.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the recorded decision. Expected values come from the reworded FMT-PIPE-008 bullet and example, not from the code. No production code was touched.

## Coverage of the reworded `FormatNode` bullet

| Side of the bullet | Covering test |
|---|---|
| A node taken from a tree keeps its parse options | `FormatNodeFollowsTheTreeVersionForTheSpecEmptyClass`, `FormatNodeKeepsEmptyClassBracedForCSharp11Tree`, `FormatNodeConvertsEmptyClassForCSharp12Tree` (unchanged) |
| A node parsed with parse options, without a tree of its own, keeps them | `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions` (the formerly failing test, rewritten), `FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration` (new) |
| A node parsed without parse options gets the default parse options | `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion` (unchanged) |
| A node built without parse options gets the default parse options | `FormatNodeFormatsNodeBuiltWithSyntaxFactoryForTheNewestSupportedVersion` (new) |

## Tests

All tests are in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterLanguageVersionTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions`: replaces `FormatNodeFormatsDetachedNodeParsedWithOlderOptionsForTheNewestSupportedVersion`. The input is `SyntaxFactory.ParseCompilationUnit(SpecEmptyClassInput, options: C# 11)`. The test expects the unchanged input, per the spec example. It now runs LF and CRLF. | FMT-PIPE-008 | node text, LF and CRLF | pass |
| `FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration`: the nearest sibling shape, `SyntaxFactory.ParseMemberDeclaration` with C# 11 and C# 12 options. C# 11 stays braced and C# 12 gives `class Empty;`. Both versions run, so the test fails if `FormatNode` ignores the options in either direction. | FMT-PIPE-008 | node text, LF and CRLF | pass |
| `FormatNodeFormatsNodeBuiltWithSyntaxFactoryForTheNewestSupportedVersion`: `SyntaxFactory.CompilationUnit().AddMembers(SyntaxFactory.ClassDeclaration("Empty")).NormalizeWhitespace(eol)`. The test first asserts that the built text equals `SpecEmptyClassInput`, then expects `class Empty;`. This is the "built" side, which the spec now names and which had no test before. | FMT-PIPE-008 | node text, LF and CRLF | pass |

## Not covered, and why

- **The parsed-without-options side under CRLF.** `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion` runs LF only, and I left it unchanged. The CRLF counterpart is the built-node test: both go through `CSharpParseOptions.Default` on the node's tree.
- **Other shapes of a node without a tree, such as `.Members[0]` of a parsed compilation unit, or `ParseText(...).GetRoot().WithoutTrivia()`.** The spec author probed them in `04-spec-author.md`, and each keeps its options. The compilation-unit test and the member-declaration test pin two of these shapes. Every shape reaches the same `node.SyntaxTree.Options` read in `ReihitsuFormatter.FormatNode`.
- **The other FMT-PIPE-008 bullets and clamping.** The wording did not change, and `03-test-implementer.md` covers them.
- **Failure classes for production changes, code fixes, and analyzer parity.** None apply, because no production or analyzer code changed.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchCaseBraceRequirementTests|FullyQualifiedName~ReihitsuFormatterEntryPointTests|FullyQualifiedName~ReihitsuFormatterLanguageVersionTests"
scripts/test.sh --project architecture
```

Results at b9efa6c plus these tests: formatter filter 78 passed, 0 failed. Architecture: 89 passed, 0 failed.
