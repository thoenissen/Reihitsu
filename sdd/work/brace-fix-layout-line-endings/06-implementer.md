Role: implementer
Commit: the commit that adds this file (on 8834bea)
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every one of the 110 red tests of `05-test-implementer.md` passes. 15 tests that were green before this change now fail. All 15 assert the old split-fix output that SHARED-ATTR-017 forbids (see *Disputed tests or scope*). I did not change them.

## thoenissen/Reihitsu#1416

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs` | The fix no longer formats the owning `if` through `ReihitsuFormatter.FormatNodeInDocumentAsync`. It now edits the text: one change in front of the body (on the header line: the header's comments stay and `{` and the body move to new lines; below the header: the blank lines directly below the header line are removed and `{` is inserted there) and one change behind it (`}` goes behind the line break of the body's last line, or a line break, `}` and then ` else`, or a line break plus the next code at the brace column). The braces copy the whitespace in front of the line anchor: the `if`, or the `else` of an `else if`, or the `}` directly in front of an `else`, linked only by whitespace on one line (`FindLineAnchor`). The withholds are cheap checks in the same method (`TryCreateChanges`): anything other than spaces in front of the anchor (other code, a comment, or a tab); a directive or disabled text in the gap; a body on the header line that ends on a later line, counting its trailing comments; another line break from the end of the header through the end of the body's last line (`LineEndingUtilities.HoldsOtherLineBreak`). The `ReihitsuFormatter.FindUnsupportedSyntax` guard is gone because no formatter runs any more. | SHARED-BRACE-002, SHARED-BRACE-003, SHARED-FIX-007, SHARED-FIX-008 |

Decision 10 holds: the brace fixes stop delegating to the formatter, so they are no longer members of the write-back finding's class.

### Tests run
- `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5405BracesMustNotBeOmittedAnalyzerTests|FullyQualifiedName~RH5406BracesMustNotBeOmittedFromMultiLine|FullyQualifiedName~RH5407UseBracesConsistentlyAnalyzerTests"` → `Passed! - Failed: 0, Passed: 152, Skipped: 0, Total: 152`

### Disputed tests or scope
- **Header that spans several lines (spec reading, no test).** SHARED-BRACE-002/003 decide "on the line of its `if` or `else`" by the line of the keyword. For `if (a &&⏎    b) M();`, taken literally, the body starts on a later line than the `if`. The "keeps its lines" branch would then insert `{` below the `if` line, inside the condition. The implementation decides "on the header line" by whether a line-break trivia stands between the token that ends the header (`)` or `else`) and the body. That puts this body in the header-line branch: `{` and the body go below `b)`. The braces still take the column of the line on which the `if` starts. The same reading covers `if (a) /* x⏎y */ M();`: the comment stays behind the header, the body moves below `{`, and the line-ending check covers the line break inside the comment. Neither shape has a test. The spec author may want to word SHARED-BRACE-002/003 by the header's last line.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1388

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider.cs` | The empty line it inserts uses the detected line ending (`ReihitsuFormatterHelpers.DetectEndOfLine`, already used on the other path) instead of the break line's own line break. When the detected ending is `\n` and the insertion point follows a `\r` (lone-`\r` documents), the `\n` goes in front of that `\r` so the two do not join into one CRLF. This gives `break;\n\r`, which decision 11 accepts. | RH5011-004, SHARED-FIX-008 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentationCommentCodeFixUtilities.cs` | `MoveContentToNextDocumentationLineAsync` writes the detected line ending, not the line break of the affected line. The private `GetLineBreak` it alone used is deleted. RH8304 is the only caller. | RH8304-004, SHARED-FIX-008 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs` | New `MovesOtherLineBreak`, called from `TryCreateReorder`. For each block whose slot changes, it checks the span from the start of the `#region` line through the line break of the `#endregion` line with `LineEndingUtilities.HoldsOtherLineBreak`, and the fix is withheld on a hit. | RH7309-007, SHARED-FIX-008 |

### Tests run
- `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5011Break|FullyQualifiedName~RH8304Xml|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH7004Using"`: every test passes. They were measured inside the combined run under #1142, whose 15 failures are all attribute tests.

### Disputed tests or scope
_None._ RH5011-004's lone-`\r` sentence still needs the rewording that decision 11 records.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1627

### Changes
Covered by the #1416 change. The closing brace goes after the line break of the body's last line, so whitespace and comments at the end of that line stay where they are and nothing is written behind `}`.

### Tests run
Included in the brace run above: 152 of 152 pass.

### Disputed tests or scope
_None._

### Follow-up candidates
_None._ The `}···` write-back defect of the other formatter-delegating fixes is already in `00-lead.md`.

