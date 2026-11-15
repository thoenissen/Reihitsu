Role: spec author
Commit: 5ae1390
Verdict: #1147: spec written (clarifications); #1152: spec written (clarification); #1386: no spec change; #1387: spec written (clarifications); #1594: spec written, output question open; #1655: spec written, output question open; #1736: change doc written

My prompt carried no intended fix or suspected cause. `00-lead.md` summarizes triage notes from the issues (lane reasons). I used them as context and checked each against the specs. Spec architecture tests (`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`) pass on the edited specs: 37/37.

## thoenissen/Reihitsu#1147

### Changed requirements
- SHARED-INIT-001 — A comment in front of a token anchors its line — clarified: the "at most one comment on the token's own line" coverage limit is removed. Only the comment that starts the token's line is checked. A further comment behind it on that line is not checked, and neither is a comment behind the end of a comment that started on an earlier line. This applies the maintainer's decision #1362 Q7 (P12), recorded on the issue. `Uses` gains SHARED-COMMENT-011.
- RH7304-001 — clarified for a comment that leads the region's first line of code. See thoenissen/Reihitsu#1594 below, which shares this change (P12 instance #1176 Q1).
- No change to RH5110, RH5203, RH5205, RH5206, RH5301–RH5305, RH5306, or RH5308. Each already states "A line that starts with a comment is judged as SHARED-COMMENT-011 specifies". SHARED-COMMENT-011 already states both directions: a comment in the right column exempts the token behind it, and a misplaced comment puts the line out of place. Every instance in the issue and its comments is therefore a bug against an unchanged requirement.
- RH5305-001 says "whose closing bracket has no comment in front of it on the bracket's line" and then defers to SHARED-COMMENT-011. Both clauses give "not reported" for the issue's instance (`/* end */ ];` with the comment in the `[` column), so I left the wording alone. Rewriting it would decide new shapes, such as a closing bracket behind a multi-line comment's tail.

### Examples for the tests
- SHARED-INIT-001 (new examples: `/* a */ /* b */ P = 1,` at the member column is not reported; the same at a wrong column is reported on the member).
- RH7304-001 (new comment-led example).
- The other instances come from the issue text and its comments; none is a new spec example. Each needs a regression test next to a variant whose comment is misplaced, which is still reported (issue comment of 2026-10-22, "P12 clarification").

### Output questions
_None._ The RH7304 fix question is listed under #1594.

## thoenissen/Reihitsu#1152

### Changed requirements
- RH5204-005 — The diagnostic is on the line's first token, comment, or directive — clarified: a diagnostic on a documentation comment starts at its first `///` or `/**` and ends at the end of the comment's text on its last line, without the line break. The existing multi-line `///` example under RH5204-006 ("a `///` documentation comment moves as a whole") already shows that span.

### Examples for the tests
- RH5204-005 (indented `/// <summary>M</summary>` above a method).

### Output questions
_None._

## thoenissen/Reihitsu#1386

No spec change. SHARED-COMMENT-001 already makes a `///` or `/** */` comment that documents nothing (CS1587) an ordinary comment in every documentation mode. RH5033–RH5049 all list SHARED-COMMENT-001 and use the "every ordinary comment" wording, so each instance is a bug against unchanged requirements.

A fact the next roles should check: the issue's fourth instance is `/// a⏎// note⏎/// b⏎void N()`. Its expectation depends on whether the compiler reports CS1587 for `/// a`.
- If the compiler does not report it, `/// a` documents `N`. RH5034 then reports `// note` and not `/// a`, as today.
- If the compiler reports it, `/// a` is an ordinary comment in front of the documentation comment `/// b`. RH5034-001 then demands that neither `/// a` nor `// note` is reported, because "a comment before the documentation comment" is not reported. That is not the issue's reading, "judged as an ordinary comment as well". The spec, not the issue, decides.

### Examples for the tests
- SHARED-COMMENT-001, RH5033-001, RH5038-001, RH5034-001 (inputs from the issue).

### Output questions
_None._

## thoenissen/Reihitsu#1387

