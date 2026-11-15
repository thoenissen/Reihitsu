Role: auditor
Commit: dff2aa91b8092aae06c248a1cf6916106541510a
Verdict: #1695: pass; #1696: pass; #1707: findings; #1721: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

State: the checkout is `dff2aa9` with a clean tree; `origin/main` and `git ls-remote origin refs/heads/main` are both `846aca462c94c0d85bc19a1d5ad9d78605590c29`, which is the merge base, so main is merged. The production diff is the four files `05-implementer.md` names (`ReihitsuFormatter.cs`, `CopyrightHeaderTemplateResolver.cs`, the RH8402 and RH3003 code fixes). The test diff is the seven test files `04-test-implementer.md` names. `scripts/lane-check.sh behavior-change` confirmed the lane.

Probes ran outside the repository, with fixtures in the session scratchpad. They used `scripts/apply-fix.sh` on head and on a temporary worktree of `origin/main` (removed afterwards), and `scripts/format.sh` for document formatting.

## thoenissen/Reihitsu#1695

### Scope
Full audit, head `dff2aa9` against base `846aca4`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | none | – |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Is the target's last line ended by its trailing trivia? (`EndTargetLineAsFormatted`, `ReihitsuFormatter.cs:588`) | Only the last trivia of the written-back trailing trivia | FMT-PIPE-010's "when that trivia ends in a line break". In parsed text, a token's trailing trivia runs through the first `EndOfLineTrivia`, so that trivia is always the last one. A multi-line `/* */` stays one trivia, and documents with syntax errors (skipped tokens) never reach the code. | Agree | ok |
| Which line break is rewritten | That final `EndOfLineTrivia` when its text is not `DetectEndOfLine(root)` | `PreparationPhase/LineEndings`, which rewrites every `EndOfLineTrivia` whose text is not `FormattingContext.EndOfLine`. Both leave comment text and literals alone. | Agree. Parity holds for `\n`, `\r\n`, a lone `\r`, and the one-character terminators Roslyn lexes as `EndOfLineTrivia`. | ok |
| Which whitespace is removed | `WhitespaceTrivia` directly in front of that line break (`TriviaListUtilities.RemoveTrailingWhitespace` on the list without its line break) | `CleanupPhase.CleanWhitespaceBeforeEndOfLine` (whitespace directly in front of an `EndOfLineTrivia`). `StripTrailingWhitespaceAtEndOfLine` only covers a line break in the next token's leading trivia, which parsed text never has behind a trailing trivia. | Agree. Whitespace in front of a comment (W2) and inside a `//` comment's text (W3) stay on both sides. | ok |
| Trailing trivia without a line break | Returned unchanged | Document formatting. A target followed by code on its line (T7) keeps its whitespace. A target at the end of a file without a final newline, `}···<EOF>`, also keeps it: RH5404 on `class D { int x; }···` as the file's last line gives `}···` on head, and `scripts/format.sh` on the formatted file keeps `}···` too. | Agree with document formatting (see *Hints*) | ok |
| Write-back sites | `WriteBackTrailingTrivia` (after `RemoveLeadingComments`), the general path of `FormatNodeInDocumentWithContextAsync`, and the non-shared-line branch of `FormatSplitFieldInContext` | `rg TrailingTrivia ReihitsuFormatter.cs`. The only other writes are the split path's shared-line branch and its member-separation branch, which already wrote `lineBreak` from `DetectEndOfLine(root)` and removed whitespace, and the preceding token's `StripBeforeFollowingLineBreak` (FMT-PIPE-010's first exception). `targetNode == root` returns the phases' output, which has no write-back. | All sites covered (FC-016) | ok |

Fusion check on the trailing side: the written line break is always `\n` or `\r\n`. A parsed lone `\r` is never followed by `\n`. A lone-`\r` blank line behind the target therefore gives `}\n\r`, two line breaks, and no blank line is lost. The leading-side fusion L1 is Decision 4.

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line ending detection | `LineEndingUtilities.DetectEndOfLine` (Core), forwarded by `ReihitsuFormatterHelpers.DetectEndOfLine` | Single owner, reused |
| Line-ending normalization of `EndOfLineTrivia` | `PreparationPhase/LineEndings`; `ReihitsuFormatter.EndTargetLineAsFormatted` (new, write-back only); `FormatSplitFieldInContext`'s `lineBreak` (pre-existing) | Two statements of the rule. Decision 3 accepted this as the narrow fix, and parity is shown above. No new finding. |
| Whitespace before a line break | `CleanupPhase.CleanWhitespaceBeforeEndOfLine` (private, phase); `TriviaListUtilities.RemoveTrailingWhitespace` (reused); `LineBreakTriviaUtilities.RemoveTrailingWhitespace` (forwarder); `EndOfLineWhitespace.StripBeforeFollowingLineBreak` | Pre-existing set of owners. The diff adds no copy and reuses the helper the split path already used for the same purpose. |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterNodeTrailingLineEndTests` (19 tests) | Exact document text, line endings included, after one pass, and unchanged after a second pass with the same target (except T7) | Any foreign or kept line break or space at the end of the target's last line. `Escape` makes `\r`/`\n` visible, and the comparison is `Assert.AreEqual` on the full text. | `AssertFormatsExactly`, `AssertFormatsUnderEveryLineEnding` (LF and CRLF) | Adequate. Both sides of each boundary are present: W1/W4/W5 removed, W2/W3 kept, T7 kept, comment-internal `\n` kept. |
| RH3202 / RH5111 / RH7101 `FixWritesTheDetectedLineEnding…InAMixedDocument` / `…LoneCarriageReturnDocument` | The code fix's exact output in mixed and lone-`\r` text | Same | `ApplyCodeFixAsync` on exact text, which avoids FC-033 | Adequate |
| RH3202 / RH5111 / RH7101 `FixRemovesWhitespace…` | The fix output under LF and CRLF | Spaces behind the result | `VerifyCodeFixWithEndOfLine` | Adequate |

### Failure classes dismissed
- FC-001, FC-002: see *Guard scope*. The qualifiers "ends in a line break" and "directly in front of" are explicit conditions.
- FC-003: both sides are tested for every boundary (see *Assertion adequacy*).
- FC-004, FC-005, FC-019–FC-021: comments behind the target are kept (`//`, `/* */`, multi-line `/* */`, and a comment kept on the inner token through `RemoveLeadingComments`). A directive cannot stand in a trailing trivia.
- FC-006, FC-022–FC-027, FC-029, FC-036: the change rewrites only whitespace and line-break trivia, never tokens.
- FC-007, FC-009: the fixes already converge. The change makes their output equal to what a second fix or format would write.
- FC-008: Fix All. The changed span grows only by the whitespace and line break at the end of the target's own line, so adjacent targets do not overlap. The existing Fix All scenarios of the three classes pass (`05-implementer.md`).
- FC-010–FC-012: no phase changed. The second pass is asserted.
- FC-013, FC-014: no analyzer is involved.
- FC-015: see *Policy ownership* (Decision 3).
- FC-016: all write-back sites are covered.
- FC-017: exact text, both line endings, and a second pass.
- FC-028: `FormatNode` writes no document trivia back, and FMT-PIPE-010 names only the two document entry points.
- FC-030: the change shrinks what the fix leaves behind.
- FC-031: the XML docs of both entry points, `WriteBackTrailingTrivia`, `FormatSplitFieldInContext`, and the new helper describe the new behavior.
- FC-032: the implementer ran every project after the compiled change.
- FC-033: mixed and lone-`\r` cases use exact text.
- FC-034, FC-035: expected values come from the spec examples (`04-test-implementer.md`) and fail on `main`.
- FC-037: not a text-only claim.
- FC-038–FC-043: no unsupported shape is involved.

### Architecture findings
_None_ beyond the skeptic's, which the lead decided (Decision 3).

### Hints
- Pre-existing and unrelated: whole-document formatting keeps whitespace behind the file's last token when the file has no final line break. `scripts/format.sh` on `namespace N;\n\nclass D\n{\n    int x;\n}···` leaves `}···`. FMT-CLEAN-005 says the file "ends with the last token's text". The CLI reaches this (`CleanupPhase.CleanTrailingTrivia`: `CleanWhitespaceBeforeEndOfLine` needs a line break, and `StripTrailingWhitespaceAtEndOfLine` reads the EOF token's empty leading trivia). The node-level entry points match this behavior, so it does not touch #1695. It is a follow-up candidate for the lead.

## thoenissen/Reihitsu#1696

### Scope
Full audit, head `dff2aa9` against base `846aca4`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | none | – |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Line ending of the inserted header and separator (RH8402-005) | `LineEndingUtilities.DetectEndOfLine` on the document's own syntax root, with `Environment.NewLine` when the tree is missing or the file has no line break | The formatter's detection, `ReihitsuFormatterHelpers.DetectEndOfLine` on the same root. It deliberately does not use `GetRootWithParsedDocumentation`, so the documentation-mode root cannot make the two counts differ (FC-040 analogue). | Agree | ok |
| Analyzer comparison after the fix (RH8402-002) | First line break of the file | The header the fix inserts stands at the file's start, so its own first line break is the file's first. In a lone-`\r` file the header is `\n`, and the comparison normalizes to `\n`. A header without an internal line break is not affected by the choice. Under RH8402-006 the kept line break behind the old run follows the header's inner breaks. | Agree. `VerifySpecificationFileFix` re-analyzes the fixed file. | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The fix's line ending | `LineEndingUtilities.DetectEndOfLine`. The private `GetPreferredLineEnding` copy is deleted. | Single owner (FC-015 closed) |
| Header line-break rewrite | `CopyrightHeaderTemplateResolver.NormalizeLineEndings(string, string)`. The `SourceText` overload delegates to it. | Single owner |
| Analyzer's first-line ending | `CopyrightHeaderTemplateResolver.NormalizeLineEndings(string, SourceText)` | Intended to stay (RH8402-002) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Six mixed and lone-`\r` RH8402 fix tests | Header and separator in the detected ending, and the rest of the file unchanged | A first-line ending or a lone `\r` in the header | `VerifySpecificationFileFix` (exact line endings, with the fixed file re-analyzed) | Adequate. They fail on `main` (`04-test-implementer.md`). |
| `FixWritesTheDetectedLineEndingWhenTheFirstLineEndsInTheMajorityLineEnding` | The other side of the boundary | – | same | Adequate |
| `FixInsertsHeaderInFrontOfCode`, `FixWithHeaderEndingInLineBreakWritesTheMachineLineEndingInAOneLineFile` | One-line file uses `Environment.NewLine` | A hard-coded `\n` on a host whose newline is `\r\n` | same | Cannot fail on the Linux-only CI (see *Hints*). There is no stronger test without a production seam. The behavior is the shared helper's fallback. |
| `HeaderInTheFirstLineEndingOfAFileWithMixedLineBreaksIsNotReported` | The analyzer still compares in the first line's ending | A diagnostic | `VerifySpecificationFile` | Adequate |

### Failure classes dismissed
- FC-001–FC-003: see *Guard scope*. Both sides of the first-line versus detected boundary are tested.
- FC-004, FC-005, FC-019–FC-021: header placement is unchanged, and the existing comment and directive tests pass.
- FC-006, FC-022–FC-027: no code is rewritten. This is a text insertion.
- FC-007: re-analysis inside the verifier.
- FC-008: one header per file, and the existing Fix All scenario passes.
- FC-009: unchanged.
- FC-010–FC-014, FC-028, FC-038–FC-043: no formatter phase or unsupported shape is involved.
- FC-015, FC-016: see *Policy ownership*. No other fix keeps a first-line copy (`rg "Lines\[0\]"` finds only the analyzer's).
- FC-017: see *Assertion adequacy*.
- FC-031: the summaries of both overloads and the new comment describe the new code.
- FC-033: exact text through the verifier.
- FC-035: expected values come from the change doc and RH8402-005.

### Architecture findings
_None._

### Hints
- The one-line-file arm of RH8402-005 (`Environment.NewLine`) is unobservable on CI, which runs only on `ubuntu-latest`. This is known to the run (`04`, `05`) and inherent. No finding.

## thoenissen/Reihitsu#1707

### Scope
Full audit, head `dff2aa9` against base `846aca4`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3003UseStringEmptyForEmptyStringsCodeFixProvider.cs:44-50`; tests `src/Reihitsu.Analyzer.Test/Clarity/RH3003UseStringEmptyForEmptyStringsAnalyzerTests.cs` | FC-006, FC-016, FC-003 | – | PR-introduced | The replacement now takes the literal's own trivia, which is often empty. A `""` that touches a word token with no trivia between them therefore becomes `string.Empty` glued to that token, giving code that does not compile or changes meaning. On `main`, the elastic trivia made the code-action clean-up insert the space. Failing members, observed with `scripts/apply-fix.sh RH3003` under LF and CRLF (head output, then the `main` output): **word token in front**: `return"";` → `returnstring.Empty;` (main `return string.Empty;`); `yield return"";` → `yield returnstring.Empty;`; `foreach (var c in"")` → `foreach (var c instring.Empty)`; `from x in a select"";` → `selectstring.Empty`. **Word token behind**: `return ""is string;` → `string.Emptyis string`; `return ""as object;` → `string.Emptyas object`; `return ""switch { _ => 1 };` → `string.Emptyswitch { … }`. `returnstring.Empty;` even parses, so the runner reports it as `fixed`. Holding members: punctuation on either side (`a+""`, `(""+s)`, `=""`), and any trivia between the literal and its neighbour (the run's tests). Not reported by the analyzer, so not members: `case"":` and `s is""` (constant contexts). No test covers the side with no trivia between the literal and a word token (FC-003). | Keep the explicit trivia, which fixes the reported drift. Add a cheap guard on each side: when the literal's leading trivia is empty and the previous token's text ends in an identifier-part character, give `string` one space of leading trivia; when its trailing trivia is empty and the next token's text starts with one, give `Empty` one space of trailing trivia. Add tests for both sides and for a punctuation neighbour that gets no space. The owner re-derives the guard. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Trivia of the new `string.Empty` tokens | Leading trivia of `string` = the literal's leading trivia; `.` and `Empty`'s leading trivia empty; `Empty`'s trailing = the literal's trailing | The token boundary with the neighbours. A string literal can abut a keyword or identifier with no trivia; `string` and `Empty` cannot. | Disagree: the rewrite inspects no neighbour (finding #1) | finding |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Explicit-trivia token construction in fixes that bypass the formatter | RH3001, RH3006 (each builds its own tokens), RH3003 (new) | No shared helper exists and none was added. RH3001 and RH3006 wrap in parentheses, which are punctuation, so the adjacency hazard of finding #1 does not reach them. |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3003 five drift tests (`FixOfCommented…`, `FixOfLiteralBehindACommentLine…`) | Every line break in front of the replaced literal keeps the document's ending | A line feed in a CRLF run, or the reverse on a CRLF host | `VerifyCodeFixWithEndOfLine` (LF and CRLF) | Adequate. They fail on `main` under CRLF. |
| RH3003 three holding tests | Other side of the boundary (no comment; comment in `+`'s trivia; no break in the gap) | Same | same | Adequate for the line-break boundary. The adjacency boundary has no test (finding #1). |
| RH3005 four characterization tests | Output unchanged from `main` | Drift | same | Adequate as characterization (Decision 7) |

### Failure classes dismissed
- FC-004, FC-005, FC-019–FC-021: comments in front of and behind the literal are tested and kept. A directive in front of the literal stays in its leading trivia, which is carried unchanged.
- FC-007: one pass. The diagnostic disappears for the compiling members.
- FC-008: the existing Fix All scenario passes. Replacements are per literal and do not overlap.
- FC-009: unchanged.
- FC-010–FC-014, FC-028, FC-038–FC-043: no formatter is involved, since the fix does not delegate to the formatter.
- FC-015: see *Policy ownership*.
- FC-017: both line endings are run.
- FC-022: the literal is replaced as a whole, and only `""` is reported.
- FC-031: the new inline comment describes the explicit trivia correctly. It will need the adjacency guard added once finding #1 is repaired.
- FC-033: `VerifyCodeFixWithEndOfLine` writes uniform endings, and the defect is about uniform documents.
- FC-034: the issue's own shape is the first test.
- FC-035: expected values are the input with `string.Empty` substituted.

### Architecture findings
_None._ Finding #1 is one local construction in one fix.

### Hints
- On `main`, the elastic clean-up also rewrote the spacing around the replacement next to punctuation (`a+""` → `a+ string.Empty`, `(""+s)` → `(string.Empty + s)`), against SHARED-FIX-006. Head keeps the author's spacing there, which is an unchanged requirement's output and not in question. The repair of #1 must keep it: add a space only next to a word token.

## thoenissen/Reihitsu#1721

Nothing to audit. The diff changes nothing for this issue (Decision 6). I confirmed that the tests Decision 6 names exist on head: `ReihitsuFormatterLineEndingIndependenceTests.KeptLineBreaksInsideLiteralsCommentsAndDisabledTextAreTheSameInTheCopy` and `ReihitsuFormatterTests.FormatSyntaxTreeWritesLineFeedWhenEveryLineBreakIsALoneCarriageReturn` (`rg`). No production or spec text for FMT-PIPE-007, FMT-EOL-004, or FMT-PIPE-019 changed.

## Whole run

### Visible output is decided
- FMT-PIPE-010 expected values (formatter class, RH3202, RH5111, RH7101): the whitespace values are covered by Q1 = B (`00-lead.md` *Maintainer answers*; change doc *Decisions*). The line-ending values are demanded by SHARED-FIX-008, which is unchanged.
- RH8402 expected values, including the changed `FixInsertsHeaderInFrontOfCode`: covered by Q2 = B.
- RH3003 and RH3005 expected values: demanded by SHARED-FIX-006, RH3003-004, and RH3005-004, which are unchanged.
- No rule page changed.
- No undecided output.

### Scope
No edit outside the run's scope. No `TODO`, and no tracker reference in the source diff (`rg` on added lines).

### Cost of special cases
None found. `EndTargetLineAsFormatted` is a cheap trivia check and serves every target that ends its line (the whitespace half), not only the rare mixed-ending class. No unsupported shape is involved.
