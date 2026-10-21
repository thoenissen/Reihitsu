# Overlap check: #939 and the principle spec issues #1138, #1139, #1161, #1260, #1331 (round 8)

Read 2026-10-21 against `origin/main` at 8e0f35d (#1301/#1302/#1303 merged, so #985/#993/#995 are done). Sources: bodies and comments of #939, #998, #1138, #1139, #1161, #1260, #1331; `squad/README.md` (Lanes); `sdd/README.md`; `sdd/specs/` and `RequirementCoverageTests.EnforcedSpecPaths` on main.

## State on main that matters

- `EnforcedSpecPaths` = `analyzer/`, `cli/`, `shared/code-fixes.md`, `shared/diagnostics.md`, plus every shared requirement an enforced spec lists under `Uses`.
- Coverage today: every `shared/` spec 0 uncovered, `cli/` 0/18 uncovered; **all 13 `formatter/` specs fully uncovered (202 requirements)** and `playground/playground.md` 7/7 uncovered. `RequirementAttribute` exists in Analyzer.Test, Cli.Test, Core.Test; missing in Formatter.Test and Playground.Test.
- Last IDs: SHARED-DIAG-003, SHARED-FIX-005, SHARED-COMMENT-010, SHARED-DOC-016, SHARED-ORDER-007, SHARED-TYPEREGION-006, **SHARED-USING-010** (SHARED-USING-007 already exists: "Alphabetical order within a group, stable on ties").
- `requirement-ids.txt` is sorted: two runs adding IDs with the same prefix touch the same hunk (textual conflict, resolved on rebase); different prefixes merge cleanly.
- Lane check: `AGENTS.md` classifies as **tooling**; `sdd/` and Markdown elsewhere is docs. A docs-lane diff cannot add a requirement to an enforced spec, so every issue below that adds a requirement runs in the **specs-and-tests** lane, although its body says "docs/spec lane".

## Per candidate

### #939 Tag formatter, shared, CLI, Playground tests
- Lane: specs and tests for existing behavior (tests only; no spec text except filing open questions as sub-issues of #912).
- Touches: `Reihitsu.Formatter.Test/**` (202 tags + focused tests where none exists), `Reihitsu.Playground.Test/**` (7), new `RequirementAttribute.cs` in both projects; optionally `RequirementCoverageTests.EnforcedSpecPaths` (adding `formatter/`, `playground/`, `shared/`). Shared and CLI are already covered, so in practice it is a formatter + Playground run.
- Blockers: none. Size: largest of the set (13 formatter specs).
- Note: amendments of shared text by #1138/#1260/#1331 keep IDs, so tags stay valid.

### #1138 P9/P10, P14a-d, P23, P24 -> `shared/code-fixes.md`
- Lane: specs and tests (code-fixes.md is enforced; new SHARED-FIX-006/007/008 need covering tests that pass today).
- Touches: `sdd/specs/shared/code-fixes.md` (new FIX-006, -007, -008; FIX-002 clarification; FIX-003 P23 sentence and RH3202 example replacement), `sdd/specs/formatter/pipeline.md` (FMT-PIPE-010 `Why:` references SHARED-FIX-006), `requirement-ids.txt` (SHARED-FIX hunk), tags in `Reihitsu.Analyzer.Test/**` fix tests (existing tests that already meet the rule).
- Blockers: none. Blocks: #1331 (P26), #1263, #1269, #1283, #1284, #1140, #1144.

### #1139 P12/P13 -> line-leading comments, comment kinds
- Lane: specs and tests.
- Touches: `sdd/specs/shared/comment-placement.md` (SHARED-COMMENT-001 + P6 wording for P13), the P12 home ("e.g. a new SHARED-DIAG-00x in diagnostics.md", spec author's choice), `Uses` lines of the rule specs that judge line starts (RH5110, RH5203, RH5206, RH5204 and further RH51xx/RH52xx layout specs under `sdd/specs/analyzer/`), `requirement-ids.txt`, tags in `Reihitsu.Analyzer.Test/Formatting/**`.
- Blockers: none.
- **ID collision risk:** if P12 goes into `diagnostics.md`, it competes with #1260 (SHARED-DIAG-004...008) and #1331 (SHARED-DIAG-009, plus P31). Avoid: home P12 in `comment-placement.md` as **SHARED-COMMENT-011** (it is a comment rule), or have the coordinator pre-assign SHARED-DIAG-011.

### #1161 P11 -> formatter may do more than the analyzers
- Lane: docs if the wording goes only into `sdd/README.md` (*Open questions*); tooling if it touches `AGENTS.md`. Recommend `sdd/README.md` only (docs lane, smallest squad, no tests).
- Touches: `sdd/README.md`; optionally `AGENTS.md` (*Issue intake*).
- Blockers: none. Soft link: #1331 places P31 "next to P11, as its mirror" in `diagnostics.md`; it only needs to know where P11 lives.

### #1260 P15-P22, alias/Razor extensions
- Lane: specs and tests (diagnostics.md enforced; DOC/TYPEREGION/USING enforced via `Uses`).
- Touches: `shared/diagnostics.md` (new DIAG-004...008, DIAG-002 and -003 amended, DIAG-006 addition from the round-8 comment: one diagnostic per declarator at its identifier), `shared/type-regions.md` (TYPEREGION-002/-006), `shared/using-directive-ordering.md` (P18, item 18 on USING-004), `shared/element-documentation.md` (new DOC-017), `requirement-ids.txt` (DIAG, DOC, maybe USING hunks), tags in `Reihitsu.Analyzer.Test/**`.
- Blockers: none.
- **ID collision on main:** the body names "SHARED-USING-007 (P18)", but SHARED-USING-007 already exists with another meaning. The P18 text must either amend USING-007 (it is the within-group comparison rule) or take **SHARED-USING-011**. Lead prompt must say so. Also covering tests: several of these requirements describe intended behavior that today's code violates (bugs follow); the coverage sentence leaves those shapes out, as #1139 does.

### #1331 P25-P31, P18/P22 clarifications
- Lane: specs and tests.
- Touches: `shared/diagnostics.md` (new DIAG-009 P25, DIAG-004 amended P28, P31 new requirement, DIAG-008 clarified P22), `shared/code-fixes.md` (FIX-007 narrowed by P26), `shared/ordering-move-safety.md` (ORDER-004 P27), `shared/element-documentation.md` (DOC-004 P29, DOC-008 P30), `shared/using-directive-ordering.md` (P18 text written by #1260), `requirement-ids.txt`, Analyzer.Test tags.
- Blockers stated: #1260 (DIAG-008, USING-007/P18), #1138 (FIX-007). "Other principles can be written first", but every file it edits except `ordering-move-safety.md` is also edited by #1260 or #1138, so a split run buys nothing.

### #998 opt-out switch
Runs last: depends on every analyzer batch and #939 merged. Not a candidate now.

## Pairwise file overlaps

| Pair | Shared files | Verdict |
|---|---|---|
| #939 - any spec issue | `requirement-ids.txt` no (#939 mints no ID); `formatter/pipeline.md` with #1138 (text edit vs tags only); Analyzer.Test no (#939 is Formatter/Playground) | free |
| #1138 - #1260 | none (code-fixes.md/pipeline.md vs diagnostics/type-regions/using/element-doc); different ID prefixes; #1260 cites SHARED-FIX-003 only by reference while #1138 amends its text | free |
| #1138 - #1139 | none (unless #1139 chose code-fixes.md, it will not) | free |
| #1138 - #1161 | none | free |
| #1139 - #1260 | `shared/diagnostics.md` + SHARED-DIAG IDs **only if** P12 lands there; otherwise none. Both may tag the same Analyzer.Test files (attribute lines only) | free if P12 -> SHARED-COMMENT-011 (or pre-assigned DIAG-011) |
| #1139 - #1161 | none | free |
| #1260 - #1161 | none | free |
| #1331 - #1260 | diagnostics.md (DIAG-004, -008), element-documentation.md, using-directive-ordering.md, SHARED-DIAG IDs | must wait (stated blocker) |
| #1331 - #1138 | code-fixes.md (FIX-007) | must wait (stated blocker) |
| #1331 - #1139 | diagnostics.md/SHARED-DIAG IDs only if P12 lands there; SHARED-COMMENT-001 vs DOC-004 (P29) are related concepts but different files | low |
| #1331 - #1161 | none in files; P31 text refers to P11's location | prefer #1161 first |
| #998 - all | `RequirementCoverageTests.cs`, `sdd/README.md` (*Tests and requirements*, *Checks*) shared with #939 / #1161 | after everything |

## Requirement-ID plan (to put in the lead prompts)

- #1260: SHARED-DIAG-004...008, SHARED-DOC-017, P18 as an amendment of SHARED-USING-007 or as SHARED-USING-011 (not a new "007"); item 18 amends SHARED-USING-004.
- #1138: SHARED-FIX-006, -007, -008.
- #1139: P12 as SHARED-COMMENT-011 (preferred) or SHARED-DIAG-011; no other new ID.
- #1331 (later): SHARED-DIAG-009 (P25), SHARED-DIAG-010 (P31); no new FIX/ORDER/DOC IDs (amendments only).
- #939, #1161: no new IDs.

## Recommendation

Start now, in parallel (5 squads, one slot spare):
1. **#939** — tests lane; Formatter.Test + Playground.Test only; mints no ID, edits no spec text. Largest run; optionally split it into two squads by formatter spec (e.g. pipeline/structural/line-breaks/indentation vs the rest + Playground) using the spare slot, provided one of them alone creates `RequirementAttribute.cs` in Formatter.Test and only one touches `EnforcedSpecPaths`.
2. **#1138** — specs-and-tests lane; code-fixes.md + FMT-PIPE-010 `Why:` only.
3. **#1260** — specs-and-tests lane; resolve the SHARED-USING-007 collision as above.
4. **#1139** — specs-and-tests lane; P12 in comment-placement.md (SHARED-COMMENT-011) so it stays out of diagnostics.md; if the spec author insists on diagnostics.md, use the pre-assigned SHARED-DIAG-011 and expect a `requirement-ids.txt` rebase against #1260.
5. **#1161** — docs lane; `sdd/README.md` only (touching `AGENTS.md` turns it into tooling lane).

Wait:
- **#1331** — after #1260 and #1138 merge (stated blockers; it edits the same four shared files), preferably after #1161 too (P31 is written as P11's mirror). Starting its independent parts early (P27 in ordering-move-safety.md only) is possible but saves little.
- **#998** — last, after #939, #1331 and every analyzer batch.
