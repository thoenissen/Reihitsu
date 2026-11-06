Role: implementer
Commit: 1d19e02
Verdict: #1136: tests pass; #1134: tests pass; #1135: tests pass; #1262: tests pass; #1332: tests pass; #1336: tests pass; #1340: tests pass

Inputs: `00-lead.md`, `02-skeptic.md`, `05-spec-author.md`, `06-test-implementer.md`, the change docs `withhold-unsafe-fixes.md` and `silence-uncorrectable-namespaces-and-regions.md`, and the specs they name. The prompt carried no intended fix, suspected cause, or hoped-for verdict. On e929cb0 the focused filter of the test handover gave exactly the 81 failures it lists; after 1d19e02 all of them pass and no test assertion, input, or expected output was changed.

## thoenissen/Reihitsu#1136

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | New `SplitsDirectivePair(root, openBracePosition, closeBracePosition)`: true when a brace pair inserted at the two positions puts exactly one brace into the code a directive governs — an `#if` group branch, a `#region` pair, or a `#pragma`/`#nullable`/`#line` directive up to the next one of its kind or the end of the file. A brace at a directive's position lands in front of it | SHARED-FIX-003 (brace split) |
| RH7004 fix (see #1336) | First consumer of the predicate | SHARED-FIX-003 |

### Policy owners

`rg -l 'SplitsDirectivePair|ContainsUnbalancedConditionalDirectives\(' src` (non-test): `SyntaxTriviaUtilities` (owner of both), RH7004 fix (only consumer of the new predicate), RH7309 fix, `OrderingMoveSafety`, `ExpressionBodyRewriteUtilities`, `AccessorExpressionBodyTransform` (relocation predicates, a different question: whether a span moved as one block carries a directive away). No earlier owner of the brace-split decision exists.

### Tests run

See *Whole run*; the #1136 tests are the RH7004 conditional, pragma, nullable, and region pair tests, all passing.

### Disputed tests or scope

Scope: `SplitsDirectivePair` is a new public member of `Reihitsu.Core`. It states SHARED-FIX-003's brace-split condition once, so the other brace-adding fixes the skeptic lists for an audit (RH3202–RH3208, RH5405–RH5407) can use it instead of a copy; today only RH7004 consumes it.

## thoenissen/Reihitsu#1134

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | New `FieldOrderSetsLayout(type)` (struct or record struct unless `[StructLayout]` names `LayoutKind.Auto`/`Explicit`; class or record only with a `StructLayout` attribute naming another kind; any qualification, `Attribute` suffix, named arguments), `IsLayoutField(member)` (instance non-const field, instance field-like event, instance property with a backing field: bodiless accessors not `abstract`/`extern`/partial definition, or the `field` keyword), `ChangesLayoutOrder(type, moved, target)` mirroring `ChangesInitializerExecutionOrder`; `HasInitializer` made public | SHARED-FIX-003 (run time), RH7109-006 |
| `RH7109…CodeFixProvider.IsMoveSafe` | Also refuses `ChangesLayoutOrder` | RH7109-006 |

### Policy owners

`rg -l 'StructLayout|LayoutKind' src` (non-test): only `OrderingDeclarationUtilities`. Consumers: RH7109 fix, RH7309 fix (`RegionBlockLayout`).

### Disputed tests or scope

Scope: three new public members and one member made public in `Reihitsu.Core` (`OrderingDeclarationUtilities`), shared by RH7109 and RH7309 so that neither fix holds a private copy (the skeptic's one-owner note).

## thoenissen/Reihitsu#1135

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/RegionBlockLayout.cs` (new, internal) | Models the body as gap, block, gap, …, gap and the reorder as a permutation of blocks with gaps fixed. `ChangesMemberOrder(type)`: compares the written and the reordered sequence of static-initializer members, instance-initializer members, and (for a layout type) layout fields, counting the members declared in disabled text of the body outside every member (parsed with the tree's options) at the position of that text. `KeepsSequenceBetween(start, end)`: whether the segments from one position to another stay adjacent and in order | RH7309-006 bullets 1, 2, 4; SHARED-FIX-003 (build configurations) |
| `RH7309…CodeFixProvider` | Registration and application share one `TryCreateReorder` (blocks, category order, RH7309-005 directive check, then `ChangesMemberOrder`, `SeparatesCommentFromBlock`, `ChangesDocumentedDeclaration`). Registration now needs the semantic model, because the categories decide the permutation | RH7309-006 |

An identity order is still offered at registration and changes nothing at application, as before; I did not change that (see follow-up candidates).

### Policy owners

`rg -l 'HasInitializer\(' src` (non-test): `OrderingDeclarationUtilities` (owner) and `RegionBlockLayout` (consumer). SHARED-ORDER-007's predicate has one owner.

## thoenissen/Reihitsu#1262

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH7303…CodeFixProvider` | New `TryFindRemovablePartner`: the partner must itself lie inside executable code (`RegionDirectiveUtilities.IsWithinElementBody`); used at registration and application | RH7303-005 |
| `RH7304…Analyzer.OnSyntaxTree` | Pairs all region directives as the compiler does (`GetRegionPairs(root, null)`) and skips every pair that has a region directive inside executable code at or between its two directives | RH7304-005 |
| `RH7304…CodeFixProvider` | New `TryGetRegionIndentation`; the fix is registered only when the text in front of the region's first token on its line is whitespace (or a BOM) | SHARED-FIX-003 |
| `RH7306…Analyzer` | `EndsWithForbiddenSuffix` and `ForbiddenSuffix` made internal; new internal `IsForbiddenSuffixAlone` and `GetDescription(directive)` | RH7306-003, RH7306-004 |
| `RH7306…CodeFixProvider` | The private copy of `EndsWithForbiddenSuffix` and of the constant is removed; the reported directive and its partner are judged on their descriptions with the analyzer's predicate (`TryGetDirectivesToFix`), and the fix is not registered when either description that ends with the word is the word alone | RH7306-003, RH7306-004; the skeptic's architecture finding, fixed narrowly as maintainer answer 1 decided |
| `RH7309…CodeFixProvider` | `HasCommentAbove`/`SeparatesCommentFromBlock`: a moved block whose `#region` has a comment above it in its leading trivia (whitespace and line breaks only between) withholds the fix; `ChangesDocumentedDeclaration`: in modes `Parse`/`Diagnose`, a documentation comment in a member's leading trivia that lies in another segment than the member withholds the fix unless the segments between them stay adjacent | RH7309-006 bullets 3, 4 |

### Policy owners

- `rg -l 'EndsWithForbiddenSuffix|ForbiddenSuffix =' src` (non-test): the RH7306 analyzer (owner) and the RH7306 fix (consumer only, no copy left).
- `rg -l 'GetRegionPairs\(' src` (non-test): `RegionDirectiveUtilities` (owner), RH7301 and RH7304 analyzers, `RegionNamingRewriter`. RH7304 now pairs with no filter, like `TryFindMatchingDirective`, which its fix and the RH7303 fix use.
- `rg -l 'IsDocumentationCommentTrivia\(' src` lists the shared helper in `SyntaxTriviaUtilities` and its consumers; the RH7309 fix reads the trivia kind only, so the documentation mode decides through the parser (SHARED-DOC-004).

## thoenissen/Reihitsu#1332

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH7501…CodeFixProvider` | New `CanMoveBreak`, checked in `TryGetFixableNodes`: the `break` starts its line (line of the previous token's end differs), the closing brace starts its line, and no directive or disabled text lies in the trailing trivia of the previous token, any trivia of the `break`, or the leading trivia of the closing brace | RH7501-004 |

A lone `break` on its own line still moves and leaves its block (`VerifyCodeFixLeavesEmptyBlockWhenBreakIsOnlyStatement` passes).

## thoenissen/Reihitsu#1336

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH7004…CodeFixProvider.ApplyCodeFixAsync` | Reuses the original `using` keyword (keeps `using /* c */`); the new `)` takes the semicolon's leading trivia (keeps a comment in front of `;`); when the semicolon's trailing trivia holds a comment, it goes behind `)` with its line break and the statement keeps only the line break, so the comment stays on the header line | RH7004-004, SHARED-FIX-003 |
| `RH7004…CodeFixProvider.WouldSeparateDirective` | Withholds when `SplitsDirectivePair(root, declaration.FullSpan.End, closeBrace.FullSpan.Start)` or when the enclosing block's closing brace has a conditional directive or disabled text in its leading trivia (code another configuration would put behind the new block) | SHARED-FIX-003 |
| `RH7101…CodeFixProvider.TryGetFixableFieldDeclaration` | Not offered when the declaration's leading trivia holds a documentation-comment trivia; the parser produces those only in modes `Parse`/`Diagnose` | RH7101-006 |
| `ModifierOrderingCodeFixProviderBase.ReorderingWouldDropCommentOrDirective` | Also true when the leading trivia of the token after the last modifier holds a comment, a directive, or disabled text | SHARED-MODORDER-003 |

B3 needed no change: the tests characterize `main`'s behavior and pass.

### Policy owners

`rg -n 'IsCommentOrDirective' src/Reihitsu.Core` — the comment/directive predicate the modifier base already used; reused, no copy.

### Architecture findings

None. Note for the lead: RH7101-006 is a condition of the fix on top of `ReihitsuFormatter.SplitsFieldDeclaration`, as the change doc decides. If thoenissen/Reihitsu#1384 gives `FieldDeclarationSplitTransform` a documentation guard, the fix condition and the transform guard would state the same rule twice (the skeptic's note); today they are not duplicates, because the transform still splits a documented declaration.

## thoenissen/Reihitsu#1340

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH7003…Analyzer.IsEligibleNamespace` | Returns false when the namespace has a `BaseNamespaceDeclarationSyntax` among its members | RH7003-005 |

## Whole run

### Tests run

```shell
scripts/build.sh --configuration Debug                     # Build succeeded, 0 warnings, 0 errors (after the last edit)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7003|FullyQualifiedName~RH7004Using|FullyQualifiedName~RH7101DoNot|FullyQualifiedName~RH7105|FullyQualifiedName~RH7106|FullyQualifiedName~RH7109|FullyQualifiedName~RH7303|FullyQualifiedName~RH7304|FullyQualifiedName~RH7306|FullyQualifiedName~RH7309|FullyQualifiedName~RH7501"
# before: Failed: 81, Passed: 299; after: every group passes
scripts/test.sh --configuration Debug                      # all projects except selfhosting
```

Full run on the formatted tree: Analyzer 6595/6595, Formatter 4517/4517, Core 607/607, CLI 251/251, Tooling 125/125, Architecture 115/115, Playground 17/17 — all passed. After the last edit (two Debug build warnings, RH1003 and S3267, removed by rewriting the code, no suppression) RH7309 (53/53) and Architecture (115/115) were run again and passed. `scripts/format.sh` was run over every changed file. The Release build was not run (cleaner).

### Disputed tests or scope

No test disputed.

Scope, together with the issue sections above: the handovers name 13 production files (the skeptic's surfaces); the run changes those 13, plus `SyntaxTriviaUtilities` and one new internal file (`RegionBlockLayout`) — under double. New public members in `Reihitsu.Core`: `SyntaxTriviaUtilities.SplitsDirectivePair`, `OrderingDeclarationUtilities.FieldOrderSetsLayout`, `IsLayoutField`, `ChangesLayoutOrder`, and `HasInitializer` made public. New internal members of the RH7306 analyzer for its fix. No adjacent fix was taken.

### Architecture findings

_None._

### Output questions

_None._ Every visible change is one the accepted change docs list.

### Follow-up candidates

- `FieldOrderSetsLayout` reads only the attribute lists of the declaration it is given. A `partial class` whose `[StructLayout(LayoutKind.Sequential)]` sits on another part is treated as having no layout, so RH7109 and RH7309 would still reorder its fields (for a partial struct, a missing `Auto`/`Explicit` on another part only withholds more). The base classes have no semantic model at the point of the check.
- RH7309: the fix is still offered for a reported type whose blocks are already in order (identity permutation) and then changes nothing — the shape of the test implementer's nested-type follow-up (FC-009, SHARED-FIX-005). Withholding it in `TryCreateReorder` is a one-line change but changes visible output outside the run's issues.
- RH7309: disabled text at member level is parsed as members on its own. A member that an `#if` splits across active and disabled text is not recognized, so its initializer is not counted.
- RH7304: the analyzer still reports a directive whose region's first token does not start its line (`/* c */ int _a;`); RH7304-001 does not claim that report, and the fix is now withheld for it.
