Role: skeptic
Commit: 5a16498e0d4c327fcdf10b2338b2e36653e49ebc
Verdict: question for the maintainer

Inputs: thoenissen/Reihitsu#1331 (body and both comments), thoenissen/Reihitsu#1304, #1318, #1335 (for scope), `00-lead.md`, `01-spec-author.md`, `git diff origin/main...HEAD -- sdd/`, the shared specs the diff touches and the rule specs and code named below. My prompt carried no intended fix or hoped-for verdict; the principle texts were not reopened.

Fixtures were run with `scripts/apply-fix.sh` from a scratch directory outside the repository (LF and CRLF arms agree in every run below); rules without a fix were checked by reading the analyzer.

## Premise

The run writes decided principles; the premise checked is that the diff states them faithfully, consistently with the other specs, and that each new or amended requirement has a shape the code meets today. P25, P27, P28, P29, P30, P31, the SHARED-FIX-003 wording, the P11 sentence, and the P18/P22 "already present" claims are faithful in substance. The following do not hold as written.

### Must change, no maintainer decision needed

1. **SHARED-DIAG-004 contradicts itself.** The diff deleted "This requirement covers a rule's verdict on what the declaration is." Its first paragraph is now unconditional: every rule that judges a partial type MUST judge it by its symbol ("of an accessibility when any part declares it so") and report it at most once. For `public partial class C { … }` plus `partial class C { … }`, that binds RH7104 to "public, report at most once", while the new second paragraph binds it to report the second part. The issue keeps P15 for verdicts on what the declaration is; the deleted sentence is what scoped it. The first paragraph needs its scope back.
2. **SHARED-DIAG-009** says "A `partial` element MUST be judged with all its parts together, as SHARED-DIAG-004 states", unconditionally. That contradicts the per-part paragraph (P28) of the requirement it cites. It should defer to SHARED-DIAG-004 without restating only half of it.
3. **The RH7104 example under SHARED-DIAG-004 does not conform.** The run with the example as written gave `public partial class C;` for the second part, not `public partial class C⏎{⏎}`. The fix formats the declaration, and the empty body becomes a semicolon body. That is the P14d instance thoenissen/Reihitsu#1335 D1, so the handover's "conforms" is wrong for this shape. With a member in each body (`private int _a;` / `private int _b;`, as RH7104-004's own examples do), the fix output is exactly `public partial class C` with the body unchanged, which conforms.
4. **SHARED-DIAG-004, second paragraph:** "it MUST report every part whose own modifiers it reports" is circular. It should say which parts are reported, for example each part whose own modifiers break the rule.
5. **SHARED-DOC-004 amendment:** "lies in the declaration's leading trivia" holds only when documentation comments are parsed. With `DocumentationMode.None`, Roslyn's lexer keeps a `///` behind code in the trailing trivia of the token in front of it (source reading, not run). Under P30, RH8401, which uses SHARED-DOC-004, runs in mode `None`. SHARED-COMMENT-001 already says a `///` documents a declaration by its text wherever it would if parsed. The sentence should rest on position rather than trivia, or point at SHARED-COMMENT-001 for mode `None`. Low severity.
6. **RH7103 non-goal** "The layout of the fixed type. The fix lays the type out with the formatter (FMT-PIPE)" (line 21) now contradicts SHARED-FIX-007's moved-code bullet. The diff changed the same statement in `ordering-move-safety.md` and left this one, so the two should read alike. The line-23 non-goal ("How much of the type the fix reformats … open question") is decided by P26 for the moved member only. The rest of the type stays with thoenissen/Reihitsu#969 and thoenissen/Reihitsu#1335.
7. **SHARED-DIAG-009 uses "group" in two senses.** First it means the scope the element lies in ("the region, the accessibility, or the member kind within which the rule judges it"), then the interface whose region the element belongs in ("belongs to several groups at once"). SHARED-USING-007 uses "group" in the first sense only. Minor.

### Conformance of the new and amended requirements

