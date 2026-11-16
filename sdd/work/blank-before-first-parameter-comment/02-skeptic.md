Role: skeptic
Commit: 78555a1
Verdict: output question

My prompt carried no intended fix or verdict. The issue's option A, its "Done when", and its Before/After blocks are proposals (`agent-filed`). I checked them below and found two of them inaccurate.

## Premise

The issue is real, and the CLI reaches it through whole-document formatting. Its reported blocks are inaccurate, and it overstates how much it matters.

- Reproduced on 78555a1 with `scripts/format.sh <dir>`. Input is the issue's record, plus the same shape as a union:

  ```csharp
  public record Pet(
      // the first case
      int Cat,
      int Dog);

  public union U(
      // the first case
      int,
      string);
  ```

  Output:

  ```csharp
  public record Pet(

                    // the first case
                    int Cat,
                    int Dog);

  public union U(

                 // the first case
                 int,
                 string);
  ```

  The union parses: the file was formatted, not skipped for syntax errors. The issue's own *Before* and *After* show the comment and parameters at column 4. The real output aligns them behind `(` (FMT-INDENT-012), so neither of the issue's blocks is formatter output. A test must not pin the issue's block (see *Issue goals*).
- Callers that reach the case: the CLI (above). The RH5020 code fix and the RH8303 code fix insert the same blank line, confirmed with `scripts/apply-fix.sh` (below). The RH3004 and RH5305 code fixes reach it through the formatter (change doc entry 7, from the spec author; not re-run).
- Cause in code: `BlockStartUtilities.IsFirstInBlock` (`src/Reihitsu.Formatter/Pipeline/Core/Utilities/BlockStartUtilities.cs:25`) exempts only a missing preceding token, `OpenBraceToken`, and a switch-label colon. `BlankLineTriviaBoundaryRewriter.VisitToken` (`.../BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs:287`) reads it. Behind `(`, `[`, or `<` the blank line is therefore inserted, which is what today's FMT-BLANK-011 says. This is a behavior change, not a bug.
- Severity: the issue's own construct is a comment inside a parameter list, and RH5035 reports every such comment, the record, union, and indexer lists included (RH5035-001). It is therefore rare by definition (`AGENTS.md`, *Code-fix coverage*). On its own, the issue changes output only for code another rule asks the user to rewrite. The cases a user who follows the rules actually meets come from widening the change. No comment-placement rule reports any of these: the first element of a collection expression or list pattern, a tuple expression, a parenthesized expression, a cast, an array size, or an element access. RH5038 covers argument lists, RH5039 statement headers, RH5044 attribute lists, RH5046 type parameter, type argument, and tuple type lists.

## Mechanism and candidates

_N/A_ (behavior change). The dispatch fact is the kind of the token in front of the comment's owning token, plus the parent kind for `<`. Candidates by token, derived from the Roslyn token kinds:

- `OpenBraceToken`: exempt today in the formatter, RH5020, and RH8303. Unchanged.
- `OpenParenToken`: always an opener. Parents include parameter list, union case-type list, argument list, attribute argument list, statement headers (`if`, `for`, `while`, `using`, `catch`, ...), cast, parenthesized expression, tuple expression, tuple type, positional pattern, parenthesized pattern, and parenthesized lambda parameter list.
- `OpenBracketToken`: always an opener. Parents include collection expression, list pattern, bracketed parameter list (indexer), bracketed argument list (element access), array rank specifier (array size), attribute list, and the calling-convention list of `delegate* unmanaged[...]`.
- `LessThanToken`: an opener only under `TypeParameterList`, `TypeArgumentList`, or `FunctionPointerParameterList`. Under `LessThanExpression` and a relational pattern (`is < 5`) it is an operator.
- `CommaToken`, operators, everything else: keeps the blank line.
- Switch-label colon: exempt today. Unchanged.

## Breaking inputs

Failure classes that reach the change:

- FC-003: test both sides of each boundary. Generic `<` against operator `<`; `(` against `,` in the same list; author's blank line present and absent.
- FC-013 / P31: formatter against RH5020 and RH8303 (see *Internal choices* 2 for a concrete instance in the draft).
- FC-015 / FC-016: one predicate in three places (see *Architecture findings*).
- FC-010 / FC-017: a second pass, and CRLF.
- FC-040: `///` under DocumentationMode `None` is an ordinary comment and falls to RH5020 instead of RH8303. Both exempt it under the draft, so the outcome is the same either way, but one test should pin it.

