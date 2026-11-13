Role: auditor
Commit: 1f8d728df2cef930d3bd52761ea325141170c3a6
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the repair range, finding 8 of audit 19, and decisions 33–34.

## Scope

- Head `1f8d728`. Base `origin/main` is `d6a01ee`. It is also the merge base, and `git ls-remote origin refs/heads/main` returns the same SHA, so main is merged.
- The branch has since moved to `15194d2`, which adds only `24-spec-checker.md`. As instructed, I ignored it.
- This is a repair delta against `3fb8575` (audit 19): `git diff 3fb8575 1f8d728`.
  - Production: `BlankLineCodeFixUtilities.cs`. The `lineBeginsWithMember` predicate changed, and the new private helper `GetLineStartToken` replaces the copied line-start lookup.
  - Specs: RH5417-003 and RH3202-002 were clarified, with three examples each.
  - Tests: five new tests each in the RH5417 and RH3202 classes.
- The diff matches handovers 20–23. Scope did not grow beyond decision 33.
- `scripts/lane-check.sh bug-fix`: confirmed.
- Evidence:
  - I ran `scripts/apply-fix.sh` (LF and CRLF, up to 3 iterations) for RH5417 and RH3202 over my own fixtures in the session scratchpad.
  - I ran a small Roslyn probe (a scratchpad console project against the Release Roslyn and `Reihitsu.Core` DLLs) to check the RH5203 claim in handover 21.
  - The test results are the roles' own. I re-ran no test.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Unchanged since audit 19. |
| 2 | closed | Unchanged since audit 19. The type-header line still has its tests. |
| 3 | open, deferred | Decision 29 made it a follow-up. The repair did not touch it, which is intended. |
| 4–7 | closed | Unchanged since audit 19. |
| 8 | closed | All four listed members are now refused and keep the output test implementer 20 derived from `main`. Each has an exact LF+CRLF test for both rules: (a) the continuation of a wrapped initializer, (b) a statement of a sibling's body, (c) the closing line of a verbatim string, (d) the tail of the member's own multi-line comment. The members that held still separate: a sibling's or the member's own first token, an attribute list (probe `[System.Obsolete] void M() { } int _y;` separated at column 4), and a comment that starts on the line. The comment case is tested: `FixSeparates…BeginsItsLineBehindAComment`. |

Decision 33 also asked for the dead `startsLine` initializer in RH5203 `ApplyCodeFixAsync` to be removed. That was not done, and the reason handover 21 gives does not hold: see **Finding #9**.

Decision 34 dropped a missing-comma defect. That defect rests on the same premise, and my probe refutes it: `startsLine` is `true` on that input.

## thoenissen/Reihitsu#1140

Finding #9 and the separation predicate are listed under #1144, because the RH5417/RH3202 shared-line helper and the RH5203 fix belong to both issues.

## thoenissen/Reihitsu#1141

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1144

### Against the specs

The rewritten RH5417-003 and RH3202-002 define "the line begins with a member" by three conditions. Each holds in the code (`BlankLineCodeFixUtilities.cs:146-151`):

| Spec condition | Code |
|---|---|
| No earlier token reaches into the line | `StartsLineBehindComments`: `previousToken.Span.End <= lineStart` |
| Only whitespace and comments that start on the line stand in front | Same method's trivia filter. `IsCommentTrivia` covers documentation comments, matching the spec's "comments" |
| The line's first token is the first token of the member itself or of a member in front, in the same list | `AncestorsAndSelf().OfType<MemberDeclarationSyntax>().TakeWhile(first token equal).Any(Parent == member.Parent)`. The token lies at or before the member's first token, because `GetLineStartToken` returns the first token that starts on the member's own line. So "in front" needs no extra check |

The three examples of each spec match the new tests' input and output byte for byte, apart from the RH5417 (d) test, which omits the `int _a;` lead-in.

Probes on the refused side (all apply-fix, LF and CRLF, 1 action, converged):

| Input | Output |
|---|---|
| `    } void M() { }`, the closing brace of a sibling's body | Not separated, braces at column 6. The spec names this shape ("closing brace inside a sibling's body") |
| `    int _x; /* a⏎       b */ void M() { } int _y;`, a comment tail held in the previous token's trailing trivia | Not separated |
| A raw string's closing line, `        """; void M() { } int _y;` | Not separated |
| RH3202 `    } int Get() => _a;` | Not separated, braces at column 6 |

