Role: skeptic
Commit: a36fdfc
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue prescribes a solution ("walk the list's tokens once, carrying the previous token"). Because the issue is `agent-filed`, I treat that as a proposal, not a goal (see *Issue goals*).

## Premise

The premise holds, and it is reached. Every timing below comes from the Debug `Reihitsu.Cli.dll` that `scripts/format.sh` builds (`dotnet src/Reihitsu.Cli/bin/Debug/net11.0/Reihitsu.Cli.dll <dir> --check`), or from `scripts/apply-fix.sh`. Wall-clock times include about 0.25 s of process start-up for the CLI and about 4.8 s of start-up and build check for `apply-fix.sh`.

**How the precheck is isolated.** `FindTriviaInFrontOfComma` (`src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs:144`) runs after the `DescendantNodesAndSelf` loop of every other shape check and returns at the first comma whose gap holds the FMT-UNSUP-009 shape. So:

- A *tail* fixture puts the shape at the last comma. The comma check then visits every comma, the file is refused, and no other phase runs. Its time is parse plus the full precheck.
- A *head* fixture puts the shape at the first comma. The comma check stops at once, while the other checks still walk the whole list.
- The difference between the two is the comma check alone.

Generators, written to `<dir>/A.cs`. Each tail element list is `0, 1, …` on one line, followed by the comment, a line break, and the last comma:

```shell
# CLI, array initializer; variant "tail" or "head"
python3 - "$n" "$v" <<'EOF'
import sys
n=int(sys.argv[1]); v=sys.argv[2]
if v=="tail":
    body=", ".join(str(i) for i in range(n))+" /* c */\n        , "+str(n)
else:
    body="0 /* c */\n        , "+", ".join(str(i) for i in range(1,n+1))
print("namespace N;\n\ninternal static class C\n{\n    private static readonly int[] A = new[] { "+body+" };\n}")
EOF
```

Whole-document formatting (CLI), array initializer, each run twice:

| n | tail (comma check runs to the end) | head (comma check stops at once) |
|---|---|---|
| 1 000 | 0.48 / 0.37 s | 0.24 / 0.24 s |
| 5 000 | 0.86 / 0.93 s | 0.26 / 0.23 s |
| 10 000 | 2.68 / 2.48 s | 0.32 / 0.30 s |
| 20 000 | 8.31 / 8.07 s | 0.43 / 0.39 s |

Every tail run printed `error: …/A.cs(5,<col>): unsupported syntax: a comment or directive between a list element and the comma behind it. The file was left unchanged.` The tail column grows about 3.5x per doubling of n once start-up is subtracted, which is quadratic. The head column is linear. So the rest of the precheck, the parse, and the CLI's own checks are linear, and the quadratic cost is the comma check.

**Control: the cost depends on one parent's child count.** I put 20 000 commas into 2 000 separate `int[]` fields of 10 elements each, with the tail shape in one final two-element field (`new[] { 0 /* c */\n        , 1 }`). That took 0.57 / 0.76 s, against 8 s for one 20 000-element list.

**Other parent kinds**, CLI, n = 20 000, tail shape:

| Fixture | Time | Result |
|---|---|---|
| Enum, `V0, V1, …, V19999 /* c */\n    , Z` | 18.9 s | refused, same reason |
| Argument list, `M(0, 1, …, 19999 /* c */\n            , 0);` inside `M(params int[] a)` | 10.4 s | refused, same reason |

**Code-fix registration** (`scripts/apply-fix.sh RH5303 <dir>`, two arms: LF and CRLF). The fixture is a collection initializer in `return new List<int> { 0,\n            1, …, n-1 /* c */\n            , n };` (tail), or `{ 0 /* c */\n            , 1, …, n-1,\n            n }` (head). Every run reported `2 no-fix-offered`, so RH5303 reports the initializer and the provider withholds its fix after calling `ReihitsuFormatter.FindUnsupportedSyntax(formatTarget)` (`RH5303…CodeFixProvider.cs:89`).

| n | head | tail |
|---|---|---|
| 1 000 | 4.83 s | 5.01 s |
| 10 000 | 5.51 s | 10.35 s |
| 20 000 | 5.68 s | 27.66 s |

At n = 20 000 that is about 11 s of precheck per registration in Debug. The issue's Release figures (4.7 to 14 s at 20 000) are of the same order.

**The common case is not the tail case.** A list that holds no shape at all makes the comma check visit every comma, exactly like the tail fixture. That follows from the code, which only returns early on a find. I used the tail shape only to keep formatting out of the measurement.

**Callers that reach it:**

- Whole-document formatting and the CLI, on any non-generated file with one long separated list (`FormattingPipeline.cs:148` runs `PrecheckPhase` first).
- The registration of every code fix that calls `ReihitsuFormatter.FindUnsupportedSyntax` on a node that contains such a list. There are 28 call sites under `src/Reihitsu.Analyzer.CodeFixes/`. Examples: RH5303 and RH5305 on the initializer or collection itself, RH5101 and RH5102 on an argument list, and RH7101 on `fieldDeclaration.Parent`, which is the whole type. On that path the precheck is the whole cost of registration.

