Role: spec author
Commit: 9994b09
Verdict: #1401: text written; #1377: text written

Inputs: thoenissen/Reihitsu#1401 (no comments), thoenissen/Reihitsu#1377 and its round-9 comment, the follow-up issues thoenissen/Reihitsu#951, #953, #956, #958, #959, #960, #976 and their comments, `00-lead.md`, `sdd/specs/shared/code-fixes.md` (SHARED-FIX-008), `sdd/specs/formatter/pipeline.md` (FMT-PIPE-007). The prompt carried no intended fix or verdict beyond the maintainer's constraints, which `00-lead.md` also records.

## thoenissen/Reihitsu#1401

### Changed requirements

None. Only the rulebook and non-goals changed; no requirement heading, ID, or normative text was touched.

- `sdd/README.md`, *Tests and requirements*:
  - the attribute is now listed as declared in `Reihitsu.Analyzer.Test`, `Reihitsu.Cli.Test`, `Reihitsu.Core.Test`, `Reihitsu.Formatter.Test`, and `Reihitsu.Playground.Test`. Established by `rg -l "class RequirementAttribute" src`: five declarations, one in each of those projects;
  - "Enforced today" now reads `analyzer/`, `cli/`, `formatter/`, `playground/`, `shared/code-fixes.md`, `shared/diagnostics.md`, plus used `shared/` requirements, which matches `RequirementCoverageTests.EnforcedSpecPaths` on this commit. The rest of the section and *Checks* is unchanged; nothing from thoenissen/Reihitsu#998 was anticipated.
- Non-goals that still called a decided question open now point to its follow-up issue. Each follow-up is still open and adds no requirement yet, so every non-goal stays, with a pointer, and none was dropped:

| Spec | Non-goal | Pointer |
|---|---|---|
| `formatter/indentation.md` | lines inside a multi-line interpolation hole ("see the open questions") | thoenissen/Reihitsu#951, decided as a bug |
| `formatter/indentation.md` | continuation lines no requirement owns ("open question") | thoenissen/Reihitsu#953, behavior change |
| `formatter/raw-string-alignment.md` | multi-line interpolation holes and nested raw strings; tab-indented raw strings | thoenissen/Reihitsu#951, decided as a bug |
| `formatter/line-breaks.md` | switch-expression arms keep the author's line breaks; comma and `)` position in argument lists | thoenissen/Reihitsu#956, behavior change |
| `formatter/horizontal-spacing.md` | token pairs no rule covers | thoenissen/Reihitsu#958, behavior change |
| `formatter/cleanup.md` | trailing whitespace in comments, documentation lines, and directive lines, and behind the last token ("undecided") | thoenissen/Reihitsu#959 (comment lines, behavior change) and thoenissen/Reihitsu#960 (directive lines without message text, last token; bug) |
| `formatter/line-endings.md` | line breaks inside block comments and disabled text ("undecided") | thoenissen/Reihitsu#959, behavior change |
| `playground/playground.md` | no message for generated input ("open question") | thoenissen/Reihitsu#976, behavior change |

The issue says "formatter specs", but thoenissen/Reihitsu#976 is the Playground spec's question, so `playground/playground.md` is included too. The kind of each follow-up (bug or behavior change) comes from that issue's labels and its "Squad lane" line. A pointer is written as `thoenissen/Reihitsu#<N>`; no spec cited an issue before this, so there was no house style to follow. `SelfReferentialTrackerReferenceTests` scans C# only.

### Examples for the tests

None. No requirement was added or changed.

### Open questions to file

_None._

## thoenissen/Reihitsu#1377

### Changed requirements

Each spec now lists SHARED-FIX-008 under `Uses`. No ID, heading, `Why:`, or example changed.

- SHARED-STMT-004 — The fix inserts one blank line above the statement and its comments — clarified (citation): "the line ending the document already uses" becomes "the line ending SHARED-FIX-008 specifies".
- SHARED-AFTERBRACE-001 — The blank line goes directly before the next statement — clarified (citation), same replacement.
- SHARED-REGION-002 — The fix inserts one empty line on the reported side — clarified (citation), same replacement.
- RH5011-005 — A statement on the `break` line is moved below a blank line — clarified (citation), same replacement.
- RH8303-005 — The fix inserts one empty line above the comment — clarified (citation): "the line ending the document uses" becomes "the line ending SHARED-FIX-008 specifies".
- SHARED-ATTR-008 — The separate-line fix breaks the line after the list — clarified (citation): "the line ending detected as FMT-PIPE-007 specifies for the formatter" becomes "the line ending SHARED-FIX-008 specifies". SHARED-FIX-008's first sentence is "the line ending FMT-PIPE-007 detects", and the round-9 comment on thoenissen/Reihitsu#1377 (P35) says the two do not differ for any input. FMT-PIPE-007 had no other reference in `shared/attribute-targets.md`, so it was removed from that spec's `Uses` (SpecStructureTests requires `Uses` to list exactly the referenced IDs).
- SHARED-ATTR-017 — The split fix gives each attribute a list on its own line — clarified (citation), same as SHARED-ATTR-008.
- RH5011-004 — The fix inserts a blank line below the `break` line — clarified (citation added, own rule kept), see below.
- RH8304-004 — The fix moves the element onto a new `///` line — clarified (citation added, own rule kept), see below.

