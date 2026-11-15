Role: spec author
Commit: 39869ad
Verdict: spec written

The prompt named the work (write the escalation-10 declaration into the specs and change docs) and no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's answers in `00-lead.md`, which are applied as binding. The shape is written exactly as `04-skeptic.md` §1 defines it; its *Special cases* are taken from §2, and each named code site was checked on HEAD (`src/` is `main`'s). `scripts/test.sh --project architecture --filter Sdd`: every spec structure and consistency check passes; the only failure is `EveryRequirementOfAnEnforcedSpecIsCovered` for RH5107-004, FMT-BREAK-038, and FMT-UNSUP-009, which the test implementer's `[Requirement]` tests close (FMT-INDENT-033 is retired and needs none).

## thoenissen/Reihitsu#1737

### Changed requirements

- None in this round. RH5107-004 and RH5107-003's pointer stay as written at 19e8ee2: the declaration concerns the gap in front of the comma, RH5107-004 a comment behind it.

Change doc `sdd/changes/2026-11/rh5107-comment-behind-leading-comma.md`: new decision *Not affected by the declaration of FMT-UNSUP-009* records the escalation-10 answer and why RH5107's analyzer guard (SHARED-JOIN-001) stays. `Status: Draft`.

### Examples for the tests

Unchanged from `03-spec-author.md` (RH5107-004).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None new._

## thoenissen/Reihitsu#1738

### Changed requirements

- FMT-BREAK-038 — changed (not yet shipped): the held-comma paragraph and its third example are deleted; "unless it is held" is dropped; the multi-line-comment sentence is replaced by a pointer to FMT-UNSUP-009; `Why:` reworded (a held comma is refused, not given a line and column). The unheld part — the move, the comment behind the comma, arrays, trailing commas — is unchanged.
- FMT-INDENT-033 — retired (it is in the ledger since 19e8ee2): heading kept, body replaced by `Status: Retired in the change that added it — …` naming FMT-UNSUP-009 and FMT-BREAK-038.
- `indentation.md` non-goal reworded: names the switch-arm, positional-pattern, and base-list commas without FMT-INDENT-033, and states that FMT-BREAK-038 leaves no comma line in the initializer family.
- `line-breaks.md` `Uses`: FMT-INDENT-033 removed.

