# Lead — rh2-fix-safety

- Issues (prompt order): thoenissen/Reihitsu#967, #1286, #1287, #1288 — all `agent-filed` (goals are proposals).
- Branch: `claude/eager-ride-45bd9o` (designated by the session; replaces the playbook's `claude/batch-967-...` name). Draft PR: thoenissen/Reihitsu#1609.
- Slug: `rh2-fix-safety`. Lane: **bug fix**.

## Lane reasons

- #967 — RH2005 fix: observed wrong output (`static public` loses indentation; also RH2004 `[Obsolete]` + no-modifier instance in the comment) and a fix offered that breaks the build (references outside the type). Maintainer decided both as bugs (in #930). Bug fix.
- #1286 — RH2001 fix: observed unsafe/invalid output for four inputs (comments dropped; inactive `#if` references; mutable struct property semantics; `readonly readonly`). Decided by P1 / SHARED-FIX-001/-003. Bug fix. A comment adds facts: any inactive branch and inactive declaration of the new name behave the same; the casing fixes' shared check `CasingCodeFixProviderBase<T>.HasExcludedCodeOccurrenceAsync` exists.
- #1287 — partial declarations judged as one declaration (P15, SHARED-DIAG-004, recorded by #1260, closed): RH2002, RH2004, RH2101, RH7002, RH7401–RH7412, RH8306; RH2004 fix on one part of a partial withheld. Triage comment: bug against SHARED-DIAG-004, bug-fix lane, no change doc; the rule's coverage sentence (RH2004-002, which reports both parts) is widened as a clarification. RH7104/RH7105/RH7106 are explicitly NOT in scope (P28).
- #1288 — RH2004 on explicit interface operators/conversions: maintainer round 7b chose option A — not reported (RH2004-003 exemption extends to operators and conversions); the fix-withholding (Q3) is then moot. Ordinary-operator `public` part moved to #1435 (not in this run). Bug fix.

## Roles

Bug-fix lane: spec author (clarifications only: the maintainer-decided requirements for RH2005, RH2001, RH2004, partial-declaration coverage sentences) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer answers

Maintainer: "keep it simple".

- Q1 (partial member, single diagnostic location; RH2002, RH2004, RH8306, RH7401–RH7412): **A** — reported once, on the first part the rule would report on its own; generated parts skipped; the defining part before the implementing part; when no part qualifies by what it writes, the defining part.
- Q2 (RH2001 mutable-struct boundary): **C** (not C′) — fix withheld for a property whose type is a struct not declared `readonly` (option C as asked: "every struct not declared readonly"; the earlier "settable" in this note was the lead's gloss, not the answer) (per the type symbol of the compilation's references); `Nullable<T>` of an enum or `readonly` struct counts as readonly. No special-type exemption: on netstandard2.0/.NET Framework targets `int`/`bool` properties lose the fix — accepted by the maintainer.
- Q4 (RH2001 property typed by a type parameter not constrained to a reference type): **A** — reported, fix withheld (as specified in RH2001-008 and implemented).
- Q5 (RH2005: `public` on its own line behind another modifier, indentation only): **A** — fix offered, the line keeps its place (`private static⏎        int Counter;`).
- Q6 (RH2005: comment/directive/`#if` line in front of a non-first `public`; RH2001: such a line between type and name): **A** — reported, no fix offered.
- Q3 (RH2005 references outside the type body that still compile — `nameof` in the type's own attribute, `cref` from another type): **B** — every reference outside the bodies of the type's declarations withholds the fix; spec text as written.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use session-designated `claude/eager-ride-45bd9o` | The session environment requires pushing only to this branch | Branch names are internal | 560af96 |
| 2 | Lane | Change doc for #1287 despite its body's "behavior change (change doc)" | No change doc; bug-fix lane | Later triage comment on #1287 classifies it as a bug against SHARED-DIAG-004 with a coverage clarification | Behavior is fixed by SHARED-DIAG-004 and maintainer decisions P15 | — |
| 3 | Lane | Spec author in bug-fix lane | Run it | Each issue needs maintainer-decided requirements recorded (clarifications) | Decided by maintainer answers quoted in the issues | fd5393f |
| 4 | Skeptic | #967 case 1 premise | Narrow to declarations with attribute lists on their own line (incl. comment/directive lines between attributes and declaration, both RH2004 and RH2005); the inserted keyword takes over the first token's leading trivia. The `#pragma` → unparsable output is a safety rail repaired in scope | `static public` without attributes is already correct; RH2004-006/RH2005-003 ("column of the first token") decide the output | An unchanged requirement decides the output | — |
| 5 | Skeptic | Architecture finding: excluded-`#if` check private in `CasingCodeFixProviderBase<T>.HasExcludedCodeOccurrenceAsync` | Change the structure in this run: share the check between the casing fixes and the RH2001 fix instead of copying it | A narrow fix would add a second copy (FC-015); both components are changed by the run or are its counterpart | Internal structure; casing-fix outputs unchanged | — |
| 6 | Skeptic | RH2005-004 wording | "any field the declaration declares" | Fix changes every declarator at once; only one safe reading | Wording only | — |
| 7 | Skeptic | RH2001-007 scope | Widen to comments or directives between the name and the end of the declaration, except inside the initializer | SHARED-FIX-003 already withholds these; one place states the class | Both readings withhold the fix | — |
| 8 | Skeptic | #1287 goal "RH2004-002 and its tests are updated" | Replace by RH2004-001 coverage and the characterization test near `RH2004AccessModifierMustBeDeclaredAnalyzerTests.cs:881` | RH2004-002 covers partial types/methods, which stay unreported | Goal wording only | — |
| 10 | Spec author 03 | RH2001: get-only property of a non-readonly struct, and a type parameter not constrained to a reference type | Withhold the fix for both (type-parameter half re-asked as Q4 after spec checker 09) | Literal reading of answer 2C ("every struct not declared readonly"); the type-parameter half goes beyond it; matches the spec author's recommendation and the maintainer's "keep it simple" | Covered by the maintainer's answer to Q2 | 9494bcd |
| 11 | Implementer 05 | Three tests keep fixable markup in fixed code without `MarkupMode.Allow` | Test harness setting is wrong; route to test implementer to add `FixedState`/`BatchFixedState.MarkupHandling = Allow` | Repository convention (e.g. RH8204); asserted outputs unchanged | Harness setting only | 38ab0b9 |
| 12 | Implementer 05 | Scope growth: 15 production files, shared `PartialMemberUtilities` (SHARED-DIAG-012, 5 callers) and `ExcludedCodeUtilities` (casing, RH2001, RH2005) | Accept | One owner per decision instead of copies; all callers are components the run changes | Outputs follow the specs and answers | bd592fd |
| 13 | Implementer 05 | RH7405–RH7408: a part in another file always counts as reported | Accept | Correct whenever every part would be reported; the only-some-parts case is #1334's, unspecified | No specified output differs | bd592fd |
| 14 | Implementer 05 | Architecture finding: generated-code decision in three places (CLI `GeneratedFileUtilities`, formatter `IsAutoGeneratedSource`, new private `PartialMemberUtilities.IsGeneratedCode`) | Fix narrowly, file follow-up | Unifying reaches the CLI and formatter, which the run does not change, and carries a visible CLI question (`*.generated.cs`) | Analyzer copy follows the compiler's own rule (SHARED-DIAG-001); CLI/formatter untouched | — |
| 15 | Spec author 07 | Owning spec of `ExcludedCodeUtilities` | `shared/casing.md` (SHARED-CASING-007) | Its document scope is exactly SHARED-CASING-007's | Spec ownership is internal | 3178c94 |
| 16 | Auditor 10 | Architecture finding: `PartialMemberUtilities.GetReportedPart` predicts which part the analyzer driver analyzes (cause of audit finding 4) | Change the structure in this run: the part that carries a partial member's single diagnostic is decided where every part is visible (once per symbol), not predicted from which syntax-node analyses the driver will run; the diagnostic still goes on the part SHARED-DIAG-012 selects | A narrow fix per member kind would add another prediction; components are ones the run changes | Output is SHARED-DIAG-012 / Q1 as answered | — |
| 17 | Auditor 10 | Architecture finding: RH2001 guard keeps its own list of positions whose trivia the field builder loses (cause of audit finding 2) | Change the structure in this run: the withhold decision follows from what the rewrite actually keeps, so no second list of positions exists; use the shared trivia predicates (audit finding 3) | Adding positions to the list repeats the copy; component is changed by the run | Withholding per RH2001-007 / SHARED-FIX-003 | — |
| 18 | Auditor 10 | Audit finding 1 (comment/directive line dropped when `public` is not first and on its own line; pre-existing) | Repair in this run | Same defect class as #967 in the same helper; SHARED-FIX-003, RH2005-003 decide the output | Unchanged requirements decide it | — |
| 19 | Auditor 10 | Hints: RH2005 `#if` judged by start only; `[GeneratedCode]`-only parts; IDE open-document display of cross-document RH2004 diagnostic | Dropped, not filed | Unverified; no reproduction | — | — |
| 20 | Spec author 12 | Q6's RH2005 half as new RH2005-005 vs. paragraph in RH2005-003; RH2001 half extends RH2001-007 | Accept both | Each withheld case has its own heading; decision 7 keeps the RH2001 class in one place | Spec structure only | 5d0db01 |
| 21 | Spec author 12 | RH2001 name on its own line below the type, no comment: layout | Assert the current output `private int⏎    _value;` only | The RH2001 fix formats its field; SHARED-FIX-007 (formatting the result changes nothing) decides it, unchanged | An unchanged requirement decides it; same output as before the run | — |
| 22 | Test implementer 13 | Two RH2001 tests (comment line in front of the type; comment behind the type on its line) keep the multi-outcome helper | Keep as is; no spec decision added | Only SHARED-FIX-003 constrains them, and the tests assert exactly that; current output keeps the comment, as before the run | The run does not change these outputs | e70fd29 |
| 23 | Implementer 14 | Stand-in rule for RH7409–RH7412: implementing part stands in only when the defining part is generated | Accept | Keeps `main`'s characterization of the only-some-parts case (#1334) and follows SHARED-DIAG-012's fallback order; one place to change | Only-some-parts outputs unchanged from `main` | 2f1fb5a |
| 24 | Implementer 14 | Decision 16 scope: only RH2004 moved to a per-type symbol action; RH2002, RH8306, RH74 bases report from each part's node analysis with the part chosen from all parts | Accept | The chosen part is never generated (SHARED-DIAG-012 step 1) and every non-generated part of these kinds gets a node analysis; the one kind the driver skips (partial constructor defining part) is RH2004's, now per type. A per-type move of the RH74 bases needs `Compilation.GetSemanticModel` (RS1030) | Output is SHARED-DIAG-012 | 2f1fb5a |
| 9 | Skeptic | Spec-author follow-up candidates 1 and 2 | Collapse into one spec-wording follow-up for SHARED-DIAG-004; candidate 2 folds into Q1's answer | SHARED-NESTED-002 already decides candidate 1 | Wording follow-up only | — |

## Follow-up candidates

1. SHARED-DIAG-004 wording (extended by 03-spec-author: last paragraph should not read as covering the region a part of a partial member lies in): exclusion list should name SHARED-NESTED-002's per-part reports; second paragraph should not seem to cover RH8306 summaries (replaces spec-author candidates 1+2) — 02-skeptic.md.

2. Unify the generated-code decision (CLI `GeneratedFileUtilities`, formatter `IsAutoGeneratedSource`, analyzer `PartialMemberUtilities.IsGeneratedCode`); carries a visible CLI question: `Foo.generated.cs` is generated for the compiler but not skipped by the CLI — 05-implementer.md.
3. #1334 note: RH7405–RH7408 count a part in another file as reported; the only-some-parts case can then report nothing — 05-implementer.md.

4. RH2001 fix on `static private int Value { get; }` writes five spaces between `private` and `static`, and a comment between the two modifiers moves across `private` (pre-existing, outside the issues) — 10-auditor.md.

5. RH2005: `static public /* c */ int` — the removed `public`'s trailing comment moves in front of `static` (pre-existing, traced) — 14-implementer.md.
6. `DocumentationAnalysisUtilities.OverridesOrImplementsMember` (RH8201 and other documentation rules) judges the implementing part of a partial interface-implementing member as implementing nothing (pre-existing, traced) — 14-implementer.md.

## Pending output questions (batch before auditor)

- Q5: RH2005 `static⏎        public int Counter;` (indentation only) — A keep the line in place (`private static⏎        int Counter;`, rec.), B formatter layout (column 4).
- Q6: comment/directive/`#if` line in the replaced gap (RH2005 in front of non-first `public`; RH2001 between type and name) — A withhold the fix (rec.), B carry in place. ANSWERED 5A, 6A.

- Q4: RH2001 property whose type is a type parameter not constrained to a reference type — ANSWERED A.

## Earlier output questions (batch before test implementer)

- Q1 (blocks #1287 rest): location of the single diagnostic of a partial member (RH2002, RH2004, RH8306, RH7401) — A first reporting part in document order (rec.), B first part, C implementing part.
- Q2: RH2001 mutable-struct boundary — re-asked with target-framework consequence; options A, B, C, C′ (rec. C′).
- Q3: RH2005 references outside the type body that compile after the fix (nameof in own attribute, cref) — A offer fix (rec.), B withhold.
- ANSWERED: 1A, 2C, 3B (see Maintainer answers).

## State

- Audited SHA: —
- Last step: spec author 01 (fd5393f).
- Last step: skeptic 02 (7e38ec4). Output questions Q1–Q3 asked.
- Spec author 03 done (bbfd717). Continued once for decision 10.
- Spec author decision 10 applied (9494bcd).
- Test implementer 04 done (c55c7d0): all four reproduce; 82 failing tests to make pass.
- Implementer 05 done (bd592fd, 03cdad7): 3 disputed tests (harness setting).
- Test implementer 06 done (38ab0b9).
- Spec author 07 done (3178c94).
- Cleaner 08 clean (481ca6d).
- Spec checker 09 (d885383): one finding → Q4 asked.
- Q4 answered A; nothing to change.
- Audited SHA: 42db22c37641c0f41e0d6ac2617869e0b61c6541 — BLOCKED (10-auditor.md, bbc40f7): findings 1, 2a, 2b, 3, 4a, 4b.
- Test implementer 11 done (2d72935): 22 failing; tests accept several outputs for findings 1/2b → Q5, Q6 answered 5A, 6A.
- Spec author 12 done (5d0db01): RH2005-003 clarified, RH2005-005 new, RH2001-007 widened.
- Test implementer 13 done (e70fd29): 17 intended reds in RH2001/RH2005 plus 4a/4b reds.
- Implementer 14 done (2f1fb5a, d09d88f): all tests pass (analyzer 7153, core 649, architecture 115).
- Next: cleaner (15), spec checker (16), auditor on repair only (17), validator (18).
- (old line) Next (was: test implementer 13) narrows tests to the answered outputs; implementer (14) repairs findings 1–4 with decisions 16–18.
- Repair round 1 (of 3) worklist: test implementer (11) adds regressions for 1, 2a, 2b, 4a, 4b; implementer (12) repairs 1–4 with decisions 16–18; cleaner, spec checker, auditor (repair only), validator.
