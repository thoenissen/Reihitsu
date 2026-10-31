Role: test implementer
Commit: 1c604fe
Verdict: tests written

Repair round 1, auditor finding #1 (`12-auditor.md`) and decision 14 (`00-lead.md`). The coordinator's message named no intended fix. It said the expected output is `main`'s output, "which whole-document formatting also gives". That second claim does not hold for these inputs; see *Findings*. The expected values below are `main`'s code-fix and `FormatNodeInDocumentAsync` outputs with a space in the target. The change doc says those outputs stay as they are ("Every other node-in-document output stays as it is"), and FMT-PIPE-024 makes a decision that reads outside the root take its answer from a fact.

How I obtained them:

- `main` (`origin/main` `c24bc1d`), in a temporary worktree outside the repository, removed afterwards: `scripts/apply-fix.sh RH6023` over the auditor's fixtures (`fx6023`, `fx6023b`, copied into one directory), under LF and CRLF.
- A scratch probe of `FormatNodeInDocumentAsync`, with a space and with a tab, built once against `main` and once against this branch.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAMemberBinding` (`a?.B⏎  .C=5;` → `.C = 5;` in column 10) | RH6023-002, FMT-PIPE-024 | `Verify`, LF then CRLF (`VerifyUnderBothLineEndings`) | fails on branch (`a?.B.C = 5;`); passes on `main` |
| `…VerifyFixKeepsTheWrappedLinkOfACompoundAssignedMemberBehindAMemberBinding` (`a?.B⏎ .C+=5;` → `.C += 5;` moved from column 9 to 10) | RH6023-002, FMT-PIPE-024 | same | fails on branch (joined); passes on `main` |
| `…VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAnElementBinding` (`a?[0]⏎  .C=5;`) | RH6023-002, FMT-PIPE-024 | same | fails on branch (`a?[0].C = 5;`); passes on `main` |
| `…VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberInANestedConditionalAccess` (`a?.B?.B⏎    .C=5;` → `.C = 5;` moved from column 12 to 13) | RH6023-002, FMT-PIPE-024 | same | fails on branch (joined); passes on `main` |
| `…VerifyFixKeepsTheWrappedLinksOfALongerAssignedChain` (`a?.B()⏎  .D⏎  .C =5;` → `.C = 5;`, every link kept) | RH6023-002, FMT-PIPE-024 | same | fails on branch (`a?.B().D⏎      .C = 5;`); passes on `main` |
| `…VerifyFixKeepsAnAssignedMemberOnOneLineBehindAMemberBinding` (`a?.B.C=5;` → `a?.B.C = 5;`) | RH6023-002, FMT-PIPE-024 | same | passes on branch and on `main` (the other side of the boundary: `Left` on one line) |
| `…VerifyFixOnAnAssignedMemberBindingKeepsItOnItsLine` (`a?.B=a;` → `a?.B = a;`) | RH6023-002, FMT-PIPE-024 | same | passes on branch and on `main` (the other side: `Left` is a bare binding) |
| `ReihitsuFormatterDetachedRootTests.ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember` (`a?.B⏎          .C =⇥5;`, target: the assignment) | FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` (space and tab, LF and CRLF, second pass) | fails on branch in both variants (`a?.B.C = 5;`). On `main` the space variant passes and the tab variant fails (joined) |

All 7 RH6023 tests were run on `main` with the same file: 22/22 tests of the class pass there. On this branch the class gives 17 passed and 5 failed. Every failure is "Iterative code fix application … did not match", with the joined or re-laid chain.

Each code-fix test reports one diagnostic, the operator marked `#0`, and runs under LF and then CRLF through the private helper `VerifyUnderBothLineEndings`. On `main`, every fixture converges in one iteration (`apply-fix.sh`).

## Findings

- **`main`'s fix output is not whole-document formatting's output for these inputs.** Observed on `main` with `scripts/format.sh`:
  - `a?.B⏎ .C = 5;` — the link moves to column 9, under `?`.
  - The nested case and `a?.B()⏎ .D⏎ .C = 5;` also go to column 9.
  - `a?[0].C = 5;` is joined.

  The `main` fix keeps the links and aligns them in column 10, or 13 for the nested case, under the dot of the last binding. The tests pin `main`'s fix output, as decision 14 and the change doc ask. The difference between the fix and whole-document formatting already exists on `main`, and this run does not change it. Whether it matters (SHARED-FIX-007, P11) is for the lead. No caller other than RH6023 was checked.
