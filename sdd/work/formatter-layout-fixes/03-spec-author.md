Role: spec author
Commit: f944e1e
Verdict: spec written

This amendment covers the maintainer's answers Q1 (#1391) and Q2 (#1611) and lead decisions 5 and 6 (`00-lead.md`). The prompt carried no intended fix or hoped-for verdict beyond those recorded decisions.

## Changed requirements

- FMT-INDENT-013 — A line inside a list element aligns with the element — clarified (Q1, #1391): every base list counts as a list from FMT-INDENT-016 here, whatever the number of its types. That covers a class, struct, record, or interface base list and an enum's underlying type. A line inside the only type aligns with that type's first token, as a line inside a type of a two-type list does. FMT-INDENT-016's "two or more types" applies only to the later base types it aligns. The new examples cover both sides of the qualifier (FC-002): `class B : System. // c⏎IDisposable` and `enum E : System. // c⏎Int32` (the maintainer's example), plus a line inside the later type of a two-type list (the issue's input).
- FMT-RAW-001 — The closing marker starts under the opening quotes — clarified (Q2, #1611): the tab joins the list of whitespace characters that count as one column each. It counts as one column, not as a tab stop, and stays as written (FMT-TAB-003, FMT-RAW-002). New example with `→` = tab: four tabs in front of the content and closing lines become 12 spaces followed by the same four tabs, which is the RH5202 fix's output.
- FMT-RAW Non-goals — narrowed: only tab-width alignment (counting a tab as more than one column, or replacing it with spaces) stays out of scope, with thoenissen/Reihitsu#1432. Until then a tab counts as one column.
- FMT-TAB Non-goals (`tab-replacement.md`) — same thoenissen/Reihitsu#1432 sentence, aligned to say that alignment counts these tabs as one column (FMT-RAW-001) and that tab-width alignment is the planned extension. No requirement text changed.
- FMT-BREAK-015 — Wraps inside a list element are joined — clarified (decision 5): the `=` of a parameter's default value is not an operator wrap from the exclusion list. The gaps in front of it and behind it are wraps inside the element, and this rule joins them in bracketed and parenthesized parameter lists. FMT-BREAK-016 defers to it there.
- FMT-BREAK-016 — Assignments keep target, operator, and value together — clarified (decision 5): inside a parameter list it does not decide the gaps around a default `=`, because FMT-BREAK-015 owns them. The parameter default stays in the construct list because FMT-PIPE-014 names it in the assignment family, and the output is the same.
- Decision 6: no spec text was wrong. The RH5111-004 examples already keep `A = 1,`. The wrong claim was in `01-spec-author.md` (*#1149, Examples for the tests*). It is corrected there: the RH5111 fix of `enum E { A⏎    = 1, }` gives `A = 1,`, because the fix formats only the member and the trailing comma stays. Only whole-document formatting drops the comma (FMT-STRUCT-019).

No new requirement IDs, so `requirement-ids.txt` is unchanged. No rule page changed. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` gives 51 passed.

## Examples for the tests

- FMT-INDENT-013: the single-type base list, the enum underlying type, and the two-type later-type example. The skeptic's other base-list cells are interface, struct, and record base lists, and a line inside the first type of a two-type list.
- FMT-RAW-001: the tab example (`→` = U+0009 in the test source). Also run RH5202 over the result and confirm it reports nothing, and that a second pass changes nothing.
- FMT-BREAK-015/-016: no new example. The existing parameter-default line in FMT-BREAK-016's second example stays the test seed, now tagged FMT-BREAK-015.
- RH5111-004: the enum-member fix result is `A = 1,`.

## Output questions

_None._

## Internal choices

- **Change doc for Q2. The lead decides; my recommendation is none.** `sdd/README.md` says "Extending behavior into a non-goal is a behavior change and needs a change doc", and the tab case was a FMT-RAW non-goal. Read literally, that calls for a change doc and widens the lane to behavior change. I wrote none, for three reasons. The non-goal only narrows: tab-width alignment, the extension thoenissen/Reihitsu#1432 proposes, stays excluded. The one-column measure is what SHARED-FIX-007, SHARED-DIAG-010, and RH5202-003 already demand, because the formatter must not produce output RH5202 reports or undo the RH5202 fix. And the maintainer's answer is recorded where it takes effect, in the spec (`squad/README.md`, *What reaches the maintainer*). The alternative is a change doc under `sdd/changes/2026-11/` with Q2 as its decision, plus a lane widening to behavior change.
- **Change doc for Q1: none.** The answer picks one of two readings of existing text: FMT-INDENT-013 says "a list from FMT-INDENT-016", and FMT-INDENT-016 says "two or more types". It adds no behavior beyond that requirement's own wording. The alternative is the same change doc as above.
- Q1 is written into FMT-INDENT-013, not FMT-INDENT-016. FMT-INDENT-016 aligns later base types, and a single-type list has none. Only the inner-line alignment of -013 is in question. The alternative was to drop "with two or more types" from -016, which would muddle what -016 aligns.
- Decision 5 is stated in both requirements: -015 claims the gap, and -016 points to -015. That way neither text reads as owning the gap alone.
- The 01 handover is corrected in place, with a pointer to this file, so a test implementer who reads only 01 does not assert the comma-less result.

## Follow-up candidates

_None._ The candidates in `00-lead.md` stay as they are.

## Amendment

Lead decision 9 (`00-lead.md` at `b9e59f8`): the change doc is written, as `sdd/README.md` requires for extending behavior into a non-goal. File: `sdd/changes/2026-11/raw-string-tab-one-column.md`, `Status: Accepted` (lead-accepted; both visible output changes are maintainer-answered). It records Q2 (A), the FMT-RAW tab extension, as the change, and Q1 (A) as the decided reading of FMT-INDENT-013/-016 for a single-type base list. Each has input, before, and after in C#. The run's lane is now behavior change for all six issues. The *Internal choices* entries above that recommend no change doc are superseded.
