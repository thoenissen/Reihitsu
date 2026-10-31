Role: auditor
Commit: 640c650
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Repair delta of round 2 against `6285aee`. The head is `640c6509bdd684965c33e24c664faed70cd3af5b`. `origin/main` is `57f0f4537d38372f412108a8adbc0c2cd8051678`, which matches `git ls-remote origin refs/heads/main` and is an ancestor of the head. The checkout is the head with a clean tree. `git diff 6285aee..640c650` is what handovers 19–22 describe: `RawStringLineShifts` (`MoveLines`, `CollectEdits`, new `IsHoleOpeningLine` and `InsertLeadingText`), the new `RawStringMovedLineStartTests` (29 tests), the decision, and handovers 18–22. No spec or change doc changed, and `scripts/lane-check.sh behavior-change` confirms the lane. Scope did not grow.

**How I checked.** I reused the scratchpad harnesses and added two:

- round trip with values (`h/h.cs`) under LF and CRLF, plus idempotency;
- whole-tree annotation probe (`ha/a.cs`): every node and token of the input annotated;
- final text against the `origin/main` snapshot (`hm/h.cs`);
- new `hn23/n.cs`: node-level entry points on raw literals (statement, literal, and block through `FormatNodeInDocumentAsync`; the first hole with the statement as context through `FormatNodeInDocumentWithContextAsync`; `FormatNode` on the attached literal and statement), under LF and CRLF, built against the head, `main` (`hnm23`), and `6285aee` (`hn23r1`, snapshot in `r1snap/`).

I ran no repository test.

## Status of earlier findings

- **#1–#7 — closed** (closed in `18-auditor.md`; the repair touches none of their code).
- **#8 — closed.** Both members it named now round-trip: the first content line that starts with a hole (`$` and `$$`) and a later content line that starts with a hole behind a text line. The 13 members in `19-test-implementer.md` hold. I probed further members that the enumeration did not name, and none fails (scratchpad `c23/`):
  - the outer literal's first content line *and* a nested literal's first content line both start with a hole, so two text elements are inserted in nested nodes (`n1`, `n9`);
  - a nested literal behind a text line whose own first line starts with a hole (`n2`);
  - consecutive hole lines, `{x}{y}`, and a hole behind an empty line (`n5`);
  - `$$$` with `{{{x}}}`, a text line `{{y}}`, and `{{{{z}}}}` (one text brace, then a hole) (`n6`);
  - hole lines that start with an alignment `,5` or a format clause `:N2` (`n4`);
  - a hole line that starts with `x}` behind a lone `{` (`m4`);
  - two literals in one argument list, an expression-bodied property, and a `+` of an aligned and a column-0 literal (`n7`, `n8`, `m3`);
  - a multi-line raw literal nested in a hole of a verbatim `$@"` string and of a regular `$"` string, so it is walked as an outermost literal (`m1`, `m2`);
  - a verbatim `$@"` string nested in a raw hole whose line starts with its own hole `{`; `StartsInsideNestedText` gives that line no shift, so `IsHoleOpeningLine` is never reached and the verbatim text is left alone (`n3`).

  Every one of them round-trips with values under LF and CRLF in every phase, is idempotent, keeps every annotation of the whole tree, and has the same final text as `main`.

  **Node-level entry points.** On `n1`, `n2`, `n5`, `n6`, `n9`, `raw6`, `raw7`, and `raw8`, all six node-level modes give byte-identical output at the head and at `6285aee`, so the repair changed no node-level result. They differ from `main` only in the context mode with a target inside a moving literal. That difference is the third case of FMT-PIPE-017's gap, which the change doc records. On `main`, the annotation was lost and the document came back unchanged.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | medium | `src/Reihitsu.Formatter.Test/Unit/Pipeline/RawStringMovedLineStartTests.cs`, `AssertAnnotationsKept` | FC-017 | – | PR-introduced | **The gap.** The repair is the first path of the move that rebuilds a node of the literal: `InsertLeadingText` calls `node.WithContents(node.Contents.Insert(0, …))` on the `InterpolatedStringExpressionSyntax`. FMT-PIPE-022 requires annotations to survive "also where the phase changes … the node's children". `AssertAnnotationsKept` annotates only:<br>- every token of each literal;<br>- every node inside its holes.<br>It does not annotate the `InterpolatedStringExpressionSyntax` node or its `InterpolatedStringText` nodes, and `PhaseAnnotationTests` does not either, for any literal.<br>**Falsifying change.** Build the node as `SyntaxFactory.InterpolatedStringExpression(node.StringStartToken, contents, node.StringEndToken)`. That drops the literal's annotation on every first-line-hole member, and no test fails.<br>**Behavior holds today.** My whole-tree probe finds no lost annotation on any member above, including the nested double insertion. The helper's summary ("every token of each literal and every node inside its holes") is accurate, but it is narrower than the guarantee the new path needs. | Also annotate each literal node and its `InterpolatedStringText` nodes in `AssertAnnotationsKept`, so the first-line-hole members assert that the rebuilt literal keeps its annotation. |
