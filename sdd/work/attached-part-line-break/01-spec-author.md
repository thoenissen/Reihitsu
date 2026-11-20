Role: spec author
Commit: 268c9bc
Verdict: change doc written

Spec, change doc, and rule page are committed in cd904e8 on the recommended options below. Answers other than the recommendations change RH5115-001, RH5115-004/005, and the rule page; nothing here is built or tested yet.

Prompt note: the lead's summary of the agent comment on the issue was read as context only; every statement below was checked against the specs and the formatter on this commit.

## Files

- `/home/user/Reihitsu/sdd/changes/2026-11/rh5115-index-and-null-forgiving-line.md` — change doc, `Status: Draft`, six visible output changes.
- `/home/user/Reihitsu/sdd/specs/analyzer/RH5115.md` — new spec.
- `/home/user/Reihitsu/documentation/rules/RH5115.md` — new rule page.
- `/home/user/Reihitsu/sdd/specs/README.md` (index row), `/home/user/Reihitsu/sdd/specs/requirement-ids.txt` (RH5115-001 to -006).
- `/home/user/Reihitsu/sdd/specs/formatter/line-breaks.md` — FMT-BREAK-040 Why: "No analyzer rule reports such a part (P11)" became "RH5115 reports such a part." FMT-BREAK-039's matching sentence is unchanged and still true.
- `/home/user/Reihitsu/sdd/specs/shared/fluent-chain-layout.md` — introduction names the RH5115 fix among the chain fixes; the non-goal on rejoining attached parts no longer calls it formatter-only.

## Changed requirements

- RH5115-001 — An index or a null-forgiving operator below its expression is reported — added
- RH5115-002 — A comment or a directive in the gap does not prevent the report — added
- RH5115-003 — The diagnostic is on the part's first token — added
- RH5115-004 — Outside a chain with links, the fix joins the part — added
- RH5115-005 — In a chain with links, the fix lays out the chain — added
- RH5115-006 — A comment or a directive in the gap withholds the fix — added
- FMT-BREAK-040 — Why reworded, behavior unchanged — clarified

Proposed descriptor: ID RH5115, Category Layout, Severity Warning, enabled by default, Fix All, title "Index and null-forgiving operator must stay on the line of their expression". The code owners in the spec are the type names the implementer must use: `RH5115IndexAndNullForgivingOperatorMustStayOnExpressionLineAnalyzer`, `RH5115IndexAndNullForgivingOperatorMustStayOnExpressionLineCodeFixProvider`. The diagnostic message and the resources are not written in the spec, as usual. `Release.ruleset` lists every rule, so it needs an RH5115 entry. That is the implementer's job; I did not touch it.

## Examples for the tests

RH5115-001 (both blocks), RH5115-002, RH5115-003, RH5115-004, RH5115-005; RH5115-006 uses RH5115-002's example. The rule page's Violation/Correction is checked by `RuleDocumentationExampleTests`.

How I checked them: I stripped the markup from each of the 8 spec examples and ran them through `scripts/format.sh`. Every input parses. The formatter joins every reported part as RH5115-004/005 state. The outputs of RH5115-004 and -005 are stable under formatting. RH5115-002's input is refused as unsupported (FMT-UNSUP-014), which is why the fix is withheld there. In the negative block of RH5115-001, the formatter joins `s⏎!.Length` (a link, RH5112's) and `Foo⏎(1)` (FMT-BREAK-039). Neither is RH5115's.

`scripts/test.sh --project architecture` fails 3 tests, all because the rule does not exist yet: `AnalyzerSpecHeaderMatchesItsRule` (no rule RH5115), `EveryCodeOwnerNamesAnExistingType` (the two code owners), and `EveryRequirementOfAnEnforcedSpecIsCovered` (RH5115-001 to -006). The `Uses` and ledger checks pass.

## Output questions

All of RH5115 is a new rule (escalation 5), so each visible output of the change doc needs the maintainer's answer. Four questions:

**1 · RH5115 · new rule, scope per part.** The issue covered `[`, `?[`, `!`, and `(` only in chains with a link (plus `?[` everywhere). Since FMT-BREAK-040, the formatter joins `[`, `?[`, and `!` everywhere. FMT-BREAK-039 joins every argument list's `(`, and the separate candidate thoenissen/Reihitsu#1827 (milestone 1.10.0, "whether wanted is decided in that run") proposes a rule for that parenthesis.

