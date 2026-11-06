Role: spec author
Commit: 09a5cd28
Verdict: #1257: nothing to do; #1279: spec written; #1281: spec written; #1282: nothing to do; #1417: nothing to do; #1422: nothing to do

Repair round 2 after audit 2. It carries out lead decisions 26–29 (`00-lead.md`) for auditor findings 5–8 (`14-auditor.md`). All edits are clarifications in the bug-fix lane. They write down what SHARED-FIX-002, SHARED-FIX-003, SHARED-FIX-007, SHARED-DIAG-011, and maintainer answers 2 and 4 already demand, plus one shape decided on recommendation (row 29). No change doc. No rule page changes: each edit is a withheld fix, a narrowed report in a corner case, or a layout corner case. My prompt restated the lead's decided rows and named no intended fix or verdict beyond them, so I had nothing to set aside.

I compiled every new input with the .NET 10 SDK in a scratch file-based app outside the repository. All of them compile. For RH3106, I also compiled each input with the parameter lists removed, to confirm the errors the spec names:
- `var n = delegate () { return delegate { }; };` gives CS8917.
- `Coll e = [delegate { }];` gives CS8917.
- `c += delegate { };` with two static operators gives CS9342.
- `d += delegate { };` with two C# 14 instance `operator +=` gives CS0121.
- `new Coll { delegate { } }` gives CS0121.
- `var f = Action () => delegate { };` compiles.

## thoenissen/Reihitsu#1257

Nothing to do. Row 27's SHARED-FIX-002 sentence covers RH3005's nested comparison too, and it needs no RH3005 text.

## thoenissen/Reihitsu#1279

### Changed requirements

- SHARED-FIX-002 — clarified (row 27).
  - The old text did cover nested instances, because "every reported diagnostic whose fix is offered on the starting code" includes the inner one. It just did not say so.
  - New sentence: a diagnostic inside code that another fix rewrites or moves is covered like any other. The same application corrects it where that fix left it. Only a guard that withholds its fix on the code the earlier fixes produce leaves it reported.
  - New example: RH3004 nested anonymous methods, both rewritten in one Fix All application. The inner lambda's braces are in column 44 and its statement in column 48. Fixing outer first or inner first gives the same output; I traced both orders against RH3004-006.
- RH3004-003 — clarified. Its opening clause now names the requirements that withhold the fix: RH3004-004, -005, and -007. Before, it named only -005's condition.
- RH3004-006 — clarified (row 29, decided on recommendation).
  - Applies when the block's first statement starts on the line of its opening brace and moved code stands on later lines.
  - If none of those later lines starts left of the first statement in the source, they keep their indentation relative to the first statement.
  - Otherwise they move together until the leftmost of them stands in the statement column, one level inside the braces, and they keep their indentation relative to each other. Blank lines, directive lines that stay in column 0, and lines that keep their columns do not count for "leftmost".
  - The sentence "every line moves by the same number of columns" now names its exceptions.
  - Written down as existing behavior: a line that continues a verbatim string literal keeps its columns (SHARED-FIX-003). The auditor found this holding.
  - The Why gives the reason.
  - Two new examples, one on each side (FC-002):
    - `Run(delegate (int x) { Foo(x);⏎            if (x > 0)⏎                Foo(x + 1);⏎        });` becomes braces at column 12, `Foo(x);` and `if` at 16, and `Foo(x + 1);` at 20.
    - `Action<int> c = delegate (int x) { Foo(x,⏎<47>x + 1);⏎<41>};` keeps the continuation aligned under `x`: `Foo(x,` at 28, `x + 1);` at 32.
    - These columns are for the source with the markup removed.
- RH3004-007 — added (row 28). "A multi-line raw string literal in the block withholds the fix."
  - Covers a plain, interpolated, or UTF-8 multi-line raw string.
  - The diagnostic stays reported.
  - A one-line raw string and a multi-line verbatim string do not withhold the fix.
  - It is recorded as a deliberate gap in coverage (AGENTS.md *Code-fix coverage*). The Why cites SHARED-FIX-003.
  - Examples: a block holding a three-line `"""` literal is reported with no fix; a one-line `"""a"""` is fixed.
  - `RH3004-007` is added to `sdd/specs/requirement-ids.txt`.

