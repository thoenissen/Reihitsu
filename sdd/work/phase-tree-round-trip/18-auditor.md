Role: auditor
Commit: 6285aee
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Repair delta of round 1 against `fb1573e`. The head is `6285aeef6d24d0860c4e89910d86df99e3d88587`. `origin/main` is `57f0f4537d38372f412108a8adbc0c2cd8051678`, which matches `git ls-remote origin refs/heads/main` and is an ancestor of the head. The checkout is the head with a clean tree. `git diff fb1573e..6285aee` is what handovers 12–17 describe, and `scripts/lane-check.sh behavior-change` confirms the lane.

**Scope grew.** The maintainer's decisions `…-would-merge-owner.md` and `…-member-brace-owner.md` brought in three new surfaces, and I audited each of them in full, not as a delta:

- a new public `Reihitsu.Core/TokenSeparationUtilities`;
- the RH3002 code fix as a consumer of that helper;
- brace ownership moved between `LineBreakBlockRewriter` and `DeclarationBraceLineBreakRewriter`.

The spec changed as well: FMT-PIPE-021 now compares values, and FMT-STRUCT-026 and FMT-BREAK-002 were clarified.

**How I checked.** I reused the first audit's scratchpad harnesses, rebuilt against the head and against the `origin/main` snapshot:

- round trip with values, under LF and CRLF, plus idempotency;
- annotation probe;
- node-level entry points through an `AdhocWorkspace`;
- elastic-trivia scan;
- a new probe of `TokenSeparationUtilities` on 30 token pairs.

I ran no repository test.

## Status of the first audit's findings

- **#1 — closed.** Every member it named now round-trips, and the transform writes the separator line break:
  - `=> // c` then `/* d */`;
  - `// d` on its own line;
  - `/* c */ =>`;
  - local function, operator, and conversion (one path).

  Further members also hold:
  - two block comments;
  - a blank line in front of the comment;
  - a multi-line `/* a … b */`;
  - a `///` comment, which the parser keeps leading `42` even without the line break (see Hints).

  The void, `throw`, and moved-comment paths are unchanged.
- **#2 — closed.** The phase output has no line break between nested inserted `}` braces for every member I tried:
  - comment, directive (with and without `DEBUG`), and inner `else` body;
  - three levels with an inner `else`;
  - an inner `else if`;
  - a nested `if` behind a leading comment;
  - a dangling `else` led by a comment.

  The outer `}` still gets its line break in front of a comment or directive. An `if` under `while` is unaffected.
