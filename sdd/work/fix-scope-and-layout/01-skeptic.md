Role: skeptic
Commit: 1868a22
Verdict: #1140: proceed narrower (architecture finding, escalation 9 likely); #1141: proceed (one instance no longer reproduces); #1144: proceed narrower (three instances no longer reproduce, one is specified behavior); #1145: proceed (Q4: refuse option recommended); #1094: proceed narrower (Q4 does not reproduce); #1393: not reproduced; #1511: proceed (class wider than reported); #1561: proceed (analyzer, not the fix)

The prompt carried no intended fix or verdict. Every observation below was measured on `1868a22` with a Debug build: `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF; LF and CRLF agreed in every arm unless noted) and `scripts/format.sh` on copies. Fixtures lived under the session scratchpad and are not part of the repository. `⏎` marks a line break in inline inputs.

## thoenissen/Reihitsu#1140

### Premise

Reachable: every instance is a code fix offered on an ordinary document (IDE, Fix All). Re-measured instance by instance:

| Instance | Rule | Today on `1868a22` | Reproduces |
|---|---|---|---|
| `class C<T> where T : class` + member `int  x  =  1;` | RH5110 | `int x = 1;` too | yes |
| `interface I<T> where T : class⏎{⏎}` | RH5110 | becomes `interface I<T>⏎    where T : class;` | yes |
| misindented `return` (`if (a) M(a);⏎int  y = 1;⏎  return y;`) | RH5204 | `if` body braced, two blank lines inserted, `int y` | yes |
| misindented `else` | RH5204 | with braced branches only the `else` line moves (not reproduced); with unbraced branches (`if (a)⏎ M(a);⏎  else⏎ M(!a);`) both bodies get braces. The count rise 4→8 did not reproduce with either input; the issue gives no input | partly |
| `return status switch {…}` misaligned arms | RH5206 | blank line inserted before `return` | yes |
| `if (x==1 && a⏎.B())⏎{ int  y = 1; }` | RH5112 | `if (x == 1 && a.B())` and `int y = 1;` | yes, but see Output questions 2 |
| `M(⏎1+2,⏎3)` | RH5101 | `M(1 + 2,⏎  3)` | yes, but see Output questions 3 |
| `a = () => { var x⏎= 1; int  y = x; };` | RH5111 | whole outer assignment formatted (lambda broken, `int y`) | yes (RH5111-004 does not cover a construct inside an assignment's value) |
| `/// <summary>P</summary>⏎int P { get; set; }⏎= 5;` | RH5111 | doc comment rewritten to three lines | yes |
| `/// <summary>A</summary>⏎int _a⏎;` | RH5113 | doc comment rewritten to three lines | yes |
| #1169 Q1 bullets 1–4 (constraint, `[Serializable][Obsolete]`, `internal  sealed  class  C`, `///<summary>x</summary>`) | RH5412/RH5414 | each formatted as reported | yes |
| #1169 Q1 bullet 5 (`namespace N;⏎// remark⏎class C⏎{⏎}`) | RH5412 | blank line after `namespace N;` | yes |
| #1209 Q2 (`else { int  x = 1; }` with reported `if` body) | RH5405 | `int x` | yes |
| #1209 Q3 (blank line above fixed `if`) | RH5405 | blank line inserted | yes |
| #1213 Q1, #1214 Q3, #1215 Q2, #1216 Q5, #1217 Q2, #1218 Q5 (construct inside an `if`/`foreach` header, body `int  x = 1;`) | RH5303–RH5308 | body formatted in each | yes (all six) |
| #1219 Q2, #1222 Q1 (doc comment on the property) | RH5401, RH5408 | doc comment rewritten to three lines | yes |
| #1220 Q1 (`else` body formatted) | RH5403 | `int x` | yes |
| #1220 Q2 (`while (a) { }⏎do { M(a); } while (a);`) | RH5403 | blank line before `do` | yes |
| #1220 Q3 second part (`else if … else { M(a, b); }`, last block reported) | RH5403 | correct `else if` body re-indented and `else` moved to column 13 | yes |
| #1221 Q3 (`class D { int  _x; }`) | RH5404 | `int _x;` | yes |
| #1235 Q2 (`    /**/ int _a;`) | RH3201 | ` int _a;` (indentation removed) | yes |
| comment of 2026-11-07: `[Obsolete]⏎#region legacy⏎#endregion⏎class A⏎{⏎}` | RH5412 | blank lines around both directives and above `class A;`; control `#region legacy⏎class A⏎{⏎}⏎#endregion` gets none | yes |
| same comment, three RH5417 region inputs (region around `get`, region inside the unreported `set` body, `#region getters` above `int P { get { return _x; } }`) | RH5417 | blank lines inserted in each | yes |

