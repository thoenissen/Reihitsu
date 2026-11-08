Role: spec author
Commit: 502b4ed
Verdict: change doc written

Continuation of `01-spec-author.md` after the maintainer's answers (OQ1 = A, OQ2 = A) and the lead's Decisions 2–5 in `00-lead.md`. Lane: behavior change (Decision 4).

## #1385

### Changed requirements
- FMT-BLANK-006 — A directive directly before a statement or a switch section suppresses its blank line — changed (OQ1 = A): the FMT-BLANK-004 section blank line is not inserted when a directive directly precedes the section; "directly preceded" is defined (last non-whitespace trivia is a directive; a whole conditional group ending in `#endif` counts; a comment behind the directive does not). Heading renamed, Why extended (RH5011-002), two section examples added. Uses gains RH5011-002.
- FMT-BLANK-004 — `break` is separated on both sides — changed: the section sentence points at the FMT-BLANK-006 exemption.
- FMT-BLANK-016 — clarified (Decision 3): the coverage list now names the blank line in front of a `break` statement and the switch-section gap, and refers to FMT-BLANK-006 for statements and sections.
- Change doc: `sdd/changes/2026-11/directive-before-switch-section.md` (Status: Draft), with both OQ1 inputs (`#pragma` and the disabled `#if NEVER … #endif` group) as visible output changes and answer 1A in `Decisions`.

### Examples for the tests
- FMT-BLANK-006: the unchanged switch (`#pragma` and `#if NEVER … #endif` directly in front of sections) and the comment case (`#pragma`, blank line, `// two`, `case 2:`).
- FMT-BLANK-016: as in `01-spec-author.md`; for the `break`-before path use skeptic fixture 08 (`a++;⏎#pragma …⏎// why⏎break;` in a loop → blank line between `#pragma` and `// why`).
- `InsertsBlankLineAboveDirectiveOfSwitchSectionAfterBreakSection` now expects the input unchanged (no blank line).

## #1398
No change since `01-spec-author.md`.

## #1239

### Changed requirements
- FMT-REGION-007 — A `#region` line is `#region`, one space, and the description — clarified (OQ2 = A): whitespace between `#` and `region` is removed, a tab (four spaces after FMT-TAB-002) included; Why extended; examples added for the described pair (`# region fields` / `#  endregion` → `#region Fields` / `#endregion // Fields`), the tab case, and a disabled branch.
- FMT-REGION-002 — clarified: the `#endregion` of a described region has no whitespace after `#`.
- FMT-REGION-004 — clarified: the same for an undescribed `#region` and its `#endregion`, bare or with text; example `# region` / `#  endregion  note` → `#region` / `#endregion note`.
- Spec intro names the start of the line. No change doc (Decision 4: a reading of FMT-REGION-007).

### Examples for the tests
- FMT-REGION-007 (three new examples), FMT-REGION-004 (the new gap example). All are whole-document formatting; node-level entry points skip the region phase, so `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncReplacesTabBetweenHashAndRegionKeyword` stays valid.

### Non-goals
- The RH7301 code fix and its output (Decision 2): any change stays inside the formatter's region phase; `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`'s behavior for its other caller is unchanged.
- Directives other than `#region`/`#endregion` (follow-up candidate already in `00-lead.md`).
- RH7302 (#1444).

## Output questions
_None._

## Internal choices
- One change doc for the FMT-BLANK-006 widening only; the FMT-BLANK-016 and region changes are clarifications (Decision 4).
- FMT-BLANK-006 defines "directly preceded" in its normative text rather than in a new requirement; alternative: a new requirement for the section gap.
- The gap-after-`#` rule is stated in each of FMT-REGION-002/-004/-007, with the examples mostly under -007; alternative: one new requirement for the start of every region line.

## Follow-up candidates
_None_ beyond those in `00-lead.md`.