| 10 | low | `RawStringLineShifts.ApplyEdits` summary | FC-031 | – | PR-introduced | The summary says an edit "at a position inside the token's text … replaces the leading spaces of that line of the text". The repair now sends a text token an edit at its own end (`LineStart == Span.End`, `OldLength` 0), and that edit appends the indentation of the next line, the one that starts with a hole. The code is correct, because `LineStart > SpanStart` includes the end. The summary does not name this new input. | Say that an edit at the token's end appends the indentation of a line that starts with a hole behind the token. |
| 11 | low | `RawStringLineShifts.IsHoleOpeningLine` | – (dead branch) | – | PR-introduced | The conjunct `interpolation.Parent is InterpolatedStringExpressionSyntax` always holds for an `InterpolationSyntax`. The summary says "a content line of an interpolated raw string literal", but the predicate does not test for raw. Only the shift map keeps non-raw holes away (see Guard scope). Behavior is correct (`n3`). | Drop the conjunct. Alternatively, state in the summary that only moving lines reach the predicate, so a non-raw hole never does. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A moved line that starts at a hole's `{` gets its indentation as text (`IsHoleOpeningLine`) | `FindToken(line.Start)`: the token is an `InterpolationSyntax.OpenBraceToken` whose `SpanStart` is the line start | The parser: the indentation in front of a hole is `InterpolatedStringText`. `StartsInsideNestedText` gives a hole line of a non-raw string no shift, so only raw holes reach the predicate | Agree on every member listed under #8, including `$$`/`$$$`, nested, and verbatim/regular nesting. The predicate is broader than its summary (#11) | ok |
| Which text holds that indentation (`previousToken.IsKind(InterpolatedStringTextToken)`, otherwise a new element at `Contents[0]`) | The token in front of the `{` | The parser: the text token in front ends at the line start, because it holds the line break. Only the start token can stand directly in front of a hole, and then the hole is `Contents[0]` | Agree. An empty or whitespace-only first line puts a text token in front (tested), and a hole's `}` is always followed by line-break text | ok |
| Value of the extended text token and of the inserted element | `CreateMovedTextToken` keeps `ValueText`; the inserted element has value `""` | The lexer strips the closing indentation | Agree; values were compared on every member | ok |
| Moved line's indentation length on a hole-opening line (`GetIndentationLength`) | `FindTrivia(line.Start)` or the leading space count | `{` carries no trivia, so the length is 0 and the edit only adds spaces. A move to the left cannot reach column 0 (FMT-RAW-003; nested shifts add up to the inner quote column, which is at least 1) | Agree | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which token holds a moved raw-string line's indentation | `rg -n "InterpolatedStringTextToken\|OpenBraceToken\|InterpolationSyntax" src/Reihitsu.Formatter/Pipeline/Indentation` finds the decision only in `RawStringLineShifts` (`CollectEdits`, `IsHoleOpeningLine`, `InsertLeadingText`, `ApplyEdits`, `HoldsRawStringText`, `IsNonRawStringText`). `LayoutComputer` (holes, line 360) and `LayoutModel` (`_rawStringHoleLines`) register hole lines for layout and write no indentation | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RawStringMovedLineStartTests` (29) through `AssertMoved` | FMT-PIPE-021 with values in every phase; final text by FMT-RAW-001 to FMT-RAW-004 under LF and CRLF; second-pass stability | A reparse difference in a phase tree; a changed final text; a second-pass change | `AssertRuleResult` (reparse hook with `HaveSameValue`), `FormatSyntaxTree` | Adequate for #8. The expected text is derived from the spec (`CreateSource` at the quote column), not from the code |
| Same tests, `AssertAnnotationsKept` | FMT-PIPE-022 | An annotation on zero, several, or a wrong-kind element | Annotates literal tokens and nodes inside holes only | weaker than the guarantee the new node rebuild needs, finding #9 |

## Failure classes dismissed
These are dismissals for the repair delta. The dismissals of `11-auditor.md` and `18-auditor.md` still hold for unchanged code.

- FC-001, FC-002, FC-022: covered under Guard scope. They are #8's classes, and the probes above found no failing member.
- FC-005, FC-019, FC-020, FC-021: a comment or directive inside a content line is text. The hole lines with trivia (`,5`, `:N2`, `x}`) keep the trivia path.
- FC-006: no literal value changes. The inserted element's value is `""`, which matches the reparse.
- FC-010, FC-012: every probe is idempotent, and `AssertRuleResult` checks a second pass.
- FC-011: no loop was added.
- FC-007–FC-009, FC-013, FC-014, FC-030: no analyzer or code fix changed.
- FC-015: covered under Policy ownership.
- FC-023: no identifier or escape handling is involved.
- FC-024, FC-029, FC-036: not reached.
- FC-025–FC-027: nesting was enumerated (nested and double-nested insertions, raw in verbatim and regular holes).
- FC-028: node-level outputs are unchanged against `6285aee` in all six modes.
- FC-031: the `MoveLines` and `CollectEdits` summaries describe the new code, and the new methods are documented. See #10 for `ApplyEdits`.
- FC-032: the compiled change `d0276cb` was tested by the implementer across every project, and the cleaner rebuilt and reran the class at `24b8d3e`. The later commits are handovers only.
- FC-033–FC-035: the expected values come from the spec.
- FC-037: no text-only proof is involved.

## Architecture findings

_None._ The repair keeps the token-editing move that the decision chose and adds no second owner.

## Hints
- **Harnesses and inputs** are in `/tmp/claude-0/-home-user-Reihitsu/73a9bd65-0b12-5c4b-9a2a-b0eaaa85c1fe/scratchpad/`:
  - inputs `c23/n1.cs`–`c23/n9.cs` and `c23/m1.cs`–`c23/m4.cs`;
  - node-level harnesses `hn23/n.cs`, `hnm23/n.cs`, `hn23r1/n.cs`;
  - outputs `n23_head.txt`, `n23_main.txt`, `n23_r1.txt`, `c23_head.txt`, `c23_main.txt`.
- The two follow-up candidates that the decision deferred still stand: the `///` separator wording, and direct `Reihitsu.Core.Test` coverage of `TokenSeparationUtilities`.
