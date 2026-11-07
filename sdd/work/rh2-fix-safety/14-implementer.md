Role: implementer
Commit: 2f1fb5a
Verdict: #967: tests pass; #1286: tests pass; #1287: tests pass; #1288: nothing to do

Repair round 1, for audit findings 1, 2a, 2b, 3, 4a, and 4b of `10-auditor.md`, carried out along decisions 16–18 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the recorded lead decisions, which were followed. No test was changed.

## thoenissen/Reihitsu#967

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DeclarationModifierUtilities.cs` | `AddAccessibilityModifiers(MemberDeclarationSyntax, …)` now first runs the new private `CarryLeadingTriviaOfLaterAccessibilityModifiers`: the leading trivia of every accessibility modifier that is not the first modifier moves onto the next kept token (the next non-accessibility modifier, or the token behind the modifier list). A line such a modifier started therefore keeps its place, and the following token stands on it in the modifier's column (finding 1, decision 18). New public `HasCommentOrDirectiveInFrontOfLaterAccessibilityModifier(SyntaxTokenList)`: a comment, directive, or disabled text in that trivia, judged with the shared `SyntaxTriviaUtilities.IsCommentTrivia` / `IsDirectiveOrDisabledTextTrivia`. Remarks of the member overload updated. | RH2005-003, RH2005-005, SHARED-FIX-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2005FieldsMustBePrivateCodeFixProvider.cs` | The fix is not registered when `HasCommentOrDirectiveInFrontOfLaterAccessibilityModifier(fieldDeclaration.Modifiers)` holds. | RH2005-005 |

Observed: `static⏎        public int Counter;` gives `private static⏎        int Counter;` (LF and CRLF). The comment, `#pragma`, and `#if NEVER … #endif` lines in front of a later `public` withhold the fix. RH2004 is unaffected, because it reports only declarations without an accessibility keyword. The carry still keeps such a comment rather than dropping it if a future caller reaches the helper without the RH2005 check.

### Policy owners
- `rg -n "AddAccessibilityModifiers?\(|IsAccessibilityModifier\(" src --glob '!*Test*'`: `DeclarationModifierUtilities` only (production callers: the RH2004 and RH2005 fixes). The new withholding check reuses its private `IsAccessibilityModifier`, so "which modifiers the helper removes" keeps one owner.

### Tests run
See *Whole run*. The RH2005 tests of rounds 11 and 13 (`VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifier` (+CRLF), the five `VerifyNoFixIsOfferedFor…InFrontOfAPublicBehindAnotherModifier` tests) pass.

### Disputed tests or scope
- Scope: one new public member in `Reihitsu.Core` (`HasCommentOrDirectiveInFrontOfLaterAccessibilityModifier`). It lives there so that the set of removed modifiers has one owner; the alternative was a copy of the accessibility-kind test in the RH2005 fix.

### Architecture findings
_None._

### Follow-up candidates
- By trace, not constructed: the token-list overload still gives the new modifier the trailing trivia of the removed one. So `static public /* c */ int` (a comment behind a non-first `public` on its line, outside RH2005-005) would move the comment in front of `static`. This is pre-existing and outside the four issues.

## thoenissen/Reihitsu#1286

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | Decision 17. `HasCommentOrDirectiveInsideDeclaration` (the separate list of positions) is removed. `TryCreateFieldDeclaration` now builds the field and accepts it only when the new `KeepsEveryCommentAndDirective` holds: the ordered texts of every comment, directive, and disabled-text trivia of the property equal those of the built field (`GetCommentsAndDirectives`, through the shared `SyntaxTriviaUtilities` predicates, finding 3). The builder now also drops the leading trivia of the initializer's `=`, because the gap between the name and `=` held the accessor list, which the field does not have. Together with the new identifier and the new `;` it already wrote, this covers every RH2001-007 place: identifier leading (2b) and trailing, the accessor list, the `}`/`=` gap, and the `;`'s leading trivia (2a). `IsSupportedPropertyShape` keeps only the shape checks, and `RegisterCodeFixesAsync` calls `TryCreateFieldDeclaration`. | RH2001-007, SHARED-FIX-003 |

Before formatting, the field text for the common `{ get; set; } = 5` was already `_value= 5`, because `=` had empty leading trivia. Dropping the `=` leading trivia therefore changes nothing there. The `;`-on-its-own-line and name-on-its-own-line tests (decision 21) still give `private int _value = 5;` and `private int⏎    _value;`.

### Policy owners
- `rg -n "DisabledTextTrivia|SingleLineCommentTrivia" src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH200*.cs`: no private trivia-kind list remains in RH2001. The one hit is the call to `SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia`.
- What the field loses is now stated only by the builder. The guard compares the builder's output with its input, so no second statement of the positions exists.

### Tests run
See *Whole run*. All RH2001 tests pass: the five `…InFrontOfTheSemicolon` tests, the five `…InFrontOfTheName` tests, both `…WithoutCommentOrDirective` pairs, and the two multi-outcome tests of decision 22.

### Disputed tests or scope
_None._

### Architecture findings
_None._ Decision 17 is carried out.

### Follow-up candidates
_None._ The ordered comparison would also withhold the fix if the RH7105 reorder ever put two comments between modifiers out of order. That is the follow-up candidate 4 shape, and no test reaches it.

