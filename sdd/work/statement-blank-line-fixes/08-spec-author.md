Role: spec author
Commit: e9c0361 (code read at head, with the parallel implementer's uncommitted region-check and `ContainsBlankLine` edits in the worktree)
Verdict: #946: nothing to write; #972: nothing to write; #1096: spec written; #1076: nothing to write; #1626: spec written; #1625: spec written

Repair round after the implementer (00-lead.md worklist, decisions #14, #15, #18–#20). My prompt carried no intended fix, suspected cause, or hoped-for verdict. No change doc, no rule page: nothing a user sees changes.

## thoenissen/Reihitsu#1626

### Changed requirements

- Code owners of `shared/statement-blank-lines.md` (SHARED-STMT) — `BlankLineCodeFixUtilities` added. Its `InsertBlankLineBefore` is now the one owner of the blank line in front of a statement (05-implementer), and the type also holds the indentation and gap rebuild of SHARED-STMT-004. The two bases stay: the analyzer base reports, the code-fix base picks the statement's first token.
- Code owners of `shared/blank-line-after-brace-fixes.md` (SHARED-AFTERBRACE) — `BlankLineCodeFixUtilities` removed, now `—`. `SpecStructureTests.EveryTypeIsCodeOwnerOfAtMostOneSpec` (and `sdd/README.md`) allow a type to own one spec only. The after-brace behavior is the shared owner's mechanism with the placement the caller picks (`behindLeadingDirectives: true` from the RH3208 and RH5030 providers, owned by their rule specs), so no single type implements it. SHARED-AFTERBRACE already lists SHARED-STMT-004 under *Uses*.
- `sdd/specs/README.md` index rows for SHARED-STMT and SHARED-AFTERBRACE are synced.

### Internal choices

- The owner type goes to SHARED-STMT, not SHARED-AFTERBRACE. Most of the type's code (about 150 lines moved from the code-fix base: gap rebuild, column, initializer case) implements SHARED-STMT-004, and #1626 is about the blank line in front of a statement. SHARED-AFTERBRACE-002 is defined as giving SHARED-STMT-004's output. Alternative: leave `BlankLineCodeFixUtilities` on SHARED-AFTERBRACE and SHARED-STMT without it. That would hide where SHARED-STMT-004's insertion now happens. Another alternative is merging the two specs, which is out of scope.
- RH5024–RH5027 Code owners rows are unchanged. Their region exception (`RegionDirectiveBlankLineUtilities`, decision #20) implements SHARED-BLANK-006, whose spec (`shared/blank-line-spacing.md`) already names that type. RH5032's row does not name it either. Alternative: add it to the four rule rows. Rejected, because the test allows one spec per type.
- `SyntaxTriviaUtilities.ContainsBlankLine` (decision #19): no spec names it (`rg -n "ContainsBlankLine|SyntaxTriviaUtilities" sdd/specs documentation` finds nothing), so nothing changes.

## thoenissen/Reihitsu#1096

### Changed requirements

- SHARED-BLANK-001 — clarified (decision #14 follow-through): the Why no longer claims that the code fixes of RH5001–RH5010, RH5012–RH5019, RH5021, and RH5029 use `TokenGapAnalysis` to decide how many line breaks to insert. At head, `BlankLineCodeFixUtilities.InsertBlankLineBefore` decides with `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`, and `rg -n TokenGapAnalysis src/Reihitsu.Analyzer.CodeFixes` finds nothing. The sentence now names the formatter's blank-line phase and the analyzers of RH5001–RH5019, RH5021, and RH5029 (both bases call `TokenGapAnalysis.Between`), so decision #8 is done and no revert is needed.

## thoenissen/Reihitsu#1625

### Changed requirements

- SHARED-AFTERBRACE-002 — clarified (decision #15). The condition now reads "When at least one line break stands between the closing brace and the next statement and every such line break lies inside a comment, …". A counter-example is added: `int L() => 1; /* a */ L();` keeps `L();` behind the comment on the brace's line. That output comes from `RH3208…AnalyzerTests.VerifyStatementBehindSingleLineCommentStaysOnClosingBraceLine` and is unchanged from `main`.
- RH3208-002 — clarified the same way: its pointer to SHARED-AFTERBRACE-002 carries the same "at least one line break" condition.
- SHARED-STMT-004's third comment bullet is unchanged. As 04-test-implementer notes, its same-line sentence already decides the case with no line break.

### Examples for the tests

SHARED-AFTERBRACE-002's new counter-example: the existing characterization test `VerifyStatementBehindSingleLineCommentStaysOnClosingBraceLine` can now be tagged SHARED-AFTERBRACE-002 (test implementer's call). No new test is needed.

## Output questions

_None._

## Follow-up candidates

_None_ new. The placement around a directive (decision #18) is already in `00-lead.md`.

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.
- Files changed: `sdd/specs/README.md`, `sdd/specs/shared/statement-blank-lines.md`, `sdd/specs/shared/blank-line-after-brace-fixes.md`, `sdd/specs/shared/blank-line-spacing.md`, `sdd/specs/analyzer/RH3208.md`.
