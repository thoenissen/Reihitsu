Role: auditor
Commit: c08deac2c12f1c4d5a78f4b91b4f9780bdaf00b3
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `7c131eba4d5ce6011b2838d4a3b59308d532ad42` (audit 2, `13-auditor.md`). Head `c08deac2c12f1c4d5a78f4b91b4f9780bdaf00b3` is checked out, and the tree is clean. Base `origin/main` is `1788f94d265c4e13052d5145404ae3ffd21e6acc`, which matches the prompt and `git ls-remote`, and it is an ancestor of the head. `scripts/lane-check.sh behavior-change`: confirmed.

The repair diff matches handovers 14–16. It adds four tests (+150 lines) to `tests/Reihitsu.Analyzer.Test/Naming/RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.cs` and changes the work files. It touches no production code, spec, change doc, or rule page, so scope did not grow. The guard under test, `compilation.IsSymbolAccessibleWithin(candidate, recordType)` in `RecordPositionalParameterUtilities.GetMember` (`src/Reihitsu.Analyzer/Core/RecordPositionalParameterUtilities.cs:69`), is byte-identical to audit 2, so I reused that audit's guard-scope evidence. I ran no tests. The four new inputs match the four inputs of audit 2's probe, which failed on the public-only mutant and passed on the head. The test implementer's mutation runs (`14-test-implementer.md`) independently show that every new test fails under at least one of the two mutants. I trust the cleaner's build (0 warnings) and the 45/45 RH4120 result.

Previous findings:
- **#1: closed** (already closed in audit 2, unchanged).
- **#2: closed** (already closed in audit 2, unchanged).
- **#3: closed.** Every member it listed is now pinned, each by its own test that expects the parameter to be reported:
  - protected field of an intermediate source record: `ParameterMatchingAProtectedFieldOfAnIntermediateRecordInFrontOfAnInheritedDictatedPropertyIsReported`
  - internal field in the same project: `ParameterMatchingAnInternalFieldOfAnIntermediateRecordOfTheSameProjectInFrontOfAnInheritedDictatedPropertyIsReported`
  - a private field of the enclosing base record, which the nested record can access: `ParameterOfANestedRecordMatchingAPrivateFieldOfItsEnclosingBaseRecordInFrontOfAnInheritedDictatedPropertyIsReported`
  - the record's own private field: `ParameterMatchingADeclaredPrivateFieldThatHidesAnInheritedDictatedPropertyIsReported`

  Traced against the two mutants:
  - The public-only model (`DeclaredAccessibility == Public || ContainingType == recordType`) skips the protected, internal, and nested-private fields. The walk then reaches the dictated `Person.name`, so the diagnostic disappears in those three tests.
  - The assembly-scoped `within` (`IsSymbolAccessibleWithin(candidate, recordType.ContainingAssembly)`) skips the protected field, the nested-private field, and the own private field. Those three tests fail.

  Together the two mutants cover all four tests. Both the hand model that FC-044 warns against and a wrong `within` are now caught.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
_N/A: the repair changes no predicate, guard, or exemption. The `GetMember` guard is byte-identical to audit 2, and that audit's table holds unchanged._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The member a positional parameter stands for, and its accessibility | `rg -n "IsSymbolAccessibleWithin" src --type cs`: one hit, `RecordPositionalParameterUtilities.GetMember` (line 69). Unchanged since audit 2 | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `…ProtectedFieldOfAnIntermediateRecord…IsReported` | A protected member that the record can access stops the walk in front of the dictated member, and that member is not dictated | Missing RH4120 on `BehindProtected(name)` (public-only and assembly-scoped mutants) | `Verify(source, onConfigure)` with a metadata reference to `Meta`. It compares the exact diagnostic set, compiler errors and warnings included | ok |
| `…InternalFieldOfAnIntermediateRecordOfTheSameProject…IsReported` | The same for an internal member of the same assembly | Missing RH4120 (public-only mutant) | same | ok |
| `ParameterOfANestedRecordMatchingAPrivateFieldOfItsEnclosingBaseRecord…IsReported` | A private member of the enclosing type is accessible within the nested record, so it is matched | Missing RH4120 (both mutants) | same. `PeekOuter()` reads the field, so the only warning that could appear, CS0414, does not fire | ok |
| `ParameterMatchingADeclaredPrivateFieldThatHides…IsReported` | A private member that the record declares itself is matched | Missing RH4120 (assembly-scoped mutant) | same, with `PeekField()` | ok |

Each test uses a PascalCase parameter (`Other`) in its intermediate record, so the markup's single diagnostic is the parameter under test. An extra or missing diagnostic fails the exact-set comparison. Each test carries `[Requirement("SHARED-CASING-014")]` and `[Requirement("RH4120-001")]`.

Visible output (check 10): each new expected value is "reported". SHARED-CASING-014's record-parameter bullet demands it: when the member the compiler matches is not dictated, the parameter is reported. Maintainer answer 2B in `00-lead.md` covers it, and the run does not change that requirement in this round.

## Failure classes dismissed
- FC-001, FC-002, FC-044: no guard changed. Audit 2's trace holds, and the class it left open on the test side (FC-003, FC-017) is closed above.
- FC-015, FC-016: one owner, unchanged.
- FC-031: the four new `<summary>` texts describe their inputs and expectations accurately, and they follow the file's style (`Verifies that…`, no final period). There is no `TODO`, no commented-out code, and no tracker number.
- FC-032, FC-035: the expected values come from SHARED-CASING-014 and answer 2B, not from running the code.
- FC-004–FC-014, FC-018–FC-030, FC-033, FC-034, FC-036–FC-043, and the tab-width matrix: the repair is test-only and touches no production, trivia, layout, formatter, precheck, code-fix, or unsupported-shape path.

## Architecture findings
_None._ `00-lead.md` decision 6 already decided the structural point of #3 (the guard reads the compiler's accessibility result, and the tests pin it), and this round's tests carry it out.

## Hints
- Audit 2's hint on SHARED-CASING-014's "declares or inherits" wording stands. `00-lead.md` decision 7 leaves it unchanged, and I see no reason to reopen it.
- `00-lead.md`, *Decisions taken* row 2 still carries the superseded "stays reported". The PR summary should not carry it over. This is unchanged from audits 1 and 2.

## Follow-up candidates
_None._
