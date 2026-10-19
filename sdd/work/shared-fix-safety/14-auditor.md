Role: auditor
Commit: 2e5aad7b838267e4eee50a6425fdc77b3c92fe09
Verdict: findings

My prompt carried the issue, the slug, the SHAs, the handovers and decisions to read, and the maintainer's constraints. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of `git diff 3b113db HEAD -- . ':(exclude)sdd/work'`.

State checks:

- The checkout is `2e5aad7`. `git ls-remote origin refs/heads/main` returns `3b113db4e563067c9247c15936f85cf601ad3c95`, which is an ancestor of HEAD through merge commit `5efd029`.
- The diff is what the handovers describe:
  - `AGENTS.md`, one sentence;
  - `sdd/specs/requirement-ids.txt`, three IDs;
  - `sdd/specs/shared/code-fixes.md`;
  - five files under `squad/decisions/`;
  - 11 changed test files and one new one, all under `src/Reihitsu.Analyzer.Test`.
- `scripts/lane-check.sh tooling` prints "confirmed — claimed tooling, diff needs tooling".

The maintainer's constraints hold:

- no file under `sdd/changes/`;
- no change outside the test project under `src/`;
- no `sdd/work/coordinator/` change;
- no requirement beyond SHARED-FIX-001 (clarified) and SHARED-FIX-003, -004, -005;
- the non-goals changed as item 5 of the issue asks;
- the `AGENTS.md` sentence is the issue's text word for word.

The ledger is in `LC_ALL=C` order, has no duplicates, and keeps every ID from `origin/main`. SHARED-DIAG-003 is not on `origin/main`, so the alignment that `2026-10-18-shared-fix-003-skeptic-scope.md` answer 4 requires falls to thoenissen/Reihitsu#1069 if this PR merges first. The region-name tab change doc of thoenissen/Reihitsu#1083 is not on `origin/main` either, so the absent exception matches the issue comment.

