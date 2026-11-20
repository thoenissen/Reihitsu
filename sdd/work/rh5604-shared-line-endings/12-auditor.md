Role: auditor
Commit: 05fc8bb8f78cd03f836b1f6c2406cd15af9878e7
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta, round 1, against `909b83b` (audit 1, `07-auditor.md`, findings #1 and #2). Head `05fc8bb` is checked out; base `origin/main` = `31d2931af8e2894fba9e66a281edd23b4de9ae6c` (fetched, equal to the prompt), an ancestor of the head. `scripts/lane-check.sh bug-fix`: confirmed.

`git diff 909b83b 05fc8bb -- . ':!sdd/work'` touches exactly what 08 and 09 describe: `sdd/specs/analyzer/RH5604.md` (RH5604-001 and RH5604-005 clarified to every line terminator, two examples, the `␍` convention, the intro), `sdd/specs/formatter/line-endings.md` (intro back to `origin/main`; `git diff 31d2931 HEAD` of that file is empty), and eight added `[Requirement]` attributes on four test methods in `RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.cs`. No production file, assertion, fixture, or data row changed. Scope did not grow. Production code, the other tests, and their evidence are byte-identical to audit 1, which is reused.

Maintainer answer 1 (A) is recorded under *Maintainer answers* in `00-lead.md`; decisions 6 and 7 are internal and put no output in question (their cases are neither demanded nor pinned).

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | The expected outputs of the three `VerifyForeignTerminator…` methods (16 rows) and of `VerifyLoneCarriageReturnInsideXmlDocumentationIsNormalizedToLf` are now demanded by RH5604-001 ("every line terminator FMT-EOL-001 names") and RH5604-005 ("whatever line terminator it is"), and covered by maintainer answer 1A. The tests carry both tags |
| 2 | closed | The implementation sentence and the restated code-owner rule are gone from both intros; the RH5604 intro keeps only behavior ("converts the file whatever else it holds, also a file the formatter leaves unchanged …"), which RH5604-005 and the two RH5604-005-tagged tests already demand |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

Checked against the code (`RH5604CodeMustNotContainMixedLineEndingsAnalyzer.OnSyntaxTree`, `LineEndingUtilities.DetectEndOfLine`, `LineEndingUtilities.NormalizeLineEndings`, `RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.ApplyCodeFixAsync`):

- RH5604-001 trigger "the line breaks of a file are not all the same": the analyzer reports when more than one distinct text occurs among `EndOfLineTrivia` and `XmlTextLiteralNewLineToken` (`counts.Count <= 1` returns). Same carriers as the counting clause; literals, block comments, and disabled text excluded as RH5604-003 and the non-goals say.
- RH5604-001 predominant ending "`\r\n` when it occurs at least as often as all other line breaks together, `\n` otherwise": `DetectEndOfLine` counts length-2 carriers against every length-1 carrier; agrees with FMT-PIPE-007's "a single-character break counts as `\n`". For a file of `\r\n` and lone `\r` only, or of lone `\r` and U+2028 only, the text and the code give the same predominant ending and the same reported lines.
- RH5604-001 "every line terminator": probed with Roslyn 5.12.0 (the repository's version, from the package cache) in a scratch console project outside the repository. A lone `\r`, U+0085, U+2028, and U+2029 at a directive's end each parse as one `EndOfLineTrivia` of that character; inside `///` each parses as one `XmlTextLiteralNewLineToken` of that character; no diagnostics, the text round-trips. The analyzer and the fix select by kind and text, so U+0085 and U+2029 at those two sites, which no test row pins, take the path the U+2028 rows pin.
- New RH5604-001 example (one file, lone `\r` after `int x;`, after `#pragma warning disable CS0168`, after `/// <summary>`): the analyzer's logic replayed on it gives two distinct endings, predominant `\n`, and exactly lines 3, 5, and 8 reported — the three marked spans, each from line start through the `\r` (RH5604-004). New RH5604-005 example: one line reported, and the kind-based rewrite to `\n` gives the shown output.
- The `␍` convention sentence ("a `␍` with code behind it on the same line is a lone `\r`") reads every block of the spec unambiguously; the earlier blocks have no `␍` with code behind it.

## Guard scope
_N/A — the repair changes no predicate, guard, or exemption; the guard-scope table of `07-auditor.md` holds for unchanged code._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which line terminators RH5604 reports and the fix rewrites | `RH5604.md` RH5604-001/005 now name them by reference to FMT-EOL-001 (`line-endings.md`), which lists them once; the code owners are unchanged (`07-auditor.md`) | one statement of the terminator list, referenced — holds |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyForeignTerminatorBetweenTokensIsFixedToThePredominantLineEnding` (8 rows, now RH5604-001, RH5604-005) | A lone `\r`, U+0085, U+2028, U+2029 between tokens is reported and fixed to the predominant ending, LF and CRLF | No diagnostic, a different span, or the terminator left in place; exact text compare | `Verify`, byte-exact fixtures | adequate; assertions unchanged since audit 1 |
| `VerifyForeignTerminatorAtTheEndOfADirectiveIsFixedToThePredominantLineEnding` (4 rows, same tags) | Same at a directive's end, `\r` and U+2028 | Same | `Verify` | adequate; U+0085/U+2029 share the kind (probe above) |
| `VerifyForeignTerminatorInsideADocumentationCommentIsFixedToThePredominantLineEnding` (4 rows, same tags) | Same inside `///`, `\r` and U+2028 | Same | `Verify` | adequate; same note |
| `VerifyLoneCarriageReturnInsideXmlDocumentationIsNormalizedToLf` (pre-existing, newly tagged) | Lone `\r` inside `///` of an LF file is reported on line 1 and fixed to `\n` | Same | `Verify` with `WithSpan(1, 1, 2, 1)` | adequate; the tags match what it asserts (the third site of the new RH5604-001 example) |

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-015, FC-016, FC-044: no predicate or owner changed in the repair.
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-006, FC-022..FC-029, FC-036: no rewrite changed; the spec now states behavior the unchanged code already has (probed above).
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-010..FC-014: no code-fix, convergence, idempotency, or phase interaction changed.
- FC-038..FC-043: no shape declared, no precheck touched.
- FC-017: see *Assertion adequacy*; no assertion changed.
- FC-031: no method body or summary changed.
- FC-032: the attributes are the last compiled change (`eeacf15`); the cleaner's Release build (0 warnings) and RH5604 47/47 in 09 and 10 ran after it.
- FC-033, FC-034, FC-035: fixtures and expected values unchanged since audit 1.
- FC-037: the test change is not claimed comment-only; it adds attributes.
- Check 11: no production code in the repair.

## Architecture findings
_None._

## Hints
- RH5604's intro still reads "reports every line whose line ending differs from the file's predominant line ending" without RH5604-001's condition. For a file whose line breaks are all one other terminator (all lone `\r`, say), FMT-PIPE-007 detects `\n`, so the intro read with that detection would claim every line is reported, while the analyzer reports none (`counts.Count <= 1`). The sentence predates the run and the natural reading ("the file's own predominant ending") is correct; decision 6 leaves the case unstated on purpose. Not a finding.
- U+0085 and U+2029 at a directive's end and inside `///` have no test row; the probe shows they produce the same carrier kinds as the pinned U+2028 rows, so the kind-based analyzer and fix treat them alike. Not a finding.

## Follow-up candidates
_None_ new.
