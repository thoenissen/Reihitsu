Role: spec author
Commit: 97c6e64
Verdict: spec written

I re-derived every finding of `02-skeptic.md` against the code before changing anything. I used my scratch file-based probe outside the repository: it prints the rule's diagnostics as markup, the action count per diagnostic, one Fix All result, and the single fixes applied one after another. I also ran `scripts/trace.sh` for the formatter part of Q4. The skeptic's suggested wording was treated as a suggestion only.

After the revision:
- All 104 example blocks of the 21 specs reproduce, 0 mismatches. The two SHARED-TYPEREGION-005 blocks were also run against RH7303 explicitly.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed.

Lead constraints respected:
- `RH7103.md`, `formatter/region-formatting.md`, `shared/ordering-move-safety.md`, and `shared/using-directive-ordering.md` are not edited.
- No ID was retired, moved, or renumbered. The two new IDs are SHARED-TYPEREGION-005 and SHARED-TYPEREGION-006, added to the ledger.
- No decided-but-not-implemented behavior was written into a spec.

# Per-finding table

| Finding | Re-derived | What changed, or why not |
|---|---|---|
| A1 | Yes. `System.Linq` / `Microsoft.X` / `System` reports `System`. The RH7204 and RH7206 "neighbor" clauses decide Input A of Q2 and Q3. | RH7203-002, RH7204-002, and RH7206-002 now say only that a directive is never reported *because of* a directive of another group. RH7203-002 also says equal keys are not reported, measured against the directive of its own group directly above. The examples stay: in each, the pair is adjacent and has no partner in its own group. |
| A2 | Yes (Q4). | RH7302-001 and RH7302-002 are qualified to a `#region` written without whitespace between the `#` and the keyword, following the RH6004 precedent. "Every `#region`" became "every such `#region`". |
| A3 | Yes. Normalizing the spacing is #964, decided but not implemented. | The RH7302 non-goal now says only that the fix keeps the spacing. |
| A4 | Yes (Q5). | RH7303-004 now covers only a region whose `#region` and `#endregion` are both reported and each lie between two statements of the same body. That condition also keeps the P10 brace cases (N6) out. |
| A5 (a) | Yes. With `/* c */ int _a;` both directives are reported. | RH7304-001 now requires that the first token after the `#region` starts its line. The comment case is N1. |
| A5 (b) | Yes. The straddling inner region reports `#region Inner` and `#endregion // Outer`. | RH7304-001 now pairs as FMT-REGION-003 does, and covers only pairs whose two directives, and every region directive between them, lie between declarations (SHARED-TYPEREGION-005). The case is N2. |
| A6 | Yes. Each comment ends up above the other region. | RH7309-004 now covers only declarations in which no comment lies between two top-level region blocks. The case is N3. |
| A7 | Yes. P10 is decided; the brace cases are not implemented. | The RH7310 non-goal about blank lines is removed. RH7310-004 now covers only a region between two declarations, where the nearest non-blank lines above and below belong to declarations. RH7303-004 is narrowed as under A4. Neither sentence covers a removal next to a brace any more. |
| A8 | Yes. `{|RH7207:using B = int;|}` / `using System;`. | RH7207-003 now states the location for an alias of a non-name type: the whole directive. Its example is independent of N4, because it is reported for the kind order, regular before alias. |
| A9 | Yes. A no-break space does not split `Private methods`. | RH7308-002 now splits at spaces and tabs, not at whitespace. |
| A10 | Accepted for RH5031, RH5032, RH7304, and RH7310 (SHARED-BLANK-007 and my earlier probe). | `general-notes.md` now names RH7301, RH7302, RH7303, RH7306, and RH7307, the five rules I confirmed by running them. RH7309 and RH7311 also count such regions, but they are left out because of N5. |
| A11 | Yes. `instance` and `generic` are among the ignored words. | The RH7308 page now names `instance` and `generic` among the ignored words, with `Instance methods` as an example. |
| B: top-level regions | Yes. RH7308, RH7309, RH7311, and SHARED-TYPEREGION-003 restated the definition. | New SHARED-TYPEREGION-006 in `shared/type-regions.md`, now titled "Regions of a type". It covers the pairs between the declaration's members, inside its braces and outside nested types, not nested in another pair; members of a nested region count through the outer region; each partial declaration is checked on its own. It is qualified to declarations where no pair crosses a member boundary, which keeps straddling pairs out. RH7308-001, RH7309-001, RH7311-001, RH7311-002, and SHARED-TYPEREGION-003 now reference it. The RH7309 non-goal that restated it is removed. RH7311-002's example keeps only the partial case and points to SHARED-TYPEREGION-006 for the nested region and nested type. |
| B: executable code | Yes. | New SHARED-TYPEREGION-005 defines both "inside executable code" (block, accessor list, lambda or anonymous method, local function, statement) and "between declarations" (members of a type or enum, declarations of a namespace, top level of a file). Both are positive lists, so the Q6 places (expression bodies, initializers, parameter lists, attribute arguments) are in neither. RH7303-001, RH7303-002, RH7304-001, RH7310-001, and RH7310-002 reference it. `RegionDirectiveUtilities` stays with FMT-REGION and is not claimed. |
| B: pairing | Yes. | RH7301-001 references FMT-REGION-003 instead of restating it. |
| C: non-name alias | Yes (N4). | Only RH7207-003's location is stated (A8). RH7204-001 is qualified to aliases of a namespace or type name. |
| C: extension blocks | Yes. A static class with a method in a region and an extension block outside one is reported by RH7305 and RH7305A. | SHARED-TYPEREGION-002 counts extension blocks with the nested types. RH7308 already says nested types are never reported. |
| C: `#if`/`#else` duplicate | Yes. RH7311 reports the second `#region Fields` (N5). | RH7311-001 is qualified to regions outside any `#if` branch. |
| C: `#endregion //Implementation` | Yes. The fix registers but does not change the text. | Left out. RH7306-003 already covers only descriptions with text before the word. Filed under Q7. |
| C: Q6 blast radius | Accepted. My probe confirms RH7310 reports an empty region inside an array initializer. | Handled through the positive lists of SHARED-TYPEREGION-005. Added to Q6. |

