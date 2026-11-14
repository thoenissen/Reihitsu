Role: spec author
Commit: 1276fd8
Verdict: change doc written; spec written

My prompt carried the maintainer's batch-2 answers and the lead's decisions 11–13, which are inputs to this stage. It carried no intended fix, suspected cause, or hoped-for verdict beyond them, so nothing was ignored.

I agree with decisions 11, 12, and 13 and carried out all of them. I disagree with none. The change doc `sdd/changes/2026-11/fix-offering-safety.md` stays `Status: Draft` for the lead to accept.

`scripts/test.sh --project architecture`: 116 passed. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which names only the run's new requirements: RH5308-005, RH5308-006, CLI-021, FMT-PIPE-033, FMT-UNSUP-008, PLAY-009, SHARED-FIX-011, and SHARED-FIX-012. The test implementer's tags resolve it.

I re-ran the formatter, through `scripts/format.sh`, on the "not affected" example and on the batch-2 inline input. Both outputs are observed:
- The "not affected" example gets a blank line in front of `// c`.
- The inline input is kept as written.

## thoenissen/Reihitsu#1600

### Changed requirements

- CLI-021 — Files whose text another build compiles as code are left alone — added (Q8 B).
  - The CLI leaves the file unchanged in every mode, also with `--utf8-bom`.
  - It decides by FMT-PIPE-033's definition.
  - It counts the file in the total, not as changed, and not as needing formatting in `--check` or `--dry-run`.
  - The summary names it in `Skipped <n> file(s) with code hidden in another build configuration.`
  - `--verbose` prints `Skipped (code hidden in another build configuration): <path>`.
  - The file does not affect the exit code.
  - Precedence: a file with syntax errors, or a generated file, is skipped as such. A file that holds both a carrier and an unsupported shape is skipped under CLI-021, with no `error:` line.
- CLI-014 — the summary list gains the new line, after the syntax-error line — changed.
- CLI-015 — the `--verbose` list gains the new line — changed.
- PLAY-009 — Input whose text another build compiles as code is shown unchanged with a message — added (Q8 B).
  - The message is "The input contains code hidden in another build configuration and was left unchanged."
  - The Playground decides by FMT-PIPE-033's definition.
  - Input with a syntax error shows PLAY-006's message instead.
  - Input that also holds an unsupported shape shows this message, not PLAY-008's.
- FMT-PIPE-033 — changed:
  - The paragraph on the node-level entry points now says the document is "not reported as unsupported", and that the CLI and the Playground name it as skipped (CLI-021, PLAY-009).
  - The example's lead-in is changed the same way.
  - Why (decision 12): the overclaim is narrowed. Commented-out code and templates stay formattable "wherever they stand outside every conditional group or hold no such line".
  - Why (decision 12): a new sentence names the over-refusal as decided: a carrier whose `#elif`, `#else`, or `#endif` lines close only groups it opens itself still counts, because telling it apart means pairing directives inside the text.
  - Why (Q8 B): "left alone silently, as a file with a syntax error is" is replaced. The formatter returns the file as it returns a file with a syntax error. The CLI and the Playground name it as skipped.
- FMT-PIPE-003 — Why: "is decided by FMT-PIPE-033, and so is the case it leaves open" — clarified (decision 13).
- SHARED-FIX-012 — Why: the "one check made once for every fix" sentence now reads "a check of the diagnostic's document made once, before any fix registers … a fix that changes other documents checks those when it computes them, by the same definition" — clarified (decision 13).
- `Uses` updated:
  - `reihitsu-format.md`: +FMT-PIPE-033, +FMT-UNSUP-002.
  - `playground.md`: +FMT-PIPE-033, +FMT-UNSUP-002.
  - `pipeline.md`: +CLI-021, +PLAY-009.
- Ledger: CLI-021 and PLAY-009 inserted at their sorted positions.

Change doc:
- *What*: the CLI/Playground sentence, and the "one check" wording (decision 13).
- *Why*: the over-refusal sentence and the CLI/Playground sentence (decision 12, Q8).
- #1 *after*: the CLI before and after (`--check --verbose`, exit code 1 → 0), the write-mode and `--utf8-bom` notes, and the Playground before and after.
- #1: a new example, the balanced-directive carrier (the skeptic's commented-out `#if TRACE … #endif` in `#if DEBUG`), which is left alone.
- #2: one line saying such a carrier also withholds every fix.
- *Decisions*:
  - The heading "… alone silently" becomes "… alone".
  - A new decision, "The CLI and the Playground name the skipped file" (Q8 B).
  - The "one check" decision is retitled and reworded.
  - The over-refusal is added to "The lines that count".
- *Alternatives*: Q8 A, and pairing directives inside the carrier.

### Examples for the tests

- CLI-021:
  - The `C.cs` example with `--check --verbose`: the exact output, exit code 0.
  - Without `--verbose`: no per-file line.
  - Write mode: the file is not written, also with `--utf8-bom`.
  - Together with an unformatted file: the exit code follows that file.
  - A file holding both a carrier and FMT-UNSUP-004's `int /* c */ a = 1;`: the skip line, no `error:` line, and exit code 0.
