Role: auditor
Commit: c5374c2
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from thoenissen/Reihitsu#1926, `00-lead.md`, handovers `01` to `12`, the change doc `sdd/changes/2026-11/assignment-and-arm-comment-unsupported.md`, FMT-UNSUP-020/021/022 and the clarified requirements, and `git diff eed7195c0b6fb583ab4271173b7a4ad022851c5f HEAD`.

## Scope

Full audit. Head c5374c2 (checked out, clean tree). `origin/main` is eed7195c0b6fb583ab4271173b7a4ad022851c5f, and it is an ancestor of HEAD through merge commit 71519cb. The diff matches the handovers:
- production: `PrecheckPhase.cs` and `LineBreakAssignmentRewriter.cs` only;
- specs, change doc, `AGENTS.md` precheck row, `requirement-ids.txt`;
- tests as listed in `06` and `09`.

Main's production changes since the run's base 1788f94 were checked against the deleted joins: `LineBreakPhase` (single-line interpolations), `ControlFlowBraceTransform`, `SwitchCaseBraceRewriter`, and `DocumentationCommentFormattingPhase`. None of them touches `LineBreakAssignmentRewriter`, and none moves trivia into a gap behind an operator, an `=`, or an arm's `=>`.

Checks of the issue:
- Both proposed shapes are refused.
- The answered widenings are refused: 1B, 2A, 3A, and batch 2 Q2 B.
- The CLI path is pinned by CLI-019.
- The per-fix tests follow decision 4.
- The change doc lists the deletions under *Special cases*, and the diff carries every one of them: the five refusals behind the operator or the `=`, and the FMT-UNSUP-019 walk.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/ObjectInitializerContributor.cs:122-123` (the `InitializerExpressionSyntax { Parent: AssignmentExpressionSyntax }` case) | FC-031 / FC-042 (comment only) | – | PR-introduced | The inline comment says the members and the closing brace follow the brace "where it stands, which is behind a comment that leads its line". After this change, a comment in front of a nested initializer's brace stands behind the assignment's `=`, and FMT-UNSUP-020 refuses it. FMT-INDENT-023 now says so, and FMT-INDENT-030 dropped the nested-initializer mention. No input a caller passes reaches that situation: a whole document is refused, RH5304 formats the member assignment and is withheld, and no fix formats the bare initializer. The comment now describes refused input. The code itself still serves `Child = {` on the `=` line and a brace that starts its line, so no branch is deletable. `02-skeptic.md` flagged this comment (FC-031), and no role carried it out. | Reword the comment: a brace that starts its line takes the assigned member's column, and the members and closing brace follow the brace where it stands. Drop the clause about the comment. Comment-only; confirm with `scripts/verify-text-only.sh`. |

Why `pass, text cleanup`: finding #1 sits in a `//` comment inside a method body. It cannot reach compiled behavior, diagnostics, or rule metadata.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| FMT-UNSUP-020, assignment (`PrecheckPhase.AddLastCandidates` → `FindTriviaBehindToken(assignment.OperatorToken, assignment.Right)`) | Trailing trivia of the operator, and leading trivia of `Right.GetFirstToken()` | The deleted `WouldJoinAcrossUnjoinableTrivia(operator, Right first token)` in `NormalizeCollectionInitializerAssignment` (`initializer.GetFirstToken()`, where `initializer` is `node.Right`), `NormalizeCollectionExpressionAssignment` (`node.Right.GetFirstToken()`), and `NormalizeAssignmentValuePlacement` (`node.Right.GetFirstToken()`) | Same two trivia lists. Same predicate: `FindFirstUnsupportedOperatorTrivia` = `IsCommentTrivia \|\| IsDirectiveOrDisabledTextTrivia` = `ContainsUnjoinableTrivia`. The join is reached only from `VisitAssignmentExpression`, so every visited node is inside the precheck's input and holds the whole gap | ok |
| FMT-UNSUP-020, equals-value clause (`FindTriviaBehindToken(clause.EqualsToken, clause.Value)`) | Trailing trivia of `=`, and leading trivia of `Value.GetFirstToken()` | The deleted guards in `NormalizeCollectionExpressionEqualsValue` and `NormalizeEqualsValuePlacement` (`node.Value.GetFirstToken()`) | Same span and predicate. Every owner is covered: property, declarator, enum member, and parameter (indexer, record, primary-constructor, local-function, and lambda included). Reached only from `VisitEqualsValueClause` | ok |
| Context and detached entry points | The context copy that `FormattingPipeline.Execute` hands to the precheck, then to the phases (`ReihitsuFormatter.FormatContext`) | The phases see the same root | The precheck input equals the phases' input, so no node-level input reaches a deleted join with trivia in its gap (FMT-UNSUP-003) | ok |
| Kept front join (`NormalizeOperatorPlacement`) | Trailing trivia of the previous token, and leading trivia of the operator or `=` | Spec, FMT-UNSUP-020 "Behind the operator" (front side not in the shape) | Disjoint from the shape's span, and correctly kept. Probe `x⏎/* front */ = 1;` was left as written and stable | ok |
| FMT-UNSUP-021 (`arm.EqualsGreaterThanToken`, `arm.Expression`) | Trailing trivia of `=>`, and leading trivia of the expression's first token | No join behind an arm's `=>` (FMT-BREAK-041). The front gap is `JoinedGapUtilities.GetGap` → `SwitchArmArrow` (FMT-UNSUP-017) | Adjacent but disjoint spans | ok |
| FMT-UNSUP-022, attribute argument | Trailing trivia of `NameEquals.EqualsToken`, and leading trivia of `Expression.GetFirstToken()` | `ListElementInteriorJoinRewriter.CanJoin` → `WouldJoinAcrossUnjoinableTrivia(previousToken, token)` | Same gap and predicate. The join's refusal is kept, because it serves every other gap inside a list element (change doc, *Checked and kept*) | ok |
| FMT-UNSUP-022, using alias | Trailing trivia of `Alias.EqualsToken`, and leading trivia of `NamespaceOrType.GetFirstToken()` | No join behind the alias `=`. `OrganizeUsingDirectivesInDocumentAsync` runs no precheck (FMT-PIPE-030) and moves whole directives | – | ok |
| Reason precedence (`CreateFirstUnsupportedSyntax`) | Earliest `FullSpan.Start`. On a tie, the first listed wins (017, 018, 020, 021, 022 last) | Spec order rule: FMT-UNSUP-004 for a declarator comment (`FindCommentInDeclaration` also checks the value's first-token leading trivia), FMT-UNSUP-010 for a directive in a multi-declarator field | Agree. Pinned by `OverlapWithTheVariableDeclarationShapesGivesTheirReason` and the 004/010 rows of `UnsupportedNameEqualsCommentTests.OtherShapesKeepTheirReason` | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Comment, directive, or disabled text behind an assignment operator or an equals-value `=` | `PrecheckPhase.AddLastCandidates`/`FindTriviaBehindToken` (the only formatter owner; `rg WouldJoinAcrossUnjoinableTrivia src` shows no remaining join behind them). `RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.ContainsJoinBlockingTrivia` is the analyzer surface's own RH5111-005 guard: a wider span that includes the front gap and the construct's other trivia, kept by the change doc | One formatter owner. The RH5111 guard is a different surface, and the spec justifies it |
| Gap behind a switch arm's `=>` | `PrecheckPhase` (021) only. `JoinedGapUtilities.GetGap` owns the front gap (017) | ok |
| Gap behind a name-equals `=` | `PrecheckPhase` (022) for attribute arguments and using aliases. `JoinedGapUtilities.GetEqualsGap` → `AnonymousMemberEquals` (017; distinct parent). `ListElementInteriorJoinRewriter` holds a shared join refusal, which serves other gaps | ok |
| Retired FMT-UNSUP-019 | `rg "FMT-UNSUP-019\|AssignmentInArm" src tests`: no hits | ok |
| Code-fix gate (SHARED-FIX-010) | Per-fix `FindUnsupportedSyntax` calls (30 providers). The only formatting callers without a gate are `BlankLineCodeFixUtilities`, `DocumentationCommentCodeFixUtilities`, and RH3208. RH3208 inherits the gate of `ExpressionBodyToBlockCodeFixProviderBase`. These predate the run, are not specific to this shape, and their fixes change the document by their own edit | Pre-existing structure, unchanged by the run |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UnsupportedAssignmentCommentTests` spec examples and `EveryAssignmentOperatorIsUnsupported`, `AssignmentIsUnsupportedWhereverItStands`, `EqualsOfEveryOwnerOfAnEqualsValueClauseIsUnsupported` | Input returned unchanged, with reason and location | Formatted output, another reason, a location off the shape, or an LF-only pass | `AssertUnsupportedFor` (LF and CRLF, exact text, reason, `AreSame` tree, location span), plus `AssertStartsAt` | strong |
| `...CommentOutsideTheGap*IsSupported`, `SpecExampleCommentsOutsideTheShapeAreSupported` (020/021/022) | The other side is formatted and is a fixed point | A refusal, a second-pass change, or a phase that is not a no-op | `AssertSupported` (+ `AssertRuleResult`) | strong |
| `...InEveryDocumentationMode` (020/021/022) | FC-040: the same answer under Parse, None, and Diagnose | A mode-dependent answer | `AssertUnsupportedFor` per mode | strong |
| `AssignmentInDisabledTextIsNotChecked`, `ArmInDisabledTextIsNotChecked`, `ShapeInDisabledTextIsNotChecked` | A token in disabled text is no token | A refusal with X undefined | Both helpers, both builds | strong |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape`, `NodeThatHoldsOnlyTheValueDoesNotHoldTheShape`, `EqualsValueClauseWithoutItsOwnerHoldsTheShape`, `NodeThatHolds*` (021/022) | FMT-UNSUP-003 locality, both sides (FC-041/FC-028) | A value target refused, or a holder formatted | Direct `FindUnsupportedSyntax`/`FormatNode`/document and context entry points, with a beside-method formatted | strong |
| `OutputOf*IsNotUnsupported` (020/021/022) | FMT-UNSUP-005: no phase produces the shape (FC-038) | The second pass refuses the first pass's output | Format twice, LF and CRLF, plus stability | strong |
| `CommentJoinTests`/`DirectiveJoinTests` `...IsRefusedBeforeTheJoin` | The whole pipeline refuses before the join | Output changed, or wrong reason | Local `AssertRefusedAsUnsupported` (exact text and reason; LF only) | adequate. The line-ending axis is covered by the `UnsupportedAssignmentCommentTests` rows on the same inputs |
| Code-fix rows (RH7101, RH2001, RH3202/RH3203, RH5206, RH5301, RH5304, RH5111, RH5201) withheld; RH5301/RH5303/RH5305 still offered | SHARED-FIX-010 gate on the formatted node (FC-043/FC-041) | Fix offered, or withheld | `VerifyNoCodeFixIsOffered` (with diagnostics marked) / `VerifyCodeFixWithEndOfLine` (one pass, re-analysis, Fix All) | strong |
| Retargeted carriers (`_ = (`, `new P⏎/* c */ {`, `return new int[]`, FMT-INDENT-034/023, FMT-BLANK-018/020, FMT-BREAK-030 examples) | Unchanged behavior on inputs that are still supported | An output change | `AssertRuleResult` / `AssertPhaseKeepsInput` | strong. Written from spec text and green on the branch before the production change (`06`, `09`), so they pin `main`'s output |

Visible output is decided (check 10): every expected value that the diff changes from formatted to refused, or from fix offered to fix withheld, is covered by the maintainer answers in `00-lead.md` (escalation 10, 1B, 2A, 3A, batch 2 Q2 B) and decision 7 (reasons, delegated). The retargeted carriers assert `main`'s output for still-supported inputs, so no visible output changes. No rule page changed.

## Failure classes dismissed
- FC-001, FC-002: the spans agree (*Guard scope*). "Behind" and "every owner" are explicit conditions of the walk.
- FC-003: both sides are tested: in front of and behind the operator, one line and across lines, with no trivia, inside the value, and behind `ref`.
- FC-004: the deleted refusals are reached only by nodes that hold the whole gap, inside the precheck's input.
- FC-005, FC-019, FC-020, FC-021: directives, `#pragma`, `#region`, `#if` groups in both builds, and `/** */` mid-line are covered.
- FC-006, FC-018, FC-022 to FC-026, FC-029, FC-036: no rewrite, rename, literal, or member-kind logic is added.
- FC-007, FC-008, FC-009, FC-030, FC-043: no fix code changed. The gates take the formatted node, and the RH5304 `no-progress` on `main` becomes withheld.
- FC-010, FC-011, FC-012: covered by the closure and stability helpers. My own probe was stable on its second pass: a nested `L =⏎{` collection-initializer join, a relocated `/** d */` landing in front of `L` (outside the gap), a `when`-less arm with a line break alone, and `int P { get; }⏎⏎/** c */ = 1;`.
- FC-013, FC-014: no analyzer changed. RH5111 keeps reporting (RH5111-005).
- FC-015, FC-016, FC-042: see *Policy ownership*; the stale comment is finding #1. No other guard serves only the declared shapes:
  - `ObjectInitializerContributor` still serves the brace on the `=` line;
  - `ListElementInteriorJoinRewriter` and `NormalizeOperatorPlacement` serve other gaps;
  - FMT-BLANK-011 serves every operator.
- FC-017: see *Assertion adequacy*.
- FC-027, FC-039: the walk visits every `AssignmentExpressionSyntax`, `EqualsValueClauseSyntax`, arm, attribute argument, and alias with no stop condition. Tests cover nested arms, lambdas, initializer values of a field and a property, interpolation holes, and chained assignments.
- FC-028, FC-041: the context copy is what both the precheck and the phases receive. A bare `NameEqualsSyntax` target holds the `=` but is not matched by the walk; no caller formats one (`AGENTS.md`, *Supported inputs*), so it is dropped.
- FC-031, FC-037: the changed summaries in `PrecheckPhase` and `LineBreakAssignmentRewriter` describe the new code. Finding #1 is outside the changed methods. No `TODO`, and no issue numbers.
- FC-032: the build is trusted from the cleaner. The merge 71519cb changed no C# by hand (decision 9), and the validator runs next.
- FC-033, FC-034, FC-035: this is not a bug fix. Expected values come from the spec and the change doc, not from patched code (`06` states the prototype decided no value).
- FC-038: no phase before the line-break phase writes into the gaps, and no rewriter earlier in the line-break phase does either. I traced the documentation-comment relocation (it appends line breaks to the previous token only), the field split (refused by 004/010), attribute split and merge (they move trivia at list edges), using ordering (whole directives), and main's new interpolation handling (it keeps code as written). The `OutputOf*` tests and my probe agree.
- FC-040: covered by tests.
- FC-044: the shapes are syntactic. Nothing models a computed result.
- Tab-width matrix: no column logic changed.

## Architecture findings
_None._

## Hints
- `PrecheckPhase.FindTriviaBehindToken` has the same body as the older `FindTriviaBehindBinaryOperator` and `FindTriviaBehindExpressionBodyArrow` (operator trailing trivia, then the next node's first-token leading trivia). Each names its own shape, and no span differs, so this is not a defect. Expressing the older two through the new helper would leave one statement of "the gap behind a token" in the class.

## Follow-up candidates
_None new._ The `int P { get; } /** c */ = 1;` relocation candidate stays as `00-lead.md` records it. My probe confirms that `main`'s layout is stable and not refused.
