Role: auditor
Commit: 54b50c9c
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

State: the checkout is `54b50c9cfb8258f19664bdee91070093c093a72b`. `origin/main` (`git ls-remote`) is `8ac5705a03d0344a46ad991b4ab26d246a1dadd3`, which is the merge base, so it is merged. The diff `1d852e38..54b50c9c` outside `sdd/work/` holds four files: `sdd/specs/analyzer/RH8402.md`, the RH8204 and RH8402 code fixes, and the RH8402 test class. That matches handovers 11 to 15. `scripts/lane-check.sh bug-fix`: confirmed. The repair stays inside the surfaces of findings #1 and #2 (decisions 24 and 25), so scope did not grow and this is a repair-delta audit.

Evidence: static tracing, plus 11 probe tests in a throwaway worktree of `54b50c9c` under the scratchpad. All 11 passed. The worktree was removed afterwards, and no repository file other than this handover changed. The build result in 14 and the focused test results in 13 and 14 are trusted.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The `default:` branch and `StripPlaceholderTags` are deleted (`rg StripPlaceholderTags src`: no hits). `ExtractInnerContent` now returns the source text between the tags for an `XmlElementSyntax` and `string.Empty` otherwise. The analyzer registers only `XmlElement` and `XmlEmptyElement` and reports `xmlNode.GetLocation()`. For that exact span `FindNode(…, getInnermostNodeForTie: true)` returns the element itself, so dropping the `Span == diagnosticSpan` comparison changes no reachable result. An element without its own end tag is still withheld by `CanRewriteElement`. The empty-element path is covered by the existing `<placeholder/>` tests (lines 122, 178, 240, 633). |
| 2 | closed | Both failing members of #2's enumeration now pass: a header that ends with a line break with code on the next line, and the same header with N blank lines behind the run. They are tested (`FixWithHeaderEndingInLineBreak…`, LF and CRLF, N = 1 and 2), and 12 saw them fail before the change. The members #2 named as holding still hold. |

## thoenissen/Reihitsu#1269

### Scope

