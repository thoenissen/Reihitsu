Role: skeptic
Commit: 6bbad3c
Verdict: proceed narrower

I reviewed only the revision in 6bbad3c against `05-skeptic.md`, using `06-spec-author.md`. The lead's message carried no intended fix, suspected cause, or hoped-for verdict.

Most of my 05 findings are resolved. Three blocking findings remain: one 05 finding is only partly resolved, one revised scope still includes a fix that breaks code, and one false claim I missed in rounds 03 and 05.

Fixtures were run with `scripts/apply-fix.sh` (LF and CRLF, same result in both). Compile results come from `csc.dll` against the net10.0 reference pack, run on the original text.

## Premise

### 05 findings: status

| 05 finding | Status |
|---|---|
| 1. RH3002-006 | Partly resolved. The lead-in no longer claims that one pair always stays, and RH3002-004 is no longer named. The second case is still wrong for a receiver (blocking 1). |
| 2. RH3005-004/-005 | Resolved. RH3005-005 names the swapped binding, the `1 < v` example matches (`no-fix-offered`), and RH3005-004 defers to RH3005-005. |
| 3. RH3003-001 | Resolved. A literal inside a `case` label is out of RH3003-001, and RH3003-002 covers only the value or pattern, so the `when` clause of Q1 is silent. |
| 4. RH3201-001/-002, Q5 and Q6 | Resolved. Both inputs fall outside both requirements. RH3201-002's two clauses match `IsDocumentationLine` (`///` but not `////`). |
| 5. SHARED-EMPTY-002 | Resolved. It is scoped to a list holding only whitespace. |
| 6. RH3106-001 | Resolved. "Declared type is a delegate type" excludes `var`. |
| 6. RH3103-001/-004 | Resolved. "Written type argument" excludes `Nullable<>`, and the listed positions exclude a using alias. |
| 6. RH3004-003 | **Not resolved.** See blocking 2. |
| 7. RH3002-001 | Resolved. Accessor and tuple element are now claimed; I checked both earlier (`get => (_f)` and `((a), 1)` are both fixed). |
| 8. RH3004 Q3 and RH3201 Q4 additions | Resolved. |
| 9. SHARED-BODY Q1 "Touches" | Resolved. It now names the SHARED-FIX-003 example and its test. |

### Blocking findings

1. **RH3002-006, second case.** The second case says that when the outermost pair is the receiver of RH3002-002 and the content needs a pair there, every pair but the outermost is reported. For a cast or a `switch` expression as content, the code does the opposite. Fixture: `return (((int)o)).ToString();` becomes `return ((int)o).ToString();`. The outer pair is reported, and the pair around the cast is never reported (RH3002-003).

   RH3002-002's own example, `var c = {|RH3002:(((object)s))|}.ToString();`, contradicts the second case in the same spec. The first case does not cover a receiver, because it requires the outermost pair to stand in an RH3002-001 position. Yet `06-spec-author.md` maps `NestedParenthesesAroundCastInMemberAccessChainReportTheOuterPair` and `…SwitchExpression…` to it. The second case holds for binary and conditional content as receiver, for the RH3002-005 target, and for the throw operand.
2. **RH3004-003: the scope still contains fixes that do not parse.** The scope excludes only the operand of a cast and the *left* operand of a binary operator. A lambda cannot stand unparenthesized as the *right* operand either, whatever its body.
   - `Action f = g + delegate () { };` compiles, and the fix writes `Action f = g + () => { };` (`invalid-result`).
   - `Func<int> f = g + delegate () { return 1; } + h;` compiles, and the fix writes `Func<int> f = g + () => 1 + h;` (`invalid-result`). On that text, csc reports `CS1525: Invalid expression term ')'` first.

   Both are inside RH3004-003's scope today. This is a further instance for RH3004 Q3 (P1, compile error). Q3's own mechanism sentence ("regroups with … a binary operator after it") is also too narrow: any binary-operand position breaks.
3. **RH3201-001 still claims the RH3201 Q1 inputs (P13).** I missed this in rounds 03 and 05. RH3201-001 carves out only an empty `//` whose lines above and below both start with `//`. That leaves Q1's inputs inside RH3201-001 as "reported":
   - an empty `//` between a `/* First. */` line and a `// Second.` line;
   - an empty `/* */` line between two `//` lines.

   The code reports them, but Q1 is "Decided by principle P13": a line that holds only a `/* */` comment counts like a `//` line, so these should not be reported. Under the standard the lead applied to Q5 and Q6, decided but unimplemented behavior must not be stated as a requirement either way. Both inputs have to fall outside RH3201-001 as well.

### Checked and holding

These revised claims hold against the code:

- RH3002-001's new positions;
- the first case of RH3002-006: `var x = ((((int)o)));` reports every pair but the innermost, and so does `Func<int> f = ((() => a));`;
- RH3003-001 and RH3003-002;
- RH3005-004 and RH3005-005;
- RH3103-001 and RH3103-004;
- RH3106-001;
- RH3201-002;
- SHARED-EMPTY-002.

I found no other input inside the new RH3005 or RH3106 scopes whose fix breaks the code. For RH3106 I tried a variable of type `System.Delegate`, `object`, and `var`: `System.Delegate` and `object` are not delegate types, and `var` has no declared type, so all three fall outside the scope.

## Mechanism and candidates

_N/A_. This is a spec of existing behavior. Candidates for RH3004-003's scope are every position where unparenthesized lambda text regroups or does not parse: a cast operand, and either operand of a binary operator.

## Breaking inputs

These are added to round 05:

- **Characterization only (untagged):** pin these as tests without a requirement tag:
  - `Action f = g + delegate () { };`
  - `Func<int> f = g + delegate () { return 1; } + h;`
  - RH3201 Q1's two inputs.
- **RH3002-006:** keep `(((int)o)).ToString()` and the `switch` receiver form out of the second case's tags until the text matches.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers, code fixes, `Reihitsu.Core`, formatter | must stay |
| Specs | must change: RH3002-006, RH3004-003, RH3201-001 |
| Tests | must change, as mapped, except that the RH3002-006 mapping of the cast and `switch` receiver tests changes with the text |
| Open-question issues | must change: add the two RH3004 inputs to RH3004 Q3 |

## Simpler option

_None._

## Questions for the maintainer

_None._

## Wording nits (non-blocking)

- **RH3201-005, first bullet.** "The comment is the only thing on its line" can be read to include a whitespace-only `/* */` that spans several lines. For that input, the fix leaves an empty line (RH3201 Q3), so under that reading the bullet's "together with the whole line, its line break included" is false. Saying "a comment on a single line" would remove the doubt. A test tagged with this bullet must use a single-line comment.
- **RH3103-001: correct cases nothing covers.** A property's type (`Nullable<int> P { get; }`), `new Nullable<int>(5)`, `(Nullable<int>)o`, and `default(Nullable<int>)` are reported and fixed correctly (from the code: each is a generic name binding to `Nullable<T>`, and `T?` stands in all of them), but no requirement covers them since the narrowing.
- **RH3002-001: correct cases nothing covers.** Arguments of a constructor initializer (`: base((a))`) are reported and fixed correctly (from the code: the parent is an `ArgumentSyntax`), but no requirement covers them since the narrowing.
