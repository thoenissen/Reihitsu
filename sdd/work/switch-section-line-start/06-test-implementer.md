Role: test implementer
Commit: 56ad47f
Verdict: tests written

Correction round for lead decisions 10 and 11. My prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code touched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SwitchSectionLineStartTests.SectionBehindDirectiveKeepsItsLine` (re-derived, decision 10) | FMT-BREAK-033, FMT-BREAK-035; FC-019, FC-020 | AssertRuleResult | Passes on 7451f70 production; fails on e38c060^ (`case 1: return 1;` and `case 2: return 2;` stay on label lines) |
| `DelimitedDocumentationRelocationTests.MovesDelimitedDocumentationWithStatementBehindCaseLabel` (renamed from `KeepsDelimitedDocumentationWithStatementBehindCaseLabel`, decision 11) | FMT-BREAK-035 | AssertRuleResult | Passes on 7451f70 production; fails on e38c060^ (`case 1: /** d */ M(2);` kept) |

Changes:

- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/SwitchSectionLineStartTests.cs`: the directive is now `#if !DEBUG` … `#else` … `#endif`. `AssertRuleResult` parses with the default options, which define no `DEBUG`, so `case 2: return 2;` is active code behind the directive and is expected as `case 2:` / `return 2;`. The `#else` branch `case 3: return 3;` is disabled text and is expected exactly as written, which checks that the formatter leaves disabled text unchanged (`AGENTS.md`). Summary updated.
- `src/Reihitsu.Formatter.Test/Regression/FullPipeline/DelimitedDocumentationRelocationTests.cs`: the expected output is now `case 1:` alone, then `/** d */ M(2);` on the next line at statement indentation, per maintainer answer 6 and the FMT-BREAK-035 example (`case 2:` / `/** d */ A();`). The test now verifies that requirement, so it is tagged `[Requirement("FMT-BREAK-035")]`. Its name and summary said "keeps", which is no longer the behavior, so the test is renamed to `Moves…`; no other file references the old name outside `sdd/work/`.

Both test files were run through `scripts/format.sh` (0 of 2 reformatted). `scripts/test.sh --project architecture`: 92 passed.

## Scenarios (bug fix)

_N/A_ — behavior change.

## Not covered, and why

- A disabled branch holding a section is not split into its own test: the `#else` arm of the re-derived test already covers it. Each line ending is still covered, because `AssertRuleResult` runs LF and CRLF and a second pass.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchSectionLineStartTests.SectionBehindDirectiveKeepsItsLine|FullyQualifiedName~DelimitedDocumentationRelocationTests.MovesDelimitedDocumentationWithStatementBehindCaseLabel"
```
