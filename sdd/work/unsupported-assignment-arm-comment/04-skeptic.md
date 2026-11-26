Role: skeptic
Commit: d273108
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. My scope was the amendment `git diff 2581f64 HEAD -- sdd AGENTS.md` (spec-author commit 0106e32), read against `00-lead.md` (answers 1B, 2A, 3A; decision 5), `02-skeptic.md`, and `03-spec-author.md`. I did not redo the premise checks of `02-skeptic.md`. Fixtures went to a scratch directory outside the repository and were deleted afterwards.

## Premise

The amendment carries out the maintainer's answers.

- 1B: FMT-UNSUP-020 now covers every assignment operator, the twelve compound operators listed, wherever the assignment stands. FMT-UNSUP-019 is retired, and its input gets the new reason (change doc, arm/member section).
- 2A: the deconstruction declaration `var (a, b) = // c⏎t;` is refused. So are `(a, b) =` and `(var a, b) =`.
- 3A: the `=` of an enum member, a parameter default, and a variable declarator is in the shape. For a declarator, FMT-UNSUP-004 keeps its reason for a comment, and FMT-UNSUP-010 keeps its reason for a directive in a field with several declarators.

**Decision 5 holds: no caller formats a bare equals-value clause or a bare variable declarator.** I listed every `FormatNode*`, `FormatNodeInDocument*`, and `FindUnsupportedSyntax` call in `src/Reihitsu.Analyzer.CodeFixes` with `grep`. `grep EqualsValue` over `src/Reihitsu.Analyzer.CodeFixes` and `src/Reihitsu.Cli` finds nothing. Each target or context is one of these:

- RH5301 and RH5303: the object creation (`RH5303...CodeFixProvider.TryGetFormatTarget`, lines 46-52).
- RH5304: the member assignment whose parent is an initializer.
- RH5305: the collection expression.
- RH5206: the switch expression.
- RH5308: the conditional.
- RH5302: the logical binary.
- RH5101 and RH5102: argument lists.
- RH5403: a statement.
- RH5110: a declaration.
- RH3004 and RH5205: a lambda.
- RH3208: a local function.
- RH7101: the field in its type.
- `FluentChainCodeFixHelper`: the chain node in the statement or member that starts its line.
- RH5111 (`GetFormattingNode`/`GetFormattingContextNode`, lines 75-101): the target is an assignment, declarator, property, parameter, or enum member. The context is the chain node, the declaration's parent, the parameter list, or the enum declaration. A declarator is therefore always formatted inside the declaration that holds it.

`sdd/changes/2026-10/node-root-detached.md:177` says the initializer fixes pass "the statement, equals-value clause, or arrow clause". That is history. The current code passes the object creation or the assignment. So FMT-UNSUP-003's "an equals-value clause target is refused" changes no output a caller reaches.

**The deleted refusals are sound.** `LineBreakAssignmentRewriter` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakAssignmentRewriter.cs`) has five refusals behind the operator or the `=`:

- `NormalizeCollectionInitializerAssignment` (line 78);
- `NormalizeCollectionExpressionAssignment` (line 102);
- `NormalizeAssignmentValuePlacement` (line 188);
- `NormalizeCollectionExpressionEqualsValue` (line 219);
- `NormalizeEqualsValuePlacement` (line 253).

Each method is private and called only from `VisitAssignmentExpression` (lines 396-400) or `VisitEqualsValueClause` (lines 415-417), and each tests exactly the gap `(operator or =, first token of the value)`. Both tokens belong to the visited node. So every input that reaches one of these joins holds the shape, provided the precheck refuses every comment, directive, and disabled text in that gap, for every owner of the clause (see *Internal choices*, item 1). The front joins (`NormalizeOperatorPlacement`, line 133) are kept, as the change doc says.

**The repository's own source holds none of the new shapes.** A `grep` of `src` for a comment behind `=` or a compound operator, and for a directive or comment on the line behind a trailing `=`, finds nothing. So `FormatterSelfHostingTests` is not affected.

## Mechanism and candidates

_N/A._ This is a behavior change. The candidate owners of an `EqualsValueClauseSyntax` in Roslyn are `VariableDeclaratorSyntax` (field, event field, local, `using`/`fixed` statement, `for` initializer, `const`), `PropertyDeclarationSyntax`, `EnumMemberDeclarationSyntax`, and `ParameterSyntax`. `ParameterSyntax` includes indexer parameters, record and primary-constructor parameters, local-function parameters, and lambda parameters with defaults. The spec's four categories cover all of them. The other `=` tokens are on the spec's other side or belong to another shape:

- `NameEqualsSyntax` of a using alias and of an attribute argument (03's output question 2);
- the anonymous-object member and `let` clause, which FMT-UNSUP-017 refuses.

## Breaking inputs

Failure classes that reach the amendment: FC-038 (no phase produces the shape), FC-039 (an inner shape inside a declaration's excepted initializer value), FC-041 (a node-level input that holds only part of the shape), FC-042 (spec examples), and FC-043/FC-009 (code fixes).

**FC-038, new owners.** No phase I know moves trivia into the gap behind an enum member's, a parameter's, or a declarator's `=`:

- documentation-comment relocation leaves a comment in front of the `=` (02 probed `int P { get; } /** c */ = 1;`);
- the field split is refused for any comment or directive in a multi-declarator field (FMT-UNSUP-004/010);
- the front join of `=` refuses a comment and is kept.

I found no new member.

**FC-041, operand-only inputs that must stay offered.** The change doc names RH5301 and RH5303. RH5305 belongs to the same class: it formats the collection expression behind a commented `=`, which holds neither the operator nor the `=`. Add one still-offered case for it, such as `int[] L { get; } =⏎    // a⏎    [1,⏎2];` with the collection expression as target. No existing test pins that case: `grep` finds no RH5305 test with a comment behind `=`.

