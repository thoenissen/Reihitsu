Role: auditor
Commit: 84726e2
Verdict: findings

My prompt carried no intended fix, no suspected cause, and no hoped-for verdict.

## Scope

This is a repair-delta audit of CI repair round 1 (range 2ef59f5..84726e2), measured against the earlier audit of 7b22e79 (`git show 2231fe0:sdd/work/blank-line-rule-list/06-auditor.md`). Head `84726e232885b84a69b0f4400353b39dcfa8577e` is the checkout. `origin/main` resolves to `99ae644d69a2f520c3a8786fdb601389819ff267` (the same SHA from `git ls-remote`), and that commit is an ancestor of the head.

The repair diff matches the handovers:
- It deletes `HasEndRegionDirectiveInTrailingTrivia`, `EnsureBlankLineBeforeEndRegionsInTrailingTrivia`, and their dispatch from `BlankLineTriviaBoundaryRewriter` (decision 18, `10-implementer.md`).
- It adds four tests to `RegionDirectiveLineAboveTests` (decision 19, `11-test-implementer.md`).
- Everything else is `sdd/work/`.

`src/` holds no other change between 7b22e79 and 2ef59f5 apart from the comment fix 96e4b45, so the earlier audit's evidence still holds for everything the repair did not touch. Scope did not grow, and no spec changed.

`scripts/lane-check.sh bug-fix` reports `no lane`. The only reason is the run note `squad/notes/2026-11-03-blank-line-rule-list.md`, which classifies as Tooling. Every other path classifies as Text, Test, or Production. Decision 20 records this, and it is the lead's decision.

Evidence gathered in this audit. All fixtures and the instrumented tree are in the scratchpad, outside the repository.

- **Reachability of the deleted path.** I instrumented a copy of the head tree: `BlankLineTriviaBoundaryRewriter.VisitToken` throws `PROBE-TRAILING-REGION` when a token's trailing trivia holds a `#region` or `#endregion`.
  - The full formatter suite passed 4406/4406 with no probe hit.
  - The CLI formatted 12 new adversarial fixtures with no probe hit. Each fixture puts region directives next to an earlier rewrite that moves trivia:
    - unbraced `if`/`else`/`while`/`for`/`foreach`/`using`/`lock` bodies (brace insertion)
    - all seven expression-bodied member kinds and accessor bodies
    - multi-declarator fields (field splitting)
    - braced, unbraced, and mixed switch sections
    - using directives inside regions, at file and namespace level (reordering)
    - a `///` comment behind source and a `///` comment above `#endregion` (doc-comment relocation)
    - attribute lists split and merged on a type, a method, a parameter, and an accessor
    - region directives inside an argument list, a ternary, a collection initializer, an invocation chain, a lambda body, an enum, a base list, and a constructor initializer
    - a `do … while` and an `if … else` across a region
  - The earlier audit's instrumented run (analyzer 6411, CLI 251, and the repository's 1843 files) covers the analyzer and code-fix callers.
  - No caller reaches the path, so deleting it changes no reachable output.
- **Mutation of `BlankLineEditor.cs:98`.** I disabled the whitespace skip in `IsMissingRequiredBlankLineBeforeRegionDirective` by appending `&& trivia.Count < 0` to its `while` condition. With that change:
  - the four new tests passed (4/4);
  - the whole formatter suite passed (4406/4406);
  - a rewriter-level probe test failed. It runs `BlankLineRegionDirectiveRewriter` alone through `BlankLineRegionDirectiveRewriterTests.ApplyRewriter` on the input of `WhitespaceOnlyLineAboveRegionCountsAsBlankLine` and asserts the input comes back unchanged.
  - With the skip restored, the same probe test passed. See finding #1.

## Status of the earlier audit's items

The earlier audit (7b22e79) had no numbered findings, so this audit's first finding is #1. Its hints:

