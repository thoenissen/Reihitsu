Role: test implementer
Commit: 67f49f7
Verdict: #1387: tests written; #1147, #1152, #1386, #1594, #1655, #1736: nothing to do this round

My prompt carried no intended fix, suspected cause, or hoped-for verdict. This round closes the gap that `04-test-implementer.md` left open for #1387 and that decision 12 in `00-lead.md` decided: each split-mode attribute rule without its own conditional-directive test now has one. Expected values come from SHARED-ATTR-019 as amended (`sdd/specs/shared/attribute-targets.md`) and each rule's `-004` requirement. All 11 rules are split rules (one attribute per list). Each lists SHARED-ATTR-019 under `Uses`, and its `-004` withholds the fix when SHARED-ATTR-019 withholds it. So each spec admits the `#region` boundary case. The tests copy the shape of RH5506's `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheAttributes` and `VerifyDiagnosticWithoutCodeFixWhenRegionDirectiveIsBetweenTheAttributes`.

## thoenissen/Reihitsu#1147, #1152, #1386, #1594, #1655, #1736
- #1147: nothing to do this round.
- #1152: nothing to do this round.
- #1386: nothing to do this round.
- #1594: nothing to do this round.
- #1655: nothing to do this round.
- #1736: nothing to do this round.

## thoenissen/Reihitsu#1387

### Tests
Every file is `src/Reihitsu.Analyzer.Test/Formatting/RH<id>…AnalyzerTests.cs`. Each of the 11 rules RH5502, RH5504, RH5508, RH5510, RH5512, RH5516, RH5518, RH5520, RH5522, RH5526, and RH5528 gains these two tests. The owner differs per rule: assembly and module list (`assembly:`/`module:` specifier), struct, enum, constructor, property, field, event, interface, delegate, and method (`return:` specifier).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoDiagnosticWhenConditionalDirectiveIsBetweenTheAttributes` (`[A,⏎#if DEBUG⏎ B,⏎#endif⏎ C1]` on the rule's owner) | SHARED-ATTR-019 | `Verify` LF/CRLF | red, ×11 rules × 2 line endings: "expected 0 actual 1". The actual diagnostic is the rule's own ID on the list, and no compiler diagnostic is involved |
| `VerifyDiagnosticWithoutCodeFixWhenRegionDirectiveIsBetweenTheAttributes` (`#region`/`#endregion` between the attributes) | RH<id>-004, SHARED-ATTR-019 | `VerifyNoCodeFixIsOffered` | green, ×11 |

The conditional test carries only SHARED-ATTR-019, as the RH5514 and RH5531 tests of round 04 do: each rule's `-004` covers only the withheld fix, not the not-reported case.

Run on this head with unchanged production code: all tests of the 11 rule classes (analyzer and formatter) give 173 cases, 22 failing. The failing cases are exactly the new conditional tests. No existing test changed state.

### Not covered, and why
- `#pragma` boundary: not repeated per rule. RH5506's rewritten `VerifyDiagnosticWithoutCodeFixWhenCommentOrDirectiveIsInsideTheList` covers `#pragma` and `#if` inside an argument for the shared report site. Per rule, the boundary case uses `#region` because the brief asks for one boundary case. Adding `#pragma` per rule would be cheap if the lead wants it.
- Disabled-text variant (`#if false … #else … #endif`) and the conditional directive in front of the list: covered only on RH5506, as in round 04. They exercise the same span check as the tests above. Under the role file they are not proven to share one call site, so the gap is deliberate and cheap to close.
- Merge-mode rules (RH5524, RH5529) and RH5514/RH5531: already covered in round 04.

## Commands
```shell
scripts/test.sh --project analyzer --filter "(FullyQualifiedName~RH5502|FullyQualifiedName~RH5504|FullyQualifiedName~RH5508|FullyQualifiedName~RH5510|FullyQualifiedName~RH5512|FullyQualifiedName~RH5516|FullyQualifiedName~RH5518|FullyQualifiedName~RH5520|FullyQualifiedName~RH5522|FullyQualifiedName~RH5526|FullyQualifiedName~RH5528)&(Name~ConditionalDirectiveIsBetweenTheAttributes|Name~RegionDirectiveIsBetweenTheAttributes)"
```
