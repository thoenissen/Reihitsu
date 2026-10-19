Role: spec author
Commit: b09b6f0
Verdict: spec written

Repair round on `07-auditor.md`, findings 1–6 (spec and rule-page side). I re-ran every finding against the code at b09b6f0. Production code is unchanged since 4738ef2.

- Analyzer spans: my scratch analyzer runner (documentation mode `Diagnose`, compiler CS15xx warnings printed).
- Fix results: `scripts/apply-fix.sh` under LF and CRLF.

All six findings reproduce exactly as the auditor describes them. Every example in the changed specs still matches its markup, except RH8202-004's unmarked block, as before. `scripts/test.sh --no-install --project architecture` passes, 72 of 72.

No requirement was added, retired, or moved, and the ledger and index are unchanged. No rule page changed: none of the pages states the boundaries narrowed here.

## Changed requirements

- **RH8204-002 — The diagnostic is on the element — clarified (finding 1).**
  - New coverage sentence: the requirement covers a `<placeholder>` that is empty or closed by its own `</placeholder>`.
  - An unclosed element spans through the line break after it, and one closed by another element's end tag spans through that end tag. Both are now outside coverage.
- **RH8204-003 — The fix replaces the element with its content — clarified (findings 1 and 2).** The coverage now requires a `<placeholder>` that:
  - has no attributes;
  - is empty or closed by its own `</placeholder>`;
  - has content that is empty or text with at least one character that is not white space, and no `>`, character reference, entity reference, or XML element. The `>` exclusion was there before.
- **RH8202-001 — A `<value>` element on a property or indexer is reported — clarified (finding 3).**
  - The coverage's "well-formed XML apart from a missing end tag of the reported element" became "apart from a reported element that has no end tag at all — neither its own `</value>` nor the end tag of an element that contains it".
  - RH8202-002 to RH8202-004 build on RH8202-001's coverage, so their text is unchanged.
- **RH8201-005 — The fix replaces the comment with `<inheritdoc/>` — clarified (findings 4 and 5).**
  - The normative sentence now keeps "the ordinary comments and preprocessor directives before or after it in the member's leading trivia, and the attribute lists that follow it". An attribute list is not trivia.
  - The coverage now requires a member indented with four spaces for each enclosing type and block-bodied namespace. Its reported comment, ordinary comments, and attribute lists must start their own lines at that indentation, and directives must be at column 0.
  - The `Why:` ends with "so they stay where they are".

## Examples for the tests

Changed example blocks, which the test implementer copies verbatim (finding 6):

- **RH8103-001:** the empty line before the class's closing `}` is removed. The block now ends with `    {` / `    }` / `}`.
- **RH8202-001:** the empty line after `public int Weight { get; }` and before the class's closing `}` is removed.

No other example changed. No test may cover the shapes excluded above:

- an unclosed `<placeholder>`, or one closed by another element's end tag;
- a white-space-only placeholder, or a placeholder with attributes;
- a `<value>` closed by `</remarks>`;
- a member indented other than four spaces per level for RH8201-005.

## Open questions to file — additions

All drafts are in /tmp/claude-0/-home-user-Reihitsu/a83d1851-0465-53dd-8fc0-41a018f3f1a0/scratchpad/oq/. Every new code block was extracted from its draft and run: spans with the analyzer runner, fix results with `scripts/apply-fix.sh` under LF and CRLF. Each draft states the output observed.

### `rh8204.md` (`SDD open questions: sdd/specs/analyzer/RH8204.md`)

Q1 is still a new question. It now covers the content the fix does not keep, and it touches RH8204-002 as well. One runnable block was added with three shapes (finding 2):

- `Saves the<placeholder> </placeholder>customer` becomes `Saves thecustomer`.
- `Saves <placeholder>current customer` followed by `</summary>` on the next line: the parser closes the placeholder with `</summary>` (CS1570). The fix leaves `/// Saves current customer` and `/// `, so the summary's end tag is gone.
- `<placeholder hint="1>2">a & b</placeholder>` becomes `Handles 2">a & b now`.

New Q3 is decided by principle P1 (#912), that is SHARED-FIX-003 (finding 1):

- Input: `/// <remarks>Fast</remarks>` / `/// <placeholder>Later text` / `public void Run()` in a class.
- Output: CS1570. RH8204 reports `<placeholder>Later text` plus its line break. The fix leaves `    ///     public void Run()`, and `apply-fix` reports `invalid-result` under LF and CRLF.
- Touches: RH8204-002, RH8204-003, SHARED-FIX-003.
- Q3 is numbered after Q2 because the earlier Q2 (Fix All with nested placeholders) went to #1143.

### `rh8202.md` (`SDD open questions: sdd/specs/analyzer/RH8202.md`)

New Q4 is a new question (finding 3):

- Input: `/// <remarks><value>The size</remarks>` on a property.
- Output: CS1570. The diagnostic spans `<value>The size</remarks>`. The fix is offered and leaves `/// <remarks>` without its end tag, under LF and CRLF.
- Touches: RH8202-001 to RH8202-004.

### `rh8107.md` (new draft, `SDD open questions: sdd/specs/analyzer/RH8107.md`)

Q1 is a new question. It is the same shape for RH8107, the spec already on `main`:

- Input: `/// <remarks><returns>Nothing</remarks>` on a `void` method.
- Output: CS1570. RH8107 reports `<returns>Nothing</remarks>`. RH8107-007 does not withhold the fix, and the fix leaves `/// <remarks>`, under LF and CRLF.
- Touches: RH8107-005, RH8107-006, RH8107-007.

### `rh8201.md` (`SDD open questions: sdd/specs/analyzer/RH8201.md`)

Q1 is still decided by principle P9 (#912). A runnable LF block for finding 4 was added: `Run` indented at column 6 and `Stop` at column 2.

- `Run`: the ordinary comment, `/// <inheritdoc/>`, and the member's first line move to column 4. The braces stay at column 6.
- `Stop`: `/// <inheritdoc/>` and the attribute list move to column 4. The member's first line stays at column 2.

## Follow-up candidates (not filed, for the run report)

- The RH8204 provider has no end-tag guard. `DocumentationCommentCodeFixUtilities.CanRemoveElement`, which RH8107 and RH8202 use, would cover RH8204 Q3. A guard for an element closed by another element's end tag would also cover RH8202 Q4 and RH8107 Q1. This is production code, outside this lane.
