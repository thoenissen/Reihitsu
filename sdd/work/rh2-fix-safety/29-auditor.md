Role: auditor
Commit: a90e4f0b84ee2c46d4b25f35881579f660dbf748
Verdict: pass

The prompt carried context from `00-lead.md` (decisions 25 and 30, Q9). It carried no intended fix, no suspected cause, and no hoped-for verdict, so there was nothing to ignore.

State check:
- The checkout is `a90e4f0`.
- `origin/main` is `aebe204` (`git ls-remote` agrees), and it is an ancestor of HEAD.
- The merge `46c86a0..a90e4f0` changes only `.github/PULL_REQUEST_TEMPLATE.md`, `squad/README.md`, `squad/lead.md`, `sdd/changes/2026-11/pr-describes-the-change.md`, and `00-lead.md`. None of them is a run file, and none is in `src/`, `sdd/specs/`, or `documentation/`.
- `scripts/lane-check.sh bug-fix` reports `confirmed`.
- The repair `7714c11..46c86a0` outside `sdd/work/` changes four files: `sdd/specs/analyzer/RH2005.md`, `src/Reihitsu.Core/DeclarationModifierUtilities.cs`, `RH2005FieldsMustBePrivateAnalyzerTests.cs` (+218 lines, the ten Q9 tests), and `DeclarationModifierUtilitiesTests.cs`. Handovers 24–28 describe exactly this.

I trusted the cleaner's Release build (`27-cleaner.md`, 0 warnings) and the implementer's whole-project runs (`26-implementer.md`: analyzer 7288, core 660, architecture 115).

Probes ran with `scripts/apply-fix.sh RH2005 <dir>`, which runs each fixture under LF and CRLF, and with `scripts/format.sh`. Fixtures are under `/tmp/claude-0/-home-user-Reihitsu/11377a95-24cc-5692-825a-5a87f974a5d7/scratchpad/audit29/`, and I re-ran the `audit23/` fixtures. "Observed" means a probe printed the result. "By trace" means it was read from the code.

## Scope

- Head `a90e4f0`; base `origin/main` `aebe204`.
- This is a repair-delta audit against `7714c11` (`23-auditor.md`).
- The spec change is RH2005-003's Q9 sentence, its *Why* sentence, and its five-row example. It records maintainer answer Q9 = A, which closes audit finding 7, and stays inside that finding's decision. Decision 30 removes code and changes no spec.
- So I audited:
  - the one changed branch of `ReplaceAccessibilityModifiers` and the guard that protects it;
  - the member overload's new following-token handling;
  - both production callers (RH2005, RH2004);
  - the removed overloads, branch, and operand;
  - the new and changed tests.
- Scope did not grow.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–4 | closed | Closed in `17-auditor.md`. This repair does not touch their code paths. |
| 5 | closed | Closed in `23-auditor.md`. I re-ran the 28 `audit23/rh2005` fixtures (A01–A28), which cover every member #5 enumerated, under LF and CRLF. The output is byte-identical to `23-auditor.md`'s run except A12 (`static public(int A, int B)`), which is a Q9 row and now reads `private static (int A, int B)`. Totals are unchanged: 42 fixed, 10 no-fix-offered, 4 no-diagnostic, 0 invalid-result, 0 line-ending-drift. |
| 6 | closed | Closed in `23-auditor.md`; the test was not touched. |
| 7 | closed | RH2005-003 now decides every row through the Q9 sentence and its example. Observed on HEAD, LF and CRLF identical, using the `audit23/rh2005b` fixtures B01–B05 plus A12:<br>- `static public(int A, int B)` → `private static (int A, int B)`<br>- `static public@int` → `private static @int`<br>- `static    public int` → `private static    int`<br>- `static public    int` → `private static int`<br>- `static public  volatile int` → `private static volatile int`<br>- `static volatile public(int A, int B)` → `private static volatile (int A, int B)`<br>Ten exact-text tests (five rows, LF and CRLF twins) pin these outputs. |
| 8 | closed | Both `SyntaxTokenList` overloads, the `followingToken.RawKind == 0` branch, and the `tokens.Count == 0` operand are removed. `rg "SyntaxTokenList, SyntaxKind\|AddAccessibilityModifiers?\(.*Modifiers," src sdd documentation` finds nothing outside `sdd/work/`. The `IsAccessibilityModifier` summary now `cref`s the member overload and carries the `file` rationale. The public remarks no longer `cref` the private method. The Core test calls the member overload. |

