Role: skeptic
Commit: afa1548
Verdict: #1633: proceed narrower; #1634: proceed narrower; #1647: proceed; #1685: proceed; #1542: output question; #1929: output question

Inputs ignored as instructed: thoenissen/Reihitsu#1929 names a suspected cause and a fix location (`PatternAnchor`'s fallback), and `00-lead.md` records the coordinator's expectation that #1929 needs no output question. Neither was used; the anchor code was read only to derive the candidate set.

Every fixture below was run with `scripts/format.sh <dir>` twice (second pass identical unless stated), on afa1548, whose formatter equals `main`. Fixtures lived under the session scratchpad and are quoted inline.

Run-wide fact that changes the rarity of several questions: RH5104 (*Comments must be on their own line*) reports every comment that shares a line with code, and RH5043 (base list), RH5044 (attribute list), and RH5046 (type parameter, type argument, tuple list) report every comment inside those constructs, with or without a line of its own. Every comment-held input of #1633 question 2 and of #1634 therefore violates another Reihitsu rule and is rare by definition (`AGENTS.md`, *Special cases do not grow the source*). The comment-on-its-own-line inputs of #1647 violate none of them.

## thoenissen/Reihitsu#1633

### Premise

Reachable: CLI and whole-document formatting on any type whose base list is wrapped behind its `:`; code fixes RH5412–RH5418 that format a type declaration (change doc). The issue's "Before" (column 0) is stale: since FMT-INDENT-034 the first base type takes the continuation indent, column 4 — the issue's own reading B. The issue's motivation ("no requirement places the first one") is no longer true; what remains is a layout preference, which is a legitimate output question for the no-comment shape (question 1), because that shape violates no rule and is written in real code.

Observed (fixture `b1633.cs`, abridged):

```csharp
class G<T> :
    I1 where T : class
{ int _x; }
class O
    :
    I1,
    I2
{
}
struct S :

    I1
{
}
class M :
#if DEBUG
    I1
#else
    I2
#endif
{
}
class P : I1
    , I2
    , IDisposable
{ public void Dispose() { } }
```

gives `class G<T> :⏎    I1⏎    where T : class`, `class O :⏎    I1,⏎    I2;` (FMT-BREAK-041 joins the `:`), `struct S :⏎⏎    I1;` (blank line kept), the `#if` block unchanged, and the comma-first list unchanged (`    , I2`). Stable on a second pass.

### Mechanism and candidates

_N/A_ (behavior change). Candidates for FMT-BREAK-042, from `BaseListSyntax` owners: class, struct, record, record struct, interface, enum declarations, with and without a primary constructor, with a `PrimaryConstructorBaseTypeSyntax` first (`Base(1)`), with a constraint clause behind the list, with the `:` itself on its own line (FMT-BREAK-041 then FMT-BREAK-042 on one gap sequence), with blank lines in the gap, nested types.

### Breaking inputs

- FC-019/FC-020/FC-021 (FMT-BREAK-042 joins a gap): `/* c */` behind the colon, `// c` behind the colon, a comment on its own line between `:` and the first type, and the `#if DEBUG … #else … #endif` fixture above. Under FMT-INDENT-036 the active branch's type (here `I2`, since no symbol is defined) moves to column 10 while the disabled `I1` keeps column 4 (FMT-INDENT-011); under question 2 = B all stay in column 4. None found pinned.
- FC-027: `class O⏎    :⏎    I1` (two joins in sequence), `struct S :⏎⏎    I1` (blank lines go with the join), `class G<T> :⏎    I1 where T : class`, `class K() :⏎    Base(1), I1`, a nested type. None found pinned.
- FMT-BREAK-038 base-list commas: `class P : I1⏎    , I2⏎    , IDisposable` (two leading commas), and a comment behind a leading comma (`class C : I1⏎    , /* c */ I2`). A comment in front of a later-line comma is already refused by FMT-UNSUP-009, which covers every comma token, base lists included. None found pinned.
- FC-010/FC-017: LF and CRLF, second pass.
- Existing tests that keep today's shape outside the formatter: `RH6021ColonsMustBeSpacedCorrectlyAnalyzerTests` (`VerifyOnlyMissingLeadingSpaceIsFixedWhenTrailingSideIsLineBroken` and the `class E :⏎    B` / `class F :⏎    B` inputs) pin RH6021's own fix output, which does not go through the formatter; they stay unchanged. No formatter test pins a base list wrapped behind its colon (searched `tests/Reihitsu.Formatter.Test`, `tests/Reihitsu.Cli.Test`).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Line-break phase (FMT-BREAK-042, FMT-BREAK-038 base list) | must change |
| Indentation phase (`BaseTypeListContributor`, FMT-INDENT-036) | must change only if question 2 = A |
| Code fixes RH5412–RH5418 | not involved in code; output follows the formatter |
| RH5043, RH6021 analyzers and rule pages | must stay |
| Rule pages | not involved |

