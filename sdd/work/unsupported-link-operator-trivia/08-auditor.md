Role: auditor
Commit: 1011371115ba27a58804f4bab5bc0ac3b35e2d1d
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of head 1011371 against base `origin/main` 31d2931af8e2894fba9e66a281edd23b4de9ae6c. State verified: the checkout is 1011371. `git fetch origin main` resolves to 31d2931, and 31d2931 is an ancestor of the head. The diff (31 files) matches the handovers 01–07: the spec and change-doc files, the five production files (`FluentChain`, `PrecheckPhase`, `ChainLineBreakRewriter`, `MethodChainAlignmentContributor`, `AttachedPartGapUtilities`), the test files in 03, and `sdd/work/`. `scripts/lane-check.sh behavior-change` confirms the lane. I relied on the cleaner's Release build and on the roles' focused test results. I ran no test project.

Probes I ran with `scripts/format.sh` on fixtures under the scratchpad (outside the repository), all on the head:

- **Doc and block comments in front of operators split by whitespace** (`c.N(1) /// d⏎?⏎.N(2)`, `/** d */⏎!⏎.N(2)`, `/** d */⏎?⏎.N(2)`, `s /* a */⏎?⏎.Length`, `t⏎/// d2⏎!⏎.Length`), plus a split later link with `!.` behind it, an interpolation hole, an index of `?[ ]`, and a two-link `?.` chain. Result: every operator was joined and every comment kept its place. A second pass changed 0 files.
- **Edge members of the shape:**
  - These are refused with FMT-UNSUP-015's reason at the comment: `a?.B! /* c */ .B`, `a?.B! /* c */ ?.B`, `w!! /* c */ .Length`, `a? /* c */ .V += 1`, `a?.B? /* c */ .B`, `(a!)? /* c */ .B`, and `y? /* c */ .B` inside an `is` pattern conjunction.
  - These are formatted, because they are front gaps: `a?[0] /* c */ .B` and `a?.B /* c */ ?.B`.
  - `a!? /* c */ [0]` keeps FMT-UNSUP-014's reason.
