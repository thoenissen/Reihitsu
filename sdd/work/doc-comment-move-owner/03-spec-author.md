Role: spec author
Commit: 181fb96
Verdict: spec written

Amendment round. It carries out the skeptic's spec amendments 1 and 2 (decision #7) and brings the change doc in line with decisions #5 and #6. My prompt carried no intended fix or suspected cause. No output changes.

## Changed requirements

- FMT-DOC-001 — Documentation written behind code moves onto a new line — clarified (coverage paragraph only; normative sentence and examples unchanged):
  - Amendment 2: it decides comments in front of a type or member declaration, enum members included. It leaves every other position undecided for both forms: statement, using directive, `extern alias`, assembly attribute list, `else`/`catch` clause, accessor, parameter or type parameter, base-list entry, initializer element, between an attribute list and its declaration, and inside an expression.
  - Amendment 1: for a delimited comment behind an opening brace, only a comment that ends the brace's line stays (FMT-INDENT-006). One with code behind it on the brace's line (`class C { /** d */ void M() { } }`) is not moved by FMT-DOC-001, and goes onto the next line with that code when the line breaks behind the brace.
- Change doc `sdd/changes/2026-10/doc-comment-move-owner.md` (still `Status: Draft`):
  - *What*: brace placement keeps the `{ /** d */ code` case (decision #5); the blank-line phase inserts no line break in front of any documentation comment (decision #6); the coverage paragraph mirrors amendment 2.
  - *Why*: a paragraph on why brace placement keeping that case leaves no phase-order dependency.
  - *Visible output changes*: still none; it now names the brace-at-end-of-line case and shows the `{ /** d */ code` input with its unchanged output.
  - *Decisions*: the brace sentence narrowed to a comment ending the brace's line; a new section "Brace placement keeps carrying a delimited comment that has code behind it" records decision #5 and the dropped issue goal; "The blank-line phase keeps only the blank line" states decision #6.
  - *Alternatives*: the issue's literal goal (brace placement moves no documentation comment) is listed as rejected because it changes output.

No new requirement IDs. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Requirement|FullyQualifiedName~Change"`: 51/51 pass.

## Examples for the tests

No new spec example. The change doc's `class C { /** d */ void M() { } }` → `class C` / `{` / `    /** d */ void M()` / `    {` / `    }` / `}` was run through `scripts/format.sh` on 181fb96 and matches today's output. So do `class C` / `{ /** d */ void M() { } }` (same output), `if (a > 0) { /** d */ a++; }` (→ `{` / `/** d */ a++;`), and `{ /** doc */` ending the brace's line (unchanged). Decision #9's tests pin these; the spec deliberately gives the brace-with-code case no example, because no requirement decides it.

## Output questions

_None._

## Internal choices

- The brace-with-code case is stated as a fact in FMT-DOC-001's coverage paragraph, without an example and without citing FMT-BREAK, because FMT-BREAK-003 read literally says otherwise and that conflict is follow-up candidate 1. Alternative: give it an example in FMT-DOC-001, which would read as a decision.
- Amendment 2 lists the undecided positions by name instead of "every other position" alone, so a reader can tell an accessor or a base-list entry is open. Alternative: the generic sentence only.

## Follow-up candidates

_None new._ Candidates 1 and 2 in `00-lead.md` stand unchanged.
