# Phase 5 re-inventory: open issues without a milestone (triage sections 1–3)

Collected 2026-11-05 from the public REST API: 52 open issues without a milestone (#1505–#1588, no pull requests among them). `origin/main` = `a59a003` (#1581, merged 2026-11-05 20:19 UTC). Triage #1503 closed 2026-11-05 21:39 UTC. Nothing was written to GitHub.

**Timing.** Every issue was filed during the formatter rebuild (umbrella #1482). #1505–#1579 were filed 2026-10-30 to 2026-11-03 as follow-ups of rebuild PRs #1504–#1578; the timeline shows the PR that filed each one. #1582–#1588 were filed 2026-11-05 19:40 UTC, before #1581 merged, as follow-ups of #1581. None was filed after the rebuild. No PR has merged after #1581, so only a *later rebuild PR* can have resolved an issue. Those cases are marked in the table.

**Method.** I read each issue's body and comments, its cross-references, and the cited code and spec on `origin/main` (by `git grep`/`git show`). I reproduced nothing myself. The maintainer's triage comments of 2026-11-05 measured #1515, #1549, #1562, #1566 and #1587 on `a59a003`.

## 1. Milestones

Open milestones (verified): `1.2.0` (current release, lowest version number), `1.2.1`, `1.2.2`, `1.3.0`, `1.4.0`, `1.5.0`, `vNext`, `Maintenance`.

| Group | Count | Recommended milestone | In question |
|---|---|---|---|
| Critical bug | 1 | `1.2.0` | — |
| Minor bug | 5 | `1.2.1?` (4), `1.2.2?` (#1543) | `1.2.0`, `1.2.1`, `1.2.2` |
| Extension | 22 | `vNext` | `1.3.0` |
| Maintenance | 24 | `Maintenance` | — |

Why these minor-bug milestones: #1511, #1529, #1561 and #1588 are fix-convergence and fix-equals-formatter defects. Their siblings #1274 (RH5308 converges) and #1141 (RH6023/RH6024 P9) are already in `1.2.1`. #1543 first needs a reading (A or B), and its shapes are rare, so I suggest `1.2.2?`.

"Decide" issues: I read these the way the repository already uses them (#1472, #1474 under `Maintenance`): an issue whose deliverable is an internal decision, about ownership or a spec reading, with no user-visible output at stake. They are grouped under Maintenance and listed in §3. Output questions inside Extension or bug issues keep their group and are listed separately in §4.

## 2. Issues

Columns added beyond the triage format: **Spec ID** (bugs: the requirement the defect violates; verified in `sdd/specs/requirement-ids.txt` and the spec text on `origin/main`), **State after rebuild**, and **Overlaps**.

### Critical bug

| # | Title | Group | Reason | Labels to add | Milestone | Spec ID | State after rebuild | Overlaps |
|---|---|---|---|---|---|---|---|---|
| [#1566](https://github.com/thoenissen/Reihitsu/issues/1566) | Formatter: anonymous method in a hole of a regular interpolated string gets line breaks that do not compile below C# 11 | Critical bug | The output does not compile (CS8967) in a C# ≤10 project. The maintainer reproduced it on `a59a003` with `--lang-version 10`. Narrow input, but the output is non-compiling. | — | `1.2.0` | FMT-PIPE-008 (version-dependent rules follow the language version; its Why: "must not produce syntax the project's compiler rejects"), FMT-PIPE-003 | Filed during rebuild (#1565). Open: confirmed on `a59a003`. #1503 triage did not treat it as an exception (O-6), but the triage rules here classify non-compiling output as critical. | #1568 (anonymous-method braces in holes), same construct |

### Minor bugs

| # | Title | Group | Reason | Labels to add | Milestone | Spec ID | State after rebuild | Overlaps |
|---|---|---|---|---|---|---|---|---|
| [#1511](https://github.com/thoenissen/Reihitsu/issues/1511) | RH5204: one application re-indents a first type behind a leading doc comment or directive | Minor bug | The fix makes no progress on a misindented first type. No code damage. | — | `1.2.1?` | SHARED-FIX-001, RH5204-006 | Filed 10-31 (#1510). Later PRs changed the node-level entry points (#1520, #1580), so this is **possibly resolved and needs reproduction**. | #1561 (RH5204 no-progress, different input) |
| [#1529](https://github.com/thoenissen/Reihitsu/issues/1529) | RH5111, RH6023: assignment inside a conditional access with a wrapped member is laid out differently by fix and formatter | Minor bug | The fix output differs from whole-document formatting, so formatting the fix's result moves `.C`. Layout only. It needs a pick of A, B or C. | `bug` | `1.2.1?` | SHARED-FIX-007 | Filed 10-31 (#1520). Open; not re-measured since. | #1141 (`1.2.1`, RH6023/RH6024 P9: makes the RH6023 half moot) |
| [#1543](https://github.com/thoenissen/Reihitsu/issues/1543) | FMT-BLANK: no blank line between an own-line comment and every brace the formatter places | Minor bug | RH5024/RH5025 (and RH5026 for the `else`/`catch` shapes from #1392) report a blank line the formatter keeps on purpose. The analyzer and formatter disagree; no damage. | — | `1.2.2?` | SHARED-DIAG-010. The formatter follows FMT-BLANK-020, which explicitly exempts these braces, so the formatter is not at fault as written. The defect is the analyzers reporting a kept shape, or else FMT-BLANK-020's exemption list (reading A). | Filed 11-01 (#1541). Open. A 11-05 comment widened it with #1392's chained-keyword shapes. | #1392 (closed, folded in), #1562 |
| [#1561](https://github.com/thoenissen/Reihitsu/issues/1561) | RH5204: the fix makes progress on a multi-line doc comment in front of a switch label | Minor bug | RH5204 reports formatter output and its fix is a no-op. Rare input. | `formatter` | `1.2.1?` | SHARED-FIX-005 and SHARED-DIAG-010. The issue cites SHARED-FIX-002 (Fix All), which does not fit. | Filed 11-02 (#1558). #1570 later moved line breaks before switch braces, so this is **possibly resolved and needs reproduction**. | #1511, #1533, #1564 (`/** */` placement) |
| [#1588](https://github.com/thoenissen/Reihitsu/issues/1588) | Code fixes: a fix whose whole edit is a node-level formatting call is offered where the formatter refuses the document | Minor bug | A no-op fix is offered in a document with a syntax error. No damage. | — | `1.2.1?` | SHARED-FIX-005 (with FMT-PIPE-005; precedent RH7101-001) | Filed 11-05 (#1581). Open: on `origin/main` only RH5105, RH5106 and RH7101 use `SyntaxTreeErrorVerdictCache`. | — |

### Extensions

| # | Title | Group | Reason | Labels to add | Milestone | State after rebuild | Overlaps |
|---|---|---|---|---|---|---|---|
| [#1515](https://github.com/thoenissen/Reihitsu/issues/1515) | Specified layout for comments at the arrow of an expression-bodied member | Extension | Specifies a new formatter output for comments at `=>`. | — | `vNext` | Open on `a59a003` (maintainer comment). The `PhaseReversalExceptionList` part of done-when is gone with #1575. | **Near-duplicate** of #1439 (`vNext`) and #963 part 1 (`1.2.1`, bug). The maintainer asked to settle them together, with #1539. |
| [#1521](https://github.com/thoenissen/Reihitsu/issues/1521) | FMT-PIPE-010: node-in-document entry point writes back what phases put before a target not starting its line | Extension | Changes `FormatNodeInDocumentAsync` output (change doc). It is user-visible only if a code fix passes such a target, which is unverified; if none does, it is Maintenance. | — | `vNext` | Filed 10-31 (#1520). #1580 (context entry point lays out from the first-line column) touched the same area, so this is **possibly partly resolved and needs reproduction**. | #1474 (`Maintenance`, blank lines before a target that starts its line) |
| [#1533](https://github.com/thoenissen/Reihitsu/issues/1533) | FMT-DOC: delimited doc comment behind `{`, a label colon, or an earlier comment is moved like `///` | Extension | Formatter output change (output question A/B/C). | — | `vNext` | The switch-label positions were settled by #1558 (option C there). Open for `{` and behind an earlier comment. | #1563, #1564, #1561 |
| [#1534](https://github.com/thoenissen/Reihitsu/issues/1534) | FMT-BREAK-018: doc comment behind `?`/`:` leaves the operator starting its line… | Extension | Behavior-change lane: output question on comment placement. It also has a minor-bug side: RH5308 reports formatter output with a no-progress fix (FMT-BREAK-018, SHARED-DIAG-010). | `analyzer` | `vNext` | Filed 10-31. The output samples predate #1548 and #1570, so this is **possibly changed and needs reproduction**. | #1274 (`1.2.1`, RH5308 converges), #1273, #1147 |
| [#1537](https://github.com/thoenissen/Reihitsu/issues/1537) | FMT-STRUCT-003, FMT-SWITCH-010: brace a body or section whose trivia carries a movable directive | Extension | The formatter would brace more bodies (behavior change). | — | `vNext` | Open | #1538 (Why text of the same guard); #1337 (directive guards) |
| [#1540](https://github.com/thoenissen/Reihitsu/issues/1540) | FMT-SWITCH-007: comment behind the last label on the first statement's line when braced | Extension | Output question. Option A is no change (then it becomes a spec clarification and Maintenance). | — | `vNext` | Open | — |
| [#1542](https://github.com/thoenissen/Reihitsu/issues/1542) | FMT-BREAK-022, FMT-INDENT: content of a parenthesized/positional pattern indented inside its parentheses | Extension | New formatter layout (output question). | — | `vNext` | Open | #1442 item 18 (`vNext`, parenthesized `case` pattern, FMT-BREAK-022); #953 (continuation lines) |
| [#1546](https://github.com/thoenissen/Reihitsu/issues/1546) | New rule: a comment must not share a line with code | Extension | New analyzer rule. | — | `vNext` | Open | **Strong overlap** with #1160 (`vNext`, "a comment does not touch code"); consider merging |
| [#1549](https://github.com/thoenissen/Reihitsu/issues/1549) | FMT-BREAK-027: wrap inside a declaration's attribute list is joined; lambda stays on its attribute list's line | Extension | Formatter output change (output question). | — | `vNext` | The colon-spacing part was decided by #1548. The wrap and lambda parts are unchanged on `a59a003` (maintainer comment). | #953; #1555, #1557 (attribute lists) |
| [#1550](https://github.com/thoenissen/Reihitsu/issues/1550) | FMT-SPACE-024: spacing of `ref`/`ref readonly` before a tuple type; wrapped `ref` joined | Extension | Formatter output change. | — | `vNext` | Open | — |
| [#1552](https://github.com/thoenissen/Reihitsu/issues/1552) | FMT-BREAK-024: accessor attribute on a one-line auto-property with a multi-line initializer keeps the list on its line | Extension | Behavior change. The output arguably contradicts FMT-BREAK-024's wording; reading A would make it a minor bug. | — | `vNext` | Open | #1442 item 15 (accessor lists) |
| [#1554](https://github.com/thoenissen/Reihitsu/issues/1554) | FMT-BREAK: no blank line inside a parameter list before a `///` in a parameter's attribute group | Extension | Formatter output change on a rare, CS1587-warned input. | — | `vNext` | Open | — |
| [#1556](https://github.com/thoenissen/Reihitsu/issues/1556) | FMT-SPACE: space behind a comment before a parameter's attribute list is kept | Extension | **Reported as a bug, but it is not one.** FMT-SPACE-013 applies the `(`→`[` no-space decision to the whitespace behind a comment the `(` carries, so `(/* c */[A]` is the specified output. Keeping the space is a behavior change, as the issue itself suspects. | `enhancement` (the `bug` label stays) | `vNext` | Open | — |
| [#1559](https://github.com/thoenissen/Reihitsu/issues/1559) | FMT-SPACE: one space between a switch label's colon and an empty statement | Extension | Formatter output change. | — | `vNext` | Open | #1560 (`case 1: ;`) |
| [#1560](https://github.com/thoenissen/Reihitsu/issues/1560) | New rule: a switch label's statement and a second label do not share the label's line | Extension | New analyzer rule (optional, P11). | — | `vNext` | Open | #1559 |
| [#1567](https://github.com/thoenissen/Reihitsu/issues/1567) | Parenthesized lambda behind a modifier/return type on the next line is joined | Extension | Formatter output change (output question). | — | `vNext` | Open | #953 (reading B) |
| [#1568](https://github.com/thoenissen/Reihitsu/issues/1568) | Anonymous method's block aligns under its first token outside interpolation holes too | Extension | Formatter output change. | — | `vNext` | Open (#1565 did the in-hole half) | #1566 |
| [#1579](https://github.com/thoenissen/Reihitsu/issues/1579) | Trailing whitespace behind the message text of `#warning`, `#error` … is removed | Extension | Formatter output change. | — | `vNext` | Open | **Near-duplicate** of #960 (`vNext`, trailing whitespace at a directive line); consider merging |
| [#1584](https://github.com/thoenissen/Reihitsu/issues/1584) | Analyzers: do not report code generated by external tools | Extension | Analyzers report less (scope open: A/B/C). | `documentation` | `vNext` | Open | #1585; #976 |
| [#1585](https://github.com/thoenissen/Reihitsu/issues/1585) | FMT-PIPE-006: formatter's generated-code detection agrees with the analyzers' | Extension | Formatter and CLI skip a different set of files (A/B). | `cli` | `vNext` | Open. Note that FMT-PIPE-006 on `origin/main` states the case-insensitive match explicitly, so reading A changes a spec, not a bug. | #1584 |
| [#1586](https://github.com/thoenissen/Reihitsu/issues/1586) | Field declaration with several declarators inside a `#region` is split | Extension | The formatter and the RH7101 fix would split more fields. | `analyzer` | `vNext` | Open | #1337 (`1.2.2`, declarator holding a directive), #1336 (`1.2.0`), #1587 |
| [#1587](https://github.com/thoenissen/Reihitsu/issues/1587) | Split field does not repeat the line break written behind the type | Extension | Formatter and RH7101 fix output change. | `analyzer` | `vNext` | Open on `a59a003` (maintainer comment) | #1586; #953 (column) |

### Maintenance

| # | Title | Group | Reason | Labels to add | Milestone | State after rebuild | Overlaps |
|---|---|---|---|---|---|---|---|
| [#1505](https://github.com/thoenissen/Reihitsu/issues/1505) | Formatter.Test: every test helper parses input through one rule | Maintenance | Test helpers only. | `maintenance`, `formatter` | `Maintenance` | **Partly resolved** by #1513: `ObservedPipeline.Run` now takes parse options. `FormatterPhaseTestsBase` still calls `ParseText(input)` without them, and neither helper goes through `SecondPassInput.Create`. | — |
| [#1507](https://github.com/thoenissen/Reihitsu/issues/1507) | ArchitectureTests: every guard name in the phase-ownership table resolves | Maintenance | Architecture test. | `maintenance` | `Maintenance` | The stale names `CanRewrite` and `IsAfterSourceOnSameLine` are gone from the table (#1506). The test still checks only phase order, so the issue stands. | #1544 |
| [#1512](https://github.com/thoenissen/Reihitsu/issues/1512) | Analyzers: remaining line-start/terminator checks use the shared facts | Maintenance | Refactor. Any diagnostic change goes through a change doc. **Decide** | `maintenance` | `Maintenance` | Open: `AttributeTargetUtilities.StartsLine` is still private | — |
| [#1516](https://github.com/thoenissen/Reihitsu/issues/1516) | FMT-PIPE: no phase output carries elastic trivia | Maintenance | Internal invariant; outputs are kept. It carries the `enhancement` label, but no user-visible change. | `maintenance` | `Maintenance` | Open (no elastic requirement in `pipeline.md`) | — |
| [#1517](https://github.com/thoenissen/Reihitsu/issues/1517) | FMT-BREAK: only the cleanup phase removes whitespace before a line break | Maintenance | Single owner; final output unchanged. | `maintenance` | `Maintenance` | Open (`CleanupTrailingWhitespaceBeforeToken` still exists) | #1583 (same line-break-phase trim) |
| [#1518](https://github.com/thoenissen/Reihitsu/issues/1518) | FMT-SWITCH: inserted switch-section braces write no layout of their own | Maintenance | Single owner; outputs kept. **Decide** | `maintenance` | `Maintenance` | **Narrowed** by #1558 and #1570 (phase order changed; the shapes are fixed by a narrow check in `AddBraces`). The structural part is open. | — |
| [#1519](https://github.com/thoenissen/Reihitsu/issues/1519) | FMT-BREAK: property and indexer accessor-list braces have one owner | Maintenance | Single owner; outputs kept. | `maintenance` | `Maintenance` | Open (`DeclarationBraceLineBreakRewriter` and `PropertyLayoutLineBreakRewriter` both still exist) | — |
| [#1538](https://github.com/thoenissen/Reihitsu/issues/1538) | FMT-STRUCT-003: Why text says bracing rebuilds trivia | Maintenance | Spec text only. | — | `Maintenance` | Open (the stale sentence is still at `structural-transforms.md:113`) | #1537 (if it lands, rewrite the Why there) |
| [#1539](https://github.com/thoenissen/Reihitsu/issues/1539) | FMT-PIPE-013: expression-body conversion carries trivia through the shared carrier | Maintenance | Ownership refactor. **Decide** | — | `Maintenance` | Open | #1515, #1439 |
| [#1544](https://github.com/thoenissen/Reihitsu/issues/1544) | AGENTS.md: phase-ownership table names the blank-line policy and the keep rule | Maintenance | Docs. | — | `Maintenance` | **Partly resolved**: the `LineBreakPhase` row now says it "keeps every blank line". The `BlankLinePhase` row still lacks `BraceBlankLinePolicy`. | #1507 |
| [#1545](https://github.com/thoenissen/Reihitsu/issues/1545) | scripts: `verify-text-only.sh --strict-docs` counts only externally reachable docs | Maintenance | Tooling. | — | `Maintenance` | Open | — |
| [#1553](https://github.com/thoenissen/Reihitsu/issues/1553) | FMT-SPACE: same-line gap held as the right token's leading trivia is normalized | Maintenance | Ownership. No current reproducer (the one instance was fixed in #1551). **Decide** | — | `Maintenance` | Open (the spacing rewriter normalizes leading trivia only behind a comment) | — |
| [#1555](https://github.com/thoenissen/Reihitsu/issues/1555) | SHARED-ATTR-017: code-fix attribute-list split and formatter split are one implementation | Maintenance | Single implementation. The claimed two-pass code-fix defect is unverified; if it reproduces, it is a minor bug (SHARED-FIX-001). **Decide** | — | `Maintenance` | Open | #1142 (`1.2.1`, attribute-list split fixes) |
| [#1557](https://github.com/thoenissen/Reihitsu/issues/1557) | FMT-BREAK-027: `param:`/`typevar:` list on another owner is specified | Maintenance | Spec clarification plus test. | — | `Maintenance` | Open (FMT-BREAK-027 still says "unspecified") | #1549 |
| [#1562](https://github.com/thoenissen/Reihitsu/issues/1562) | RH5020 and the formatter: comment before a section's `break` | Maintenance | **Resolved on `a59a003`**: the formatter writes the blank line (option A, via #1496/#1498). Only the regression test is missing. It carries the `bug` label, which stays. | `maintenance` | `Maintenance` | **Resolved; needs only the regression test** | #1543 |
| [#1563](https://github.com/thoenissen/Reihitsu/issues/1563) | FMT-BREAK-008: spec states where a comment before a section's block body goes | Maintenance | Spec clarification plus test. **Decide** | `documentation` | `Maintenance` | Open | #1533, #1564 |
| [#1564](https://github.com/thoenissen/Reihitsu/issues/1564) | FMT-BREAK-033: `/** */` placement depends on the documentation mode | Maintenance | Spec clarification plus test. | `documentation` | `Maintenance` | Open | #1533, #1563 |
| [#1571](https://github.com/thoenissen/Reihitsu/issues/1571) | FMT-BLANK: blank-line phase writes no line-ending break except behind a multi-line comment | Maintenance | Ownership and test; output unchanged. | — | `Maintenance` | Open (`InsertBlankLineBeforeToken` still writes it) | #1572 |
| [#1572](https://github.com/thoenissen/Reihitsu/issues/1572) | SHARED-COMMENT-012: code behind a multi-line comment's closing line keeps its line | Maintenance | Spec clarification; no output change intended. | `documentation` | `Maintenance` | Open (spec in `sdd/specs/shared/comment-placement.md`) | #1571 |
| [#1574](https://github.com/thoenissen/Reihitsu/issues/1574) | SHARED-BLANK-006: blank line after a region directive is decided once | Maintenance | Single owner (Core); behavior kept. | `analyzer` | `Maintenance` | Open (the formatter keeps its own `EnsureBlankLineAfterRegionDirectives`) | — |
| [#1576](https://github.com/thoenissen/Reihitsu/issues/1576) | FMT-PIPE-027: self-hosting scans check no single phase changes the sources | Maintenance | Test coverage. | — | `Maintenance` | Open (`Reihitsu.SelfHosting.Test` has no `PhaseStabilityCheck`) | #1577 (same requirement; one change) |
| [#1577](https://github.com/thoenissen/Reihitsu/issues/1577) | Formatter.Test: idempotency tests outside FormatterTestsBase check per-phase stability | Maintenance | Test coverage. | — | `Maintenance` | Open (none of the seven classes uses the check) | #1576 |
| [#1582](https://github.com/thoenissen/Reihitsu/issues/1582) | RH5114: code fix uses shared `IsDocumentationCommentTrivia` | Maintenance | Refactor, no behavior change. The issue names the wrong path (the file is under `Rules/Layout/`), and its SHARED-FIX-003 principle does not fit. | — | `Maintenance` | Open (the inline pair is at line 93) | — |
| [#1583](https://github.com/thoenissen/Reihitsu/issues/1583) | Reihitsu.Core: one trailing-whitespace trim, `TriviaListUtilities.RemoveTrailingWhitespace` | Maintenance | Refactor. The premise is inaccurate: the owner is `internal` in `Reihitsu.Formatter` (`Pipeline/Core/Utilities`), not `Reihitsu.Core`, so the CodeFixes copy may need it moved first. | — | `Maintenance` | Open (all three copies are present) | #1517 |

## 3. Needs a maintainer decision (Maintenance "decide" issues)

- **#1512** — For each analyzer-side copy (the `AttributeTargetUtilities.StartsLine` copy and the RH5202, RH5401, RH5603, RH8402 and copyright helpers' terminator sets), does the shared fact give the intended answer, or is the difference deliberate and spec-recorded?
- **#1518** — Should the switch-case brace phase place inserted braces through `BracePlacer`, or should FMT-SWITCH state why a second brace placement is needed?
- **#1539** — Should `ExpressionBodyToBlockConverter` carry its trivia through `BlockWrapTriviaUtilities`, or stay its own carrier with a recorded reason?
- **#1553** — Does the spacing phase own same-line whitespace held in the right token's leading trivia, or is FMT-PIPE-021 narrowed?
- **#1555** — Should the RH55xx attribute-split code fix reuse the formatter's split, or is the difference intended and recorded?
- **#1563** — Should FMT-BREAK-008 state today's output (the comment stays on the label line, `{` moves), or should the comment move with `{`?

## 4. Output questions pending in other groups (for reference)

These need an A/B/C pick before implementation but stay in their user-facing group: #1515, #1521 (change doc), #1529, #1533, #1534, #1537, #1540, #1542, #1543, #1546 (scope), #1549, #1550, #1552, #1554, #1556, #1559, #1567, #1568, #1584, #1585, #1586, #1587.

## 5. Possibly resolved or changed — needs reproduction

- **#1562** — resolved on `a59a003` (maintainer comment). Only the regression test is missing.
- **#1505** — partly resolved by #1513.
- **#1544** — partly resolved (the `LineBreakPhase` half).
- **#1518** — narrowed by #1558 and #1570. The structural part is open.
- **#1521** — possibly affected by #1580.
- **#1511**, **#1561** — RH5204 no-progress, reported before #1570 and #1580. Re-run `scripts/apply-fix.sh RH5204`.
- **#1534** — output samples predate #1548 and #1570.

## 6. Duplicates and overlaps

- With older open issues:
  - #1515 ≈ #1439 / #963 part 1
  - #1546 ≈ #1160
  - #1579 ≈ #960
  - #1529 ↔ #1141
  - #1534 ↔ #1274 / #1273
  - #1542 ↔ #1442 item 18
  - #1552 ↔ #1442 item 15
  - #1586 ↔ #1337 / #1336
  - #1555 ↔ #1142
  - #1521 ↔ #1474
  - #1584 ↔ #976
  - #1542, #1549, #1567 and #1587 ↔ #953 (continuation lines)
- Within the set:
  - #1576 + #1577
  - #1584 + #1585
  - #1586 + #1587
  - #1559 + #1560
  - #1533 + #1563 + #1564 (+ #1561)
  - #1537 + #1538
  - #1515 + #1539
  - #1507 + #1544
  - #1517 + #1583
  - #1571 + #1572
  - #1566 + #1568
  - #1511 + #1561
  - #1549 + #1555 + #1557
  - #1543 + #1562

## 7. Answer format

> `ok` — apply everything as proposed.
> `ok, minor bugs → 1.2.2` — change one group's milestone.
> `ok, #1566 → 1.2.1, #1556 Minor bug` — override single issues.
