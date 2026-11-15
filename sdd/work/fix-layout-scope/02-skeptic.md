Role: skeptic
Commit: 04a85d6
Verdict: #1140: proceed narrower (plus output questions); #1687: proceed (the class is wider than the issue); #1748: proceed (the class is wider than the issue)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every observation below is `main`'s behavior: the branch changes no source yet (d1884a0 is spec-only). All fix outputs come from `scripts/apply-fix.sh <ID> <dir>` on fixtures in the session scratchpad, run under LF and CRLF. Both endings gave identical diffs in every case. All the code-fix providers involved are reached by the IDE through the analyzer package; the CLI runs no code fix. So for every case below, the caller is "the code fix, applied in the IDE", and the formatted node is named per rule.

## thoenissen/Reihitsu#1140

### Premise

Real. Reachable: each fix delegates to `ReihitsuFormatter.FormatNodeInDocumentAsync` with a node larger than the reported construct, or with a target that starts its line. Every instance the issue and the spec diff name reproduces on `main`:

| Rule (formatted node) | Input | `main` output beyond the reported code |
|---|---|---|
| RH5110 (declaration, `RH5110…CodeFixProvider.cs:59`) | `class C<T> where T : class` + member `int  x  =  1;`; empty `interface I<T> where T : class { }` | `int x = 1;`; the interface becomes `where T : class;` |
| RH5110 | `public  class D<T>  :  object where T : class { }` | header respaced (`public class D<T> : object`), `{ }` becomes `;` |
| RH5110 | `void M<T>( T t ) where T : class { int  y  =  1; }` | `M<T>(T t)`, `int y = 1;` |
| RH5110 | `class C<T> where T : class {` (brace on the clause's line) | `{` moves to its own line (see output question 4) |
| RH5111 (outer assignment, `GetFormattingNode`) | the spec's lambda declarator | lambda body expanded to a block over 4 lines, `int y = x;` |
| RH5111 | `a = () => { int L(int p⏎= 1) => p; int  y = L(); };` | lambda expanded, local function converted to a block body, blank line inserted, `int y` |
| RH5111 / RH5113 / RH5401 / RH5408 (property or declaration) | one-line `/// <summary>X</summary>` above | documentation comment spread over three lines |
| RH5113 | `int  _a⏎;` | `int _a;` |
| RH5206 (innermost statement) | `status++;⏎return status switch {…};` | blank line inserted above `return` |
| RH5112 (SHARED-CHAIN-009 fallback) | the issue's `if (x==1 && a⏎.B() …)` | header and body formatted (`x == 1`, `int y = 1;`) |
| RH5101 (argument list) | `M(⏎1+2,⏎3);` | `M(1 + 2,` |
| RH5403 (owning statement) | `if (a) M(a);⏎else { M(a); M(a); }` | unbraced `if` branch wrapped in braces |
| RH5403 | `if (a) { M(a); M(a); }⏎else M( a );` | `else` branch wrapped in braces, `M(a)` respaced |
| RH5403 | `if (a>0) { a++;  a--; }` | `a > 0` |
| RH5403 | `if (a > 0) { int N() => 1; a += N(); }` | local function converted to a block body, blank line inserted |
| RH5404 (declaration) | `sealed  class B { int _x; }` | `sealed class B` |
| RH5404 | `class B { int M() => 1; [System.Obsolete] int _x; }` | `M` converted to a block body, the attribute split onto its own line |
| RH5404 | `class B { int  _x  =  1+2; void M() { M(); } }` | `int _x = 1 + 2;`, `M`'s body expanded |

Severity: the issue's own statement, "no fix is urgent", holds. Every extra change is what document formatting would also do. None of them drops, relocates, or changes a comment, a directive, or a literal. No output failed to compile or needed a second pass (all fixtures: 1 action, 1 iteration, except RH5110 `a.cs`, which had 2 diagnostics). No safety rail is hit. The extra changes reach syntax forms owned by other rules (expression body to block: RH3202/RH3208; attribute split: RH5517), not only whitespace. So the scope violation is larger than "layout", but it is still only canonical formatter output.

The issue's structural cause is confirmed by reading the spec, not just asserted. FMT-PIPE-017 keeps the first token of a `FormatNodeInDocumentWithContextAsync` target on its line and in its column. No entry point can therefore move the first token of the changed code onto a new line: `where` (RH5110), the `{` of the reported block (RH5403), the `{` of the type body (RH5404). Today these fixes get that line break only by formatting the enclosing node.

### Mechanism and candidates

Three separable mechanisms, each with a membership test that can be decided. The dispatch is the set of `FormatNodeInDocumentAsync` call sites in `src/Reihitsu.Analyzer.CodeFixes` (27 files, listed with `rg -n "FormatNodeInDocument(WithContext)?Async"`).

**M-A — leading trivia of a target that starts its line.** A fix that passes `FormatNodeInDocumentAsync` a target that starts its line and is not a direct child of the root gets the target's leading trivia decided by the phases (FMT-PIPE-010 keeps it only otherwise). The blank-line phase inserts or removes blank lines above the target or its leading comment. The documentation-comment phase rewrites a documentation comment in it. The indentation phase re-indents a comment line in it. Defining type: `ReihitsuFormatter.FormatNodeInDocumentAsync`, with the facts "preceding token" and "list position" of FMT-PIPE-024. Candidates are every fix whose target starts its line:

| Fix (target) | Measured input | Result |
|---|---|---|
| RH5403 (statement) | `a = 1;⏎// c⏎if …`; `a = 1;⏎if …` (no comment); `case 1:⏎a = 2;⏎if …` | fails: blank line inserted in all three |
| RH5403 | `a = 1;⏎⏎      // c` (comment at column 6) | fails: the comment is re-indented to column 8 |
| RH5403 | `#if DEBUG … #endif` directly above | holds: no blank line |
| RH5404 (type) | `}⏎// c⏎sealed  class B {…}` | fails: blank line above `// c` |
| RH5404 | `}⏎class B {…}` (no comment) | holds |
| RH5404 | one-line `/// <summary>B</summary>` above | fails: rewritten to three lines |
| RH5206 (statement) | `status++;⏎return status switch…` | fails: blank line |
| RH5110 (method declaration) | `int _a;⏎// c⏎void M<T>() where …` | fails: blank line above `// c` |
| RH5111 (expression statement) | `int b = 0;⏎// c⏎b⏎= 1;` | fails: blank line above `// c` |
| RH5113, RH5401, RH5408 (member) | `int _a;⏎// c⏎<member>` | fails: blank line above `// c` |
| RH5113, RH5401, RH5408, RH5111 (member) | one-line documentation comment | fails: rewritten to three lines |
| RH5412 (type in a file-scoped namespace, `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`) | `namespace N;⏎// c⏎class A { }` | fails: blank line above `// c` |
| RH5417 (member, `FormatMemberOnLinesOfItsOwnAsync`) | `int _a;⏎// c⏎void M() { M(); }` | fails: blank line above `// c` |
| RH3202, RH3203 (expression-body fixes; they format only the new block) | `int _a;⏎// c⏎<member> => …` | holds |
| RH5413–RH5416 (same base as RH5412), RH2001, RH3004, RH5205, RH5301–RH5308, the fluent-chain context fallback | — | not measured; same entry point. The expression targets (RH5205, RH5301–RH5308, RH3004) start their line only when wrapped |

One mechanism covers #1748, the RH5206 blank line, the four documentation-comment instances (#1120 Q3, #1121 Q1, RH5401, RH5408), RH5412's P14a instance, and the RH5417 region instances of the 2026-11-07 comment (blank line between `#region getters` and `P`; not re-measured).

**M-B — the formatted node is larger than the reported construct.** Two causes. They need different repairs.

- *A local choice.* RH5111's `GetFormattingNode` takes `FirstAncestorOrSelf<AssignmentExpressionSyntax>()` before it looks for a nearer `VariableDeclaratorSyntax`, `ParameterSyntax`, `PropertyDeclarationSyntax`, or `EnumMemberDeclarationSyntax`. So a declarator or parameter inside the value of an assignment formats the whole assignment. Class: reported kind {declarator, parameter default; property and enum member cannot occur inside an assignment value} × enclosing value {assigned lambda, assigned anonymous method, object-initializer member assignment `new C { X = () => { … } }`}. Measured failing: declarator in an assigned lambda, parameter default of a local function in an assigned lambda. Not measured: the anonymous-method and object-initializer cells, which take the same path. Holding: an assignment inside an assigned lambda (the inner assignment is found first), and a declarator in a field-initializer lambda (no assignment ancestor).
- *The changed code's first token moves to a new line.* RH5110 (clauses), RH5403 (block; header, other branches), RH5404 (header). No entry point can do this without formatting the enclosing node (FMT-PIPE-017, above). The RH5113 instance is not of this kind: the semicolon joins the previous token, so no column is needed.
- *A spec that orders the larger node.* RH5112/RH5201 (SHARED-CHAIN-009) and RH5206 ("innermost statement" sentence of RH5206-003): output questions 1 and 3.

**M-C — the interior of the reported construct.** RH5101/RH5102 elements, the members of RH5404, and the statements of RH5403's block are formatted as a whole. This is output question 2.

### Breaking inputs

Failure classes that reach the change: FC-030 (the issue itself), FC-015/FC-016 (one decision, many fixes), FC-020/FC-021 (comments and directives in the leading trivia), FC-027 (enclosing scopes for RH5111), FC-043 (`FindUnsupportedSyntax` must be asked about the node the formatter receives once a target narrows: RH5403 `:74–76` and RH5110 ask about the statement or declaration today), FC-007 (each narrowed fix must still clear its own diagnostic in one application), and the tab-width matrix (below).

Inputs the test implementer must cover for M-A, per fix touched, under LF and CRLF:

- a comment line directly above the target, behind a statement or member (fails today);
- for RH5403 and RH5206, no comment at all (fails today: blank line above the statement);
- inside a switch section behind another statement (fails today);
- a comment line in another column than the target (re-indented today);
- a one-line documentation comment (rewritten today);
- a directive line directly above the target (holds today; must keep holding);
- a blank line already present above the comment (must stay one blank line);
- the target directly behind an opening brace with a blank line between them. FMT-PIPE-010 removes that blank line today (`ReihitsuFormatterNodeBlankLineTests.RemovesBlankLineBetweenOpeningBraceAndRewrittenFirstMember`). Under SHARED-FIX-006 a fix keeps it;
- a `/** */` documentation comment written behind the code in front of the target (`a = 1; /** Doc */⏎a = 2;`). The entry point moves it to its own line today (FMT-PIPE-010, first exception; `SeparatesDelimitedDocumentation…` tests). RH5111-004 already keeps it for context targets;
- tab indentation in front of the target. Today a tab in the not-kept leading whitespace counts as four columns, and the fix writes spaces: `\t\tif (a > 0) { a++; }` became `        if (a > 0)` with the block at 8. Once the leading trivia is kept, SHARED-FIX-007's first bullet makes the tab count as one column, so the block's lines are laid out from column 2. The input class violates RH5601 and is rare. The cell must still be pinned, and it must be one of the two outcomes the spec allows (laid out from column 2, or the fix withheld for a tab, as SHARED-BRACE-003 does for the brace fixes).

For M-B (RH5111): the measured declarator and parameter cells, the anonymous-method and object-initializer cells, and the two holding cells.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Code fixes RH5403, RH5404 (#1748), RH5110, RH5111, RH5113, RH5206, RH5401, RH5408, RH5412–RH5416, RH5417 (M-A) | must change |
| RH5111 `GetFormattingNode` (M-B, local) | must change |
| RH5113 fix (move only the semicolon) | must change, if the RH5113-003 rewrite is kept |
| RH5110, RH5403 header and branches, RH5404 header (M-B, structural) | must stay as on `main` unless the lead takes escalation 9 (see Architecture findings) |
| RH5112/RH5201, RH5206 scope, RH5101/RH5102 and RH5403/RH5404 interior | must stay until output questions 1–3 and 5 are answered |
| `ReihitsuFormatter.FormatNodeInDocumentAsync` and FMT-PIPE-010/FMT-PIPE-024 | involved only if the M-A owner is the entry point (Internal choices) |
| Analyzers | not involved |
| Fix All | not involved: every fix uses `BatchFixer` |
| Rule pages | not involved: nothing the user is told changes |
| `Reihitsu.Core` | not involved |

### Refuse option

- **M-A.** Withholding is no real option: the class is every fix applied below a statement or member, which is code people commonly write. The requested change (keep the target's leading trivia) needs no new type. It is either one change to the entry point or one shared step in the code-fix layer (Internal choices). The cost is small against the class.
- **M-B, RH5111.** A local change to the node choice. There is nothing to refuse.
- **M-B, structural (RH5110, RH5403, RH5404).** Refusing means withholding the fix wherever formatting the enclosing node would change code outside the reported part. That reads the formatter's result, but it withholds the fix for any type or statement that is not already formatter-clean, which is common. That is escalation 9, not a local refusal. Keeping `main`'s behavior leaves these instances on #1140, which the issue already allows ("fixed narrowly with a cheap guard or stays as on `main`").
- **Declaring a shape unsupported.** Not an option. No shape is involved: the defect is the scope of what a fix writes back, not a construct the formatter cannot handle.
- **Simpler option.** Narrow the run to M-A and the RH5111 node choice (and RH5113's semicolon), and link #1140 with `Refs`.

### Output questions

Questions 1–3 of `01-spec-author.md` stand. Two notes for the lead before asking them:

- **Question 1.** "After, A" on #1140 shows `x == 1`, i.e. the chain's whole line formatted. SHARED-FIX-006 literally gives only the chain changed (`if (x==1 && a.B() != null)`; not measured). That was not offered as an option. Under SHARED-CHAIN-009 the chain-only result is rejected because its first token's column differs from the formatted context's. Consider offering it as option C, or as a replacement for A. No entry point formats a statement's header without its body, so A as written needs new machinery, and C needs none.
- **Question 2.** The `1+2` example understates what "interior" means for RH5403 and RH5404. There the interior includes syntax-form rewrites owned by other rules (question 5 below). The spec author states that the answer to question 2 decides RH5403/RH5404 too. Ask it separately, or with question 5's example, so the answer is given on the real stakes.

New, within the run's issues:

**4 · RH5110 · Generic type constraints should be on their own line — opening brace on the clause's line**

Input:
```csharp
class C<T> where T : class {
    int _x;
}
```

Before (`main`):
```csharp
class C<T>
    where T : class
{
    int _x;
}
```

After, A (what RH5110-003 as rewritten in d1884a0 says: the body's braces stay as written):
```csharp
class C<T>
    where T : class {
    int _x;
}
```

After, B (recommended):
```csharp
class C<T>
    where T : class
{
    int _x;
}
```

Why B: the brace shares the line the fix lays out, so leaving it there gives a line that formatting the result changes again, and that a brace rule reports.

Only needed if RH5110's scope is narrowed in this run. If RH5110 stays as on `main` (see Refuse option), revert the RH5110-003 hunk (D4) and skip this question.

**5 · RH5404 · Element must not be on a single line — members that other rules would rewrite**

Input:
```csharp
class B { int M() => 1; [System.Obsolete] int _x; }
```

Before (`main`):
```csharp
class B
{
    int M()
    {
        return 1;
    }
    [System.Obsolete]
    int _x;
}
```

After, A (recommended):
```csharp
class B
{
    int M() => 1;
    [System.Obsolete] int _x;
}
```

After, B: as `main`.

Why A: the expression body and the attribute's line belong to RH3202 and RH5517, which report them and offer their own fixes.

The same holds for RH5403, measured: `if (a > 0) { int N() => 1; a += N(); }` converts `N` to a block body and inserts a blank line on `main`.

### Internal choices

1. **Owner of M-A.** A: `FormatNodeInDocumentAsync` keeps the target's leading trivia in every case, as FMT-PIPE-018 does for the context entry point. This is one owner. It means amending FMT-PIPE-010 (and the now unused "preceding token", "list position", and documentation-boundary facts of FMT-PIPE-024 for this entry point). It changes formatter tests that pin today's behavior (`ReihitsuFormatterNodeBlankLineTests`: `InsertsBlankLineAboveCommentOfRewrittenProperty`, `RemovesBlankLineBetweenOpeningBraceAndRewrittenFirstMember`, `SeparatesDelimitedDocumentation…`, and others). B: a shared code-fix helper restores the leading trivia after formatting, and each fix calls it. FMT-PIPE-010 is untouched, but this adds a second owner of the same decision (FC-015), and it is subject to the tab cell above. Recommendation: **A**. Code fixes are the entry point's only callers (`rg` finds no other caller in `src`), and FMT-PIPE-010's own `Why` names SHARED-FIX-006 as its reason. So today FMT-PIPE-010 contradicts the requirement it claims to serve. Either way, nothing visible beyond SHARED-FIX-006 and M1 is in question.
2. **Restated scope sentences.** d1884a0 writes "the leading trivia stays as written" into eight rule requirements separately (RH5110-003, RH5111-004, RH5113-003, RH5206-003, RH5401-003, RH5403-003, RH5404-003, RH5408-005). That is one rule stated in eight places, and RH5412–RH5416 and RH5417 still lack it. Recommendation: state it once (SHARED-FIX-006: the leading trivia of the node a fix formats is not part of what it formats, a documentation comment included). Rule specs then keep only the `Uses` entry, the removal of the contradicting text, and their examples.
3. **D4 granularity.** RH5403-003 and RH5404-003 each mix an M-A sentence (leading trivia, M1) with an M-B sentence (header, other branches). If M-B stays as on `main`, split the hunk: keep the M-A part and revert the header and branch sentences together with the examples that need them (RH5403 "other branch stays"; the `sealed  class` spacing in the RH5404 example).

### Issue goals

- "Each input above changes only the code its diagnostic is about": change it to the M-A instances, the RH5111 instances, and RH5113. The structural instances (RH5110, RH5403/RH5404 header and branches) need escalation 9 or stay. The instances behind questions 1, 2, 3, and 5 wait for answers. No visible output changes from narrowing; the PR uses `Refs #1140`.
- "The rule specs state the narrowed scope": change it to removing the contradicting text and referencing SHARED-FIX-006 (Internal choice 2). Internal.
- The remaining-instance list (RH5303–RH5308, RH5405–RH5407, RH3201, RH5412–RH5416) is not part of the spec author's work and was mostly not measured here. RH5412 is an M-A member, and RH5413–RH5416 share its base. Treat RH5303–RH5308 and RH3201 as unmeasured, not as reproduced.

### Architecture findings

- **The leading trivia of a fix's target has two owners with opposite answers.** FMT-PIPE-010/FMT-PIPE-024 make `FormatNodeInDocumentAsync` decide it (blank lines, documentation comments, comment columns). SHARED-FIX-006 (unchanged) and, after d1884a0, eight rule specs forbid every fix from changing it. The fixes are the entry point's only callers. Shown by every failing M-A row above. This reaches the run through #1748 directly.
- **No entry point writes back only the changed code when its first token moves to a new line** (FMT-PIPE-017). RH5110, RH5403, and RH5404 can be narrowed only by computing the line break and column inside the fix, which predicts the indentation phase (question 10 of the role). #1140 records a reverted ~300-line attempt of exactly that. The alternative is a write-back of part of a formatted node: one shared mechanism, likely more than 100 lines or a new type. Either way it is escalation 9. The fluent-chain helper already compares two formatter results to choose what to write back (`FluentChainCodeFixHelper.FormatChainAsync`), so a per-fix splice would be a further copy of that decision.

### Follow-up candidates

- FU1 of `01-spec-author.md` (SHARED-FIX-007 for a gap-only fix) stays outside the run. None of #1687's measured members depend on it: each of them lays out the member's lines.
- RH5303–RH5308 and RH3201 scope instances: unmeasured. They stay on #1140.

## thoenissen/Reihitsu#1687

### Premise

Real for the confirmed case, and the class is wider than the issue. Reachable through each fix in the IDE.

The issue's fallback argument is wrong: "RH5417 reports a member sharing a line" is false. RH5417 reports a body's braces. The 2026-11-14 comment's claim that `int _a; public C() => _a = 1;` violates RH5417 is wrong for the same reason; RH5417 does not report it (spec author's finding, confirmed by reading RH5417's reporting condition in its spec). The comment's RH3208 case (a local function sharing a line with a statement) is outside SHARED-FIX-007, which covers members only. It is not a defect, so drop it.

### Mechanism and candidates

Mechanism: a fix that changes a member's lines decides the shared line only where it calls `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (RH3202, RH5417). The attribute fix decides it in a private branch that handles only a sibling in front (`TargetAttributePlacementCodeFixProviderBase.cs`, the `SharesLineWithSiblingMember(member, previousToken, openBracket)` branch). Every other member-changing fix does not decide it at all.

Class: fix × side of the sibling {in front, behind} × whether the member's line begins with a member (the qualifier of RH3202-002/RH5417-003).

| Fix | Sibling behind | Sibling in front |
|---|---|---|
| RH5517 (field attribute, separate line) | fails: `[Obsolete]⏎int _a; int _b;` | holds |
| RH5513 (method attribute) | fails: `[Obsolete]⏎void M() { } int _b;` (that input also violates RH5417) | not measured; same branch as RH5517 |
| RH3203, RH3205, RH3207 (expression body to block) | fails: `} int _b;` | RH3203 fails: `int _a; public C()`, with the block laid out from column 12 |
| RH3206 | not measured; same base `ExpressionBodyToBlockCodeFixProviderBase` | — |
| RH3202 | holds (`FormatMemberOnLinesOfItsOwnAsync`) | — |
| RH5401 | fails: `int A => 1; int _b;` | — |
| RH5408 | — | fails: `int _b; int A { get; set; }` |
| RH5113 | — | fails: `int _b; int _a;` |
| RH5404 (nested type) | fails, both sides: `int _a; class B` with the body from column 12 and `} int _b;` | (same fixture) |
| RH5110, RH5412–RH5416, RH2001 | not measured | — |

The issue says the class is the "fixes that format a member". The measured class is every fix whose result leaves the member it changes sharing a line with a sibling. That includes the join fixes (RH5401, RH5408, RH5113), where both members already shared a line before the fix.

### Breaking inputs

FC-016, FC-015 (the attribute fix's private copy of the shared-line decision), FC-025 (the seven expression-bodied kinds; local functions are excluded as statements), FC-019/FC-021 (a comment in the gap to the sibling: `SharesLineWithSiblingMember` requires whitespace only, so the line stays shared; that must be the intended outcome and be pinned), and the qualifier boundary (the member's line begins with a non-member, such as a type header written on one line). Every measured cell, under LF and CRLF.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Attribute separate-line fixes (RH5501–RH5530 family through the base) | must change (sibling behind) |
| `ExpressionBodyToBlockCodeFixProviderBase` (RH3203, RH3205, RH3206, RH3207) | must change |
| RH5401, RH5408, RH5113, RH5404 fixes | must change, or withhold (Refuse option) |
| RH3202, RH5417 | must stay |
| SHARED-ATTR-008, SHARED-BODY-003, or the rule specs | must change if the RH3202-002 qualifier is copied (spec author's note) |
| Analyzers, rule pages, Fix All | not involved |

### Refuse option

Withhold each fix when `SharesLineWithSiblingMember` holds: a cheap guard per base, and a few lines. Is the class rare? Two members on one line is a shape people seldom write. No rule reports it (#1446), but "violates another rule" is a sufficient condition for rare, not the definition, so the class qualifies as rare. The requested change costs a call-site change for the fixes that already format through the formatter: the existing helper takes the formatting step as a delegate. For the attribute fix, which writes its own trivia (SHARED-ATTR-008), it costs either reusing the helper or growing its private branch (FC-015). Declaring a shape unsupported is not an option: the formatter handles the shape, and only the fixes do not.

### Output questions

_None._ SHARED-FIX-007 (unchanged) decides the separated output, and withholding for this rare class puts nothing in question.

### Internal choices

Recommendation: route the formatter-delegating fixes (the expression-body base, RH5401, RH5408, RH5113, RH5404) through `FormatMemberOnLinesOfItsOwnAsync`, as RH3202 does. For the attribute fix, reuse the same helper rather than extending its private branch. If that does not fit SHARED-ATTR-008's own column, withhold the fix for a sibling behind (rare). Alternative: withhold everywhere. That is cheaper, but it gives up safe ground the helper already covers.

### Issue goals

"Required output … as the RH5417 and RH3202 fixes do": keep it, and widen it to the measured failing fixes. "Where a fix's input already violates another rule (RH5417 …)": drop this reasoning, because its premise is false. The rarity argument above stands on its own. No visible output beyond SHARED-FIX-007 is in question.

### Architecture findings

The shared-line decision (SHARED-FIX-007) has two owners, `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` and the private branch in `TargetAttributePlacementCodeFixProviderBase`. They already disagree: the private branch handles a sibling in front only. Shown by RH5517's `a_after`/`b_before` fixtures. The run's change newly depends on this, so it is in scope.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1748

### Premise

Real. The issue's input under RH5403 gives today's output exactly as reported, under LF and CRLF. RH5404 with `}⏎// c⏎sealed  class B { int _x; }` inserts the blank line too. M1 (maintainer, answer A) decides the output. Reachable: RH5403 fix → `FormatNodeInDocumentAsync(document, statement)` (`RH5403…CodeFixProvider.cs:42`); RH5404 fix → `FormatNodeInDocumentAsync(document, declaration)` (`:35`).

The issue understates the class. For RH5403 a comment is not needed: `a = 1;⏎if (a > 0) { a++; }` also gets a blank line above `if`, also inside a switch section. A misindented comment above the statement is re-indented. M1's wording ("the leading trivia, blank lines included, stays as written") already covers all of these, and so does RH5403-003 as rewritten. For RH5404, the no-comment case holds today.

### Mechanism and candidates

M-A under #1140; #1748's members are the RH5403 and RH5404 rows there.

### Breaking inputs

The M-A list under #1140, for RH5403 and RH5404. The issue's "Done when" asks for one test per rule with a comment line. The no-comment RH5403 case, the switch-section case, and the misindented comment fail today too and need tests of their own.

### Surfaces

RH5403 and RH5404 code fixes: must change. Everything else: as under #1140, M-A.

### Refuse option

None that is cheaper. The class is every RH5403/RH5404 fix below another statement or member, which is common code, and the change needs no new type.

### Output questions

_None._ M1 decides them. One effect of M1 to state in the PR: the misindented comment above the statement now keeps its column (the output changes from `main`), and M1's "leading trivia … stays as written" covers it.

### Internal choices

The M-A owner (#1140, Internal choice 1).

### Issue goals

"A code-fix test per rule for a comment line above the target": keep it, and add the no-comment and switch-section RH5403 cases (internal).

### Architecture findings

The first M-A finding under #1140.

### Follow-up candidates

_None._

## Whole run

- Fixtures were in the session scratchpad. All runs exited 0, with no `not-converged`, `invalid-result`, or `line-ending-drift`.
- No repository file was changed besides this handover.