### Changed requirements
- SHARED-ATTR-019 — clarified and renamed ("A conditional directive between the attributes keeps the list unreported; a comment or directive inside withholds the split fix"). Changes:
  - The judged span of a one-attribute-per-list rule is now named: the gaps between `[`, the target specifier, the attributes, the commas, and `]`.
  - A conditional directive or excluded code there is not reported (SHARED-DIAG-008). One inside an attribute (its argument list) is outside the span.
  - `#region`, `#endregion`, and `#pragma` in the span keep the report and withhold the fix.
  - The example `[A,⏎#if DEBUG⏎#endif⏎ B]`, which was shown as reported, is replaced by a `#pragma` example (reported, no fix) and a conditional example (not reported).
- SHARED-ATTR-020 — clarified and renamed ("A conditional directive between the lists keeps them unreported; a comment or directive around the group withholds the merge fix"). The judged span for a reported list runs from `[` of the first list of its target to `]` of the reported list. A conditional directive there means the list is not reported. A directive in front of the first list, or behind the reported list, is outside the span. The existing `P` example (`#if true` in front of the lists) stays reported. A new example `[A]⏎#if DEBUG⏎[B]⏎#endif⏎[D] int a` is not reported.
- `attribute-targets.md` `Uses` gains SHARED-DIAG-008.
- RH5102-004 and RH5109-004 already follow SHARED-DIAG-008 on `main`; unchanged.
- RH5111-005 already followed SHARED-DIAG-008 for conditional directives. It is rewritten under #1655, which also changes the `#region`/`#pragma` case the issue lists for RH5111 (see the output question there).
- RH5511-004, RH5514-005, and RH5524-004 refer to SHARED-ATTR-011/019/020 and need no edit. Placement rules (SHARED-ATTR-011) judge where the list stands, not its inside, so a directive inside the list keeps the report and withholds the fix, as written. SHARED-ATTR-005 already leaves a directive behind the list unreported.

### Audit asked by the issue (for the PR)
- RH3002-009: "a comment or a preprocessor directive between `(` and `)`" withholds the fix and keeps the report, also for `#if`. The span RH3002 judges is the parenthesized expression, so SHARED-DIAG-008 says a conditional directive there is not reported. The two specs disagree. This is a follow-up candidate, below.
- RH5408-003: no finding. Every directive keeps the property unreported. SHARED-DIAG-008 allows that for a join rule ("a rule's own requirement may still leave such a shape unreported").