## thoenissen/Reihitsu#1142

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs` | `ApplySplitCodeFixAsync` replaces only the text of the reported list's span with `[target attribute]` lists. Before each further list it writes the detected line ending and a copy of the whitespace at the start of the list's line (`FormattingTextAnalysisUtilities.GetLeadingWhitespace`). Before this change, the owner was rebuilt from `SyntaxFactory` lists. Their elastic trivia let Roslyn's code-action cleanup re-format the code around the list, which caused the drift and the re-indentation. `BuildSplitReplacementLists` and `IndexOfList` are deleted. | SHARED-ATTR-017, SHARED-FIX-006, SHARED-FIX-008, RH55xx split requirements |

### Tests run
- `scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~AttributeListsMustFollowShapeRules|FullyQualifiedName~AttributesMustFollowPlacementRules"` → `Failed! - Failed: 15, Passed: 425, Skipped: 0, Total: 440`. Every red test of the handover for this issue passes. The 15 failures are listed below.

### Disputed tests or scope
These 15 tests were green on 7b3bed7 and fail now. Each asserts the output that Roslyn's cleanup produced. SHARED-ATTR-017 forbids that output: "what follows the reported list on its line — … another attribute list, or the declaration the list belongs to — stays behind the last list", and "the list shares its line with other code" keeps every other line as written. The test implementer rewrote only RH5514's Fix All scenario to the specified output (`[First]⏎[Second] [Third]⏎[Fourth]`). The other split rules share the provider path, so their scenarios now give the same specified shape.

- `FixAllCorrectsEveryDiagnostic` in the test classes of RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528, RH5531 (13 tests). Input `[First, Second] [Third, Fourth]` (with target and indentation per rule). The test asserts `[Second]⏎[Third]`. The specified output is `[Second] [Third]`: `[Third, Fourth]` stood behind the reported list on its line, so it stays behind `[Second]`. Each test probably also needs its `NumberOfFixAllIterations` aligned, as RH5514's was.
- `RH5518…VerifyDiagnosticForMergedFieldSpecifierListOnSingleLinePropertyAccessorWithExpressionBody` and `RH5528…VerifyDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessorWithExpressionBody`. Input `public int Value { [field: First, Second] get => 1; }`. The test asserts that the accessor list is broken open (`{` alone, the lists and `get` at column 8). The specified output is `    public int Value { [field: First]⏎    [field: Second] get => 1; }`: the further list goes behind the four spaces at the start of the list's line, and `get => 1; }` stays behind it. That output is unusual for a user, but it is what SHARED-ATTR-017's shared-line rule states. If the lead finds it unacceptable, it is an output question for this shape, not an implementation choice.

### Follow-up candidates
_None._

## Policy owners

- Line ending a fix writes: `rg -n "DetectEndOfLine\(" src --glob '*.cs' --glob '!*Test*/**'` has one owner, `LineEndingUtilities.DetectEndOfLine`, and `ReihitsuFormatterHelpers.DetectEndOfLine` forwards to it. The changed files call one of the two. No private copy remains: the deleted `GetLineBreak` in `DocumentationCommentCodeFixUtilities` was one.
- Foreign line break in moved text: `rg -n "HoldsOtherLineBreak\(" src --glob '*.cs' --glob '!*Test*/**'` has one owner, `LineEndingUtilities.HoldsOtherLineBreak`. Its users are `OrderingMoveSafety`, RH3004, RH7004, and now the brace base and RH7309.
- Leading whitespace of a line: `rg -n "GetLeadingWhitespace\(" src/Reihitsu.Core` has one owner, `FormattingTextAnalysisUtilities.GetLeadingWhitespace`, reused by the split fix. The brace base reads the text in front of the anchor token directly, because that text must be spaces only (a tab or code there withholds the fix). It is a guard, not a second copy of the helper.

## Production diff size

`git diff --shortstat c2866ea -- src ':(exclude,glob)src/*Test*/**'`: 5 files changed, 314 insertions(+), 138 deletions(-). No new type. No new public member: every added member is `private`.

## Deleted special cases

_N/A._ No shape is declared unsupported (answer 5).

## Tests run (whole run)

- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/test.sh --project analyzer --configuration Debug` → `Failed! - Failed: 15, Passed: 8627, Skipped: 0, Total: 8642`. The 15 are the disputed attribute tests above, and nothing else fails.
- `scripts/test.sh --project architecture --configuration Debug` → `Passed! - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.

## Architecture findings

_None._ The split-fix defect was Roslyn's cleanup re-formatting elastic trivia. Roslyn is not a component of this repository, so this is not a shared decision between two of its components; the skeptic reached the same conclusion.
