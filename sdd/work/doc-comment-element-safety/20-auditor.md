Role: auditor
Commit: 9af3a140c105ed98079a0bac2e49b852c2d94ba2
Verdict: pass — #1329: pass; #1389: pass; #1241: pass; #1344: pass; #1242: pass

Notation: `⏎` is a line break and `U+FEFF` the byte order mark character. "Base" is `f3d514d`. The previous audit is `13-auditor.md`, made on `9161d75`.

The coordinator's message named the repair commits, the head, and the `origin/main` SHA, and said the build is green. It carried no intended fix, suspected cause or hoped-for verdict.

## thoenissen/Reihitsu#1329

### Scope

Repair delta against `9161d75`. Files changed for this issue: the FMT-DOC-013 text in `sdd/specs/formatter/documentation-comments.md`, the `## Exceptions` bullets on the RH8301, RH8304, RH8305 and RH8307 rule pages, and two new tests in `DocumentationCommentWithoutOwnEndTagTests`. No production code changed for this issue.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| F4 | closed | FMT-DOC-013's third normative sentence and its coverage sentence now name FMT-DOC-002 next to FMT-DOC-001, and an example was added. The new example matches the output I observed at `9161d75` for the same input. Two new tests tag FMT-DOC-013 and FMT-DOC-002: `CommentWithoutEndTagSeparatedFromEarlierBlockCommentIsLeftAsWritten` and `CommentWithoutEndTagBehindBannerAtStartOfFileIsLeftAsWritten`. The guard in `DocumentationCommentFormattingPhase.Execute` is byte-identical to the audited one, and it runs after relocation, so the spec now describes what the code already does. |
| F5 | closed | See #1241. |

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|

_N/A — no new finding._

### Guard scope

_N/A — no predicate changed for this issue. The `IsLeftAsWritten` row of `13-auditor.md` still holds: the code is byte-identical._

### Policy ownership

_N/A — unchanged since `13-auditor.md`._

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CommentWithoutEndTagSeparatedFromEarlierBlockCommentIsLeftAsWritten` and `CommentWithoutEndTagBehindBannerAtStartOfFileIsLeftAsWritten` | The comment is moved below `/* c */` or `/* banner */`, and its text and the declaration after it are unchanged | Any byte difference, under LF or under CRLF, on either pass | `AssertRuleResult`: exact text and a second pass | adequate. They cover FC-019 and FC-021: a comment in the gap the edit touches. |

### Failure classes dismissed

See *Whole run*.

### Hints

_None._

## thoenissen/Reihitsu#1389

### Scope

Repair delta against `9161d75`:
- **Code:** `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` and its first-token call in `VisitToken`, and the new `ReihitsuFormatterHelpers.CreateByteOrderMarkTrivia(IEnumerable<SyntaxTrivia>)`.
- **Spec:** a clarification of FMT-CLEAN-009 in `sdd/specs/formatter/cleanup.md`.
- **Tests:** ten new tests in `ByteOrderMarkTests` and `CleanupPhaseTests`, and the literal escapes for F3.

The spec change stays inside FMT-CLEAN-009, the requirement F1 and F2 name, so I audited that requirement fully against the code and the tests.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| F1 | closed | When no line break is kept, the BOM and the indentation after the removed blank lines now become one white-space trivia, the shape the parser produces. When a line break is kept, the order is BOM, line break, indentation, so no two white-space trivia are adjacent. I ran a probe app (scratchpad, now deleted) that calls `ReihitsuFormatter.FormatSyntaxTree` three times, LF and CRLF. Every input was stable after pass 1 and had the BOM once at the start, followed by the column-0 layout: <br>- the four F1 (a) inputs; <br>- the BOM followed by spaces on the first line; <br>- a tab-indented next line; <br>- an indented `#region`, `#pragma`, `[System.Obsolete]`, `/* c */` or `namespace`; <br>- a doubled BOM; <br>- a BOM followed by a line of white space only at the end of the text. <br>The F1 (b) input (a mid-file `U+FEFF` line before `else`) is now stable too: the `U+FEFF` is removed, as on base. Four new tests pin F1 (a) and two pin F1 (b). |
| F2 | closed | `startsText = token.FullSpan.Start == 0` is read before the base visit, and only the branch for a token without a preceding token passes it on. Every other `CollapseLeadingBlankLines` call keeps the default `false` and behaves as on base. The guard now agrees with Cleanup's `startsText: original.FullSpan.Start == 0`. Probed: a `U+FEFF` blank line mid-file before `else` and between members, and a `U+FEFF` at the end of the `class C {` line: all are removed and stable. Five new formatter tests and one Cleanup unit test pin this side of the boundary. FMT-CLEAN-009 now states it: only the `U+FEFF` at the start of the text is a byte order mark, and elsewhere a blank line holding it is white space, removed by FMT-CLEAN-001, FMT-BLANK-007 and FMT-BLANK-008. Both references say that. The case the Indentation phase keeps (a `U+FEFF` in front of an indented code line) is excluded by a coverage sentence, and the maintainer chose answer A for it (`00-lead.md`, last section). The method summary now states the start-of-text scope (FC-031). |
| F3 | closed | `DocumentationCommentUtilities.ByteOrderMark` is now `'﻿'`. Every test literal F3 listed now uses `"﻿"`. I scanned all 1,700 tracked `src/**/*.cs` files, ignoring a leading encoding signature, and found no `U+FEFF` character. |

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|

