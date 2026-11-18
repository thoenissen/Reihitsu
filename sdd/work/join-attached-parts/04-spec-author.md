Role: spec author
Commit: 55d27ef
Verdict: spec written

Carries out the maintainer's answers recorded in `00-lead.md` (batch 1: 1A–5A; question 6: declare the shape, 6.1A, 6.2A, 6.3A). The prompt carried no intended fix or hoped-for verdict beyond those answers.

The declaration deletes special cases (change doc *Special cases*), so it is not the "deletes nothing" case:

- `ArgumentListLineStartRewriter.CanJoin`'s `WouldJoinAcrossUnjoinableTrivia(previousToken, token) == false` term — exactly FMT-UNSUP-014's gap for FMT-BREAK-039's argument lists, both tokens inside the root.
- `MethodChainAlignmentContributor.Contribute`'s column for an attached part's first token that starts a line (`"MethodChainAttachedPart"`, root column vs. anchor column). On `main` it is reached only through blocking trivia (in a chain with links every other such gap is joined already); confirmed by output: `items.Where(…)⏎.ToArray() // c⏎[0]` keeps `[0]` aligned to the links on `main`.
- The FMT-BREAK-040 owner that decision 3 creates carries no refusal for the gap in front of the part's first token.

Kept, with reasons in the change doc: `CloseGap`'s refusal (member-name rejoin, link operators), `ListElementInteriorJoinRewriter`'s refusal (all interior wraps), the refusal for the `?`–`[` gap and `AlignRestOfRun` (the shape does not cover that gap), the parameter-list `(` joins, the Core chain model's trivia checks (analyzers), the code fixes (no own guard).

## Changed requirements

- FMT-UNSUP-014 — Comments and directives in front of an argument list, an element access, or a null-forgiving operator are unsupported — added (ID registered).
- FMT-BREAK-039 — An argument list starts on the line of the code in front of it — changed: *Why* states the join never meets the shape; the comment/directive example is now refused ("returned unchanged and reported as unsupported").
- FMT-BREAK-040 — changed (Draft with the change doc): *Why* states the join in front of the part never meets the shape, and that the `?`–`[` gap still yields to FMT-BREAK-001.
- RH2001-007 — changed: a comment in front of an argument list, an element access, or a null-forgiving operator of the initializer's value withholds the fix (FMT-UNSUP-014); Uses row.
- RH7101-007 — changed: names FMT-UNSUP-014 among the shapes that withhold the fix; Uses row.
- `AGENTS.md` PrecheckPhase row: the eighth shape.
- Change doc `sdd/changes/2026-11/attached-part-below-element.md`: `Status: Accepted`; *What* states the declaration; visible output changes 6 (file refusals with CLI lines) and 7 (RH3202, RH2001, RH7101 fixes withheld), all *before*s produced with `scripts/format.sh` / `scripts/apply-fix.sh` on the current code; *Decisions* records 1A–5A and 6.1A/6.2A/6.3A plus four decisions; *Special cases* (Deleted, Checked and kept); *Alternatives* updated.

`scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: FMT-BREAK-040 and FMT-UNSUP-014 have no covering test (expected).

## Examples for the tests

- FMT-UNSUP-014: the three examples (refused parts with location line 7, column 21; directive with location line 6, column 1; the other side, formatted). Also each condition's both-side cases: `x! /* c */ ?[0]` and `x /* c */ !?[0]` refused; `s // c⏎!.Length`, `n /* c */ ?.Length`, `[Obsolete /* c */ ("x")]`, `void M /* c */ (int x)`, `n? /* c */ [0]` not refused; node-level: the argument list alone and the invoked expression alone are not refused; FMT-UNSUP-004 wins for `[Obsolete(nameof /* c */ (C))] private int _e;`, FMT-UNSUP-010 for a directive in front of a part in a field with several declarators.
- FMT-BREAK-039: the changed comment/directive example (now refused).
- FMT-BREAK-040: `n? // c⏎[0]` keeps its line break (stated in *Why*; not an example block).
- Change doc changes 6 and 7: the CLI lines and the three withheld fixes.
- Each deleted special case needs no throw-site test (no phase refusal is added; FMT-UNSUP-006 unchanged).

## Output questions

**1 · Formatter · a block comment in front of an argument list or index on the same line**

The maintainer's 6.3A names "a comment, a directive, or disabled text in the gap"; I wrote the spec on that literal reading, which includes a gap without a line break, as FMT-UNSUP-008 and FMT-UNSUP-011 do. That member deletes no special case of its own. In the precedent (FMT-UNSUP-011) the same-line member was asked separately, so the lead may want to confirm it.

Input:
```csharp
class C{
    int M(int[] arr)
    {
        return Foo /* x */ (arr /* first */ [0]);
    }

    int Foo(int x)
    {
        return x;
    }
}
```

Before (`main`): only `class C{` is corrected; both comments stay.

After, A (as written, recommended): the file is unchanged; `Error: <file>(4,20): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator. The file was left unchanged.` Also RH2001/RH7101 fixes on `= Offset /* three */ (3)` (offered on `main`) are withheld.

After, B: as on `main` — only a gap with a line break is refused.

Why A: one rule for the place, as for the other operator shapes; the comment is rare either way. If B is chosen, FMT-UNSUP-014's *On any line* condition, the change doc's last example of change 6, and the decision "The gap in front of the part, on any line" change.

## Internal choices

- Defined the `?` and `!` of the shape by the syntax tree (the token's parent and the next token), not by the `FluentChain` model, so the precheck needs no chain model. Alternative: reuse `FluentChainAttachedPart.FirstToken`, which would leave `x!⏎?[0]`'s `?` gap out of the shape.
- Kept the `?`–`[` inner gap out of the shape, as 6.2A names the gap in front of the part; listed including it as an alternative in the change doc (it would delete the remaining inner-gap refusal).
- Reason text `a comment or directive in front of an argument list, an element access, or a null-forgiving operator` (the conditional element access is omitted from the reason for length; the requirement text names it).
- CLI lines written with `Error:` as the CLI prints today (the binary-operator change doc wrote `error:`).
- Updated `AGENTS.md`'s PrecheckPhase row with the eighth shape in this commit, as the earlier declarations did.

## Follow-up candidates

- A comment on a line of its own in front of a wrapped chain link inside a statement gets a blank line above it (outside this run's shape). Input `var d = s⏎    // c7⏎    .Length;`, output on `main`:
  ```csharp
          var d = s

                  // c7
                  .Length;
  ```
  The blank line splits the statement; no requirement seems to call for it (looks like FMT-BLANK treating the comment line as a statement boundary).
