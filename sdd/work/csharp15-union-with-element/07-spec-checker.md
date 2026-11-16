Role: spec checker
Commit: 6e9ea23
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Checked against `git diff 1d2d29a...HEAD` at 6e9ea23. Architecture run: `scripts/test.sh --project architecture --no-install` -> `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118`.

## thoenissen/Reihitsu#1762

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/formatter/pipeline.md` FMT-PIPE-029, last paragraph (line 1605) | It still names the context the RH7101 fix passes as "the class, struct, record, or interface declaration whose member it is". The diff makes `SplitsFieldDeclaration` (FMT-PIPE-031) and the split (FMT-STRUCT-021) accept a union, and the fix now passes a union declaration as context (test `VerifyCombinedStaticFieldInAUnionIsSplit`). Add "or union" there; add FMT-PIPE-029 to the change doc's `Affected requirements`, which lists FMT-PIPE-031 but not FMT-PIPE-029 (the RH7101-002 Why edit is likewise not listed). | spec author |

### Checked
1. ok. Change doc `union-declaration-layout.md` is present (new behavior), has no task list, has `Visible output changes`, is `Accepted`, and `Decisions` records Q1A, Q2A, Q3A.
2. Finding 1. Otherwise the requirements changed (FMT-BREAK-002, -007, -011, -023, FMT-STRUCT-017, -021, FMT-PIPE-031, RH7101 Why) match the diff; each has an example and a `Why:`; FMT-STRUCT-017's union case matches `EmptyTypeDeclarationSemicolonTransform`.
3. ok. No new IDs from this issue; the ledger is untouched for it.
4. ok. No code-owner headers changed that need updating; no new type; `SpecConsistencyTests` pass (architecture 118/118).
5. ok. No rule page affected; `documentation/rules` has no union mention and the diff touches none.
6. ok. Q1 to Q4 answered under `Maintainer answers` in `00-lead.md`; follow-ups (a) to (d) are under `Follow-up candidates` and not written into the specs.
7. ok. Architecture result quoted above; FMT-BREAK-002/007/011/023, FMT-STRUCT-017/018/021 and FMT-PIPE-031 each have a covering `[Requirement]` test in the diff.
8. ok. Every test in the test implementer's table carries its tag (the CLI test carries FMT-BREAK-002 and FMT-BREAK-007); the spec author's examples (FMT-STRUCT-017 union, -021 union, RH5404-003 change 7) are tagged.
9. ok. `sdd/work/` holds only this run's directory.

## thoenissen/Reihitsu#1763

### Findings
None.

### Checked
1. ok. Change doc `with-element-spacing.md` is present, `Accepted`, has `Visible output changes`, and `Decisions` records Q4A.
2. ok. FMT-SPACE-028 is new (one heading, normative sentence first, `Why:`, example); FMT-SPACE-002's order lists it with FMT-SPACE-025; the FMT-BREAK-022 addition states the existing `with(...)` element layout and has an example; all match `WithSpacingRule.IsWithElementKeyword`.
3. ok. `FMT-SPACE-028` is inserted after `FMT-SPACE-027` in `requirement-ids.txt`; no ID reused.
4. ok. No owner change (`WithSpacingRule` is covered by the existing spacing spec owners); architecture suite passes.
5. ok. RH5305's rule page is unaffected (no page text on `with`).
6. ok. Q4 answered in `00-lead.md`; follow-up (b) is recorded.
7. ok. FMT-SPACE-028 is covered by seven `[Requirement("FMT-SPACE-028")]` tests; FMT-BREAK-022 by two.
8. ok. `RH5305-004` test and the FMT-SPACE-028 examples are tagged as the table lists.
9. ok.