_N/A — no new finding._

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Keep the BOM in BlankLines | `trivia[0]` of the token, only when the token has no preceding token and `token.FullSpan.Start == 0` before the base visit | Cleanup `CleanWhitespaceBeforeEndOfLine` (`original.FullSpan.Start == 0`, trivia index 0), FMT-CLEAN-009 ("only a `U+FEFF` at the start of the text is a byte order mark"), and answer 4A | Yes. Both owners use the same position test. | agree |
| Shape of the trivia that keeps the BOM | One white-space trivia: the BOM joined with the indentation trivia that followed the last removed line break. When a line break is kept: BOM, line break, indentation. | `IndentationRewriter.TryHandleLineStartWhitespace`, which treats the first white space at a line start as that line's indentation | Yes. Pass 1 and pass 2 see the same trivia (probed). | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| BOM at the start of the text | `rg "StartsWithByteOrderMark\|CreateByteOrderMarkTrivia" src` finds the helpers in `ReihitsuFormatterHelpers` and two callers: `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` and `CleanupPhase.CleanWhitespaceBeforeEndOfLine`. Both now restrict the BOM to text position 0. The pre-existing owners are unchanged: `IndentationRewriter`, `HasOwningLeadingSourceOnSameLine`, `RH7304…CodeFixProvider`, `RH5204…Analyzer`, and Core's private constant. | The two owners this PR added agree. The Indentation difference is decided (maintainer answer A) and excluded by FMT-CLEAN-009's coverage sentence. |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The four `ByteOrderMarkBefore…IndentedFirstToken`, `…IndentedComment`, `…IndentedDocumentationComment` and `…IndentedUsingDirective…KeptInOnePass` tests | The BOM is kept once, the result is laid out from column 0, and it is stable | Leftover indentation on pass 1, a BOM that is missing or repeated, or a pass-2 change | `AssertRuleResult`: exact text, LF and CRLF, second pass | adequate. Each expected value is the BOM-free formatter output with the BOM put in front, so the oracle does not come from the changed branch (FC-035). |
| The five `ByteOrderMarkCharacterOnBlankLineBefore{Else,Finally,OpeningBrace,ClosingBrace,Statement}IsRemoved` tests | Outside the start of the text, the `U+FEFF` goes with its blank line, or the blank line stays empty | `U+FEFF` kept, extra indentation, or a pass-2 change | `AssertRuleResult` | adequate (FC-003: the other side of `startsText`) |
| `CleanupPhaseTests.RemovesByteOrderMarkCharacterOnBlankLineInTheMiddleOfTheText` | Cleanup alone removes white space that starts with `U+FEFF` on a mid-file blank line | The `U+FEFF` or the spaces are kept | Direct `CleanupPhase.Execute`, exact string | adequate |
| The escaped literals: `ByteOrderMarkTests`, `CleanupPhaseTests`, `DocumentationCommentUtilitiesTests`, `RH8304…AnalyzerTests` | Unchanged | — | — | adequate. They can no longer degrade silently (FC-033). |

### Failure classes dismissed

See *Whole run*.

### Hints

- **Detached roots keep a mid-file `U+FEFF`.** The proxy `FullSpan.Start == 0` is true for every detached root, so `ReihitsuFormatter.FormatNode` keeps a `U+FEFF` that opens the leading blank line of a node taken from mid-file. The probe showed it:
  - `FormatNode` on the detached member parsed from `U+FEFF⏎⏎    int B;` gives `U+FEFFint B;`.
  - `FormatNode` on the attached member `B` of `class C⏎{⏎    int A;⏎U+FEFF⏎⏎    int B;⏎}` gives leading trivia `U+FEFF⏎    `.
  - Base drops the `U+FEFF` in both. Formatting the whole file drops it too.
- **No shipped code fix shows this in its output:**
  - RH5401's `CanCollapse` reads only `ToString()`, which leaves out leading trivia.
  - `FormatNodeInDocumentWithContextAsync` writes back the target's original leading trivia.
  - `FormatNodeInDocumentAsync` passes `rootPrecedingToken`, so a mid-file target never takes this branch.
- Cleanup has used the same proxy since round 1. Only an outside caller of the public `FormatNode` could see the difference, so this is an observation, not a finding.

## thoenissen/Reihitsu#1241

### Scope

Repair delta against `9161d75`. The gap notes on `documentation/rules/RH8107.md`, `RH8202.md`, `RH8204.md` and `RH8309.md` changed. Text only.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| F5 | closed | All eight rule pages now use one wording: "never closed, or closed by an end tag with a different name, including one that differs only in case". The examples match the tests that pin them: `<Returns>Nothing</returns>`, `<Value>The size</value>`, `<Placeholder>Later text</placeholder>`, and `<summary>S</returns>` and `<Summary>S</summary>` on the layout-rule pages. The pages match SHARED-DOC-017 and FMT-DOC-013, and none mentions the formatter. The spec checker reports `RuleDocumentationConventionTests` green (`17-spec-checker.md`). |

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|

