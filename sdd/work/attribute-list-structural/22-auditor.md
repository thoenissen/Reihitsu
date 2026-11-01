Role: auditor
Commit: 8df7d7a93e443c1eb6e88f58998a4a98d2276cc5
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from thoenissen/Reihitsu#1492, `00-lead.md` (decision 23), `11-auditor.md`, `17-auditor.md`, `18-` through `21-`, the specs, the change doc, and the repair diff. Outputs below were measured with the Debug CLI of this head's production code (unchanged since `6be829f`; build of the session's repository checkout) and the Debug CLI of the `origin/main` archive (`3a7bbea`) from the earlier audit, first and second pass, fixtures in the session scratchpad.

## Scope

Repair delta against `6be829f4c98ed6668906c561114a94684c0037dd`. Head `8df7d7a93e443c1eb6e88f58998a4a98d2276cc5` (checked out, clean). Base `origin/main` = `3a7bbea376798e2a21cf9bc8fc1aeaeb4f26c81d`, an ancestor of head. The repair diff matches `18-` to `21-`: two data rows and one summary in `AttributeListStructuralTransformTests.cs`, one test in `AttributeListStructuralFinalOutputTests.cs`, one sentence of the change doc, and the `StructuralTransformPhase` row of `AGENTS.md`. No production code, spec requirement, or rule page changed; scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed. Test results taken from `18-` (101 passed) and `20-`.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 7 | closed | Phase rows `[⏎        method: A, B]` → `[method: A][method: B]` and `[ return: A, B]` → `[return: A][return: B]`; final-output test `[⏎ method: C, D] get;` in an accessor list over several lines. Traced against `AttributeListShapeUtilities.BuildSplitAttributeLists:143`: a line break behind `[` is the `[` token's trailing trivia and the next line's indentation the specifier's leading trivia, so reverting line 143 leaves `[        method: A]` in the phase row and in the accessor's first pass. That is `main`'s first pass, measured here (`[         method: C]⏎[         method: D]`), and what `18-` observed under the mutation. The `[ return: …]` row does not reach the specifier branch (the space is `[`'s trailing trivia), as `18-` states; it is a correct row of FMT-STRUCT-028, not a guard of the branch. Both owners that reach the branch (declaration, accessor) are now covered |
| 8 | closed | Change-doc sentence and `AGENTS.md` row reworded as suggested; both match FMT-STRUCT-029 and FMT-BREAK-032 ("In either layout, a merge of an accessor's lists keeps no line break…"). Its class has members the enumeration did not name; see finding 9 |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | low | `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/StructuralTransformPhase.cs:15-16` (class summary); `sdd/specs/formatter/structural-transforms.md:10` (introduction); `sdd/changes/2026-11/attribute-list-structural.md:242` ("FMT-STRUCT-029 (merge on parameters and type parameters)"); `src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/AttributeListStructuralTransformTests.cs:12` (class summary) | FC-031 | #8 | PR-introduced | Every one of these texts was written by this run before round 2 and states the structural merge's extent as "one list per target on parameters and type parameters" (or "merges the lists of one target on a parameter or type parameter"), the same statement finding 8 repaired in the `AGENTS.md` row. Since round 2 the structural merge also merges `param:`/`typevar:` lists on every non-accessor owner (FMT-STRUCT-029, second paragraph), and the test class tests it (`ParamAndTypevarListsOfAnotherOwnerAreMergedAndNotSplit`). Members that hold: the `AGENTS.md` row and change-doc line 258 (repaired), the change doc's *What* bullet on `param:`/`typevar:` lists and line 262, the `AttributeListMergeTransform` summary, and the `AttributeListStructuralFinalOutputTests` class summary (names FMT-STRUCT-029 without an extent). Text only: an `internal` class's summary, a spec introduction (not a requirement), change-doc rationale, and a test summary; none reaches compiled behavior, diagnostics, public API documentation, or rule metadata. | Extend each statement with "and of `param:`/`typevar:` lists on any non-accessor owner" (spec author for the spec and change doc; implementer for the phase summary; test implementer for the test summary). |

## Guard scope

