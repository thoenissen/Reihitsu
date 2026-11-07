Role: spec author
Commit: 9c326aa4
Verdict: #1279: spec written; #1281: spec written (example only); #1257, #1282, #1417, #1422: nothing to do

Architecture changes after audit 4, lead rows 40–41 (`00-lead.md`) for auditor findings 14–15 (`26-auditor.md`). Bug-fix lane: RH3004-006 and RH3106-003 are clarified; RH3004-008 is a new requirement that states a withheld fix, decided on recommendation under the maintainer's standing instruction (no output question raised). No change doc, no rule page change (a withheld fix in a corner case; the normal case is unchanged). My prompt named the change row 41 asks for; I followed the rows, not a hoped-for verdict, and found nothing to set aside.

## thoenissen/Reihitsu#1279

### Changed requirements

- RH3004-006 — The lambda is laid out as the formatter lays it out — clarified (row 41, finding 15). The first "stays" bullet now decides on the whole string, never on its tokens: a string literal or interpolated string expression, its span from its first character (`@`, `$`, or the opening quote) through its closing quote and a `u8` suffix, holes included. A line stays when its first character lies inside that span, after the line the string starts on: also a line starting with a hole's `{`, a line inside a hole that spans lines, and a line starting with the closing quote. Nested strings belong to the outermost string. In disabled text, the strings are those that lexing the disabled text between its two directives gives. Four examples added (below).
- RH3004-007 — Why: "A verbatim string literal keeps its continuation lines" became "A verbatim or interpolated string keeps every line that starts inside it" — clarified.
- RH3004-008 — A string that disabled text leaves open withholds the fix — added (row 41, withheld case). Only on the block-body path. A string literal or interpolated string that starts in disabled text and does not end before the directive that ends that disabled text withholds the fix; one that ends there does not. Registered in `sdd/specs/requirement-ids.txt`.
- RH3004-003 — the list of withholding requirements gains RH3004-008 — clarified.

### Examples for the tests

- RH3004-006, new:
  - own-line block (audit inputs (a) and (b) plus a hole line): `$@"Hello⏎{name}⏎";` and `$@"Hello {⏎                name} done";`. `{name}` and `";` stay in column 0, `name} done";` in column 16, every other line moves by 19 (statements at 31).
  - the other side of "after the line the string starts on": `Console.WriteLine(⏎    @"a⏎b");` — the `@"a` line moves (to 27), `b");` stays.
  - shared brace line (audit input (c)): `{ var s = $@"Hello⏎{name}";⏎Console.WriteLine(s);` — `{name}";` stays in column 0 and does not count for the leftmost line; `Console.WriteLine(s);` lands in column 31.
  - disabled text whose string closes: `#if DEBUG` / `Console.WriteLine($@"Hello⏎{name}");` / `#endif` — `{name}");` stays in column 0, without `DEBUG` (the test default) and with it.
  - One of them under CRLF, as the auditor asks.
- RH3004-008, new: `#if A` / `Console.WriteLine(@"Hello` / `#elif B` / `            world");` / `#endif` — reported, no code fix. Its other side is the `DEBUG` example of RH3004-006. **`RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails until a test tagged `[Requirement("RH3004-008")]` exists** — the test implementer's to add.

### Output questions

_None._ The standing instruction applies.

### Decided on recommendation

1. **RH3004 · Use lambda syntax — a line inside a hole of a string that spans lines.** The line stays, keeping its columns (`name} done";` stays in column 16). Alternative: the line moves with the statements, since a hole's text is code and moving it does not change the value. Rejected: it reintroduces a decision on a unit smaller than the string, which is the class row 41 closes, and it would move hole text away from the string text around it that stays.
2. **RH3004 · Use lambda syntax — disabled text that leaves a string open.** No code fix is offered (RH3004-008). Alternative: decide the open string's lines on the disabled text alone (they hold code and move). Rejected: under other symbols those lines are part of the string's value, so the output would depend on the symbols and could change a literal (SHARED-FIX-003).

### Internal choices

- The withheld case is a new requirement, RH3004-008, rather than an extension of RH3004-007: RH3004-007 is about multi-line raw strings in active and disabled code alike, while RH3004-008 is about disabled text that cannot be decided, with its own condition and examples.
- "Can span lines" names verbatim, interpolated (holes spanning lines, C# 11), and raw strings; raw strings stay withheld in a block body by RH3004-007, so the RH3004-006 bullet only refers to it.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1281

Row 40 (finding 14): RH3106-003 already decides it (the first bullet, the outcome condition). The spec had no `for`-header example, so the not-reported side gains one with the auditor's three probed inputs: `for (var a = delegate () { }; a != null; a()) { }` in a method body, the same in a lambda body, and `for (var arr = new[] { delegate () { } }; arr.Length > 0; arr[0]()) { }`. A sentence states that uses of the local outside the declaration, also in the same statement, do not decide it. Clarified, no new ID.

## thoenissen/Reihitsu#1257, #1282, #1417, #1422

Nothing to do. Row 42 (finding 16) is comment-only and the test implementer's.

## Whole run

- Files: `sdd/specs/analyzer/RH3004.md`, `sdd/specs/analyzer/RH3106.md`, `sdd/specs/requirement-ids.txt`.
- `scripts/test.sh --project architecture`: 114 passed, 1 failed — the coverage test for the new RH3004-008, as expected before its test exists.
- No rule page changed (`documentation/rules/RH3004.md`, `documentation/rules/RH3106.md`).
