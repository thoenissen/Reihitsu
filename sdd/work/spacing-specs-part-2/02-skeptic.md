Role: skeptic
Commit: 8c41b0e6a6cb24a01ff22c70fc8b5f4d192b6471
Verdict: proceed

Issue thoenissen/Reihitsu#987, lane *specs and tests for existing behavior*. I checked the spec diff (`git diff origin/main -- sdd/specs`) against the code. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The run specifies existing behavior, so the premise to check is that each requirement states what the code does today. I read the code that decides each rule:

- RH6013, RH6018, RH6019, RH6020, and RH6022 scan the spaces and tabs after the token in the source text.
- RH6014 and RH6015 use `SameLinePrecedingWhitespaceAnalysis.GetSpan`.
- RH6016 calls `AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing` on every `DotToken`, every `?` whose parent is a `ConditionalAccessExpressionSyntax`, and every `->`.
- RH6017 uses `GetLeadingWhitespaceRunStart`, bounded by the previous token on the same line.
- RH6021 uses `DetermineLineBreakTolerantSpacing`.
- RH6023 and RH6024 use `HasOperatorSpacingViolation`. RH6023 checks the 13 assignment kinds, `EqualsValueClause`, and `NameEquals`. RH6024 checks all 22 `BinaryExpressionSyntax` kinds outside structured trivia.
- The fixes: the guard in `RemoveWhitespaceRunCodeFixProviderBase`, `TryGetReplacement` of RH6016 and RH6021, and `OperatorSpacingCodeFixProviderBase`, which formats the smallest node that holds the operator and both neighboring tokens.
- The formatter rules that `SpacingPolicy` applies.

Result: for the inputs it names, every requirement states current behavior. I found no requirement that contradicts the code. I did not re-run every block; the spec author's harness did that. I re-ran the Q1 and Q3 inputs with `scripts/apply-fix.sh`, and both premises hold.

Checks of the spec author's choices:

- **RH6014, RH6015, and RH6017 without SHARED-WS-002: agreed.** Their requirements report only a run that directly follows the previous token on the same line. The fix's guard is `previousToken.Span.End..token.SpanStart`, which for every in-spec input is exactly the run, so it can never hold a comment or a directive.
- **RH6016 and RH6021 on SHARED-GAP-001: agreed.** Both providers set the guard span to the replacement span.
- **Kept cases the lead was asked to confirm** (`class E /* note */: B`, `s /* note */.Length`, `s /* note */. Length`): no whitespace sits between the comment and the checked token in any of them, so thoenissen/Reihitsu#1078's scope does not reach them. The formatter agrees in each case: FMT-SPACE-013 inserts the space before `:`, keeps `*/.`, and removes the space after `.`. Keep them.
- **Classification of Q1, Q2, and Q3: agreed.** Q1 is ordinary, Q2 is decided by P7 (SHARED-FIX-005), and Q3 is P4 and ordinary.

Spec corrections for the spec author. Both are negative examples that pass whatever the code does (FC-017):

1. **RH6024-002.** Every `is` with a pattern in the example is spaced correctly (`o is null`, `o is string s`, `a is > 0 and < 5`), so the block says nothing about whether patterns are checked. Mis-spaced versions are not reported today, because `IsPatternExpression` is not registered. Verified with `scripts/apply-fix.sh RH6024`: `no-diagnostic` under LF and CRLF for

   ```csharp
   _ = o is  null;
   _ = o  is string s;
   return a is  > 0 and < 5;
   ```

2. **RH6016-002.** `int? n = null;` has no whitespace next to the `?`, so it does not show that a nullable `?` is not reported. `int ? n = null;` is not reported by RH6016 (`scripts/apply-fix.sh RH6016`: `no-diagnostic`, LF and CRLF).

Wording points, not blocking:

- **RH6021-005's `Why:`** says inserting a space between a comment and the colon changes a placement the author chose. The formatter does insert exactly that space (FMT-SPACE-013), so a later reader could take the `Why:` as a policy that the formatter breaks. SHARED-GAP-001's own `Why:` (the fix does not decide placements next to a comment, `general-notes.md`) is the reason that holds for both surfaces.
- **RH6016-005 and RH6021-005** consist entirely of SHARED-GAP-001's second sentence plus a reference. The merged pattern (RH6006-003) folds the shared reference into the fix requirement instead. Either form passes the checks.

## Mechanism and candidates

_N/A._ No bug fix in this run. The candidates the dispatch code decides are listed under *Breaking inputs*.

## Breaking inputs

Candidate set, taken from the dispatch code, with the defining type of each:

