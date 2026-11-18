Role: implementer
Commit: 08a771a
Verdict: #849: test disputed (one test; all others pass); #850: tests pass

My prompt named the issues, the slug, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, handovers 02 to 05, the change doc, and the specs. Q6 A (one `Reihitsu.Core` owner) is carried out.

## thoenissen/Reihitsu#849

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/GetOnlyMemberExpressionBodyRewriteUtilities.cs` (new) | Holds the shape predicate `HasConvertibleShape` (analyzer), the conversion `TryConvertToExpressionBody` (formatter and fix), `ConvertAccessorToExpressionBody` (accessor-level conversion), and `ContainsBlockingDirective`. Every private helper moved here unchanged from the two transforms: seams, rehost, semicolon trivia, throw parentheses, and convertible statement. The language gates (C# 6, and C# 7 for throw) are one helper, `IsAvailable` | SHARED-GETONLY-001, -002, -005; FMT-STRUCT-009 to -015 unchanged |
| `src/Reihitsu.Formatter/.../Rewriter/GetOnlyMemberExpressionBodyTransform.cs` | Now a thin visitor that calls `TryConvertToExpressionBody` with `FormattingContext.LanguageVersion` and `EndOfLine`. The property-initializer check moved into Core (`TryGetGetter`) | FMT-STRUCT-014, -015 |
| `src/Reihitsu.Formatter/.../Rewriter/AccessorExpressionBodyTransform.cs` | Now a thin visitor that calls `ConvertAccessorToExpressionBody` | FMT-STRUCT-009 to -013 |
| `src/Reihitsu.Analyzer/Base/GetOnlyMemberExpressionBodyAnalyzerBase.cs` (new) | Reports `member.AccessorList` when `HasConvertibleShape(member, parseOptions.LanguageVersion)` holds. Comments, directives, and disabled text are not consulted | SHARED-GETONLY-001 to -004, -007 |
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer.cs` (new) | Registers for `PropertyDeclaration` | RH3209-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/GetOnlyMemberExpressionBodyCodeFixProviderBase.cs` (new) | Registers the fix only when three checks pass: `TryConvertToExpressionBody` succeeds, `FindUnsupportedSyntax(getter as written)` is null, and `FindUnsupportedSyntax(converted member)` is null. To apply, it replaces the member with the converted one (annotated) and runs `FormatNodeInDocumentAsync` on that member. From the result it splices onto the member as written only the formatted arrow clause, the semicolon, and the trailing trivia of the token in front of `=>`. This is the RH3202 `ConvertBodyAsync` precedent (decision 4). Fix All uses `BatchFixer`; `FormatsThroughFormatter` is true | SHARED-GETONLY-003, -005, -006, -007; SHARED-FIX-003, -006, -010, -011 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3209…CodeFixProvider.cs` (new) | Supplies the ID and title | RH3209-001 |
| `AnalyzerResources.resx/.cs`, `CodeFixResources.resx/.cs` | Adds `RH3209Title` and `RH3209MessageFormat` ("Get-only properties should use an expression body."), and the code-fix title "Convert to expression body" | — |
| `Reihitsu.sln`, `Release.ruleset`, `src/Reihitsu.Analyzer.Package/README.MD` | Adds the rule page item, `Action="Error"`, and a README row with ✔ for analyzer, code fix, and formatter | — |
| `AGENTS.md` | The phase-table guard `AccessorExpressionBodyTransform.ContainsBlockingDirective` now reads `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective`. `PhaseOwnershipDocumentationTests.EveryGuardNameInOwnershipMapResolves` requires the name to resolve. See *Disputed tests or scope* | — |

The rule pages already existed (spec author), so I left them unchanged.

### Policy owners

- `rg -n "AccessorExpressionBodyTransform\.|GetOnlyMemberExpressionBodyTransform\." src` found no caller outside the two transforms before the move. Nothing in production now calls the moved members except the Core type. The transforms, both analyzers, and both fixes reach the rule only through `GetOnlyMemberExpressionBodyRewriteUtilities`.
- Trailing-whitespace trim: `rg -n "RemoveTrailingWhitespace|TrimTrailingWhitespace" src` finds four existing copies:
  - `TriviaListUtilities.RemoveTrailingWhitespace` (Formatter, internal)
  - `LineBreakTriviaUtilities.RemoveTrailingWhitespace` (Formatter)
  - `DeclarationModifierUtilities.TrimTrailingWhitespace` (Core, private)
  - `BlankLineCodeFixUtilities.TrimTrailingWhitespace` (CodeFixes, private)

  The moved `RemoveLineBreaks` called the formatter's internal copy, which Core cannot reach. It now inlines the four-line loop. That leaves a fifth copy of a one-line policy, recorded under *Follow-up candidates*.
- Unsupported-shape check: `ReihitsuFormatter.FindUnsupportedSyntax`, reused as is.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 14 files changed, 1051 insertions(+), 497 deletions(-). The count covers both issues.

- New types: `GetOnlyMemberExpressionBodyRewriteUtilities` (Core, about 540 lines, of which about 400 were moved from the transforms), `GetOnlyMemberExpressionBodyAnalyzerBase`, `GetOnlyMemberExpressionBodyCodeFixProviderBase`, two analyzers, and two code-fix providers.
- New public members: in Core, `HasConvertibleShape`, `TryConvertToExpressionBody`, `ConvertAccessorToExpressionBody`, and `ContainsBlockingDirective`.