# Changed requirements

- Added: SHARED-TYPEREGION-005 (inside executable code and between declarations); SHARED-TYPEREGION-006 (the top-level regions of a type declaration).
- Changed, all unmerged in this branch:
  - SHARED-TYPEREGION-002: counts extension blocks.
  - SHARED-TYPEREGION-003: references SHARED-TYPEREGION-006.
  - RH7203-002, RH7204-001, RH7204-002, RH7206-002, RH7207-003.
  - RH7301-001, RH7302-001, RH7302-002.
  - RH7303-001, RH7303-002, RH7303-004.
  - RH7304-001.
  - RH7308-001, RH7308-002.
  - RH7309-001, RH7309-004.
  - RH7310-001, RH7310-002, RH7310-004.
  - RH7311-001, RH7311-002.
- Docs: `documentation/rules/general-notes.md` (A10) and `documentation/rules/RH7308.md` (A11).
- Later consumers of SHARED-TYPEREGION-006, not edited in this run:
  - RH7103-004, whose spec belongs to another batch.
  - RH7110 and RH7401–RH7412 when their specs are written.

# Examples for the tests

- Every requirement ID of the 21 specs, including SHARED-TYPEREGION-005 and SHARED-TYPEREGION-006, and SHARED-USING-001 … SHARED-USING-009, which are enforced through `Uses`.
- Cases left out of the specs get characterization tests without a `[Requirement]` tag: Q1–Q8 and N1–N6. Run the RH7303, RH7306, and RH7310 fixes under CRLF too (FC-033).

# For the lead (existing issues and specs)

