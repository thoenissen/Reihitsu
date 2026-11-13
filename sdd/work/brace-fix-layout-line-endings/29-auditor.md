Role: auditor
Commit: 809714efd3547a0a2ce5c91d0bae91c4e394e33c
Verdict: findings

The prompt named the audit's scope (the refusal of decisions 23 and 24) and the SHAs; it carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

## Scope

Delta audit of the structural refusal of decision 23 and the spec rewording of decision 24, against `e7a0c2e66ec29b33b1b7c617e313cab6333cdd28`, the head audit 21 checked. Not a full audit.

- **Head.** `809714efd3547a0a2ce5c91d0bae91c4e394e33c` checked out.
- **Base.** `git ls-remote origin refs/heads/main` gives `166e3dd2690edc7ed9cfd593d3e7d8eee8764c1b`; `git merge-base --is-ancestor` confirms it is merged.
- **Delta.** `git diff e7a0c2e..HEAD` matches handovers 22 to 27:
  - one code-fix base, `TargetAttributeListShapeCodeFixProviderBase.cs` (`StartsOnTheLineOfTheHoldingBrace` replaced by `StartsBehindAnOpenBraceOnItsLine`);
  - two test files (RH5514: 8 tests; RH5528: 1 test);
  - SHARED-ATTR-017 and SHARED-ATTR-022, the headings and *Why* of the 14 per-rule withhold requirements;
  - the change doc and the work files.
- **Scope growth.** None. Every change falls under decisions 23 and 24.
- **Evidence reused from audit 21.** Everything this delta did not touch: the brace fixes, RH5011, RH8304, RH7309, SHARED-ATTR-019, and SHARED-ATTR-023.
- **New evidence.**
  - Static tracing of the new scan and its tests.
  - `scripts/apply-fix.sh RH5514` on seven probe fixtures under LF and CRLF, in a scratch directory outside the repository (`…/scratchpad/fx29`). Results: 6 fixed, 8 no-fix-offered, 0 not-converged, 0 line-ending-drift.
- **Trusted results.**
  - Implementer 24: analyzer suite 8804/8804, architecture 117/117.
  - Cleaner 25: `Build succeeded`, 0 warnings; RH5514/RH5528 87/87.
