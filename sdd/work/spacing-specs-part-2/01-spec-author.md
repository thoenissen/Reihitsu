Role: spec author
Commit: 053b069
Verdict: spec written

Issue thoenissen/Reihitsu#987, lane *specs and tests for existing behavior*. No change doc: every requirement states current behavior. No rule page under `documentation/rules/` changes, because no documented behavior changes. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

New specs: `sdd/specs/analyzer/RH6013.md` to `RH6024.md`, `sdd/specs/shared/operator-spacing.md` (SHARED-OPSPACE, owner `OperatorSpacingCodeFixProviderBase`), and `sdd/specs/shared/token-gap-fixes.md` (SHARED-GAP, owner `CommentSafeSpanReplacementCodeFixProviderBase`). `sdd/specs/README.md` and `sdd/specs/requirement-ids.txt` are updated.

## Changed requirements

All added:

- RH6013-001 — Whitespace after an opening attribute bracket is reported
- RH6013-002 — One diagnostic per bracket
- RH6013-003 — The fix deletes the whitespace unless a comment is in the gap
- RH6014-001 — Whitespace before a closing attribute bracket on the same line is reported
- RH6014-002 — One diagnostic per bracket
- RH6014-003 — The fix deletes the whitespace unless a comment is in the gap
- RH6015-001 — Whitespace before a nullable `?` on the same line is reported
- RH6015-002 — A `?` that is not a nullable type, and documentation references, are not reported
- RH6015-003 — One diagnostic per `?`
- RH6015-004 — The fix deletes the whitespace unless a comment is in the gap
- RH6016-001 — Whitespace next to a member-access symbol on the same line is reported
- RH6016-002 — A side across a line break and other `?` symbols are not reported
- RH6016-003 — One diagnostic per symbol
- RH6016-004 — The fix removes the whitespace on each reported side
- RH6016-005 — A comment in a rewritten gap withholds the fix
- RH6017-001 — Whitespace before a postfix `++` or `--` on the same line is reported
- RH6017-002 — Prefix operators and operator declarations are not reported
- RH6017-003 — One diagnostic per operator
- RH6017-004 — The fix deletes the whitespace unless a comment is in the gap
- RH6018-001 — Whitespace after a unary minus is reported
- RH6018-002 — The space that keeps two minus signs apart is not reported
- RH6018-003 — One diagnostic per sign
- RH6018-004 — The fix deletes the whitespace unless a comment is in the gap
- RH6019-001 to RH6019-004 — same four requirements as RH6018, for the unary plus
- RH6020-001 — Whitespace after `&` or `*` of a pointer expression is reported
- RH6020-002 — The space that keeps two `&` apart is not reported
- RH6020-003 — One diagnostic per operator
- RH6020-004 — The fix deletes the whitespace unless a comment is in the gap
- RH6021-001 — A base-list or constructor-initializer colon without a space is reported
- RH6021-002 — One space on a side, or a line break, is not reported
- RH6021-003 — One diagnostic per colon
- RH6021-004 — The fix puts one space on each reported side
- RH6021-005 — A comment in a rewritten gap withholds the fix
- RH6022-001 — Whitespace after `new` of an implicitly typed array is reported
- RH6022-002 — One diagnostic per creation
- RH6022-003 — The fix deletes the whitespace unless a comment is in the gap
- RH6023-001 — Assignment and initializer `=` are checked
- RH6023-002 — The fix puts one space on both sides
- RH6024-001 — Binary operators are checked
- RH6024-002 — Directive conditions and patterns are not reported
- RH6024-003 — The fix puts one space on both sides
- SHARED-OPSPACE-001 — A gap that is not exactly one space is reported
- SHARED-OPSPACE-002 — Gaps with a line break or a comment are not reported
- SHARED-OPSPACE-003 — One diagnostic per operator, on the operator
- SHARED-OPSPACE-004 — The fix puts one space on both sides
- SHARED-GAP-001 — A comment in a rewritten gap withholds the fix

How shared behavior is used:

- RH6013, RH6014, RH6015, RH6017, RH6018, RH6019, RH6020, and RH6022 use SHARED-WS-001 and SHARED-WS-002 (their fixes derive from `RemoveWhitespaceRunCodeFixProviderBase`).
- RH6016 and RH6021 use SHARED-GAP-001. Their fixes rewrite one or both gaps around the token, and only a comment in a gap that is rewritten withholds the fix. SHARED-WS-002 does not fit them: it checks the whole gap around one deleted run, and RH6021 inserts whitespace, which is a non-goal of `shared/whitespace-removal-fixes.md`. Both specs depend on that one requirement, so it is shared, as the issue asks.
- RH6023 and RH6024 use SHARED-OPSPACE-001 to SHARED-OPSPACE-004.
- Formatter policies are referenced, not restated: FMT-SPACE-004/005/006/007/009/010/012/013/014 and FMT-CLEAN-001.

