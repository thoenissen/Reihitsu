Role: auditor
Commit: 2a0087be859d03011871b0bfc1f240170ca99541
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit, first audit of the run. Head `2a0087b` (last code commit `3a6d91e`; later commits touch only `sdd/work/`), checked out in the working tree. `origin/main` = `6ab3d7567373d1ebc440fa19a630bca69f4c2fc0`, as in my prompt (re-fetched), and an ancestor of the head. The diff against it is what the handovers describe: `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs` (+39/−7), the new `src/Reihitsu.Formatter.Test/Regression/Cleanup/EndOfFileWhitespaceTests.cs`, `sdd/specs/formatter/cleanup.md`, and `sdd/work/`. `scripts/lane-check.sh bug-fix`: confirmed. Issue thoenissen/Reihitsu#1747 read; the decision it rests on, thoenissen/Reihitsu#923 comment 6232535812 Q3 ("Trailing whitespace after the last token of the file is removed whether or not a final line break follows; both inputs give the same output"), read and matches the skeptic's summary.

Executed (fixtures in the scratchpad, outside the repository): `scripts/format.sh` then `--check` over 16 files — the issue's exact input (LF and CRLF), `/** doc */···`, `/// <summary/>···`, `}` + U+00A0, `} /* a\n   b */···` (multi-line comment in the trailing trivia), CRLF `/* end */···`, top-level statement `...();···`, `#endregion···`, `#pragma ... X···`, `}···\n\n···`, `}\t/* c */\t`, `// end\n/* x */···`, block namespace `}···`, `\n\n···/* end */···`, byte order mark + `}···`. Every whitespace-trivia member ends at the comment's or token's text; the byte order mark stays; the `///` line and the directive line keep their inner whitespace (non-goals, thoenissen/Reihitsu#960 item 1 and FMT-DOC). Second pass (`--check`): `0 of 14` and `0 of 2 file(s) need formatting`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `sdd/specs/formatter/cleanup.md`, FMT-CLEAN-005, first sentence | FC-002 (spec wording) | – | same requirement | "MUST remove every line break, whitespace-only line, and whitespace trivia after the last token of a file, so the file ends with the last token's text" read alone demands removing the space in `} /* c */` (whitespace trivia after the last token) and ending the file with the last token — while the requirement's own third paragraph and example keep that space and end the file with `/* c */`, and the code does the same. The run added the "whitespace trivia" clause to a sentence whose "every line break" part already over-reaches against FMT-CLEAN-006. The rest of the requirement settles the reading, so no output is in question. Spec text only: it cannot reach compiled behavior, diagnostics, or rule metadata. | Narrow the first sentence to the condition the requirement states, e.g. "every line break and whitespace-only line after the last token of a file, and the whitespace trivia that ends the file, so the file ends with the text of its last token or of the comment behind it". |
| 2 | low | `sdd/work/final-whitespace-no-newline/00-lead.md`, `Maintainer answers` | – (check 10 trail) | – | same requirement | The run changes FMT-CLEAN-005, so check 10 can rest the new expected values only on a recorded maintainer answer; `Maintainer answers` says "None yet", and #923 Q3 appears only inside decision 2's `Why`. The answer exists and covers every member the tests assert (verified on the issue), so nothing is undecided; only its record is in the wrong place. Run note only: cannot reach compiled behavior. | Record #923 Q3 (comment 6232535812) under `Maintainer answers`, in English summary, and point decisions 2 and 3 at it. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whitespace trivia at the end of the last real token's trailing trivia ends the text (`CleanTrailingTrivia`, `else if` arm) | `original.GetNextToken(includeZeroWidth: true)` is `EndOfFileToken` and its whole leading trivia holds only whitespace and line breaks (`FindFirstSignificantTriviaIndex < 0`) | `EndOfLineWhitespace.StripBeforeFollowingLineBreak` (next token's leading trivia starts with a line break) and `CleanWhitespaceBeforeEndOfLine` (line break follows in the same list) | Disjoint and complementary: a parsed trailing trivia that ends in whitespace has no line break, so the end-of-file token's leading trivia is empty; a leading trivia that starts with a line break is the helper's case; one holding a comment or directive keeps the trailing arm off, so a gap such as `} /* c */` before a later comment line is untouched. Same condition as the line-break removal it sits beside (unchanged on `main`), so whitespace and line-break removal agree on "nothing significant follows". | holds |
| Whitespace at the end of the end-of-file token's leading trivia ends the text (`CleanLeadingTrivia`) | Only the `EndOfFileToken`; whitespace trivia at the list's end after `CleanWhitespaceBeforeEndOfLine` | Spec condition "nothing but whitespace trivia and line breaks follows the whitespace up to the end of the file" | Whitespace removed before line breaks, so a whitespace trivia in front of a comment (`[ws, comment, EOL]`) is never exposed at the end; directive and disabled-text trivia are not `WhitespaceTrivia`, so `#endif···` and a directive's own line break stay | holds |
| Byte order mark kept when the removal empties a list that starts the text (`RemoveWhitespaceThatEndsText`) | `startsText`, result empty, first trivia starts with U+FEFF | `CleanWhitespaceBeforeEndOfLine`'s byte-order-mark branch in the same class; `IndentationRewriter`, `BlankLineTokenCleanupRewriter` | Same outcome as the sibling branch (byte order mark alone, following whitespace dropped); `startsText: false` on the trailing arm is right, since a token's trailing trivia never starts the text. Exercised by the existing `ByteOrderMarkTests.ByteOrderMarkAloneBeforeLineBreakIsKept` second pass (implementer) and the CLI fixture | holds |
| Detached root (`nextToken` is `None`) | unchanged: line breaks only | FMT-CLEAN-008 | Split off unchanged | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Remove whitespace trivia at the end of a list | `TriviaListUtilities.RemoveTrailingWhitespace` (owner, reused); `LineBreakTriviaUtilities.RemoveTrailingWhitespace` forwards to it | one owner |
| Remove line breaks at the end of the file | `CleanupPhase.RemoveTrailingEndOfLineTrivia` (unchanged); `LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia` is the line-break phase's own join helper, a different decision | unchanged, pre-existing |
| Whitespace that ends a token's line | `EndOfLineWhitespace.StripBeforeFollowingLineBreak` (line break follows; consumers `CleanupPhase`, `ReihitsuFormatter.FormatNodeInDocumentAsync` for the token in front of a target); new end-of-text arm in `CleanupPhase` | Not a second owner of one decision: the end-of-text case has a single consumer (the token in front of a node-level target is never followed by the end of the text). See *Hints* for the helper's class summary. |
| Byte order mark survives whitespace removal | `ByteOrderMarkUtilities` (shared facts); branches in `CleanupPhase` (two), `IndentationRewriter`, `BlankLineTokenCleanupRewriter` | pre-existing per-phase pattern on the shared utility; the new branch agrees with its sibling (above) |
| Column of the end-of-file line | `LayoutComputer.IsAloneOnEndOfFileLine` (indentation, column 0 for a compilation unit) | The cleanup removal never changes a line-start column the indentation phase set: whitespace in front of the end-of-file token on its own line is already gone after indentation, and the other members remove whitespace behind text |
| End-of-file analyzer counterparts | `RH5602` (reports trailing whitespace, fix removes it — matches), `RH5603` (text edit, no formatter), `StatementBracesCodeFixProviderBase` (text edit) | no code fix passes the compilation unit as a formatter target (rg over `ReihitsuFormatter.*` calls in `Reihitsu.Analyzer.CodeFixes`); `FormatNodeInDocumentWithContextAsync` and `FormatNodeInDocumentAsync` write back the target's original trailing trivia, so no code-fix output changes |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 7 `AssertRuleResult` failing-member tests (spaces, tab, byte order mark, `} /* c */`, `/* end */`, two comments, comment-only) | exact end-of-file output | any trailing whitespace, a lost byte order mark, a lost interior space before `/* c */`, a lost blank line | `AssertRuleResult`: exact text, LF and CRLF, line-ending check, second pass, phase stability, per-phase round-trip | adequate (FC-017, FC-033: explicit escaped strings, nothing appended) |
| `OutputWithoutFinalLineBreakEqualsOutputWithFinalLineBreak` (7 rows) | FMT-CLEAN-005's normative equivalence | outputs differ, or both end in whitespace (`TrimEnd` arm) | `FormatSyntaxTree` both arms, `Unsupported` null, `AssertSecondPassIsStable` | adequate |
| `FormatDocumentRemovesTheSpacesBehindTheLastToken` | CLI path output | trailing spaces | `FormatDocumentAsync`, LF and CRLF, exact | adequate |
| `FormatNodeInDocumentOnTheCompilationUnitRemovesTheSpacesBehindTheLastToken` | root target path | trailing spaces | exact, LF and CRLF | adequate (no production caller passes the root; a direct call is allowed) |
| holding tests (`}···\n`, `}\n···`, `} /* c */\n···`, `#pragma ...\n···`) | main's already-correct behavior kept | trailing whitespace or a lost directive line break | `AssertRuleResult` | adequate |
| `GapsOnTheLineOfTheLastStatementAreKept`, `SpacesInsideASingleLineCommentThatEndsTheFileAreKept` | other side of the condition | a removed gap or comment text | `AssertRuleResult` unchanged | adequate; the guard's tighter boundary (the space between `}` and `/* c */`) is pinned by the `} /* c */` tests' exact expected value |
| `WhitespaceBehindAnEndIfThatEndsTheFileIsKept` (untagged) | main's directive-structure behavior | change inside directive or disabled text | `AssertRuleResult` unchanged | adequate as characterization (lead decision 7) |
| `GapInFrontOfATargetOnTheSameLineIsKept` (untagged) | node-level consumer of the helper unchanged | gap removed in `int a;   int b;` | exact, LF and CRLF | adequate |