## thoenissen/Reihitsu#1287

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/PartialMemberUtilities.cs` | Decision 16. `GetReportedPart` no longer predicts the driver: the constructor special case (`analyzingPart`) is gone. It returns the declaration when it is the part `SelectReportedPart` chooses from all parts, otherwise `null`. Its remarks state the contract: callers analyze every declaration of the kind outside generated code, and a caller whose analysis does not reach every part decides from the containing type with `TryGetParts` and `SelectReportedPart`. New `GetStandInSymbol(symbol, compilation, ct)` returns the defining part's symbol for an implementing part whose defining part is in generated code, and the symbol as it is otherwise. The compiler binds facts such as the implemented interface member to the defining part only. | SHARED-DIAG-012 |
| `src/Reihitsu.Analyzer/Rules/Design/RH2004AccessModifierMustBeDeclaredAnalyzer.cs` | A member declared in two parts is no longer reported from the declaration node action. A new `SymbolKind.NamedType` action (`OnNamedType`) walks the type's members once, gets both parts through `TryGetParts` (deduplicated by defining part, `ShouldSkip` on the defining part), and reports on `SelectReportedPart`'s choice when that part states no accessibility. The type symbol sees the defining part of a partial constructor, which no declaration analysis reaches (finding 4a). A `partial` declaration with only one part is still reported by the node action, as before. | RH2004-001, SHARED-DIAG-004, SHARED-DIAG-012 |
| `src/Reihitsu.Analyzer/Core/InterfaceImplementationUtilities.cs` | `GetInterfaceRegionName(symbol, compilation, ct)` judges the member on `GetStandInSymbol`. | RH7409-001–RH7412-001, SHARED-DIAG-012 |
| `RH7409…RH7412…Analyzer.cs` (4 files) | Pass `semanticModel.Compilation` and the token to `GetInterfaceRegionName`. | same |

How the new contract covers each consumer:
- RH74 bases: the type-declaration node action runs for every type declaration outside generated code.
- RH8306: the property node action runs for both parts.
- RH2002: the method symbol action runs for both parts.

All three call `GetReportedPart` from the analysis of each part. The RH2004 constructor was the one case where no analysis reaches every part, and it now decides from the type.

### Policy owners
- `rg -n "PartialDefinitionPart|PartialImplementationPart" src --glob '!*Test*'`: `PartialMemberUtilities` only. `GetStandInSymbol` is the single place that maps a part to the symbol facts are judged on.
- `rg -n "GetInterfaceRegionName" src`: RH7409–RH7412 only. `DocumentationAnalysisUtilities.OverridesOrImplementsMember` calls `GetImplementedInterfaceName` directly and is unchanged.

### Tests run
See *Whole run*. The RH2004 constructor and RH7409–RH7412 generated-defining-part tests pass, and so does every other partial-member cell of round 11.

### Disputed tests or scope
- **Why the stand-in is conditional.** Judging every implementing part as the member, the unconditional form the auditor offered, broke two passing characterization tests from `main`: `RH7409…Tests.VerifyPartialImplementationMethodIsJudgedAtItsDefiningPart` and `RH7410…Tests.VerifyPartialImplementationPropertyIsJudgedAtItsDefiningPart`. In each, one part lies in region `I` and the other outside it. That is the only-some-parts case that RH7409-001 does not cover (#1334's question). The tests pin "judged at the defining part", and the unconditional form added a second diagnostic on the implementing part. Following SHARED-DIAG-012's fallback order, the implementing part therefore stands for the member only when the defining part is in generated code, which keeps both characterizations and fixes 4b. If #1334 decides the only-some-parts case the other way, the condition in `GetStandInSymbol` is the one place to change.
- RH2002, RH8306, and the RH74 bases still report from per-part analyses, with the choice made from all parts. Only RH2004 moved to a per-type-symbol action. The RH74 bases' per-part facts need a semantic model of each part's tree, and a symbol action could get one only through `Compilation.GetSemanticModel` (RS1030). Moving them would also mean rewriting the twelve subclass hooks to be symbol-based. The lead may judge whether this meets "once per symbol" in decision 16.

### Architecture findings
_None new._ Decision 16 is carried out as described above.

### Follow-up candidates
- By trace, not constructed: `DocumentationAnalysisUtilities.OverridesOrImplementsMember` (RH8201 and the other documentation rules) judges an implementing part of a partial member on its own symbol. So an implementing part of a partial method that implements an interface member reads as "implements nothing". This is pre-existing and outside the four issues.

## thoenissen/Reihitsu#1288

Nothing to do: the audit has no finding for it.

## Whole run

```shell
scripts/build.sh --configuration Debug                       # 0 errors; one RH5206 warning before format, gone after
scripts/format.sh <the 10 changed .cs files>                  # formatted 1 of 10 (PartialMemberUtilities.cs)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001|…RH2002|…RH2004|…RH2005|FullyQualifiedName~RH8306|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH74"
                                                              # Passed 389, Failed 0
scripts/test.sh --configuration Debug --project analyzer      # Passed 7153, Failed 0
scripts/test.sh --configuration Debug --project core          # Passed 649, Failed 0
scripts/test.sh --project architecture                        # Passed 115, Failed 0 (Release build, no warnings)
```
