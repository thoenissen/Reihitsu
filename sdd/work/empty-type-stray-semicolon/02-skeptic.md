Role: skeptic
Commit: e432b0a
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict. "Narrower" means: the issue's directive goals, its RH5416 test change, and its "rule pages are updated" item are dropped. The `};` part goes ahead as the spec diff states it.

## Premise

The issue is real for its first instance, and the case is reachable.

- **Callers.** The analyzer reaches it in every build. The code fix reaches it on the reported declaration, through `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`, which formats the declaration through `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`. Whole-document formatting (CLI) reaches it in `EmptyTypeDeclarationSemicolonTransform`. All three read the same Core predicate.
- **Cause, from the code.** `EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport` (`src/Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities.cs:53`) returns `false` whenever `typeDeclaration.SemicolonToken` is present. Roslyn makes the `;` behind `}` that token, so a declaration with a brace body counts as a semicolon declaration. `CanConvertSafely` and the formatter transform go through `ShouldReport`, so all three surfaces skip the shape.
- **Executed.** I ran `scripts/apply-fix.sh RH5412 <dir>` on the fixtures below, each starting `namespace N;` plus a blank line unless noted. Every one gives `no-diagnostic` under LF and CRLF:
  - `class A\n{\n};`
  - `class B { };`
  - nested `class H\n{\n    class I\n    {\n    };\n}`
  - `class E<T> where T : class\n{\n};`
  - `class A\n{\n} ; // note`
  - `class A\n{\n}\n#if X\n#endif\n;`
  - `class A { }; class B { };`
  - in a block namespace: `class A\n    {\n    };`
  - global namespace, no `namespace`: `class A\n{\n};`
  - with a doc comment, an attribute, and `public sealed partial`: `class A\n{\n};`
  - `class A\n{\n}\n\n;`
  - `class A\n{\n} /* x */ ; // y`
  - `class A\n{\n}; /* x */ class B;`
  - `class A\n{\n}\n// c\n;`
  - `class B\n{\n} /** d */;`
  - `class C\n{\n}\n#if X\n    int y;\n#endif\n;`
- **Other kinds.** RH5413 (`struct S\n{\n};`), RH5414 (`interface I\n{\n};`), RH5415 (`record R(int Value)\n{\n};` and `record class Q { };`), and RH5416 (`record struct S(int Value)\n{\n};`) all give `no-diagnostic`. The spec author checked RH5418.
- **Two semicolons do not parse.** `class A\n{\n};;`, `class A;;`, and the nested forms of both (`class I { };;` and `class I;;` inside a class) give `parse-error` under apply-fix. This bears out FMT-STRUCT-017's *Why* and the change doc's rejected alternative ("a second one does not compile"), at namespace level and inside a type alike.
- **Today's formatter output.** `scripts/format.sh <dir>` on the same fixtures:
  - The empty `};` forms are kept and only broken onto lines: `class B\n{\n};`.
  - A `;` on its own line is indented: `}\n    ;`, and `}\n\n    ;` with a blank line.
  - `} /* x */; // y` and `} /** d */;` are unchanged.
  - `}\n// c\n;` becomes `}\n\n    // c\n    ;`: a blank line is inserted, and the comment and the `;` are indented.
  - A second run gave no change.
- **Maintainer decision.** thoenissen/Reihitsu#1169 question 2 (decided by P11 in round 7a of thoenissen/Reihitsu#912) states the expected output for `class C\n{\n};` and the RH5412–RH5416 kinds: reported, and fixed to `class C;`. The change doc's *Why* cites this as "#912, question 2 of the empty-type spec questions". The question is #1169 Q2 under #912. This is internal change-doc text.
- **Issue goals 2 and 3 are not real defects.**
  - Goal 2 (a conditional directive in the body is not reported) is contradicted by the accepted change `sdd/changes/2026-11/report-across-conditional-directives.md`. It is also contradicted by the issue's own correction comment of 2026-10-22.
  - Goal 3 (`#region` / `#pragma` in the body: reported, no fix) already holds. It is pinned by RH5412's `#region` test (`RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.cs` around line 590) and by RH5418's region-or-pragma test (`RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests.cs:341`). Both run the same shared Core predicate.
  - The blockers thoenissen/Reihitsu#1260 and thoenissen/Reihitsu#1240 are closed.

## Mechanism and candidates

Mechanism: a type declaration counts as already being a semicolon declaration whenever its `SemicolonToken` is present, even when it also has a brace body. Roslyn attaches a `;` written behind `}` to the declaration as that token.

Dispatch:
- The analyzer: `EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase.Initialize` → `RegisterSyntaxNodeAction(OnTypeDeclaration, _declarationKind)`, one kind per rule.
- The formatter: `EmptyTypeDeclarationSemicolonTransform.Visit{Class,Struct,Interface,Record,Union}Declaration`.

