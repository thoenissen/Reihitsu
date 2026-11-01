Role: spec author
Commit: d4465ac
Verdict: change doc written

Amendment round for skeptic findings M1, M2, M3 (`02-skeptic.md`, lead decisions 2–4). The prompt carried no intended fix or verdict to ignore. I treated the skeptic's I1–I3 as suggestions and re-derived each member at d4465ac with `scripts/format.sh --force` (CLI, whole document), `scripts/trace.sh`, and `scripts/apply-fix.sh`. The fixtures are in the session scratchpad, LF only. The four open output questions stay open. The spec stays on option A for each, and a fifth question is added (below). Change doc: `sdd/changes/2026-11/blank-line-sole-owner.md`, still `Status: Draft`.

## What re-deriving changed against the skeptic's handover

- **M1 confirmed, and the token set is exact.** The line-break phase removes the blank lines between an own-line comment and the token behind it for exactly the tokens of the narrowed FMT-BLANK-018 set, because the call sites are the same. It covers block, type, namespace, enum, accessor and switch braces, method, `if`, `else`, lambda and anonymous-method bodies, object, collection, `with` and anonymous-object initializers, switch expressions, property-pattern `}`, complex-element `}`, collection-expression and list-pattern `]`, and parenthesized-pattern `)`. It does **not** cover array-initializer, extension-block and interpolation braces, property-pattern `{`, complex-element `{`, or chained keywords: today's output keeps the blank line there, and RH5024 and RH5025 report that output (`apply-fix.sh`: fixed). The skeptic's table did not include those tokens, so a rule written for every brace would have changed visible output.
- **I3 placed differently.** The skeptic proposed sentences in FMT-BLANK-007, FMT-BLANK-008 and FMT-BLANK-019. I wrote one new requirement, FMT-BLANK-020, because its token set is FMT-BLANK-018's while 007, 008 and 019 cover every brace directly in front of the token (internal choice below). A documentation comment needs nothing new: the blank-line phase already removes the blank line behind it (FMT-BLANK-010, traced).
- **M2 confirmed** for all three owners: complex-element `{`, interpolation `}`, and extension-block `{` and `}`. In each case the blank line is kept, or inserted when the input has none.
- **M3 confirmed. One effect is new: the line-break phase inserts a blank line in front of a directive.** With a non-region directive and a blank line in front of a glued comment (`a++;⏎#pragma …⏎⏎/* d */ }`, `#if`/`#endif` variants, initializer variant), today's `LineBreakPhase` inserts a blank line in front of the first directive of the gap (trace: `LineBreakPhase` hunk `+` above `#if`/`#pragma`). FMT-BREAK-030 drops that insertion, and no blank-line rule asks for it. This is visible output change 10 and output question 5. No analyzer reports either output (RH5020, RH5022, RH5024: no-diagnostic).
- **Directive before a closing bracket.** Today the line-break phase removes `#endif⏎⏎]` and `#endif⏎⏎)`, and keeps the blank line above the directive, so the behavior matches FMT-BLANK-009. FMT-BLANK-009 now covers FMT-BLANK-019's tokens. The skeptic had not traced this case.
- **I2 scope.** The skeptic excluded only a blank line that a non-region directive separates from the comment. Today's output keeps the blank line above a glued comment whenever any directive is in the gap, including `#endif⏎// c⏎⏎/* d */ };`. FMT-BLANK-018 therefore removes nothing in a gap that holds a directive.

## Changed requirements

