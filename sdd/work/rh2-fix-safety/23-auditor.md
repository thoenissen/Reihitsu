Role: auditor
Commit: 7714c11d207284baa3c281cd63273c9c3cf3ad16
Verdict: findings

The prompt named decision 25 as the structural change made for audit finding 5. That is context from `00-lead.md`. It carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

State check:
- The checkout is `7714c11`.
- `origin/main` is `4e61f29`, an ancestor of HEAD.
- Between `3f9d996` and `7714c11`, the merge of main overlaps the run's files only in `sdd/specs/requirement-ids.txt`. There it adds `RH5202-004`–`006` from main. The run's own ledger delta against main is RH2001-007/-008, RH2005-004/-005, RH7002-004, and SHARED-DIAG-012.
- `scripts/lane-check.sh bug-fix` reports `confirmed`.
- The repair `a79eb22..3f9d996` (outside `sdd/work/`) is what handovers 18–22 describe:
  - `sdd/specs/analyzer/RH2005.md`;
  - `src/Reihitsu.Core/DeclarationModifierUtilities.cs`;
  - the RH2005 fix provider (guard rename and comment);
  - the RH2005 tests (+999 lines);
  - the RH2001 test `VerifyFixKeepsACommentLineInFrontOfTheType`.

I trusted the cleaner's Release build (`21-cleaner.md`) and the implementer's full analyzer, core, and architecture runs (`20-implementer.md`).

Probes ran with `scripts/apply-fix.sh RH2005 <dir>`, which runs each fixture under LF and CRLF. Formatter checks ran with `scripts/format.sh <file>`. Fixtures are under `/tmp/claude-0/-home-user-Reihitsu/11377a95-24cc-5692-825a-5a87f974a5d7/scratchpad/audit23/`, outside the repository. "Observed" means a probe printed the result. "By trace" means it was read from the code.

## Scope

- Head `7714c11`; base `origin/main` `4e61f29`.
- This is a repair-delta audit against `a79eb22` (`17-auditor.md`).
- The repair changed RH2005-003 and RH2005-005, but only to record the maintainer's answers Q7 = A and Q8 = A, and decision 28. The spec change stays within finding 5's decision, which `17-auditor.md` already audited in full. So I audited the changed requirements, the one method that now decides the trivia, its guard, its two callers (RH2005, RH2004), and the new tests. Scope did not grow beyond that.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–4 | closed | Closed in `17-auditor.md`. The repair does not touch their code paths, except #1's helper, which #5 covers. |
| 5 | closed | Every listed member holds, under LF and CRLF:<br>- (a) `static public⏎        int` → `private static⏎        int` (observed, A01; tested).<br>- (f) `static⏎        public⏎        int` → `private static⏎        int` (observed, A02; tested). With `int` in another column, `int` moves to `public`'s column (A03; tested).<br>- (b) `public // note`, (c) a `#pragma` line, (d) an `#if` block, and (e) a comment line behind a later `public`: the fix is withheld (tested). Also observed: A18–A22, including a `#region` pair and an `#if`/`#else` around the type.<br>- The class #5 enumerated holds as well: `public` in the middle, last behind one modifier, last behind two, and behind an attribute list with a doc comment; a blank line in front of or behind `public`; an end-of-line or block comment behind the modifier in front (kept in place, A07–A09); a tab; several declarators; `new`; `global::`. Observed, A01–A17, A23, A24, A27, A28.<br>- One member's output changed from `main` without a decision, although nothing breaks there: a non-first `public` that neither starts nor ends its line. It is reported as #7. It is not a repeat of #5: no comment, directive, or line is lost or moved, and the decision now has one owner. |
| 6 | closed | `VerifyFixKeepsACommentLineInFrontOfTheType` accepts only "withheld" or `Format(inPlace)`, and is tagged SHARED-FIX-007. |

