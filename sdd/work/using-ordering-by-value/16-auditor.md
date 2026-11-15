Role: auditor
Commit: 925f598
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the scope (repair round 2) and asked me to confirm Finding 1 of `11-auditor.md`.

State verified:
- The checkout is 925f598030298c3b0788ee9e1f8500504dfb3499, and `git status` is clean.
- `git ls-remote origin refs/heads/main` returns 81eabebef044033483214fc0f01e63cf06067bb1, which is an ancestor of HEAD.
- The merge 925f598 is clean. `git diff --name-only 318fe96 925f598` is exactly the file list of `git diff 8a26419 81eabeb`, and that list does not overlap the PR's files (`comm -12` is empty).

The repair diff is what handovers 12–15 describe. `git diff 25baa47 HEAD`, excluding `sdd/work/` and the merged files, holds three specs and three test files:
- `sdd/specs/shared/using-directive-ordering.md`, `sdd/specs/analyzer/RH7203.md`, `sdd/specs/analyzer/RH7206.md`
- the RH7203 and RH7206 analyzer tests and `UsingDirectiveNameValueTests`

The repair changes no production code. The one production file in `git diff --stat 25baa47 HEAD -- src/Reihitsu.Core src/Reihitsu.Analyzer src/Reihitsu.Formatter` is `LineBreakListRewriter.cs`, and it came from `main` through the merge.

## Scope

Repair delta against 25baa47 (audit 1 on 9bd5966). Head 925f598, base `main` 81eabeb. Scope did not grow: the repair puts the maintainer's Q2 answer into the specs that audit 1 already covered and pins it with tests. No requirement ID, change doc, or rule page was added or changed.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The maintainer decided the output under `Maintainer answers` Q2 (A): a leading `global::` is ignored in the sort key. The spec now states it:<br>- SHARED-USING-004 adds the sentence "removed … before the root namespace and the sort key … are taken" and lists `global::System.IO` among the equal names.<br>- SHARED-USING-007 defines the key as "the value … without a leading `global::` qualifier" and adds the Q2 formatter example.<br>- RH7203-001, RH7203-002, and RH7206-001 have new examples.<br>The code already did this: `GetSortKey` → `GetNameValue` → `StripGlobalQualifier` (`src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs:91-96, 378-406`), unchanged, and its summaries describe it. The plain spelling is now pinned on every surface audit 1 named: RH7203 report and fix, RH7203 negative, RH7206 report and negative, and two formatter tests. Each test fails on `main`'s key, where `global::…` sorts before `System…` because `g` < `s`. The Q2 example output `System; System.Collections; global::System.IO` is exactly the formatter test's expected value. Every member audit 1 enumerated is decided by the same answer: plain `global::`, `static global::`, and the global section all go through the one `GetSortKey`. Alias keys are unaffected. No member still fails. |

