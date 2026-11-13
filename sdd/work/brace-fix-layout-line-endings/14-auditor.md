Role: auditor
Commit: 53c7cd0d84a2ebb616506ce76e842c0760cff3ba
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

## Scope

Full audit. Head `53c7cd0d84a2ebb616506ce76e842c0760cff3ba` (checked out, clean tree). Base `origin/main` = `166e3dd2690edc7ed9cfd593d3e7d8eee8764c1b` (`git ls-remote` agrees), merged into the branch (`git merge-base --is-ancestor` true). Run diff `git diff 166e3dd..53c7cd0`: 5 production files under `src/Reihitsu.Analyzer.CodeFixes`, 22 test files, specs, change doc, work files — what handovers 06, 09, 12 describe. The merge of `166e3dd` brought 11 `src` files of other rules (RH4001, RH4009, …), none overlapping the run's files; the validator's run covers them.

Evidence was gathered with `scripts/apply-fix.sh` (LF and CRLF) on fixtures in the session scratch directory, against the head and against a scratch worktree of `166e3dd`, and with two throw-away probe tests in a scratch worktree of the head (both worktrees removed afterwards). Nothing in the repository was changed besides this file.

## thoenissen/Reihitsu#1416

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs:288-298` (`TryCreateClosingChange`) | FC-003; unrecorded withhold against SHARED-BRACE-002 | – | PR-introduced | A body whose last token has no line break behind it and is followed by the end of the file gets no fix. SHARED-BRACE-002 says the fix MUST wrap the body wherever SHARED-BRACE-003 does not withhold it, and SHARED-BRACE-003 has no such case. The change doc does not list it and no test covers it. `main` (`166e3dd`) offered the fix for each failing member (measured). Members: a top-level statement file ending in `if (args.Length > 0) System.Console.WriteLine();` without a final line break: no fix offered (fails). `if (args.Length > 0)⏎    System.Console.WriteLine(); // c` without a final line break: no fix offered (fails). The same files with a final line break: fixed (holds). Inside a type the next token is always a `}` or a member, so only top-level statements reach this. | Either write the closing brace without a trailing line break when the next token is the end of the file, which is what SHARED-BRACE-002 asks; or treat it as a rare class that is refused: a SHARED-BRACE-003 bullet, a change-doc entry, and a test. |
| 5 | low | `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs:2955-2960`, `RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.cs:987-990`, `RH5407UseBracesConsistentlyAnalyzerTests.cs:774-776` | FC-031 | – | PR-introduced | The `GetFixAllScenario` comments still describe the old mechanism: both bodies "resolve to the identical owning if-statement", "the batch fixer … collapses into a single reformat", "interference beyond that identical-owner collapse is unreachable", "two genuinely separate reformats". The fix no longer formats the owning `if`. Each body now yields its own text changes, and interference is reachable. Two nested RH5406 bodies, `if (b)⏎    if (a)⏎        M(a,⏎          b);`, both insert their `}` at the same position (probe below). | Reword the three comments to describe the text-change fixes, or drop the justification. Comment-only. |
| 6 | low | `src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs:30` | FC-015 | – | PR-introduced | `IndentationLevel = "    "` is a private copy of the indentation unit. `SyntaxIndentationUtilities.IndentSize` in `Reihitsu.Core` owns it, and other code fixes use it (`BlankLineCodeFixUtilities`, RH5103, `TargetAttributePlacementCodeFixProviderBase`). | `new string(' ', SyntaxIndentationUtilities.IndentSize)`. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Brace column / other code or tab in front of the owner (SHARED-BRACE-003 bullets 2 and 4) | Text from the start of the anchor's line to the anchor. The anchor is the `if`, extended by `FindLineAnchor` to the `else` of an `else if`, then to the `}` of the other block body, each only when the tokens are adjacent with whitespace-only trivia | SHARED-BRACE-002/-003 wording ("an `else` whose clause holds that `if`", "a closing brace that ends the other body … directly in front of that `else`, with only whitespace between them") | Agree. Traced and measured: `} else if` (anchor `}`), `else /* c */ if` (withheld, measured), `else`⏎`if` (anchor `if`), `if (x) M(); else if (b)` (withheld), `} /* c */ else` (withheld, tested) | holds |
| Directive or disabled text between header and body | `headerEnd.TrailingTrivia` + `firstToken.LeadingTrivia`, exactly the gap the spec names | SHARED-BRACE-003 bullet 3. The formatter guard `ControlFlowBraceTransform.cs:103` is no longer reached by the fix | Agree. Directive behind the body and between body lines: offered (tested) | holds |
| Multi-line body on the header's last line | Line of the body's first token against the line on which the body's last token or its last non-whitespace trailing trivia ends | SHARED-BRACE-003 bullet 1, header's-last-line reading (decision 13) | Agree; a delimited comment spanning lines behind the body counts (tested both sides) | holds |
| Header-line vs below-header body | First `EndOfLineTrivia` in the gap; a line break inside a delimited comment does not count | SHARED-BRACE-002 ("a line break outside a comment") | Agree (tested: `if (a) /* x⏎y */ M(a, b);`) | holds |
| Foreign line ending | From `headerEnd.Span.End` through `bodyLastLine.EndIncludingLineBreak` | SHARED-BRACE-003 bullet 5, SHARED-FIX-008 second paragraph | Agree. The span covers the removed blank lines, the header-line gap, and the body tail. LF above the `if` and behind the other body: offered (tested) | holds |
| End of file behind the body | `nextToken` is `None` or `EndOfFileToken` | none in SHARED-BRACE-002/-003 | Disagree: the code withholds, the spec demands the fix | finding #1 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line ending a fix writes | `LineEndingUtilities.DetectEndOfLine` (Core), forwarded by `ReihitsuFormatterHelpers.DetectEndOfLine`; the brace base calls the Core owner | one owner |
| Foreign line break in moved text | `LineEndingUtilities.HoldsOtherLineBreak`: `OrderingMoveSafety`, RH3004, RH7004, brace base, RH7309 | one owner |
| Indentation unit | `SyntaxIndentationUtilities.IndentSize`; private copies `StatementBracesCodeFixProviderBase.IndentationLevel` (new), `RH3004…IndentationWidth`, `RH7004…IndentationSize` (pre-existing) | finding #6 for the new copy |
| Brace-line anchor (`FindLineAnchor`, `IsSeparatedByWhitespaceOnly`) | `rg "IsSeparatedByWhitespaceOnly\|FindLineAnchor"`: only the brace base; no Core helper decides the same | one owner |
| Unsupported-shape refusal for the brace fixes | `ReihitsuFormatter.FindUnsupportedSyntax` call removed; the fix no longer runs the formatter, so SHARED-FIX-010 no longer applies (FC-043 dismissed) | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixKeepsTheHeaderTheBodyAndTheBlankLinesAsWritten(endOfLine)` and the other `(string endOfLine)` brace tests | Exact output, line endings included, LF and CRLF | Any changed token gap or line break | `VerifyCodeFixWithEndOfLine` (exact raw text) | adequate |
| `NoFixIsOffered…` brace tests (code in front, directives, tab, LF in moved code, lone `\r`, multi-line on header line) | No action registered; diagnostic still reported | An action is registered | `Verify` with markup + `GetCodeFixActionsAsync` / `VerifyNoCodeFixIsOffered` empty | adequate |
| `FixKeepsTrailingWhitespace…AndWritesNoneBehindTheClosingBrace` (#1627 shapes) | Whitespace stays on the body line; nothing behind `}` | `}···` | exact text | adequate |
| `FixAllWrapsBothBodiesOfAnIfAndAnElseOnOneLineInTwoIterations`, `FixAllWrapsTheOuterAndTheInnerBodyOfNestedIfsOnOneLineInTwoIterations` | Fix All converges to the specified text | Different text or iteration count | `Verify` with `NumberOfFixAllIterations = 2` | adequate |
| `GetFixAllScenario` (RH5405/06/07) | Two diagnostics corrected in one document | – | `BatchCodeFixTestsBase` | adequate as a test; comment stale (finding #5) |

### Failure classes dismissed
- FC-004, FC-019, FC-020, FC-021: the fix keeps the comments in the gap behind the header, comment lines above the body, and comments behind the body. It only trims spaces and tabs behind the header and in front of the next code. Tested with `//`, `/* */`, a multi-line `/* */`, `#pragma`, `#nullable`, `#region`, and disabled text.
- FC-005: the directive guard checks `IsDirective` and `DisabledTextTrivia` separately from comments.
- FC-006: the braces enclose exactly one embedded statement. Dangling-`else` shapes keep their binding: the `else` stays behind the inner `if` inside the new block, or the inner `if` with code in front is withheld.
- FC-007: one application removes the diagnostic. RH5030 and the layout reports on the result are the maintainer's answers 2 and 6.
- FC-008: probe in a scratch worktree of the head. Nested RH5406 bodies (`if (b)⏎    if (a)⏎        M(a,⏎          b);`) put two `}` insertions at one position, and Fix All still reaches the specified output (passes with `NumberOfFixAllIterations = -2`). An if body and an else body below their headers put their changes at different positions.
- FC-009: `RegisterCodeFixesAsync` runs the same `TryCreateChanges` that the apply path runs, and every registered action inserts braces.
- FC-010, FC-011, FC-012, FC-028, FC-030, FC-038 to FC-043: the brace fixes no longer call the formatter, and no formatter file is in the diff.
- FC-013, FC-014: the remaining analyzer/formatter differences on the fix's output (`} else`, `}` then `b++;` without a blank line, a misindented body) are decided by maintainer answers 2 and 6 and #1220 Q3, as recorded in the change doc.
- FC-022: verbatim strings spanning lines in a body below the `if` are kept, on the header line they are withheld, and a foreign break inside them is withheld (tested).
- FC-023, FC-024, FC-025, FC-029, FC-036, FC-018: the fix does not touch identifiers, declarations, expression bodies, paired layouts, or renames.
- FC-026: `else if`, `} else if`, two identical `M(a, b);` on one line, and nested `if`s are tested; positions are recomputed per diagnostic from the current root.
- FC-027: a body with its own braces and a body with a statement lambda are tested.
- FC-032: the merge of `166e3dd` changed no file the run touches; the validator re-runs.
- FC-033, FC-017 (line endings): lone `\r` documents are built explicitly (`ConvertEndOfLine`) and asserted on raw text.
- FC-034, FC-035: the #1416 instances from the issue and its comments are reproduced as written, and the test implementer's tests were red before the implementation (05, 06).
- FC-037: the lane check, not grep, decides the lane.

