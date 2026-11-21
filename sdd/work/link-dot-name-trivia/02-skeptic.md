Role: skeptic
Commit: e471892c
Verdict: proceed

Inputs read: thoenissen/Reihitsu#1888 (`agent-filed`), `00-lead.md`, `01-spec-author.md`, the spec diff against `origin/main` (`unsupported-syntax.md`, `line-breaks.md`, `RH2001.md`, `RH7101.md`, `AGENTS.md`), the change doc `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md`, FMT-UNSUP-004, SHARED-CHAIN-001, RH6016-004/005, the *Decisions* of `unsupported-link-operator-trivia.md`, and the code regions `ChainLineBreakRewriter` (`CloseGap`, `CloseInnerGaps`, `NormalizeChain`, `GetWrappedLayoutLineBreaks`, visitor overrides), `ListElementInteriorJoinRewriter.IsOperatorWrap`, `NoSpaceSpacingRule.IsMemberAccessDotToken`, `PrecheckPhase` (reason constants, operator-trivia helper). The prompt carried no intended fix, cause, or verdict.

All fixtures below were run with `scripts/format.sh <dir>` on a copy in a scratch directory (deleted afterwards); "→" lines show the changed output lines only, and every output also loses its final newline (CLI behavior, not quoted again).

## Premise

Real, and reached by whole-document formatting and the CLI. The issue's input reproduces exactly (fixture `E1.cs` = the issue's input): `            C();` → `        C();`.

The mechanism is in `ChainLineBreakRewriter.NormalizeChain`: every link's dot-to-name gap goes through `CloseGap(link.DotToken, link.Name.GetFirstToken(), gaps)`, which skips a gap where `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` is true; nothing later places a name that starts a line, so it takes the column of the enclosing construct. The chain rewriter visits every `MemberAccessExpressionSyntax`, `ConditionalAccessExpressionSyntax`, invocation, element access, and postfix unary, so the join (FMT-BREAK-020) holds outside wrapped chains too — checked with a plain line break and no comment (fixture `E4.cs`):

```csharp
class C
{
    private int _x;

    void M(string t)
    {
        this.
            _x = 1;
        var n = nameof(C.
                       M);
        var s = $"{t.
                    Length}";
        var q = t.
            Length;
    }
}
```

→ `this._x = 1;`, `nameof(C.M)`, `$"{t.Length}"`, `t.Length;`. So the FMT-BREAK-020 addition "no member name of a link starts a line" holds once the shape is refused.

The defect generalizes beyond chains; every member access outside a list element fails the same way (fixture `E2.cs`):

```csharp
using System;

class C
{
    private int _x;

    void M(object o, string t)
    {
        this. // c1
            _x = 1;
        var n = nameof(C. // c2
                       M);
        var b = o is string { Length. // c3
                                  IsNormalized: true };
        var s = $"{t. // c4
                    Length}";
        Func<int> f = () => t. // c5
                            Length;
    }
}
```

Observed: `_x = 1;` → statement column, and a blank line is inserted behind the statement; `IsNormalized: true` → statement column (the property pattern is also broken into braces around it); `Length}";` (interpolation hole) → statement column; `Length;` (lambda body) → statement column; `nameof(C. // c2` / `M);` unchanged (list element, FMT-INDENT-013). Every one of these is a simple member access in the syntax, so FMT-UNSUP-016 refuses all of them; that matches the spec's *Every link* bullet. The parse kind of `Length.IsNormalized` in an extended property pattern and of `C.M` inside `nameof` was not checked by running Roslyn; the test implementer's precheck test settles it.

The spec's "other side" example for FMT-UNSUP-016 (`Other.cs`, copied verbatim from the spec diff) comes back unchanged today apart from the final newline, as the spec author states.

Locations in the spec and the change doc were recounted from the texts: (5,24), (7,24), (8,1), (7,18), (7,21) are the columns of the first comment or directive in the gap.

Severity: the shape is rare — a comment or directive between a `.` and its name needs trailing-dot style, which C# code seldom uses; the common way to put a directive or comment in a chain is in front of a leading `.`, which stays supported (gap in front of a link). Refusing it loses no fix and leaves no construct unformatted for code people commonly write.

## Mechanism and candidates

Mechanism: the line-break phase refuses to join a name onto its `.` when the gap holds a comment, a directive, or disabled text, and no requirement or phase places a name that then starts a line, so the indentation phase gives it the column of the enclosing construct unless the line lies inside a list element (FMT-INDENT-013).