No analyzer calls the precheck.

**Severity.** The issue states it correctly and does not overstate it. Time grows quadratically with the number of elements in *one* list. It is negligible for realistic lists: at n = 1 000 the comma check costs about 0.1 to 0.2 s in Debug. It becomes noticeable only for lists of several thousand elements in one parent, such as hand-written data tables and embedded byte arrays. That input class is rare but reachable. No verdict, reason, location, or other visible output is involved.

## Mechanism and candidates

**Mechanism.** For every comma token, the comma check asks Roslyn for a neighboring token: `comma.GetPreviousToken()` at line 153, and `previousToken.GetNextToken(includeZeroWidth: true)` in `EnumerateGapTrivia` at line 201. Each lookup costs time that grows with the number of children of the comma's parent. A parent with n separated elements therefore costs O(n) per comma and O(n²) per list.

I measured the dependence on one parent's child count with the many-short-lists control above. I did not trace Roslyn's internals to see which of the two calls dominates. The issue's split is 4.7 s for `GetPreviousToken` and 8.1 s with `GetNextToken` added, at 20 000, so both contribute.

**Dispatch.** There is no kind switch. The check takes every token of `node.DescendantTokens()` whose kind is `SyntaxKind.CommaToken`. It does not descend into structured trivia, so commas in documentation comments and directives are not visited. Candidates are therefore every syntax node that holds a comma among its own children, which in practice means every owner of a `SeparatedSyntaxList`:

- initializers (array, collection, object), collection expressions;
- argument, bracketed argument, parameter, bracketed parameter, type argument, type parameter, and attribute argument lists;
- `VariableDeclarationSyntax` declarators, enum members, base lists, constraint clauses;
- tuple types and expressions, positional and property patterns, list patterns, switch expression arms;
- anonymous object members, `for` initializers and incrementors, array rank specifiers (omitted sizes), function pointer parameter lists, interpolation alignment.

All of them share the mechanism, because the cost depends only on how many children the comma's parent has. Measured: array initializer, collection initializer, enum member list, argument list. The rest follow from the dispatch; I did not run them.

## Breaking inputs

**FC entries.** None of the catalog's classes concerns cost. The ones that touch the changed code concern verdict identity: a comment or directive in a gap, a zero-width token in a gap, and node-level inputs. The class matrix below covers this defect.

**Class matrix for the defect.** It has three axes:

- parent kind: any of the candidates above;
- where the first shape sits in that parent's list: none, at the last comma, or at the first comma;
- the entry point: whole document (CLI), or node-level `FindUnsupportedSyntax` (code-fix registration).

| Shape position | Whole document | Node-level |
|---|---|---|
| none | fails (quadratic): follows from the code; formatting time masks it on this path | fails (quadratic): follows from the code, not run in isolation |
| at the last comma | fails (measured, 4 parent kinds) | fails (measured, RH5303) |
| at the first comma | holds (linear, measured) | holds (linear, measured) |

Many short lists hold (linear, measured).

**What the change must keep identical.** The issue requires the precheck's verdicts, reasons, and locations to stay identical. These are the facts of today's code that a rewrite could change, each with existing coverage in `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedListSeparatorCommentTests.cs`:

1. **The gap starts behind the last token *with text* in front of the comma.** `GetPreviousToken()` skips zero-width tokens, so an omitted element belongs to the gap. The gap also includes the leading and trailing trivia of every zero-width token in it. Covered by the rows `typeof(Dictionary<\n// c\n,>)` and `typeof(Dictionary< // omitted\n                ,>)` (line 127f.), and by the supported rows at lines 310f.
2. **A line break counts wherever it stands in the gap's text,** also inside a multi-line comment. Covered by the spec-example tests.
3. **The first comma in text order with the shape wins.** Its reason depends on whether `comma.Parent is VariableDeclarationSyntax`. Covered by `FirstOfTwoCommentsIsReported` and `DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment`.
4. **Commas in structured trivia and in disabled text are not visited.** Covered by `CommasInsideDocumentationCommentsAndDirectivesAreSupported` and `CommaInADefinedBranchIsUnsupportedAndInDisabledTextIsNot`.
5. **Node-level inputs are decided from the node alone.** A comma whose previous token with text starts before `node.FullSpan.Start` is skipped (line 155f.). Covered by `FindUnsupportedSyntaxAnswersForTheListAndNotForPartOfIt` and `ElementTargetWithoutTheCommaIsNotRefused`. I found no node that a code fix passes whose first token in front of a comma lies outside the node: every list owner starts with a token that has text, and so does every format target I read. I did not construct such a case, so I make no claim that one is reachable.

**Candidate inputs for the reproduction.**

