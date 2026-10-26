Role: spec author
Commit: 2844903
Verdict: spec written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to make the specs consistent with the audit-3 decision, option B (`00-lead.md`, last section; `squad/decisions/2026-10-26-phase-prediction-removal-audit-3.md`). That decision keeps `FormatNodeInDocumentWithContextAsync` as on `main` and records a gap. The prompt also named the two contradictions that `39-test-implementer.md` (*Spec observations*) reports:
- SHARED-FIX-007, "formatting the result changes nothing in that code";
- RH5111-004, "a value that spans several lines keeps its own layout from its first line on".

This round records the gap in both, as a decision. The decided behavior is unchanged: every output named below is one that the tests of `39-test-implementer.md` assert and that `40-implementer.md` passes.

## Changed requirements

- **SHARED-FIX-007 — clarified (recorded gap).**
  - New bullet: "Changed code on a later line of the node it is formatted in — a recorded gap." It applies when a fix formats its changed code together with a larger node around it, and the changed code starts a later line of that node. The RH5111 fix of an enum member, a parameter, or a variable declarator is the example it names.
  - In that case the fix keeps the changed code's leading trivia as it stands (FMT-PIPE-018): blank lines, comments and directives in their columns, a `///` written behind the code in front of it, and the whitespace in front of the first token.
  - So the first line keeps its column. The other lines take the formatter's columns with that first line in the formatter's column, and a line aligned to an anchor on the first line can stand off the anchor.
  - A move that formatting the result then makes to that line, to the trivia above it, or to the lines that depend on it is not a difference the requirement counts.
  - The bullet calls this a decision, not an oversight. It names the change `phase-prediction-removal` and FMT-PIPE-017, and leaves the restructuring of the context entry point to a later change.
  - It also states the other side of the qualifier: changed code that does not start its line (a first parameter behind `(`, a first declarator behind its type), or that starts the node's first line, is not in the gap.
  - The tab bullet above it now points to the new bullet. The blank line between those two bullets, which made the list loose, is removed.
  - New example line, which refers to the RH5111-004 examples, following the RH7103/SHARED-DOC-004 precedent.
  - Header `Uses`: + FMT-PIPE-018, + RH5111-004.
- **RH5111-004 — clarified (recorded gap).**
  - The normative sentence now points to a new paragraph, "Recorded gap". That paragraph covers an enum member, a parameter, or a variable declarator that starts a later line of its enum, parameter list, or declaration.
  - For such a construct, the leading trivia is kept as in the document (FMT-PIPE-018) and the first line keeps its column. The other lines take the formatter's columns, so a multi-line value does not keep its own layout there.
  - The paragraph says the gap is a decision: it is the gap of SHARED-FIX-007 and FMT-PIPE-017, the output is the fix's output from before the change, and the fix is still offered.
  - The other side of the qualifier: the first parameter behind `(` and the first declarator behind its type are outside the gap, and so are assignment expressions, properties, and object-initializer members. The existing example now carries the title "constructs outside the gap".
  - The Why adds one sentence on why the gap stays.
  - New examples, each taken verbatim from a passing test of `39-test-implementer.md`:
    - an enum member in column 6 (`VerifyEnumMemberOnALaterLineIsJoinedInItsColumn`);
    - `First, /// Doc` kept on its line (`VerifyEnumMemberWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAsWritten`);
    - the wrapped declarator: `b` at 14, `2` at 18, `1` at 24 (`VerifyDeclaratorOnALaterLineKeepsItsFirstLineColumnAndLeavesTheWrappedArgumentInTheFormattersLayout`).

    The phases' column 8 for `b` comes from FMT-PIPE-017's gap example. I did not use the test implementer's "`2` 18 → 14" for document-level formatting, because I could not derive it from a spec.
  - Header `Uses`: + FMT-PIPE-017, + FMT-PIPE-018, + SHARED-FIX-007.

No new IDs. FMT-PIPE-017, FMT-PIPE-018, and `AGENTS.md` are unchanged from `38-spec-author.md`.

`scripts/test.sh --configuration Debug --project architecture` on the working tree gives `Passed: 89, Failed: 0`. That run includes the `Uses` and requirement-ID checks.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` stays **`Status: Accepted`**, because the audit-3 decision covers this. What changed:
- **Affected requirements:** + RH5111-004. SHARED-FIX-007 was already listed.
- **What, node-level bullet:** the closing sentence now reads "…lays out its code in the columns the formatter gives it, apart from that gap, which SHARED-FIX-007 and RH5111-004 record for the code fixes."
- **Decision "The node-level entry points lay out their node at its own column", last paragraph:** new sentences say that SHARED-FIX-007 and RH5111-004 record the gap for the code fixes. They also cover the other callers of the context entry point:
  - RH3208 passes a context only for a local function behind a label on its line, which does not start its line, so it is outside the gap.
  - RH5112 and RH5201 keep the chain-in-context result only when the chain has the same text and first-token column as when the context is formatted. So the first-line column of the gap does not reach them. The kept trivia does, for example `{ /// Doc` (the RH5201 #11 tests), and the general wording of SHARED-FIX-007 covers it.

## Rule page

`documentation/rules/RH5111.md` is unchanged: the gap is a corner case of the fix's layout, not a change to what is reported or how to fix it.

## Examples for the tests

- RH5111-004 (recorded gap): the three new examples. Each one is already an existing test (named above). Of those tests, two carry `RH5111-004`. The declarator test is deliberately untagged (`39-test-implementer.md`).

  Now that RH5111-004 states the gap, the spec checker may tag the declarator test `RH5111-004`. That is a tagging matter, not a new test.
- SHARED-FIX-007: no new seed. The bullet points to the RH5111-004 examples.

## Open questions for the maintainer

_None._ RH5201-005 and SHARED-CHAIN-008/009 make no claim about the target's leading trivia. So the `///` behind `{` that stays on its line contradicts nothing there, and SHARED-FIX-007's new bullet covers it in general terms. Follow-up candidates are unchanged from `40-implementer.md`.