### Deleted special cases

_N/A._ No shape is declared unsupported (Q5).

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3209|FullyQualifiedName~RH3210"`: Failed 1, Passed 79, Total 80. The one failure is the disputed test below.
- `scripts/test.sh --configuration Debug --project analyzer`: Failed 1, Passed 11011, Total 11012. The failure is the same test.
- `scripts/test.sh --configuration Debug --project formatter`: Passed 5908 of 5908. This includes `GetOnlyMemberExpressionBodyTests` and `EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests`, both unchanged.
- `scripts/test.sh --configuration Debug --project core`: Passed 738 of 738.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198. `EveryCodeOwnerNamesAnExistingType`, `AnalyzerSpecHeaderMatchesItsRule`, and `EveryRequirementOfAnEnforcedSpecIsCovered` now pass.
- `scripts/build.sh --configuration Debug`: 0 errors. The only warnings were RH5603 (final newline) on the new files. `scripts/format.sh` then corrected those files.

### Disputed tests or scope

- **`RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests.FixKeepsCarriageReturnLineFeedLineEndings`** fails. The test is wrong; the code is not.
  - The source has two diagnostics, `A` and `B`. The expected text has both converted.
  - `VerifyCodeFixWithEndOfLine` first runs `Verify(sourceWithEndOfLine, fixedSourceWithEndOfLine)`, which applies fixes iteratively. That part passes, so the CRLF result with both members converted is correct.
  - It then asserts `Assert.AreEqual(fixedSourceWithEndOfLine, ApplyCodeFixAsync(plainSource))`. `ApplyCodeFixAsync` applies the first action for the first diagnostic only, so it returns `A` converted and `B` unchanged: `…public int A => _value;\r\n\r\n    public int B\r\n    {\r\n        get => _value; // b\r\n    }\r\n}`.
  - The test's expected value would need a helper that applies every fix, or a source with one diagnostic. The RH3210 CRLF test has one diagnostic and passes.
  - Route to the test implementer. One option is to split `B` (the comment behind the getter's semicolon under CRLF) into a CRLF test of its own.
- **`AGENTS.md`, one guard name in the phase table** (outside the production paths). Moving `ContainsBlockingDirective` into Core made the old name dangle, and `PhaseOwnershipDocumentationTests` failed on it. I changed only that one name. The lead may route the text to the spec author for review.
- **Scope growth** (no new rule):
  - The Core type also owns the accessor-level conversion for `set` and `init` (`ConvertAccessorToExpressionBody`). The get-only conversion is defined in terms of it (SHARED-GETONLY-001: "a block that FMT-STRUCT-009 to FMT-STRUCT-013 convert").
  - The new type is public, as Core types are.
  - Analyzer and code-fix base classes were added so that the two rules share one call site. 05-test-implementer asked for that under *Not covered* for #850.
- **The fix keeps the signature but aligns as the formatter would.** When the signature's spacing differs from the formatter's (`public  int  A`) and the getter's expression spans several lines, the continuation lines keep the columns the formatter computed for its own signature. The splice keeps the signature as written (SHARED-GETONLY-005, Q4 A), so those lines can end up off by the difference. The RH3202 `ConvertBodyAsync` fix shares this. No test covers the combination, and I wrote no special case for it. This is a cheap-check boundary, not a destructive one.

### Architecture findings

_None._ The shared owner (Q6 A) closes the skeptic's finding: the formatter, both analyzers, and both fixes call one type.

### Follow-up candidates

- There are now five private or internal copies of "remove trailing whitespace trivia": `TriviaListUtilities.RemoveTrailingWhitespace`, `LineBreakTriviaUtilities.RemoveTrailingWhitespace`, `DeclarationModifierUtilities.TrimTrailingWhitespace`, `BlankLineCodeFixUtilities.TrimTrailingWhitespace`, and the loop in `GetOnlyMemberExpressionBodyRewriteUtilities.RemoveLineBreaks`. One public Core owner, such as `SyntaxTriviaUtilities`, could replace them. Small, internal, and no output change.

## thoenissen/Reihitsu#850

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer.cs` (new) | Registers for `IndexerDeclaration` on the shared base | RH3210-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3210GetOnlyIndexersShouldUseAnExpressionBodyCodeFixProvider.cs` (new) | Supplies the ID and title on the shared base | RH3210-001 |
| Resources, `Reihitsu.sln`, `Release.ruleset`, package README | Same entries as RH3209, with "Get-only indexers should use an expression body." | — |

Everything else is shared with #849: the Core type, both bases, and the formatter transforms.

### Policy owners

As under #849.

### Production diff size

Included in the #849 figure.

### Deleted special cases

_N/A._

### Tests run

All RH3210 tests pass: they are in the filtered run above, 80 tests with the one RH3209 failure.

### Disputed tests or scope

_None_ beyond those under #849.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Commit 08a771a holds the production changes. The handover is committed separately. Nothing is pushed.
- Lane check: not run; it belongs to the lead.