Dispatch: the candidate set is every syntax node whose `.` token is followed by a name the chain layout can join — `MemberAccessExpressionSyntax` of kind `SimpleMemberAccessExpression` and `MemberBindingExpressionSyntax` (the `link.DotToken` of `FluentChainLink`, also the last token of `?.`, `!.`, `!?.`). Other `DotToken` parents, from `NoSpaceSpacingRule.IsMemberAccessDotToken` and the grammar: `QualifiedNameSyntax` (not joined by any phase, see *Follow-up candidates*), `QualifiedCrefSyntax` (structured trivia), the `->` of `PointerMemberAccessExpression` and the `::` of `AliasQualifiedNameSyntax` (no dot). The spec's *Member accesses and member bindings only* bullet names exactly this split.

## Breaking inputs

Failure classes that reach the change:

- **FC-038** (a phase produces the shape). Checked: no formatter code builds a member access or member binding (`rg "MemberAccessExpression\(|MemberBindingExpression\(" src/Reihitsu.Formatter` finds none); the joins the chain layout writes carry whitespace only; `EnsureLineBreak` writes in front of a link operator, never behind a dot. No producer found. The test implementer still covers a second pass over the other-side example and over a chain whose comment stands in front of a leading `.`.
- **FC-042** (a guard that only serves the declared shape survives). The dot-to-name call of `CloseGap` in `NormalizeChain` must lose its unjoinable-trivia skip; `CloseGap` itself stays for the gap in front of the first link (called from `ApplyWrappedLayout`). The tests that lock the old refusal must change with it (below).
- **FC-039** (nested occurrence). The check must descend into every member access: inside an initializer value, a lambda body, an interpolation hole, an attribute argument, a property pattern, a `nameof` argument.
- **FC-040** (documentation mode). `///` and `/** */` behind the `.` on its line and on a line of their own, parsed with and without documentation mode. The spec counts both the `.`'s trailing trivia and the name's leading trivia, so either attachment is in the shape; the test pins it in both modes.
- **FC-041** (locality, FMT-UNSUP-003). A node-level target that is the name alone does not hold the shape; a target that is the member access does; a target that is the member access's expression (`a` in `a. // c⏎B`) does not.
- **FC-043** (code-fix query). RH5201, RH5112, RH3202, RH2001, RH7101 inputs from the change doc: the fix is withheld, the diagnostic stays.
- **FC-005 / FC-020** — directives as well as comments: `#if`/`#else`/`#endif` (example 2 of the spec), `#if false` with disabled text, `#pragma warning disable`, `#region` in the gap.
- **FC-003** — both sides of each condition: the gap on one line (`t. /* c */ Length`) and across a line break; a comment in front of the link operator, between the operator's tokens (FMT-UNSUP-015 reason), behind the name in front of `(` (FMT-UNSUP-014 reason), inside a type-argument list, inside the argument list, behind the link.
- **FC-027** — enclosing scopes: statement, assignment target (`this. // c⏎_x = 1;`), equals-value of a local (FMT-UNSUP-016 reason) and of a field with one declarator, attribute argument of a field (FMT-UNSUP-004 reason), field with several declarators holding a directive in a value (FMT-UNSUP-010 reason), argument, interpolation hole, lambda body, property pattern, expression body.