### Refuse option

- Comment-held first base type (question 2): leave it where FMT-INDENT-034 puts it today (column 4), i.e. question 2 = B. Cost: zero production lines and no FMT-INDENT-036; the later base types still align under the first (FMT-INDENT-016), and the `#if` case stays consistent across branches. The class is rare by definition (RH5043 reports every comment in a base list; RH5104 every end-of-line comment). FMT-INDENT-036 is a new requirement and a contributor change that serves only this class.
- Declaring unsupported (escalation 10, must be asked, not decided): the spec author recorded "add the gap to FMT-BREAK-041 and FMT-UNSUP-017" as an *internal choice*. Declaring a shape unsupported is never internal. Exact shape: a comment, directive, or disabled text between a base list's `:` and the first token of its first base type (trailing trivia of the `:` and leading trivia of that token), whatever lines it spans. It deletes nothing that exists on `main`. With question 2 = A it would replace FMT-INDENT-036 and FMT-BREAK-042's blocking sentence. With question 2 = B there is nothing to delete, and *a shape that deletes nothing is not declared*. It would refuse `class C : // why` files, which RH5043 already reports. Recommendation: answer question 2 with B, which makes the declaration moot. If the lead keeps A as the recommendation, the declaration is a third option of question 2.
- Question 3 (comma-first): cheap extension of an existing requirement (FMT-BREAK-038); no simpler option.

### Output questions

Question 1 and question 3 as drafted are sound. Question 2: same input, but the recommendation should be B (unchanged). Why B: RH5043 already reports the comment, and the base types then keep one column in every build configuration. If A stays recommended, show the declare-unsupported option as C, in the form `AGENTS.md` prescribes for it.

### Internal choices

- FMT-BREAK-042 as a new requirement rather than a gap of FMT-BREAK-041 — agree, given that FMT-BREAK-041's gaps are refused under FMT-UNSUP-017. The alternative is the unsupported shape above, which is not internal.
- The enum example in FMT-BREAK-042 also drops the trailing `A,` comma (a structural transform). That is correct output but mixes two requirements into one example. Recommend removing the trailing comma from the input.

### Issue goals

- "Before: column 0" — stale; drop. No output change.
- Reading B of the issue *is* today's output; the question's "After, B: unchanged" is the honest form. No output change.
- "Each input gets a regression test with the idempotency check" — keep.

### Architecture findings

_None._

### Follow-up candidates

- P11, a different construct: no rule reports a base list wrapped behind its colon, and RH6021's fix writes `class TestClass :⏎    System.IDisposable`, which the CLI will then join. This is analyzer-clean code the formatter changes, a new-rule candidate of low value. It is noted for the lead, and I recommend no issue.

## thoenissen/Reihitsu#1634

### Premise

Reachable through the CLI and whole-document formatting. Both members the issue names are held by a comment, and every such input is reported by another rule: `List< // c` and `IComparable< // c` by RH5046 and RH5104, `[ // c` by RH5044 and RH5104, `System. // c` by RH5104. The issue's "Before" (block column 4) is stale: FMT-INDENT-034 now places these lines (issue comment). What still looks wrong today is narrow:
- the constraint `where T : System. // c⏎IDisposable` puts `IDisposable` (column 12) left of `System` (column 18);
- a comment-held first type argument stands at the continuation column, not behind `<`.

