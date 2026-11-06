Role: implementer
Commit: see the commit that adds this file (on top of 1f4dfe6e)
Verdict: tests pass

Repair round 1, for audit findings 1–4 (`08-auditor.md`), lead decisions 15–18 and maintainer answer 6 (`00-lead.md`), against the tests of `10-test-implementer.md` and the spec diff of 82129fca. My prompt named the decided rows and the condition of answer 6; it carried no intended fix, suspected cause, or hoped-for verdict beyond them, so I had nothing to set aside. No test file and no file under `sdd/specs` was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | For a kept block body, the lambda is still formatted with `FormatNodeInDocumentAsync` (parameter list, `=>`, braces at the anchor column), and then `MoveStatementsAsync` replaces the formatted block's interior with the original block's content as moved code: the text from the first to the last non-whitespace character inside the braces, its first line at the closing brace's column plus one level, every later line moved by the same column delta (`ShiftIndentation`). A comment behind the original `{` on its line stays behind the new `{` (`GetOpenBraceTail`). A line that starts inside a non-raw string literal keeps its columns; a line inside a multi-line raw string literal moves with its closing marker, unless a move to the left is wider than the marker's indentation (`CanMoveLine`). `RestoreDocumentationCommentsAsync` and `GetDocumentationComments` (Decision 12) are removed: the `///` text is now copied as written | RH3004-003, RH3004-006, SHARED-FIX-007 (moved code) |
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentOrderFixAllProvider.cs` (new) | Shared Fix All: applies the document's fixes one after another in document order; before each, the provider registers its fix again on the current document (so its guard decides on the code the earlier fixes produced); each step's text changes are applied as text (incremental parse, no re-serialization), and the later diagnostics' spans are mapped through them; a span that a change overlaps is left reported | SHARED-FIX-002, RH3005-008, RH3101-005, RH3105-005, RH3106-002 |
| `RH3005UseReadableConditionsCodeFixProvider.cs`, `RH3101…CodeFixProvider.cs`, `RH3105…CodeFixProvider.cs`, `src/Reihitsu.Analyzer.CodeFixes/Base/EmptyParenthesesCodeFixProviderBase{TNode}.cs` | `GetFixAllProvider` returns `DocumentOrderFixAllProvider.Instance` instead of `BatchFixer` | same |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider.cs` | `KeepsMeaningAsync` also runs when a `BinaryExpressionSyntax` encloses the anonymous method up to the statement, and compares the operator it binds before and after the removal (`IsOverloadResolvedCall` includes binary expressions; a binary operator without an operator symbol, such as `??` or `as`, only has to keep binding to none) | RH3106-002 |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs` | `TakesNaturalType` works on the position expression (`GetPositionExpression`: through parentheses, conditional branches, switch-expression arms) and adds: element of `new[] { … }`; expression body or `return` of a lambda that itself takes its natural type (recursive) | RH3106-003 |
| `src/Reihitsu.Core/RewriteParseUtilities.cs` | `ParsesAsWritten` no longer serializes and diffs the whole text or compares the whole tree. It descends both roots while exactly one child node differs (`IsIncrementallyIdenticalTo`), re-parses only that node's text incrementally, walks the same child path in the re-parsed tree to the innermost member declaration (not a namespace) around the change, and compares that member (full span and tokens/structure). An unchanged root returns `true` at once. Same public signature | Finding 4 (lead decision 18) |

The RH3107 fix shares `EmptyParenthesesCodeFixProviderBase`, so it gets the document-order Fix All too. Its fixes do not interact (audit), so its result is the same as with `BatchFixer`; its tests pass unchanged.

## Answer 6 (A) and the moved-code mechanism

No existing fix shifts moved lines by a column delta. RH7309 moves region blocks verbatim at the same depth, and RH7303 only removes directive lines. RH7004 and `TypeMemberOrderingCodeFixProviderBase` (RH7103 and others) run `FormatNodeInDocumentAsync` over the node that holds the moved code. That reformats it, so they are not a moved-code mechanism (see *Follow-up candidates*). The general path therefore lives in the RH3004 fix: the formatter lays out what the fix creates, and the fix moves the original text. A one-line block with several statements needs **no dedicated code path**. Its content is the text between the braces like any other block, and its first line is placed one level inside the braces. Answer 6 resolves to **A**, as `FixMovesTheStatementsOfAOneLineBlockOntoOneLine` asserts.

## Policy owners

- Fix All ordering: `rg -n "FixAllProvider.Create|: FixAllProvider|new CodeFixContext\(" src --glob '!**/bin/**' --glob '!**/obj/**' --glob '!*Test*/**'` finds `DocumentOrderFixAllProvider` (new, the four guarded providers and RH3107 through the base), RH3002's and RH7101's own batch rewrites, the casing Fix All, and `Reihitsu.Tooling/FixtureRunner.cs` (the fixture runner, not a fix). None of them re-registers a fix on an evolving document, so the new class has no copy.
- Re-parse check: `rg -n "IsIncrementallyIdenticalTo|WithChangedText" src --glob '!**/bin/**' --glob '!**/obj/**' --glob '!*Test*/**'` finds only `RewriteParseUtilities`. RH3002's private generic-name scan stays as it was (Decision 3 follow-up).
- Moving lines by a column delta: `rg -ln "ShiftIndentation|columnDelta|ColumnDelta" src …` finds only the RH3004 fix among code fixes. The formatter's `RawStringLineShifts` moves raw-string lines in the indentation phase, a different decision: it computes the final column, while the fix only keeps the lines' relative layout. See *Architecture findings*.

## Tests run

All in Debug unless noted.

| Command | Result |
|---|---|
| `scripts/test.sh --configuration Debug --project analyzer --filter "<the five classes of 10-test-implementer.md>"` on 1f4dfe6e before any change | Failed 16, Passed 163, Total 179: exactly the 16 named tests |
| same five classes plus `RH3107`, after the change | Passed 191, Failed 0 |
| `scripts/test.sh --configuration Debug --project analyzer` | Passed 6852, Failed 0 |
| `scripts/test.sh --configuration Debug --project core` | Passed 621, Failed 0 (incl. every `RewriteParseUtilitiesTests`) |
| `scripts/test.sh --configuration Debug --project architecture` | Passed 115, Failed 0 |
| `scripts/build.sh` (Release) | 0 warnings, 0 errors |

Performance (finding 4): scratch file-based app outside the repository, Debug build of the code-fix project, RH3105 on `M(this.A + j);` × 2,000 in a 3,609-line file.
- Registering all 2,000 fixes: 1.1–1.3 s, about 0.6 ms per diagnostic. The auditor measured 14.8 s.
- Fix All in the document with the new provider: 4.4 s. `BatchFixer` on the same input takes 1.9 s.
- At 4,000 diagnostics (7,209 lines): registration 3.2 s, Fix All 10.7 s.
- A first version that built each step's document from a rewritten root took 28 s, because every step serialized the whole text. The text-change steps fixed that.

The RH3004 edge cases I ran with `scripts/apply-fix.sh RH3004` (LF and CRLF) all behaved as designed:
- a comment behind `{`;
- a raw string;
- a verbatim string with a continuation line;
- `#if`/`#else` with disabled text;
- a multi-line `/* */` comment;
- an argument lambda.

## Disputed tests or scope

_None._ Scope notes for the lead:

- New shared component `DocumentOrderFixAllProvider` (internal, `Reihitsu.Analyzer.CodeFixes/Core`, five consumers through four providers and one base). This is the shared sequential Fix All path that the auditor's architecture finding named. It is one place instead of a Fix All provider per rule.
- The RH3004 fix grew by about 200 lines (moved-code text handling).
- RH3106-003 as written (followed literally): an anonymous method in a conditional branch under `var` is not reported even when the other branch is a delegate-typed expression, e.g. `var x = b ? delegate () { } : someAction;`. That code compiles without the list too. The spec puts the anonymous method in the position of the conditional expression, so the analyzer does not report it. This is a narrower report, not a broken fix. No test covers it.

## Architecture findings

_None new._ The RH3004 finding (Decision 12, audit #3) is resolved by the second structural option the auditor named: the fix formats only the code it creates and moves the original lines itself, so no formatter phase is replayed or undone, and the `///` restore is gone.

One decision is close to the formatter without being a copy: which lines of moved code keep their columns. The fix keeps them for lines inside non-raw string literals and moves them for lines inside raw literals. `RawStringLineShifts` makes the analogous call for the indentation phase. If more fixes come to move code by a delta (see the follow-up below), that rule belongs in one shared helper.

## Follow-up candidates

- `TypeMemberOrderingCodeFixProviderBase` (RH7103 and the other member-ordering rules) and the RH7004 fix run `FormatNodeInDocumentAsync` over the type or the new `using` statement. That reformats the moved members or statements, which SHARED-FIX-007's moved-code bullet forbids for code the formatter would change. Their tests use only formatter-clean code. A shared "move lines by a delta" helper, the one the RH3004 fix now has privately, would serve them too. This is a defect against an unchanged requirement, in components this run does not change.
- RH3004: the delta is taken from the block's first content line. When that line is a directive in column 0 (`#if` directly after `{`), every following statement moves by the directive's delta and ends up one level deeper than the directive. RH3004-006 says "the block's first line", so this is as specified, but the spec could name the first statement's line instead.
- RH3106-003 conditional-under-`var` reading above (spec wording).
- Hint 1 of the audit (`HaveSameTokensAndStructure` recursion depth): its depth is now bounded by the innermost member around the change rather than by the file, but it is still recursive.
