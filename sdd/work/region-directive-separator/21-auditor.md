Role: auditor
Commit: 5c7d495
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1486, `00-lead.md` (decisions 17 and 18), `11-auditor.md`, `16-auditor.md`, `17-spec-author.md`, `18-test-implementer.md`, `19-cleaner.md`, `20-spec-checker.md`, `git diff cbcc78f..5c7d495`, and the counterparts named below: FMT-PIPE-019, FMT-PIPE-024, FMT-TAB-002, FMT-EOL-002, the `AGENTS.md` `RegionFormattingPhase` row, the `RegionFormattingPhase` summary, the test-class summary of `ReihitsuFormatterNodeRegionTests`, and the `main` version of the `FormatNodeInDocumentWithContextAsync` remarks.

## Scope

This is a delta audit of the structural change for decisions 17 and 18, against `cbcc78f`. Head `5c7d495` is checked out and the tree is clean. Base `f8dc7ef7a71bffbf373675d0613e32a77a1ef5a6` equals `git ls-remote origin refs/heads/main` and is the merge base, so `origin/main` is merged.

The diff matches the handovers:
- `pipeline.md` FMT-PIPE-028, the `region-formatting.md` non-goal, and three places in the change doc (17).
- Three tests and two helpers in `ReihitsuFormatterNodeRegionTests` (18).
- Comment-only edits in `ReihitsuFormatter.cs` and `FormattingContext.cs` (19, text-only proof PASS).
- Decisions 17 and 18 in `00-lead.md`.

`scripts/lane-check.sh behavior-change` confirmed the lane. No predicate, guard, or compiled production line changed. The first two audits' evidence for production code is byte-identical and reused.

Status of the earlier findings:

- #4 — **closed.** Every location it listed no longer lists the preparation phase's effects.
  - FMT-PIPE-028 now says the requirement exempts region directives "from the region phase and from nothing else", and that the preparation phase does to a region directive at node level what it does to any directive (FMT-PIPE-019).
  - The non-goal points to FMT-PIPE-028.
  - The change doc's *What* bullet, its code-fix paragraph, and its decision section name the phase as a whole.
  - The line-ending member that failed is now pinned at node level for all three entry points, as the next section shows. A member that #4's enumeration of statement locations did not name still makes the overclaim; it is reported as #6.
- #5 — **closed.** Both members it listed are gone:
  - the misplaced reason ("…still becoming four spaces, because this entry point serves code fixes");
  - the incomplete list of skipped region requirements. All three remarks now say "no region requirement (FMT-PIPE-028)".

  The repair removed the reason instead of moving it, which drops a clause `main` has. That is reported as #7, which repeats #5.
