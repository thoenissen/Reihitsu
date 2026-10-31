Role: auditor
Commit: 3815e6ef919f361b029874c6f346522f750bb29a
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from these sources:
- `00-lead.md` and the previous audit of the addition, `05-auditor.md`;
- `06-spec-author.md` through `11-spec-checker.md`;
- `squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md` and the change doc `sdd/changes/2026-10/fixed-keyword-spacing.md`;
- FMT-SPACE-002, FMT-SPACE-012, FMT-SPACE-013, RH6001, RH6006, and the diff e6c896f..3815e6e.

## Scope

- **Head:** 3815e6e, checked out, clean tree.
- **Base:** `origin/main` = 802afbf (the SHA in my prompt). It is an ancestor of the head.
- **What I audited:** the delta e6c896f..3815e6e. It answers finding #7 with the maintainer's option A. It is not a repair round, but I audited it as a delta against `05-auditor.md`. The scope stayed where the decision put it: one case label in `KeywordSpacingRule.IsSpacedKeyword`, FMT-SPACE-012, the change doc, and the tests.
- **Diff against the handovers:** it matches `06-` to `11-`.
- **Results I trusted:** the cleaner's build (0 warnings) and its formatter and analyzer runs (3267 and 6307 passed).
- **Probes:** I ran `scripts/format.sh` twice over fixtures in the session scratchpad (`fx12/A.cs`, `fx12/B.cs`). I also ran it once from a temporary worktree of e6c896f over `fx12/B0.cs`, then removed the worktree.

## Previous findings

- **#7** (`05-auditor.md`): **closed**. Both members it named now hold:
  - `fixed(int* p = items)` becomes `fixed (int* p = items)`.
  - `fixed/* c */(…)` becomes `fixed/* c */ (…)`.
  - RH6001 does not report either output: its guard skips a keyword whose trailing trivia holds whitespace.
  - Probe and tests: `OneSpaceAfterFixedKeyword`, `OneSpaceAfterFixedKeywordBehindBlockComment`, and `VerifyFormatterFixesGluedFixedKeyword`.
  - The holding members are unchanged.
