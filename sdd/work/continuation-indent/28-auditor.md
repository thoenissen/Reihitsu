Role: auditor
Commit: c14c4bd
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `9df98aa` (the commit audited in `22-auditor.md`; its code tree is `f434b0b`'s). Head `c14c4bd` (`git rev-parse HEAD`). `origin/main` = `git ls-remote origin refs/heads/main` = `ab547b7cdad3279b17a9cfb87428dff6e56314b7`, an ancestor of HEAD. Diff `git diff 9df98aa HEAD -- . ':(exclude)sdd/work'`: 15 files (4 spec/change docs, 5 test files, 6 production files), matching handovers 23 to 27: `PatternLineStartUtilities` now owns `IsIntroducedByCaseLabel` and `StartsOwnLine` reads it; `LineBreakInitializerRewriter`, `ContinuationJoinRewriter`, `BinaryOperatorLineBreakRewriter`, and `ReihitsuFormatterHelpers` read that one copy; `ComputeDocumentLineStartColumn` derives from `ComputeDocumentColumn`; the `NormalizeBinaryOperatorPosition` summary is fixed. One byte change no handover names: `ContinuationJoinRewriter.cs` gained a UTF-8 byte order mark. `.editorconfig` says `charset = utf-8-bom`, so this is harmless (see Hints). Scope did not grow: no new surface, and the spec changes clarify FMT-BREAK-017/022/041 and FMT-UNSUP-019 as decisions 29 and 30 record.

Evidence: the head's Release CLI (`src/Reihitsu.Cli/bin/Release/net11.0`, built 02:38 after the last `src` commit `9282dcd`), plus CLIs built from temporary worktrees of `ab547b7` (`main`) and `9df98aa` (pre-repair), both removed afterwards. Fixtures in the scratchpad (`a28/p1.cs`, `p2.cs`, `p3.cs`, `arm.cs`) were formatted twice under LF and CRLF: every one stays stable on the second pass, and the CRLF output equals the LF output. The pre-repair → head diff over the fixtures changes exactly the case-label rows and nothing else.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member the finding listed now joins, with the line break behind the combinator: `case P and⏎{`, `case 1 or⏎{`, `case 1 or not⏎{`, `case not⏎{`, `case P and not⏎{`, a nested `… or P and⏎{`, and `case 4:⏎case not⏎{…} and P:`. Each gives the same layout as the brace-behind form (`p1.cs`, `p3.cs`), so one tree now has one fixed point. The members that held still hold, unchanged from pre-repair: `o is P and⏎{` and `when p.B is (not)⏎{` keep the brace below; the subpattern `B: P or⏎{` and `B: not⏎{` keep it below; `case (8 or⏎{…})` and `case (1 or not⏎{…})` keep it below; `case 5 or⏎(…)` keeps `(` below; the switch-arm `P and⏎{` keeps it below. List patterns (`case [1] or⏎[…]`, `case not⏎[…]`, `a is⏎[…]`) and positional patterns without a type (`case (1, 2) or⏎(…)`, `t is not⏎(…)`) join in every context. Neither `StartsOwnLine` nor FMT-BREAK-022 starts them on their own line, so the two agree (`p2.cs`, byte-identical pre-repair and head). The architecture part is closed too: the case-label condition has one definition (`PatternLineStartUtilities.IsIntroducedByCaseLabel`), read by FMT-BREAK-022's placement, both joins, and the node-level root fact. |
| 2 | closed | FMT-UNSUP-019's first condition names object-initializer, `with`-initializer, and index-initializer members, with refused examples inside an arm and an anonymous-object member and formatted examples outside both. `EachInitializerMemberInsideAnArmOrAMemberIsUnsupported` has 6 rows (3 initializer kinds × arm/member), `InitializerMemberOutsideAnArmOrAMemberIsSupported` has 3 rows, and the spec example asserts its location (14,38). The change doc lists the input. |
| 3 | closed | The `unless …` clause is back in the first sentence, and the type-test `is` exception stands as its own sentence. It describes the code. |
| 4 | closed | `ComputeDocumentLineStartColumn` returns `ComputeDocumentColumn(token) - (token.SpanStart - position)`. The two forms are equal: the old code decremented for the BOM when `position > 0`, the new one when `SpanStart > 0`. With `text[0] == '﻿'` on line 0, the skip loop passes the BOM, so `position ≥ 1` whenever `SpanStart > 0`, and both conditions coincide. The BOM rule now lives only in `ComputeDocumentColumn`. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A `case` label introduces a pattern (FMT-BREAK-022) | `IsIntroducedByCaseLabel`: climb through `BinaryPatternSyntax`/`UnaryPatternSyntax` parents only. True when the reached node's parent is `CasePatternSwitchLabelSyntax`, or when the node has no parent and the facts carry `CaseLabelPattern` | FMT-BREAK-022's text: own pattern plus operands of `and`/`or`/`not`; not through a parenthesized pattern, a subpattern, or `when` | Same boundary: `p3.cs` rows for `when`, parentheses, and subpattern stay apart; combinator and `not` rows join | holds |
| Brace keeps its own line, joins (`StartsOwnLine`) vs placement (`LineBreakInitializerRewriter.VisitRecursivePattern`) | `{` whose parent clause belongs to a `RecursivePatternSyntax` without type and positional clause, multi-line, and not case-introduced | `VisitRecursivePattern`: any multi-line property clause, `keepOpenBraceInline` from the same `IsIntroducedByCaseLabel` | Same condition, one owner. A typed or positional pattern's `{` is never the first token behind a gap | holds |
| Case-label fact for a rebuilt chain (`BinaryOperatorLineBreakRewriter.VisitBinaryPattern`) | Fact taken on the original chain root before `base.VisitBinaryPattern`, passed as `chainConstruct`. In `NormalizePatternChain`, `rightFirstToken` comes from the (possibly rebuilt) `node`, so the climb ends at that node: no parent → `IsRootIn` reads `chainConstruct`; unchanged node → the real parent answers | Same predicate on the intact tree | A rebuilt chain gets the fact it had before the rebuild, and an unchanged chain is answered by its real parent, which gives the same result. A chain under a parenthesized pattern, a subpattern, an `is`, or a list element gets `None` | holds |
| Case-label fact in `ContinuationJoinRewriter.IsJoinedGap` | `StartsOwnLine(token, _context.RootConstruct)` on the tree `Visit` receives, whose parents are intact (one `DescendantTokens` pass, no rebuild before the check) | Same predicate | Agrees. At node level only the root lacks a parent, and the entry point's fact answers for it | holds |
| Node-level root fact | `ReihitsuFormatterHelpers`: `target is PatternSyntax && IsIntroducedByCaseLabel(target, None)` on the document tree | Same predicate | One owner. The only pattern a code fix formats is RH5306's `ListPatternSyntax`, whose elements' chains have the list as parent and so get `None`, as in the document | holds |
| Line-start column (`ComputeDocumentLineStartColumn`) | Leading space, tab, or BOM of the token's line, then the column via `ComputeDocumentColumn` | `ComputeDocumentColumn` | Equal to the previous result (finding 4 row) | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A `case` label introduces a pattern | `rg IsIntroducedByCaseLabel`: one definition in `PatternLineStartUtilities`, readers `LineBreakInitializerRewriter.VisitRecursivePattern`, `BinaryOperatorLineBreakRewriter.VisitBinaryPattern`, `PatternLineStartUtilities.StartsOwnLine`, `ReihitsuFormatterHelpers`. Other `CasePatternSwitchLabelSyntax` hits (`SwitchCaseWhenLineBreakRewriter`, `CaseWhenClauseContributor`) decide the `when` clause, and the analyzer hits (RH3003, RH5020, cognitive complexity) are unrelated | one owner |
| Multi-line pattern delimiter on its own line | `StartsOwnLine` (joins), `LineBreakInitializerRewriter.VisitRecursivePattern`/`VisitParenthesizedPattern` (placement). Both share the case-label condition. `BinaryOperatorLineBreakRewriter.IsMultiLineDelimitedPattern` decides whether a chain is wrapped, a different decision that audit 22 found in agreement with FMT-BREAK-017 | one owner per decision |
| Document column counting (BOM zero) | `rg uFEFF src/Reihitsu.Formatter`: the BOM rule is in `ComputeDocumentColumn` only; `ComputeDocumentLineStartColumn` keeps `﻿` only in its whitespace skip; `LayoutComputer.GetOpeningQuotesColumnBeforeRawStringShift` is FMT-RAW-005's own rule, on `main` | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PatternChainLayoutTests.SpecExampleBraceOfAPropertyPatternInACaseLabelJoinsItsCombinator` | Case-label `and⏎{`/`or⏎{` join, and `is P and⏎{` keeps the brace below | brace left below the combinator in the label, or joined behind `is` | `AssertRuleResult` (exact, LF+CRLF, second pass, phase stability) | adequate |
| `PatternChainLayoutTests.SpecExampleBraceWrittenBehindItsCombinatorGivesTheSameOutput` | Both break positions give one layout | different output for the brace-behind form | same expected text, `AssertRuleResult` | adequate |
| `ContinuationJoinTests` (3 new) | `case not⏎{`/`or not⏎{` join; brace-behind form gives the same output; `when … is not⏎{` and subpattern `not⏎{` keep the brace below | wrong side joined | `AssertRuleResult` | adequate |
| `LineBreakRequirementTests.SpecExampleCaseLabelIntroducesItsPatternThroughCombinatorsAndNot` | FMT-BREAK-022's both sides | brace moved in a label, or kept inline in `when`/subpattern | `AssertRuleResult` | adequate |
| `ReihitsuFormatterNodeColumnTests.CaseLabelNotPatternTarget…` (2) | The node-level fact gives the document layout | different layout at node level | `AssertFormatsTarget` + `AssertRuleResult` | adequate. They pin the fact in `ContinuationJoinRewriter`. The `BinaryOperatorLineBreakRewriter` path with a chain as node-level root is untested, but no caller formats a bare pattern chain (*Supported inputs*, decision 33) |
| `UnsupportedAssignmentInArmCommentTests` (new and updated rows) | Initializer members are refused inside an arm or member and formatted outside both; the reported location is right | refusal outside, or none inside | `AssertUnsupportedFor`/`AssertSupported` + location assert | adequate |

## Visible output is decided

The changed expected values are the case-label joins (decision 29) and the FMT-UNSUP-019 refusals of initializer members (decision 30). The lead decided both under the maintainer's standing instruction (`00-lead.md`, *Maintainer answers*), and both are recorded under *Decisions taken* and in the change docs (`continuation-join.md` section 2; `assignment-comment-in-arm-unsupported.md`). No other expected value changed: the characterization rows already passed before the repair.

## Failure classes dismissed

- FC-001, FC-002, FC-015: guard scope and policy tables above; one condition now, one owner.
- FC-003: both sides of the case-label boundary are tested and probed (combinator and `not` vs parentheses, subpattern, `when`, `is`, arm).
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-038: the newly joined gaps (combinator → `{`, `not` → `{`) are FMT-UNSUP-017's, so the precheck refuses any comment, directive, or disabled text there. The joins write only whitespace.
- FC-006, FC-024, FC-025: no meaning-changing rewrite; only line breaks move.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code fix changed. No analyzer reports property-pattern braces or combinators (`rg PropertyPatternClause|BinaryPattern|UnaryPattern` in the analyzer projects: RH3002, RH3006, cognitive complexity only).
- FC-010: every probe stable on a second pass, LF and CRLF.
- FC-011: no new loop; `NormalizePatternChain` still runs once per chain root.
- FC-012: no later rewriter re-breaks a joined case-label brace (FMT-BREAK-022 leaves it in place there; probes stable).
- FC-013, FC-014: no analyzer covers the changed layout (above).
- FC-016: the class enumeration for #1 is in the previous-findings row.
- FC-017: assertion table.
- FC-022, FC-029, FC-040: no raw string, key/value layout, or documentation comment involved.
- FC-023, FC-036: no identifiers or renames.
- FC-026: replacements are still keyed by original token identity; `rightFirstToken` comes from the same `node` the replacements apply to.
- FC-027: nested chains (`… or P and⏎{`), a chain under parentheses inside a label, and a chain inside a subpattern of a label pattern are checked.
- FC-028: the node-level fact is captured from the document; the line-start column is unchanged (finding 4).
- FC-031: every changed summary (`StartsOwnLine`, `IsIntroducedByCaseLabel`, the class summary, `IsJoinedGap`, `NormalizePatternChain`, `VisitBinaryPattern`, `VisitRecursivePattern`'s comment, `ComputeDocumentLineStartColumn`, `NormalizeBinaryOperatorPosition`) describes the new code.
- FC-032, FC-035: expected values checked against the spec examples and against the pre-repair and `main` CLIs.
- FC-033, FC-034: reproductions use the finding's own shapes under LF and CRLF.
- FC-037: the text-only claim for finding 3 rests on the cleaner's `verify-text-only.sh` pass, not on grep.
- FC-039, FC-041: the walk stops at a node without a parent, which is the formatting root or the rebuilt chain, and in both cases the captured fact answers.
- FC-042, FC-044: no guard serves only a declared shape; the case-label condition is a cheap ancestor check.

## Architecture findings

_None._ Decision 29's structural change is carried out: FMT-BREAK-022's case-label condition has one statement, which every reader shares.

## Hints

- `ContinuationJoinRewriter.cs` gained a byte order mark in this round (`.editorconfig`: `charset = utf-8-bom`). Four other files the run created still have none (`QueryClauseContributor.cs`, `ContinuationLineContributor.cs`, `PatternLineStartUtilities.cs`, `JoinedGapUtilities.cs`), as do two files on `main`. No test enforces it, and nothing compiled depends on it.

## Follow-up candidates

- Pre-existing, outside the repair delta (the pre-repair and head outputs are byte-identical): in a switch-expression arm, the brace of a multi-line property pattern without a type that FMT-BREAK-022 starts on its own line lands left of its pattern. Input (`a28/arm.cs`):

  ```csharp
  return o switch
  {
      P and { A: 1,
          B: 2 } => 1,
      not { A: 3,
          B: 4 } => 2,
      _ => 0
  };
  ```

  The arm's `P`/`and`/`not` stand at column 19. The `{`/`}` stand at column 8 on `main` and at column 16 on this branch; FMT-INDENT-026 reads as the pattern's first token, column 19. Structural cause: `PatternAnchor.GetColumn` (`src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/PatternAnchor.cs`) climbs through the combinator and `not` wrappers to find the context, but its fallback returns `pattern.GetFirstToken()` of the original pattern. For a property pattern without a type, that token is the brace itself, so `RecursivePatternContributor` anchors the brace to itself and keeps whatever column an earlier step gave it. The fallback should probably read the first token of the climbed-to pattern (`current`). FMT-INDENT-026's "otherwise the pattern's own first token" allows the self-anchoring reading too, so the spec wording may need the same clarification.

## Commands

```text
git rev-parse HEAD; git ls-remote origin refs/heads/main; git merge-base --is-ancestor   c14c4bd; ab547b7; merged
head Release CLI twice over a28/{p1,p2,p3,arm}.cs (+ CRLF copies)                        all stable, CRLF = LF
pre-repair CLI (9df98aa worktree) and main CLI (ab547b7 worktree) over the same fixtures   diffs only on case-label rows (pre→head); worktrees removed
```