- FMT-PIPE-023 alone does not decide these outputs: the branch is already tab-equal. The tests fail on the value that FMT-PIPE-024 (decision from the fact, i.e. the document's answer) and the change doc decide.
- No test or spec found wrong. No scope growth. No new architecture finding beyond the auditor's.

## Not covered, and why

- RH5111 on the same shapes: the auditor reports its output unchanged against `main`. No test is added.
- Other code fixes that format such an assignment: none named by the auditor. Not swept.
- Fix All: the shapes do not co-occur in the fixtures. The class keeps its existing `GetFixAllScenario`.
- FC-019/FC-020 (comment or directive in the wrapped gap): SHARED-OPSPACE-002 governs the operator gap, not the link gap. Not in finding #1's member list.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember"
```

## Repair (decision 19)

`SelfReferentialTrackerReferenceTests.RepositoryCommentsContainNoSelfReferentialTrackerReferences` read the summary of `VerifyUnderBothLineEndings` (`<c>#0</c>`, line 839 of the RH6023 test file) as a tracker number. The summary now reads "Verifies the one diagnostic marked in the source and the fix result under LF and under CRLF". I checked every comment line this run added under `src/`, using a regex over `git diff 621b90e HEAD -- src` for comment lines containing `#` followed by a digit. That line was the only match. The change touches comments only; the test markup `{|#0:…|}` sits inside string literals and is unchanged. `scripts/test.sh --project architecture`: 92/92 pass.

## Round 2 (repair round 2: auditor finding 5, maintainer answer 8)

Commit: 26e6286a

Maintainer answer 8 (A): when an assignment inside a conditional access has both its member and its value wrapped, the RH5111 and RH6023 fixes keep the line break in front of the member and move the value onto the operator's line. The expected values are the branch's output from `scripts/apply-fix.sh` over the auditor's fixtures (`fxr2a`, `fxr2b`, plus an `a?.B` variant of `eb5111.cs` that I derived), under LF and CRLF. Each converges in one iteration. I checked each one against answer A: every link line is kept, the operator gets one space on each side, and the value joins the operator's line. Columns:

- `.C` stays in column 10 behind `a?.B` and behind `a?[0]`.
- In the nested conditional access, `.C` moves from column 12 to 13, under the dot of the last binding. That matches round 1's RH6023 nested test and `main`'s fix alignment for that shape.

Nothing contradicts answer A.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5111…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAMemberBinding` (`a?.B⏎  .C =⏎    5;` → `a?.B⏎  .C = 5;`) | RH5111-004, FMT-PIPE-024 | `VerifyUnderBothLineEndings` (new in the RH5111 class) | passes on branch |
| `RH5111…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberBehindAnElementBinding` (`a?[0]⏎  .C =⏎    5;` → `a?[0]⏎  .C = 5;`) | RH5111-004, FMT-PIPE-024 | same | passes on branch |
| `RH5111…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberInANestedConditionalAccess` (`a?.B?.B⏎    .C =⏎    5;` → `a?.B?.B⏎     .C = 5;`) | RH5111-004, FMT-PIPE-024 | same | passes on branch |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberWhoseValueIsWrapped` (`a?.B⏎  .C=⏎    5;` → `a?.B⏎  .C = 5;`) | RH6023-002, FMT-PIPE-024 | `VerifyUnderBothLineEndings` | passes on branch |

The RH5111 diagnostic is marked on the whole assignment expression (RH5111-003); the RH6023 diagnostic is marked on the operator. I did not rerun these on `main`. The auditor's run there gives the joined `a?.B.C = 5;`, so on `main` these tests would fail on the joined line.

Not added:
- RH5111 with the member on one line (`one5111.cs`, `a?.B.C =⏎5;` → `a?.B.C = 5;`): the auditor lists it as holding, and its output is unchanged.
- RH6023 compound with a wrapped value (`eb6023.cs`): it holds on both trees, per the auditor.

Results:
- RH5111 and RH6023 classes on the branch: 86 of 86 pass.
- Architecture project: 92 of 92 pass. No comment added in this round contains `#` followed by a digit.

Commands:

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests"
```
