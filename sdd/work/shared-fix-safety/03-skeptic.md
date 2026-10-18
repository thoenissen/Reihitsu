Role: skeptic
Commit: 1bf3eeb
Verdict: question for the maintainer

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The issue writes down policy the maintainer already decided (P1 and P7 on thoenissen/Reihitsu#912), so the premise is real. The spec diff holds exactly the five items of thoenissen/Reihitsu#1068 and follows both decisions in `squad/decisions/`. The `AGENTS.md` sentence is the issue's text word for word, and the three IDs sit in sorted position in `requirement-ids.txt`.

Every example was run with `scripts/apply-fix.sh <RH####> <fixture-dir>`, LF and CRLF, on fixtures outside the repository:

| Example | Observed today |
|---|---|
| RH5022 on `{⏎⏎⏎M();` | fixed in 2 iterations (open, thoenissen/Reihitsu#1076) |
| RH5023 on the same input | one application leaves one blank line below `{`, as the spec says |
| RH5023 on `"class C\n{\n}\n\n"` | `no-progress` (open, #1076) |
| RH5024 on the `#region R` input, then RH5032 on the result | RH5024 deletes the line and RH5032 inserts it again (open, #1076; #1065 Q1 decided that RH5024 is at fault) |
| RH4106 with `Value` used in `#if NEVER` | renames the active uses and leaves `return Value + 1;` in the excluded branch (open, thoenissen/Reihitsu#1073) |
| RH2005 on `public string first;`, then RH4106 | `private string first;`, then `private string _first;`. Nothing is restored. |
| RH4106 on `_1st` | `no-fix-offered` |

The decision wording note in `02-spec-author.md` holds. The question in `2026-10-18-shared-fix-003-own-reported-text.md` names RH5604 as a fix that the scoped answer keeps. RH5604-005 rewrites line breaks that other RH5604 diagnostics report. So "a diagnostic of the fix's own rule" is the reading that decision chose, and it needs no new question.

The premise stops holding where the spec turns shipped, specified, or tested behavior into a violation that neither the issue nor the two decisions considered. Each case below gives different output under the two readings, so they are questions (see Questions):

1. **Renames update documentation comments.** RH4105 rewrites `<param name="Value">` and `<paramref name="Value"/>`, and RH4106 rewrites `<see cref="Value"/>`. This is doc-comment text that no diagnostic of the rule reports. `RH4105MethodParameterCasingAnalyzerTests.VerifyDiagnostics`, tagged RH4105-003, pins three such `<param>` updates. SHARED-CASING-007 requires "every reference" to change.
2. **Fixes that delegate to the formatter change text in the node they format.**
   - RH3202-004 requires formatting the whole method. That reflows its doc comment and re-aligns a raw literal.
   - RH7103 formats the whole containing type. That capitalizes an unrelated `#region` and adds `// Helpers` to its `#endregion`.
   - 39 files under `src/Reihitsu.Analyzer.CodeFixes` call `FormatNodeInDocumentAsync` or `FormatNode`.
3. **Declaration order counts as a run-time change in some cases.** RH7109 reorders the instance fields of a `[StructLayout(LayoutKind.Sequential)]` struct, which changes its field offsets. The spec's own RH7103 example treats a reorder as safe.

Conflict with an in-flight shared spec (FC-015). thoenissen/Reihitsu#1069 adds SHARED-DIAG-003 on branch `claude/quirky-euler-dsqp4e`. It says a rule's "code fix MUST NOT change" literal or comment text, and exempts "a rule whose subject is the text of a comment or of a directive". SHARED-FIX-003 exempts "text that a diagnostic of the fix's own rule reports" instead. The #1069 decision (`2026-10-18-shared-diag-003-comment-subject-rules.md` on that branch) says the two are meant as the same reading. They still decide differently on RH5604-005. RH5604 checks line endings, which is code, yet it reports and rewrites line breaks inside `///` comments. SHARED-FIX-003 allows that rewrite, and SHARED-DIAG-003 forbids it. The two specs give one decision two owners.

Other findings, not blocking:

- **RH8107 example.** The SHARED-FIX-003 example says the fix changes "only the element its diagnostic reports". Its own output also removes the space between `</summary>` and `<returns>`. In the multi-line case, RH8107-006 also removes the `///` lines the element occupies. Both lie outside the RH8107-005 span. How Q2 is answered decides whether the reported text includes them.
- **"At that location" in SHARED-FIX-001.** It can be read as the diagnostic's span or as the position, for example below the brace. Only the position reading makes RH5022's second report a violation. The two-rule example settles this today; the normative sentence alone does not.
- **SHARED-WS-001 joins the contradiction.** The known contradiction between the two-rule example and RH5022-003/-004 also involves SHARED-WS-001, which RH5022 uses: "no whitespace outside the span is changed". `02-spec-author.md` names only RH5022-003/-004.
- **What `AGENTS.md` claims SHARED-FIX-003 covers.** The new sentence says SHARED-FIX-003 "states what makes a fix unsafe". SHARED-FIX-003 covers changed text, but not moving or reordering a comment, a directive, or disabled text (FC-004). The sentence before it in `AGENTS.md`, and `documentation/rules/general-notes.md`, both call that unsafe. A relocating fix is therefore not clearly a bug against any spec.

## Mechanism and candidates

_N/A._ No bug fix. The candidate fix paths, derived from the code, are listed under Breaking inputs.

## Breaking inputs

Candidate fix paths for SHARED-FIX-003:

- **Comment, directive, or literal text.** Three fix paths change it:
  - Renamer-based fixes. `CasingCodeFixProviderBase{T}` and the RH2001 provider call `Renamer.RenameSymbolAsync(…, default, …)`, which retargets doc-comment references.
  - Fixes that delegate to the formatter. `RegionFormattingPhase`, `DocumentationCommentFormattingPhase` and `RawStringAlignmentPhase` change text inside the formatted node.
  - Trivia-editing blank-line fixes (thoenissen/Reihitsu#1075).
- **Run-time behavior.** Two kinds of fixes change it:
  - `TypeMemberOrderingCodeFixProviderBase` (RH7102, RH7103, RH7109) changes declaration order.
  - RH2001 turns a property into a field.

Failure classes that reach the spec: FC-004, FC-006, FC-007, FC-009, FC-015, FC-017, FC-018, FC-020, FC-022, FC-024, FC-030, FC-033, FC-036.

Covering-test candidates that pass today (verified above or by the existing tests named in `02-spec-author.md`):

- **SHARED-FIX-001:**
  - RH5028, a three-line run with one diagnostic;
  - RH5023, two diagnostics in one run;
  - the RH5023 half of the two-rule example.
- **SHARED-FIX-003:**
  - the RH4106 collision (SHARED-CASING-009 test);
  - RH4106 `nameof(Value)` to `nameof(_value)`;
  - RH7103 `_b` withheld (SHARED-ORDER-007 tests);
  - RH5604 keeping a literal's line break (RH5604-003, RH5604-005).

  The RH8107 example also passes, but whether it is a valid example depends on the answer to Q2.
- **SHARED-FIX-004:** the RH2005-then-RH4106 chain. Assert the exact text after both fixes, not just "fixed" (FC-017).
- **SHARED-FIX-005:** RH4106 `_1st`. Assert that no action is offered (FC-009).

Examples that fail today and must not be written as passing tests:

- RH5022 on a run;
- RH4106 with `#if NEVER`;
- the RH5024/RH5032 cycle;
- RH5023 at the end of the file.

For #1073, not this run: condition 4 says "a name". Disabled text that names an unrelated `d.Value`, or a comment inside disabled text, is where the guard's scope is decided.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/code-fixes.md` | Must change once Q1 to Q4 are answered: SHARED-FIX-003 conditions 2 and 3, and the RH8107 example's sentence |
| `AGENTS.md` | Sentence must stay as the issue gives it; the FC-004 gap above is for the lead |
| `sdd/specs/requirement-ids.txt` | Must stay (correct) |
| SHARED-DIAG-003 (#1069, other branch) | Must be aligned with SHARED-FIX-003 so one requirement owns the fix side (Q4) |
| RH3202-004, RH4105-003 and SHARED-CASING-007, RH5604-005, RH8107-006 | Must stay in this run; each conflicts under one reading of Q1, Q2 or Q4 |
| RH2005 non-goal on outside references, RH7103 non-goal on reformat extent | Must stay in this run. SHARED-FIX-003 now decides the first and, depending on Q2, part of the second. Follow-up candidates. |
| RH5022-003/-004, RH5024 to RH5027, SHARED-WS-001 | Must stay; known contradiction, resolved by #1076 |
| Analyzers, code fixes, formatter, `Reihitsu.Core` | Must stay (no production change) |
| Tests | Must change: covering tests for SHARED-FIX-003, -004, -005 |
| Rule docs, `general-notes.md` | Not involved |

## Simpler option

The questions touch only SHARED-FIX-003, condition 2 (Q3) and condition 3 (Q1, Q2, Q4), and the `AGENTS.md` sentence that names it. SHARED-FIX-001, -004, -005 and the non-goal change do not depend on them. They can proceed while SHARED-FIX-003 waits for the answers. The lead decides.

## Questions for the maintainer

### Q1 — Does a rename's update of a documentation-comment reference change comment text?

**Input** (RH4105):

```csharp
public class C
{
    /// <summary>Returns <paramref name="Value"/>.</summary>
    /// <param name="Value">The value</param>
    /// <returns>The value</returns>
    public int M(int Value)
    {
        return Value;
    }
}
```

**Real output:** the parameter, its uses, `<paramref name="value"/>` and `<param name="value">` are all renamed. RH4106 likewise turns `<see cref="Value"/>` into `<see cref="_value"/>`.

**Readings:**
- **A.** A reference to the renamed symbol in a documentation comment (`cref`, `<param>`, `<paramref>`, `<typeparam>`, `<typeparamref>`) is part of the new name the fix exists to produce. Output as today.
- **B.** It is comment text that no diagnostic of the rule reports. Output: no fix is offered whenever a doc comment names the symbol, so every documented parameter loses its fix. The tagged test `VerifyDiagnostics` becomes wrong.
- **C.** The code is renamed and the comment is left alone. Output: `int value` beside `<param name="Value">`, which raises CS1572/CS1573 where documentation is generated. This contradicts SHARED-CASING-007 if a `cref` counts as a reference.

### Q2 — Does canonical formatter output inside the node a fix formats change comment, directive, or literal text?

**Inputs:**
- RH3202 on `///<summary>runs</summary>⏎///<returns>one</returns>⏎public int M() => 1;`
- RH3202 on `public string M() =>⏎"""⏎a⏎""";`, with the literal indented eight spaces
- RH7103 on a type with `private int _instance; private static int _count;` and, further down, `#region helpers` containing a method with `///<summary>runs</summary>`

**Real output:**
- First input: the doc comment becomes four `/// ` lines.
- Second input: the literal's content and closing lines are re-indented to line up with the opening quotes after `return`, with the value unchanged.
- Third input: `#region Helpers`, the doc comment reflowed, and `#endregion // Helpers`. None of these are in the moved or crossed members.

**Readings:**
- **A.** It is layout, which the non-goal leaves to FMT-PIPE-010. Output as today for all three.
- **B.** Every changed character counts. Output: no fix for all three, and RH3202-004 contradicts SHARED-FIX-003. RH8107-006's removal of the `///` lines and whitespace around the reported element counts too, unless the reported text includes them.
- **C.** Content-preserving changes do not count, and changed or added words do. Content-preserving means doc-comment whitespace, line breaks and `///` exterior, and the indentation a raw literal strips. Output: the first two as today. The third keeps `helpers` and `#endregion`, or gets no fix. This reading must keep a line break inside `/* */` as content, because #1075 decides that the RH5011 instance is a violation.

### Q3 — Does a declaration order that an ordering fix produces count as a run-time change?

**Input** (RH7109):

```csharp
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct S
{
    public int _a;
    public readonly long _b;
}
```

**Real output:** `_b` is moved in front of `_a`, so the field offsets change.

**Readings:**
- **A.** The new order the fix exists to produce does not count, as the new name does not. Only orders that the program's execution depends on count, such as initializer order (SHARED-ORDER-007). Output as today.
- **B.** The instance-field order of a type with sequential or explicit layout also counts. Output: no fix for `S`. The spec's RH7103 `_a` example stays.
- **C.** Every observable difference counts, including the field order reflection returns. Output: no fix for `S`, and none for the spec's own `_a` example. That contradicts the example and the moves SHARED-ORDER-006 keeps available. It would also withhold RH2001's property-to-field rewrite always.

### Q4 — Which requirement owns "a fix does not change literal or comment text"?

**Input:** the RH5604-005 example, whose fix rewrites line breaks inside `/// <summary>` that other RH5604 diagnostics report.

**Readings:**
- **A.** SHARED-FIX-003 owns the fix side, and SHARED-DIAG-003 refers to it. Output: RH5604-005 as today.
- **B.** The subject exemption of SHARED-DIAG-003 governs. Output: RH5604-005 becomes a bug, because RH5604 checks code.

The #1069 run may settle this by changing its wording if both runs agree on A.
