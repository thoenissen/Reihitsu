Role: skeptic
Commit: 1bde6ff
Verdict: proceed narrower

My prompt named the files to read and gave no intended fix, suspected cause, or hoped-for verdict. The spec author's recommendations on Q1–Q8 were treated as claims to test.

## Premise

The run writes specs for existing behavior, so the premise is that each requirement states what the code does today and leaves out what looks wrong. For most requirements that holds. `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"` gives 26 passed. But eleven sentences either contradict the code or decide a case that is an open question. The spec author must fix them before tests are derived from the spec (see *Breaking inputs*, A1–A11).

Evidence comes from `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF) and `scripts/format.sh` on fixtures in the scratchpad, which have since been deleted. Rules without a code fix (RH7305, RH7305A, RH7307, RH7308, RH7311) cannot go through `apply-fix.sh`. Statements about them that I did not confirm by a run are marked *code reading*.

## Mechanism and candidates

_N/A._ This is not a bug fix. The candidate set below comes from the dispatch code, not from the issue text:

- The using rules register `CompilationUnit`, `NamespaceDeclaration`, and `FileScopedNamespaceDeclaration`. They classify directives with `UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup` and `GetRootNamespace`. `Name` is a `NameSyntax`, so it is `null` for an alias of a non-name type.
- The region rules walk `DescendantTrivia(descendIntoTrivia: true)`, which includes directives in excluded branches. Executable code is decided by `RegionDirectiveUtilities.IsWithinElementBody`, whose switch checks `BlockSyntax`, `AccessorListSyntax`, `AnonymousFunctionExpressionSyntax`, `LocalFunctionStatementSyntax`, and `StatementSyntax`. It stops at `TypeDeclarationSyntax`, namespaces, and the compilation unit. Top-level regions come from `GetTopLevelRegions`.

## Breaking inputs

### A — Sentences the spec author must change (current code contradicts them, or they decide an open question)

**A1. RH7203-002, RH7204-002, RH7206-002 decide Q1–Q3.** These requirements forbid a report when the directive's "neighbor" belongs to another group:

- RH7203-002: reading "neighbor" as the adjacent directive, the code contradicts it. RH7203 reports `using Microsoft.A;` in `Microsoft.B` / `System` / `Microsoft.A` and in `Microsoft.B` / `static System.Math` / `Microsoft.A`. It also reports `using System;` in `System.Linq` / `Microsoft.X` / `System`. Each case comes out `fixed` (reported) in the run.
- RH7204-002 ("neighbor alias") and RH7206-002: whichever way "neighbor" is read, the sentence fixes Input A of Q2 and Q3 as not reported. That is the very case the questions ask about.

The negatives should state only what holds under every reading, for example: a directive is never reported *because of* a directive of another group. The examples can stay as they are: in each of them the two directives are adjacent and have no same-group partner.

**A2. RH7302-001 and RH7302-002 cover `# region` (Q4).** `# region Fields` is reported and no fix is offered (`no-fix-offered`). With `# region fields`, the fix writes `# region Fields`, and the input is still reported afterwards: the run applies the fix, then shows `no-fix-offered`. The two-space form behaves the same way. The cause is in `RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer`: `ParentTrivia.ToString().AsSpan().Slice(7)`. The RH6004 spec already sets the precedent: it qualifies its sentence to directives written without whitespace between the `#` and the keyword. RH7302-001 and RH7302-002 need the same qualifier, so that `# region` is left out of the spec.

**A3. The RH7302 non-goal "the formatter normalizes it (FMT-REGION)" is wrong for the fix's output.** The formatter leaves `#region   Members` unchanged; see thoenissen/Reihitsu#914 Q1. Normalizing the spacing is decided but not implemented (thoenissen/Reihitsu#964). Decided-but-not-implemented behavior stays out of the spec, so the non-goal should say only that the fix keeps the spacing.

**A4. RH7303-004 covers the straddling pair (Q5).** Only the example avoids that case. The normative sentence ("the whole line of the directive it pairs with") still requires removing the type-level `#region`, which RH7303 does not report. The sentence has to be narrowed, for example to pairs whose directives are both reported.

**A5. RH7304-001 is false for two inputs.**

- **(a) A block comment before the region's first token on its line.** Input: `#region Fields` / `/* c */ int _a;` / `#endregion // Fields`, all at four spaces. The analyzer compares against the line text up to the token (`    /* c */ `), so it reports both directives. The fix writes `    /* c */ #region Fields`, and the run gives `invalid-result`.
- **(b) A straddling inner region.** Input: `#region Outer` / `int _x;` / `#region Inner` / `void M() { #endregion }` / `#endregion // Outer`, with the method at six spaces. The analyzer pairs only the directives that lie outside executable code. It therefore pairs `#region Inner` with `#endregion // Outer` and reports both. The fix pairs as the compiler does, through `TryFindMatchingDirective` over all directives: it re-indents `#region Inner` and the `#endregion` inside `M`, which is executable code. The run gives `no-progress`.

