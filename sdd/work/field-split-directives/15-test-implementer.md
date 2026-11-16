Role: test implementer
Commit: f6df1fa
Verdict: tests written

Repair round 2, auditor finding #1 (#1728), decision 3 in `00-lead.md`: enumerate the whole class, every way FMT-UNSUP-010 can start at the same directive as another FMT-UNSUP shape, across every path of `PrecheckPhase.FindUnsupportedSyntax` that orders the candidate reasons. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code was changed.

Expected values come from `sdd/specs/formatter/unsupported-syntax.md`: FMT-UNSUP-010's tie paragraph (a directive "between a declarator and a comma on a later line" gives FMT-UNSUP-009's reason and location; "every other directive in the declaration gives this requirement's reason"), and FMT-UNSUP-009's two tie bullets.

## The class

The order is decided in one place, `PrecheckPhase.FindUnsupportedSyntax` (`src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs:89-103`). It picks one of two candidate lists from `isDeclaratorComma`, and the first entry in the list wins when two candidates start at the same trivia. `isDeclaratorComma` is a property of the node's first comma gap that holds a comment, directive, or disabled text and a line break, and that gap's first trivia is the separator candidate.

The axes:

- **Which shape ties with FMT-UNSUP-010.** FMT-UNSUP-010 finds only directives and disabled text.
  - FMT-UNSUP-004 finds only comments, so it never starts at the same trivia. No tie.
  - FMT-UNSUP-009 ties at a directive in front of a comma on a later line.
  - FMT-UNSUP-008 ties at a directive behind `?` or `:`.
  - FMT-UNSUP-008 and FMT-UNSUP-009 never share a trivia, because no comma follows `?` or `:` directly in valid code. So no three-way tie exists.
- **Which list is used (`isDeclaratorComma`).** It decides only when the tied trivia is the first in the text. In that case the flag's comma gap starts at the tied trivia or after it:
  - **At the tied trivia.** That is a FMT-UNSUP-009 tie at that comma. A declarator comma uses list D, and any other comma uses list N.
  - **After the tied trivia.** The flag comes from a later comma: in the same field, in another member of the whole-document input, or in a local declaration inside a value.
  - **No such gap.** List N is used.
- **What stands in the flag's gap**: a directive or a comment. Both set the flag.
- **The node passed in**: the whole document, or the field declaration alone, as a node-level entry point passes it. The flag is computed over the node, so the same tie can go through list D for the document and through list N for the field.
- **Line ending**: `AssertUnsupportedFor` runs LF and CRLF, and the `FindUnsupportedSyntax` test loops both.

## Members and the tests that cover them

All tests are in `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedFieldDeclarationDirectiveTests.cs`. New in this round: test (5), test (6), test (7), and the last row of test (4).

