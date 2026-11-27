Role: implementer
Commit: 9808734b
Verdict: test disputed (two tests; every other handed-over test passes)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (answers 1B, 2A, 3B, 4A; decisions 2 and 3 carried out here), `01-spec-author.md` to `04-test-implementer.md`, the specs RH4105, RH4115, RH4117, RH4128, `shared/casing.md` (SHARED-CASING-015), and both change docs. No test assertion, input, or expected output was changed. Two tests fail only because of how they are written; see *Disputed tests or scope* under *Whole run*.

## thoenissen/Reihitsu#1087

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DeconstructionVariableUtilities.cs` (new) | `IsDeconstructionVariable(SingleVariableDesignationSyntax)`: the one statement of the boundary (decision 2). True for a variable of a parenthesized designation, a declaration expression that is an element of a tuple expression, and a variable whose pattern is the pattern of a positional subpattern, reached only through `and` patterns and parenthesized patterns (answer 1B) | RH4117-001, RH4117-003, RH4115-002 |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4117DeconstructionVariableCasingAnalyzer.cs` | Registers `SingleVariableDesignation` instead of `DeclarationExpression` and reports exactly what the helper includes | RH4117-001, RH4117-003 |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4115LocalVariableCasingAnalyzer.cs` | Excludes what the helper includes, skips `const` local declarations, and skips every identifier named `_` (declarator, `for`, `foreach`, `using`, `fixed`, `catch`, designation) | RH4115-002, RH4115-004, RH4115-005 |

The RH4115 and RH4117 code fixes are unchanged; they already take the node types of the new positions. SHARED-CASING-015 for their fixes is under *Whole run*.

### Tests run

`Naming` filter: every #1087 test in the table of `04-test-implementer.md` passes, except `RH4115…Tests.FixKeepsTheLayoutAndCommentsOfAMultiLineAnonymousObjectMember` (LF and CRLF), disputed under *Whole run*.

## thoenissen/Reihitsu#1088

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4128LocalConstantCasingAnalyzer.cs` (new) | Registers `LocalDeclarationStatement`; reports every variable of a `const` declaration that is not camelCase, except `_` | RH4128-001, RH4128-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/RH4128LocalConstantCasingCodeFixProvider.cs` (new) | `CasingCodeFixProviderBase<VariableDeclaratorSyntax>`, camelCase conversion, Fix All from the base | RH4128-003 |
| `src/Reihitsu.Analyzer/AnalyzerResources.resx` | `RH4128Title`, `RH4128MessageFormat`: *Local constant names should be in camelCase* (equal to the rule page heading and the package README row the spec author wrote) | RH4128 |
| `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx`, `CodeFixResources.cs` | `RH4128Title` "Fix casing" and its wrapper property | RH4128-003 |
| `Release.ruleset` | `RH4128` as `Error`, next to RH4127 | — |
| `Reihitsu.sln` | `documentation\rules\RH4128.md` solution item | — |

### Tests run

`RH4128LocalConstantCasingAnalyzerTests`: all 12 pass (compiled once the two types existed). `AnalyzerPackageMetadataTests`, `AnalyzerResourcesTests`, `CodeFixResourcesTests` pass within the full analyzer run.

## thoenissen/Reihitsu#1978

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/RangeVariableDeclarationUtilities.cs` (new) | `DeclarationKinds` and `GetIdentifier(SyntaxNode)`: which query syntax declares a range variable and with which identifier. Before this run, RH4124 held the only copy; RH4105's analyzer and fix would have added two more (FC-015) | RH4105-005 |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer.cs` | Reads the kinds and the identifier from the helper instead of its own switch; no behavior change (all RH4124 tests pass) | RH4124-003 (unchanged) |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4105MethodParameterCasingAnalyzer.cs` | Registers the range-variable kinds as well; `_` is skipped for both; the class summary quotes the new title (FC-031) | RH4105-002, RH4105-005 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/RH4105MethodParameterCasingCodeFixProvider.cs` | `CasingCodeFixProviderBase<SyntaxNode>` (was `ParameterSyntax`); identifier of a parameter or of a range-variable declaration | RH4105-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` (`GetDeclaredSymbol`) | Resolves the declared symbol of a query clause, a `join … into`, and a query continuation, so the existing guards and the renamer handle range variables | RH4105-006 |

### Tests run

Every #1978 test in `04-test-implementer.md` passes, except `RangeVariablesWithTheSameNewNameInOneQueryAreNotFixed`, disputed under *Whole run*.

## Whole run

### Changes — SHARED-CASING-015 (decision 3, answers 3B and 4A)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingAnonymousMemberUtilities.cs` (new) | `NameAnonymousObjectMembersAsync`: for every reference of the declared symbol that the reference search reports (the same search the renamer and the cascade use), it finds an anonymous object member without `Name =` whose whole expression is the reference, or a member access, member binding, or conditional access ending in it, and writes `OldName = ` in front of the expression. The expression's leading trivia moves in front of the name; nothing else moves | SHARED-CASING-015 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `PrepareNamedRenameAsync`: writes the names first, then resolves the symbol and the cascade again in that solution. `RenameWithoutConflictAsync` runs the renamer and every existing safety check against the solution with the names written, so the added `Name = ` counts as baseline and not as a change the rename made. `ApplyCodeFixAsync` now returns the result of the same checked rename (`RenameDeclarationAsync`, which replaces `HasRenameConflictAsync`) instead of calling the renamer directly; Fix All already used it. Summaries updated (FC-031) | SHARED-CASING-015, SHARED-CASING-007 |