- Hint 1 (the trailing-trivia `#endregion` path is unreached and its remark overclaims): **closed**. The path and its remark are deleted.
- Hint 2 (duplicated line-start walk): **superseded**. The copy in the deleted method is gone. `FindLineStartIndex` and the loop in `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives` remain, as on `main`.
- Hint 3 (region columns inside expression braces): unchanged. It is lead follow-up candidate 3.
- Hint 4 (`IsAdjacentToNonFormattableLine` has no production caller): unchanged. Decision 16 dropped it.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter.Test/Regression/BlankLines/RegionDirectiveLineAboveTests.cs`: `WhitespaceOnlyLineAboveRegionCountsAsBlankLine`, `WhitespaceOnlyLineAboveEndRegionCountsAsBlankLine`, `WhitespaceOnlyLineBetweenCommentAndRegionCountsAsBlankLine`, `WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceCountsAsBlankLine` | FC-017 (with FC-033: the harness normalizes the scenario away) | – | PR-introduced | **What the tests claim.** Each summary says a whitespace-only line above a region directive counts as the required blank line and that "no second blank line is inserted". Decision 19 added them to pin the whitespace skip at `BlankLineEditor.cs:98`.<br>**Why the tests cannot fail.** Each test asserts the output of the full pipeline. `BlankLineCollapser` is the last rewriter of the same `BlankLinePhase`. It collapses every run of two or more blank lines in a token's leading trivia to one, unconditionally. So a reader that ignores the whitespace-only line inserts a second blank line, the collapser removes it within the phase, and the final output is byte-identical.<br>**Proof by mutation.** With the skip disabled, all four tests and all 4406 formatter tests pass. A rewriter-level test on the same input fails (*Scope*).<br>**Class members.** The members are the tests whose invariant is "the reader sees an existing blank line and inserts nothing", asserted through the pipeline:<br>- The three tests that reach line 98 fail to pin it.<br>- `…AboveEndRegionBeforeClosingBraceCountsAsBlankLine` does not reach line 98 at all (`11-test-implementer.md`). Its claim is equally unobservable.<br>- `BlankLineBetweenIfDirectiveAfterTypeBraceAndRegionIsKept` holds, because it claims only that the blank line is kept.<br>- Tests that assert an insertion are not members, because the collapser cannot hide a missing line.<br>**What follows.** The new tests give SonarCloud the line coverage it asked for, but they do not pin the behavior. The implementer's claim that the skip is "load-bearing" holds inside the phase but not for the formatter's output. | Pin the reader where the collapser cannot mask it. One option is a test through `BlankLineRegionDirectiveRewriterTests.ApplyRewriter` (the rewriter alone) for `#region`, with the input of `WhitespaceOnlyLineAboveRegionCountsAsBlankLine` and the expected output equal to the input. I confirmed it fails under the mutation and passes on the head. A matching test for `#endregion` would go through `BlankLineTriviaBoundaryRewriter`, or a direct unit test of `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective` would cover both. Either keep the four pipeline tests as characterizations of the final output and rewrite their summaries to claim only what the final output shows (the whitespace-only line stays the one blank line and its whitespace is removed), or drop the "no second blank line is inserted" clause. Owner: test implementer. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-BLANK-005 for a region directive in trailing trivia (formatter) | Removed. No blank-line rewriter now inspects trailing trivia for `#region` or `#endregion` (`rg -n "TrailingTrivia" src/Reihitsu.Formatter/Pipeline/BlankLines/` shows only gap readers) | `#region` was never handled in trailing trivia (`BlankLineRegionDirectiveRewriter` reads leading trivia only). The analyzer reads the source text, where the parser files every directive as leading trivia | The two directive kinds now agree; before the repair only `#endregion` had a trailing path. No caller produces either shape (instrumented run above) | agree |
| SHARED-BLANK-005 for a region directive in leading trivia | Unchanged since the earlier audit (byte-identical reader and loop). `IsMissingRequiredBlankLineBeforeRegionDirective` now has one caller, `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives`, which passes `previousToken.EndsLine` | Core `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore` | As the earlier audit traced. The whitespace-skip arm is unpinned by output tests (finding #1) | agree |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| SHARED-BLANK-005 decision | `rg -n "IsMissingRequiredBlankLineBefore" src --glob '!*Test*'` finds:<br>- Core `IsMissingRequiredBlankLineBefore(bool, bool, bool)`, the one decision. Its text overload is used by `RegionDirectiveBlankLineAnalyzerBase` and `RegionDirectiveBlankLineCodeFixProviderBase`.<br>- Formatter `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective` (fact reader), called only by `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives`. That method is used by `BlankLineRegionDirectiveRewriter` and `BlankLineTriviaBoundaryRewriter`.<br>`rg -n "EnsureBlankLineBeforeEndRegionsInTrailingTrivia\|HasEndRegionDirectiveInTrailingTrivia\|EnsureBlankLineBeforeFirstEndRegion" --glob '!sdd/work/**' .`: nothing | one owner. The repair removed a second call site of the reader. Not a finding |
| SHARED-BLANK-007 skip (inactive directives) | Analyzer `IsActive: true` filter; `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives`. The copy in the deleted trailing method is gone | one fewer copy. Not a finding |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `WhitespaceOnlyLineAboveRegionCountsAsBlankLine` | Stated: the whitespace-only line counts as the blank line and no second one is inserted. Observable: the final output keeps one blank line, with its whitespace stripped | A reader that does not skip the whitespace would insert a second blank line. The same phase's collapser removes it, so the test cannot observe it (mutation: passes) | `AssertRuleResult` (exact text, LF and CRLF, second pass, `PhaseStabilityCheck`) | **weak**: finding #1 |
| `WhitespaceOnlyLineAboveEndRegionCountsAsBlankLine` | Same, for `#endregion` | Same (mutation: passes) | `AssertRuleResult` | **weak**: finding #1 |
| `WhitespaceOnlyLineBetweenCommentAndRegionCountsAsBlankLine` | Same, with the skip ending on trivia in the list | Same (mutation: passes) | `AssertRuleResult` | **weak**: finding #1 |
| `WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceCountsAsBlankLine` | Final output for `#endregion` before `}` after a whitespace-only line; it does not reach line 98 | An extra blank line from any rewriter would be collapsed in the same phase | `AssertRuleResult` | adequate as a characterization of the final output. Its summary claims more (finding #1) |

## Visible output is decided

- The expected values of the four new tests equal `main`'s output. The test implementer ran them on `99ae644`. The unchanged requirements demand those values: SHARED-BLANK-001 (a whitespace-only line is blank), SHARED-BLANK-005 / FMT-BLANK-013, and the cleanup of trailing whitespace.
- The deleted path is reached by no caller, so no reachable output changes.
- No rule page changed.

## Failure classes dismissed

- FC-001, FC-002, FC-003: the repair adds or narrows no guard. It removes a guard and a path that no caller reaches, and the guard-scope rows above show agreement.
- FC-004, FC-005, FC-019, FC-020, FC-021: the repair moves, joins, or deletes no trivia. It removes an insertion path that never runs. The adversarial fixtures carry directives next to every trivia-moving rewrite I could construct.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no code, literal, identifier, or member-kind rewrite changed. FC-025's seven expression-bodied kinds are in fixtures a02 and a12 for the reachability probe.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changed. RH5031's fix is text-based and untouched.
- FC-010, FC-011, FC-012: the remaining loop is unchanged, and the formatter suite's `PhaseStabilityCheck` passed (4406). The insert-then-collapse interplay inside the blank-line phase is what masks the reader in finding #1. On the head it does not occur, because the reader skips the whitespace.
- FC-013, FC-014: no analyzer change and no new rule.
- FC-015, FC-016: see *Policy ownership*. The trailing copy is removed. The sibling `#region`-in-trailing-trivia shape was never handled and is unreached.
- FC-028: the deletion does not touch `PrecedingTokenFacts` or node-level roots.
- FC-031: the class summary of `BlankLineTriviaBoundaryRewriter` ("blank-line boundaries before comments and #endregion directives") still holds for the leading-trivia paths. No summary, comment, spec, or document names a removed member (`rg` above). The summary of `IsMissingRequiredBlankLineBeforeRegionDirective` describes no specific caller. The new tests' summaries overclaim (finding #1).
- FC-032: after the deletion, the implementer re-ran the formatter, analyzer, CLI, Core, and architecture suites (`10-implementer.md`), and I re-ran the formatter suite.
- FC-034, FC-035: no reproduction. The new tests' expected values were checked against `main`, not against a patched copy.
- FC-037: no text-only claim is made.

## Architecture findings

_None._ The repair removes a call site and a private copy of the SHARED-BLANK-007 skip. It adds no owner or prediction.

## Hints

1. `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective` is `public static` and has one caller in its own class (`10-implementer.md` notes it). Leaving it accessible keeps the direct unit test that finding #1 suggests possible. Low value either way.
2. `scripts/lane-check.sh` classifies the run note that `squad/lead.md` section 8 step 4 requires as Tooling. After publishing, every bug-fix run whose note is on the branch will therefore fail its lane check, as decision 20 records. This belongs in the run note, not in this run.
