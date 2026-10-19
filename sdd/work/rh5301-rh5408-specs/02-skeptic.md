Role: skeptic
Commit: 9d288818ca26cf22ecc396498d6bff8ec36fe465
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane: specs and tests for existing behavior. I checked the spec diff (commit above) against the analyzers, the code-fix providers, `InitializerLayoutAnalysisUtilities`, `StatementBlockParentPolicy`, `StatementBracesCodeFixProviderBase`, and the `Reihitsu.Core` predicates they call.

Most requirements state what the code does. Seven places do not. In some, the spec states a case that looks wrong as intended behavior. In others, the wording describes a different condition than the code checks. The spec author has to repair them before tests are derived. None of them needs a maintainer decision now: each is a spec correction, a case decided by a #912 principle, or a non-blocking open question to file.

How the facts were established: `scripts/apply-fix.sh RH#### <dir>` (LF and CRLF, both identical in every case below) and `scripts/format.sh --dry-run <dir>` on fixtures in the scratchpad, deleted afterwards. "Fix no-progress" means the fix was offered and changed nothing. All fixtures sit inside `class C { … }` with the parameters the code needs.

## Premise

### S1 — SHARED-INIT-001 states a case decided by P12 as intended (repair the spec, file the case)

The first bullet says: "A comment that ends on the token's line MUST start at the expected column". The intro says "MUST check every comment in front of that token". `IsAlignedAt` checks every comment in the token's leading trivia against the expected column. So two comments in front of a member, the first one in its column, are reported:

```csharp
var c = new C
        {
            /* a */ /* b */ P = 1
        };
```

RH5301 reports `P = 1`. The fix is offered and changes nothing (no-progress). `scripts/format.sh` leaves the line unchanged. The line's first element, `/* a */`, is in the member column, so P12 says the line is not reported. The offered no-op fix also breaks SHARED-FIX-005 (P7). The same predicate serves RH5303 and RH5304.

The spec has to leave the case of more than one comment on the token's line out. The lead files it in `SDD open questions: sdd/specs/shared/initializer-layout.md`, marked "Decided by principle P12 (#912)", also P7 (SHARED-FIX-005).

### S2 — SHARED-INIT-001 gives the wrong column for a comment that starts above a closing brace and ends on its line (repair the spec)

The code chooses the extra level by where the comment *starts*: any comment that does not start on a closing brace's line is expected one level deeper. The spec's first bullet covers a comment that ends on the token's line, with no extra level. Its second bullet adds the extra level only for a comment "on a line of its own".

```csharp
var c = new C
        {
            P = 1
            /* a
            b */ };
```

This is not reported: the comment starts at the column of `new` plus 4, and the brace behind it is not checked. With `/* a` at the column of `new`, RH5301 reports the creation, and the fix moves `/* a` to the column of `new` plus 4. Under the spec's first bullet the outcomes would be reversed. The bullets have to state the code's distinction: a comment that starts on an earlier line than a closing brace starts one level deeper, whether or not it ends on the brace's line.

### S3 — "On the line of the element before it" is not the condition the code checks (repair five requirements; file the RH5305 case)

RH5303-002, RH5304-002, RH5305-002, RH5306-002, and SHARED-INIT-002 all say an element is misplaced "on the line of the element before it". RH5303 and `FindFirstComplexElementMisalignment` use a `HashSet` of start lines; RH5304, RH5305, and RH5306 use `Distinct()`. So the code reports an element only when it starts on the line where an *earlier element starts*. An element that starts on the last line of a multi-line element before it is not caught:

```csharp
int[] a = [
              N(1,
                2), 3
          ];
```

RH5305 does not report this. The formatter puts `3` on its own line (FMT-BREAK-022), so analyzer-clean code is changed (FC-013).

- **RH5305:** the wording decides the output, so the requirement has to be reworded to the code's condition. The case is filed in `SDD open questions: sdd/specs/analyzer/RH5305.md`, marked "Decided by principle P11 (#912)": a gap inside the construct the rule names.
- **RH5303, RH5304, SHARED-INIT-002:** the element-column check reports such an element in any practical layout, so only the wording is wrong.
- **RH5306:** unaffected in practice, because a list pattern with a multi-line inner pattern is exempt as a whole. The wording should still match.

