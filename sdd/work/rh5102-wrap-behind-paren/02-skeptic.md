Role: skeptic
Commit: f7d8ef0
Verdict: proceed

The change is right, but the issue and the change doc misstate today's output. The issue says the RH5102 fix splits `M(⏎1, 2)` into one argument per line, so the fix and the CLI reach two different stable results. That is false. The fix delegates to the formatter, which joins the list to `M(1, 2)`, exactly as the CLI does. The real defects on `main` are different ones, listed under *Premise*. The maintainer's answer A still stands on them, and its *after* output is unaffected. The change doc's *before* blocks for entries 1 and 2, its *Why*, and its rejected option B must be corrected (*Internal choices* 1).

My prompt carried no intended fix or verdict; nothing to ignore.

## Premise

Branch `src/` is identical to `origin/main` (6a3d59f; `git diff 6a3d59f --stat -- src` is empty), so every run below shows `main`'s behavior. Fixtures were in the scratchpad and are deleted.

`scripts/apply-fix.sh RH5102 <dir>` (LF and CRLF give the same status in every row):

| Input | RH5102 on `main` | Fix result | Formatter (`scripts/format.sh`) |
|---|---|---|---|
| `M(⏎1, 2);` (entry 1) | reported | `fixed`, 1 iteration: `M(1, 2);` | `M(1, 2);` |
| `M(1, 2⏎);` (entry 2) | reported | **`no-progress`**: the fix is offered and changes nothing | kept as written |
| `M(⏎1, 2⏎);` | reported | joins to `M(1, 2⏎);`, then **`no-progress`** | `M(1, 2⏎);` |
| `M(1, 2 // c⏎);` (entry 3) | reported | `no-fix-offered` | kept |
| `M( // c⏎1, 2);` (entry 4) | reported | `no-fix-offered` | `M( // c⏎1,⏎2);` |
| `M(⏎#pragma …⏎1, 2);` (entry 4) | reported | `no-fix-offered` | one argument per line |
| `M(⏎/* c */ 1, 2);` | reported | `no-fix-offered` | one argument per line |
| `M( /* a⏎b */ 1, 2);` | reported | `no-fix-offered` | kept as single-line (FMT-BREAK-012: the line break is inside the comment text) |
| `M(⏎#region R⏎1, 2⏎#endregion // R⏎);` | reported | `no-fix-offered` | one argument per line |
| `new C(⏎1, 2)`, `new(⏎1, 2)`, `: this(⏎1, 2)`, `class D() : B(⏎1, 2);` | reported | joined, like entry 1 | joined |
| `M(⏎a: 1, b: 2)`, `M(⏎out var x, out var y)` | reported | joined | joined |

`scripts/apply-fix.sh RH5101` over the same fixtures: entry 1 and `M(⏎1, 2⏎)` are reported and fixed to the same join. None of the comment, directive, or `)`-only shapes is reported. `RH5101` therefore still reports entry 1 after the change, and its fix gives `M(1, 2);`, as the change doc says.

What is real on `main`:

1. **`M(1, 2⏎)` and `M(⏎1, 2⏎)`: RH5102 reports a shape the formatter keeps, and offers a fix that changes nothing.** This is a defect against existing requirements: SHARED-DIAG-010 (P31) and SHARED-FIX-005 (FC-009, FC-013). The issue does not mention it, but it is the strongest reason for the change.
2. **Entry 3 and `M( /* a⏎b */ 1, 2)`: RH5102 reports, without a fix, a shape the formatter keeps.** This violates SHARED-DIAG-010 too.
3. **Entry 1: RH5102 and RH5101 both report one list, and both fixes give the same `M(1, 2)`.** No circle and no two stable results, only a duplicate diagnostic. RH5102's own title ("all on one line or each on its own line") calls this list compliant.

Not real: the issue's "its fix splits the list into one argument per line" and "the analyzer and the formatter disagree" for entry 1. The formatter join of a list wrapped behind `(` was specified in #1733 (81eabeb, 2026-11-15 08:40), before the issue was filed. The fix has delegated to `FormatNodeInDocumentAsync` since well before that. So at filing time the fix already joined. The claim comes from the RH5109 analogy, where the fix rebuilds the list itself, and was not observed.

Caller reachability: whole-document formatting, the CLI, and the RH5102 code fix all reach these lists. The analyzer registers `SyntaxKind.ArgumentList` and reads only the list, so every parent (invocation, `new`, target-typed `new`, constructor initializer, primary-constructor base type) reaches it in the same way.

The new reading only removes diagnostics: the reports after the change are a subset of the reports on `main`. A reported list after the change has an argument that spans lines, or arguments that start on two lines, and either makes `(` and `)` lie on different lines. The removed set is exactly this: `(` and `)` on different lines, all arguments starting on one line, none spanning lines.

## Mechanism and candidates

_N/A_ (behavior change). For the record, the mechanism on `main` is this. `RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.OnArgumentList` decides "multi-line" by the lines of `(` and `)`, while FMT-BREAK-012 decides it by the elements. So a line break or trivia only around the arguments makes a list multi-line for the rule and single-line for the formatter.

