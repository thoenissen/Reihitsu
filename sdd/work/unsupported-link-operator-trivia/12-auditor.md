Role: auditor
Commit: 0ed53a5ff94b697068c49579b9afb9224fb72ed4
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against 1011371 (repair round 1: commits 31c6717, d74866c, 7f6864d, 0ed53a5). Base `origin/main` is 31d2931af8e2894fba9e66a281edd23b4de9ae6c; `git ls-remote origin refs/heads/main` gives the same SHA, and it is an ancestor of the head. The checkout is 0ed53a5. The repair diff matches 09–11: one test file (`UnsupportedLinkOperatorCommentTests.cs`, +117), one summary in `PrecheckPhase.cs`, and handovers under `sdd/work/`. No spec, change doc, or production logic changed, so scope did not grow and this is a delta audit. `scripts/lane-check.sh behavior-change` still confirms the lane. I relied on the cleaner's Release build at 7f6864d, its text-only proof for `PrecheckPhase.cs`, and the test implementer's focused run (263 of 263) with its order mutation. I ran no test project.

Previous findings:

- **#1** (medium, FC-003, missing FMT-UNSUP-004/015 overlap test) — **closed**. `CommentInsideALinkOperatorInAnAttributeArgumentOfAVariableDeclarationGivesTheReasonOfTheVariableDeclaration` covers 4 gaps × 2 comment kinds × field with one declarator, field with several declarators, and event field, asserting `VariableDeclarationCommentReason` at the comment. `…OfAnotherMemberGivesTheReasonOfTheLinkOperator` covers the other side for method, property, nested class, and local function. The test implementer showed that moving FMT-UNSUP-015's candidate to the front fails every case of the first test. The directive test adds the FMT-UNSUP-010 overlap outside a value, matching the spec's sentence "FMT-UNSUP-010 excepts no initializer value … In a field with one declarator, the same directive gives this requirement's".
- **#2** (low, FC-031, "exactly" in `FindTriviaInsideLinkOperator`'s summary) — **closed**. The summary now names `FluentChain.GetLinkOperatorTokens` as the one definition and says the precheck looks at more than the links of the chains the phase normalizes.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
_N/A — the repair adds or changes no predicate, guard, or exemption; the guard-scope table of 08-auditor.md stands unchanged (byte-identical production logic, text-only proof PASS for the one changed production file)._

## Policy ownership
_N/A — the repair changes no policy; the owners recorded in 08-auditor.md are unchanged._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CommentInsideALinkOperatorInAnAttributeArgumentOfAVariableDeclarationGivesTheReasonOfTheVariableDeclaration` (24 cases) | FMT-UNSUP-004 span includes a field's and an event field's attribute arguments; where FMT-UNSUP-004 and FMT-UNSUP-015 start at the same comment, FMT-UNSUP-004's reason and location apply | FMT-UNSUP-015's reason, a wrong location, or no refusal; the candidate-order mutation turns all 24 red (09) | `AssertUnsupportedFor` (LF and CRLF, exact text, reason, tree identity, location of the only occurrence of `/* c */` or `// c` in `WrapInClass`) | adequate. Case construction traced: `gap[0][1..]` gives `?`, `!`, `!`, `!?`, so the arguments are `Source? c .Length`, `Source! c .Length`, `Source! c ?.Length`, `Source!? c .Length` — exactly the four gaps of `_operatorGaps`. |
| `CommentInsideALinkOperatorInAnAttributeArgumentOfAnotherMemberGivesTheReasonOfTheLinkOperator` (32 cases) | FMT-UNSUP-004's span does not reach attribute arguments of members that are no variable declaration; FMT-UNSUP-015 refuses there | FMT-UNSUP-004's reason (span widened), or no refusal | `AssertUnsupportedFor` | adequate |
| `DirectiveInsideALinkOperatorInAnAttributeArgumentOfAFieldKeepsTheReasonOfTheFieldDeclaration` (2 builds) | FMT-UNSUP-010 wins with several declarators, FMT-UNSUP-015 with one, both at `#if X`, in every build | Reasons swapped or a build-dependent answer; red under the order mutation (09) | `AssertUnsupportedFor` with parse options | adequate. FMT-UNSUP-004 covers comments only, so a directive is correctly not its case. |

Visible output (check 10): the new tests pin refusal reasons for already-refused inputs; the reasons are demanded by FMT-UNSUP-004, FMT-UNSUP-010, and FMT-UNSUP-015's overlap paragraph, none of which the run changes in the repair, and FMT-UNSUP-015's reason text is maintainer answer 1A.

## Failure classes dismissed
- The repair carries no compiled production change (text-only proof PASS for `PrecheckPhase.cs`; the other changed C# file is a test). Every class that needs a behavior change — FC-001, FC-002, FC-004–FC-016, FC-018–FC-030, FC-032–FC-044 and the tab-width matrix — cannot reach it; the dismissals of 08-auditor.md stand for the unchanged production code.
- FC-003: closed by the new overlap tests (#1).
- FC-017: see *Assertion adequacy*.
- FC-031: the one changed summary describes the code (#2); the new test summaries describe their cases. No `TODO`, no commented-out code, no tracker number in the repair's source lines.

## Architecture findings
_None._

## Hints
- The reflowed summary of `FindTriviaInsideLinkOperator` (`PrecheckPhase.cs:420`) leaves one 152-character line while its neighbours wrap near 120. Long documentation lines are common in the repository and no tool enforces a width, so this is cosmetic only.
- The *Hints* of 08-auditor.md (`OutputOfFormattedLinkOperatorsIsNotUnsupported` asserts stability, not exact output; per-member-access array allocation in the precheck) are unchanged and remain non-blocking.

## Follow-up candidates
_None_ beyond the 2A follow-up already in `00-lead.md`.
