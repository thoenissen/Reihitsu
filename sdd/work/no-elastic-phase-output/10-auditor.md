Role: auditor
Commit: 2889c013b6183eb121062e04d4e21d6224b5767a
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Full audit of head 2889c013b6183eb121062e04d4e21d6224b5767a against base `origin/main` abe1313690b712a5b12e3e3cde26f2b4e9b7c3f9.
- State verified: the checkout is the head SHA. `git ls-remote origin refs/heads/main` returns abe1313. `git merge-base --is-ancestor origin/main HEAD` holds.
- The diff matches the handovers:
  - Production: 3 files, as 05 names.
  - Tests: 3 analyzer test files, the `PhaseTreeRoundTrip` helper, and 2 new formatter test classes, as 04 names.
  - Specs and change doc: as 01, 03, and 08 name.
- `scripts/lane-check.sh behavior-change` confirms the lane.
- Issue: thoenissen/Reihitsu#1516 (`agent-filed`).

Executed checks (worktrees and fixtures outside the repository, all removed afterwards):

- **CLI, head vs. main.** `scripts/format.sh` ran from a head worktree and from a main worktree over the same corpus of 21 accessor-merge shapes, under LF and CRLF.
  - Shapes covered: property, indexer, and event (`add` and `remove`); one-line and several-line accessor lists, also a layout the phase changes; `param:`, `typevar:`, and no specifier; 2 and 3 lists; a first list that already holds a comma, with and without a space, and with a line break inside it; an attribute argument list `A(1,2)`; an interleaved `[return:]` list and an interleaved unspecified list; `][` with no gap; extra spaces; and an initializer on the same line and on the next line.
  - Result: byte-identical output between main and head. A second pass at head changed nothing.