The tests that pin the inserted blank lines exist: `VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted` (`src/Reihitsu.Analyzer.Test/Formatting/RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.cs:230`), `VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration`, `…AfterClosingBrace`, `VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements` (`RH5405BracesMustNotBeOmittedAnalyzerTests.cs:449/489/535`). Their asserted output contradicts the *Blank lines* bullet of SHARED-FIX-006, which is unchanged; the maintainer's round-7a comment on this issue names them as changing.

Is the spec wrong instead? No for SHARED-FIX-006 (maintainer principle P9). But many rule specs state the opposite scope in their own fix requirement, each with a `Why:`: RH5110-003 ("format the declaration that owns the reported clause"), RH5204-006 ("format the smallest node around it that starts on an earlier line"), RH5206-003 (second sentence: "format the innermost of these that contains it"), SHARED-BRACE-002 ("laying out the `if` statement that owns it"), SHARED-CHAIN-009 (fallback to the statement), RH5101-003/RH5102-003 ("format the argument list as the formatter formats it in place"), and the analogous requirements of RH5303–RH5308, RH5401, RH5403, RH5404, RH5408, RH5412–RH5416 (SHARED-EMPTY-003), RH5417, RH3201. The triage comment on this issue (bug-fix lane, no change doc) names only RH5111-004 and RH5401-003 for alignment. Every listed rule requirement needs the same alignment to SHARED-FIX-006, as clarification; where the alignment leaves two outputs, it is an output question below.

### Mechanism and candidates

Mechanism: a code fix that delegates to `ReihitsuFormatter.FormatNodeInDocumentAsync` writes back the formatter's layout of every token and trivia inside the node it passes — and of that node's leading trivia when the node starts its line and is not a root child (FMT-PIPE-010) — so it changes every piece of code in that node that the formatter would change, not only the code its diagnostic is about.

