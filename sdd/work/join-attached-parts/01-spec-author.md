Role: spec author
Commit: 4cc58bc
Verdict: change doc written

## What main does today (established with `scripts/format.sh` and `scripts/apply-fix.sh RH5302` on fixtures)

| Case | Outside a chain with links | In a chain with links |
|---|---|---|
| Argument list `Foo⏎(1)`, `class D(int a) : B⏎(a, 2);` | joined (FMT-BREAK-039) | joined (FMT-BREAK-039) |
| Element access `arr⏎[0]`, `this⏎[0]`, `d⏎["k"]⏎[0]`, `(arr)⏎[0]`, `arr⏎[0] = 1` | NOT joined; `[0]` at the statement's column | joined |
| Conditional element access `n⏎?[0]`, `n?⏎[0]` | joined (`ChainLineBreakRewriter.JoinConditionalElementAccesses`), not stated by any requirement | joined |
| Null-forgiving `Get()⏎!`, `s⏎! + ""` | NOT joined; `!` at the statement's column | joined, not stated by FMT-BREAK-020 |
| Element access directly in a list element `Foo(arr⏎[0], 2)` | joined before the list is laid out → one line | same |
| `!` directly in a list element `Foo(s⏎!, 2)` | not joined, list one-per-line | joined after the list → `Foo(x.ToString()!,⏎2)` |
| `?[` in a list element `Foo(n⏎?[0], 2)` | joined after the list → `Foo(n?[0],⏎2)` | — |
| Part in an interpolation hole of a list element `Baz($"{x.ToString()⏎[0]}", 2)` | not joined | joined after the list → two lines |
| Conditional `c ? arr⏎[0] : 1` | not joined, conditional wrapped one branch per line | joined, conditional judged as one line |
| Collection `[arr⏎[0], 2]` | not joined, one element per line | joined, one element per line |
| Part kept by a comment/directive/disabled text | not joined; statement's column | not joined; column of the chain's anchor |

So of the issue's cases only the argument list is done; element access and the trailing `!` remain, and `?[` changes only in how a list counts it.

## Changed requirements

- FMT-BREAK-040 — An element access or a null-forgiving operator starts on the line of the code in front of it — added (sdd/specs/formatter/line-breaks.md, end of file; registered in sdd/specs/requirement-ids.txt).
- FMT-BREAK-020 — retitled "Split member names are rejoined, and split attached parts are no wrap" — changed: the attached-part join moves to FMT-BREAK-039/FMT-BREAK-040; a line break in front of an attached part still does not count as a chain wrap.
- FMT-BREAK-015 — changed: names the FMT-BREAK-040 gaps among those it does not join.
- FMT-BREAK-031 — changed: lists the FMT-BREAK-040 join among the minimal-separator joins (no output change).

Change doc: sdd/changes/2026-11/attached-part-below-element.md (Status: Draft), visible output changes 1–5 (element access and `!` joined outside chains; conditional judged after the join; lists judged as the line they become, including three cases `main` already joins after the list; collection expressions keep one element per line; RH5302 code-fix results). No rule page changes.

Checks: every spec example was run through `scripts/format.sh`: the FMT-BREAK-040 *after* blocks are stable under formatting on `main`, and the inputs format on `main` to the *before* the change doc lists. The change doc's *before* blocks are `main` output; *after* blocks are `main`'s output of the hand-joined input, except change 4 (precedent: the in-chain split and FMT-BREAK-039). `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: FMT-BREAK-040 has no covering test yet (expected; the test implementer adds it).

## Examples for the tests

- FMT-BREAK-040: both examples (basic joins incl. blank line and assignment target; list/conditional/collection counting). Plus each change-doc case: `this⏎[0]`, `d⏎["k"]⏎[0]`, `(arr)⏎[0]`, `n?⏎[0]`, lambda body, interpolation hole outside a list, query clause and array initializer in a list element, the in-chain `Baz(x.ToString()⏎!, 2)` and `Baz($"{x.ToString()⏎[0]}", 2)`, the RH5302 fix inputs, the qualifier counter-case `s⏎!.Length` (a link, not decided here), and the FMT-BREAK-001 guard (comment, block comment, `#if`) — not joined.
- Existing tests tagged FMT-BREAK-020 that cover an attached-part join in a chain (`ChainLinkLayoutTests`, `ChainGapJoinTests`) now cover FMT-BREAK-040.

## Output questions

**1 · Formatter · element access or `!` kept below its element by a comment** (the issue's goal "a part kept on its own line by such trivia goes to the root column")

Input:
```csharp
var a = arr // first
    [0];
var b = list.Count.ToString() // first
    [0];
```

Before (`main`):
```csharp
var a = arr // first
[0];
var b = list.Count.ToString() // first
                  [0];
```
(`[0]` at the statement's column outside a chain; at the chain anchor's column in a chain. Same for `!` and for an argument list kept by a comment, `Foo // c⏎(1)`.)

After, A (recommended): unchanged from `main`.

After, B:
```csharp
var a = arr // first
        [0];
var b = list.Count.ToString() // first
                  [0];
```

Why A: a comment between an element and its attached part is rare, the join already removes the line for every other input, and B is an indentation change of its own (it would also apply to an argument list kept by a comment, which FMT-BREAK-039 left as today). Requirement IDs: FMT-BREAK-001, FMT-BREAK-040, FMT-INDENT (no requirement decides this column today). The FMT-BREAK-040 spec has no guard example yet, so it does not lock this column; add one after the answer.

The visible output changes 1–5 of the change doc are for the lead to ask as listed.

## Internal choices

- New requirement FMT-BREAK-040 as the one owner of the gaps, modeled on FMT-BREAK-039, rather than widening FMT-BREAK-020 (which sits in the chain section and would keep two statements of the join).
- FMT-BREAK-040 references FMT-BREAK-039's counting rules instead of restating them.
- FMT-BREAK-015 keeps the word "postfix" among operator wraps it leaves alone; only the FMT-BREAK-040 gaps were added.
- The issue prompt's owner hint (`JoinConditionalElementAccesses`, `NormalizeChain`) was ignored; no code owner was added to the spec header.

## Follow-up candidates

- Spec wording: FMT-BREAK-015 says postfix operator wraps are owned by FMT-BREAK-016 to FMT-BREAK-020, but none of those states a postfix `++`/`--` (`Foo(i⏎++, 2)`); output on `main` not checked. Internal (spec ownership), not a visible-output question unless the output turns out wrong.
- Analyzer counterpart for a line break in front of an attached part (the issue mentions it): a new rule, out of scope (P11).