## thoenissen/Reihitsu#1105

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Sort key drops a leading `global::` (spec now states it; code unchanged) | `GetNameValue`: the concatenated `ValueText` of the tokens of `UsingDirectiveSyntax.Name`, then `StripGlobalQualifier` on the ordinal prefix `global::` | Root (`GetRootNamespace`) and `System` membership (`IsSystemNamespaceUsing`) read the same stripped value. SHARED-USING-004 says the qualifier is "part of neither", with or without trivia around `::` | Same string for root and key. The trivia around `::` is already absent from the value, so `global :: X` and `global::X` give the same key. An extern-alias qualifier (`Lib::X`) is kept in both, as on `main` (12, internal choice) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Sort key / `global::` strip | `rg 'GetSortKey\|GetNameValue\|StripGlobalQualifier' src --glob '*.cs'` finds only `UsingDirectiveOrderingUtilities` (lines 45, 57, 91-96, 160, 242, 378-406). `rg 'sort key' sdd/specs` finds only `using-directive-ordering.md` and the RH720x specs; `sdd/specs/formatter/using-directives.md` and the rule pages state no key spelling | one owner, and the specs agree |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7203 `GlobalQualifierIsNotPartOfTheSortKey` | `System.Collections` after `global::System.IO` is reported | No diagnostic (`main`'s key) | `Verify` (exact set) | adequate |
| RH7203 `CodeFixPlacesADirectiveBeforeAGlobalQualifiedDirectiveThatSortsAfterIt` | The fix gives `System.Collections; global::System.IO` with the spelling kept, and converges | Different output, a spelling change, or a diagnostic left after re-analysis | `Verify(source, fixed, Diagnostics(id, msg))` on location `#0` | adequate |
| RH7203 `GlobalQualifiedDirectiveThatSortsAfterByItsValueIsNotReported` | `global::System.IO` after `System.Collections` is clean (FC-003, other side) | A diagnostic (`main` reports it) | `Verify` | adequate |
| RH7206 `GlobalQualifierIsNotPartOfTheSortKey` / `GlobalQualifiedStaticDirectiveThatSortsAfterByItsValueIsNotReported` | Both sides for `static global::System.Math` vs `static System.Console` | A missing or an extra diagnostic | `Verify` | adequate. The tags RH7206-001 and RH7206-002 match their texts ("MUST NOT report one whose type name sorts before … no earlier") |
| Formatter `GlobalQualifierIsNotPartOfTheSortKey` | The Q2 output, idempotent | Any difference in pass 1 or 2, LF or CRLF, or a phase that is not stable | `AssertRuleResult(input, expected)` | adequate |
| Formatter `GlobalQualifiedDirectiveInOrderByItsValueIsUnchanged` | Stable valid code stays unchanged | `main` moves `global::System.IO` first | `AssertRuleResult(input)` | adequate |

### Failure classes dismissed
The catalog is unchanged since audit 1: the merge from `main` touches only `squad/notes/`.
- FC-001 to FC-003 and FC-015 to FC-017: traced in the tables above. Both sides of the strip are pinned.
- FC-004 to FC-014 and FC-018 to FC-043: the repair has no production change. Audit 1's dismissals of these classes were made on byte-identical production code (`git diff 25baa47 HEAD -- src/Reihitsu.Core src/Reihitsu.Analyzer` is empty), so they still apply.
- FC-023 (escaped or `@global` qualifier): still a shape people do not write, as in audit 1. It is not reported.
- FC-031: the summaries of the new tests describe their inputs. No production comment changed.
- FC-033: the RH7203 fix has no CRLF variant here. The formatter test runs LF and CRLF on the same reorder, and the ordering decision does not read line endings (13).

The global section (`global using global::…`) and a RH7206 code-fix test are not pinned. Both reach the same `GetSortKey` and `ComputeCanonicalOrder` that the pinned tests cover, and nothing in the specs asks for them. This is not a finding.

Visible output (check 10): every expected value the repair adds is the Q2 answer's example, or follows from it through RH7203-004 and SHARED-USING-007. Q2 is recorded under `Maintainer answers` in `00-lead.md`. No rule page changed.

Comments and scope (checks 8 and 9): there is no `TODO`, no tracker number, and no edit outside the run's components.

### Architecture findings
_None._

### Hints
- The Q2 answer asks for the `global::` key change to be named in the PR's Review notes. That is the lead's task when the PR body is written, and this tree cannot show it.
- Audit 1's performance hint on `GetDirectivesOutOfOrderWithinTheirGroup` stands as recorded and was not taken (Decision 8).

## thoenissen/Reihitsu#1266

### Findings
_None._ The repair does not touch #1266's behavior. The new RH7203 and RH7206 tests use two-directive blocks, so the every-earlier comparison is unaffected.

### Guard scope
See #1105.

### Policy ownership
See #1105.

### Assertion adequacy
See #1105.

### Failure classes dismissed
See #1105.

### Architecture findings
_None._

### Hints
_None._

## thoenissen/Reihitsu#1267

### Findings
_None._ The repair does not touch #1267. Alias keys are the alias name, and non-name aliases have no `Name`.

### Guard scope
See #1105.

### Policy ownership
See #1105.

### Assertion adequacy
See #1105.

### Failure classes dismissed
See #1105.

### Architecture findings
_None._

### Hints
_None._

## Whole run

- Nothing was executed in this round. No finding depended on a run, so I relied on the focused results in 13 (analyzer 5/5, formatter 21/21) and in 14 (Release build clean, RH720 237/237, UsingDirective 171/171).