Candidates, with their defining types:

| Candidate | Defining type | Rule | Version gate |
|---|---|---|---|
| Class | `ClassDeclarationSyntax` | RH5412 | C# 12 |
| Struct | `StructDeclarationSyntax` | RH5413 | C# 12 |
| Interface | `InterfaceDeclarationSyntax` | RH5414 | C# 12 |
| Record class | `RecordDeclarationSyntax`, `RecordDeclaration` | RH5415 | C# 9 |
| Record struct | `RecordDeclarationSyntax`, `RecordStructDeclaration` | RH5416 | C# 10 |
| Union | `UnionDeclarationSyntax` | RH5418 | C# 15 |

Every candidate goes through the same predicate, and every one fails. An enum (`EnumDeclarationSyntax`, a `BaseTypeDeclarationSyntax` that is not a `TypeDeclarationSyntax`) is not dispatched and is not involved. A delegate has no body.

## Breaking inputs

Failure classes that reach the changed decision:

- **FC-002 and FC-019 — where trivia "between `}` and `;`" sits in the syntax tree.**
  - A comment on the `}` line is trailing trivia of `}`: `} /* x */;`, and `} // x\n;` (EOL included).
  - A comment or directive on a later line is leading trivia of `;`: `}\n// c\n;`, `}\n#if X\n#endif\n;`, and disabled text.
  - A comment behind the `;` is trailing trivia of `;`: `}; // note`, `}; /* x */ class B;`.
  - The span FMT-STRUCT-018 names covers the first two locations. The third is what SHARED-EMPTY-005 carries to the new `;`.
- **FC-004 — the carried comment must survive.** When the braces and the `;` go, a comment that was trailing trivia of the old `;` must not be dropped. Today's conversion carries the close brace's trailing trivia (`EmptyTypeDeclarationSemicolonTransform.CreateSemicolonToken`).
- **FC-003 — test both sides of the boundary.** Cover nothing between `}` and `;`, whitespace only, one line break, a blank line, and each kind of trivia.
- **FC-005 and FC-020 — directives are not comments.** A directive between `}` and `;`, and disabled text there, are separate members of the class, not covered by a comment test.
- **FC-040 — documentation mode.** `} /** d */;` (delimited doc comment) and `} /// d\n;` can parse as structured trivia, depending on the documentation mode.
- **FC-006 — version gates.** `class C { };` under C# 11 stays unreported and unconverted. The record kinds stay converted from C# 9 and C# 10.
- **FC-026 — several declarations on one line, and Fix All.** `class A { }; class B { };` has several identical tokens on one line.
- **FC-027 — nesting.** A nested type `class I { };` inside a class; a block namespace; the global namespace.
- **FC-007 — convergence.** The fix converges in one pass.
- **FC-010 and FC-033 — line endings.** Formatter idempotency, CRLF included.
- **FC-013 — analyzer and formatter.** The comment-between shape is reported, and the formatter keeps it. RH5412–RH5418 are syntax-form rules, not layout rules, so SHARED-DIAG-010 does not cover them, as with the existing SHARED-EMPTY-004 shapes. This is no conflict.

### Class matrix

The varying axes:
- (a) the kind: 6 values, sharing one predicate;
- (b) what lies between `}` and `;`: nothing, space, line break, blank line, `/* */` on the `}` line, `//` on the `}` line, a comment on its own line, a directive, disabled text, a doc comment;
- (c) what lies behind `;`: nothing, a line comment, a block comment followed by another declaration, another declaration;
- (d) the position: top-level in a file-scoped namespace, a block namespace, nested in a type, the global namespace;
- (e) the version: at or below the gate.

Every member is reachable through the analyzer and the CLI. Every member of (a) × "nothing / space / line break / blank line" fails today (not reported, not converted). Every comment, directive, and disabled-text member of (b) is also not reported today. After the change it is to be reported without a fix, per SHARED-EMPTY-005's second example.

### Candidate inputs for the test implementer