### Examples for the tests
- SHARED-ATTR-019 (RH5506: `#pragma` reported without fix; `#if` inside an attribute's argument reported without fix; `#if` between attributes not reported).
- SHARED-ATTR-020 (RH5524: `#if` between the lists not reported).
- RH5102-004, RH5109-004 (issue inputs; already in the spec examples).

### Output questions
_None._ The RH5111 `#pragma` case is part of the question under #1655.

## thoenissen/Reihitsu#1594

The issue's input `/* c */ #region` is invalid C# (CS1040), as the maintainer's triage notes. The valid input that a caller reaches is the RH7304 instance of P12 decided in #1176 Q1: a comment leading the region's first line of code.

```csharp
class C
{
    #region Fields
    /* c */ int _a;
    #endregion // Fields
}
```

On `main` the analyzer measures the line's text up to `int`, `    /* c */ `, so it reports both directives. The fix is withheld because that text is not whitespace.

### Changed requirements
- RH7304-001 — clarified: when a comment that starts and ends on the first token's line stands in front of the token and starts the line (SHARED-COMMENT-011), the directives are compared with the comment's indentation. The input above is not reported. Directives in another column than the comment are reported, wherever the token behind it stands. `Uses` gains SHARED-COMMENT-011.
- RH7304-004 — unchanged until output question 1 is answered.

### Examples for the tests
- RH7304-001 (new example: `Fields` not reported; `Values` and `Names` reported).

### Output questions

**1 · RH7304 · Region directives must use consistent indentation with containing code — a comment leads the region's first line**

Input:
```csharp
class C
{
  #region Fields
    /* c */ int _a;
  #endregion // Fields
}
```

Before (`main`): both directives are reported; no fix offered.

After, A (recommended): both directives are reported; the fix gives
```csharp
class C
{
    #region Fields
    /* c */ int _a;
    #endregion // Fields
}
```

After, B: both directives are reported; no fix offered.

Why A: the fix indents the directives like the line's first element, as it already does when that element is a token. Withholding it would refuse an input it can rewrite safely, because only the directives' whitespace changes.

Requirement touched: RH7304-004. On A, its text becomes "the indentation of the element RH7304-001 compares with" and gains this example. On B, it gains a withholding clause. Today's code withholds only because it measures the comment text as indentation.

## thoenissen/Reihitsu#1655

### Changed requirements
- RH5111-005 — changed wording, renamed "A comment or directive in the construct keeps it unreported or withholds the fix". The spec is written on reading A of output question 2 (below). The new text:
  - A construct is not reported when every gap of the judged span that holds a line break also holds a comment or a directive (`#region`/`#endregion`/`#pragma` included). The formatter does not join such a gap (FMT-BREAK-001), so it keeps the shape (SHARED-DIAG-010).
  - Exception: a variable declarator that the formatter refuses as unsupported (FMT-UNSUP-004). It stays reported, and the fix is withheld (SHARED-FIX-010).
  - A construct with a wrapped gap that holds neither a comment nor a directive stays reported, and the existing fix-withholding clause still applies.
  - The conditional-directive paragraph (SHARED-DIAG-008) and the "comment in front of a parameter or enum member" example are unchanged.
  - `Uses` gains FMT-BREAK-001, FMT-UNSUP-004, SHARED-DIAG-010, and SHARED-FIX-010.
- If the answer is B, RH5111-005 goes back to its `main` text, and SHARED-DIAG-010 needs an exception for RH5111.

### Examples for the tests
- RH5111-005: the enum member, the property, the parameter, the assignment with `/* c */` behind `=`, and the assignment with `#pragma` in front of `=` are not reported. `var y // c⏎= 1` is reported without a fix. The mixed gaps `y // c⏎=⏎2` and `y /* c */ =⏎3` are reported without a fix. The existing conditional-directive and comment-in-front examples are kept.

### Output questions

**2 · RH5111 · Assignments must have proper line breaks — a comment or directive in the wrapped gap**

Input:
```csharp
enum E
{
    A // c
        = 1,
}

class C
{
    int Q { get; set; } // c
        = 5;

    void M()
    {
        int y;
        y = /* c */
            2;
        y
#pragma warning disable CS0168
            = 3;
    }
}
```

Before (`main`): each of `A`, `Q`, and both assignments is reported on the whole construct; no fix offered.

After, A (recommended): not reported; the input stays as written.

After, B: as before, each is reported; no fix offered.

Why A: the formatter keeps every one of these shapes because it never joins across a comment or a directive. A diagnostic on them could never be cleared, and RH5042 already reports a comment around the `=`.

The run's specs disagree here:
- SHARED-DIAG-010 says a layout rule does not report a gap the formatter keeps because of a comment or directive.
- RH5111-005 on `main` shows `int Q { get; set; } // c⏎= 5;` and `y = /* c */⏎2;` as reported.
- thoenissen/Reihitsu#1387 lists "`#region`/`#pragma` stays reported" for every rule, RH5111 included.

The maintainer's triage of #1655 kept the enum example as the case to fix, which points to A. The lead may judge that this triage already answers the question. A declarator such as `var y // c⏎= 1;` stays reported without a fix under both options, because the formatter refuses it as unsupported (FMT-UNSUP-004).

## thoenissen/Reihitsu#1736

### Changed requirements
- Change doc `sdd/changes/2026-11/rh5109-parameters-on-one-line.md` (`Status: Draft`).
- RH5109-001 — changed: the parameters alone decide whether the list is multi-line, as FMT-BREAK-012 decides by elements. Parameters that all start on one line, none spanning lines, are not reported, including when:
  - that line is not the `(` line;
  - the `)` stands on a later line;
  - a comment or directive holds the first parameter off the `(` line.

  New examples: a reported `Q(⏎int a, int b,⏎int c)`, and an unreported block with the three shapes above. The intro sentence is adjusted.
- RH5109-003 — example changed: `N(⏎int a, int b)` is replaced by `N(⏎int a, int b,⏎int c)`, which gives `N(int a,⏎int b,⏎int c)`.
- `documentation/rules/RH5109.md` — the Description sentence now says that the parameters, not the parentheses, decide whether the list spans lines. The Violation/Correction examples are unchanged.

### Examples for the tests
- RH5109-001 (both blocks), RH5109-003 (new `N`).
- The issue also asks for a test that the fix result is stable under the formatter. Use the RH5109-003 output.

### Output questions
The change doc's `Visible output changes` lists five entries for the lead to ask, the rule page included. Entry 3, a comment holding the first parameter off the `(` line, follows the literal maintainer answer ("not reported"), although FMT-BREAK-012 lays that list out one parameter per line. The change doc's `Decisions` section records why. If the lead reads the answer as not covering entry 3, that entry is the open question.

## Whole run

### Internal choices
- No change doc for SHARED-INIT-001, RH7304-001, RH5204-005, SHARED-ATTR-019/020, or RH5111-005. Each brings a rule spec in line with an unchanged shared requirement (SHARED-COMMENT-011, SHARED-DIAG-008, SHARED-DIAG-010) or with a recorded maintainer decision (#1362 Q7, #1176 Q1), which matches the lead's triage. Alternative: add them to the #1736 change doc's visible output changes.
- SHARED-ATTR-019 and SHARED-ATTR-020 each now hold the report decision and the fix decision under one heading, as RH5102-004 and RH5109-004 do. Alternative: new IDs, which would add one more ID to the `Uses` of about 17 rule specs.
- The RH5305-001 wording is left as is (see #1147).
- RH7304: only a comment that starts and ends on the first token's line counts. A token behind the closing `*/` of a comment that spans several lines is left as `main` specifies it.

### Follow-up candidates

**F1 · RH5102 · Arguments must be on same line or separate lines — a line break behind `(` only** (same contradiction as #1736, for argument lists; outside the run's issues)

Input:
```csharp
class C
{
    void N()
    {
        M(
            1, 2);
    }

    void M(params int[] values)
    {
    }
}
```

Before (`main`): RH5102-001 reports the list (its parentheses are on different lines and two arguments share a line), and its fix splits the list into one argument per line. FMT-BREAK-012 joins it to `M(1, 2);`.

After, A (recommended): not reported, as decided for RH5109.

After, B: reported; the fix splits the list.

Why A: the same reasoning as the maintainer's answer on #1736.

**F2 · RH3002 · Statement must not use unnecessary parentheses — a conditional directive inside the parentheses** (the audit asked by #1387)

Input:
```csharp
class C
{
    int M(int a)
    {
        return (a
#if DEBUG
            + 1
#endif
            );
    }
}
```

Before (`main`): RH3002-009 says this is reported and no fix is offered.

After, A (recommended): not reported (SHARED-DIAG-008: the conditional directive lies in the span RH3002 judges).

After, B: reported; no fix offered, as RH3002-009 states.

Why A: another build compiles another expression between the parentheses, so the verdict can be wrong for it.

**F3 · RH7304** — a region whose first token stands behind the closing `*/` of a comment that spans several lines is reported on `main` with the fix withheld, because the line text up to the token is not whitespace. RH7304-001 only covers a first token that starts its line, so this case is unspecified; it is rare.

### Architecture findings
_None._

### Files
- /home/user/Reihitsu/sdd/changes/2026-11/rh5109-parameters-on-one-line.md
- /home/user/Reihitsu/sdd/specs/analyzer/RH5109.md
- /home/user/Reihitsu/sdd/specs/analyzer/RH5111.md
- /home/user/Reihitsu/sdd/specs/analyzer/RH5204.md
- /home/user/Reihitsu/sdd/specs/analyzer/RH7304.md
- /home/user/Reihitsu/sdd/specs/shared/attribute-targets.md
- /home/user/Reihitsu/sdd/specs/shared/initializer-layout.md
- /home/user/Reihitsu/documentation/rules/RH5109.md
