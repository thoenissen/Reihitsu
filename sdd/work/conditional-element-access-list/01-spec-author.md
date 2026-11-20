Role: spec author
Commit: 0d1958ce
Verdict: change doc written

One change doc covers both issues (shared cause: the bracketed argument list of an `ElementBindingExpressionSyntax`, which the formatter's join and RH5307 both skip because they test for an element access parent): `sdd/changes/2026-11/conditional-element-access-list.md`, `Status: Draft`. The spec is written on the recommended options of the two output questions below; the tests should wait for the maintainer's answers.

My prompt carried no intended fix or verdict to ignore. The issues' own suggested outcomes (#1877 option A, #1878 "span covers the whole conditional access expression") are treated as proposals.

## thoenissen/Reihitsu#1877

### Premise, reproduced

`scripts/format.sh` on `main` (0d1958ce), whole document: `c?[1,⏎2]` stays wrapped, `c?[⏎1, 2]` stays wrapped (its second line is realigned to column 19), `e?[0,⏎1]?[1,⏎2]` stays wrapped, while `d![1,⏎2]` and `d[1,⏎2]` are joined. The premise holds. Reachability: whole-document formatting and the CLI reach it; so does every code fix that formats a node holding the list (RH5101, RH5111, RH5302, RH5308 observed with `scripts/apply-fix.sh`: each joins `d[1,⏎2]` inside its formatted node today and leaves `c?[1,⏎2]` wrapped).

### Changed requirements

- FMT-BREAK-014 — Indexer arguments and angle-bracket lists are joined — changed: the lists it joins are named by parent kind (`ElementAccessExpressionSyntax`, `ElementBindingExpressionSyntax`, `ImplicitElementAccessSyntax`); the parent MUST NOT change whether a list is joined; a fixed-size buffer's declarator list is stated as not decided by it; the gaps of a conditional element access are divided (`[`…`]` is FMT-BREAK-014's, in front of `?` and between `?` and `[` stay FMT-BREAK-040's). New example with `c?[`, `![`, `c?[⏎`, a chained `?[…]?[…]`, and a comment between the brackets.

### Examples for the tests

FMT-BREAK-014 (second example). Its *before* parts were checked with `scripts/format.sh`: everything except the three `c?`/`e?` lists matches today's output exactly.

### Output questions

**1 · Formatter — wrapped argument list of a conditional element access**

Input:
```csharp
var a = c?[1,
           2];
var g = c?[
    1, 2];
```

Before (`main`):
```csharp
var a = c?[1,
           2];
var g = c?[
           1, 2];
```

After, A (recommended):
```csharp
var a = c?[1, 2];
var g = c?[1, 2];
```
Code fixes that format a node holding such a list join it too, as they already join `c[1,⏎2]` there. RH5111, for example, turns `a⏎    = c?[1,⏎         2] ?? 0;` into `a = c?[1, 2] ?? 0;` instead of today's `a = c?[1,⏎       2] ?? 0;`; RH5101, RH5302, and RH5308 change the same way.

After, B: unchanged, as in *Before*.

Why A: `?` in front of the brackets does not change how the index reads, and the maintainer's direction on #1869 is that `?` and `!` must not differ from a plain element access. Note: with B, question 2 can only be answered "not reported", because a layout rule must not report a shape the formatter keeps.

### Internal choices

- FMT-BREAK-014 states the lists by their parent kind in the syntax tree, rather than by examples; alternative: keep "a wrapped bracketed argument list (`m[1, 2]`)" and add `c?[…]` as an example only.
- A fixed-size buffer's size list (`fixed int b[⏎3];`, parent `VariableDeclaratorSyntax`; today kept wrapped and realigned) is named as the other side of the condition and left undecided ("this requirement does not decide it"), not specified; alternative: specify today's keep.
- FMT-BREAK-040 is not edited; FMT-BREAK-014 states the division of the gaps once.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1878

### Premise, reproduced

`scripts/apply-fix.sh RH5307` on `main`: `c?[1,⏎2]` and `c?[1,⏎2].ToString()` are not reported; `d![1,⏎2]` and `d[1,⏎2]` are reported and fixed to one line; `e?[0][1,⏎2]` is reported on `[0][1,⏎2]`; `c?[0,⏎1][1,⏎2]` gets one diagnostic (on the element access) whose fix leaves `c?[0,⏎1][1, 2]`. A comment in front of an element access's `[` (`d /* c */ [1,⏎2]`, or a `//` line between `d` and `[`) does not keep the report or the fix today. The premise holds. The issue's `c[1,⏎2] (null-forgiving)` line lost its `!`; the observation applies to `d![1,⏎2]`.

### Changed requirements

- RH5307-001 — A wrapped element access is reported — changed: the list of an element binding is reported; the parent MUST NOT change whether a list is reported (`m[`, `d![`, `c?[` alike); a comment in front of the `[` or around the `?` does not keep the report; the exclusion sentence and its negative example are removed; new positive example (`c?[`, `d![`, `c?[⏎`, `c?[…].ToString()`, `c /* c */ ?[…]`) and a negative one (comment between the brackets of `c?[…]`).
- RH5307-003 — Where the diagnostic is — changed: the list of an element binding is located on the bracketed argument list (= the element binding expression, `[` through `]`). Written on recommendation A of question 2.
- RH5307-004 — The fix joins the reported argument list — changed: the reported list may belong to an element binding; "the element access" that must stay as written now reads "the element access or the conditional access"; the explanation under example `e` no longer cites the exclusion; new example with `c?[1,⏎2] ?? 0`, a comment behind the `]`, and `e?[0,⏎1][1,⏎2]` with two overlapping diagnostics.
- Spec intro and non-goals: the intro names `c?[…]`; non-goals add the fixed-size buffer list and give the gaps in front of the brackets and around `?` to RH5115.
- `documentation/rules/RH5307.md`: the description now reads "indexer argument lists (`target[...]`, `target?[...]`)"; the exclusion sentence is removed. The page changes because what is reported changes in the normal case.

### Examples for the tests

RH5307-001 (the `!`/`?` example and the comment-between-brackets negative), RH5307-003 (via RH5307-001), RH5307-004 (the new last example). All RH5307 spec examples that hold `?` were run through `scripts/apply-fix.sh RH5307` with markup stripped: 0 parse errors; the parts this change leaves alone (the `d![…]` list, the existing `e`/`h`/`g` cases, the element access of `e?[0,⏎1][1,⏎2]`) are reported and fixed as the examples state.

### Output questions

**2 · RH5307 · Indexer bracketed arguments should be single lined — wrapped list of a conditional element access**

Input:
```csharp
var a = c?[1,
           2];
var j = c?[1,
           2].ToString();
var n = e?[0,
           1][1,
              2];
```

Before (`main`): `a` and `j` not reported. `n`: reported once, on `[0,⏎1][1,⏎2]`; the fix gives `e?[0,⏎           1][1, 2]`.

After, A (recommended) — reported on the brackets, fixed:
```csharp
var a = c?{|RH5307:[1,
           2]|};
var j = c?{|RH5307:[1,
           2]|}.ToString();
var n = e?{|RH5307:{|RH5307:[0,
           1]|}[1,
              2]|};
```
Fixed: `var a = c?[1, 2];`, `var j = c?[1, 2].ToString();`, `var n = e?[0, 1][1, 2];`.

After, B — reported from the receiver through the `]`, same fix:
```csharp
var a = {|RH5307:c?[1,
           2]|};
var j = {|RH5307:c?[1,
           2]|}.ToString();
```

After, C: not reported, as in *Before* (the rule page keeps "A null-conditional index access (`target?[...]`) is not checked.").

Why A: the element accesses inside a conditional access are already reported from the token behind the `?` (`e?{|RH5307:[0][1,⏎2]|}` today), and the `?` belongs to RH5115, so A keeps one location rule; B's span is not a syntax node when code follows the `]`. Rule page: with A or B the exclusion sentence is removed and the description names `target?[...]`.

### Internal choices

- The location is written on recommendation A; with B, RH5307-003 and the examples' markup change, nothing else.
- `c /* c */ ?[1,⏎2]` is written as reported. It is an FMT-UNSUP-014 shape; SHARED-DIAG-010 says an unsupported shape is reported per the rule's own requirements. Whether the fix is offered there follows SHARED-FIX-010 on the node the fix formats; the spec states no fix result for it, as it states none today for `d /* c */ [1,⏎2]` (whose fix is offered on `main`).
- One change doc for both issues, because the fix's result depends on the formatter's join and SHARED-DIAG-010 ties the report to it.

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project architecture`: 198 passed.
- No new requirement IDs; `requirement-ids.txt`, `Uses` rows, and code owners unchanged.
- Files: `sdd/changes/2026-11/conditional-element-access-list.md`, `sdd/specs/formatter/line-breaks.md`, `sdd/specs/analyzer/RH5307.md`, `documentation/rules/RH5307.md`.