Repair delta against `1d852e3`, head `54b50c9c`. Only `RH8204DoNotUsePlaceholderElementsCodeFixProvider.ExtractInnerContent` changed (finding #1).

### Findings

_None._

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8204: what the fix writes in place of the element | The node `FindNode` returns for the diagnostic span: for an `XmlElementSyntax`, the source text from `StartTag.Span.End` to `EndTag.Span.Start`; otherwise empty | Analyzer `OnXmlElement` (kinds `XmlElement` and `XmlEmptyElement`, location = the element span); `CanRewriteElement` (same `FindNode` call) | Agree. For both reported kinds, the node found for the span is the reported element | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Placeholder content extraction | `ExtractInnerContent` only (`rg StripPlaceholderTags src`: none) | one owner |

### Assertion adequacy

_N/A — the repair adds no test for #1269. The deletion is covered by the existing RH8204 content and empty-element tests (`Verify`, exact text). 13 ran them (71 passed, RH8402 and RH8204)._

### Failure classes dismissed

- FC-006, FC-022: the content of an element is still copied verbatim from the source text.
- FC-009: every reported span still gets a change. An empty element is replaced by an empty string, as before.
- FC-031: the new `<remarks>` describe the code as it now stands.
- All other classes: unchanged from `10-auditor.md`. The repair changes only the unreachable branch.

### Architecture findings

_None._

### Hints

_None._

## thoenissen/Reihitsu#1342

### Scope

Repair delta against `1d852e3`, head `54b50c9c`. Files: `GetLeadingHeaderSpan`, `EndsWithLineBreak`, and `EnsureHeaderIsSeparatedFromCode` in the RH8402 code fix; the RH8402-006 spec text and its two new examples; the six new tests and the `VerifySpecificationFileFix` overload.

### Findings

_None._

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8402: end of the replaced span on the replace path | `headerEndsWithLineBreak` = `EndsWithLineBreak(expectedHeader)`, after `NormalizeLineEndings`. When it is true: through the run's last comment, the white space behind it on its line, and the one `EndOfLineTrivia` that ends that line (`commentRunLineEnd`). When it is false: through the comment and that white space only (`commentRunEnd`, unchanged) | `EnsureHeaderIsSeparatedFromCode`, which uses the same `EndsWithLineBreak` and adds a line break only when the header has none and the kept text does not start with one. RH8402-006 as widened. The analyzer's `StartsWith(sourceText, expectedHeader)` | Agree. A header that ends with a line break consumes exactly one line break of the file, or none when code follows on the comment's line, and the separator adds none. A header without one keeps the file's line break, and the separator adds one only in front of code on the same line. After the fix the file starts with the exact header, so the analyzer's `StartsWith` holds and the fix converges. | holds |
| Same span: the insert path and the no-comment path | Unchanged. `commentRunLineEnd` is read only when `hasCopyrightComment` is true | RH8402-005 | Untouched by the repair; equal to `main` (`git show 8ac5705:…` compared) | holds |

Class of #2, re-enumerated on the head. Axes: header ends with a line break (yes, no, two line breaks) × what follows the run's last comment × line ending. Every member was traced, and those marked "probe" were also run (all passed):

- Header with a line break, code on the next line: holds (test, LF and CRLF).
- Header with a line break, 1 or 2 blank lines: holds (tests).
- Header with a line break, a blank line then `#nullable enable`: holds (probe; one blank line kept).
- Header with a line break, `#nullable enable` directly on the next line: holds (probe; no blank line).
- Header with a line break, a `///` comment directly on the next line: holds (probe).
- Header with a line break, two copyright comments in the run: holds (probe; only the last comment's line break is consumed).
- Header with a line break, `/* … */` followed by white space and then a line break: holds (probe).
- Header with a line break, `/* … */ namespace N;` on one line: holds (test for LF, probe for CRLF).
- Header with a line break, a blank line in front of the run: holds (probe; the leading blank line is replaced as before).
- `copyrightText` ending in `\r\n` in an LF file: holds (probe; normalized, then treated as ending with a line break).
- Header ending in `\n\n`, one blank line behind the run: the output has the header's own blank line plus the file's one blank line (probe). The analyzer's `StartsWith` demands the header's blank line, and SHARED-FIX-006 demands the file's blank line, so this is the requirement's literal reading and not a finding.
- Header without a line break: unchanged (the RH8402-006 tests, plus probes for code on the next line and for code behind `*/`).

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| The header ends with a line break | `rg -n EndsWithLineBreak src`: the private `RH8402…CodeFixProvider.EndsWithLineBreak`, used by both call sites (`ApplyCodeFixAsync` → `GetLeadingHeaderSpan`, and `EnsureHeaderIsSeparatedFromCode`). There are other private string helpers of the same name in `Reihitsu.Cli/DiffGenerator.cs` and `Reihitsu.Tooling/UnifiedDiffWriter.cs`. Those decide diff output in assemblies the code fixes do not reference, and `SyntaxTriviaUtilities.EndsWithLineBreak` works on trivia, not strings | one owner of this decision |
| Old-header span | `GetLeadingHeaderSpan` only | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixWithHeaderEndingInLineBreakReplacesOldHeaderDirectlyInFrontOfCode` (+ CRLF) | No blank line is written in front of the code | One extra line break (the #2 counterexample) | `VerifySpecificationFileFix` → `Verify`: exact fixed text, one-pass fix, Fix All, re-analysis | adequate; failed before the change (12) |
| `…KeepsBlankLineBehindIt` (+ CRLF), `…KeepsTwoBlankLinesBehindIt` | The file's N blank lines stay, no more and no fewer | N+1 (head before the repair) or 0 (`main`) | same | adequate; failed before the change (12) |
| `…ReplacesOldDelimitedHeaderFollowedByCodeOnItsLine` | The header's own line break separates the code, and nothing is added | A blank line or a second line break in front of `namespace N;` | same | adequate as the other side of the condition (passes on both sides by design, as 12 states) |
| `VerifySpecificationFileFix(source, fixedSource, copyrightText)` overload | The two-argument helper delegates with `SpecificationCopyrightText` and `company = Example`, which is the same configuration `CreateSpecificationConfiguration` builds | — | — | No weakening of existing tests (39 passed in 12) |

### Failure classes dismissed

- FC-001, FC-002, FC-003: see *Guard scope*. Both sides of `headerEndsWithLineBreak` are tested, and the boundary members (code on the same line, the next line, blank lines, directive, doc comment) were traced or probed.
- FC-004, FC-005, FC-019, FC-020, FC-021: the span still stops at the first trivia that is not white space, a line break, or a comment. A directive or doc comment behind the run is never consumed (probes). At most one `EndOfLineTrivia` is added to the span, and the header writes it back.
- FC-007, FC-008: `Verify` runs the fix, Fix All, and re-analysis. After the fix the file starts with the exact header, so the analyzer reports nothing more.
- FC-009: no action becomes a no-op. The fix always writes the header.
- FC-010 to FC-014, FC-028, FC-038 to FC-043: RH8402 has no formatter path.
- FC-015, FC-016: see *Policy ownership*.
- FC-017, FC-033, FC-034: CRLF variants exist for the spec examples. A CRLF `copyrightText` in an LF file was probed.
- FC-031: the summary of `GetLeadingHeaderSpan`, its new parameter, the inline comment in the trivia loop, the comment above the `hasCopyrightComment` branch, and the `EndsWithLineBreak` summary all describe the new code.
- FC-032: 14 built `6ab3e5f4` with 0 warnings. `54b50c9c` adds only a handover.
- All other classes: unchanged from `10-auditor.md`.

### Architecture findings

_None._

### Hints

_None._

## thoenissen/Reihitsu#1343

### Scope

Repair delta against `1d852e3`: the repair touches no file of this issue.

### Findings

_None._

### Guard scope

_N/A — nothing changed for this issue in the repair; see `10-auditor.md`._

### Policy ownership

_N/A — nothing changed for this issue in the repair._

### Assertion adequacy

_N/A — no test of this issue was added or changed._

### Failure classes dismissed

All classes: as in `10-auditor.md`. The repair does not reach the RH8308, RH8309, or RH8401 paths.

### Architecture findings

_None._

### Hints

_None._

## thoenissen/Reihitsu#1459

### Scope

Repair delta against `1d852e3`: the repair touches no file of this issue.

### Findings

_None._

### Guard scope

_N/A — nothing changed for this issue in the repair; see `10-auditor.md`._

### Policy ownership

_N/A — nothing changed for this issue in the repair._

### Assertion adequacy

_N/A — no test of this issue was added or changed._

### Failure classes dismissed

All classes: as in `10-auditor.md`. The repair does not reach the RH8305 or RH8307 paths or `IsPrimaryConstructorRun`.

### Architecture findings

_None._

### Hints

_None._

## Whole run (repair delta)

- New findings: none. No repeat of the class of #1 or #2.
- Visible output (check 10): the new expected RH8402 values are demanded by unchanged requirements:
  - "no blank line" restores `main`'s output;
  - "N blank lines kept" is SHARED-FIX-006, the same ground `10-auditor.md` accepted for a header without a trailing line break.
  
  The RH8402-006 text change widens that requirement's coverage to the configuration and adds no new output decision. No `undecided output`. `documentation/` is unchanged.
- Cost of special cases (check 11): `headerEndsWithLineBreak` is one cheap check on the configured text and serves a valid configuration, not a rare syntax class. No shape is declared, so FC-042 does not apply.
- No `TODO`, commented-out code, or tracker number in the changed source (`git diff 1d852e38..54b50c9c -- src | rg "^\+.*(TODO|#[0-9]{3,})"`: none).
- The hints of `10-auditor.md` stand as recorded in `00-lead.md`: decision 26 for the RH8309 accessor case, and the follow-up candidate for the RH8307 predicate copy.