Both are rare by definition.

FMT-INDENT-038 as written reaches far more than the comment-held lines. A qualified name wrapped behind a dot *without* any comment is not joined anywhere except in a constraint and inside argument-, parameter-, attribute-, tuple-, and type-argument-like list elements (FMT-BREAK-015). Observed (fixtures `q1634.cs`, `q1634b.cs`; every line below kept as written, at the continuation column):

```csharp
using X = System.
    Text;

namespace Foo.
    Bar
{
    class C : System.IDisposable
    {
        void System.
            IDisposable.Dispose()
        {
        }

        object M(object o)
        {
            var a = (System.
                IDisposable)o;
            return default(System.
                IDisposable);
        }
    }
}
```

and, in a method body, `var t = typeof(System.⏎    IDisposable);`, `var s = new System.Text.⏎    StringBuilder();`, `System.⏎    IDisposable b = null;`, `global::System.⏎    IDisposable g = null;` — all unchanged. `void M(System.⏎IDisposable a)` and `where T : System.⏎IDisposable` are joined. `class B : System.⏎IDisposable` is not joined and aligns `IDisposable` with `System` (FMT-INDENT-013).

These no-comment wraps violate no rule, and FMT-INDENT-038 moves every one of them whose name starts mid-line. Derived from the requirement text, not run — four columns right of the outermost name's first token:
- `Text` from column 4 to 14 (using alias);
- `Bar` from 4 to 14 (namespace);
- `IDisposable.Dispose()` from 12 to 17 (explicit interface);
- cast from 16 to 25;
- `default(…)` from 16 to 31;
- `typeof(…)` from 12 to 27;
- `new System.Text.⏎StringBuilder()` from 12 to 24.

None of them is in question 5, in the change doc's *Visible output changes*, or in the issue. A name that starts its line (member type, local type) keeps its column.

### Mechanism and candidates

_N/A_ (behavior change).
- FMT-INDENT-037: the candidate lists are those of FMT-INDENT-015 — `AttributeListSyntax`, `TypeArgumentListSyntax`, `TypeParameterListSyntax`, `FunctionPointerParameterListSyntax` — in a member type, a constraint, a parameter (nested-list case), an object creation, a base type, and a type declaration.
- FMT-INDENT-038: every `QualifiedNameSyntax` (the outermost one, `AliasQualifiedNameSyntax` as its left side included) in the positions above.

### Breaking inputs

- FMT-INDENT-037, the attribute list: **an ambiguity with visible output.** The requirement's condition says the first element starts a line "because a comment, a directive, or disabled text … keeps FMT-BREAK-014 from joining it". FMT-BREAK-014 does not cover attribute lists, and `[⏎    Obsolete]` without any comment is not joined today (observed: kept, `Obsolete]` in column 4). One reading moves `Obsolete]` to column 1 for the no-comment input too; the other leaves it at column 4 while `[ // c⏎Obsolete]` goes to column 1. Either narrow FMT-INDENT-037 to the angle-bracket lists the issue names, or show the no-comment attribute list in question 4.
- FC-019/FC-020: a comment on its own line (`class D<⏎    // c⏎    T>`), an end-of-line comment, a directive group inside a type argument list, a function-pointer list (`delegate*<⏎// c⏎int, void>`).
- FC-027: the nested-list case (`void N(List< // c⏎int> a)`, column 11 today under FMT-INDENT-013), a base type (`class B : IComparable< // c⏎T>`), a constraint.
- FMT-INDENT-038: every no-comment member listed under *Premise*, the comment-held constraint, a list-element name (`void O(System. // c⏎IDisposable a)` and `class D : System. // c⏎IDisposable`, both aligned with `System` today and staying so).
- FC-028: RH5110's fix (change doc: column 22 to 30) is a node-level path.
- No existing test pins any of these; searched for `< // ` in the RH5110 tests.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Indentation phase (`AngleBracketListAlignmentContributor` or equivalent; a new qualified-name placement) | must change only for the options chosen |
| Line-break phase | not involved (the join follow-up is out of scope) |
| RH5044, RH5046, RH5104 | must stay |
| RH5110 code fix | not involved in code; output follows |
| Rule pages | not involved |

