Role: spec author
Commit: e13d9a7
Verdict: change doc written

My prompt contained no proposed fix, no suspected cause, and no verdict that anyone was hoping for. It told me to apply the maintainer's answer to escalations 4 and 5, which is option A: an anchor on a comment-led line is always measured at its real column. `00-lead.md` and `squad/decisions/2026-10-25-phase-prediction-removal-first-on-line-anchor.md` record that answer, and this round follows it.

All outputs named below were observed with `scripts/format.sh` and `scripts/apply-fix.sh` on fixtures in the scratchpad, outside the repository:
- "today" was observed in a temporary worktree of c49431e, which I have removed again;
- "after" was observed on e13d9a7.

Columns are 0-based.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` stays `Status: Draft`. The maintainer has to approve it again (escalation 2). Changes in this round:

- **Affected requirements**: added FMT-INDENT-023, SHARED-INIT-002, RH5303-003, and RH5304-002.
- **What**:
  - The D2 bullet now says that the rule also holds for an anchor that is itself the first code on its line.
  - A new bullet says that RH5303 and RH5304 measure a wrapped complex element from its brace (SHARED-INIT-002). After this change they report the layout they used to accept.
  - A second table lists the changed outputs for anchors that are the first code on a comment-led line:
    - the complex element (the shape of the two disputed tests)
    - the nested initializer `Child =` / `/* c */ {`
    - the array initializer
    - the collection expression and the list pattern
    - `new` of an object creation, of a collection creation, and of an anonymous object
    - the switch expression, lambda, binary, conditional, chain, and `is` pattern
  - Every row except the nested initializer was observed as stated, today and after. The nested initializer row states the output that the spec now requires; e13d9a7 does not produce it yet (see below).
  - A paragraph covers three points:
    - The negative side: a comment on a line above the anchor changes nothing.
    - A token that anchors nothing, such as `/* Keep open. */ {` of a creation initializer, changes nothing.
    - RH5301, RH5303 (braces at `new`) and RH5305 already measure `new` and the opening bracket where they stand. They report today's formatter output for those rows and accept the new output. I observed this with `apply-fix.sh`: RH5301, RH5303, and RH5305 report the c49431e layout and return `no-diagnostic` on the e13d9a7 layout.
- **Decisions**:
  - New: "An anchor that is the first code on its line is measured behind a comment too". It gives the maintainer's reason (one rule without an exception) and the analyzer consequence. Code the old formatter laid out is now reported, and the fixes lay it out the new way.
  - "Changed output is only output that violates an existing requirement today" now names the one changed rule check, SHARED-INIT-002.
- **Alternatives**: option B (narrow FMT-INDENT-030 for an anchor that is first on its line) is recorded as rejected.

## Changed requirements

- **SHARED-INIT-002 — A wrapped complex element is laid out as a block — changed.**
  - The column of the opening brace is the column at which the brace itself stands.
  - When a comment ends on the brace's line in front of it, SHARED-INIT-001 checks the comment. The closing brace and the expressions MUST then be measured from the brace's column behind the comment, not from the comment's column.
  - Other side of the qualifier (FC-002): when the comment is on a line above, or there is no comment, the brace starts its line and is measured at that line's column.
  - New example with both sides:
    - `/* e */ {` with the expressions at 32 and `}` at 28, followed by an element led by `// f` on the line above: not reported.
    - The old layout (expressions at 24, `}` at 20): reported on the whole complex element.
  - `Uses` changed from `—` to FMT-INDENT-023 and FMT-INDENT-030, which the Why now cites.
- **RH5303-003 — A misaligned complex element is reported — changed (examples only).** Three examples, all following RH5303-004:
  - Not reported: the comment-led element with leading comments on an expression (`/* k */ 1`) and on the closing brace (`/* c */ }`), at 32 and 28.
  - The old layout: reported on the whole complex element.
  - Only one expression out of place (`1` at 24, `2` at 32, `}` at 28): reported on that expression.
- **RH5304-002 — A wrapped collection initializer that is not a block is reported — changed (examples only).**
  - `D = {` / `/* e */ {` with the expressions at 40 and `}` at 36: not reported.
  - The old layout (32 and 28): reported on the whole assignment.
- **FMT-INDENT-030 — Anchors are measured at their final column — changed (wording and example).**
  - A new paragraph: the rule holds also when the anchor is itself the first token of its line. It names the anchors: the brace of a complex element or nested initializer, the bracket of a collection expression, `new`, the first token of a governing expression, and a left operand. The comment takes the column of the line's start, and the anchor is measured behind it.
  - A token that anchors nothing, such as the brace of a creation initializer whose members align to `new`, moves no other line.
  - New example, the complex element: the expressions go from 24 to 32 and the closing brace from 20 to 28. The negative side is a second element whose `// f` comment stands on the line above; it keeps its brace at 20.
  - On e13d9a7 the formatter produces exactly this output.
- **FMT-INDENT-023 — Initializers without a keyword align to their target or their brace — changed (wording and example).**
  - When a comment leads the opening brace's line, the comment takes the line's start column; for a nested member initializer that is the member's column (FMT-INDENT-006). The brace's column is where it stands behind the comment, and the closing brace and the members follow that column.
  - Other side: a comment on the line above leaves the brace starting its line.
  - New example: `Child =` / `/* c */ {` / `X = 2` / `}`. Output: `X` at 32, `}` at 28.

No new IDs, so the ledger is unchanged. `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd|FullyQualifiedName~PhaseOwnership|FullyQualifiedName~RuleDocumentation"` on the working tree gives `Passed: 52, Failed: 0`. That run includes `UsesListsExactlyTheReferencedRequirements`.

**Rule pages unchanged.** `documentation/rules/RH5303.md` and `RH5304.md` say "When a leading comment shares a brace or entry line, the comment provides that line's alignment anchor". That is still true: the comment still decides where its own line starts. A complex element's contents are indented "one additional level" from its braces, and that also matches the new measurement. A comment leading a complex element's brace is a corner case, so I left both pages alone.

## Examples for the tests

- **SHARED-INIT-002, RH5303-003, RH5304-002**: derive the tests from the examples. All "not reported" blocks are stable under the e13d9a7 formatter, which I checked with `format.sh`.
- **FMT-INDENT-030**: the new complex-element example. It passes on e13d9a7.
- **FMT-INDENT-023**: the new nested-initializer example. **e13d9a7 does not produce it yet.** It leaves `X` at 24 and `}` at 20, which is today's output.
  - Observed cause: `ObjectInitializerContributor` handles `InitializerExpressionSyntax { Parent: AssignmentExpressionSyntax }`. When the brace is first on its line (`LayoutComputer.IsFirstOnLine`), that branch takes the assigned member's column as the initializer's anchor, not the brace's column after placement. That is the exemption option B would have kept.
  - No analyzer checks this construct (RH5301 lists the layout of a member's value as a non-goal; RH5304 lists a nested object initializer as one), so only the formatter is involved.
- **Existing tests whose expectation changes:**
  - `RH5303CollectionInitializerShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterKeepsCommentPrefixedComplexElementStable` and `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterKeepsCommentPrefixedComplexElementStable`.
    - The new formatter output is in `05-implementer.md`: RH5303 45/49/45, RH5304 57/61/57. I confirmed it on e13d9a7.
    - The inputs are now reported. RH5303 reports the whole complex element, because `/* Keep close. */` is at 25, not at 45. RH5304 reports the whole assignment.
  - `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyNoDiagnosticForCommentPrefixedComplexElementTokens` and `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyNoDiagnosticForCommentPrefixedComplexElementTokens` assert the old layout as clean. Under SHARED-INIT-002 it is reported: for RH5303 on the whole complex element, for RH5304 on the whole assignment.
  - `VerifyFormatterKeepsCommentPrefixedCollectionStable` (`/* Keep open. */ {` of a creation initializer) keeps its expectation. Its members align to `new`.

## Defect observed in passing (not an open question)

**A node-level format of a complex element behind the last line of a multi-line comment gives other columns than document-level formatting.** This is new in this run.

The fixture is a statement. Its collection initializer's opening brace is misaligned at column 14, so RH5303 reports the braces, and the fix formats the statement through `FormatNodeInDocumentAsync`:

```csharp
        var d = new Dictionary<int, int>
              {
                    /* a
                       b */ {
                        1,
                        2
                    }
                };
```

| | `scripts/format.sh` (document) | `scripts/apply-fix.sh RH5303` (node-level fix) |
|---|---|---|
| e13d9a7 | `1` and `2` at 32, `}` at 28 | `1` and `2` at 38, `}` at 34 |
| c49431e | `1` and `2` at 24, `}` at 20 | `1` and `2` at 24, `}` at 20 |

- With a one-line comment `/* e */ {` instead, both entry points give 32/28 on e13d9a7.
- Under SHARED-INIT-002 the fix's output (38/34) is reported again, so the fix does not converge. `apply-fix.sh` already shows `no-progress` against today's analyzer.
- This violates RH5303-005 ("lays out ... as the formatter lays it out"). I did not determine the cause.
- Candidates are the node-level column steps that the change doc lists as "Not changed here" (`ReihitsuFormatterHelpers.ComputeTokenColumn`, `AdjustNodeIndentation`): they do not move a line that starts inside comment trivia. The other candidate is the layout counting the comment's tail in the detached tree.
- The input is a token behind a multi-line comment's last line. thoenissen/Reihitsu#1382 will later make the formatter break before such a token (SHARED-COMMENT-012).
- `RH5203` on `/* a` / `b */ Call(1,` / `2);` gives the same column through its fix and through the document format, so the divergence is not general.

## Open questions for the maintainer

_None._
