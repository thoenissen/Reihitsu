Role: spec author
Commit: a3190e7
Verdict: #1140: change doc written; #1687: no spec change (as asked); #1748: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decisions in `00-lead.md`, which this amendment carries out (D5 spec side, D6, D7, M2–M5).

**A change doc is required, so the lane has to widen to behavior change.** D5 changes what FMT-PIPE-010, FMT-PIPE-017, FMT-PIPE-018, and FMT-PIPE-024 require of `FormatNodeInDocumentAsync`. That is not a clarification (the code does not do it today) and not a bug fix (the formatter spec stated today's behavior): `sdd/README.md` requires a change doc for a changed requirement with new behavior. The precedent is `sdd/changes/2026-11/context-target-first-line.md`, which changed the same requirements for the context entry point, with code-fix results as its only visible outputs. SHARED-CHAIN-009 (M2) and RH5206-003 (M4) are the same kind of change: each requirement ordered the larger node, and `main` follows it. I wrote one change doc for the shared cause of #1140 and #1748, `sdd/changes/2026-11/fix-layout-scope.md`, `Status: Draft`. Its `Visible output changes` lists nine changes, and `Decisions` names the answer that decides each (M1–M5, or unchanged SHARED-FIX-006/007). Change 9, the tab-indented target, is the one no maintainer answer names (see Output questions). For traceability, its `Affected requirements` also lists the D3 clarifications. `scripts/lane-check.sh` will need `behavior-change` once source changes.

## thoenissen/Reihitsu#1140

### Changed requirements

Formatter (D5):
- FMT-PIPE-010 — Node-level entry points format only their target — changed: both entry points keep the target's leading trivia in every case. The exception for the whitespace in front of a documentation comment the target moves is gone, and the `Why` names SHARED-FIX-006 as the reason.
- FMT-PIPE-012 — `FormatNode` … — changed (wording): it no longer cites the preceding-token and list-position facts.
- FMT-PIPE-017 — Node-level targets take their columns from the indentation phase — changed: every character in front of the node's first token counts one column, a tab included. The tab example now gives `→→Call(1,` / seven spaces `2);`.
- FMT-PIPE-018 — The node-in-document entry points write back their target's leading trivia as in the document — changed: retitled, and it covers both entry points, including documentation comments (FMT-DOC) and FMT-BLANK-001/011/012/013/014. The FMT-PIPE-029 exception stays context-only. Two new `FormatNodeInDocumentAsync` examples (RH5403 comment; one-line `<summary>` plus a blank line behind `{`) and a `/** */`-behind-code case.
- FMT-PIPE-023 — example wording: the blank line is kept because the trivia is written back.
- FMT-PIPE-024 — Node-level input facts are plain values — changed: the `Preceding token` and `List position` rows are removed, together with the list-position paragraph. The blank-line, documentation, and comment-alignment bullets now decide as if no token stood in front of the root. The example now shows no blank line inserted behind `var a = 1;`.
- FMT-DOC-001 — `Why` only: the node-level entry points keep a leading documentation comment (FMT-PIPE-018).

Shared:
- SHARED-FIX-006 — changed: new bullet *Leading trivia* (D7, M1) and new bullet *Inside the reported construct* (M3, M5), with examples (RH5403 comment, RH5408 documentation comment, RH5101 `1+2`, RH5404 members).
- SHARED-CHAIN-008 — changed: the chain's columns come from the column its first token keeps.
- SHARED-CHAIN-009 — retitled "A chain fix changes only the chain", changed (M2): no fallback to the formatted statement. Examples: RH5201 (unchanged), RH5112 `var r=a.B()` / `.C()` in column 15, RH5112 in an `if` condition.

Rules:
- RH5110-003 — reverted to `main` (D6). It keeps `SHARED-FIX-006` and `FMT-BLANK-011` in `Uses` and gains a comment-above-method example (the leading trivia per SHARED-FIX-006).
- RH5111-004 — clarified (D7): the restated leading-trivia sentence is removed. The documentation-comment example is kept and refers to SHARED-FIX-006.
- RH5113-003 — clarified (D7, D10): only "the code in front of the semicolon stays" remains.
- RH5206-003 — changed (M4): the fix changes only the switch expression, laid out from its first token's document column (FMT-PIPE-017). The "innermost statement" sentence and the restated trivia sentence are gone. New example: the switch expression in an `if` condition.
- RH5401-003, RH5408-005 — clarified (D7): the restated sentences are removed, the examples are kept.
- RH5403-003 — reverted to `main` (D6: header and other branches), and changed (M5): the statements inside the reported block keep their own code, with no blank line between them. Examples: `int N() => 1; a += N();` and the #1748 comment.
- RH5404-003 — reverted to `main` (D6: header), and changed (M5): the members keep their own code, with no blank line between them. Examples: question 5's `class B`, and the #1748-style comment with `class B` (no header spacing).
- RH5101-003 — changed (M3): "lay out", and each argument keeps its own code. New example: `1+2`.
- RH5102-003, RH5303-005, RH5304-004, RH5305-004, RH5306-004, RH5307-004, RH5308-004 — changed (M3): one sentence each that references SHARED-FIX-006 *Inside the reported construct*. `Uses` updated.
- RH5417-003 — changed (M3/M5): the statements of the body keep their own code, and no blank line is inserted between them.

No new requirement IDs. No rule page changes: what is reported and how to fix it stay the same for the user.

### Examples for the tests

SHARED-FIX-006 (both new bullets), FMT-PIPE-018 (the two `FormatNodeInDocumentAsync` examples and the `/** */` case), FMT-PIPE-024 (no blank line behind `var a = 1;`), FMT-PIPE-017 (the tab example), SHARED-CHAIN-009 (the RH5112 examples), RH5206-003 (`if` condition; `return` behind `status++;`), RH5101-003 (`1+2`), RH5403-003 (local function; comment), RH5404-003 (members; comment), RH5110-003 (comment above method), RH5111-004, RH5113-003, RH5401-003, RH5408-005 (documentation comments).

Formatter tests that pin the old behavior and now contradict the spec: `ReihitsuFormatterNodeBlankLineTests` (`InsertsBlankLineAboveCommentOfRewrittenProperty`, `RemovesBlankLineBetweenOpeningBraceAndRewrittenFirstMember`, `SeparatesDelimitedDocumentation…InOnePassForRewrittenAndUnchangedProperty`, `…ForUnchangedProperty`), and any test asserting the old FMT-PIPE-024 example or FMT-PIPE-017 tab example. `DocumentLevelFormattingSeparatesDelimitedDocumentationWithoutTrailingWhitespace` is document-level and stays.

Effect to pin (derived, not measured): RH5404 on `class A { class B { } }` now leaves `class B { }` inside A, where it became `class B;`, and B stays reported. The RH5404-002 example (`class A { class B { } }`) is a diagnostics example and does not contradict this.

### Output questions

**1 · RH5101 · Argument list line breaks — an argument that spans lines** (RH5101-003, RH5102-003, SHARED-FIX-006 *Inside the reported construct*). M3 decided the spacing inside an element, not its line breaks. The spec now says nothing about a wrap inside an element, so the rule's "as the formatter lays it out" still governs it.

Input:
```csharp
        M(
            Foo(1,
                    2),
            3);
```
Before (`main`, derived from the join of wraps inside list elements; not measured):
```csharp
        M(Foo(1, 2),
          3);
```
After, A (recommended): the wrap stays, and the continuation line is aligned by the formatter from the element's new place.
```csharp
        M(Foo(1,
              2),
          3);
```
After, B: as `main`.
Why A: the line break inside `Foo(…)` is the element's own code, which M3 keeps, and its column is the list's layout. Not blocking: the tests for one-line elements can be written now.

**2 · Any layout fix · a target indented with tabs** (FMT-PIPE-017, SHARED-FIX-007 first bullet; change 9 of the change doc). With the leading trivia kept, `→→if (a > 0) { a++; }` (RH5403) gives `→→if (a > 0)`, `  {`, `      a++;`, `  }`. On `main` it gives eight spaces and the block in column 8. SHARED-FIX-007's first bullet already decides this (a tab counts one column, the fix does not replace it), so this is not open under the spec. But no maintainer answer names it, and it looks odd. The alternative is to withhold the fix where the whitespace in front of the target holds a tab, as SHARED-BRACE-003 does. Recommendation: keep it as the spec states. The input violates RH5601, and changing it means changing SHARED-FIX-007. Lead's call whether to list it in the next batch.

### Internal choices

- One change doc for #1140 and #1748 (shared cause). Alternative: no change doc, per D3. Rejected for FMT-PIPE-010/017/018/024, SHARED-CHAIN-009, and RH5206-003, because these change what `main` does as specified (see top). The D3 rule clarifications stay clarifications. They are listed in `Affected requirements` only so their outputs appear together.
- The preceding-token and list-position facts are removed from FMT-PIPE-024 rather than kept with "none" for every entry point. They only decided the root's leading trivia, which no entry point writes back now. This deletes code: `PrecedingTokenFacts`, `RootListPosition`, and their readers in the blank-line, documentation, and comment-indentation code. The "documentation boundary" fact stays: it is derived from the root itself. Alternative: keep the facts (dead).
- *Inside the reported construct* is stated once in SHARED-FIX-006, and each rule references it, as D7 does for the leading trivia. Alternative: one sentence per rule.
- RH5403-003 and RH5404-003 still "format the statement/declaration" (D6). They now carry the M5 exception for the reported block or body. The other branches of an `if` stay as on `main`. In SHARED-FIX-006, the requirements RH5110-003, RH5403-003, and RH5404-003 still contradict the *Formatting* bullet ("MUST NOT format the … statement, or the type that contains that node"). This is the known D6 gap, left on #1140 (FU3); per `sdd/README.md` it is not written into the spec as a note.
- SHARED-FIX-007's larger-node bullet is left as it is (it still points at FMT-PIPE-018, which now covers both entry points).
- `AGENTS.md`'s architecture paragraph names only the context entry point for the leading-trivia write-back. It is still true, so it is not edited. The implementer may extend it when `FormatNodeInDocumentAsync` changes.

### Follow-up candidates

FU1–FU3 of `00-lead.md` are unchanged. No new candidates.

## thoenissen/Reihitsu#1687

No spec text, as asked. Whether a qualifier is needed in SHARED-ATTR-008 or SHARED-BODY-003 waits for the implementation (D8).

## thoenissen/Reihitsu#1748

### Changed requirements

Covered by SHARED-FIX-006 *Leading trivia* (M1), FMT-PIPE-010/018 (D5), and the comment examples in RH5403-003 and RH5404-003. The examples now give no header spacing difference (D6).

### Examples for the tests

RH5403-003 comment example (with and without the comment, and behind a `case` label), RH5404-003 comment example, FMT-PIPE-018 RH5403 example, under LF and CRLF.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## Whole run

- Changed files: `sdd/changes/2026-11/fix-layout-scope.md` (new), `sdd/specs/formatter/pipeline.md`, `sdd/specs/formatter/documentation-comments.md`, `sdd/specs/shared/code-fixes.md`, `sdd/specs/shared/fluent-chain-layout.md`, `sdd/specs/analyzer/RH5101.md`, `RH5102.md`, `RH5110.md`, `RH5111.md`, `RH5113.md`, `RH5206.md`, `RH5303.md`–`RH5308.md`, `RH5401.md`, `RH5403.md`, `RH5404.md`, `RH5408.md`, `RH5417.md`, and this handover.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: passed, 51/51. `--filter "FullyQualifiedName~Rule"`: passed, 7/7.