- The architecture finding of `16-auditor.md` is **resolved in structure**. The spec states the node-level rule once, in FMT-PIPE-028, and refers to the preparation phase as a whole. The non-goal, the three remarks, and the `FormattingContext` summary refer to FMT-PIPE-028 without restating it. Two places outside the structural change still state the rule in the old form (#6).

Evidence:
- Line-ending tests (the line breaks counted by hand):
  - `ForeignLineBreakInput` has seven line breaks: five predominant and two foreign.
  - The namespace variant has eleven line breaks with two foreign.
  - The predominant ending is therefore the same whether it is counted over the node or over the document (FMT-PIPE-007).
- `NormalizeLineEndings` followed by `.Replace("¶" + endOfLine, foreign)` puts the foreign break exactly behind the two directives.
- Both directives lie inside the target: `#region bar` is leading trivia of `int`, and `#endregion // Stale` is leading trivia of `}`.
- Focused tests were not re-run, because no finding depends on them (`18-test-implementer.md`: 22 passed).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | low | `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeRegionTests.cs` line 14 (class summary, unchanged since round 1); `sdd/changes/2026-11/region-directive-separator.md` line 13 (*What*, third bullet, edited in this round) | FC-016, FC-031 | #4 (and #1) | PR-introduced | Same mechanism as #1 and #4: a statement says what node-level formatting does to a region directive's text, and it disagrees with the preparation phase. Decision 17 limits every such place to "no FMT-REGION requirement applies; the preparation phase still runs". Two places say more. **(a)** The test-class summary says the node-level entry points "keep the text of region directives". The class itself holds tests showing that they do not: a tab between the tokens becomes four spaces (`…ReplacesTabsBehindRegionKeywordsOfTheTabExample`, `…ReplacesTabBetweenHashAndRegionKeyword`), and the line break behind the directive is rewritten (the three new `…RewritesForeignLineBreaksEndingRegionDirectives` tests). Neither earlier enumeration named this location. **(b)** The change-doc bullet says "the separator, the capitalization, the `#endregion` comment, and whitespace at the end of the line stay as written". This round replaced the explicit exception "The one change left is … a tab between a directive's tokens becomes four spaces" with "The preparation phase still runs", so the bullet now claims, without an exception, that a tab separator stays as written. The FMT-PIPE-028 tab example contradicts that: `#region`, a tab, `bar` becomes `#region    bar`. Later in the same change doc, the code-fix paragraph states the tab case correctly, so the doc is inconsistent with itself, not wrong as a whole. Output is not affected. Locations that hold: FMT-PIPE-028 and its two examples, the non-goal, the three `ReihitsuFormatter` remarks, the `FormattingContext` property summary and constructor parameter, the `RegionFormattingPhase` summary (it speaks about the phase only), the `AGENTS.md` row ("the node-level entry points skip it"), the change doc's code-fix paragraph and decision section, the change doc's "The region phase owns the line form" (whole-document separator; true, because no line break lies in the separator), and every per-test summary in `ReihitsuFormatterNodeRegionTests` and the RH7104/RH7105 tests (each is bounded to its own input). | (a) Restate the class summary in the form decision 17 sets, for example: "Tests that the node-level entry points apply no region requirement (FMT-PIPE-028), while the document and syntax-tree entry points apply them". (b) Drop "the separator … stay as written" and say only what FMT-PIPE-028 says (no region requirement is applied), or keep the list and add "apart from what the preparation phase does (FMT-PIPE-019)". |
| 7 | low | `src/Reihitsu.Formatter/ReihitsuFormatter.cs` `<remarks>` of `FormatNodeInDocumentWithContextAsync` (lines 247–249) | FC-031 | #5 | PR-introduced | On `main`, the remark gives the reason for keeping accessor blocks and accessor lists: "…rather than becoming expression-bodied members, because this entry point serves code fixes." Round 1 attached that reason to the tab clause (#5). This round removed it instead of putting it back, so the remark now gives no reason for any of its three exemptions. The two sibling remarks keep theirs: `FormatNodeInDocumentAsync` says "because this entry point serves code fixes; for the same reason, it applies no region requirement", and `FormatNode` says "Like every node-level entry point". The PR deletes a correct clause from a public member's XML documentation. | Restore the reason as `FormatNodeInDocumentAsync` words it: "…rather than becoming expression-bodied members, because this entry point serves code fixes; for the same reason, no region requirement is applied (FMT-PIPE-028)." |

## Guard scope
_N/A — the change alters no predicate, guard, or exemption. Production edits are comment-only (`19-cleaner.md`, `scripts/verify-text-only.sh` PASS). The guard table of `11-auditor.md` for `FormattingContext.FormatsRegionDirectives`, `GetRegionDescription`, `GetEndRegionText`, `AddReplacementIfChanged`, and `GetRegionPairs` stands unchanged._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Tabs between directive tokens become four spaces | Code: `TabReplacementPhase.ReplaceDirectiveTriviaTabs`, the only owner. Statements: FMT-TAB-002 (owner) and FMT-PIPE-019. FMT-PIPE-028's tab example and the change doc's code-fix paragraph cite it as an example, not as a list | one owner; consistent |
| Line break that ends a directive uses the detected line ending | Code: `LineEndingNormalizationPhase`, the only owner. Statements: FMT-EOL-002 (owner) and FMT-PIPE-019. Nothing in the run excludes it any more | one owner; consistent; pinned at node level by the three new tests |
| What node-level formatting does to region directives | FMT-PIPE-028, the only statement. It is referenced by the non-goal, FMT-PIPE-024, the three `ReihitsuFormatter` remarks, the `FormattingContext` summary and constructor parameter, the `RegionFormattingPhase` summary, and the `AGENTS.md` row. It is restated in the old form by the `ReihitsuFormatterNodeRegionTests` class summary and the change-doc *What* bullet | #6 |
| Why the node-level entry points keep shapes (code fixes) | FMT-PIPE-011 and FMT-PIPE-028 (owners). The `FormatNode` and `FormatNodeInDocumentAsync` remarks and the `FormattingContext` summary give the reason. The `FormatNodeInDocumentWithContextAsync` remark has lost it | #7 |
| Node-level skip of the region phase (code) | `FormattingContext.FormatsRegionDirectives` is read only by `RegionFormattingPhase` and set to `false` at `ReihitsuFormatter.cs` 131, 203, and 297 | one owner (unchanged) |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncRewritesForeignLineBreaksEndingRegionDirectives` | At node level, the line break that ends a region directive takes the detected line ending (FMT-EOL-002 through FMT-PIPE-019), and no region requirement applies (FMT-PIPE-028) | A foreign break kept behind `#region bar` or `#endregion // Stale`; `Bar` capitalized; `// Bar` written; any other byte; second pass different. Checked under CRLF and under LF | `AssertNodeInDocumentWithForeignLineBreaks`: exact document text, both predominant endings, second pass | adequate. The test implementer showed it fails under the reading that keeps the break. The helper cannot normalize the mixture away (FC-033): the input is built after `NormalizeLineEndings`, and `SourceText.From` keeps it |
| `…FormatNodeInDocumentWithContextAsyncRewritesForeignLineBreaksEndingRegionDirectives` | Same, through the context entry point with namespace `N` as context | Same | Same helper with context | adequate. It closes the member that `16-auditor.md` traced but did not run |
| `…FormatNodeRewritesForeignLineBreaksEndingRegionDirectives` | Same, on the detached class | Same | `FormatNode`, exact text, both endings, second pass | adequate |

Visible output is decided. Every expected value the change adds follows from unchanged requirements and equals `main`:
- the predominant ending behind the directives: FMT-EOL-002 and FMT-PIPE-019;
- lower-case `bar` and the stale `// Stale` kept at node level: maintainer answer 6 and FMT-PIPE-028;
- the expanded property: FMT-PIPE-011 and the existing FMT-PIPE-028 example.

No rule page changed.

## Failure classes dismissed
- FC-001, FC-002, FC-003: no predicate changed. FC-003 for the node/whole-document boundary is already satisfied by the paired tab tests (`16-auditor.md`). The whole-document line-ending counterpart is `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten`.
- FC-004, FC-005, FC-019, FC-020, FC-021: no trivia handling changed.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no syntax rewrite changed.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code-fix provider changed. An RH7105 variant with mixed line endings is not testable through `Verify`, as `18-test-implementer.md` explains; the entry point the fix calls is covered.
- FC-010, FC-011: no production change. The new tests run a second pass.
- FC-012: the preparation phase and the region phase do not undo each other at node level, because the region phase does not run there.
- FC-013, FC-014: no rule decision or formatter decision changed.
- FC-015: every policy has one code owner. The leftover statement duplication is #6.
- FC-016: reported as #6.
- FC-017: see *Assertion adequacy*.
- FC-028: all three node-level entry points are pinned with the same expected value. `FormatNode` and `FormatNodeInDocumentAsync` agree.
- FC-031: reported as #6 and #7. The other changed summaries describe the code.
- FC-032: there is no compiled change after the test runs.
- FC-033, FC-034, FC-035: the expected values come from the requirements. The fixture is the counterexample of `16-auditor.md` #4 with the tab replaced by one space. The scratch edit in `18-test-implementer.md` showed that the tests can fail.
- FC-037: the comment-only claim is proven by `scripts/verify-text-only.sh` (19).
- Tab-width matrix: no column is measured from a region directive's interior.

## Architecture findings
The structural change resolves the architecture finding of `16-auditor.md` in the spec and in production comments: FMT-PIPE-028 is the one statement, and it names the preparation phase as a whole. #6 shows that the same rule (what node-level formatting does to a region directive's text) is still restated in two places outside the change. Both earlier enumerations missed the first, the test-class summary of `ReihitsuFormatterNodeRegionTests`. This round reintroduced the second, the change-doc *What* bullet, by dropping an explicit exception. Both are documentation. The output and the spec are correct. Applying decision 17's form to those two places completes the structure; no new component is involved.

#7 repeats #5 in the same remark. The rule it touches (node-level entry points keep shapes because they serve code fixes) is stated in FMT-PIPE-011 and FMT-PIPE-028 and repeated in three remarks. The repair of #5 dropped one repetition's reason instead of aligning it with the other two.

## Hints
- `ReihitsuFormatter.cs` line 249 now runs to about 130 characters ("is applied (FMT-PIPE-028). Version-dependent rules follow … clamped"), while the next line is short. The wrap is uneven, but the formatter does not rewrite comment text, and it is not a convention violation that a check enforces.
- `BuildRegionTrivia` / `BuildUndescribedEndRegionTrivia` (decision 16) and follow-up candidates 2 and 3 in `00-lead.md` are unchanged.