Dispatch: `RegisterSyntaxNodeAction(OnArgumentList, SyntaxKind.ArgumentList)`. `ArgumentListSyntax` parents are `InvocationExpressionSyntax`, `ObjectCreationExpressionSyntax`, `ImplicitObjectCreationExpressionSyntax`, `ConstructorInitializerSyntax`, and `PrimaryConstructorBaseTypeSyntax`. The decision does not depend on the parent. Bracketed argument lists and attribute argument lists are other kinds and out of scope (spec non-goals).

## Breaking inputs

Failure classes that reach the change: FC-003, FC-009, FC-013, FC-019/FC-020/FC-021, FC-022, FC-027, FC-033.

**Matrix of the removed class** (arguments all start on one line, none spanning lines):

- Axis A, the gap between `(` and the first argument: same line; plain line break; `//` comment then a line break; `/* */` on its own line; a multi-line `/* a⏎b */` whose only line break is inside the comment; `#pragma`; `#region`.
- Axis B, the gap between the last argument and `)`: same line; plain line break; `//` comment holding `)`; a directive.
- A conditional directive in any gap is excluded earlier (RH5102-004, unchanged).

Every cell except (same line, same line) must be **not reported** after the change. All of them are reported on `main` (table above). Cover at least the following, each as not reported:

- `M(⏎1, 2)`
- `M(1, 2⏎)`
- `M(⏎1, 2⏎)`
- `M(1, 2 // c⏎)`
- `M( // c⏎1, 2)`
- `M(⏎#pragma …⏎1, 2)`
- `M( /* a⏎b */ 1, 2)`

Add one non-invocation parent, such as `new C(⏎1, 2)` or `: base(⏎1, 2)`, for FC-027. A test with CRLF line endings is cheap, and the decision counts lines (FC-033).

**Boundary, still reported** (FC-003). Each must stay reported:

- Arguments start on two lines: `M(⏎1, 2,⏎3)`.
- An argument spans lines, each variant behind a line break after `(`, so the old paren rule cannot be what reports it:
  - a lambda with a block body: `M(⏎1, () =>⏎{⏎})`;
  - a multi-line verbatim string: `M(⏎1, @"a⏎b")`;
  - a raw string literal;
  - an interpolated string with a line break in a hole;
  - a nested wrapped call;
  - a multi-line comment inside the argument: `M(⏎1, 2 /* a⏎b */ + 3)`. The formatter splits it too, and the fix is withheld.

On `main` I confirmed `M(1, @"a⏎b")`, `M(1, """⏎…""")`, and `M(1, $"{x⏎}")` as reported and fixed one argument per line. `M(⏎1, 2,⏎3)` and `M(⏎1, () =>⏎{⏎})` are fixed to `M(1,⏎2,⏎3)` and `M(1,⏎() =>⏎{⏎})`. The formatter gives the same results. Change-doc entry 5 is correct.

**Unchanged and not reported:** `M(1,⏎2)` and `M(⏎1,⏎2)` (`no-diagnostic` on `main`).

**Existing tests:** none asserts a report for a list whose arguments all start on one line. The only list that opens with `(⏎` (`FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis`, `FixMovesLeadingCommasOfSeveralArgumentListsInOneDocument`) has arguments on two lines and stays reported. No other test project names RH5102.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5102 analyzer | Must change: decide multi-line from the arguments (RH5102-001) |
| RH5102 code fix and Fix All | Must stay. With fewer diagnostics, every list still reported is one the formatter splits, so the `no-progress` cases disappear without touching the fix |
| Formatter (FMT-BREAK-012 behavior) | Not involved. Only the *Why* text changed in the spec |
| `Reihitsu.Core` | Not involved |
| Rule page `documentation/rules/RH5102.md` | Must stay. Its sentence "The code fix applies the one-argument-per-line form" is false on `main` for entry 1 (the fix joins) and becomes true after the change |
| RH5101, RH5038 | Not involved. RH5101 keeps reporting entry 1, RH5038 the comments of entries 3 and 4 |
| Tests | Must change: new analyzer tests per *Breaking inputs* |

## Refuse option

- **Withhold the fix or leave as written:** not applicable. The change removes diagnostics and adds no special case. The cost of refusing ("no change") is that the SHARED-FIX-005 and SHARED-DIAG-010 defects for `M(1, 2⏎)`, `M(⏎1, 2⏎)`, entry 3, and `M( /* a⏎b */ 1, 2)` stay. The requested change costs a few analyzer lines, the shape RH5109 already has, with no new type and no new mechanism. The input class is common code (`M(⏎a, b)` and `M(a, b⏎)` are ordinary), not rare.
- **Declare a shape unsupported:** the candidate is "a comment, directive, or disabled text between a list's opening parenthesis and its first element, with a line break in that gap", which is the entry-4 shape. It is not an option under *Special cases do not grow the source*, because it deletes nothing. The formatter's held-first-element layout is no separate special case. It falls out of `LineBreakListRewriter.HasElementSplitSignal` (`HasLeadingEndOfLine` on the first element) once the join refuses to cross the trivia, and that refusal serves every other list too. Declaring it would also refuse code people write, such as `Foo( // why⏎a, b)` and `#if` arguments. No escalation 10.
- **Narrower option:** keep reporting entry 1 (the fix joins, consistent with the formatter) and stop reporting only the `)`-side shapes. This would fix the SHARED-FIX-005 and SHARED-DIAG-010 defects. It contradicts the maintainer's answer A and keeps the duplicate with RH5101, so I do not recommend it. Recorded only because the issue's option B was shown with the wrong output.