- **#1 to #6:** closed in earlier audits. The delta touches none of their code.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | medium | `sdd/changes/2026-10/fixed-keyword-spacing.md:25-27` (*No exception for the fixed-size buffer*) and `:13` (*What*); `src/Reihitsu.Formatter.Test/Regression/Spacing/HorizontalSpacingRequirementTests.cs` (`FixedSizeBufferKeepsOneSpaceAfterFixedKeyword`, `SeveralSpacesInFixedSizeBufferBecomeOne`) | FC-017, FC-003 | – | PR-introduced | **The change alters the output for a fixed-size buffer. The change doc says it does not, and no test pins the altered output.**<br><br>The change doc says the buffer keeps "the same output the formatter gives today", and `06-spec-author.md` claims no output change for the buffer. That holds only when the gap after `fixed` is pure whitespace. When a `/* */` comment is glued to the type, the output changes:<br>- Before (KeywordSpacingRule returned `null` for `fixed`, so the gap was only collapsed): `public fixed/* c */int _b[4];` stays as written. Checked by formatting with the e6c896f build.<br>- At head (`IsSpacedKeyword` gives 1, and FMT-SPACE-013 applies it behind the comment): the same input becomes `public fixed/* c */ int _b[4];`.<br>- In the same way, `public fixed /* c */int _c[4];` becomes `public fixed /* c */ int _c[4];`.<br>- Control: `public/* c */int _a;` stays as written at head. That is the `null` path that `fixed` used before.<br><br>The new output is the specified one: FMT-SPACE-012 now says exactly one space after `fixed`, and FMT-SPACE-013 puts it behind the comment. It is stable on a second pass.<br><br>What is wrong:<br>- The accepted change doc records the decision with a false claim, and leaves this output change out of *What*. The maintainer accepted it in advance on that text.<br>- The two buffer tests that stand for the decision were green before the change (`07-test-implementer.md` lists them as "green"). They stay green if `FixedKeyword` is removed from `IsSpacedKeyword`, so they cannot falsify the decision.<br>- The only buffer input the change alters, a comment glued to the type, has no test. That is the other side of the boundary that the whitespace-only tests cover.<br><br>Class (a `fixed` token's right-hand gap: context × gap content), against FMT-SPACE-012/013, LF and CRLF:<br>- Failing (changed output, not recorded, untested): buffer `fixed/* c */int`; buffer `fixed /* c */int`.<br>- Holding, recorded or covered by the `if` parity the change doc states:<br>&nbsp;&nbsp;- statement `fixed(` and `fixed/* c */(` (tested);<br>&nbsp;&nbsp;- statement `fixed /* c */(` and `fixed/* a *//* b */(`, which become `… (` (probed, as `if`);<br>&nbsp;&nbsp;- statement and buffer with only whitespace (tested; unchanged for the buffer);<br>&nbsp;&nbsp;- a tab (the preparation phase replaces it first);<br>&nbsp;&nbsp;- a line break or `///` before the next token, which `AreSeparatedByEndOfLine` excludes (FMT-SPACE-001). | Owners to re-derive:<br>- **Test implementer:** add a FMT-SPACE-012/FMT-SPACE-013 regression test, `public fixed/* c */int _b[4];` becoming `public fixed/* c */ int _b[4];`. It is red without `FixedKeyword` in `IsSpacedKeyword`.<br>- **Spec author:** in the change doc's buffer decision, say that a comment glued to the buffer's type gets one space behind it, as for the statement. Drop the "same output … today" claim, and list the shape under *What*.<br><br>The change doc is not merged yet, so it can still be corrected. Since the maintainer accepted the earlier text in advance, the lead decides whether the corrected text goes back to the maintainer or falls under the run's standing instruction. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether one space follows `fixed` (`KeywordSpacingRule.IsSpacedKeyword`, gaining `FixedKeyword`) | The left token's kind only. That covers every `fixed` token, both the statement keyword and the fixed-size-buffer modifier, before any right token on the same line. With a comment in the left token's trailing trivia, the space goes after the last comment (FMT-SPACE-013). | `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.OnSyntaxTree`. It reports a `fixed` whose next token is `(`, whose trailing trivia holds no whitespace, and which no line break separates from the `(`. | On RH6001's domain they agree: every formatter output holds whitespace in `fixed`'s trailing trivia, so the analyzer's whitespace check skips it. Probed: `fixed (`, `fixed/* c */ (`, `fixed /* c */ (`, `fixed/* a *//* b */ (`. The formatter's span is wider, covering the buffer and right tokens other than `(`. P11 allows that direction, and no RH rule reports the buffer gap. | ok |
| `TrimGapBeforeToken` (`SpacingPolicy.GetDesiredSpacesAfter(previousToken, token) != 0`) for a token behind `fixed` | Reads the same policy. | – | For `fixed`, the policy now returns 1 instead of `null`. Both are `!= 0`, so the trim decision for a token behind `fixed` is unchanged. | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which keywords take one space before their `(` | Code: `KeywordSpacingRule.IsSpacedKeyword` (13 kinds, now with `FixedKeyword`) and `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.OnSyntaxTree` (11 kinds). Specs: FMT-SPACE-012 and RH6001-001. Rule page: `documentation/rules/RH6001.md:12`. Other `FixedKeyword` hits in production code are not spacing decisions: `CommentPositionUtilities`, RH5017, and `SyntaxIndentationUtilities.GetEmbeddedStatement`. `ListElementInteriorJoinRewriter.IsTightBefore` does not treat the `(` of a `FixedStatementSyntax` as tight, so a join writes one space there too. | The owners now agree on `fixed`. The remaining difference, `new` and `operator`, is justified by RH6001's non-goals and allowed by P11. The two-owner duplication predates the run (`05-auditor.md`). No `fixed(` or `fixed/*` occurs in the repository's own source or fixtures outside the new tests, so the self-hosting scans see no output change. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `HorizontalSpacingRequirementTests.OneSpaceAfterFixedKeyword` | FMT-SPACE-012 example: `fixed(` becomes `fixed (`. | `fixed(` kept; any other change. | `AssertRuleResult`: exact output, LF and CRLF, second pass, phase stability. | Adequate. It was red before the change. |
| `….OneSpaceAfterFixedKeywordBehindBlockComment` | `fixed/* c */(` becomes `fixed/* c */ (`: the comment stays, and one space goes behind it. | Comment moved; space placed in front of the comment; no space. | `AssertRuleResult` | Adequate. It was red before. |
| `….OneSpaceAfterEachOfNestedFixedKeywords` | Both `fixed` tokens of a stacked pair get their space (FC-026). | Only the first one spaced. | `AssertRuleResult`; the input is at the FMT-INDENT-004 columns, so only spacing differs. | Adequate. It was red before. Its correction in `09-test-implementer.md` is right against FMT-INDENT-004. |
| `….SeveralSpacesAfterFixedKeywordBecomeOne`, `….SpacedFixedKeywordIsKept` | Whitespace-only gap gives one space; the spaced form is stable. | Width other than one. | `AssertRuleResult` | Adequate as stability and boundary checks. Both were green before the change, through the FMT-SPACE-002 collapse. |
| `….FixedSizeBufferKeepsOneSpaceAfterFixedKeyword`, `….SeveralSpacesInFixedSizeBufferBecomeOne` | The change doc's "no exception for the buffer" decision. | Only a width other than one with whitespace-only gaps. | `AssertRuleResult` | Weaker than the decision (finding #8). Both pass with or without `FixedKeyword` in `IsSpacedKeyword`. The only buffer input the decision changes, a comment glued to the type, is untested. |
| `RH6001KeywordsMustBeSpacedCorrectlyFormatterTests.VerifyFormatterFixesGluedFixedKeyword` | RH6001 reports `fixed(`. The formatter output is exactly `fixed (`, stable on a second pass, and raises no RH6001, under LF and CRLF (FC-013). | Diagnostic missing; different output; a second pass changes it; RH6001 on the output. | Own three steps with `ApplyAllowUnsafeToTestProject` and the public `ReihitsuFormatter.FormatSyntaxTree`. | Adequate; the same strength as `VerifyFormatter`. It goes through the public entry point rather than `FormattingPipeline.Execute`, which is at least as strong. See *Hints* for the copied helper. |

