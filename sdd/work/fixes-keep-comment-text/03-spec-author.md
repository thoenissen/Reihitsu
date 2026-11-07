Role: spec author
Commit: f42dde0
Verdict: spec written

Amendment round. It carries out `00-lead.md` decisions 4, 5, 6, 8, and 9 and the maintainer's answer B on output question 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decisions, which `00-lead.md` records.

No change doc (decision 3 stands). The RH5011 fix for the own-line shapes is the outcome the maintainer chose in #1075 ("insert after the comment's end"). The other changes clarify requirements whose decision is unchanged.

No new requirement IDs: `requirement-ids.txt` is unchanged from 461faa4. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd&FullyQualifiedName!~RequirementCoverage"`: 26 passed. `RequirementCoverageTests` still fails for RH5011-007, RH5025-006, and RH5030-003 until their tests exist.

## thoenissen/Reihitsu#1075

### Changed requirements

- **RH5011-007** — rewritten per decision 4 and retitled *A comment that spans the end of the `break` line moves the blank line below it* (clarification of an added requirement).
  - When the `break` line ends inside a comment and the next token (a statement or a switch label) starts on a later line than the line where the comments end, the fix inserts one empty line directly below that line, with that line's line ending (SHARED-FIX-008).
  - When the next token starts behind the comment on its last line, the fix is withheld.
  - The diagnostic is reported in both cases.
  - `Uses` is unchanged: SHARED-BLANK-001, SHARED-FIX-003, SHARED-FIX-008.
- **RH5027-001** — the negative example from 461faa4 gains the `/* c */ while` and `} /* c */ while` shapes, per decision 5. Its title line now names "a comment". `Uses` gains RH5024-001, because the text says that RH5024 reports the blank line above `} /* c */ while`. The normative sentence is unchanged and already decides both shapes.
- **RH5025-006** — "regular" is dropped, per decision 6. The requirement now names verbatim and raw interpolated strings, with one `$` or several. The *Why* says that a hole's `{` cannot start a line in a regular interpolated string. The example gains a `$$"""` case.
- **RH5030-003** — unchanged. RH5030 stays withheld (decision 4).
- **RH5023-002** — unchanged, per maintainer answer B.

### Examples for the tests

**RH5011-007.** The two spec examples cover the whole matrix in `02-skeptic.md`. Write one small test per row:

| Skeptic row | Input | Expected |
|---|---|---|
| a1 | `break; /* a⏎b */⏎a++;` | `break; /* a⏎b */⏎⏎a++;` |
| a3 | `break; /* a⏎b */⏎default:` | `break; /* a⏎b */⏎⏎default:` |
| a5 | `break; /* x */ /* a⏎b */ // c⏎// note⏎a++;` | the blank line goes below `b */ // c`, and `// note` stays directly above `a++;` |
| a2 | `break; /* a⏎b */ a++;` | reported, no fix offered |
| a4 | `break; /* a⏎b */ default:` | reported, no fix offered |

- Cover CRLF on at least one inserting row (FC-033).
- Add a Fix All scenario in which one of the inserting rows is next to a plain RH5011-004 case.
- Assert convergence: after the fix, RH5011 no longer reports.
- Row a6 (`/** a⏎b */`): I did not check whether the test harness, under `DocumentationMode.Parse`, parses it as a documentation comment. If it does, RH5011-001 says it is not reported. If the analyzer reports it, it belongs to the inserting class. Write the test for whichever of the two the harness shows, and do not assert a behavior that the spec does not state.

**RH5027-001.** One test per shape in the negative example: code, a verbatim literal, a multi-line comment, `/* c */ while`, and `} /* c */ while`. Also take `do s = $"""⏎x{a}⏎⏎"""; while (…);` from `02-skeptic.md`. Keep `do⏎    a++;⏎⏎while` and `} while` as controls that stay reported.

**RH5025-006.** `$"""`, `$$"""`, and `$@"`, each not reported. Add the control from `02-skeptic.md` that stays reported: a lambda `{` inside a hole, with a blank hole line above it.

**RH5030-003.** As in 461faa4. The withheld rows are b2, b3, b6, and b7. The rows b1 and b4 must still be fixed.

### Output questions

_None._

### Internal choices