- **RH6016:** every `DotToken` the tree walk yields. That covers the dots of `MemberAccessExpressionSyntax`, `QualifiedNameSyntax`, and `MemberBindingExpressionSyntax`, and also of **`ExplicitInterfaceSpecifierSyntax`**, which the spec does not mention (it is open question N1 below). It also covers the `?` of `ConditionalAccessExpressionSyntax` and the `->` of `MemberAccessExpressionSyntax`.
- **RH6023:** `AssignmentExpressionSyntax` (13 kinds), `EqualsValueClauseSyntax`, and `NameEqualsSyntax`.
- **RH6024:** `BinaryExpressionSyntax` (22 kinds). Not `IsPatternExpressionSyntax`, `RelationalPatternSyntax`, or `BinaryPatternSyntax`.
- **RH6021:** `BaseListSyntax`, `ConstructorInitializerSyntax` (`base` and `this`).
- **RH6018, RH6019, RH6020:** `PrefixUnaryExpressionSyntax` of kind UnaryMinus, UnaryPlus, AddressOf, or PointerIndirection. RH6017: `PostfixUnaryExpressionSyntax` of kind PostIncrement or PostDecrement. RH6015: `NullableTypeSyntax` outside structured trivia. RH6013 and RH6014: `AttributeListSyntax`. RH6022: `ImplicitArrayCreationExpressionSyntax`.

Failure classes the test implementer must cover:

- **FC-019 / FC-005 / FC-020 — comments and directives.** For RH6013-003, RH6018-004, RH6019-004, and RH6020-004, the spec shows only a comment on the next line; a directive in the gap withholds the fix through the same guard. RH6022-003 shows only a directive, and RH6016-004 has a directive on the unreported side.
- **FC-003 — both sides of each boundary.** For each rule, one test with a comment in the gap and one without; one with a line break on a side and one without (RH6016, RH6017, RH6021, RH6023, RH6024).
- **FC-033 / FC-017 — line endings.** The line-end runs `···⏎` of RH6013, RH6018, RH6019, RH6020, and RH6022 depend on trivia, so run them under CRLF too.
- **FC-008 / FC-026 / FC-027 — Fix All with overlapping changes.** Today Fix All does not clear everything in one application for the inputs below, which violates SHARED-FIX-002 (merged and enforced through `FixAllCorrectsEveryDiagnostic`). Verified with the spec author's scratch probe (`FIXALL=1`, BatchFixer, document scope, one pass), compared against the single fixes applied one after another:

  | Rule | Input | Fix All, one pass | Single fixes one after another |
  |---|---|---|---|
  | RH6024 | `var x = a+b+c;` | `a + b+c` (still reported on the second `+`) | `a + b + c` |
  | RH6024 | `return a==b&&b>0;` | `a == b&&b > 0` (still reported on `&&`) | `a == b && b > 0` |
  | RH6024 | `var z = a+b*c  -  a;` | `-` still reported | fully fixed |
  | RH6024 | `return a+M(a+b, b+c, c)+c;` | last `+` still reported | fully fixed |
  | RH6023 | `d[k=1]=2;` | `d[k = 1]=2` (still reported) | `d[k = 1] = 2` |
  | RH6016 | `return s ? . Length;` and `return s? . Length;` | `s?. Length` (still reported on `.`) | `s?.Length` |

  The right-nested `a=b=1`, `a+=b-=2`, and the RH6016-004 input `s ? .Length` do converge in one pass.

  The existing RH6024 Fix All scenario (`value  is  string && value  as  string != null`) contains no nested reported operators, so it passes today. A Fix All scenario or a spec-derived code-fix test that contains the shapes above fails; the current Fix All result of those shapes is not to be encoded as expected output. They are listed for filing below (N2).
