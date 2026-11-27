Role: auditor
Commit: 123929e54882ee2b1a6471a577a160f28af33e9e
Verdict: findings

The prompt named no intended fix, suspected cause, or hoped-for verdict. `00-lead.md` asks the auditor to judge the implementer's note that the new check is stricter for "nullable/tuple type-argument qualifiers, unreached by tests". I judged it from probes, not from that note: the probes reach the stricter cases, and they remove fixes that `main` offers (#1).

## Scope

Full audit of the head `123929e54882ee2b1a6471a577a160f28af33e9e` against `origin/main` `2c7702214d596aa331304502d68d83d7818750f4`. `git ls-remote` matches that SHA, and `main` is merged into the head. The diff matches the handovers: `CasingRenameSafetyUtilities.RewritesOtherCodeAsync` now walks tokens through the new `OnlyRenamesOrQualifies`, `FindQualifiedName`, `IsNameOf`, and `IsQualifierToken`; its one caller passes `identifier`; the RH4103, RH4106, and RH4111 tests are new; the rest is `sdd/work/`. `scripts/lane-check.sh bug-fix` confirms the lane.

How I checked: static tracing, plus `scripts/apply-fix.sh` probes (LF and CRLF) on the head and on a temporary worktree of `main`. The fixtures were in the scratchpad and the worktree is removed.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs` `IsQualifierToken` (about line 655) via `FindQualifiedName` | FC-044, FC-001 | – | PR-introduced | The renamer can qualify a renamed name with a type that has type arguments. The new check recognises that qualifier only from a fixed list of token kinds, which leaves out `[`, `]`, `?`, `(`, and `)`. When the type argument is an array, a nullable value type, or a tuple, the rename is read as a rewrite of other code and the fix is withheld. SHARED-CASING-009 (unchanged) says: "A reference that the renamer renames, or renames and qualifies with … a type … does not withhold the fix". `main` offers this fix for some of these inputs, so the PR removes fixes users get today. No test or decision covers that. Counterexample (RH4103; the local function's name follows every rule): `class Base<T> { protected static int getValue() => 1; } class Derived : Base<int[]> { int Get(int other) { int GetValue() => 2; return getValue() + GetValue() + other; } }`. On `main` the fix renames the method and writes `return Base<int[]>.GetValue() + GetValue() + other;`. On the head no fix is offered. Members of the class, by the type argument inside the qualifier the renamer writes. **Fail, introduced by this PR** (fixed on `main`, withheld on the head): `int[]`, `int[,]`, `int?`. **Fail on both** (withheld on `main` and on the head, against the same sentence): `(int, int)`, `List<int[]>`. **Hold**: `int`, `List<int>` (nested `>>`), `string?` (the renamer writes `Base<string>`), `Sample<T>._value`, `this.`, `Sample.`, the namespace qualifier (`RH4002…FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`), and `nameof(Sample._value)`. | Take the qualifier from the syntax of the renamed tree, not from a list of token kinds. The tokens in front of the renamed identifier must be exactly the left side of the `MemberAccessExpressionSyntax`, `QualifiedNameSyntax`, or `AliasQualifiedNameSyntax` (or the cref equivalent) whose name is that identifier. That left side must be `this`, `base`, or a name or type. RH2001 already reads its qualifier this way (`RenameChangesBindingAsync`: `memberAccess.Name == identifierName`, `qualifiedName.Right == identifierName`). This reads the renamer's result instead of predicting which qualifier shapes it writes (FC-044), and it is still a cheap check. Add one regression test, for example `Base<int[]>` on RH4103. The other option is to refuse the class: keep the check and record the gap in SHARED-CASING-009 as a decision. That keeps a regression against `main` that the lead has to accept explicitly. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A token that carries no old name stays as written | Every token of every changed document, including structured trivia (`DescendantTokens(descendIntoTrivia: true)`); kind and text are compared | Previously: change spans from `GetChanges` that overlap or touch tokens; RH2001 compares bindings | Stricter than `main`, and as strict as the "no code other than…" sentence of SHARED-CASING-009 requires. Comment, whitespace, and disabled-text trivia are no longer compared. With default `SymbolRenameOptions` the renamer does not rewrite comments. Probe: `/* a */ Value /* b */` keeps both comments. Excluded code stays covered by `ExcludedCodeUtilities` | holds |
| An old-name identifier may be renamed (to the new name or kept as its own name) and qualified | Renamed tokens from the aligned index: skips tokens that `IsQualifierToken` accepts up to the first identifier spelled like the new name or the old name, and requires `.` or `::` directly in front of it | SHARED-CASING-009: "renames and qualifies with `this.`, `base.`, a type, or a namespace"; RH2001 takes the qualifier from `MemberAccessExpression.Name` / `QualifiedName.Right` | Does not agree: the token list is narrower than "a type" (see #1). The scan stops at the first identifier spelled like the new or old name. A qualifier that contains that spelling, such as a namespace `N.Order` around a class `order`, is therefore refused. That input is also refused on `main`, so it is not a regression | finding #1 |
| A non-qualifier insertion next to a cascade reference (cast, parentheses, or type arguments added after the name) | Same walk | SHARED-CASING-009: "rewrite … beyond renaming it and qualifying its name" withholds | Agrees. On `main` such an insertion passed because it touched a cascade token; it is now refused as the spec requires. The renamer produced none of these in my probes | holds |
| Fix All composition (`CandidatesConflictAsync`, `ApplyFixAllAsync`) | Same `RenameWithoutConflictAsync` with each candidate's own `identifier` | Single fix | Same check, same input | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a rename only renamed and qualified the cascade (SHARED-CASING-009, rewritten references) | `CasingRenameSafetyUtilities.RewritesOtherCodeAsync`/`OnlyRenamesOrQualifies` (casing single fix and Fix All, one caller in `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync`); `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.RenameChangesBindingAsync` (`rg -n "RenameSymbolAsync\|RewritesOtherCodeAsync\|RenameChangesBindingAsync" src`) | Two owners. Decision 1 keeps them separate, and the spec allows that: RH2001-011 takes over only the binding paragraphs. Not a new finding. The qualifier shape is decided twice, by tokens in the casing check and by syntax in RH2001. #1 suggests the casing check read the syntax as RH2001 does |
| Qualifier token recognition (`IsQualifierToken`) | `CasingRenameSafetyUtilities.IsQualifierToken` only (`rg -n "IsQualifier" src`) | One owner |
| Text diff of the renamed tree | None remain (`rg -n "GetChanges\(originalTree\)" src` finds nothing) | The old copy is gone |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH4106…FixQualifiesTheFieldReferenceNextToALocalWithTheNewName` (8 rows) | The rename is offered and only renames and qualifies, for sequences the old differ check handled differently | No fix, or any other token changed | `Verify` (full text, single fix and Fix All) | adequate; covers both sides of the old misalignment boundary (FC-003) |
| `RH4106…DeconstructionVariable…`, `…Parameter…`, `…InsideAnArgument…`, `…AssignedFieldReference…` | Same, for each kind of capturing symbol and each position | same | `Verify` | adequate |
| `RH4106…FixQualifiesTheStaticFieldReferenceWithItsTypeNextToALocalWithTheNewName` | A type qualifier is accepted | No fix | `Verify` | adequate for a plain type name. No row has a type with type arguments (`Sample<T>.` holds by probe; `Base<int[]>.` fails, #1) |
| `RH4106…FixAllQualifiesTheReferencesOfEveryFieldNextToLocalsWithTheNewNames` | Fix All composes two qualified renames (FC-008) | One rename skipped | `Verify` | adequate |
| `RH4106…NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField` | Input 1 stays withheld | A fix is offered | `Verify(testCode, testCode)` | adequate as a characterization test |
| `RH4111…FixQualifiesThePropertyReferenceNextToALocalWithTheNewName` (2 rows), `…AssignedPropertyReferenceNextToAParameter…` | Same, for PascalCase targets | same | `Verify` | adequate |
| `RH4103…FixQualifiesTheCallNextToALocalFunctionWithTheNewName` (2 rows), `…LocalDelegate…` | Same, for invocations | same | `Verify` | adequate |
| `RH4103…FixAllQualifiesTheCallNextToALocalFunctionAndSkipsTheRenameThatWouldCastAnArgument` | In one document, a cast rewrite stays withheld (its diagnostic stays) while a rename that only qualifies is applied | Cast rename applied, or the qualified rename skipped | `Verify` with markup allowed and one Fix All iteration | adequate; the one Fix All iteration is enough because the withheld diagnostic is stable by design |

## Failure classes dismissed

- FC-002: the spec words "renames" and "qualifies" are both explicit conditions (`IsNameOf`, and the `.`/`::` requirement). The qualifier's scope is #1.
- FC-004, FC-005, FC-019, FC-020, FC-021: the check reads the renamer's result and edits nothing. The renamer runs with default options (no renaming in comments or strings). Probe `/* a */ Value /* b */ + _value + other` → `/* a */ this._value /* b */ + _value + other`. Directives and excluded code are handled by `ExcludedCodeUtilities`, which did not change.
- FC-006: compile errors are still caught by `HasAddedCompileErrorsAsync`, which did not change. The token walk only adds refusals or removes misaligned ones, and never accepts a token outside the cascade that changed.
- FC-007, FC-009, FC-030: the rename's output is unchanged and the formatter is not involved.
- FC-010 to FC-014, FC-028, FC-029, FC-038 to FC-043: formatter, precheck, and analyzer layout are not involved.
- FC-016: the generic check serves every casing rule. The siblings I traced are `this.`, a type, a namespace, a nested generic type, and `nameof`. The failing siblings are in #1.
- FC-022, FC-023: literal and verbatim tokens are compared by `Text`, and old names by `ValueText` (`@`-escaped identifiers included).
- FC-024: partial-method parameters spelled differently are renamed to `identifier`, which `IsNameOf` accepts. The implementer's full analyzer run (12087 passed) includes the partial-method cascade tests.
- FC-026: the walk is positional, with no lookup by text. The repeated-token rows (`Value * _value * Value`, `Value + _value + _value`) are tested.
- FC-027: not reachable. The check works per token, not per syntax level.
- FC-032, FC-033, FC-034, FC-035, FC-037: the tests reproduce the issue's own input 2 and the skeptic's rows. The expected values come from SHARED-CASING-009 and from the renamer output on `main` for the holding rows. No line-ending dependency: LF and CRLF give the same result in every probe.
- FC-036: `nameof(Value)` → `nameof(Sample._value)` changes the string, as any rename does. This is pre-existing and allowed by spec ("a `nameof` operand").
- FC-018: references are still retargeted by the Renamer.
- FC-015: covered under Policy ownership.
- Performance: the walk covers every token of each changed document once per rename. That is negligible next to the compile-error check over the affected projects, which did not change.

## Architecture findings

#1 is part of the class the skeptic named (FC-044), which this run fixes. The casing guard still predicts the renamer's result instead of reading it. Before, it predicted through the differ's alignment; now it predicts through a list of the token kinds a qualifier may contain. RH2001 makes the same "is this only a qualifier" decision by reading the syntax of the qualified node. The input that shows the casing guard and the renamer disagreeing is `Base<int[]>` / `Base<int?>` / `Base<(int, int)>` in #1. The suggested change stays inside `CasingRenameSafetyUtilities` and adds no type.

## Hints

- RH4002 with an attribute class `fooAttribute` and a usage `[foo]`: no fix is offered, on `main` and on the head alike. I did not confirm that this check causes it. The usage token `foo` is not one of the old names (`fooAttribute`), so if the renamer writes `[Foo]`, the walk refuses that unchanged-looking rename. Uncertain and pre-existing, so not a finding.
- The `RewritesOtherCodeAsync` remarks say "instead of a text diff whose alignment is a heuristic". This gives the reason for the design, not a description of earlier behavior, so it is not an FC-031 issue. Mentioned only in case the owner wants to remove the reference to history.

## Follow-up candidates

_None._ The members of #1 that `main` also fails (`(int, int)`, `List<int[]>`) are part of #1 and are not filed separately.
