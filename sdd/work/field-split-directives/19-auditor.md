Role: auditor
Commit: 93f8866daa7f053d0b6e534f791e545aeda54ab9
Verdict: pass

#1728: pass. #1586: pass.

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against f179726 (audit 2, `14-auditor.md`). Head 93f8866 is checked out. The base `origin/main` is eb89003, matching the prompt after `git fetch`, and it is an ancestor of the head. The delta outside `sdd/work` matches handovers 15–18:
- `UnsupportedFieldDeclarationDirectiveTests.cs`: one new row in `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirement`, plus three new tests.
- `sdd/specs/formatter/unsupported-syntax.md`: two clarifying sentences, one in FMT-UNSUP-009's first tie bullet and one in FMT-UNSUP-010's tie paragraph (decision 4).

No production code changed. Both spec edits write down behavior the code and `main` already have (decision 4). Scope did not grow, so this is a delta audit. `scripts/lane-check.sh behavior-change` confirmed the lane. Evidence of audits 1 and 2 for everything outside the delta is byte-identical and reused.

Executed:
- I re-ran audit 2's surviving mutation. In `PrecheckPhase.FindUnsupportedSyntax`, I moved `firstOperatorTrivia` ahead of `firstFieldDirective` in the `isDeclaratorComma` list. Then I ran `scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"`. Result: 7 failed, 331 passed. The failures were all 6 rows of the new conditional-operator/declarator-comma test, plus `FindUnsupportedSyntaxGivesThisRequirementsReasonBehindAConditionalOperatorForTheDocumentAndTheField`. The mutation is now killed. I reverted it with `git checkout`, and `git status` was clean afterwards.
- The test implementer's other five fail checks (`15-test-implementer.md`) were not re-run. Each one maps to rows I traced below, and none of my findings depends on them.

## Previous findings

| # | Status | Note |
|---|---|---|
| 1 | closed | The member that audit 2 left open is the FMT-UNSUP-010 vs FMT-UNSUP-008 tie order in the declarator-comma list. Test (6) now pins it, and row 1 of that test is audit 2's own input. Test (7) pins it through the root node. The surviving mutation now fails 7 tests (above). Audit 2 named no other member that still fails. |
| 2 | closed | Closed in audit 2. Unchanged. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | None | – |

### Class enumeration of #1 (classes, not instances)

A tie can decide a reason only when the tied trivia starts earliest of all candidates. Comma gaps are disjoint and in text order, and `FindTriviaInFrontOfComma` returns the first qualifying gap. So a FMT-UNSUP-009 tie happens only at that first gap, and the flag then comes from that same comma. A FMT-UNSUP-008 tie takes its list from whatever first gap follows in the node. I traced every member of the axes the test implementer listed against the code and the spec; each holds:

- **010 ∧ 009 at the field's own declarator comma (D1).** Expected FMT-UNSUP-009. Covered by existing tests (1)–(3).
- **010 ∧ 009 at a local's declarator comma inside a value (D2).** Expected FMT-UNSUP-009, and `comma.Parent is VariableDeclarationSyntax` gives that. Covered by new test (5). In its conditional-group row, `DEBUG` is undefined, so `, y = 2` is disabled text and the `#else` branch's comma is the gap's comma. The input is valid, and `#if DEBUG` is the first trivia of both the gap candidate and the field-directive candidate.
- **010 ∧ 008, flag set by a later declarator comma (D3–D6).** Expected FMT-UNSUP-010. The flag's comma can be in the same field (directive or comment in front of it), in a later field of the document, or in a later local inside a lambda value. In the later-field row, the first field's `2, _b` comma has no gap trivia, so the later `_c // c , _d` gap sets the flag. In the lambda row, the inner `// c` is also a FMT-UNSUP-004 candidate of the local declaration, but it starts after the pragma, so it does not change the result.
- **010 ∧ 009 at a non-declarator comma (N1).** Expected FMT-UNSUP-010. Covered by existing test (8).
- **010 ∧ 008 with no gap, or with a later non-declarator gap (N2, N3).** Expected FMT-UNSUP-010. In the new row of (4), `// c` is in front of a comma of `Math.Max` inside the value. FMT-UNSUP-004 excepts it (it is inside the value), and it sets the flag to false.
- **Node axis (D5/N4).** Test (7) runs the same text on the root (declarator list) and on the field alone. On the field, no comma gap qualifies, so the other list is used. LF and CRLF are both run.
- **Members that cannot occur, confirmed.**
  - 010 ∧ 004: FMT-UNSUP-004 finds comments only, and FMT-UNSUP-010 finds directives and disabled text only, so they never start at the same trivia.
  - 008 ∧ 009: a comma never directly follows `?` or `:` in valid code.
  - A tie at disabled text: the opening directive always precedes the disabled text in the same trivia list.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Tie order, declarator list (009 over 010; 010 over 008) | Candidate order when the node's first qualifying comma gap is a declarator comma | FMT-UNSUP-010 tie paragraph; FMT-UNSUP-009 first bullet (both clarified) | Agrees. Tested both ways: D1/D2 rows and D3–D6 rows; the mutation is killed | ok |
