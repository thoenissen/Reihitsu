Role: implementer
Commit: 2fae9e7
Verdict: tests pass (one existing documentation-example test fails on the rule page; see *Disputed tests or scope*)

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Design/RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer.cs` (new) | The analyzer: resolves the limit once per compilation (default 15), registers on `MemberBodyUtilities.MemberKinds`, computes the complexity of the block body or the arrow expression, reports on the name token when the body has a nested structure and the complexity exceeds the limit. The member symbol (for recursion) is taken for methods and local functions only | RH2009-001, -002, -004, -006 |
| `src/Reihitsu.Analyzer/Core/CognitiveComplexityWalker.cs` (new) | `CSharpSyntaxWalker` that sums the increments: `if` chains and `?:` chains (parentheses around the false branch continue the chain) as one nesting construct; switch statement/expression once, sections/arms nested, case-label and arm pattern combinators not counted; loops and `catch` nest their body only; `goto` 1; logical-operator and pattern-combinator trees counted as sequences in source order (`&`, `|`, `^` only for built-in operators returning `bool`/`bool?`); recursion 1 once for an invocation whose symbol (normalized: reduced-from, original definition, partial definition part) is the member's; lambdas/anonymous methods nest their body without counting toward RH2009-004; local functions skipped. Disabled text is trivia and is never visited | RH2009-003, -004, -005, SHARED-DIAG-008 |
| `src/Reihitsu.Analyzer/Core/MemberBodyUtilities.cs` (new) | Decision 1: the one owner of member kinds → (block body, expression body, name token), moved out of RH2008 | RH2008-002, -004, -005 |
| `src/Reihitsu.Analyzer/Core/ConfiguredLimitUtilities.cs` (new) | Decision 2: the one owner of reading the `design` section, resolving a setting to its default, and the `>` comparison | SHARED-LIMIT-001, -002 |
| `src/Reihitsu.Analyzer/Rules/Design/RH2008MethodsShouldNotExceedTheLineLimitAnalyzer.cs` | Uses `MemberBodyUtilities` for its kinds and its body/name mapping (behavior unchanged) | RH2008-002, -004, -005 |
| `src/Reihitsu.Analyzer/Base/LineCountAnalyzerBase.cs` | `ResolveLimit`, the design-section read, and the comparison delegate to `ConfiguredLimitUtilities` (behavior unchanged) | SHARED-LIMIT-001, -002 |
| `src/Reihitsu.Analyzer/Data/ConfigurationCategoryDesign.cs`, `ConfigurationManager.cs` | New setting `maximumMethodComplexity` (`MaximumMethodComplexity`), parsed and validated like the line limits; `ParseLineLimit` renamed `ParseLimit` | RH0001-007 |
| `src/Reihitsu.Analyzer/AnalyzerResources.resx` | `RH2009Title`, `RH2009MessageFormat` | RH2009-006 |
| `src/Reihitsu.SelfHosting.Test/Utilities/AnalyzerViolationReport.cs` | `RH2009` on `_ignoredDiagnosticIds` | Maintainer answer 11 |
| `Release.ruleset` | `RH2009` with `Action="None"` | Decision 4 |
| `src/Reihitsu.Analyzer.Package/README.MD`, `Reihitsu.sln` | RH2009 row; `documentation/rules/RH2009.md` and `documentation/cognitive-complexity.md` as solution items | Implementer step 5 |
| `sdd/specs/analyzer/RH2008.md`, `RH2009.md`, `shared/configured-limits.md`, `sdd/specs/README.md` | Code owners: `MemberBodyUtilities` (RH2008), `CognitiveComplexityWalker` (RH2009), `ConfiguredLimitUtilities` (SHARED-LIMIT), headers and index rows | 04-spec-author *Internal choices* |

## Policy owners

- Member → body mapping: `rg -n "MethodDeclarationSyntax method => \(method.Body" src` → only `Core/MemberBodyUtilities.cs:54`.
- Limit resolution and comparison: `rg -n "count > limit|\?\? _default|Configuration\?\.Design" src` (non-test, no bin/obj) → only `Core/ConfiguredLimitUtilities.cs:22`.
- Cognitive complexity: new; no other owner exists.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 10 files changed, 895 insertions(+), 46 deletions(-) — of which the walker is 634 lines, the analyzer 99. New types: `RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer`, `CognitiveComplexityWalker` (the issue's own implementation), `MemberBodyUtilities`, `ConfiguredLimitUtilities` (decisions 1 and 2).

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests"` — Passed 174, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH0001ConfigurationFileMustBeValidAnalyzerTests|FullyQualifiedName~RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests|FullyQualifiedName~RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests"` — Passed 274, Failed 0.
- `scripts/test.sh --configuration Debug --project selfhosting --filter "FullyQualifiedName~AnalyzerViolationReportTests|FullyQualifiedName~RulesetTests"` — Passed 34, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed 198, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed 11825, Failed 1: `RuleDocumentationExampleTests.EveryNonOptedOutDocumentPassesVerification` (below).
- `scripts/build.sh` (Release) — 0 warnings, 0 errors.
- The full self-hosting scan (`AnalyzerSelfHostingTests`) was not run locally; it is outside the default run and did not finish within 10 minutes here.

## Disputed tests or scope

- **Rule page example does not compile.** `RuleDocumentationExampleTests.EveryNonOptedOutDocumentPassesVerification` fails: "RH2009 (documentation/rules/RH2009.md) [ExampleDoesNotParse]: Violation example does not parse as a compilation unit: (2,1): error CS0106: The modifier 'public' is not valid for this item". The Violation (and Correction) examples are bare methods with `public`/`private` modifiers outside a type. The test is unchanged infrastructure and the page is the spec author's, so I changed neither. Two ways out for the lead: the spec author wraps both examples in a class (the violation then reports 16 against 15 and needs no opt-out, as 07 expected), or an opt-out in `RuleDocumentationExampleOptOuts` (test implementer). Wrapping keeps the example verified.
- **Recursion receiver.** RH2009-003 says an invocation whose target is the member's own symbol, "written as `M(…)`, `this.M(…)`, or `C.M(…)`". I count every invocation that binds to the member's symbol, also on another instance (`other.M(…)`); no test covers that form. If the list is meant to be exhaustive, the walker needs a receiver check (a few lines); the spec author should say which reading holds.
- Lambda parameter default values are scored at the level where the lambda is written, not inside its body; no spec clause or test covers them.

## Architecture findings

_None._

## Follow-up candidates

_None._
