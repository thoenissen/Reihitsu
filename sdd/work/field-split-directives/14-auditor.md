Role: auditor
Commit: f179726e2753a7a068479848f4adaec81baf6310
Verdict: #1728: findings; #1586: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against cce9f0a (audit 1, 09-auditor.md). Head f179726 is checked out; base `origin/main` = eb89003 (`git ls-remote` agrees) and is an ancestor of the head. The delta matches handovers 10–13: `AGENTS.md` (one guard name in the `StructuralTransformPhase` row), `sdd/specs/formatter/unsupported-syntax.md` (one sentence in FMT-UNSUP-009's second tie bullet), three new tests in `UnsupportedFieldDeclarationDirectiveTests.cs`, and `sdd/work` files. No production code changed, and the spec text is a clarification of what FMT-UNSUP-010's last paragraph already decides, so scope did not grow and this is a delta audit. `scripts/lane-check.sh behavior-change`: confirmed. Evidence of audit 1 for everything outside the delta is byte-identical and reused.

Executed:
- Mutation (local, reverted with `git checkout`, never committed): in `PrecheckPhase.FindUnsupportedSyntax`, the `isDeclaratorComma` candidate list with `firstOperatorTrivia` moved ahead of `firstFieldDirective`. `scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"`: 328 passed, 0 failed. The mutation survives.
- `scripts/format.sh --check` on two fixtures in the scratchpad (outside the repository), on the unmutated head: a directive behind `?` in the first declarator's value plus a directive in front of the declarator comma on a later line, and a directive in front of an attribute-argument comma plus one in front of the declarator comma. Both reported `(4,1): unsupported syntax: a directive inside a field declaration with several declarators`, which is FMT-UNSUP-010's reason as its last paragraph demands. Current behavior is correct; only its test is missing.

## Previous findings

| # | Status | Note |
|---|---|---|
| 1 | open | Closed for the members the repair tested: the non-declarator list (FMT-UNSUP-010 over FMT-UNSUP-009 at an attribute-argument, tuple-type, and value comma, and over FMT-UNSUP-008 behind `?` and `:`), with the one-declarator counter-side. The test implementer's fail check covers that list. Still open for the member the finding named as "in both lists it goes ahead of `ConditionalOperatorCommentReason`": in the `isDeclaratorComma` list, the order of `firstFieldDirective` before `firstOperatorTrivia` is pinned by no test (mutation above). See the row below. |
| 2 | closed | `AGENTS.md` now names `FieldDeclarationSplitTransform.HasDisabledTextInLeadingTrivia`, which exists (`FieldDeclarationSplitTransform.cs:78`). |