| # | Input (kind RH5412 unless noted) | Expected per spec | Existing test |
|---|---|---|---|
| 1 | `class F\n{\n};` | reported, fix `class F;` | none found |
| 2 | `class G { };` | reported, fix `class G;` | none found |
| 3 | `class C\n{\n}\n;` and `}\n\n;` | reported, fix `class C;` | none found |
| 4 | `class D\n{\n}; // note` | reported, fix `class D; // note` | none found |
| 5 | `class A\n{\n} /* note */;` | reported, no fix; formatter unchanged | none found |
| 6 | `class B\n{\n} // note\n;` | reported, no fix | none found |
| 7 | `}\n// c\n;`, `}\n#if X\n#endif\n;`, `}\n#if X\n    int y;\n#endif\n;` | reported, no fix | none found |
| 8 | `} /** d */;` | reported, no fix | none found |
| 9 | `class A { }; class B { };` (Fix All) | `class A;\nclass B;` | none found. The shape without `;` is pinned today: `class A { } class B;` gives `class A;\nclass B;`, and `class A { } // note` gives `class A; // note` |
| 10 | nested `class H\n{\n    class I\n    {\n    };\n}` | `I` reported, fix `class I;` | none found |
| 11 | RH5413 `struct S {};`, RH5414, RH5415 `record R(int Value) { };`, RH5416, RH5418 | reported, fixed | RH5418 `UnionWithBracesAndSemicolonIsNotReported` (line 448) pins the **old** behavior and must be inverted. It is tagged SHARED-EMPTY-001 |
| 12 | `class C\n{\n    int _x;\n};` | not reported | none found |
| 13 | `class C { };` under C# 11 | not reported, not converted | version-gate tests exist for the shape without `;`; none with `;` |
| 14 | formatter on #1–#4, #10, run twice (LF and CRLF) | FMT-STRUCT-017 example | none found |
| 15 | RH5404 `class H { class I { }; }` | today's fix output keeps `class I\n    {\n    };`. After: `class I;` | none found. Confirmed today's output with `scripts/apply-fix.sh RH5404` |

Searches I ran:
- multiline `rg` over `tests/` and `src/` for an empty type followed by `;`: the only test hit is the RH5418 test in row 11, and no repository source holds the shape, so the self-hosting scan is unaffected;
- `rg "#pragma|#region"` over the RH5412–RH5418 tests;
- `Requirement("SHARED-EMPTY-` tags: 001 ×24, 002 ×7, 003 ×23, 004 ×14, 005 ×0.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `EmptyTypeDeclarationSemicolonAnalysisUtilities` (`ShouldReport`, `CanConvertSafely`, `HasMeaningfulBodyTrivia`) | Must change. It is the single owner of the decision for all three callers |
| Analyzers RH5412–RH5416, RH5418 (`EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase`) | Behavior must change: new diagnostics. They report on the name as today |
| Code fixes (`EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`) | Behavior must change: offered for the new shapes. Its offer reads `CanConvertSafely`, and its rewrite is the formatter's |
| Fix All | Involved; the `BatchFixer` stays. Cover row 9 |
| Formatter `EmptyTypeDeclarationSemicolonTransform` (structural phase) | Must change |
| Other formatter phases | Not involved. Indenting a `;` kept on its own line is pre-existing; see *Follow-up candidates* |
| RH5404 | Not involved. Its fix output changes only through the formatter (row 15). Its own `SemicolonToken` gap is a follow-up |
| RH5113 | Not involved. It does not register type declarations |
| `OrderingDeclarationUtilities.DeclarationHoldsConditionalDirective` | Not involved. It decides the body by `OpenBraceToken`, which is correct |
| Rule pages RH5412–RH5416, RH5418 | Not involved. "Replace the empty brace body with a trailing semicolon" covers `{ };`, and the withheld fix is covered by `general-notes.md` |
| Node-level formatter entry points | Not involved. There is no prediction of a phase decision (question 10) |
| Tests | The RH5418 test in row 11 must change, and new tests are needed for SHARED-EMPTY-005 (architecture test currently failing on it, as the spec author noted) |

## Refuse option

- **Withhold.** The refusal is to leave `};` unreported and unconverted, which is today's behavior. It costs nothing in code. But it contradicts the maintainer's P11 decision on thoenissen/Reihitsu#1169 Q2, so it is not available for the basic shape.
- **Is the class rare?** People seldom write `};` (a C++ habit). A comment, directive, or disabled text between `}` and `;` is rarer still. Withholding the fix for that sub-class is what the spec already does (SHARED-EMPTY-004 via FMT-STRUCT-018). It is a cheap check: two trivia lists the syntax tree already provides, next to the four lists `HasMeaningfulBodyTrivia` already checks. It is no machinery.
- **Declaring a shape unsupported.** The shape: a comment, directive, or disabled text between a type declaration's closing brace and the semicolon behind it, from the trailing trivia of `}` through the leading trivia of `;`.
  - It would delete no existing special case. The only guard it could spare is the one this run adds. `HasMeaningfulBodyTrivia` stays for comments inside the body.
  - It would refuse whole files instead of withholding one fix.
  - Under `AGENTS.md` ("A shape that deletes nothing is not declared"), it is not an option for escalation 10. I name it only so the lead sees it was weighed.
- **Simpler options.** None. One shared Core predicate decides all three surfaces, so the change already has a single owner.

## Output questions

The change doc's visible output changes are the lead's batch. Two of them are not covered by the #1169 Q2 answer, and I add my recommendation for each.