## thoenissen/Reihitsu#967

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | medium | `src/Reihitsu.Core/DeclarationModifierUtilities.cs:264-269` (`ReplaceAccessibilityModifiers`, the branch where a removed non-first modifier does not start its line) | FC-016 (sibling shape) | – (no trivia is lost or moved, and one method owns the decision; see #5's row above) | undecided output | When a removed non-first `public` neither starts nor ends its line, the branch replaces the whitespace at the end of the modifier in front with `public`'s trailing trivia. On `main` (and on `a79eb22`), that modifier kept its gap, and the inserted `private` took `public`'s trailing trivia.<br>So the fix result changes for one-line inputs whose gaps are not single spaces. Observed on HEAD (LF = CRLF), and by trace on `main`:<br>- `static public(int A, int B) Counter;`: `main` gives `private static (int A, int B) Counter;`, HEAD gives `private static(int A, int B) Counter;`.<br>- `static public@int Counter;`: `main` gives `private static @int Counter;`, HEAD gives `private static@int Counter;`.<br>- `static    public int Counter;`: `main` gives `private static    int Counter;`, HEAD gives `private static int Counter;`.<br>- `static public    int Counter;`: `main` gives `private    static int Counter;`, HEAD gives `private static    int Counter;`.<br>- `static public  volatile int Counter;`: `main` gives `private  static volatile int Counter;`, HEAD gives `private static  volatile int Counter;`.<br>Every result compiles and keeps every comment and directive. No requirement decides these gaps:<br>- RH2005-003's normative text covers a non-first modifier that starts or ends its line, and its one-line example has single spaces, which give the same result on `main` and HEAD.<br>- Its *Why* ("changes no more of the layout than the removed keyword occupied") reads both ways.<br>- Q7 answers only for a modifier that ends its line.<br>- SHARED-FIX-007 does not separate the two outputs. The formatter keeps `static(` and `static@int` as written (observed), and it collapses the wide gaps of both `main`'s and HEAD's multi-space results (observed). So that part is pre-existing, and HEAD's third row is formatter-clean where `main`'s was not.<br>No test pins any of these inputs. | The lead decides whether one-line gap widths of an unformatted declaration are in question. If they are, ask them as one output question: A is HEAD's "the gap behind `public` stays", B is `main`'s. Then pin the answer with one test per row (LF and CRLF). If they are not, record the decision and add the tests for HEAD's output, as decision 29 did for the first-position outputs. |
| 8 | low | `src/Reihitsu.Core/DeclarationModifierUtilities.cs:105-108`, `:167-172` (the two `SyntaxTokenList` overloads), and `:277-280` (the `followingToken.RawKind == 0` branch); also the `tokens.Count == 0` operand at `:258` | – (code quality: dead code) | – | PR-introduced | The repair removed the last production caller of `AddAccessibilityModifier(SyntaxTokenList, SyntaxKind)` and `AddAccessibilityModifiers(SyntaxTokenList, IReadOnlyList<SyntaxKind>)`. `rg -n "AddAccessibilityModifiers?\(" src --glob '!*Test*'` finds only the member overloads, called from the RH2004 and RH2005 fixes. The token-list overloads are now called only from `DeclarationModifierUtilitiesTests.cs:63`.<br>The branch that appends a carried leading trivia (the indentation of a removed `public` that started its line) to the *trailing* trivia of the list's last token exists only for these overloads. No caller reaches it, and its remarks state a second, different placement rule ("trivia that would go to that token stays at the end of the list").<br>The public overload's XML remarks `cref` the private `ReplaceAccessibilityModifiers`.<br>`tokens.Count == 0` is true only when no kinds are passed and the first two modifiers are both accessibility keywords. No caller does that: RH2004 and RH2005 pass exactly one kind, and RH2005 reports only `public`, which cannot stand next to another accessibility keyword without CS0107. | Remove the two token-list overloads and the default-token branch, and point the one Core test at the member overload (or drop it). Then `followingToken` is never `default`. Update the `IsAccessibilityModifier` summary, which `cref`s the removed overload. Drop the unreachable `tokens.Count == 0` operand. Alternatively, the lead records keeping them as a decision. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2005-005: a comment or directive next to a later accessibility modifier (`HasCommentOrDirectiveNextToLaterAccessibilityModifier`) | For each accessibility modifier at index ≥ 1: its leading trivia, its trailing trivia, and the leading trivia of `GetNextToken()` | `ReplaceAccessibilityModifiers`.<br>Branch "starts its line": it moves the leading trivia to the next token, and drops the trailing trivia and the next token's own leading trivia.<br>Branch "does not start": it appends the leading and trailing trivia to the token in front, keeps the next token's leading trivia, and trims only `WhitespaceTrivia` from the end of the token in front.<br>Spec: RH2005-005's two places. A same-line comment in front of the modifier is trailing trivia of the token in front, so it is not inspected; that matches "`static /* note */ public` is not covered" | The guard covers every span the method moves or drops. The only span not inspected (the trailing trivia of the token in front) loses only whitespace. The guard is wider than the drop in the second branch (a comment line between `public` and the type would be kept there), and that is the output Q8 decided | ok |
| "Starts its line" | `ContainsEndOfLine(modifier.LeadingTrivia)` or a line break in the trailing trivia of the kept token in front | RH2005-003's Q5 sentence. A token starts its line exactly when one of the two holds; the token in front is the original modifier, unchanged, because RH2005 removes one keyword | Agree (FC-002: the qualifier is an explicit condition) | ok |
| "Ends its line" (Q7) | Not a separate predicate. The trailing trivia, with its line break, moves to the token in front | RH2005-003's Q7 sentence and the one-line example | Agree for a line break. For whitespace-only trailing trivia, see #7 | #7 |
| Where `private` stands | `GetFirstTokenBehindAttributeLists`, unchanged | – | – | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Where the trivia of a removed accessibility keyword goes | `ReplaceAccessibilityModifiers` only. `rg -n "CarryLeadingTrivia\|RemoveAccessibilityModifiers\|InFrontOfLaterAccessibility" src sdd/specs` finds nothing. The member overload only restores the first token's leading trivia, which decides where `private` stands. `rg -n "IsAccessibility\|PublicKeyword\|PrivateKeyword" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core`: `ModifierOrderingUtilities` (reordering, not removal) and RH7104 (`GetAccessibilityModifierKinds` only) | one owner; decision 25 is carried out |
| Which modifiers are removed | `IsAccessibilityModifier`, in `DeclarationModifierUtilities` only | one owner |
| Trivia kinds that count as comment or directive | `SyntaxTriviaUtilities.IsCommentTrivia` / `IsDirectiveOrDisabledTextTrivia` | one owner |
| Trimming whitespace at the end of trivia | The new private `DeclarationModifierUtilities.TrimTrailingWhitespace`, and the private copy in `StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs:168`, which predates the run and lives in another assembly | not a policy (a five-line helper); hint |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixKeepsTheLine{Break,Of}…` (a, f, column move, middle, last behind two, two lines in front, attribute list) +CRLF | RH2005-003 (Q5, Q7) | any other text, e.g. `private⏎static` | `Verify`, exact text, iterative and Fix All | adequate |
| `VerifyFixReplacesAPublic{Between,Behind}TwoModifiersOnOneLine` +CRLF | RH2005-003 one-line | other text | `Verify` | adequate; single spaces only (#7) |
| `VerifyFixKeeps{TheLineBreak,TheComment}BehindAFirstPublicThatEndsItsLine` +CRLF | `main`'s unchanged output (decision 29; by trace, identical on `main`) | other text | `Verify` | adequate |
| `VerifyNoFixIsOfferedFor…` (b, inline block comment, e, c, d, and the middle, own-line, and last-behind-two variants) +CRLF | RH2005-005 (Q8) | a fix registered | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixAllKeepsTheLinesOfLaterPublicModifiersAndSkipsACommentBehindOne` | SHARED-FIX-002, with RH2005-003/-005 | a withheld field fixed, or another layout | `Verify`; the remaining diagnostic is asserted by markup in `FixedState`/`BatchFixedState` | adequate |
| No test for a one-line non-first `public` with gaps other than one space | – | – | – | missing, #7 |

## thoenissen/Reihitsu#1286

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding._ The repair only narrows `VerifyFixKeepsACommentLineInFrontOfTheType` (#6, closed). No production code of RH2001 changed.

### Guard scope
_N/A — no guard changed in this round._

### Policy ownership
_N/A — no policy changed in this round._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixKeepsACommentLineInFrontOfTheType` | SHARED-FIX-003, and SHARED-FIX-007 when the fix is offered | the comment dropped or moved, or a layout the formatter changes | `VerifyCodeFixIsWithheldOrProducesOneOf(testData, true, Format(inPlace))` | adequate; the formatter is the oracle that SHARED-FIX-007 names |

## thoenissen/Reihitsu#1287

Not touched by the repair. `17-auditor.md` stands (no finding).

## thoenissen/Reihitsu#1288

Not touched by the repair. `10-auditor.md` and `17-auditor.md` stand (no finding).

## Whole run

### Visible output is decided
- RH2005-003 (a, f, the column move, the middle and last-behind-two variants, two lines in front, the attribute list): the Q5 and Q7 sentences and examples. Maintainer answers 5A and 7A.
- RH2005-005 (b, the inline block comment, c, d, e, and the variants): Q8 = A and decision 28.
- The first-position characterization tests: `main`'s unchanged output (decision 29). By trace, `main` gives the same text.
- RH2001: the accepted set narrowed; no new value.
- No rule page changed (`git diff a79eb22 7714c11 -- documentation` is empty).
- One output changed without a decision, with no test pinning it: #7.

### Comments and conventions
- No `TODO`, no commented-out code, and no tracker number in an added `src/` line.
- FC-031: the summaries and remarks of `HasCommentOrDirectiveNextToLaterAccessibilityModifier`, both `AddAccessibilityModifiers` overloads, and `ReplaceAccessibilityModifiers`, and the RH2005 provider's inline comment, describe the new code. The token-list remarks describe an unreachable path (#8).

### Scope
- Within decisions 25–28 and finding 6.
- Production paths: the two files that `20-implementer.md` names.
- The renamed public guard has one caller.

### Failure classes dismissed
- **FC-001, FC-002**: the guard and the method inspect the same spans (*Guard scope*).
- **FC-003**: both sides are tested:
  - starts / does not start its line;
  - ends / does not end its line;
  - a comment / a line break alone;
  - first / not first.
- **FC-004, FC-005, FC-019, FC-020, FC-021**: comment, `#pragma`, `#if`, `#if`/`#else`, and `#region` behind a later `public`, and a comment on the modifier in front. Probed and tested; every result is withheld or kept in place.
- **FC-006**: every probe compiles (`invalid-result` 0). The rewrite changes only trivia and the keyword.
- **FC-007, FC-009**: every probe converges in one iteration, and an offered fix always changes `public`.
- **FC-008**: the guard is decided at registration. BatchFixer applies the per-field actions, and each one touches only its own field (Fix All test).
- **FC-010–FC-014, FC-028, FC-029, FC-032**: no formatter change. The RH2005 fix does not format, and the merged main's formatter change touches no run file.
- **FC-015**: one owner per changed policy.
- **FC-016**: the sibling shapes were enumerated (#5's row); the one changed sibling is #7.
- **FC-017**: exact-text assertions with CRLF twins (see the table above).
- **FC-018, FC-036**: no rename.
- **FC-022–FC-027**: no literal or expression-body path. A verbatim identifier, `global::`, a tuple type, and several declarators were probed.
- **FC-030**: no formatting.
- **FC-033**: CRLF twins go through the code-fix path. `apply-fix` reports LF and CRLF output identical, with no line-ending drift.
- **FC-034**: the reported shapes (a–f) are tested verbatim.
- **FC-035**: expected values come from spec examples or `main`'s unchanged output.
- **FC-037**: no comment-only claim.
- The RH2004 caller: by trace, no accessibility keyword is removed there, so the loop adds every modifier unchanged and the following token is replaced by itself. The output equals `a79eb22`'s, and the RH200 tests pass (`20-implementer.md`).

### Architecture findings
_None._ Decision 25 is carried out: one method decides both sides of a removed accessibility keyword's trivia for every position, and the guard inspects the gaps that method merges.

### Hints
- **Trivia helper duplicated across assemblies.** `TrimTrailingWhitespace` is now private in `Reihitsu.Core` and private in `StatementShouldBePrecededByABlankLineCodeFixProviderBase`. A shared `SyntaxTriviaUtilities` member could serve both. This is a trivial helper, and the code-fix copy predates the run.
- **Partly pre-existing: one-line gaps the fix leaves unformatted.** On a one-line declaration whose gaps are wider than one space, both `main`'s and HEAD's results keep a gap the formatter collapses (rows 4 and 5 of #7), which SHARED-FIX-007 does not allow for changed code. HEAD fixes row 3 and leaves rows 4 and 5 as they were on `main`. If the lead asks #7, the same answer can decide this.

## Commands

```shell
git rev-parse HEAD; git fetch origin main; git merge-base --is-ancestor 4e61f29 HEAD
scripts/lane-check.sh bug-fix                                   # confirmed
scripts/apply-fix.sh RH2005 <scratchpad>/audit23/rh2005         # 28 fixtures x LF/CRLF: 42 fixed, 10 no-fix-offered, 4 no-diagnostic, 0 invalid-result, 0 line-ending-drift
scripts/apply-fix.sh RH2005 <scratchpad>/audit23/rh2005b        # 5 one-line gap fixtures x LF/CRLF: 10 fixed (#7)
scripts/format.sh <scratchpad>/audit23/fmt/F1.cs                # static( and static@int kept
scripts/format.sh <scratchpad>/audit23/fmt/F2.cs                # wide modifier gaps collapsed
```
