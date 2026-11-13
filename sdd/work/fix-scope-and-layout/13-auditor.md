Role: auditor
Commit: 261ba8f9ab232bad1133cad3b1c822b816597524
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It only described what the run had already done.

## Scope

- Head `261ba8f`. Only `00-lead.md` changed after `d3c17e1`. `origin/main` and the merge base are both `d6a01ee` (`git ls-remote origin refs/heads/main` returns the same SHA), so main is merged. This is a full audit of `git diff d6a01ee HEAD`.
- The diff matches the handovers. Production changes are `OperatorSpacingCodeFixProviderBase`, `TargetAttributePlacementCodeFixProviderBase`, `BlankLineCodeFixUtilities`, `TrailingCommaCodeFixHelper`, and the RH3102, RH3201, RH3202, RH5103, RH5203, RH5204, RH5205 and RH5417 fixes. The RH5204 analyzer and `FormattingTextAnalysisUtilities.IsOperatorGapViolation` (now public) also changed. The brace fixes RH5403 and RH5405–RH5407 and their specs and tests are identical to `main` (decision 20). No change doc and no rule page changed.
- `scripts/lane-check.sh bug-fix`: `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`.
- Evidence: Debug build of the head, 0 errors. I put fixtures in the session scratchpad and ran `scripts/apply-fix.sh` under LF and CRLF on the head. A second worktree at `d6a01ee`, built in Debug, gave the `main` comparison. The test results are the roles' own; I re-ran no test.

## thoenissen/Reihitsu#1140

