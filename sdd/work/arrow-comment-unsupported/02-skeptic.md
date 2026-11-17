Role: skeptic
Commit: 86eb1aa (statements about today's output come from this tree, whose `src/` and `scripts/` are identical to `main` at 17fb08c; the spec diff is 6990dee)
Verdict: output question

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's own prescriptions (delete RH5401-004's format-and-compare; "the column-4 layout stops being reachable") are treated below as claims.

## Premise

Real. A caller reaches it: whole-document formatting through the CLI (`scripts/format.sh`), and the code fixes RH3202, RH3203, RH3205–RH3208, RH5401, RH7101 and every formatter-delegating fix whose formatted node holds an expression body. Re-checked on this tree:

- Change doc changes 2, 6, 8 reproduce exactly as written (`scripts/format.sh` on a fixture dir): `int B =>⏎// c⏎1;` gets a blank line behind `=>` and both lines in column 4; `get => // c⏎_v;` puts `_v;` in the accessor's column; with a conditional group behind the arrow the active branch (`2;`, `0;`) moves to column 4.
- RH3202 today, `scripts/apply-fix.sh RH3202` on `int Get() => // c⏎42;` and `int Other() =>⏎// after arrow⏎42;`: `fixed (1 action(s), 2 iteration(s))`, output `{// c` / `return 42;` and `{` / `// after arrow` / `return 42;` (change 9's Before holds).
- RH7101 today on change 10's input: `fixed (1 action(s), 1 iteration(s))`.
- RH5401 today on `int A => // c⏎1;`: `no-fix-offered` (reported, no fix), LF and CRLF.

No repository source outside test string literals holds the shape (multiline `rg` for `=>` followed by a line starting with `//`, `/*`, or `#`, and `rg` for `=> //` / `=> /*`): self-hosting is not affected.

The expression bodies the check must cover are exactly the parents of `ArrowExpressionClauseSyntax` — `BaseMethodDeclarationSyntax` (method, constructor, destructor, operator, conversion operator), `PropertyDeclarationSyntax`, `IndexerDeclarationSyntax`, `AccessorDeclarationSyntax`, `LocalFunctionStatementSyntax`. Lambdas and switch-expression arms carry their `=>` as a bare token, not in an arrow clause. FMT-UNSUP-012's list matches this set.

The claims in *Special cases* hold against the code:

- `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` (line 70–71): the second `WouldJoinAcrossUnjoinableTrivia(ArrowToken, Expression.GetFirstToken())` reads exactly `ArrowToken.TrailingTrivia` and the first token's `LeadingTrivia` (`SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`) for comments, directives, disabled text. The rewriter visits only property and indexer declarations, so the arrow clause is always whole.
- `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams` lines 124–125 check the same two lists; `IsLayoutOnly` is "nothing but whitespace and end-of-line", so in a syntax-error-free input it fails only on comments, directives, and disabled text. The getters it sees come either from the input (refused by the precheck) or from `AccessorExpressionBodyTransform.ConvertToExpressionBody`, which builds the arrow with empty trivia and the expression `WithoutLeadingTrivia()` (line 146, 198).
- `ExpressionBodyToBlockConverter`: only the six `ExpressionBodied*Transform`s call it, each on a whole member. `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` does not convert by itself; it formats the member and takes the formatter's block, returning the document unchanged when the formatter keeps the expression body.

## Mechanism and candidates

_N/A_ — behavior change. For the new check, the dispatch type is `ArrowExpressionClauseSyntax` (set above); the two trivia lists are `ArrowToken.TrailingTrivia` and `Expression.GetFirstToken().LeadingTrivia`.

## Breaking inputs

Failure classes that reach this change: FC-038, FC-039, FC-040, FC-041, FC-042, FC-043, FC-003, FC-005, FC-019/FC-020, FC-025, FC-027, FC-010/FC-017.

**FC-038 (a phase writes the shape).** I tried to make each phase write a comment or directive into the gap. None did (`scripts/format.sh`, one pass, then a second pass that was stable). The members below were run in two fixture files, `A`, `B`, `D`, `E`, `F` in one and `G`, `H` in the other; they are shown together here:

```csharp
class C
{
    private int _v;

    int A { get /* c */ => _v; }
    int B
    {
        get
            // c
            => _v;
    }
    int D { /* c */ get => _v; }
    int E { get { /* c */ return _v; } }
    int F { get { return _v; /* c */ } }
    int G { get { return /* c */ _v; } }
    int H { get { return // c
                _v; } }
}
```

None was converted or lifted into `=> /* c */`. `F` became `int F => _v; /* c */`; the others kept their block or accessor list. Documentation comments in front of the arrow (`int A /// doc⏎=> 1;`, `int B⏎/// <summary>doc</summary>⏎=> 2;`, the same in front of a method's and a getter's arrow) were moved onto lines of their own **in front of** the arrow, never behind it. `int A =>⏎⏎1;` (blank line, no comment) joins to `int A => 1;`. The test implementer covers each of these as an FMT-UNSUP-005 case: format, then `FindUnsupportedSyntax(output) == null`.

**FC-039 (nested occurrences).** Cases: a local function in a lambda body inside a property's expression body; a local function in a lambda inside a field initializer with one declarator (FMT-UNSUP-004's initializer exception must not hide it) and with several (FMT-UNSUP-010's reason wins, as the spec says); an accessor of a property in a nested type; an expression body whose expression is a lambda (`Func<int> F => /* c */ () => 1;` refused, `Func<int> F => () => /* c */ 1;` not refused).

**FC-040 (documentation mode).** `int A =>⏎/// <summary>x</summary>⏎1;` and `int A => /// x⏎1;` under `DocumentationMode.None`, `Parse`, `Diagnose`: the same answer and location in each mode.

**FC-041 (locality).** The same text as a node-level input: the member and the `ArrowExpressionClauseSyntax` are refused. The expression alone, with the comment in its leading trivia, is not refused. Each is asked as a target, as a detached node, and as a document root.

**FC-003 / boundary, both sides.** Refused: `=> /* c */ ref _x`, `=> /* c */ throw new X()`, `=> /* c */ (1)`. Not refused: `=> ref /* c */ _x`, `=> throw /* c */ new X()`, `=> (/* c */ 1)`, `=> 1 /* c */;`, `=> 1; // c`, `int A /* c */ => 1;`, a comment above the member, and the lambda and switch-arm arrows.

**FC-005 / FC-020 (directives and disabled text).** Each kind behind the arrow is refused: `#if`/`#else`/`#endif` (both builds), `#if false … #endif` with only disabled text, `#pragma warning disable`, `#nullable enable`, `#region`/`#endregion`. Each needs its own test, because the check for comments does not cover directives.

**FC-025.** All seven expression-bodied kinds plus accessors (`get`, `set`, `init`, `add`, `remove`), with and without `async`. The one-line form (`int Get() => /* c */ 42;`) is checked for each kind.

**FC-042.** The three deletions above. The one surviving copy of the gap check is `ExpressionBodyRewriteUtilities.BlocksRewrite`'s first condition (directive between `=>` and the expression). The spec keeps it for the analyzers (*Internal choices*). Every other trivia-gap guard I found (`ArrowLineHoldsComment` for the comment in front of the arrow, the first `WouldJoinAcrossUnjoinableTrivia` in `CollapseExpressionBody`, the arrow-leading seam in `HasLayoutOnlySeams`) also serves the comment in front of the arrow, which stays supported, so it must stay.

**FC-043.** Every formatter-delegating code fix already asks `FindUnsupportedSyntax`. My scan of `Rules/` and `Base/` found one fix that formats without asking, `TargetAttributePlacementCodeFixProviderBase`, and it passes an identity formatter. `ExpressionBodyToBlockCodeFixProviderBase` asks about `GetFormattingInput(node)`. RH3208 overrides that to return the line-starting label, which is also the context it formats. `FormatMemberOnLinesOfItsOwnAsync` formats only the member, so a neighbour member holding the shape does not withhold the fix. The tests should include one member sharing its line with a neighbour that holds the shape: the fix is still offered for the clean member.

**FC-010 / FC-017.** Every "not refused" and FMT-UNSUP-005 case is checked under LF and CRLF, with two passes.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase.FindUnsupportedSyntax`, `UnsupportedSyntax` reason | Must change (new shape and reason) |
| `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` | Must change: delete the second join check; the first (in front of the arrow) stays |
| `ExpressionBodyToBlockConverter` | Must change: delete the cases for the gap behind the arrow; the arrow-leading handling, the blank-line/line-break leading trivia of the expression, and the semicolon handling stay |
| `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams` | Must change: delete lines 124–125; the other seams stay |
| `AccessorExpressionBodyTransform` | Must stay |
| `ExpressionBodyRewriteUtilities.BlocksRewrite` / `RequiresExpressionBodyPreservation` | Must stay (analyzers decide by it, SHARED-BODY-001) |
| Analyzers RH5040, RH5401, RH3202–RH3208 | Must stay (they do not consult the precheck) |
| Code fixes (RH3202–RH3208 base, RH5401, RH7101, others) | Must stay in code. Behavior changes only through `FindUnsupportedSyntax`. RH5401's `CanCollapse` stays: it also decides the wrapped expression and the comment in front of the arrow |
| Fix All | Not involved beyond the per-diagnostic withholding |
| `Reihitsu.Core` | Not involved |
| CLI | Not involved in code; output changes through the reason |
| Rule pages | Not involved (`general-notes.md` covers a missing fix) |
| `AGENTS.md` precheck row | Changed by the spec author (sixth shape) |
| Tests | Must change. The tagged behavior that goes away is in `Formatter.Test/Regression/Structural/ExpressionBodyArrowCommentTests.cs`, `ExpressionBodyDirectiveGuardTests.cs`, and the FMT-STRUCT-026 example tests. Other test files with inputs that put a comment or directive right behind `=>` and must be checked are `ExpressionBodiedTransformTests.cs`, `ExpressionBodiedLocalFunctionTransformTests.cs`, `ExpressionBodiedMethodTests.cs`, `ExpressionBodiedIndexerTests.cs`, `GetOnlyMemberExpressionBodyTests.cs`, `StructuralTransformSeparator*Tests.cs`, `ExpressionBodyNextLineExpressionTests.cs`, `CommentJoinTests.cs`, `DirectiveJoinTests.cs`, `CommentBeforeBraceBlankLineTests.cs`, `IdempotencyTests.cs`, and the analyzer tests of RH3202, RH3205–RH3208 (some of those hits are lambdas; each needs to be read) |

## Refuse option

The requested change is itself the third kind of refusal: it declares the shape unsupported. Cost: one check over every `ArrowExpressionClauseSyntax` reading two trivia lists, plus a reason string. It deletes more production lines than it adds (the converter cases, one join check, two seam checks). It adds no type.

The other refusals cost more:

- **Leave as written.** For a property, an indexer, or an accessor, the formatter does not leave it as written today: the indentation phase moves the expression to the member's column. Leaving it would need a new guard in the indentation phase, a special case of its own.
- **Withhold locally.** For a method, keep the expression body behind a comment, as FMT-STRUCT-007 does for a directive. That also deletes the converter cases. But the kept body still has its expression moved to the member's column (same indentation defect), and RH3202–RH3208 would then have to stop reporting such members (SHARED-BODY-001), which changes diagnostics.

Declaring is the cheapest of the three.

Is the class rare?

- **Comments: yes, by the repository's definition.** RH5040 (enabled, Warning) reports every comment directly before or after the arrow of a member or accessor (SHARED-COMMENT-003: own-line and same-line alike). RH3202, RH3203, and RH3205–RH3208 (enabled, Warning) report every expression-bodied method, constructor, operator, conversion operator, finalizer, and local function. So every comment cell of the shape, local functions included, already violates another rule. This is a stronger argument than the spec's "no more common" for local functions.
- **Directives: not by definition, for some owners.** A property with a conditional group behind the arrow is reported by RH5401. A method with the same shape is reported by no rule I could find: `scripts/apply-fix.sh RH3202` gives `no-diagnostic`, which SHARED-BODY-001 intends. The realistic input is the build-flag idiom (output question 3 below). So declaring directives refuses whole files that hold an idiom some people write and that no other rule reports. `main` already damages its layout. This is a cost the maintainer should see when answering question 3.

The shape the issue is not about (escalation 10): **a comment, directive, or disabled text in front of the arrow.** Its span is the trailing trivia of the token in front of `=>` plus the leading trivia of `=>`. It excepts the member's own leading trivia (a comment above the member). Facts:

- On `main` it leaves the same defect the issue says becomes unreachable. Properties, indexers, getters, and a directive-split arrow all put `=>` in the member's column, and an own-line comment gets a blank line inserted. A method gets `/* c */ {` (spec author's follow-up). Fixtures and output are under *Follow-up candidates*.
- Declaring it would delete: the first join check in `CollapseExpressionBody` (and with it the whole refusal); `ArrowLineHoldsComment`; the converter's split of own-line comments out of the arrow's leading trivia; the arrow-leading and getter-keyword-trailing seams in `HasLayoutOnlySeams`; RH5401-004's comment-in-front clause, and with it the RH5401 / SHARED-DIAG-010 conflict.
- What it refuses: comment cells only where RH5040 (members, accessors) or RH3208 (local functions) already reports, so rare by definition. Directive cells are the `#if`-split arrow (`int A⏎#if DEBUG⏎=> 1;⏎#else⏎=> 2;⏎#endif`), which I judge rare.

I recommend declaring it, in a change of its own (`AGENTS.md`: a shape the issue is not about is usually declared separately). See output question 7.

## Output questions

My additions to the spec author's six; the lead batches them.

**To question 3 (directives belong to the shape) — the realistic input.** The change doc shows `1;`/`2;`; the form people write is:

```csharp
static class BuildInfo
{
    public static bool IsDebug =>
#if DEBUG
        true;
#else
        false;
#endif

    public static bool IsDebugBuild() =>
#if DEBUG
        true;
#else
        false;
#endif
}
```

Before (`main`, `scripts/format.sh`): both active `false;` lines move to column 4; the property is reported by RH5401 without a fix; the method is reported by nothing (`scripts/apply-fix.sh RH3202`: `no-diagnostic`).

```csharp
static class BuildInfo
{
    public static bool IsDebug =>
#if DEBUG
        true;
#else
    false;
#endif

    public static bool IsDebugBuild() =>
#if DEBUG
        true;
#else
    false;
#endif
}
```

After A: refused at `(4,1)`; the file is unchanged, CLI exit code 2. After B: as `main`. I still recommend A, because `main` already damages the layout and a refusal at least tells the user. But for the method, no other rule reports this input, so "rare by definition" does not cover it. The question should show this input rather than `1;`/`2;`.

**To questions 1 and 2 — the cost argument.** Every member whose good `main` output the declaration gives up (change 4, change 7) is already reported by RH3202/RH3208 as a whole. That is the fact behind "rare", and the questions may say so in plain words.

**7 · Escalation 10 · Declare a comment or directive in front of the arrow unsupported too.** Not about this issue's shape. Asked per `squad/README.md`, escalation 10.

Input:
```csharp
class C
{
    private int _v;

    int P // c
        => 1;

    int V
    {
        get // c
            => _v;
    }

    int Q
        /* c */ => 2;

    int B()
        /* c */ => 1;
}
```

Before (`main`, `scripts/format.sh`):
```csharp
class C
{
    private int _v;

    int P // c
    => 1;

    int V
    {
        get // c
        => _v;
    }

    int Q
    /* c */ => 2;

    int B()
    /* c */ {
        return 1;
    }
}
```
(`int B()` is from 01-spec-author.md's follow-up; the others are from my fixtures.)

- After A (recommended): the input unchanged; the CLI reports it as unsupported syntax at the first such comment, with a reason such as "a comment or directive between an expression body's arrow and the code in front of it". The fixes of RH3202–RH3208 and RH5401, and every formatter-delegating fix whose node holds it, are no longer offered on that code. Declared in a change of its own.
- After B: as `main`, and the follow-ups below stay open.

Why A: every comment case is already reported by RH5040 or RH3208, and the shape removes three layout defects at once. Special cases it would delete: the property and indexer join refusal, the converter's handling of comments on the arrow's line and in front of it, the get-only lift's checks in front of the getter's arrow, and RH5401's reporting of a property the formatter keeps.

## Internal choices

- **SHARED-BODY-001's Why is now partly stale.** "The analyzer, the code fix, and the formatter take the decision from one predicate" no longer holds for the directive clause: the formatter refuses through the precheck before `BlocksRewrite` runs, so that condition is dead on the formatter's path. Recommend rewording the sentence (the analyzers keep the predicate; the formatter decides the gap through FMT-UNSUP-012). Keeping `BlocksRewrite` unchanged is right: changing it changes RH3202–RH3208 diagnostics.
- **FMT-UNSUP-012's Why for local functions.** Recommend citing RH3208, which already reports every expression-bodied local function, instead of "a comment behind it is no more common". The same holds for RH3202–RH3207 and the converted kinds. No output changes.
- **Accepted the spec author's choices:** keep `CanCollapse`; FMT-UNSUP-010 keeps its reason; lambdas and switch arms stay outside the shape; FMT-STRUCT-024's example replaced. I found nothing that contradicts them.

## Issue goals

- "Delete RH5401-004's format-and-compare for a comment behind `=>`": drop. The format-and-compare is not specific to that comment; it also decides the wrapped expression and the comment in front of the arrow. The comment behind the arrow is withheld through `FindUnsupportedSyntax`, which the fix already asks at registration (line 90). No visible change.
- "The column-4 layout of #1731 stops being reachable and needs no repair": overstated, so change the statement. It holds only for the gap behind the arrow. The same column layout remains for a comment or directive in front of the arrow (property, indexer, getter; output question 7 shows it), and for a `return` value on the next line (follow-ups). No visible change from correcting the statement.
- "Whether RH5401 still reports the property … is decided in that change": settled by SHARED-DIAG-010's last sentence (an unsupported shape is reported, fix withheld). Keep.
- The prescribed CLI reason text: already output question 6.
- Every other goal is kept.

## Architecture findings

_None_ within the run. The column defect below is an indentation-phase gap (no requirement places these lines), not two components owning one decision.

## Follow-up candidates

Each is outside thoenissen/Reihitsu#1732 and becomes an `agent-filed` issue unless question 7 absorbs it.

1. **RH5401 · Expression-style get-only properties should be single-lined — reports a property the formatter keeps (comment in front of the arrow).** This is a SHARED-DIAG-010 conflict. RH5401-004 states the report and the spec diff restates it, while SHARED-DIAG-010 forbids a layout rule from reporting "a gap that the formatter does not join because a comment or a directive sits in it". thoenissen/Reihitsu#1730 tracked only the comment behind the arrow and is closed. Option A of question 7 resolves it.

   Input:
   ```csharp
   class C
   {
       int B // note
           => 1;
   }
   ```
   Before: reported, no fix (`scripts/apply-fix.sh RH5401`: `no-fix-offered`, LF and CRLF).
   - After A (recommended): not reported.
   - After B: reported, no fix, and RH5401-004 names SHARED-DIAG-010 as an exception.

   Why A: SHARED-DIAG-010.

2. **Formatter · a line in front of or behind an expression body that the line-break phase cannot join lands in the member's column.** The twin of thoenissen/Reihitsu#1731, which is closed. Unless question 7 answers A, these stay:

   Input:
   ```csharp
   class C
   {
       int P // c
           => 1;

       int A
   #if DEBUG
           => 1;
   #else
           => 2;
   #endif
   }
   ```
   Before: `=> 1;` and the active `=> 2;` are moved to column 4.
   - After A (recommended): they keep one continuation indent, column 8.
   - After B: as `main`.

   Why A: a continuation line is indented inside its member, as the input wrote it.

3. **Formatter · the value of a `return` on the next line lands in the statement's column.** No FMT-INDENT requirement places it. A converted method shows it too.

   Input:
   ```csharp
   class C
   {
       int M()
       {
           return
               2;
       }

       int Get() =>

           2;
   }
   ```
   Before (`main`): `return⏎        2;` in both members. The value is moved to `return`'s column, and in `Get` the converted body keeps the line break: FMT-STRUCT-006 says "an expression written on the line after the arrow MUST join the created `return`", and two readings exist for a blank line in between.
   - After A (recommended): the value keeps a continuation indent, `return⏎            2;`, and `Get` joins as `return 2;`.
   - After B: as `main`.

   Why A: a continuation line is indented inside its statement, and the conversion's join should not depend on a blank line.

4. **Formatter · `return // c⏎_v;` inside a getter** is the same column class as item 3 (fixture output: `_v;` moved to the column of `return`). It belongs in the issue for item 3.
