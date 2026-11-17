Role: skeptic
Commit: 1684b9b59df6368abbb9cbf1a2a7a93b3addf3cb
Verdict: #889: proceed narrower; #1476: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Fixtures lived under the session scratchpad and are not part of the repository. `apply-fix.sh` ran every fixture under LF and CRLF; unless stated otherwise, both arms gave the same status and diff, and every "fixed" result below is "1 action(s), 1 iteration(s)".

## thoenissen/Reihitsu#889

### Premise

Reachable: yes. The RH5201 and RH5112 code fixes reach a chain in a statement that starts mid-line through `FluentChainCodeFixHelper.FormatChainAsync` (`src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs`). Every shape below reports RH5201 or RH5112, and the fix is offered.

The issue's premise is **stale**. It was written for #887. #1760 (commit 4666dbb) later changed the helper to `FormatNodeInDocumentWithContextAsync`. That entry point passes the document column of the chain's first token as a fact (`targetFirstTokenColumn`) and the context's own document column (`rootFirstTokenColumn`), and it writes back only the chain (FMT-PIPE-017, FMT-PIPE-018; `src/Reihitsu.Formatter/ReihitsuFormatter.cs:286-364`, `FormatContext` at `:641`). So the choice of context no longer decides any column of the chain. A misspaced token in front of the mid-line statement (`return  a`) gives no column difference whichever context is chosen.

I ran a mutation check. A detached worktree in the scratchpad, since deleted, removed the clause `&& SyntaxTokenPositionUtilities.IsFirstOnLine(node.GetFirstToken())` from `GetContextNode`. The same fixtures ran through `apply-fix.sh` on HEAD and on the mutant, and the two outputs were compared with `diff`:

- 11 layout fixtures, each under LF and CRLF (listed below): the outputs were **byte-identical** on both builds.
- 2 refusal-scope fixtures (below): the outputs **differ**. The only observable effect of the clause that I found is the scope of the SHARED-FIX-010 refusal. The clause makes the whole line-starting statement the context, so an unsupported shape anywhere in that statement withholds the fix. Without the clause, only the mid-line statement is checked.

No expected value below comes from the mutant (FC-035). All of them come from HEAD.

### Mechanism and candidates

_N/A_ (no defect). The decision in question: `GetContextNode` returns the nearest `StatementSyntax` or `MemberDeclarationSyntax` ancestor whose first token starts a line, or `null`. With `null`, the fix calls `FormatNodeInDocumentAsync` on the chain. The context is used twice, for the layout and for `IsFormattingRefused`.

Candidates that make the nearest statement or member start mid-line, from the syntax kinds that can share a line with code in front of them:
- an embedded statement behind `if (…)`, `else`, a loop header, or `using (…)`. `if` is the representative, since all of them are `StatementSyntax` behind a header token.
- a statement behind a `case` label (`SwitchSectionSyntax` is not a statement, so the climb reaches `SwitchStatementSyntax`);
- a statement inside a lambda block that opens mid-line (`BlockSyntax` is itself a `StatementSyntax` that starts mid-line, so the climb passes it as well);
- a top-level `GlobalStatementSyntax` behind a using directive on the same line. Here no ancestor starts a line, so the context is `null` and the fix takes the `FormatNodeInDocumentAsync` branch.

### Breaking inputs

Classes walked: FC-027 (enclosing scope), FC-030 (fix formats more than its node), FC-007 (convergence), FC-017 (LF and CRLF), FC-043 (refusal asked about the node the formatter receives), FC-003 (both sides of the boundary). None fails. Everything holds on HEAD.

Layout fixtures, run with `scripts/apply-fix.sh RH5201 <dir>` and `scripts/apply-fix.sh RH5112 <dir>`. Every fixture: fixed, converged in one iteration, code in front of the chain kept as written (the double space in `return  a` and the `if(k)` / `case 1:` / `()=>{ var r=` text stay). The output is identical with and without the clause.

RH5201, `if` embedded statement:
```csharp
class C
{
    object M(dynamic a, bool k)
    {
        if(k)return  a.B()
            .C();

        return null;
    }
}
```
→ `.C();` in column 22, under the `.` of `.B()`:
```csharp
        if(k)return  a.B()
                      .C();
```