| Requirement | Shape | How | Observed | Conforms |
|---|---|---|---|---|
| SHARED-DIAG-009 | RH7409, `class D : IB, IA`, `M` in `#region IB` | read `InterfaceImplementationUtilities.GetImplementedInterfaceName` (walks `AllInterfaces` in base-list order) | not reported | yes |
| SHARED-DIAG-009 | RH7409, base list in the other part (`C.M` in `#region I`, `D.M` in no region) | same, symbol-based | `C.M` not reported, `D.M` reported | yes (read, not run) |
| SHARED-DIAG-009 | `class C : IA, IB`, `M` in `#region IB` | existing characterization test `VerifyMethodImplementingTwoInterfacesIsJudgedByTheFirstInBaseListOrder` | reported | no — thoenissen/Reihitsu#1334 |
| SHARED-DIAG-010 | RH8307 example | apply-fix RH8307 | `///` element reported and fixed; `/** */` element reported, fix offered and changes nothing (`no-progress`) | `///`: yes; `/** */`: no — thoenissen/Reihitsu#1347 |
| SHARED-DIAG-010 | RH7103 order example | apply-fix RH7103 | `Create` reported and fixed | yes |
| SHARED-DIAG-004 | RH7104 two-part example | apply-fix RH7104 | see item 3 | only with member bodies |
| SHARED-FIX-007 moved code | RH7309 example | apply-fix RH7309 | output identical to the spec's after-state | yes |
| SHARED-FIX-007 moved code | RH7004 example | apply-fix RH7004 | `s.Flush();`, blank lines inserted between the moved statements | no — thoenissen/Reihitsu#1335 |
| SHARED-ORDER-004 | first example (two top-level regions) | apply-fix RH7103; read RH7110 (`RegionDirectiveUtilities.GetTopLevelRegions`) | RH7103: no diagnostic | yes |
| SHARED-ORDER-004 | RH7102 example | read: RH7102 and RH7109 analyzers use no region utility | reported | no — thoenissen/Reihitsu#1339 |
| SHARED-DOC-004 | RH8303 fix example | apply-fix RH8303 | reported; fix inserts a blank line inside the comment | no — thoenissen/Reihitsu#1345 |
| SHARED-DOC-004 | RH8301, `int A; ///<summary>` … `int B;` | apply-fix RH8301 | no diagnostic | no — thoenissen/Reihitsu#1345 |
| SHARED-DOC-004 | RH8304, `int A; /// <summary>⏎/// Value⏎/// </summary> <remarks>x</remarks>⏎int B;` | apply-fix RH8304 | reported; fix splits the elements, the comment stays behind `int A;` | yes (both MUSTs) |
| SHARED-DOC-004 | RH8308, `int A; /// <summary>⏎/// Value⏎/// </summary> trailing⏎int B;` | apply-fix RH8308 | reported; fix removes `trailing`, the comment stays behind `int A;` | yes (both MUSTs) |
| SHARED-DOC-004 | RH8305 `int A; /// <summary>Value</summary>` and RH8307 `int A; /// <summary>Total⏎/// sum</summary>`, each before `int B;` | apply-fix | reported, but both fixes move the comment onto its own line below a new blank line (`int A;⏎⏎/// <summary>…`) | no — not covered by thoenissen/Reihitsu#1345 (new P29 instances, below) |
| SHARED-DOC-004 | RH8026 example | not run (no fix); in modes `Parse`/`Diagnose`, Roslyn places the `///` in the leading trivia of `B` | `A` reported | expected yes; test implementer confirms |
| SHARED-DOC-008 | RH8501 example in mode `None` | read: RH8501 registers `OnAttribute` without a mode check | reported | yes |
| SHARED-DOC-008 | RH8401 example in mode `None` | untagged `VerifyNoDiagnosticsWhenDocumentationModeIsNone` pins silence | not reported | no — thoenissen/Reihitsu#1346 |

Every new or amended requirement keeps at least one conforming shape. For SHARED-DIAG-004 that holds only after item 3.

### Existing tests that now pin the opposite of a requirement

All of these are untagged and were written before this run. They belong to the follow-ups; the lead decides whether their "open question" summaries get a comment-only update now (FC-031):

