Role: skeptic
Commit: 731f093
Verdict: #1640: proceed; #1651: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Both issues are `agent-filed`; their goals are treated as proposals. Neither reports a defect: both ask for a refactor with no output change, so there is nothing to reproduce. The tests are characterization tests of today's output.

All fixtures below were run with `scripts/format.sh <dir>` on HEAD `731f093`, and run a second time ("Formatted 0 of 7 file(s)" for `fx2`). Analyzer checks used `scripts/apply-fix.sh <RH####> <dir>`, LF and CRLF. Fixtures lived in the scratchpad and have been deleted.

## thoenissen/Reihitsu#1640

### Premise

Real. `ChainLineBreakRewriter.CloseGap` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs:206-220`), `GetPendingToken` (`:94-100`) and `Replace` (`:273-281`) repeat `LineBreakTriviaUtilities.JoinGaps` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs:244-265`) line for line:
- the front token's trailing trivia becomes `CreateJoinedTrailingTrivia(pendingFront, originalBack)`;
- the back token becomes `RemoveLeadingEndOfLineAndWhitespace(pendingBack)`;
- the result is one `ReplaceTokens` over the map.

`CloseGap`'s own filter, `AreSeparatedByEndOfLine && !WouldJoinAcrossUnjoinableTrivia`, is the same filter `GenericConstraintLineBreakRewriter.JoinUnstructuredWraps` (`:147-149`) passes into `JoinGaps`. So this is the same decision (FC-015).

Reachability: every member-access chain in a document reaches `NormalizeChain` and `JoinConditionalElementAccesses`, through the CLI and through every code fix that formats a chain.

Mechanically, the copy cannot be swapped 1:1, for the reason the issue gives. The chain's map also holds the line-break insertions of `EnsureLineBreak` (`:292-313`), and `JoinGaps` takes no edits of another kind. It returns a new node whose tokens are no longer the original ones.

Feasibility without an output change, established from the code:
- **Disjoint sides.** The joins write only `front.TrailingTrivia` and `back.LeadingTrivia` of the gaps they close. `EnsureLineBreak` writes only:
  - `prev.TrailingTrivia`, where `prev = OperatorToken.GetPreviousToken()`, for links with index ≥ 1;
  - `OperatorToken.LeadingTrivia` for those same links, in the comment-tail branch.

  `FluentChainLink.OperatorToken` is `OperatorTokens[0]` (`src/Reihitsu.Core/FluentChainLink.cs:51`). The only join with `prev` in front or `OperatorToken` behind is the gap in front of link 0's operator. So no (token, side) pair is written by both groups, but one token can carry one of each (cells H and I below).
- **Gap order does not matter.** Each token is the front of at most one gap and the back of at most one. Each gap reads only the side it writes and the back token's text. The chain closes gaps out of text order (dot/name gaps, then attached parts, then operator-inner gaps, then link 0), but the result does not depend on the order. `JoinGaps`'s summary demands "in text order", which is stricter than the code needs (FC-031 if the summary is left as it stands).
- **Layout decisions read the tree before the joins.** These are `chain.IsWrapped`, `link.StartsLine`, `link.HasInnerLineBreak`, `isRootLineIntact`, `HasLineBreak`, and `IsBehindMultiLineCommentTail`. Reading any of them from a joined tree changes output wherever a join removes the only line break that made a link wrapped (cells J and K).

### Mechanism and candidates

_N/A_: no defect. The dispatch is `NormalizeChain` (`:320-362`), with `CloseGap` callers at `:336` (dot/name), `:346`/`:347` (attached parts), `:353` (operator-inner) and `:382` (link 0), plus `JoinConditionalElementAccesses` (`:242-265`, joins only, for chains without links).

### Breaking inputs

Classes: FC-015, FC-016, FC-026 (a chain carries many identical `.` tokens, so any re-resolution of a token after a first rewrite by text or position can pick the wrong one), FC-027 (a chain nested in an argument of another chain), FC-019/FC-020/FC-021 (comment or directive in a join gap: the filter must still refuse it), FC-010 and FC-017 (LF and CRLF, second pass).

Cells and today's final output (characterize at least these):

