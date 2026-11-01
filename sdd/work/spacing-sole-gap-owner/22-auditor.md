Role: auditor
Commit: 9a226d4
Verdict: pass

## Scope

This is the repair-round-3 delta audit of thoenissen/Reihitsu#1493, covering `git diff 4c943fc 9a226d4`.

State checks:
- The head is `9a226d4b7bb5ae72f2157eda79ae7389652b4932` on `claude/focused-brown-4e3k0u`.
- `git ls-remote origin refs/heads/main` returns `e3842c855ff487ae71a403d50dcd6613fb6d8194`, and `git merge-base --is-ancestor` confirms the branch contains it.
- `scripts/lane-check.sh behavior-change` reports `confirmed`.

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

The repair diff matches handovers `19` to `21` and lead row 27:
- FMT-SPACE-021's second example now declares `unsafe struct C` in both blocks.
- FMT-SPACE-021's parameter bullet now names every parameter list.
- `DeclaredNameSpacingTests.MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample` mirrors the `struct` change.
- `ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType` adds a conversion operator, an anonymous method, and an extension block, and its summary names all ten lists.

No production file changed, and the change doc is untouched. The scope did not grow, so this is a delta audit. Evidence from `13-auditor.md` and `18-auditor.md` is reused for everything outside the delta.

### Previous findings

- **#1: closed** (in `18-auditor.md`). Nothing in this repair touches it.
- **#2: closed.** Both FMT-SPACE-021 example blocks (`horizontal-spacing.md` lines 1043 and 1072) and both literals of the verbatim test now read `unsafe struct C`. Nothing else in them changed. The spec author compiled both blocks as `net10.0` with `AllowUnsafeBlocks` and got no errors. The verbatim test passes.
  - I enumerated the class "example or test code that C# rejects structurally" over the run's other `unsafe class C` blocks. These are FMT-SPACE-024 (spec line 1262), FMT-SPACE-027 (spec line 1399), and change-doc entries at lines 373, 579, and 751.
  - None of them declares a fixed-size buffer. They hold function pointers, which are legal in a class.
  - Change-doc entry 6 (line 373) has no buffer, as `18-auditor.md` recorded.
  - Every remaining fixed-size buffer in the run's specs and tests sits in a struct: spec lines 1047 and 1076, and `DeclaredNameSpacingTests` lines 466–474 (`unsafe struct S`).
- **Hint "the parameter bullet names fewer lists than the code covers": closed.**
  - The bullet now names a method, a local function, a constructor or primary constructor, an operator or conversion operator, an indexer, a delegate, an anonymous method, an explicitly typed lambda, and an extension block.
  - I checked this against Roslyn's `ParameterSyntax` owners. `ParameterListSyntax` belongs to:
    - a method, a constructor, an operator, a conversion operator, a destructor (no parameters), and a delegate;
    - a local function, an anonymous method, and a parenthesized lambda;
    - a type declaration's primary constructor (records included), and an extension block.
  - `BracketedParameterListSyntax` belongs to an indexer.
  - A simple lambda's `ParameterSyntax` has no type, so "that has a type" excludes it.
  - `FunctionPointerParameterSyntax` and `CrefParameterSyntax` are other node types, and the rule's `ParameterSyntax` arm does not reach them.
  - The list is therefore complete and agrees with the single arm `ParameterSyntax parameter when right == parameter.Identifier` in `DeclaredNameSpacingRule.IsDeclaredName`.
  - The rule's summary says "the type of a parameter" generically, so it needed no change.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | None | – |

## Guard scope
_N/A — the repair changes no predicate, guard, or exemption. `DeclaredNameSpacingRule` is byte-identical to `4c943fc`. The parameter bullet's agreement with its `ParameterSyntax` arm is recorded under Previous findings._

## Policy ownership
_N/A — no policy moved and no production file changed. The only other place that enumerated parameter lists was the test summary, which now matches the bullet. `grep` over `sdd/` (outside `sdd/work/`) and `src/` finds no third copy of the enumeration. Change-doc entry 6 names parameters generically._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample` | The second example of FMT-SPACE-021 | Any gap differing from the spec's output block, or a second pass that changes something | `AssertRuleResult` (exact text under LF and CRLF, second pass, phase stability) | Adequate. The literals equal the spec blocks line for line, and the input now compiles. |
| `ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType`, new lists | FMT-SPACE-021: one space between type and name in a conversion operator, an anonymous method, and an extension block (and that block's member) | `int?v`, `int?m`, `int[]n`, `string?s`, or `int[]k` staying tight | `AssertRuleResult` | Adequate. Every new parameter has a type ending in `?` or `]`, which a tight gap leaves joined, so a missing rule fails the test. The added constructs are written in their canonical block layout, so input and expected differ only in the declared-name gaps. I compiled the expected text in a scratch `net10.0` project (`LangVersion latest`, nullable on) and the build succeeded with warnings only, so the test input is valid C# 14. The focused class passes on this head (26 of 26). |
| Check 10 for the new expected values | — | — | — | Covered. The only gaps that differ between input and expected are declared-name gaps, which answer 7C covers ("every declared name", `00-lead.md`). The operator's block body is unchanged from round 2 and covered by FMT-STRUCT-005. The `delegate(` and `extension(` gaps are authored tight and stay tight: no rule of the run decides them, and FMT-SPACE-002 keeps an empty gap. |

## Failure classes dismissed

- **FC-001, FC-002, FC-003.** No guard changed. The parameter scope is now stated exhaustively, and it agrees with the code's arm.
- **FC-004, FC-005, FC-019, FC-020, FC-021.** No edit or trivia handling changed.
- **FC-006, FC-010, FC-011, FC-012.** No phase changed. `AssertRuleResult` runs phase stability on the new inputs. The anonymous method in the new test is a local initializer, not an argument, so the pre-existing `LineBreakPhase` hint about anonymous methods next to lambda arguments is not reached, and the stability check passes.
- **FC-007, FC-008, FC-009, FC-014, FC-018, FC-030, FC-036.** No code fix, analyzer rule, or rename is touched.
- **FC-013, FC-032.** Production behavior is byte-identical to the previously audited head.
- **FC-015, FC-016.** One owner, `DeclaredNameSpacingRule`. The spec text and the test summary now agree with it.
- **FC-017.** The assertion is exact text under LF and CRLF, with two passes and phase stability.
- **FC-022 to FC-029.** No literal, identifier, `partial`, expression-bodied, re-resolution, nesting, detached-node, or key/value logic changed.
- **FC-031.** The changed test summary describes the test. The rule summary is unchanged and still accurate.
- **FC-033, FC-034, FC-035.** These are not reproductions. The expected values come from the spec and answer 7C, and the test implementer's discrimination run on `origin/main` fails both tests on the declared-name gap.
- **FC-037.** No text-only claim about a `.cs` production file is made in this round; only a test file changed.

## Architecture findings

_None._

## Hints

- **Carried from `13-auditor.md` and `18-auditor.md`, unchanged:**
  - Question 15 must reach the maintainer in the run report.
  - The pre-existing `LineBreakPhase` stability reversal on lambda arguments next to an anonymous method still holds.
  - The undecided gap in front of a relational pattern's operator still holds.
