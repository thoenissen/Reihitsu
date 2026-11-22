Role: spec author
Commit: fe22aca
Verdict: change doc written

Spec commit: 4046a07. Change doc: `sdd/changes/2026-11/assignment-and-arm-comment-unsupported.md` (Status: Draft).

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I followed the maintainer's answer in `00-lead.md` (declare the shapes unsupported, in this change).

## Changed requirements

- FMT-UNSUP-020 — Comments and directives behind the `=` of an assignment or a property initializer are unsupported — added. Reason: `a comment or directive behind the = of an assignment or a property initializer`.
- FMT-UNSUP-021 — Comments and directives behind the arrow of a switch arm are unsupported — added. Reason: `a comment or directive behind the => of a switch arm`.
- FMT-UNSUP-019 — clarified: initializer members outside an arm or a member, and a property initializer's `=`, are now FMT-UNSUP-020's (text and the other-side example's lead sentence).
- FMT-UNSUP-011, FMT-UNSUP-012, FMT-UNSUP-017 — clarified: their *Why* and *These gaps only* text points to FMT-UNSUP-020/021 where they said the gap was supported.
- FMT-BREAK-016, FMT-BREAK-041 — clarified: the join behind a simple assignment's or a property initializer's `=` never meets a comment; a comment behind a switch arm's `=>` is refused, a line break alone there is kept.
- FMT-INDENT-034 — clarified: the list of lines it places names the refused cases (its examples use compound operators and stay valid).
- RH7101-007, RH2001-007 — clarified: the fix is withheld for the new shapes; RH2001 names `= new R { A = /* three */ 3 };` and an arm inside the initializer.
- `AGENTS.md` precheck row — fourteenth and fifteenth shape added.
- `sdd/specs/requirement-ids.txt` — FMT-UNSUP-020, FMT-UNSUP-021 inserted.

No rule page changes.

## Examples for the tests

- FMT-UNSUP-020: four examples — statement and property initializer with `//` (location 3,24); one-line `x = /* one */ 1;` plus object, `with`, and index initializer members (location 12,13); directive behind `=` (location 6,1); the other side (enum member, comment in front of a property's `=`, parameter default, comment in front of an assignment's `=`, compound `+=`, comment inside the value).
- FMT-UNSUP-021: two examples — `// one` and `/* two */` behind arms (location 7,25); the other side (line break alone behind `=>`, comment behind the arm's expression on either side of the comma, block-bodied lambda, `case` label).
- The change doc's *Code fixes* list: RH7101, RH2001, RH3202 (and RH3203 by the same base), RH5206, RH5301 move from fixed to no fix offered; each *before* is the `scripts/apply-fix.sh` result on `main` (RH3203 was not run; it shares `ExpressionBodyToBlockCodeFixProviderBase` with RH3202).

How I checked: every C# block of FMT-UNSUP-020/021 is a fixed point of `scripts/format.sh` on this branch's base (each was formatted and came back byte-identical apart from the final newline), so the refused ones show what *after* keeps and the other-side ones are formatted without a report. The change doc's *before* outputs come from `scripts/format.sh` and `scripts/apply-fix.sh` (RH7101, RH2001, RH3202, RH5206, RH5301) on `main`. `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` names FMT-UNSUP-020 and FMT-UNSUP-021 (no tagged test yet), which the test implementer resolves.

Facts for the skeptic:

- The issue says the line behind a switch arm's `=>` stays a continuation line "only because a comment keeps it apart". That does not hold: a line break alone behind an arm's `=>` is kept too (FMT-BREAK-041 lists it as not joined); `1 =>⏎2,` formats to `2,` on a continuation line. So FMT-UNSUP-021 deletes nothing; the change doc says it is declared on the maintainer's explicit answer.
- `A += { 1 }` inside an object initializer is a syntax error (`scripts/format.sh` skips it as such), which is why the change doc lists the FMT-BREAK-001 refusal in `LineBreakAssignmentRewriter`'s collection-initializer join as deleted. The other refusals in that rewriter serve compound operators or other equals-value clauses and are listed as kept.

## Output questions

**1 · Formatter · comment behind a compound assignment operator outside a switch arm or an anonymous-object member**

The maintainer's answer names "the `=` of an assignment". The spec is written on that literal reading (A). Whether `+=`, `??=`, and the other compound operators belong to the shape is undecided; it changes what the formatter and the CLI do for statement-level code.

Input:

```csharp
class C
{
    int M(int x)
    {
        x += // add
        3;

        return x;
    }
}
```

Before (`main`):

```csharp
class C
{
    int M(int x)
    {
        x += // add
            3;

        return x;
    }
}
```

After, A (the spec as written): same as *before*; only `=` is refused.

After, B (recommended): the input unchanged.
`error: <file>(5,14): unsupported syntax: a comment or directive behind an assignment operator. The file was left unchanged.`
Code fixes no longer offered on that code: every fix that formats a node holding it (RH7101 on the type, RH3202/RH3203 on a converted body, RH5403, and others), as for A's shape.
Declared in this work.

Why B: then `x = // c` and `x += // c` behave the same, and the shape becomes one rule with one reason.
Special cases B would delete: the walk from an assignment up to the nearest switch arm or anonymous-object member that decides today's arm-and-member shape, and the comment-and-directive checks in the line join behind an assignment operator, for a plain value and for a collection expression.

If B is chosen, the spec author also: widens FMT-UNSUP-020 to every assignment operator; folds FMT-UNSUP-019 into it (retire it or narrow its reason, the lead decides); and replaces the compound-operator examples of FMT-INDENT-034 (`x += // c`, `y += // c⏎3 + 4`, the collection-element example, and the RH5302 root example `var r = (x += // c⏎3) > 0 &&⏎y;`), for instance with an enum member's `=` and a line break alone behind an arm's `=>`, both of which `scripts/format.sh` places at the continuation indent.

Requirement IDs: FMT-UNSUP-019, FMT-UNSUP-020, FMT-INDENT-034, FMT-BREAK-016.

The change doc lists the other visible changes for the lead's batch: the statement and property-initializer cases, one-line comments and initializer members (`new R { A = /* c */ 3 }` at statement level, formatted on `main`), a directive behind `=`, the switch arm cases, and the code fixes newly withheld. The maintainer's answer covers them. The initializer-member reading is in *Internal choices*.

## Internal choices

- Two requirements (FMT-UNSUP-020 for `=`, FMT-UNSUP-021 for an arm's `=>`), each with its own reason, instead of one requirement with two reasons. Alternative: one requirement in the style of FMT-UNSUP-017's table.
- Object-, `with`-, and index-initializer members count as assignments (they are assignment expressions, as FMT-UNSUP-019 already counts them). This is visible output and is listed as such in the change doc. Alternative: exclude them, which needs a parent condition and leaves `new R { A = // c⏎3 }` on a continuation line.
- The new shapes are listed after every other shape, so an overlapping shape keeps its reason (FMT-UNSUP-019 for `=` in an arm or member, FMT-UNSUP-010 for a directive in a multi-declarator field). Alternative: exclude the arm and member from FMT-UNSUP-020 by the FMT-UNSUP-019 walk.
- A property initializer counts only where the property declaration is part of the input (FMT-UNSUP-003), as FMT-UNSUP-004 does for a declaration. Alternative: decide by the equals-value clause's parent, which reads outside the input.
- The spec is written on the literal reading (A) of output question 1, not on the recommendation, because the maintainer's answer covers only A.

## Follow-up candidates

- **Escalation 10 option, outside the issue:** the remaining gaps behind an `=` that a comment or directive still holds on a continuation line: a directive or disabled text behind a variable declarator's `=` (FMT-UNSUP-004 refuses only comments there), and a comment, directive, or disabled text behind an enum member's `=` or a parameter's default `=`. Input `enum E { A = // c⏎1 }` → today `1` at the continuation indent (verified); declaring it would refuse and report the file. The same FMT-BREAK-016 equals-value join refusal serves all three, so declaring all of them, together with FMT-UNSUP-020 and option B, would delete it. Recommendation: file as an `agent-filed` follow-up carrying the escalation-10 question, as a change of its own.
