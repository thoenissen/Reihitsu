Role: spec author
Commit: 910a09e5
Verdict: change doc written

Inputs read: thoenissen/Reihitsu#1888 and its one comment (triage placement only), `00-lead.md`, `sdd/changes/2026-11/unsupported-link-operator-trivia.md` (*Decisions*: the maintainer's answer 2A to declare this shape in a change of its own), `sdd/specs/formatter/unsupported-syntax.md`, FMT-BREAK-001/019/020, SHARED-CHAIN-001/006, FMT-INDENT-013, RH2001-007, RH7101-007. The prompt carried no intended fix or verdict.

Written:

- Change doc `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md` (`Status: Draft`) with visible output changes, *Special cases* (Deleted / Checked and kept), decisions, alternatives.
- Spec diff: `sdd/specs/formatter/unsupported-syntax.md`, `sdd/specs/formatter/line-breaks.md`, `sdd/specs/analyzer/RH2001.md`, `sdd/specs/analyzer/RH7101.md`, `sdd/specs/requirement-ids.txt`.
- `AGENTS.md`: the `PrecheckPhase` row names the tenth shape.
- No rule page changes: the rules lose their fix only for the refused shape (`general-notes.md` covers it).

## Changed requirements

- FMT-UNSUP-016 — Comments and directives between a link's dot and its member name are unsupported — added
- FMT-UNSUP-015 — Comments and directives inside a link operator are unsupported — changed (the gap behind the operator's `.` now names FMT-UNSUP-016; no behavior of FMT-UNSUP-015 changes)
- FMT-BREAK-020 — Split member names are rejoined, and split attached parts are no wrap — changed (the join never meets the shape; no member name of a link starts a line)
- RH2001-007 — The fix is withheld for a comment or directive inside the property — changed (`= Source. /* three */ Length;` withholds the fix)
- RH7101-007 — The fix is withheld for a comment inside the declaration — changed (names FMT-UNSUP-016)
- `Uses` rows: unsupported-syntax.md gains FMT-BREAK-020 and FMT-INDENT-013; line-breaks.md, RH2001.md, RH7101.md gain FMT-UNSUP-016.

## Examples for the tests

- FMT-UNSUP-016: the three examples (refused links, location line 7 col 24; directive in every build, location line 8 col 1; the other side, formatted). Every condition bullet carries inline cases on both sides; the FMT-UNSUP-004/010 precedence paragraph gives `[Obsolete(nameof(C. /* c */ M))] private int _e;` (FMT-UNSUP-004's reason) against `int c = t. /* c */ Length;` (this reason).
- Change doc, *Visible output changes* 1 and 2: four CLI inputs with locations (5,24), (8,1), (7,18), (7,21); withheld fixes for RH5201, RH5112, RH3202, RH2001 (RH2001-007), RH7101 (RH7101-007).
- FMT-BREAK-020: no new example; the refusal is FMT-UNSUP-016's.

Checks run:

- `scripts/format.sh` on every formatter input of the change doc and on the FMT-UNSUP-016 examples: the *before* outputs in the change doc are today's output (up to the CLI's final-newline removal). The other-side example comes back unchanged apart from the final newline. The refused examples change today, as the change doc shows.
- `scripts/apply-fix.sh RH5201|RH5112|RH3202|RH2001|RH7101` on the five fix inputs: each fix is offered today, `fixed (1 action(s), 1 iteration(s))` under LF and CRLF, with the *before* outputs quoted.
- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: `FMT-UNSUP-016: no test covers it`. That is expected until the test implementer tags covering tests; nothing else fails.

## Output questions

**1 · Formatter/CLI · reason text for the new unsupported shape**

The change doc's CLI lines use a reason the maintainer has not chosen yet. The reason was asked as an output question for FMT-UNSUP-015 too (batch 1, answer 1A). Input (any of the change doc's, e.g.):

```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B(). // c
            C();

        return r;
    }
}
```

Before (`main`): formatted, `C();` moves to the statement's column (change doc, change 1).

After, A (recommended): file unchanged, CLI reports
`error: <file>(5,24): unsupported syntax: a comment or directive between a . and its member name. The file was left unchanged.`

After, B: file unchanged, CLI reports
`error: <file>(5,24): unsupported syntax: a comment or directive between a member access's . and its name. The file was left unchanged.`

Recommendation: A — it names the place in the words a user reads in the code, like FMT-UNSUP-015's `inside a ?. or !. operator`, and it fits `?.` (member binding) as well as `.`.

The visible output changes in the change doc (refusing the four inputs, withholding the five fixes) are covered by the maintainer's pre-run answer to declare the shape; I list no further output question for them, but the lead decides whether the batch shows them.

## Internal choices

- The shape is a requirement of its own (FMT-UNSUP-016) with its own reason, not folded into FMT-UNSUP-015; alternative: widen FMT-UNSUP-015 and its reason. FMT-UNSUP-015 is about the operator's tokens and its reason names the operators.
- The shape is defined by syntax kind — the operator token of a `SimpleMemberAccessExpression` or a `MemberBindingExpressionSyntax` and the first token of its name — not by the chain model's outermost-chain walk, so every member access counts, also one the chain layout never treats as part of a wrapped chain (an assignment target, `nameof(...)`). Alternative: only links of a `FluentChain` the line-break phase builds.
- On any line, and directives and disabled text included, as for FMT-UNSUP-015 (same reasoning; recorded in the change doc's *Decisions*).
- Precedence with FMT-UNSUP-004 and FMT-UNSUP-010 follows FMT-UNSUP-014/015's paragraph unchanged. FMT-UNSUP-015 and FMT-UNSUP-016 never claim the same trivia: the operator's last token is the `.`, and FMT-UNSUP-015 counts only the `.`'s leading trivia, FMT-UNSUP-016 only its trailing trivia and the name's leading trivia.
- The only special case listed as deleted is `CloseGap`'s unjoinable-trivia check for the dot-to-name gap in `ChainLineBreakRewriter.NormalizeChain` (the issue names the same one). I found no other code that serves the shape: the indentation phase has no rule for a name below its dot, and FMT-INDENT-013's list-element alignment and the spacing phase's `NormalizeGapBehindLeadingComment` are generic; both are listed under *Checked and kept*.

## Follow-up candidates

**1 · Formatter · a comment between a qualified name's `.` and its name outside a base list** (outside #1888; FMT-INDENT-013 specifies the base-list case only). Reached through whole-document formatting and the CLI. Input:

```csharp
using System. // c1
    Text;

class C
{
    void M()
    {
        N(new System. // c2
            Text.StringBuilder());
    }

    void N(object o)
    {
    }
}
```

Today's output: `Text;` of the using directive moves to column 1; the continuation inside the argument aligns with the argument (FMT-INDENT-013), which looks right.

```csharp
using System. // c1
Text;
```

Why it looks wrong: the continuation of a using directive at column 1 reads as a new line of code. Readings: (A) indent the continuation one level, like other continuation lines; (B) leave the directive as written; (C) declare the gap between a qualified name's `.` and its name unsupported. Option C is an escalation-10 option and must reach the maintainer as such; I do not recommend it here — it would refuse the base-list layout FMT-INDENT-013 specifies — and recommend filing the case with options A and B, A recommended.

## Architecture findings

_None._
