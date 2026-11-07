Role: spec author
Commit: d7c5d39d
Verdict: #1279: spec written; #1281: spec written; #1257, #1282, #1417, #1422: nothing to do

Class-ending narrowing after audit 6, lead rows 46–48 (`00-lead.md`) for auditor findings 20–22 (`38-auditor.md`). Bug-fix lane: each change records a deliberate gap in a fix's coverage or report (AGENTS.md, *Code-fix coverage*), decided by the lead under the maintainer's standing instruction; no output question raised. No change doc. No rule page changed (`documentation/rules/RH3004.md`, `documentation/rules/RH3106.md`): a withheld fix and a corner-case report are not normal-case changes. My prompt named the outcomes rows 47–48 decided; those are the lead's recorded decisions, which I followed except for one wording point of row 48 (see *Internal choices*), and it carried nothing else to set aside.

## Changed requirements

- RH3004-009 — A directive or disabled text in the anonymous method withholds the fix — changed (row 47), retitled from "A conditional directive whose chain crosses the anonymous method withholds the fix". Both paths: any preprocessor directive of any kind, or any disabled text, from the anonymous method's first token (`async` or `delegate`) through the block's `}` — parameter list, between list and block, or block. A directive wholly outside the anonymous method does not withhold. Closes finding 21 (a)–(d).
- RH3004-008 — A string or a comment that spans lines withholds the fix in a block body — changed (row 47), retitled from "A string that holds a directive line in some build withholds the fix". Block-body path only: a string literal of any kind (regular, verbatim, interpolated, raw, UTF-8; span with holes and `u8`) or a delimited comment (`/* */`, `/** */`) that starts on one line and ends on a later one. Closes finding 20 (a)–(d) without deciding on `#` or on whitespace.
- RH3004-007 — A multi-line raw string literal withholds the fix only in a block body — changed, retitled from "A multi-line raw string literal in a block body withholds the fix". Its block-body case is now withheld by RH3004-008, which the text points to; the expression-body and one-line cases stay its own.
- RH3004-006 — The lambda is laid out as the formatter lays it out — clarified (simplified). The line rule now covers only what a block the fix keeps can hold: a line stays only when it holds only comments and its first comment starts in column 0 (`///` lines by their own marker); every other line moves. Removed: string-continuation lines, directive lines, disabled text and statements in disabled text, comment-continuation lines, and the "independent of preprocessor symbols" sentence, all now withheld by RH3004-008/-009.
- RH3004-003 — withholding list drops RH3004-007 (it no longer withholds on its own) — clarified.
- RH3106-003 — deliberate gap restated by outcome (row 48) — clarified, no new ID.

No ID added or retired; `sdd/specs/requirement-ids.txt` unchanged.

## Examples for the tests

- RH3004-009, withheld: `#if DEBUG`/`#else`/`#endif` in column 0 inside the block (with and without `DEBUG`; the old RH3004-006 example, the departure from answer 7); finding 21(a) param-list chain with `#define A`; an expression-body block with `#if A`/`#else`/`#endif` inside the parameter list; with `#define A`, the old chain crossing `delegate`, and finding 21(d) `#region` before `delegate` with `#endregion` in the block; `#region` plus `#pragma warning disable/restore` inside the block. Offered: chain wholly outside the anonymous method, exact output in the spec.
- RH3004-008, withheld: finding 20(a) (multi-line comment, chain outside, `#define A`); finding 20(d) (form feed `\f` before `#else` in a verbatim string, `#define A`); with no directive in the document, `@"a⏎b #c"` plus `/* first⏎second */`, `$@"Hello⏎{name}"`, and a multi-line raw string in a block body. Offered: a block with one-line strings (`"Hello " + name`, `$"{name}!"`) and one-line comments (`// …` at column 12, trailing `/* done */`), exact output in the spec; and `return @"a⏎#b";` as an expression body (existing test).
- RH3004-007: unchanged examples (block raw string now withheld by RH3004-008; expression-body raw string and one-line raw string offered).
- RH3106-003, reported with no fix: the three existing invocation shapes plus finding 22's `[0].BeginInvoke(null, null)`, `Action x = …[0]`, `Func<int> g = …[0]`, `y = …[0]`, `Run(…[0])` with `Run(Action)` alone, `Action[] arr = new[] { … }`, `IEnumerable<Action> e = new[] { … }`. Not reported: the existing `.Length`, `[0].Method`, `[0] is Action`, `var` and `for` initializers, `new[] { delegate () { }, new object() }`, plus the auditor's observed-holding `[0].DynamicInvoke()`, `object o = …[0]`, `Delegate d = …[0]`, which no test covers yet.

