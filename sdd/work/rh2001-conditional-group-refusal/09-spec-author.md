Role: spec author
Commit: a05bb8c (statements derived from it)
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It routed auditor finding #2 to me (lead decision 6), and this handover covers only that finding. Finding #1 goes to the test implementer.

## Changed requirements

None. RH2001-010 and the other specs are unchanged: the boundary text already covers both shapes ("whether it is the group's `#if` or only its `#endif`"; any conditional directive in the leading trivia of the first modifier).

Change doc `sdd/changes/2026-11/rh2001-conditional-group-refusal.md`, *Visible output changes* (auditor finding #2):

- Section 2 gains shape (a): `#if NET` stands in front of an earlier member `_a`, and only the `#endif` stands behind `private int B { get; }`, with `NET` defined. Input, `main` output, and after are given. The section notes that a property in the middle of such a group keeps the fix.
- Section 3 gains shape (b): the property is the active `#else` branch (`#if NET` / `private int _a;` / `#else` / `private int B { get; }` / `#endif` / `private int _d;`, without `NET`). Input, `main` output, and after are given.
- *Unchanged* now also names a group around several members with a member of the group in front of the property and one behind it.
- *Decisions*, "Directly in front of ...": the example list gains the `#endif`-alone-behind case next to its mirror.
- `Status` stays `Accepted`. Both shapes follow from maintainer answer A and RH2001-010 (lead decision 6), so no output question is new.

I ran both inputs through `scripts/apply-fix.sh RH2001`, under LF and CRLF:

- On `origin/main` (`eed7195`, in a temporary worktree), each is fixed with exactly the *Before* output in the doc.
- On the head, each gets no fix.

## Examples for the tests

RH2001-010 shape (a) of finding 1 is section 2's new input. The test implementer pins it under finding #1. Shape (b) is section 3's new input, and the test implementer may pin it with the same tag. It is a fourth member of the behind-side axis: an `#else` in front and an `#endif` alone behind.

## Output questions

_None._

## Internal choices

- I put shape (a) in section 2 rather than section 3. Its only directive next to the property is the `#endif` behind it, so it matches section 2's heading ("behind a property without an initializer"). Section 3 is about groups that wrap the property itself. The alternative was section 3.

## Follow-up candidates

_None._ The auditor's two candidates are already recorded in `00-lead.md`.