**Spec examples I re-ran** with `scripts/format.sh` on this branch (no production change yet). Both outputs match the spec text exactly:

- FMT-INDENT-023, the complex element initializer `Dictionary<int, int> D = new() { /* c */ { 3, 4 } };`, written as in the spec: the elements go to column 45 and `}` to column 41.
- FMT-INDENT-034 example 1, `x = ( // c⏎y + 3);` and `1 =>⏎3 + 4,`: the result is `y + 3);` in column 12 and `3 + 4,` in column 24.

I checked the CLI locations in the change doc and in the FMT-UNSUP-020 examples by counting: (3,24), (6,13), (12,29), (5,14), (7,30), (5,22), (3,9), (6,1), (4,1), (8,1), (7,25), and the spec's (12,13) and (3,9). All are correct.

**Existing tests the amendment breaks or retargets.** These come on top of `02-skeptic.md` and `03-spec-author.md` (*Coverage first*: update these, do not duplicate them).

1. **Tests that run `LineBreakPhase` alone and pin a refusal this change deletes.** They feed refused input straight to the phase, so they fail as soon as the guards go. Delete each case, or move it to a precheck test under FMT-UNSUP-020. This corrects `02-skeptic.md`, which said `CommentJoinTests:84` "still passes": that test runs the phase alone (`CommentJoinTests.ExecutePhase`, line 36).
   - `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/CommentJoinTests.cs`:
     - `AssignmentValueIsNotCollapsedIntoTrailingComment` (line 81, `x = // note`);
     - `VariableInitializerIsNotCollapsedIntoTrailingComment` (line 111, `var value = // note`).
     - `AssignmentOperatorIsNotCollapsedIntoTrailingComment` (line 96) is a front gap and stays.
   - `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/DirectiveJoinTests.cs` (phase alone, line 53):
     - `AssignmentValueIsNotCollapsedAcrossDirective` (`x =⏎#if DEBUG`);
     - `VariableInitializerIsNotCollapsedAcrossDirective` (`var value =⏎#if DEBUG`).
     - `AssignmentOperatorIsNotCollapsedAcrossDirective` and `PropertyInitializerOperatorIsNotCollapsedAcrossDirective` put the directive in front of the `=` and stay.
   - `tests/Reihitsu.Formatter.Test/Unit/LineBreaks/JoinSeparatorPhaseTests.cs`, `CommentInTheGapStillRefusesEveryJoin` (line 440): the `var x = // x⏎1;` case at line 454. The `// w` and `// n` cases stay.
   - `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/LeadingTriviaMultiLineDetectionTests.cs`, `TernaryWithLeadingCommentLineStaysSingleLine` (`var x =⏎// note⏎cond ? a : b;`, phase alone): the join behind `=` now crosses `// note`. Retarget it so the comment stands above the statement; the test is about the ternary.
   - `tests/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakPhaseBlankLineTests.cs` around line 40 (`Child =⏎⏎/* c */ {`), already in 02's list. It is a phase-alone test as well.
2. **Tests that pinned the replaced FMT-INDENT-034 compound-operator examples.** These now fail because the input is refused or the fix is withheld. Retarget them to the new examples:
   - `tests/Reihitsu.Formatter.Test/Regression/Indentation/ContinuationLineIndentationTests.cs`:
     - `SpecExampleValueKeptOnItsOwnLineByACommentIsIndentedOneLevel` (line 32);
     - `SpecExampleContinuationInsideACollectionElementIsIndentedFromTheElementsLine` (line 76);
     - `SpecExampleContinuationOfAnAssignmentInsideALogicalExpressionIsIndentedFromTheStatementsLine` (line 446);
     - `ContinuationOfAnAssignmentInsideAConditionIsIndentedFromTheStatementsLine` (line 487);
     - `ContinuationOfAContinuationLineIsIndentedOneMoreLevel` (line 528).
   - `tests/Reihitsu.Analyzer.Test/Formatting/RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.cs` and `tests/Reihitsu.Analyzer.Test/Formatting/RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.cs`: `VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart` in each (`(x += // c⏎3) > 0`). The fix is now withheld, so retarget the input to `( // c⏎x + 3) > 0`, as the spec now shows.
