Role: spec author
Commit: e270a88
Verdict: text written

Repair round 1, item (a) of `08-auditor.md` finding #1. I re-derived the change from the code instead of taking the auditor's suggestion as given. It stays a clarification of current behavior under the maintainer's escalation-4 decision (option A: narrow the clause to what the code does), so there is no change doc. No requirement ID was added, retired, or renumbered, and `Uses` and code owners are unchanged. My prompt carried no intended fix or hoped-for verdict, so there was nothing to ignore.

## What the code does

`ReihitsuFormatter.FormatNode` passes `node.SyntaxTree.Options` to `LanguageVersionResolver.Resolve`. Nothing else decides the version. Which tree `node.SyntaxTree` returns is Roslyn's business, and it depends on how the node was obtained. A node has no tree of its own in a useful sense, because Roslyn always returns one. That is why the old phrase "without a tree of its own" was inexact.

I probed this against the Release `Reihitsu.Formatter.dll` built from e270a88. The probe is a scratch console project outside the repository, referencing Microsoft.CodeAnalysis.CSharp 5.9.0. The input is `class Empty\n{\n}`, parsed as C# 11 (`root`, `cls = root.Members[0]`). Each node goes to `FormatNode` with no `indentLevel`. "11" means the output stayed braced, and "14" means `class Empty;`.

| Node passed | Reported tree | Result |
|---|---|---|
| `cls` (inside the tree) | C# 11 | 11 |
| `ParseCompilationUnit(src, options: C# 11)`, `ParseMemberDeclaration(src, options: C# 11)` | C# 11 | 11 |
| `ParseCompilationUnit(src)`, `ParseMemberDeclaration(src)`, `SyntaxFactory.ClassDeclaration("Empty").NormalizeWhitespace()` | default | 14 |
| `cls.ReplaceToken`, `cls.ReplaceTrivia`, `cls.WithoutTrivia()`, `cls.WithLeadingTrivia()`, `cls.WithTrailingTrivia(lf)`, `cls.NormalizeWhitespace()` | C# 11 | 11 |
| `root.ReplaceNode(cls, cls.WithIdentifier(…))`, as root and as `.Members[0]` | C# 11 | 11 |
| `root2.RemoveNode(…)`, `root.InsertNodesAfter(…)` | C# 11 | 11 |
| `ParseMemberDeclaration(C# 11).ReplaceToken(…)` and `.WithoutTrivia()` | C# 11 | 11 |
| `cls.WithIdentifier(new token)`, `cls.AddModifiers(…)`, `cls.WithAdditionalAnnotations(…)`, `ParseMemberDeclaration(C# 11).WithIdentifier(…)` | default | 14 |
| `SyntaxFactory.CompilationUnit().AddMembers(cls)`, as root and as `.Members[0]` | default | 14 |
| `cls.WithIdentifier(…).NormalizeWhitespace()`, `cls.WithIdentifier(…).WithoutTrivia()` | default | 14 |
| `cls.WithIdentifier(cls.Identifier)`, `cls.Update(<same parts>)`, `pcu.WithMembers(pcu.Members)` (nothing changes, the same instance comes back) | C# 11 | 11 |
| `CSharpSyntaxTree.Create(cls.WithIdentifier(…), C# 11).GetRoot()` | C# 11 | 11 |

The auditor's twelve shapes all reproduce. The split runs between edits that return a new root, which carry over the edited node's tree options, and nodes created anew, which get the default options: factory methods, generated `With…`/`Add…` methods, and `WithAdditionalAnnotations`. An edit of a created node keeps the default options.

## Text changed

`sdd/specs/formatter/pipeline.md`, FMT-PIPE-008:

- The `FormatNode` bullet now holds only the normative source: "the parse options of the tree that Roslyn reports as the given node's `SyntaxTree`, whether or not the node has a parent". This holds for every probed shape.
- A new list after the bullets says which tree Roslyn reports, with four cases:
  - inside a parsed tree;
  - `Parse…` or `CSharpSyntaxTree.Create` with parse options;
  - an edit that returns a new root (named members), which carries over the edited node's tree options to it and the nodes inside it;
  - these nodes get the default options: `Parse…` without options, built with `SyntaxFactory`, a generated `With…`/`Add…` or `WithAdditionalAnnotations` call that changes the node, and nodes inside such a node. A later edit keeps the default options, and a no-op `With…` returns the node itself.
- The last example paragraph names one shape on each side of every case.

I deliberately did not write the last case as "every other node". I probed only the members named, and a catch-all would claim more than I verified.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 51 passed, 0 failed.

Within escalation 4: the decision was to narrow the clause to the code, not to change it. The derived-node split is that same code, `node.SyntaxTree.Options`, seen through Roslyn. No second reading of the code yields different output, so I raised no question. One point the lead may want to weigh: a caller that edits a C# 11 node with `WithIdentifier` gets C# 14 output, which a C# 11 compiler rejects. That is the same exposure the maintainer accepted for a node parsed without options (option C was rejected). The only production caller (RH5401 `CanCollapse`) passes a node inside a tree.

## Changed requirements

- FMT-PIPE-008 — Version-dependent rules follow the language version — clarified. The `FormatNode` source is stated through the node's reported `SyntaxTree`, with the cases that decide which tree that is, including nodes derived after parsing.

## Examples for the tests

FMT-PIPE-008. Setup for every case, under LF and CRLF (`eol` is `"\n"` or `"\r\n"`):

```csharp
var src  = $"class Empty{eol}{{{eol}}}";
var o11  = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);
var root = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(src, o11).GetRoot();
var cls  = (ClassDeclarationSyntax)root.Members[0];
var sameId = SyntaxFactory.Identifier(cls.Identifier.LeadingTrivia, "Empty", cls.Identifier.TrailingTrivia);
```

For every input below, `node.ToFullString() == src`, so each test can assert its input first. A second `FormatNode` pass is stable, and nothing differs between LF and CRLF.

Keeps the C# 11 options, so `FormatNode(node).ToFullString() == src` (unchanged):

```csharp
cls.ReplaceToken(cls.Identifier, sameId)
cls.WithoutTrivia()
cls.NormalizeWhitespace(eol: eol)
root.ReplaceNode(cls, cls.WithIdentifier(sameId))   // compilation unit; the WithIdentifier class inside it now reports C# 11
```

Gets the default options, so `FormatNode(node).ToFullString() == "class Empty;"`:

```csharp
cls.WithIdentifier(sameId)
cls.WithAdditionalAnnotations(new SyntaxAnnotation())
SyntaxFactory.CompilationUnit().AddMembers(cls)
cls.WithIdentifier(sameId).WithoutTrivia()           // a later edit keeps the default options
```

The test implementer chooses how many to pin. One per side is enough to cover the auditor's gap: `cls.WithoutTrivia()` and `cls.WithIdentifier(sameId)`. The `ReplaceNode` / `AddMembers` pair is the clearest contrast, because the same changed class goes into a compilation unit with opposite results.

These existing tests already pin the parsed and built shapes: `FormatNodeKeepsParseOptionsOfDetachedNodeParsedWithOlderOptions`, `FormatNodeFollowsTheParseOptionsOfDetachedMemberDeclaration`, `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion`, and `FormatNodeFormatsNodeBuiltWithSyntaxFactoryForTheNewestSupportedVersion`.

For item (c) and the cleaner's item (b), the clause to align with is "the language version of the tree Roslyn reports as the node's `SyntaxTree`". A node parsed without parse options, the input of `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion`, reports the default parse options. "A detached node carries the default parse options" is false as a general statement.

## Open questions for the maintainer

_None._
