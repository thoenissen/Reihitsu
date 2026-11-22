Role: test implementer
Commit: 61036b2 (head the tests were written and run on; this handover's commit adds them)
Verdict: tests written

Decision 17 in `00-lead.md` (auditor finding #3, `17-auditor.md`), read with the spec author's amended FMT-BREAK-044 (7babdbe) and decisions 19 and 20. The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. No spec was touched, and only the test file and this handover are staged.

Rule under test (FMT-BREAK-044, *Structural transforms*): below C# 11, the FMT-STRUCT-001 brace transform wraps no `if`/`else` body when a non-verbatim interpolation that contains the body holds a documentation comment anywhere between its braces, at every depth; otherwise the body is braced, also when only a sibling interpolation of the same string or only an ordinary comment is present. Expected values come from that sentence (body unbraced → output equals input, `AssertKept`) and, on the braced side, from FMT-STRUCT-001/026 with the spacing of decision 7 (FMT-SPACE-002, FMT-SPACE-009), the same derivation as `13-test-implementer.md`.

## Tests

File `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/NonVerbatimInterpolationLineBreakTests.cs`. Every test runs `AssertRuleResult` (LF and CRLF, exact output, second pass, phase stability) through `AssertKept`/`AssertResult` on the class's `Build` fixture, C# 10 unless stated.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| ElseBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 — re-derived: was `if (b) {return 1; }else /** d */ return 2;`, now the input unchanged | FMT-BREAK-044 | `AssertKept` | fail |
| NestedIfBodyFollowedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (`if (a) if (b) return 1; /** d */ return 2;`, finding #3 member 1, spec `u`) | FMT-BREAK-044 | `AssertKept` | fail |
| NestedElseBodyFollowedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (member 2) | FMT-BREAK-044 | `AssertKept` | fail |
| ThreeNestedIfBodiesFollowedByDocumentationCommentInInterpolationKeepTheirLineInCSharp10 (member 3) | FMT-BREAK-044 | `AssertKept` | fail |
| OuterIfBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (`if (a) /** d */ if (b) return 1; return 2;`; auditor's holding member, re-derived: today `… if (b) {return 1; }return 2;`) | FMT-BREAK-044 | `AssertKept` | fail |
| InnerIfBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (today `if (a) {if (b) /** d */ return 1; }…`) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentInFrontOfNestedElseInInterpolationKeepsItsLineInCSharp10 (today the outer and `else` bodies are braced) | FMT-BREAK-044 | `AssertKept` | fail |
| ElseIfChainFollowedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (today `if (a) {return 1; }else if …`) | FMT-BREAK-044 | `AssertKept` | fail |
| ElseIfBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 | FMT-BREAK-044 | `AssertKept` | fail |
| IfStatementInInterpolationWithDocumentationCommentElsewhereKeepsItsLineInCSharp10 (`… }) /** d */}`, comment next to no brace, spec `v`) | FMT-BREAK-044 | `AssertKept` | fail |
| IfStatementInVerbatimStringNestedInNonVerbatimInterpolationWithDocumentationCommentOutsideItKeepsItsLineInCSharp10 (`$"a {$@"x {F(() => { if (b) … })}" /** d */} b"`, comment in the containing `$"` interpolation, outside the `$@"` string; decision 19) | FMT-BREAK-044 | `AssertKept` | fail |
| IfStatementInVerbatimStringNestedInNonVerbatimInterpolationWithDocumentationCommentKeepsItsLineInCSharp10 (comment in the `$@"` hole inside the `$"` hole) | FMT-BREAK-044 | `AssertKept` | pass |
| IfStatementInNonVerbatimStringNestedInVerbatimInterpolationWithDocumentationCommentKeepsItsLineInCSharp10 (`$@"a {$"x {… /** d */ …}"} b"`) | FMT-BREAK-044 | `AssertKept` | pass |
| NestedIfBodiesInInterpolationGetBracesOnTheirLineInCSharp10 (no comment → `if (a) {if (b) {return 1; } }return 2;`) | FMT-BREAK-044 | `AssertResult` | pass |
| NestedIfBodiesFollowedByOrdinaryCommentInInterpolationGetBracesOnTheirLineInCSharp10 (`/* c */` → `{return 1; /* c */ } }return 2;`, spec `t` nested) | FMT-BREAK-044 | `AssertResult` | pass |
| IfStatementInInterpolationGetsBracesWhenAnotherInterpolationOfTheStringHoldsDocumentationCommentInCSharp10 (spec `w`, decision 20) | FMT-BREAK-044 | `AssertResult` | pass |
| IfStatementInInterpolationGetsBracesWhenAnotherStringHoldsDocumentationCommentInCSharp10 (`var t = $"x {/** d */ 1}";` on the line before) | FMT-BREAK-044 | `AssertResult` | pass |
| NestedIfBodyFollowedByDocumentationCommentInInterpolationIsBracedInCSharp11 (C# 11 side of member 1, spec `u` C# 11 block) | FMT-BREAK-044 | `AssertRuleResult`, full expected text | pass |

Round-1 structural tests re-derived under decision 17: only `ElseBodyLedBy…` changes. `IfBodyLedBy…` and `IfBodyFollowedBy…` already expect the input unchanged; `IfBodyBehindOrdinaryComment…`, `IfBodyFollowedByOrdinaryComment…`, `OrdinaryCommentBetweenStatements…`, `IfStatementInInterpolationGetsBracesOnItsLineInCSharp10`, and `IfBodyLedByDocumentationCommentInInterpolationIsBracedInCSharp11` hold no doc comment in the hole or run under C# 11 and stay as they are (green). The doc-comment-phase tests hold no `if` and are untouched.

Each of the 11 failing tests fails for the right reason: the only difference between observed and expected output is the braces the transform inserts (e.g. `if (a) if (b) {return 1; }/** d */ return 2;`, `if (a) /** d */ if (b) {return 1; }return 2;`, `if (b) {return 1; }return 2; }) /** d */}`); no line break and no other character differs.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Finding #3 members 1–3 (nested if, nested else, three levels), C# 10 | LF, CRLF | formatter pipeline | reproduced | inner bodies braced with `}/** d */`; expected every body unbraced |
| `if` in a hole that holds a doc comment not next to any inserted brace, C# 10 | LF, CRLF | formatter pipeline | reproduced (new member of the class) | body braced; expected unbraced |

## Not covered, and why

- `while`/`for`/`foreach`/`using` bodies: decision 17 and the amended spec name `if`/`else` bodies only; `UsingStatementInInterpolationKeepsItsLineInCSharp10` (decision 8) pins that a using body is not braced at all.
- Documentation mode `None`, directives, `//`: as in `13-test-implementer.md` (cannot occur in a one-line hole, or lex as ordinary comments, which the ordinary-comment tests cover).
- Plain `$@"` / `@$"` with a doc comment: the verbatim condition is the shared predicate argument, already tested on both sides (`AtDollarVerbatimInterpolationIsBrokenInCSharp10`, spec example `b`).
- Node-level entry points: the language version reaches the phases through the context the two existing `…InCSharp10Project` tests pin; no code-fix path changed.
- The whole second spec example as one test: every one of its members (`x`, `y`, `z`, `u`, `v`, `t`, `w`, C# 10 and C# 11 for `u`, `y`, `x`) is pinned by a member test of this class.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"
```

Result on this head: 79 tests, 11 failed (listed above), 68 passed.

## Follow-up candidates

_None._
