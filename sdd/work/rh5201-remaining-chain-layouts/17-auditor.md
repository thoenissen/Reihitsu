Role: auditor
Commit: 6fd338f92d6e9681cdf9d2baafb1a1276bd0a286
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta, repair round 1. Previous audit: `2ca825e` (`12-auditor.md`).

State checks:
- Head: the checkout is `6fd338f`, on branch `claude/busy-cori-4e7jd0`. It is the prompt's `06786b3` plus one lead commit that changes only `00-lead.md` (`git diff 06786b3 HEAD --stat`). The audited source tree is the same as `06786b3`.
- `origin/main`: `git ls-remote` gives `77a1f37`, and it is merged (`git merge-base --is-ancestor`).
- The merge: `06786b3` brought in only `.github/workflows/SonarCloud.yml` and `Directory.Build.props`. That is exactly `d63eab4..77a1f37`.
- Lane: `scripts/lane-check.sh behavior-change` confirmed.

Repair diff (`git diff 2ca825e 06786b3`, merge files excluded). It is what handovers 13 to 16 describe:
- one `IsMisplaced` summary comment;
- two analyzer tests in `RH5201MethodChainsShouldBeAlignedFirstCallTests.cs`;
- the RH5201-002 wording and *Why* sentence in `sdd/specs/analyzer/RH5201.md`;
- handovers.

No production logic changed, and scope did not grow. Evidence from the first audit for `FluentChain`, `SyntaxTokenPositionUtilities`, `ChainLineBreakRewriter`, and the analyzer logic is byte-identical and reused.

Evidence gathered in this round: 10 fixtures on the shared header from `02-skeptic.md`, run with `scripts/apply-fix.sh RH5201` under LF and CRLF.

| Fixture | Input | Result |
|---|---|---|
| y03 | Test (b) shape with a space in front of `.Next` | fixed in one action to the test's input |
| y07 | Test (b) misaligned | fixed in one action |
| y05 | Conditional prefix behind an own-line `*/` | fixed in one action |
| y08 | `*/ /* y */ .Next.Self()` | fixed in one action |
| y04, y06 | Formatter output of the x01 shape | no-diagnostic |
| y01, y02, y09, y10 | First link behind an own-line `*/` whose `*/` line does not start in the root column | no-progress; pre-existing on `main`, see *Follow-up candidates* |

## Previous findings

- #1 — **closed**. Both cells now have a test:
  - (a) `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported` (x01 shape: only `.Self` is reported; the fix gives the formatter layout);
  - (b) `VerifyFirstCallBehindFirstLinkBlockedByBlockCommentOnRootLineIsNotReported`.

  `13-test-implementer.md` shows each test going red against the exact mutation that #1 named: `linkIndex > 0` dropped gives `expected 1 actual 2`, and `FirstLink.StartsLine &&` dropped gives `expected 0 actual 1`, under LF and CRLF. Other evidence:
  - I measured the columns of test (b): the root line `.Self` and the next `.Self` are both in column 30.
  - y03 and y07 confirm that test (b)'s input is the formatter's stable output.
  - The fixed text of test (a) equals the formatter output from the first audit's x01.
- #2 — **closed**. The `IsMisplaced` summary now says "a link behind the first that starts a line behind the closing line of a multi-line comment standing below the token in front of the link". That matches `linkIndex > 0 && link.StartsLine && IsBehindMultiLineCommentTail`. The cleaner proved the change text-only with `verify-text-only.sh` (PASS).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedFirstCallTests.cs`, `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported` | – (traceability; `sdd/README.md:77`) | – | PR-introduced | The repair added a MUST NOT clause and an inline example to RH5201-002: the chain's first link behind an own-line `*/` is not reported by the `*/` paragraph (`var a = c` ⏎ `/* x` ⏎ `*/ .Next.Self();`: `.Self` reported, `.Next` not). `15-spec-author.md` names this test as that clause's not-reported side. Its summary states the same behavior. But the test carries only `[Requirement("RH5201-006")]`, `-005`, `SHARED-CHAIN-004`, and `-005`. `sdd/README.md:77` requires a test to tag each requirement it covers. No test tagged RH5201-002 holds a first link behind an own-line `*/`, so `RequirementCoverageTests` cannot connect the clause to its test | Add `[Requirement("RH5201-002")]` to the test. Test-only; no output changes |

