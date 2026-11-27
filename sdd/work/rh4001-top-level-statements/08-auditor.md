Role: auditor
Commit: dc6146a
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Scope

This is a full audit of thoenissen/Reihitsu#1086, base `0feb518` (`origin/main`, confirmed with `git ls-remote`; it is an ancestor of the head) to head `dc6146a`.

The prompt named head `710f346`. The checkout holds one more commit, `dc6146a` ("Record spec check stage"), which only changes two lines of `00-lead.md`. The audited tree is therefore the prompt's tree plus the lead's state record, so this is not a state mismatch.

The diff matches the handovers:
- one production file: `RH4001TypeNameShouldMatchFileNameAnalyzer.cs`, +19/−1;
- five tests in `RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs`;
- the spec `RH4001.md` and `requirement-ids.txt`;
- the change doc `sdd/changes/2026-11/rh4001-top-level-statements.md` (Accepted);
- the run's handovers.

`scripts/lane-check.sh behavior-change`: confirmed.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `sdd/specs/analyzer/RH4001.md:202` (RH4001-007, third example); `tests/Reihitsu.Analyzer.Test/Naming/RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs`, `FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported` | FC-017, FC-034 | – | PR-introduced | The spec text says "a file `Program.cs` without top-level statements, whose statements stand inside `Main`". The test is named `...WithStatementsOnlyInsideMain...`, and its summary says it "verifies that a file whose statements stand only inside a method of its first type is checked". Yet `Main` is empty in both the example and the test: `private static void Main() { }`. So no statement stands inside `Main`, and the test cannot observe "is checked". It passes for every predicate, including one that exempts all files, because `Program` matches the file name either way. The boundary itself is covered by `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed`. This test only misdescribes itself, and the spec example misdescribes itself in the same way. | Put a statement in `Main`, such as `System.Console.WriteLine();`, in both the spec example and the test, so that the text, the test name, and the code agree. Adjust the summary to say that a file with statements only inside a type member is not exempted, and is not reported when its first type matches. If the lead prefers not to touch the test code, reword the spec sentence, the test name, and the summary to "whose entry point is `Main`". |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A file is exempt from RH4001 (`HasTopLevelStatements`) | `CompilationUnitSyntax.Members` only (direct children). Descendants, namespace members, and disabled text are not inspected. | RH4001-007: "a global statement among [the compilation unit's] members". `FindFirstTypeDeclaration` walks the compilation unit and nested namespaces. | Agree. A `GlobalStatementSyntax` occurs only as a compilation-unit member; in a namespace or a type, a statement is a parse error, so neither the requirement nor the predicate needs to descend. A statement inside a type member is a `StatementSyntax` under a method, not a member, so the predicate leaves it out, as the requirement's qualifier demands (FC-002). "Any member" and "the first member" agree on every compiling file (CS8803). A top-level local function is a `GlobalStatementSyntax` wrapping a `LocalFunctionStatementSyntax`, so it is exempt, as example 2 states. | ok |
| Code-fix offer | The diagnostic's location | `RH4001TypeNameShouldMatchFileNameCodeFixProvider.RegisterCodeFixesCoreAsync` acts only on reported diagnostics and does not recompute the first type's eligibility | Agree; no diagnostic means no fix | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "This file holds top-level statements" | `rg "GlobalStatementSyntax\|SyntaxKind.GlobalStatement" src` finds one owner of the decision, `RH4001TypeNameShouldMatchFileNameAnalyzer.HasTopLevelStatements`. The other hits decide different things: `MemberLineStartRewriter.IsEmptyGlobalStatement` (a stray `;` member, layout) and `CommentPositionUtilities.cs:112` (a member-kind list for comment placement). | One owner, ok |
| Which type a file is named after / expected file name | `RH4001TypeNameShouldMatchFileNameAnalyzer.FindFirstTypeDeclaration` and `RH4001TypeNameShouldMatchFileNameHelper.GetExpectedFileNameStem` / `GetComparableFileName`, all unchanged. RH4012 defers the file-name concern to RH4001 (`sdd/specs/analyzer/RH4012.md:20`). | Unchanged, ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FileWithTopLevelStatementsIsNotReported` | A statement followed by a mismatched `class Helper` in `Program.cs` gives no diagnostic (this is the #1086 "Done when" input) | Any RH4001 diagnostic, or a compiler error. `CSharpAnalyzerVerifierTest` with no markup asserts exactly zero diagnostics, and `OutputKind.ConsoleApplication` avoids CS8805. | `Verify(source, onConfigure)` (analyzer-only) | ok; failed before the implementation (03) |
| `FileWithTopLevelLocalFunctionAndNamespacedTypeIsNotReported` | The exemption holds with a top-level local function and a type inside a block namespace | A diagnostic on `Calculator` | same | ok; failed before the implementation (03) |
| `FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported` | Its summary claims that a file with statements only in a type member is checked | Not falsifiable for the claim: `Main` is empty and `Program` matches `Program.cs`, so every predicate passes | same | weak; finding #1 |
| `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed` | A statement inside `Main` does not exempt the file: `Startup.cs` is reported on `Program` and renamed to `Program.cs` | No diagnostic, which a `DescendantNodes().OfType<StatementSyntax>()` predicate or a ConsoleApplication-wide exemption would cause; or a different rename | `Verify(source, null, onConfigure)` with `FixedState` | ok; this is the FC-003 side of the boundary |
| `FileWithoutTopLevelStatementsIsReportedWhenAnotherFileHoldsThem` | A project-wide or semantic exemption is ruled out: `Startup.cs` is reported and renamed while `Program.cs` holds statements | No diagnostic on `Helper`, which a compilation-level entry-point check would cause | same, two documents | ok |

## Failure classes dismissed

- FC-001, FC-002, FC-003: traced under *Guard scope*. Both sides of the boundary are tested (tests 1, 2 against 4, 5).
- FC-015, FC-016: one owner (see *Policy ownership*). No sibling copy of the decision exists, and RH4012 defers to RH4001.
- FC-044: the guard reads the parsed tree of the configuration being analyzed. A statement only in an inactive `#if` group is disabled text and leaves the file checked. That is the compiler's own result, not a model of it.
- FC-004, FC-005, FC-019, FC-020, FC-021: no token gap, join, or trivia edit. The change only adds an early return in an analyzer.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite is added. The code fix (a file rename) is unchanged and reached only through a diagnostic.
- FC-007, FC-008, FC-009, FC-018, FC-030: the code fix is unchanged. RH4001 has no Fix All. With no diagnostic, no action is registered.
- FC-010 to FC-014: the formatter is not involved. A file name is not something the formatter judges, so FC-013 and FC-014 do not apply.
- FC-028, FC-038 to FC-043: no formatter entry point, no precheck, and no unsupported shape is involved.
- FC-017: see finding #1. The other tests are adequate.
- FC-031: the summary of `OnSyntaxTreeAction` and the new inline comment describe the new early return, and the summary of `HasTopLevelStatements` matches its body.
- FC-032, FC-035: the expected values come from the spec examples and the maintainer decision, not from patched code. The test implementer observed the two not-reported tests failing before the implementation.
- FC-033: the harness sets `OutputKind.ConsoleApplication` in every test with top-level statements. Compiler errors would fail the verifier, and no error is suppressed.
- FC-034: test 1 has the shape of the #1011 Q3 input: a statement, then `class Helper` in `Program.cs`. The test only adds a constant to `Helper`.
- FC-037: not used. Lane check and verify-text-only were run as scripts.

Visible output (check 10): the expected values of all five tests and the unchanged rule page are covered by the maintainer's decision. That decision is recorded under `Maintainer answers` in `00-lead.md` and as the first entry of the change doc's `Decisions`. I confirmed the issue text on #1086 (#1011 Q3, option a, 2026-10-18). The tests of the reported cases (4, 5) assert behavior that is unchanged from `main`.

Special-case cost (check 11): the change is one member-kind check with no new type. The input class is the standard minimal-API `Program.cs` shape, not a rare one. No unsupported shape is involved.

Code quality and comments: no `TODO`, no commented-out code, and no tracker numbers in source. `HasTopLevelStatements` is private and documented. No scope edits outside the run.

## Architecture findings

_None._

## Hints

- `documentation/rules/RH4001.md` keeps a pre-existing `## Exceptions` section (split files, the EF migration prefix, generic braces), while `AGENTS.md` keeps exceptions off rule pages. The run correctly does not add the new exemption there. The page's existing section predates the convention and is unrelated to this run.

## Follow-up candidates

_None._
