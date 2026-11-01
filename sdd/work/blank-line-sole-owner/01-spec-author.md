Role: spec author
Commit: c32c391c45df46a4a9aea7c6992ab5963bdb7058
Verdict: change doc written

Change doc: `sdd/changes/2026-11/blank-line-sole-owner.md` (`Status: Draft`). The prompt carried no intended fix or verdict to ignore.

## Issue statements checked against the current tree

Every statement of thoenissen/Reihitsu#1496 about today's code still holds at c32c391 (after #1490/#1491 merged); none is moot:

- `BlankLineEditor.EnsureBlankLineAfterClosingBrace` inserts the SHARED-BLANK-003 line above a block statement, and `BracePlacer` → `TokenGapNormalizer` (`blankLineCount: 0`) removes it again in every pass. Confirmed with `scripts/trace.sh` on the issue's first input: pass 1 and pass 2 both show the insertion (blank-line phase) and the removal (line-break phase).
- Comment glued to `{` (`Child =⏎/* c */ {`): same insert/remove pair in every pass (trace of the issue's second input); `TokenGapNormalizer.HasOwnLinePlacementOwner` still exists.
- `LineBreakBlockRewriter.GapCarriesDirectiveOrDisabledText` and its collapse to one blank line (`blankLineCount: 1`) still exist.
- `RootListPosition.KeepsBlankLineAboveBlockStatement` still calls `TokenGapNormalizer.HasOwnLinePlacementOwner`.
- `PhaseReversalExceptionList` still carries three `BlankLineNextToBrace` entries (two `CommentLedFirstTokenAnchorTests`, one `StructuralTransformSeparatorFinalOutputTests.MembersWithACommentAtTheArrowReachAStableOutput`, whose reversal is `int Get3()⏎/* c */ {` — a comment glued to a method body's brace, covered by FMT-BLANK-018).
- thoenissen/Reihitsu#1396 and thoenissen/Reihitsu#1081 are open and unimplemented: RH5030 has no block-statement exemption, RH5025-001 does not name the standalone block.

What the issue did not know (found by tracing current `main`):

- The line-break phase is also the only remover of a blank line before the closing `]` of a collection expression or list pattern and the closing `)` of a parenthesized pattern (`LineBreakInitializerRewriter`, `blankLineCount: 0`). Without a new blank-line rule this output would change → FMT-BLANK-019 (clarification, no visible change).
- The glued-comment reversal is not limited to `{`: it also happens for a comment glued to closing braces (`/* d */ }` in blocks, initializers, switch expressions, property patterns, type bodies), to the `]` of a collection expression or list pattern, and to the `)` of a parenthesized pattern. It does **not** happen for an array initializer's braces, a property pattern's opening brace, an argument/parameter/tuple `)`, an element-access `]`, or an opening `[`: there the FMT-BLANK-011 line stays today. The line-break phase also removes an author's blank line above such a glued comment, not only the inserted one.
- `break;⏎{` reverses too: FMT-BLANK-004's blank line after `break` is removed by the line-break phase (`ReihitsuFormatterNodeBlankLineTests.DoesNotInsertBlankLineAboveBlockStatementAfterBreak` pins it). → output question 1.
- `}⏎#if X⏎{` gets no blank line today (FMT-BLANK-009 removes it); `A();⏎#pragma …⏎⏎{` loses the author's blank line today (FMT-BLANK-009).

## Changed requirements

- FMT-BLANK-008 — No blank line before an opening brace or a chained keyword — changed (does not apply to the brace FMT-BLANK-017 names)
- FMT-BLANK-009 — Around a directive, only the blank line next to the token is removed — changed (same exclusion)
- FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line — changed (exclusion for FMT-BLANK-018)
- FMT-BLANK-017 — The opening brace of a block statement that follows a statement keeps the statement rules — added
- FMT-BLANK-018 — A comment glued to a brace gets no blank line of its own — added
- FMT-BLANK-019 — No blank line before the closing bracket of a collection expression or pattern — added (clarification: today's output, new owner)
- FMT-BREAK-030 — The phase keeps the blank lines of every gap it does not join — added; FMT-BREAK's non-goal and intro sentence changed to point at it
- FMT-PIPE-024 — Node-level input facts are plain values — changed (a block statement gets a list position like any other statement)
- RH5025-005 — The opening brace of a block statement that follows a statement is not reported — added

Ledger updated; `Uses` of `blank-lines.md` gains FMT-BREAK-022. `SpecConsistencyTests` and `SpecStructureTests` pass; `RequirementCoverageTests` fails only for the five new IDs, which need their tests.

Code owners are unchanged: no type is added or removed by the spec.

## Examples for the tests

- FMT-BLANK-017 — every example (after `}`, after another statement kept/none inserted, after `break`, behind `#if` and `#pragma`, first statement of a switch section and `if` body removed).
- FMT-BLANK-018 — author's blank line above `/* c */ {` in an initializer removed; glued comments at a block statement, initializer brace and closing brace unchanged; array initializer brace and argument-list `)` separated; block statement with glued comment after `}` and after `A();`.
- FMT-BLANK-019 — collection expression `]` and list pattern `])`.
- FMT-BREAK-030 — the line-break phase run alone leaves the example unchanged (phase-level).
- RH5025-005 — the analyzer example (two not reported, `case 1:⏎⏎{` reported).
- FMT-PIPE-024 — `FormatNodeInDocumentAsync` with a block statement as target behind `}` gets the blank line (`ReihitsuFormatterNodeBlankLineTests` expectations for block statements after `}` and after `break` change; the issue counts six).
- Pre-change outputs in the examples were confirmed with `scripts/format.sh`/`scripts/trace.sh` at this commit; post-change outputs are the spec's decision.

## Output questions

### 1 · Block statement after `break` (FMT-BLANK-004 vs. "after any other statement the author's blank is kept")

Input:

```csharp
class C
{
    void M(int a)
    {
        while (a > 0)
        {
            break;
            {
                a++;
            }
        }
    }
}
```

Real output on `main`: unchanged (no blank line); RH5011 reports the `break`. It looks wrong because the formatter's output is reported, and the only reason is the line-break phase this issue removes. Requirements: FMT-BLANK-004, FMT-BLANK-017, RH5011-001, RH5025-005.

- **A (recommended, written into the spec):** FMT-BLANK-004 applies like above any statement → `break;` / blank line / `{`. RH5011 and RH5025 silent.
- **B:** the maintainer's "after any other statement, keep the author's blank line, none required" also overrides FMT-BLANK-004 → output unchanged (no blank line), RH5011 keeps reporting the formatter's output.

Why A: FMT-BLANK-004 is, like SHARED-BLANK-003, a rule about the statement before the block, and A is the only option whose output no rule reports.

### 2 · Which block statements are exempt — braced switch section, top-level, labeled

Input (first statement of a switch section; the same question applies to a top-level `{` after `var a = 1;⏎⏎` in a file with top-level statements and to `L:⏎⏎{`):

```csharp
class C
{
    void M(int a)
    {
        switch (a)
        {
            case 1:

                {
                    a++;
                }
                break;
        }
    }
}
```

Real output on `main`: the blank line below `case 1:` is removed; RH5025 reports it. Requirements: FMT-BLANK-008, FMT-BLANK-017, RH5025-001, RH5025-005. The maintainer said "the `{` of any block statement", but named the two cases "after `}`" and "after any other statement", and superseded only the standalone-block case of thoenissen/Reihitsu#1081 item 1, which lists the braced switch section separately.

- **A (recommended, written into the spec):** only a block statement that follows another statement of its block or switch section is exempt → output unchanged (blank line removed, RH5025 reports), also top-level and labeled blocks unchanged.
- **B:** every block statement is exempt →

```csharp
            case 1:

                {
                    a++;
                }
```

kept as written, RH5025 silent; likewise the top-level and labeled forms keep the author's blank line.

Why A: the label is the braced section's header, as #1081 decided, and A changes nothing beyond the two cases the maintainer named.

## Internal choices

- FMT-BLANK-018's token set keeps today's output exactly (braces except array-initializer braces and a property pattern's opening brace; `]` of collection expression/list pattern; `)` of parenthesized pattern; author's blank line removed too), instead of the issue's literal "the `{`", which would add a blank line above glued comments at closing tokens and remove one at array-initializer/property-pattern braces. Alternative: the literal wording (visible changes, would need an output question).
- Precedence between insertion and removal rules is decided by narrowing scopes (FMT-BLANK-008, FMT-BLANK-011), stated once in the change doc's `Precedence` section, with no separate ranking requirement. Alternative: a FMT-BLANK tie-break requirement "removal wins", which no current input would exercise.
- FMT-BLANK-019 added as a clarification so the line-break phase can drop its blank-line half without changing output. Alternative: leave those blank lines to the author (visible change).
- The line-break phase's "keep the blank lines" is a new requirement FMT-BREAK-030 (testable, phase alone), not only a non-goal sentence.
- RH5025 rule page (`documentation/rules/RH5025.md`) left unchanged: it speaks of a declaration and its opening brace; the block-statement exemption is an exception. The issue's "rule page changes" goal is dropped for that reason.
- RH5030 and RH5020 specs unchanged; the change doc records that both keep reporting what they report today (maintainer decisions).

## Follow-up candidates

### Parenthesized pattern content loses its indentation

Outside this run (indentation/line-break layout, not blank lines). Reachable through the CLI and whole-document formatting.

Input:

```csharp
class C
{
    void M(object o)
    {
        if (o is (
                1 or 2
                ))
        {
        }
    }
}
```

Real output on `main` (`scripts/format.sh`):

```csharp
class C
{
    void M(object o)
    {
        if (o is
              (
        1 or 2
              ))
        {
        }
    }
}
```

It looks wrong: the pattern's content sits left of its own parentheses, at the statement's column, and the `(` moved off the `is` line. Readings: content aligned one level inside the parentheses (for example `(` / `    1 or 2` / `)` under `is`), or the parenthesized pattern kept on the `is` line. Recommendation: file as an `agent-filed` follow-up against FMT-BREAK-022/FMT-INDENT; the maintainer picks the layout.