This covers every provider derived from the base, so RH4101, RH4102, RH4105, RH4106 to RH4115, RH4117, RH4120, and RH4128 alike. The fix that was withheld because the member is read by its old name (`RH4106…NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField`, replaced by the test implementer) is now offered, because the renamed result compiles.

### Policy owners

- Deconstruction boundary: `rg -n "ParenthesizedVariableDesignationSyntax|PositionalPatternClauseSyntax" src` gives `DeconstructionVariableUtilities` (the only analyzer owner) and formatter spacing, blank-line, and indentation sites, which make layout decisions unrelated to the boundary.
- Range-variable declarations: `rg -n "QueryContinuationSyntax" src` gives `RangeVariableDeclarationUtilities` and `CasingCodeFixProviderBase{T}.GetDeclaredSymbol`. The latter is a symbol lookup per node type, the same as every other case in that switch.
- Anonymous member names: `rg -n "AnonymousObjectMemberDeclaratorSyntax|TryGetInferredMemberName" src` gives `CasingAnonymousMemberUtilities` (writes the name, used by the casing fixes), `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` (detects an inferred name to withhold its fix, RH2001-011; the follow-up candidate in `00-lead.md`), `RH3002` (whether parentheses are needed), and formatter layout sites. None of them makes the casing decision.
- `const` locals: `IsConst` is tested in RH4115 (`false`) and RH4128 (`true`); this is the one-modifier boundary the skeptic judged not worth a helper.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src`: 15 files changed, 494 insertions(+), 84 deletions(-). This includes the spec author's resource and package README edits. New types: `DeconstructionVariableUtilities`, `RangeVariableDeclarationUtilities`, `CasingAnonymousMemberUtilities` (internal static helpers), `RH4128LocalConstantCasingAnalyzer`, `RH4128LocalConstantCasingCodeFixProvider` (the new rule, named by the spec).

### Deleted special cases

_N/A._

### Tests run

- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project analyzer` (before the range-variable helper refactor): `Failed: 3, Passed: 12344, Total: 12347`, all three disputed below.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` (final code): `Failed: 3, Passed: 999, Total: 1002`, the same three.
- `scripts/test.sh --configuration Debug --project architecture` (final code): `Passed: 198, Failed: 0`. The three earlier failures (`AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType`, `EveryRequirementOfAnEnforcedSpecIsCovered`) are gone.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Not run: `Reihitsu.SelfHosting.Test` (outside the default run). A search of `src` for local constants and range variables not in camelCase found none; all hits in `tests` are inside raw-string fixtures.

### Disputed tests or scope

1. **`RH4115LocalVariableCasingAnalyzerTests.FixKeepsTheLayoutAndCommentsOfAMultiLineAnonymousObjectMember` (LF, CRLF).** Its `Verify(source, fixed)` part passes. It fails at the second assertion of `VerifyCodeFixWithEndOfLine`, which applies **only the first offered action** (`TryApplyCodeFixAsync` takes `actions[0]` for the first diagnostic) and compares the result with the fully fixed source. The source has two diagnostics, `Total` and `Count`, so one action cannot give `var count` and `Count = count`. Actual result of the one action: `var total …; var Count = 1; … // The sum / Total = total, // trailing / Count /* last */`. The comments and layout match what the test expects for the member that was renamed. The test needs one diagnostic in its source, or an expected text for the first action. The spec (SHARED-CASING-015) and the production result agree; only the harness use is wrong.
2. **`RH4105MethodParameterCasingAnalyzerTests.RangeVariablesWithTheSameNewNameInOneQueryAreNotFixed`.** The single fix and the Fix All result are both as the test expects. The testing framework fails with "No code fixes were expected, but a fix was offered that made no changes to the code": because `BatchFixedCode` equals the source, it expects no Fix All action, while the shared Fix All provider always offers one (it only learns when applied that SHARED-CASING-010 skips every candidate). As an experiment I added `test.NumberOfFixAllIterations = -1;` to the test locally. The test then passed, and I restored the file byte for byte (not committed). `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField` uses the same kind of setting (`-2`) for the same harness behavior. Changing production so Fix All offers no action when every candidate is skipped would require running the whole Fix All while the action is being built. That is not specified, so I did not do it.
3. **Scope:** three new internal helper types, listed above. `RangeVariableDeclarationUtilities` also touches RH4124, which is outside the issues' rules, with no behavior change. I did it so that the run does not add two more copies of RH4124's clause switch (FC-015). If the lead prefers, it can be reverted to private copies in RH4105.

### Architecture findings

_None._ Decision 2 (one deconstruction boundary) and decision 3 (SHARED-CASING-015 in the shared base) are carried out as described above.

### Follow-up candidates

_None_ new. The RH2001-011 candidate in `00-lead.md` stands: `RH2001…CodeFixProvider` could reuse `CasingAnonymousMemberUtilities` instead of withholding.
