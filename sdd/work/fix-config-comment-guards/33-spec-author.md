Role: spec author
Commit: 6f0a875
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's batch 3 answer and decision 32 of `00-lead.md`, which it named as the work.

Change doc: `sdd/changes/2026-11/member-move-conditional-groups.md` (Status: Draft). Its *Visible output changes* were measured with `scripts/apply-fix.sh` on a temporary worktree at `57acf0d` (LF and CRLF the same; worktree removed).

## Changed requirements

- SHARED-FIX-009 — A fix does not work out another build configuration — added (`sdd/specs/shared/code-fixes.md`). The maintainer's principle for every fix: no condition evaluated, no branches of several groups combined, nothing assembled from parts several branches write; where only that could show safety, the fix is withheld. Excluded code may be read whole (members, statements, names), counted as present, so the reading can only withhold. Each rule's spec says which directives withhold its fix.
- SHARED-ORDER-008 — Only groups of whole members or whole statements keep a member move — added. Applies to RH7102, RH7103, RH7109, RH7309. Defines *group of whole members* (directives between members, active branch whole members, each excluded branch reads as whole member declarations, no nesting) and *group of whole statements* (inside the body block of a method, constructor, destructor, operator, conversion operator, or accessor; whole statements; no nesting; no `field` keyword in an accessor's branch). Every other conditional directive in the moved or crossed span refuses the move. Examples on both sides.
- SHARED-ORDER-007 — Initializer execution order is kept — changed: back to main's text, plus what the run keeps (all member reordering fixes incl. RH7102 apply both guards, decisions 4/5/12). Removed everything decisions 21–31 added: per-configuration judgment, symbol assignments, membership condition, split-member paragraph, the 256-configuration limit and their examples (split `_b`, `!DEBUG static`, `K`, issue #1593's `X`/`Y`, `_t`/`_b`, tc1, nt2, sg1/sg1i/cr1/sg1t, far9, cap2). New paragraph: every member of an excluded branch counts as present in every build; branches that exclude each other both count (new example, no fix). The examples for split members, nesting, and type-closing branches moved to SHARED-ORDER-008 with the outcome "no fix offered".
- SHARED-ORDER-006 — Balanced directive pairs travel intact — changed: the move stays available only when SHARED-ORDER-007 and SHARED-ORDER-008 do not refuse it, for a member reordering fix. Intro of the spec names RH7309 for 007/008; header `Uses` adds SHARED-FIX-009.
- RH7102-006 — changed: adds SHARED-ORDER-008; the split-crossed-declaration sentence now says SHARED-ORDER-008 refuses the move; `Why` reworded. `Uses` adds SHARED-ORDER-008.
- RH7103-006 — changed: adds SHARED-ORDER-008 (and `Uses`).
- RH7109-006 — changed: adds SHARED-ORDER-008 (and `Uses`); "When no `#if` … takes part in the refusal, the diagnostic is still reported" → "The diagnostic is still reported" (as decision 12 did for RH7102).
- RH7309-005 — changed: the fix is also withheld when SHARED-ORDER-008 refuses the reorder; a balanced pair keeps it only when SHARED-ORDER-008 keeps it. `Uses` adds SHARED-ORDER-008.
- `sdd/specs/requirement-ids.txt`: SHARED-FIX-009, SHARED-ORDER-008. `sdd/specs/README.md` unchanged (no new code owner). No rule page changed: only withheld fixes in corner cases.