Input:
```csharp
var a = arr
    [0];
var b = this.Foo
    (4);
var c = Foo
    (4);
var d = x.B()
    [0]
    .C();
```
Before (`main`): nothing reported.
- A (recommended): `[`, `?[`, `!` everywhere; no argument list. Reported: the `[` of `a`, the `[` of `d`. `b` and `c` are left to the argument-list candidate.
- B: A plus an invocation's `(` in a chain with at least one link. Reported: `a`, `b` (`this.Foo` has a link), `d`; not `c`.
- C: the issue as written. Every part only in a chain with a link, `?[` everywhere. Reported: `b`, `d`; not `a`, not `c`.
- D: every part of FMT-BREAK-039 and FMT-BREAK-040, everywhere (absorbs #1827). Reported: `a`, `b`, `c`, `d`, and also `new List<int>⏎(4)` and `: base⏎(1)`.

Why A: one rule per formatter requirement, no `this.Foo⏎(4)` versus `Foo⏎(4)` split, and the argument-list candidate stays the maintainer's separate decision. If the maintainer wants that candidate anyway, D is better than building it as a second rule later.

**2 · RH5115 · comment or directive in the gap.** The issue asked not to report such a gap, because the formatter used to keep it. The formatter now refuses it (FMT-UNSUP-014), and SHARED-DIAG-010 has a layout rule report such a shape, with no fix.

Input:
```csharp
var a = arr // first
    [0];
```
Before: not reported.
- A (recommended): reported on `[`, no fix offered.
- B: not reported (the issue's goal; this needs an exception to SHARED-DIAG-010, like RH5111's and RH5419's).

Why A: SHARED-DIAG-010 decides it unchanged, and the analyzer-only user learns that the code needs a change the formatter will not make.

**3 · RH5115 · the fix in a chain with links.**

Input:
```csharp
var e = x
    [0].B()
    .C();
```
Before: not reported.
- A (recommended):
```csharp
var e = x[0].B()
            .C();
```
  (lays out the whole chain, as the RH5112/RH5201 fixes do).
- B:
```csharp
var e = x[0].B()
    .C();
```
  (deletes only the gap; `.C` is no longer under `.B`, and RH5201 then reports it).

Why A: the join moves the chain's anchor, and only the chain layout keeps the links aligned in one application. Outside a chain with links, both options give the same output (`arr[0]`, `n?[0]`, `s! + ""`).

**4 · RH5115 · rule identity and page.** ID RH5115, title "Index and null-forgiving operator must stay on the line of their expression", Warning, enabled by default, Fix All. The new page shows the violation `return values⏎    [0];` and the correction `return values[0];`. Also: like RH5105, the rule reports nothing in a file with a syntax error (SHARED-JOIN-002), because the formatter leaves such a file alone.

## Internal choices

- A new rule, not an extension of RH5201. The alternative was to fold the check into RH5201. RH5201 reports links, and an attached part is never a link (SHARED-CHAIN-001); P11 forbids widening a rule to a construct it only roughly fits.
- The ID comes from the line-break range (RH51xx, next to RH5105 and RH5113). The alternative was RH52xx (alignment).
- Location: the part's first token, so `?` for both gaps of a conditional element access, with one diagnostic per part. The alternative was the `[` for `n?⏎[0]`. This follows RH5201-004's first-token-of-operator rule.
- The spec uses SHARED-JOIN-002 but not SHARED-JOIN-001, which is the opposite of RH5115-002. SHARED-DIAG-010 decides that.
- The chain or no-chain split for the fix is stated by SHARED-CHAIN-001's model ("an attached part of a chain with at least one link"), not by syntax kinds.
- The rule page names the realignment in wrapped chains in one clause under "How to fix it". The alternative was to leave it out. The clause is the normal case for chains, not a corner case.

## Follow-up candidates

- thoenissen/Reihitsu#1827 should get a comment from the issue manager stating that this run left all argument lists' `(` to it (if answer 1A stands), so its overlap note with this issue ("whichever is built second has to state which one reports it") is settled. Under 1D it is covered by this run instead.
