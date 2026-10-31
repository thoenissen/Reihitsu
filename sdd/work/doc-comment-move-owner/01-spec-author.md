Role: spec author
Commit: 52b69fa
Verdict: change doc written

Change doc: `sdd/changes/2026-10/doc-comment-move-owner.md` (`Status: Draft`). It lists no visible output change: every input keeps today's output. The lead's prompt carried no intended fix or suspected cause.

## Changed requirements

- FMT-DOC-001 — Documentation written behind code moves onto a new line — changed: widened to delimited comments with the reach the code gives them today. For a delimited comment, "other source text" is the token in front of it or a comment that trails that token. The coverage sentence leaves comments before a statement or a using directive, or inside an expression, undecided for both forms. For delimited comments it also leaves undecided a comment behind an earlier comment of the same leading trivia, and one behind an opening brace or a switch-label colon. FMT-INDENT-006 already keeps the opening-brace case in place.
- FMT-DOC-002 — clarified: "single-line" moved into the normative sentence, because the spec now covers delimited comments too. No behavior change.
- FMT-DOC-003 — changed: widened to delimited comments (not moved), with a delimited example added (moved over from FMT-BLANK-012).
- FMT-DOC-004 — clarified: "single-line" added to the normative sentence.
- FMT-DOC-012 — changed: the list of single-line-only requirements is now FMT-DOC-002 and FMT-DOC-004 to FMT-DOC-011. FMT-DOC-001 and FMT-DOC-003 decide the position of a delimited comment. The Why no longer gives the decision to FMT-BLANK.
- FMT-BLANK-012 — Documentation moved off code is separated from it — changed (narrowed): a comment that FMT-DOC-001 moved gets FMT-BLANK-011's blank line in the same run, including the first-in-block exemption. A stranded comment gets none. The phase no longer moves anything.
- FMT-DOC non-goal on delimited comments, FMT-DOC intro, FMT-DOC `Uses` (+FMT-BLANK-012, +FMT-INDENT-006), and FMT-BLANK's non-goal on moving documentation comments — updated.
- `AGENTS.md` phase table: the documentation phase's "Decisions it owns" now names `/** */` relocation. I did not touch the guard-name columns (e.g. `IsStrandedInlineDocumentation` in the blank-line row). They name implementation, and the implementer keeps them true.

No new requirement IDs, so `requirement-ids.txt` is unchanged. `SpecConsistencyTests`/`SpecStructureTests`/`RequirementCoverageTests` pass (51/51, `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Requirement"`).

## Examples for the tests

FMT-DOC-001 (new delimited examples: enum member; `int A; /* c */ /** doc */ int B;` → `int A; /* c */` / blank / `/** doc */ int B;`), FMT-DOC-003 (`var x = 1; /** stray */` unchanged), FMT-BLANK-012 (all four examples, including `{ /// <inheritdoc/>` with no blank line). Every new example was run through `scripts/format.sh` on 52b69fa and matches today's output.

## Issue statements against the current tree

Traced on 52b69fa with `scripts/trace.sh` and `scripts/format.sh`:

- Statement: "The documentation phase moves a `///` when `IsWrittenBehindSource` and `DocumentsFollowingCode` hold…". True. It also moves one behind the node-level root's preceding token through `IsWrittenBehindRootPrecedingToken` / `FormattingContext.RootPrecedingToken`, which came from the merged blocker work.
- Statement: "The blank-line phase moves every `/** */` written behind code". Overstated. `EnsureBlankLineBeforeFirstComment` (two line breaks when the comment does not start its line) moves a delimited comment only when all of these hold:
  - it is the first comment in its token's leading trivia;
  - a preceding token exists;
  - that token is not `{` and not a switch-label colon (`IsFirstInBlock`);
  - `DocumentsFollowingCode` holds.
  
  Not moved today:
  - `class C` / `{ /** doc */` / `void M()…`;
  - `{ /** d */` before a statement or an initializer member;
  - `case 1: /** d */`;
  - `/* c */ /** d */` on a line of its own;
  - `/* banner */ /** doc */` at file start;
  - the second comment of `int A; /** a */ /** b */` (it stays behind the first one after the move).