Dispatch: the candidates are the fixes that call a node-level entry point (`rg "ReihitsuFormatter\.FormatNode" src/Reihitsu.Analyzer.CodeFixes`): `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` (RH5412–RH5416), `ExpressionBodyToBlockCodeFixProviderBase<TNode>`, `OperatorSpacingCodeFixProviderBase` (RH6023/RH6024, #1141), `StatementBracesCodeFixProviderBase` (RH5405–RH5407), `FluentChainCodeFixHelper` (RH5112/RH5201/RH5202), RH3004, RH3202, RH3203, RH3208, RH5101, RH5102, RH5110, RH5111, RH5113, RH5204, RH5205, RH5206, RH5301–RH5308, RH5401, RH5403, RH5404, RH5408, RH5417, RH7101. The excess falls into three axes:

- (a) the passed node is larger than the diagnostic's code (enclosing statement, type, member, outer assignment): RH5110, RH5204, RH5206, RH5112 fallback, RH5111 nested in an assignment, RH5303–RH5308, RH5403, RH5404, RH5405, RH5412–RH5416;
- (b) the leading trivia of a passed node that starts its line is formatted (doc comment rewritten, blank line inserted above it): RH5111 property, RH5113, RH5206, RH5401, RH5403, RH5405, RH5408, RH5412 (b4, b5);
- (c) content inside a right-sized node is formatted beyond what the diagnostic names (spacing, nested lambdas, blank lines around region directives inside the node): RH5101/RH5102 argument content, RH5412/RH5417 region blank lines, and #1141's operator fixes.

RH3201 Q2 is not this mechanism: the RH3201 fix removes the line's indentation together with the comment by text edit (`    /**/ int _a;` → ` int _a;`).

### Breaking inputs

Failure classes: FC-030 (the issue itself), FC-027 (another enclosing scope: header of `if`/`foreach`, argument, `=` clause, lambda), FC-026 (`else if` chains), FC-017 (tests that pin the blank line). Every instance of the table above is a candidate regression input; for axis (b) the test implementer covers a documentation comment, a `//` comment, and a blank line above the target; for axis (a) a construct inside an `if` header with a sibling body, inside a lambda, and inside an initializer.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Code fixes listed under axes (a)–(c) | must change (or be withheld for a rare class) |
| `ReihitsuFormatter` node-level entry points | possibly must change (axis b is FMT-PIPE-010's leading-trivia rule); decided by the lead with the architecture finding |
| Analyzers | not involved |
| Rule specs listed under *Premise* | must change (clarification to SHARED-FIX-006) |
| Rule pages | not involved |
| Tests pinning inserted blank lines | must change |

### Refuse option

Withholding the fix wherever its formatted node holds other code the formatter would change is not a cheap check (it needs the formatter's result and a per-rule notion of "the diagnostic's code"), and it would withhold RH5204, RH5110, RH5303–RH5308 and the operator fixes for code people commonly write — escalation 9. A local refusal is reasonable only for rare classes: the RH5111 declarator inside an assignment's lambda value, and region directives inside an accessor list or a member body (RH7303 reports a region inside a member) for RH5417 and between an attribute list and its type for RH5412. No unsupported shape whose declaration would delete a special case was found for this issue; none is proposed.

Simpler options inside the run: RH5204 already has a path that replaces only a line's indentation from the analyzer's expected-indentation map (the region-directive path of `RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider`); the token and comment diagnostics could use the same data. The context entry point `FormatNodeInDocumentWithContextAsync` (target written back, context formatted) exists for RH5111/RH7101/RH3208, but FMT-PIPE-017 keeps the target's first token in its column, so it cannot serve RH5204.

### Output questions

Asked under #1140 because they decide what the fixes write; each needs the maintainer because a rule requirement and SHARED-FIX-006 give different output.

**1 · RH5204 · Indentation must use 4 spaces per scope level — misindented statement in an unformatted body**

```csharp
class C
{
    int M(bool a)
    {
        if (a) M(a);
        int  y = 1;
          return y;
    }
}
```

Before (`main`): the body is formatted — `if (a)` gets braces, a blank line is inserted above `int y = 1;` and above `return y;`, `int  y` becomes `int y`, `return` moves to column 8.

After, A (recommended): only the reported line moves.

```csharp
        if (a) M(a);
        int  y = 1;
        return y;
```

After, B: as today (RH5204-006 as written).

Why A: SHARED-FIX-006 and the maintainer's P9 decision name exactly this; RH5204-006 is the requirement that changes. The same answer settles RH5110-003, RH5206-003, SHARED-BRACE-002, RH5303–RH5308, RH5403, RH5404, RH5412–RH5416 and RH5417, which state the same wider scope; the lead may treat the triage comment ("the fix follows SHARED-FIX-006") as already covering it, in which case this is not asked.

**2 · RH5112 · Wrapped fluent calls should keep the first call on the original line — chain in an `if` header**

```csharp
        if (x==1 && a
            .B())
        {
            int  y = 1;
        }
```

Before (`main`), as SHARED-CHAIN-009's fallback demands:

```csharp
        if (x == 1 && a.B())
        {
            int y = 1;
        }
```

After, A (recommended): the line of the chain is laid out as the formatter lays it out; the body stays.

```csharp
        if (x == 1 && a.B())
        {
            int  y = 1;
        }
```

After, B: only the chain changes.

```csharp
        if (x==1 && a.B())
        {
            int  y = 1;
        }
```

After, C: as today.

Why A: SHARED-CHAIN-009 formats the code in front of the chain because the chain's columns depend on it; the embedded body is on no line of the chain, so formatting it is what SHARED-FIX-006 forbids.

**3 · RH5101 · First argument should be on the same line — content inside the list**

```csharp
        M(
            1+2,
            3);
```

Before (`main`):

```csharp
        M(1 + 2,
          3);
```

After, A (recommended): as today — the argument list is the construct the rule names and lays out.

After, B: only the list's line breaks and columns change.

```csharp
        M(1+2,
          3);
```

Why A: RH5101-003 and RH5102-003 name the argument list as the code the fix formats, and B needs a formatter entry point that applies only some phases to a node — a new mechanism for a spacing difference RH6024 reports on its own. The same answer decides content inside the construct for RH5102, RH5206 (arms), RH5303–RH5308.

### Internal choices

- Where the leading-trivia part (axis b) is fixed: in each fix (pass a node that does not start its line, or restore the leading trivia), or once in `FormatNodeInDocumentAsync` (FMT-PIPE-010 keeps the leading trivia of every target). No visible difference for the fixes listed; the second changes FMT-PIPE-010 and every other caller (RH5417's documentation-comment exception in FMT-PIPE-010 relies on today's rule). Recommendation: decide with the architecture finding; prefer one place if no caller needs the formatted leading trivia.

### Issue goals

- "the rule specs state the narrowed scope" — keep; it is the clarification list under *Premise*, longer than the issue knows.
- "the RH5204 count of the `else` input falls instead of rising" — drop: no input is given and two constructed inputs do not show a rise.
- "Each input … in one application" — keep.
- The whole instance list in one run — change: it spans about twenty fixes (see *Architecture findings*); the lead may narrow the run to a subset and keep the rest under `Refs`. No visible output in question by narrowing.

### Architecture findings

Components: about twenty code fixes and `ReihitsuFormatter.FormatNodeInDocumentAsync`. Shared decision: which code a fix changes and how that code is laid out. The fixes decide scope only by choosing a node, and the entry point formats everything inside it (axes a–c); the fixes that need less than a node compute columns themselves — `RH5103…CodeFixProvider.GetIndentation`, `TargetAttributePlacementCodeFixProviderBase.ApplyCodeFixAsync` (SHARED-ATTR-008 depth model), the RH5203 fix, the RH5409–RH5411 and RH3201/RH3102 text edits — which predict the indentation phase (#1144, #1145, #1094 show the misses). Third cause: the scope is restated per rule in about fifteen rule requirements that drifted from SHARED-FIX-006. Inputs that show it: the table under *Premise*, and the #1144/#1145/#1094 instances below. A structural change (an entry point that lays out only the changed code) would exceed 100 lines; escalation 9 when chosen.

### Follow-up candidates

- RH5204 does not report a misindented statement that starts with an identifier or a contextual keyword (`      var y = x;`, `      x++;` at column 6 in a method body: `no-diagnostic`). RH5204-001 names only keywords and modifiers, so this is specified behavior; listed only for the lead's awareness, not as a defect.

## thoenissen/Reihitsu#1141

### Premise

Reachable through the RH6023/RH6024 fixes. Re-measured:

| Instance | Today | Reproduces |
|---|---|---|
| RH6024 `_ = a+M(a, () => { return 1; });` | `_ = a + M(a,⏎() =>⏎{ … });` over five lines | yes — SHARED-FIX-006's own RH6024 example demands the lambda stays |
| RH6023 `Action f=()=>{ M(); };` | lambda broken onto four lines | yes |
| RH6023 `var x=new[]{1,2};` | `var x = new[] {1, 2 };` | yes |
| RH6024 `_ = a⏎/* c */ +b;` | `_ = a⏎/* c */ + b;`, no blank line | **no** |
| RH6024 `_ = a+⏎b;` | `_ = a⏎+ b;` (operator moved) | yes |
| RH6023 `a=⏎1;` and `a⏎=1;` | both `a = 1;` | yes |

Fix All cases of #1106 Q3, as single fixes applied until clean: `a+b+c`, `a==b&&b>0` reach `a + b + c`, `a == b && b > 0` in 4 iterations for 5 diagnostics; `d[k=1]=2` reaches `d[k = 1] = 2` in 2 iterations for 2. No Fix All runner exists; the test implementer covers it through `GetFixAllScenario`.

### Mechanism and candidates

Mechanism: `OperatorSpacingCodeFixProviderBase` formats the smallest node that holds the operator and both neighbours through `FormatNodeInDocumentAsync`, so everything inside that node — operands, lambdas, initializers, the line break next to the operator — gets the formatter's layout. Same mechanism as #1140 axis (c). Candidates: every operator the two rules check (binary operators, assignment and compound assignment, inside expressions, declarators, element access, lambdas); the operand kinds that the formatter would change: lambdas, initializers, argument lists, wrapped operands.

### Breaking inputs

FC-030, FC-019/FC-021 (a comment or a line break in the gap: SHARED-OPSPACE-004 decides only gaps without line break and comment, so a gap with one stays as written), FC-007 (after the narrowing, a wrapped gap must not leave the diagnostic: the test implementer checks `a=⏎1;` and `a+⏎b;` clear in one application), FC-008 (the three Fix All inputs).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `OperatorSpacingCodeFixProviderBase` | must change |
| RH6023/RH6024 analyzers | must stay |
| SHARED-OPSPACE-004 | must stay (already narrow) |
| Rule pages | not involved |

### Refuse option

Withholding the fix when the node holds anything the formatter would change refuses common code; not recommended. The narrower option is the requirement itself: SHARED-OPSPACE-004 asks only for one space in each reported gap without line break and comment, which needs no formatter call.

### Output questions

_None._ SHARED-FIX-006's RH6024 example and SHARED-OPSPACE-004 decide every instance.

### Internal choices

_None._

### Issue goals

- Drop the `/* c */` instance: it no longer reproduces; keep it as a characterization test.

### Architecture findings

Part of the #1140 finding (operator fixes delegate to a node-level entry point for a two-gap change).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1144

### Premise

Reachable through each fix. Re-measured against `scripts/format.sh` on the fixed document:

| Instance | Today | Reproduces |
|---|---|---|
| RH5101 raw string `M(⏎"""⏎a⏎""",⏎2);` | content and closing quotes in column 10; formatting changes nothing; RH5202 `no-diagnostic` | **no** (resolved on main) |
| RH5102 raw string `M(1, """⏎a⏎""");` | same | **no** |
| RH5205 `Func<int, int> c = [Obsolete] (x) =>⏎{ return x; };` | `[Obsolete]⏎(x) =>` and `{` in column 27; document formatting puts `(x) =>` and `{` in column 8 | yes |
| RH5203 `M(1,⏎ // two⏎ 2);` | `// two` moved to column 0, `2` in column 10 | yes; expected output in question (Output questions 1) |
| "misindented target keeps its column" | FMT-PIPE-017 specifies it | not a defect |
| #1168 RH5409 `B ,` before `}` / `, }` | `B ` with trailing space / `     }` one column right | yes |
| #1170 Q1 RH5417 `void M() { } int _x;` | `} int _x;` | yes |
| #1170 Q1 `void M() { } void N() { }` | `} void N()⏎  {` (braces in column 6) | yes |
| comment 2026-10-21: RH5417 `int _x; void M() { }` | `int _x; void M()⏎{` with the braces in column 12 | yes |
| comment 2026-10-21: RH3202 `… => _value; private int _x;` | `} private int _x;` | yes |
| #1209 Q4 / comment 2026-11-07: RH5405 `else if (b)⏎M(a, b);` | braces in column 13 (inner `if`); document formatting: column 8 | yes |
| #1220 Q3 first part: RH5403 `else if (b) { M(a, b); }` | block in column 13 | yes |
| #1230 RH3102 `a++; ;⏎a++;` | `a++;         a++;` on one line | yes |
| #1235 Q3 RH3201 whitespace-only `/*…*/` over three lines after `{` | an empty line stays after `{` | yes |
| #1238 Q2 RH3106 `delegate () { }`, RH3107 `[Obsolete /* a */ ( ) /* b */]` | `delegate { }`, `[Obsolete /* a */ /* b */]` — one space each | **no** |

Spec check: the shared-line instances are demanded by SHARED-FIX-007 *Shared line* (its RH5417 example is this case). The `else if` column is not decided by SHARED-BRACE-002, whose column sentence covers only an `if` that starts its line, so SHARED-FIX-007 decides it (column of `else`). RH5205: FMT-PIPE-017 lays out the target's other lines with the columns the indentation phase gives them at their depth, shifted by an offset of zero here (`[` keeps column 27 in both), so `(x) =>` in column 27 contradicts FMT-PIPE-017 — a formatter entry-point defect, not only a fix defect. RH5409, RH3102, RH3201 Q3: SHARED-FIX-007 first sentence.

### Mechanism and candidates

Two mechanisms, both members of the #1140 architecture finding:

1. A fix that writes its own whitespace computes a column or a line break the indentation phase or the cleanup phase owns, and misses inputs where the phase decides otherwise: RH5409–RH5411 (trailing whitespace left), RH3102 (line break removed with the empty statement), RH3201 (empty line left), RH5203 (comment line rewritten).
2. A fix that formats a node that does not start its line, or whose layout depends on code in front of it, gets the node-level layout where it differs from the document's: RH5405/RH5406/RH5407 and RH5403 on an `else if` body (the inner `if` is the owner and does not start its line); RH5417 and RH3202 (the member shares its line with another member; `FormatNodeInDocumentAsync` keeps what is outside the target); RH5205 (a lambda behind an attribute list, whose document layout places `(x) =>` from the statement's column).

Candidates for 2: `StatementBracesCodeFixProviderBase` (RH5405–RH5407) and RH5403 with `else if` at each nesting; `ExpressionBodyToBlockCodeFixProviderBase` (RH3202, RH3203, and the other expression-body kinds, FC-025) and RH5417 with a member before or behind on the line; RH5205 with and without attribute lists.

### Breaking inputs

FC-026 (`else if` chains, nested), FC-025 (seven expression-bodied kinds for the shared line), FC-028 (node-level vs document layout), FC-007 (RH5417 `void M() { } void N() { }` needs two applications today — one per diagnostic, which is allowed; the column 6 is the defect). Each reproduced row is a regression input; LF and CRLF for each.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Fixes RH5203, RH5205, RH5403, RH5405–RH5407, RH5409–RH5411, RH5417, RH3102, RH3201, RH3202 | must change (or be withheld for a rare class) |
| `FormatNodeInDocumentAsync` (RH5205 case) | possibly must change (FMT-PIPE-017) |
| RH5101/RH5102 fixes | must stay (characterization tests) |
| RH3106/RH3107 fixes | must stay |
| Specs | FMT-PIPE-010 must stay (the issue's requested sentence is already FMT-PIPE-017); RH5203-003 depends on Output questions 1 |

### Refuse option

- RH5205 with an attribute list on the lambda: a rare shape (lambda attributes); withholding the fix when the lambda has an attribute list is a cheap syntactic check and avoids touching the entry point. Recommended over changing FMT-PIPE-017's implementation for it.
- `else if` brace fixes: common code; refusing is not acceptable (escalation 9 if chosen).
- Shared-line members (RH5417, RH3202): withholding when another member shares the line is a cheap check, but SHARED-FIX-007 has an explicit requirement and example for exactly this case, so withholding would contradict an unchanged requirement — not available without an output question. Whether the shape is rare was not established.
- RH5409–RH5411, RH3102, RH3201: the defects are on the edited line only; no refusal needed.

### Output questions

**1 · RH5203 · Multi-line arguments should be aligned — comment line above the argument**

```csharp
        M(1,
 // two
 2);
```

Before (`main`):

```csharp
        M(1,
// two
          2);
```

After, A (recommended):

```csharp
        M(1,
          // two
          2);
```

After, B (RH5203-003 as written: "MUST NOT change anything else"):

```csharp
        M(1,
 // two
          2);
```

Why A: the maintainer decided A in #1123 Q1 (shape 1); RH5203-003 as written says B. One of the two has to change.

### Internal choices

_None._

### Issue goals

- Drop "FMT-PIPE-010 carries the sentence above": FMT-PIPE-017 already states that node-level targets take the document's layout. No visible effect.
- Drop the RH5101/RH5102 raw-string instances as defects; keep one characterization test each (RH5202 does not report the result). No visible effect.
- Drop #1238 Q2 (does not reproduce); characterization test only.
- The misindented-target instance stays dropped (specified by FMT-PIPE-017).

### Architecture findings

Members of the #1140 finding: mechanism 1 is fixes predicting phase decisions; mechanism 2 is node-level layout diverging from document layout where the target does not start its line.

### Follow-up candidates

- #1443 (RH5417 type `}` on the method's line) stays outside the run.
- #1416 (SHARED-BRACE-002 alignment) overlaps the `else if` column; the lead decides whether the `else if` instance is closed here or left to #1416.

## thoenissen/Reihitsu#1145

### Premise

Reachable through the RH5103 fix. Both parts reproduce:

- Q2: `if (a) M(); M();` and `L: M(); M();` → reported, `no-fix-offered`. RH5103-004 withholds only for a gap holding something other than spaces and tabs, and RH5103-003 gives the column (the preceding statement starts its line → its line's indentation). Bug against RH5103-003/004.
- Q4: `void N() { M(); M(); }` → `void N() { M();⏎    M(); }` (column 4); formatter: column 8. `Action a = () => { M(); M(); };` → moved `M();` in column 8; formatter: braces under `(` in column 19, statements in column 23. RH5103-003 does not decide a preceding statement that does not start its line outside a switch section, so SHARED-FIX-007 decides: the formatter's column (8 and 23).

### Mechanism and candidates

Q2: `HasEditableLeadingGap` requires that the token in front of the reported statement has the preceding statement as its nearest `StatementSyntax` ancestor; when the preceding statement ends with a nested statement (an embedded statement or a block), that token belongs to the nested one and the fix is withheld. Candidates from the `StatementSyntax` hierarchy, measured with `<stmt> M(…);` on one line:

| Preceding statement | Fix offered today |
|---|---|
| `if (a) M();`, `if (a) { }`, `if … else M();` | no |
| `while (a) M();`, `for (;;) { }`, `foreach (…) M();` | no |
| `using (d) { }`, `lock (o) { }`, `fixed (…) { }` | no |
| `try { } finally { }`, `checked { }`, `unsafe { }` | no |
| `L: M();` | no |
| local function `void F() { }` | no |
| `{ }`, `do { } while (a);`, `switch (a) { }` | yes |

Q4: `GetIndentation` returns the preceding statement's line indentation for any statement whose parent is a block, which is the block's statement column only when the preceding statement starts its line.

### Breaking inputs

FC-001/FC-016 (guard scope: every row of the table), FC-027 (lambda block, method block, local function block), FC-015 (the column is a copy of the indentation phase's decision).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5103 fix | must change |
| RH5103 analyzer | must stay |
| RH5103-003 | clarification: the block case (Q4), or the withheld class if the refuse option is taken |
| `documentation/rules/RH5103.md` | its "check the indentation" sentence about one-line switches stays unless that case is fixed; the issue does not ask for the switch case |

### Refuse option

Q4: both inputs already violate RH5417 (`void N() { M(); M(); }`: reported and fixed by RH5417; the lambda: reported without a fix), so the class "preceding statement does not start its line inside a block" is rare by definition. Withholding the fix there is one syntactic check; computing the formatter's column for a lambda block (anchor-derived, FMT-INDENT-028) inside the fix is another prediction of the indentation phase. Recommended: withhold for Q4 and record the decision in RH5103's spec. This drops the issue goal "both Q4 inputs produce the formatter's indentation"; withholding a fix for a rare class puts no output in question (`squad/README.md`), but the lead should note that #1116 Q4 was decided as a P10 bug by the maintainer.

Q2 needs no refusal: narrowing the guard is a cheap check.

### Output questions

_None._

### Internal choices

- Q4: implement the formatter's column vs withhold (above). Recommendation: withhold.

### Issue goals

- "Both Q4 inputs produce the formatter's indentation" — change to "the fix is withheld" under the refuse option.
- "RH5103-003 states the block rule" — keep, as the withheld class or as the column.

### Architecture findings

Member of the #1140 finding (the fix predicts the indentation phase's column).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1094

### Premise

Reachable through the RH5513 fix (shared base `TargetAttributePlacementCodeFixProviderBase`, so every attribute-placement rule using SHARED-ATTR-008 is a candidate).

- Q2: `int _a; [Obsolete] void M()` → `int _a; [Obsolete]⏎    void M()`. Formatter: `int _a;⏎[Obsolete]⏎void M()`. Reproduces; SHARED-FIX-007 *Shared line* demands the formatter's result.
- Q3: `Run(1, () =>⏎{` with the block's `{` in column 15 and `[Obsolete] void L()` in column 19 → `void L()` in column 12 (not the brace's column 15 the issue reports, but still wrong). SHARED-ATTR-008 does not cover it (its innermost enclosing brace is not at four spaces per scope), so SHARED-FIX-007 decides: column 19, under its attribute list. Reproduces.
- Q4: `\t\t\t[Obsolete] void L()` → `\t\t\t[Obsolete]⏎            void L()` (twelve spaces, the attribute's visual column at tab width 4). That is the output the maintainer decided in #1030 Q4. **Does not reproduce.**

### Mechanism and candidates

The fix indents the declaration by `ComputeBaseIndentLevel × 4` (nesting depth) unless an initializer or anonymous object encloses it, and never moves anything in front of the list: so it misses every enclosing brace that is not at its depth column (continuation-indented lambdas, anchor-aligned blocks — FC-027), and it leaves the list on a line it shares with another member. Candidates: the attribute-placement rules that use SHARED-ATTR-008 (RH5511, RH5513, and the other target rules of `attribute-targets.md`) with: a member before the list on its line; a lambda block in a wrapped argument; a lambda block anchored to `(` (FMT-INDENT-028); a switch-section statement.

### Breaking inputs

FC-027, FC-015 (depth model is a copy of the indentation phase), FC-016 (sibling rules of the shared base).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `TargetAttributePlacementCodeFixProviderBase` | must change (Q2, Q3) |
| SHARED-ATTR-008/009 | must stay; SHARED-FIX-007 decides the uncovered inputs |
| Tab handling | must stay |

### Refuse option

Q3 (an attribute list on a local function inside a lambda block that is not at its depth column) is rare; withholding the fix when the innermost enclosing brace is not at four spaces per scope is the condition SHARED-ATTR-008 already states as its coverage boundary, so it is a cheap check. Q2 is SHARED-FIX-007's explicit shared-line case; refusing it would contradict that requirement.

### Output questions

_None._

### Internal choices

- Q4 has a second reading: SHARED-FIX-007's tab bullet ("laid out from the column its first token has with that tab counted as one column") read with the attribute list as the changed code's first token would put `void L()` in column 3, the output #1030 Q4 rejected. Recommendation: keep today's output; the maintainer's #1030 Q4 answer covers it. If the lead reads the tab bullet otherwise, this becomes an output question.

### Issue goals

- Drop Q4: it already gives the decided output; add a characterization test.

### Architecture findings

Member of the #1140 finding (depth model predicts the indentation phase).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1393

### Premise

Only the RH5104 half remains (the RH5417 half moved to #1443). `M();⏎M(); // second` → `M();⏎// second⏎M();`, no blank line. **Does not reproduce.** RH5104-005 already states the case ("MUST NOT insert a blank line above the moved comment") and its spec example contains this exact shape; `RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests.VerifyRelocatedCommentGetsNoBlankLineWhenPreviousLineIsCode` (line 335) and four more `[Requirement("RH5104-005")]` tests cover it.

### Mechanism and candidates

_N/A._

### Breaking inputs

None beyond the existing tests; a characterization test with the issue's exact input under LF and CRLF is optional.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5104 fix | must stay |
| RH5104-005 | must stay (already states the case) |

### Refuse option

_None._

### Output questions

_None._

### Internal choices

- Link: the triage comment asks to close it with a regression test; AGENTS.md asks `Refs` for a bug that does not reproduce. The issue's purpose (no blank line, RH5104-005 states it) is met on main. Recommendation: `Closes`, with the note that it was resolved before the run.

### Issue goals

- "RH5104-005 states the P14a case" — already true; drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1511

### Premise

Reachable: RH5204 reports in any document. Reproduces for both inputs (`no-progress`, LF and CRLF), and the class is wider than the issue states:

| Input (`    ` = four stray spaces) | Today |
|---|---|
| `/// <summary>x</summary>⏎    public class C⏎{⏎}` | `no-progress`; the class also becomes `public class C;` |
| `#nullable enable⏎    public class C⏎{⏎}` | same |
| `    public class C⏎{⏎}` (no leading trivia) | same |
| `// header⏎    class C⏎{ int _a; }` | `no-progress`; the class body is re-indented relative to column 4 |
| `    namespace N⏎{ class C { } }` | `no-progress`; namespace body reformatted |
| `    // header⏎class C⏎{⏎}` (misindented comment first) | `no-progress`; `class C;` |
| `class A { int  _a; }⏎⏎  class B⏎{⏎}` (later top-level type) | fixed, but the whole file is formatted (`int _a`, `class B;`) — #1140 class |
| `namespace N;⏎⏎  class B⏎{⏎}` | fixed, `class B;` — #1140 class |

### Mechanism and candidates

Mechanism: for a reported token or comment that no ancestor encloses from an earlier line — the first token of the compilation unit, or a comment in front of it — `TryGetFormattingScope` falls back to the reported node, a direct child of the root, and `FormatNodeInDocumentAsync` keeps a root child's leading trivia (FMT-PIPE-010), which holds the whitespace the fix has to change. Candidates: every kind RH5204-001 checks that can be the compilation unit's first token (type, namespace, delegate, enum), with no leading trivia, a `//` comment, a `///` comment, a directive; and a misindented comment that starts the file.

### Breaking inputs

FC-007, FC-033 (both line endings), FC-030 for the later-top-level rows.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5204 fix | must change |
| RH5204 analyzer | must stay |
| RH5204-006 | covered by the #1140 output question 1; if answered A, the same narrowing decides #1511 |

### Refuse option

Withholding the fix for a misindented first token of a file is cheap but leaves a common, trivially fixable case unfixed; the narrowing of #1140 output question 1 (replace only the reported line's indentation) corrects it without a special case. If #1140 Q1 is answered B, withholding is the fallback.

### Output questions

_None of its own_; #1140 output question 1 decides what else the fix changes. The issue's required output admits either.

### Internal choices

_None._

### Issue goals

- Widen: the class covers a first token without leading trivia, behind a `//` comment, and a namespace; the issue names two members.

### Architecture findings

Member of the #1140 finding (scope chosen by node).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1561

### Premise

Reachable through whole-document formatting (the issue's input, after `scripts/format.sh`, stable on a second pass) and directly in source. Today's formatter output differs from the issue's (`b */ case 2:` keeps column 0 instead of 16), but the defect stays: RH5204 reports, the fix ends `no-progress` (LF and CRLF). RH5204-001 checks "a line whose first token (SHARED-BLANK-008)…", and SHARED-BLANK-008 says a token behind the closing line of a multi-line comment does not start its line; SHARED-COMMENT-011 makes a comment that starts its line the line's first element. So RH5204 must not report it: the spec already picks the issue's first alternative. No spec clarification needed.

### Mechanism and candidates

Mechanism: the analyzer decides "first on its line" with `SyntaxTokenPositionUtilities.IsFirstOnLine` (any end-of-line trivia between the previous token and the token), which counts a token behind the closing `*/` of a multi-line comment as first on its line; SHARED-BLANK-008 is `SyntaxTokenPositionUtilities.StartsLine`. The analyzer then measures the comment's closing line as that token's indentation. Candidates: every token kind RH5204-001 checks, behind a multi-line `/* */` or `/** */` comment that starts its line:

| Token behind `b */` | Today |
|---|---|
| `case` (with `/** */`, the issue's shape) | reported, `no-progress`; the fix also inserts a blank line above the comment |
| `}` | reported, `no-progress` |
| `{` | reported, `no-progress` |
| `else` | reported, `no-progress` |
| `catch` | reported, `no-progress` |
| `return` | reported, `no-progress` |
| `private int _b;` | reported, `no-progress` |
| `case` with the closing line indented to the expected column | not reported (accidental match) |
| `M();`, `int _b;` | not reported (RH5204 never checks a statement starting with an identifier or a member starting with a type keyword) |

### Breaking inputs

FC-013 (formatter output flagged), FC-007, FC-021 (`/* */` ending mid-line), FC-040 (`/**/` parsed as documentation comment vs `/* */`). Test both comment kinds, LF and CRLF, and the comment in place vs out of place (SHARED-COMMENT-011: a misplaced comment is still reported, as the comment).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5204 analyzer | must change |
| RH5204 fix | not involved (no diagnostic left to fix) |
| Formatter (FMT-BREAK-033) | must stay |
| RH5204 spec | must stay |

### Refuse option

Withholding the fix would turn `no-progress` into `no-fix-offered` but leave a report RH5204-001 forbids; not a fix of the defect. Declaring a shape unsupported is an option (escalation 10): the shape is "a token that stands behind the closing `*/` of a delimited comment spanning several lines, on that comment's last line", from the comment's first character through that token, excepting nothing. It would delete the special handling in FMT-BREAK-033's second paragraph, FMT-PIPE-017's sentence on a first token behind a multi-line comment, and SHARED-COMMENT-012's case for it (code-level guards not enumerated here). It would refuse files with `/* a⏎b */ int x;` or a Javadoc-style `/**⏎ * doc⏎ */ void M()`; the formatter itself produces the shape from `break; /** a⏎b */ case 2:` (FMT-BREAK-033). Recommendation: do not declare in this run; if declared, in a change of its own. The analyzer fix is needed either way, because declaring does not change what RH5204 reports.

### Output questions

_None_ (RH5204-001 with SHARED-BLANK-008 decides: not reported). The lead may still list declaring the shape as escalation 10.

### Internal choices

_None._

### Issue goals

- The issue offers "not reported" or "fixed in one application"; the spec decides "not reported". Drop the second alternative.

### Architecture findings

FC-015 class: RH5204's analyzer uses a private notion of "first on line" next to the shared `StartsLine` that SHARED-BLANK-008 names. Within the run: the analyzer, its fix (which consumes `BuildExpectedIndentationMap` for directives), and `Reihitsu.Core`.

### Follow-up candidates

- Other analyzers that call `IsFirstOnLine` where their spec says SHARED-BLANK-008 may share the defect (`rg -l IsFirstOnLine src/Reihitsu.Analyzer`); not enumerated here.

## Whole run

- Build: `scripts/build.sh --configuration Debug` on `1868a22`, 0 warnings, 0 errors.
- No test was run; no repository file other than this handover was changed.
