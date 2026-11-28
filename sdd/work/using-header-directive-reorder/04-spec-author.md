Role: spec author
Commit: f5d1270
Verdict: spec written

Follow-up round for the maintainer's answer 4 in `00-lead.md`: question 7 (change doc case 8, a conditional group that encloses the block), answer B. The prompt carried no intended fix or hoped-for verdict. The change doc stays `Status: Accepted`.

## Changed requirements

- **SHARED-USING-009** — changed.
  - The boundary is now a conditional directive (`#if`, `#elif`, `#else`, `#endif`) in the leading trivia of the first token of the block's first directive, whether its group ends in front of the block or encloses it. This replaces the boundary "disabled text in front", which round 03 used.
  - Disabled text in front always follows a conditional directive in the same leading trivia, so it is covered by the new boundary.
  - Every other directive in front keeps the fix: `#nullable`, `#pragma`, `#define`, `#region`. That is the other side of the qualifier (FC-002), shown by the existing `#nullable enable` example.
  - New title: *Blocks with preprocessor directives among their directives or a conditional directive in front of them are not reordered*.
  - The enclosing-group example (`#if !LEGACY` … `#endif`) is now negative: reported, no fix, formatter unchanged.
  - The `#if DEBUG` example's text now names the conditional directives.
  - *Why* now gives the maintainer's reason: any `#if` in front means no fix, whether or not another branch holds a using.
- **FMT-USING-004** — the intro of the round-03 example now says "a conditional group in front of it". The example's code is unchanged.
- **FMT-USING-012** — unchanged. It never mentioned conditional groups after round 03.
- **RH7201-004 … RH7207-004** — the no-fix sentence now says "or with a conditional directive (`#if`, `#elif`, `#else`, `#endif`) in front of its first directive". The fixed-block sentence now says "none of them a conditional directive".
- **RH7207-001** — the header sentence now says "none of them a conditional directive".
- **Change doc** `sdd/changes/2026-11/using-header-directive-reorder.md`:
  - *What* and *Why* now use the conditional-directive boundary.
  - Case 7's text is updated.
  - Case 8 is now "Before and after: reported, no code fix offered; the formatter keeps the block". Today's behaviour is confirmed with `scripts/apply-fix.sh RH7203` (`no-fix-offered`) and `scripts/format.sh` in round 03.
  - *Decisions* adds the case 8 answer and has a new section, *A conditional directive in front of the block keeps it unsafe*, in place of the disabled-text section.
  - *Alternatives* now lists reordering an enclosed block as rejected by the maintainer.

Rule pages are unchanged.

## Examples for the tests

- SHARED-USING-009: the `#if DEBUG` group in front and the `#if !LEGACY` enclosing group, both negative.
- FMT-USING-004: the conditional-group example, where only the shared line is split.
- FMT-USING-012: unchanged, and still the only failure of `scripts/test.sh --project architecture` ("FMT-USING-012: no test covers it").
- RH7201–RH7207 -004 and RH7207-001: unchanged examples.

## Output questions

_None._

## Internal choices

- The boundary is stated as the kind of directive in front of the block (conditional or not), not as disabled text plus `#if`. Disabled text in front always follows a conditional directive in the same trivia, so one check covers both answers.

## Follow-up candidates

_None._
