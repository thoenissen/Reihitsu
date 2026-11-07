Role: spec author
Commit: 64e7b475
Verdict: #1279: spec written; #1281: spec written; #1257, #1282, #1417, #1422: nothing to do

Final narrowing round, lead rows 44–45 (`00-lead.md`) for auditor findings 17–19 (`32-auditor.md`). Bug-fix lane: each change records a deliberate gap in a fix's coverage or report (AGENTS.md, *Code-fix coverage*), decided on recommendation under the maintainer's standing instruction; no output question raised. No change doc. No rule page changed (`documentation/rules/RH3004.md`, `documentation/rules/RH3106.md`): a withheld fix and a corner-case report are not normal-case changes. My prompt named the outcomes rows 44–45 decided; those are the lead's recorded decisions, which I followed, and it carried nothing else to set aside.

## Changed requirements

- RH3004-008 — A string that holds a directive line in some build withholds the fix — changed (row 44 (b)). Retitled from "A string that disabled text leaves open withholds the fix". Two items, block-body path only: (1) a string of active code holding a line, after its first, whose first non-whitespace character is `#`, whatever directives stand near it; (2) disabled text that leaves a string open (unchanged). Item (1) replaces the earlier active-branch wording ("an active branch inside the block … under symbols that turn that branch off"), which it subsumes: a string that would be left open under other symbols always spans a directive line. It is wider: it also withholds a string holding a `#` line in no branch at all, and one in a branch opened before the anonymous method (finding 17).
- RH3004-009 — A conditional directive whose chain crosses the anonymous method withholds the fix — added (row 44 (a), finding 18 and 17(a)). A `#if`/`#elif`/`#else`/`#endif` inside the block whose chain has a directive before `delegate` or after the block's `}`. Both paths (a block holding a directive never takes the expression-body path). Registered in `sdd/specs/requirement-ids.txt`.
- RH3004-003 — withholding list gains RH3004-009 — clarified.
- RH3004-006 — the disabled-text sentence of the first "stays" bullet also points to RH3004-008's `#` line item — clarified.
- RH3106-003 — deliberate gap added (row 45, finding 19): an element of an implicitly typed array creation whose delegate is invoked after an element access in the same expression, directly or through `Invoke`, is reported with no code fix. Clarified, no new ID.

## Examples for the tests

- RH3004-008, withheld: the existing `#if A`/`#elif B` example (no symbols, `A`, `B`); audit 17(b) with `#define A` (chain wholly outside, block holds no directive); `Console.WriteLine(@"a⏎#b");` with no directive anywhere. Offered: `Console.WriteLine(⏎    @"a⏎b #c");` (`#` behind text) with its exact output; the `DEBUG` string example of RH3004-006.
- RH3004-009, withheld: audit 18 input 1 with `A`; audit 18 input 2 (branches swapped) with no symbol; audit 17(a) with `#define A` (also RH3004-008). Offered: `#if A` / `Action a = delegate () { Console.WriteLine(1); };` / `#else` / … / `#endif` with `#define A`, exact output in the spec; the `DEBUG` chains of RH3004-006. **`RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails until a test tagged `[Requirement("RH3004-009")]` exists.**
- RH3106-003, reported with no fix: `var r = new[] { delegate () { return 1; } }[0]();`, `new[] { delegate () { } }[0]();`, `new[] { delegate () { } }[0].Invoke();`. Not reported: `.Length`, `[0].Method`, `[0] is Action`, and the existing `for` example with `arr[0]()` in the incrementor.

## Existing examples whose outcome changes

None of the spec's examples changes outcome. The RH3004-008 example stays withheld (now by item 1 with `A`, item 2 otherwise). No RH3004-006 or RH3004-007 example holds a `#` line in a string or a chain crossing the anonymous method. The RH3004 test class has no test of a `#` line in a string outside a branch; the implementer's earlier probe of that shape (`32-auditor.md`, "Holding") was offered and is now withheld — no spec example or test named it. RH3106: no example changes; finding 19's inputs were never spec examples.

## Output questions

_None._ The standing instruction applies.

## Internal choices

- Row 44 (a) is a new requirement, RH3004-009, not folded into RH3004-008: its reason differs (the lambda's text is not a complete directive chain, so neither layout nor safety can be decided), while (b) and the disabled-text case share RH3004-008's reason (a string's lines differ by build). `sdd/README.md`: two decisions with different reasons are two requirements.
- RH3004-008 item 1 stays on the block-body path, like the rest of RH3004-008: the expression-body path moves no line itself, and the auditor found it holding. Alternative: withhold on both paths, as row 44 words it without a path; rejected as giving away safe ground.
- RH3106-003's gap names invocation after an element access (direct or `Invoke`), not element access alone: the auditor probed `[0].Method` and `[0] is Action` as not reported, so element access by itself does not trigger it. Alternative: row 45's wording "invocation, element access, or `Invoke`"; rejected because it would read as covering the holding shapes.

## Follow-up candidates

_None new._ The two general cases are already listed in `00-lead.md`.

## Whole run

- Files: `sdd/specs/analyzer/RH3004.md`, `sdd/specs/analyzer/RH3106.md`, `sdd/specs/requirement-ids.txt`.
- `scripts/test.sh --project architecture`: 114 passed, 1 failed — the coverage test for the new RH3004-009, as expected before its test exists.
