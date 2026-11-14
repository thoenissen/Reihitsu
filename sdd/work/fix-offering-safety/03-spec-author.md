Role: spec author
Commit: 9bf6c0b
Verdict: change doc written; spec written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's recorded answers and the lead's decisions, which are inputs to this stage; nothing was ignored.

Change doc: `sdd/changes/2026-11/fix-offering-safety.md` (`Status: Draft`). It covers Q1 B, Q2 A, Q3 B, Q4 C (declared in this work, with *Special cases*), and Q5 A, each under `Decisions`. #1150's SHARED-FIX-011 is not in it: it carries the maintainer's 2026-11-06 text, which the lead treats as spec text without a change doc.

`scripts/test.sh --project architecture` (spec, requirement, and rule-documentation tests): 50 passed. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, and it names only RH5308-005, RH5308-006, FMT-PIPE-033, FMT-UNSUP-008, SHARED-FIX-011, and SHARED-FIX-012 as uncovered. The test implementer's tags resolve it.

I ran the formatter examples through `scripts/format.sh` on this tree (scratchpad copies). The `before` blocks of the change doc and the formatted blocks of FMT-PIPE-033 are observed output. One example was corrected to what the formatter writes: the raw string's content moves to column 16, and a blank line goes in front of `return`.

## thoenissen/Reihitsu#1600

### Changed requirements

