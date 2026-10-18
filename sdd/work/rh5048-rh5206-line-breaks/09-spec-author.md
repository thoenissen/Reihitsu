Role: spec author
Commit: f63b726 (on top of the spec checker's 4ae032e; analyzer and code-fix sources unchanged since 72b08b0)
Verdict: text written

Repair round for the three findings of `08-spec-checker.md`. My prompt carried no intended fix or verdict. Each finding was re-derived against the code and the specs.

## Changed requirements

No normative sentence (MUST, MUST NOT, SHOULD) changed. Existing tags stay valid and no test needs re-checking.

- **SHARED-CHAIN-008 — Why changed** (finding 2).
  - Old: "Taking the formatted layout keeps fixed code identical to formatted code".
  - New: "Taking the layout the formatter gives the whole chain, instead of a layout of its own, clears every diagnostic the chain had in one application".
  - Reason: the old wording is the claim this run removed from the other formatter-delegating fixes. Node-level formatting can differ from formatting the whole document; that is open question #1112.
  - The normative sentence and the example are unchanged.
- **`documentation/rules/RH5102.md` — corrected** (finding 1). "All three of the following are accepted" became "Both of the following are accepted". The block holds two examples, and they are exactly the two shapes RH5102-001 accepts (one line, and one argument per line with a multi-line argument).
- **`documentation/rules/RH5103.md` — corrected** (finding 3). The clause "or when the reported statement is the first one on the line" is removed. The sentence now reads: "No code fix is offered when a stray semicolon separates the statements. Split the line by hand there."
  - The analyzer never reports the first statement of a line: RH5103-001 reports only a statement that has a preceding statement starting on the same line.
  - The provider guard for that case (`GetPreviousStatement` returns `null`) is reachable only through a diagnostic the analyzer did not produce. That is what `FixIsNotOfferedWhenDiagnosticTargetsFirstStatement` constructs.
  - So the page described no case a user can hit. This is no open question, because there is no observable behavior that looks wrong.
  - The remaining no-fix cases on the page (stray semicolon) match RH5103-004. A comment in the gap is the general limitation in `general-notes.md`.

## Checks

- `scripts/test.sh --project architecture`: 72 passed, 0 failed. This includes `SpecConsistencyTests`, `SpecStructureTests`, `RequirementCoverageTests`, and `RuleDocumentationConventionTests`.
- No C# example changed, so the harness was not run again.

## Examples for the tests

_None._ No requirement's normative text changed.

## Open questions to file

_None new._ The list in `03-spec-author.md`, extended in `05-spec-author.md`, stands. All of it is filed as #1111–#1125, plus the comment on #1113.