## Guard scope
_N/A — the repair changes no predicate, guard, or exemption._ The two guards that finding #1 named are unchanged. Their table in `12-auditor.md` still holds, and both boundaries now have a test on each side.

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The root's line reaches the first invoked link | Unchanged by the repair. One owner, `FluentChain.IsRootLineShortOfFirstInvokedLink` (`12-auditor.md`) | one owner |
| A token behind an own-line multi-line comment's closing line | Unchanged by the repair. One owner, `SyntaxTokenPositionUtilities.IsBehindMultiLineCommentTail`. RH5201's private `GetLineStartColumn` measures the token's text line for the column check. That is a different decision, but it bears on the follow-up candidate below | one owner for the decision the run changed |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported` | The first link behind an own-line `*/` in the root column is not reported. The first call is reported exactly once at its `.` | `.Next` reported (2 diagnostics), `.Self` missing, a fix result other than the formatter layout, more than one incremental iteration, or Fix All needing more than one iteration | `VerifyFix` → `Verify` with markup, `Diagnostics(id, msg, 1)`, `NumberOfIncrementalIterations = 1`, `NumberOfFixAllIterations = 1`, LF and CRLF | adequate. Proven red on the `linkIndex > 0` mutation. Tag gap is finding #3 |
| `VerifyFirstCallBehindFirstLinkBlockedByBlockCommentOnRootLineIsNotReported` | A comment that blocks the first link on the root's line, with no line break, does not make the root's line fall short. No diagnostic | Any diagnostic | `Verify` with no expected diagnostics, LF and CRLF | adequate. Proven red on the `FirstLink.StartsLine` mutation |

Visible output (check 10):
- Test (a) asserts `.Self` reported and `.Next` not, with the formatter layout as the fix. That is covered by maintainer answers 2A and 6A, and by RH5201-006, which the run does not change in the repair.
- Test (b) asserts no diagnostic. That is `main`'s output, unchanged.
- The rule page is untouched.
- The RH5201-002 sentence states existing behavior (`linkIndex > 0` was already in `2ca825e`).

## Failure classes dismissed
- FC-001, FC-002, FC-005, FC-015, FC-016, FC-044: no predicate changed in the repair. The analysis in `12-auditor.md` holds. The RH5201-002 qualifier "other than the chain's first link" is the explicit `linkIndex > 0` condition.
- FC-003, FC-017: closed by #1 (both sides tested, mutations shown red).
- FC-004, FC-006, FC-018 to FC-026, FC-029, FC-036: no rewrite added; the fix output is the unchanged formatter's.
- FC-007, FC-008: the new test pins one incremental iteration and one Fix All iteration. The no-progress shapes y01, y02, y09, and y10 are pre-existing and lie outside the run's change (see *Follow-up candidates*).
- FC-009, FC-043: no fix registration changed.
- FC-010 to FC-014: formatter unchanged; no new rule.
- FC-027, FC-028, FC-030: nothing changed.
- FC-031: #2 closed. The new test summaries describe what the tests assert.
- FC-032: the repair changed no compiled production code after the test implementer's 191/191 run. The cleaner's change is proven text-only.
- FC-033: both new tests run LF and CRLF.
- FC-034: x01 is the auditor's shape itself, not a cousin.
- FC-035: the expected values came from the specs and the unchanged formatter. The tests were seen red against mutated code, not written from it.
- FC-037: the text-only claim was proven by `verify-text-only.sh`, not by grep.
- FC-038 to FC-042: no shape declared and no precheck change.

## Architecture findings
_None._

## Hints
_None._

## Follow-up candidates

- **RH5201 judges a first link behind an own-line `*/` by the column of the comment's last line, and its fix cannot change that column.** Pre-existing on `main`. The run does not touch this path: `linkIndex > 0` keeps link 0 on `main`'s branch, which is the `StartsLine` column check, and `GetAnchorLink` and `GetLineStartColumn` are unchanged.

  Example: a first link held below the root by a delimited comment whose `*/` line does not start in the root column.
  ```
  var a = c
          /* x
              */ .Next.Self();
  ```
  RH5201 reports `.Next` because the `*/` line starts in column 20, not 16. The fix formats the chain, but the formatter never moves the content lines of a comment, so `.Next` stays reported and every later fix application changes nothing.

  Class members, each `scripts/apply-fix.sh` no-progress under LF and CRLF:
  - `.Next` behind it with a later call (y02);
  - the same with `/* x` misaligned too (y01, which converges into y02);
  - a chain without a call (y09);
  - a first link that is itself the first call (y10).

  Holds: the `*/` line in the root column (x01, now tested).

  Structural cause: RH5201's private `GetLineStartColumn` reads the token's text line. SHARED-COMMENT-012 says a rule that checks in which column a line starts must not take the comment's last line for a line of its own. Core's `SyntaxTokenPositionUtilities.IsBehindMultiLineCommentTail` already recognizes that line, but the column check does not use it.

  Suggested direction: decide the first-link column by the comment's first line, or withhold the report or fix for this rare class. A first link behind a misaligned comment tail is rare.

  Reachability: any user document through the analyzer and its code fix.
