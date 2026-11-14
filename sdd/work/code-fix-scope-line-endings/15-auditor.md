Role: auditor
Commit: a219aacc2c2b9d45625ce28841048305eb1436c6
Verdict: findings (#1283: pass; #1284: pass, #2 and #3 closed; #1285: findings, #1 closed, new medium #4 repeating #1's class)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

## Scope

- Audited tree: `a219aacc2c2b9d45625ce28841048305eb1436c6`. The checkout is at `5551674`, which differs from it only in `00-lead.md`. The working tree is clean.
- Base: `origin/main` is `cf6d26b03fe1917f193d412c8540916f93a00dc4` (`git ls-remote`). It matches the prompt, and the audited tree contains it.
- Repair delta: `888654727cd8..a219aac`. It changes three production files: the RH3001 provider, `BlankLineCodeFixUtilities`, and `ExpressionBodyCodeFixUtilities`. It adds 21 RH3001 tests and changes RH3001's spec, the requirement IDs, and the change doc. That matches handovers 10 to 14.
- `scripts/lane-check.sh behavior-change`: confirmed.
- The scope did not grow. The repair stays inside the RH3001 fix and the two helpers that findings #2 and #3 named. The evidence from `09-auditor.md` on RH3006 and on the SHARED-BODY write-back still holds byte for byte. Neither code path changed.
- Evidence:
  - `scripts/apply-fix.sh RH3001` (LF and CRLF arms) on 23 scratch fixtures outside the repository, run on three trees: the audited tree, a detached worktree at `888654727cd8`, and a detached worktree at `origin/main`. Both worktrees have since been removed.
  - Static tracing of `TouchesWord`, `BuildReplacement`, and `FindReplacement`.
  - No test was re-run. I relied on the cleaner's Release build and the implementer's focused runs.

## thoenissen/Reihitsu#1283

Nothing in the repair reaches the RH3006 fix. The shared wrapper rename in `BlankLineCodeFixUtilities` is not on its path. The evidence and verdict of `09-auditor.md` stand. No findings.

## thoenissen/Reihitsu#1284

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 2 | closed | `rg "FormatNodeAsync\|FormatNodeInDocumentAsync" src/Reihitsu.Analyzer.CodeFixes` finds one wrapper left, `BlankLineCodeFixUtilities.FormatNodeInDocumentAsync`. `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` (three arguments) and `FormatMemberOnLinesOfItsOwnAsync` (three arguments) both pass it. The body is unchanged, so behavior is identical. The other providers' inline `ReihitsuFormatter.FormatNodeInDocumentAsync(...).Output` calls predate the run and are not handed to a helper as delegates. |
| 3 | closed | The line-separation text now sits on the four-argument overload, which does the work. Its closing clause, "no gap is broken, and the separators around the member stay as written", matches the code: `breaksBefore` and `breaksAfter` are false off a member line, and `formatAsync` gets the member alone. That agrees with RH3202-002 and RH5417-003. The three-argument summary says it formats the whole member, which is what `FormatNodeInDocumentAsync` does. RH3202 is no longer named under "only the member is formatted". |

### Findings
_None._

### Guard scope
_N/A — the repair adds or changes no predicate on this issue's path._

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Formatter call wrapper handed to the code-fix helpers | `BlankLineCodeFixUtilities.FormatNodeInDocumentAsync` only | one owner; finding #2 closed |
| Line separation for a member that shares its line | `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (four arguments, with a three-argument front) | one owner |

### Assertion adequacy
_N/A — the repair adds or changes no test for this issue._

### Failure classes dismissed
- Every class: the repair is a delegate rename and summary text on this path. No input reaches different code. FC-031 is checked under finding #3.

### Architecture findings
_None._

## thoenissen/Reihitsu#1285

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member that #1 listed is now reported without a fix on the audited tree. The tests cover `return`, `yield return`, `case`, `goto case`, `when` (switch-expression arm), `where`, `select`, `orderby`, `group`, `by`, `on`, `equals`, the trailing side (`where !(w)select w`, `return !(a)is true`), and Fix All. Probes cover `case!true:`, `return! a`, `return!this.P`, `orderby !(w)descending`, and `!(a)as`. A `when` case guard is traced: its token in front of `!` is `WhenKeyword`, so the same check applies. No parenthesized output that groups nothing remains in any keyword-touching member. |

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3001NotOperatorShouldNotBeUsedCodeFixProvider.cs` `TouchesWord`, front-side clause; `sdd/specs/analyzer/RH3001.md` RH3001-009 (letter and Why); the change doc's *Visible output changes* | FC-002, FC-015 | #1 | PR-introduced | See *Finding 4* below | Key the front side on whether the token in front of `!` would merge with the first thing the replacement writes. That is the operand's first token, or a comment kept in front of it. Use the existing owner, `TokenSeparationUtilities.WouldMergeIntoOneToken`, as RH3002's fix does. Amend RH3001-009's letter to match, and add one fixed test, `return!(a)` → `return(a) == false`, next to the withheld ones. The other choice is to accept the wider refusal: then correct the Why and the guard's summary, and list `return!(a)` and the other members under *Visible output changes*. Either way, the trailing side stays as it is (see below). |

#### Finding 4

The front-side clause of `TouchesWord` withholds the fix whenever a keyword or identifier stands directly in front of `!`. It does not check what the replacement writes next to that word. RH3001-009's Why and the guard's own summary give the reason as "Without `!`, the operand's first token would merge with the word in front of it (`returna`)". That reason does not hold when the replacement starts with a token or a comment that cannot merge. In those members the rewrite was safe and compliant with RH3001-004, and both `main` and `888654727cd8` offered it. The repair now withholds it. The change doc does not list it as a visible change.

| Input (word directly in front of `!`) | `main` | `888654727cd8` | audited tree |
|---|---|---|---|
| `if (b) return!(a);` | `return(a) == false` | `return(a) == false` | no fix |
| `yield return!(a);` | `yield return(a) == false` | same | no fix |
| `case!(true):` | `case(true) == false:` | same | no fix |
| `_ when!(y) => 1` | `_ when(y) == false => 1` | same | no fix |
| `where!(w) select w` | `where(w) == false select w` | same | no fix |
| `return!/* c */a;` (comment between `!` and the operand) | – | `return/* c */a == false` (RH3001-005) | no fix |

- Traced, not probed, and failing the same way: `select!(w)`, `goto case!(true)`, `on!(a)`, `equals!(a)`, `group!(w) by!(w)`, `orderby!(w)` without a direction, and a verbatim operand `return!@a`. The verbatim case is not a merge, because the existing owner's own example says `return` and `@a` stay two tokens.
- Members that hold, where withholding is right: the operand's first token is a word and no comment stands between `!` and the operand. Examples are `return!a`, `return! a` (the fix strips that space), `case!true`, `return!this.P`, and every trailing-side member. In every trailing member `false` merges with the word behind it.
- Trailing members whose binding needs the parentheses, such as `return !(a)is true` → `((a) == false)is true` and `!(a)as object`, are also refused although the parentheses would group. The front-side member `return!a is true` → `return(a == false) is true` is refused the same way. Telling binding apart from merging there would need a second parse, so it is not a cheap check. Keeping those members refused is right for a rare class, and the suggested change leaves them withheld.

Why it is a finding:

- `AGENTS.md`, *Code-fix coverage*: "A fix that refuses inputs it could rewrite safely is still worth narrowing, as long as the narrowing stays a cheap check". The narrowing here is one call to an existing helper on two tokens.
- The policy "would these two tokens merge if written together" already has one owner in `Reihitsu.Core`, `TokenSeparationUtilities.WouldMergeIntoOneToken`. That owner is used by RH3002's fix (`CreateReplacement`), by `LineBreakTriviaUtilities`, and by `ExpressionBodyToBlockConverter`. `IsWord` is a second, approximate statement of the same question (FC-015). The guard's summary states the merge as its reason, so the approximation shows exactly where the reason and the predicate diverge (FC-002).
- `12-implementer.md` considered the owner. It kept the token-kind check because the spec's letter withholds `return!(a)`. Decision 18 chose "token-kind check" as the one cheap check. Neither recorded that the letter then contradicts its own Why for these members.

On the repeat: it is the same decision as #1, what the RH3001 fix does where removing `!` exposes the operand or `false` to a neighboring word, failing in the same component against RH3001-004/-009. #1 fixed too much, and this one refuses too much. Under `README.md`, *Architecture findings*, the lead decides it instead of starting another repair round. The class is already refused, so accepting the wider refusal with corrected text is a legitimate outcome.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Front side: withhold where the word in front of `!` would merge | Kind of the token in front of `!`, its trailing trivia, and `!`'s leading trivia | RH3001-009's Why ("the operand's first token would merge with the word in front of it"); `BuildReplacement`, which writes `!`'s trailing trivia and the operand's leading trivia, without leading whitespace, in front of the operand; `WouldMergeIntoOneToken` | **Disagree.** The predicate never looks at the operand's first token or at a comment the replacement keeps, so it withholds where nothing merges. | finding #4 |
| Trailing side: withhold where `false` would merge with the word behind | Kind of the token behind the negation's last token, plus the trivia on both sides of that gap | RH3001-009 letter; `false` merges with every keyword or identifier | Agree with the letter, and exact for merging. It also refuses binding-parenthesized members, as stated above, which is accepted as a rare class. | holds |
| Order of the checks | `TouchesWord` runs before `HasSafeOperandTrivia` and `FindReplacement`. It only withholds. | RH3001-008 (constant pattern) and RH3001-006/-007 | The checks are independent. `case!true when x:` is withheld by both RH3001-008 and RH3001-009, and they agree. | holds |
| Fix All | `BatchFixer` calls `RegisterCodeFixesAsync` per diagnostic, so the guard applies on the Fix All path | FC-008 | `FixAllLeavesANegationTouchingAWordReported` | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether two tokens written together would merge | `rg -n "WouldMergeIntoOneToken" src --glob '!*Test*'`: `Reihitsu.Core.TokenSeparationUtilities`, used by the RH3002 fix, `LineBreakTriviaUtilities`, and `ExpressionBodyToBlockConverter`. `RH3001…CodeFixProvider.TouchesWord`/`IsWord` approximate it by token kind | finding #4 |
| What a code fix does when its rewrite would merge two tokens | RH3002 spec and fix: insert one space. FMT-STRUCT-026 and the line-break join rule: write exactly one space. RH3001-009: withhold the fix | see hint 1 |
| Keyword-kind test | `rg -n "IsKeywordKind" src --glob '!*Test*'`: only `IsWord` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NegationsTouchingAWordAreReportedWithoutACodeFix` and the 13 `…Directly…`/`…Touches…` tests | Reported, and no action registered for any diagnostic | An action registered, or the diagnostic missing | `VerifyNoCodeFixIsOffered`: exact diagnostics, then `RegisterCodeFixesAsync` for each diagnostic on the markup-free source | adequate. Every member is its own small test, and both sides are covered. |
| `NegationsSeparatedFromAWordBySpaceOrCommentAreFixed`, `NegationBehindReturnAndASpaceIsFixed`, `NegationSeparatedFromReturnByACommentIsFixed`, `NegationBehindWhenOfASwitchExpressionArmAndASpaceIsFixed`, `NegationWhoseOperandIsSeparatedFromSelectByACommentIsFixed` | The other side of the boundary: whitespace or a comment keeps the fix, with exact output | No fix, parentheses, or a moved comment | `Verify` (exact text, iterative and Fix All) | adequate (FC-003) |
| `NegationOnTheLineBelowReturnIsFixed` (LF and CRLF) | A line break between the word and `!` keeps the fix and the line ending | An LF in the CRLF arm | `VerifyCodeFixWithEndOfLine` | adequate (FC-017) |
| `NegationDirectlyBehindAnOpeningParenthesisIsFixed` | Punctuation in front of `!` does not withhold | No fix | `Verify` | adequate |
| `FixAllLeavesANegationTouchingAWordReported` | Fix All fixes the others and leaves the touching one reported | Fix All rewrites it, or drops a sibling | `Verify` with markup allowed in the fixed and batch states | adequate (FC-008) |
| Missing | A front-side touch whose operand starts with `(` or a kept comment | – | – | Part of finding #4. No test pins either side of that boundary, so neither the narrowing nor the wider refusal is asserted. |

### Failure classes dismissed
- FC-001: the guard inspects at least the span its decision needs. Finding #4 is the opposite direction, a span wider than its reason.
- FC-004, FC-019, FC-021: the guard only withholds. On the fixed side, a comment between the word and `!`, or behind the operand, is kept in place. The tests assert this exactly.
- FC-005, FC-020: a directive in either gap makes the trivia non-empty, so the guard does not withhold. `HasSafeOperandTrivia` is unchanged and still refuses a directive between `!` and the operand. A directive in front of `!` is outside the replaced span.
- FC-006, FC-022, FC-023: no new rewrite. A verbatim operand is covered under finding #4.
- FC-007, FC-009: a withheld fix registers nothing. The fixed side is unchanged from the first audit.
- FC-010–FC-014, FC-028, FC-038–FC-043: the formatter is not involved. No shape is declared, and the lead decided that declaring is not an option (decision 14).
- FC-016: the trailing side is covered next to the front side, as the first audit's enumeration required.
- FC-026: the replacement is built from the node.
- FC-031: the summaries of `TouchesWord`, `IsWord`, and the `RegisterCodeFixesAsync` comment describe the new code. `TouchesWord`'s stated reason is the subject of finding #4.
- FC-035: the expected values come from RH3001-009's examples, and the withheld tests were red before the guard (`11-test-implementer.md`).

### Architecture findings
_None beyond the repeat._ Finding #4 repeats #1's class, the merge decision of the RH3001 fix, inside one provider. It is listed in `Findings` with `Repeats #1`, and the lead decides it under `README.md`, *Architecture findings*. The second owner of the merge question (`IsWord` next to `TokenSeparationUtilities`) is part of it.

## Whole run

### Visible output is decided
- Every new expected value either withholds a fix, which `AGENTS.md`, *Code-fix coverage*, decides for a rare class and which is recorded in the change doc's *Decisions* (decision 14), or is today's output with whitespace or a comment in the gap (RH3001-004, RH3001-005, unchanged).
- No rule page changed.
- The change doc's *Visible output changes* omits the punctuation-operand members of finding #4, such as `return!(a)`, which change from `main`'s `return(a) == false` to no fix. That is covered by the suggested change of #4.

### Comments and conventions
No `TODO`, no tracker numbers, and no commented-out code in the repair diff (`git diff … -- src`, `rg`).

### Cost of special cases
The new guard is one cheap token and trivia check that serves a refusal. It is not machinery.

## Hints

1. **Policy divergence on the same decision, decided by the lead.** RH3002's spec and fix, FMT-STRUCT-026, and the line-break join rule all answer "the rewrite would merge two tokens" by writing one space, through `WouldMergeIntoOneToken`. RH3001-009 answers the same question by withholding. The change doc rejects "insert a space" because "the fix would decide spacing in a gap the author left empty". RH3002 does exactly that. `main`'s RH3001 output for `return!a` was `return a == false`, a space inserted. Decision 14 followed the first audit's "refuse this class" suggestion, which is permissible for a rare class. This hint only notes that the repository answers the same question two ways. It does not block.
2. **Helper placement.** The shared formatter wrapper now lives in `BlankLineCodeFixUtilities`, a class named for blank lines. Its summary links to `ExpressionBodyCodeFixUtilities`, and `ExpressionBodyCodeFixUtilities` depends on it. Both helpers are internal to one assembly, and the behavior is identical. This is a naming and placement observation only.
3. **Tooling, pre-existing.** `scripts/apply-fix.sh` asks for actions on the first diagnostic of a fixture only (`FixtureRunner.RunIterationAsync`, `diagnostics[0]`). In `return!!a`, it reports `no-fix-offered` for the whole fixture, although the inner `!a` still gets its fix (traced: the token in front of the inner `!` is `!`). A sweep that reads `no-fix-offered` as "nothing is fixable" can be misled when the first diagnostic is withheld.