As RH7304-001 is written, (a) is decided one way and (b) is not reported at all. Both inputs must be left out: the sentence should cover a first token that starts its line, and pairs with no region directive inside executable code between them.

**A6. RH7309-004 ("the code between two blocks … MUST stay in place") decides a comment-relocation case.** Input: `// The disposal logic` above `#region IDisposable`, and `// The state of the object` above `#region Fields`. After the fix, each comment sits above the other region. This is the new question N3 below. The sentence has to leave comment lines directly above a `#region` out.

**A7. The RH7310 non-goal conflicts with the decided principle P10.** The non-goal says the blank lines left after a removal belong to RH5023, RH5028, and the formatter. P10 (thoenissen/Reihitsu#912) says a fix's output is laid out as the formatter would lay it out. The run shows the conflict:

- `int _a;` / blank line / `#region Methods` / blank line / `#endregion // Methods` / `}` becomes `int _a;` / blank line / `}`. `format.sh` then removes that blank line.
- RH7303-004 has the same shape: a region with blank lines inside a method body leaves a blank line after `{` and before `}`, and the formatter removes both.

thoenissen/Reihitsu#1137 is the precedent for replacing a non-goal that a decided principle overrides. Remove the non-goal. Then check "every other line unchanged" in RH7303-004 and RH7310-004 against P10 and P9 (see N6).

**A8. RH7207-003 is false for an alias whose target is not a name** (code reading). `using B = int;` and `using P = (int, int);` have a null `Name`. In that case `firstUsing.Name?.GetLocation() ?? firstUsing.GetLocation()` puts the diagnostic on the whole directive, not on "the namespace or type it imports". The spec should either narrow the sentence or state that location. For the grouping of these aliases, see N4.

**A9. RH7308-002 says "split … at whitespace".** `_labelSeparators` holds only space, tab, `/`, `&`, `,`, and `-`. For example, a no-break space between words does not split a label. The sentence should name space and tab (FC-002).

**A10. The new sentence in `documentation/rules/general-notes.md` claims too much.** It says "the rules about regions check them as well". That is false for:

- RH5031 and RH5032 (SHARED-BLANK-007: region directives in disabled code are not considered);
- RH7310 (`#if false` / `#region Methods` / blank line / `#endregion` / `#endif` gives `no-diagnostic`);
- RH7304 (a misindented region in `#if false` gives `no-diagnostic`).

Verified only for RH7301, RH7302, RH7303, RH7306, and RH7307. By code reading, RH7309 and RH7311 count such regions (see N5). Either name only the rules that do check them, or move the sentence to those rules' pages.

**A11. The RH7308 rule page is wrong for two qualifiers.** "Access modifiers and other modifier keywords are ignored" together with "a label that contains any other word … is not recognized" implies that `Instance methods` and `Generic methods` are not recognized. Both are recognized: `instance` and `generic` are in `_modifierWords`.

### B — Shared behavior restated instead of referenced

- **"The top-level regions of a type declaration"** is restated in RH7308-001, in RH7309's non-goal and RH7309-001, -002, and -004, in RH7311-001 and -002, and in SHARED-TYPEREGION-003. The existing spec RH7103-004 already relies on the same notion. These are the `GetTopLevelRegions` and `TryFindContainingRegion` callers. The definition covers: not nested in another region (members count through the outer region), not in a nested type, not inside executable code, and each partial declaration separately. More than one spec relies on it, which is the maintainer's condition for a shared requirement. `shared/type-regions.md` is the natural home; it would reference `RegionDirectiveUtilities` without claiming it.
- **"Inside executable code"** lives in RH7303-001 and is used by RH7304 and RH7310 under `Uses`, and by the top-level-region definition above. The same predicate decides it for all of them. It belongs with that shared requirement.
- **RH7301-001 restates the pairing rule** ("each `#endregion` closes the nearest preceding `#region` …"). FMT-REGION-003 already states it with a Why, so RH7301-001 should reference that ID.

### C — Inputs the code decides that the spec does not mention

- **An alias of a non-name type** (`int`, a tuple, an array, a pointer, a `using unsafe` alias): the root namespace is `""`. See N4.
- **Extension blocks (C# 14)** (code reading). `ExtensionBlockDeclarationSyntax` is a `BaseTypeDeclarationSyntax`, so SHARED-TYPEREGION-002 counts it as a nested type. RH7308 never reports it, because its member kind is `Type`. Neither spec mentions it (FC-024).
- **The same description in two `#if` / `#else` branches** (code reading): RH7311 reports the second one. See N5.
- **`#endregion //Implementation`**: RH7306 reports it and offers a fix that changes nothing (`no-progress`). See the Q7 assessment.
- **Expression bodies and initializers** (Q6): these also decide RH7304 and RH7310, not only RH7303. See the Q6 assessment.

### Failure classes the test implementer must cover

- **FC-002**: the split characters in A9.
- **FC-004 and FC-021**: comment relocation (A6); a comment before the first token (A5a).
- **FC-007 and FC-009**: `# region` (A2); the straddling inner region (A5b); `//Implementation` (C).
- **FC-013**: formatter output `# region Fields` is reported by RH7302 (Q4).
- **FC-020**: region directives in `#if false`, rule by rule (A10).
- **FC-023**: `@` and spaced names in using directives (thoenissen/Reihitsu#1105).
- **FC-024**: partial declarations and extension blocks.
- **FC-025 and FC-027**: the expression-bodied member kinds and initializers (Q6).
- **FC-033**: the RH7303, RH7306, and RH7310 fixes edit text lines, so test them under CRLF too.

A case that stays out of the spec gets a characterization test without a `[Requirement]` tag.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7201–RH7311 analyzers | Must stay (no production change in this lane) |
| Their code fixes and Fix All | Must stay |
| Formatter (`UsingDirectiveOrderingPhase`, `RegionFormattingPhase`) | Not involved; must stay. `UsingGrouping` delegates to `UsingDirectiveOrderingUtilities`, so N4 applies to the formatter too |
| `Reihitsu.Core` (`UsingDirectiveOrderingUtilities`, `RegionDirectiveUtilities`, `RegionCategoryUtilities`) | Must stay |
| Specs | Must change: A1–A9 and B |
| Rule docs | Must change: `general-notes.md` (A10) and `RH7308.md` (A11). `RH7302.md` is right as changed, for `#region` written without a space |
| Tests | Must change: tag and add tests for every new ID and for SHARED-USING-001 … -009 (now enforced) |

## Simpler option

For A1, one neutral negative per rule ("never reported because of a directive of another group") replaces all three "neighbor" clauses. It takes no side on Q1–Q3 and needs no example change.

## Questions for the maintainer

None blocks the run. Every item below is filed per `sdd/README.md` (*Open questions*) by the lead.

### Assessment of the spec author's questions

**Q1–Q3 (RH7203, RH7204, RH7206)**
- Real; reproduced by runs (see A1).
- Not decided by a principle. P4 does not apply, because the formatter and the fixes organize every input.
- Not filed yet: thoenissen/Reihitsu#916 Q1–Q3, #965, and #966 are about other cases.
- Low severity: RH7207 reports every input, and its fix corrects them.
- Add the `System` variant: `System.Linq` / `Microsoft.X` / `System` is reported. A `System`-root pair is never hidden, because the `System` group is filtered on its own.
- The three questions are one pattern. Cross-link the three issues so they can be decided once.

**Q4 (RH7302 `# region`)**
- Real; reproduced (see A2).
- Decided in substance: thoenissen/Reihitsu#1043 Q1 → #1079 settled that `# region` is a region directive, because C# allows whitespace after the `#`. That rules out reading (b).
- The fix does not clear its own diagnostic, also with a single space. That part is P7 (SHARED-FIX-001).
- The formatter writes `# region Fields` for both `# region fields` and `# region Fields`, and RH7302 reports it (FC-013).
- File it marked as decided by thoenissen/Reihitsu#1043 Q1 and principle P7 (#912), or extend #1079. The lead decides which.
- Follow-up candidate outside this run: FMT-REGION-001 requires a rewritten line to start with `#region`, but the formatter keeps `# region`. That belongs to `formatter/region-formatting.md`.

**Q5 (RH7303, straddling region)**
- Real.
- Decided by principle P1: the fix removes a directive that its own rule does not report, which SHARED-FIX-003 forbids.
- File it marked "Decided by principle P1 (#912)". Bringing the fix in line with SHARED-FIX-003 is reading (b). Reading (c), reporting the partner too, would be a behavior change that needs a change doc. The maintainer may still prefer it, but compliance does not require it.
- The blank line left after `{` belongs to N6.

**Q6 (RH7303, expression bodies and initializers)**
- Real; reproduced. A property initializer (`int P { get; } = #region … 2;`) is not reported either.
- The P11 marking is right.
- Add the shared blast radius: `IsWithinElementBody` also decides RH7304, RH7310, and every top-level-region rule (RH7305, RH7305A, RH7308, RH7309, RH7311, RH7103, RH7110, RH7401–RH7412). RH7310 today reports an empty region inside a field initializer, and its fix removes it (`fixed` in the run).
- List the whole set: the expression bodies of methods, properties, indexers, operators, conversion operators, constructors, and finalizers; field and property initializers; constructor-initializer arguments; parameter defaults; attribute arguments.

**Q7 (RH7306, a description that is only `Implementation`)**
- Real; reproduced.
- Not decided by a principle:
  - SHARED-FIX-003 permits the change, because both directives are reported.
  - The formatter leaves `#region` / `#endregion //` alone (FMT-REGION-004).
  - RH7301 sees two empty descriptions and reports nothing.
- Add `#endregion //Implementation`: it is reported, and the offered fix changes nothing (`no-progress`). That part is decided by P7 (SHARED-FIX-005). File it in the same issue, marked as decided.

**Q8 (RH7308, nested regions)**
- Real by code reading: `GetTopLevelRegions` keeps only nesting depth 1, and `TryFindContainingRegion` returns the outer region.
- Not filed yet.
- Add reading (c). The page's two sentences together ("a top-level region" and "only members placed directly inside the region") mean that members in a nested region are not checked at all, so neither input is reported. RH7308-001 correctly leaves nested-region members out.

### New questions to file

- **N1 — RH7304: a comment before the region's first token** (A5a).
  - Analyzer part: decided by principle P12. The line's first element, the comment, sets the column, so the directives at four spaces match.
  - Fix part: decided by principle P1 (SHARED-FIX-003): the result does not parse.
  - File it marked "Decided by principle P12 / P1 (#912)".
- **N2 — RH7304: the pairing around a straddling inner region** (A5b).
  - The open question: the analyzer pairs only the directives outside executable code; the fix pairs as the compiler does (FMT-REGION-003).
    - Reading (a), compiler pairing, skipping pairs that have a directive inside executable code: nothing is reported.
    - Reading (b), current: `#region Inner` and `#endregion // Outer` are reported.
  - Already decided:
    - P7: the fix does not clear its diagnostic (`no-progress`).
    - P9: the fix moves an `#endregion` inside executable code, which its diagnostic is not about.
- **N3 — RH7309: a comment above a moved region stays where it was** (A6). Readings:
  - (a) The comment lines directly above a `#region` travel with its block, as attached trivia does in SHARED-ORDER-002 and FMT-USING-008.
  - (b) The fix is withheld. `AGENTS.md` (*Code-fix coverage*) asks a fix that would relocate a comment to withhold itself. SHARED-FIX-003 does not list relocation.
  - (c) Current: each comment ends up above another region.
- **N4 — `shared/using-directive-ordering.md`: an alias of a non-name type** (C).
  - Input: `using A = System.Text;` / blank line / `using B = int;`.
  - Real output: RH7207 reports it, and the fix gives `using B = int;` / blank line / `using A = System.Text;`. A tuple target behaves the same way.
  - Why it looks wrong: the empty root's order key equals the `System` key, and the ordinal tie-break puts `""` first. So the group is placed before the `System` alias group, against SHARED-USING-005. SHARED-USING-004 does not define a root for a target that is not a name.
  - Touches SHARED-USING-004, SHARED-USING-005, RH7204, and RH7207-003.
  - The issue for that spec (thoenissen/Reihitsu#916) is closed. The lead decides whether to reopen it or open a new one.
- **N5 — RH7311: the same description in two `#if` / `#else` branches** (code reading; no run is possible without a fix). Input: `#if DEBUG` / `#region Fields` … `#endregion` / `#else` / `#region Fields` … `#endregion` / `#endif`. The second `#region` is reported, although in every configuration only one `Fields` region is compiled. The test implementer can confirm it with a characterization test before the lead files it.
- **N6 — the RH7303 and RH7310 fixes leave a blank line next to a brace**, which the formatter then removes (A7). These are candidates for "Decided by principle P10 (#912)". The lead weighs them against P9, which keeps the layout around the fix unchanged.

### Items the spec author reported to the lead

- **thoenissen/Reihitsu#1105 (P3)**: I agree that the RH7204 alias-target and RH7206 static-name spellings are further instances.
- **thoenissen/Reihitsu#1151**: the trailing-whitespace case (`#region members  ` / `#endregion // members`) falls inside #1151's decision. The comment is exactly the trimmed region name, so it should be renamed. Comment on #1151 instead of filing a question.
- **thoenissen/Reihitsu#1143 and #1135**: I agree. Nothing new.
- **Follow-up candidate outside the run**: the non-goal in `shared/diagnostics.md` speaks of inactive code. The spec author already noted it. A10 is the part this run must fix.
