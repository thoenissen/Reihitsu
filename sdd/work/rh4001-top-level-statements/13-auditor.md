Role: auditor
Commit: 8c19df6
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Scope

This audits the repair delta only, for thoenissen/Reihitsu#1086. It compares the earlier audited head `dc6146a` with head `8c19df6cddb996d098e93c0dde77162e410d980f`. The base is `origin/main` `0feb518`, which `git ls-remote` confirms and which is an ancestor of the head. The checkout is the head.

The repair delta matches handovers 09 to 12:
- `sdd/specs/analyzer/RH4001.md`: one added line, `System.Console.WriteLine();`, inside `Main` in the third example of RH4001-007;
- `RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs`: the same statement in `FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported`, and a reworded summary for that test;
- `00-lead.md` bookkeeping, and handovers 08 to 12.

The repair touches no production file, no change doc, no rule page, and no requirement sentence. The scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed.

Previous findings:
- **#1** (low, example 3 of RH4001-007 and its test claim statements inside an empty `Main`): **closed**. The spec example and the test now hold identical code with a statement inside `Main`, so the sentence "whose statements stand inside `Main`", the test name, and the code agree. The new summary no longer claims "is checked". The test cannot observe that claim. `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed` (`Startup.cs`, reported) still asserts it.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|

_N/A: the repair changes no predicate, guard, or exemption. `HasTopLevelStatements` and the code-fix path are byte-identical to `dc6146a`, so the evidence in `08-auditor.md` carries over._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|

_N/A: the repair changes no policy. The ownership evidence in `08-auditor.md` is byte-identical for the production code._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported` (repaired) | `Program.cs` has a statement only inside `Main` of its first type `Program`, followed by `class Helper`, and gives no RH4001 diagnostic (RH4001-007, example 3; RH4001-001). | Any RH4001 diagnostic, or a compiler error under `OutputKind.ConsoleApplication`. `Verify` with no markup asserts zero diagnostics. | `Verify(source, configure)`, analyzer-only | ok. The test claims only what it can observe: the stated outcome of the example. The "is checked" side of the boundary is still covered by `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed`, which is unchanged. The test code matches the spec example verbatim. |

## Failure classes dismissed

- FC-017, FC-034: these were the classes of #1. They are closed (see *Assertion adequacy*), and the test input is now the spec example verbatim.
- FC-031: the reworded summary describes the test body, and it states nothing that the test does not assert.
- Every other class: the repair adds no production code, rewrite, guard, or formatter involvement. Their dismissals in `08-auditor.md` hold unchanged.

Visible output (check 10): the repaired test expects no diagnostic. That is required by RH4001-001, which this run does not change, because `Program` matches `Program.cs`. It is unchanged from `main`, and the maintainer decision recorded under `Maintainer answers` covers it as well.

Special-case cost (check 11), comments, and scope: nothing new. No `TODO`, no tracker numbers, and no edits outside the run.

## Architecture findings

_None._

## Hints

- The pre-existing `## Exceptions` section on `documentation/rules/RH4001.md`, noted in `08-auditor.md`, is unchanged and unrelated to this run.

## Follow-up candidates

_None._