- **Overlap with FMT-UNSUP-004:**
  - `[A(nameof(s? /* c */ .Length))] private int _f;` gives `a comment between a variable declaration's own tokens` at (3,18).
  - The same attribute on a local function gives FMT-UNSUP-015's reason at (5,22).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedLinkOperatorCommentTests.cs` (no test); `PrecheckPhase.FindUnsupportedSyntax` candidate order | FC-003 | – | same requirement | FMT-UNSUP-015's overlap paragraph says that when FMT-UNSUP-004 refuses the same comment, FMT-UNSUP-004's reason and location apply, and that the two meet only outside a value. No test covers that case. The test implementer listed it under *Not covered* because they found no input. The spec checker then found one: a link operator in an attribute argument of a field. I confirmed the code is correct today: `[A(nameof(s? /* c */ .Length))] private int _f;` reports `a comment between a variable declaration's own tokens` at `/* c */`. No test pins it, though. Today, moving the FMT-UNSUP-004 candidate behind FMT-UNSUP-015's would still pass every test of the run. The FMT-UNSUP-010 overlap test guards only the field-directive member. | Add a test (tags FMT-UNSUP-004 and FMT-UNSUP-015). For `[A(nameof(s? /* c */ .Length))] private int _f;` (also with two declarators), assert `VariableDeclarationCommentReason` at `/* c */`. Add the other side: the same attribute on a member that is not a variable declaration, such as a local function or a method, gives `LinkOperatorCommentReason`. |
| 2 | low | `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs`, summary of `FindTriviaInsideLinkOperator` | FC-031 | – | PR-introduced | The summary says the precheck "refuses exactly the gaps the line-break phase joins inside a link operator". The precheck actually checks every member access and member binding of its input. The line-break phase joins only gaps of links that `FluentChain` collects in a chain it normalizes. Lead decision 9 and the implementer's handover call the precheck's set a superset. For example, a binding inside a postfix `++` is checked but never joined. This is text only and cannot reach compiled behavior. | State it as one shared token definition, `FluentChain.GetLinkOperatorTokens`, so every gap the phase can join inside a link operator is refused. Drop "exactly". |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| FMT-UNSUP-015 refusal (`PrecheckPhase.FindTriviaInsideLinkOperator`) | For every `SimpleMemberAccess` and `MemberBinding` descendant whose first operator token lies in `node.Span`: the trailing trivia of each operator token but the last, and the leading trivia of each but the first. Both come from `FluentChain.GetLinkOperatorTokens`. | `ChainLineBreakRewriter.CloseInnerGaps`, which joins every gap between consecutive `link.OperatorTokens` that holds a line break, now with no trivia check. `link.OperatorTokens` comes from the same `GetLinkOperatorTokens`. | Same token source and the same gaps (operator tokens are adjacent, so a gap is exactly one token's trailing trivia plus the next token's leading trivia). The precheck set contains the phase set: every chain link is a member access or binding inside the input, and a binding link exists only when its conditional access is walked inside the input. | holds |
| FMT-UNSUP-003 boundary for a partial input | `node.Span.Contains(tokens[0].Span)`. Only ancestors of the first token contain it, and the binding is a descendant of the node, so the node holds the whole conditional access or member access. | `FluentChain.Collect` builds a binding link only with `pendingConditional != null`, that is, when the conditional access lies inside the walked input. A member access holds its own `!`. | Agree: a node that holds only the `.` gets no link from the chain model and no refusal from the precheck. Tests: `BranchOfAConditionalAccessAloneDoesNotHoldTheShape`, `NullForgivingOperatorAloneDoesNotHoldTheShapeOfItsMemberAccess`, `PartsOfANullForgivingConditionalAccessAloneDoNotHoldTheShape`. | holds |
| Binding link tokens: `GetLinkOperatorTokens(memberBinding)` replaces `GetConditionalTokens(pendingConditional, …)` in `Collect` | `memberBinding.OperatorToken.GetPreviousToken().Parent` as the conditional access | The `pendingConditional` that `Collect` passes down | Traced for `a?.B`, `a?.B.C`, `a?.B()`, `a?.B![0]`, `a?.B?.C`, `a?.B = 1`, and `u!?.B`. The binding a `pendingConditional` reaches is always the leftmost node of that access's `WhenNotNull`, so the token in front of its `.` is that access's `?`. | holds (identical links; core, analyzer, and formatter suites green per 04) |
| FMT-UNSUP-014 `!` exclusion (`AttachedPartGapUtilities.StartsLinkOperator`, rewritten) | The token after the `!`, skipping a conditional-access `?`. Then the `.`'s parent, and whether the `!` is `GetLinkOperatorTokens(link)[0]`. | The old check: `.` with a `MemberAccess` parent, or a conditional `?` followed by `.` | Equivalent. A `!` directly in front of a member access's `.` is always that access's `Expression` operator. A `!` directly in front of a conditional access's `?` is always its `Expression` operator. A binding's `.` always follows a `?`. Probe: `a!? /* c */ [0]` keeps FMT-UNSUP-014's reason. | holds |
| `CloseGap`'s trivia check (kept) | The dot-to-name gap and the front gap of the first link | FMT-UNSUP-015 excludes both gaps | Disjoint from the shape | holds (kept by necessity) |
| `FluentChainLink.HasInnerLineBreak(true)` (RH5201-003, kept) | The inner operator gaps, which ignore a comment-held break | Not the precheck: analyzers do not consult it | Analyzer only. RH5201 reporting is unchanged by design (decision 3). | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which tokens form a link operator | `rg -n "OperatorTokens\|GetLinkOperatorTokens\|StartsLinkOperator\|GetNullForgivingInFront" src` (non-test). Owner: `FluentChain.GetLinkOperatorTokens`. It uses the private `GetConditionalTokens` and `GetNullForgivingInFront`; `GetConditionalTokens` also builds the `?[` attached-part tokens, which are FMT-UNSUP-014's. Readers: `FluentChain.Collect` and `CollectMemberAccess`, `PrecheckPhase.FindTriviaInsideLinkOperator`, `AttachedPartGapUtilities.StartsLinkOperator`. Consumers of the result: `ChainLineBreakRewriter.CloseInnerGaps`, `FluentChainLink.HasInnerLineBreak`, `MethodChainAlignmentContributor.Contribute`, RH5201 and RH5112. | One owner; the earlier private copy in `StartsLinkOperator` is gone (decision 10) |
| Is the gap trivia a comment, directive, or disabled text? | `PrecheckPhase.FindFirstUnsupportedOperatorTrivia`, which FMT-UNSUP-008, -011, and -015 share. `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` is still used by `CloseGap`, `IsFirstLinkBlocked`, the root-line check, and `HasInnerLineBreak(true)`, all outside the shape. | No new copy |
| Aligning a link operator's later tokens | `AlignRestOfRun` is deleted. `rg -n "AlignRestOfRun\|CloseInnerGaps\|TryCloseGap"` over `src`, `sdd/specs`, `documentation`, `AGENTS.md`, and `squad` finds only the kept `CloseInnerGaps`. | Deleted as the change doc lists |
| Reason text | `PrecheckPhase.LinkOperatorCommentReason`. The test copy `FormatterTestsBase.LinkOperatorCommentReason` follows the repository pattern for every reason. | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UnsupportedLinkOperatorCommentTests` refusal rows (spec examples, 60-cell gap × trivia matrix, doc comments × 3 modes, conditional groups × builds, contexts, members) | FMT-UNSUP-015: the input comes back unchanged, with the reason and the location of the first trivia | Output differs, nothing is reported, the reason is wrong, or the span is wrong | `AssertUnsupportedFor` (LF and CRLF; exact text; reason; tree identity; location) | adequate |
| `CommentOutsideTheLinkOperatorIsSupported`, `CommentNextToATokenThatIsNoLinkOperatorIsSupported`, `ConditionalGroupAroundTheWholeStatementIsSupported`, `SpecExampleCommentsOutsideTheLinkOperatorsAreSupported` | The other side is not refused, and the output is a fixed point | Refused, or a second pass changes the output | `AssertSupported` (LF and CRLF, plus a second pass) | adequate for the boundary. They do not pin output, which the run does not change. |
| `CommentInTheGapOfAnotherShapeKeepsItsReason`, `DirectiveInsideALinkOperatorInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration`, `FirstShapeInTheTextIsReported` | FMT-UNSUP-001 ordering and the FMT-UNSUP-010, -014, and -008 overlaps | The wrong reason or location | `AssertUnsupportedFor` | adequate. The FMT-UNSUP-004 overlap is missing (#1). |
| Node-level tests (`NodeLevelEntryPointsRefuse…`, `BranchOfAConditionalAccessAlone…`, `NullForgivingOperatorAlone…`, `PartsOfANullForgiving…`, `FindUnsupportedSyntaxAnswersForABuiltNode`, `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck`) | FMT-UNSUP-003 and FMT-UNSUP-007; FC-028, -041, -043 | A partial node is refused; a whole node is accepted; a built node and an attached node answer differently | Direct entry-point calls with exact text and location | adequate |
| `OutputOfFormattedLinkOperatorsIsNotUnsupported` | FMT-UNSUP-005: formatted output of split operators beside comments is not refused, and it is stable | The second pass refuses the output, or changes it | Two `FormatSyntaxTree` passes, LF and CRLF | adequate for its invariant (see *Hints*) |
| `ChainLinkLayoutTests.EveryTokenOfALinkOperatorSplitByWhitespaceJoinsTheLineOfItsFirstToken` | FMT-BREAK-019 and SHARED-CHAIN-006 after the deletions: whitespace-only splits join in every context | Any output difference | `AssertRuleResult` (exact output, idempotency, each phase run alone) | adequate. The expected value equals `main`'s output and is demanded by the unchanged SHARED-CHAIN-006 and FMT-INDENT-029. |
| Rewritten refusal tests in `ConditionalAccessWrapBetweenOperatorTokensTests`, `ChainGapJoinTests`, `ChainLinkLayoutTests` | FMT-UNSUP-015 | As above | `AssertUnsupportedFor` | adequate. The removed expected outputs are replaced by the maintainer's declare answer. |
| Code-fix tests (RH5201, RH5112, RH3202 ×2, RH2001 ×2, RH7101 ×2) | SHARED-FIX-010, RH2001-007, RH7101-007: no fix is offered, and the diagnostic stays | A fix is offered | `VerifyNoCodeFixIsOffered` (LF and CRLF where parameterized) | adequate |
| `RH2001…VerifyFixIsOfferedForACommentInFrontOfALinkOperatorOfTheInitializerValue` | The front gap does not withhold the fix | No fix is offered, or the output differs | `VerifyCodeFixWithEndOfLine` (exact) | adequate. `Source /* three */?.Length` is demanded by FMT-SPACE-006 and FMT-SPACE-013 (unchanged) and equals `main`. |
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveInsideALinkOperatorIsReportedAsUnsupported` | CLI: exit 2, file unchanged, exact stderr line | Any difference | `Program.Main` | adequate. The reason text is covered by maintainer answer 1A. |

Visible output (check 10): every expected value the diff adds or changes is covered.

- The refusals and the reason text are covered by the maintainer's triage answer (declare the shape) and by batch-1 answer 1A, both recorded in the change doc's `Decisions` and in `00-lead.md`.
- The two supported outputs are demanded by unchanged requirements.
- No rule page changes.

## Failure classes dismissed
- FC-001, FC-002: the guard spans agree; see *Guard scope*.
- FC-004: handled. The join inside an operator now relies on the precheck. The precheck reads the same tokens, so a comment can never reach `CloseInnerGaps`. Probes show comments in front of split operators are kept.
- FC-005, FC-019, FC-020, FC-021: covered by the matrix (directives, `#region`, `#if false`, `#nullable`, `#line`, empty groups, comments of every form on both sides).
- FC-006: no rewrite changes meaning. Joins carry only whitespace.
- FC-007: no fix changes; fixes are only withheld.
- FC-008: a withheld fix registers no action, so no Fix All path runs on the shape.
- FC-009, FC-043: every formatter-delegating fix asks `FindUnsupportedSyntax` about its formatted node or context. `FluentChainCodeFixHelper.IsFormattingRefused` checks the context. Covered by the fix tests.
- FC-010, FC-011, FC-012: idempotency is covered by `AssertRuleResult`, `AssertSupported`, and a second-pass probe. No loop or insert/collapse pair changed.
- FC-013: no analyzer changes. RH5201-003 stays analyzer-only (decision 3).
- FC-014: no new rule.
- FC-015, FC-016: one owner (see *Policy ownership*). The sibling shapes `?[` and `!?[` stay FMT-UNSUP-014's. The dot-to-name gap is answer 2A's follow-up.
- FC-017: see *Assertion adequacy*.
- FC-018, FC-023, FC-036: no renaming.
- FC-022: literals are untouched. Interpolation holes are covered.
- FC-024, FC-025: member kinds reach the precheck only through `DescendantNodesAndSelf`, which is kind-agnostic. Expression-bodied property, property initializer, field, and local function are covered.
- FC-026: no token re-resolution after an edit. Joins are applied in one rewrite, keyed by original token.
- FC-027, FC-039: nested occurrences are covered by the context rows (argument list of another link, `?[ ]` index, lambda, local function in a lambda, switch arm, interpolation hole).
- FC-028, FC-041: see the node-level rows.
- FC-029: no key/value layout.
- FC-030: no fix's formatted node changes.
- FC-031: comments updated in `ChainLineBreakRewriter`, `MethodChainAlignmentContributor`, `AttachedPartGapUtilities`, and the test-class summary. #2 is the one remaining imprecision.
- FC-032: the validator runs the default set on the head.
- FC-033, FC-034, FC-035: not a reproduction. Expected values were checked against `main` (03).
- FC-037: no text-only proof is claimed.
- FC-038: no phase writes trivia into an operator gap. Doc-comment relocation and attached-part joins were probed and kept comments outside the operator. The second pass is stable.
- FC-040: three documentation modes are covered.
- FC-042: `AlignRestOfRun` and the inner-gap trivia check are deleted. `rg` over the formatter's line-break, blank-line, spacing, documentation, indentation, and cleanup phases found no other guard serving only the shape. The kept guards are justified in the change doc's *Checked and kept*.
- FC-044: a conditional group inside an operator is refused in every build because its directive stands in a gap in every build. No build is modelled.
- The tab-width matrix: no column computation changed. The deletion removes a column write that only the declared shape reached.

## Architecture findings
_None._ The change removes a decision that two phases shared: the line-break phase refused the join, and the indentation phase placed what the refusal kept. The precheck and the chain model now read one statement of the operator tokens.

## Hints
- `OutputOfFormattedLinkOperatorsIsNotUnsupported` asserts non-refusal and stability, but not that the comments survive. Suppose a phase before the line-break phase moved a comment into an operator gap, and the unguarded join then broke the syntax. The second pass would return the broken tree unchanged with `Unsupported == null`, and the test would pass. I ran the rows' inputs through the CLI on the head: every comment kept its place and the output was correct. So this is no defect today, only a weaker net than an exact-output assertion.
- `FluentChain.GetLinkOperatorTokens` allocates a small array for every member access during the precheck. The cost is negligible: it runs once per file or once per fix registration.

## Follow-up candidates
_None_ beyond the 2A follow-up already in `00-lead.md`.