## Existing examples whose outcome changes from fixed to withheld

From the old RH3004-006 (removed from it; the first is now an RH3004-009 example):
1. the `#if DEBUG`/`#else`/`#endif` block with directives in column 0;
2. the `#region Output` block with indented `#if DEBUG`/`#endif`;
3. the block whose first statement is in a `#if DEBUG` branch and `Console.WriteLine(-x);` stands at column 16;
4. the two multi-line comments `/* Console.WriteLine(x + 1); … */` (column 0) and `/* Console.WriteLine(x + 3); … */` (column 12);
5. the `$@"Hello⏎{name}⏎";` / `$@"Hello {⏎name} done";` block;
6. the `Console.WriteLine(⏎@"a⏎b");` block (a line on which a string starts);
7. the shared-brace-line `{ var s = $@"Hello⏎{name}";` block;
8. the `#if DEBUG` block with `$@"Hello⏎{name}");` (disabled-text string).

From the old RH3004-008: the offered `Console.WriteLine(⏎@"a⏎b #c");` example (now withheld, used as a withheld example) and its statement that the `DEBUG` example of RH3004-006 is offered.

Tests that assert those outputs flip to `VerifyNoCodeFixIsOffered`, and so does every other RH3004 test that expects a fix for a block body holding a directive, disabled text, a string over several lines, or a delimited comment over several lines (among them the two "`#` behind text" tests, the region/directive/disabled-text layout tests and the multi-line string and comment layout tests tagged RH3004-006). Tests of withholding tagged RH3004-008/-009 stay withheld; their tags follow the new split (directives and disabled text → RH3004-009; strings and comments over several lines → RH3004-008).

## Output questions

_None._ The standing instruction applies.

## Internal choices

- Row 47's two conditions map onto the existing IDs by reason: directives and disabled text (both paths) → RH3004-009; strings and comments over several lines (block path) → RH3004-008. Nothing retired: the two have different reasons and paths. RH3004-007 is kept for its expression-body side and points to RH3004-008 for the block. Alternative: retire RH3004-007 into RH3004-008; rejected because its expression-body statement is a requirement of its own (seven tests tag it).
- "A comment that spans lines" is read as a delimited comment, `/* */` or `/** */`. Consecutive `///` lines, which Roslyn may parse as one trivia, are decided per line by their own marker (the code already does this), and no line of them can start with `#`. Alternative: withhold for a `///` run as well; rejected as giving away safe ground.
- RH3004-009's span starts at the first token, `async` or `delegate`; row 47 says "from `delegate`". RH3004-004 already withholds for a directive between `async` and the parameter list, so the outcomes are equal.
- RH3106-003: row 48 says "every other array-element position is reported with the fix withheld". The auditor observed `[0].DynamicInvoke()`, `object o = …[0]` and `Delegate d = …[0]` as not reported, so that sentence would be false for them, and other unprobed positions could be the same. The gap therefore says: not reported in the listed shapes (MUST NOT, tested or observed), and everywhere else the list MAY be reported, and where it is, no code fix is offered. Spec and code cannot disagree on any member, which is what row 48 asks for. Alternative: row 48's literal wording; rejected because it would specify reports the code does not make.

## Follow-up candidates

_None new._ The RH3004 structural fix (build the lambda from the original nodes, which would let directives and multi-line strings be rewritten) and the RH3106 consuming-expression probe are already listed in `00-lead.md`; the RH3004 one should now name the whole RH3004-008/-009 gap, and the RH3106 one conversions and arguments as well (auditor's suggestion under #22).

## Whole run

- Files: `sdd/specs/analyzer/RH3004.md`, `sdd/specs/analyzer/RH3106.md`.
- `scripts/test.sh --project architecture`: 115 passed, 0 failed. No requirement lost all of its tests, so no coverage failure; tests whose expectations now contradict the spec are the test implementer's (list above).