How the statements were established:

- I wrote a scratch harness outside the repository. It referenced `Reihitsu.Tooling.csproj` to load the shipped analyzers and code-fix providers. For each input it printed the reported spans in verifier markup, recorded per diagnostic whether a fix was offered, applied the single fixes one after another, and ran the provider's Fix All (`BatchFixer`) once.
- I extracted every C# block of the 14 new specs, removed its markup, and ran it through the harness:
  - every reported span matched the markup exactly;
  - every before/after pair matched;
  - Fix All matched the sequential result in every block;
  - no example has a compile error, except RH6020-002's `& &a`, which the spec says does not compile.
- `scripts/test.sh --project architecture` filtered to `SpecConsistencyTests`, `SpecStructureTests`, and `RuleDocumentation*` passes (26 tests). `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails only on the 49 new IDs listed above, because no test is tagged yet.

Note for the skeptic: the gap check that SHARED-OPSPACE-001/002 specify is implemented by `FormattingTextAnalysisUtilities.HasOperatorSpacingViolation`. That type is already a code owner of `shared/blank-line-spacing.md`, so it is not listed here, and no spec owns this method's behavior on its own. `OperatorSpacingCodeFixProviderBase` is the only owner of SHARED-OPSPACE.

## Examples for the tests

Every requirement above has an example from which a test can be derived. Most cases already have a test in `src/Reihitsu.Analyzer.Test/Formatting/RH60xx…AnalyzerTests.cs`, but none of those tests is tagged. These cases have no existing test as far as the test names show:

- RH6013 tabs, `return:`/`assembly:` targets, parameter and lambda attributes, the line-end run, and the next-line comment that withholds the fix. Today's tests are `VerifyIssueIsDetectedAndFixed` and Fix All only.
- RH6015 casts, `typeof`, type arguments, `as int ? ?? 0`, and `?[]`.
- RH6018–RH6020 line-end runs, `-(-a)`, interpolation holes, and the next-line comment that withholds the fix.
- RH6022 tab, multi-dimensional, line-end run, interpolation hole, `new int []`, and the directive that withholds the fix.
- RH6016 `using`/namespace/`global::` qualified names and the extended property pattern.
- RH6021 tab sides, enum/record/struct/primary-constructor base lists, and other colons not reported.
- RH6023 enum member, property initializer, parameter default, `using` alias, `with`, `??=`, and interpolation hole.
- RH6024 `>>>`, `??`, `a- -b`, interpolation hole, `is` patterns, and relational patterns.
- SHARED-GAP-001: both blocks, for each of RH6016 and RH6021.
- SHARED-OPSPACE-001 to SHARED-OPSPACE-004: these are enforced through `Uses`, so each needs a tagged test, for example on RH6024.

## Open questions to file

### Q1 — for a new issue `SDD open questions: sdd/specs/shared/operator-spacing.md`: the operator fixes reformat the whole operand expression

Input and real output (RH6023, RH6024; verified with the harness):

```csharp
_ = a+M(a, () => { return 1; });     // RH6024 fix →
_ = a + M(a,
          () =>
          {
              return 1;
          });

Action f=()=>{ M(1); };              // RH6023 fix →
Action f = ()=>
           {
               M(1);
           };

var x=new[]{1,2};                    // RH6023 fix →  var x = new[] {1, 2 };
```

`_ = a⏎            /* c */ +b;` also gains a blank line before the comment line (FMT-BLANK-011), from the RH6024 fix.

Why it looks wrong: the fix formats the smallest syntax node that contains the operator and both neighboring tokens. That node is the whole binary expression, assignment, or declarator, so the fix rewraps lambdas, argument lists, and initializers that the diagnostic is not about. FMT-PIPE-010 gives its reason as "code fixes may only change the code their diagnostic is about". The rule pages say only "Use a single space on both sides of the operator".

Readings:
- (a) The fix changes only the two gaps next to the operator: `_ = a + M(a, () => { return 1; });`.
- (b) Current behavior: the whole node is formatted.

Recommendation: (a). This touches SHARED-OPSPACE-004 and FMT-PIPE-010. No principle decides it: P1 does not apply, because nothing breaks at compile time or run time and no comment or literal changes, and P7 does not apply.

### Q2 — same issue as Q1: an operator fix that changes nothing in a file with a syntax error. Decided by principle P7 (#912); it violates SHARED-FIX-005 (added by thoenissen/Reihitsu#1068)

Input:

```csharp
class C
{
    void M(int a)
    {
        a=1;
    }

