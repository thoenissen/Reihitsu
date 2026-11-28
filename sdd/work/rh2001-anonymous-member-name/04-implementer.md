Role: implementer
Commit: a13a255 (production changes; this handover is the next commit)
Verdict: blocked

The prompt carried no intended fix, cause, or hoped-for verdict. Lead decision 7 (Fix All on row 18) is the point this handover stops at: 2 of the 23 handed-over tests stay red, both Fix All, both caused by `WellKnownFixAllProviders.BatchFixer`'s merge of text changes, and the two existing ways around it do not fit (see *Disputed tests or scope*). The lead decides.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `RenamePropertyToFieldNameAsync` calls `CasingAnonymousMemberUtilities.NameAnonymousObjectMembersAsync` before `Renamer.RenameSymbolAsync`, re-resolves the annotated property in the named solution, and renames there. It is the one rename path of both the registration check (`RenameChangesBindingAsync`) and the applied fix, so the check judges the result the fix ships. `GivesInferredMemberNameAsync` lost its anonymous-object branch (lead decision 4, FC-015) and is now `GivesInferredTupleElementNameAsync`, tuple elements only (RH2001-011 unchanged). XML summaries and remarks of the three methods updated. | RH2001-012, RH2001-011 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingAnonymousMemberUtilities.cs` | `WriteMemberName` builds the `=` token with explicit empty leading trivia instead of `SyntaxFactory.Token(SyntaxKind.EqualsToken)`, which carries an elastic marker. The elastic marker made the code-action cleanup re-lay out the anonymous creation: in `VerifyFixWritesTheNameOfAMemberInThePropertysOwnInitializer` the closing `};` landed at column 4 instead of under `{`; with the change the output equals the explicit-name control. All SHARED-CASING-015 casing tests stay green. | SHARED-CASING-015, RH2001-012 (SHARED-FIX-007) |

## Policy owners
- `rg -n "TryGetInferredMemberName|AnonymousObjectMemberDeclaratorSyntax" src --type cs`: the anonymous-member decision is now only in `CasingAnonymousMemberUtilities.GetMemberNamedByReference`; RH2001 keeps `TryGetInferredMemberName` for tuple arguments only (RH2001-011, a separate decision). Other hits are unrelated: formatter `LineBreakAssignmentRewriter` / `JoinedGapUtilities` (layout), RH3002 analyzer (parentheses).
- Fix All owners: `rg -n "FixAllProvider" src/Reihitsu.Analyzer.CodeFixes --type cs` finds `DocumentOrderFixAllProvider` (shared, document-scoped, ~20 users) and the private `CasingCodeFixProviderBase<T>.CasingFixAllProvider` (solution-scoped, casing-specific).

## Production diff size
`git diff --shortstat origin/main...a13a255 -- src`: 2 files changed, 36 insertions(+), 25 deletions(-). No new type or member.

## Deleted special cases
_N/A._ (no shape declared unsupported). The anonymous-object branch of the former `GivesInferredMemberNameAsync` is deleted under lead decision 4.

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsed"`: `Failed: 2, Passed: 338, Total: 340` — red: `VerifyFixAllWritesTheNameOfAMemberNamedByAPropertyReadThroughAnotherFixedProperty`, `FixAllCorrectsEveryDiagnostic` (both "Expected '1' iterations but found '2' iterations").
- `scripts/test.sh --configuration Debug --project analyzer`: `Failed: 2, Passed: 12426, Total: 12428` (the same two).
- `scripts/test.sh --configuration Debug --project architecture`: `Passed: 221`.
- `scripts/format.sh` over both files: 0 of 2 changed.

## Disputed tests or scope

The two red tests assert correct behavior (SHARED-FIX-002: one Fix All application). They are not disputed; the default Fix All provider cannot deliver it here. Evidence from a temporary probe test (removed) that printed each single fix's `GetTextChangesAsync` on the starting document, as `BatchFixer` merges them:

- Row 18 (`new { Value.Name }`, both properties fixed): Value's fix changes `[142..143) 'V' -> '_v'`, Name's fix inserts `[142..142) '' -> 'Name = '`. Two changes at one position (FC-008, as the skeptic predicted); the merger drops one fix, and a second iteration finishes it with the expected `new { Name = _value._name }`.
- Inherited scenario (`new { Alpha, this.Beta }`): Beta's changes are fine-grained, but Roslyn's syntax differ reports Alpha's fix as one change `[76..133)` from behind the member `Alpha` through the end of the property `Alpha { get; set; }`, which covers Beta's insertion at 78. The same happens with `new { Alpha, Beta }`; with the names already written (`new { Alpha = Alpha, Beta = this.Beta }`) the changes stay small and do not overlap. Shaping the fix's output to steer Roslyn's differ would be a prediction of another component, so I did not try.

Ways out, for lead decision 7:

