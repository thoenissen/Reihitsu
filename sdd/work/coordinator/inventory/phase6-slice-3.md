# Phase 6 slice 3 (issues #1568–#1685)

Snapshot: 2026-11-16, working tree `claude/sdd-coordinator` (= `origin/main` at `1d2d29a9`, #1779). All 31 issues open; body and every comment read.

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 1568 | vNext | formatter/IndentationPhase (LambdaAlignmentContributor) | continuation columns | open | — | M | yes | — | yes | Alignment still gated by `IsAlignedInRawStringHole` (`LambdaAlignmentContributor.cs:90,140`). |
| 1571 | Maintenance | formatter/BlankLinePhase | multi-line comment closing line | open | — | S | no | — | no | `BlankLineEditor.InsertBlankLineBeforeToken` still writes the EOL (`BlankLineEditor.cs:336`); run with #1572, #1641. |
| 1572 | Maintenance | specs only (SHARED-COMMENT-012) + tests | multi-line comment closing line | open | — | M | no | — | no | SHARED-COMMENT-012 (`comment-placement.md:571`) still says the formatter breaks in front of the token, except statements/sections/labels. |
| 1574 | Maintenance | code fixes (shared) + formatter/BlankLinePhase | one-owner refactor | open | — | S | no | — | no | Formatter `BlankLineRegionDirectiveRewriter` still decides on its own (`:90,113`); Core `RequiresBlankLineBelow` has no formatter caller. |
| 1576 | Maintenance | tests/self-hosting | test infrastructure | open | — | S | no | — | no | `Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests` (moved project) has no `PhaseStabilityCheck`. |
| 1577 | Maintenance | tests/formatter | test infrastructure | open | — | S | no | — | no | None of the seven classes calls the per-phase check yet; pair with #1576. |
| 1579 | vNext | formatter/CleanupPhase | directive normalization | open | — | S | yes | — | no | Near-duplicate scope of #960 (slice 1); `cleanup.md:19` still lists directive-line whitespace as non-goal; run together. |
| 1583 | Maintenance | formatter + code fixes (shared) + Core | one-owner refactor | open | — | S | no | — | no | Premise moved: owner `TriviaListUtilities` is now in `Reihitsu.Formatter` (not Core); copies in `LineBreakTriviaUtilities:368`, `BlankLineCodeFixUtilities:253`, and a new one in Core `DeclarationModifierUtilities:279`, so the owner must move to Core. |
| 1585 | vNext | formatter + CLI + analyzer (generated-code detection) | generated-code detection | open | — | M | yes | — | yes | FMT-PIPE-006 still header-only, case-insensitive; overlaps #1584 (analyzer side) and the CLI file-name list. |
| 1586 | vNext | formatter/StructuralTransformPhase (field split) + RH7101 fix | field splitting | open | — | S | yes | 1728 | yes | `CarriesDirective` still scans all trivia (`FieldDeclarationSplitTransform.cs:75`); once #1728 declares inner directives unsupported, the guard can shrink to leading trivia. |
| 1587 | vNext | formatter/StructuralTransformPhase (field split) | field splitting | partly done | The issue's comment input (`int⏎/* c */ _x, _y`) is now refused: FMT-UNSUP-004 (comment inside a variable declaration) | S | yes | — | yes | Remains: the comment-free `private int⏎ _g, _h;` repeating the break, and its column (#953). |
| 1605 | vNext | formatter/pipeline entry points + code fixes | partial write-back | partly done | RH3004 fix no longer restores `///` (no `RestoreDocumentationCommentsAsync`; moved block keeps its lines via `MoveStatementsAsync`, `RH3004UseLambdaSyntaxCodeFixProvider.cs:125-134`) | L | yes | — | no | Remains: the layout-only node mode itself = the partial write-back design of #1140/#1774 (and #1684); design escalation, not an output question. |
| 1633 | vNext | formatter/LineBreakPhase + IndentationPhase (base list) | continuation columns | open | — | M | yes | — | yes | FMT-INDENT-016 still places only later base types; comment of 11-15 adds the comma-first base-list case (ties to #1739 comma rule). |
| 1634 | vNext | formatter/IndentationPhase | continuation columns | open | — | M | yes | — | yes | Comment-held lines in `<…>` first element and qualified names; FMT-INDENT-015/032 unchanged. |
| 1636 | vNext | formatter/LineBreakPhase + analyzer/RH5110 | extension block layout | open | — | M | yes | — | yes | FMT-BREAK-023 still lists no extension block (FMT-BREAK-037 already does); RH5110 analyzer has no extension case; derive owner set once with #1785. |
| 1639 | vNext | formatter/LineBreakPhase (GenericConstraintLineBreakRewriter) | unowned wrap joins | open | — | S | yes | — | yes | Blocker #1631 closed (#1632); FMT-BREAK-037 joins only type-parameter/colon gap. |
| 1640 | Maintenance | formatter/LineBreakPhase (ChainLineBreakRewriter) | one-owner refactor | open | — | S | no | — | no | `CloseGap`/`GetPendingToken` still present (`ChainLineBreakRewriter.cs:93,205`); same file as #1651. |
| 1641 | Maintenance | formatter/core utilities (TokenGapUtilities) | multi-line comment closing line | open | — | S | no | — | no | Wrong summary still at `TokenGapUtilities.cs:15-17`; text-only. |
| 1645 | vNext | formatter/LineBreakPhase | unowned wrap joins | open | — | S | yes | — | yes | Anonymous-object `NameEquals` named by no FMT-BREAK requirement; option B ~ #953. |
| 1646 | vNext | formatter/LineBreakPhase + UsingDirectiveOrderingPhase | unowned wrap joins | open | — | S | yes | — | yes | No line-break requirement names the using-alias `=`. |
| 1647 | vNext | formatter/IndentationPhase | continuation columns | open | — | S | yes | — | yes | FMT-INDENT-006 still names the collection expression only; related #1272. |
| 1648 | vNext | formatter/LineBreakPhase | unowned wrap joins | open | — | S | yes | — | yes | Lambda arrow join; related #1567 (lambda joins), #1515. |
| 1651 | Maintenance | formatter/LineBreakPhase (rewriters) | one-owner refactor | open | — | S | no | — | no | Private copies remain (`ChainLineBreakRewriter.EnsureLineBreak:291`, `HasLineBreak:108`). |
| 1652 | vNext | formatter/LineBreakPhase (lists) | unowned wrap joins | open | — | S | yes | — | yes | Needs a change doc only for B. |
| 1653 | vNext | formatter/LineBreakPhase (accessor lists) | unowned wrap joins | open | — | S | yes | — | yes | Confirm unwrapped baseline first, as the issue says. |
| 1654 | Maintenance | formatter/LineBreakPhase (assignment + interior join) | one-owner refactor | open | — | S | no | — | no | Both `NormalizeEqualsValuePlacement` and `ListElementInteriorJoinRewriter` still reach the gap. |
| 1658 | vNext | formatter/RegionFormattingPhase (new directive owner) | directive normalization | open | — | M | yes | — | yes | Needs a new owner for non-region directives incl. disabled branches; pair with #1579/#960. |
| 1659 | vNext | formatter/SwitchCaseBracePhase | switch braces | open | — | S | yes | — | yes | Filer recommends B (keep today's output); if the maintainer picks B, close without code. |
| 1672 | Maintenance | formatter/PrecheckPhase | internal refactor | open | — | S | no | — | no | `values.Find` per token still at `PrecheckPhase.cs:251`; issue says close if measurement shows no gain. |
| 1682 | Maintenance | analyzer/RH8307 + formatter/DocumentationCommentFormattingPhase | one-owner refactor | open | — | S | no | — | no | Two `TryGetFirstMeaningfulContentPosition` copies remain; related #1681. |
| 1685 | vNext | formatter/IndentationPhase (case labels) | continuation columns | open | — | S | yes | — | yes | FMT-INDENT-021 covers only a `when` that starts a line. |

## Closure candidates

- #1587 — partly done: the comment variant is refused by FMT-UNSUP-004; narrow the issue to the comment-free repeated break (or fold into #953 + a one-line field-split question).
- #1605 — partly done: the RH3004 motivating symptom is gone on main; the remaining layout-only mode is the partial write-back design of #1140/#1774, so close in favour of that design issue or re-scope to it.

## Overlaps

- #1579 + #960 (slice 1) + #1658 — directive-line normalization; one cleanup/directive run.
- #1571 + #1572 + #1641 (+ #1642) — code behind a multi-line comment's closing line; one spec/test run.
- #1576 + #1577 — FMT-PIPE-027 per-phase check coverage.
- #1640 + #1651 — both rework `ChainLineBreakRewriter`; one refactor run (with #1654 for the same phase).
- #1574 + #1583 + #1682 — move duplicated decisions into `Reihitsu.Core`.
- #1586 + #1587 + #1728 — field splitting guards/output; #1728 first.
- #1636 + #1785 — extension-block owner set (constraints and braces) derived once.
- #1585 + #1584 — one generated-code decision for analyzers, formatter, CLI.
- #1605 + #1140 remainder + #1774 + #1684 — partial write-back design.
- #1639, #1645, #1646, #1648, #1652, #1653 — unowned-wrap joins in the line-break phase; one batch of output questions.
- #1568, #1633, #1634, #1647, #1685 (+ #953, #1659) — columns of unowned continuation lines; #953's general answer may decide several; #1633 comma-first case ties to #1739.