FC-032: the merged commit `3b113db` (#1104) changes only tests, specs, and rule docs. No production file changed between the test runs in `06`, `09` and `11` and HEAD, so those results still apply. I did not re-run the test suite.

The issue's three examples are not in the spec, because they fail today (`04-spec-author.md`, "Where the shipped text differs from the issue"). The normative sentences still cover each one, so this is a recorded deviation, not a missing requirement. The PR must state it (`AGENTS.md`, *Issue intake*).

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/shared/code-fixes.md`, SHARED-FIX-003 "Example — another run-time change, for RH7103"; `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceField` | FC-017 | PR-introduced | **Spec.** The example's one block mixes `_a` (fix offered) and `_b` (no fix) and has no result block. `sdd/README.md` (Spec format) requires one: "When one block mixes diagnostics that get a fix with diagnostics that do not, the second block shows the result."<br><br>**Test.** Without an expected text, the covering test asserts only `Assert.IsNotEmpty(actions)` and the order of three substrings, so it cannot see anything else the action changes. I ran `scripts/apply-fix.sh RH7103 <fixture> --max-iterations 1` on the example, LF and CRLF. The one application moves `_a`, and it also rewrites the unrelated `private static int Compute() => 1;` into a block body: the type-wide reformat that thoenissen/Reihitsu#969 tracks. The test passes with that rewrite in the result.<br><br>`IsNotEmpty` is the same weakening that finding 1 of `08-spec-checker.md` had reverted in the sibling test. The provider registers one action per diagnostic (`TypeMemberOrderingCodeFixProviderBase.RegisterCodeFixesAsync`). | **Spec author:** add the result block, choosing an input whose result rewrites no member the diagnostic is not about.<br><br>**Test implementer:** assert `HasCount(1)` and the exact text of that block. |
| 2 | low | `RH4106PrivateFieldCasingAnalyzerTests.NoCodeActionIsRegisteredWhenTheRenameWouldCauseACompileError` | — | PR-introduced | This test duplicates the existing `NoFixIsOfferedWhenTheNewNameIsTaken`, which tests the same input with the class named `D`. The existing test reports through markup and runs `Verify(testCode, testCode)`. With zero expected iterations, that "will validate that no code fixes are offered" (Microsoft.CodeAnalysis.Testing 1.1.4, `NumberOfIncrementalIterations`). A registered action here would rename and change the document, so both tests make the same observation. Elsewhere this run tagged the existing test when it was the spec example (RH5604, RH4106 `nameof`). | Tag the existing test with SHARED-FIX-003 and drop the duplicate. Keep the `_1st` test (SHARED-FIX-005): its `GetCodeFixActionsAsync` assertion also fails on a registered action that changes nothing. |

## Guard scope

The diff adds no code predicate. The table checks each bound that SHARED-FIX-003, -004 and -005 add against the code that decides the same thing today. Disagreements already filed as bugs are recorded, not raised again.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Compile error (SHARED-FIX-003, condition 1) | the whole result | `CasingCodeFixProviderBase{T}.HasRenameConflictAsync`: Renamer conflicts and added errors in the binding domains (SHARED-CASING-009); RH2005 has no guard | Casing fixes agree on the tested collision. RH2005 does not: outside references stop compiling (thoenissen/Reihitsu#967). | ok; violation filed |
| Run time: initializer order | moved and crossed members | `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder`, through the `IsMoveSafe` overrides of RH7103 and RH7109 (SHARED-ORDER-007) | RH7103 and RH7109 agree (the `_a`/`_b` tests). RH7309 swaps whole regions with no such check; see Hints. | ok for this diff |
| Run time: instance-field order that sets a layout | instance fields of a default struct, or of a type with `[StructLayout(LayoutKind.Sequential)]` | none in RH7109, RH7103, or RH7309 | Disagrees on the triggering side, as the skeptic-scope decision answer 3 records. Non-triggering side tested (Explicit, Auto, class). | ok; the follow-up has no issue yet (Hints) |
| Run time: new name or accessibility does not count | the renamed symbol, the changed modifier | none needed | agree (`FixRenamesFieldReferencedInNameof`, `CodeFixInteractionTests`) | ok |
| Text a diagnostic of the fix's own rule reports | that rule's reported spans | per rule, no shared predicate | RH5601, RH5604, RH8107 and RH7302 agree on the tested inputs | ok |
| Doc-comment name that binds to the renamed symbol | doc-comment references | `Renamer.RenameSymbolAsync` with default options (references, not comment words) | agree: `cref` renamed, the summary word kept | ok |
| `#endregion` comment that repeats the region name | the `#endregion` text | RH7302's ordinal substring replace | Agrees for an exact name and for a comment without the name. Disagrees for a substring (`cat` → `cAt`), which is thoenissen/Reihitsu#1110 Q2. | ok; open question filed |
| Layout does not count | whitespace around comments, directives, and doc-comment XML tags; raw-literal indentation | formatter phases, used through `FormatNodeInDocumentAsync` | RH3202 and RH8107 agree. RH5011 inside `/* */` does not (thoenissen/Reihitsu#1075). | ok; violation filed |
| `#if`-excluded code naming a changed name | disabled text | none in the casing base | Disagrees on the triggering side (thoenissen/Reihitsu#1073). Non-triggering side tested. | ok; violation filed |
| No-op fix (SHARED-FIX-005) | the result compared with the input | `CasingCodeFixProviderBase{T}.TryGetFixedIdentifier`, which requires the identifier to differ | Agree for `_1st`. RH5023 at the end of the file does not (thoenissen/Reihitsu#1076). | ok; violation filed |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| What makes a fix unsafe | SHARED-FIX-003; the `AGENTS.md` *Code-fix coverage* bullet; `documentation/rules/general-notes.md`; SHARED-DIAG-003 (thoenissen/Reihitsu#1069, not on `origin/main`) | `AGENTS.md` and `general-notes.md` also name relocating, reordering and dropping comments, directives, and disabled text. The 2026-10-19 decision (answer 1) keeps that gap as a follow-up. The SHARED-DIAG-003 alignment is decided (skeptic-scope, answer 4). Consistent. |
| Initializer order on a move | SHARED-ORDER-007 and `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` (`Reihitsu.Core`), used by RH7103 and RH7109 | One owner. RH7309 is a consumer that does not use it (Hints). |
| Collision or compile error on a rename | SHARED-CASING-009 and `CasingCodeFixProviderBase{T}` | A casing-specific instance of SHARED-FIX-003, condition 1. No contradiction. |
| A fix that changes nothing | SHARED-FIX-005; SHARED-CASING-008 and `TryGetFixedIdentifier` | A specific instance of the general rule. Consistent. |
| Per-rule convergence | SHARED-FIX-001; FC-007 (catalog) | RH5022-003/-004, the matching RH5024 to RH5027 requirements, and SHARED-WS-001 now contradict it for a run. This is known and decided under reading A of thoenissen/Reihitsu#1076. See Hints. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5002 `OneApplicationCorrectsTheLineInFrontOfTheReportedIf` (LF, CRLF) | one application, nothing reported again | a second iteration, or a different text | `Verify` with exactly 1 iteration (positive is exact, per the framework docs) | adequate |
| RH5028 `OneApplicationRemovesTheWholeReportedRun` (LF, CRLF) | the whole run goes in one application | a blank line left, or a second iteration | `Verify`, exactly 1 iteration | adequate |
| RH5023 `OneApplicationPerDiagnosticCorrectsTheRun…` (4 tests) | one application per diagnostic; the first blank line is not reported | a third iteration, or an RH5023 diagnostic in the fixed state | `Verify`, exactly 2 iterations | adequate |
| RH4106 `NoCodeActionIsRegisteredWhenTheRenameWouldCauseACompileError` | collision: reported, no action | an action registered | `Verify` plus `GetCodeFixActionsAsync` (span-based; `RegisterCodeFixesAsync` reads only `Location.SourceSpan`) | adequate; duplicate (finding 2) |
| RH4106 `FixIsOfferedWhenTheRenameCausesNoCompileError` | the other side of the collision | no fix, or a different text | `Verify` | adequate |
| RH4106 `FixRenamesFieldReferencedInNameof` (tagged) | the new name is not a run-time change | `nameof` left stale, or the fix withheld | `Verify` | adequate |
| RH4106 `FixRenamesTheBoundDocumentationReferenceAndKeepsTheCommentText` | the bound `cref` is renamed and the look-alike word kept | the summary word renamed, or the `cref` stale | `Verify` | adequate |
| RH4106 `FixLeavesAnExcludedBranchThatDoesNotNameTheFieldUnchanged` | the excluded branch is unchanged | any edit to the disabled text | `Verify` | adequate (non-triggering side only, by constraint) |
| RH4106 `NoCodeActionIsRegisteredWhenTheFixWouldNotChangeTheDocument` | no no-op action | any registered action, even one that changes nothing | `Verify` plus `GetCodeFixActionsAsync` | adequate, and stronger than `Verify(t, t)` |
| RH7103 `NoCodeActionIsRegisteredWhenTheMoveWouldChangeTheRunTimeValueOfAField` | both fields reported; `_b` has no action | an action for `_b` | `Verify` plus `GetCodeFixActionsAsync` | adequate. It overlaps `NoCodeFixWhenMoveChangesStaticInitializerExecutionOrder`, but adds the "both reported" assertion the spec states. |
| RH7103 `CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceField` | `_a` has an action, and the result is safe | any change outside the three asserted lines, such as the observed `Compute` rewrite, or a second action | `IsNotEmpty` plus substring order | weak (finding 1) |
| RH7103 `CodeFixIsOfferedWhenCrossedMemberBodyContainsActiveBalancedConditional` (repaired) | one action, the exact move | a second action, or a different text | `HasCount(1)` plus exact text | adequate |
| RH7109 Explicit, Auto, and class tests | a reorder is offered where field order sets no layout | the fix withheld, or `[FieldOffset]` left behind | `Verify` | adequate |
| RH7302 `VerifyCodeFixRenamesOnlyTheEndRegionCommentThatRepeatsTheRegionName` | a repeating `#endregion` comment is renamed; another one is kept | `// helpers` changed, or `// members` stale | `Verify` | adequate |
| RH3202 `VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals` | only layout changes | a changed word or literal value | `Verify` | adequate |
| RH5601 `VerifyCodeFixKeepsTabInsideTheTextOfATrailingComment` (LF, CRLF) | the tab in the comment text is kept and not reported | the inner tab replaced or reported | `Verify` | adequate |
| RH5604 `VerifyCodeFixConvertsEveryLineEndingWhicheverDiagnosticItIsAppliedTo` (tagged) | reported line breaks rewritten; the literal's `\r\n` kept | the literal changed | `GetCodeFixActionsAsync`, `HasCount(1)`, exact text | adequate |
| RH8107 `VerifyCodeFixChangesOnlyTheReportedElementOfTheDocumentationComment` | only the reported element (and the layout space) removed | a changed summary | `Verify` | adequate |
| `CodeFixInteractionTests.RH4106FixDoesNotRestoreTheCodeTheRH2005FixStartedFrom` | the RH4106 fix does not restore `public string first` | RH4106 output differs from `private string _first`, or RH2005 reports it | three verifiers in sequence, exact text, same configuration as the base `Verify` | adequate for the one pair |

## Failure classes dismissed

- **Guards in code: FC-001, FC-002, FC-003, FC-015, FC-016.** No code guard or predicate changed. As spec qualifiers they are traced under Guard scope.
  - FC-002: every qualifier is an explicit condition of the text.
  - FC-003: the triggering sides of the layout, comment-content, and `#if` qualifiers are known bugs, untested by the maintainer's constraint.
  - FC-016: one sibling outside the follow-up list; see Hints.
- **Trivia: FC-004, FC-005, FC-019, FC-020, FC-021.** No rewrite was added. The tests carry the trivia the examples name: `#if` and disabled text, a trailing comment, doc comments, and `#endregion` comments.
- **Rewrites: FC-006, FC-022 to FC-027, FC-029.** No production change. Raw strings (RH3202) and a verbatim literal (RH5604) are pinned exactly.
- **FC-036.** The new name not counting as a run-time change is a maintainer decision (`2026-10-18-shared-fix-003-rename-not-runtime-change.md`).
- **Code fixes: FC-007, FC-008, FC-009, FC-018, FC-030.** No fix changed.
  - FC-009: covered by the empty-action assertions.
  - FC-018: the `cref` test.
  - FC-030: observed but pre-existing (thoenissen/Reihitsu#969); finding 1 is where it hides a weak test.
- **Formatter: FC-010 to FC-014, FC-028.** No formatter or code-fix change.
- **FC-031.** Every new or changed summary describes its test. No `TODO`, no commented-out code, and no tracker number in the source diff.
- **FC-032.** The merge brought no production change.
- **FC-033.** The CRLF variants go through `CSharpCodeFixTest` with explicit `\r\n` strings, and Microsoft.CodeAnalysis.Testing compares the text without normalizing it.
- **FC-034.** No bug reproduction in this run.
- **FC-035.** No implementation was patched. The expected values are the spec blocks, checked against the decisions.
- **FC-037.** No text-only claim was made.

## Hints

Follow-up candidates and uncertain observations. They do not block.

- **RH7309 is an untracked SHARED-FIX-003 run-time violation (FC-016).** It is not in any follow-up list of this run. `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider` swaps whole region texts, and `CanReorderSafely` checks only directives. On

  ```csharp
  using System;

  public class C : IComparable
  {
      #region IComparable

      private static readonly int Y = X + 1;

      public int CompareTo(object obj) => Y;

      #endregion // IComparable

      #region Fields

      private static readonly int X = 5;

      #endregion // Fields
  }
  ```

  `scripts/apply-fix.sh RH7309` (LF and CRLF) moves `Fields` first. `Y` then becomes `6` instead of `1`. The same swap can reorder the instance fields of a default struct. No open issue covers it.
- **No issue for the RH7109 field-layout follow-up.** I found none for skeptic-scope answer 3 (default struct, and a class with `[StructLayout(LayoutKind.Sequential)]`).
- **The FC-004 follow-up (2026-10-19, answer 1) should also name disabled text.** The decision words the gap as "relocating or reordering". Condition 3 of SHARED-FIX-003 also names no disabled text (`#if`-excluded code), so a fix that edits or drops it violates no condition, unless it renames a name there (condition 4).
- **Non-goals in other specs that SHARED-FIX-003 now decides.**
  - The `RH2005.md` non-goal on outside references (thoenissen/Reihitsu#967): condition 1 now decides it.
  - The `RH7103.md` non-goal on how much of the type the fix reformats (thoenissen/Reihitsu#969): changed words are now decided.

  Both still read "open question".
- **thoenissen/Reihitsu#1076 names no spec changes.** Its text states the decision ("each fix removes the whole run it reports") but not that RH5022-003/-004, the matching RH5024 to RH5027 requirements, and SHARED-WS-001 must change. Until then, those enforced specs contradict the clarified SHARED-FIX-001.
- **The doc-comment layout carve-out is the spec author's reconciliation.** `05-spec-author.md` derives it from skeptic-scope answer 2, and decision A's question was about `//`, `/* */` and directives. Decision A's answer reads "a comment" without a kind. The lead may want the maintainer to confirm it, as was done for `LayoutKind.Auto`.
- **`CodeFixInteractionTests` derives from no code-fix test base.** `AGENTS.md` says a test class that verifies a code fix derives from `BatchCodeFixTestsBase` or `SingleCodeFixTestsBase`. That convention decides Fix All coverage per provider, which the RH2005 and RH4106 classes already carry, and a two-rule chain cannot use a single-pair base. Not a finding.
- **A net change to a test outside the run's requirements.** The repaired RH7103 active-conditional test now also asserts the exact move. It is a strengthening; the PR should name it.
- **The PR description should carry:**
  - the dropped issue examples;
  - the follow-ups listed above;
  - the SHARED-DIAG-003 alignment note (skeptic-scope, answer 4).