- **A — A solution-scoped sequential Fix All for RH2001**: apply the fixes one after another on the evolving solution, each re-registered on the code the earlier ones produced, as `CasingFixAllProvider` does for the casing fixes. Satisfies SHARED-FIX-002 and makes both tests pass without touching them. Cost: a new Fix All type (or a solution-scoped mode of the shared `DocumentOrderFixAllProvider`, which has ~20 consumers) — beyond this run's named production paths.
- **B — Switch RH2001 to the existing `DocumentOrderFixAllProvider`**: tried (one line). Both Fix All tests pass, but `VerifyFixQualifiesThePropertyReferenceNextToALambdaParameterInAnExpressionBodyInAPartialPartInAnotherDocument` and `VerifyFixWritesTheNameOfAMemberInThePropertysOwnInitializer` then fail; the first because that provider keeps only the diagnostic's own document, so a rename in a partial part in another document is lost. Reverted; not viable without extending the shared provider (that is A).
- **C — Leave `BatchFixer`**: needs the test implementer to set two Fix All iterations on both tests, and a SHARED-FIX-002 exception for RH2001 in the spec; a Fix All that leaves a diagnostic behind is visible, so this would be the maintainer's.

Recommendation: A, as a private solution-scoped provider modeled on the casing one, if the lead accepts the scope growth.

Adjacent fix taken (small, 1 line, in a component this run changes): the elastic `=` token in `CasingAnonymousMemberUtilities.WriteMemberName`. For the casing fixes, which do not run the formatter, the elastic marker presumably let the code-action cleanup re-lay out an anonymous creation the fix named a member in; no casing test pins such a layout (all stay green). The lead may want a casing regression test for it.

## Architecture findings
_None_ beyond lead decision 4, which this change carries out: RH2001 no longer decides which anonymous member takes its name from a reference; `CasingAnonymousMemberUtilities` is the only owner.

## Follow-up candidates
- A casing-fix test for a multi-line anonymous creation whose member name the fix writes, pinning the layout after the elastic-trivia fix (if the lead does not take it as part of this run).

## Round 2 — lead decision 8 (option A)

Commit: 31c86f4 — production commit "RH2001: Fix All through a solution-scoped mode of DocumentOrderFixAllProvider".
Verdict: tests pass

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs` | Opt-in solution-scoped mode, no new type: the class now derives from `FixAllProvider`; new factory `CreateSolutionScoped(analyzer, title)` returns the instance itself, whose `GetFixAsync` runs `ApplySolutionFixAllAsync`. That method takes the documents of the scope (document, project, solution) one after another, each from the solution the earlier fixes produced; a document whose text an earlier fix changed is analyzed again (`GetDiagnosticsAsync`), else the context's diagnostics are used; then the existing `ApplyFixAllAsync`/passes run on it. `ApplyPassAsync` gained `keepsSolution`: in that mode the next document is the fix's changed document with its whole solution (so a rename in another partial part is kept) instead of `WithText` on the old one. `Create` is unchanged and passes `keepsSolution: false`, so the ~20 document-mode users run exactly the old code path. Class summary and the changed methods' docs updated. | SHARED-FIX-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `GetFixAllProvider` returns `DocumentOrderFixAllProvider.CreateSolutionScoped(new RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer(), CodeFixResources.RH2001Title)` instead of `BatchFixer`. | SHARED-FIX-002, RH2001-012 |

### Production diff size
Round delta: 2 files changed, 89 insertions(+), 9 deletions(-) (doc comments included), under the ~100-line bound. Whole run vs `origin/main`: 3 files changed, 125 insertions(+), 34 deletions(-). No new type; one new internal static member (`CreateSolutionScoped`) and one override (`GetFixAsync`) on an internal class.

### Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsed"`: `Passed: 340, Failed: 0` (the partial-part-in-another-document test passes under Fix All as well).
- `scripts/test.sh --configuration Debug --project analyzer`: `Passed: 12428, Failed: 0`.
- `scripts/test.sh --configuration Debug --project architecture`: `Passed: 221`.
- `scripts/build.sh --configuration Debug`: 0 warnings (a first draft made `ApplyPassAsync` an instance method, RH7103; kept static with a parameter instead).

### Disputed tests or scope
- Scope growth the lead already accepted under decision 8: a shared helper with other consumers gains a mode. Its document mode is untouched in behavior.
- Known limit of the solution mode, not exercised by any test: a document re-analyzed because an earlier fix changed it uses all diagnostics the analyzer reports on its current code, not only those whose fix was offered on the starting code (SHARED-FIX-002's "offered on the code the application starts from"). For RH2001 that needs diagnostics in two parts of one partial type whose fixes interact; rare. No guard added.

### Follow-up candidates
- Unchanged from round 1 (casing layout test after the elastic-token fix, decision 9).
