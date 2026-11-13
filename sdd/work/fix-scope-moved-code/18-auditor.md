Role: auditor
Commit: 2ee125d87462b0aafc4e00ba1366369631ba2c6b
Verdict: #969: findings; #1335: findings; #1263: pass; #1151: pass; #1604: findings

My prompt named the run, the base, and the files to read. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Head `2ee125d87462b0aafc4e00ba1366369631ba2c6b` (checked out, clean). Base `origin/main` = `4bdcd13186c2882e63985526458f63c4b359236d`, an ancestor of the head. Full audit of `git diff 4bdcd13..2ee125d`: 64 files. Production changes are in `OrderingMoveSafety`, `SyntaxIndentationUtilities`, `RegionDirectiveBlankLineUtilities`, `AccessorOrderingUtilities`, and `OrderingDeclarationUtilities` (deletion), and in the fixes RH3004, RH7004, RH7102/7103/7109 (base), RH7104, RH7105/7106 (base), RH7107, RH7108, RH7302, RH7303, and RH7310. The diff matches what handovers 07, 10, 13, and 14 describe. `scripts/lane-check.sh behavior-change` confirmed the lane.

How I checked: static tracing, then a scratch harness outside the repository that runs one diagnostic's analyzer and its first code action on a fixture byte for byte, without normalizing line endings. I ran the harness against the head and against a temporary worktree of the base, which I have since removed. Every counterexample below is a measured output.

## thoenissen/Reihitsu#969

### Findings
#1 (moved member with a right shift; member `f5b`) and #2 (member moves in mixed and lone-CR documents; `f8`, `f9`). The whole-line predicate, crossed declarations sharing a line, the separators, the `///`-behind-code refusal, and the left-shift boundary all hold.

## thoenissen/Reihitsu#1335

### Findings
- RH7004: #1 (`interp`, `f2`, `f3c`, `f4`) and #2 (`f10`, `f12`).
- RH7107/RH7108: #3 (one-line swap; `f6`, `f11`) and #2 (accessor list over several lines; `f13`).
- RH7104, RH7105, and RH7106 hold. The fixes now rewrite only the keywords, and the tests pin the exact output under LF and CRLF.

## thoenissen/Reihitsu#1263

### Findings
_None._ `RegionDirectiveBlankLineUtilities.RemoveDirectiveLines` handles the body edges and the joined runs as SHARED-FIX-007 *Removed directive line* and RH7303-004 state. I traced edges at `{` and `}` of any construct, a `case` label, doubled runs, and runs that start with a removed line. Q5 A covers it.

## thoenissen/Reihitsu#1151

### Findings
_None._ The RH7302 rename only touches the `#endregion` description when it equals the `#region` description as SHARED-REGDESC-001 reads both. The replaced span is the suffix of the trimmed text behind the keyword, so the keyword, the `//`, and the whitespace stay. I traced `#endregion   //items`, `// a // b`, an empty description, and the keyword case `#region end`. Q6 A covers it.

## thoenissen/Reihitsu#1604