Five requirements state the same condition with five private implementations. The spec author may consider one shared statement of it (`sdd/README.md`, shared behavior). That is optional; the wording fix is not.

### S4 — RH5305-001 states a case decided by P12 as intended (repair the spec, file the case)

The analyzer compares the raw character columns of `[` and `]`, with no comment anchor:

```csharp
int[] a = [
              1
          /* end */ ];
```

RH5305 reports this. The fix is offered and changes nothing (no-progress), and `scripts/format.sh` leaves it unchanged. The line's first element, the comment, is in the column of `[`, so P12 says it is not reported. The no-op fix breaks SHARED-FIX-005 (P7).

RH5305-001 ("its closing bracket does not start at the column of its opening bracket") currently requires the report. The spec has to leave out a closing bracket that a comment precedes on its line. File the case in the RH5305 issue, marked "Decided by principle P12 (#912)", also P7. RH5306 is not affected, because any comment exempts the list pattern.

### S5 — The RH5306-001 exemption is worded wrongly (repair the spec)

"In front of its opening bracket on the line before it" does not match the code. `ContainsNonDocumentationCommentOrDirective` walks the list pattern's own trivia: from the leading trivia of `[` through the trailing trivia of `]`. So:

- A comment that trails the token in front of the pattern does not exempt it. `var b = v is // c⏎        [1,⏎         2];` is reported, and the fix is offered and applied.
- A comment on a line of its own between that token and `[` does exempt it. `var b = v is⏎        // c⏎        [1,⏎         2];` is not reported.
- By the same code, a comment in front of `[` on `[`'s own line, when `[` starts that line, also exempts it.

The wording has to name the comments the code counts: those in front of `[` after the token that precedes the list pattern.

### S6 — RH5307-004 claims a fix that is not offered (narrow the spec, file a normal question)

The fix is withheld for these inputs, all reported by RH5307:

- `var a = x[m[1,⏎            2]];`
- `var a = j[0 /* c */][1,⏎                     2];`
- `var a = j[N(1,⏎            2)][1,⏎                2];`

`scripts/format.sh` joins the reported list in the first and the third, e.g. `x[m[1, 2]]` and `j[N(1,⏎            2)][1, 2]`. With a plain receiver (`j[0][1,⏎ 2]`) the fix is offered and joins.

Mechanism: the provider does not guard the reported list. It re-resolves a list from the diagnostic's first token, which for an element access is the receiver's first token. It takes the first bracketed argument list among that token's ancestors, otherwise the argument list of the innermost element access among them, and applies the comment and single-line guard to *that* list. So the guard inspects another list:

- whenever the reported access sits inside another bracketed argument list;
- whenever the receiver starts with an element access.

The fix is withheld when that other list fails the guard.

RH5307-004 has to be narrowed to the inputs where the fix is offered, the way RH5401-003 is narrowed to properties without attribute lists. File a normal open question in `SDD open questions: sdd/specs/analyzer/RH5307.md`: guard scope (`AGENTS.md`, Code-fix coverage), since the fix refuses inputs it could rewrite safely.

### S7 — SHARED-BRACE-002 states the `else if` layout, decided by P10, as intended (repair the spec, file the case)

"The braces start their own lines at the column of the `if`" is literally what the code does for an `else if` body, but document formatting does something else. Input:

```csharp
if (a)
{
    M(a, b);
}
else if (b)
    M(a, b);
```

The RH5405 fix produces `{`, `M(a, b);`, `}` at the column of the inner `if` keyword (`else` column + 5, with the body 4 deeper). `scripts/format.sh` on the original puts the braces at the `else` column. On the fix result, it moves them back there.

The cause: the owner is the inner `if`, which does not start its line, and node-level formatting lays it out from its own column. The same holds for RH5406 and RH5407, which share the base class.

RH5403's fix behaves the same way. For `else if (b) { M(a, b); }` the block lands at the inner `if` column. For `else if (b)⏎{⏎    M(a, b);⏎}⏎else { M(a, b); }`, where the `else` block is reported, the fix also re-indents the correct `else if` body and moves the `else` to that column (P9 as well).

SHARED-BRACE-002 has to state the column only for an `if` that starts its line, and leave the `else if` case out. RH5403-003 does not state a column and can stay. File the case in `SDD open questions: sdd/specs/shared/statement-braces.md` and in the RH5403 issue, marked "Decided by principle P10 (#912)". P10's requirement, SHARED-FIX-007, is still planned in #1138.

