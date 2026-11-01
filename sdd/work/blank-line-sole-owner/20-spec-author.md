Role: spec author
Commit: d744bbb
Verdict: spec written

Repair round 2, the implementer finding D2 and the two FMT-BLANK-018 readings in `19-implementer.md`, decided by the lead in `00-lead.md` row 28. The prompt named the changes it wanted. It carried no suspected cause and no hoped-for verdict. I checked both readings against the formatter built from the branch's HEAD (d744bbb) before writing them. The inputs are in the session scratchpad under `r20/`.

## Changed requirements

- FMT-SWITCH (spec header) — the `Uses` row of `sdd/specs/formatter/switch-case-braces.md` gains FMT-BLANK-018. FMT-SWITCH-007 names it since 17-spec-author. `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` passes now. The architecture project passes 92/92 (Debug).
- FMT-BLANK-018 — A comment glued to a token belongs to the token's line — clarified. A new paragraph after the definition says that the comment shares the token's line only when no line break stands between the comment's start and the token, inside a comment included. It also says that the end-of-file token takes no glued comment. A new last example shows both sides. The other requirements and the change doc are unchanged, because both readings state today's behavior.

### Verified behavior (branch HEAD)

| Input | Output | Reading |
|---|---|---|
| `A();⏎/* x⏎ */ B();` | blank line inserted above `/* x` | a multi-line comment ending on the token's line is not glued |
| `.B()⏎/* x⏎ */ .D();` (chain link) | blank line inserted above `/* x` | same, the implementer's `ChainLinkBehindMultiLineCommentTests` case |
| `A();⏎/* x */ B();` | unchanged | the other side: glued, no FMT-BLANK-011 line |
| `}⏎// trailing comment` at the end of the file | blank line inserted above the comment | the end-of-file token has no glued comment |

The spec example input (`A();⏎/* a */ B();⏎/* b⏎ */ B();` in a method, `// trailing` behind the class) formats to the example's output, and a second pass changes nothing.

## Examples for the tests

FMT-BLANK-018, the new last example. `ChainLinkBehindMultiLineCommentTests.ReportedLinkBehindMultiLineCommentTailStartsItsOwnLine` and `TrailingCommentWithNoBlankLineGainsOneAtEndOfFile` already cover the two readings. A test of the spec example itself is optional.

## Output questions

_None._ Both readings state the output the code gives today, on the branch and in the existing tests. Nothing visible changes.

## Internal choices

- The readings are written as part of the definition of "shares that line", not as new bullets. The bullets decide blank lines for a glued comment, while these readings decide whether a comment is glued at all. The alternative was a note behind the bullet list.
- `FMT-BLANK-018` is appended to the end of the `Uses` row. The row is not sorted, and the test checks only the set.

## Follow-up candidates

_None._ The chain-link probe (`var x = A()⏎.B()⏎/* x⏎ */ .D();`) joins `.B()` onto `A()` and puts `.D()` on its own line behind the comment. That layout belongs to the line-break and indentation requirements and the RH-reported chain-link case the implementer cited. It is outside this run's question.