Probes on the separated side (all at column 4, the member column):

- `/** d */ void M() { } int _y;`
- `class I { } void M() { } int _y;`
- `public int P { get; set; } void M() { } int _y;`
- RH3202 `/** d */ int Get() => _a; int _y;`
- RH3202 `int _a; int Get() => Compute(1,⏎ 2); int _y;`: the member's last line begins with a continuation, but the column comes from the member's first line, which begins with a member.

### Findings

**Finding #9** (Whole run).

## thoenissen/Reihitsu#1145

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1094

Nothing to report. The repair did not touch it. Finding 3 stays deferred.

## thoenissen/Reihitsu#1393

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1511

Nothing to report. The repair did not touch it.

## thoenissen/Reihitsu#1561

Nothing to report. The repair did not touch it.

## Whole run

Audited SHA: `1f8d728df2cef930d3bd52761ea325141170c3a6` (base `d6a01ee`).

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | low | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider.cs:56` (`startsLine` initializer) | – (dead branch) | – (audit 19 raised it as a hint, not a finding) | PR-introduced | **What the run does.** Decision 27's registration guard offers the fix only when `SyntaxTokenPositionUtilities.StartsLine(firstToken)` holds. Decision 33 then asked for the initializer that became dead to be removed.<br><br>**Why it is still there.** Handover 21 kept it, on the argument that `StartsLine` judges against the token with text in front while `ApplyCodeFixAsync`'s `GetPreviousToken()` returns a zero-width missing token. That is not so. `StartsLine` reaches `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`, which calls the same `token.GetPreviousToken()` with the same defaults (`includeZeroWidth: false`).<br><br>**Probe.** Parsed in a scratchpad console against the Release Roslyn DLLs:<br>- `M(a⏎    b)` (missing comma): the previous token is `a`, `StartsLine` is `true`, and the initializer is `true`.<br>- `M(a, /* x⏎ */⏎    b)` and a `#if`/`#endif` gap: both `true`/`true`.<br><br>Trailing trivia always takes the first line break behind a token, so whenever the guard holds, the initializer is `true`. Its `false` value is unreachable behind the guard. The missing-comma defect that decision 34 dropped does not arise from this value. | Replace the initializer with `true` (or drop the variable's initial computation), as decision 33 states. No test changes. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Shared line: the member's line begins with a member (decision 33) | From the start of the member's first line to `GetLineStartToken(root, memberLine)`: the previous token's span end, trivia ending behind the line start (whitespace, or comments starting on the line), and the ancestor members whose first token is that token | `TryGetListIndentation` (SHARED-STMT-004), which calls the same `StartsLineBehindComments` and uses the same `TakeWhile(first token equal).Any(same parent)` shape over statements. RH5417-003 and RH3202-002 as rewritten | Agree: the same span and the same boundaries. The only difference is the anchor line, which is the member's own line here and the previous sibling's line for statements. That follows from the decision each one answers | holds (closes #8) |
| Line-start token lookup | `FindToken(line.Start)`, advanced to the next token when it starts before the line | Was copied in the member and statement paths. Now one owner, `GetLineStartToken`. The statement path's behavior is byte-identical | Agree | holds |
| `SharesLineWithSiblingMember` | Unchanged from audit 19 | — | — | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a line's indentation gives the column of the list it begins | `BlankLineCodeFixUtilities.StartsLineBehindComments`, used by `TryGetListIndentation` (statements) and by `FormatMemberOnLinesOfItsOwnAsync` (members) | One owner of the start-of-line check. The first-token-equality shape is written twice, once over `StatementSyntax` and once over `MemberDeclarationSyntax`, in the same file and with the same semantics. Acceptable |
| Line-start token | `GetLineStartToken` (one copy). `RawStringLineShifts.cs:214` decides something else (raw-string content lines) | ok |
| Column of code separated from a sibling | `FormatMemberOnLinesOfItsOwnAsync` (the line's indentation) and `TargetAttributePlacementCodeFixProviderBase` (the SHARED-ATTR-008 depth) | Still two owners. They now produce different columns on none of the members I traced. The helper refuses the finding-8 lines, and the attribute base separates there at the depth column, which is the member column. Where both separate, the line begins with a member at that member's column. The only disagreement left is the pre-existing mis-indented-sibling hint below |
| Whether an RH5203 argument starts its line | `SyntaxTokenPositionUtilities.StartsLine` (the guard) and the `startsLine` initializer (`RH5203…:56`) | The second is a redundant restatement that is always `true` behind the guard (finding #9) |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5417 and RH3202 `FixLeaves…ContinuationOfASiblingsInitializer` (a) | A line beginning with a non-member token keeps its members together. The body is laid out from the member's column | Separation, or braces at another column | `VerifyUnderBothLineEndings` (exact, LF + CRLF) | adequate |
| RH5417 and RH3202 `…StatementOfASiblingsBody` (b) | The line-start token is not a member's first token, so no separation | Same | same | adequate. The closing-brace variant (`} void M()`) runs through the same condition and the same result (probed). No separate test is needed |
| RH5417 and RH3202 `…ClosingLineOfASiblingsVerbatimString` (c) | An earlier token reaches into the line, so no separation | Same | same | adequate |
| RH5417 and RH3202 `…TailOfAMultiLineComment` (d), and `FixSeparates…BeginsItsLineBehindAComment` | The two sides of "a comment whose text starts on that line" (FC-003) | (d) separated, or the comment-start case not separated | same | adequate. Both sides are pinned |

### Visible output is decided

Every new expected value is covered:

- **The refused shapes (a)–(d).** Their expected values are `main`'s output. Handover 20 derived it on a `d6a01ee` worktree. Decision 33 covers it (a rare class keeps `main`'s output), and RH5417-003/RH3202-002 state it.
- **The two separated-side tests.** SHARED-FIX-007 (*Shared line*) demands their output. The run leaves that requirement unchanged (`git diff d6a01ee -- sdd/specs/shared/code-fixes.md` is empty).

No rule page changed. Nothing is undecided.

### Failure classes dismissed

- **FC-001, FC-002, FC-003, FC-015, FC-016:** traced above. The predicate's span equals its counterpart's. Each qualifier in the spec (earlier token, comment start, first token, same list) is a condition in the code, and each is tested on both sides.
- **FC-004, FC-005, FC-019, FC-020, FC-021:** the repair only narrows when a gap is broken. A refused line keeps all of its trivia. A directive cannot share a member's line.
- **FC-006, FC-022–FC-026, FC-036:** no token rewrite. Verbatim and raw literals enter only as the line-start condition (c), which is tested and probed.
- **FC-007:** each refused probe converged in 1 iteration. The refused outputs equal `main`'s.
- **FC-008, FC-009, FC-018:** no registration changed.
- **FC-010–FC-014, FC-038, FC-039, FC-041–FC-043:** no formatter phase, precheck, or analyzer changed.
- **FC-017:** see the table above.
- **FC-027:** multi-line siblings and multi-line members were probed (continuation on the member's last line, closing brace of a multi-line sibling).
- **FC-028, FC-029, FC-030:** the formatter call is unchanged.
- **FC-031:** the summaries of `FormatMemberOnLinesOfItsOwnAsync` and `GetLineStartToken` describe the new code.
- **FC-032:** apply-fix built the head before it ran.
- **FC-033, FC-034:** the tests carry the audit's own shapes, LF and CRLF.
- **FC-035:** the expected values come from `main` and SHARED-FIX-007, not from the patched code.
- **FC-037:** no text-only claim.
- **FC-040:** `IsCommentTrivia` accepts ordinary and documentation comment kinds alike, so the result does not depend on the documentation mode.

### Architecture findings

_None new._ The two-owner column (audit 13, audit 19 #8) is structurally unchanged. The refusal of decisions 28 and 33 leaves no traced member on which the two owners give different columns. The one exception is the pre-existing hint below. Finding #9 does not repeat the class of an earlier audit finding.

### Hints

- Carried from audit 19: the helper copies the whitespace of the member's line as written. A sibling at a wrong column (`      int _x; void M() { }`) therefore passes its column on, while the attribute base would use the depth. RH5204 already reports that line on `main`. This predates the run.
- `1f8d728` leaves handover 21 asserting a mechanism (`GetPreviousToken` returning the missing token) that the probe refutes. Only `00-lead.md` decision 34 relies on it. If finding #9 is repaired, the lead may want to re-read decision 34's premise.