_N/A — the repair adds or changes no predicate, guard, or exemption; production code is byte-identical to `6be829f`, whose guards `17-auditor.md` traced._

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Extent of the structural attribute-list merge (text) | FMT-STRUCT-029 (the requirement); restated in `AGENTS.md:171`, `structural-transforms.md:10`, `StructuralTransformPhase.cs:15-16`, `AttributeListMergeTransform.cs:11-15`, change doc lines 18–21, 242, 258, 262, `AttributeListStructuralTransformTests.cs:12` | Several restatements of one requirement; four lag behind it (finding 9, architecture finding below). Code owners unchanged from `17-auditor.md` (one shape predicate, one merge) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| A `NothingBehindTheOpeningBracketStaysInTheFirstCreatedList` row `LineBreakBehindOpeningBracketInFrontOfSpecifier` | Phase text `[method: A][method: B]`, LF/CRLF, no elastic trivia, FMT-PIPE-021 round trip | Indentation kept as the specifier's leading trivia (`[        method: A]`) | `AssertAttributeListText` (exact text from first `[` to last `]`) | adequate; discriminates the line-143 mutation |
| A same test, row `SpaceBehindOpeningBracketInFrontOfSpecifier` | Phase text `[return: A][return: B]` | Space left behind `[` | `AssertAttributeListText` | adequate for FMT-STRUCT-028; does not reach the specifier branch (Roslyn puts the space in `[`'s trailing trivia), and does not claim to |
| B `AccessorSplitDropsTheLineBreakBehindTheOpeningBracketInFrontOfTheSpecifier` | Exact final output in one pass, LF/CRLF, second pass equal, phase stability | First pass `[         method: C]` (`main`'s and the mutation's) | `AssertRuleResult` (asserts the first pass equals `expected`, then the second) | adequate |

Visible output (check 10): the three expected values equal `main`'s fixed point, measured here: `main` reaches `[method: C]⏎[method: D]⏎get;` and `[method: A]⏎[method: B]⏎void M()` only in its second pass, `[return: A]⏎[return: B]` in its first; head reaches all three in one pass. The one-pass outputs are demanded by FMT-PIPE-004/FMT-SPACE-004 and covered by decisions 4, 18 and 22 and the change doc's visible change 1 (a line break behind `[`, with or without a specifier, per FMT-STRUCT-028's text). No rule page changed.

## Failure classes dismissed

- FC-001, FC-002, FC-003, FC-005, FC-006, FC-015 (code), FC-019 to FC-024, FC-026 to FC-030 — no production code changed in the repair; the evidence of `17-auditor.md` applies byte-identically.
- FC-007, FC-008, FC-009, FC-018 — no code-fix path touched.
- FC-010, FC-011, FC-012 — the new tests assert LF and CRLF and a second pass; head measured stable on the three new inputs and on `[method:⏎ A, B]`, accessor `[method:⏎ C, D] get;`, and `[⏎ return:⏎ A, B]`.
- FC-013, FC-014, FC-036 — no analyzer, rule, or rename.
- FC-016 — the specifier branch of the trivia drop is now tested on both owners.
- FC-017 — finding 7 closed; the new rows assert exact text.
- FC-025 — not reached.
- FC-031 — finding 9; the two repaired texts and the new test summaries are accurate.
- FC-032 to FC-035, FC-037 — no text-only claim and no `verify-text-only.sh` verdict relied on; the tests were written after the production code (round 3 is a test-gap repair), and `18-` proved they fail under the mutation in a throwaway worktree.

## Architecture findings

Finding 9 repeats the class of finding 8: the extent of the structural merge, which FMT-STRUCT-029 states, is restated in summary form in `AGENTS.md`, the spec introduction, the phase's class summary, the merge transform's summary, the change doc, and a test class summary. Round 2 widened the requirement, and each restatement had to be found and updated by hand; finding 8 named two of them, and four more still state the old extent. This is "one rule is stated in more than one place, so the statements can drift apart" (`squad/README.md`, *Architecture findings*), confined to text.

## Hints

- Pre-existing, unchanged by the run: a line break behind a specifier's colon is copied into every created list (`[method:⏎ A, B]` → `[method:⏎A]⏎[method:⏎B]`, accessor alike; `[⏎ return:⏎ A, B]` settles at `[return:⏎A]⏎[return:⏎B]` on `main` in two passes and on head in one). Head's fixed point equals `main`'s. It belongs to follow-up candidate 4 (line breaks inside attribute-list brackets are never joined).
- Pre-existing: `line-breaks.md:10` says the line-break phase "only adds and removes line breaks", while its accessor split and merge (FMT-BREAK-032, FMT-BREAK-031's recorded exception) change tokens; `main`'s phase changed tokens for every attribute list. Text only; the run narrowed the gap rather than opening it.
