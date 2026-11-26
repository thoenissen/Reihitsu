Role: auditor
Commit: 9fc0054a0d4f2693eb6b7b6529e5a104f6a57293
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `5b79d152a854bf779fed97278e0b5fb66f1dbea8` (audited in `08-auditor.md`). Head `9fc0054a0d4f2693eb6b7b6529e5a104f6a57293` is checked out with a clean tree. Base `origin/main` is `eed7195c0b6fb583ab4271173b7a4ad022851c5f`, which matches `git ls-remote` and is an ancestor of the head, so it is merged. `scripts/lane-check.sh behavior-change` confirms the lane.

The repair diff matches handovers 09–12. It touches the change doc (`sdd/changes/2026-11/rh2001-conditional-group-refusal.md`), one test file (`tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.cs`, four tests added and nothing else changed), and `sdd/work/`. `git diff 5b79d15..HEAD -- src sdd/specs documentation` is empty. So the production code, the specs, and the rule page are byte-identical to the first audit, and I reuse its evidence for them: guard spans, policy ownership, and the failure classes that only the production code can reach. The scope did not grow.

I relied on the cleaner's green build (11) and on the test implementer's focused runs (10): 176 passed in the trivia class and 313 for `RH2001`, plus the mutation check on both arms of the behind check. No finding depended on re-running them.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `VerifyNoCodeFixIsOfferedForAnEndifAloneBehindAPropertyWithoutAnInitializer` puts only `#endif` in the behind token's trivia. `B`'s first modifier holds a blank line and no directive. The `#if NET` is in `_a`'s trivia. `VerifyNoCodeFixIsOfferedForAnIfAloneBehindAPropertyWithoutAnInitializer` puts only `#if NET` in `_x`'s first token. `B`'s first modifier follows `{` with no directive, and the `#endif` sits in `Read`'s trivia, away from `B`. Each test isolates one directive kind on the behind token, under LF and CRLF. The test implementer narrowed the check to `#if` only, then to `#endif` only. Each mutation failed exactly the matching test, so the kind axis is pinned on both arms. The other side of the boundary is pinned too (`VerifyFixKeepsAPropertyInTheMiddleOfAGroupAroundSeveralMembers`) |
| 2 | closed | Section 2 of *Visible output changes* now has shape (a), with its input, the `main` output, and the after state. Section 3 now has the active-`#else` property with the same three parts. *Unchanged* names the middle-of-group property, and *Decisions* names the `#endif`-alone-behind reading. Both shapes follow from maintainer answer A and RH2001-010's boundary text ("whether it is the group's `#if` or only its `#endif`"). Section 2 already covers shape (b) of finding 1 in kind (`#if` alone behind, symbol defined): a group of whole members behind a property without an initializer |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _N/A — no new finding._ | | | | | | | |

## Guard scope

_N/A — the repair changes no predicate, guard, or exemption. The production code is byte-identical to `5b79d15`, and the guard-scope table of `08-auditor.md` stands unchanged._

## Policy ownership

_N/A — the repair touches no production code, so no owner moved. The table of `08-auditor.md` stands unchanged._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyNoCodeFixIsOfferedForAnEndifAloneBehindAPropertyWithoutAnInitializer` | An `#endif` alone in the leading trivia of the token behind a property without an initializer withholds the fix, and the diagnostic stays at `B` | A code action is registered, or the diagnostic moves or disappears | `VerifyNoCodeFixIsOffered` (exact diagnostic, `Assert.IsEmpty(actions)`, LF and CRLF) | adequate. This is the only directive next to `B`, so the front check cannot mask it. The `#endif`-blind mutation fails only this test |
| `VerifyNoCodeFixIsOfferedForAnIfAloneBehindAPropertyWithoutAnInitializer` | An `#if` alone in the leading trivia of the behind token withholds the fix | same | same | adequate. The `#if`-blind mutation fails only this test |
| `VerifyNoCodeFixIsOfferedForAPropertyThatIsTheActiveElseBranch` | A property that is the active `#else` branch gets no fix | same | same | adequate as a pin of the change-doc example. Both sides withhold here: `#if`/`#else` in front of the first modifier and `#endif` behind. So it pins the example's visible output, not one side of the check. The one-sided tests pin each side |
| `VerifyFixKeepsAPropertyInTheMiddleOfAGroupAroundSeveralMembers` | A property inside a group, with no directive next to it, keeps the fix. The output is exactly `private readonly int _b;`, the group stays in place, and the reference is renamed | A refusal of any enclosing group fails it. Any byte of different output, or a fix that does not converge, also fails it | `Verify` (full fixed source, LF and CRLF) | adequate |

Visible output (check 10): the three no-fix expectations follow from maintainer answer A, recorded under `Maintainer answers` in `00-lead.md` and in the change doc's *Decisions*. The middle-of-group expectation is the output `main` produces, so it is not new output. On `main`, `ConditionalGroupsWrapWholeMembers` (`OrderingDeclarationUtilities.cs:293` at `eed7195`) evaluates `All` over an empty set of conditional directives on both tokens and returns `true`, so the same rewrite runs. That rewrite is unchanged by the run. The change doc's *Unchanged* section names this shape. Nothing visible is undecided.

## Failure classes dismissed

- FC-001, FC-002, FC-004–FC-016, FC-018–FC-030, FC-036, FC-038–FC-044: none can reach the repair, which touches no production code, spec, or rule page. The first audit's dismissals still hold for the code, which is byte-identical.
- FC-003: closed by finding 1's tests. Both arms of the behind-side kind axis are pinned, and both sides of the boundary are pinned too: the fix is withheld when a directive stands next to the property and kept when the group only encloses it.
- FC-017: see *Assertion adequacy*. Every new test runs under both line endings, and the kept-fix test asserts the exact output.
- FC-031: the four new XML summaries describe their fixtures exactly. There is no `TODO`, no commented-out code, and no tracker number in source.
- FC-032, FC-035: the expected values come from the change doc. Those values were checked with apply-fix on `main` (09) and statically above. None was taken from a patched copy.
- FC-033, FC-034: the tests are the change doc's exact examples and the auditor's exact shapes, under LF and CRLF.
- FC-037: no text-only claim.
- Check 11 (cost of special cases): no production code was added.

## Architecture findings

_None._

## Hints

- The hint of `08-auditor.md` about the asymmetry in a group of three properties still stands (lead decision 7). The middle-of-group test now pins its kept side.
- The change doc puts shape (a) under section 2 ("of whole members behind a property"), although that group also encloses the property. Section 3's heading would fit it too. The spec author recorded this choice (09, *Internal choices*), and the document reads correctly either way.

## Follow-up candidates

_None new._ FC1 and FC2 from `08-auditor.md` are recorded in `00-lead.md`.
