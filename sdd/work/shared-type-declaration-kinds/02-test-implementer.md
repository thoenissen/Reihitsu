Role: test implementer
Commit: fe82a22
Verdict: tests written

The prompt carried no intended fix or hoped-for verdict. The scope came from `00-lead.md` (Decisions 2–4) and `01-skeptic.md` (*Narrow option*): a test-only change that adds one canary, with no production, spec, or rule-doc change. It pins existing behavior, so there is no reproduction, and the test passes on the unchanged tree.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.ArchitectureTests.General.TypeDeclarationKindsTests.TypeDeclarationSyntaxTypesAndKindsAreFullyEnumerated` (`src/Reihitsu.ArchitectureTests/General/TypeDeclarationKindsTests.cs`) | none (canary for a Roslyn version bump; no requirement behind it) | plain MSTest `Assert.AreSequenceEqual` | pass |

How the test finds the pairs it compares against nine pinned `Type: Kind` pairs:

- **Kinds on `TypeDeclarationSyntax` types.** It calls `SyntaxFactory.TypeDeclaration(kind, "Sample")` for every `SyntaxKind` value and records the node type and kind of each call that succeeds. A new kind on an existing type therefore shows up as a new pair without anyone listing it first. This is the blind spot the skeptic named: it is how `RecordStructDeclaration` arrived on `RecordDeclarationSyntax`.
- **Kinds the factory rejects.** Enum, extension block, and delegate are taken from a parsed source.
- **Missing partners.** Every kind where `SyntaxFacts.IsTypeDeclaration` is true but no type was found becomes `(no syntax type): Kind`. Every concrete `BaseTypeDeclarationSyntax` subtype with no kind found becomes `Type: (no syntax kind)`. A new kind or a new type that neither discovery path produces still fails the test, and the message names it.
- **Delegates.** `DelegateDeclaration` is included because `SyntaxFacts.IsTypeDeclaration` counts it, and RH5108/RH5110 select it.
- **Failure message.** It tells the reader to review the explicit kind and type lists in the analyzers, Core, and the formatter, and the specs that list type kinds, and then to update the pinned pairs.

The test was watched failing for the right reason. I temporarily removed the `enum` from the parsed source. The test then failed with `(no syntax type): EnumDeclaration` and `EnumDeclarationSyntax: (no syntax kind)` in the actual list, which shows both missing-partner markers work. I then restored the file.

RH5404's existing type canary (`RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyBaseTypeDeclarationTypesAreFullyEnumerated`) is left unchanged, per skeptic internal choice 1A.

## Scenarios (bug fix)
_None._ No reproduction: the run covers existing behavior and changes nothing a user sees.

## Not covered, and why
- **Line endings, formatter idempotency, code-fix convergence, comment or directive gaps:** do not apply. The test reads Roslyn's syntax model through reflection and the factory, and formats no source.
- **Failure classes:** none of the catalog reaches this change, because it touches no production code. FC-017 (asserting less than is guaranteed) is met by pinning the full ordered set of pairs, not a subset.
- **Per-rule exclusion table (the issue's option 2):** dropped by lead decision 2.
- **The skeptic's follow-up candidates 1–4:** these are output changes outside this run. No test was written for them.

## Commands
```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.General.TypeDeclarationKindsTests"
```
Full architecture project: 198 passed.

## Follow-up candidates
_None._