- PLAY-009:
  - The example input, with the message.
  - The message disappears on the next successful format.
  - Input holding a carrier and an unsupported shape shows PLAY-009's message.
- FMT-PIPE-033: as in 03.
  - Add the two balanced-directive inputs from 04-skeptic.md item 1. Both are left unchanged, which pins the decided over-refusal.
  - Carrier kinds to cover (04-skeptic.md item 2):
    - `@"…"u8`;
    - `"""…"""u8`;
    - an interpolated verbatim string whose `#else` line comes after a hole.
  - Not carriers, as controls (04-skeptic.md item 3):
    - a `///` comment;
    - a ` * #else` line in a `/* */` comment;
    - a `#else` on the carrier's first line.
- SHARED-FIX-012: as in 03, plus the coverage notes from decision 13, which are inputs for the test implementer and the implementer:
  - **Casing Fix All path.** `CasingCodeFixProviderBase`'s `CasingFixAllProvider` decides each candidate with `CanRegisterCodeFix` and does not go through `RegisterCodeFixesAsync`. A base check in `RegisterCodeFixesAsync` therefore does not reach it.
    - Test: a solution-scope casing Fix All started from a document without a carrier, where a second document holds a carrier and a reported name.
  - **RH2001.** `Renamer.RenameSymbolAsync` reaches a `partial` part in another document.
    - Test: an RH2001 property whose type has a `partial` part, in a second document, that holds a carrier.
  - **Cost.** The base check reads the whole document each time a fix registers, and the BatchFixer registers once per diagnostic. A per-tree cache, like `SyntaxTreeErrorVerdictCache`, keeps this linear. This is an implementation note, not a spec point.

### Output questions

_None._

### Internal choices

- **Two new requirements (CLI-021, PLAY-009), not edits of CLI-008 and PLAY-006.** CLI-019 and PLAY-008 set the precedent: one requirement per kind of skip. The alternative is to widen CLI-008 and PLAY-006 to "syntax errors or hidden code", which would mix two reasons in one requirement.
- **The CLI and the Playground decide by FMT-PIPE-033's definition, not their own** (FC-015).
  - The formatter returns `Unsupported` null for such a file, so neither host can tell it from an unchanged result.
  - The implementer exposes the one predicate where both reach it, as `ReihitsuFormatterHelpers.HasSyntaxErrors` is exposed today.
  - The alternative is a third result outcome on the formatter's result, which changes FMT-UNSUP's result contract.
- **Check order in the CLI: syntax errors, then generated, then hidden code, then formatting.** This follows the code's existing order and FMT-PIPE-033's "applies before the precheck".

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1150

No change in this stage. SHARED-FIX-012's Why edit does not touch SHARED-FIX-011.

## thoenissen/Reihitsu#1274

### Changed requirements

- SHARED-DIAG-010 — changed (decision 11).
  - New sentence: a shape the formatter refuses as unsupported (FMT-UNSUP-001) is not a shape it keeps. The formatter asks the user to rewrite it, so the rule reports it as its own requirements state, and its fix is withheld (SHARED-FIX-010).
  - New example: RH5308 reports the conditional with `// c` behind `?`, as in RH5308-005.
  - `Uses` gains FMT-UNSUP-001, FMT-UNSUP-008, RH5308-005, and SHARED-FIX-010.
- FMT-UNSUP-008 unchanged (Q9 A).

Change doc:
- *What*: says the inline case is refused, and names the CLI's exit code 2 (CLI-017, CLI-019) and the SHARED-DIAG-010 sentence.
- #4, first input: the *after* names exit code 2 in every mode, and the `warning:` form with `--suppress-unsupported`.
- #4, new example (Q9 A): the skeptic's inline input (`a ? /* yes */ 1 : /* no */ 2`, and the laid-out `? /* yes */ 1`).
  - Before: formatted as written, and RH5308 reports nothing.
  - After: unchanged, `error: <path>(5,21): …`, exit code 2, and fixes withheld.
- #4, `var z` example: retitled, and its *after* names exit code 2.
- #4, "not affected" example: now input plus observed output, with FMT-BLANK-011's blank line.
- *Decisions*:
  - the Q4 decision names the SHARED-DIAG-010 sentence;
  - a new paragraph records Q9 A.
- *Special cases, Checked and kept*: `DocumentationCommentFormattingPhase`'s relocation of a `/** */` comment, which writes the line break into the owning token's leading trivia. It names the conditional's operator placement as a reader, but it serves every operator, so it is kept.
- *Alternatives*: Q9 B.

### Examples for the tests