**1 · RH5418 · Empty unions should use semicolon declarations — a semicolon behind the empty body**

Input:
```csharp
union U(int, string)
{
};
```

Before (`main`): not reported. The formatter writes it unchanged.

After, A (recommended): reported on `U`; the fix and the formatter write
```csharp
union U(int, string);
```

After, B: not reported (today).

Why A: it is the same construct and the same shared check as RH5412–RH5416, which the maintainer decided to report. B would need a kind-specific exception in the shared predicate.

**2 · RH5412 · Empty classes should use semicolon declarations — a comment between the closing brace and the semicolon**

Input:
```csharp
class A
{
} /* note */;
```

Before (`main`): not reported. The formatter leaves it unchanged.

After, A (recommended): reported on `A`, no fix offered. The formatter leaves it unchanged.

After, B: not reported (today).

Why A: a comment inside the body is already reported without a fix (SHARED-EMPTY-004), and a comment next to the body is no different. B would need a reporting exception that no other comment position has.

Every other listed change follows from the #1169 Q2 answer. That covers the one-line form, the `;` on a later line, the comment behind the `;` following the new one, and RH5404's fix output (row 15). It also covers the five kinds RH5412–RH5416, which the answer names.

## Internal choices

- **SHARED-EMPTY-003 vs SHARED-EMPTY-005 wording.** SHARED-EMPTY-003 says "A comment that followed the closing brace on its line MUST follow the semicolon". Read literally, it also covers `}` `// note` on the `}` line with `;` on the next line, and that reading gives `class B; // note`. SHARED-EMPTY-005 withholds the fix for exactly that input (its second example, `B`).
  - The specific requirement and its example make the intent clear, so this is not two readings in the spec author's intent. But the wording overlaps.
  - Recommendation: the spec author scopes SHARED-EMPTY-003's second sentence to a closing brace that no semicolon follows. No visible output changes.
- **The change doc's *Why* citation.** It should read thoenissen/Reihitsu#1169 question 2 (decided by P11 under #912), not "#912, question 2". This is internal text.
- **The spec author's other internal choices:** a separate SHARED-EMPTY-005, FMT-STRUCT-017 and FMT-STRUCT-018 changed instead of new IDs, and the rule-spec intros naming SHARED-EMPTY-005. I see no reason to overturn them. Recommendation: keep.

## Issue goals

thoenissen/Reihitsu#1275 is `agent-filed`.

- **Bullet 1 (`};` reported, fixed, stray `;` removed).** Keep. RH5418 joins it as output question 1.
- **Bullet 2 (directive / excluded body not reported, "already holds").** Drop. It is false today (the correction comment) and reversed by the accepted change `report-across-conditional-directives.md`. Dropping it keeps today's specified output, so nothing visible is in question.
- **Bullet 3 (`#region` / `#pragma` reported, no fix).** It already holds and is pinned (see *Premise*). It needs no work and no new test.
- **"The RH5416 test `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` changes".** Drop. The test pins the specified behavior. Nothing visible changes.
- **"Done when … rule pages are updated".** Drop. The pages already cover the form, so no visible change is needed.
- **The issue's requirement list.** Drop SHARED-TRAIL-002 (trailing commas) and SHARED-DIAG-008. Neither changes for `};`. This is internal.
- **The PR.** The issue's purpose (`};` reported and converted) is met, so `Closes #1275` fits, with the dropped goals stated.

## Architecture findings

_None._

The run's defect has one owner, the Core predicate that the analyzer, the code fix, and the formatter all read. RH5404 copies the same predicate ("has a `SemicolonToken`" taken as "is a semicolon declaration"), but RH5404's behavior is outside this run and the run does not depend on it. See follow-up 1.

## Follow-up candidates

1. **RH5404, a type with members and a `;` behind its braces.** This is the spec author's candidate 1, and I confirm it. The structural cause: the predicate "a type declaration is a semicolon declaration" is written as `SemicolonToken.IsKind(SemicolonToken)` in `RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:52` as well as in the Core helper this run changes. The issue for it should name that second statement of the rule.
2. **Formatter, the line of a `;` kept behind a type's closing brace.** This is the spec author's candidate 2, extended by an observation from `scripts/format.sh` on `main`. With a comment on its own line between `}` and `;`, the formatter also inserts a blank line in front of the comment, and indents both the comment and the `;`. It is stable on a second run. This case keeps its braces after this run (FMT-STRUCT-018), so it stays reachable.

   Input:
   ```csharp
   class A
   {
   }
   // c
   ;
   ```

   Before (`main`):
   ```csharp
   class A
   {
   }

       // c
       ;
   ```

   The same holds for a directive or disabled text there (`#if X` … `#endif` followed by `    ;`). This belongs in the same follow-up as candidate 2; the output to want there is that run's output question.