Narrowed by decisions 3 and 20 to RH5204 (with #1511 and #1561), RH6023/RH6024 (see #1141), and the shared-line fixes RH5417/RH3202 (see #1144). The PR links it with `Refs`.

- RH5204-006 holds. The fix replaces only the whitespace at the start of the reported line, using `BuildExpectedIndentationMap`. A `///` run moves as a whole. Later lines of a delimited comment keep their column (fixture `/** a⏎b */ case 2:`: only the first line moves). Region directives use the same path. The fix no longer calls the formatter, so SHARED-FIX-010 no longer applies. BatchFixer edits do not overlap, because each reported line gets one change and a `///` run holds no other reported line.
- Visible outputs: the RH5204 expected values are covered by maintainer answer 1A. That includes the blank line kept in `VerifyClosingBraceBehindStrayDocumentationCommentAndBlankLineIsDetectedAndFixed` and the continuation line that keeps its column (decision 12).
- No finding of its own. Findings #2 and #4 concern the shared-line helper, listed under #1144.

## thoenissen/Reihitsu#1141

- SHARED-OPSPACE-004 holds. The fix and the analyzer decide the gap with one function, `FormattingTextAnalysisUtilities.IsOperatorGapViolation`. A gap with a line break, a comment, or a directive stays as written.
- Fixture checks: `a+-b` gives `a + -b`. `a→+b` gives `a + b`. `$"{a+b}"` gives `$"{a + b}"`. `a+b*a-b` is fixed in a single pass.
- The changed expected values are demanded by SHARED-FIX-006, which is unchanged. Its own RH6024 example says the lambda stays.
- No finding.

## thoenissen/Reihitsu#1144

- RH5205-004: the fix is withheld when `lambda.AttributeLists.Count > 0`. That is a rare class, and the check is cheap.
- RH5409 trailing comma, RH3102 and RH3201 Q3 meet SHARED-FIX-007 for the reported shapes.
- RH5203 comment line: the output is maintainer answer 2A. **Finding #1** covers a comment on the argument's own line, and **finding #5** covers documentation comments.
- RH5417/RH3202 shared line: correct when the member's line starts with a sibling member. **Finding #2** covers a line that starts with the type's header. **Finding #4** covers the guard's untested other side.
- The `else if` brace column was left to #1416 (decision 20), so the PR links #1144 with `Refs` or names the dropped instance.

## thoenissen/Reihitsu#1145

- RH5103-003 and RH5103-005 hold. `HasEditableLeadingGap` now accepts any preceding statement whose last token is the token in front of the reported statement. The fixture with `if … else`, `do … while`, `{ }`, `switch { }` and `using (…) M(a);`, each followed by `M(a);`, was fixed in single passes, with every moved statement at column 8. RH5103-005 compares the line of the block's `{` with the line on which the preceding statement starts, which is exactly what the spec says.
- No finding.

## thoenissen/Reihitsu#1094

- Q2 (a sibling in front of the list) holds: `int _a; [Obsolete] int _b;` becomes `int _a;⏎[Obsolete]⏎int _b;`. **Finding #3** covers a sibling behind the member.
- Q3: SHARED-ATTR-021 holds in code, and the tab-counts-as-four rule follows decision 26. **Finding #6** covers the spec wording.
- Q4: characterization test.

## thoenissen/Reihitsu#1393

- Characterization test `FixMovesACommentBehindTheSecondOfTwoStatementsAboveItWithoutABlankLine` (LF and CRLF), as answer 3A asks. No production change. No finding.

## thoenissen/Reihitsu#1511

- This is covered by the RH5204 rewrite: the fix edits text, so FMT-PIPE-010's root-child rule no longer applies. The eight tests cover a first type with no leading trivia, behind `//`, `///` or a directive, a namespace, a misindented comment that starts the file, a later top-level type, and a file-scoped namespace. No finding.

## thoenissen/Reihitsu#1561

- RH5204-001 with SHARED-BLANK-008 holds. A token behind the closing line of a `/* */` or `/** */` comment that spans several lines is removed from the map. Its expected column still aligns the comment, which is still reported when it is out of place. A token behind a one-line `/**/` stays reported, as SHARED-COMMENT-011 requires. **Finding #7** is a stale comment in the same file.

## Whole run

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider.cs:58-87` | FC-007, FC-016 | – | PR-introduced | The RH5203 fix does not converge, and its output grows with every application, when a delimited comment stands in front of the argument on the argument's own line. Input: `M(1,⏎   /* a */ 2);` inside a method. Head: after 10 iterations, `/* a */` is in column 10 and more than 100 spaces separate it from `2`. Each application keeps the old gap and adds the alignment whitespace again (`apply-fix.sh`: `not-converged`, LF and CRLF). `main`: `no-progress` (2 iterations, the text stays stable). Failing members, traced or run: a `/* */` comment (run); a `/** */` comment (run); either one below a `//` comment line (run, `// two⏎   /* a */ 2`); a comment written directly against the argument with no space (traced, same branch). Members that hold: only comment lines above an argument that starts its line; a comment behind the comma; directive lines. RH5203-003 decides only "nothing but whitespace in front of the argument since the line break", so this class is unspecified. It is also rare: a comment inside an argument list is RH5038's class (decision 22). | Refuse this class: withhold the fix when anything but whitespace stands in front of the argument on its line. That is RH5203-003's own precondition. |
| 2 | medium | `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs:138` (`FormatMemberOnLinesOfItsOwnAsync`) | FC-001, FC-015 | – | PR-introduced | The column given to code that is separated from a sibling is the indentation of the member's first line. That column is right only when the line starts with a member of the same list. When the line starts with the type's header, the separated code lands in the type's column. RH5417, `namespace N { class C { void M() { } int _x; } }` with the class at column 4: head gives `class C { void M()⏎…⏎    int _x; }`, with `int _x;` at column 4 (the formatter and SHARED-ATTR-008 say 8). `main` left `} int _x; }` on one line. RH3202, `class C { int _value; public int GetValue() => _value; private int _x; }`: head puts `public int GetValue()`, its braces, and `private int _x;` at column 4. The `public` line is then reported by RH5204 (FC-007). Failing members: RH5417 with the sibling behind; RH3202 with the sibling in front and behind; all when the line starts with the type's `{` header. Members that hold: a line that starts with a sibling (the run's tests); the RH5513 path in the same shape (`class C { int _a; [Obsolete] void M() { } }` gives `[Obsolete]` at column 8, because it uses the depth column). The class is rare: RH5404 reports a type written on one line. | Refuse this class: separate only when the member's first line starts with a member of the same list, and leave the rest as on `main`. Alternatively, take the column from one owner (see *Architecture findings*). |
| 3 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs:129-156` | FC-016 | – | same requirement | The attribute-placement fix separates only a sibling in front of the list. A sibling behind the changed member stays on the line the fix edits, which the shared-line bullet of SHARED-FIX-007 forbids. RH5517, `[Obsolete] int _a; int _b;` gives `[Obsolete]⏎    int _a; int _b;` (head; `main` gives the same output, because the base had no shared-line handling). Member that holds: a sibling in front (`int _a; [Obsolete] int _b;` gives three lines). RH5417 and RH3202 handle both sides through `FormatMemberOnLinesOfItsOwnAsync`, so the same requirement is met on one side in one fix and on both sides in the others. No other rule reports two fields on one line. | Also break behind a member whose last line the fix edits, using `SharesLineWithSiblingMember(member, lastToken, nextToken)` as the helper does. Or record the member behind as a follow-up of #1094. |
| 4 | medium | `src/Reihitsu.Analyzer.Test/Formatting/RH5417…AnalyzerTests.cs`, `RH3202…AnalyzerTests.cs`, `RH5513…AnalyzerTests.cs` | FC-003, FC-019 | – | PR-introduced | `SharesLineWithSiblingMember` separates only when the gap holds nothing but whitespace. Every new test covers only that side. No test has a comment in the gap between the two members, for example `void M() { } /* c */ int _x;` or `int _a; /* c */ [Obsolete] void M()`. A future change that also separated across a comment, and moved or dropped it, would stay green. | Add one test per helper caller path (the RH5417 or RH3202 member fix and the attribute base): a comment in the gap, the comment kept in place, no separation. |
| 5 | low | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider.cs:71`; `sdd/specs/analyzer/RH5203.md` RH5203-003 | FC-002 | – | PR-introduced | RH5203-003 says that "a comment that starts its own line" between the argument and the comma MUST be moved. The code moves only `//` and `/* */`. In `M(1,⏎ /** two */⏎ 2);` the `/** two */` line stays in column 1 (run), and a `///` line is not moved either (traced). The class is rare: a documentation comment inside an argument list. | Make the spec and the code name the same kinds. The cheaper change is the spec: name `//` and `/* */`. |
| 6 | low | `sdd/specs/shared/attribute-targets.md:770` (SHARED-ATTR-021), `:246` (SHARED-ATTR-008) | FC-002 | – | PR-introduced | SHARED-ATTR-021 says "four spaces for each such brace that encloses it", while SHARED-ATTR-008's new sentence says "for each scope that encloses that brace". The code (`ComputeBaseIndentLevel(ancestor)`) also counts a switch section. For a block in a switch section (`case 1:⏎    {` at column 16 inside a `switch` at column 8), the code and the existing test `RH5513…VerifyCodeFixIndentsInsideBlockOfSwitchSection` offer the fix. Read literally, SHARED-ATTR-021 counts three enclosing braces, so 12, and would withhold it. | Spec text: count the scopes SHARED-ATTR-008 counts, a switch section included, in both sentences. |
| 7 | low | `src/Reihitsu.Analyzer/Rules/Layout/RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.cs:188` | FC-031 | – | PR-introduced | The `SetDirectiveIndentation` summary still says "Because the code fix's directive path reads this same map". The fix now has one path for every reported line. | Say "because the code fix reads this same map for every reported line". |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5204: the token behind a multi-line comment's closing line is not checked | The token's leading trivia: a `/* */` or `/** */` that starts above the token's line and ends on it | `SyntaxTokenPositionUtilities.StartsLine` (SHARED-BLANK-008) | Agrees for every multi-line case (the last line break lies inside the comment). It differs on purpose for a one-line `/**/` at the start of a line, which SHARED-COMMENT-011 keeps reported (04, the `StartsLine` attempt was reverted) | holds |
| RH5204 fix: the line edited | The whitespace at the start of the diagnostic's line (BOM skipped); for `///`, every line of the trivia | `BuildExpectedIndentationMap` keys | One map, one key per line | holds |
| RH5103: editable leading gap | Token in front == `previousStatement.GetLastToken(includeZeroWidth, includeSkipped)`; the gap is spaces and tabs only | The analyzer's preceding statement of the same list | Agree | holds |
| RH5103-005 | Line of the block's `{` == line of the preceding statement's first token | RH5103-005 text | Agree; both sides tested | holds |
| RH5205-004 | `LambdaExpressionSyntax.AttributeLists` | RH5205-004 ("an attribute list of its own") | Agree; parameter attributes are not counted, as the spec says | holds |
| SHARED-ATTR-021 | Leading whitespace of the line of the first enclosing brace in front of the list (tab = 4) | `ComputeBaseIndentLevel(ancestor) × 4`, the SHARED-ATTR-008 depth model | The code agrees with SHARED-ATTR-008's model; the spec wording is narrower | holds in code; finding #6 |
| Shared line, detection | `left.TrailingTrivia` + `right.LeadingTrivia` all whitespace; sibling = member whose parent is the member's parent | SHARED-FIX-007 shared line; the formatter's `MemberLineStartRewriter` | Agree | holds; finding #4 (test) |
| Shared line, column | Indentation of the member's first line (`BlankLineCodeFixUtilities`) vs. depth column (attribute base) | The formatter's member column | Disagree when the line starts with the type's header | finding #2 |
| Shared line, sides | Attribute base: the gap in front of `[` only; helper: both sides | SHARED-FIX-007 shared line (both sides) | Attribute base is narrower | finding #3 |
| RH5203 comment-line handling | The argument's first-token leading trivia, `startsLine` state machine | RH5203-003's precondition (only whitespace in front of the argument on its line) | Precondition never checked | finding #1 |
| Operator gaps | The two gaps next to the operator | `HasOperatorSpacingViolation` (the same `IsOperatorGapViolation`) | Identical | holds |
| Trailing comma | Whitespace run in front of the comma; spaces and tabs behind it; `, }` when the comma starts its line | SHARED-FIX-007 | Holds for the tested shapes; see *Hints* on the `}` column | holds |
| RH3102 line-break hand-over | The previous token's trailing trivia (whitespace only, no line break); the empty statement's trailing line break | SHARED-FIX-007 | Holds; the false side is a hint | holds |
| RH3201 whole-line removal | Whitespace in front of the comment on its first line and behind it on its last line | the formatter's line removal | Agree | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Operator gap violation | `FormattingTextAnalysisUtilities.IsOperatorGapViolation`, called by `HasOperatorSpacingViolation` (RH6023/RH6024 analyzers) and `OperatorSpacingCodeFixProviderBase` | one owner; ok |
| RH5204 expected indentation | `RH5204…Analyzer.BuildExpectedIndentationMap`, read by the fix | one owner; ok |
| A token starts its line behind a multi-line comment | `SyntaxTokenPositionUtilities.StartsLine` (shared) and the loop in `RH5204…Analyzer.AlignCommentIndentation` | Two owners. The difference (one-line `/**/`, SHARED-COMMENT-011) is justified in 04/07, and parity for the multi-line case was traced above. ok |
| Shared-line detection | `BlankLineCodeFixUtilities.SharesLineWithSiblingMember` (callers: `FormatMemberOnLinesOfItsOwnAsync` for RH5417/RH3202, `TargetAttributePlacementCodeFixProviderBase`) | one owner; ok |
| Column of code separated from a sibling | `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (`GetLineIndentation` of the member's line) and `TargetAttributePlacementCodeFixProviderBase.ApplyCodeFixAsync` (`ComputeBaseIndentLevel × 4`) | two owners that disagree; finding #2 |
| Line indentation read | `BlankLineCodeFixUtilities.GetLineIndentation` (private, predates the run), `FormattingTextAnalysisUtilities.GetLeadingWhitespace` (trailing-comma helper and attribute base), `RH5103…GetIndentation`, `RH5104…GetIndentation` | Copies that predate the run, which reuses existing ones; ok |
| Attribute depth model | `SyntaxIndentationUtilities.ComputeBaseIndentLevel`/`HasAnchorScopeAncestor`, reused | ok |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5204 `FixMovesOnlyTheReportedLine…`, `…Else…`, `…WrappedArgument…`, `…MemberLine…` | Only the reported line's indentation changes | Any other byte differs | `Verify`, exact output, LF and CRLF | adequate |
| RH5204 #1511 tests (8) | The first top-level element is fixed in one application with the rest as written | `no-progress`, or the body formatted | `Verify`, exact, LF and CRLF | adequate |
| RH5204 #1561 tests (8 no-diagnostic, 1 misindented comment) | No report behind a multi-line comment's closing line; the misplaced comment is still reported | A diagnostic on the token, or none on the comment | analyzer `Verify` | adequate; the one-line `/**/` side is covered by `VerifyMemberInScopeColumnBehindMisplacedBlockCommentIsDetected` |
| RH5204 `VerifyEmbeddedIfBehindMultiLineComment…OneApplicationPerDiagnostic` (+CRLF), `GetFixAllScenario` | One application per diagnostic; Fix All in one iteration | Iteration count differs | exact counts | adequate |
| RH6023/RH6024 new tests, Fix All tests | Exactly the two gaps change; Fix All in one iteration | Lambda, operand, or wrapped gap changed; 2 iterations | `Verify` (the framework default Fix All count is 1), LF and CRLF | adequate |
| RH5203 comment-line tests | Comment lines above the argument are aligned; the rest is unchanged | Comment column or another line differs | `Verify`, exact | adequate for the tested shapes; no test for a comment on the argument's line (finding #1) |
| RH5417 / RH3202 shared-line tests | The member and its sibling get lines of their own, at the member column, without blank lines | Shared line kept, wrong column, blank line | `Verify`, exact, LF and CRLF | the whitespace side is adequate; the comment side is missing (finding #4) |
| RH5513 `FixPutsTheAttributeListBehindAFieldOnALineOfItsOwn`; RH5511 five rewritten tests | The list gets its own line after a sibling in front | List still behind the sibling | `Verify`, exact | adequate; the sibling-behind side is not covered (finding #3) |
| RH5513 / RH5517 SHARED-ATTR-021 tests (withheld, offered at depth, tab) | Withheld off the depth column, offered on it, tab = 4 | Fix offered or withheld on the wrong side | `VerifyNoCodeFixIsOffered` / `Verify` | adequate, both sides (FC-003) |
| RH5103 12 kind tests + RH5103-005 both sides | The statement moves to the preceding statement's line column; withheld when `{` is on the line | No fix, or a fix in a one-line block | `Verify` / `VerifyNoCodeFixIsOffered`, LF and CRLF | adequate |
| RH5205 `NoFixIsOfferedForALambdaWithAnAttributeList` | Withheld | Fix offered | `VerifyNoCodeFixIsOffered`, LF and CRLF | adequate; the other side is covered by existing RH5205-003 tests |
| RH5409 trailing-comma tests (6) | No trailing whitespace; `}` in the `{` line's column; result equals the formatter | Trailing space; wrong column | `Verify` + `AssertFormattingChangesNothing` | adequate |
| RH3102 / RH3201 rewritten tests | The next statement keeps its line; no empty line left | Joined line; empty line | `Verify` / `ApplyCodeFixAsync` + `AssertFormattingChangesNothing` | adequate (one RH3102 test duplicates another, which is harmless) |
| Characterization tests (RH5101, RH5102, RH3106, RH3107, RH5104 #1393, RH5513 tab) | `main`'s output stays | Output differs | exact | adequate |

### Visible output is decided

Every changed expected value is covered:

- RH5204 by answers 1A and 4A.
- RH5203 by answer 2A.
- RH6023/RH6024 by SHARED-FIX-006 (unchanged; its RH6024 example).
- RH5417, RH3202, RH5511, RH5513, RH5409, RH3102 and RH3201 by SHARED-FIX-007 (unchanged).
- RH5103 by RH5103-003 and RH5103-004 (their unchanged core sentences).
- The withheld fixes (RH5103-005, RH5205-004, SHARED-ATTR-021) are rare-class refusals.
- No rule page changed. Nothing is undecided.

### Failure classes dismissed

- FC-004, FC-005: every new edit is gated to whitespace-only gaps (`SharesLineWithSiblingMember`, the RH3102 hand-over, the RH3201 whole-line check, the trailing-comma runs), and the RH5204 and RH6023/RH6024 fixes replace whitespace only. No comment, directive, or disabled text is dropped or moved in any fixture. Finding #1 adds whitespace but loses nothing.
- FC-006: no rewrite changes tokens other than the removal of `;` and `,`, which predates the run.
- FC-008: RH5204, RH6023/RH6024 and the attribute base use BatchFixer with non-overlapping text edits; for operators, two operators share a gap only when they are adjacent, which no binary or assignment pair the rules check produces. RH5417 and RH3202 switched to `DocumentOrderFixAllProvider`. Fix All tests pass with one iteration.
- FC-009: every remaining fix changes text whenever it is registered (the diagnostic implies a violating gap or line). The one exception is finding #1's class, which is a convergence finding.
- FC-010, FC-011, FC-012, FC-038–FC-042: no formatter phase or precheck changed, and no shape is declared (answer 4A).
- FC-013: the formatter keeps `b */ case 2:` behind the comment. RH5204 no longer reports it, which was the #1561 defect. The RH5203 report a wrapped argument can get after an RH5204 fix is accepted by RH5204-006 (decision 12, answer 1A).
- FC-014, FC-018, FC-036: no new rule and no rename.
- FC-020, FC-021: a directive cannot share a member's line. Comments in the gaps are refused by the whitespace-only checks (untested, finding #4). A `/* */` that ends mid-line is the #1561 shape, tested.
- FC-022: lines inside multi-line strings are excluded by the RH5204 analyzer's `GetStringLineIndices`. Operator gaps inside interpolation holes are edited like any other gap (fixture).
- FC-023, FC-024: no identifier or `partial`-dependent logic changed.
- FC-025: RH3202 handles methods only. Other expression-body kinds are a recorded follow-up candidate (04).
- FC-026: RH3102 re-resolves through `TrackNodes`, and the other edits are positional text changes on the original snapshot.
- FC-027: SHARED-ATTR-021 tested for a lambda block and a type brace; RH5103 for method, lambda and switch-section blocks.
- FC-028, FC-029: no node-level vs document comparison and no paired key/value layout changed.
- FC-030: RH5204 and RH6023/RH6024 no longer format a node. RH5417 and RH3202 format the member as on `main`.
- FC-032, FC-035: I built the head myself before every fixture. The expected values derive from requirements (03/06).
- FC-033, FC-034: the tests run the issues' shapes under LF and CRLF.
- FC-037: no text-only proof claimed.
- FC-043: RH5417/RH3202 ask `FindUnsupportedSyntax(member)`, while the formatter receives the member with only the whitespace of its first or last token changed. Whitespace cannot create or remove the declared comment shape. RH5205 is unchanged on that point.

### Architecture findings

- **Two owners of one column.** `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` and `TargetAttributePlacementCodeFixProviderBase.ApplyCodeFixAsync` both decide the column of code that a fix puts on a new line to separate it from a sibling member: the helper by the indentation of the member's first line, the base by the SHARED-ATTR-008 depth. Both components are in the run, and they disagree on `class C { int _a; … }` written on one line (finding #2: column 4 vs 8). The class is rare (RH5404), so refusing it ends the disagreement without a structural change.
- The #1140 structural cause is unchanged and already recorded as a follow-up candidate in `00-lead.md`: fixes choose scope by choosing a node, and fixes that need less predict the indentation phase.

### Hints

- RH3102: the new hand-over applies only when the previous token's trailing trivia is whitespace. `a++; /* c */ ;⏎a++;` still joins the two lines as on `main` (traced). This is rare, predates the run, and is untested.
- Decision 22 calls a comment inside an argument list a rare class (RH5038), yet the RH5203 comment-line code serves exactly that class. Maintainer answer 2A decided its output, so it is not a check-11 finding.
- `TrailingCommaCodeFixHelper`'s `, }` branch takes the `}` column from the indentation of the `{` line, which predicts the indentation phase (recorded in 04). Not verified for an initializer whose `{` ends a header line, such as `new[] {`.
- `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5417…CodeFixProvider.cs` gained a UTF-8 BOM, which matches `.editorconfig` (`charset = utf-8-bom`).