- #1105 (P3): RH7204 and RH7206 are further instances; the skeptic agrees.
- #1151: the trailing-whitespace case belongs in #1151's decision. Input `#region members  ` / `#endregion // members`; the fix gives `#region Members  ` and leaves `#endregion // members`. Comment on #1151 rather than filing a question. RH7302-005 stays narrowed to a `#region` line that ends with its description.
- #1143, #1135: nothing new.
- Follow-up outside this run:
  - FMT-REGION-001 requires a rewritten line to start with `#region`, but the formatter writes `# region fields` as `# region Fields` (`scripts/trace.sh`, RegionFormattingPhase). That belongs to `formatter/region-formatting.md`.
  - The `shared/diagnostics.md` non-goal about inactive code.

# Open questions to file

## `SDD open questions: sdd/specs/analyzer/RH7203.md` (new issue; cross-link with the RH7204 and RH7206 issues, one pattern)

**Q1 — Another non-`System` group between two directives hides an out-of-order pair.** Touches RH7203-001, RH7203-002.

- Input A: `using Microsoft.B;` / `using Alpha.X;` / `using Microsoft.A;` — not reported.
- Input B: `using Microsoft.B;` / `using System;` / `using Microsoft.A;` — `Microsoft.A` reported.
- Input C: `using Microsoft.B;` / `using static System.Math;` / `using Microsoft.A;` — `Microsoft.A` reported.
- Input D: `using System.Linq;` / `using Microsoft.X;` / `using System;` — `System` reported. A `System`-root pair is never hidden, because the `System` group is filtered on its own.
- Why it looks wrong: in every input the two directives of one group are out of order (SHARED-USING-007). Whether that is reported depends on which kind of directive lies between them.
- Readings:
  - (a) Compare with the previous directive of the same group, whatever lies between. A–D are all reported.
  - (b) Compare only directives that are adjacent in the block. Only directly adjacent pairs are reported, so B, C, and D are not.
- Recommendation: (a).
- Severity: low. RH7201 and RH7207 report every input, and their fixes correct it.
- Not decided by a principle; P4 does not apply, because the formatter organizes every input.

## `SDD open questions: sdd/specs/analyzer/RH7204.md` (new issue; cross-link)

**Q2 — Another alias group between two aliases hides an out-of-order pair.** Touches RH7204-001, RH7204-002.

- Input A: `using B = System.Text;` / `using X = Microsoft.Y;` / `using A = System.IO;` — not reported.
- Input B: `using B = System.Text;` / `using static System.Math;` / `using A = System.IO;` — `A` reported.
- Readings, recommendation, and severity as in Q1.

## `SDD open questions: sdd/specs/analyzer/RH7206.md` (new issue; cross-link)

**Q3 — Another static group between two static directives hides an out-of-order pair.** Touches RH7206-001, RH7206-002.

- Input A: `using static System.Math;` / `using static Microsoft.Y.Z;` / `using static System.Console;` — not reported.
- Input B: `using static System.Math;` / `using A = System.Text;` / `using static System.Console;` — `System.Console` reported.
- Readings, recommendation, and severity as in Q1.

## `SDD open questions: sdd/specs/analyzer/RH7302.md` (new issue, or extend #1079; the lead decides)

**Q4 — `# region` is checked at a fixed offset.** Decided by #1043 Q1 (→ #1079: `# region` is a region directive) and by principle P7 (#912), SHARED-FIX-001. Touches RH7302-001, RH7302-002, SHARED-FIX-001.

- Inputs and real output:
  - `# region Fields` is reported, and no fix is offered.
  - `# region fields` is reported, and the fix gives `# region Fields`, which is still reported.
  - `#  region fields` gives `#  region Fields`, which is still reported.
- Formatter: it writes `# region fields` as `# region Fields` (`scripts/trace.sh`), so its own output is reported (FC-013).
- Cause, from reading the code: the analyzer checks `ParentTrivia.ToString().AsSpan().Slice(7)` instead of the description.
- Expected under the decisions: the description is checked. `# region Fields` is not reported, and `# region fields` is reported and fixed in one application.

## `SDD open questions: sdd/specs/analyzer/RH7303.md` (new issue)