### Architecture findings
_None._ The skeptic's write-back finding (decision 10) is a follow-up; the brace fixes no longer reach it.

### Hints
_None._

## thoenissen/Reihitsu#1388

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | medium | `src/Reihitsu.Analyzer.Test/…/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs` (new RH7309-007 tests); code `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs:131` | FC-003 | – | same requirement | RH7309-007 says that a block which keeps its place among the blocks does not withhold the fix, even when it holds a foreign line break. The `order[slot] == slot → continue` branch implements that. No test covers it. The line-ending fixture (`LineEndingSource`) has two blocks, and both move. The "between the blocks" test covers only code outside every block. Deleting the `continue` (checking every block) would still pass every test. | Add a test with three blocks where one keeps its slot and holds the LF, in a CRLF document: the fix is offered, and the LF stays in place. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7309 withhold for a moved block | For each slot whose block changes: from the `#region` line start through the `#endregion` line's line break. The block text the fix moves ends before that break, so the check covers one break more than it moves | RH7309-007 ("the line break that ends that line included") and SHARED-FIX-008 second paragraph ("its trailing trivia included") | Agree; the extra break matches the spec's wording | holds |
| RH5011 lone-`\r` shift (`insertPosition--`) | One character in front of the insertion point, when the detected ending is `\n` | RH5011-004 (lone-`\r` document) | Agree for the specified case. It also applies in an LF-majority document whose `break` line ends in a lone `\r`, where the result `break;\n\r` keeps SHARED-FIX-008 (see Hints) | holds |
| RH8304 line break | Writes `DetectEndOfLine(root)` into whitespace in front of the element, never directly behind a line break | RH8304-004, SHARED-FIX-008 | Agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line break of an inserted line | `DetectEndOfLine` only. The private copier `DocumentationCommentCodeFixUtilities.GetLineBreak` was deleted. `MoveContentToNextDocumentationLineAsync` has one caller, RH8304. The other helpers in the file (`LayOutElementAsync`, RH8305/RH8307) go through the formatter and are unchanged | one owner |
| Foreign line break in moved blocks | `LineEndingUtilities.HoldsOtherLineBreak` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5011 `FixInsertsLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns`, `FixInsertsTheDetectedLineEndingWhenTheBreakLineEndsInAnotherOne` | Exact raw text, `break;\n\r` and `break;\n\r\n` | Any other break sequence | `ApplyCodeFixAsync` + `Assert.AreEqual` | adequate |
| RH8304 lone-`\r` and mixed tests | Exact raw text | Copied `\r` or LF | same | adequate |
| RH7309 `NoFixIsOffered…` (LF in moved block, LF in a comment, lone `\r`), `FixIsOffered…Between…`, `…AllCarriageReturnLineFeed` | Withheld or exact output | – | `VerifyNoCodeFixIsOffered`, exact text | adequate. The kept-place branch is missing (finding #4) |
| RH7004 characterization test | No drift with a comment or directive in front of the next statement | Drift | exact text | adequate |

### Failure classes dismissed
- FC-016: the skeptic enumerated the sibling fixes that copy line breaks. RH8402 and the formatter write-back are follow-up candidates in `00-lead.md`. The split fix's kept breaks are finding #3 under #1142.
- FC-017, FC-033: every line-ending test asserts raw text, and lone `\r` is built explicitly.
- FC-009: RH7309's withhold sits in `TryCreateReorder`, which the registration uses, so it never offers a no-op.
- Others as under #1416, or not reachable: the changes only insert a line break or add a guard.

### Architecture findings
_None._

### Hints
- RH5011-004 states the lone-`\r` output only for documents whose line breaks are all a lone `\r`. The code applies the same shift in an LF-majority document whose `break` line ends in a lone `\r`, and the result there meets SHARED-FIX-008. The spec author may state it; nothing is wrong.

## thoenissen/Reihitsu#1627

### Findings
_N/A — no finding._ The closing brace is inserted behind the line break that ends the body's last line, so the whitespace and comments at the end of that line stay where they are. Tested for RH5405 (below the `if`, on the `if` line, with `;`, `/* x */`, and `// x` in front of the whitespace), RH5406, and RH5407, under LF and CRLF.

### Guard scope
_N/A — no guard added for this issue; it is carried by the #1416 change._

### Policy ownership
_N/A — no new policy._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixKeepsTrailingWhitespaceOfABodyOnTheLineOfItsIfOnTheBodyLine(endOfLine, behindBody)` and the three `…WritesNoneBehindTheClosingBrace` tests | `···` stays on the body line, and `}` ends its line | `}···` | exact raw text | adequate |

### Failure classes dismissed
FC-016 (RH3202 and the other formatter-delegating fixes keep `}···`): a follow-up candidate in `00-lead.md`, outside the run. The other classes are as under #1416.

### Architecture findings
_None._

### Hints
_None._

## thoenissen/Reihitsu#1142

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs:127` (`StartsOnTheLineOfTheAccessorListBrace`) | FC-001 | – | same requirement | SHARED-ATTR-022 withholds the split for a list on the line of an accessor list's `{`. Its *Why* is that the whitespace at the start of that line is the indentation of the declaration that owns the braces, so a further list lands "in front of … the rest of the accessor list, and its `}`, at the column of the declaration that owns them, which is a layout nobody writes" (maintainer answer 7). The guard only checks accessor lists. The same cause applies to the `{` of a type, a namespace, and a block (measured, LF and CRLF). `class C { [A, B] void M() { } }` (RH5514) becomes `class C { [A]`⏎`[B] void M() { } }`; on `main` it was `class C {`⏎`    [A]`⏎`    [B]`⏎`    void M() { } }`. `namespace N { [A, B] class E { } }` (RH5506) becomes `namespace N { [A]`⏎`[B] class E { } }`. `void N() { [A, B] void L() { } }` (RH5514) becomes `    void N() { [A]`⏎`    [B] void L() { } }`. Holds: the accessor-list `{` (withheld) and a statement in front on the line (`int x = 0; [A]`⏎`        [B] void L() { }`, the same output as `main`). The change doc lists none of the failing outputs. | Refuse the rare class with the same cheap check, widened: withhold the split when the reported list starts on the line of the `{` that opens the body holding the list's owner (accessor list, type, namespace, block). State it in SHARED-ATTR-022 and the change doc. Withholding a fix for a rare class puts nothing in question. Keeping the current output is the other option; that output was not shown to the maintainer. |
| 3 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs:113-114` (`Target?.ToFullString()`, `attribute.ToString()`) | FC-016, FC-021 | – | same requirement | The lists are rebuilt from node text, so a line break inside the reported list is kept or repeated wherever it is part of that text. (a) A line break in the target specifier's trailing trivia is copied into every new list, and the first attribute's indentation is dropped. `    [field:`⏎`        A, B]` (RH5518) becomes `    [field:`⏎`A]`⏎`    [field:`⏎`B]` (measured). SHARED-ATTR-017 says the line breaks between the new lists are the only line breaks the fix writes. `main` already wrote `[field:`⏎`B]`, but it kept `A` in column 8. (b) A line break inside an attribute's argument list is kept with its own ending. In a CRLF document, `[A(1,\n      2), B]` becomes `[A(1,\n      2)]\r\n    [B]` (probe, raw text), against SHARED-FIX-008, first paragraph: a line break kept in the changed span takes the detected ending. The all-lone-`\r` member is the same: the `\r` is kept and `\n` is inserted. Holds: a line break between attributes (`[field: A,`⏎`B]` → `[field: A]`⏎`[field: B]`, better than `main`), after `[`, and before `]`. All of these are dropped, and a line break inside an attribute in an all-LF or all-CRLF document is kept as written. | Refuse the rare class: withhold the split fix when the target specifier's trivia holds a line break, or when the list holds a line break other than the detected ending (`LineEndingUtilities.HoldsOtherLineBreak` over the list's span). State it in SHARED-ATTR-017 or SHARED-ATTR-019 and the change doc, with a test per member. |
| 7 | low | `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs:110-112` | dead branch | – | PR-introduced | The `string.Empty` separator for a placement mode other than `SeparateLine` cannot be reached from any split provider: all 14 resolve `SeparateLine`, and only placement providers override `ResolvePlacementMode`. If it were reached, it would write `[A][B]`. | Drop the branch, or state why it stays. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Split withheld on the line of an accessor list's `{` (SHARED-ATTR-022) | Line of `AccessorListSyntax.OpenBraceToken` against the line of the list's start; only for a list whose parent is an accessor in an accessor list | SHARED-ATTR-022 *Why*: the line's whitespace belongs to the declaration that owns the braces | Disagree. The decision covers every `{` that opens the body holding the owner; the guard checks the accessor list only | finding #2 |
| Comment or directive inside the list | `SyntaxNodeUtilities.InteriorContainsCommentOrDirective`, checked both at registration and at apply | SHARED-ATTR-019 | Agree | holds |
| Inline auto-property accessor | `IsAttributeListOnInlineAutoPropertyAccessor` (unchanged), registration only | SHARED-ATTR-014, analyzer base | Agree | holds |
| Text the replacement keeps | `Target.ToFullString()` + `attribute.ToString()` | SHARED-ATTR-017 (only the separators are written), SHARED-FIX-008 first paragraph | Disagree for a line break in the target's trivia or inside an attribute | finding #3 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whitespace at the start of a line | `FormattingTextAnalysisUtilities.GetLeadingWhitespace` (Core), reused by the split fix | one owner |
| Line ending of the separator | `ReihitsuFormatterHelpers.DetectEndOfLine` → `LineEndingUtilities.DetectEndOfLine` | one owner |
| "List on the line of its accessor list's `{`" | only `StartsOnTheLineOfTheAccessorListBrace`. `AccessorOrderingUtilities` and `TargetAttributePlacementCodeFixProviderBase` read accessor braces for other decisions (09) | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Split CRLF tests (`…WithCarriageReturnLineFeed`, `VerifySplitFixWritesCarriageReturnLineFeed…`) | Every break the fix inserts is CRLF, exact text | An LF separator or a re-indented neighbour | `Verify` + `ApplyCodeFixAsync` raw-text `Assert.AreEqual` | adequate |
| RH5514 shared-line, code-around, tab, and switch-section tests | Only the reported list changes | Any other line moved | exact text | adequate |
| `GetFixAllScenario` of the 14 split rules (`[Second] [Third]`, `NumberOfFixAllIterations = -2`) | Each split changes only its own list under Fix All | `[Second]⏎[Third]` | `BatchCodeFixTestsBase` | adequate. The iteration bound is "at most 2", which matches what SHARED-ATTR-017 guarantees (the output, not the pass count) |
| SHARED-ATTR-022 tests (withheld on the `{` line; offered with `{` above or behind the property name) | Both sides of the line comparison | Offered on the `{` line, or withheld below it | `GetCodeFixActionsAsync` empty; `VerifyCodeFixWithEndOfLine` | adequate for the accessor list; the wider class is finding #2 |

### Failure classes dismissed
- FC-004, FC-019, FC-020, FC-021: comments and directives around the list keep their lines (tested). A comment inside the list withholds the fix (SHARED-ATTR-019).
- FC-006: attribute order, target, and arguments are kept, so the meaning is unchanged. A trailing comma (`[A, B,]`) is dropped as on `main`.
- FC-008: two lists on one line give non-overlapping replacements; tested in all 14 Fix All scenarios.
- FC-009: the registration predicate matches the apply path's early returns.
- FC-030: the fix replaces only the list's span with a text change, and the Roslyn cleanup that re-laid out the elastic trivia is gone.
- FC-013, FC-014: the placement rules report the lists and declarations that now share a line, by SHARED-ATTR-017 and #1109 Q1.
- Formatter-stability and precheck classes: the formatter is not involved.

### Architecture findings
_None._

### Hints
_None._

## Whole run

### Visible output is decided
Every expected value the diff adds or changes in a test of a visible surface is demanded by an unchanged requirement, or covered by a maintainer answer recorded in the change doc's `Decisions` or under `Maintainer answers` in `00-lead.md`:
- brace layout: answers 1 to 6 and #1220 Q3;
- the brace withholds: rare classes, decisions 7 to 9;
- RH5011 `break;\n\r`: decision 11, a rare document;
- split outputs: #1109 Q1, answer 7, decisions 6, 12, and 14.

No rule page changed. Two visible output changes that no test asserts are missing from the change doc: the top-level end-of-file withhold (finding #1) and the brace-line split outputs (finding #2). Both are rare classes, and each finding carries its suggested refusal.

### Cost of special cases
- RH5011's `insertPosition--` serves only lone-`\r` line endings. Withholding would need its own registration guard against an offered no-op (FC-009) and costs no less, so it is not a finding.
- `FindLineAnchor` serves `else if` and `} else`, which is code people commonly write.
- No shape is declared unsupported (answer 5), so nothing must be deleted under FC-042. The removed `FindUnsupportedSyntax` call was the last reason the brace fixes consulted the precheck.

### Checks
- Lane: `scripts/lane-check.sh behavior-change` → `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- Source hygiene: no `TODO` and no tracker number (`#123`, `issue 123`, `PR 123`) in the added source lines.
- Scope: no edit outside the run's components; RH7004 has a characterization test only.
- Build, format, and the default test set are trusted from 09 and 10; the validator runs next.

### Commands
- `scripts/apply-fix.sh` RH5405 (6 fixtures), RH5406 (1), RH5514 (2), RH5506 (1), RH5518 (2) on the head; RH5405, RH5514, and RH5518 on a scratch worktree of `166e3dd`.
- Two probe tests in a scratch worktree of the head, `scripts/test.sh --project analyzer --filter "FullyQualifiedName~AuditProbe…"`: nested RH5406 Fix All passed; the kept LF in a split attribute was confirmed as `[A(1,<LF>      2)]<CR><LF>    [B]`. Both worktrees were removed.
