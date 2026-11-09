Role: implementer
Commit: f15f600 (base); changes committed with this handover
Verdict: #1597: tests pass; #1598: nothing to do; #1016: nothing to do; #1015: nothing to do; #1084: nothing to do

Repair round 1, lead decisions 13, 16, 17 (auditor finding 1, SHARED-CASING-012 as rewritten in 2499cbe). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1597

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `HasCallerArgumentExpressionNaming(cascade, names, identifier, …)`: groups the cascade's parameters by their containing symbol; inspects each owner's parameter list (method, property, or the receiver list of an extension block) with, as exempt identifiers, the names of the cascade parameters that list declares other than the new name; for an extension block owner (`INamedTypeSymbol { IsExtension: true }`) also inspects every member's parameter list with no exempt identifier. New overload over one parameter list holds the attribute test. `IsNameofArgument` → `IsExemptNameofArgument`: exempt only when the single argument is `nameof(<IdentifierName>)`, the identifier is in the exempt set, and the attribute's tree has `CSharpParseOptions.LanguageVersion >= CSharp11`. New `GetParameters(ISymbol)` (method, property, extension receiver). The call site passes the new name. | SHARED-CASING-012, RH4105-003, RH4120-003 |

The check stays syntactic plus constant values: argument shape, identifier text, and the tree's language version; no binding of the `nameof` operand. XML summaries and remarks of the changed methods rewritten to the new behavior (FC-031).

### Policy owners
- `rg -n "CallerArgumentExpression" src` (production hits): only `CasingCodeFixProviderBase{T}.cs` (`HasCallerArgumentExpressionNaming` ×2, `IsExemptNameofArgument`). One owner.
- `rg -n "IsExtension|ExtensionParameter|LanguageVersion.CSharp11" src` (production hits): `CasingCodeFixProviderBase{T}.cs` (new) and `RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.cs:68` (same parse-options pattern, a different policy). No private copy of this policy.

### Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this round: 28 files changed, 171 insertions(+), 86 deletions(-).
- Delta of this round: 1 file changed, 79 insertions(+), 23 deletions(-). No new type, no new public member (three private static methods: one new overload, one renamed and widened, `GetParameters`).

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4105MethodParameterCasingAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4120|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4114"` → Passed 86, Failed 0 (the five red tests of `12-test-implementer.md` now green).
- `scripts/test.sh --configuration Debug --project analyzer` → Passed 7849, Failed 0. (A first run reported 7546 after the kernel killed a process for lack of memory; the rerun gave the full count.)
- `scripts/test.sh --configuration Debug --project architecture` → Passed 117, Failed 0.
- `scripts/format.sh` on the changed file: 0 changes.

### Disputed tests or scope
_None._ The C# 10 test (example d) passes by the language-version condition; per decision 17 the spec author rewords the *Why*, not the code.

The extension block's own receiver list is inspected with the cascade's exemption (a `CallerArgumentExpression` on the receiver itself naming the receiver by unqualified `nameof`); no test covers that list, and the spec's first bullet ("a parameter list that declares a symbol of the cascade") includes it. Not tested because a receiver carrying `CallerArgumentExpression` is exotic; noted for the auditor.

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1598

Nothing to do in this round.

## thoenissen/Reihitsu#1016

Nothing to do in this round.

## thoenissen/Reihitsu#1015

Nothing to do in this round.

## thoenissen/Reihitsu#1084

Nothing to do in this round.

## Whole run

- Build: `scripts/build.sh --configuration Debug` → Build succeeded, no warnings in the changed project.
- Analyzer 7849/0, architecture 117/0 (Debug).
