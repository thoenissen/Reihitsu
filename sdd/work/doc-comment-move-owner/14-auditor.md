Role: auditor
Commit: a424721
Verdict: pass

My prompt named the issue, slug, lane, SHAs, the repair range, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta of repair round 1 against `ee49ee6` (the head of 09-auditor.md), for thoenissen/Reihitsu#1491 (behavior change).

- Head: `a4247214023753b471346d36bbffcedd026ef5e3`. The checkout is clean and at this commit.
- `origin/main` and `git ls-remote origin refs/heads/main` both give `8c7d5bec9fb134a44d6cfc07a46a072055a70100`, the merge base of the head.
- `git diff ee49ee6..a424721` matches 10-test-implementer.md and 11-implementer.md: `DocumentationCommentFormattingPhase` (delimited relocation and finding #2's text), `UsingDirectiveOrderingRewriter` (one guarded branch), `UsingLeadingTriviaBuilder.HasLineBreakBeforeSignificantTrivia`, the new `DelimitedDocumentationInExpressionTests` (24), 9 RH3202 code-fix tests, and the handovers. No spec, change doc, or rule page changed.
- `scripts/lane-check.sh behavior-change`: `confirmed`.
- The scope grew into the using-directive phase (decision #14). It is the counterpart that reads the trivia the repair writes. It is not an unrelated surface, so this is a delta audit, not a full one.

Method:

- Static trace of the new delimited branch against every phase that reads the trivia between the documentation phase and the blank-line phase. Only the using-directive phase runs there.
- Mutation runs in a scratch worktree of the head.
- Independent differential runs. I built `Reihitsu.Cli` from `origin/main` and from the head, formatted the same fixtures with both, under LF and CRLF, twice each, and compared main against head and each pass against the next. Total: 434 fixtures in four corpora (below), outside the implementer's corpus.
- `scripts/apply-fix.sh` for RH3202 and RH5308, from both versions.

Corpora:

1. 26 expressions × 6 contexts. The contexts are `return`, expression body, field initializer, local, argument, and top-level. The expressions:
   - `?`/`:` in an argument, a lambda, an interpolation hole, and a switch arm;
   - both operators commented at once;
   - nested in both branches;
   - a multi-line `/** a … d */`, `/** d */ /* c */`, `/** d */ /** e */`, and no-space gaps;
   - four partly wrapped shapes;
   - a parenthesized branch, and `-/** d */ - /** e */ y` (one token both owner and previous token);
   - `is`, `await`, `new`, query `in`, and `?.`/`??`.
2. 19 using-directive shapes, each where the moved comment stands behind a directive: reorder; regroup with a new group; chains of moved comments, in order and reversed; a non-neighbor predecessor; multi-line comments; a block comment in front; alias, static, global, and `extern alias`; namespace-scoped and file-scoped usings; and an assembly attribute or namespace behind the comment.
3. 7 blocks that cannot be reordered safely (active `#if true`, `#pragma`, and `#region` in the block), so they take `SeparateDirectivesSharingALine`. Plus 9 more using shapes with a multi-line `/* x … y */` in front of the comment, a reordered predecessor, no-space gaps, and `// t` behind.
4. 18 statement shapes and 8 member shapes behind `;` or `}`: `return`, `if`, declaration, `foreach`, `using`, local function, `break`, `else`, `catch`, label, block, `yield break`, `lock`, own-line and `//` behind, fields, methods, properties, enum members, attributes, and nested types.

Results:

- **Main against head.** 0 differing files in all four corpora. Every fixture was reformatted.
- **Idempotency.** Every fixture is a fixed point after one pass on both versions. The exceptions are 4 fixtures with `/** d */ #region R` on one line. That input is not valid C# (CS1040: a directive must start its line), so no caller produces it, and main and head behave the same on it.
- **Code fixes.** `apply-fix.sh` over 78 fixtures × LF/CRLF gives byte-identical reports on main and head for RH3202 (52 fixed, no non-convergence) and RH5308 (24 fixed, 6 `no-progress`).
- **The RH5308 `no-progress` cases.** All six are `b ? 1 :` / `/** d */ 2` with the comment already on its own line in the input. That case is not moved and behaves the same on main.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member 09-auditor.md listed is now identical to `main`: (a) to (e), with the comment directly behind the operator or behind `/* c */`, in an expression body and in a `return` statement. They are pinned by the 16 failing-member formatter tests (LF and CRLF, two passes, phase stability) and the 8 RH3202 tests. My corpus 1 adds the enclosing scopes FC-027 names (argument, lambda, interpolation, switch arm, field and local initializer, top-level), both operators at once, and nested branches. All are identical to `main` and idempotent. RH5308 reports are identical to `main` on these shapes. No member of the class fails (see *Class enumeration*). |
| 2 | closed | The class summary of `DocumentationCommentFormattingPhase` (`:16-18`) now states FMT-DOC-001's conditions: the comment documents the code following it; a single-line comment in every position; a delimited comment only as the first comment, not behind `{` or a switch-label colon. `MoveBeforeOwningToken`'s `<returns>` (`:77`) says "onto a new line". |

### Class enumeration (finding #1)

The mechanism is this: the documentation phase decides where a moved comment's line break goes, and a later component reads the trivia shape that results. The axes:

- the token in front of the comment;
- which later component reads the shape;
- the gap shape;
- the line ending.

The repair writes the delimited line break into the owning token's leading trivia, as `main`'s blank-line phase did. The token in front only loses the whitespace at the end of its trailing trivia, which the pre-repair code already removed.

The readers are:

1. **The using-directive phase.** It is the only phase between the documentation phase and the blank-line phase. It newly sees an `EndOfLineTrivia` in front of the comment. Its reorder path rebuilt that leading trivia without the line break, and the repair's using-phase branch keeps it. Its other two paths already keep it:
   - `SeparateDirectivesSharingALine` leaves leading trivia untouched, and `RequiresSeparatingLineBreak` returns false because `SpansLine(successorLeadingTrivia)` holds.
   - For a reordered non-neighbor, `LeadingDocumentationCommentSpansLine` stops at the `EndOfLineTrivia` and returns false. The predecessor then gets its line break, `beginsLine` is true, and nothing is inserted.

   Corpora 2 and 3 cover every path, and all are identical to `main`.
2. **The blank-line phase.** `StartsOwnLine` and `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` read the leading trivia up to the comment. The comment counts as starting its line, so FMT-BLANK-011's blank line is inserted. `EnsureBlankLineBeforeFirstComment` replaces the gap, so the owning token's leading trivia ends up as `main`'s (two line breaks, then the comment).
3. **Every phase after the blank-line phase**, `TernaryLineBreakRewriter` included. They see `main`'s shape, except that the token in front has no trailing whitespace; the cleanup phase removes that whitespace on `main`. Corpora 1 and 4 and the implementer's 2890 fixtures show no difference.

The `///` form keeps the previous token's trailing trivia (unchanged; follow-up candidate 3).

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Delimited branch of `RegisterRelocation` (`:203-214`): the line break goes into the owning token's leading trivia; a zero count strips the previous token's trailing whitespace | Reached only after `IsMovableDelimitedDocumentation` (the first comment of the leading trivia, not first in block) and `DocumentsFollowingCode`. `isPreviousTokenInRoot` = the previous present token exists and is inside the root | `main`'s `EnsureBlankLineBeforeFirstComment`, which wrote the break into the same leading trivia. The `///` branch keeps `HasOwningLeadingSourceOnSameLine` and the trailing placement | The move set is unchanged: the branch sits behind the same guards as before the repair. The branch no longer reads `HasOwningLeadingSourceOnSameLine`, which cannot change the result for a delimited comment. That comment is the first comment, and Roslyn ends trailing trivia in front of a documentation comment, so the owning token's leading trivia starts with the comment, except at a detached root's first line, which already took the owner path. A previous token outside the root gets no count, as before. A token that is both an owner and a previous token (`-/** d */ - /** e */ y`) goes through the existing `replacements` lookup, and its output is identical to `main`. | holds |
| Using reorder path keeps a line break (`UsingDirectiveOrderingRewriter:149-153`) | `beginsLine == false` (same group, and the rebuilt predecessor trailing trivia has no `EndOfLineTrivia`), and `HasLineBreakBeforeSignificantTrivia` (an `EndOfLineTrivia` in the current directive's leading trivia before its first non-layout trivia) | `CreateLeadingTrivia` drops the layout before the first significant trivia. `RequiresSeparatingLineBreak` decides the predecessor's break | They agree. When the predecessor gets a break, `beginsLine` is true and no second break is inserted. Parsed input cannot reach the branch: Roslyn puts the first line break after a token into its trailing trivia, unless a documentation comment stops the trailing trivia first, and then the comment is the first significant trivia, with no break in front of it. Mutations in a scratch worktree, filter `Using`: forcing the condition false fails 2 tests (`SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass`, `SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt`). Dropping `beginsLine == false` fails 8 tests. Both sides of the boundary are pinned (FC-003). | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Moving a documentation comment written behind code | `DocumentationCommentFormattingPhase.RegisterRelocation`, unchanged as sole owner. The using branch keeps an existing line break and decides no move. | one owner |
| Line break before the first significant leading trivia | `rg "bool (Has\|Contains)\w*LineBreak\w*\("` over Formatter and Core: `UsingLeadingTriviaBuilder.HasLineBreakBeforeSignificantTrivia` (new; uses Core's `SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex`), `UsingTrailingTriviaBuilder.ContainsLineBreak` (the whole list), `TokenGapUtilities.HasLineBreakBetween` (token gap), `ChainLineBreakRewriter.HasLineBreak` (token gap), and `UsingDirectiveOrderingUtilities.StartsItsLine` (source text). None of these checks the leading trivia before its first significant trivia. | one owner |
| Where a moved comment's line break goes | One method, two placements by comment form: delimited in the owning token's leading trivia, `///` in the previous token's trailing trivia. Each keeps `main`'s shape for its form, and the code comment at `:199-202` states why. | justified (output parity with `main`). The `///` placement is the cause of the pre-existing `///` case in follow-up candidate 3 (*Hints*). |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DelimitedDocumentationInExpressionTests`, 16 failing-member tests | Full-pipeline output for a delimited comment behind `?`/`:` equals `main`'s, and `?` and `:` start their lines | The operator ends its line (the pre-repair output), a second-pass change, or a phase reversal | `AssertRuleResult` (LF and CRLF, two passes, phase stability) | Adequate. They failed on `0c1b1a7` with exactly finding #1's difference (10-test-implementer.md). The expected values are `main`'s, observed from a `main` worktree, not from a patched copy (FC-035). |
| `DelimitedDocumentationInExpressionTests`, 2 holding members with `:` at its line start, and 6 operator characterizations | Output unchanged for the holding side of the boundary | Any difference | `AssertRuleResult` | Adequate (FC-003, holding side) |
| RH3202, 9 code-fix tests | The fix's output equals `main`'s for the expression-body members | The operator ends its line, or no convergence | `Verify`, LF and CRLF | adequate |
| Using-phase branch | Covered by existing tests | See *Guard scope* (mutations) | `AssertRuleResult` | Adequate. Both sides of the condition are pinned. |

Visible output is decided (check 10):

- Every added expected value is `main`'s output, and the repair restores it. No rule page changed, and the change doc still lists no visible output change.
- The FMT-BREAK-018 tag on outputs that do not fully meet "followed by its branch" was decided under decision #15, and its output question is carried by follow-up candidate 3 (*Hints*).

## Failure classes dismissed

- FC-001, FC-002, FC-003, FC-015: see *Guard scope* and *Policy ownership*.
- FC-004, FC-005, FC-019, FC-020, FC-021: no comment, directive, or disabled text is lost or reordered in 434 fixtures. Every fixture is identical to `main`. Gaps with `/* c */`, multi-line `/* x … y */`, `// t`, own-line comments, and active `#if`, `#pragma`, and `#region` blocks are in the corpora.
- FC-006, FC-022 to FC-026, FC-029, FC-036: the repair moves trivia only. Tokens and literals are unchanged.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changed. RH3202 and RH5308 reports are byte-identical to `main` (Fix All is unchanged; 10-test-implementer.md).
- FC-010, FC-011, FC-012: every valid fixture is a fixed point after one pass on the head, under LF and CRLF. The blank-line phase and the using phase consume the new shape without undoing it.
- FC-013: RH5308 reports are identical to `main` (see *Scope*).
- FC-014: no new rule.
- FC-016, FC-027: see *Class enumeration*. Enclosing scopes, nesting, and multi-line comment forms all hold.
- FC-017: see *Assertion adequacy*.
- FC-028: the RH3202 tests run the code fix's node-level path. A delimited comment on a detached root's first line takes the unchanged owner path.
- FC-031: finding #2 is closed. The new comments (`:161-164`, `:199-202`, using rewriter `:145-148`, the `HasLineBreakBeforeSignificantTrivia` summary) describe the new code. No `TODO`, no commented-out code, no tracker numbers.
- FC-032: the Release build was green after the repair (12-cleaner.md), and the validator follows.
- FC-033, FC-034, FC-035: the tests use the reported shapes under LF and CRLF on both surfaces, with expected values from `main`.
- FC-037: no text-only claim.
- Class matrix "Tab width across formatter phases": not reached. No column is computed by the repair.

## Architecture findings

_None._ The repair gives the delimited move `main`'s trivia shape at every phase boundary from the blank-line phase on. The using phase only keeps a line break it finds; it does not predict or copy the documentation phase's decision. One component still owns each decision.

## Hints

- Not a finding: the tests tagged `[Requirement("FMT-BREAK-018")]` pin `main`'s output `:` / blank / `/** d */ 2;`. That output meets "start of their own lines" but arguably not "each followed by its branch". Decision #15 keeps the tag, and follow-up candidate 3 carries the output question. If the follow-up changes the layout, these tests change with it.
- Pre-existing, identical on `main`: for `b ? 1 :` / `/** d */ 2` with the comment already on its own line, RH5308 reports the formatter's output and its fix makes no progress (`apply-fix.sh RH5308`). This belongs with follow-up candidate 3.
- The `///` placement in the previous token's trailing trivia is what still leaves `?`/`:` ending a line for `///` (09-auditor.md *Hints*). Giving `///` the delimited placement would likely fix that, but it changes visible `///` output, so it stays with follow-up candidate 3.
- My scratch worktrees (`origin/main` and a head copy for the mutations) have been removed. The fixture directory `/tmp/claude-0/-home-user-Reihitsu/1249348a-0267-5977-878d-b142ddea0555/scratchpad/aud14` holds the corpora and results.
