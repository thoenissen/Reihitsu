Role: test implementer
Commit: c2ef612
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. In the `tests` lane the check itself is test code, so the architecture test and its self-tests are this role's deliverable; no production path changed. `scripts/lane-check.sh tests` confirms the lane at c2ef612.

## Tests

All in `tests/Reihitsu.ArchitectureTests/General/ResourceTextLiteralTests.cs` (new), with the finding type `ResourceTextLiteral.cs` (new). `TrackerReferenceFixtureDirectory.cs` is reused for the fixtures and changes only its summary to name its second user (no new fixture type, FC-015). No spec covers architecture tests, so no test carries a requirement tag.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TestSourcesQuoteNoResourceText` | the check (issue goal) | repository scan of `tests/` with the real texts | pass |
| `RepositoryResourceTextsAreReadFromBothFilesAndDecoded` | non-vacuous scan; entity decoding | `LoadResourceTexts` per real `.resx` | pass |
| `ScanReportsRealResourceTextWithFileAndLine` | skeptic row 1 with a real value; file and line named | fixture, literal built at run time with `SymbolDisplay.FormatLiteral` | pass |
| `LoadResourceTextsSkipsHeaderSamplesAndDecodesEntities` | rows 11 (XML-comment header `Blue` / sample), entity decoding, placeholder fragment | fixture `.resx` | pass |
| `ResourceTextOfValueWithoutPlaceholdersIsTheWholeValue` | FC-003, no-placeholder side | `GetResourceText` | pass |
| `ResourceTextOfValueWithPlaceholdersIsItsLongestFixedFragment` | FC-003, placeholder side, incl. `{0,-5}`, `{1:N0}`, leading placeholder | `GetResourceText` | pass |
| `ScanReportsRegularLiteralEqualToText` | row 1, file and line | fixture | pass |
| `ScanReportsRegularLiteralContainingText` | row 2 (containment) | fixture | pass |
| `ScanIgnoresLiteralHoldingOnlyPartOfText` | FC-003, other side of containment | fixture | pass |
| `ScanReportsLiteralHoldingFormattedMessage` | row 3 (the `LineCountSelfHostingTests` shape) | fixture | pass |
| `ScanReportsLiteralHoldingFormatString` | row 5 | fixture | pass |
| `ScanReportsInterpolatedStringTextPart` | row 4 | fixture | pass |
| `ScanReportsInterpolatedRawStringTextPart` | row 4/7, `$$"""…"""` | fixture | pass |
| `ScanComparesDecodedValueOfEscapedRegularLiteral` | row 6, `\"` | fixture | pass |
| `ScanComparesDecodedValueOfVerbatimLiteral` | row 6, `""` | fixture | pass |
| `ScanReportsSingleLineRawStringLiteral` | row 7 | fixture | pass |
| `ScanReportsMultiLineRawStringLiteralAtTheTextLine` | row 7; line of the text, not of the opening quotes | fixture | pass |
| `ScanReportsMultiLineRawStringLiteralAtTheTextLineWithCrLf` | same, CRLF | fixture | pass |
| `ScanReportsUtf8StringLiteral` | `"…"u8` token kind | fixture | pass |
| `ScanReportsEveryLiteralAtItsOwnLine` | several findings, each at its line | fixture | pass |
| `ScanIgnoresCommentsDocumentationCommentsAndDisabledText` | row 8 (FC-033: real trivia kinds) | fixture | pass |
| `ScanIgnoresDiagnosticIdArgumentsAndSharedWordsWithRealTexts` | rows 9, 10, 11 against the real texts | fixture | pass |
| `ScanExcludesBuildOutputDirectories` | row 12 | fixture | pass |

Design as decided in `00-lead.md` rows 3 and 4: containment of the decoded value (longest fixed fragment for placeholder values) in a literal token's `ValueText` or an interpolated text part; no minimum length, no exempt file. The scan takes its texts as a parameter, and the self-tests use synthetic texts (`Synthetic resource text for the scan.`, `Member has {0} lines, over the limit of {1} for sure.`), so this file is not reported by the repository scan (skeptic internal choice 4). Values are read with `XDocument` from the root's `data` children only.

### Evidence the tests fail for the right reason

- Repository scan against the pre-clean-up tree (`git archive 5db426f^ tests`, scanned with the current `.resx` values by pointing the scan root there temporarily): **Failed, `Found 268 literal(s)`, in 24 files**, including the four `LineCountSelfHostingTests.cs` sites (lines 108, 123, 138, …) on the fragment ` lines of code, which exceeds the limit of `. Same counts as `01-skeptic.md`. Reverted afterwards.
- Mutations of the scan, each reverted:
  - `StringLiteralToken` dropped: 9 tests fail (all regular/verbatim/escaped/real-value/bin-obj positives).
  - each of `Utf8StringLiteralToken`, `SingleLineRawStringLiteralToken`, `MultiLineRawStringLiteralToken`, `InterpolatedStringTextToken` dropped: exactly its own tests fail (1, 1, 2, 2).
  - `token.Text` instead of `token.ValueText`: the two decoded-value tests fail.
  - equality instead of containment: 5 tests fail (contained, formatted message, format string, both multi-line raw).
  - line at the token start instead of at the text: both multi-line raw tests fail.
  - whole value instead of the longest fragment: does not compile under the warnings-as-errors build (unused field), so not run; the two `GetResourceText` tests and the `.resx` fixture test assert the fragment by `Assert.AreEqual`/sequence equality directly.

## Scenarios (bug fix)

_N/A_: not a bug fix.

## Not covered, and why

- Existing tests whose output changes: none. The run changes no shipped output, and no test file under `tests/` quotes a resource text (the new repository test passes on c2ef612).
- Generated-file suffixes (`.g.cs`, `.Designer.cs`): not excluded and not tested; `tests/` holds none (01-skeptic), and the skeptic's scope is `bin/`/`obj/` only.
- A literal inside an `#if` group whose symbol is undefined (`#if DEBUG`): parsed as disabled text, so not scanned, the same as `#if false` (row 8). The scan parses without preprocessor symbols, as `SelfReferentialTrackerReferenceTests` does.
- A whole-value text (no placeholders) split by an interpolation hole (`$"Use {x} syntax."` against `Use lambda syntax.`): not reported, because no text part contains the value. The decided rule compares per text part (00-lead row 3); the old tree had no such site.
- Composite-format brace escapes (`{{`) in resource values: no value holds one, so `GetResourceText` does not model them.
- Code fixes, formatter, CRLF/idempotency of formatter output, Fix All, Renamer, unsupported shapes (FC-004 … FC-044 outside *Tests and evidence* and FC-015): not reachable, nothing but test code changes. FC-017: assertions name exact file, line, and text where the issue requires file and line. FC-031: the summaries describe the current behavior of new code only.

## Commands

```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.General.ResourceTextLiteralTests"
scripts/test.sh --project architecture
```

Both green at c2ef612 (23 and 221 tests). `scripts/format.sh` over the three changed files reports nothing left to format.

## Follow-up candidates

- `FindRepositoryRoot` now has a ninth private copy (`ResourceTextLiteralTests.cs`), and the `bin`/`obj` segment skip a second copy next to `SelfReferentialTrackerReferenceTests.IsExcludedFromScan`; this extends the consolidation candidate already in `00-lead.md`. `TrackerReferenceFixtureDirectory` now serves two test classes under a tracker-specific name, which the same clean-up could rename.
