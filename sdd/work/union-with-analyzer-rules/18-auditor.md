Role: auditor
Commit: 6b25d32
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `6b25d32c77b760966dda7b49eba8d32fb1a62ac4`; the checkout is that SHA, and the tree is clean.
- Base `origin/main` `0866955f2b10e15906e33c02b3d4954006042cf7` (`git ls-remote` agrees). It is an ancestor of the head.
- Repair delta against `7a9f5d9` (13-auditor), repair round 2 (lead decisions 17-19).
- The diff is what handovers 14-17 describe:
  - five new tests in the RH5505, RH5507, and RH5521 test classes;
  - one clause on `documentation/rules/RH5106.md`;
  - handovers and `00-lead.md`.
- The repair touches no production code, no spec, and no change doc, so the scope did not grow. `scripts/lane-check.sh behavior-change` confirms the lane.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Finding #1 enumerated six members. Each now has a test that runs the fix on a type declared with a semicolon behind other code: <ul><li>RH5505 class: `ClassDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved`.</li><li>RH5505 record: `record R;` behind `class C` in `SpecificationExampleIsReportedAndFixed`.</li><li>RH5507 struct: `StructDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved`.</li><li>RH5507 record struct: `record struct R;` behind `struct S` in `StructSpecificationExampleIsReportedAndFixed`.</li><li>RH5521 interface: `InterfaceDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved`.</li><li>RH5507 union: the existing `UnionAttributeListOnDeclarationLineIsReportedAndMoved`.</li></ul>Lead decision 17 records the crash fix for RH5505, RH5507, and RH5521. |
| 2 | closed | `RH5106.md` now names "the case-type lists of unions" in the same position as `RH5105.md`. RH5106-001 already states it, and Q1 A covers it. |

No member is missing from the class of #1:
- The guard runs only for `SeparateLine` placement (`TargetAttributePlacementCodeFixProviderBase.cs:297-299`).
- A type declared with a semicolon has no members. So inside it, the only lists are its own lists, plus the lists of its primary-constructor parameters and type parameters.
- `AttributeTargetUtilities.TryResolveLayoutTarget` resolves those parameter and type-parameter lists to `Parameter` and `GenericParameter` even under `property:`, and those are not separate-line targets.
- So the type's own lists are the whole class, and every type kind that can be declared with a semicolon is covered: class, record, record class (same `TypeDeclarationSyntax` arm as record), struct, record struct, interface, and union.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope

_N/A — the repair adds or changes no predicate, guard, or exemption. The only guard it tests (`braces is { Open.RawKind: None }` in `HasUnusableEnclosingScope`) is byte-identical to `7a9f5d9`, and 13-auditor traced it against `SyntaxIndentationUtilities.IsIndentingAncestor`._

## Policy ownership

_N/A — the repair changes no policy. The two owners of the brace-scope set stand as 13-auditor recorded them: an architecture finding with a follow-up (lead decision 19)._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH5505…SpecificationExampleIsReportedAndFixed` | The RH5505-001 example verbatim: `[A] class C` and `[A] record R;` (the second behind other code) are reported on the list and moved by RH5505-003; `[A]⏎[B]⏎record class Q;` is not reported | An NRE on `R`, a diagnostic on `Q`, or a different fix text | `Verify` on the batch base (single fix plus framework Fix All over 2 diagnostics), LF+CRLF | ok |
| `RH5505…ClassDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved` | A class declared with a semicolon behind `using System;` is reported and fixed | NRE, or a different fix text | `Verify`, LF+CRLF | ok |
| `RH5507…StructSpecificationExampleIsReportedAndFixed` | The struct example of RH5507-001 verbatim, including `record struct R;` behind other code | NRE, a diagnostic on `T`, or a different fix text | `Verify` on the batch base, Fix All over 2 diagnostics, LF+CRLF | ok |
| `RH5507…StructDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved` | A struct declared with a semicolon behind other code is reported and fixed | NRE, or a different fix text | `Verify`, LF+CRLF | ok |
| `RH5521…InterfaceDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved` | An interface declared with a semicolon behind other code is reported and fixed | NRE, or a different fix text | `Verify`, LF+CRLF | ok |

**Why these assertions are strong enough.**
- 14-test-implementer disabled the guard locally and watched exactly these tests fail with `NullReferenceException` under LF and CRLF, 12 failures in all. That is the falsifying observation, and it happened.
- The expected values come from spec examples, not from the patched code.
- I checked that the two `SpecificationExample` inputs equal the RH5505-001 and RH5507-001 examples verbatim. The only additions are the attribute class declarations the examples need.
- I did not re-run the tests: no finding turns on a test result.

**Visible output (check 10).**
- The new expected values are code-fix results on struct, class, and interface lists. The unchanged requirements RH5505-003, RH5507-003, and RH5521-003 demand them. Before the fix, `main` crashed on these inputs instead of offering the fix (lead decision 17).
- The RH5106 page change is covered by Q1 A.
- No output is undecided.

## Failure classes dismissed

- **FC-001, FC-002, FC-015:** the repair adds no predicate and moves no policy.
- **FC-003 – FC-014, FC-018 – FC-030, FC-036, FC-038 – FC-043:** the repair adds no production code, rewrite, phase, entry point, or precheck shape. The only compiled behavior it exercises is the guard, unchanged and already audited in 13-auditor.
- **FC-016:** this was finding #1, now closed (see *Previous findings*).
- **FC-017:** see *Assertion adequacy*. The tests cover LF and CRLF, and the batch base runs Fix All on the two-diagnostic tests.
- **FC-031:**
  - The summaries of the five new tests describe what they assert.
  - The RH5106 page now agrees with RH5106-001 and with RH5105's page.
  - The new code has no `TODO`, no commented-out code, and no tracker number.
- **FC-032:** the repair has no compiled change after the implementer's last full run. The cleaner built the head without warnings (16-cleaner), and the test implementer ran all 38 tests of the three classes green on `8da4e05`.
- **FC-033, FC-034, FC-035:** the expected values come from spec examples and the auditor's fixtures, not from code output. The inputs are the reported crash shape.
- **FC-037:** the repair makes no comment-only claim.

## Architecture findings

_None._ The repair adds no new one. The two owners of the brace-scope set (13-auditor) stay a recorded follow-up (lead decision 19).

## Hints

_None._ The RH5417 `Release.ruleset` hint from 13-auditor is already a follow-up candidate in `00-lead.md`.
