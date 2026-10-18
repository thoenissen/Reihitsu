Role: test implementer
Commit: bbb1d10
Verdict: tests written

Repair round, finding 9 of `09-auditor.md`. The coordinator's message carried no intended fix or hoped-for verdict; nothing had to be ignored. No assertion of an existing test changed, and no production code was touched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5604 `VerifyCrLfWinsTieWhenItIsEncounteredFirst` (renamed from `VerifyFirstEncounteredLineEndingWinsWhenCountsAreTied`) | RH5604-001, FC-031 | `Verify(source)` | pass |
| RH5604 `VerifyCrLfWinsTieWhenLfIsEncounteredFirst` (new) | RH5604-001, -004, -005, FC-017 | `Verify(source, fixedSource, …)` | pass |

**Renamed test.** The test's input (`internal class TestClass\r\n{\n}`) and its assertion (the `{\n` line is reported) are unchanged. It agrees with RH5604-001's tie rule (`\r\n` wins), but it cannot separate that rule from a "first encountered wins" rule. Only the name and summary changed. They now state the CRLF tie rule and point to the discriminating test. The tag RH5604-001 stays, because the test does verify a tie that resolves to `\r\n`.

**New test.** The input is `internal class TestClass\n{\r\n}`: a tie in which LF comes first. The test asserts that line 1 (the LF line) is reported and that the fix gives `internal class TestClass\r\n{\r\n}`. Under a "first encountered wins" rule, the `{\r\n` line would be reported instead, so this test discriminates the two rules. It does so without a documentation comment, which `VerifyCrLfWinsTieIncludingXmlDocumentationLineEnding` needs.

## Tags checked against the narrowed requirements (bbb1d10)

I re-derived the spec author's claim that no other tag has to move, and it holds. I checked every tagged test of each requirement narrowed in `10-spec-author.md` against the shapes that are now unclaimed. I used `rg` for `/**`, `# region`, and multi-line interpolations over the touched test classes, then read the hits.

- RH5601-001: the tagged inputs are indentation before `void`, `#pragma→warning` and `#define→FEATURE` before `class`/`void`, `int→x`, `int→→x`, and `x;→// note`. None of them is a tab before a continuation `///` line or after a `/** */` comment. The only `/**` hit is `/** col→umn */`, which is comment content tagged RH5601-002, and that stays claimed.
- RH5601-002: every interpolated input has its tab in the literal text (`$"{1}→"`), not in a hole.
- RH5602-001: no tagged input has an interpolated string on the line. `char c = 'a';··` is a character literal, not a string literal. The `/**` hit is the untagged multi-line documentation-comment test.
- RH5603-001/-002: the tagged files end with `}` or with `// end` on a line that is not a directive.
- RH6001-001/-002, RH6005-001: the comment inputs are `/* note */`, never `/** */`.
- RH6004-002: `#region Methods` and `#endregion // Methods` have no whitespace after `#`. The raw-string `#endregion` is literal text.
- RH6009-001, RH6010-001, RH6011-001: the comment inputs are `/* note */` and `/* Keep. */`.
- RH6003, RH6008, RH6012: only the Why and `Uses` changed, so no tag is affected.

## Not covered, and why

Unchanged from `06-test-implementer.md`. This round adds no exclusion.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"
```

Results:

- RH5604 tests: 24 passed.
- Architecture filter: 39 passed.
- `scripts/format.sh` on the changed file: 0 files changed.