Class matrix for the refused shape (every cell must be refused, with this requirement's reason unless named):

| Axis | Members |
|---|---|
| Link kind | `a.B`; `a?.B`; `a!.B`; `a!?.B`; `this.B`; `base.B`; `global::System.Console` (member access on an alias-qualified name) |
| Trivia kind | `//`; `/* */`; `///`; `/** */`; `#if`/`#else`/`#endif` with disabled text; `#pragma` |
| Place in the gap | `.`'s trailing trivia on its line; own line between `.` and name; name's leading trivia on the name's line |
| Line break in the gap | none; one |
| Enclosing scope | as listed under FC-027 above |

Existing tests that assert today's behavior for the shape and must change (FC-042, FC-035):

- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainGapJoinTests.cs` — `SingleLineCommentBetweenDotAndMemberNameKeepsItsLine` (line ~350) and the `#if X` case at line ~499.
- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainSplitMemberNameRejoinTests.cs` — `CommentBetweenDotAndMemberNameKeepsSplit`, `BlockCommentBetweenDotAndMemberNameKeepsSplit`, `OwnLineCommentBeforeMemberNameKeepsSplit`, and the `#if DEBUG` case at lines ~339/354; all tagged FMT-BREAK-001.
- `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedLinkOperatorCommentTests.cs` — the rows `Use(s?. /* c */ Length)`, `Use(t!. /* c */ Length)`, `Use(u!?. /* c */ Length)` (line ~477) and `Use(c. /* c */ Name)` (line ~499) assert *supported*; they become refused under FMT-UNSUP-016.

Tests found with a comment behind a dot that stay valid because the dot belongs to a qualified name, not a member access: `RH4009…Tests` (`namespace MyCompany./* x */Project`), `RH2003…Tests` (`new System./* c */NotImplementedException()`), `RH3103…Tests`, `MultiLineCommentGapSpacingTests`, `BaseListContinuationAlignmentTests`, `ArgumentContinuationLineAlignmentTests`, `ParameterContinuationLineAlignmentTests`, `ReihitsuFormatterSplitsFieldDeclarationTests`, `RH7101DoNotCombineFieldsCommentAndDirectiveTests` (the last two are FMT-UNSUP-004 cases already). `RH6016…AnalyzerTests` line 358 (`string. /* comment */Empty`) is an analyzer test; the RH6016 fix does not call the formatter, so it is unaffected.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` (new check, new reason constant) and `UnsupportedSyntax` reason | must change |
| `ChainLineBreakRewriter.NormalizeChain` (dot-to-name gap no longer skips unjoinable trivia) | must change |
| `ChainLineBreakRewriter.CloseGap` (still used for the first link), `GetWrappedLayoutLineBreaks`, `FluentChainLink.HasInnerLineBreak` | must stay |
| `ListElementInteriorJoinRewriter`, indentation phase (FMT-INDENT-013), `HorizontalSpacingRewriter.NormalizeGapBehindLeadingComment`, `NoSpaceSpacingRule` | must stay (they serve every gap or qualified names; none has a case for this shape) |
| Code fixes (RH5201, RH5112, RH3202, RH2001, RH7101 and every other fix that formats) | code must stay; output changes through SHARED-FIX-010 |
| Analyzers (RH5048, RH5112, RH5201, RH6016, …) | not involved |
| `Reihitsu.Core` (`FluentChain`, `FluentChainLink`, `AttachedPartGapUtilities`) | not involved |
| CLI code | not involved (reason text flows from the formatter) |
| Rule pages, `general-notes.md` | not involved |
| Tests | must change (new precheck tests, the flipped tests above, FMT-BREAK-020 test without the refusal) |

## Refuse option

The maintainer already chose to declare the shape (batch 1 of the FMT-UNSUP-015 run, answer 2A, recorded in `unsupported-link-operator-trivia.md`), so escalation 10 is answered for this shape. For the record: the local refusal — the formatter leaves the name line as written — would need a guard in the indentation phase for a line that starts with a member name, i.e. a new special case; the issue's alternative (align the name under its dot) would need a column rule of its own. Declaring costs one cheap check in `PrecheckPhase` built on the existing operator-trivia helper pattern (`FindFirstUnsupportedOperatorTrivia` over two trivia lists), one reason constant, and deletes the skip in `NormalizeChain`. The class is rare (above). The declared span is exactly the `.`'s trailing trivia and the name's first token's leading trivia of a `SimpleMemberAccessExpression` or `MemberBindingExpressionSyntax`, excepting qualified names, crefs, `->`, `::`, and anything in disabled text. It deletes the dot-to-name skip of `CloseGap` and nothing else (the spec author's *Checked and kept* list holds against the code I read). It refuses no code people commonly write: leading-dot chains with comments or `#if` in front of the `.` stay supported.

No simpler option: the change is already the narrow one.

## Output questions

**1 · Formatter/CLI · reason text for the new unsupported shape** — the spec author's question 1 (`01-spec-author.md`) stands as written. I agree with its recommendation A (`a comment or directive between a . and its member name`): it names the place in the words of the code and fits `?.` as well as `.`.

**2 · Formatter/CLI · a block comment between a `.` and its name on one line** — only if the lead does not read answer 2A as covering it. 2A was asked with a split example (`a.B(). // c` ⏎ `C();`); the spec refuses the gap on any line, as FMT-UNSUP-015 does.

Input:
```csharp
#nullable enable

class C
{
    int? M(string? s, string t)
    {
        var a = s?. /* c1 */ Length;
        var b = t. /* c2 */ Length;

        return a + b;
    }
}
```

Before (`main`):
```csharp
        var a = s?. /* c1 */Length;
        var b = t. /* c2 */Length;
```

After, A (recommended): the file is unchanged, and the CLI reports `error: <file>(7,21): unsupported syntax: a comment or directive between a . and its member name. The file was left unchanged.`

After, B: as on `main` (formatted; the space behind each comment is removed).

Why A: the boundary is a place in the syntax tree, as for every other declared operator shape, and a refusal that depended on a line break would keep the join refusal's question open for the same gap. My reading is that 2A names the place ("the gap between a link's `.` and its member name") and so already covers A; the lead decides whether to show it.

## Internal choices

- **Reading of FMT-UNSUP-016's first sentence.** It says "the `.` of a link (SHARED-CHAIN-001)", and the bullets define the shape by syntax kind. SHARED-CHAIN-001 defines a link as a member access or member binding with `.`, `?.`, `!.`, `!?.`, so the two agree, including a member access that no wrapped chain contains (assignment target, `nameof`, property pattern). Recommendation: keep the kind-based definition (the spec author's choice); it is what the precheck can check cheaply and what `NormalizeChain` visits.
- **Precedence paragraph.** Checked against FMT-UNSUP-004's text: attribute arguments are in a field declaration's span and not excepted, while every member access of an initializer value is inside the value except when the access is the value's first or last token — and the dot gap is never the leading trivia of the value's first token nor the trailing trivia of its last token, so the paragraph holds. No change recommended.

