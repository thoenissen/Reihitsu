Role: auditor
Commit: 3fb857502404baadabe31b7c47a4ff0cc4af1226
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It only named the repair range, the findings, and the decisions to audit against.

## Scope

- Head `3fb8575`. Base `origin/main` is `d6a01ee`, which is also the merge base, and `git ls-remote origin refs/heads/main` returns the same SHA, so main is merged.
- This is a repair delta against `261ba8f` (audit 13): `git diff 261ba8f 3fb8575`.
- Production changes: `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (decision 28) and `RH5203…CodeFixProvider.RegisterCodeFixesAsync` (decision 27). The cleaner made a comment-only change to the RH5204 analyzer (finding 7).
- Spec changes: RH5203-003, RH5417-003 and RH3202-002 were amended. SHARED-ATTR-008 and SHARED-ATTR-021 were reworded. No new requirement ID was added.
- Test changes: 15 tests in the RH5203, RH5417, RH3202 and RH5513 classes.
- The diff matches handovers 14–17. Scope did not grow beyond findings 1, 2, 4, 5, 6 and 7.
- `scripts/lane-check.sh bug-fix`: confirmed.
- Evidence: I built the head and ran `scripts/apply-fix.sh` (LF and CRLF) over fixtures in the session scratchpad. The test results are the roles' own. I re-ran no test.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The fix is registered only when `SyntaxTokenPositionUtilities.StartsLine(argument.GetFirstToken())` holds. Every member the finding listed is withheld and has a test, LF and CRLF: `/* */`, `/** */`, the case below a `//` line, a comment written directly against the argument, and the closing line of a multi-line comment. Members that held stay offered: comment lines above an argument that starts its line (`FixMovesADelimitedCommentLine…`, existing tests). I found no reported shape where `IsFirstOnLine` (the analyzer) and `StartsLine` (the fix) disagree other than a comment or a comment's tail in front of the argument, which is exactly the refused class. |
| 2 | closed for the members it listed | The type-header line (RH5417 with the sibling behind; RH3202 with siblings in front and behind) now keeps `main`'s output, and the spec examples are tested. **Finding #8** covers members of the same class that #2 did not name. |
| 3 | open, deferred | Decision 29 recorded it as a follow-up in `00-lead.md`. The repair did not touch it, which is intended. |
| 4 | closed | Each helper caller path now has a test with a comment in the gap, both sides: RH5417 ×2, RH3202 ×2, RH5513 ×1. |
| 5 | closed | RH5203-003 now names `//` and `/* */` and says a documentation comment keeps its column. The code moves only `SingleLineCommentTrivia` and `MultiLineCommentTrivia`. `FixLeavesADelimitedDocumentationCommentLine…` pins the `/** */` side. |
| 6 | closed | SHARED-ATTR-008 and SHARED-ATTR-021 both count "scope levels", a switch section's statements included. This matches `ComputeBaseIndentLevel` and `VerifyCodeFixIndentsInsideBlockOfSwitchSection`. |
| 7 | closed | The `SetDirectiveIndentation` summary no longer mentions a directive path of the fix. |

Decision 31 (`Closes #1144`, Review notes) concerns the PR text, which is not yet written. Nothing in the diff can show it. Decision 32: the RH5417 test input and expected output now start with `using System;` and a blank line, so the CRLF half carries CRLF. What the test asserts about the members is unchanged.

## thoenissen/Reihitsu#1140

Finding #8 is listed under #1144, because the RH5417/RH3202 shared-line helper is shared by both issues.

## thoenissen/Reihitsu#1141

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1144

- RH5203-003 (decision 27) holds. See finding 1 above.
- RH5417-003 and RH3202-002 (decision 28) hold for the type-header line. **Finding #8**: the new guard is wider than the requirement it implements.

## thoenissen/Reihitsu#1145

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1094

- The SHARED-ATTR-008/021 wording is consistent. See finding 6 above.
- The new RH5513 comment-gap test is adequate.
- Finding 3 is deferred (decision 29).

## thoenissen/Reihitsu#1393

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1511

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1561

Nothing to report. The repair did not touch it.

## Whole run