### S8 — RH5404-003 says every member is put on its own line, which is false for enums (repair the spec and the rule page)

For `enum E { One, Two }` the fix produces `enum E⏎{⏎    One, Two⏎}`. The formatter deliberately keeps enum member lists as written (`formatter/line-breaks.md`, Non-goals). `enum F { }` becomes `enum F⏎{⏎}`, which agrees with the semicolon sentence because enums are not listed there. RH5404-003 has to say what happens to enum members.

`documentation/rules/RH5404.md` ("Expand the declaration so the opening brace, each member, and the closing brace are on their own lines. The code fix applies this change automatically") is wrong for enums in the same way. That is a rule-page correction, not an open question: neither the analyzer nor the formatter asks for one enum member per line.

### What holds

I traced each of the following through the code, and where marked, confirmed it by running:

- **RH5301-001 to -003, RH5303-001 to -004, RH5304-001 to -003:** braces, columns, and diagnostic locations, apart from S1 to S3.
- **RH5302-001 to -005:** a run confirmed that the `a⏎|| b⏎   && c` block is not reported and is formatter-stable.
- **RH5308-001 to -003:** the code skips a single-line conditional even when it contains a nested conditional. The spec words RH5308-001 so that it avoids stating that case (Q4).
- **RH5401, RH5402, RH5405-001/-002, RH5406, RH5407, RH5408-001 to -004:** confirmed against the code.
- **SHARED-BLOCK-001** matches `StatementBlockParentPolicy` exactly.
- **SHARED-BRACE-001** matches the three analyzers.
- **RH5403-001's empty-block exemption** is documented intent on the rule page.
- **The formatter leaves these not-reported examples alone:** the RH5308-002 `z` block, the SHARED-INIT-002 `{ 1, 2 }`/`{ 3, 4 }` part, RH5307-002's first block, and RH5408-002's wrapped signature and wrapped initializer value (the formatter does not join the accessor list there).

The SHARED-INIT-001 not-reported example gets blank lines inserted before its line-leading comments, but columns are unchanged and RH5301 still does not report it. That blank-line insertion is the #1148 class and outside this run.

Three of the spec author's open questions were reproduced:

- statement-braces Q1: the fix produces `} // trail`, duplicating the comment;
- RH5304 Q1: the formatter expands `L = { 1, 2 }`;
- the empty `L =⏎{⏎}`: Roslyn parses it as an object initializer, so it is not reported, and the formatter leaves it.

Notes for the lead on filing:

- The open questions cite SHARED-FIX-006 and SHARED-FIX-007. These do not exist yet in `shared/code-fixes.md`; #1138 (open) adds them, so the question texts should say "planned in #1138".
- RH5306 Q1 (the comment exemption) and Q3 (the multi-line inner-pattern exemption) are gaps inside the construct RH5306 names, which is P11's "own gap" clause as adopted on #912. The lead decides whether they are marked "Decided by principle P11 (#912)" or stay normal questions. RH5304 Q1 and RH5308 Q4 are different: the rule page states the opposite intent explicitly, so they are genuine two-reading questions as filed.

## Mechanism and candidates

_N/A_: there is no bug premise in this lane. The one mechanism found (RH5307 fix guard) is stated in S6.

These are the dispatch sets I checked, so the test implementer can derive candidates from them:

| Rule | Registered on | Further filter |
|---|---|---|
| RH5301 | `ObjectInitializerExpression` | parent `ObjectCreationExpressionSyntax` or `ImplicitObjectCreationExpressionSyntax`; elements `AssignmentExpressionSyntax` |
| RH5303 | `CollectionInitializerExpression` | same parents; complex elements are `ComplexElementInitializerExpression` |
| RH5304 | `ObjectInitializerExpression` | same parents; assignments whose right side is `CollectionInitializerExpression` (an empty `{ }` is an object initializer and is skipped) |
| RH5302 | `LogicalAndExpression`, `LogicalOrExpression` | outermost by direct parent; a parenthesized operand starts its own chain, and the fix's "outermost chain" stops at parentheses as well |
| RH5305 | `CollectionExpression` | — |
| RH5306 | `ListPattern` | exempt when a non-`///` comment or directive is in its full trivia, or an inner pattern is multi-line |
| RH5307 | `BracketedArgumentList` | parent `ElementAccessExpressionSyntax` or `ImplicitElementAccessSyntax`; `ElementBindingExpression` (`?[]`) is excluded (Q1) |
| RH5308 | `ConditionalExpression` | outermost by direct parent; nesting only through `WhenTrue` and `WhenFalse` directly |
| RH5401 | `PropertyDeclaration` | with expression body and no accessor list |
| RH5402, RH5403 | `Block` | `StatementBlockParentPolicy.IsCovered` |
| RH5404 | the class, struct, interface, record, record struct, and enum kinds, plus `ExtensionBlockDeclaration` | extension block is Q1 |
| RH5405 to RH5407 | `IfStatement` | — |
| RH5408 | `PropertyDeclaration` | with accessor list and no expression body |

## Breaking inputs

- **FC-003:** test each column and line condition on both sides: brace or element at the expected column versus one column off; one-line versus two-line initializer, collection expression, list pattern, indexer list, conditional, and type body; an `=` gap with and without a line break (RH5408-003).
- **FC-019, FC-020, FC-021:** comments and directives in every gap a guard inspects:
  - RH5302-005: operator trailing trivia versus a comment in front of the operator;
  - RH5306: each trivia position named in S5;
  - RH5307-002: inside the brackets versus behind `]`;
  - RH5408-003: accessor list, signature gap, initializer, `=` gap, `;` gap, behind the declaration;
  - RH5402-003: SHARED-GAP-001;
  - SHARED-INIT-001: a comment before an opening brace, an element, and a closing brace, `///` continuation lines, and the S2 shape.
- **FC-026:** `else if` chains and nested `if` bodies for the brace fixes and RH5403 (S7). Use only the requirements as repaired.
- **FC-027:**
  - nested complex elements (SHARED-INIT-002);
  - conditionals nested in a branch, and conditionals inside parentheses or arguments, which form a separate outermost conditional;
  - logical chains split by parentheses;
  - element accesses with element-access receivers or inside other indexer lists (S6);
  - fix targets in a field initializer, an expression-bodied member, an argument, a lambda, and a switch arm. The RH5305, RH5306, and RH5307 targets climb to the nearest such node, and the RH5308 target climbs to the enclosing statement or member.
- **FC-028:** the RH5401 fix guard measures detached `FormatNode` output, while the fix applies `FormatNodeInDocumentAsync`. Cover a property with and without a documentation comment, and an explicit interface implementation (`int I.P⏎    => 1;`).
- **FC-007, FC-009:** assert that no fix is offered for withheld cases. Do not tag the no-op shapes of S1 and S4, RH5302 Q1, RH5308 Q1/Q2, or the #1150 directive body as fix requirements; characterization only.
- **FC-030:** fix examples have to avoid code outside the reported construct (P9 instances listed by the spec author).
- **FC-013:** for each not-reported example, the formatter must not change it in a way the rule would report.
- **FC-033:** LF and CRLF for every fix test (apply-fix showed identical results on both).
- **FC-017:** assert exact fix output.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Specs in the diff | Must change: S1 to S8 |
| `documentation/rules/RH5404.md` | Must change: enum members (S8) |
| Other rule pages | Must stay (no further error found) |
| RH5301 to RH5308 and RH5401 to RH5408 analyzers | Must stay (lane: no production change) |
| Their code-fix providers, Fix All (`BatchFixer` everywhere) | Must stay |
| `InitializerLayoutAnalysisUtilities`, `StatementBlockParentPolicy`, `StatementBracesCodeFixProviderBase` | Must stay |
| `Reihitsu.Core` (`SyntaxNodeUtilities`, `SyntaxTriviaUtilities`, `DocumentationCommentUtilities`) | Not involved; no spec claims them |
| Formatter | Not involved |
| Tests | Must change: tagged tests derived from the repaired specs (next role) |

## Simpler option

_None_ for the run as a whole.

## Questions for the maintainer

_None_ that block the run. The new cases from S1, S3, S4, S6, and S7 go into the per-spec open-questions issues as described above:

- S1, S4: P12, also P7;
- S3 (RH5305): P11;
- S7: P10;
- S6: a normal question.