RH5201, `case` label:
```csharp
class C
{
    object M(dynamic a, int k)
    {
        switch (k)
        {
            case 1:return  a.B()
                .C();
        }

        return null;
    }
}
```
→
```csharp
            case 1:return  a.B()
                            .C();
```

RH5201, lambda block opened mid-line:
```csharp
using System;

class C
{
    void Run(Action a)
    {
    }

    void M(dynamic a)
    {
        Run(()=>{ var r=a.B()
            .C(); });
    }
}
```
→
```csharp
        Run(()=>{ var r=a.B()
                         .C(); });
```

RH5201, top-level statement behind a using directive (null context):
```csharp
using System;  object r = ((dynamic)1).B()
    .C();
```
→
```csharp
using System;  object r = ((dynamic)1).B()
                                      .C();
```

RH5112, the same three statement shapes with the first link wrapped (`return  a` / `.B()` / `.C();`, likewise for `case 1:` and `Run(()=>{ var r=a` / `.B()` / `.C(); });`). The first link is joined and the next link aligned, for example:
```csharp
        if(k)return  a.B()
                      .C();
```
```csharp
            case 1:return  a.B()
                            .C();
```

RH5201, wider lines inside the chain (FC-027: multi-line sibling), all identical with and without the clause:
- `if(k)return  a.B(1,` / `2)` / `.C();` → `2)` in column 25, `.C();` in column 22;
- `if(k)return  Run(() =>` / `{` / `return 1;` / `}).B()` / `.C();` → the braces in column 25, `return 1;` in 29, `.C();` in 27;
- the same behind `case 1:` → the braces in column 31, `.C();` in 33;
- `if(k)return  a.B((Func<int, int>)(x =>` / `{` / `return x;` / `}))` / `.C();` → the braces in column 42, `.C();` in 22.

Refusal-scope fixtures (FC-003, the side of the boundary that the clause decides), run with `scripts/apply-fix.sh RH5201 <dir>`:
```csharp
class C
{
    object M(dynamic a, bool k, bool j)
    {
        if(k && /* c */ j)return  a.B()
            .C();

        return null;
    }
}
```
```csharp
using System;

class C
{
    void Run(Action a, int b)
    {
    }

    void M(dynamic a)
    {
        Run(()=>{ var r=a.B()
            .C(); }, 1 + /* c */ 2);
    }
}
```
HEAD: `no-fix-offered` for both, under LF and CRLF. The comment between a binary operator and its right operand is FMT-UNSUP-011, and it stands in the line-starting context but outside the mid-line statement. Mutant: both `fixed`, with `.C();` aligned under `.B()`. A test on one of these fails when the clause is removed. No layout test does.

The existing test `NoFixIsOfferedWhenTheChainContextHoldsACommentTheFormatterRefuses` (`RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs:2555`) puts the refused comment in a statement that starts its line. Its nearest statement is also its context, so it does not observe the clause.

No existing RH5201 or RH5112 test places a chain in a statement that starts mid-line. I checked with `rg` for `if (…) return`, `case …:` statements, and `{ var` inside the two test classes.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5201 / RH5112 analyzers | must stay |
| RH5201 / RH5112 code-fix providers, `FluentChainCodeFixHelper` | must stay |
| Fix All | not involved |
| Formatter (`FormatNodeInDocumentWithContextAsync`) | must stay; not involved beyond being called |
| `Reihitsu.Core` | not involved |
| Rule pages | not involved |
| Specs | must stay. No spec states how the context is selected (searched `fluent-chain-layout.md`, `RH5201.md`, `RH5112.md`). SHARED-FIX-010 says only that the fix asks about "the context" |
| Tests (`RH5201…AnalyzerTests.cs`, `RH5112…AnalyzerTests.cs`) | must change: add tests |

### Refuse option

_N/A_ as a refusal: nothing is defective, and the run adds tests only. No shape is a candidate for declaring unsupported here; FMT-UNSUP-011 is already declared. A simpler option: drop the "fails when the clause is removed" criterion and keep only the mid-line layout tests. See *Internal choices*.

### Output questions

_None._ Every output pinned above is today's output and follows SHARED-CHAIN-008, SHARED-CHAIN-009, and SHARED-FIX-006. The refusal-scope fixtures withhold a fix for a rare class (a comment that FMT-UNSUP-011 refuses), which is a supported end state.

### Internal choices