The matrix to cover. Each cell: formatter output, RH5020 or RH8303 report, second pass stable.

| Axis | Members |
|---|---|
| Token in front | `(` of a parameter list, a union case-type list, an argument list, a statement header, a tuple expression; `[` of a collection expression, a list pattern, an indexer parameter list, an element access; generic `<` of a type parameter list and a type argument list; operator `<` (keeps the blank line); `,` (keeps); `{` and case colon (unchanged regressions) |
| Comment kind | `//`, `/* */` on its own line, `///`, `/** */` on its own line, `/** */` written behind `(` (moved, entry 4) |
| Author's blank line behind the opener | absent (none inserted) / present (kept behind `(` `[` `<`, removed behind `{`) |
| Opener's line | ends with the opener / ends with a trailing comment, `M( // a⏎ // b⏎ x)` (the RH8303-002 wording names this; FMT-BLANK-011 and RH5020-002 decide it by token kind too) |
| List content | non-empty / empty, where the comment is owned by the closing token: `M(⏎ // c⏎ )`, `[⏎ // c⏎ ]` |
| Directive between opener and comment | already exempt today; one regression |
| Line endings | LF, CRLF |

Observed today on 78555a1, all via `scripts/format.sh`. Each shows the blank line the change removes.

```csharp
// input
class C
{
    void M(
        // nothing yet
        )
    {
        int[] a = [
            // nothing yet
        ];
    }
}
```

```csharp
// output today
class C
{
    void M(

           // nothing yet
           )
    {
        int[] a = [

                      // nothing yet
                  ];
    }
}
```

```csharp
// input
class C
{
    void M(int a)
    {
        if (
            // first
            a > 0)
        {
        }
    }

    bool N(int a, int b)
    {
        return a <
            // compared
            b;
    }
}
```

```csharp
// output today
class C
{
    void M(int a)
    {
        if (

        // first
        a > 0)
        {
        }
    }

    bool N(int a, int b)
    {
        return a <

        // compared
        b;
    }
}
```

In the second pair, the operator `<` case must keep its blank line after the change.

Practical constraint for the test implementer: a comment inside a field's or local's declared type, as in `List<⏎// c⏎int> F;`, is refused as unsupported (FMT-UNSUP-004). The CLI reported `unsupported syntax: a comment between a variable declaration's own tokens` and left the file unchanged. Put the type-argument `<` case in a method return type or an invocation (`M<⏎// c⏎int>()`), which formats.

Structural transforms that could change the token in front of a comment do not, for comments. The attribute-list split (`[Obsolete,⏎ // c⏎ Serializable] class C`) and the parameter attribute merge (`([Obsolete] [⏎// c⏎CLSCompliant(true)] int x)`) both left the lists as written, verified with `scripts/format.sh`. The switch-case brace phase swaps a case colon and `{`, which are both exempt. No member found that fails for a reason beyond the change itself.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `BlankLinePhase` (`BlankLineTriviaBoundaryRewriter`) | Must change |
| `DocumentationCommentFormattingPhase.IsMovableDelimitedDocumentation` | Must stay (entry 4 keeps the move behind `(`). It reads the same predicate today (`DocumentationCommentFormattingPhase.cs:281`) |
| `IndentationPhase`, `LineBreakPhase` | Must stay (entries show unchanged columns) |
| RH5020 analyzer | Must change (`RH5020...Analyzer.cs:56`, private `IsFirstCommentInBlock`) |
| RH8303 analyzer | Must change (`RH8303...Analyzer.cs:114`, private `IsFirstInBlock`) |
| RH5020 and RH8303 code fixes | Not involved (no diagnostic, so no fix) |
| RH3004 and RH5305 code fixes | Output changes through the formatter; code not involved |
| RH5035, RH5038, RH5039, RH5044, RH5046 | Must stay |
| `Reihitsu.Core` | Not involved, unless the lead takes the architecture finding below |
| `documentation/rules/RH5020.md` | Must change (drafted) |
| `documentation/rules/RH8303.md` | Should change (see *Internal choices* 3) |
| CLI | Not involved beyond the formatter's output |