Change doc `sdd/changes/2026-11/initializer-leading-comma.md` rewritten for the answer: *What* drops the held-comma and column bullets, FMT-INDENT-033 is "added and retired in this change", cases `c` and `g` (held) leave the visible-output blocks (their change is in the declaration's doc), `Depends on` the declaration's doc. *Decisions*: batch-1 Q1 recorded with its held-comma half and batch 1b marked superseded; batch-1 Q2 kept (comma-first code stays supported); new decision for escalation 10 (Q4 A, Q5 A). One alternative added (the held comma alone on its line, superseded). `Status: Draft`.

### Examples for the tests

- FMT-BREAK-038: its two remaining examples (moving commas with and without comments in object, anonymous, `with`, collection-expression, property-pattern constructs, trailing comma; arrays `e`, `f`, `y`). The skeptic's extra cases still apply except the held ones: nested `Items = { … }` and complex element initializer, `new[] { … }` and `stackalloc`, a property pattern directly behind `case`, nesting inside an argument and a lambda, CRLF and a second pass, and the RH5301/RH5303/RH5304 fix paths on a leading-comma input (result equals whole-document formatting, SHARED-FIX-007).
- Drop from the test list: every held-comma case in initializers (`// k` held in each construct, the directive-held `X = 3⏎#if A⏎, Y = 4⏎#endif`) and the multi-line `/* a⏎ b */` in front of the comma — these become FMT-UNSUP-009 refusal cases.

### Output questions

_None._

### Internal choices

_None beyond the run-wide ones below._

### Follow-up candidates

_None new._ The switch-arm, positional-pattern, and base-list follow-ups in `00-lead.md` concern unheld commas and stay.

## Whole run

### Changed requirements

- FMT-UNSUP-009 — Comments and directives between a list element and the comma behind it are unsupported — added (ledger updated). Every comma token of the code; comment (every documentation mode), directive, or disabled text in the token-in-front's trailing trivia or the comma's leading trivia; comma on a later line of the text (a break inside a multi-line comment counts). Reason `a comment or directive between a list element and the comma behind it`; location the first such trivia. `Uses` gains FMT-BREAK-001 and SHARED-BLANK-008.
- FMT-BREAK-011 — changed: the exception for a comma kept off the previous line is deleted.
- FMT-BREAK-012 — changed: the held-comma paragraph (with its SHARED-BLANK-008 sentence) is replaced by two sentences pointing to FMT-UNSUP-009; `Why:` reworded; the held example becomes "a comment behind a moving comma goes with it; a block comment in front of a comma on the element's line keeps nothing" (`, /* c */ 2`, `, // c`, `Foo(1 /* one */, 2,⏎3)`), with `N`, `P`, `3 // keep⏎, 4`, and the `#if X` list removed.
- FMT-BREAK-037 — changed: "a comma kept off the previous line stays…" replaced by a pointer to FMT-UNSUP-009; "a comma" dropped from the multi-line-comment counting sentence (plus a refusal sentence); `P` and `Q` removed from the comments example and `O` from the multi-line-comment example, intros reworded.
- FMT-INDENT-032 — changed: no comma lines to place (rule, scope sentence, and the held `O` example).
- FMT-STRUCT-027 — changed: "or the comment led the comma on a line of its own" replaced by a refusal sentence; example rewritten to `B⏎, // comma⏎}` and `2⏎, // comma⏎};` (outputs checked with `scripts/format.sh` on `main`, run twice and stable). `Uses` gains FMT-UNSUP-009.
- RH5110-003 — changed: the fix is withheld where the formatter refuses the declaration (SHARED-FIX-010), e.g. FMT-UNSUP-009; the comment example is split — `M` (comment behind the colon) keeps its fix, `N` (comment in front of a comma) "is reported, and no code fix is offered". `Uses` gains FMT-UNSUP-009, SHARED-FIX-010. Rule page unchanged (withheld fix in a corner case; `general-notes.md` covers it).
- `AGENTS.md` — the `PrecheckPhase` row names the third shape (FMT-UNSUP-009), as it names the first two.

Change doc: **its own**, `sdd/changes/2026-11/comma-behind-comment-unsupported.md`, not folded into #1738's — the declaration reaches every list and changes six requirements in four other specs, while #1738's doc covers only the initializer family. It holds What, Why, the visible output change (input, `main` output from `scripts/format.sh`, run twice and stable, and After: unchanged with the CLI line `error: <file>(3,21): …`, fixes no longer offered), *Decisions* (escalation 10 Q4 A and Q5 A, superseding batch-1 Q2 for this shape and making batch 1b moot; every comma token; text lines; FMT-UNSUP-004 keeps its reason), *Special cases* with *Deleted* (five `main` code sites, the spec text, and what this run never writes) and *Checked and kept*, and the alternatives. `Status: Draft`.

### Examples for the tests

- FMT-UNSUP-009: its three examples (refused lists: parameter with `// c1`, constraint with `/* c2 */`, argument with `// c3` on its own line, initializer `// c4`, multi-line comment `/* a⏎ b */,`; the directive list refused in both builds of `DEBUG`; the other side — enum with comma-last `#if X`, `Foo(true /* force */, false)`, comments behind commas, comma-first without comments, `Foo(1⏎#if NEVER⏎, 2⏎#endif⏎)` without `NEVER`). Also the bullets' two-sided cases: `typeof(Dictionary<⏎// c⏎,>)` refused vs `typeof(Dictionary<, // c⏎>)` not; a `///` and a `/** */` comment in front of a comma, in each documentation mode; a trailing comma held (`2 /* two */⏎, }`); a declarator comma with a comment (FMT-UNSUP-004's reason) and with a directive (FMT-UNSUP-009's reason); every entry point, node-level targets holding the comma vs an element-only target; closure (FMT-UNSUP-005) on the comma-moving examples.
- FMT-BREAK-012, FMT-BREAK-037, FMT-INDENT-032, FMT-STRUCT-027, RH5110-003: their changed examples. The existing tests of the deleted special cases (`ListWrapByElementsTests`, `ConstraintClauseLineBreakTests`, `ConstraintClauseAlignmentTests`, `TrailingCommaCommentOwnerTests`, `ArgumentContinuationLineAlignmentTests`, `ParameterContinuationLineAlignmentTests`, `NestedListInParameterAlignmentTests`, `StrandedDocumentation*Tests` with `1 /// stray⏎, 2`, `ZeroWidthTokenLineStartTests`, the RH5107/RH5110/RH5203 formatter-side tests, and the FMT-BREAK-011/012/037 requirement tests) turn their held cases into refusal tests or are deleted (`04-skeptic.md` §2).

### Output questions

_None._ The declaration's visible output was asked and answered (escalation 10).

### Internal choices

- **Own change doc for the declaration** rather than folding it into #1738's. Alternative: one doc for #1738 and the declaration.
- **FMT-UNSUP-004 keeps its reason** where both shapes refuse the same comment in front of a declarator comma (they start at the same trivia, so FMT-UNSUP-001's "first in the text" does not decide). This keeps the message such a declaration reports today unchanged, so nothing visible is in question; a directive there is refused under FMT-UNSUP-009 only. Alternative: FMT-UNSUP-009's reason for a declarator comma.
- **Omitted element:** the gap is measured from the token in front of a zero-width omitted element (`Dictionary<⏎// c⏎,>` refused); the skeptic found that either reading refuses the same inputs.
- **FMT-INDENT-033 retired, not removed:** it is in the ledger since 19e8ee2; the rulebook forbids removing a ledger line.
- **`AGENTS.md` updated** (PrecheckPhase row), as the change that declared FMT-UNSUP-008 did.

### Follow-up candidates

_None new._
