Role: skeptic
Commit: db9a911
Verdict: proceed narrower

I reviewed only the revision in db9a911 against `03-skeptic.md`, using `04-spec-author.md`. The lead's message carried no intended fix, suspected cause, or hoped-for verdict.

Six round-1 findings are fully resolved. Four more are resolved in the narrowed requirement itself, but the narrowing created a new false claim somewhere else, and three new instances fall inside the revised scopes. The specs need one more spec-author round before the test implementer.

Fixtures were run with `scripts/apply-fix.sh` (LF and CRLF, same result in both). Compile results come from `csc.dll` against the net10.0 reference pack, run on the original and the fixed text.

## Premise

### Round-1 findings: status

| Finding | Status |
|---|---|
| A.1 RH3002-002 | Resolved. The new example `(((object)s)).ToString()` reports the outer pair, which matches `IsSafeChainExpression`. |
| A.1 RH3002-006 | **Not resolved.** See 1 below. |
| A.2 RH3002-001 | Resolved. Wording residue in 7 below. |
| A.3 RH3005-005 | False claim removed, but **over-narrowed**. See 2 below. |
| A.4 SHARED-BODY-003 | Resolved. The new text and example change nothing outside `=> … ;`. The open question is incomplete (see 9 below). |
| A.5 SHARED-BODY-001 | Resolved. |
| A.6 SHARED-EMPTY-002 | Whitespace resolved. The interior comment is still claimed (see 5 below). |
| B.7 RH3101-004 | Resolved. The example uses a non-virtual `M()`. |
| B.8 RH3201-002 | **Not resolved, now inverted.** See 4 below. |
| C.9 RH3003-002 | Narrowed, but **RH3003-001 now overclaims**. See 3 below. |
| C.10 RH3201-002 | Narrowed, but **RH3201-001 now overclaims**. See 4 below. |

### New false claims

1. **RH3002-006.**
   - The lead-in "MUST report all of them but one" contradicts RH3002-001's own example `return {|RH3002:({|RH3002:(b)|})|};`. There, both pairs are reported; when no pair is needed, none stays.
   - The second bullet names RH3002-004 among the positions where "the outermost pair stays", but the code does the opposite:
     - `return ((_d))();` reports neither pair (`WouldLeaveCast` is true for both), which is RH3002-004's own example;
     - `return (((_d)))();` reports only the **outermost** pair, and the fix leaves `((_d))()`.

   The bullet holds for RH3002-002, RH3002-005, and the operand of a throw expression.
2. **RH3005-004 against the narrowed RH3005-005.** The guard also withholds the fix when the original comparison binds to a built-in operator but the swapped one binds to a user-defined operator. Fixture:

   ```csharp
   class V
   {
       public static implicit operator int(V v) => 0;
       public static bool operator <(V a, int b) => true;
       public static bool operator >(V a, int b) => true;
   }

   class C
   {
       bool M(V v)
       {
           return 1 < v;
       }
   }
   ```

   The original compiles and binds to the built-in `int < int`; `v > 1` would bind to `V.operator >`. The rule reports it and offers no fix (`no-fix-offered`). The new RH3005-005 covers only the original binding, so RH3005-004 now claims a fix here.
3. **RH3003-001.** It reports every empty literal "unless RH3003-002 applies". With RH3003-002 narrowed to "the value or pattern of a `case` label", RH3003-001 now claims that the `""` in `case var x when x == "":` is reported. It is not: any `CasePatternSwitchLabelSyntax` ancestor exempts it (RH3003 Q1).
4. **RH3201-001 and RH3201-002.**
   - **(a) Trailing comment.** RH3201-001 now claims that the trailing empty `//` of RH3201 Q6 (`int _a; //` between two `//` lines) is reported. It is not.
   - **(b) `///` that documents nothing.** The new RH3201-002 says that a line starting with a `//` comment counts, and that only a `///` comment documenting a declaration does not. So for RH3201 Q5's input (`// First.` / `//` / `/// Second.` in a method body), it now states the P13 outcome: not reported. The code reports and removes it. That is decided-but-not-implemented behavior written into the spec.

   Both inputs have to fall outside RH3201-001 and RH3201-002 alike, scoped the way RH3004-003 and RH3005-004 are scoped.
