Role: test implementer
Commit: afd0000
Verdict: tests written

This is an amendment round for decision 11 in `00-lead.md`, after `07-implementer.md`. The prompt summarized the four tests and gave the target `#region Fields`. I checked each one against FMT-REGION-007 myself and did not take that summary on trust. The prompt carried no suspected cause or hoped-for verdict. Inputs: `00-lead.md` (decision 11), `06-test-implementer.md`, `07-implementer.md`, FMT-REGION-002 and FMT-REGION-007 in `sdd/specs/formatter/region-formatting.md`.

## Check against the spec

All four tests feed `#region Fields···` (three spaces behind a description that already has its one space) through whole-document formatting. FMT-REGION-007 says that "whitespace behind the description is removed", and its example turns `#region Baz···` into `#region Baz`. Each test asserted that the whitespace is kept, so each one contradicted the spec. The disputes in `07-implementer.md` are confirmed for all four. The part of each test that still holds is the close: it is canonical, or for a mismatch it repeats the trimmed description (FMT-REGION-002, "using the description of the matching `#region`"). That part is kept.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RegionFormattingTests.TrailingOpeningWhitespaceWithCanonicalCloseIsStable` → renamed `TrailingOpeningWhitespaceWithCanonicalCloseIsRemoved`. The expected output is now the canonical source (`#region Fields`), where before the input was kept unchanged. Summary rewritten, tag added | FMT-REGION-007 | AssertRuleResult | passes |
| `RegionFormattingTests.MismatchedCloseUsesNormalizedOpeningDescription` → renamed `MismatchedCloseUsesTrimmedOpeningDescription`. The expected opening is now `#region Fields` (was `#region Fields···`), and the close is still `#endregion // Fields`. Summary rewritten, tags added | FMT-REGION-002, FMT-REGION-007 | AssertRuleResult | passes |
| `RH7301RegionsShouldMatchFormatterTests.VerifyFormatterPreservesTrailingOpeningWhitespaceWhenCloseIsCanonical` → renamed `VerifyFormatterRemovesTrailingOpeningWhitespaceWhenCloseIsCanonical`. It now calls `VerifyFormatter(input, source)` with no diagnostic, because the input stays analyzer-clean. Before, it used the unchanged-output overload. Summary rewritten, tag added | FMT-REGION-007 | VerifyFormatter | passes |
| `RH7301RegionsShouldMatchFormatterTests.VerifyFormatterUsesNormalizedOpeningDescriptionForMismatchedClose` → renamed `VerifyFormatterUsesTrimmedOpeningDescriptionForMismatchedClose`. The expected opening is now `#region Fields`, and the RH7301 diagnostic is unchanged. Summary rewritten, tags added | FMT-REGION-002, FMT-REGION-007 | VerifyFormatter | passes |

I did not run the new expectations red on the pre-change code. `07-implementer.md` records that the old expectations (`#region Fields···` kept) were green on `e615ba1`. The old and new expectations differ only in that whitespace, so the new ones fail on that code for exactly the FMT-REGION-007 difference.

## Was the earlier list of changed expectations complete?

No. These four tests should have been in the list of existing tests whose expectation changed in `04-test-implementer.md`. That list came from one throwaway experiment, which disabled the region phase at node level. That experiment surfaces FMT-PIPE-028 only. I never ran the equivalent probe for FMT-REGION-007's whole-document line form, so the trailing-whitespace tests were missed. Now that the implementation is in place, the full formatter, analyzer, CLI, and architecture projects pass with these four changes and nothing else. Every remaining test asserts exact output, and a test pinning the old behavior through the region phase would fail. So no other existing test was missed. The tests with wide separators that still pass were already explained in `04-test-implementer.md`: phase-alone tests, node-level tests under FMT-PIPE-028, and analyzer fixes that do not call the formatter.

## Not covered, and why

Nothing new. This round only corrects expectations, and the matrix in `04-test-implementer.md` and `06-test-implementer.md` stands.

## Commands

```shell
scripts/test.sh --project formatter   # Passed 4409, Failed 0
scripts/test.sh --project analyzer    # Passed 6415, Failed 0
scripts/test.sh --project architecture  # Passed 92
scripts/test.sh --project cli         # Passed 251
```
