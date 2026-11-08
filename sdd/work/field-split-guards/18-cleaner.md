Role: cleaner
Commit: see git log (subject ends in [skip ci])
Verdict: clean

Audited SHA: d45fae6.

## thoenissen/Reihitsu#963

### Formatted
_None_ (format.sh: Formatted 0 of 1 / 0 of 2 file(s) after each edit).

### Comment fixes (auditor finding #2)
- `FieldDeclarationSplitTransform.CarriesCommentInRepeatedPart`, `Splits`, `CreateSplitField` and `ReihitsuFormatter.SplitsFieldDeclaration` no longer enumerate comment kinds. They state the boundary the code uses (trivia of the tokens from the first token up to the first declarator, without the declaration's and the first declarator's leading trivia, as the parser assigns ownership) and point to FMT-STRUCT-030 as its single statement.

### Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S4144 (identical method bodies) | ReihitsuFormatterSplitsFieldDeclarationTests, the two `ReturnsTrueFor...UnlessDocumentationCommentsAreNotParsed` tests | Both bodies moved unchanged into the private helper `AssertSplitsOnlyWhenDocumentationCommentsAreParsed`, placed last in the class; assertions and inputs untouched |

### Unresolved
_None._

### Checks
- `TEXT-ONLY PROOF: PASS — base=d45fae6 head=worktree — 2 changed file(s) (0 non-compiled, 2 C# comment/layout-only)` (production files, `--strict-docs` run flagged ReihitsuFormatter.cs as public API documentation, so the run without strict was used for the pass line; the change is documentation only, tokens identical).
- scripts/build.sh: 0 Warning(s), 0 Error(s).
- Focused formatter tests (SplitsFieldDeclaration, FieldDeclarationSplit): passed (see run tail).

## thoenissen/Reihitsu#1384
No change of its own; the files above are shared with #963.

## Whole run
Release build warning-free; changed files formatted; comment-only production change proven text-only.