The seven requirements in the issue body said only what SHARED-FIX-008 says, and the round-9 comment settles SHARED-ATTR-008 and SHARED-ATTR-017. Each of these seven now cites SHARED-FIX-008 and no longer restates it.

**What RH5011-004 and RH8304-004 say that SHARED-FIX-008 does not (kept, as instructed).** Both requirements copy the line break of one particular line: RH5011-004 "ending with the same line break as that line" (the `break` line), and RH8304-004 "the line break of the line the element starts on". SHARED-FIX-008 writes the line ending FMT-PIPE-007 detects for the whole document. The two agree in a document whose line breaks are all CRLF or all LF, and they differ in two cases:

1. **Mixed line endings.** The requirements use that one line's line break. SHARED-FIX-008's first sentence would give the predominant line ending, but its non-goals exclude documents whose line breaks already mix line endings. The line-specific rule is kept.
2. **All line breaks are a lone `\r`.** The requirements' text gives `\r`. SHARED-FIX-008 (P35) says a fix writes `\n` there. This is a direct contradiction, and it was there before this run: SHARED-FIX-008 already applies to every code fix. Changing the requirements' text to `\n` would change their meaning, which this run must not do, so the text is kept. Each requirement gains one sentence: in a document whose line breaks are all CRLF or all LF, that line break is the line ending SHARED-FIX-008 specifies. That sentence is true under both texts and changes no output. The contradiction is listed below for the maintainer.

Reading the code (not running it): `RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider` (RH5011-004 path) and `DocumentationCommentCodeFixUtilities.GetLineBreak` (RH8304) copy the text of the line's own line break. They fall back to `ReihitsuFormatterHelpers.DetectEndOfLine` only when the line has none. So in a lone-`\r` document they would write `\r`, which matches RH5011-004/RH8304-004 and contradicts SHARED-FIX-008. No test covers either rule in a lone-`\r` or mixed document; the tagged CRLF tests (`SeparateLineFixPreservesCarriageReturnLineFeedAndConverges`, `VerifyFixInsertsTheLineBreakOfTheLineWithCarriageReturnLineFeed`) use uniform documents.

### Examples for the tests

None. The text changes add no behavior; the existing tests tagged with these IDs stay valid.

### Open questions to file

- **RH5011-004 and RH8304-004 vs SHARED-FIX-008 in a lone-`\r` document.** Input: the RH5011-004 example (or the RH8304-004 example) with every line break a lone `\r`. Output by the requirements' text, and by the code as read: the inserted line break is `\r`. SHARED-FIX-008 (P35): `\n`. This looks wrong because two active requirements give different output for the same input. P35 points to the reading in which SHARED-FIX-008 holds and the fixes are bugs; it is not mine to choose. Requirement IDs: RH5011-004, RH8304-004, SHARED-FIX-008. Belongs to `SDD open questions: shared/code-fixes.md` (thoenissen/Reihitsu#1408, open). If the maintainer reads P35 as already deciding this, it is a bug follow-up instead, and its fix would also align the two requirements' text.

## Whole run

- Files changed: `sdd/README.md`; `sdd/specs/formatter/{indentation,raw-string-alignment,line-breaks,horizontal-spacing,cleanup,line-endings}.md`; `sdd/specs/playground/playground.md`; `sdd/specs/shared/{statement-blank-lines,blank-line-after-brace-fixes,region-directive-blank-lines,attribute-targets}.md`; `sdd/specs/analyzer/{RH5011,RH8303,RH8304}.md`. No requirement ID was added, retired, moved, or renumbered; `requirement-ids.txt` is untouched. No rule page under `documentation/rules/` states a line ending or one of these non-goals, so none changed. AGENTS.md was not touched.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.SpecConsistencyTests|FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.SpecStructureTests|FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.RequirementCoverageTests"`: `Passed!  - Failed: 0, Passed: 39, Skipped: 0, Total: 39` (14 + 12 + 13 test methods in the three classes).
- `scripts/lane-check.sh docs --head worktree`: `LANE CHECK: confirmed — claimed docs, diff needs docs`.

### Follow-up candidates (out of scope, not edited)

- Other requirements restate the line-ending rule in the same words but are not in this run's list: RH5002-005 ("the line ending the document already uses", a copy of SHARED-STMT-004), RH5020-005 ("the line ending the document already uses"), RH5030-003 (a copy of SHARED-AFTERBRACE-001's clause). RH8402-005 uses "the line ending of the file's first line, or `\n`", which differs from SHARED-FIX-008 for a mixed or lone-`\r` file.
- Non-goals outside the formatter specs still call a question open that may since have been decided: `cli/reihitsu-format.md` (the `--dry-run` header) and `shared/ordering-move-safety.md` (a balanced `#pragma`/`#nullable` pair). I did not check whether their question issues are decided.
- SHARED-FIX-008's first sentence applies to every document, but its non-goals exclude documents with no line break or with mixed line endings. The round-9 comment treats SHARED-ATTR-008/-017 (FMT-PIPE-007 detection, which also covers those documents) as equal to SHARED-FIX-008, so the two can be read differently for those documents. This is an observation only and does not block this run.