## thoenissen/Reihitsu#1728

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 (open) | medium | `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs:89-96` (the `isDeclaratorComma` list); `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedFieldDeclarationDirectiveTests.cs` | FC-003, FC-017 | – | PR-introduced | `isDeclaratorComma` is a property of the first comma-gap in the whole node, not of the tied trivia. When a combined field has a directive behind `?` or `:` in a value and, later in the text, a directive in front of a declarator comma on a later line, the declarator list is used and its order decides the tie at the operator directive. Input: `class C⏎{⏎    private int _a = true ?⏎#pragma warning disable CS0162⏎        1 : 2⏎#pragma warning disable CS0414⏎        , _b;⏎}`. FMT-UNSUP-010 ("Every other directive in the declaration gives this requirement's reason") demands its reason at the first `#pragma`, and the head gives it. But swapping the last two entries of that list (making it FMT-UNSUP-008's reason) passes all 328 precheck tests. The new 008-tie rows hold no declarator-comma directive, so they reach only the other list. | Add one `AssertUnsupportedFor(FieldDeclarationDirectiveReason, …)` row (or test) with the input above, expecting the location of `#pragma warning disable CS0162`; a `:` variant is optional. |

### Requirements checked
- FMT-UNSUP-009 second tie bullet (clarified): consistent with FMT-UNSUP-010's last paragraph and with the code (non-declarator list: `firstFieldDirective` first). It names only the reason; the location is the shared starting trivia either way.
- FMT-UNSUP-008 has no tie paragraph of its own. Its only tie is with FMT-UNSUP-010, which FMT-UNSUP-010 decides. FMT-UNSUP-004 refuses only comments outside values, and FMT-UNSUP-009 cannot share a trivia with FMT-UNSUP-008 because a comma never directly follows `?` or `:`.
- The new rows' inputs parse without errors (disabled text holds the inactive branch), and each first directive is the trivia both shapes start at. The tuple-type and attribute rows have a non-declarator first comma gap, so they exercise the non-declarator list as intended.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Tie order of reasons, non-declarator list (FMT-UNSUP-010 over 009 and 008) | Candidate order at an equal `FullSpan.Start` | FMT-UNSUP-010 last paragraph; FMT-UNSUP-009 second bullet | Agrees; now tested (fail check in 10) | ok |
| Tie order of reasons, declarator list (FMT-UNSUP-010 over 008) | Candidate order when the node's first comma gap is a declarator comma | FMT-UNSUP-010 last paragraph | Agrees (CLI run); untested | finding #1 |
| No guard or predicate changed in the repair | — | — | — | _N/A — no production code in the delta._ |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Unsupported shapes and their tie order | `PrecheckPhase.FindUnsupportedSyntax` only; unchanged since audit 1 | one owner |
| Field split kept whole for disabled text in front | `FieldDeclarationSplitTransform.HasDisabledTextInLeadingTrivia` (private, `FieldDeclarationSplitTransform.cs:78`), now also named in `AGENTS.md` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DirectiveInFrontOfAnotherCommaOnALaterLineGivesTheReasonOfThisRequirement` (4 rows) | FMT-UNSUP-010's reason and location win over FMT-UNSUP-009 at a non-declarator comma | FMT-UNSUP-009's reason; other span; formatted output | `AssertUnsupportedFor` (LF/CRLF, unchanged text, reason, span) | adequate; fails under the non-declarator-list mutation (10) |
| `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirement` (3 rows) | FMT-UNSUP-010's reason wins over FMT-UNSUP-008 when no declarator-comma directive is present | FMT-UNSUP-008's reason | `AssertUnsupportedFor` | adequate for the non-declarator list; the declarator-list member is finding #1 |
| `DirectiveAtTheSamePlaceOfAFieldWithOneDeclaratorGivesTheReasonOfTheOtherShape` (4 rows) | With one declarator, FMT-UNSUP-009 or FMT-UNSUP-008 gives the reason | FMT-UNSUP-010's reason | `AssertUnsupportedFor` | adequate |

### Failure classes dismissed
All dismissals of audit 1 stand: the delta changes no production code, no phase, no code fix, and no visible output. For the delta:
- FC-001, FC-002: no predicate changed.
- FC-031: the test class summary and the three test summaries describe what the tests assert. The FMT-UNSUP-009 bullet and the `AGENTS.md` row match the code.
- FC-032, FC-035: the expected values come from FMT-UNSUP-010's last paragraph, not from observed output.
- All other classes: the delta has no code path through which they could arise.

### Architecture findings
_None._ Finding #1 is a missing test of one tie member, not a structural defect.

### Hints
- The global `isDeclaratorComma` flag selects the whole tie order from the first comma gap in the node. It agrees with the spec for every tie I traced, because a tie with the separator candidate can only happen at that gap, and the declarator list puts FMT-UNSUP-010 before FMT-UNSUP-008 just as the other list does. A per-tie decision would remove the need for two lists. Cosmetic; not a finding.

### Follow-up candidates
_None_ beyond 00-lead.md's.

## thoenissen/Reihitsu#1586

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | None | – |

The repair touches nothing of the split side (FMT-STRUCT-022, FMT-PIPE-031, RH7101-005, `FieldDeclarationSplitTransform`, the RH7101 fix). Audit 1's pass stands.

### Guard scope, Policy ownership, Assertion adequacy, Failure classes dismissed
_N/A — unchanged since audit 1 (09-auditor.md); the delta does not reach them._

### Visible output is decided
_N/A — the delta adds expected values only for the CLI/precheck reason text, which FMT-UNSUP-010 (unchanged by the repair) demands, and changes no rule page._

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## Whole run (delta)

- No `TODO`, no commented-out code, and no tracker reference in the added lines.
- Scope: no edit outside the run's tests, spec, `AGENTS.md`, and `sdd/work`.
- The mutation was reverted. `git status` showed a clean tree before this handover. Fixtures were kept in the scratchpad.
