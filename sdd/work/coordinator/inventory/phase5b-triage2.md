# Phase 5b — triage proposal 2 (open issues without a milestone)

Snapshot: 2026-11-09, `origin/main` at `b673099` (#1674). 22 open issues without a milestone (#1645–#1673), none with comments. Nothing has been written to GitHub.

**No issue was reproduced.** Every row is assessed from the issue text plus a short look at `main` where noted.

Filed by: #1645–#1656 by P2 (PR #1644); #1658–#1659 by P3 (#1657); #1661 by P1 (#1660); #1664–#1668 by P12 (#1662); #1672–#1673 by the precheck run (#1671). #1663 and #1674 filed none of these.

## Milestones

| Group | Count | Recommended milestone | Other open milestones |
|---|---|---|---|
| Critical bug | 3 | `1.2.1` (1.2.0 released) | — |
| Minor bug | 2 | `1.2.1` or `1.2.2`, per issue | — |
| Extension | 12 | `vNext` | `1.4.0`, `1.5.0` |
| Maintenance | 5 | `Maintenance` | — |
| Close | 2 | none (close as duplicate) | — |

## Issues

| # | Title | Group | Reason | Labels to add | Milestone | Run |
|---|---|---|---|---|---|---|
| [#1666](https://github.com/thoenissen/Reihitsu/issues/1666) | RH2001: conditional modifier in front of the first modifier gives a field that does not compile in another build | **Critical bug** | The fix's output does not compile with `DEBUG` (CS0106/CS1004); confirmed on `main` that the guard skips the first modifier's leading trivia (`Skip(1)`), and the refusal is a cheap check. | — | 1.2.1 ⚠ | P11 |
| [#1667](https://github.com/thoenissen/Reihitsu/issues/1667) | Member-moving fixes: conditional layout attribute on another partial part | **Critical bug** | Reorders a sequential layout in another build (same class as #1595); rare shape, but the cheap refusal (check every part's header) is missing — `DeclarationHoldsConditionalDirective` reads one part on `main`. | — | 1.2.1 ⚠ | P11 |
| [#1665](https://github.com/thoenissen/Reihitsu/issues/1665) | RH7109/RH7103/RH7309: conditional group before the span that closes the type | **Critical bug** | Reorders another type's layout in another build, so critical by the letter, but the input is highly contrived (see doubts). | — | 1.2.1 ⚠ | P11 |
| [#1673](https://github.com/thoenissen/Reihitsu/issues/1673) | Code fixes: no action is registered when the fix cannot build or find its node (SHARED-FIX-005) | Minor bug | A no-op action does no damage; `main` confirms registration on `TryCreate… == false` / `GetFormatTarget is null` (pattern from #1671), but no input reaching it is shown yet. | — | 1.2.1? | P11 |
| [#1655](https://github.com/thoenissen/Reihitsu/issues/1655) | RH5111: a gap the formatter keeps because of a comment is reported | Minor bug | A report without a fix on a rare comment placement, a spec conflict (SHARED-DIAG-010 vs RH5111-005) with no damage. | — | 1.2.2? | Q1 |
| [#1659](https://github.com/thoenissen/Reihitsu/issues/1659) | `return`/`throw`-only section with multi-line label braces the switch (output question) | Extension | Changes formatter output; the issue itself recommends keeping today's output. | — | vNext | — |
| [#1658](https://github.com/thoenissen/Reihitsu/issues/1658) | Whitespace between `#` and non-region directive keywords removed (output question) | Extension | New formatter output for a gap no requirement owns. | — | vNext | — |
| [#1653](https://github.com/thoenissen/Reihitsu/issues/1653) | Interface indexer wrapped before `[` keeps accessor list on one line (output question) | Extension | New formatter output for a case no requirement decides. | — | vNext | — |
| [#1652](https://github.com/thoenissen/Reihitsu/issues/1652) | Compound assignment inside one argument: list returns to one line (output question) | Extension | New formatter output for a case no requirement decides. | — | vNext | — |
| [#1650](https://github.com/thoenissen/Reihitsu/issues/1650) | RH5106, RH5107 check indexer parameter lists (P11 question) | Extension | Two rules would report more (P11, the rules' own construct). | `documentation` | vNext | — |
| [#1649](https://github.com/thoenissen/Reihitsu/issues/1649) | RH5111 reports compound assignments below their target (P11 question) | Extension | RH5111 would report more (removes a stated non-goal). | `documentation` | vNext | — |
| [#1648](https://github.com/thoenissen/Reihitsu/issues/1648) | Lambda arrow alone on its line is joined (output question) | Extension | New formatter output for an unowned gap. | — | vNext | — |
| [#1647](https://github.com/thoenissen/Reihitsu/issues/1647) | Comment before a list pattern's `]` indented to the elements' column (output question) | Extension | Changes formatter output beyond what FMT-INDENT-006 names. | — | vNext | — |
| [#1646](https://github.com/thoenissen/Reihitsu/issues/1646) | `=` of a using alias below its name is joined (output question) | Extension | New formatter output for an unowned gap (today's column-0 continuation is ugly but unspecified). | — | vNext | — |
| [#1645](https://github.com/thoenissen/Reihitsu/issues/1645) | `=` of an anonymous-object member below its name is joined (output question) | Extension | New formatter output for an unowned gap. | — | vNext | — |
| [#1668](https://github.com/thoenissen/Reihitsu/issues/1668) | Member-moving fixes: refuse any conditional block and drop the excluded-text parse | Extension ⚑ | Fewer fixes offered is a visible change the maintainer decides; the maintainer already chose to keep the parse (see doubts). | `enhancement` | vNext | — |
| [#1672](https://github.com/thoenissen/Reihitsu/issues/1672) | PrecheckPhase: compute each value's first/last token once | Maintenance | Performance-only refactor with unchanged output (`values.Find` per token confirmed on `main`), to be measured first. | — | Maintenance | — |
| [#1661](https://github.com/thoenissen/Reihitsu/issues/1661) | SHARED-COMMENT-001: `/***` decided as the compiler parses it | Maintenance | Spec clarification; the text definition at `comment-placement.md:27` still admits `/***`. | — | Maintenance | — |
| [#1654](https://github.com/thoenissen/Reihitsu/issues/1654) | FMT-BREAK-015/016: one owner for the gap behind a parameter default's `=` | Maintenance | One-owner refactor with unchanged output. | — | Maintenance | — |
| [#1651](https://github.com/thoenissen/Reihitsu/issues/1651) | FMT-BREAK: line-break rewriters share one "line break in front of a moved token" check | Maintenance | Refactor to a shared helper with unchanged output. | — | Maintenance | — |
| [#1664](https://github.com/thoenissen/Reihitsu/issues/1664) | RH7102: a constant in a region is not compared with a field outside it | Close | Duplicate of #1339 (1.2.2, run Q2: RH7102 checks each top-level region on its own); the spec already says so and the analyzer lags. | — | — (close) | Q2 |
| [#1656](https://github.com/thoenissen/Reihitsu/issues/1656) | scripts: verify-text-only `--strict-docs` checks containing type's accessibility | Close | Duplicate of #1545 (Maintenance), same false FAIL on a `public` member of an `internal` type. | — | — (close) | — |

⚠ = critical bug, into 1.2.1. ⚑ = flagged for the maintainer. `agent-filed` stays on every issue; no label is removed.

## Duplicates, overlaps and doubts

- **#1664 = #1339.** #1339's instance (`#region Fields` / `#region Constants`) is #1664's second input; the first input (field outside every region) and the test that asserts only "no action" (`NoCodeFixWhenDirectivesAreInLeadingTrivia`) should be carried into #1339/Q2 before closing. If you prefer not to close, make it a Minor bug, 1.2.2, run Q2.
- **#1656 = #1545** (same script, same function `IsPubliclyVisible`, same false FAIL). Close #1656, or merge its "effective accessibility" wording into #1545.
- **#1665 is contrived** (an `#if` whose branch closes the type and opens another). Critical only by the letter, like #1593/#1595; downgrade to minor/1.2.2 is defensible. The guard must stay a cheap check (a conditional directive ahead of the span that does not wrap whole members, reusing the existing whole-unit helper); anything more argues against *Special cases do not grow the source*. Option to name (not decided here, an output question for the maintainer): declare an `#if` group with unbalanced braces/type boundaries an unsupported shape instead of growing the guard.
- **#1667, #1666** are rare `#if` shapes too, but each has a cheap syntactic refusal, so the missing refusal is the bug. #1666 confirmed on `main` (`HasConditionalDirectiveBetweenModifiers` uses `Skip(1)`).
- **P11 grows to 8 issues** with #1665–#1667 and #1673 (#1600 #1150 #1274 #1143 + 4). Alternative: a new run "other-build guards" right after P11 for #1665–#1667.
- **#1673** is related to #1150 (P11, SHARED-FIX-005 where the formatter makes no change) but distinct. Its premise is unconfirmed: if no input makes the build/find step fail, it has no caller reaching it and should be closed (*Supported inputs*); the run must find an input first, as the issue itself says.
- **#1655** overlaps #1387 (Q1: layout rules do not report a gap holding a conditional directive, RH5111 included) — same principle for comments. Its declarator instance `int x // c⏎= 1;` is now a comment inside a variable declaration outside its initializer, i.e. an unsupported shape since #1671 (FMT-UNSUP-004); only the enum-member instance remains a plain "formatter keeps it" case. Check SHARED-DIAG-010's wording against refused files in Q1.
- **#1668** records an option the maintainer already declined for now; it could also be closed as decided. It is a "special cases" simplification that costs visible fixes, so it stays the maintainer's call.
- **#1659**: the filer recommends B (today's output). If you pick B, close it instead of keeping it in vNext.
- Related, not duplicates: #1648 ~ #1567 (lambda joins); #1649 ~ #1155 (RH5111 nested assignments); #1650 ~ #1158, #1349 (indexer/parameter-list scope); #1647 ~ #1272 (list patterns); #1645/#1646 ~ #953 (continuation indent); #1653 ~ #1519 (accessor-list owner); #1658 ~ #1444 (`# region` reporting); #1651 ~ #1640 (ChainLineBreakRewriter).
- **Already resolved on `main`:** none of the checked ones (#1666, #1667, #1673, #1672, #1661, #1656, #1664 still match `main`).
- **Below C# 12:** none matters only below C# 12.

## Answer format

> `ok` — apply everything as proposed.
> `ok, minor bugs → 1.2.2` — change one group's milestone.
> `ok, #1665 → 1.2.2 minor, #1664 Minor bug` — override single issues.
