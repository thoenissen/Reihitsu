Role: spec author
Commit: 177478b
Verdict: spec written

This round applies the maintainer's batch-1 answers recorded in `00-lead.md`: 1A, 2A, 3A, 4A with the change to two rules (RH5115 for `[` and `?[`, RH5116 for `!`), and 5A. It also notes decision #1, which moves the predicate to Core.

Prompt note: the prompt carried no intended fix or verdict beyond the recorded answers. Everything below follows `00-lead.md`, `01-spec-author.md`, and `02-skeptic.md`.

## Files

- `sdd/changes/2026-11/rh5115-rh5116-index-and-null-forgiving-line.md`: the change doc. It was renamed from `rh5115-index-and-null-forgiving-line.md`, since it now covers two rules. Status is `Accepted`, and the answers are under *Decisions → The maintainer's answers*. Visible change 2 gained the 5A outputs, and change 6 now names two pages.
- `sdd/specs/analyzer/RH5115.md`: narrowed to the index access. The title is now "Index access must stay on the line of its expression". Code owners are `RH5115IndexAccessMustStayOnExpressionLineAnalyzer` and `RH5115IndexAccessMustStayOnExpressionLineCodeFixProvider`.
- `sdd/specs/analyzer/RH5116.md`: new. Title "Null-forgiving operator must stay on the line of its expression". Code owners are `RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer` and `RH5116NullForgivingOperatorMustStayOnExpressionLineCodeFixProvider`.
- `documentation/rules/RH5115.md`: retitled. The `!` is removed from the description, the reasoning, and the fix advice. The example is unchanged.
- `documentation/rules/RH5116.md`: new page. Violation `return value⏎    !;`, correction `return value!;`. It uses a non-nullable `string` parameter, so the example needs no `#nullable enable`.
- `sdd/specs/README.md`: the RH5115 row is updated and an RH5116 row is added. `sdd/specs/requirement-ids.txt` gains RH5116-001 to RH5116-006.
- `sdd/specs/formatter/line-breaks.md` FMT-BREAK-040 Why, and `sdd/specs/shared/fluent-chain-layout.md` (introduction and non-goal): both now name RH5115 and RH5116.

## Changed requirements

- RH5115-001: Index access below its expression is reported. Changed: `!` removed, `?.` added to the negative example.
- RH5115-002: Comment or directive in the gap does not prevent the report. Changed: `!` removed.
- RH5115-003: Diagnostic on the access's first token. Changed: `!` removed.
- RH5115-004: Outside a chain with links, the fix joins the access. Changed: added the 5A sentence (nothing else changes, an aligned later line keeps its column) and example `c`.
- RH5115-005: In a chain with links, the fix lays out the chain. Wording only.
- RH5115-006: Comment or directive withholds the fix. Wording only.
- RH5116-001 to RH5116-006: added. They mirror RH5115-001 to RH5115-006 for `!`. RH5116-003 states that `s⏎!⏎?[0]` gives one diagnostic per rule, with the `?` going to RH5115.
- FMT-BREAK-040: Why reworded. Clarified, behavior unchanged.

Both specs and the change doc record the answers as agreed: Warning, enabled by default, Fix All, nothing reported in a file with a syntax error (SHARED-JOIN-002), a report without a fix for a comment or directive in the gap, and the chain layout in a chain with links.

## Decision #1 (the predicate moves to Core)

The specs name it by type, never by path. The code owner of the predicate stays `AttachedPartGapUtilities` under FMT-BREAK (`sdd/specs/formatter/line-breaks.md` header and `sdd/specs/README.md`). The architecture test looks for the type in any production project, so a move to `Reihitsu.Core` that keeps the name needs no spec change. Both analyzer specs state in their introductions that the analyzer, its fix, and the other rule decide by that one predicate, not by a copy. The change doc records the move under *Decisions → One predicate for the gap*.

**If the implementer renames the type or splits it**, update the FMT-BREAK code-owner list in `line-breaks.md` and in `sdd/specs/README.md`, the type name in the two analyzer introductions, and the `LineBreakPhase` row of the phase table in `AGENTS.md`, which names `AttachedPartGapUtilities.IsAttachedPartGap`.

## Examples for the tests

- RH5115: 001 (both blocks), 002, 003, 004, 005. 006 uses 002's example.
- RH5116: 001 (both blocks), 002, 003, 004, 005. 006 uses 002's example.
- Both rule pages are checked by `RuleDocumentationExampleTests`.

The skeptic's breaking inputs in `02-skeptic.md` split by part:

- To RH5115: everything with `[` or `?`.
- To RH5116: `s⏎!.Length` and `s⏎!?.Length` not reported; `s⏎!?[0]` reported on `!`; `f⏎!(1)` reported on `!` only; the `!` of `s⏎!⏎?[0]`.
- `s!⏎?[0]` is RH5115's, on `?`.

Checks I ran:

- I stripped the markup from all 16 spec code blocks and ran them through `scripts/format.sh`. Every input parses.
- The formatter joins every reported part as stated.
- Both comment examples (RH5115-002, RH5116-002) are refused as unsupported (FMT-UNSUP-014).
- The 005 outputs match the formatter's output of their inputs.
- The 004 outputs differ from the formatter's output only on the line 5A leaves to other rules.
- On those outputs, `scripts/apply-fix.sh RH5201` reports and realigns `.Trim` (RH5115-004 `c`), and `scripts/apply-fix.sh RH5203` reports and realigns `2` (RH5116-004 `b`), under LF and CRLF.

`scripts/test.sh --project architecture`: 195 passed and 3 failed. All 3 fail only because the rules do not exist yet:

- `AnalyzerSpecHeaderMatchesItsRule`: no RH5115 or RH5116 rule.
- `EveryCodeOwnerNamesAnExistingType`: the four new type names.
- `EveryRequirementOfAnEnforcedSpecIsCovered`: RH5115-001 to RH5115-006 and RH5116-001 to RH5116-006.

The ledger and `Uses` checks pass.

Implementer notes (not spec):

- `Release.ruleset` needs entries for both IDs.
- The analyzer and code-fix type names from round 01 change. Use the names above.

## Output questions

_None._ All five batch-1 answers are applied. The two-rule split raises no new visible case. For `s⏎!⏎?[0]`, both rules report and their fixes work on different gaps, which follows from answer 4.

## Internal choices

- **Requirement IDs.** I kept RH5115-001 to RH5115-006 for the narrowed requirements and added RH5116-001 to RH5116-006. These IDs were never merged, so no retirement is needed. The alternative was to retire the six and renumber.
- **Change doc file name.** I renamed it to `rh5115-rh5116-index-and-null-forgiving-line.md`, because it now creates two specs. The alternative was to keep the old name. Nothing outside `sdd/work/` referenced the old one.
- **One change doc for both rules.** They share a cause, an issue, and a fix design. The alternative was one change doc per rule.
- **RH5116 negative example.** `f &&⏎!f` shows that a logical negation is not a null-forgiving operator. It is also a non-goal.
- **The RH5116 page says** that a `!` directly in front of `.` or `?.` is not reported by the rule. That is the normal case for a user who wraps a chain at `!.`, so it is not a corner-case exception. The alternative was to leave it out. Remove it if the lead reads it as an exception.

## Follow-up candidates

_None new._ `00-lead.md` already lists the comment on thoenissen/Reihitsu#1827 (1A stands, so argument lists stay with it) and the query-clause candidate from `02-skeptic.md`.