**Q5 — Straddling region: the fix removes a `#region` it does not report.** Decided by principle P1 (#912), SHARED-FIX-003. Touches RH7303-004, SHARED-FIX-003.

- Input: `class C` / `{` / `    #region Members` / `` / `    void M()` / `    {` / `        var x = 1;` / `` / `        #endregion` / `    }` / `}`.
- Real output: only the `#endregion` is reported. The fix removes both directive lines, including the unreported type-level `#region Members`.
- Compliance means withholding the fix for such a pair. Also reporting the type-level partner would be a behavior change that needs a change doc; the maintainer may still prefer it.
- The blank line the fix leaves after `{` is N6.

**Q6 — Regions inside expression bodies and initializers are neither inside executable code nor between declarations.** Decided by principle P11 (#912): a gap inside the construct the rule names. Touches RH7303-001 and SHARED-TYPEREGION-005, and through it RH7304, RH7310, SHARED-TYPEREGION-006, RH7305, RH7305A, RH7308, RH7309, RH7311. Outside this batch, it also touches RH7103, RH7110, and RH7401–RH7412.

- Inputs not reported by RH7303:
  - `int M() =>` / `#region Value` / `1;` / `#endregion`
  - `private int[] _a = new[]` / `{` / `#region Values` / `1,` / `#endregion` / `};`
  - `int P { get; } =` / `#region …` / `2;` / `#endregion`
- RH7310 reports an empty region inside a field initializer (`new[] { #region Values` / `#endregion` / `1, }`), and its fix removes it.
- The whole set of places that `RegionDirectiveUtilities.IsWithinElementBody` does not treat as executable code:
  - the expression bodies of methods, properties, indexers, operators, conversion operators, constructors, and finalizers;
  - field and property initializers;
  - constructor-initializer arguments;
  - parameter defaults;
  - attribute arguments.

## `SDD open questions: sdd/specs/analyzer/RH7304.md` (new issue)

**N1 — A comment before the region's first token on its line.** Analyzer part decided by principle P12 (#912). Fix part decided by principle P1 (#912), SHARED-FIX-003. Touches RH7304-001, RH7304-004.

- Input: `    #region Fields` / `    /* c */ int _a;` / `    #endregion // Fields`.
- Real output: both directives are reported, because the indentation is taken as `    /* c */ `. The fix writes `    /* c */ #region Fields`, which does not parse.
- Expected under P12: the comment is the line's first element, so the directives at four spaces match and nothing is reported.

**N2 — Pairing around a straddling inner region.** Touches RH7304-001, RH7304-004, FMT-REGION-003.

- Input: `    #region Outer` / `    int _x;` / `    #region Inner` / `      void M()` / `      {` / `    #endregion` / `      }` / `    #endregion // Outer`.
- Real output: the analyzer pairs only the directives outside executable code, so `#region Inner` is paired with `#endregion // Outer` and both are reported. The fix pairs as the compiler does: it re-indents `#region Inner` and the `#endregion` inside `M`, and the diagnostic stays.
- Open part — readings:
  - (a) Compiler pairing (FMT-REGION-003), skipping pairs that have a directive inside executable code. Nothing is reported.
  - (b) Current pairing. Both directives are reported.
- Already decided:
  - The fix does not clear its diagnostic: principle P7 (#912), SHARED-FIX-001.
  - The fix edits an `#endregion` inside executable code, which its diagnostic is not about: principle P9 (#912).

## `SDD open questions: sdd/specs/analyzer/RH7306.md` (new issue)

**Q7 — A description that is only `Implementation`.** Touches RH7306-001, RH7306-003.

- Input: `#region Implementation` / `int _a;` / `#endregion // Implementation`.
- Real output: both directives are reported. The fix gives `#region` / `#endregion //`: an unnamed region with a dangling `//`.
- Not decided by a principle. SHARED-FIX-003 permits the change, because both directives are reported. The formatter leaves the result alone (FMT-REGION-004), and RH7301 sees two empty descriptions.
- Readings:
  - (a) Withhold the fix when nothing would remain of the description. Recommended: choosing a name is the user's decision, as in RH7301-005.
  - (b) Remove the `//` as well, giving `#region` / `#endregion`.
  - (c) Keep the current behavior.
- Same issue, a second part decided by principle P7 (#912), SHARED-FIX-005: `#region IDisposable` / `#endregion //Implementation`. The `#endregion` is reported, and the offered fix changes nothing.

## `SDD open questions: sdd/specs/analyzer/RH7308.md` (new issue)

**Q8 — A member in a nested region is judged by the outer region's label.** Touches RH7308-001, SHARED-TYPEREGION-006.

- Input 1: `#region Fields` / `int _a;` / `#region Properties` / `public int P => 1;` / `#endregion` / `#endregion`.
  - Real output: `P` is reported against `Fields`.
- Input 2: `#region IDisposable` around `#region Properties`, which holds a method.
  - Real output: not reported.
- The rule page says "Only members placed directly inside the region are considered".
- Readings:
  - (a) Judge each member by the innermost region around it. Input 1 is not reported; the method in input 2 is.
  - (b) Keep the current behavior and correct the page.
  - (c) Do not check members of a nested region at all. Neither input is reported. This is what the page's two sentences together say.

## `SDD open questions: sdd/specs/analyzer/RH7309.md` (new issue)

**N3 — A comment above a moved region stays in place.** Touches RH7309-004.

- Input: `// The disposal logic` above `#region IDisposable`, and `// The state of the object` above `#region Fields`.
- Real output: after the fix, each comment sits above the other region.
- Readings:
  - (a) Comment lines directly above a `#region` travel with its block, as attached trivia does in SHARED-ORDER-002 and FMT-USING-008.
  - (b) The fix is withheld. `AGENTS.md` (*Code-fix coverage*) asks a fix that would relocate a comment to withhold itself; SHARED-FIX-003 does not list relocation.
  - (c) Keep the current behavior.

## `SDD open questions: sdd/specs/analyzer/RH7311.md` (new issue)

**N5 — The same description in an `#if` and an `#else` branch.** Touches RH7311-001, SHARED-TYPEREGION-006.

- Input: `#if DEBUG` / `#region Fields` / `int _a;` / `#endregion` / `#else` / `#region Fields` / `int _b;` / `#endregion` / `#endif`.
- Real output: my probe confirms that the second `#region Fields` is reported.
- Why it looks wrong: only one `Fields` region is compiled in any configuration.
- Readings:
  - (a) Do not compare regions of mutually exclusive branches.
  - (b) Keep the current behavior: the editor shows both regions.

## For `shared/using-directive-ordering.md` (lead decides: #916 is closed)

**N4 — An alias of a type that is not a name.** Touches SHARED-USING-004, SHARED-USING-005, RH7204, RH7207, and the formatter (`UsingGrouping` delegates to `UsingDirectiveOrderingUtilities`).

- Input: `using A = System.Text;` / blank line / `using B = int;`.
- Real output: RH7207 reports it, and the fix gives `using B = int;` / blank line / `using A = System.Text;`. A tuple target behaves the same way.
- Why it looks wrong: the root of such an alias is `""`. Its group key equals that of `System`, and the ordinal tie-break places it first, against SHARED-USING-005. SHARED-USING-004 defines no root for a target that is not a name.
- The RH7204 and RH7207 specs leave this case out. RH7207-003 only states its location.

## For the lead to weigh against P9

**N6 — The RH7303 and RH7310 fixes leave a blank line next to a brace.** Candidate for "Decided by principle P10 (#912)". Touches RH7303-004, RH7310-004.

- RH7310: `int _a;` / blank line / `#region Methods` / blank line / `#endregion // Methods` / `}` becomes `int _a;` / blank line / `}`.
- RH7303: a region with blank lines inside, at the start or end of a body, leaves a blank line after `{` or before `}`.
- In both cases the formatter removes that blank line.
- P10 says a fix's output is laid out as the formatter would lay it out. P9 says a fix keeps the layout around it. Both specs now cover only regions between two declarations or two statements, so neither decides this case.

# Decisions needed

_None._