## Failure classes dismissed

- FC-001, FC-002: the changed predicate's span is the left token kind, the same as the other keywords of the rule. Its counterpart agrees on RH6001's domain (*Guard scope*).
- FC-004, FC-005, FC-019, FC-020, FC-021: the rule only sets whitespace after the last trailing comment and collapses whitespace in front of it. It never moves a comment. A directive cannot sit in a same-line gap, and a line break ends the rule's reach (FMT-SPACE-001). Probed with one comment, two comments, and whitespace in front of a comment.
- FC-006, FC-022 to FC-025, FC-029, FC-036: only whitespace between `fixed` and the next token changes. A space after a keyword cannot change meaning or compile status.
- FC-007 to FC-009, FC-018, FC-030: no code fix changed. Code fixes that delegate to the formatter now also space `fixed(`, the same as `if(`. The full analyzer run after the compiled change passed (`08-implementer.md`, `10-cleaner.md`).
- FC-010, FC-011, FC-012: the tests check the second pass and per-phase stability under LF and CRLF. My fixtures were stable on a second pass. No other phase writes the gap after `fixed`. The join's tight-pair table does not treat this `(` as tight.
- FC-013, FC-014: closed with #7; see *Guard scope*. No spacing rule reports the buffer gap. RH6006 only checks whitespace after `(`.
- FC-015, FC-016: see *Policy ownership* and #8's enumeration.
- FC-026, FC-027: nested statements are tested. The rule is token-local, so enclosing scope does not matter.
- FC-028: the formatter test uses the public whole-tree entry point. The rule runs identically under node-scoped entry points, because `SpacingPolicy` is the only owner.
- FC-031: the summary of `KeywordSpacingRule` lists examples ("such as …") that remain true. The new test summaries describe their bodies.
- FC-032: the analyzer and formatter runs were taken after dd2f609, the only compiled production change. The later commits change tests only, and the cleaner re-ran both projects on 1cf2db2.
- FC-033, FC-034, FC-035: the expected values come from the spec example and the change doc. Three tests were red before the implementation (`07-test-implementer.md`).
- FC-037: no text-only claim is made.
- Tab-width matrix: the tab after `fixed` is replaced by the preparation phase before spacing. Probed: `fixed<TAB>int` becomes `fixed int`. No column prediction is involved.

## Architecture findings

_None._ #8 is an incomplete record and a weak test for a decision that one owner makes correctly. No two components disagree. It repeats no earlier finding's class:
- #7 was analyzer against formatter, on which keywords take a space.
- #5 was a missing comment-shape test for RH6001's separation guard, a different decision in a different component.

## Hints

- `VerifyFormatterFixesGluedFixedKeyword` copies the three steps of `FormatterTestsBase<TAnalyzer>.VerifyFormatter` because that helper takes no solution transform for `unsafe` code. Its input is also written twice, once with markup (`testData`) and once as `plainSource`. An overload of `VerifyFormatter` that accepts a solution transform would remove the copy. This is a test-infrastructure suggestion, not a defect.
- `OneSpaceAfterFixedKeywordBehindBlockComment`, `SeveralSpacesAfterFixedKeywordBecomeOne`, and `SpacedFixedKeywordIsKept` declare an unused `int[] others` parameter in their fixtures. This is cosmetic.