Visible output (check 10): every changed expected value is formatter output at the end of a file and is demanded by #923 Q3, which covers each member (each equals the same input with a final line break, already today's output on `main`). Its record location is finding 2. No rule page, diagnostic, or code-fix expected value changes.

## Failure classes dismissed
- FC-003 — both sides tested: next token is the end-of-file token with only whitespace and line breaks (trailing arm taken) and with a comment (`/* end */` tests: arm not taken, trailing line break kept).
- FC-004, FC-005, FC-019, FC-020, FC-021 — the change deletes only `WhitespaceTrivia` at the end of the text; comments (block on the token's line, on their own line, `//`, `/** */`, `///`), directives (`#pragma`, `#endif`, `#endregion`) and disabled text are traced and executed above and keep their text and line breaks.
- FC-006, FC-022–FC-027, FC-029, FC-036 — no rewrite of syntax, literals, identifiers, members, or layout pairs.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043 — no code fix changes, and no code fix's output changes (see *Policy ownership*).
- FC-010, FC-011 — single pass, no loop; second pass stable under LF and CRLF in tests and in 16 CLI fixtures.
- FC-012 — no phase writes whitespace at the end of the text; `PhaseStabilityCheck` runs each phase alone on every output.
- FC-013, FC-014 — RH5602 reports exactly what is now removed; the formatter output no longer carries it.
- FC-015, FC-016 — see *Policy ownership*; all sibling members (both trivia lists, LF/CRLF/byte order mark/tab, comment positions) are covered.
- FC-028 — detached roots unchanged by design (lead decision 5, FMT-CLEAN-008); node-in-document targets write back their trailing trivia (FMT-PIPE-010); no caller observes a difference.
- FC-031 — summaries of the class, `CleanLeadingTrivia`, `CleanTrailingTrivia`, the new method, and the inline comments above the arms describe the new code.
- FC-032 — no project result reused across the compiled change; the implementer ran formatter, analyzer, and CLI tests after it.
- FC-034 — the tests use `class C` rather than the issue's `namespace N; class D`; same last token and trivia; the exact issue input was executed through the CLI (LF and CRLF) and holds.
- FC-035 — expected values are hand-written text, not taken from a patched copy.
- FC-037 — no comment-only claim.
- FC-038–FC-042 — no shape declared unsupported; no precheck change; check 11: the only special case is the byte order mark branch, which keeps an unchanged requirement (FMT-CLEAN-009) on the new path with a cheap check.

## Architecture findings
_None._

## Hints
- `EndOfLineWhitespace`'s class summary says it owns removing the whitespace "when the line ends right after it", which now reads as if it covered the end of the text; its method summary is precise (line break in the following token's leading trivia). A word in the class summary would keep the two owners' boundary obvious. Not a finding: the end-of-text case has one consumer.
- `EndOfFileWhitespaceTests.SpacesBehindTheLastTokenOfAFileWithAByteOrderMarkAreRemoved` writes the byte order mark as an invisible literal U+FEFF inside the string; the neighbouring `ByteOrderMarkTests` and `TrailingTriviaCleanupTests` use `﻿`, which shows the reader the fixture carries one. A literal one has precedent (`ReihitsuFormatterDocumentationElementTests`).