Audited SHA: `3fb857502404baadabe31b7c47a4ff0cc4af1226` (base `d6a01ee`).

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | medium | `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs:141-156` (`lineBeginsWithMember`) | FC-001, FC-002, FC-015, FC-007 | #2 | PR-introduced | **The predicate.** `lineBeginsWithMember` asks whether the first token at or behind the line start has *any* ancestor member of the same list. RH5417-003 and RH3202-002 ask whether the line "begins with a member of that type", and their *Why* reads that line's indentation as "the column of the member that begins its line". So the guard accepts lines that do not begin with a member's first token. On those lines the separated code goes to a column that is not a member column.<br><br>**Failing members** (run with `apply-fix.sh`, LF and CRLF, 1 iteration each):<br>(a) The continuation line of a sibling with a wrapped initializer, `int _a = Compute(1,⏎                     2); void M() { }`. RH5417 puts `void M()` and its braces at column 21. RH3202 gives the same shape, `2); int Get() => _a;`, also column 21.<br>(b) A statement line inside a sibling's body, `        A(); } void M() { }`. RH5417 puts `void M()` at column 8.<br>(c) The closing line of a sibling's verbatim string, `b"; void M() { }`. `FindToken` lands inside the literal and `GetNextToken` gives `;` of the sibling. RH5417 puts `void M()` at column 0.<br>(d) The tail line of the member's own leading multi-line comment, `       b */ void M() { } int _y;`. RH5417 puts `int _y;` at column 7.<br>In each case the class column is 4, so RH5204 then reports the moved line (FC-007). `main` kept these members on their line.<br><br>**Members that hold:** a line that starts with a sibling's or the member's own first token, including an attribute list (`[System.Obsolete] int Get() => _a; int _b;`, run); the type-header line (decision 28).<br><br>**Counterpart in the same file.** `TryGetListIndentation`/`StartsLineBehindComments` (SHARED-STMT-004) already decides "does this line's indentation give the list's column" for statements, and it requires both `StartsLineBehindComments(lineStartToken)` and `statement.GetFirstToken() == lineStartToken`. Its summary names "the tail of a comment that starts on an earlier line, a wrapped continuation, a nested statement, a closing brace" as giving no column. The member predicate has neither condition.<br><br>**Other component.** The attribute base still separates in shape (a) at the depth column (RH5513, `2); [Obsolete] void M()` gives `[Obsolete]` at column 4, run). So the two-owner disagreement from audit 13's architecture finding remains on these members.<br><br>Every member is rare: two members on one line behind a wrapped or nested construct. | Refuse this class with the same cheap check the statement counterpart uses: separate only when the line's first token starts its line behind whitespace and comments whose text starts on that line, and that token is the first token of the member or of a member of the same list (`AncestorsAndSelf().OfType<MemberDeclarationSyntax>().TakeWhile(m => m.GetFirstToken() == lineStartToken).Any(m => m.Parent == member.Parent)`). Add a test for a refused member, for example (a), next to the existing separated side. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5203 fix offered (decision 27) | The argument's first token: the previous token's trailing trivia plus its leading trivia must end with a line break after its last content, so no comment or comment tail stands in front of it on its line | RH5203-003 "nothing but whitespace in front of the argument since the line break"; the analyzer's `IsFirstOnLine` (report condition) | Agrees with the spec. It is narrower than the analyzer exactly by the refused class (a comment in front on the line) | holds |
| RH5203 comment-line kinds | `SingleLineCommentTrivia`, `MultiLineCommentTrivia` that start their line | RH5203-003 (`//`, `/* */`; documentation comments stay) | Agree | holds |
| Shared line: the line begins with a member (decision 28) | `root.FindToken(memberLine.Start)` (moved to the next token when it starts before the line), then any ancestor `MemberDeclarationSyntax` whose parent is the member's parent | RH5417-003/RH3202-002 ("the line begins with a member"); `TryGetListIndentation` + `StartsLineBehindComments` in the same file (first-token equality, start of line behind comments) | Disagree: the member predicate accepts continuation lines, nested lines, literal tails and comment tails | finding #8 |
| Shared line: whitespace-only gap | Unchanged from audit 13 | — | — | holds; comment side now tested |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether the indentation of a line gives the column of a list it begins | `BlankLineCodeFixUtilities.TryGetListIndentation`/`StartsLineBehindComments` (statements, SHARED-STMT-004) and the new `lineBeginsWithMember` in `FormatMemberOnLinesOfItsOwnAsync` (members); the line-start lookup `root.FindToken(line.Start)` + `GetNextToken` is copied at lines 143-148 and 267-272 | Two owners of one decision with different spans; finding #8. The shared lookup is a copy, not a drift |
| Token starts its line (SHARED-BLANK-008) | `SyntaxTokenPositionUtilities.StartsLine`, reused by the RH5203 fix; `IsFirstOnLine` is the analyzer's report condition | one owner for the fix; ok |
| Column of code separated from a sibling | `FormatMemberOnLinesOfItsOwnAsync` (line indentation) and `TargetAttributePlacementCodeFixProviderBase` (depth) | Still two owners. The type-header class is refused, but they still disagree on the members of finding #8 |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5203 five `NoFixIsOffered…` tests | Reported, no fix | An action registered | `VerifyNoCodeFixIsOffered`, LF + CRLF | adequate; the offered side is covered by the existing tests and `FixMovesADelimitedCommentLine…` (FC-003) |
| RH5203 `FixMovesADelimitedCommentLine…`, `FixLeavesADelimitedDocumentationCommentLine…` | The `/* */` line moves to the alignment column; the `/** */` line keeps its column | Comment column differs | `Verify`, exact, LF + CRLF | adequate |
| RH5417 `FixLeavesTheMembersOnTheLines…TypeWrittenOnOneLine`, `FixLeavesTheMemberInFront…` (amended, decision 32); RH3202 `FixLeavesTheMembersOnTheLines…` | The type-header line is not separated; the body is laid out from the member column | Separation, or another column | `Verify`, exact, LF + CRLF (both halves now carry their line ending) | adequate for the type-header member. No test pins a line that begins inside a sibling (finding #8) |
| RH5417 ×2, RH3202 ×2, RH5513 ×1 comment-gap tests | The comment stays in place, no separation | Comment moved or members separated | `Verify`, exact, LF + CRLF | adequate (closes #4) |

### Visible output is decided

Every new expected value is covered:

- RH5203 refusals by RH5203-003 as amended (decision 27, a rare-class refusal). The `/** */` column by RH5203-003 (decision 30, rare class).
- RH5417/RH3202 type-header outputs equal `main`'s output (decision 28, a rare-class refusal). Test implementer 15 cross-checked them on a `d6a01ee` worktree.
- The comment-gap characterizations equal `main` and the head alike.

No rule page changed. Nothing is undecided.

### Failure classes dismissed

- FC-003: decision 27 and decision 28 are each tested on both sides of their boundary. The boundary that finding #8 moves is untested, which belongs to #8.
- FC-004, FC-005, FC-019, FC-020, FC-021: the repair adds only refusals. A withheld RH5203 fix touches no trivia. The helper now breaks fewer gaps, never more. A directive cannot share a member's line.
- FC-006, FC-022–FC-026, FC-036: no token rewrite was added. The verbatim-string shape enters only through finding #8 (c).
- FC-008: the RH5203 BatchFixer registers per diagnostic, so withheld diagnostics drop out of Fix All. The helper path keeps `DocumentOrderFixAllProvider`.
- FC-009: every remaining registration changes text.
- FC-010–FC-014, FC-038–FC-043: no formatter phase, precheck or rule was added or changed. `FindUnsupportedSyntax(member)` is unchanged.
- FC-017: see the table above.
- FC-027: the multi-line and nested sibling forms are exactly finding #8.
- FC-028, FC-029, FC-030: no new formatter call. The refused shapes format the member as `main` did.
- FC-031: the summary of `FormatMemberOnLinesOfItsOwnAsync` and the RH5203 inline comment describe the new code. They state the intent ("a member column only when the line begins with a member"), which the code implements more widely (finding #8).
- FC-032: I built the head before every run.
- FC-033, FC-034: the CRLF harness artifact was corrected (decision 32). The fixtures run the auditor's own shapes.
- FC-035: the expected values come from the spec and `main`.
- FC-037: no text-only proof is claimed. The RH5204 change is a summary only.

### Architecture findings

The two-owner column from audit 13 persists. `FormatMemberOnLinesOfItsOwnAsync` (the indentation of the member's line) and `TargetAttributePlacementCodeFixProviderBase` (the SHARED-ATTR-008 depth) still both decide the column of code put on a new line to separate it from a sibling member.

Decision 28 refused one disagreeing class, the type-header line. They still disagree on a line that begins inside a sibling: for the continuation line of a wrapped field initializer, the attribute base gives column 4 and the helper gives column 21 (finding #8).

The same file already holds a third predicate for the same question on statements (`TryGetListIndentation`), with the stricter span. Finding #8 repeats #2's class (the same decision failing between the same components), so the lead decides it under *Architecture findings* rather than in another repair round.

### Hints

- `RH5203…ApplyCodeFixAsync` still sets `startsLine` from `previousToken.TrailingTrivia.Any(EndOfLineTrivia)`. Behind the new registration guard (`StartsLine` holds), I could not construct a reported input where that value is false, so the branch looks dead. Low value, not a finding.
- The helper copies the whitespace of the member's line as written, so a sibling at a wrong column (`      int _x; void M() { }`) passes its column on to the separated code. RH5204 already reports that line on `main`. This predates the run and is not part of #8.