- **#3 — closed.** The phase output is `return a` and `return \U00000061`, and `return@a`, `return@"s"`, `return$@"s"`, `return.5`, `return(a)`, `return"""x"""`, `return-a`. All round-trip.
- **#4 — closed.** I compared 27 node-level combinations between the head and `main`: method, constructor with initializer, local function, generic method with a constraint, statement block, property, and event, each as a block, declaration, or accessor-list target, through `FormatNodeInDocumentAsync` and through `FormatNode` on the attached node. The outputs are byte-identical. Whole-document outputs are identical to `main` on a member-body battery: constraints, wrapped parameters, initializers, comments in front of `{`, events, lambdas, local functions. The only difference is finding #7's intended change.
- **#5 — closed.** No elastic trivia on `(`/`)` for conditional, assignment, lambda, and query operands, in an accessor, a get-only property, and an indexer. The same holds for the other created tokens I scanned.
- **#6 — closed.** Values match for interpolated literals at column 0, for plain and UTF-8 literals at column 0, for indented closing lines, and for `$$` literals. This was checked wherever the structure allowed a value comparison; see #8.
- **#7 — closed.** The change doc's *What* lists the input.
- **A1 — resolved.** `TokenSeparationUtilities.WouldMergeIntoOneToken` is the only owner (Policy ownership).
- **A2 — resolved.** `LineBreakBlockRewriter` places block and event-accessor-list braces both on the block and on the owning declaration. Nothing predicts another rewriter's reach any more.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | high | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs`, `CollectEdits` (`root.FindToken(line.Start)`) and `ApplyEdits` (trivia branch) | FC-022, FC-001 | – | PR-introduced | **What fails.** When a content line of an interpolated raw string literal starts directly with a hole's `{` (or `{{`), no text token stands at the line start. `FindToken` then returns the hole's open-brace token, and `ApplyEdits` writes the moved indentation as whitespace trivia in front of that brace. The parser reads the same text as an `InterpolatedStringText` token in front of the interpolation. So the IndentationPhase and CleanupPhase trees break FMT-PIPE-021's structure bullet.<br>**Input.** `var s = $"""` / `{x} a` / `""";` with every line at column 0. The harness reports `Interpolation@[67..70) vs InterpolatedStringText@[50..67)`, under both LF and CRLF.<br>**Failing members:**<br>- the first content line starting with a hole (`$` and `$$`);<br>- a later content line starting with a hole after a text line (`a` / `{x}`).<br>**Holding members:**<br>- a content line starting with text at column 0;<br>- a hole line inside a multi-line hole at column 0 (`2)} b`);<br>- the closing line;<br>- any literal whose closing line is indented, because the indentation is then text (`    {x} a`, and moves left and right).<br>**Origin.** This shape occurs only when the closing marker is in column 0. `ApplyEdits` has been unchanged since the first round, so the defect was present at `fb1573e` and my first audit did not enumerate it.<br>**Effect.** The final text equals `main`'s, and annotations survive.<br>**Not a repeat.** It shares #6's input axis (closing marker in column 0), but its decision is different: which token holds a moved line's indentation, not which value a moved token keeps. The lead may weigh it against escalation 12.<br>**Coverage.** No test or fixture has a hole at the start of a content line at column 0. | Give a moved line that starts at a hole's open brace its indentation as literal text, the same as the parser reads it. Add round-trip tests for the `$` and `$$` first-line members and the later-line member. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Line break behind an inserted `}` (`GetTokenBehindInsertedCloseBrace`/`GetTokenBehindIfStatement`) | The original tree's `else` keyword, the token behind the `if`, or `default` when the parent `if` braces this one (`CanWrap`) | The token that follows the inserted `}` in the output; `WrapWithBraces` uses the same `CanWrap` | Agree on 12 nesting shapes (comment, directive, `else`, `else if`, three levels, dangling `else`, `while` parent) | ok |
| Line break behind a created `return` (`LeadsWithCommentOnItsLine` plus `lineBreak`) | The expression's leading trivia up to the first `EndsLine` | The parser gives a comment in front of the first line break to the token in front | Agree for `/* */`, `//`, and multi-line comments; `///` is excluded, and the parser keeps it leading anyway | ok |
| Space behind a created `return`, the list join, and RH3002's space | Lexing the two units alone versus concatenated, trivia included | The C# lexer | Exact by construction. The 30-pair probe matches the summary, including `..`+`^` and `?`+`.` (parser-composed, no merge), `?`+`?` and `<`+`<` (merge), `a` (merge), and `@a` (no merge) | ok |
| Value of a moved raw-string token (`CreateMovedTextToken`) | Token kind `InterpolatedRawStringEndToken` | The lexer's value | Agree (values compared on every literal shape listed under #6) | ok |
| Which token holds a moved line's indentation (`CollectEdits`/`ApplyEdits`) | `FindToken(line.Start)`, then text when inside or at the start of a raw-text token, otherwise leading trivia | The parser: content-line indentation is text of the literal | Disagree when the line starts at a hole's open brace | finding #8 |
| Member-body and event-accessor-list braces (`NormalizeBodyBraces`, `VisitLocalFunctionStatement`, `VisitEventDeclaration`, `VisitBlock`, `VisitAccessorList`) | The visited node, inside the formatting root | FMT-BREAK-002 for document and node-level targets | Agree (27 node-level combinations, whole-document battery identical to `main`) | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether two tokens written tight read as other tokens | `rg -n "WouldMerge\|IsIdentifierPartCharacter\|ParseToken\(\|ParseTokens\(" src --glob '!*Test*'` finds only `Reihitsu.Core/TokenSeparationUtilities` (`ParseTokens`). Its callers are `ListElementInteriorJoinRewriter.CreateSeparator`, `ExpressionBodyToBlockConverter.CreateReturnStatement`, and `RH3002…CodeFixProvider.CreateReplacement` (×2). `UnaryOperatorSpacingUtilities` remains for the spacing rule, which asks a different question | one owner |
| Member-body and event-accessor-list brace placement | `rg -n "NormalizeOwnedBraces\|EnsureBraceOnOwnLine\|EnsureAccessorsStartOnSeparateLines\|IsMemberBody" src/Reihitsu.Formatter/Pipeline` finds them in `LineBreakBlockRewriter` only. `DeclarationBraceLineBreakRewriter` keeps type, namespace, and indexer braces, and `AccessorListLayout` keeps property and indexer accessor lists | one owner for the decided scope. Property and indexer accessor lists keep two paths, already a follow-up candidate |
| Line break behind an inserted `}` | `ControlFlowBraceTransform` (`CanWrap` shared by the wrap and the lookup) | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Reparse hook (`PhaseTreeRoundTrip`, now with `HaveSameValue`) | FMT-PIPE-021, values included | A differing `ValueText`, value type, or value (arrays element-wise) | `FindDifference` | As strong as the amended requirement. It sees #8 only if a fixture has the shape, and none does |
| `PhaseTreeRoundTripTests` value tests (7) | Values of moved raw-string tokens | A moved text token carrying its indentation as value | `ExecuteAndCompare`, `GetPhaseOutput` | Adequate for #6. None places a hole at the start of a content line (#8) |
| `StructuralTransformSeparatorTests` nested `}` (4), `return` separator (6), at-arrow comments (7), throw parentheses (3), plus two holding elastic shapes | FMT-STRUCT-026 | An unwanted or missing gap or line break; elastic trivia anywhere in the phase output | gap helpers, `AssertSameTree`, elastic scan | adequate |
| `StructuralTransformSeparatorFinalOutputTests.EveryCreatedReturnEndsUpOneSpaceInFrontOfItsExpression` / `MembersWithACommentAtTheArrowReachAStableOutput` | Final output; stability with an unspecified layout | Changed output; second-pass change | `AssertRuleResult` / `AssertSecondPassIsStable` plus the recorded reversal (decision `…-arrow-comment-reversal.md`) | adequate |
| `ReihitsuFormatterNodeBraceTests` (9) | FMT-BREAK-002 for node-level targets; FC-028 | Braces left in place, a second-pass change, or attached `FormatNode` disagreeing | `FormatNodeInDocumentAsync` (LF, CRLF, second pass) plus `FormatNode`; expected values taken from `main` | adequate |
| `RH3002…AnalyzerTests` (5 new) | RH3002-008 on `a`, `@a`, `@"s"`, a string, and a number | A missing or extra space | `Verify` | Adequate for the fix. The shared owner itself has no direct `Reihitsu.Core.Test` (see Hints) |
| `PhaseStabilityCheckTests.CheckCatchesBlankLinePhaseInsertingBlankLineAboveCommentGluedToMemberBodyBrace` | The recorded reversal stays real | The blank-line phase not reversing on that text | `AssertOnlyPhaseChanges` | adequate |