- A single list of tens of thousands of elements that holds no shape. This is the common case, and the precheck's answer for it is `null`.
- The same list with the tail shape, whose answer is the FMT-UNSUP-009 reason at the comment.
- Both through `FindUnsupportedSyntax` on the list owner.

My recommendation is one array initializer, plus one other parent kind such as an argument list or an enum. A per-kind sweep would add nothing, because the mechanism does not depend on the kind.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` comma check (`FindTriviaInFrontOfComma`, `EnumerateGapTrivia`) | Must change: its cost |
| `PrecheckPhase` other checks (FMT-UNSUP-004, -008, -010, -011) and the candidate ordering in `FindUnsupportedSyntax` | Must stay: they are measured linear by the head fixtures |
| `ReihitsuFormatter.FindUnsupportedSyntax` | Must stay: it delegates |
| Code fixes (28 call sites) | Must stay: they gain speed, with no change of verdict |
| CLI, other formatter phases | Must stay (see the follow-up candidate on formatting cost) |
| Analyzers, Fix All, `Reihitsu.Core`, rule docs | Not involved |
| `sdd/specs/formatter/unsupported-syntax.md` | Must stay: no spec states a cost, and no requirement changes, so no clarification or change doc is needed |
| Existing precheck tests | Must stay unchanged and pass |

## Refuse option

There is no meaningful refuse option. The cost is in deciding *whether* to refuse, so withholding a fix or leaving a construct as written does not avoid it: the check runs on every input before any refusal.

Declaring a shape unsupported is not an option either. "A long list" is not a shape that a comment or directive defines, and refusing it would refuse common code.

**"No change"** is the one alternative. It costs nothing, but it leaves code-fix registration at about 11 s (Debug) for a 20 000-element list.

## Output questions

_None._ Nothing visible changes. Verdicts, reasons, and locations must stay identical, as the existing tests lock.

## Internal choices

1. **How the reproduction proves the cost.** A wall-clock assertion is the only direct proof, and wall-clock budgets are flaky under CI contention (`AGENTS.md`, the `Regex` timeout note).
   - **A.** Use one test with a generous budget on a large n. For example, the precheck of a 50 000-element list under a budget of a few seconds. Today that is tens of seconds; a linear scan is milliseconds, so the headroom is two orders of magnitude. There is precedent: `[Timeout(30000, CooperativeCancellation = true)]` in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/CommentJoinTests.cs:203`. The precheck observes no cancellation token, so the test implementer must confirm how such a timeout fails before relying on it.
   - **B.** Use no timing test. Keep only verdict-identity tests, and record the measurement in the PR.

   Recommendation: **A** with a large margin. AGENTS.md requires a regression test first for a formatter bug fix, and only a time bound can fail today.

2. **The issue's "Done when" sweep over the repository's own sources.** The repository's sources are formatted, so whole-document precheck answers `null` for every file both before and after the change. A sweep of them proves almost nothing about verdict identity.

   Recommendation: rely on the existing precheck tests, which hold every FMT-UNSUP-009 case, plus the new large-list inputs. Run the sweep only as a cheap extra (`scripts/format.sh src --check` reports no file with unsupported syntax before and after).

## Issue goals

- **"Walk the list's tokens once, carrying the previous token along"** is a prescribed solution, not a goal. Drop it as a requirement: the goal is linear cost with identical answers, and the implementer chooses how. No visible output.
- **"Verdicts, reasons, and locations identical on the repository's own sources and on the existing precheck tests"**: keep the existing-tests half, and treat the sources half as weak evidence (internal choice 2). No visible output.
- **"The precheck of a 20 000-element array initializer takes time that grows linearly with n (well under one second)"**: keep it as the purpose. A test cannot robustly assert "grows linearly"; it can assert a generous bound (internal choice 1). No visible output.
- **The issue's statement that the precheck is about 5 % of formatting time** holds: at n = 10 000, about 2.2 s of a 40.5 s CLI run. Fixing the precheck therefore does not make formatting such a file fast; see the follow-up candidate.

## Architecture findings

_None._ The cost comes from a local choice of navigation API. No two components share a decision.

## Follow-up candidates

**Whole-document formatting is itself quadratic in the length of one list. This is outside this issue.**

CLI timings on a clean single-line array initializer (`new[] { 0, 1, …, n-1 }`, no shape, so the file is formatted), Debug:

| n | Time |
|---|---|
| 2 500 | 2.9 s |
| 5 000 | 9.9 s |
| 10 000 | 40.5 s |

That is about 4x per doubling. The issue reports 144 s for one `FormatSyntaxTree` pass at 20 000 in Release. Removing the precheck's share leaves the rest, which lies in later phases that I did not attribute.

The input class (thousands of elements in one hand-written list) is rare but reached by the CLI and by code fixes that format such a list. It changes no output, so it is a performance follow-up, not an output question. The issue manager should search for a duplicate first.