- RH7405 `VerifyOverrideOfOtherMemberKindDoesNotMakeMethodOverrideRegion` and `VerifyOverrideInOtherPartialDeclarationDoesNotMakeOverrideRegion`, plus RH7409 `VerifyMethodImplementingTwoInterfacesIsJudgedByTheFirstInBaseListOrder`. They pin the opposite of SHARED-DIAG-009 bullets 3, 2 and 1 respectively, and their summaries still call the case an open question (thoenissen/Reihitsu#1334).
- RH7109 `MixedAccessibilityInterfaceViolationWithDirectivesHasNoCodeFix` expects a diagnostic on a field in `#region Readonly` against a field outside every region. That is the opposite of SHARED-ORDER-004 (and of RH7109-001 on `main`) — thoenissen/Reihitsu#1339.
- RH8401 `VerifyNoDiagnosticsWhenDocumentationModeIsNone` pins the opposite of SHARED-DOC-008 — thoenissen/Reihitsu#1346.

## Mechanism and candidates

_N/A._ No bug is claimed. The candidate rule sets per requirement are in `01-spec-author.md`; the additions from this check are RH8304 and RH8308 as conforming shapes for SHARED-DOC-004, and RH8305 and RH8307 as non-conforming fixes.

## Breaking inputs

- **FC-024 (partial):** the RH7104 two-part shape with member bodies (verified). A nested variant, in which another part writes `protected` and the reported part omits it, should give `protected` from the fix; not verified.
- **FC-003 (both sides of the boundary):** for SHARED-ORDER-004, a pair in one top-level region is reported, a pair in two top-level regions is not, a member in a region nested in a top-level region is compared with the rest of that region, and a member outside every region is compared only with members outside every region. Use RH7103 and RH7110 only.
- **FC-017 (LF only):** the RH7309 moved-code example under CRLF as well. Its two blank lines between `_a` and `_b` are the layout the bullet protects.
- **FC-033 (harness normalizes the scenario):** the RH8501 mode-`None` test must really apply mode `None` (`ApplyDocumentationModeNoneToTestProject`). Pair it with a mode-`Parse` run so the test can tell "runs in every mode" from "never ran".
- **FC-009 / FC-013:** do not pin the RH8307 `/** */` element, which is non-conforming. Pin the `///` element as reported and fixed in one pass (verified).
- **SHARED-DOC-004 behind-code shapes:** RH8304 and RH8308 as in the table, both reported, and the fix output keeps `int A; /// <summary>` on one line (verified). RH8026 per the spec example.
- **SHARED-DIAG-009:** RH7409 `class D : IB, IA` with `#region IB`, and the base list in the other part. Do not include `class C : IA, IB`.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Shared specs `diagnostics.md`, `element-documentation.md` | must change (items 1–5, 7) |
| Shared spec `code-fixes.md` (SHARED-FIX-007) | must change depending on Q1 |
| Rule spec RH7103 (non-goal) | must change (item 6) |
| Analyzers (RH7102–RH7110, RH74xx, RH83xx, RH8401, RH8402, RH8501) | must stay |
| Code fixes, Fix All | must stay |
| Formatter, `Reihitsu.Core` | not involved |
| Rule docs under `documentation/rules/` | not involved; the behavior follow-ups own them |
| Tests | must add covering tests for SHARED-DIAG-009 and SHARED-DIAG-010, plus the new parts of SHARED-DIAG-004, SHARED-FIX-007, SHARED-ORDER-004, SHARED-DOC-004 and SHARED-DOC-008, using conforming shapes only |

## Simpler option

_None._

## Questions for the maintainer

**Q1 — How far does P26's moved-code bullet reach?** SHARED-FIX-007 now says that code a fix moves "to another position or into another block without changing it", including "the statements a fix wraps into a new block", keeps its layout, and that its separators follow SHARED-ORDER-002. Three enforced requirement families say the opposite for code their fixes move or re-nest, and today's fixes follow them:

- (a) SHARED-BRACE-002 (RH5405, RH5406, RH5407): `if (a)⏎    M(a,b);`. Today and per the spec: `{ M(a, b); }` (run with RH5405). Under the bullet: `M(a,b);`.
- (b) Expression-body conversion. SHARED-FIX-006's RH3203 example calls the expression body "the replaced node". For `private int M() => _a+M( );`, today's output is `return _a + M();` (run with RH3202). Under the bullet, where the expression moves into a new block: `return _a+M( );`.
- (c) Using-block organizers. RH7201-004 to RH7207 say "rewrite the whole block as the formatter's using phase does" (FMT-USING-002). For `using Microsoft.CodeAnalysis;⏎using System.Linq;⏎using System;`, today and per the spec, a blank line comes before `using Microsoft.CodeAnalysis;` (run with RH7201). Under the bullet, the separators stay where they were (SHARED-ORDER-002), so no blank line is inserted between the directives.

thoenissen/Reihitsu#1335 names only RH7004, RH7101, and RH7104–RH7108.

- Reading A: P26 covers all of (a)–(c). They are P26 instances that need follow-ups, and until those land, this PR leaves SHARED-FIX-007 contradicting SHARED-BRACE-002 and RH7201-004 to RH7207.
- Reading B: P26 covers code moved unchanged within its own list or block: members, accessors, region blocks, and RH7004's statements. It does not cover code that the diagnostic's correction wraps in new syntax, or a block the fix reorganizes. SHARED-FIX-007 gets a scope sentence.

The spec author's P26 instance 1 (SHARED-BRACE-002) is case (a) of this question.

**Q2 — Is P30's every-mode side meant to include RH8301, RH8302 and RH8303?** The amended SHARED-DOC-008 gates "a rule that reads the elements of documentation comments" and requires every mode for "a documentation rule that reads no element of a documentation comment". Its Why names "a `///` comment without XML" as text that every mode keeps. RH8301–RH8303 judge the lines and the position of a documentation comment and read no XML element. Each has a test tagged SHARED-DOC-008, `VerifyNoDiagnosticsWhenDocumentationModeIsNone`, whose input holds no XML element (`///Summary.`; `/// Summary.` followed by a blank line; `/// Summary.` directly after a method) and which expects nothing to be reported in mode `None`.

- Reading A: like element rules, they are gated. This is today's behavior and the handover's classification, and the requirement text has to say so. `/// Summary.⏎⏎void Method()` in mode `None` reports nothing.
- Reading B: they check in every mode, as the requirement's head clause reads. RH8302 reports that blank line, the three tagged tests pin the opposite of the requirement they cover, and a follow-up besides thoenissen/Reihitsu#1346 is needed.

P30 names only RH8401 and RH8402.

## Principle instances found in passing (for the lead to file)

- **P29:** the RH8305 and RH8307 fixes move a `///` comment behind code onto its own line, after a new blank line. Inputs and output are in the conformance table. SHARED-DOC-004 now says only the RH8303 fix may move it. thoenissen/Reihitsu#1345 covers RH8301–RH8303 only. Belongs to the SDD open-questions issue of `sdd/specs/shared/element-documentation.md`, marked "Decided by principle P29 (#912)".
- **P26:** depends on Q1.