| # | Member | List | Expected (spec) | Test |
|---|---|---|---|---|
| D1 | 010 ∧ 009 at the field's own declarator comma: pragma; conditional group; group only in one build | D | 009 reason and location (FMT-UNSUP-010 tie paragraph; FMT-UNSUP-009 first bullet) | (1) `DirectiveInFrontOfADeclaratorCommaOnALaterLineKeepsTheReasonOfAListSeparator`, (2) `DeclaratorInDisabledTextDoesNotCountAndAtTheCommaTheReasonOfAListSeparatorWins`, (3) `ThirdDeclaratorInAConditionalGroupIsUnsupportedInEveryBuild` (existing) |
| D2 | 010 ∧ 009 at a declarator comma of a local declaration inside a value of the field: pragma; conditional group | D | 009 reason and location: the directive stands "between a declarator and a comma on a later line" (FMT-UNSUP-010), and the comma is between two declarators of a declaration in a field with several declarators (FMT-UNSUP-009 first bullet) | (5) `DirectiveInFrontOfADeclaratorCommaOfALocalInAValueKeepsTheReasonOfAListSeparator` (new, 2 rows) |
| D3 | 010 ∧ 008, flag from a later directive in front of the same field's declarator comma: behind `?`; behind `:`; conditional group behind `?` | D | 010 reason and location ("every other directive") | (6) `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirementBeforeALaterDeclaratorComma` (new), rows 1–3. Row 1 is the auditor's input |
| D4 | 010 ∧ 008, flag from a later comment in front of the same field's declarator comma | D | 010 | (6), row 4 |
| D5 | 010 ∧ 008, flag from a later field's declarator comma (whole document) | D | 010 | (6), row 5; (7) `FindUnsupportedSyntaxGivesThisRequirementsReasonBehindAConditionalOperatorForTheDocumentAndTheField` (new), on the root |
| D6 | 010 ∧ 008 inside a lambda value, flag from a later local's declarator comma in the same lambda | D | 010 | (6), row 6 |
| N1 | 010 ∧ 009 at a comma that separates no declarators: attribute argument list, tuple type, call in a value, conditional group in a value | N | 010 (FMT-UNSUP-009 second bullet; FMT-UNSUP-010 "every other directive") | (8) `DirectiveInFrontOfAnotherCommaOnALaterLineGivesTheReasonOfThisRequirement` (existing, 4 rows) |
| N2 | 010 ∧ 008 with no qualifying comma gap: behind `?`; behind `:`; conditional group behind `?` | N | 010 | (4) `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirement` (existing rows 1–3) |
| N3 | 010 ∧ 008, flag false from a later comment in front of a comma of a call | N | 010 | (4), new row "Behind the question mark, with a comment in front of a later comma of a call" |
| N4 | The same input as D5 with the field declaration alone as the node | N | 010 | (7), on the field |
| – | Counter-side: one declarator, so 010 does not apply, and 009 or 008 gives the reason | – | 009 / 008 | `DirectiveAtTheSamePlaceOfAFieldWithOneDeclaratorGivesTheReasonOfTheOtherShape` (existing) |

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirement` (new row: later comma of a call) | FMT-UNSUP-010, FMT-UNSUP-008 | `AssertUnsupportedFor` (LF and CRLF, unchanged text, reason, span) | pass |
| `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirementBeforeALaterDeclaratorComma` (6 rows) | FMT-UNSUP-010, FMT-UNSUP-008 | `AssertUnsupportedFor` | pass |
| `DirectiveInFrontOfADeclaratorCommaOfALocalInAValueKeepsTheReasonOfAListSeparator` (2 rows) | FMT-UNSUP-010, FMT-UNSUP-009 | `AssertUnsupportedFor` | pass |
| `FindUnsupportedSyntaxGivesThisRequirementsReasonBehindAConditionalOperatorForTheDocumentAndTheField` | FMT-UNSUP-010, FMT-UNSUP-008 | `ReihitsuFormatter.FindUnsupportedSyntax` on the root and the field, LF and CRLF; `AssertReportsShape` | pass |

Precheck suite: 338 passed (328 before this round, plus 10).

## Fail checks (local mutations of `PrecheckPhase.cs`, each reverted with `git checkout`, none committed)

| Mutation | Tests that fail | Members caught |
|---|---|---|
| List D: 008 ahead of 010 (the auditor's surviving mutation) | 7: all 6 rows of (6), and (7) | D3–D6 |
| List N: 008 ahead of 010, 009 left behind 010 | 5: rows 1–3 and the new row of (4), and (7) on the field | N2, N3, N4 |
| List D: 010 ahead of 009 | 5: (1), (2), (3), both rows of (5) | D1, D2 |
| List N: 009 ahead of 010 | 4: the 4 rows of (8) | N1 |
| `isDeclaratorComma` forced to true | 6: the 4 rows of (8), plus 2 from the FMT-UNSUP-004/009 tests (tuple, function pointer) | flag selection, list N side |
| `isDeclaratorComma` forced to false | 6: (1), (2), (3), both rows of (5), plus `DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment` | flag selection, list D side |

The two forced-flag mutations were written as `isDeclaratorComma || true` and `isDeclaratorComma && false`, because a plain constant leaves the local unused, and S1481 then fails the build. After every revert, `git diff --quiet src/Reihitsu.Formatter` held and the suite passed: 338 tests.

## Scenarios (bug fix)
Not a bug fix.

## Not covered, and why
- **FMT-UNSUP-010 ∧ FMT-UNSUP-004.** No such tie exists. FMT-UNSUP-004 finds comments only, and FMT-UNSUP-010 finds directives and disabled text only (`FindFirstComment` versus `FindFirstDirectiveOrDisabledText`). Their order by position is already covered by `FirstOfACommentAndADirectiveInTheDeclarationIsReported`.
- **FMT-UNSUP-008 ∧ FMT-UNSUP-009 at the same trivia, with or without 010.** Not possible. The FMT-UNSUP-008 places are the trailing trivia of `?` and `:` and the leading trivia of a branch's first token. A comma gap starts behind the last token with text in front of the comma, so it holds one of these places only when the comma directly follows `?` or `:`. That is a syntax error, and the formatter returns such input before the precheck.
- **Disabled text as the tied trivia.** Disabled text always follows the `#if`, `#elif`, or `#else` that opens it, in the same trivia list. That directive is therefore the first trivia of every candidate that holds the disabled text, so a tie always starts at a directive. The conditional-group rows cover this.
- **Other entry points** (`FormatDocumentAsync`, `FormatNode`, the `FormatNodeInDocument*` methods, the CLI). Each passes its node to the one `FindUnsupportedSyntax`. `rg -n "FindUnsupportedSyntax\(" src/Reihitsu.Formatter` shows `PrecheckPhase.Execute` and `ReihitsuFormatter.FindUnsupportedSyntax` as the only callers. The only input that changes the order is the node passed in, and (7) varies it: document and field.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck.UnsupportedFieldDeclarationDirectiveTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"
```

## Hints
- **Member D2 (a directive in front of a local declarator comma inside a field value) rests on a literal reading.** Two sentences agree on FMT-UNSUP-009's reason, and the code gives it:
  - FMT-UNSUP-010's "between a declarator and a comma on a later line" names any declarator, not only one of the field.
  - FMT-UNSUP-009's first bullet covers a declarator comma "in a field declaration with several declarators".

  The CLI check on the scratchpad fixture reported `(8,1): unsupported syntax: a comment or directive between a list element and the comma behind it`. The spec author may want to state this case in one sentence in FMT-UNSUP-010's tie paragraph, so that "every other directive" cannot be read against it. Nothing visible is in question, because the code already does this.

## Follow-up candidates
_None._
