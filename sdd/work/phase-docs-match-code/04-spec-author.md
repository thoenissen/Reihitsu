Role: spec author
Commit: e587ab6
Verdict: text written

This follow-up carries out the maintainer's answer to escalation 4 (`squad/decisions/2026-10-30-phase-docs-match-code-detached-node-version.md`, option A). It is still a clarification of behavior the code already has, so there is no change doc. No requirement ID was added, retired, or renumbered, and `Uses` is unchanged. My prompt carried no intended fix or verdict beyond the recorded decision, so there was nothing to ignore.

## What the code does

`ReihitsuFormatter.FormatNode` resolves the language version from `node.SyntaxTree.Options`. Roslyn gives a node without a tree a tree that carries the options it was parsed with. Only a node parsed or built without options gets `CSharpParseOptions.Default`. I checked this against the built `Reihitsu.Formatter.dll` with a scratch probe that used no repository test code. The input was `class Empty\n{\n}`, passed to `FormatNode` with no `indentLevel`:

| Node | Options on `node.SyntaxTree` | `FormatNode` result |
|---|---|---|
| `SyntaxFactory.ParseCompilationUnit(src, options: C# 11)` | C# 11 | `class Empty\n{\n}` (unchanged) |
| `SyntaxFactory.ParseCompilationUnit(src, options: C# 11).Members[0]` | C# 11 | unchanged |
| `SyntaxFactory.ParseMemberDeclaration(src, options: C# 11)` | C# 11 | unchanged |
| `CSharpSyntaxTree.ParseText(src, C# 11).GetRoot().WithoutTrivia()` | C# 11 | unchanged |
| `SyntaxFactory.ParseCompilationUnit(src)` | default (C# 14) | `class Empty;` |
| `SyntaxFactory.CompilationUnit().AddMembers(SyntaxFactory.ClassDeclaration("Empty")).NormalizeWhitespace(eol: "\n")` | default (C# 14) | `class Empty;` |

## Text changed

`sdd/specs/formatter/pipeline.md`, FMT-PIPE-008:

- The `FormatNode` bullet now reads: "the parse options of the given node's syntax tree. A node taken from a tree, or parsed with parse options without a tree of its own, keeps those parse options; a node parsed or built without parse options belongs to a tree with the default parse options;"
- The last example sentence is replaced. A node without a tree of its own, parsed with C# 11 parse options, stays as it is. Parsed without parse options, or built with `SyntaxFactory`, it gets the C# 12 result.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 51 passed, 0 failed.

## Changed requirements

- FMT-PIPE-008 — Version-dependent rules follow the language version — clarified: the `FormatNode` source is narrowed. A node parsed with parse options keeps them. A node parsed or built without them gets the default parse options.

## Examples for the tests

Both sides use `SpecEmptyClassInput` (`class Empty\n{\n}`) and `ReihitsuFormatter.FormatNode(node)` with no `indentLevel`.

Keeps its parse options: this is the decided outcome for the failing test.

```csharp
var node = SyntaxFactory.ParseCompilationUnit(SpecEmptyClassInput, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11));
// FormatNode(node).ToFullString() == "class Empty\n{\n}"   (input unchanged)
```

`FormatNodeFormatsDetachedNodeParsedWithOlderOptionsForTheNewestSupportedVersion` should expect the unchanged input. Its name and summary should say that the node keeps its C# 11 options, for example `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions`.

Gets the default parse options (two inputs):

```csharp
var parsed = SyntaxFactory.ParseCompilationUnit(SpecEmptyClassInput);
// FormatNode(parsed).ToFullString() == "class Empty;"

var built = SyntaxFactory.CompilationUnit()
                         .AddMembers(SyntaxFactory.ClassDeclaration("Empty"))
                         .NormalizeWhitespace(eol: "\n");
// built.ToFullString() == "class Empty\n{\n}"
// FormatNode(built).ToFullString() == "class Empty;"
```

`FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion` already covers the parsed input with `TopLevelInput`. The built input is new: the spec now names "built with `SyntaxFactory`", and no test covers it yet. I observed both outputs with LF only. If the test also runs CRLF through its helper, the built input needs `NormalizeWhitespace(eol: "\r\n")`.

## Open questions for the maintainer

_None._