_N/A — no new finding._

### Guard scope

_N/A — no code changed for this issue._

### Policy ownership

_N/A — unchanged since `13-auditor.md`._

### Assertion adequacy

_N/A — no test changed for this issue._

### Failure classes dismissed

All classes: the repair for this issue is documentation only.

### Hints

_None._

## thoenissen/Reihitsu#1344

### Scope

Repair delta against `9161d75`. The Core constant and the two literals in the RH8304 and Core tests now use the `﻿` escape (F3). The RH8304 and RH8309 rule pages changed (F5).

### Previous findings

| # | Status | Evidence |
|---|---|---|
| F3 | closed | See #1389. The compiled constant is the same character, so `GetContinuationPrefix` behaves exactly as audited. |
| F5 | closed | See #1241. |

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|

_N/A — no new finding._

### Guard scope

_N/A — no predicate changed; the constant compiles to the same `char`._

### Policy ownership

_N/A — unchanged since `13-auditor.md`._

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH8304….VerifyFixDoesNotRepeatTheByteOrderMark` and `DocumentationCommentUtilitiesTests.GetContinuationPrefixExcludesByteOrderMarkInFrontOfTheExterior` | Unchanged | Unchanged | Unchanged | adequate. The BOM can no longer be stripped invisibly. |

### Failure classes dismissed

All classes: the repair for this issue changes no behavior.

### Hints

_None._

## thoenissen/Reihitsu#1242

Nothing changed for this issue in the repair rounds. The verdict of `13-auditor.md` (pass) stands.

## Whole run

### Scope

- **Checkout.** The head is `9af3a140c105ed98079a0bac2e49b852c2d94ba2`, on branch `claude/dreamy-albattani-e7phpl`. The working tree was clean.
- **Base.** `origin/main` is `f3d514d0b1cbd8b3e540f2b2c7243dcd21760929`, the SHA the coordinator named, and it is an ancestor of the head.
- **This is a repair delta against `9161d75`.**
- **The diff is what the handovers describe:**
  - **production:** `BlankLineTokenCleanupRewriter.cs`, `ReihitsuFormatterHelpers.cs`, `DocumentationCommentUtilities.cs` (`15-implementer.md`);
  - **tests:** six files ("Repair round 2" in `05-test-implementer.md`);
  - **specs and pages:** two spec files and eight rule pages (`14-spec-author.md`, `18-spec-author.md`);
  - **work docs.**
- **Ledger.** The requirement ledger is unchanged.
- **Lane.** `scripts/lane-check.sh bug-fix` reported `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`.
- **BlankLines scope.** In the off-limits BlankLines phase, the repair touches only `CollapseLeadingBlankLines` and its first-token call, so it stays inside answer 4A.
- **Scope did not grow.** The spec changes are confined to FMT-DOC-013 and FMT-CLEAN-009, the requirements the findings named. Both are clarifications of behavior the code already had or now restores to base, and I audited both fully.

### How the facts were established

- **Static tracing.** Every changed method, and the Indentation and Cleanup counterparts.
- **A probe app.** A scratchpad file-based app against the head: 16 full-file inputs, three passes each, LF and CRLF, plus three `FormatNode` calls.
- **A character scan.** All tracked `src/**/*.cs` files, searched for `U+FEFF`.
- **No repository test was run.** I trusted the cleaner's build and the roles' focused and full results (`15-implementer.md`, `16-cleaner.md`).
- **Clean-up.** The probe is deleted. This handover is the only file I changed.

### Failure classes dismissed

- **FC-001, FC-002, FC-003.** The BOM guard now carries the "start of the text" qualifier, and both sides are tested.
- **FC-010, FC-011, FC-012.** The trivia shape is the one the parser produces; probed stable to pass 3 under LF and CRLF.
- **FC-013.** The column-0 output no longer gives RH5204 an indentation to report.
- **FC-015.** The two owners the PR added agree on position.
- **FC-016.** The sibling shapes were probed: tab, directive, attribute, block comment, doubled BOM, BOM with spaces.
- **FC-017.** Exact text, two passes, both line endings.
- **FC-028.** One observation only, in the #1389 hints.
- **FC-031.** The summaries and the inline comment in `VisitToken` describe the new code.
- **FC-032.** The last compiled change is `aa9d904`. The implementer ran the full formatter, Core and analyzer suites on it, and the validator runs everything.
- **FC-033.** Closed with F3.
- **FC-035.** The expected values come from BOM-free formatter output.
- **Every other class.** The repair rewrites no syntax, changes no code fix, adds no rule, and renames nothing.

### Hints

- The `FormatNode` observation under #1389 is the only new hint.
- The hints of `13-auditor.md` still apply, except the Indentation hint, which the maintainer answered (A).