- FMT-PIPE-033 — Whole-document formatting leaves a file alone whose text another build compiles as code — added (Q1 B; Q2 A's gap is recorded in it).
  - `FormatDocumentAsync`/`FormatSyntaxTree` return the input unchanged, with `Unsupported` null, when the tree holds a *hiding carrier*.
  - A hiding carrier is a string literal or a `/* */`/`/** */` comment that spans lines, starts inside a conditional group, and holds a later line whose text is `#`, optional whitespace, then `elif`, `else`, or `endif` (lead decision 7).
  - The node-level entry points do not check it, because SHARED-FIX-012 keeps every fix out of such a document.
- SHARED-FIX-012 — No fix is offered in a document whose text another build compiles as code — added (Q3 B; Q2 A's gap is recorded in it).
  - Every code fix is withheld when the diagnostic's document holds a hiding carrier, and, for a fix that changes other documents too (a rename), when one of those holds one.
  - The fix decides this at registration, by FMT-PIPE-033's definition.
- FMT-PIPE-003 — Why: one sentence pointing to FMT-PIPE-033 — clarified.
- SHARED-FIX-003 — *Build configurations* bullet: one sentence pointing to SHARED-FIX-012 — clarified.
- FMT-UNSUP-001, FMT-UNSUP-002, and the FMT-UNSUP non-goal now name FMT-PIPE-033 next to FMT-PIPE-005/006. Such an input is returned unchanged and not reported, and FMT-PIPE-033 applies before the precheck — changed.
- `code-fixes.md` non-goal now names SHARED-FIX-012.
- `Uses` updated: `pipeline.md` (+SHARED-FIX-012), `unsupported-syntax.md` (+FMT-PIPE-033), `code-fixes.md` (+FMT-PIPE-033).
- `AGENTS.md`, *Key conventions*: the bullet on input the formatter leaves alone names FMT-PIPE-033.

### Examples for the tests

- FMT-PIPE-033: the `#if !A` comment example, returned unchanged with `Unsupported` null. Its other side is the comment holding `#if`/`#endif` with no enclosing group plus the Markdown raw string, which is formatted. The decided-gap example (member (b)) is formatted. All three are observed with the CLI.
- SHARED-FIX-012:
  - Run the RH3004 and RH6024 example with `A` defined: both are reported and neither fix is offered. RH6024 is withheld although its edit is safe, which pins the document-wide scope.
  - Other side: a comment with directive lines and no enclosing group, where RH6024 is fixed.
  - Decided gap: RH6024 in member (b) without `B`, where the fix is offered.
- The test implementer also covers each condition on both sides:
  - a raw string and an interpolated verbatim string as carriers;
  - a carrier holding only `# Heading` or `#nullable` (not a carrier);
  - `#  else` (whitespace after `#`, still counts);
  - for SHARED-FIX-012, a rename (casing base) whose other document holds a carrier.

### Output questions

_None._ (See *Whole run* for points the lead may want to confirm.)

### Internal choices

- **Scope of the fix-side withholding: the whole document**, not "a carrier that starts at or before the end of the fix's edit". The base class runs before any fix registers and knows only the diagnostic. Neither the diagnostic nor the edit bounds where a fix writes in the other build's string: a rename reaches every reference, and a move writes where the member lands. The alternative needs per-fix edit spans, which is what Q3 B avoids. This is a rare-class withholding, which is the lead's to decide.
- **Renames into other documents:** I specified "every document the fix changes". The casing base already computes the renamed solution at registration (`HasConflictAnnotationsAsync` walks the changed documents), so the check is cheap there. The alternative is to record those other documents as a gap.
- **One definition:** SHARED-FIX-012 says the fixes decide by FMT-PIPE-033's definition, not by a copy (FC-015). The implementer should put the predicate in one place both callers reach. The fixes reach formatter types only through `ReihitsuFormatter` (FMT-PIPE-009), so either a `ReihitsuFormatter` query or a Core helper the formatter also uses.
- **New base type as code owner:** it does not exist yet, so `code-fixes.md` keeps `Code owners: —`. The implementer adds the type to the shared spec's code owners (and `sdd/specs/README.md`) when it is created. Recommend SHARED-FIX as the owning spec.
- **RH3004-008 is unchanged.** RH3004's private `ContainsLineStartingWithHash` counts every `#` line in carriers inside the method for a layout reason of its own (RH3004-006/008), not for hiding. It stays a separate decision. The skeptic's FC-015 note applies only if the implementer finds the two checks answer the same question.
- **Precedence:** where a document holds both a hiding carrier and an FMT-UNSUP shape, FMT-PIPE-033 applies first and nothing is reported. I read this from "like a syntax error" (FMT-UNSUP-002 already lets FMT-PIPE-005/006 apply first). It only matters for files that hold both rare shapes.
- **CLI:** I added no CLI requirement. The CLI sees an unchanged result with `Unsupported` null, so it counts the file as unchanged: `Unchanged: <path>` with `--verbose`, and `--check` passes it. CLI-010 (`--utf8-bom` normalizes the encoding of a processed file) applies as it does today for that file; the encoding does not touch the hidden text.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1150

### Changed requirements

- SHARED-FIX-011 — changed (skeptic challenges 1 and 3, both agreed):
  - The first sentence now reads "returns its input unchanged because the syntax tree holds an error (FMT-PIPE-005)", which closes the blanket-compare reading.
  - The example is replaced. The format-only fix is now RH5308 with `void N(` unclosed, reported with no fix. The edit-then-format fix is now RH3004 (`Action a = delegate () { M(); };`), not offered. A control states that the RH6023 text-edit fix still writes `a = 1;` in the same file.
  - `code-fixes.md` `Uses` gains RH5308-004.
- FMT-PIPE-005 — Why: appended "apart from a shape refused as unsupported (FMT-UNSUP-001, SHARED-FIX-010)" (challenge 2, agreed).

### Examples for the tests

- SHARED-FIX-011:
  - the RH5308 example (no fix with the syntax error, fixed once it is closed);
  - RH3004 with a syntax error (no fix);
  - the RH6023 control, offered and not tagged SHARED-FIX-011;
  - one test per base, helper, and direct provider in the skeptic's corrected list.

### Output questions

_None._

### Internal choices

- **Challenge 4 (RH5401-004 against SHARED-FIX-011's third sentence): not changed here.** The sentence is the maintainer's text, and RH5401 is outside the run's issues. It is already in `00-lead.md` *Follow-up candidates*. The spec checker or auditor may flag RH5401-004; the answer is that follow-up.
- SHARED-FIX-011's second sentence still names FMT-STRUCT-003 and FMT-STRUCT-023 as examples. Both still decline today. The Q6/Q7 follow-ups will retire them in their own changes.

### Follow-up candidates

_None new._

## thoenissen/Reihitsu#1274

### Changed requirements

- FMT-UNSUP-008 — Comments and directives between a conditional operator and its branch are unsupported — added (Q4 C, declared in this work).
  - The span is the trailing trivia of `?`/`:` and the leading trivia of the branch's first token.
  - Refused there: a comment (every kind and documentation mode), a directive, or disabled text.
  - Not refused: anything in front of the operator, and anything inside a branch behind its first token.
  - Reason: `a comment or directive between a conditional operator and its branch`. Location: the first such trivia.
- RH5308-005 — The fix is withheld for a comment or directive behind an operator — added. The diagnostic stays, and the fix is withheld through FMT-UNSUP-007/SHARED-FIX-010, which the RH5308 fix already calls.
- FMT-BREAK-018 — Why: one sentence saying the branch join never meets such trivia (FMT-UNSUP-008) — clarified. `line-breaks.md` `Uses` gains FMT-UNSUP-008.
- `RH5308.md` `Uses` gains FMT-UNSUP-007, FMT-UNSUP-008, SHARED-FIX-005, and SHARED-FIX-010 (SHARED-FIX-005 is for RH5308-006 under #1673).
- `AGENTS.md`, *Formatter phase ownership*, `PrecheckPhase` row: names the second shape.
- Change doc *Special cases*:
  - Deleted: the two `WouldJoinAcrossUnjoinableTrivia` refusals in `TernaryLineBreakRewriter` (`MoveQuestionTokenToNextLine`, `NormalizeColonTokenPosition`).
  - Checked and kept: the shared utility and FMT-BREAK-001, `EndLineInFrontOf`, `ConditionalExpressionContributor`, `ConditionalOperatorSpacingRule`, `ListElementInteriorJoinRewriter.IsOperatorWrap`, the spacing phase's comment guards, SHARED-COMMENT-011, and the RH5308 fix and analyzer.
  - A node holding a conditional always holds both of its operators and the trivia behind them, so no partial-shape input (FMT-UNSUP-003) keeps the deleted guards alive.

### Examples for the tests

- FMT-UNSUP-008:
  - Refused: `? // c1`, `: // c2`, and `? /* c3 */ 1` (inline).
  - Refused: an `#if DEBUG … #else … #endif` branch behind `?`.
  - Formatted (other side): a comment on its own line in front of `?`, a comment behind the condition, a comment behind the true branch, and a comment inside a branch's argument.
  - The test implementer adds: nested conditionals, a conditional in a lambda in a branch, every entry point, `///` in both documentation modes, the location of the first of two, and closure (FMT-UNSUP-005: the line-break phase must not move a comment in front of the operator into the span).
- RH5308-005: two reported conditionals (`?` and `:` members) with no fix; the comment-above control, where the fix is offered.
- Existing tests of the two deleted refusals are rewritten to the refusal.

### Output questions

_None._

### Internal choices

- **I read the answer literally: any comment between the operator and its branch, including the inline `a ? /* c */ 1`.** That input is formatted and fixed in one application today, and after this change it is refused. This follows the maintainer's "comments are supported only above the `?` or `:` line", and the change doc lists it as a visible change (#4, second input). The lead should confirm that the batch-1 question showed or implied this case. If not, it is a narrowing question for the maintainer, not a choice I make.
- **RH5308 keeps reporting the refused conditional, with no fix.** This follows the precedent of every refused shape (FMT-UNSUP non-goal, SHARED-FIX-010, RH7101-007). SHARED-DIAG-010 speaks of shapes the formatter *keeps*, not shapes it refuses as a whole.

### Follow-up candidates

- The lead's formatter-class candidate (a continuation line after a `//` comment is laid out in the statement's column) loses its conditional member, `a ? // c`, which is now refused. The `b + // c` and `int A => // c` members remain. Its sibling candidate ("true branch behind a `//` on the `?` line in column 8") is resolved by FMT-UNSUP-008 and can be dropped.

## thoenissen/Reihitsu#1143

No spec change. SHARED-FIX-002 decides it, including the guard-dependent second passes (RH5104, RH5405) that the issue allows with a comment. Tests tag SHARED-FIX-002.

## thoenissen/Reihitsu#1673

### Changed requirements

- RH5308-006 — The fix is withheld for a conditional outside any statement or member — added.
  - The case is a conditional in an `[assembly:]`/`[module:]` attribute argument: reported, no fix.
  - It is the skeptic's narrowed instance and a SHARED-FIX-005 bug fix. I recorded it in the rule's spec because `AGENTS.md` (*Code-fix coverage*) asks that a deliberately unfixed class read as a decision there.
  - It is not a maintainer-decided output and is not in the change doc.

Nothing for the modifier-ordering base, RH7107, or RH7108 (dropped, lead decision 4).

### Examples for the tests

- RH5308-006: the `[assembly: System.CLSCompliant(true ? … )]` example with `VerifyNoCodeFixIsOffered`, plus a `[module:]` variant. Tag RH5308-006 and SHARED-FIX-005.

## thoenissen/Reihitsu#1678

### Changed requirements

- SHARED-ORDER-008 — the decided-gap paragraph now gives the maintainer's reason (Q5 A): the analyzers analyze the build whose symbols the project defines as they stand, which is the common case, and a move is judged in that build — clarified. No behavior changes, so no test is needed. The issue is linked with `Refs`, and its result is "gap kept as a decision".

## Whole run

### Changed requirements, collected

| ID | Kind |
|---|---|
| FMT-PIPE-033 | added |
| FMT-UNSUP-008 | added |
| SHARED-FIX-012 | added |
| RH5308-005 | added |
| RH5308-006 | added |
| SHARED-FIX-011 | changed (#1150 corrections) |
| FMT-UNSUP-001 | changed (FMT-PIPE-033 named) |
| FMT-UNSUP-002 | changed (FMT-PIPE-033 named) |
| FMT-PIPE-003 | clarified (Why) |
| FMT-PIPE-005 | clarified (Why) |
| FMT-BREAK-018 | clarified (Why) |
| SHARED-FIX-003 | clarified (pointer) |
| SHARED-ORDER-008 | clarified (gap reason) |

The ledger has the five new IDs at their sorted positions.

### Visible output changes beyond the answers

- **Inline comment behind the operator** (`a ? /* c */ 1`): refused and no longer fixed (#1274 above). This is within the answer's wording, but the lead should check it against the question as asked.
- **Every fix in the document is withheld** (SHARED-FIX-012), not only the fix nearest the carrier. This is a rare-class withholding scope and the lead's to decide.
- **Rename fixes are withheld when another document they change holds a carrier.** The same kind of decision as the previous item.
- **A document with both a hiding carrier and an unsupported shape is not reported.** This only affects files that hold both rare shapes.
- **RH5308-006** (#1673): no fix for a conditional in an assembly or module attribute, where today's fix changes nothing. SHARED-FIX-005 already decides it.

None needs a maintainer answer in my judgment. The first item is the only one that could; I recommend keeping it, since it follows the answer's own sentence.

### Not written (per prompt)

Q6 and Q7 (declare FMT-STRUCT-023 and FMT-STRUCT-003 shapes), and the process follow-up on escalation 9.

### Files

- `sdd/changes/2026-11/fix-offering-safety.md`
- `sdd/specs/formatter/pipeline.md`
- `sdd/specs/formatter/unsupported-syntax.md`
- `sdd/specs/formatter/line-breaks.md`
- `sdd/specs/shared/code-fixes.md`
- `sdd/specs/shared/ordering-move-safety.md`
- `sdd/specs/analyzer/RH5308.md`
- `sdd/specs/requirement-ids.txt`
- `AGENTS.md`
- this handover

No rule page changes. RH5308's normal case is unchanged, and `general-notes.md` covers a withheld fix.
