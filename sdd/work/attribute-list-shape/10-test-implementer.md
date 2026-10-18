Role: test implementer
Commit: c1964af
Verdict: tests written

Repair round on finding 1 of `08-spec-checker.md`. This round tests against the specs as narrowed in 8f02d0f, using `09-spec-author.md`. The left-out case is a #1093 instance: a property, indexer, or event on one line whose accessor list SHARED-ATTR-006 does not exempt. The accessor has a body or an expression body, or a comment sits inside the accessor list. No production code was touched, and every test passes on the unchanged code. My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the three tests and the goal, and I re-derived the list of affected tests from the revised specs instead of taking it over.

## How the affected tests were found

- I scanned the body of every `[Requirement]`-tagged test in the 16 list-shape classes. The search looked for an accessor list on one line that holds an attribute list together with `=>`, `{ }`, `/*` or `//`.
- The scan finds only RH5528 `VerifyNoDiagnosticForInlineAutoPropertyWithOwnAttributeListAndTrailingComment`. Its comment follows the accessor list: `int Q { [return: A, B] get; set; } = 1; // c`. SHARED-ATTR-006 says such a comment does not end the exemption, so the test stays tagged.
- The scan ran after the repairs below. Before them, it would also have matched the three tests the spec author named.
- SHARED-ATTR-013 now excludes accessor lists and refers them to SHARED-ATTR-014. So every tagged test that reports a multi-attribute accessor list should carry SHARED-ATTR-014. Two RH5531 tests carried only SHARED-ATTR-013 for that, and both were retagged (see below).

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5528 `VerifyDiagnosticForMultiAttributeAccessorListOnlyWhenTheAccessorListSpansSeveralLines` (renamed from `…UnlessThePropertyIsAnInlineAutoProperty`) | RH5528-001, SHARED-ATTR-014 | `Verify(source)` | pass. The one-line `int Q { [return: A, B] get => 1; }` line is removed. The test now holds the revised SHARED-ATTR-014 example: exempt `P`, not reported, and multi-line `R`, one diagnostic. The removed case stays characterized by the untagged `VerifyDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessorWithExpressionBody`. |
| RH5528 `VerifyDiagnosticForMultiAttributeAccessorListWhenCommentSitsBetweenTheAccessors` | none (untagged characterization of the left-out case) | `Verify(source)` | pass, unchanged otherwise |
| RH5531 `VerifyDiagnosticWithoutCodeFixWhenCommentSitsBetweenOneLineAccessorLists` (was `…WhenCommentIsInsideOrBetweenTheLists`) | none (untagged characterization of the left-out case) | `Verify` + `GetCodeFixActionsAsync` (list index 1) | pass. Only `int Q { [A] /* c */ [B] get; set; }` is left. The multi-line `P` part was removed, because it duplicated the tagged `VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent`. The RH5531-006, SHARED-ATTR-019 and SHARED-ATTR-020 tags were removed. A mutation that removes the comment from `codeFixData` makes the test fail, so index 1 is the reported list. |
| RH5531 `VerifyDiagnosticAndCodeFixForMultilineAccessorPolicyViolation` | SHARED-ATTR-013 tag replaced by SHARED-ATTR-014 | `Verify(source, fixed)` | pass |
| RH5531 `VerifyDiagnosticForMultiAttributeListsOfAccessorsInMultiLineAccessorLists` | SHARED-ATTR-014 added. SHARED-ATTR-013 kept for its `[A] [B]` single-attribute lists, which are not reported. | `Verify(source)` | pass |

Coverage after the repair (number of tagged tests):

| Requirement | Tagged tests |
|---|---|
| SHARED-ATTR-013 | 43 |
| SHARED-ATTR-014 | 10 |
| SHARED-ATTR-019 | 16 |
| SHARED-ATTR-020 | 11 (RH5524, RH5529) |
| RH5528-001 | 8 |
| RH5531-001 | 4 |
| RH5531-006 | 1 (`VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent`, the multi-line `[First, /* keep */ Second]`, the same shape as the revised example) |

`RequirementCoverageTests` passes.

## Scenarios (bug fix)

_N/A_: there is no bug fix in this lane.

## Not covered, and why

- **The left-out #1093 case.** It is asserted only by untagged tests:
  - the two above;
  - RH5528 and RH5518 `…OnSingleLinePropertyAccessorWithExpressionBody`, unchanged.

  No tagged test uses a one-line owner whose accessor list SHARED-ATTR-006 does not exempt.
- **Everything else is unchanged from `06-test-implementer.md`.**

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ShapeRulesAnalyzerTests"
scripts/test.sh --project architecture
```

Results on c1964af:

- the 16 list-shape classes: 200 passed;
- the architecture project, including `RequirementCoverageTests`, `SpecConsistencyTests` and `SpecStructureTests`: 72 passed;
- `scripts/format.sh` on the two changed files changed nothing.
