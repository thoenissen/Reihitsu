Role: spec checker
Commit: fc320af5f30cbdfb3cf5c1f26e66c7ad116ed67a
Verdict: consistent (both issues; four non-blocking notes)

Inputs: `00-lead.md`, `01-spec-author.md`, thoenissen/Reihitsu#1401 (no comments), thoenissen/Reihitsu#1377 with both comments (round 9, P35; round 10, RH5011-004 and RH8304-004), the follow-up issues thoenissen/Reihitsu#951, #953, #956, #958, #959, #960, #976 as they stand now, `RequirementCoverageTests.EnforcedSpecPaths` on this tree, SHARED-FIX-008, FMT-PIPE-007, and `git diff origin/main HEAD -- . ':(exclude)sdd/work'` (15 files, 28 insertions, 28 deletions). The prompt carried no intended fix or verdict beyond the maintainer's constraints, which `00-lead.md` also records.

## thoenissen/Reihitsu#1401 - consistent

- `sdd/README.md`: the five `RequirementAttribute` declarations (Analyzer.Test, Cli.Test, Core.Test, Formatter.Test, Playground.Test) match `find src -name RequirementAttribute.cs`. "Enforced today" equals `EnforcedSpecPaths` (`analyzer/`, `cli/`, `formatter/`, `playground/`, `shared/code-fixes.md`, `shared/diagnostics.md`). Nothing of thoenissen/Reihitsu#998 is anticipated: the other bullets and *Checks* are untouched.
- Non-goal pointers, each against the follow-up issue as it is now (all seven are open, none has added a requirement yet, so keeping every non-goal with a pointer is correct):
  - #951 is a bug (labels `bug`, `formatter`); raw-string holes, nested raw strings, and tab-indented raw strings are all in it. `indentation.md` and `raw-string-alignment.md` state it correctly.
  - #953 is a feature, "indent continuation lines one level deeper"; `indentation.md` states a decided behavior change.
  - #956 covers one switch-expression arm per line and argument lists like parameter lists; both `line-breaks.md` sentences are correct.
  - #958 covers lambda arrow, conditional, named-argument colon, `::`, element access and invocation `(`, and initializer braces. The `horizontal-spacing.md` pointer names only the pairs that are in the non-goal and does not claim `case` colons, which #958 does not cover. Correct.
  - #959 (comment/doc-line trailing whitespace; line terminators in block comments and disabled text) and #960 (directive lines without message text; behind the last token) are split correctly in `cleanup.md`, and `line-endings.md` points only to #959.
  - #976 is a feature; `playground.md` is right to include it, although the issue says "formatter specs".
- No "open question", "undecided", or "not specified yet" wording is left in `sdd/specs/formatter/` or `sdd/specs/playground/` (grep).
- No requirement heading, ID, normative sentence, or `Why:` was touched in these specs; tracker numbers appear only in spec Markdown, not in source, so `SelfReferentialTrackerReferenceTests` is unaffected.

## thoenissen/Reihitsu#1377 - consistent

- All nine requirements (the seven listed, plus RH5011-004 and RH8304-004 from the round-10 note) now cite SHARED-FIX-008. The seven listed ones replace the restated clause with "the line ending SHARED-FIX-008 specifies" and keep their other wording; none restates the rule any more. Each of the 11 affected specs (RH5011, RH8303, RH8304, and the four shared specs, counting attribute-targets once) lists SHARED-FIX-008 under `Uses`. IDs, headings, `Why:`, examples, and `requirement-ids.txt` are unchanged.
- SHARED-ATTR-008 and -017: replacing "detected as FMT-PIPE-007 specifies for the formatter" with SHARED-FIX-008 is right per the round-9 comment (P35) and SHARED-FIX-008's first sentence; no open question is needed. FMT-PIPE-007 was removed from the `Uses` of `attribute-targets.md` because nothing else in it references FMT-PIPE-007 (grep), which `SpecStructureTests` requires.
- RH5011-004 and RH8304-004 keep their own line-break rule and gain only one true-under-both-texts sentence naming SHARED-FIX-008 for all-CRLF or all-LF documents. That is a citation and changes no output, as the round-10 note asks; the lone-`\r` contradiction stays for the code fix in thoenissen/Reihitsu#1388.
- No requirement changes meaning; no requirement is added, retired, moved, or renumbered.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | None blocking. | - |

## Notes (non-blocking, for the lead)
1. The spec author's "Open questions to file" entry (RH5011-004 / RH8304-004 against SHARED-FIX-008 in a lone-`\r` document) is already decided and tracked: the round-10 comment on #1377 says the contradiction is real, is decided by P35, and "lands with the code fix in #1388". Filing it in thoenissen/Reihitsu#1408 would duplicate that, and #1408 concerns brace-fix layout. I recommend not filing it; the PR description can mention the two kept clauses and #1388 instead.
2. Requirements outside both lists restate the rule the same way and stay as they are: RH5002-005, RH5020-005 ("the line ending the document already uses"), RH5030-003 (copy of SHARED-AFTERBRACE-001's clause), and RH8402-005 (differs: "the line ending of the file's first line, or `\n`"). Candidates for a follow-up, not part of this run's lists.
3. `cli/reihitsu-format.md` and `shared/ordering-move-safety.md` may still call a decided question open (the author did not check); out of scope for #1401, which names formatter specs.
4. `sdd/work/spec-readme-line-ending-citations/` is the only `sdd/work/` content in the diff; the lead deletes it before publishing.

## Checked
1. Change doc: ok - none required (text-only; no requirement added, changed in meaning, or retired; no rule, phase, or surface added or removed).
2. Specs match the diff: ok - nothing the diff changes alters behavior; the added RH5011-004 and RH8304-004 sentences are true under both texts.
3. IDs: ok - `requirement-ids.txt` untouched; no ID added, retired, moved, or renumbered.
4. Code owners: ok - no header owner touched; `scripts/test.sh --project architecture` below includes `SpecConsistencyTests`.
5. User documentation: ok - no rule page states a line ending or these non-goals; none changed. AGENTS.md untouched (`git diff` shows no file outside `sdd/`).
6. Open questions: ok - none to file (note 1).
7. Coverage and consistency: ok - `scripts/test.sh --project architecture` at fc320af: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72` (includes `RequirementCoverageTests`, `SpecConsistencyTests`, `SpecStructureTests`). No new active requirement, so no new covering test is needed and the docs lane holds.
8. Nothing left behind: ok (note 4).