- **RH5011 own-line shapes stay under RH5011-007 rather than going into RH5011-004.** RH5011-004 keeps its condition ("the `break` line ends outside a comment"). RH5011-007 states the other side of that condition (FC-002), with an example on each side. Alternative: widen RH5011-004 and keep RH5011-007 for the withheld case only. That was not done because RH5011-004's tests and examples would then have to change too.
- **The line the blank line goes below is defined as "the first line, from the `break` line on, that no comment continues beyond".** This mirrors RH5011-004's "ends outside a comment", so a `// c` that ends the line counts as ending on it (row a5).

### Follow-up candidates

These are unchanged from `00-lead.md`:
- the RH3208 trailing-comment shape;
- the RH5023 hole question.

## thoenissen/Reihitsu#1261

### Changed requirements

- **SHARED-BRACE-002** — reverted to its `main` text, per decision 8, plus one sentence: a comment behind the body on its last line stays inside the block, written once, and the fix MUST NOT repeat it behind the closing brace (SHARED-FIX-003). The *Why* gains one sentence. One example is added: RH5405 with `// trail`. In it the `if` is the first statement of its block, the spacing is canonical, and there is no other branch, so the example asserts none of the outputs that #1209 decided are wrong. `Uses` is `FMT-STRUCT-001, SHARED-FIX-003`. Everything else from the 461faa4 rewrite is gone: the "only that body" rule, the layout of moved code, the `else if` column, and the two examples.
- **FMT-PIPE-010** — clarified, per decision 9. The clarification holds whichever of the two existing owners ends up writing the trivia:
  - "Both MUST keep the target's trailing trivia" gains the rule that each comment in that trivia is written once.
  - When formatting leaves such a comment behind a token inside the result (the target's original last token, with inserted tokens behind it, such as FMT-STRUCT-001's closing brace), the comment stays there and is not written again behind the result's last token.
  - The code behind the target keeps its line.
  - The *Why* gains the reason (SHARED-FIX-003; document formatting writes the comment once).
  - A second example is added: `FormatNodeInDocumentAsync` on `if (a)⏎    M(a, b); // trail⏎b++;`.
  - The `Uses` row of `pipeline.md` gains SHARED-FIX-003.
  - I checked whether any other spec needs the clarification. FMT-CLEAN-008's *Why* ("restore the target's original trailing trivia") points to FMT-PIPE-010 and needs no change. `ControlFlowBraceTransform` withholds wrapping when the trailing trivia holds a directive, so the clarification names comments only.

Observed at f42dde0 with `scripts/apply-fix.sh RH5405` (LF and CRLF): the input in the SHARED-BRACE-002 example gives `M(a, b); // trail⏎} // trail⏎b++;`. `scripts/format.sh` writes the comment once, behind `M(a, b);`.

### Examples for the tests

- **SHARED-BRACE-002.** Its new example, for RH5405. Use the skeptic's matrix rows that avoid #1209's decided-wrong outputs:
  - `// trail` with the body on the `if` line;
  - a multi-line `/* a⏎b */`;
  - RH5406, with a multi-line body;
  - RH5407, an `else` body whose `if` body is already braced;
  - a nested `if (b)` that is the body of `if (a)`.

  Per decision 10, no test for `else if`.
- **FMT-PIPE-010.** Its new example, as a formatter test on `FormatNodeInDocumentAsync`. If the write-back in `FormatNodeInDocumentWithContextAsync` changes too, cover it with the same shape.

### Output questions

_None._ Decision 8 means questions 2–6 from `01-spec-author.md` are not asked. The new example's output is today's fix output without the duplicate, which is the outcome the maintainer decided in #1261.

### Internal choices

- **FMT-PIPE-010 is stated as output (comment once, behind the token where formatting leaves it), not as an owner.** The implementer picks which existing owner writes the trivia (decision 9). An implementation that moves the comment behind `}` would contradict both the FMT-PIPE-010 example and the SHARED-BRACE-002 example.

### Follow-up candidates

These are unchanged from `00-lead.md`:
- extend #1140 with the region blank lines;
- extend #1140/#1144 with the SHARED-BRACE-002 alignment.

## Whole run

- Rule pages are unchanged. RH5011's normal case is still "insert a blank line below the `break`", and the rest are corner cases or a withheld fix.
- Non-goals for the run, carried over:
  - no change to RH5023, RH5204, RH5602, RH6004, or RH8301–RH8303;
  - no change to RH3208's output through `BlankLineCodeFixUtilities`;
  - no new RH5030 fix for comment-held line breaks;
  - no SHARED-BRACE-002 alignment beyond the comment-once sentence.