| Cell | What it varies | Input (inside `void M() { … }`) | Output today |
|---|---|---|---|
| G | every join kind at once | `var x = a⏎.B⏎?⏎.C()⏎!.⏎D()?⏎[0]⏎.⏎E()⏎.F();` | `var x = a.B` / `?.C()` / `!.D()?[0]` / `.E()` / `.F();` (one per line, aligned under `.B`) |
| H | one token: join on its leading side, inserted line break on its trailing side | `var x = a.B()⏎.⏎C .D();` | `var x = a.B()` / `.C` / `.D();` |
| I | one token: comment-tail line break on its leading side, operator-inner join on its trailing side | `var x = a.B()⏎/* one⏎two */ ?⏎.C()⏎.D();` | `var x = a.B()` / (blank line, FMT-BLANK-011) / `/* one` / `two */` / `?.C()` / `.D();` |
| J | chain wrapped only inside a link operator | `var x = a.B()?⏎.C().D();` | `var x = a.B()` / `?.C()` / `.D();` |
| K | prefix link wrapped only inside its operator | `var x = a.B?⏎.C.D()⏎.E();` | `var x = a.B` / `?.C` / `.D()` / `.E();` |
| L | chain without links, conditional element accesses | `var x = a⏎?[0]⏎?⏎[1];` | `var x = a?[0]?[1];` |
| B | inserted line break behind a block comment and behind whitespace | `var x = a.B() /* c */ .C()⏎.D();` and `var y = a.B()   .C()⏎.D();` | `a.B() /* c */` / `.C()` / `.D();` and `a.B()` / `.C()` / `.D();` |

