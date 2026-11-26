Role: spec author
Commit: 1c6d597
Verdict: spec written

Spec commit: 577e60a. Change doc `sdd/changes/2026-11/assignment-and-arm-comment-unsupported.md` is now `Status: Accepted`.

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (*Maintainer answers*, batch 2; decision 7), `03-spec-author.md`, and `04-skeptic.md`.

## Changed requirements

- FMT-UNSUP-022 — Comments and directives behind the `=` of an attribute argument or a using alias are unsupported — added (batch 2, Q2 B).
  - Shape: a comment, directive, or disabled text in the trailing trivia of the `=` of a name-equals whose parent is an attribute argument or a using directive (`global` and namespace-scoped included), or in the leading trivia of the first token behind it.
  - Other side: the anonymous-object member's `=` stays FMT-UNSUP-017's; a named attribute argument with `:`; a comment in front of the `=` or behind the value's first token.
  - Node-level: an input holds the shape when it holds both the `=` and the trivia in the gap; the value alone, also with the comment in its leading trivia, is formatted (FMT-UNSUP-003).
  - Listed after every other shape, so FMT-UNSUP-004 keeps its reason for an attribute argument of a field, and FMT-UNSUP-010 for a directive there in a multi-declarator field.
  - Reason (decision 7): `a comment or directive behind the = of an attribute argument or a using alias`.
  - Deletes no special case; declared on the maintainer's explicit answer.
- FMT-UNSUP-020 — clarified:
  - The equals-value bullet now reads as `04-skeptic.md` *Internal choices* 1 asks: the gap belongs to the shape for every owner, a comment and a directive alike, and a declarator's clause formatted without its declaration is refused with this requirement's reason; with the declaration in the input, the order rule gives FMT-UNSUP-004's or FMT-UNSUP-010's reason.
  - "No other `=`" now names FMT-UNSUP-022 for the attribute argument and the using alias.
  - The other-side example drops the using alias and the attribute (now refused); it keeps `c1`–`c4` (property, parameter, assignment in front of `=`, inside the value).
  - The reason is unchanged (decision 7).
- FMT-UNSUP-021 — clarified: listed after FMT-UNSUP-004 through FMT-UNSUP-020 (was "after every other").
- RH7101-007 — clarified: the list of refused shapes gains FMT-UNSUP-022 (an attribute argument in the type the fix formats); *Uses* row gains FMT-UNSUP-022.
- `unsupported-syntax.md` *Uses* gains FMT-BREAK-015; `requirement-ids.txt` gains FMT-UNSUP-022.
- `AGENTS.md` precheck row: the fifteenth shape, FMT-UNSUP-022.

Change doc:
- Title widened to "behind an assignment operator, an `=`, or a switch arm's `=>`"; *What*, *Why*, *Affected requirements* name FMT-UNSUP-022.
- *Visible output changes*: the reasons line names FMT-UNSUP-022's reason; new section for the attribute argument and the using alias (before from `scripts/format.sh`, CLI line at (2,14); without the alias at (10,15)); *Unchanged* now lists the front-of-`=` and named-colon cases instead of the name-equals ones.
- *Code fixes*: RH7101 with an attribute argument in the type (before from `scripts/apply-fix.sh`: splits `_a, _b`; after: no fix); RH3202 on a method whose attribute holds the shape (before: block body; after: no fix); RH5305 named as still offered, with its before/after (`04-skeptic.md` *Internal choices* 2; checked with `apply-fix.sh`); the using alias withholds no fix (the RH7201–RH7207 fixes go through `OrganizeUsingDirectivesInDocumentAsync`, which runs no precheck, FMT-PIPE-030); RH5101/RH5102 already withheld for a comment in the attribute argument list (RH5101-004).
- *Special cases*, *Checked and kept*: FMT-BREAK-001's refusal in FMT-BREAK-015's join behind an attribute argument's `=` (the shared `WouldJoinAcrossUnjoinableTrivia` check in `ListElementInteriorJoinRewriter.CanJoin`, which the other gaps in an element need).
- *Decisions*: the batch 2 answer (name-equals declared in this work, against the run's recommendation); the two reasons (decision 7, delegated); "Two requirements" became "Three requirements"; the order paragraph names FMT-UNSUP-021 and FMT-UNSUP-022.
- *Alternatives*: keep the name-equals supported (rejected by the maintainer); one reason for every `=` (not chosen).

How I checked:
- `scripts/format.sh` on this branch: the FMT-UNSUP-022 input (before matches `03-spec-author.md`), the new FMT-UNSUP-020 other side, and the FMT-UNSUP-022 other side; both other sides are fixed points. A `[N(Name =⏎1)]` without a comment is joined, confirming the FMT-BREAK-015 join exists.
- `scripts/apply-fix.sh` for RH7101, RH3202, and RH5305 befores.
- Locations counted from the blocks: (2,14), (10,15).
- `scripts/test.sh --project architecture`: 196 passed, 2 failed, both for the test implementer: the FMT-UNSUP-019 tags in `tests/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedAssignmentInArmCommentTests.cs`, and no tests yet for FMT-UNSUP-020, 021, 022.

## Examples for the tests

- FMT-UNSUP-022: the refused example (location 2,14), the attribute-alone example (10,15), and the other side; plus from the text: a directive behind a using alias's `=`, a `global using` alias, one-line `[N(Name = /* c */ 1)]`, the field-attribute case keeping FMT-UNSUP-004's reason, a node-level value target that is formatted.
- FMT-UNSUP-020: the new other side; one precheck row for a declarator or equals-value clause target with a comment behind `=`, refused with FMT-UNSUP-020's reason (`04-skeptic.md` *Internal choices* 1).
- The change doc's *Code fixes*: add RH7101 and RH3202 with an attribute argument, and the RH5305 still-offered case, to the list in `03-spec-author.md`.
- The test lists of `02-skeptic.md`, `03-spec-author.md`, and `04-skeptic.md` (*Breaking inputs*) still apply.

## Output questions

_None._ Batch 2 is answered and recorded; the wording is decision 7.

## Internal choices

- **A third requirement, FMT-UNSUP-022, rather than widening FMT-UNSUP-020**: decision 7 gives the name-equals its own reason, and each requirement of the spec carries one reason. Alternative: one requirement with two reasons.
- **FMT-UNSUP-022 listed last**, after FMT-UNSUP-021: keeps FMT-UNSUP-004/010 reasons for an attribute argument of a field, and the anonymous-object member under FMT-UNSUP-017. Its gaps never coincide with FMT-UNSUP-021's.
- **FMT-BREAK-015 is not changed** to say its join behind an attribute argument's `=` never meets a comment: its refusal is the shared check every gap inside an element uses, and the change doc records it under *Checked and kept*. Alternative: a sentence in FMT-BREAK-015 like the one FMT-BREAK-016 carries.
- **No rule page changes**: RH5042-002 (the analyzer does not report a comment around a name-equals `=`) is an analyzer rule and stays as it is; the formatter's list does not depend on it.

## Follow-up candidates

_None new._ The `int P { get; } /** c */ = 1;` candidate stays as `00-lead.md` records it.