- Statement: "…and also a `///` the documentation phase did not move". Moot in today's order. The documentation phase moves every `///` that passes `DocumentsFollowingCode`, and the blank-line phase skips the rest (`IsStrandedInlineDocumentation`). In c16/c19-style traces the blank-line phase added only the blank line for `///`.
- Statement: "The line-break phase moves a `///` behind `{` by accident: `BracePlacer.EnsureFirstContentOnNewLine` … called from `DeclarationBraceLineBreakRewriter`". Moot in today's order: the documentation phase moves it first. The method is also called from `LineBreakBlockRewriter` and `LineBreakInitializerRewriter`. For a delimited comment behind `{` that a line break follows, `BracePlacer` does not move it, because the leading trivia already holds the line break after the comment. When code follows on the same line (`class C { /** d */ void M() { } }`, `if (a > 0) { /** d */ a++; }`), `BracePlacer` moves the token together with its leading comment. Result: `{` / `/** d */ void M()`.
- Statement: "Once the line-break phase runs before the blank-line phase…". Hypothetical. The order today is documentation → using → blank lines → line breaks (`FormattingPipeline`).
- Both input examples of the issue give the issue's desired output today. `DelimitedDocumentationBeforeBlockStatementTests` exists. Its inputs put `{` on the next line; the issue's own example (`/** Doc */ {` on one line) is not among them.
- Using directive (the issue asks whether it is moved): `using System; /** doc */` / `using System.IO;` is moved today, as is the `///` form. In both forms, pass 2 removes the blank line in the using phase and the blank-line phase re-adds it. This is the using-block blank-line flip-flop that thoenissen/Reihitsu#1497 covers.

## Output questions

_None._ No visible output changes. The one candidate is a delimited comment behind `{` that moves like `///`. It would change output, and FMT-INDENT-006's example keeps that comment in place, so it goes under Follow-up candidates.

## Internal choices

- The statement case stays undecided for both forms; it is not decided for delimited comments only, as the issue proposed. FMT-BLANK-012's "precedes the code it documents" never decided statements, which CS1587 says are not documented. Alternative: specify the block-statement case for `/** */` only.
- Delimited comments behind `{`, a switch label, or an earlier comment of their own leading trivia are left out of FMT-DOC-001 by a coverage sentence instead of an exception with a Why. Their unmoved output has no reason beyond history and differs from `///`. Alternative: an explicit "MUST NOT move" exception.
- FMT-BLANK-012 is kept and narrowed, referring to FMT-BLANK-011, instead of being retired. Alternative: retire it and rely on FMT-BLANK-011 alone. That would drop the "in the same run" statement and the stranded-comment boundary.
- FMT-DOC-001 for delimited comments says "start a new line in front of the comment", not "onto a line of its own", because today's output keeps code behind the comment (`/** Doc */ {`, `/** doc */ int B;`).
- The change doc is dated `2026-10`. Move it if the PR merges in a later month.

## Follow-up candidates

1. **Formatter · delimited documentation comment behind an opening brace, a switch label, or an earlier comment on its line.** Today it stays; the `///` form in the same place moves.

   Input:
   ```csharp
   class C
   { /** doc */
       void M()
       {
       }
   }
   ```
   Before (`main`): unchanged.

   After A: unchanged (today).

   After B, like `///`:
   ```csharp
   class C
   {
       /** doc */
       void M()
       {
       }
   }
   ```
   Same split for `case 1: /** d */`, `/* c */ /** d */` on a line of its own, and `/** a */ /** b */`. Recommendation B: one rule for both forms. It changes FMT-INDENT-006's example and the tests that pin it: `KeepsDelimitedDocumentationBehind*Stable` in `ReihitsuFormatterNodeBlankLineTests`, `InlineDocumentationCommentIndentationTests`, `LineStartIndentationTests`, and the RH5204 formatter/analyzer tests.

2. **Formatter · documentation comment written behind code before a statement, a using directive, or inside a declaration or expression.** FMT-DOC-001 leaves this undecided for both forms. Today such a comment is moved with a blank line, which looks wrong in some places:
   ```csharp
   class C
   {
       [System.Obsolete] /** d */ void M()
       {
       }

       void N(int a, int b)
       {
           N(1, /** d */ 2);
       }
   }
   ```
   Today's output:
   ```csharp
   class C
   {
       [System.Obsolete]

       /** d */ void M()
       {
       }

       void N(int a, int b)
       {
           N(1,

             /** d */ 2);
       }
   }
   ```
   `///` behaves the same because both forms use `DocumentsFollowingCode`, which accepts the first token of any node (`void` of the return type, the literal `2`). Readings:
   - A: keep.
   - B: treat such a comment as stranded (FMT-DOC-003) and leave it where it was written.

   Recommendation B: the compiler does not read it as documentation. The issue manager should search for an existing issue first.

## Notes

- FMT-PIPE-024's "Documentation boundary" fact and `BlankLineTokenCleanupRewriter.StartsWithSingleLineDocumentationCommentAfterLineBreak` name only a single-line comment that leads a node-level root. FMT-PIPE-010's exception for the removed whitespace in front of a moved comment is stated for any documentation comment. I did not verify delimited comments at the node level (`FormatNodeInDocumentAsync`, target whose first token carries a `/** */` behind the preceding token).
- Files: `/home/user/Reihitsu/sdd/changes/2026-10/doc-comment-move-owner.md`, `/home/user/Reihitsu/sdd/specs/formatter/documentation-comments.md`, `/home/user/Reihitsu/sdd/specs/formatter/blank-lines.md`, `/home/user/Reihitsu/AGENTS.md`.