- SHARED-DIAG-010's new example: tag it with RH5308-005's test, reported and no fix.
- FMT-UNSUP-008: as in 03, plus the following.
  - The inline input above: refused, location (5,21), and the CLI exits with code 2.
  - Not refused (04-skeptic.md):
    - the nullable `?` (`int? /* a */ b`);
    - `?.` and `?[]`;
    - `a ? ref /* c */ x : ref y`.
  - Refused (04-skeptic.md): `$"{(a ? /* c */ 1 : 2)}"`.
- Existing tests whose input holds the declared shape. The test implementer rewrites each to the refusal or to a withheld fix (04-skeptic.md):
  - `DelimitedDocumentationInExpressionTests`: the `?`/`:` cases;
  - `RH3202…AnalyzerTests.VerifyFixMovesDelimitedDocumentationBehind{Question,Colon}…`;
  - `CommentJoinTests.Ternary{Question,Colon}…`;
  - `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin`: its `return b ? // r` part;
  - `SameLineGapRulePhaseTests.DocumentationCommentsAroundTheQuestionMarkKeepTheirPlace`: if it runs through an entry point.
- RH5308-005's other-side example, with the comment above `?`: the expected output carries FMT-BLANK-011's blank line. Use the observed output.
- `N(/* c7 */ 2)` comes out as `N(/* c7 */2)`. Use the observed output; this is a follow-up already in `00-lead.md`.

### Output questions

_None._

### Internal choices

- **The SHARED-DIAG-010 sentence comes after "SHARED-JOIN-001 is one instance", and the example is new.** A sentence without an example would leave the spec checker nothing to tie RH5308-005 to. The alternative is the sentence alone.

### Follow-up candidates

_None new._ The RH5302 twin (Q10 A), the FMT-BLANK-011 blank line, and the `/* c7 */2` spacing are already in `00-lead.md`. Per my prompt, I did not write Q10.

## thoenissen/Reihitsu#1143

No change.

## thoenissen/Reihitsu#1673

No change.

## thoenissen/Reihitsu#1678

### Changed requirements

- SHARED-ORDER-008 — "and a move is judged in that build" becomes "and this gap leaves such a move to be judged in that build" — clarified (decision 13). Wording only; nothing visible changes.

## Whole run

### Changed requirements in this stage

| ID | Kind |
|---|---|
| CLI-021 | added |
| PLAY-009 | added |
| CLI-014 | changed |
| CLI-015 | changed |
| SHARED-DIAG-010 | changed (sentence and example) |
| FMT-PIPE-033 | changed (CLI/Playground pointer, Why) |
| FMT-PIPE-003 | clarified (Why) |
| SHARED-FIX-012 | clarified (Why) |
| SHARED-ORDER-008 | clarified (wording) |

### Visible output my text adds beyond the answers

Q8 B gave only the `--verbose` line. The rest is mine, written to follow the syntax-error skip, as the answer says:
1. **Summary line text**: `Skipped <n> file(s) with code hidden in another build configuration.` It is printed after the syntax-error line and before the unsupported-syntax line.
2. **Playground message text**: "The input contains code hidden in another build configuration and was left unchanged."
3. **The `--verbose` line only with `--verbose`**, as for a syntax error. The summary line is always printed when the count is non-zero.
4. **Exit code unaffected.** In `--check` and `--dry-run`, such a file used to be "Not formatted" and gave exit code 1. Now it gives 0 unless another file needs formatting.
5. **`--utf8-bom` no longer rewrites the encoding of such a file.** 03 had said CLI-010 applied to it. As a skipped file, it is now left untouched, as a syntax-error file is.
6. **Precedence**: a file holding both a carrier and an unsupported shape gets the skip line, no `error:` line, and no exit code 2. The Playground shows PLAY-009's message for it, not PLAY-008's. This follows FMT-PIPE-033 applying before the precheck, as 03 decided and decision 9 accepted.
7. **The balanced-directive carrier is shown as left alone** in change doc #1 and #2. The output was already decided by decision 12. Only its listing is new.

Items 1–3 and 5 are, in my judgment, within "like the syntax-error skip". Item 4 is too. The lead may still want to confirm it, because it turns a `--check` exit code 1 into 0 for such a file.

### Not written (per prompt)

Q10 (the RH5302 twin) stays a follow-up.

### Files

- `/home/user/Reihitsu/sdd/changes/2026-11/fix-offering-safety.md`
- `/home/user/Reihitsu/sdd/specs/cli/reihitsu-format.md`
- `/home/user/Reihitsu/sdd/specs/playground/playground.md`
- `/home/user/Reihitsu/sdd/specs/formatter/pipeline.md`
- `/home/user/Reihitsu/sdd/specs/shared/code-fixes.md`
- `/home/user/Reihitsu/sdd/specs/shared/diagnostics.md`
- `/home/user/Reihitsu/sdd/specs/shared/ordering-move-safety.md`
- `/home/user/Reihitsu/sdd/specs/requirement-ids.txt`
- this handover

No rule page and no `AGENTS.md` change. RH5308's normal case is unchanged, and the `AGENTS.md` key-convention bullet already names FMT-PIPE-033 at formatter level.