## Failure classes dismissed
These are dismissals for the repair delta. The first audit's dismissals still hold for unchanged code.

- FC-005, FC-019, FC-020, FC-021: comments and directives behind nested braces and at the arrow were traced, and the ones that failed are now closed.
- FC-006: no rewrite changes a literal or a meaning, and values are now compared.
- FC-007, FC-008, FC-009, FC-030: the RH3002 fix output is unchanged for existing inputs (239 RH3002 tests pass per `16-cleaner.md`). The new owner differs from the old private copy only for units the lexer splits alone (`..`), which a removed parenthesis cannot sit next to.
- FC-010, FC-012: the one reversal is recorded by decision. All other scratch inputs are idempotent.
- FC-011: no loop was added.
- FC-013, FC-014: no analyzer rule changed.
- FC-015: covered under Policy ownership.
- FC-023: Unicode escapes and verbatim identifiers were probed and hold.
- FC-024, FC-029, FC-036: not reached.
- FC-025: all `return` kinds share one path, and the void form holds.
- FC-026, FC-027: nesting was enumerated (see #2).
- FC-028: closed with #4.
- FC-031: the summaries of `CreateMovedTextToken`, `CreateReturnStatement`, `WrapWithBraces`, `DeclarationBraceLineBreakRewriter`, and `LineBreakBlockRewriter` describe the new code. The remaining `PropertyLayoutLineBreakRewriter` remark, "indexer accessor list laid out by DeclarationBraceLineBreakRewriter", is still true.
- FC-032: the last compiled change is `d9aa414`. The implementer ran every project there, and the cleaner reran the formatter tests and the RH3002 filter.
- FC-033, FC-034, FC-035: the node-level expected values come from `main`, as the decision requires.
- FC-037: no text-only proof is involved.

## Architecture findings

_None._ A1 and A2 are resolved as the decisions chose, and the repair adds no second owner.

## Hints
- **`///` behind a created `return`.** With a comment at the arrow, `/// d` on the line after the arrow follows `return` with no line break: the phase output is `return········/// d`. Roslyn does not lex `///` as trailing trivia, so the comment keeps leading `42` and FMT-PIPE-021 holds. Read literally, FMT-STRUCT-026's third separator bullet asks for a line break in front of any comment that leads the next token. Its opening sentence forbids one the parse does not need. The follow-up on the arrow-comment layout could settle that wording. The final output equals the other at-arrow members.
- **No direct tests for the new owner.** `Reihitsu.Core/TokenSeparationUtilities` is a new public Core helper with no tests in `Reihitsu.Core.Test`. Its documented boundaries (`..`, comment starts, the parser-composed `>`+`>`) are pinned only through consumers.
- **Harnesses** are in `/tmp/claude-0/-home-user-Reihitsu/73a9bd65-0b12-5c4b-9a2a-b0eaaa85c1fe/scratchpad/`:
  - `h/h.cs` (round trip with values), `hm/h.cs` (its `main` copy);
  - `hn/n.cs` and `hnm/n.cs` (node-level, head and `main`);
  - `he/e.cs` (elastic trivia), `ha/a.cs` (annotations), `ht/t.cs` (merge-owner probe);
  - inputs in `cases/`, including `raw6.cs`, `raw7.cs`, and `raw8.cs` for finding #8.
