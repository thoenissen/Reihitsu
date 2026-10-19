Role: spec author
Commit: c27ba5f
Verdict: spec written

# Scope of the diff

- 19 new analyzer specs: `sdd/specs/analyzer/RH7201.md` … `RH7207.md`, `RH7301.md` … `RH7305.md`, `RH7305A.md`, `RH7306.md` … `RH7311.md`.
- 2 new shared specs, each relied on by more than one spec (the handover's condition):
  - `sdd/specs/shared/region-descriptions.md` (SHARED-REGDESC, owner `RegionDescriptionAnalyzerBase`): what a `#region`/`#endregion` description is (used by RH7301, RH7306, RH7307, RH7308, RH7309, RH7311) and how RH7306/RH7307 check every directive.
  - `sdd/specs/shared/type-regions.md` (SHARED-TYPEREGION, owner `TypesOrganizedWithRegionsAnalyzerBase`): checked types, counted members, "organized", location (used by RH7305, RH7305A).
- `SHARED-USING` reused unchanged; the fix requirement of every using rule references FMT-USING-001, -002, -003, -005 … -009 instead of a new shared requirement. `shared/ordering-move-safety.md` untouched. `RegionDirectiveUtilities` and `OrderingDeclarationUtilities` are referenced, never claimed. `RegionCategoryUtilities` (used only by RH7309's analyzer and fix) is claimed by RH7309. `UsingDirectiveOrderingCodeFixProviderBase` and `UsingDirectiveCodeFixUtilities` stay unclaimed (no spec specifies them as such; FMT-USING's intro already says the RH72xx fixes run its phase).
- 77 IDs added to `sdd/specs/requirement-ids.txt` (ordinal order; `RH7305-…` sorts before `RH7305A-…`); 21 rows added to `sdd/specs/README.md`.
- Rule pages corrected where wrong:
  - `documentation/rules/RH7302.md`: "looks at the first character that is a letter" was wrong (`#region 1st fields` is not reported); now "the first character of the description".
  - `documentation/rules/RH7308.md`: `-` is a separator too; only modifier keywords are ignored, and a label with any other word (`Helper methods`) is not recognized. The page's "Only members placed directly inside the region are considered" was left as is, see Q8.
  - `documentation/rules/general-notes.md`: one sentence — region directives in an excluded `#if` branch are still checked by the region rules (verified for RH7301, RH7302, RH7303, RH7306, RH7307; FMT-REGION-006 already relies on it). The skeptic may want to challenge whether this belongs in general notes.

# How the statements were established

Every example block in the 21 specs was run against the shipped analyzers and fixes with a scratch file-based app outside the repository (project reference to `Reihitsu.Tooling`; markup diff of the rule's diagnostics, action count per diagnostic, one Fix All application, and the single fixes applied one after another): 100 blocks, 16 before/after pairs, 2 "no code fix is offered" cases, 0 mismatches. The verifier was checked to fail on a deliberately wrong expectation. `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed. `RequirementCoverageTests` fails, as expected, for every new ID and for SHARED-USING-001 … SHARED-USING-009 (now enforced, because enforced specs list them under `Uses`).

# Changed requirements

All added (existing behavior, specs and tests lane):

- RH7201-001 … RH7201-004; RH7202-001 … RH7202-004; RH7203-001 … RH7203-004; RH7204-001 … RH7204-004; RH7205-001 … RH7205-004; RH7206-001 … RH7206-004; RH7207-001 … RH7207-004
- RH7301-001 … RH7301-005; RH7302-001 … RH7302-005; RH7303-001 … RH7303-004; RH7304-001 … RH7304-004
- RH7305-001, RH7305-002; RH7305A-001 … RH7305A-003
- RH7306-001 … RH7306-003; RH7307-001, RH7307-002; RH7308-001 … RH7308-003
- RH7309-001 … RH7309-005; RH7310-001 … RH7310-004; RH7311-001 … RH7311-003
- SHARED-REGDESC-001, SHARED-REGDESC-002; SHARED-TYPEREGION-001 … SHARED-TYPEREGION-004

Points the skeptic should try to break:

- RH7203-001, RH7204-001, RH7206-001 say "directly follows"; the non-adjacent cases are left out and filed (Q1–Q3).
- RH7302-002's sentence also covers `# region Fields`, which the code reports — filed as Q4, a likely bug against RH7302-002.
- RH7302-005 is written for a `#region` line that ends with its description; the trailing-whitespace case is reported to the lead below (#1151).
- RH7306-003 covers only descriptions with text before the word (Q7). RH7303-004's example avoids a straddling pair (Q5). RH7308-001 covers members outside any nested region (Q8).
- RH7309-002 states that interface declarations are not checked, with a Why; that is current behavior, not a question.
- RH7207-002 states that a group boundary with two or more blank lines is not reported (RH5023 reports the extra line; verified).
- RH7207-003: for an alias directive first in the block, the diagnostic is on the alias target, not the alias name (the other RH72xx rules use the alias name).

# Examples for the tests

Every requirement ID above has examples. SHARED-USING-001 … SHARED-USING-009 need tagged tests too; the RH72xx test classes cover them already in part (`DisabledConditionalUsingBlocksAreNotReportedWhenTheyCannotBeSafelyReordered` in each class for SHARED-USING-009; RH7207 tests for -001 … -008). FMT-USING IDs are not enforced. RH7310's nested case keeps `NumberOfFixAllIterations = 2` as current behavior (#1143), not as a requirement.

# Reported to the lead (not to be filed as new questions)

- #1105 (P3), further instances, confirmed by running them: RH7204 groups an alias by the spelling of its target (`using B = System.Text;` then `using A = System . IO;` is not reported, `using A = System.IO;` is); RH7206 groups a static directive by spelling (`using static System . Math;` or `using static @System.Math;` before `using static System.Console;` is not reported). RH7202 and RH7205 do not depend on the name.
- #1151, related instance: `#region members  ` (trailing whitespace) with `#endregion // members` — the RH7302 fix gives `#region Members  ` and leaves `#endregion // members`, which RH7301 then reports. The comment is exactly the region name, but the fix compares against the description with its trailing whitespace. The lead decides whether this belongs in #1151 or needs a question.
- #1143: RH7310's nested empty regions (already listed there). No further instance found in this family; all other Fix All examples converge in one application.
- #1135: nothing new; the struct-layout part is already in the issue.

# Open questions to file

## `SDD open questions: sdd/specs/analyzer/RH7203.md` (new issue)

**Q1 — Another group between two directives hides an out-of-order pair, a `System` or `using static` directive does not.** Touches RH7203-001, RH7203-002.

- Input A: `using Microsoft.B;` / `using Alpha.X;` / `using Microsoft.A;` — real output: not reported.
- Input B: `using Microsoft.B;` / `using System;` / `using Microsoft.A;` — real output: `Microsoft.A` reported.
- Input C: `using Microsoft.B;` / `using static System.Math;` / `using Microsoft.A;` — real output: `Microsoft.A` reported.
- Why it looks wrong: in all three, `Microsoft.A` and `Microsoft.B` are in the same group and out of order (SHARED-USING-007). Whether that gets reported depends on which kind of directive is between them. The analyzer compares a directive with the previous directive of the same kind (System roots and other roots counting as separate kinds), and only when the two are in the same group. RH7201 and RH7207 report all three blocks, and the fix organizes them, so the user is not left without a diagnostic.
- Readings: (a) compare with the previous directive of the same group, whatever lies between — A, B, and C are reported; (b) compare only directives that are adjacent in the block — B and C are not reported either.
- Recommendation: (a), so RH7203 reports the order within a group and RH7207 reports the group's contiguity.

## `SDD open questions: sdd/specs/analyzer/RH7204.md` (new issue)

**Q2 — Another alias group between two aliases hides an out-of-order pair, a `using static` directive does not.** Touches RH7204-001, RH7204-002.

- Input A: `using B = System.Text;` / `using X = Microsoft.Y;` / `using A = System.IO;` — not reported.
- Input B: `using B = System.Text;` / `using static System.Math;` / `using A = System.IO;` — `A` reported.
- Why it looks wrong: `A` and `B` are in the same group (target root `System`) and out of order in both inputs. The analyzer compares an alias only with the previous alias, and only when that alias is in the same group.
- Readings and recommendation as in Q1: (a) compare with the previous alias of the same group, so A is reported; (b) adjacent only, so B is not reported. Recommend (a).

## `SDD open questions: sdd/specs/analyzer/RH7206.md` (new issue)

**Q3 — Another static group between two static directives hides an out-of-order pair, an alias does not.** Touches RH7206-001, RH7206-002.

- Input A: `using static System.Math;` / `using static Microsoft.Y.Z;` / `using static System.Console;` — not reported.
- Input B: `using static System.Math;` / `using A = System.Text;` / `using static System.Console;` — `System.Console` reported.
- Why it looks wrong, readings, and recommendation: as in Q2, for `using static` directives.

## `SDD open questions: sdd/specs/analyzer/RH7302.md` (new issue)

**Q4 — `# region Fields` is reported, and no fix is offered.** Touches RH7302-001, RH7302-002, SHARED-FIX-001.

- Input: `class C` / `{` / `    # region Fields` / `    int _a;` / `    #endregion // Fields` / `}` (a space between `#` and `region`, which C# allows).
- Real output: RH7302 reports `# region Fields`, and the fix registers no action. With two spaces, `#  region fields` is reported and the fix gives `#  region Fields`, which is reported again, because the check reads the text from `on Fields` instead of from the description.
- Why it looks wrong: the description starts with an upper-case letter. From reading the code, the analyzer skips a fixed seven characters of the directive text instead of reading the description, so any whitespace after `#` shifts what it checks. The fix reads the real description, finds nothing to capitalize, and offers nothing, so the user gets a diagnostic they cannot resolve, or a fix that does not clear its own diagnostic (SHARED-FIX-001).
- Readings: (a) bug — check the directive's description, so `# region Fields` is not reported and `# region fields` is reported and fixed; (b) intended — `#region` must be written without a space. Under (b), the diagnostic would still have to say so, and the fix would have to offer the correction.
- Recommendation: (a). Under (a) the case is a bug against RH7302-002 as written.

## `SDD open questions: sdd/specs/analyzer/RH7303.md` (new issue)

**Q5 — A straddling region: the fix removes a `#region` that is not reported.** Touches RH7303-001, RH7303-004, SHARED-FIX-003.

- Input: `class C` / `{` / `    #region Members` / `` / `    void M()` / `    {` / `        var x = 1;` / `` / `        #endregion` / `    }` / `}`.
- Real output: only the `#endregion` inside `M` is reported. The fix removes both directive lines and gives `class C` / `{` / `` / `    void M()` / `    {` / `        var x = 1;` / `` / `    }` / `}`, which leaves a blank line after `{`.
- Why it looks wrong: the type-level `#region Members` is not reported (RH7303-002), yet the fix deletes it together with its name. SHARED-FIX-003 lets a fix change only directive text that its own diagnostic reports. Removing only the `#endregion` would leave an unpaired `#region`, so the fix cannot simply leave the partner.
- Readings: (a) current — a region is one unit, so removing both directives is part of the fix; (b) withhold the fix when the partner directive is not reported, and leave the diagnostic; (c) also report the type-level partner of a straddling pair, so both removed directives are reported.
- Recommendation: (b) or (c). Choosing between them is the maintainer's decision.

**Q6 — Regions inside an expression body or a field initializer are not reported.** Decided by principle P11 (#912): a gap inside the construct the rule names. Touches RH7303-001, RH7303-002.

- Input A: `class C` / `{` / `    int M() =>` / `        #region Value` / `        1;` / `        #endregion` / `}` — not reported.
- Input B: `class C` / `{` / `    private int[] _a = new[]` / `    {` / `        #region Values` / `        1,` / `        #endregion` / `    };` / `}` — not reported.
- Real output: neither is reported. The same region inside a block body, a lambda body, or an accessor list is reported.
- Why it looks wrong: in both inputs the region sits inside executable code of an element, which the rule page names ("inside executable code — a method or constructor body, … a lambda, or any statement"). The check looks only for blocks, accessor lists, anonymous functions, local functions, and statements among the directive's ancestors, so an arrow expression body or an initializer expression is missed.

## `SDD open questions: sdd/specs/analyzer/RH7306.md` (new issue)

**Q7 — A description that is only `Implementation` is fixed to an unnamed region with `#endregion //`.** Touches RH7306-001, RH7306-003.

- Input: `class C` / `{` / `    #region Implementation` / `` / `    int _a;` / `` / `    #endregion // Implementation` / `}`.
- Real output: both directives are reported, and the fix gives `#region` / … / `#endregion //`.
- Why it looks wrong: the fix removes the whole name and leaves a dangling `//`. The region becomes unnamed, which no other rule asks for, and the region's heading is lost.
- Readings: (a) withhold the fix when nothing would remain of the description, and leave the diagnostic; (b) remove the description and the `//`, giving `#region` / `#endregion`; (c) current behavior.
- Recommendation: (a). Choosing a name is the user's decision, as in RH7301-005.

## `SDD open questions: sdd/specs/analyzer/RH7308.md` (new issue)

**Q8 — A member in a nested region is judged by the outer region's label.** Touches RH7308-001.

- Input: `class C` / `{` / `    #region Fields` / `    int _a;` / `    #region Properties` / `    public int P => 1;` / `    #endregion` / `    #endregion` / `}`.
- Real output: `P` is reported against `Fields` ("The standard region 'Fields' should contain only field declarations."). With an unrecognized outer label (`#region IDisposable` around `#region Properties` holding a method), the nested `Properties` label is never checked.
- Why it looks wrong: the rule page says "Only members placed directly inside the region are considered". `P` is directly inside `Properties`, which matches its kind. The analyzer looks only at the top-level region around a member.
- Readings: (a) judge each member by the innermost region around it, so the first input is not reported and the method in `Properties` is; (b) current, judged by the top-level region, and the page sentence is corrected.
- Recommendation: (a), which is what the page promises.

# Non-blocking notes

- `sdd/specs/shared/diagnostics.md` has the non-goal "Code in an inactive `#if` branch … no rule can report it". Region directives in such a branch are reported by RH7301, RH7302, RH7303, RH7306, and RH7307. The non-goal speaks of code, not directives, so the specs do not contradict it, but a reader may stumble over it. Not changed; that spec is outside this run.
- New-rule candidates under P11: none found.

# Decisions needed

_None._