3. **Parameter default:** `tests/Reihitsu.Formatter.Test/Regression/Indentation/ParameterContinuationLineAlignmentTests.cs:371` (`internal void M(int value = // keep`). The input is now refused.
4. **Precheck tests on the retired requirement:** `tests/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedAssignmentInArmCommentTests.cs`.
   - Its other-side rows at lines 216-219 (`x += // c` as a statement, in an argument, and in a collection expression outside an arm; and in a lambda block inside an arm) are now refused.
   - Line 198 (`A = x |=⏎#pragma`) and the rest keep refusing with the new reason. Every test that asserts the old reason text changes.
   - `UnsupportedBinaryOperatorCommentTests.cs:423-424` (`i += /* c */ 1`, `n ??= /* c */ 1` as "not refused under FMT-UNSUP-011") stay rows of FMT-UNSUP-011's other side, but the whole input is now refused. Assert them through FMT-UNSUP-020, or drop them.
5. **No change:** I checked these and their result stays the same.
   - `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests` lines 1230, 1450, 1625, and 1917: already no fix (RH5111-005).
   - `RH7309...InitializationOrderTests.cs:1114` and `RH7103...Tests.cs:1998`: already `VerifyNoCodeFixIsOffered`.
   - `RH3004UseLambdaSyntaxMultiLineTokenAndDirectiveTests` lines 1409, 1445, 1483, 2536, 2568, and 2580: the target is the lambda, which holds no `=`.
   - `ReihitsuFormatterNodeColumnTests` lines 221 and 627: the chain targets hold no `=`.
   - `RH5303`/`RH5304` tests with `var d = /* a⏎b */`: the multi-line comment is trailing trivia of the declaration's `=`, outside the object-creation and member-assignment targets.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` / `FindUnsupportedSyntax` | Must change. One walk over assignment expressions and equals-value clauses, listed after FMT-UNSUP-004 through 018. FMT-UNSUP-019's walk is removed |
| `LineBreakAssignmentRewriter` | Must change. Delete the five refusals behind the operator or the `=` (lines 78, 102, 188, 219, 253). Must stay: `NormalizeOperatorPlacement`'s refusal, which is the front gap |
| Other formatter phases | Not involved |
| Code fixes | No code change. Withheld through SHARED-FIX-010 wherever the formatted node or context holds the shape. Besides the change doc's list, this also reaches fixes that format a member with a parameter default (RH3202/RH3203/RH3208, RH5110, RH5401, RH5417, RH5419), a primary-constructor parameter default (empty-type fixes), and a chain whose line's statement holds the shape (`FluentChainCodeFixHelper`). The change doc's "Every code fix whose formatted node holds…" sentence covers these; its list is "commonly", not exhaustive. RH5301, RH5303, and RH5305 must stay offered where only the operand is formatted |
| Analyzers, rule pages, CLI code, `Reihitsu.Core` | Not involved |
| Specs | See *Internal choices* |

## Refuse option

The run is itself the refusal, decided by the maintainer (1B, 2A, 3A). No other simpler option exists. The amendment declares no new shape beyond the answers. The using-alias and attribute-argument `=` stay supported, and are asked as 03's output question 2. I agree with A there: neither gap has a join in `LineBreakAssignmentRewriter` (it visits no `NameEquals` other than the anonymous member's), so declaring them would delete nothing.

## Output questions

No new ones. I agree with 03's two questions and recommendations: 1A, the reason `a comment or directive behind an assignment operator or the = of a declaration`, and 2A, the name-equals stays supported.

## Internal choices

1. **FMT-UNSUP-020, the "Every equals-value clause" bullet, reads narrower than the requirement needs.** The bullet says: "Behind a variable declarator's `=`, FMT-UNSUP-004 refuses a comment with its own reason, so this requirement gives its reason there for a directive and disabled text." Read literally, the shape at a declarator covers only directives. That reading contradicts the spec's own FMT-UNSUP-003 example: the equals-value clause target of `var d =⏎/* a */ new List<int> { 1 };` holds no declaration, so FMT-UNSUP-004 does not refuse it, and the example has it refused "with FMT-UNSUP-020's reason". The narrow reading would also leave the deleted join in `NormalizeEqualsValuePlacement` meeting a comment in any input that holds a declarator's clause without its declaration, and a `//` comment there would comment out the value. Recommendation: the spec author words it as "the gap belongs to the shape for every owner. Where the input also holds the declaration, FMT-UNSUP-004's reason is given for a comment and FMT-UNSUP-010's for a directive in a field with several declarators (the order rule)". The test implementer should add one precheck row: a `VariableDeclaratorSyntax` or clause target with a comment behind `=`, refused with FMT-UNSUP-020's reason. No visible output depends on this, because no caller formats a bare declarator or clause.
2. **Change doc, *Code fixes*: name RH5305** among the fixes that stay offered (FC-041 above), or leave it covered by the general sentence. I recommend naming it, because a test pins it.
3. **`02-skeptic.md`'s "still pass" line for `CommentJoinTests:84` is wrong** (*Breaking inputs*, item 1). The test implementer should use the list above instead.

## Issue goals

No change beyond decision 4 in `00-lead.md`.

## Architecture findings

_None._

## Follow-up candidates

_None new._ The `int P { get; } /** c */ = 1;` candidate stays as `00-lead.md` records it.