### Findings
#1 and #2, through the RH7004 and member-move members listed above. The dropped helper goal (decision 3) and the RH3004 raw-string goal (decision 2) are recorded. RH3004 now uses the shared line rule, and its outputs on the base match the head for the tab-in-front-of-the-first-line case I measured.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` `StartsInsideContent` / `TryShiftMovedLines`; consumers `RH7004…CodeFixProvider.TryCreateChanges`, `OrderingMoveSafety.TryCreateWholeLineMove` | FC-001, FC-022, FC-015, FC-034 | – | PR-introduced (RH7004-007 and SHARED-ORDER-010 are requirements of the run) | The refusal asks whether a line starts *inside a token*. An interpolated string is several tokens, so a line inside it that starts *at* a token boundary (the `{` of a hole, or the closing `"`) counts as movable. A shift then writes whitespace into the string's value. RH7004 (measured `interp`): `var t = $@"a⏎{x}";` becomes `var t = $@"a⏎    {x}";`, so the value changes from `"a\n{x}"` to `"a\n    {x}"`. The base keeps `{x}` in column 0. Same with the closing quote (`f2`): `$@"a{x}⏎";` becomes `$@"a{x}⏎    ";`. Same with `@$"` (`f3c`). Member move with a right shift (`f5b`, RH7109; a field at column 2 whose slot is at column 4): `private readonly string _b = $@"a⏎{_a}";` becomes `…$@"a⏎  {_a}";`, while the base keeps the value. An accessor move with a right shift goes through the same helper (traced). Also offered against RH7004-007's text, without corruption (`f4`): `var t = $"{x⏎    + 1}";`, an interpolated string whose hole spans lines; the requirement withholds every string literal of any kind that spans lines. **Members that hold:** verbatim and raw literals, which are one token (refused). Interpolated raw strings `$"""`, with content in column 0 or indented (`f3`, `f3b`): refused. A left shift (a boundary line has no whitespace and is refused). Shift 0 (lines kept as written). RH3004, whose registration guard `WithholdsFixForSpanningStringOrComment` checks string *nodes* that span lines. **Test gap:** `RH7004…NoCodeFixForAnInterpolatedVerbatimStringOverTwoLinesAmongTheMovedStatements` covers only a line that starts inside the text token, a simplified cousin (FC-034). | Refuse the class with a cheap check on what the requirement names: withhold when a string expression (literal or interpolated string node) in the moved span spans lines and the shift is not 0, as RH3004's `IsString` check does, and keep one owner of that decision. Add tests for the `{`-at-line-start and closing-quote members for RH7004 and a right-shifted member. |
| 2 | high | `RH7004…CodeFixProvider.GetEndOfLine` and the moved text; `OrderingMoveSafety.TryCreateWholeLineMove` (moved, crossed, and gap text copied as written) | FC-017, FC-033 | – | PR-introduced (unchanged SHARED-FIX-008) | SHARED-FIX-008: every line break the fix creates or keeps in the code it changes, "such as one inside code it moves", MUST be the FMT-PIPE-007 line ending, and in an all-lone-`\r` document it MUST be `\n`. The fixes no longer go through the formatter, and they copy line breaks as written. RH7004 also picks the break of the declaration's line instead of the detected one. Measured: **mixed** CRLF document with LF on two lines. RH7004 (`f12`) writes `using (…)⏎`, `{⏎`, and the moved statements with LF; the base writes CRLF. RH7109 (`f8`): the moved field keeps `= 1\n`; the base writes CRLF. RH7107 with an accessor list over several lines (`f13`): the crossed `set` keeps LF; the base writes CRLF. **All lone `\r`** (`f9` RH7109, `f10` RH7004): every line break of the changed code stays `\r` (RH7004 writes new ones as `\r`); the base writes `\n`. **Holds:** uniform LF and uniform CRLF, which are the only arms the tests and `VerifyCodeFixWithEndOfLine` exercise. The one-line accessor swap, RH7302, RH7303/RH7310 (they only remove lines), and RH7104–RH7106 (they only change keywords). | Write the line ending `Reihitsu.Core.LineEndingUtilities.DetectEndOfLine` gives for every created line break and every line break in the replaced span. Or refuse the class (a replaced span that holds a line break other than the detected one). Add a mixed and a lone-`\r` test for one move fix and for RH7004. |
| 3 | high | `src/Reihitsu.Core/OrderingMoveSafety.cs` `TryCreateSwapWithinLine` (via `AccessorOrderingUtilities.TryCreateMove`); RH7107, RH7108 | FC-004, FC-021, FC-019 | – | PR-introduced (SHARED-ORDER-009 is a requirement of the run) | The swap exchanges only the accessors' `Span`s and leaves everything between them in place. On one line, every comment between or behind the accessors is *trailing* trivia of the accessor in front of it, so it is relocated to the other accessor. SHARED-ORDER-009 lets only the whitespace stay and says a comment behind the moved declaration travels with it. Measured `f11`: `{ set { } get => 1; /* g */ }` becomes `{ get => 1; set { } /* g */ }`, so the comment that followed `get` now follows `set` (the base keeps `get => 1; /* g */`). `f6`: `{ set { } /* s */ get => 1; }` becomes `{ get => 1; /* s */ set { } }`, so the comment behind the target `set` now follows `get` (the base keeps it behind `set`). **Holds:** a comment between `{` and the first accessor (`f7`, trailing trivia of `{`, not of an accessor), a comment inside an accessor's span, and lists without comments (the tests). | Refuse this class: withhold the swap when a comment stands in the accessor list outside the accessors' spans, between the target and the closing brace. It is a rare shape, and the check is cheap. Add a test for each failing member. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A moved line that cannot move (string, comment, disabled text) | Start of each line: `FindToken` and `FindTrivia` at the line start (`StartsInsideContent`) | RH7004-007 / SHARED-ORDER-010 text ("a string literal … that spans lines"); RH3004 `WithholdsFixForSpanningStringOrComment` (string nodes, multi-line comment trivia) | No: interpolated strings whose line starts at a token boundary | finding #1 |
| Whole-line move | `StartsItsLine` (target, moved), `IsLineStart(FullSpan.Start)` (target, moved), `EndsItsLine` (moved); crossed nodes that do not start a line join the block in front | SHARED-ORDER-009 | Yes; `///` behind code in front of the target or the moved node refused, crossed `///` stays with its node | ok |
| Swap within the line | Opening and closing brace of the list on one line; swap of `Span`s only | SHARED-ORDER-009 (whitespace stays, comments travel) | Brace condition agrees; the swapped span leaves out trailing comments | finding #3 |
| Line keeps its column | First non-whitespace position: directive kind; comment at column 0 with the next token behind the line end | SHARED-FIX-007 *Moved code* (comment line in column 0, `///` per marker, non-region directive) | Yes | ok |
| Left shift cut-off | Leading spaces and tabs of a moving line vs `-shift` | SHARED-ORDER-010 (tab counted as one) | Yes; boundary tested both sides | ok |
| RH7004 line-sharing refusals | Previous token's line, `SemicolonToken.LeadingTrivia`, moved span starting and ending at line starts | RH7004-007 four bullets | Yes | ok |
| RH3004 tab refusal | Lines after the opening brace line through the closing brace line, except the closing-brace line and blank lines | RH3004-006 | Yes | ok |
| Body edge for a removed directive | Last token of the line above the run is `{`, first token of the line below is `}` | SHARED-FIX-007 *Removed directive line* | Yes | ok |
| RH7302 rename | `GetRegionDescription` equals `GetEndRegionDescription`, non-empty | RH7302-005 / SHARED-REGDESC-001; RH7301 compares alike | Yes | ok |
| Offered vs applied | Each fix calls the same `TryCreate*` in `RegisterCodeFixesAsync` and `ApplyCodeFixAsync`; `FindUnsupportedSyntax` removed where nothing is formatted, kept for RH3004's lambda | FC-009, FC-043, SHARED-FIX-010 | Yes | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which moved lines move, keep their column, or are written empty | `SyntaxIndentationUtilities.TryShiftMovedLines`/`KeepsItsColumn`; used by `OrderingMoveSafety.TryCreateWholeLineMove`, `RH7004…TryCreateChanges`, and `RH3004…MoveStatementsAsync`/`GetMovingLineColumns`/`GetFirstStatementLinePosition` | one owner, ok (RH3004's private copy deleted, decision 19) |
| A string or comment that spans lines withholds a shifting fix | `SyntaxIndentationUtilities.StartsInsideContent` (line based); `RH3004…WithholdsFixForSpanningStringOrComment` + `IsString` (node based) | two owners that disagree, finding #1 |
| Line ending a fix writes | `Reihitsu.Core.LineEndingUtilities.DetectEndOfLine` (FMT-PIPE-007), `ReihitsuFormatterHelpers.DetectEndOfLine`; private `RH7004…GetEndOfLine` (line's own break); move helpers copy as written | private copy that disagrees, finding #2 |
| Whole-line move predicate | `OrderingMoveSafety.TryCreateWholeLineMove` only (old `MoveNodeBeforePreservingSeparators`, `MoveMemberBefore`, `MoveAccessorBefore` deleted, no references left) | ok |
| Blank lines left by a removed directive line | `RegionDirectiveBlankLineUtilities.RemoveDirectiveLines` (RH7303, RH7310) | ok for the run's fixes |
| `#endregion` description matching | `RegionDirectiveUtilities.GetRegionDescription`/`GetEndRegionDescription` (RH7302 fix, RH7301) | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyCodeFixWithEndOfLine` users (RH7004, RH710x, RH7302, RH7303, RH7310, RH7104–7106) | Exact fixed text, line endings included | Any byte difference, including a foreign line break | `Verify` plus `Assert.AreEqual` on `ApplyCodeFixAsync` | strong for uniform LF/CRLF; mixed and lone-`\r` never exercised (finding #2) |
| `RH7004…NoCodeFixForAnInterpolatedVerbatimStringOverTwoLinesAmongTheMovedStatements` | No fix for an interpolated verbatim string over lines | A fix offered | `VerifyNoCodeFixIsOffered` | weaker than RH7004-007: only the line-starts-in-text member (finding #1) |
| RH7109/RH7102/RH7103/RH7107 `NoCodeFixWhenAShifted…Holds…VerbatimString…`, `…RawString…`, `…Comment…`, `…DisabledText` | SHARED-ORDER-010 refusals | A fix offered | `VerifyNoCodeFixIsOffered` | adequate for one-token literals; no interpolated member (finding #1) |
| RH7107/RH7108 `…SwapPlacesWithinTheLine` | One-line swap, exact output | Line broken or joined, comment moved | `Verify` / `VerifyCodeFixWithEndOfLine` | no input holds a comment (finding #3) |
| RH7109 `LeftShiftMovesALineWithJustEnoughWhitespaceIntoColumnZero`, `NoCodeFixWhenALeftShiftWouldCutAMovingLineOffAtColumnZero`, tab variants | Left-shift boundary | Off-by-one refusal or cut | exact | adequate (FC-003) |
| RH7303/RH7310 blank-line tests (whitespace markers) | Which blank line stays, edges at any brace, `case` label | Wrong blank line kept, blank left at an edge | exact, LF/CRLF | adequate |
| RH7302 `CodeFixNeverRenamesTheEndRegionKeyword`, `…SaysMoreThanTheName…`, `…LineEndsWithWhitespace` | Only the description is renamed, and only on equality | Keyword or partial word renamed | exact | adequate |
| RH3004 tab and blank-line tests | RH3004-006 tab refusal; blank line written empty | Fix offered with a tab; whitespace kept | exact / no-fix | adequate |
| RH7104/RH7105/RH7106 `…LeavesTheTypeAsWritten`, `…KeepsTheExpressionBody…` | Only keywords change (SHARED-FIX-006) | Any formatting of the member or type | exact LF/CRLF | adequate |
| Fix All scenarios (`GetFixAllScenario`) of every touched fix | Two diagnostics fixed in one document, convergent | Overlapping change dropped forever | batch base | adequate |

### Visible output is decided
Every changed or added expected value I checked is covered. The covering answers are Q1–Q7, decisions 4, 5, 12, 18, 21–24, 26, 27, and 29, the change doc's *Decisions*, or unchanged SHARED-FIX-006/007 and RH7302-005/RH7303-004. These include: the RH7104 empty types that keep their braces, the RH7105 tabs behind region keywords, fixes now offered beside refused comments (decision 18 / SHARED-FIX-006, -010), the three blank lines kept above a moved member, the one-line accessor swaps (Q7 B), the RH7004 continuation-line and column-0 outputs (Q2, Q4), and the RH7310 kept whitespace blank line (unchanged from the base). No rule page changed except the RH7004 paragraph removal (decision 32, text only, convention). No `undecided output` finding.

### Failure classes dismissed
- FC-002, FC-003: no qualifier left out of the predicates, apart from finding #1; boundaries are tested on both sides.
- FC-005: RH7004 refuses comments and directives in front of the semicolon through `IsCommentOrDirective`; member moves keep `MoveRangeContainsDirectives`.
- FC-006: the RH7004 header `using (`…`)` keeps `await` and multiple declarators. Scope changes are guarded by the existing `HasFollowingStatementsThatPreventConversion`. Nothing else is rewritten.
- FC-007, FC-008, FC-009: register and apply use the same guard. Each fix removes its diagnostic. Fix All scenarios converge through the batch base. A move or swap change always alters the text.
- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028: no formatter phase changed, and the fixes no longer call the formatter, except RH3004, whose formatter use is unchanged.
- FC-016: every fix that formatted a larger node in the run's issues (skeptic 02 list) was changed. The remaining `FormatNodeInDocumentAsync` callers are outside the issues.
- FC-018, FC-023, FC-036: no rename. RH7302 only edits directive text.
- FC-020, FC-021: directives in moved code are covered by the per-line rule and the existing directive guards, which I traced. Comments are covered, apart from finding #3.
- FC-024, FC-025, FC-026, FC-029: no construct-specific rewrite. Positions are taken from one tree.
- FC-027: nested types and blocks shift relative to their own columns. A switch section parent is refused (`Parent is not BlockSyntax`).
- FC-030: this is the run's subject, and it is fixed for every named fix.
- FC-031, FC-037: summaries describe the new code (checked every changed method). No `TODO`, no tracker numbers. No text-only claim made.
- FC-032: the expected values come from the head build. The base outputs I measured fresh.
- FC-035: the expected values match the change doc's measured *Before*/*After*, and they are not derived from the implementation.
- FC-038, FC-039, FC-040, FC-041, FC-042: no unsupported shape is declared or changed. Both documentation modes are tested for the `///`-behind-code refusal. FC-043 is covered in *Guard scope*.

### Architecture findings
- **Two owners of "a string that spans lines withholds a shifting fix"** (finding #1). The components are `SyntaxIndentationUtilities.StartsInsideContent`, which is line based and used by RH7004 and the member and accessor moves, and `RH3004UseLambdaSyntaxCodeFixProvider.WithholdsFixForSpanningStringOrComment`/`IsString`, which is node based. They disagree on `var t = $@"a⏎{x}";`: RH3004 refuses it and the shared helper shifts it. The run created the shared helper (decision 19), so the second owner is within the run's components.
- **Private line-ending choice** (finding #2). `RH7004…GetEndOfLine` decides the line break the fix writes, and the move helpers copy line breaks as written. Meanwhile `LineEndingUtilities.DetectEndOfLine` owns the FMT-PIPE-007 decision. They disagree on mixed and lone-`\r` documents.

### Hints
- RH3004 (pre-existing, measured the same on the base, `f14`): the moved statements keep their own line breaks in a mixed document, against SHARED-FIX-008. This is a follow-up candidate if finding #2 is repaired without RH3004.
- `RH3004…MoveStatementsAsync`: when `TryShiftMovedLines` refuses, it returns `formattedDocument`, which is the formatter-laid-out lambda, not the original document. The registration guards make this unreachable today. Returning the original document would keep a guard drift from producing an unasked layout.
- RH7004 with a tab-indented declaration (`f15`): the new braces copy the declaration's `\t`, and the moved lines become `    \t…`. SHARED-FIX-007 decides the moved lines. "Laid out as the formatter lays it out" does not clearly decide the brace lines, no test pins either, and the base wrote spaces. It is rare (RH5601).