## Output questions

_None._ The maintainer's answer A fixes the *after* output of entry 1. The corrected *before* changes nothing about it: the true option B is "reported, the fix joins to `M(1, 2)`", which still duplicates RH5101 and contradicts the rule's title, so A remains the better answer.

I concur with decision #2 that entries 2–4 are covered by "as decided for RH5109", which decided the same four shapes explicitly. Whether to re-confirm the answer, given that the issue showed option B's output wrongly, is for the lead. I recommend not re-asking.

## Internal choices

1. **Correct the change doc (must).**
   - Entry 1 *before*: the fix gives `M(1, 2);`, not `M(1,⏎  2);`.
   - Entry 2 *before*: "RH5102 reports the argument list; a fix is offered but changes nothing (SHARED-FIX-005), so the diagnostic stays." The derived `M(1,⏎2⏎)` is wrong. The test implementer no longer needs to confirm it; it is settled here.
   - *Why*: replace "its fix split it … two stable results" with the real defects in *Premise* items 1–3.
   - *Alternatives considered*, option B: its reason "two different stable results" is false. The real reasons are the duplicate with RH5101 and the rule's title.
   - Optional: add `M(⏎1, 2⏎)` (before: joined, then `no-progress`) as a further shape.

   This changes no visible output, only the record.
2. **RH5102-001's Why** says entry 1's reading is the one FMT-BREAK-012 takes, which is correct. Optionally it could add that a line break in front of `)` is kept by the formatter, so reporting it left a fix that changes nothing. Either wording is fine; no output difference.
3. **Entry 4 and P11.** The spec cites P11 ("the formatter may do more than the rule") for not reporting entry 4. P11 itself would classify a form of the rule's own construct that the rule skips as "the rule's own gap". Because RH5102-001 now states MUST NOT for it explicitly, as RH5109-001 does after the maintainer's explicit answer there, a later audit will read it as decided, not as a gap. I recommend keeping it as written, for parity with RH5109.
4. **Line counting.** RH5102-001 does not say how lines are counted. The analyzer counts raw text lines, while FMT-BREAK-012 counts them as SHARED-BLANK-008 does, so a line break inside a multi-line comment differs between the two. Within this change that makes no difference: the removed set is the same either way, and `M( /* a⏎b */ 1, 2)` is not reported under both. I recommend leaving the spec as is. The one pre-existing disagreement is a follow-up candidate (below).

## Issue goals

- "Today's output: the fix splits the list": a stale statement, false at filing. Drop it, and state the real defects in the PR. No visible change.
- "Done when: each of `M(⏎1, 2)` and `M(1,⏎2)` has a covering test": keep it, but `M(1,⏎2)` is the not-reported shape on `main` and after, so it is no counterpart. Add the reported counterpart `M(⏎1, 2,⏎3)`, as the spec author already did. No visible change.
- "Affected: RH5102 analyzer and its code fix; RH5102-004": the code fix and RH5102-004 do not change. Only the analyzer and RH5102-001 change. No visible change beyond the decided one.
- "Answer recorded in the RH5102 spec, with a change doc": keep.

## Architecture findings

_None._ On `main`, the rule and the formatter each decided "multi-line" for the same list from different facts (parentheses against elements). That disagreement is FC-013 and is handled under P11 and SHARED-DIAG-010, not as an architecture finding (`squad/README.md`, *Architecture findings*). The change aligns the rule with RH5109 and with FMT-BREAK-012's element reading.

## Follow-up candidates

1. **RH5102 · Arguments should either all be on one line or each on its own line: a multi-line comment between arguments.** This is pre-existing and not changed by this run. Rare: RH5038 reports the comment.

   Input:
   ```csharp
   M(1, 2, /* a
       b */ 3);
   ```

   Before (`main`, and after this run): reported, no fix offered. The formatter keeps the list as written, because the line break is inside the comment text (SHARED-BLANK-008), so the diagnostic can never be cleared by formatting (SHARED-DIAG-010).

   After, A (recommended): not reported.
   After, B: reported, no fix offered (today).

   Why A: the formatter keeps this layout, and a layout rule must not report what the formatter keeps.

   RH5109 likely has the same case with a parameter list (not run).
2. **Formatter: the space behind a multi-line block comment directly after `(` is dropped.** `M( /* a⏎    b */ 1, 2);` becomes `…b */1, 2);`, while `M(1, /* a⏎    b */ 2);` keeps `b */ 2` (verified with `scripts/format.sh`). Whether the horizontal-spacing spec intends this for the gap `(`…`1` is not checked. Rare: RH5038 reports the comment. A check against `sdd/specs/formatter/horizontal-spacing.md` is the first step.