| Declarator comma covers a local declaration inside a value | `comma.Parent is VariableDeclarationSyntax`, for any variable declaration in the node | FMT-UNSUP-010: "any variable declaration inside the field declaration"; FMT-UNSUP-009 first bullet: "also where the declaration is a local declaration inside a value" | Agrees. The spec now names exactly what the predicate checks (field, local, and also `for`, `using`, and `fixed` declarations, which all have `VariableDeclarationSyntax` as the comma's parent) | ok |
| No guard or predicate changed in the repair | — | — | — | _N/A — no production code in the delta._ |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Unsupported shapes and their tie order | `PrecheckPhase.FindUnsupportedSyntax` only (callers: `PrecheckPhase.Execute`, `ReihitsuFormatter.FindUnsupportedSyntax`). Unchanged by the repair | one owner |
| Tie wording in the spec | FMT-UNSUP-009 first and second bullets; FMT-UNSUP-010 tie paragraph. All three now state the same split: declarator comma → 009, everything else → 010 | consistent |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirement`, new row "with a comment in front of a later comma of a call" | FMT-UNSUP-010 wins over 008 when the flag is false because of a later non-declarator gap | FMT-UNSUP-008's reason; another span; formatted output | `AssertUnsupportedFor` (LF/CRLF, unchanged text, reason, span) | adequate |
| `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirementBeforeALaterDeclaratorComma` (6 rows) | FMT-UNSUP-010 wins over 008 in the declarator list | FMT-UNSUP-008's reason | `AssertUnsupportedFor` | adequate; kills audit 2's mutation |
| `DirectiveInFrontOfADeclaratorCommaOfALocalInAValueKeepsTheReasonOfAListSeparator` (2 rows) | FMT-UNSUP-009 wins over 010 at a local's declarator comma inside a value | FMT-UNSUP-010's reason | `AssertUnsupportedFor` | adequate (the test implementer's "list D: 010 ahead of 009" mutation fails both rows) |
| `FindUnsupportedSyntaxGivesThisRequirementsReasonBehindAConditionalOperatorForTheDocumentAndTheField` | Same reason and location for the document (declarator list) and the field alone (other list) | Another reason on either node | `AssertReportsShape`: non-null result, reason equals `FieldDeclarationDirectiveReason`, location equals the shape's span; LF and CRLF | adequate |

## Visible output is decided
The expected values are the precheck/CLI reason and location:
- FMT-UNSUP-010 reason for D3–D6/N3/N4: demanded by FMT-UNSUP-010, whose substance is the maintainer's Q2 answer. The repair's sentence only spells out "every other directive".
- FMT-UNSUP-009 reason for D2: equal to `main`'s output for that input. `main` has no FMT-UNSUP-010, so FMT-UNSUP-009 gives it, and decision 4 records the CLI run.

No rule page changed. Nothing is undecided.

## Failure classes dismissed
All dismissals of audits 1 and 2 stand: the delta changes no production code, no phase, no code fix, and no visible output. For the delta:
- FC-001, FC-002, FC-044: no predicate or guard changed.
- FC-003: both sides of the declarator-comma flag and both nodes are now tested (above).
- FC-017: the helpers assert exact reason and span on LF and CRLF (above).
- FC-031: the four new or extended test summaries describe what their rows assert. The unchanged inline comment in `FindUnsupportedSyntax` ("FMT-UNSUP-009 for a directive, in front of a comma between two declarators") still matches the code and the clarified spec.
- FC-032, FC-035: the expected values come from the spec text, and the code was unpatched when the tests were written (15). The mutation run shows the tests can falsify the code.
- FC-042: no guard or spec clause was added that serves only a declared shape. The spec sentences state tie reasons inside FMT-UNSUP-009/010 themselves.
- FC-043: no code fix is touched.
- All other classes: the delta has no code path through which they could arise.

## Architecture findings
_None._ Decision 3 records that finding #1's repeat is handled narrowly. The repair enumerated every tie member by cause, and no member fails.

## Hints
- Audit 2's hint stands. The global `isDeclaratorComma` flag selects the tie order for the whole node, and it agrees with the spec for every member traced above. A per-candidate decision would remove the two lists. Decision 3 declined this as cosmetic; I am not re-raising it as a finding.

## Follow-up candidates
_None_ beyond those in `00-lead.md`.

## thoenissen/Reihitsu#1586

The repair touches nothing on the split side: FMT-STRUCT-022, FMT-PIPE-031, RH7101-005, `FieldDeclarationSplitTransform`, and the RH7101 fix are unchanged. Audit 1's pass stands.

## Whole run (delta)
- The added lines contain no `TODO`, no commented-out code, and no tracker reference.
- Scope: the delta edits only tests, `unsupported-syntax.md`, and `sdd/work`.
- The mutation was reverted, and the tree was clean before this handover.
