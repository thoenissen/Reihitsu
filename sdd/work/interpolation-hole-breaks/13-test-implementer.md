Role: test implementer
Commit: 3e617ef (head the tests were written and run on; this handover's commit adds them)
Verdict: tests written

Repair round 1, decision 12 in `00-lead.md`, auditor finding #1 (`11-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. No spec was touched; `sdd/specs/formatter/line-breaks.md` is modified in the tree by the parallel spec author and was not staged.

## Tests

File `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/NonVerbatimInterpolationLineBreakTests.cs`. Every test runs `AssertRuleResult` (LF and CRLF, exact output, second pass, phase stability) on the class's `Build` fixture, parsed with `CSharpParseOptions.Default` (documentation mode `Parse`, so `/** */` is a documentation comment). Existing tests are unchanged.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| DocumentationCommentInFrontOfStatementInInterpolationKeepsItsLineInCSharp10 (`var a = 1; /** d */ return a;`) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentInFrontOfDeclarationStatementInInterpolationKeepsItsLineInCSharp10 (`var a = 1; /** d */ var b = 2;`) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentBehindLocalFunctionBlockInInterpolationKeepsItsLineInCSharp10 (`int G() { return 1; } /** d */ return G();`) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentInFrontOfLocalFunctionInInterpolationKeepsItsLineInCSharp10 (`var a = 1; /** d */ int G() => a;` → `… /** d */ int G() {return a; } …`) | FMT-BREAK-044 | `AssertResult` | fail |
| DocumentationCommentInFrontOfObjectInitializerMemberInInterpolationKeepsItsLineInCSharp10 (`new P { A = 1, /** d */ B = 2 }`) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentInFrontOfAnonymousObjectMemberInInterpolationKeepsItsLineInCSharp10 (`new { A = 1, /** d */ B = 2 }`) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentInFrontOfArgumentInInterpolationKeepsItsLineInCSharp10 (`K(1, /** d */ 2)`) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentInFrontOfSwitchLabelInInterpolationKeepsItsLineInCSharp10 (`case 1: return 1; /** d */ default: return 2;`) | FMT-BREAK-044 | `AssertKept` | fail |
| IfBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (`if (b) /** d */ return 1; return 2;`, unchanged) | FMT-BREAK-044 | `AssertKept` | fail |
| ElseBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (`if (b) return 1; else /** d */ return 2;` → `if (b) {return 1; }else /** d */ return 2;`) | FMT-BREAK-044 | `AssertResult` | fail |
| IfBodyFollowedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10 (`if (b) return 1; /** d */ return 2;`, unchanged) | FMT-BREAK-044 | `AssertKept` | fail |
| DocumentationCommentBehindOpeningBraceInInterpolationKeepsItsLineInCSharp10 (`{ /** d */ return 1; }`) | FMT-BREAK-044 | `AssertKept` | pass |
| DocumentationCommentBehindInitializerBraceInInterpolationKeepsItsLineInCSharp10 (`new P { /** d */ A = 1 }`) | FMT-BREAK-044 | `AssertKept` | pass |
| DocumentationCommentInFrontOfClosingBraceInInterpolationKeepsItsLineInCSharp10 (`return 1; /** d */ }`) | FMT-BREAK-044 | `AssertKept` | pass |
| DocumentationCommentInFrontOfInterpolationEndKeepsItsLineInCSharp10 (`{K(1, 2) /** d */}`) | FMT-BREAK-044 | `AssertKept` | pass |
| DocumentationCommentInFrontOfBinaryOperatorInInterpolationKeepsItsLineInCSharp10 (`{x /** d */ + 1}`) | FMT-BREAK-044 | `AssertKept` | pass |
| IfBodyBehindOrdinaryCommentInInterpolationGetsBracesOnItsLineInCSharp10 (`if (b) /* c */ return 1; return 2;` → `if (b) /* c */ {return 1; }return 2;`) | FMT-BREAK-044 | `AssertResult` | pass |
| OrdinaryCommentBetweenStatementsInInterpolationKeepsItsLineInCSharp10 (`var a = 1; /* c */ var b = 2;`) | FMT-BREAK-044 | `AssertKept` | pass |
| IfBodyFollowedByOrdinaryCommentInInterpolationGetsBracesOnItsLineInCSharp10 (`if (b) return 1; /* c */ return 2;` → `if (b) {return 1; /* c */ }return 2;`) | FMT-BREAK-044 | `AssertResult` | pass |
| DocumentationCommentInFrontOfStatementInInterpolationIsBrokenInCSharp11 (doc-comment phase, C# 11 side) | FMT-BREAK-044 | `AssertRuleResult`, full expected text | pass |
| IfBodyLedByDocumentationCommentInInterpolationIsBracedInCSharp11 (structural phase, C# 11 side) | FMT-BREAK-044 | `AssertRuleResult`, full expected text | pass |

The 11 failing tests are exactly the auditor's failing members (7 doc-comment phase, 3 structural phase; `var b = 2;` is the auditor's "also" variant). Each fails for the right reason: the first difference is the line break the doc-comment phase writes in front of `/** d */` (e.g. `var a = 1;⏎            /** d */ return a;`, `K(1,⏎                       /** d */ 2)`), or the one `ControlFlowBraceTransform` writes behind the inserted `{` (`if (b) {⏎/** d */ return 1; }`, `else {⏎/** d */ return 2; }`) or behind the inserted `}` (`{return 1; }⏎/** d */ return 2;`). Every other character of the observed output matches the expected one.

Derivation of the expected values (from requirements, not from a patched copy):

- Doc-comment phase: FMT-DOC-001 would start a new line in front of the comment; FMT-BREAK-044 forbids that line break below C# 11, so the comment stays behind the code on its line. With the spacing already canonical and no blank line allowed (FMT-BREAK-044's blank-line part), the output is the input; in the local-function case the body conversion of decision 7 still applies.
- Structural phase: FMT-STRUCT-026 decides that an inserted `{` followed by a comment that leads the braced statement, and an inserted `}` followed by a comment that leads the next token, need a line break. FMT-BREAK-044 forbids that line break, so no wrap satisfies both and the body stays unbraced (decision 12: "refuse it by its cause"). Where no comment leads the next token (the `if` body in the `else` case), the wrap still happens with the decision-7 spacing: no separator behind the inserted `{` or in front of `else` (FMT-STRUCT-026; `}`→`else` is a pair no spacing rule covers, FMT-SPACE-002).
- If the implementer chooses instead to wrap without the line break (e.g. `if (b) {/** d */ return 1; }`), the three structural tests that expect the body to stay unbraced fail; that is a question for the lead, since decision 12 calls for a refusal.
- Ordinary `/* c */` holding cases: the comment trails the token in front of it, so FMT-STRUCT-026 writes no line break; the outputs follow decision 7's spacing and were confirmed green on this head.
- C# 11 sides: pin today's output, which the run does not change (FC-003), as in `03-test-implementer.md`.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `/** d */` between two statements in a lambda in a `$"` hole, C# 10 | LF, CRLF | formatter pipeline | reproduced | line break in front of the comment; expected the input unchanged |
| `/** d */` after a comma (initializer, anonymous object, argument), C# 10 | LF, CRLF | formatter pipeline | reproduced | line break in front of the comment; expected the input unchanged |
| `if`/`else` body led or followed by `/** d */`, C# 10 | LF, CRLF | formatter pipeline | reproduced | braces wrapped with a line break at `{` or `}`; expected the body unbraced on its line |

## Not covered, and why

- Documentation mode `None`: `/** */` lexes as an ordinary comment there and stays trailing trivia, the shape the three ordinary-comment holding tests already cover (auditor, FC-040).
- `/** d */` at the start of the hole: pinned by the existing `DelimitedDocumentationCommentInInterpolationKeepsItsLineInCSharp10` (c29).
- Node-level entry points: the language version reaches the phases through the context that `FormatNodeInDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project` and `FormatDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project` already pin; a target inside a hole is follow-up candidate 1.
- Verbatim `$@"` side for these shapes: the verbatim condition is a shared predicate argument already tested on both sides by the existing class (spec example `b`, `AtDollarVerbatimInterpolationIsBrokenInCSharp10`).
- Directives: cannot stand in a one-line hole.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"
```

Result on this head: 62 tests, 11 failed (the failing members above), 51 passed.

## Follow-up candidates

_None._