## thoenissen/Reihitsu#967

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Q9 branch: a removed non-first modifier that neither starts nor ends its line (`DeclarationModifierUtilities.cs:220-230`, which falls through to nothing) | `ContainsEndOfLine(modifier.LeadingTrivia)`; the trailing trivia of the kept token in front; `ContainsEndOfLine(modifier.TrailingTrivia)` | RH2005-003's Q9 sentence. RH2005-005's guard `HasCommentOrDirectiveNextToLaterAccessibilityModifier` inspects the modifier's leading and trailing trivia and the next token's leading trivia. | The branch drops the modifier's leading trivia (empty when it does not start its line) and its trailing trivia (whitespace only, because the guard withholds a comment there). The token in front keeps its trailing trivia unchanged, and the following token keeps its leading trivia. Every dropped span is inside the guard's span. The token in front's trailing trivia is not inspected, but it is no longer touched at all: before this round it lost its trailing whitespace. | ok |
| "Ends its line" (Q7) | It is now an explicit predicate, `ContainsEndOfLine(modifier.TrailingTrivia)` | RH2005-003's Q7 sentence | Agree. Both sides are tested: `static public⏎        int` is tested on the Q7 side and the five Q9 rows on the other. Observed: C17 (`static    public⏎        int` → `private static⏎        int`) and C18 (a space in front of the line break) take the Q7 branch, and their output is unchanged from 7714c11. | ok |
| Following token when there are no modifiers (RH2004's path) | `GetFirstTokenBehindAttributeLists(declarationWithoutLeadingTrivia)` | The earlier code skipped the replacement when `modifiers.Count == 0` | With no modifiers the loop does not run and `carriedLeadingTrivia` stays null, so the token is replaced by itself. RH2004's output is unchanged, by trace; the RH2004 tests pass (`26-implementer.md`). | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Where the trivia of a removed accessibility keyword goes | `ReplaceAccessibilityModifiers` only. `rg -n "AddAccessibilityModifiers?\(\|ReplaceAccessibilityModifiers" src --glob '!*Test*'` finds callers only through `AddAccessibilityModifier(MemberDeclarationSyntax, SyntaxKind)`, at RH2005 provider `:45` and RH2004 provider `:62`. RH7104 uses only `GetAccessibilityModifierKinds`. | One owner. Decision 25 holds, and decision 30 removed the second placement rule. |
| Which modifiers are removed | `IsAccessibilityModifier` | one owner |
| Comment and directive trivia kinds | `SyntaxTriviaUtilities.IsCommentTrivia` / `IsDirectiveOrDisabledTextTrivia` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixRemovesAOneLinePublicWith{TupleBehindPublic,VerbatimTypeBehindPublic,WideGapInFrontOfPublic,WideGapBehindPublic,WideGapBehindPublicInFrontOfModifier}` (+`CarriageReturnLineFeed`) | RH2005-003, the Q9 sentence: one row of the example each | Any other gap: `static(`, `static@int`, a collapsed or widened gap in front, or a kept gap behind. 7714c11's outputs fail all ten (`25-test-implementer.md`, red before the change). | `Verify`: exact text, iterative and Fix All | Adequate. Row 2 declares `class @int` so the fixed code compiles, and the field line stays the spec's. |
| `DeclarationModifierUtilitiesTests.AddAccessibilityModifierReplacesExistingAccessibilityAndPreservesTrivia` | The member overload replaces a first accessibility modifier and keeps its trailing trivia | `private` kept next to `public` (count 3), or a changed trailing gap | MSTest exact asserts | Adequate. The new count assertion makes "replaces" explicit. |

## thoenissen/Reihitsu#1286

Not touched by the repair. `23-auditor.md` stands (no finding).

## thoenissen/Reihitsu#1287

Not touched by the repair. `17-auditor.md` stands (no finding).

## thoenissen/Reihitsu#1288

Not touched by the repair. `10-auditor.md` and `17-auditor.md` stand (no finding).

## Whole run

### Visible output is decided
- The ten new RH2005 tests:
  - Rows 1, 3, and 4 are the three outputs the maintainer's Q9 = A answer quotes (`00-lead.md`, *Maintainer answers*).
  - Rows 2 and 5 follow from the same answer ("removed with the gap behind it; the gap in front stays as written").
  - RH2005-003 now demands all five rows.
- No rule page changed: `git diff 7714c11 HEAD -- documentation` is empty.
- Row 3 (`private static    int Third;`) keeps a gap that the formatter collapses. The maintainer chose this output explicitly. It also reads consistently with SHARED-FIX-007: the gap in front is whitespace the fix does not change, and SHARED-FIX-007's first bullet treats whitespace in front of the changed code as not part of it. `24-spec-author.md` records this reading.
- Other probes that reach the changed branch:
  - Observed: `static /* c */public int` → `private static /* c */int`, and `static/* c */public int` → `private static/* c */int`.
  - Both keep the comment in place, both compile, and the formatter leaves `/* c */int` unchanged (observed with `format.sh`).
  - By trace, `main` gives the same text: it kept the modifier in front's gap and dropped `public` with the gap behind it. So no new output is in question.

### Comments and conventions
- FC-031: the remarks of `ReplaceAccessibilityModifiers` now list three cases, and each matches its branch. The `followingToken` parameter text no longer mentions `default`. `AddAccessibilityModifiers`' remarks and the `IsAccessibilityModifier` summary describe the current code.
- `HasCommentOrDirectiveNextToLaterAccessibilityModifier`'s claim still holds: the discarded trivia is whitespace and line breaks only when the check is `false`.
- No `TODO`, no commented-out code, and no tracker number in an added `src/` line.

### Scope
- One production file and three test or spec files, as handovers 24–26 name them.
- No new public member; two public overloads were removed, and they had no production caller.

### Failure classes dismissed
- **FC-001, FC-002**: see *Guard scope*. The new branch drops only spans the guard inspects, and "ends its line" is now an explicit condition.
- **FC-003**: both sides of each qualifier are tested:
  - starts its line (Q5) / does not;
  - ends its line (Q7) / does not (Q9 rows);
  - a single-space gap (`VerifyFixReplacesAPublic{Between,Behind}TwoModifiersOnOneLine`) / a wider gap / no gap.
- **FC-004, FC-005, FC-019, FC-020, FC-021**:
  - A comment, `#pragma`, `#if`, or `#region` next to a later `public` withholds the fix (RH2005-005 tests). Re-run A18–A22 are unchanged.
  - A comment behind the modifier in front stays in place. Observed: A08, C01–C03.
  - A comment over several lines in the trailing trivia of the modifier in front is not an `EndOfLineTrivia`, so the Q9 branch keeps that trivia whole (by trace).
- **FC-006**: 0 invalid-result in 104 probe runs (A, B, and C sets, LF and CRLF). This covers a tuple, a verbatim type, `global::`, a tab gap (C04), several declarators (C08), `new` (C11, C19), `volatile`, and an attribute on the same line (C09).
- **FC-007, FC-009**: every probe converges in one iteration, and every offered fix removes `public`.
- **FC-008**: each per-field action touches only its own field. Fix All runs in every `Verify` call, and `GetFixAllScenario` is unchanged.
- **FC-010–FC-014, FC-028, FC-029, FC-032**: no formatter change. The RH2005 fix does not format, and main's merge touched no code.
- **FC-015, FC-016**: one owner. The sibling shapes along the two axes "starts its line" and "ends its line", and gap width in front, behind, or none, were probed (A, B, C sets).
- **FC-017**: exact-text assertions with CRLF twins.
- **FC-018, FC-036**: no rename.
- **FC-022–FC-027**:
  - A verbatim type (`@int`) and a verbatim name (`@Counter`) were probed.
  - No literal path.
  - Partial and explicit-interface shapes cannot carry a public field.
- **FC-030**: the fix does not format.
- **FC-033**: CRLF twins go through the code-fix path, and apply-fix reports no line-ending drift.
- **FC-034, FC-035**: the tests are the spec example rows verbatim. Their expected values come from the maintainer's answer, not from the patched code: they were red on 7714c11.
- **FC-037**: no comment-only claim.

### Architecture findings
_None._ Decision 25 still holds: one method decides the trivia of every removed accessibility keyword. Decision 30 removed the second placement rule that the token-list overloads stated.

### Hints
- The `TrimTrailingWhitespace` duplicate (`23-auditor.md`, *Hints*) is unchanged. It is a trivial helper, and the copy in `StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs` predates the run.

## Commands

```shell
git rev-parse HEAD; git ls-remote origin refs/heads/main; git merge-base --is-ancestor origin/main HEAD
scripts/lane-check.sh bug-fix                                              # confirmed
scripts/apply-fix.sh RH2005 <scratchpad>/audit23/rh2005    # 28 x LF/CRLF: 42 fixed, 10 no-fix-offered, 4 no-diagnostic, 0 invalid-result; only A12 changed vs. 23
scripts/apply-fix.sh RH2005 <scratchpad>/audit23/rh2005b   # 5 x LF/CRLF: 10 fixed, all Q9 outputs
scripts/apply-fix.sh RH2005 <scratchpad>/audit29/rh2005c   # 19 x LF/CRLF: 30 fixed, 8 no-diagnostic, 0 invalid-result, 0 line-ending-drift
scripts/format.sh <scratchpad>/audit29/fmt/F1.cs           # `/* c */int` kept
```
