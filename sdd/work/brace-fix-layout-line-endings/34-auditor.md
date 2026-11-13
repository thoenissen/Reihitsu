Role: auditor
Commit: c25bcc3ad815a7b8379a97c66bef359fec35dbcb
Verdict: pass

The prompt named the scope (repair round 3, decision 25) and the SHAs; it carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

## Scope

Repair delta of round 3 (decision 25: audit 29 findings #9 and #10) against `809714efd3547a0a2ce5c91d0bae91c4e394e33c`, the head audit 29 checked. Not a full audit.

- **Head.** `c25bcc3ad815a7b8379a97c66bef359fec35dbcb` checked out.
- **Base.** `origin/main` is `166e3dd2690edc7ed9cfd593d3e7d8eee8764c1b` after `git fetch`; `git merge-base --is-ancestor` confirms it is merged.
- **Delta.** `git diff 809714e..HEAD` matches handovers 30 to 32:
  - `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs`: the `if (token.IsMissing == false)` wrapper in `StartsBehindAnOpenBraceOnItsLine` is removed, and its XML summary names why a missing brace is not seen;
  - `src/Reihitsu.Analyzer.Test/Formatting/RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests.cs`: two tests (three rows);
  - work files only otherwise. No spec, change doc, or rule page changed (`git diff 809714e..HEAD --name-only -- sdd/specs sdd/changes documentation` is empty).
- **Scope growth.** None.
- **Evidence reused from audit 29.** Everything outside the scan's body and the two new tests, including the guard-scope analysis of SHARED-ATTR-022 and the `apply-fix.sh` probes.
- **New evidence.**
  - Static tracing of the changed loop and of both new tests, including the counter-versus-flag mutation the offered test must falsify.
  - `scripts/test.sh --project analyzer --filter "FullyQualifiedName~NestedClosedBody"`: 3 passed (both new tests, all rows).
  - `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests|FullyQualifiedName~RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests"`: 87 passed, 0 failed.
- **Trusted results.** Implementer 31: analyzer suite 8807/8807. Cleaner 32: `Build succeeded`, 0 warnings, 0 errors (so the new `<see cref="SyntaxToken.GetPreviousToken"/>` raises no cref warning).
- **Lane.** `scripts/lane-check.sh behavior-change` reports `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–8 | closed | Closed in audits 21 and 29. This delta does not touch them. |
| 9 | closed | `SplitFixIsOfferedBehindANestedClosedBodyOnTheLineOfTheList` (LF, CRLF) pins the input and output audit 29 measured: `    void M() { if (true) { } } [A]` / `    [B] void N() { }`. Traced against a flag scan: the two `}` set the flag, the `if`'s `{` clears it, and M's `{` then returns true, so the offered test fails. Test implementer 30 measured the same mutation red. The optional withheld side is also added (`…WithheldBehindANestedClosedBody…`, `C3`): the walk reaches the class's `{` with no `}` waiting. |
| 10 | closed | The `IsMissing` wrapper is gone. `SyntaxToken.GetPreviousToken()` defaults to `includeZeroWidth: false` and steps only to tokens with a width greater than 0, and a missing token has width 0, so no missing token ever reached the removed branch. The loop now visits the same tokens and takes the same branches as before: no behavior change. The first token is obtained by the same call, so it is covered too. |

## thoenissen/Reihitsu#1416

_Nothing to audit for this issue._ The delta touches no brace spec, code, or test. The evidence of audits 21 and 29 stands.

## thoenissen/Reihitsu#1388

_Nothing to audit for this issue._ The delta touches no RH5011, RH8304, RH7309, or SHARED-FIX-008 spec, code, or test.

## thoenissen/Reihitsu#1627

_Nothing to audit for this issue._ The delta touches nothing it depends on.

## thoenissen/Reihitsu#1142

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Split withheld behind an open `{` on the list's line (SHARED-ATTR-022) | Unchanged from audit 29: tokens with a width greater than 0 in front of `attributeList.OpenBracketToken`, starting on the `[`'s line, walked backwards; `}` adds one to the waiting count, `{` returns true at 0 and otherwise subtracts one. Removing the `IsMissing` wrapper changes no visited token, because zero-width tokens are not returned by `GetPreviousToken()` | SHARED-ATTR-022, all three qualifiers | Agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "A `{` still open in front of the list on its line" | `rg -n "ATTR-022" src --glob '!*Test*'` finds only `StartsBehindAnOpenBraceOnItsLine` (line 103) and its call-site comment (line 232) in `TargetAttributeListShapeCodeFixProviderBase.cs` | one owner, unchanged |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5514 `SplitFixIsOfferedBehindANestedClosedBodyOnTheLineOfTheList` [LF/CRLF] | Each `{` pairs with the `}` that closes it, by count, not by flag; exact SHARED-ATTR-017 output (`[B]` at the copied 4-space indentation) in the document's line ending | No action registered (a flag scan; measured red by test implementer 30), or a different layout or line ending | `VerifySplitFix` (verifier + raw text, line endings) | adequate |
| RH5514 `SplitFixIsWithheldBehindANestedClosedBodyWhileTheBraceOfTheClassIsStillOpenOnTheLineOfTheList` | A nested closed body in front of the list does not cancel the class's `{`, still open on that line | An action is registered: falsifies a scan that lets one closed body cancel every `{` in front of it | `VerifyNoCodeFixIsOffered` (diagnostic reported via markup, no action) | adequate; as handover 30 states, it does not separate count from flag, and the offered test does |

### Failure classes dismissed
- **FC-001, FC-002.** The scan's span and qualifiers are unchanged; see the guard table.
- **FC-003, FC-027.** Both sides of the pairing are now tested at two nesting levels; a deeper level adds no new branch to a counter.
- **FC-017.** See the assertion table; the offered test is shown to falsify the flag mutation.
- **FC-031.** The XML summary of `StartsBehindAnOpenBraceOnItsLine` now attributes the skipping of a missing brace to `GetPreviousToken`, which is true; the inline comment in `TryGetFixableAttributeList` (line 232) still reads true. Both new test summaries describe their inputs.
- **FC-008, FC-009, FC-026.** The scan's behavior is unchanged, so audit 29's convergence and registration-versus-apply evidence stands.
- **FC-035.** The new tests are characterization of behavior audit 29 measured correct; their expected outputs follow from SHARED-ATTR-017 and SHARED-ATTR-022, which the delta does not change.
- **FC-004 to FC-007, FC-010 to FC-016, FC-018 to FC-025, FC-028 to FC-030, FC-032 to FC-034, FC-036 to FC-043.** The delta edits no trivia, calls no formatter path, adds no owner, adds no guard, and declares no shape; it removes one always-true condition and adds two tests.

### Architecture findings
_None._

### Hints
- **Handover test counts.** Test implementer 30 and implementer 31 report 87 passed for the RH5514/RH5528 class filter, the cleaner 90 for a looser filter, and audit 29 quoted 87 for the head before the three new rows. Measured on the head: the class filter gives 87, and the three new rows run and pass (`FullyQualifiedName~NestedClosedBody`: 3 passed). The difference lies in how the filters count, not in a test that did not run. No action.

## Whole run

### Visible output is decided
- The offered expected value the delta adds (`    void M() { if (true) { } } [A]` / `    [B] void N() { }`) follows from the unchanged SHARED-ATTR-017 for a list that shares its line with other code (#1109 Q1 A). The withheld side is the rare-class withhold of decision 23, recorded in the change doc's *Decisions*.
- No rule page changed.

### Cost of special cases
The delta removes one dead condition and adds no production code. Nothing to report.

### Checks
- **Lane.** Confirmed (see *Scope*).
- **Source hygiene.** The `src` lines the delta adds hold no `TODO` and no tracker number.
- **Scope.** Everything changed falls under decision 25.

### Commands
- `scripts/lane-check.sh behavior-change`
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests|FullyQualifiedName~RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests"`
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~NestedClosedBody"`