5. **SHARED-EMPTY-002.** "Remove the reported list from `(` to `)`" still covers SHARED-EMPTY Q1 (P1, a comment inside the list is deleted): `delegate ( /* none */ ) { }` becomes `delegate  { }`. The requirement needs to be scoped to a list with nothing but whitespace between `(` and `)`.
6. **The revised scopes still contain P1 instances:**
   - **RH3004-003.** The scope excludes only the operand of a cast. In `Func<int> f = delegate () { return 1; } + h;`, the original compiles. The fix writes `Func<int> f = () => 1 + h;`, and csc reports `error CS0019: Operator '+' cannot be applied to operands of type 'int' and 'Func<int>'`. The lambda's expression body takes in what follows. With a block body (`delegate () { } + g` → `() => { } + g`), the result compiles. This belongs with RH3004 Q3 (same mechanism, P1, compile error), and the scope must exclude it too.
   - **RH3106-001.** "Converted to the delegate type of the variable it initializes" includes `var a = delegate () { };`, because `a` gets the anonymous method's natural type. So SHARED-EMPTY-002 still claims the fix that RH3106 Q1 records as breaking the code (CS8917). The scope needs a variable declared with a delegate type.
   - **RH3103-001 and RH3103-004.**
     - "As a type in a declaration" can be read to include a using-alias directive. Then RH3103-004 claims `using N = int?;`, which fails under C# 11 (RH3103 Q1).
     - "In `typeof`" includes `typeof(Nullable<>)`, unless "has one type argument" is meant to exclude an omitted one. The analyzer counts the omitted argument, and the fix writes `typeof(?)` (RH3103 Q1).

     Both readings need to be ruled out in the text.

### Wording residue (minor, not wrong)

7. **RH3002-001.**
   - `int P { get => (_f); }` is reported and fixed. An accessor's expression body is not plainly "of a member or a local function"; naming accessors would remove the doubt.
   - `var t = ((a), 1);` is reported and fixed to `(a, 1)`. A tuple element is an `ArgumentSyntax` but not "an argument of an argument list", so this correct, reported case is now covered by no requirement.
   - "Argument list" can still be read to include an attribute argument list, which is RH3002 Q1.

### Open-questions list in `04-spec-author.md`

8. **Markings.** Complete and correct otherwise: P11 for RH3002 Q1, RH3003 Q1, and RH3201 Q6; P1 where a fix breaks compilation, run time, or comment text; P10, P9, and P13 as in round 1; no marking for RH3004 Q1, RH3006 Q1, and RH3202 Q1 (decision 4); no marking for SHARED-BODY Q1, as the lead routed. The expression-tree point is not filed, as the lead decided.

   Additions:
   - **RH3004 Q3:** the binary-operand input from 6.
   - **RH3201 Q4:** `return /**/a;` also becomes `returna;` (run). Any comment directly followed by code joins the tokens, because the fix also removes the whitespace before the comment.
9. **SHARED-BODY Q1 is incomplete.** Its "Touches" names only SHARED-BODY-003, but `shared/code-fixes.md` on `main` shows the member-wide relayout as expected output. The SHARED-FIX-003 example "layout, for RH3202" rewrites `///<summary>runs</summary>` into three lines. Its covering test, `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals`, is tagged `SHARED-FIX-003`. Reading (a) would change both, so the question must name them.

## Mechanism and candidates

_N/A_ — this is a spec of existing behavior. RH3004's lambda text and RH3005's swapped comparison are both spliced in without parentheses. The positions where the spliced text regroups with its neighbors are the candidate set for the RH3004-003 and RH3005-004 scopes:

- for RH3004, the operand of a cast and the left operand of a binary operator;
- for RH3005, an operand that is itself a comparison.

## Breaking inputs

These are added to round 1 and are for the test implementer once the spec is corrected:

- **FC-027:**
  - RH3002-006 with `((_d))()` and `(((_d)))()`;
  - `return ((b));` (both reported);
  - the initializer form `(((int)o))`;
  - the lambda form `((() => a))`.
- **FC-006:** RH3005 with `1 < v` from 2 (reported, no fix). It is a fixture for whatever RH3005-005 ends up saying about the swapped binding.
- **Characterization only (untagged):** pin these behaviors that the spec leaves open:
  - RH3201 Q5 and Q6;
  - RH3003 Q1;
  - `var a = delegate () { };`;
  - `delegate () { return 1; } + h`;
  - `return /**/a;`;
  - the RH3103 alias and `typeof(Nullable<>)` cases.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers, code fixes, `Reihitsu.Core`, formatter | must stay |
| Specs | must change: RH3002-006, RH3003-001, RH3005-005, RH3201-001/-002, SHARED-EMPTY-002, RH3004-003, RH3106-001, RH3103-001/-004; optionally the wording in 7 |
| `documentation/rules/RH3201.md` | must stay consistent with RH3201-002 as finally worded |
| Tests | must change, as mapped in `04-spec-author.md`; the SHARED-FIX-003-tagged RH3202 test stays as is |
| Open-question issues | must change: the additions in 8 and the "Touches" of SHARED-BODY Q1 in 9 |

## Simpler option

_None._

## Questions for the maintainer

_None new._ SHARED-BODY Q1 stays an open question, filed without a principle marking, as the lead routed. It also names the SHARED-FIX-003 example and its test (see 9).
