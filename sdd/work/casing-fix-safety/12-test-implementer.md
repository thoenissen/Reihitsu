Role: test implementer
Commit: 19f39b2 (head, includes 2499cbe); changes committed with this handover
Verdict: #1597: tests written; #1598: tests written; #1016: nothing to do; #1015: nothing to do; #1084: nothing to do

Repair round 1, lead decisions 13, 14, 16 (auditor findings 1 and 2; SHARED-CASING-012 examples a–d of `11-spec-author.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from SHARED-CASING-012 as clarified in 2499cbe. No existing assertion changed.

## thoenissen/Reihitsu#1597

### Tests
All in `src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.cs`, single fix via `Verify`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoFixIsOfferedWhenACallerArgumentExpressionIsAQualifiedNameofOfAnotherSymbolWithTheOldName` — example (a), `nameof(Names.Condition)` | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | **red**: fix offered, `Condition` → `condition`, attribute unchanged |
| `NoFixIsOfferedWhenACallerArgumentExpressionIsANameofOfAnotherSymbolWithTheNewName` — example (b), `private const int condition` + `nameof(condition)` | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | **red**: fix offered, writes `bool condition, …nameof(Guard.condition)…` |
| `NoFixIsOfferedWhenAMemberOfAnExtensionBlockNamesTheReceiverByACallerArgumentExpressionLiteral` — example (c), `extension(bool Condition)` + `[CallerArgumentExpression("Condition")]` | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | **red**: fix offered, receiver → `condition`, literal unchanged |
| `NoFixIsOfferedWhenAMemberOfAnExtensionBlockNamesTheReceiverByANameofCallerArgumentExpression` — example (c), `nameof(Condition)` in the member's list | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` | **red**: fix offered, receiver and `nameof` operand renamed |
| `FixRenamesTheReceiverOfAnExtensionBlockWithoutACallerArgumentExpression` — example (c) control | SHARED-CASING-012, RH4105-003 | `Verify(src, fixed)` | green |
| `NoFixIsOfferedForAnUnqualifiedNameofOfTheOldNameBeforeCSharp11` — example (d), C# 10, `public const int Condition` + `nameof(Condition)` | SHARED-CASING-012, RH4105-003 | `Verify(src, src)` + `SolutionTransforms` → `LanguageVersion.CSharp10` | **red**: fix offered, writes `bool condition, …nameof(condition)…` |
| `FixRenamesAnUnqualifiedNameofOfTheOldNameFromCSharp11OnWhenTheTypeHasAMemberWithThatName` — other side of (d) on the same input (FC-003): C# 11, same `const` + `nameof(Condition)`, fix renames parameter and operand | SHARED-CASING-012, RH4105-003 | `Verify(src, fixed)` + `LanguageVersion.CSharp11` | green |

A private `ApplyLanguageVersion(Solution, ProjectId, LanguageVersion)` (new `#region Methods`) sets the test project's parse options, as RH5412/RH5415 tests do. The existing `FixIsOfferedWhenACallerArgumentExpressionNamesTheParameterWithNameof` (default language version) stays the other side of (a)/(b) without a competing symbol.

All five red tests fail on the expected-versus-actual text of the iterative fix (a fix is offered where none may be), not on compile errors or harness setup. The C# 10 setting was confirmed to apply (a collection-expression probe in the same test gave CS8936 under C# 10; probe removed).

### Observation on example (d) — for the lead, not a diagnosis
SHARED-CASING-012's *Why* says that before C# 11 an unqualified `nameof` in a parameter attribute refers to a member of the type, not the parameter. On this repository's compiler that does not hold for a parameter-list attribute:
- In the analyzer test under C# 10, the Renamer renamed the `nameof(Condition)` operand together with the parameter, so the semantic model binds it to the parameter.
- File-based programs with `LangVersion=10` (scratchpad `cs10/`, `dotnet run`): `before.cs` (`const int Condition` + `Check(bool Condition, [CallerArgumentExpression(nameof(Condition))] …)`) prints `1 > 0`; `after.cs` (the fix's output, `bool condition … nameof(condition)`) compiles and prints `1 > 0`; `nofield.cs` (`nameof(condition)` with no member named `condition`) compiles and prints `1 > 0`, so the parameter is in scope under C# 10.

So under C# 10 the fix's output keeps the run-time behavior for this input. The test asserts the spec as written (withhold), which is a conservative refusal and not unsafe; whether the C# 11 condition and its *Why* stay is the lead's / spec author's call. If the condition is dropped, this test flips to `Verify(src, fixed)` with the C# 11 test's expected text.

### Not covered, and why
- Fix All for (a)–(d): the auditor found Fix All recomputes `TryGetFixedIdentifier` per candidate through the same `RenameWithoutConflictAsync` path (10-auditor.md, FC-008), and the existing `FixAllSkipsTheParameterNamedByACallerArgumentExpression` covers the Fix All side of SHARED-CASING-012; spec marks Fix All optional for these rows.
- RH4120 counterparts: positional record parameters cannot be extension receivers, and (a)/(b)/(d) go through the same `HasCallerArgumentExpressionNaming` call for every casing provider (`rg -n HasCallerArgumentExpressionNaming src`, one owner per 10-auditor.md *Policy ownership*).
- Line endings, formatter, comments/directives in gaps: the fix renames through the Renamer and formats nothing (FC-010 to FC-014, FC-019 to FC-022 not reachable).

## thoenissen/Reihitsu#1598

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ApplyFixInMultiTargetSolutionAsync` in `src/Reihitsu.Analyzer.Test/Naming/RH4109PublicFieldCasingAnalyzerTests.cs` (used by the five multi-target tests) — the vacuous `Assert.IsNotEmpty(…Concat([initiatingDocumentId]))` is replaced: for every document, its `GetLinkedDocumentIds()` must equal exactly the other projects' copies with the same path (`Assert.AreSequenceEqual`), and the initiating holder document must have a linked document | FC-017, FC-033 (no requirement tag change; the five tests keep theirs) | — | green (5/5) |

Mutation check: giving each project's copy a distinct file path (`/{project}{path}`) made every multi-target test in the run filter (`~RH4109PublicFieldCasingAnalyzerTests.Fix`, the four whose names start with `Fix`) fail with "The copies of /src/Lib/A.cs in Lib(net9.0) must be linked documents of each other."; reverted after the check. So the assertion now fails when the copies are not linked.

## thoenissen/Reihitsu#1016

Nothing to do in this round.

## thoenissen/Reihitsu#1015

Nothing to do in this round.

## thoenissen/Reihitsu#1084

Nothing to do in this round.

## Whole run

- Focused run on head 19f39b2 plus these tests: RH4105 + RH4109 classes, 63 total, 58 passed, 5 failed (the five red tests above, all #1597).
- `scripts/format.sh` on both test files: 0 changes.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4105MethodParameterCasingAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4109PublicFieldCasingAnalyzerTests"
```