### Examples for the tests

- RH3004-006: the shared-brace-line example (`Run`, leftmost-line rule) and the continuation example (`Action<int> c`, relative rule). The auditor's second input, `Action<int> c = delegate (int x) { Foo(x);⏎            Foo(x + 1); };`, follows from the same rule: `Foo(x + 1);` lands at column 28.
- RH3004-007: the multi-line raw string example (no fix) and the one-line raw string example (fixed). The requirement also covers `$"""` and `"""u8` and the expression-body path (`return """…""";`); a test of one of them is worth adding.
- SHARED-FIX-002: the RH3004 nested Fix All example. A nested RH3005 Fix All test (`b = 1 < (1 < x ? 2 : 3);`, finding 6 b) follows from the same sentence.

### Output questions

_None._ The standing instruction applies.

### Decided on recommendation

**RH3004 · Use lambda syntax — first statement on the opening brace's line, later lines indented less** (row 29)

Input:
```csharp
        Run(delegate (int x) { Foo(x);
            if (x > 0)
                Foo(x + 1);
        });
```

Before (`main`): the block is kept as written (`14-auditor.md` #8; not run by me).
At 2284decb: `if` lands in column 2 and is clamped at column 0 in the assignment variant.

After, A (decided, now in RH3004-006):
```csharp
        Run((int x) =>
            {
                Foo(x);
                if (x > 0)
                    Foo(x + 1);
            });
```

After, B: withhold the fix for this shape. The diagnostic stays.

After, C: a floor at the statement column. Every line that would land left of it stops there, which flattens `Foo(x + 1);` onto `if`'s column.

Why A: it keeps the author's layout among the later lines and puts nothing left of the braces. B gives away safe ground for an input the fix can rewrite (AGENTS.md *Code-fix coverage*). C loses the relative indentation that RH3004-006 keeps everywhere else.

### Internal choices

- RH3004-006: the leftmost-line rule applies only when a later line starts left of the first statement. Otherwise the later lines keep the first statement's delta. Alternative: always measure the later lines on their own leftmost line. That would pull a continuation line aligned under an argument (the `Foo(x,` example) out of alignment.
- RH3004-007 is a requirement of its own and not a bullet of RH3004-004. Its condition is a literal, not a comment or directive outside the body, and AGENTS.md wants the gap to read as a decision.

### Follow-up candidates

- RH3004-006 with a negative shift when the first statement starts on its own line. The auditor called this shape holding, with the reasoning "every delta is then positive or keeps the relative layout". But if the source block is indented further right than the statement column, the shift is negative, and a moved line in a shallower column can still fall left of column 0 and be clamped. Example (not run):

  ```csharp
          Run(
              delegate ()
                  {
                          Foo();
  // note
                  });
  ```

  Here the shift is −8 and the comment is in column 0. Recommendation: the same leftmost-line floor as the shared-brace rule. A caller reaches it through the RH3004 code fix. It is outside row 29's shape, so I left it for the lead to route.

## thoenissen/Reihitsu#1281

### Changed requirements

- RH3106-002 — changed title to "The fix is withheld where removing the list changes what the code binds to" — clarified (row 26).
  - The normative sentence is now an outcome: the fix is withheld when, without the list, the code would bind differently or gain a compile error. That covers converting to another delegate type, and a member chosen for the anonymous method (an overload, a user-defined operator, a collection initializer's `Add` method, or an inferred type argument) becoming another one, ambiguous, or not found.
  - It applies wherever the anonymous method stands. The places are now listed only as examples: invocation, object creation, constructor initializer, and element access arguments; binary operands; compound assignment values, with both `static` operators and C# 14 instance `operator +=`; collection initializer elements.
  - The report stays in every such place unless RH3106-003 applies. The Fix All sentence is unchanged.
  - New example block, reported with no fix:
    - `c += delegate () { };` with two static operators;
    - `d += delegate () { };` with two instance `operator +=`;
    - `new Coll { delegate () { } }` with `Add(Action)` and `Add(Action<int>)`.
  - Every existing example is kept unchanged.
- RH3106-003 — clarified (row 26).
  - The normative sentence is now an outcome: the analyzer does not report where the anonymous method takes its natural type. That means no delegate type is its target, it is converted to its own natural type or to a non-delegate type, and no overload, operator, collection initializer `Add`, or generic type inference chooses among candidates for it. The bullets are now examples ("Among the places…").
  - New bullets:
    - the expression returned by an anonymous method, not only a lambda, whose return type is inferred, as in `var n = delegate () { return delegate () { }; };`;
    - an element of a collection expression whose elements are `object`, as for a type that implements only non-generic `IEnumerable`, whatever `Add` methods it declares.
  - Reported side: a lambda that declares its return type gives the target, so `var f = Action () => delegate () { };` is reported. The Why gives the reasons.
  - New examples:
    - not reported: `var n = …` and `Coll e = [delegate () { }];`;
    - reported (added to the delegate-target block): `var g = Action () => delegate () { };` and `var k = Action () => { return delegate () { }; };`.
  - Every existing example is kept unchanged.

### Examples for the tests

- RH3106-002: the new compound-assignment and collection-initializer block. Each line is a separate test (FC-003): the static `+=`, the instance `+=`, and the collection initializer.
- RH3106-003:
  - the new not-reported block, `var n` and `Coll e`;
  - the two new lines in the reported block, `var g` and `var k` with an explicit return type;
  - finding 9 a (`object o = b ? delegate () { } : someAction;` reported) is already demanded by the existing neighbour sentence.

### Output questions

_None._

### Decided on recommendation

_None._ Row 26 decides every RH3106 outcome, together with answers 2 and 4.

### Internal choices

- The collection expression is put under RH3106-003 (not reported), as row 26 puts it. The error without the list is CS8917, from converting to the `object` element, not CS0121 from the two `Add` methods. A collection initializer with the same two `Add` methods is under RH3106-002 (reported, fix withheld), because there `Add` overload resolution decides (CS0121).
- The earlier follow-up candidate from `09-spec-author.md`, `Run(() => delegate () { })` with `Run<T>(Func<T>)`, is now decided by RH3106-002's wording "a type argument inferred from it": reported, fix withheld. It needs no separate follow-up.
- An anonymous method returning another anonymous method is treated like a lambda under RH3106-003, as the auditor's question asked. Alternative: reported with the fix withheld. I rejected it because the cause is the same as for a lambda (no target, inferred return type), so it is the natural-type outcome.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1282

Nothing to do: no audit-2 finding touches it.

## thoenissen/Reihitsu#1417

Nothing to do: no audit-2 finding touches it.

## thoenissen/Reihitsu#1422

Nothing to do: no audit-2 finding touches it.

## Whole run

- Files: `sdd/specs/analyzer/RH3004.md`, `sdd/specs/analyzer/RH3106.md`, `sdd/specs/shared/code-fixes.md`, `sdd/specs/requirement-ids.txt`.
- `scripts/test.sh --project architecture`: 114 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`: "RH3004-007: no test covers it". This is expected for a new requirement, and the test implementer's `[Requirement("RH3004-007")]` test closes it. Every other spec test passes, including `Uses` and the ID registry.
- No rule page changed (`documentation/rules/RH3004.md` and `RH3106.md`). The changes are a withheld fix, narrowed or widened reports in corner cases, and a layout corner case. The normal case is unchanged.

## Amendment — row 31

Commit: ebc7eb7d. Verdict: spec written. This is a clarification in the bug-fix lane: no change doc, no rule page. My prompt carried the lead's decision (row 31) and no other intended fix or verdict.

### Changed requirements

- RH3004-006 — clarified (row 31, the same class as finding 8).
  - The case: the first statement starts on its own line, and the measured shift would put a moved line left of column 0. Then every moved line moves by the number of columns that puts the leftmost of them in the statement column, one level inside the braces, and the lines keep their indentation relative to each other.
  - Every moved line counts for the leftmost line, including a comment or directive line in front of the first statement. Blank lines, directive lines that stay in column 0, and lines that keep their columns do not count.
  - When no line would go left of column 0, the measured shift applies, even where it puts a line left of the braces.
  - The requirement now says that no moved line is ever cut off at column 0.
  - The shared-brace-line paragraph is unchanged. Together, the two paragraphs cover every shift that would cross column 0.
  - The exception list in the shift sentence and the Why are extended.
  - New examples (FC-002). Both are `Run(⏎            delegate ()⏎                {⏎                    Foo();⏎…` with the anchor and braces at column 12 and the statement column at 16. The source shift is −4.
    - Comment `// note` in column 0: the comment lands at 16 and `Foo();` at 36.
    - Comment in column 8: the measured shift applies, so `Foo();` lands at 16 and the comment at 4.

### Examples for the tests

- RH3004-006: the two new `Run(` examples, one test each.

### Decided on recommendation

- The lead decided row 31 itself; I only wrote it down. One visible consequence for the lead to weigh: the first example is a natural input. It has an old-style brace under `delegate` and a commented-out line in column 0, as an IDE comment toggle writes it. The rule moves `Foo();` to column 36, 20 columns right of the statement column, to keep its offset to the comment. The alternative, keeping a column-0 comment line in column 0 the way column-0 directives are kept, would put `Foo();` at 16. That alternative would be a different rule and is not what row 31 decides.

### Output questions

_None._ The standing instruction applies.

### Internal choices

_None._

### Follow-up candidates

_None._ The candidate in the #1279 section above is now handled by row 31.

### Whole run

- `scripts/test.sh --project architecture`: 114 passed, 1 failed. The failure is the same expected one: "RH3004-007: no test covers it".

## Amendment — row 33

Commit: 8aed8eb1. Verdict: spec written. This is a clarification in the bug-fix lane: no change doc and no rule page. My prompt carried the lead's decision (row 33) and no other intended fix or verdict.

### Changed requirements

- RH3004-006 — clarified (row 33).
  - A line whose comment starts in column 0 MUST stay in column 0, as a column-0 directive does. An example is code commented out by an editor's comment toggle. A comment line that starts in another column moves with the statements.
  - The shift sentence now names column-0 comment lines among its exceptions.
  - Column-0 directive and comment lines neither move nor count for the leftmost-line rule, in both the shared-brace-line paragraph and the column-0 paragraph.
  - A block in which no line holds a statement is now measured on its first line that does not stay in column 0. Without this, a block holding only a column-0 comment would be measured on a line that does not move.
  - The Why gives the reason.
  - The row 31 examples are replaced by three, all with braces at column 12, statement column 16, and a measured shift of −4:
    - comment in column 0: it stays at 0, and `Foo();` lands at 16;
    - comment in column 8: it moves by the measured shift to 4, and `Foo();` lands at 16;
    - comment in column 2: the measured shift would put it left of column 0, so the leftmost-line rule applies. The comment lands at 16 and `Foo();` at 34, keeping their offset of 18.
  - Existing examples are unaffected: none of them holds a column-0 comment line.

### Examples for the tests

- RH3004-006: the three `Run(` examples, one test each. The column-0 comment example replaces the row 31 example with `Foo();` at 36.

### Output questions

_None._ The standing instruction applies.

### Internal choices

- "A line whose comment starts in column 0" is read as a line whose first character is a comment, `//`, `///`, or `/*`. The continuation line of a multi-line `/* */` that happens to start in column 0 is not such a line, and it moves with its comment. The alternative was to keep every line that starts in column 0 inside a comment, which would split a block comment's lines across columns.

### Whole run

- `scripts/test.sh --project architecture`: 114 passed, 1 failed. The failure is the same expected one, "RH3004-007: no test covers it".