- **RH5111 fix, head vs. main.** `scripts/apply-fix.sh RH5111` ran at head and at main on four properties with merged accessor lists and an initializer on the next line. The `Q` property's first list holds a comma, `R` gets the several-line layout, and `S` has unspecified lists. Results under LF and CRLF were identical between head and main. Every fixture converged.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttributeTargetFormattingRewriter.cs:116` (summary of `ShapeAccessorAttributeLists`) | – (comment layout) | – | PR-introduced | One XML-summary line is 177 characters. The rewritten sentence was not re-wrapped, while every other summary line in the file and the diff wraps at about 120. Text-only: it sits inside a `///` summary and cannot reach compiled behavior, diagnostics, or rule metadata. | Re-wrap the summary's last two lines at the file's usual width. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which commas the merge writes (`separators == Spaced` → `SeparatedList(nodes)`, i.e. elastic commas; otherwise bare `CommaToken`s with empty trivia) | Every separator of the rebuilt list in `MergeAttributeListGroup` (`AttributeTargetUtilities.cs:310-312`): commas the merge creates and commas the first list held, because the whole list is rebuilt from its elements | FMT-BREAK-032's sentence "every comma between two attributes of the merged list — created and already held — with no trivia"; SHARED-ATTR-018 for the code-fix path (`Spaced`) | Agree. The phase passes `SpacedBehindList` (`AttributeTargetFormattingRewriter.cs:136`); the structural transform passes `None` (bare, unchanged); the code fixes pass `Spaced` through the two-argument overload (unchanged, byte-identical branch) | holds |
| Whether the merge writes the space behind `]` and behind a token it leaves touching a joined token (`spacesBehindList = separators != None`; was `== Spaced`) | The merged list's trailing trivia, and the previous tokens of joined gap tokens (`GetGapTokens`: the tokens behind each member's `]` other than a member's `[`) | `writtenLineBreak` (`separators == None`), the line-break carry-over, and FMT-BREAK-031's recorded exception (exactly these two spaces) | Agree. The two predicates partition the three modes: `None` carries a line break and writes no space; both spaced modes write the two spaces and carry no line break. A comma is never the previous token of a gap token, so the bare commas of `SpacedBehindList` cannot be picked up as `unseparatedTokens` | holds |
| Whether the per-phase observer checks elastic trivia (`checksElasticTrivia = FindElasticTrivia(root) == null`) | The whole input root, with `descendIntoTrivia` | FMT-PIPE-034's "adds" (not "carries") and its non-goal about node-level inputs | Agree. The check runs only when the input carries none, so "carries" equals "adds" there | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The separators an attribute-list merge writes | `AttributeTargetUtilities.MergeAttributeListGroup` (`src/Reihitsu.Core/AttributeTargetUtilities.cs`), the only builder. Callers pass the mode: `AttributeTargetFormattingRewriter.ShapeAccessorAttributeLists` (`SpacedBehindList`), `AttributeListMergeTransform` (`None`), and `TargetAttributeListShapeCodeFixProviderBase.ApplyMergeCodeFixAsync` (two-argument overload → `Spaced`), via `AttributeListShapeUtilities.MergeAttributeLists` for the two formatter callers. No private copy found (`rg "AttributeListMergeSeparators\|MergeAttributeListGroup\(\|MergeAttributeLists\("`) | one owner |
| The space behind a comma | The horizontal-spacing phase (FMT-SPACE-008, `CommaSpacingUtilities`). The line-break phase no longer writes or implies it | one owner |
| Tokens a phase builds without explicit trivia (sources of elastic trivia) | `rg` over `src/Reihitsu.Formatter` and `src/Reihitsu.Core`. Patterns: `SyntaxFactory.Token(SyntaxKind`, `SeparatedList(` with elements only, `Identifier(`, `Literal(`, `Elastic*`, `NormalizeWhitespace`, `RemoveNode(s)`, `SyntaxRemoveOptions`, `Insert*`, separated-list `Add`/`Insert`. Results: `SwitchCaseBraceRewriter.cs:728/736` replaces both trivia lists right after `Token(kind)`. Every structural-transform token is built with explicit or `default` trivia. `RawStringLineShifts` builds tokens with explicit trivia. `RegionNamingRewriter` uses `PreprocessingMessage`. The only element-only `SeparatedList` left is the `Spaced` branch, which only the code fixes reach | no other adding site |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PhaseTreeRoundTrip.ExecuteAndCompare` (observer on every formatter fixture) | No phase output carries elastic trivia when the input carries none (FMT-PIPE-034) | Any elastic trivia, also in structured trivia, in any phase's output | `FindElasticTrivia` (`DescendantTrivia(_ => true, descendIntoTrivia: true)` + `ElasticAnnotation`) | adequate. 04 shows it turned 5 existing fixtures red before the fix |
| `NoElasticPhaseOutputTests.ElasticTriviaCheckFindsMarkersOnCodeAndDirectiveTokens` | The check is not vacuous on code tokens and on directive tokens | `FindElasticTrivia` returns null for a tree with an `ElasticMarker` or `ElasticSpace` | direct | adequate |
| `NoElasticPhaseOutputTests.EveryPhaseOutputOf…`, `MergedLists…`, `Indexer…`, `ThreeParamLists…`, `EventRemove…`, `TypevarLists…` | No phase output carries elastic trivia, each preparation sub-phase included, under LF and CRLF; the final output stays as it is | An elastic trivia in any phase, or a changed final text | `ObservedPipeline.Run` (also asserts both sub-phases are reported) + `AssertRuleResult` (exact text, idempotency, CRLF, phase stability) | adequate. The expected texts equal main's output (CLI comparison above) |
| `NoElasticPhaseOutputTests.EveryPhaseOutputOfAFileEveryPhaseRewritesCarriesNoElasticTrivia` | The same invariant on a file that 10 phases change | An elastic trivia, or a phase that the fixture no longer exercises | Asserts that each named phase changes the text | adequate; not vacuous |
| `NoElasticPhaseOutputTests.FormatNodeInDocumentAsync…` (×2), `FormatNodeAddsNoElasticTrivia…` | Node-level entry points add none to a node parsed from text (FC-028: both entry points) | Elastic trivia in the returned document or node; a changed text | Exact text (document); `Contains` of the merged list (detached node), LF + CRLF | adequate for the requirement. The detached-node test checks the merged list, not the whole text; the requirement is about trivia, which it checks on the whole node |
| `AccessorAttributeMergeCommaTests` (7) | FMT-BREAK-032: every comma of the merged list leaves the line-break phase with empty leading and trailing trivia; FMT-BREAK-031: the recorded space behind `]` stays, and an unmerged list keeps its comma space | Any trivia, elastic or not, on a separator; a changed list text | `ObservedPipeline.GetPhaseOutput("LineBreakPhase")`, `Assert.IsEmpty` on both trivia lists, exact list text, LF + CRLF | adequate. Both sides of the merge condition are covered (FC-003) |
| `RH5524….VerifyCodeFixMergesIntoAFirstListThatAlreadyHoldsAComma`, `RH5531….VerifyCodeFixMergesInlineAccessorListsIntoAFirstListThatAlreadyHoldsAComma` | SHARED-ATTR-018: the code-fix merge output stays `[A, B, D]` | A missing comma space (`[A, B,D]`) | `Verify` (batch base) | adequate. They passed before the production change (04) |
| `RH5111….VerifyCodeFixForPropertyWithMergedSetterParamListsKeepsTheCommaSpace` | A formatter-backed fix keeps its result | A changed fix result | `VerifyUnderBothLineEndings` | adequate. The result equals main's (`apply-fix.sh` comparison above) |

Visible output (check 10): every formatter expected value added equals main's output, by the CLI comparison above. They follow from requirements this run does not change in output: FMT-BREAK-024/025/032 for layout and merge, and FMT-SPACE-008 for the comma space. The run's changes to FMT-BREAK-031/032 only decide trivia that has no character in the text. The analyzer expected values follow from SHARED-ATTR-018 (its examples write `[A, D, B]`) and equal main's results. No rule page changed. No undecided output.

## Failure classes dismissed
- FC-001, FC-002: the predicates changed are exhaustive mode switches over a closed enum, not span guards (see *Guard scope*).
- FC-003: covered. Merged and unmerged lists are both tested; a comma that is created and one that was already held are both tested.
- FC-004, FC-005, FC-019, FC-020, FC-021: `CanMergeAttributeListGroup` (unchanged) refuses any comment or directive inside the group and any comment in front of a gap token. The change writes no new gap a comment could sit in. The corpus held no comment cases, because the guard is untouched and the merge refuses them.
- FC-006, FC-022–FC-027, FC-029, FC-036: no rewrite of meaning, literals, identifiers, or member kinds. The commas' text is unchanged.
- FC-007, FC-008, FC-009, FC-018, FC-030:
  - The code-fix merge path (`Spaced`) is byte-identical in behavior.
  - RH5111 converged and matched main under LF and CRLF.
  - Fix All is not affected.
- FC-010, FC-011, FC-012: a second CLI pass at head is a no-op under LF and CRLF. No loop or neighbouring-phase interaction changed: the bare comma's gap is decided only by the spacing phase. Line-break rewriters after `AttributeTargetFormattingRewriter` (binary, chain, ternary, switch-when) do not touch attribute lists. `LineBreakListRewriter`'s look-ahead through `_attributeFormatter` reads only whether an element spans lines.
- FC-013, FC-014: no analyzer and no new rule are involved, and formatter output is unchanged.
- FC-015, FC-016: one owner (see *Policy ownership*); no sibling adding site.
- FC-017: see *Assertion adequacy*.
- FC-028: both node-level entry points are tested; the RH5111 path matches main.
- FC-031: the summaries of `MergeAttributeListGroup`, both enum members, and `ShapeAccessorAttributeLists` describe the new code. The overload summary ("the separators the code fixes write (`Spaced`)") is still true. Only the layout of one line is off (finding #1).
- FC-032: the analyzer suite was run after the compiled change (05). After the merge of main (thoenissen/Reihitsu#1813, a new argument-list line-break rewriter), only the build and focused formatter tests ran (09). The validator's default run covers the rest; the new rewriter does not reach attribute-list commas.
- FC-033, FC-034, FC-035: the expected values come from main's output (skeptic 02, and this audit's head-vs-main comparison), not from patched code. The shapes are the requirement's own example and the skeptic's matrix.
- FC-037: not applicable; the change is not proven text-only.
- FC-038–FC-043: no unsupported shape is declared or touched.
- FC-044: no guard models a computed result.

## Architecture findings
_None._ The shared merge serves three callers through a mode parameter of one implementation. The phase's mode and the code fixes' mode now differ only in the comma trivia, which the change doc's decision *The code fixes' merge keeps its output* records.

## Hints
- The code-fix merge (`Spaced`) still writes elastic commas. Its comma space comes from Roslyn's code-action post-processing (skeptic 02, lead decision 6). This is unchanged and no defect; it is noted only because it is the last element-only `SeparatedList` in `Reihitsu.Core`.

## Follow-up candidates
_None._