- FMT-BLANK-009 — Around a directive, only the blank line next to the token is removed — changed: also covers FMT-BLANK-019's `]` and `)`. "Between the last directive and the token" now applies only when nothing but whitespace stands between them; behind a comment, FMT-BLANK-020 decides. Two examples added.
- FMT-BLANK-017 — The opening brace of a block statement that follows a statement keeps the statement rules — changed: FMT-BLANK-020 does not apply either, and the author's blank line between a comment and the brace is kept.
- FMT-BLANK-018 — A comment glued to a brace gets no blank line of its own — changed (still new in this run): token set narrowed (M2), no removal in a gap that holds a directive (M3), `Why` updated, two examples added.
- FMT-BLANK-019 — No blank line before the closing bracket of a collection expression or pattern — changed (still new in this run): does not remove the blank line below a region directive, and points to FMT-BLANK-009 and FMT-BLANK-020.
- FMT-BLANK-020 — Behind a comment on its own line, no blank line before a brace or closing bracket — added (clarification: today's output, new owner). Ledger updated.
- Spec intro of `blank-lines.md` names the new removal.
- Change doc: `Affected requirements`, `What` (FMT-BLANK-018, FMT-BLANK-019, new FMT-BLANK-020 bullet), `Precedence` rewritten (the claim "no blank line decided by two rules" now holds, and the region-directive and directive members are named), visible output changes 7–10 added, `Unchanged` extended, five decisions added or rewritten, four alternatives added.

`SpecConsistencyTests` and `SpecStructureTests` pass. `RequirementCoverageTests` fails only for the IDs that still need tests (FMT-BLANK-017 to FMT-BLANK-020, plus the earlier FMT-BREAK-030 and RH5025-005).

## Examples for the tests

- FMT-BLANK-009: the new `]` example and the comment-behind-directive example. Both match today's output.
- FMT-BLANK-018: every example. The complex-element example matches today's output. In the directive example, method `N` differs from today only by the blank line today inserts above `#pragma` (question 5).
- FMT-BLANK-020: every example. The main example, the first-statement block and the trailing comment match today's output. The block statement after `A();` is post-change (question 3).
- Members to pin beyond the examples, LF and CRLF:
  - every M1 row of the token list above, after `A();` and after `}` for the block statement;
  - `break;⏎// c⏎⏎{` and `break;⏎/* c */ {` (questions 1 and 3);
  - `#endif⏎// c⏎⏎}` and `#endif⏎// c⏎⏎]` (removed);
  - `#endregion⏎⏎]`, `#endregion⏎]`, `#endregion⏎⏎)`, `#endregion⏎⏎/* d */ ]`, `#endregion⏎⏎/* d */ }` and `#endregion⏎/* d */ }`: blank line kept or inserted. **Assert blank lines only**: the region directive's column inside collection expressions, patterns and initializers is the skeptic's follow-up candidate, and the spec deliberately carries no example that would pin it;
  - `#pragma`/`#endif` + blank + glued comment, with the author's blank above, below, or on both sides (question 5);
  - M2's three owners, with and without the author's blank line.

## Output questions

Questions 1–2 (`01-spec-author.md`) and 3–4 (`02-skeptic.md`, there numbered 1–2) stay open and unchanged. The change doc numbers them 1–5 in this order. The spec is written on option A of each. One question is new:

### 5 · Formatter · directive in front of a comment glued to a brace

Input:

```csharp
class C
{
    void M(int a)
    {
        a++;
#pragma warning disable CS0168

    /* d */ }
}
```

Real output on `main` (the line-break phase inserts the blank line above `#pragma`):

```csharp
class C
{
    void M(int a)
    {
        a++;

#pragma warning disable CS0168

    /* d */ }
}
```

This looks wrong because no blank-line requirement asks for a blank line in front of a directive, and FMT-BLANK-016 forbids one for every statement blank line. Once the line-break phase keeps blank lines (FMT-BREAK-030), the blank line disappears unless a new rule writes it. The same happens for `#if … #endif` in front of a glued comment and in initializers (`X = 1⏎#if NEVER … #endif⏎⏎/* d */ };`). Requirements: FMT-BLANK-018, FMT-BLANK-009, FMT-BLANK-016, FMT-BREAK-030.

- **A (recommended, written into the spec):** author's blank lines kept as written, none inserted. Output = the input.
- **B:** today's output, as above (needs a blank-line rule that inserts a blank line in front of a directive).
- **C:** FMT-BLANK-009's pattern applied to the glued line. The blank line between the directive and the glued comment is removed, and the author's blank line above the directive is kept:

```csharp
class C
{
    void M(int a)
    {
        a++;
#pragma warning disable CS0168
    /* d */ }
}
```

Why A: it changes nothing but the blank line the line-break phase inserted, it follows lead decision 4 (the blank lines below `#endif` keep today's output), and no analyzer reports it.

## Internal choices

- The comment rule is a new requirement, FMT-BLANK-020, instead of sentences in FMT-BLANK-007, FMT-BLANK-008 and FMT-BLANK-019 (lead decision 2 named those three). Reason: its token set is FMT-BLANK-018's and theirs is every brace, so each would have to repeat the exceptions. Alternative: the skeptic's three sentences.
- FMT-BLANK-020 is limited to FMT-BLANK-018's tokens, so that no output changes. Alternative: every brace and chained keyword. That would be a visible change, with RH5024, RH5025 and RH5026 silent on the result; see the follow-up below.
- FMT-BLANK-018 removes nothing in a gap with any directive, not only directly below one. Alternative: the skeptic's narrower reading, which would change `#endif⏎// c⏎⏎/* d */ };`.
- FMT-BLANK-009 is extended to `]` and `)` for directives, instead of a directive clause in FMT-BLANK-019.
- FMT-BLANK-019 has no region-directive example, so as not to pin the directive's column, which the skeptic filed as a follow-up candidate.

## Follow-up candidates

**Formatter / RH5024, RH5025 · blank line between an own-line comment and a brace the line-break phase never normalizes.** This is outside the run: no line-break code is involved before or after the change. It is reachable through the CLI.

Input:

```csharp
class C
{
    int[] G =
    // c

    {
        1,
        2
        // d

    };
}
```

Real output on `main`:

```csharp
class C
{
    int[] G =

    // c

    {
        1,
        2

        // d

    };
}
```

Both blank lines around the braces stay, while the direct form `=⏎⏎{` / `2⏎⏎}` is removed. `apply-fix.sh` reports and fixes this output for RH5024 and RH5025 (FC-013). The same holds for extension-block braces and property-pattern `{` (verified), and by the same code path for complex-element `{`. Readings: FMT-BLANK-020 extends to these braces (the blank lines are removed and the analyzers are silent), or the analyzers do not report a blank line behind a comment. Recommendation: extend FMT-BLANK-020, which matches what the formatter does for every other brace. This is the brace counterpart of the skeptic's chained-keyword candidate (RH5026), and both can be one issue.

## Maintainer answers (follow-up, head 2331bd4)

The maintainer chose option A for all seven questions of the batch: this handover's 1–5, plus 6 (directive in front of a block statement after `}`) and 7 (comment glued to a block statement's brace after `}`). The change doc records them under `Decisions`, its `Pending output question` notes now read as decided, and its status is `Accepted`, set for the lead per `sdd/README.md`. The spec already matched option A everywhere and is unchanged: FMT-BLANK-017 covers 1, 2, 3 and 6, FMT-BLANK-018 covers 4, 5 and 7, and FMT-BLANK-020 also covers 3.
