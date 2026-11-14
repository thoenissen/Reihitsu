Role: spec checker
Commit: 07a64a8
Verdict: findings (one, low; no finding for the implementer or the test implementer)

Run: issues thoenissen/Reihitsu#1600, #1150, #1274, #1143, #1673, #1678 (slug `fix-offering-safety`, lane behavior change). Diff checked: `git diff origin/main...HEAD`. My prompt carried no intended fix or hoped-for verdict.

## thoenissen/Reihitsu#1600

### Findings
None. FMT-PIPE-033, SHARED-FIX-012, CLI-021, PLAY-009, CLI-014/015, and the FMT-PIPE-003/005/006 and FMT-UNSUP-001/002 edits match the diff. The change doc is `Accepted`, has `Visible output changes`, and records Q1 B, Q2 A, Q3 B, Q8 B in `Decisions`. Tags: FMT-PIPE-033 (13 tag lines, one test class), SHARED-FIX-012 (11, in the RH3004, RH6024, RH4103, RH2001 test classes), CLI-021 (8), PLAY-009 (4); 06's and 08's tables name them.

## thoenissen/Reihitsu#1150

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/fix-offering-safety.md` | The change doc never mentions SHARED-FIX-011 or RH5308-006 (no hit for either ID, nor for SHARED-FIX-002). SHARED-FIX-011 is a new requirement that withholds the fix of about 30 rules in a file with a syntax error (28 test classes plus RH5308 and RH3004), and RH5308-006 withholds a no-op fix. Both are visible fix-offering changes. Check 1 needs no change doc for a bug fix or clarification, and the lead note treats both that way, with the maintainer's 2026-11-06 answer as the source. But the doc is the run's one record of what end users see, and it lists neither the answer nor an explicit "bug fix, no output question" line for them. Either add them under `Visible output changes`, with that answer in `Decisions`, or add one sentence saying why they need no entry. | spec author |

## thoenissen/Reihitsu#1274

### Findings
None. FMT-UNSUP-008, RH5308-005, the SHARED-DIAG-010 sentence and example, the FMT-BREAK-018 Why edit, and the AGENTS.md precheck row match the code (`PrecheckPhase`, deleted refusals in `TernaryLineBreakRewriter`). Q4 C and Q9 A are recorded. FMT-UNSUP-008 has 38 tag lines in 5 files and RH5308-005 has 6. The SHARED-DIAG-010 example is tied to RH5308-005's test as 05 asked (`SpecExampleConditionalTheFormatterRefusesIsReported` carries both tags).

## thoenissen/Reihitsu#1143

### Findings
None. No spec text changed. The work is a code change toward SHARED-FIX-002, which already contains the starting-code wording that the `DocumentOrderFixAllProvider` filter implements. No change doc is needed. The kept second passes are listed in 06 and in `00-lead.md` decision 16.

## thoenissen/Reihitsu#1673

### Findings
None beyond finding 1. RH5308-006 is stated, has its ledger line, and is tagged on 3 tests. The code matches the implementer's note.

## thoenissen/Reihitsu#1678

### Findings
None. The only change is the SHARED-ORDER-008 reason (Q5 A). It writes down the maintainer's decision, not new behavior, and no test is needed.

## Whole run

### Findings
Finding 1 above.

### Checked
1. Change doc exactly when required: finding 1. Otherwise ok. The change doc is required (new formatter refusal, new unsupported shape, new skip outcome). It is `Accepted`, has `Visible output changes` and no task list. Every listed visible change has its answer under `Decisions`, or is covered by lead decisions 9, 14 and 15.
2. Specs match the diff: ok. Requirements have the normative sentence, `Why:`, examples, and `{|RH####:…|}` markup. A `Why` in `AGENTS.md` and the spec files is consistent with the code.
3. IDs: ok. CLI-021, FMT-PIPE-033, FMT-UNSUP-008, PLAY-009, RH5308-005, RH5308-006, SHARED-FIX-011 and SHARED-FIX-012 take the next free number, each at its sorted place in `requirement-ids.txt`. No ID is reused, renumbered, or deleted.
4. Code owners exist: ok. `scripts/test.sh --project architecture` (includes `SpecConsistencyTests`): `Passed!  - Failed:     0, Passed:   117, Skipped:     0, Total:   117`. `sdd/specs/README.md` needs no change (no spec added or removed). The new `ReihitsuCodeFixProviderBase` is named in no spec header; `code-fixes.md` has no owners, so nothing is missing.
5. User documentation: ok. No `documentation/rules/` file is in the diff, and none needs to change. The RH5308 page has no mention of the new refusals, and `general-notes.md` already covers comments and a diagnostic without a fix. No new rule, so no `helpLinkUri` question.
6. Open questions routed: ok. Every output question in 02, 04 and 05 has an answer in `00-lead.md` (Q1 to Q10). 03 and 05 have none open. Out-of-run candidates are under `Follow-up candidates` (Q6, Q7, Q10, FMT-BLANK-011, `N(/* c7 */ 2)`, the escalation-9 process note, RH5401-004, the continuation-line column). 07's two candidates (`PlaygroundFormatter.HasSyntaxErrors`, `scripts/trace.sh` skip report) are in the lead note too. Nothing is recorded only in the PR.
7. Requirements covered: ok. The architecture run above includes `EveryRequirementOfAnEnforcedSpecIsCovered`, so every new requirement has a `[Requirement]` test in the diff.
8. Test tags: ok. The tags in the diff match the Tests tables of 06 and 08 for FMT-PIPE-033, FMT-UNSUP-008, SHARED-FIX-011 (30 lines in 29 files: the 28 classes, RH5308, RH3004), SHARED-FIX-012, CLI-021, PLAY-009, RH5308-005, RH5308-006 and SHARED-DIAG-010. Every requirement under 05's `Examples for the tests` (carrier kinds, controls, balanced-directive inputs, the Casing and RH2001 paths, the inline, `?.`, `?[]`, `ref`, and interpolation-hole inputs) is covered by a test of the diff.
9. Nothing left behind: ok. `git ls-files sdd/work` holds only `sdd/work/fix-offering-safety/`, which the lead deletes before publishing.
