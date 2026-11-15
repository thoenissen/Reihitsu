Role: skeptic
Commit: 8c6bd7e
Verdict: proceed (wider than the issue's wording: all seven fixes, and both sides of the header split), with one output question for a rare member

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Real, and reached by a caller. The caller is each of the seven code fixes RH7201–RH7207. Each registers `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`, which calls `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync` with the reported directive's scope (the block namespace). That entry point runs `UsingDirectiveOrderingPhase` alone, and FMT-PIPE-030 makes its output final. The input is an ordinary document: a block namespace with usings inside, a comment above the first directive, and directives out of order.

Reproduced with `scripts/apply-fix.sh RH7203`, LF and CRLF, on the issue's input. The fix writes:

```csharp
namespace N
{
    // Header
    using System;
using System.Linq;

    class C { }
}
```

Without the comment, the same block is fixed at column 4. Whole-document formatting (`scripts/format.sh`, run twice, stable) writes column 4 for every directive in every failing member below, so the CLI is not affected: the indentation phase repairs the column there. The defect shows only through the code fixes.

Checked against the specs:
- SHARED-FIX-007 is unchanged and decides the result. It says the changed code is laid out as the formatter lays it out.
- FMT-PIPE-030 says the phase's output is the fix's final layout.
- Every FMT-USING-009 example sits in column 0, which is why no test catches this.
- I reconstructed each failing fix result and formatted it. For every block-namespace member it differs from the formatted text only in the one line's leading whitespace. The other differences are the trailing newline, `class C { }` → `class C;`, and in b3 the blank line after `{`, which SHARED-FIX-006 keeps. So no blank-line or order question arises.

Not a regression. Before thoenissen/Reihitsu#1581 (`git show a59a003^`), `OrganizeScopeUsingsAsync` ran the same phase on the same scope in the same way.

## Mechanism and candidates

Mechanism: the original first directive starts its own line and carries a comment in its leading trivia, and the reorder moves it away from the top. The header split of FMT-USING-009 (`UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia`, used at `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives`) then hands the whitespace in front of that directive's token to only one of the two directives that need it:
- **No blank line between comment and directive:** the whole tail, comment plus indentation, becomes the header that the new first directive inherits. The moving directive keeps an empty leading trivia.
- **A blank line among the comments:** the header ends at that blank line, and the indentation stays with the moving directive.

The other directive is still counted as starting its line (`directivesStartingTheirLine` holds the detached directive, and the first slot never takes `scopeIndentation`). So it gets no indentation from the scope either and lands in column 0. When the new first directive has a leading comment of its own, that comment lands in column 0 instead.

Dispatch: `UsingDirectiveOrderingRewriter` visits `CompilationUnitSyntax`, `NamespaceDeclarationSyntax`, and `FileScopedNamespaceDeclarationSyntax`. The defect is visible only where the first directive's line is indented, which in practice means a block namespace, at any nesting depth. The callers are the seven providers: `UsingDirectiveOrderingCodeFixProviderBase` (RH7201–RH7206) and `RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider`. All of them route through `OrganizeScopeUsingsAsync`. RH5114's fix does not use the phase; it computes the indentation itself. `UsingDirectiveOrderingSafety.CanSafelyReorder` refuses only blocks that contain directives, so every comment form reaches the phase.

## Breaking inputs

Failure classes that reach this code:
- FC-016: only one side of the split is reported; the other side fails too.
- FC-027: the compilation-unit scope at column 0 hides the defect.
- FC-033: a formatter-level test through `FormatDocument` normalizes the defect away, because the indentation phase repairs it. The reproduction must go through the code fix, or through `OrganizeUsingDirectivesInDocumentAsync`.
- FC-034: the reported shape itself.
- FC-017: LF and CRLF.
- FC-008: Fix All. The base class demands a `GetFixAllScenario`.
- FC-019 and FC-021: comment forms.
- FC-031: the summaries of `SplitOriginalFirstHeaderTrivia` and `CreateLeadingTrivia` describe the split.

FC-004 does not apply: no comment is lost or relocated. FC-020 does not apply: blocks with directives get no fix.

Class matrix. All block-namespace fixtures are 4-space indented unless noted, and each was run under LF and CRLF with identical results.

| # | Axis values | Fixture (first two directives as written) | Result today |
|---|---|---|---|
| a | `//` header, no blank line, original first moves to last | issue input | **fails**: moved directive in column 0 |
| c | `/* Header */` on its own line | same | **fails** |
| c1 | `/// Header` | same | **fails** |
| c2 | `/** Header */` | same | **fails** |
| c4 | two `//` lines | same | **fails** |
| c5 | multi-line `/* … */` ending on its own line | same | **fails** |
| b3 | blank line between `{` and the header | same | **fails** (the blank line itself stays, as SHARED-FIX-006 wants) |
| f | three directives, `Text` (original first) moves to last, `Linq` keeps its slot | `Text, Linq, System` | **fails** for `Text` only |
| g | nested block namespace (8 columns) | issue input one level deeper | **fails**: column 0 |
| nb | moves into another group (RH7201, RH7202, RH7205, RH7207) | e.g. `Zeta.Alpha; System;` | **fails**: column 0 behind the group's blank line |
| h | `// Header`, blank line, then the directives | `Linq; System;` | **fails**: the new first directive `using System;` in column 0 |
| b1 | header, blank line, `// About Linq` on the original first | `Linq; System;` | **fails**: `using System;` in column 0 |
| b2 | header, blank line, new first carries its own `// About System` | `Linq; // About System System;` | **fails**: `// About System` in column 0, `using System;` at 4 |
| bl | header, blank line, for RH7201/7202/7204/7205/7206/7207 | per rule | **fails**: the new first directive in column 0 |
| nb per rule | RH7201 `Zeta.Alpha; System`, RH7202 `A = System.Text; System`, RH7204 `B = …; A = …`, RH7205 `static System.Math; System`, RH7206 `static Math; static Console`, RH7207 `Zeta.Alpha; System` | no blank line | **fails** for all six |
| c3 | `/* c */ using System.Linq;` (comment on the directive's line) | | holds: the directive does not start its line, so it gets the scope indentation |
| e | the comment leads the second directive | | holds: the first directive keeps its slot |
| b | no comment | | holds |
| d, s2, s3 | compilation unit or file-scoped namespace at column 0, with or without a blank line | | holds (indistinguishable) |
| t1 | tab-indented block namespace | | **fails**: column 0. The same block without the comment keeps the tab. See *Output questions* 2 |
| s1 | file-scoped namespace with 4-space-indented usings and header | | column 0, while the neighbours stay at 4. The same block without the comment writes 4. Whole-document formatting writes 0 for every line. See *Output questions* 1 |
| u1 | compilation unit with 4-space-indented usings and header | | same as s1: column 0, and 4 without the comment |

Candidate inputs the test implementer must cover:
- **Reproduction (code-fix path, LF and CRLF):** a; one delimited form (c); one documentation form (c1 or c2); the blank-line side (h, b1, and b2 for the comment line); a move into another group (nb); f (middle versus last slot); and g (nesting).
- **One block-namespace test per other rule (RH7201, RH7202, RH7204, RH7205, RH7206, RH7207):** all six reproduce.
- **Holds, which must stay green:** b, c3, e, and the column-0 scopes d and s2.
- **Fix All:** two block namespaces in one document, each with a header and misordered usings.
- **Formatter guard:** whole-document formatting of a and h stays at column 4 and unchanged.

For every member, the expected text is the fix's current output with the column-0 line indented as the scope's other directives are. That is also the same block's fix result without the comment, with the comment kept where FMT-USING-009 puts it. I confirmed this by formatting each reconstructed result.

No layout rule reports the column-0 line: RH5204 (`apply-fix.sh RH5204`) reports nothing on the fix result. So this is not FC-007.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `UsingDirectiveOrderingPhase` (rewriter and `UsingLeadingTriviaBuilder`) | Must change. It owns the block's layout, and FMT-PIPE-030 makes its output the fixes' output |
| `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync` | Must stay. Computing a column here would predict the indentation phase (question 10). The phase already takes the indentation from the document's own whitespace (SHARED-USING-010) |
| Whole-document formatting / CLI | Must stay. Its output is already correct (column 4) and must not change |
| Code fixes RH7201–RH7207, `UsingDirectiveOrderingCodeFixProviderBase`, `UsingDirectiveCodeFixUtilities` | Must stay. The issue's "Affected" names them as the possible cause; they only route |
| Fix All (BatchFixer) | Must stay; covered by a test |
| Analyzers RH7201–RH7207 | Not involved |
| RH5114 code fix | Not involved: it does not use the phase |
| `Reihitsu.Core` (`UsingDirectiveOrderingUtilities`, `UsingDirectiveOrderingSafety`) | Not involved |
| Rule pages | Not involved |
| Specs | SHARED-FIX-007 decides the block-namespace members, so no change is needed for them. A clarification in FMT-USING-009 is optional (*Internal choices*) and needed only if the lead wants the s1/t1 answer written down |

## Refuse option

Withhold the seven fixes when the original first directive starts an indented line, carries a line-starting leading comment, and is not the canonical first. That is a cheap syntactic guard of about 5–10 lines in one shared place.

Withholding costs about as much as correcting it: the correction is confined to the header split in one phase. The class is not rare in the sense of `AGENTS.md`. Usings inside a block namespace are a common convention, StyleCop's SA1200 default among them. A comment above the first using is a common file shape. Refusing would leave an RH72xx diagnostic without a fix for code people commonly write, which is escalation 9 territory. Not recommended.

Declare the shape unsupported: the shape would be a comment in the leading trivia of the first using directive of a namespace. It deletes nothing, because the header split also serves the compilation unit, where the same shape is the ordinary file header. It would also refuse common code. A shape that deletes nothing is not declared, so this option is not escalated.

Simpler options: none. The narrowest change is in the phase. Changing the code fixes or the entry point would put a second owner of the block's layout next to the phase.

## Output questions

Two rare members whose fix output changes depending on how the mechanism is closed, and that no requirement decides alike. They do not block the block-namespace tests (4-space indentation), which SHARED-FIX-007 decides.

**1 · RH7203 · Using directives must be ordered alphabetically by namespace — indented usings with a header comment outside a block namespace**

Input:
```csharp
namespace N;

    // Header
    using System.Linq;
    using System;

class C { }
```

Before (`main`):
```csharp
namespace N;

    // Header
    using System;
using System.Linq;

class C { }
```

After, A (recommended):
```csharp
namespace N;

    // Header
    using System;
    using System.Linq;

class C { }
```

After, B: unchanged from `main`. Whole-document formatting later moves every directive and the comment to column 0.

Why A: without the comment the fix already writes this block with both directives at 4 columns, and a comment above the block should not change where a directive stands.

**2 · RH7203 · Using directives must be ordered alphabetically by namespace — tab-indented block namespace with a header comment**

Input (`→` is a tab):
```csharp
namespace N
{
→   // Header
→   using System.Linq;
→   using System;

→   class C { }
}
```

Before (`main`):
```csharp
namespace N
{
→   // Header
→   using System;
using System.Linq;

→   class C { }
}
```

After, A (recommended):
```csharp
namespace N
{
→   // Header
→   using System;
→   using System.Linq;

→   class C { }
}
```

After, B: `using System.Linq;` with four spaces in front, as whole-document formatting writes every line.

Why A: without the comment the fix already keeps the tab on the moved directive, and the fix does not replace a tab in front of the code it changes.

The same rule applies to RH7201, RH7202, and RH7204–RH7207, which share the output.

## Internal choices

- **Spec clarification.** The lead could have FMT-USING-009 state that the directive taking the first slot and the directive leaving it each start where the original first directive's line started, with a block-namespace example. This changes no output beyond what SHARED-FIX-007 and the output questions decide. Recommend: add it only if the answer to question 1 is A. Otherwise SHARED-FIX-007 and the tests carry the block-namespace case, and the bug-fix lane needs no spec author.
- **Test placement.** Choose between per-rule code-fix tests (seven test classes) and an RH7203 matrix plus one test per other rule. Recommend the second: one entry point and one cause, and every provider is still exercised.

## Issue goals

- **"the moved directive at the namespace indentation".** Widen this to every line of the organized block. On the blank-line side (h, b1, bl), the column-0 line is the new first directive or its leading comment, not the moved one. The fix output for those members changes from column 0 to 4, which SHARED-FIX-007 demands, so no visible output is in question.
- **"and the same text that whole-document formatting produces".** Narrow this to the using block's lines. Formatting the whole file also rewrites `class C { }` to `class C;`, removes the final newline, and removes a blank line after `{` that the fix keeps under SHARED-FIX-006. Nothing visible changes.
- **"For each … other using-ordering fix that reproduces the bug".** Keep it: all six reproduce, and each gets a test.
- **"Affected" naming `UsingDirectiveCodeFixUtilities` and `UsingDirectiveOrderingCodeFixProviderBase`.** Drop it as a location. They are only the route; the cause is in the formatter phase.
- The issue's required output for its own input is correct: I checked it against whole-document formatting.

## Architecture findings

None. One component, the using phase, decides the indentation of a directive in two branches (`scopeIndentation` for a split-off directive, and the trivia carried through the header split). That is a local defect, not two components owning one decision.

Not a new finding: FMT-PIPE-030 deliberately makes the using phase's indentation final for the fixes while the indentation phase owns the column in whole-document formatting. That design is why the defect is invisible to the CLI.

## Follow-up candidates

None.