    void N(
}
```

Real output:
- RH6023 is reported on `=`, and its fix is offered.
- Applying the fix returns the document unchanged, because `ReihitsuFormatter.FormatNodeInDocumentAsync` returns the original document for any syntax error (FMT-PIPE-005).
- The same holds for RH6024.

How it was verified: by running the analyzer and the provider directly. `scripts/apply-fix.sh` rejects fixtures that do not parse.

This touches SHARED-OPSPACE-004. Until #1068 is merged, the case stays out of the spec.

### Q3 — for a new issue `SDD open questions: sdd/specs/analyzer/RH6021.md`: a colon glued to a comment after it

Input: `class G :/* c */ B` (with `class B { }`).

Real output:
- RH6021 is reported on the colon.
- No fix is offered, because the comment is in the gap the fix would rewrite (SHARED-GAP-001).
- The formatter keeps `:/* c */`: FMT-SPACE-013 only collapses the whitespace in front of a comment and never inserts any.

Why it looks wrong: the user cannot clear the diagnostic without moving the comment, and formatted code still raises it. thoenissen/Reihitsu#1078 decided the mirror case for rules that remove whitespace; this rule requires a space.

Readings:
- (a) The colon is not reported for a side where a comment directly follows it. This matches the formatter.
- (b) It is reported, and a fix inserts a space before the comment. The analyzer would then disagree with the formatter in the other direction.
- (c) Current behavior.

Recommendation: (a). This touches RH6021-001, RH6021-005, SHARED-GAP-001, and FMT-SPACE-013. It is an analyzer/formatter disagreement (P4), so it is an ordinary question. The leading-side twin `class F /* c */: B` is consistent: the formatter inserts the space after the comment (FMT-SPACE-013), and it is specified as reported without a fix.

## Cases matching decided follow-ups

These are left out of the specs. The lead may comment them on the matching issue.

thoenissen/Reihitsu#1078. The shape: whitespace between the checked token and a comment after it, or between a comment and the checked token across a line break. The formatter keeps that whitespace, and the rule reports it without a fix.

- RH6013: `[ /* c */ Obsolete]`; `[ // c⏎        Obsolete]`
- RH6014: `[Obsolete⏎        /* keep */ ]`
- RH6015: `int⏎        /* c */ ? _d;`
- RH6016:
  - `_ = s. /* c */ Length;`
  - `_ = s /* c */ . Length;`. Here the dot is reported once, and the fix is withheld as a whole because the side after the dot touches the comment.
- RH6018: `var q = - /* c */ a;`
- RH6019: `var q = + /* c */ a;`
- RH6020: `int t = * /* c */ p;`
- RH6022: `var f = new /* c */ [] { 1 };`

One classification for the lead to check: a comment, then whitespace, then the checked token, all on one line. Examples are `[Obsolete /* c */ ]`, `int /* c */ ? a`, `a /* c */ ++`, and `s /* c */ .Length`.
- I specified these as reported without a fix. RH6003-001 and RH6008-001 do the same, and the formatter removes that whitespace (FMT-SPACE-013/014).
- #1078's list of rules does not include RH6008, whose merged spec has exactly this shape.
- If the lead reads #1078 as covering it too, these examples come out of RH6014-001/003, RH6015-001/004, RH6016-001/005, RH6017-001/004, and SHARED-GAP-001.

thoenissen/Reihitsu#1079. The shape: a rule that requires exactly one space accepts more (like RH6005's `operator  -`).

- RH6021: `class E  :  B` and `class E→ : B` are not reported. A side passes whenever the character next to the colon is a space. The rule page says one space, and the formatter makes it one (FMT-SPACE-009). A tab directly next to the colon is reported.

thoenissen/Reihitsu#1077. The shape: `/** */` that documents nothing is not treated like `/* */`.

- The RH6023 and RH6024 fixes (formatter): `a /** c */=1` becomes `a /** c */= 1`, while `a /* c */=1` becomes `a /* c */ = 1`. Likewise `_ = a /** c */+b;` becomes `a /** c */+ b`.
- The twelve analyzers report `/** */` and `/* */` in the same way.

thoenissen/Reihitsu#1074: no instance. The rules report inside interpolation holes; this was checked for RH6016, RH6017, RH6018, RH6022, RH6023, and RH6024.

## Decisions needed

_None._