1. **How to meet "the test fails when the `IsFirstOnLine` clause is removed".**
   - A: Add one refusal-scope test, the `if(k && /* c */ j)return  a.B()` fixture above, expecting no fix offered, tagged SHARED-FIX-010. It is the only test that observes the clause. It also covers FC-043 for a mid-line statement: the check asks about the same context that the formatting would receive. Consequence: it pins a refusal that is wider than the precheck strictly needs (see *Follow-up candidates*). If someone later narrows the context, the test flips, and that flip has to be decided then.
   - B: Drop the criterion. The mid-line tests then pin SHARED-CHAIN-009 but cannot detect the removal of the clause.
   - **Recommendation: A**, together with the layout tests. It is the only real observable of the decision the issue wants pinned, and it pins today's SHARED-FIX-010 behavior.
2. **Which mid-line layout shapes.** I recommend the `if` embedded statement and the `case` label for RH5201, plus the lambda-block shape (`Run(()=>{ var r=a.B()`) for one of the two rules, each under LF and CRLF, with `return  a` kept as the spacing defect in front. These pin SHARED-FIX-006 (the code in front stays as written; the formatter would make it one space) rather than a column choice. The top-level null-context shape is optional: it is the only input that reaches the `FormatNodeInDocumentAsync` branch of `FormatChainAsync`, which no test may cover today. I did not check that coverage gap with `rg` beyond the two test classes.

### Issue goals

- "Spacing defects in front of the chain so that a wrong context choice produces a visible column difference": the purpose is stale since #1760, because no context choice changes a column. Keep the misspacing for SHARED-FIX-006 and drop the stated reason. Changes no visible output.
- "The test fails when the `IsFirstOnLine` clause is removed": no layout test can meet it. Only a refusal-scope test can (*Internal choices* 1). Changes no visible output.
- "Converges in one application, re-analysis clean": keep; it holds for every fixture.
- The issue's statement that the clause decides the positional context for the layout is stale. The PR should say that the clause's remaining observable effect is the refusal scope.

### Architecture findings

_None._

### Follow-up candidates

- **The context of the chain fix is wider than its layout needs.** Since #1760 the line-starting context feeds no column. Its only observable effect is that an unsupported shape anywhere in the line-starting statement withholds the fix, even when the mid-line statement that holds the chain could be formatted. Example: the `if(k && /* c */ j)return  a.B()` fixture gives no fix on `main`, and with the nearest statement as context the result would be `.C();` aligned in column 35. The class is rare (it needs a declared unsupported shape), and the change would be visible (a fix offered where one is withheld), so it would be an output question in the follow-up. The doc comment of `GetContextNode` / `FormatChainAsync` ("formatted as positional context") states a reason that no longer decides any column (FC-031). Low priority; the lead decides whether to file it.

## thoenissen/Reihitsu#1476

### Premise

Reachable: yes. Whole-document formatting and the CLI produce the comment-led layout, RH5201 analyzes any code, and the RH5201 fix reaches it through `FluentChainCodeFixHelper`. No RH5201 test covers a chain whose root stands behind a comment that leads its line: `rg` in `RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs` found no line that starts with a comment followed by a chain root. Formatter-side coverage exists in `src/Reihitsu.Formatter.Test/Regression/Indentation/CommentLedFirstTokenAnchorTests.cs`.

The issue's snippet indents the comment line to column 12. The formatter puts that line in column 8, the column of `return` (`sdd/changes/2026-10/phase-prediction-removal.md:56,61`), and `.ToUpper` in column 17 under `.Trim`. The issue's "column 17" is right only for the formatter's column-8 layout. The tests should use the formatter's actual output, quoted below, not the issue's snippet.

I found **no disagreement** between RH5201, its fix, and the formatter. Nothing has to be filed as a spec bug.

Formatter, run with `scripts/format.sh <dir>` twice (the second pass is stable):
```csharp
class C
{
    string M(string s)
    {
        return
            /* c */ s.Trim()
             .ToUpper();
    }
}
```
→
```csharp
        return
        /* c */ s.Trim()
                 .ToUpper();
```
With a multi-line comment's last line in front of `/* c */` (input `/* a` / `   b */ /* c */ s.Trim()` / ` .ToUpper();` at column 12):
```csharp
        return
        /* a
               b */ /* c */ s.Trim()
                             .ToUpper();
```
The interior line of the comment stays as written. `.ToUpper` is aligned under `.Trim` where `.Trim` stands (column 29).

