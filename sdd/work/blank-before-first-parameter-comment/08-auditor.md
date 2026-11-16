Role: auditor
Commit: 6b45f17298311250590d8165f8635f48ba964485
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of head 6b45f17 against base `origin/main` eb89003591b8371d91e4a14b8590d764bdf6d605 (`git ls-remote` confirms; `git merge-base --is-ancestor` confirms it is merged). The diff matches the handovers: the Core owner `SyntaxTriviaUtilities.IsContentOpeningToken`, the `PrecedingTokenFacts.OpensContent` fact, `BlockStartUtilities.IsFirstBehindOpeningToken`, the boundary rewriter, the cleanup-rewriter guard, the doc-phase summary, RH5020 and RH8303, the specs FMT-BLANK-011, RH5020-002 and RH8303-002, the change doc, the two rule pages, and the tests from `04-test-implementer.md`. `scripts/lane-check.sh behavior-change`: confirmed.

Issue thoenissen/Reihitsu#1781 (`agent-filed`): the maintainer answered 1C. Under 1C, FMT-BLANK-011 names every opening token. The record and union cases are pinned at their real columns. Every requirement of the issue is addressed.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/formatter/blank-lines.md` (FMT-BLANK-011 example, `void O<`); `src/Reihitsu.Formatter.Test/Regression/BlankLines/FirstCommentBehindOpeningTokenTests.cs` (`SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanGetsNoBlankLine`, `FirstCommentBehindOtherOpeningTokensGetsNoBlankLine`, `AuthorsBlankLineBehindOpeningParenthesisBracketOrLessThanIsKept`); `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5020CommentsShouldBePrecededByABlankLineFormatterTests.cs` (`VerifyFormatterLeavesFirstCommentBehindOpeningTokenWithoutBlankLine`) | FC-017 (the assertion is wider than the guarantee) | – | same requirement | The run lists one follow-up candidate as an open output question (`00-lead.md`, `01-spec-author.md`). It says that the first element behind the `<` of a type parameter list or type argument list, and behind the `[` of an array size, lands in the column of the enclosing line instead of aligned. FMT-INDENT-015 aligns only the *later* elements, so no requirement decides this column. Even so, the FMT-BLANK-011 spec example writes it as normative output: `void O<⏎    // first⏎    T>()`. Four exact-output tests pin it as well: `O<` in three tests, and `M<` and `new int[⏎        // first⏎        3];` in `FirstCommentBehindOtherOpeningTokensGetsNoBlankLine`. `sdd/README.md`, *Open questions*: "While a question is undecided, the case never goes into a spec — no requirement, no non-goal, no note." The test implementer's own rule (columns no requirement states are asserted with `AssertBlankLinesAbove`) is applied to `if (`, the cast, and the patterns, but not to these cells. Members that hold: every cell aligned behind `(` or `[` by FMT-INDENT-012 or FMT-INDENT-025 (record, union, method, argument list, collection, list pattern, attribute argument list, indexer, element access). Members that fail: the type parameter `<` (spec example and three tests), the return-type and type-argument `<` `M<`, and the array size `new int[`. Formatter output is unchanged from `main` apart from the decided blank line, so nothing visible changes. The defect is that the spec and the exact-output tests now record a case that is still undecided. | Remove the `void O<` member from FMT-BLANK-011's formatter example; RH5020-002's analyzer example states no column and may keep it. In the three formatter tests and the RH5020 parity test, move the `<` and `new int[` cells out of `AssertRuleResult`/`VerifyFormatter(source, fixedSource)` into blank-line-only assertions (`AssertBlankLinesAbove`), as `FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine` already does. |
| 2 | medium | `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs:611-618` (`isAuthorsBlankLineBehindOpeningToken`) | FC-003, FC-017 | – | PR-introduced | The new guard decides a FMT-BLANK-011/FMT-BLANK-019 outcome: the author's blank line between an opener and a comment is kept in front of a closing `]`, `)`, or `}` that `RemovesBlankLinesDirectlyBefore` covers. Only one test exercises it: `CollectionExpressionClosingBracketCommentTests.CommentBeforeClosingBracketOfEmptyCollectionExpressionIsIndented`, an indentation test tagged FMT-INDENT-006 that happens to carry an author's blank line. No FMT-BLANK-tagged test pins either side of the boundary. Untested sides: (a) the empty list pattern `x is [⏎⏎// c⏎]`, where the blank line is kept; (b) the comment glued to `]` (`[⏎⏎/* c */ ]`), where collapsing is now skipped and the blank line goes only because `RemoveBlankLinesAboveGluedComment` (FMT-BLANK-018) removes it; (c) a property pattern behind a positional `(` (`t is (⏎⏎// c⏎{ Length: 0 }, 1)`), whose `{` falls under the same branch. All three hold today. I checked each with `scripts/format.sh` (two passes, idempotent; only the intended difference from a `main` build). A later change to either predicate would go unnoticed. | Add FMT-BLANK-011 tests (LF/CRLF, second pass) to `FirstCommentBehindOpeningTokenTests`: the author's blank line is kept behind `[` of an empty collection expression and of an empty list pattern, and the blank line behind `[` is removed above a comment glued to `]`. Optionally add the property-pattern cell. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| No blank line above the first comment behind an opener (formatter) | Kind of the preceding token, plus its parent kind for `<` (`PrecedingTokenFacts.OpensContent` from `IsContentOpeningToken`); a missing token; a switch-label colon | RH5020 `IsFirstCommentBehindOpeningToken` (RawKind 0, `IsContentOpeningToken`, colon of Case/CasePattern/Default label); RH8303 `IsFirstBehindOpeningToken` (`IsContentOpeningToken`; the file start is handled by its line checks; no colon, per RH8303-002) | Same owner for the opener class. The missing token and the switch colon agree between the formatter and RH5020: `SwitchLabelSyntax` has exactly those three subclasses | ok |
| Keep the author's blank line behind an opener in front of a closing token (`BlankLineTokenCleanupRewriter`) | `previousToken.OpensContent` plus any comment (`IsCommentTrivia`, documentation included) anywhere in the token's leading trivia | `BlankLineTriviaBoundaryRewriter`: `HasCommentInLeadingTrivia` plus `IsFirstBehindOpeningToken`; `RemoveBlankLinesAboveGluedComment` for a glued comment | Agrees. Behind `{`, the earlier branch's `RemoveLeadingBlankLines` already empties the run, so skipping the collapse is a no-op, and with `EndsLine == false` both calls are identical. A glued comment still loses the blank line through FMT-BLANK-018. Blank lines between the comment and `]` are still removed by FMT-BLANK-020 (fixture) | ok (test gap: #2) |
| `/** */` move (doc phase) | `IsFirstInBlock`: `{`, switch colon, missing token | — (a different decision, deliberately kept narrow; change-doc entry 4) | Summary updated to match | ok |
| Generic vs operator `<` | `token.Parent` kind in {TypeParameterList, TypeArgumentList, FunctionPointerParameterList} | — | `operator <(` (parent OperatorDeclaration), binary `<`, and relational `<` all open nothing; tested for binary and relational | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which token opens content | `SyntaxTriviaUtilities.IsContentOpeningToken` (Core), read by `PrecedingTokenFacts.From`, RH5020, RH8303 (`rg -n "IsContentOpeningToken\|OpensContent\|IsFirstInBlock\|IsFirstCommentInBlock" src`) | One owner; the two private analyzer copies are gone |
| First content of a switch section | `PrecedingTokenFacts.From` (`Parent is SwitchLabelSyntax`) and RH5020's private check (three label kinds) | Two owners, pre-existing, equivalent; not changed by the run (hint) |
| The FMT-BLANK-011 exemption as spec text | FMT-BLANK-011; FMT-BLANK-012 restates it as "none when it is the first content of a block, a type body, or a switch section" | Equivalent within FMT-BLANK-012's domain (FMT-DOC-001 moves only before type or member declarations, which never follow `(`, `[`, or `<`); wording drift only (hint) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FirstCommentBehindOpeningTokenTests` exact-output tests | Exact output, LF/CRLF, second pass, per-phase stability | Any blank line or column difference | `AssertRuleResult` | Strong, but pins undecided columns (#1) |
| `FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine`, `CommentBehindLessThanOperatorGetsBlankLine`, `CommentInEmptyListBehindOpeningTokenGetsNoBlankLine`, `FirstCommentBelowTrailingCommentOfOpeningParenthesisGetsNoBlankLine`, `CommentBelowDirectiveBehindOpeningBracketGetsNoBlankLine` | Blank-line state above named lines, LF/CRLF, second pass stable | A blank line where none is expected, or none where one is | `AssertBlankLinesAbove` (first line starting with the label; labels unique) | Adequate for a blank-line requirement |
| `TripleSlashCommentWithoutDocumentationParsingFollowsTheOpeningTokenRule` (formatter and RH5020) | FC-040: `///` under DocumentationMode None follows RH5020 and FMT-BLANK-011 | Blank line or report on the first `///` | `AssertRuleResult` / `Verify` | Adequate |
| RH5020 and RH8303 analyzer tests (opener matrix, comma side, operator `<`, empty list, trailing comment, union, function pointer, CRLF) | Not reported behind an opener; reported and fixed behind a comma or operator | A diagnostic on an opener-first comment; none behind a comma | `Verify` with markup / fix | Adequate; both sides of each boundary covered |
| RH5020/RH8303 formatter parity tests | FC-013: formatter output is not reported, and a second pass is stable | A diagnostic on the output | `VerifyFormatter` | Adequate; `O<` column pinned (#1) |
| RH3004 and RH5305 fix tests | Fix output without the blank line, re-analysis clean | A blank line in the fix output | `Verify` with fix | Adequate |
| Cleanup guard (`isAuthorsBlankLineBehindOpeningToken`) | Author's blank line behind an opener kept in front of a closing token | — | Only an FMT-INDENT-006 test | Weak (#2) |

## Failure classes dismissed

- FC-001, FC-002: the predicate reads exactly the preceding token, plus its parent for `<`. The qualifiers "of any construct" and "generic `<`" are explicit conditions.
- FC-004, FC-005, FC-019, FC-020, FC-021: no token is moved and no gap is joined; only the insertion of a blank line is withheld. The directive case is tested, and so is a trailing comment on the opener line. `#if`/`#else` inside an argument list was checked with `scripts/format.sh` and keeps all trivia.
- FC-006, FC-022–FC-026, FC-029, FC-036: blank-line trivia only; no rename, literal, member kind, or key/value layout reaches the change.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changed. `scripts/apply-fix.sh RH5020` and `RH8303` on a 7-file corpus give `no-diagnostic` on the formatter's output under LF and CRLF; on the raw inputs, comma and operator cases converge in one iteration per diagnostic.
- FC-010, FC-011, FC-012: two `scripts/format.sh` passes over the corpus are idempotent. It covers statement headers, `catch`/`when`, primary constructors, tuple types, implicit element access, target-typed `new`, `typeof`/`nameof`/`checked`/`default`, `stackalloc`, interpolation holes, doc comments behind `(`, `[`, and `<`, two author blank lines, a glued comment, and a property pattern behind `(`. The cleanup collapse and the boundary insert no longer meet behind `(`, `[`, and `<`. Against a `main` build, the only differences are the decided blank lines.
- FC-013, FC-014: see FC-007; RH5020 and RH8303 report nothing the formatter keeps. No other rule's page concerns blank lines behind `(` or `[` (`rg` over `documentation/rules`).
- FC-015, FC-016: resolved by lead decision 3 (one Core owner); siblings `(`, `[`, `<`, `{`, and the switch colon are all covered.
- FC-027: nested collection, nested `[`, lambdas, attribute and argument contexts are in the test matrix and the fixtures.
- FC-028: node-level roots read `Exists == false`, unchanged. The RH3004 and RH5305 fixes go through the document-scoped entry point and are tested.
- FC-031: every changed summary describes the new code (`BlockStartUtilities` class and methods, `IsMovableDelimitedDocumentation`, `PrecedingTokenFacts`, both analyzers, the Core member).
- FC-032, FC-033, FC-034, FC-035: expected values match the CLI output from a `main` build minus the decided blank line; tests run LF and CRLF; the issue's own record and union shape is tested.
- FC-037: not applicable; compiled changes, no text-only claim.
- FC-038–FC-043: no shape is declared, and the precheck is unchanged. The type-argument cells avoid FMT-UNSUP-004 (a field-type `<` is refused, as the skeptic recorded). A comment before an omitted type argument (`Dictionary<⏎// c⏎,>`) is refused as FMT-UNSUP-009, which is unchanged.
- FC-044: no computed result is modelled; the parent kind of `<` is read from the parse tree.
- Tab-width matrix: no column decision changed.

## Architecture findings

_None._ The three copies of the opener rule that the skeptic found now have one owner (lead decision 3). The remaining two-owner switch-colon check is pre-existing and equivalent (see *Hints*).

## Hints

- `src/Reihitsu.Analyzer/Rules/Layout/RH5020CommentsShouldBePrecededByABlankLineAnalyzer.cs` gained a UTF-8 BOM in a730de0. It matches `.editorconfig` (`charset = utf-8-bom`) and most source files, and is harmless. It is an unrelated byte change.
- FMT-BLANK-012 still restates FMT-BLANK-011's exemption in the old words ("first content of a block, a type body, or a switch section"). It is correct for its domain, but it is a second statement of the rule that can drift. Replacing the parenthetical with "none where FMT-BLANK-011 exempts it" would remove the restatement.
- The switch-label colon check exists in `PrecedingTokenFacts.From` and in RH5020's private predicate. It is pre-existing and equivalent.
- `src/Reihitsu.Formatter/Data/PrecedingTokenFacts.cs:15`: the rewrapped class summary has one 145-character line, which is longer than its neighbours.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`. The type-parameter, type-argument, and array-size column candidate there is the case finding #1 keeps out of the spec.