Also cover, without results quoted here: a `//` comment and an `#if` directive in a dot/name gap, an operator-inner gap, and the link-0 gap (FC-019/020); a chain inside an argument of an outer chain (FC-027).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ChainLineBreakRewriter` | Must change: its join bookkeeping goes through `JoinGaps` |
| `LineBreakTriviaUtilities.JoinGaps` | May change contract (it takes no edits of another kind today); its output for its three current callers (`GenericConstraintLineBreakRewriter`, `ListElementInteriorJoinRewriter`, `ArgumentListLineStartRewriter`) must stay |
| Formatter output, code-fix results that format chains (RH5112, RH5201, RH5048) | Must stay |
| Analyzers, CLI, `Reihitsu.Core`, rule pages, specs | Not involved |
| Tests | Characterization of the cells above; a unit test of `JoinGaps` (it has none today) |

### Refuse option

No input class is refused: there is no defect. The alternative is no change, which costs nothing and leaves one 3-line copy of the bookkeeping. Every primitive it calls is already shared, so drift is limited to the map handling.

The requested change costs one of two things, both internal: a change to the `JoinGaps` contract so it composes with other edits, or a second rewrite that has to find the original tokens again in the joined tree, which is FC-026 territory. Neither should need a new type or anything near 100 lines; if it does, that is escalation 9.

Declaring a shape unsupported: not an option for this issue (no shape costs a special case here).

### Output questions

_None._

### Internal choices

1. **How the joins are separated from the line-break edits.** Option a: `JoinGaps` gains a way to compose with edits it does not make. Option b: two rewrites, with original tokens found again in the second tree. Consequence of b: re-resolution risk (FC-026) unless the tokens are found again by something other than text or span. Recommendation: whichever keeps every layout decision on the tree before the joins, and does not find tokens again by text or position.
2. **The `JoinGaps` summary "in text order".** Recommendation: state the real precondition (each gap closes two adjacent tokens; order is free) or keep the chain's calls in text order. Either way, the summary has to match.

### Issue goals

- **"Done when: `rg -n "CreateJoinedTrailingTrivia|RemoveLeadingEndOfLineAndWhitespace"` … finds no batch join loop outside `JoinGaps`".** This cannot be checked mechanically: "batch join loop" is a judgment over the hits. Today's hits outside `JoinGaps` include the following, and only the chain keeps a pending-token map:
  - `CollapseTokenToSameLine` and `CollapseSeparatorsToPreviousElementLine` (a sequential loop, moved into `LineBreakTriviaUtilities` by `c6d199c`, after the issue was filed);
  - the binary, ternary, switch-`when`, assignment and initializer rewriters, with single joins;
  - three single joins in `LineBreakListRewriter`.

  Change to: "`ChainLineBreakRewriter` keeps no join bookkeeping of its own; its joins go through `JoinGaps`; formatter output unchanged." No visible output change.
- **"All formatter tests, including the chain tests, pass unchanged".** Keep it.

### Architecture findings

The issue itself is one (FC-015): a shared helper and a private copy. No further finding for this issue.

### Follow-up candidates

_None_ beyond those under #1651.

## thoenissen/Reihitsu#1651

### Premise

Partly real. The issue misreads one site and is stale on another.

The shared helper `LineBreakTriviaUtilities.EndLineInFrontOf` (`LineBreakTriviaUtilities.cs:55-62`) returns `previous.TrailingTrivia` when `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previous, token)`, and otherwise that trivia with an end-of-line appended. It does not remove whitespace first. Callers: `BinaryOperatorLineBreakRewriter.cs:108` and `TernaryLineBreakRewriter.cs:142` and `:180`. The helper has no unit test.

The sites the issue names, as they stand today:

1. **`ChainLineBreakRewriter.EnsureLineBreak`** (`:292-313`). This has two branches.
   - Gap without a line break: `AppendEndOfLine(RemoveTrailingWhitespace(pendingPrev.Trailing))`. This is the helper's decision, except for the whitespace removal.
   - Gap with a line break, where the operator stands behind the closing line of a multi-line comment (`IsBehindMultiLineCommentTail`, `:121-140`): an end-of-line is appended to the operator's leading trivia, behind the comment. This is not a copy. It implements FMT-BREAK-019 ¶2, the exception in SHARED-CHAIN-005, and SHARED-COMMENT-012's formatter bullet. The helper has no such branch, and no other line-start site has one (see *Architecture findings*). `HasLineBreak` (`:109-112`) is a one-line alias of `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`.
2. **`LineBreakListRewriter`: "two sites, one of them with a different fact"**. This is stale. On 2026-11-07 the second site was the private `CollapseSeparatorsToPreviousElementLine`. Commit `c6d199c` (2026-11-15) moved it into `LineBreakTriviaUtilities` (`:163-199`), where nine callers in the list and initializer rewriters share it. One site remains in the rewriter, `EnsureSeparatorsHaveEndOfLine` (`:486-526`): fact `HasTrailingEndOfLine(sep) || HasLeadingEndOfLine(next)`, write `AppendEndOfLine(RemoveTrailingWhitespace(sep.Trailing))`.
   - **The "different fact" is not a different decision for any reachable input.** `HasLeadingEndOfLine(next)` is `IsFirstOnLine(next)` = `AreSeparatedByEndOfLine(next.GetPreviousToken(), next)`. For `next = element.GetFirstToken()`, which skips zero-width tokens, the previous token is `sep`. So the fact is `EndsLine in sep.Trailing || EndsLine in next.Leading`, exactly `AreSeparatedByEndOfLine(sep, next)`.
   - The same holds for `CollapseSeparatorsToPreviousElementLine`'s `HasTrailingEndOfLine(newSep) || HasLeadingEndOfLine(next)`, because `newSep` keeps `sep`'s trailing trivia.
   - In `EnsureSeparatorsHaveEndOfLine`, the same fact also sets `hasExistingLineBreak`, which decides whether the list is split at all. That flag is a decision the helper does not return.
3. **`GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine`** (`:225-238`): fact `AreSeparatedByEndOfLine(sep, next)`, write `AppendEndOfLine(RemoveTrailingWhitespace(sep.Trailing))`. This is the helper's decision, except for the whitespace removal.

The systematic difference between the copies and the helper is whitespace: every copy removes whitespace at the end of the previous token's trailing trivia before the appended line break, and the helper keeps it. FMT-BREAK-029 ¶2 allows both ("it may remove the whitespace together with that rewrite; the final output is the same"), and FMT-CLEAN-001 removes it. The final output is identical, as cells B, M and N show. The line-break phase's own output differs.

### Mechanism and candidates

_N/A_ for the issue (no defect). The candidate sites are every place in the line-break phase that ends a line in front of a token while keeping an existing line break. Derived from `rg "PrependEndOfLine|MoveTokenToNewLine|AppendEndOfLine"` over `src/Reihitsu.Formatter`, they fall into two families:
- **Appended to the previous token's trailing trivia (the helper's family).** The chain (`:312`), `LineBreakListRewriter:518`, `GenericConstraintLineBreakRewriter:237`, `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine:189`, plus the helper's own callers (binary, ternary).
- **Prepended to the moved token's leading trivia** (`PrependEndOfLine`/`MoveTokenToNewLine`, governed by FMT-BREAK-029 ¶1). `BracePlacer:121`/`:144`, `AttributeTargetFormattingRewriter:218`, `GenericConstraintLineBreakRewriter:290` (`where`), `BinaryOperatorLineBreakRewriter:148` (pattern combinator), `SwitchCaseWhenLineBreakRewriter:158`, `TernaryLineBreakRewriter:126`/`:208`, `LineBreakInitializerRewriter:161`/`:187`/`:249`/`:485`, `DeclarationBraceLineBreakRewriter:124`. This family writes into a different trivia list and is a different helper shape. The issue does not name it.

### Breaking inputs

Classes: FC-015, FC-016, FC-019/FC-021 (comment in the gap: a block comment behind the separator, and a multi-line comment whose closing line the token stands on), FC-003 (whitespace present or absent at the end of the trailing trivia), FC-010 and FC-017 (LF and CRLF, second pass).

Axes for a migrated site: (a) whether a line break is already in the gap; (b) whitespace at the end of the previous token's trailing trivia; (c) a block comment behind the previous token on its line; (d) the moved token standing behind the closing line of a multi-line comment that starts on its own line. Cells, final output today:

| Cell | Site | Input | Output today |
|---|---|---|---|
| B | chain, (b) and (c) | see #1640 | line break behind `/* c */`; whitespace gone |
| M | argument list, (b) | `N(1,⏎  2,   3);` | `N(1,` / `2,` / `3);` |
| N | constraint, (b) | `where T : class,⏎    System.IDisposable,   new()` | `where T : class,` / `System.IDisposable,` / `new()` |
| A | chain, (d) | `var x = a.B()⏎    /* one⏎       two */ .C()⏎    .D();` | `var x = a.B()` / (blank line, FMT-BLANK-011) / `/* one` / `two */` / `.C()` / `.D();`: the chain breaks in front of `.C()` |
| I | chain, (d) and an operator-inner join | see #1640 | breaks in front of `?.C()` |

In the line-break phase's own output, axis (b) separates the copies from the helper. Phase-level tests (for example `Integration/LineBreakPhaseIntegrationTests.cs`, `Regression/LineBreaks/*`, `Unit/LineBreaks/JoinSeparatorPhaseTests.cs`) may lock either form. Axis (d) separates the chain from the helper in the final output.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ChainLineBreakRewriter.EnsureLineBreak`, gap without a line break | May change (route through the helper) |
| `ChainLineBreakRewriter.EnsureLineBreak`, comment-tail branch | **Must stay** chain behavior: FMT-BREAK-019 ¶2, SHARED-CHAIN-005, SHARED-COMMENT-012 |
| `LineBreakListRewriter.EnsureSeparatorsHaveEndOfLine` | May change for the write. The `hasExistingLineBreak` flag must stay |
| `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine` | May change |
| `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine` | Optional; already in the shared class |
| `EndLineInFrontOf` | Must stay for binary and ternary in final output; may change in whitespace handling (internal) |
| Prepend family (`PrependEndOfLine`/`MoveTokenToNewLine` sites) | Not involved |
| Formatter output, code-fix results | Must stay |
| Analyzers, CLI, `Reihitsu.Core`, rule pages, specs | Not involved |
| Tests | Unit test of both `EndLineInFrontOf` branches; characterization of A, B, I, M, N |

### Refuse option

No input class is refused: there is no defect. The alternative is no change, which costs nothing. The fact itself already has one owner (`SyntaxTriviaUtilities.AreSeparatedByEndOfLine` in Core, aliased by the chain's `HasLineBreak` and equivalent in the list's expression). What the copies repeat is a two-way choice plus whitespace removal, so drift is limited to the whitespace removal and the form in which the fact is written.

The narrower change: route the "gap without a line break" write of the chain, list and constraint sites through `EndLineInFrontOf`, keep the chain's comment-tail branch and the list's split flag, and leave the prepend family alone.

Declaring a shape unsupported is named under *Architecture findings* for the comment-tail class. That class is outside this issue's request, but per `AGENTS.md` it has to be named.

### Output questions

_None within the issue_, as long as the chain's comment-tail branch stays chain-only.

Moving that branch into `EndLineInFrontOf` would change the binary and ternary output (cells C, D, R, U under *Architecture findings*). SHARED-COMMENT-012 demands that change, but it contradicts this issue's goal of no output change. It belongs to the follow-up, not to this run.

### Internal choices

1. **Whitespace in front of the line break written at a migrated site.**
   - Option a: the helper stays as it is, so the chain, list and constraint sites keep the whitespace in the phase's output. Consequence: those sites' phase-level output changes, and any phase-level test that locks the stripped text changes too. Final output is unchanged (FMT-CLEAN-001).
   - Option b: the helper also removes the whitespace. Consequence: the binary and ternary phase-level output changes; final output is unchanged.

   Recommendation: **a**, because it matches the *Why* of FMT-BREAK-029 ("Whitespace at the end of a line has one owner, the cleanup phase … a second removal in this phase … makes the phase's output depend on where a node starts"). What would change the pick: if phase-level tests lock the stripped text at the migrated sites and none lock the whitespace at binary or ternary, b is cheaper in test churn.
2. **The list's split flag.** The list keeps computing the fact for `hasExistingLineBreak` either way, so routing its write through the helper evaluates the fact twice. Recommendation: accept that; it is a pure predicate.

### Issue goals

- **"No rewriter in the line-break phase carries a private copy of the check".** Change it to: the chain, list and constraint sites write the line break through `EndLineInFrontOf` where the helper's decision is theirs.
  - The chain's comment-tail branch is not a copy (FMT-BREAK-019 ¶2) and stays.
  - The list's split flag stays.
  - Read literally, the goal would sweep in the prepend family, a different write position (FMT-BREAK-029 ¶1) the issue does not name. Drop that reading.

  No visible output change.
- **The file list.** It is stale: the second `LineBreakListRewriter` site is now `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine`. No visible change.
- **"An architecture or unit test covers the shared helper's branches".** Keep the unit test of both branches (none exists). Drop "architecture test": a source-pattern ban on `AppendEndOfLine(RemoveTrailingWhitespace(` would be a grep-like test that a rename defeats, and nothing is needed beyond the unit test. No visible change.

### Architecture findings

The shared helper and the private copy disagree on a reachable class, and the copy is the one the spec backs.

- **Components:**
  - `LineBreakTriviaUtilities.EndLineInFrontOf` and its binary and ternary callers;
  - the list, collection and constraint separator sites;
  - the prepend family's `HasLeadingEndOfLine`/`IsFirstOnLine` check;

  versus `ChainLineBreakRewriter.EnsureLineBreak`'s comment-tail branch.
- **The shared decision:** whether a token that a requirement asks to start its line already does, when the gap's last line break stands in front of a multi-line comment and the token stands behind that comment's closing `*/`. SHARED-COMMENT-012 (in the line-break spec's `Uses`) says the token does not start its line, and "where a requirement of the formatter asks that token to start its own line, the formatter MUST break the line in front of the token". Only statements, sections and labels are excepted.
- **Why it reaches this run:** #1651 asks to move the chain onto the helper. Dropping the branch as a "copy" would regress cell A.

Every member except the chain fails today: the token stays behind `*/`. Each fixture's comment starts on its own line, directly above the token, and the token stands behind `*/` on the comment's last line.

| Cell | Requirement asking the token to start its line | Input (shape) | Output today | Analyzer on that output |
|---|---|---|---|---|
| A | FMT-BREAK-019 (chain link) | `a.B()⏎/* one⏎two */ .C()⏎.D()` | breaks: `.C()` on its own line | not run |
| C | FMT-BREAK-017 (operator ending its line moves) | `return a⏎/* one⏎two */ &&⏎b;` | `return a` / `/* one` / `two */ && b;` | RH5302 reported; its fix is applied and changes nothing (`no-progress`, LF and CRLF): FC-009 |
| U | FMT-BREAK-017 (operator already leading) | `return a⏎/* one⏎two */ && b⏎&& c;` | `&&` stays behind `two */` | not run |
| D | FMT-BREAK-018 (`?`/`:` ending their lines) | `return c⏎/* one⏎two */ ?⏎1⏎/* three⏎four */ :⏎2;` | `two */ ? 1` / `four */ : 2;` | RH5308 reported; fix `no-progress` (LF and CRLF): FC-009 |
| R | FMT-BREAK-018 (`?`/`:` already leading) | `return c⏎/* one⏎two */ ? 1⏎/* three⏎four */ : 2;` | unchanged: `?` and `:` behind `*/` | not run |
| E | FMT-BREAK-012 (argument list) | `N(1,⏎/* one⏎two */ 2, 3);` | `N(1,` / `/* one` / `two */ 2,` / `3);` | RH5203 reported, no fix offered (`no-fix-offered`) |
| P | FMT-BREAK-012 (parameter list) | `void M(int a,⏎/* one⏎two */ int b, int c)` | `int b` behind `two */`; `int c` on its own line | not run |
| Q | FMT-BREAK-012 (attribute argument list) | `[A(1,⏎/* one⏎two */ 2, 3)]` | `2` behind `two */`; `3` on its own line | not run |
| S | FMT-BREAK-022 (collection expression) | `[⏎1,⏎/* one⏎two */ 2, 3⏎]` | `2` behind `two */`; `3` on its own line | not run |
| T | FMT-BREAK-023 (`where` clause) | `where T : class⏎/* one⏎two */ where U : class` | unchanged: second `where` behind `two */` | not run |
| F | FMT-BREAK-037 (constraint) | `where T : class,⏎/* one⏎two */ System.IDisposable, new()` | `System.IDisposable` behind `two */`; `new()` on its own line | not run. Two readings (see the follow-up) |

All members are reachable through the CLI (whole-document formatting). C, D and E show that the matching analyzers report the formatter's own output.

A cheap refusal (leave the construct as written) is what the code does today, but here an unchanged requirement demands the break, so leaving the construct as written is a defect against SHARED-COMMENT-012, not an allowed refusal. The class is rare: code written behind the closing `*/` of a multi-line comment that starts on its own line.

**Declaring the shape unsupported is an option** (escalation 10; I do not recommend doing it in this run):
- **Span:** from a delimited comment (`/* */` or `/** */`) whose text holds a line terminator, through the first token behind its closing `*/` on the comment's last line, with nothing but whitespace or comments between them. The question is the leading trivia behind the last line-ending trivia, which is what `IsBehindMultiLineCommentTail` checks. It would except none.
- **Special cases it would delete:** the chain's comment-tail branch (`IsBehindMultiLineCommentTail` and its use, about 25 lines). The defects above would also no longer need a fix. Statements, sections and labels have no special code for it (SHARED-BLANK-008 facts), so nothing else is deleted.
- **What it would refuse:** any file with code on the last line of a multi-line block comment, such as `/* header⏎ */ namespace N;` or `/* a⏎ b */ int x;`. That is seldom written, but a file-level refusal is visible.
- If declared at all, I would declare it in a change of its own.

### Follow-up candidates

1. **Bug: tokens behind a multi-line comment's closing line are not broken onto their own line** (cells C, U, D, R, E, P, Q, S, T). These violate SHARED-COMMENT-012 together with FMT-BREAK-012, 017, 018, 022 and 023. RH5302 and RH5308 offer fixes that do nothing on the formatter's output (FC-009), and RH5203 reports it. One shared owner, the helper gaining the chain's branch, would cover the trailing-append family. The prepend family (R, T, U) needs the same fact in its own check. This is a visible output change, but one an unchanged requirement demands.
2. **Output question for the follow-up: FMT-BREAK-037 constraint behind a multi-line comment's closing line (cell F).** FMT-BREAK-037 defines "start a line" as "a line break MUST separate it from the token in front of it", which the end-of-line behind the comma satisfies. SHARED-COMMENT-012 says the token does not start its line.

   **FMT-BREAK-037 · Constraint behind a multi-line comment's closing line**

   Input:
   ```csharp
   class C
   {
       void M<T>()
           where T : class,
               /* one
                  two */ System.IDisposable, new()
       {
       }
   }
   ```

   Before (`main`):
   ```csharp
   class C
   {
       void M<T>()
           where T : class,
                     /* one
                  two */ System.IDisposable,
                     new()
       {
       }
   }
   ```

   After, A (recommended): `System.IDisposable` gets a line of its own behind the comment.
   ```csharp
   class C
   {
       void M<T>()
           where T : class,
                     /* one
                  two */
                     System.IDisposable,
                     new()
       {
       }
   }
   ```

   After, B: unchanged from *Before*.

   Why A: it is the layout SHARED-COMMENT-012 gives every other token a requirement asks to start its line, and the layout the chain already has.
3. **Escalation 10 candidate:** declaring the comment-tail shape unsupported (span and costs under *Architecture findings*).