- **Lane.** `scripts/lane-check.sh behavior-change` reports `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–7 | closed | Closed in audit 21. This delta does not touch them. |
| 8 | closed | All three listed members are now withheld, and each has a test: (a) a lambda in a statement on the block's `{` line (`SplitFixIsWithheldForALambdaInAStatementOnTheLineOfTheBraceOfTheBlock`), (b) a lambda in a field initializer on the type's `{` line (`…InAFieldInitializerOnTheLineOfTheBraceOfTheClass`), and (c) a local function in a switch section (`…InASwitchSectionOnTheLineOfTheBraceOfTheSwitchStatement`). The members audit 21 traced but did not run also hold now. RH5528 `[return: A, B]` on such a lambda is tested. A lambda in an expression-bodied member on a type's `{` line is withheld by the open `{` of the type (traced). Measured also: a lambda in an object initializer and in a switch expression on the statement's line are withheld (`{` open). The other side is offered, with exact output: a lambda in a multi-line block. |

## thoenissen/Reihitsu#1416

_Nothing to audit for this issue._ The delta touches no brace spec, code, or test. Audit 21's evidence stands.

## thoenissen/Reihitsu#1388

_Nothing to audit for this issue._ The delta touches no RH5011, RH8304, RH7309, or SHARED-FIX-008 spec, code, or test.

## thoenissen/Reihitsu#1627

_Nothing to audit for this issue._ The delta touches nothing it depends on.

## thoenissen/Reihitsu#1142

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | medium | `src/Reihitsu.Analyzer.Test/Formatting/RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests.cs` (the SHARED-ATTR-022 tests, lines 1178–1404) | FC-017, FC-027 | – | PR-introduced | **The gap.** SHARED-ATTR-022 pairs each `{` with "the `}` that closes that `{`". The scan implements the pairing with a counter of waiting `}` tokens. Every test that reaches the counter has at most one closed pair in front of the list: `void M() { } [A, B] void N() { }`, `$"{1}"`, and `C3`. No test has a nested closed pair. **What a wrong scan would do.** A scan that keeps a boolean "a `}` is waiting" instead of a count passes every current test. Yet it withholds the fix for `    void M() { if (true) { } } [A, B] void N() { }`, which the spec offers: the two `}` set the flag, the inner `{` clears it, and the outer `{` then counts as open. **The current code is right.** Measured with `apply-fix.sh`, LF and CRLF, the fix is offered with `    void M() { if (true) { } } [A]`⏎`    [B] void N() { }`. The test set does not pin it. | One offered test, LF and CRLF, with two nested closed bodies in front of the list on its line (the input and output above). Optionally add the withheld side `class C3 { void M() { if (true) { } } [A, B] void N() { } }`. |
| 10 | low | `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs:123` (`if (token.IsMissing == false)`) | – | – | PR-introduced | **Dead branch.** `SyntaxToken.GetPreviousToken()` defaults to `includeZeroWidth: false`, so it skips every token whose width is 0. A missing token has width 0, so it never reaches the loop, and the condition is always true. Implementer 24's handover says "Missing tokens are skipped". That describes what the API already does, not this branch. | Drop the `IsMissing` check and keep the two brace branches. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Split withheld behind an open `{` on the list's line (SHARED-ATTR-022) | Non-zero-width tokens in front of `attributeList.OpenBracketToken` whose span starts on the `[`'s line, walked backwards. A `}` adds one to a waiting count. A `{` returns true when the count is 0 and otherwise subtracts one. Trivia, skipped tokens, directive tokens, and documentation-comment tokens are not visited, because they are excluded from `GetPreviousToken()` by default | SHARED-ATTR-022, all three qualifiers. *Which `{`*: every token of that kind, including interpolation braces. *In front of the list on its line*: the token starts on the `[`'s line. *Not closed in front*: pairing by count, which in a parsed tree is the same as pairing by node, because braces nest | Agree | holds. Its tests are #9 |
| Same, at a multi-line token | The scan stops at the first token that starts on a line above, such as a raw or verbatim string closing on the list's line. Every earlier token also starts above | "Only a `{` whose token starts on the line of the reported list's `[` … counts" | Agree. Measured: `        """; [A, B] void N() { }` behind a raw string is offered | holds (see *Hints*) |
| Same, against the old enumeration | The old guard read only `attributeList.Parent.Parent`. The new scan sees every `{` on the line, so every member audit 14 (#2) and audit 21 (#8) named is a member of one condition | Decision 23 | Agree: a class refused as a whole | holds |
| Interpolation holes | A `{` of an interpolation in front of the list on its line is a token. If closed, the fix is offered (tested). If open, it is withheld (measured: a lambda inside `$"{…}"` in a one-line and in a multi-line block) | SHARED-ATTR-022 *Which `{`* | Agree. A side effect: the split no longer writes a line break into a non-verbatim interpolation hole that sits on one line, which C# before 11 rejects. A hole whose `{` stands on a line above already spans lines | holds |
| Registration vs. apply | `ApplySplitCodeFixAsync` does not re-check SHARED-ATTR-022. The action is created from the same tree the check read | FC-009 | Agree. The check only refuses | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "A `{` still open in front of the list on its line" | Only `StartsBehindAnOpenBraceOnItsLine` (`rg -n "ATTR-022" src --glob '!*Test*'` finds only the guard and its call-site comment). `TargetAttributePlacementCodeFixProviderBase.HasUnusableEnclosingScope` walks enclosing braces for another decision: missing braces, and the column check of SHARED-ATTR-021. `TrailingCommaCodeFixHelper` and `RegionDirectiveBlankLineCodeFixProviderBase` read a node's own brace, also for other decisions | one owner |
| Rule statement | SHARED-ATTR-022 states the condition. SHARED-ATTR-017 and the 14 per-rule requirements point to it by ID, with matching headings and *Why* (decision 24; `rg "body holding"` over `sdd/specs` finds nothing). The change doc's one remaining "body holding" is history in *Decisions* | one statement |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5514 `…InAFieldInitializerOnTheLineOfTheBraceOfTheClass`, `…InAStatementOnTheLineOfTheBraceOfTheBlock`, `…InASwitchSectionOnTheLineOfTheBraceOfTheSwitchStatement`; RH5528 `VerifyDiagnosticWithoutCodeFixForReturnSpecifierListOfALambdaOnTheLineOfTheBraceOfTheBlock` | The diagnostic is reported, and no action is registered | An action is registered (red on 04c09db, per 23) | `VerifyNoCodeFixIsOffered` | adequate |
| RH5514 `SplitFixIsWithheldBehindAClosedBodyWhileTheBraceOfTheClassIsStillOpenOnTheLineOfTheList` (`C3`) | A closed `{` in front does not cancel an outer open `{` | An action is registered: this falsifies a scan that looks only at the nearest brace | `VerifyNoCodeFixIsOffered` | adequate for one level; see #9 |
| RH5514 `SplitFixIsOfferedForALambdaInAStatementBelowTheLineOfTheBraceOfTheBlock` [LF/CRLF] | A `{` on a line above does not withhold; exact SHARED-ATTR-017 output | Withheld, or a different layout or line ending | `VerifySplitFix` (verifier + raw text) | adequate |
| RH5514 `…BehindABraceInsideAStringLiteral…`, `…InsideAComment…` [LF/CRLF] | The scan reads tokens, not text | Withheld: this falsifies a text scan | `VerifySplitFix` | adequate |
| RH5514 `…BehindAClosedInterpolation…` [LF/CRLF] | Interpolation braces pair like any other | Withheld: this falsifies a scan that ignores `}` | `VerifySplitFix` | adequate |
| (missing) a nested closed pair in front of the list | The pairing is a count, not a flag | – | – | finding #9 |

### Failure classes dismissed
- **FC-001, FC-002.** The three qualifiers of SHARED-ATTR-022 are explicit conditions of the scan; see the guard table.
- **FC-003.** Every qualifier is tested on both sides: open versus closed; on the list's line versus above; token versus literal or comment. The one-level limit is #9.
- **FC-015, FC-016.** There is one owner. All 16 split providers go through this base.
- **FC-008.**
  - The check reads only the original tree. A split in an earlier pass only adds line breaks behind lists that were offered, and a list offered on a line has no open `{` in front of it.
  - A later list on that line therefore sees a subset of the tokens it saw before, and the check cannot flip to "offered" in a way that changes the output.
  - Measured: every probe converged in one iteration.
- **FC-009.** See the guard table.
- **FC-004, FC-005, FC-019, FC-020, FC-021.** The delta only refuses; it edits no trivia. A directive cannot stand in front of the list on its line.
- **FC-006, FC-022.**
  - Literals: a brace inside one is not a token (tested).
  - Raw and verbatim strings that close on the list's line stop the scan (measured).
  - Interpolation: see the guard table.
- **FC-026.** The scan is positional; it re-resolves no token after an edit.
- **FC-027.** Every enclosing scope is covered, because the condition names no owner. Nesting of closed pairs is #9.
- **FC-031.**
  - The XML summary of `StartsBehindAnOpenBraceOnItsLine` describes the scan.
  - The inline comment in `TryGetFixableAttributeList` describes the new condition.
  - The test summaries of earlier SHARED-ATTR-022 tests still read true. The spec checker checked line 1178.
- **FC-035.** Test implementer 23 wrote the offered outputs from SHARED-ATTR-017 before implementer 24 changed the code. The four withheld tests were red on 04c09db.
- **FC-040.** `/** { */` in front of the list is a documentation comment, and its tokens are excluded from `GetPreviousToken()` in every documentation mode.
- **FC-007, FC-018, FC-023 to FC-025, FC-028 to FC-030, FC-032 to FC-034, FC-036, FC-037.** These are naming fixes, formatter paths, harness normalization, and grep proofs. The delta has none of them.
- **FC-010 to FC-014, FC-038 to FC-043.** The split fix does not call the formatter, and no shape is declared unsupported.

### Architecture findings
_None._

Decision 23's refusal removes the structural cause behind #2 and #8: the line of the open `{` was read at a fixed tree distance, and now one token scan decides it.

### Hints
- **A literal's continuation line in front of the list.**
  - Input: a list behind a raw or verbatim string that closes on the list's line (`        """; [A, B] void N() { }`).
  - Output: `[B]` lands at the literal's closing column (8), not at the member column (4).
  - That is the copy rule of SHARED-ATTR-017 for "the list shares its line with other code". It is the same rule as behind a wrapped expression's continuation line (`            2; [A, B] void N() { }`), and maintainer answer #1109 Q1 A already decided it.
  - It is not a member of the open-`{` class. It is rare, and it needs no action in this run.

## Whole run

### Visible output is decided
- **Offered sides.** The expected values that the delta adds (the lambda in a multi-line block; the literal, comment, and interpolation cases) follow from the unchanged SHARED-ATTR-017, which covers a list that shares its line with other code (#1109 Q1 A).
- **Withheld sides.** These are a rare-class withhold (decision 23), recorded in the change doc's *Decisions* section.
- **Change doc.** Its *Before* outputs for `C2`, `O`, and `P` match the `main` outputs that audit 21 measured.
- **Rule pages.** None changed (`git diff e7a0c2e..HEAD -- documentation` is empty).

### Cost of special cases
The delta replaces a four-arm owner switch with one line-bounded token scan of about 30 lines. It needs no new type and no new dependency. The scan is a cheap syntactic check, and no shape is declared, so FC-042 does not apply.

### Checks
- **Lane.** Confirmed (see *Scope*).
- **Source hygiene.** The `src` lines that the delta adds hold no `TODO` and no tracker number.
- **Scope.** Everything changed falls under decisions 23 and 24.

### Commands
- `scripts/apply-fix.sh RH5514 <scratch>/fx29` ran on seven probes:
  - a nested closed pair in front of the list;
  - a lambda inside an interpolation hole, in a one-line block and in a multi-line block;
  - a lambda in an object initializer;
  - a list behind a raw string's closing line;
  - a `}` from a line above in front of the list;
  - a lambda in a switch expression.
- `scripts/lane-check.sh behavior-change`.