- **FC-009 — a fix that is offered but changes nothing.** Q2, plus N3 below. The RH6023 and RH6024 tests must not assert a fix there.
- **FC-030 — a fix that formats more than its target.** Q1 and its additional inputs in N4.
- **FC-013 — analyzer and formatter disagree.** Explicit-interface dots (N1). Everywhere else, the formatter's `SpacingPolicy` decides the same gaps as the analyzers. The known exceptions are filed or decided: RH6021's two spaces (#1079), and Q3.
- **FC-022 — interpolation holes.** RH6016, RH6017, RH6018, RH6022, RH6023, and RH6024 report inside holes. The examples include them; keep them in the tests.
- **RH6020-002's example does not compile** (CS0211, the address of `&a` cannot be taken), so its test must declare the compiler diagnostic.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH6013–RH6024 | Must stay; no production change in this lane |
| Code-fix providers and their bases (`RemoveWhitespaceRunCodeFixProviderBase`, `CommentSafeSpanReplacementCodeFixProviderBase`, `OperatorSpacingCodeFixProviderBase`) | Must stay. The SHARED-FIX-002 instances (N2) and the no-op instances (Q2, N3) are filed, not fixed |
| Fix All | Must stay; tested through `FixAllCorrectsEveryDiagnostic` with the existing scenarios |
| Formatter (`HorizontalSpacingPhase`, `SpacingPolicy`) | Not involved |
| `Reihitsu.Core` (`AdjacentTokenSpacingUtilities`, `FormattingTextAnalysisUtilities`, `UnaryOperatorSpacingUtilities`) | Not involved |
| Specs: 12 analyzer specs, `shared/operator-spacing.md`, `shared/token-gap-fixes.md`, index, ledger | Must change: the added specs, plus the two example corrections above |
| Rule pages under `documentation/rules/` | Not involved; no documented behavior changes |
| Tests (`src/Reihitsu.Analyzer.Test/Formatting/RH6013…RH6024…Tests.cs`) | Must change: tags and the missing tests the spec author listed |

Note on code owners. `CommentSafeSpanReplacementCodeFixProviderBase` (owner of SHARED-GAP) is also the base of `RemoveWhitespaceRunCodeFixProviderBase` (SHARED-WS), `CollapseTokenGapCodeFixProviderBase` (RH5105, RH5106, RH5108), and the RH5402 fix. None of those rules has a spec yet. SHARED-GAP-001 holds only because RH6016 and RH6021 pass a guard span equal to the replacement span. This is a fact for later specs, not a finding.

## Simpler option

_None._ The shared specs are the ones the issue asks for: more than one spec relies on SHARED-GAP-001 and SHARED-OPSPACE.

## Questions for the maintainer

None blocks this run; each case is left out of the specs. The lead files them.

### N1 — ordinary question (P4), for a new issue `SDD open questions: sdd/specs/analyzer/RH6016.md`: the dot of an explicit interface specifier

Input:

```csharp
interface I
{
    void M();

    int P { get; }
}

class C : I
{
    void I . M()
    {
    }

    int I .P => 1;
}
```

Real output:

- RH6016 reports both dots, and the fix produces `void I.M()` and `int I.P => 1;` (`scripts/apply-fix.sh RH6016`: `fixed` under LF and CRLF).
- The formatter (`scripts/format.sh`, two passes) leaves `I . M` and `I .P` unchanged. FMT-SPACE-006 and `NoSpaceSpacingRule.IsMemberAccessDotToken` tighten only the dots whose parent is a member access, a member binding, or a qualified name, not an `ExplicitInterfaceSpecifierSyntax`.
- So formatted code raises RH6016 (FC-013).

Readings:

- (a) The dot is a member-access symbol for both surfaces. RH6016 reports it, and the formatter also produces `void I.M()` (FMT-SPACE-006 extended).
- (b) The dot is not a member-access symbol. RH6016 does not report it, and the formatter keeps `void I . M()`.
- (c) Current behavior: RH6016 reports and fixes it, and the formatter keeps it.

This touches RH6016-001 and FMT-SPACE-006.

### N2 — bug against a merged requirement: Fix All needs more than one application. It violates SHARED-FIX-002 (`shared/code-fixes.md`), which is merged, so no reading is open

The inputs and outputs are in the FC-008 table above.

- **RH6023 and RH6024** belong in `SDD open questions: sdd/specs/shared/operator-spacing.md`, next to Q1 and Q2. Each fix formats the smallest node that holds the operator and its two neighboring tokens. When that node contains another reported operator that comes earlier in the document (a left operand, or an index or argument before the operator), the two text changes overlap, and the batch fixer keeps only the earlier one.
- **RH6016** belongs in `SDD open questions: sdd/specs/analyzer/RH6016.md`. A symbol spaced on both sides is fixed by replacing the whole span from the previous token to the next token. For `?` and `.` with whitespace between them, the two changes overlap, and the batch fixer keeps only one.

The lead may only create open-question issues without approval, so I recommend filing these there, marked "violates SHARED-FIX-002 (merged)" in the same way as a principle-decided case.

### N3 — extends Q2. Decided by principle P7 (#912); it violates SHARED-FIX-005 (added by thoenissen/Reihitsu#1068)

Input:

```csharp
// <AUTO-GENERATED>
class C
{
    void M(int a)
    {
        a=1;
    }
}
```

Real output:

- RH6023 is reported, and its fix is offered.
- Applying the fix returns the document unchanged: `FormatNodeInDocumentAsync` treats the file as generated, because FMT-PIPE-006 matches the marker case-insensitively.
- The compiler does not treat the file as generated. The same file with `// <auto-generated>` is not reported at all (SHARED-DIAG-001).
- RH6024 behaves the same with `// <Auto-Generated/>` and `return a+1;`.

How it was verified: with the spec author's scratch probe, which runs the analyzers through `CompilationWithAnalyzers`, so the compiler's generated-code detection applies. This case has the same mechanism as Q2, a formatter guard that the analyzer does not share, so Q2 and N3 can be filed as one question with two inputs. It touches SHARED-OPSPACE-004.

### N4 — extends Q1 (same readings, same issue)

More inputs where the operator fix does more than set the two gaps (`scripts/apply-fix.sh`, LF and CRLF):

- RH6024: `_ = a+⏎            b;` becomes `_ = a⏎            + b;`. The fix moves the operator to the next line, although SHARED-OPSPACE's non-goals leave wrap placement to the line-break rules.
- RH6023: `a=⏎            1;` and `a⏎            =1;` both become `a = 1;`. The fix joins the line break on the side the diagnostic did not report, although SHARED-OPSPACE-001's `Why:` leaves a wrapped side to the indentation.

### Cases matching decided follow-ups

None beyond the spec author's list. thoenissen/Reihitsu#1074: no instance; the whitespace in front of a literal, such as `"abc" .Length` and `- 1`, is reported as code. thoenissen/Reihitsu#1077 and thoenissen/Reihitsu#1079: nothing new.
