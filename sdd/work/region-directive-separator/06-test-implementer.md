Role: test implementer
Commit: 6c746c9
Verdict: tests written

This is an amendment round for decisions 8–10 in `00-lead.md`, after `05-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, `04-test-implementer.md`, `05-spec-author.md`, `git diff dbdf5db..6c746c9 -- sdd`. Expected values come from FMT-REGION-004's amended text and its two new examples. `04-test-implementer.md` stays as the earlier record, and this file supersedes its row for `RegionWithoutDescriptionAndTrailingWhitespaceIsKept` and its finding 3.

"Fails now" was run on `6c746c9`. Each failure is the requirement's own difference: whitespace behind a bare `region`/`endregion` keyword is kept on the current code. I observed it with the CLI (`#region···` and `#endregion····` from a tab keep their whitespace, also inside `#if false`).

## Tests

`src/Reihitsu.Formatter.Test/Regression/Regions/RegionLineFormTests.cs` (`AssertRuleResult`: LF and CRLF, second pass, every phase alone on the output, tree round-trip):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| TrailingWhitespaceBehindBareRegionAndEndRegionIsRemoved (spec example 1, new) | FMT-REGION-004 | AssertRuleResult | fails |
| TrailingWhitespaceBehindRegionWithoutDescriptionIsRemoved (replaces `RegionWithoutDescriptionAndTrailingWhitespaceIsKept`: same input, expectation now `#region`) | FMT-REGION-004 | AssertRuleResult | fails |
| TrailingWhitespaceBehindBareEndRegionIsRemoved (new; bare `#endregion···` alone) | FMT-REGION-004 | AssertRuleResult | fails |
| TabBehindBareEndRegionIsRemoved (new; FC-012, tab phase → region phase) | FMT-REGION-004 | AssertRuleResult | fails |
| TrailingWhitespaceBehindRegionWithoutDescriptionInDisabledBranchIsRemoved (spec example 2, new) | FMT-REGION-004 | AssertRuleResult | fails |
| TrailingWhitespaceBehindBareEndRegionInDisabledBranchIsRemoved (new; bare `#endregion···` in `#if false`) | FMT-REGION-004 | AssertRuleResult | fails |

Unchanged tests that cover the other side of the new condition ("a line that has none is left unchanged"): `BareEndRegionOfUndescribedRegionStaysBare` (passes), and `WhitespaceAroundTextOfEndRegionOfUndescribedRegionInDisabledBranchIsNormalized` (fails, as before).

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeRegionTests.cs` (LF and CRLF, second pass through the same entry point):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FormatNodeInDocumentAsyncKeepsWhitespaceBehindBareRegionAndEndRegion (new) | FMT-PIPE-028 | FormatNodeInDocumentAsync | passes (guards the bare-keyword trimming from reaching node level, as the two existing whitespace guards do for FMT-REGION-007 and the labelled `#endregion`) |

Node-level counterpart: FMT-PIPE-028 excludes every FMT-REGION requirement at node level, and the amended FMT-REGION-004 adds a new case of whitespace removal. A fix that trims the bare keyword outside the region rewriter, for example in a shared trivia helper or the cleanup phase, would reach node level unnoticed without this guard. The test is needed for that reason.

## Not covered, and why

- `FormatNode` / `FormatNodeInDocumentWithContextAsync` counterparts for the bare form: the existing guards for FMT-REGION-007 and the labelled `#endregion` use only `FormatNodeInDocumentAsync`. The other two entry points are covered by the general region tests (`FormatNodeKeepsRegionDirectives`, `FormatNodeInDocumentWithContextAsyncKeepsRegionDirectives`). One more guard on the same entry point matches the existing pattern. Whether the skip is shared across all three entry points is something the implementer decides, so this rests on the existing tests, not on a static proof.
- Code-fix counterpart (RH7105/RH7104 with `#region···`): the node-level guard above covers the entry point those fixes call. No additional `Verify` test.
- RH7103 (decision 10): no new test. `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions` already pins the change-doc example, as `05-spec-author.md` states.
- Other failure classes: unchanged from `04-test-implementer.md`.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~RegionLineFormTests|FullyQualifiedName~ReihitsuFormatterNodeRegionTests"
scripts/test.sh --project architecture
```
