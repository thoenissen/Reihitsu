Role: spec author
Commit: a1b2334
Verdict: change doc written

This amendment applies the maintainer answers recorded in `00-lead.md` and builds on `02-skeptic.md`. The prompt carried no intended fix beyond those answers.

## Changed requirements

- FMT-PIPE-025 — An anonymous method in a raw-string hole keeps its lines — **withdrawn** (maintainer answer 1). It was never on `main`, so I removed it from `pipeline.md` and from the ledger rather than retiring it. The `FMT-BREAK-002` exception sentence, the non-goals in `line-breaks.md` and `blank-lines.md`, and the `Uses` entries added for it are reverted, so those three files match e094895 except for FMT-BREAK-011.
- FMT-INDENT-031 — An anonymous method in a raw-string hole aligns to its first token — **added** in `indentation.md`, with a ledger entry and `Uses` updated.
  - When the opening brace starts a line, the block shifts so that the brace stands under the first token: the first modifier, otherwise `delegate`.
  - The body keeps its indentation relative to the brace.
  - The anchor is measured after the literal moved.
  - The requirement does not apply inside the block body of a lambda or of another anonymous method in the same, innermost hole.
  - Outside holes, and in single-line raw literals, the scope depth applies as before.
- FMT-RAW-007 — **changed**: its first bullet's alignment range now names FMT-INDENT-031. `Uses` is updated.
- FMT-BREAK-011 — **changed further**: a line break that another requirement inserts in front of a lambda (FMT-BREAK-021, FMT-BREAK-022, FMT-BREAK-012) stays. A new example covers a collection initializer and a wrapped collection expression of lambdas.
- Code owners are unchanged. The type that implements FMT-INDENT-031 does not exist yet, so the implementer or spec checker adds it to `indentation.md` and `sdd/specs/README.md` once it exists.
- Change doc rewritten (still Draft):
  - What, Why, and entries 1–4 now describe the aligned shape.
  - Entry 5 is widened to all five gaps.
  - Entry 6 is unchanged.
  - Entry 7 is new.
  - `Decisions` records the maintainer answers.
- Spec architecture tests pass apart from the expected coverage gap: FMT-INDENT-031 has no test yet.
- No rule page changes.

## Examples for the tests

- **FMT-INDENT-031.** Every "after" block in its examples was verified as a fixed point of `main`'s formatter, which leaves author-written hole lines in place.
  - The issue input in a moved literal.
  - Statements with a blank line.
  - `static delegate`, which aligns under `static`.
  - An anonymous method inside an expression lambda.
  - A block lambda inside the anonymous method.
  - The unchanged side: inside a block lambda body in the same hole, and outside a hole.
- **`AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass`.** Its expected output becomes entry 1's "after" output.
- **FMT-BREAK-011.** Both examples, plus change-doc entries 5 and 7 (one test per gap; the skeptic's table names the unchanged rows) and entry 6 (with FMT-RAW-007).
- The skeptic's FC-002 list for FMT-PIPE-025 no longer applies. The qualifiers of FMT-INDENT-031 are "multi-line" literal, "innermost hole", block body of a lambda or anonymous method versus expression body, and modifier versus none. Each has an example on both sides, except a nested raw literal inside a block lambda in an outer hole, which has none yet.

## Output questions

**New visible output change beyond the answers: change-doc entry 7.**
- Today the lambda strip also undoes line breaks that FMT-BREAK-021 and FMT-BREAK-022 insert in front of a lambda element. This affects:
  - a one-line collection initializer of lambdas, which becomes `{ (x) => x, (x) => x⏎}`;
  - a wrapped collection expression whose first element is a lambda, which stays behind `[`;
  - two lambda elements on one line of a wrapped collection expression, which are not split.
- After the change these get the layout that `main` gives the same code with `(1)` in place of each lambda, which unchanged requirements already demand. Entry 7 shows the input, the before, and the after.
- The same mechanism means that, for the collection-expression row of answer 2, the columns change as well (`[` then one element per line), not only the blank line.
- Recommendation: accept. Nothing visible is in question beyond what FMT-BREAK-021 and FMT-BREAK-022 already state; it is listed because the output changes.

No other output question.

## Internal choices

- **The anchor is the first token**, per answer 1. `static delegate` therefore aligns under `static`, while FMT-INDENT-028 anchors a `static` lambda at its `(`. This is noted in the change doc's `Decisions`.
- **FMT-INDENT-031 is limited to blocks nothing else places.** It does not apply inside a lambda's or an anonymous method's block body in the same hole, where the scope depth relative to that body already places the block stably. The alternative was aligning there as well, which would change stable output today.
- **FMT-PIPE-025 was deleted instead of retired**, because it never reached `main`.

## Follow-up candidates

1. **Formatter · anonymous methods outside interpolation holes.** Their block takes the scope-depth column, while block lambdas align to the lambda. The maintainer's principle ("same formatting everywhere") and FMT-INDENT-031 now make an anonymous method in a hole align under its first token, but not one outside a hole. Example: `return M(delegate { return 1; });` gives `{` in the statement's column. Readings: A, keep the scope depth outside holes; B, align under the first token everywhere. This is outside the issue, so it is not asked in this run.
2. The `CS8967` candidate (regular `$"…"` hole below C# 11) and the lambda-behind-a-modifier candidate from `01-spec-author.md`, plus the skeptic's candidate 3, are unchanged.