## Refuse option

- **Keep today's output (option B).** Costs 0 production lines. The issue's construct is rare by definition (RH5035), so for the issue alone B is a legitimate end state. B also matches the maintainer's earlier guidance that RH5020 does not exempt a comment shape users are asked to remove (`blank-line-sole-owner.md`, "RH5020 keeps reporting a comment glued to `{`"). That precedent was about glued comments, a different shape, so it bears on the parameter list and not on the collection expression.
- **The requested change (A or C).** A small edit of three predicates plus a parent-kind check for `<`. No new type or mechanism, so no special-case machinery.
- **Declare unsupported.** The candidate shape is an own-line comment directly behind an opening parenthesis, bracket, or generic `<`. Declaring it is not an option worth escalating:
  - It deletes no special case, because the change is a predicate widening, not a guard.
  - It would refuse whole files that hold a commented first element of a collection expression, which people commonly write.
  - A shape that deletes nothing is not declared (`AGENTS.md`).
- **Narrower option.** The issue's A is dominated. It changes output only for code RH5035 reports. It needs a parent-kind check for `(` and `[` that C does not need. It leaves the commonly written collection-expression case with the blank line. I recommend that the lead drop A from the question, or keep it last and unrecommended.

## Output questions

**1 (the drafted question 1 stands, with edits).** The *Before* blocks match `main` output: the record shape was verified here, and the collection expression by the spec author. I agree with recommending C. Edits:

- Remove from *Why* the file path and the reference to an earlier internal decision. `AGENTS.md`, *Output questions*, allows no mechanisms or internal alternatives there. Instead, say plainly that the issue's own case is code another rule asks the user to remove, and that C's visible benefit is the collection expression and list pattern.
- Drop option A, or keep it last (see *Refuse option*).
- Add one line to every option's *After*: RH5020 and RH8303 no longer report the comment exactly where the formatter leaves no blank line, and keep reporting it elsewhere.

**2 (the drafted question 2) is not open; fold it into 1.** Its option B is excluded by an unchanged requirement. Under 1A or 1C, FMT-BLANK-011 neither inserts nor removes the blank line behind the opener, so the formatter keeps that shape. RH5020 is a layout rule, and its fix inserts exactly the FMT-BLANK-011 blank line ("The formatter mirrors RH5020 and RH8303"). SHARED-DIAG-010 therefore forbids RH5020, and RH8303, from reporting a shape the formatter keeps. 2B would leave a permanent diagnostic on the formatter's output (FC-013). The analyzer change follows from question 1's answer, so it is not in question (`README.md`, *What reaches the maintainer*).

## Internal choices

1. **Change-doc cost argument.** The *Why* section says "a rule keyed on the token kind ... is one cheap check; a rule limited to parameter lists would need the parent kind as well". The draft itself needs the parent kind for `<`. Recommend rewording it to "a token-kind check, plus the parent kind for `<`", and resting C on consistency with the `{` exemption and on the collection-expression case, not on cost.
2. **RH8303-002 token list must equal FMT-BLANK-011's.** The draft names `(` and `[` with the apposition "— the first parameter of a parameter list —", and `<` only for a type parameter list. Today the RH8303 analyzer reports `///` behind `(` of an argument list, behind `[` of a collection expression, and behind `<` of a type argument list (`scripts/apply-fix.sh RH8303`; fixtures below). The formatter inserts the blank line there today too. Under the draft, the formatter stops inserting it, but RH8303-002 read literally keeps the report, which is an FC-013 conflict this run would create. RH8303-001 says such comments are not covered at all, so today's reporting is a pre-existing over-report (see *Follow-up candidates*). Recommend: RH8303-002 states the same opening tokens as FMT-BLANK-011 and RH5020-002 ("of any construct", every generic `<`), with the parameter list as an example only.

   ```csharp
   // fixture 1 (RH8303 reports and fixes both today)
   class C
   {
       void M(int a)
       {
           M(
               /// <summary>x</summary>
               a);
           int[] b = [
               /// <summary>x</summary>
               1,
           ];
           _ = b;
       }
   }
   ```

   ```csharp
   // fixture 2 (RH8303 reports and fixes; the formatter writes the blank line and expands the comment)
   using System.Collections.Generic;

   class C
   {
       List<
           /// <summary>x</summary>
           int> M()
       {
           return null;
       }
   }
   ```

