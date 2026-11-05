Role: spec author
Commit: d2f81058
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The only edit: `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` header `Status` set from `Draft` to `Accepted` (`sdd/README.md`, *Change docs*: the lead sets it once every visible output change has the maintainer's answer). No other line of the change doc, and no spec, changed.

## Check of the change doc against the implemented behavior

Compared the *What* summary and *Decisions* with `12-spec-checker.md`, `08-implementer.md`, `10-implementer.md`, decisions 18–20 of `00-lead.md`, and the code at d2f81058. Every statement still holds:

- RH7101 fix calls `FormatNodeInDocumentWithContextAsync(document, field, field.Parent)` and asks `ReihitsuFormatter.SplitsFieldDeclaration` (FMT-PIPE-031), which delegates to the split's own `Splits`; no copy of the guard in the fix.
- Entry point writes back only the split fields; first keeps the target's leading trivia, last its trailing trivia; a shared-line gap gets trimmed whitespace, one line break, the phases' member column (FMT-PIPE-029, case 7).
- Field split writes no indentation; keeps one space before a comment that trailed a comma and the line break behind a comment it moves in front of a later field (`FieldDeclarationSplitTransform.BuildTrailingTrivia`/`BuildLeadingTrivia`), as *The field split keeps the layout FMT-STRUCT-022 decides* states.
- Analyzer: no report with a syntax error (`SyntaxTreeErrorVerdictCache`) or in an extension block.
- Generated-code header: checked only by `FormatDocumentAsync` and `FormatSyntaxTree` (and the CLI); `FormatNode`, both node-in-document entry points, and `OrganizeUsingDirectivesInDocumentAsync` do not check it.
- Using fixes call `OrganizeUsingDirectivesInDocumentAsync`, which checks no syntax error and no header, as *The using fixes get an entry point of their own* says.
- Fix All (decision 19): the doc says only "Fix All still splits every reported declaration" (case 9). The sequential provider (`FixAllProvider.Create`, one fix after the other on tracked nodes) gives that result; the doc names no Fix All mechanism, so nothing contradicts it. The provider choice is internal and stays out of the doc.
- Decision 20 (shared `FormatContext` helper) changes no output and is not a change-doc matter.

## Changed requirements
_None._

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Left the change doc's text unchanged, including no mention of the sequential Fix All provider; alternative: add a sentence under *Decisions*. Rejected because the doc's Fix All statement already matches the result and the provider is an implementation detail.

## Follow-up candidates
_None._