### Mechanism and candidates

_N/A_ (no defect). The axes I settled: rule (RH5201 analyzer, RH5201 fix, and RH5112 analyzer and fix as a neighbor) × what leads the line (a single-line `/* c */`, or a multi-line comment's last line followed by `/* c */`, or a multi-line comment's last line directly in front of the root) × layout (formatter output, old layout).

### Breaking inputs

Classes: FC-013 (analyzer and formatter agree on formatter output), FC-021 (`/* */` ending mid-line in front of the token), FC-007, FC-017. All hold.

Run with `scripts/apply-fix.sh RH5201 <dir>`, LF and CRLF:

| Fixture (lines after `return`) | Result |
|---|---|
| `        /* c */ s.Trim()` / `                 .ToUpper();` (formatter output) | no-diagnostic |
| `        /* a` / `           b */ /* c */ s.Trim()` / `                         .ToUpper();` (formatter output) | no-diagnostic |
| `            /* a` / `            b */ s.Trim()` / `                  .ToUpper();` (formatter output, tail directly in front of the root) | no-diagnostic |
| `            /* c */ s.Trim()` / `                     .ToUpper();` (comment at 12, aligned) | no-diagnostic |
| `        /* c */ s.Trim()` / `         .ToUpper();` (old layout, `.ToUpper` at 9) | fixed → `.ToUpper` at 17, equal to the formatter output of the same input |
| `        /* a` / `           b */ /* c */ s.Trim()` / `         .ToUpper();` (old layout) | fixed → `.ToUpper` at 25, equal to the formatter output of the same input |
| `            /* c */ s.Trim()` / `             .ToUpper();` (comment at 12) | fixed → `.ToUpper` at 21; the comment line stays at 12 (SHARED-CHAIN-009) |
| `            /* a` / `               b */ /* c */ s.Trim()` / `             .ToUpper();` | fixed → `.ToUpper` at 29 |
| `            /* a` / `            b */ s.Trim()` / `             .ToUpper();` | fixed → `.ToUpper` at 18 |

The old-layout fixture in full, with RH5201 reported on the `.` of `.ToUpper` and the fix's result:
```csharp
class C
{
    string M(string s)
    {
        return
        /* c */ s.Trim()
         .ToUpper();
    }
}
```
```csharp
class C
{
    string M(string s)
    {
        return
        /* c */ s.Trim()
                 .ToUpper();
    }
}
```

RH5112 neighbor, run with `scripts/apply-fix.sh RH5112 <dir>`: `        /* c */ s` / `            .Trim()` / `            .ToUpper();` → fixed to `        /* c */ s.Trim()` / `                 .ToUpper();`. The multi-line variant gives `           b */ /* c */ s.Trim()` / `                         .ToUpper();`. Both equal the formatter output of the same input. This is optional coverage; the issue names only RH5201.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5201 analyzer, RH5201 code fix, `FluentChainCodeFixHelper` | must stay |
| RH5112 | not involved (neighbor holds; optional test) |
| Formatter | must stay; not involved |
| Specs (RH5201-002, RH5201-005, SHARED-CHAIN-007/-009, SHARED-COMMENT-011/-012, FMT-INDENT-029/-030) | must stay; all agree with the code |
| Rule page | not involved |
| Tests (`RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`) | must change: add the three cases |

### Refuse option

_N/A_: tests only, nothing to refuse, and no shape is a candidate for declaring unsupported. The simpler option is the issue's own, three tests.

### Output questions

_None._

### Internal choices

1. Use the formatter's column-8 layout from the change doc as the "formatter output" case, not the issue's column-12 snippet. Recommended, because the fixture then equals what the CLI produces. The column-12 variant is an optional extra no-diagnostic case.
2. "Old layout" means `.ToUpper` in column 9 (the change doc's "Today" column) behind a comment at column 8. Recommended.

### Issue goals

- "Tests pin the three cases": keep.
- "Any disagreement found is filed as a spec bug against RH5201": none found; nothing to file.
- The snippet's column-12 comment line is not formatter output (see *Premise*). Use the measured output. Changes no visible output.

### Architecture findings

_None._

### Follow-up candidates

_None._