3. **RH8303 rule page.** `documentation/rules/RH8303.md` reads "Three positions are deliberately not reported", which reads as a complete count. After the change, a fourth position (behind an opening parenthesis, bracket, or `<`) is not reported, and the page would undercount. Recommend widening the first bullet in the same words as the RH5020 page and listing the edit in change-doc entry 8. The alternative is leaving the page as drafted, which keeps the page inaccurate.
4. **FMT-BLANK-011's documentation example pins a move that FMT-DOC-001 leaves undecided.** The example's input `void M(/** first */ int a, int b)` relies on the documentation phase moving the comment onto its own line. FMT-DOC-001 states that it does not decide a comment before a parameter. Recommend writing that example's input with `/** first */` already on its own line behind `(`, so FMT-BLANK-011 pins only its own decision. Change-doc entry 4 can keep the written-behind case, which is today's move and is unchanged. The move today and the blank-line exemption read the same predicate (`IsMovableDelimitedDocumentation`), so a test of entry 4's *after* is what shows the move survives.
5. **`<` stays in C.** It is reached only by code RH5046 reports, and inside a field or local type it is refused as unsupported. Including it costs one parent-kind check and keeps "every opening token" a single principle. Dropping it would make `M<⏎// c⏎T>` differ from `M(⏎// c⏎a`. Recommend keeping it. This is visible only for rare code and is already inside question 1's option C.

## Issue goals

- "The maintainer has chosen A or B": change it to a choice among B and C (A is dominated; see *Refuse option*). This is already the output question, so nothing visible is decided by the change.
- "The covering test pins the output above": change it. The issue's blocks put the comment and parameters at column 4. Real output aligns them behind `(` (column 18 for `public record Pet(`). The test pins the formatter's real output without the blank line. Indentation is unchanged from today, so no visible output is in question.
- "With A, FMT-BLANK-011 names the parameter list": superseded by the answer. Under C, the requirement names opening tokens, not lists.
- The issue says nothing about RH5020 and RH8303. They must change with the formatter (SHARED-DIAG-010), and the run must state that in the PR.

## Architecture findings

One rule is stated in three places: "the comment's owning token directly follows a token that opens content". The three statements are:

- the formatter's `BlockStartUtilities.IsFirstInBlock`;
- RH5020's private `IsFirstCommentInBlock` (`RH5020CommentsShouldBePrecededByABlankLineAnalyzer.cs:56`);
- RH8303's private `IsFirstInBlock` (`RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.cs:114`).

They agree today, apart from the switch-label colon, which RH8303 omits as its spec allows. The run must widen all three identically, including the `<` parent check, so it newly depends on three copies not drifting (FC-015). A fourth consumer, `DocumentationCommentFormattingPhase.IsMovableDelimitedDocumentation`, reads the formatter's predicate for a different decision (the `/** */` move), which the draft keeps narrow. That predicate's summary says both phases read "this one owner". Once the blank-line exemption widens and the move does not, that sentence is no longer true. The lead decides between a narrow edit of each copy plus a follow-up, and a shared owner in `Reihitsu.Core` that both analyzers and the formatter read.

## Follow-up candidates

- **RH8303 over-reports.** It reports `///` and `/** */` comments that are not documentation comments of a declaration: inside an argument list, a collection expression, or a type argument list (fixtures in *Internal choices* 2). RH8303-001 says these are not covered. This is pre-existing, and a code-against-spec bug. The inputs are rare (documentation syntax where nothing is documented). After this run the over-report remains only behind a comma, where the formatter also inserts the blank line, so it is not a conflict, only an over-report. The lead may drop it as rare.
- **Indentation of the first element behind an opener that no alignment rule covers.** The spec author's candidate stands. Add one more member: a comment behind an operator wrap. `return a <⏎// compared⏎b;` puts the comment and `b` at the column of `return` (output above). Most members are reported by RH5039, RH5044, or RH5046. The cast, the array size, the parenthesized expression, and the operator wrap are not, but they are rare comment-inside-expression shapes. Recommend dropping the candidate, or filing it as low priority.
