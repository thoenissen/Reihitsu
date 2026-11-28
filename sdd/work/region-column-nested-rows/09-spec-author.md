Role: spec author
Commit: b970d14
Verdict: spec written

I worked from `00-lead.md` (decisions 3–7), `08-implementer.md` (*Disputed tests or scope*, *Follow-up candidates*), FMT-BREAK-046, FMT-UNSUP-023, FMT-INDENT-023, and the code at b970d14 (`PrecheckPhase.StandsBetweenMembers`, `ArrayInitializerBraceLineBreakRewriter`, `ObjectInitializerContributor`). The prompt named decision 7 as the reading to state; that reading is a recorded lead decision covered by the maintainer's answer (batch 1b, 3A: element line breaks not decided), and I checked it against the implementer's statement of what the rewriter does, not taken it as given. The cleaner ran in the same tree; I did not build or run any test suite and staged only my own files.

## Changed requirements
- FMT-BREAK-046 — A multi-line array initializer puts its braces on lines of their own — clarified: the element behind the comma that follows a multi-line initializer starts the next line only when the initializer is a row of a multi-dimensional array; in a jagged array the line break in front of the element behind a multi-line array creation stays as written (`{ new[] {⏎1⏎}, new[] { 2 } }` keeps `}, new[] { 2 }` on one line; the row `{ {⏎1⏎}, { 2 } }` puts `{ 2 }` below `},`).
- Change doc `sdd/changes/2026-11/region-column-nested-rows.md`: the *What* sentence on element line breaks names the row exception; new decision *Only a row moves the element behind it*.
- `AGENTS.md`, phase table:
  - `LineBreakPhase` row: names the array-initializer brace decision (FMT-BREAK-046) among the decisions it owns, and `ArrayInitializerBraceLineBreakRewriter` among its guards.
  - `PrecheckPhase` row, sixteenth shape: "in the header of a type, an extension block, or a namespace" (was "a type or an extension block"). FMT-UNSUP-023 names the namespace header, and `StandsBetweenMembers` does not except a namespace's `{`, so a region in front of it is refused. The exception list otherwise matches the code: end of file; `}` of a `BaseTypeDeclarationSyntax` (type, extension block, enum) or `NamespaceDeclarationSyntax`; first token of a `MemberDeclarationSyntax` that is not inside a `GlobalStatementSyntax`, of a using, an extern alias, a file attribute list; the input's first token; every region in a whole file with a top-level statement; inactive directives ignored ("active").
  - `IndentationPhase` row: unchanged. It owns "alignment" generically and names no initializer contributor today; the row anchor of FMT-INDENT-023 (`ObjectInitializerContributor`, rows anchored to their own `{`) is a case of that alignment, so the row matches the code without a change.

## Examples for the tests
- FMT-BREAK-046, the jagged-array sentence: no test covers it (`ArrayInitializerBraceLineTests` has none). A characterization test of `{ new[] {⏎1⏎}, new[] { 2 } }` would lock the reading in. The inline example was not run through `scripts/format.sh` (no builds in this round); it states what 08 reports the rewriter does.

## Output questions
_None._ The jagged reading is decision 7, covered by the maintainer's answer.

## Internal choices
- Stated the jagged case inline in FMT-BREAK-046 rather than as a new example block, since it could not be run through the formatter in this round. Alternative: a full before/after block, once verified.
- Named the namespace header in the AGENTS precheck row to match the spec and code; the change doc's *What* still names only types and extension blocks, left as is (a namespace header region is rare and falls under "every other position").

## Follow-up candidates
_None._ (08's comment-led `}` case is decision 6, left as written.)

## Checks owed
- `PhaseOwnershipDocumentationTests` (architecture project) must resolve `ArrayInitializerBraceLineBreakRewriter` in the `LineBreakPhase` guard column; not run here (no builds). No requirement-ID reference, `Uses` row, or code owner was added or removed in the specs.