Unchanged: RH7102-001/RH7103-001 (decision 16's analyzer exclusion), all of RH2001 (#1616).

## Examples for the tests

- SHARED-ORDER-008, no fix offered: initializer split (`_b #if DEBUG = 1 #endif ;`, RH7109 — also SHARED-FIX-009's example), `K` modifiers split (RH7109), issue #1593's `X`/`Y` region block (RH7309), nested `#if FEATURE`/`#if DEBUG` in `_b`'s leading trivia (RH7109), tc1 and nt2 (RH7109), accessor `#if TRACE return field; #else return 0; #endif` in struct `S` (RH7103).
- SHARED-ORDER-008, fix offered: `#if DEBUG` around the whole method `Trace` in `Create`'s leading trivia (RH7103, exact output in the spec = main's output); accessor variant with `return 1;` instead of `return field;` (RH7103, offered); SHARED-ORDER-006's first example and RH7309-005's second example (group of whole statements; existing tests).
- SHARED-ORDER-007: the main examples unchanged; new: `#if DEBUG _c = Log("c") #else _e #endif` crossed by `_b = Log("b")` (RH7109, no fix).
- Change doc: `#if false` with prose (`Notes: …`) in front of the moved `_b` (RH7109): main offers the fix, now no fix.
- Tests that exist only for the per-configuration reading (the decision-21–31 rows that expect a fix main offers on a split member — sg1, sg1i, cr1, the split `_b`/`_t` row, `K`, the far9 and limit tests, cap2, the nine-symbol bracket tests) now expect "no fix offered" where a conditional directive splits a member or nests, or go away where they only pinned the limit; the test implementer sorts them under decision 32.

## Output questions

**1 · RH7103 · Static elements must appear before instance elements — `#if` block inside a method body**

Input:
```csharp
public class TestClass
{
    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }

    public static void Create()
    {
    }
}
```

Before (`main`):
```csharp
public class TestClass
{
    public static void Create()
    {
    }

    public void Run()
    {
#if DEBUG
        System.Console.WriteLine();
#endif
    }
}
```

After, A (recommended): same as `main`.

After, B: no fix offered; the diagnostic stays.

Why A: an `#if` inside a method body changes no initializer, static/instance context, or field layout, so the fix stays correct in every build without reading any configuration; B would withdraw a fix main specifies (SHARED-ORDER-006, RH7309-005) for the most common use of `#if`.

Background for the lead (not for the maintainer): the batch 3 answer's words "an `#if` inside a member" read literally give B; decision 32's "a refusal for every other conditional directive" too. I wrote the spec on A (group of whole statements in SHARED-ORDER-008), with the `field` keyword in an accessor's branch withholding (it adds a backing field in some builds). Under the standing answer A applies; if the lead reads the maintainer's answer as already deciding B, the change is local: drop the *group of whole statements* definition from SHARED-ORDER-008, retire the method-body examples of SHARED-ORDER-006 and RH7309-005 (each becomes "no code fix is offered"), and list them in the change doc.

## Internal choices

- The principle is a new requirement SHARED-FIX-009, not a bullet under SHARED-FIX-003: SHARED-FIX-003 says what a fix must not break; SHARED-FIX-009 says how a fix may not decide it. Alternative: extend *Build configurations*; rejected as it would mix a safety condition with a method restriction.
- The member-move refusal is a new requirement SHARED-ORDER-008, not part of SHARED-ORDER-007: it is a directive guard like SHARED-ORDER-003/005 and applies whether or not any initializer is involved. Alternative: one paragraph in 007; rejected (one requirement per heading).
- An excluded branch whose text does not read as whole members (`#if false` with prose) withholds the fix. It follows from the answer's "not a group of whole members"; main offers the fix; listed in the change doc.
- Unmeasured on main, stated only in the spec: the tc1/nt2 outcome on main is "fix offered" per `31-test-implementer.md`; they are SHARED-FIX-003 bug fixes and not in the change doc's list.

## Follow-up candidates

- Other fixes than the member-moving ones were not audited against SHARED-FIX-009. A search of `src` at `57acf0d` (excluding tests) for `BranchTaken`, `ConditionValue`, `PreprocessorSymbolNames`, `WithPreprocessorSymbols` finds nothing, so no fix evaluates conditions today; whether one assembles code across branches was not checked.
- The layout-from-a-conditional-attribute candidate of `30-spec-author.md` stays open: a `#if X [StructLayout(Sequential)] #endif` on the type lies outside both spans, so SHARED-ORDER-008 does not reach it.