### Refuse option

- Keep FMT-INDENT-034's continuation indent for both members (questions 4 and 5 answered "unchanged"). Cost: zero production lines; FMT-INDENT-037 and FMT-INDENT-038 are not added. Every input the issue names is rare by definition (RH5044, RH5046, RH5104), and today's output is stable and non-destructive. The requested change adds two requirements, one of them (FMT-INDENT-038) a new placement rule for a node kind no contributor handles today. FMT-INDENT-038 also moves the no-comment wraps above, which nobody asked about.
- Narrower: FMT-INDENT-037 only (cheap, within an existing contributor's list family), restricted to angle-bracket lists; drop FMT-INDENT-038.
- Declaring unsupported (escalation 10 if pursued):
  - *qualified names*: a comment, directive, or disabled text between a qualified name's `.` and its right name, or between its left part and the `.` — the qualified-name counterpart of FMT-UNSUP-016 (member access). It deletes nothing on `main`, and FMT-INDENT-038 would still be needed for the no-comment wraps, so it is not an option now.
  - *bracket lists*: a comment, directive, or disabled text between the opening bracket of an attribute, type-argument, type-parameter, or function-pointer parameter list and its first element. It deletes nothing on `main`, and with FMT-INDENT-037 dropped there is nothing to delete either.

  Neither shape is commonly written. I name them as required; neither is a valid declaration under *a shape that deletes nothing is not declared* unless the maintainer adopts FMT-INDENT-037 or FMT-INDENT-038 for comment-held lines only.

### Output questions

- Question 4 must either drop the attribute list or show the no-comment `[⏎    Obsolete]` input with both outputs (see *Breaking inputs*). Recommendation for question 4: B (unchanged); if the lead prefers A, restrict it to angle-bracket lists. Why B: every input of question 4 is reported by RH5046 or RH5044, and today's column is stable.
- Question 5 must show the no-comment wraps that FMT-INDENT-038 moves. Suggested input to add:

  ```csharp
  using X = System.
      Text;

  class C
  {
      object M(object o)
      {
          var a = (System.
              IDisposable)o;
          var t = typeof(System.
              IDisposable);
          return default(System.
              IDisposable);
      }
  }
  ```

  Before (`main`): unchanged. After, A: `Text` at column 14, the cast's `IDisposable)o;` at 25, `typeof`'s at 27, `default`'s at 31. Derived from FMT-INDENT-038, not run.

  Also state in question 5 that A gives a name inside a parameter or a base type a different column (aligned with the first token, FMT-INDENT-013) than the same name in a constraint (one level deeper); option B is the reading that is uniform with FMT-INDENT-013.

  Recommendation for question 5: C (unchanged). Why C: the only case that looks wrong is held by a comment RH5104 reports; A moves real no-comment code that nobody asked about, and the cheaper way to clean up the no-comment wraps is the join follow-up below.

### Internal choices

- FMT-INDENT-038 yields to FMT-INDENT-013 inside list elements. This is internal only if question 5 is answered A, but the resulting inconsistency (base list or parameter aligned, constraint deeper) is visible and belongs in question 5 (above).

### Issue goals

- "Before: block column" — stale; drop. No output change.
- The issue's reading A for names (align with `System`) is not the spec author's recommendation; question 5 shows both. No goal is silently dropped.
- "Check the field-type case first" — the field case is refused (FMT-UNSUP-004); using a property is right. No output change.

### Architecture findings

_None._

### Follow-up candidates

- Widen the spec author's join follow-up to its class. A qualified name wrapped behind its dot with nothing in the gap is joined in a constraint and in argument-, parameter-, attribute-, tuple-, and type-argument-like list elements. It is not joined in:
  - a member's type and a local's type;
  - a using alias and a namespace name;
  - an explicit interface specifier;
  - a cast, `typeof`, `default`, and an object creation's type;
  - a base type.

  All are observed above; one issue for the whole class, in the output-question form.

## thoenissen/Reihitsu#1647

### Premise

Real and reachable through whole-document formatting, and through RH5306's fix where it lays a list pattern out. Observed (fixture `q1647.cs`): a comment on its own line in front of a list pattern's `]` stays in the `]` column (column 21), in an `is` expression and in a switch-expression arm (`[⏎    1,⏎    // last⏎] => 1` gives the comment in the `]` column, 20). In a collection expression the comment goes to the elements' column (22, `]` at 18). In a property pattern it goes to the content column (`{ Length: 1,⏎// last⏎}`, comment at 22, `}` at 18). In front of an argument list's `)` it stays at the `)` column (`Foo(1⏎⏎// last⏎)`, both column 20). The rarity test does not apply: no rule reports a comment on its own line there.

### Mechanism and candidates

_N/A_ (behavior change). The by-cause wording ("closes content this spec indents one level deeper than the delimiter") reaches:
- `]` of `ListPatternSyntax`;
- `)` of `PositionalPatternClauseSyntax` (only under question 7 = A);
- `)` of `ParenthesizedPatternSyntax`.

The closing braces are already covered. Not reached:
- `)` of an argument list, a parameter list, a tuple, and a parenthesized expression;
- `]` of an attribute list;
- `>`.

### Breaking inputs

- FC-021 (a comment before the closing delimiter): a list pattern in an `is` expression, in a switch-expression arm, and in a `case` label; nested in a list pattern; a slice as last element (`1,⏎// trailing⏎..` is a comment in front of `..`, not of `]` — it already goes to the element column, observed); a parenthesized pattern; a block comment; a comment directly after the last element without a blank line.
- FC-010: LF and CRLF.
- None found pinned for the list-pattern `]` comment.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Indentation phase, comment placement | must change |
| RH5306 analyzer | must stay (does not report the comment alone; change doc) |
| RH5306 fix | not involved in code |
| Rule pages | not involved |

### Refuse option

Leave the comment in the delimiter's column (today). Not a rare class, so refusing has no policy weight; the requested change is the existing rule stated by cause. No simpler option.

### Output questions

Question 6 is sound. The positional half of the FMT-INDENT-006 diff is decided by question 7, not question 6. If question 7 is not answered A, the positional pattern must be removed from FMT-INDENT-006's text and example (its content is then not indented deeper than its `)`). Record that dependency; it needs no extra question.

### Internal choices

The parenthesized pattern is included by cause — agree. It is shown in question 6 as B.

### Issue goals

None to drop.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1685

### Premise

The reported "Before" no longer occurs: `main` already gives the issue's reading A. Observed (fixture `q1685.cs`):
- `case 1 when⏎    Check(value):` keeps `Check(value):` one level deeper than `case`;
- `case 2 when⏎Check(value):` (written at the `case` column) moves to that column too;
- `case > 4 and < 10 when⏎        Check(value):` goes to the same column;
- `case 3 when Check(value)⏎    && Check(value + 1):` aligns `&&` with `Check` (column 24).

All stable. The issue's purpose is met; the run adds only the FMT-INDENT-034 example and a characterization test, and the PR links it with `Closes` or `Refs` as the lead decides on "purpose met".

### Mechanism and candidates

_N/A._

### Breaking inputs

Already pinned through a code fix: `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests` (lines 917–949, 1460–1495, 1542) asserts `case 1 when⏎    Check(value):` through the formatter. No formatter-project test pins it. A formatter test for the new FMT-INDENT-034 example is the only addition: the `case 1` and `case 2` inputs, LF and CRLF, two passes.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Indentation phase | must stay |
| FMT-INDENT-034 spec | example added (done) |
| Everything else | not involved |

### Refuse option

_None needed_ — no production change.

### Output questions

_None._

### Internal choices

_None._

### Issue goals

"Done when … the maintainer's answer is specified" — no answer is needed; today's output is the recommended reading. Drop the answer step; no output change.

### Architecture findings

_None._

### Follow-up candidates

The spec author's FMT-INDENT-021 `Why:` wording mismatch — spec text only; agree it is a follow-up or an adjacent wording fix the lead may take in.

## thoenissen/Reihitsu#1542

### Premise

The parenthesized half is decided on `main` by maintainer answers and produced by the formatter (spec author, confirmed in the issue's last comment). Dropping the issue's reading A for parenthesized patterns is correct: its "keep `(` on the `is` line" contradicts an answered requirement, and the output stays as that requirement demands. The positional half is real and reachable through whole-document formatting.

Observed (fixture `q1542.cs`): every line below a positional `(` is a continuation line in column 12, or 23 inside a switch-expression arm. This holds:
- with a designation (`P(1,⏎2) p`);
- with a property clause behind it (`P(1,⏎2) { X: 1 }`);
- with named subpatterns (`(a: 1,⏎b: 2)`);
- nested (`(1, (2,⏎3))`);
- behind a combinator (`(1, 2) and (3,⏎4)`);
- inside a list pattern element (`[(1,⏎    2)]` gives `2)` four columns right of `(`).

Also observed:
- `o is not⏎(1, 2)` is joined.
- `o is P // c⏎(1, 2)` keeps the `(` on its own line.
- `o is var (x,⏎y)` — a `VarPatternSyntax` with a `ParenthesizedVariableDesignationSyntax`, not a positional pattern — stays a continuation line.

No analyzer reports a positional pattern's layout. RH5101, RH5106, RH5109, and RH5203 register only `ArgumentList` or `ParameterList`, and RH5306 only `ListPattern`, so neither option conflicts with SHARED-DIAG-010.

### Mechanism and candidates

_N/A_ (behavior change). Candidates: `PositionalPatternClauseSyntax` under `RecursivePatternSyntax`, with and without a type, with a designation, with a property pattern clause behind it. Its parent contexts:
- `IsPatternExpressionSyntax`;
- a switch-expression arm;
- a `case` label;
- a subpattern;
- a list-pattern element;
- the operand of `and`, `or`, or `not`.

### Breaking inputs

- **A visible change the run causes that the spec does not decide inside the run:** `o is (1, 1) and (2,⏎3)`. FMT-BREAK-017 makes a combinator start its own line next to a multi-line property, list, or parenthesized pattern, but not next to a positional pattern. Under question 7 = A this input becomes `and (⏎2,⏎3⏎)` with `and` kept on the line. It is in #1542's scope (a wrapped positional pattern), and its output changes in this run either way. It is therefore an output question of the run, not a follow-up: fold it into question 7 as a sub-question.
- FC-027: every parent context above, the designation, the trailing property clause, nesting, named subpatterns.
- FC-019/FC-021: a comment or directive in the gap between type and `(` (FMT-BREAK-043 blocks); a comment in front of `)` (FMT-INDENT-006, see #1647); a comment behind a comma.
- FMT-BREAK-038: `(1⏎, 2)`, and a leading comma with a comment behind it. A comment in front of a later-line comma is refused by FMT-UNSUP-009.
- FC-010/FC-017: LF and CRLF, two passes.
- Pinned already: `RecursivePatternAlignmentTests.RecursivePatternWithPositionalClauseAlignsBracesToIsKeyword` pins a one-line `(1, 2)` followed by a wrapped property clause; it must stay. No test pins a wrapped positional clause.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Line-break phase (FMT-BREAK-022 positional, FMT-BREAK-043, FMT-BREAK-038; FMT-BREAK-017 if the sub-question is answered A) | must change |
| Indentation phase (FMT-INDENT-025, FMT-INDENT-006) | must change |
| RH5306 and its fix | must stay; its fix's output follows for nested positional subpatterns |
| Rule pages | not involved |

### Refuse option

Keep today's continuation lines (question 7 = C). Not a rare class: wrapped tuple patterns in switch expressions are ordinary code, so refusing has no policy weight. Declaring unsupported is not an option — there is no special case to delete. The cheaper of the two layouts in mechanism is A, since it reuses the list-pattern contributors' rule; B needs an argument-list-style alignment for a pattern node. That is not a reason to pick either.

### Output questions

- Question 7: add a switch-expression arm to the input. That is where positional patterns are most often wrapped, and A explodes it.

  ```csharp
  int K((int, int) t)
  {
      return t switch
      {
          (1,
              2) => 1,
          _ => 0
      };
  }
  ```

  Before (`main`, observed): `(1,` at column 19 and `2) => 1,` at 23. After, A (derived from FMT-INDENT-025, not run): `(` at 19, `1,` and `2` at 23, `) => 1,` at 19. After, B: `2) => 1,` at 20.

- Sub-question 7b (the FMT-BREAK-017 member above), asked only if 7 = A:

  ```csharp
  class C
  {
      bool M(object o)
      {
          return o is (1, 1) and (2,
              3);
      }
  }
  ```

  After, A (recommended): `and` starts its own line, as next to a list pattern. After, B: `and (` stays on the line of `(1, 1)`. Why A: one rule for every multi-line delimited pattern next to a combinator.

### Internal choices

- Dropping the parenthesized goal — agree; the output stays as answered requirements demand.
- FMT-BREAK-043 is a visible join the issue did not ask for; it is shown in question 7 (`var c`). Agree with showing it there.

### Issue goals

- Reading A for parenthesized patterns — drop (contradicts maintainer answers); no output change beyond `main`.
- The "Done when" for the parenthesized inputs — already met on `main`; only the positional inputs need new tests.

### Architecture findings

_None._

### Follow-up candidates

- `o is var (x,⏎y)` (a parenthesized variable designation) keeps continuation lines while `o is (x,⏎y)` would get the positional layout. It is a different construct and outside #1542; rare. A follow-up candidate only if the lead wants pattern-family uniformity; I recommend no issue.
- P11: no rule reports a wrapped positional pattern (a different construct from RH5306's list patterns). New-rule candidate, low value; recommend no issue.

## thoenissen/Reihitsu#1929

### Premise

Real and reachable through whole-document formatting, and through RH5206's fix (change doc). In a switch-expression arm the brace of `P and { … }`, `not { … }`, and `1 or { … }` lands at column 16, left of the arm's first token at 19. Observed (fixture `q1929.cs`). Under the literal text of FMT-INDENT-026 the anchor of such a brace is itself, which gives no column, so no unchanged requirement demands any particular column for it. The spec author's question 8 is right that the `case` half is a visible change in question. The arm half is a bug under every reading.

**A failing member the issue, the change doc, and the new example do not name: list-pattern elements.** `PatternAnchor` handles only `IsPatternExpressionSyntax`, `SubpatternSyntax`, and `ParenthesizedPatternSyntax` parents. A list-pattern element is a `PatternSyntax` directly under `ListPatternSyntax`, so it falls to the same fallback. Observed:

```csharp
return a is [P and { A: 1,
    B: 2 }, not { A: 3,
    B: 4 }];
```

gives `P` and `and` at column 24 and the `{`/`}` at column 16, left of the list's `[` (column 21). In a `case` label, `case [not {⏎A: 4⏎}]:` gives `not` at 21 and `{` at 20. Both stable. The clarified FMT-INDENT-026 wording ("the first token of the pattern those wrappers lead to") covers them by cause, so no spec text is missing. But the change doc's *Visible output changes* and the requirement's new example must name them, and the test matrix must cover them. Under either reading of question 8 they go under the element's first token: the label does not introduce a pattern inside a list pattern (FMT-BREAK-022), so the brace starts its own line there.

Other observations (fixture `q1929b.cs`):
- `case P {⏎A: 1⏎}:` gives content at 21 and `}` at 17, under `P` (confirms question 8's *Why*).
- `case P and not {⏎A: 2⏎}:` gives content at 29 and `}` at 25 today (two wrappers). Under A both go under `P`.
- `case (P and {⏎A: 3⏎}):` is anchored by the parenthesis and stays.

### Mechanism and candidates

For the arm and list-element bug: a property pattern reached only through `and`, `or`, or `not` from a parent that is none of `IsPatternExpressionSyntax`, `SubpatternSyntax`, or `ParenthesizedPatternSyntax` is anchored to its own first token, its `{`. A `{` that starts its line then has no anchor of its own. Decidable for any construct by the parent kind after climbing the wrappers.

Dispatch: `PatternAnchor.GetColumn`'s switch on `current.Parent`. The candidate parents of a pattern (from the `PatternSyntax` containment hierarchy) are:
- `IsPatternExpressionSyntax` (handled);
- `SubpatternSyntax` (handled);
- `ParenthesizedPatternSyntax` (handled);
- `SwitchExpressionArmSyntax` (fails);
- `CasePatternSwitchLabelSyntax` (question 8);
- `ListPatternSyntax` (fails);
- `SlicePatternSyntax` (`[.. not {⏎…}]` — same fallback, same failure expected; not run).

Also the parenthesized pattern behind a wrapper in the same contexts (`not (⏎…⏎)` in an arm), which `ParenthesizedPatternContributor` anchors through the same method.

### Breaking inputs

- Class matrix — axis "parent after climbing": arm, `case` label, list element, slice. Axis "wrappers": `and`, `or`, `not`, `and not`, `or not`. Axis "delimited pattern": property pattern without type, parenthesized pattern. Axis "brace position": behind the wrapper on its line, on its own line.
  - Failing on `main` (observed): arm × {and, not, or} × property pattern; list element × {and, not} × property pattern, in an `is` and in a `case` label.
  - In question 8: `case` label × {and, not, and not, or, or not} × property pattern.
  - Not run: the slice member and the parenthesized-pattern member behind a wrapper; the test implementer must run them.
- FC-010: LF and CRLF (the issue asks for both).
- FC-028: RH5206's fix (node-level).
- Pinned today and changing under question 8 = A: `LineBreakRequirementTests` around lines 820–870 and `ContinuationJoinTests` around lines 745–795 pin the `case` label's brace-column content. They are updated, not duplicated. No test pins the arm or list-element columns.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Indentation phase (`PatternAnchor`, used by `RecursivePatternContributor` and `ParenthesizedPatternContributor`) | must change |
| FMT-INDENT-026 text and examples; examples of FMT-BREAK-017, FMT-BREAK-022, FMT-BREAK-041 | changed (done); the new example should add a list-pattern element |
| RH5206 fix | not involved in code; output follows |
| Line-break phase | must stay |
| Rule pages | not involved |

### Refuse option

None that fits: the brace has no column today, and leaving the construct as written is not available, because the formatter already moves the brace onto a line of its own. Not rare: `not { … }` and `P and { … }` arms are ordinary code. Declaring unsupported deletes nothing.

### Output questions

Question 8 is sound. Two additions:
1. State that option B ("case labels unchanged") applies only where the label introduces the pattern. A property pattern inside a list pattern in a `case` label goes under its element's first token under both options.
2. Optionally add `case P and not {` to the input, the two-wrapper member whose columns move furthest (content 29 to 21).

### Internal choices

_None beyond the spec author's._

### Issue goals

- "Required output: column 19" — met by both readings for the arms.
- "FMT-INDENT-026 states which reading holds" — met, pending question 8.
- "A formatter test for each arm (LF and CRLF)" — keep, and extend it to the list-element member.

### Architecture findings

_None._ The anchor has one owner (`PatternAnchor`) used by both contributors. FMT-BREAK-022's "introduced by the case label" condition decides a line break, not a column, so it is no second statement of the anchor rule.

### Follow-up candidates

_None._

## Whole run

- No build, lane check, or test run was needed for these statements; every observation comes from `scripts/format.sh` (two passes, all stable) on afa1548.
- Escalation 10 is in play for #1633 (the spec author's FMT-UNSUP-017 alternative, recorded as internal) and is named for #1634. Under *a shape that deletes nothing is not declared*, none of them is a valid declaration unless the maintainer adopts the comment-held columns (FMT-INDENT-036, FMT-INDENT-037, FMT-INDENT-038). The lead decides how to present them; the recommended answers (question 2 = B, question 4 = B, question 5 = C) make them moot.
- Change-doc amendments the lead should route to the spec author before the batch:
  - `pattern-anchor-behind-wrapper.md`: add the list-pattern element.
  - `comment-held-type-argument-and-qualified-name.md`: add the no-comment qualified-name wraps, and settle the attribute-list scope of FMT-INDENT-037.
  - `positional-pattern-layout.md`: add the FMT-BREAK-017 member, the switch-expression arm, and the dependency of FMT-INDENT-006's positional sentence on question 7.