## Issue goals

- "Declare the shape unsupported … comments, directives, and disabled text" — keep.
- "Delete the dot-to-name use of the gap check in `CloseGap`; its check for the gap in front of the first link stays" — keep; matches the code (`CloseGap` is called from `NormalizeChain` for the dot gap and from `ApplyWrappedLayout` for the first link).
- The issue says nothing about the one-line form; the spec extends the shape to it (output question 2, or covered by 2A at the lead's reading). No other goal to drop or change.

## Architecture findings

_None._ The join refusal and the missing column are one decision in one component; the precheck takes it over, which is what declaring does.

## Follow-up candidates

**1 · Formatter · a qualified name split across lines loses its column** (outside #1888; widens the spec author's using-directive candidate). The cause is not the comment: no phase joins a `QualifiedNameSyntax` split at its `.`, and no requirement places its continuation outside a list element or a base list. Fixture `Q3.cs`, plain line breaks, no comments:

```csharp
using static System.
    Math;

class C
{
    System.
        String P { get; set; }

    void M(object o)
    {
        var t = typeof(System.
                       String);
        var n = new System.
                    Text.StringBuilder();
    }
}
```

Observed: `Math;` → column 1; `String P` → member column (and the accessor list is broken over lines); `String);` and `Text.StringBuilder();` → statement column. With a `//` comment behind the dot (fixtures `E3.cs`, `Q1.cs`, `Q2.cs`) the same happens for `using static`, `using X = System. // c` (`Text;` → column 1), `namespace A. // c` (`B` → column 1), an attribute name (`[System. // c` ⏎ `Obsolete]` → `Obsolete]` in column 6), a property and a return type (member column), `typeof`, `default`, a cast, `new` (statement column); a parameter type and a generic argument in a base list align with their list element (FMT-INDENT-013). A local declaration's qualified type with a comment is already FMT-UNSUP-004.

Matrix: position of the qualified name {using / using static / using alias, namespace, attribute name, member type, return type, `typeof`/`default`/cast/`new` type, parameter type, type argument, base list} × gap {line break, `//`, `/* */` on a line break, directive}. Failing cells: every position outside a list element and a base list, with or without a comment.

Readings to carry in the follow-up as an output question: (A) join a split qualified name onto its dot like FMT-BREAK-020, so only the comment and directive cells keep the split; (B) indent the continuation one level; (C) leave it as written.

On escalation 10 for this shape (`00-lead.md` plans to ask it in batch 1): declaring "a comment between a qualified name's `.` and its name" deletes no special case today — no phase refuses a join there, because no phase joins there — and it would leave the plain-line-break cells, the bulk of the class, as broken as before; AGENTS.md says a shape that deletes nothing is not declared. My recommendation is to file the follow-up with readings A–C and not to escalate the declaration now. If the follow-up chooses A, a join refusal for comments and directives appears, and the declaration becomes an escalation-10 option in that run. The lead decides.
