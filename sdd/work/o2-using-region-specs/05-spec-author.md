Role: spec author
Commit: f9a7832
Verdict: spec written

Before changing anything I re-ran every finding of `04-skeptic.md` with my scratch file-based probe. The probe sits outside the repository and prints, for one input:
- the rule's diagnostics as markup;
- how many fix actions each diagnostic offers;
- the result of one fix, and of Fix All.

All six findings reproduce:
- **S1:** RH7309 reports `#region Values` inside a nested enum, and its fix swaps `A,` into the class body and `Dispose` into the enum.
- **S2:** the same happens for a region inside an array initializer.
- **S3:** the RH7303 fix leaves two blank lines between `var x = 1;` and `x++;`.
- **S4:** the comment above the first block ends up above `#region Fields`.
- **S5:** the RH7310 fix leaves `int _a;` / blank line / `}`.
- **S6:** only `#region Setup` is reported, and the fix removes both directives.

Each sentence was narrowed so that the uncertain case falls outside the spec, instead of describing it.

After the change:
- All 104 example blocks of the 21 specs reproduce, with 0 mismatches. The SHARED-TYPEREGION-005 blocks were run against RH7303.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed.

Constraints kept:
- `RH7103.md`, `formatter/region-formatting.md`, `shared/ordering-move-safety.md`, and `shared/using-directive-ordering.md` are not edited.
- No requirement ID was added, retired, moved, or renumbered in this round.
- No decided-but-not-implemented behavior is in a spec.

# Per-finding table

| Finding | Re-derived | What changed |
|---|---|---|
| S1 — nested enum | Yes, for RH7309; the swapped fix result was observed. RH7311 is confirmed too: an enum region repeating a class region's description is reported. For RH7305 and RH7305A, the enum's region cannot change the verdict: it only enters when the type has no region of its own, and then the enum itself lies outside every region, so the type is reported either way. No question is filed for them. | SHARED-TYPEREGION-006 now applies only when no region directive of the declaration lies inside a nested enum or a nested extension block. The extension block is excluded as well, because I could not confirm how the code treats it. RH7305, RH7305A, RH7308, RH7309, and RH7311 inherit the narrowing. The case is N7. |
| S2 — region in an initializer | Yes, for RH7309; the region was moved out of the initializer. | SHARED-TYPEREGION-006 now applies only when every region directive of the declaration lies inside executable code or between declarations (SHARED-TYPEREGION-005). The RH7309 fix part is added to Q6. |
| S3 — double blank line | Yes. | RH7303-004 now also requires that neither directive has a blank line directly above or below it. The case is added to N6. |
| S4 — comment above the first block | Yes. | RH7309-004 now applies only to a type declaration whose body holds no comment outside its top-level region blocks. The case is added to N3. |
| S5 — the type's own brace | Yes. | RH7310-004 now names sibling declarations: another member of the same type, or another declaration of the same namespace or file, and not the declaration whose braces enclose the region. The brace case is added to N6. |
| S6 — top-level statements | Yes. Only `#region Setup` is reported, and the fix removes both lines. | SHARED-TYPEREGION-005 now opens with "In a file without top-level statements", so neither list covers those files. The case is N8. A directive between a member's header and the `{` of its body lies in neither list as positions go, so it stays out of the spec. |
| Q1–Q3 severity line | — | Now reads "RH7207 reports every input, and its fix corrects it". |
| Q6 RH7309 part | Yes (S2) | Added, marked P1. |
| N3, N6 additions | Yes (S4; S3 and S5) | Added. |
| N7, N8 | Yes | Added, with the principle markings below. |

# Changed requirements in this round

All four are changed, not added, and none of them is merged yet. RH7305, RH7305A, RH7308, RH7309, and RH7311 inherit the narrowed definitions without a text change.

| Requirement | Narrowed to leave out |
|---|---|
| SHARED-TYPEREGION-005 | Files with top-level statements (S6) |
| SHARED-TYPEREGION-006 | Declarations with a directive in a nested enum or extension block, or one that lies in neither list (S1, S2) |
| RH7303-004 | Directives with an adjacent blank line (S3) |
| RH7309-004 | Comments outside the region blocks (S4) |
| RH7310-004 | The enclosing declaration's braces (S5) |

The earlier handovers `01-spec-author.md` and `03-spec-author.md` still hold for everything else. RH7103-004 is a later consumer of SHARED-TYPEREGION-006, and RH7110 and RH7401–RH7412 will be once their specs exist.

# Examples for the tests

- Every requirement ID of the 21 specs, plus SHARED-USING-001 … SHARED-USING-009, which enforced specs now list under `Uses`.
- Characterization tests, without a `[Requirement]` tag, for the cases of Q1–Q8 and N1–N8, among them:
  - S1 and S2 for RH7309, RH7311, and RH7305;
  - S6 for RH7303.
- Run S3 under CRLF as well (FC-033).

# For the lead (existing issues)

- #1105 (P3): RH7204 and RH7206 are further instances. RH7204 groups an alias by how its target is spelled: `using B = System.Text;` / `using A = System . IO;` is not reported. RH7206 does the same with `using static System . Math;` and `using static @System.Math;` before `using static System.Console;`.
- #1151: comment on it with this case. Input: `#region members  ` (trailing whitespace) / `#endregion // members`. The RH7302 fix gives `#region Members  ` and leaves `#endregion // members`, which RH7301 then reports. The comment is exactly the trimmed region name.
- #1143, #1135: nothing new.
- Follow-up candidates outside this run:
  - The formatter keeps `# region` when it rewrites the line (`# region fields` → `# region Fields`), although FMT-REGION-001 requires the line to start with `#region`.
  - The `shared/diagnostics.md` non-goal about inactive code also reads as covering region directives.

# Open questions — final ready-to-file text

Each block below is meant to be filed as written. One issue per spec, titled `SDD open questions: <spec path>`, as a sub-issue of #912. Low priority unless noted.

## `SDD open questions: sdd/specs/analyzer/RH7203.md`

Open questions found while writing the analyzer specs of #994 (RH7201–RH7311). They are left out of the spec until they are decided (`sdd/README.md`, *Open questions*). Every input was run against the shipped analyzer and code fix. Q1 has the same pattern as Q1 of the RH7204 and RH7206 issues (links), so the three can be decided together.

### Q1 — A regular directive of another non-`System` root between two directives hides an out-of-order pair

**Touches:** RH7203-001, RH7203-002, SHARED-USING-007.

**Inputs and real output:**

| Input | Real output |
|---|---|
| A: `using Microsoft.B;` / `using Alpha.X;` / `using Microsoft.A;` | not reported |
| B: `using Microsoft.B;` / `using System;` / `using Microsoft.A;` | `Microsoft.A` reported |
| C: `using Microsoft.B;` / `using static System.Math;` / `using Microsoft.A;` | `Microsoft.A` reported |
| D: `using System.Linq;` / `using Microsoft.X;` / `using System;` | `System` reported |

**Why it looks wrong:**
- In every input, two directives of one group are out of order (SHARED-USING-007). Whether that is reported depends on which kind of directive lies between them.
- The analyzer compares a directive with the previous directive of its own kind, and only when that directive is in the same group. `System` roots and other roots count as separate kinds.
- So a pair of `System`-root directives is never hidden. A pair of other roots is hidden only by a regular directive of another non-`System` root between them. A `using static` or alias directive of any root is filtered out before the comparison and never hides a pair: `using static Microsoft.X;` or `using X = Microsoft.Y;` between `Microsoft.B` and `Microsoft.A` is reported, like Input C.

**Readings:**
- (a) Compare with the previous directive of the same group, whatever lies between. A, B, C, and D are all reported.
- (b) Compare only directives that are adjacent in the block. B, C, and D are not reported either.

**Recommendation:** (a). RH7203 then reports the order within a group, and RH7207 reports the group's contiguity.

**Severity:** low. RH7207 reports every input, and its fix corrects it. The question is not decided by a principle. P4 does not apply, because the fix and the formatter organize every input.

## `SDD open questions: sdd/specs/analyzer/RH7204.md`

Open question found while writing the analyzer specs of #994. It is left out of the spec until it is decided. The input was run against the shipped analyzer and code fix. Same pattern as Q1 of the RH7203 and RH7206 issues (links).

### Q1 — Another alias group between two aliases hides an out-of-order pair

**Touches:** RH7204-001, RH7204-002, SHARED-USING-007.

**Inputs and real output:**

| Input | Real output |
|---|---|
| A: `using B = System.Text;` / `using X = Microsoft.Y;` / `using A = System.IO;` | not reported |
| B: `using B = System.Text;` / `using static System.Math;` / `using A = System.IO;` | `A` reported |

**Why it looks wrong:** `A` and `B` belong to the same group (target root `System`) and are out of order in both inputs. The analyzer compares an alias only with the previous alias, and only when that alias is in the same group. So an alias of another group between the two hides the pair, while a `using static` directive does not.

**Readings:**
- (a) Compare with the previous alias of the same group, whatever lies between. A is reported.
- (b) Compare only directives that are adjacent in the block. B is not reported either.

**Recommendation:** (a).

**Severity:** low. RH7207 reports every input, and its fix corrects it. Not decided by a principle.

## `SDD open questions: sdd/specs/analyzer/RH7206.md`

Open question found while writing the analyzer specs of #994. It is left out of the spec until it is decided. The input was run against the shipped analyzer and code fix. Same pattern as Q1 of the RH7203 and RH7204 issues (links).

### Q1 — Another static group between two static directives hides an out-of-order pair

**Touches:** RH7206-001, RH7206-002, SHARED-USING-007.

**Inputs and real output:**

| Input | Real output |
|---|---|
| A: `using static System.Math;` / `using static Microsoft.Y.Z;` / `using static System.Console;` | not reported |
| B: `using static System.Math;` / `using A = System.Text;` / `using static System.Console;` | `System.Console` reported |

**Why it looks wrong:** both `System` directives are in one group and out of order. A `using static` directive of another group between them hides the pair, while an alias does not.

**Readings:**
- (a) Compare with the previous static directive of the same group, whatever lies between. A is reported.
- (b) Compare only directives that are adjacent in the block. B is not reported either.

**Recommendation:** (a).

**Severity:** low. RH7207 reports every input, and its fix corrects it. Not decided by a principle.

## `SDD open questions: sdd/specs/analyzer/RH7302.md`

(The lead may instead extend #1079.)

### Q1 — `# region` is checked at a fixed offset

**Decided by #1043 Q1 (→ #1079: `# region` is a region directive) and by principle P7 (#912), SHARED-FIX-001.**

**Touches:** RH7302-001, RH7302-002, SHARED-FIX-001.

**Inputs and real output:**

| Input | Analyzer | Fix |
|---|---|---|
| `class C` / `{` / `    # region Fields` / `    int _a;` / `    #endregion // Fields` / `}` | reported | none offered |
| `# region fields` | reported | gives `# region Fields`, which is reported again |
| `#  region fields` (two spaces) | reported | gives `#  region Fields`, which is reported again |

The formatter also writes `# region fields` as `# region Fields` (`scripts/trace.sh`), so RH7302 reports the formatter's own output.

**Why it looks wrong:**
- C# allows whitespace between `#` and `region`, and #1079 treats such a line as a region directive.
- From reading the code, the analyzer checks the directive text after a fixed seven characters (`ParentTrivia.ToString().AsSpan().Slice(7)`), not the description. So it reads `n Fields` instead of `Fields`.
- The fix reads the real description. When that is already capitalized (`# region Fields`), it offers nothing. When it is not (`# region fields`, with one space or more), it capitalizes the word, and the diagnostic stays.

**Expected under the decisions:**
- The description is checked.
- `# region Fields` is not reported.
- `# region fields` is reported, and one application of the fix clears it.

## `SDD open questions: sdd/specs/analyzer/RH7303.md`

### Q1 — Straddling region: the fix removes a `#region` it does not report

**Decided by principle P1 (#912), SHARED-FIX-003.**

**Touches:** RH7303-004, SHARED-FIX-003.

**Input:** `class C` / `{` / `    #region Members` / `` / `    void M()` / `    {` / `        var x = 1;` / `` / `        #endregion` / `    }` / `}`.

**Real output:**
- Only the `#endregion` inside `M` is reported.
- The fix removes both directive lines, including the type-level `#region Members`, which RH7303 does not report.
- The result is `class C` / `{` / `` / `    void M()` / `    {` / `        var x = 1;` / `` / `    }` / `}`.

**Why it looks wrong:** SHARED-FIX-003 lets a fix change only the directive text that its own diagnostic reports.

**What compliance needs:** the fix is withheld for such a pair, and the diagnostic is kept. Also reporting the type-level partner would be a behavior change that needs a change doc; the maintainer may still prefer it. The blank line the fix leaves after `{` is Q4 below.

### Q2 — Regions inside expression bodies and initializers are not handled

**Decided by principle P11 (#912):** a gap inside the construct the rule names.

**Touches:**
- In this batch: RH7303-001, SHARED-TYPEREGION-005, and through them RH7304, RH7310, SHARED-TYPEREGION-006, RH7305, RH7305A, RH7308, RH7309, RH7311.
- Outside this batch: RH7103, RH7110, RH7401–RH7412.

**Inputs RH7303 does not report:**
- `class C` / `{` / `    int M() =>` / `        #region Value` / `        1;` / `        #endregion` / `}`
- `class C` / `{` / `    private int[] _a = new[]` / `    {` / `        #region Values` / `        1,` / `        #endregion` / `    };` / `}`
- `int P { get; } =` / `#region …` / `2;` / `#endregion`

The same region inside a block body, a lambda body, or an accessor list is reported.

**Why it looks wrong:** each region sits inside executable code of an element, which the rule page names ("inside executable code — a method or constructor body, … a lambda, or any statement").

**The full set of places** that `RegionDirectiveUtilities.IsWithinElementBody` does not treat as executable code:
- the expression bodies of methods, properties, indexers, operators, conversion operators, constructors, and finalizers;
- field and property initializers;
- constructor-initializer arguments;
- parameter defaults;
- attribute arguments.

**Shared blast radius:**
- RH7310 reports an empty region inside a field initializer (`new[] { #region Values` / `#endregion` / `1, }`), and its fix removes it.
- The top-level-region rules count such a region as a region of the type. Example: `public class C : IDisposable` with `#region IDisposable` … `#endregion`, followed by `private int[] _a = new[] { #region Values` / `1,` / `#endregion // Values };`. RH7309 reports `#region Values`.

The RH7309 fix on that example produces code that does not parse; that part is filed as Q3 of the RH7309 issue (link).

### Q3 — Top-level statements

**Analyzer part decided by principle P11 (#912). Fix part decided by principle P1 (#912), SHARED-FIX-003.**

**Touches:** RH7303-001, RH7303-004, SHARED-TYPEREGION-005.

**Input:** a file holding only `#region Setup` / `var x = 1;` / `System.Console.WriteLine(x);` / `#endregion`.

**Real output:**
- Only `#region Setup` is reported.
- The fix removes both directives, leaving `var x = 1;` / `System.Console.WriteLine(x);`.

**Why it looks wrong:**
- Both directives sit in top-level statement code, which is executable code.
- The analyzer decides by the token that owns the directive's trivia. `#region` belongs to the statement after it. `#endregion` belongs to the end of the file, whose parent is the compilation unit, so it is not reported.
- The fix then removes a directive it does not report, as in Q1.

### Q4 — The fix leaves blank lines that the formatter removes

**Candidate for "Decided by principle P10 (#912)".** The lead weighs it against P9, which keeps the layout around a fix unchanged. Same question as Q1 of the RH7310 issue (link).

**Touches:** RH7303-004.

**Inputs and real output:**
- A region with blank lines inside, at the start or end of a method body. The fix removes only the directive lines and leaves a blank line after `{` or before `}`.
- `var x = 1;` / blank line / `#region Work` / blank line / `x++;` / `#endregion // Work` / `x--;`. The fix leaves `var x = 1;` / blank line / blank line / `x++;`.
- `if (a)` / `{` / `}` / `#region Work` / `M(a);` / `#endregion // Work` / `M(a);`. The fix leaves `if (a) { }` directly followed by `M(a);`. The same happens when the last statement inside the region ends with `}`.

In the first two cases `scripts/format.sh` removes or collapses the blank lines; in the third it adds a blank line after the `}` (SHARED-BLANK-003).

## `SDD open questions: sdd/specs/analyzer/RH7304.md`

### Q1 — A comment before the region's first token on its line

**Analyzer part decided by principle P12 (#912). Fix part decided by principle P1 (#912), SHARED-FIX-003.**

**Touches:** RH7304-001, RH7304-004.

**Input:** `class C` / `{` / `    #region Fields` / `    /* c */ int _a;` / `    #endregion // Fields` / `}`.

**Real output:**
- Both directives are reported, because the expected indentation is taken as the line text up to the token, `    /* c */ `.
- The fix writes `    /* c */ #region Fields`, which does not parse.

**Expected under P12:** the comment is the line's first element and sets the column. The directives at four spaces match, and nothing is reported.

### Q2 — Pairing around a straddling inner region

**Touches:** RH7304-001, RH7304-004, FMT-REGION-003.

**Input:** `class C` / `{` / `    #region Outer` / `    int _x;` / `    #region Inner` / `      void M()` / `      {` / `    #endregion` / `      }` / `    #endregion // Outer` / `}`.

**Real output:**
- The analyzer pairs only the directives outside executable code. It pairs `#region Inner` with `#endregion // Outer` and reports both.
- The fix pairs as the compiler does. It re-indents `#region Inner` and the `#endregion` inside `M`, and the diagnostic stays.

**The open part (readings):**
- (a) Pair as the compiler does (FMT-REGION-003), and skip pairs that have a directive inside executable code. Nothing is reported.
- (b) Current pairing. Both directives are reported.

**Already decided:**
- The fix does not clear its diagnostic: principle P7 (#912), SHARED-FIX-001.
- The fix edits an `#endregion` inside executable code, which its diagnostic is not about: principle P9 (#912).

## `SDD open questions: sdd/specs/analyzer/RH7306.md`

### Q1 — A description that is only `Implementation`

**Touches:** RH7306-001, RH7306-003.

**Input:** `class C` / `{` / `    #region Implementation` / `` / `    int _a;` / `` / `    #endregion // Implementation` / `}`.

**Real output:** both directives are reported. The fix gives `#region` / … / `#endregion //`: an unnamed region with a dangling `//`.

**Why it looks wrong:** the fix removes the whole name, and the region loses its heading.

**Not decided by a principle:**
- SHARED-FIX-003 permits the change, because both directives are reported.
- The formatter leaves the result alone (FMT-REGION-004).
- RH7301 sees two empty descriptions and reports nothing.

**Readings:**
- (a) Withhold the fix when nothing would remain of the description, and keep the diagnostic.
- (b) Remove the `//` as well, giving `#region` / `#endregion`.
- (c) Keep the current behavior.

**Recommendation:** (a). Choosing a name is the user's decision, as in RH7301-005.

**Second part, decided by principle P7 (#912), SHARED-FIX-005:** `#region IDisposable` / `int _a;` / `#endregion //Implementation`. The `#endregion` is reported, and the offered fix changes nothing.

## `SDD open questions: sdd/specs/analyzer/RH7308.md`

### Q1 — A member in a nested region is judged by the outer region's label

**Touches:** RH7308-001, SHARED-TYPEREGION-006.

**Inputs and real output (from reading the code):**
- Input 1: `class C` / `{` / `    #region Fields` / `    int _a;` / `    #region Properties` / `    public int P => 1;` / `    #endregion` / `    #endregion` / `}`. `P` is reported against `Fields` ("The standard region 'Fields' should contain only field declarations.").
- Input 2: `#region IDisposable` around `#region Properties`, which holds a method. Not reported.

**Why it looks wrong:**
- The rule page says "Only members placed directly inside the region are considered". `P` sits directly inside `Properties`.
- The analyzer looks only at the top-level region around a member.

**Readings:**
- (a) Judge each member by the innermost region around it. Input 1 is not reported; the method in input 2 is.
- (b) Keep the current behavior and correct the page.
- (c) Do not check members of a nested region at all, as the page's two sentences together say. Neither input is reported.

## `SDD open questions: sdd/specs/analyzer/RH7309.md`

### Q1 — A comment above a moved region stays in place

**Touches:** RH7309-004.

**Inputs and real output:**
- A: `// The disposal logic` above `#region IDisposable`, and `// The state of the object` above `#region Fields`, in a class that implements `IDisposable`. After the fix, each comment sits above the other region.
- B: only `// The disposal logic` above `#region IDisposable`, the first block. After the fix, which moves `Fields` first, the comment sits above `#region Fields`.

**Readings:**
- (a) The comment lines directly above a `#region` travel with its block, as attached trivia does in SHARED-ORDER-002 and FMT-USING-008.
- (b) The fix is withheld. `AGENTS.md` (*Code-fix coverage*) asks a fix that would relocate a comment to withhold itself; SHARED-FIX-003 does not list relocation.
- (c) Keep the current behavior.

### Q2 — A region inside a nested enum counts as a region of the outer type

**Analyzer part: an open question. Fix part: decided by principle P1 (#912), SHARED-FIX-003.**

**Touches:** RH7309-001, RH7309-002, RH7309-004, SHARED-TYPEREGION-006. The same mechanism touches RH7311 (Q2 of its issue, link). RH7305 and RH7305A count the enum's region too, but that cannot change their verdict.

**Input:** `public class C : IDisposable` with `#region IDisposable` / `public void Dispose() { }` / `#endregion // IDisposable`, followed by `enum E` / `{` / `#region Values` / `A,` / `#endregion // Values` / `}`.

**Real output:**
- RH7309 reports `#region Values`.
- Its fix swaps the two blocks. It moves `A,` into the class body and `Dispose` into the enum, and the result does not parse.

**Why it looks wrong:** the code leaves out the regions of nested class, struct, interface, and record declarations, but not those of a nested enum, because an enum is not a `TypeDeclarationSyntax`.

**Readings for the analyzer part:**
- (a) The enum's regions belong to the enum, as for every other nested type. Nothing is reported.
- (b) Keep the current behavior.

### Q3 — The fix moves a region out of a field initializer

**Decided by principle P1 (#912), SHARED-FIX-003.** The analyzer part, a region in an initializer counting as a region of the type, is Q2 of the RH7303 issue (link, P11).

**Touches:** RH7309-004, SHARED-TYPEREGION-006.

**Input:** `using System;` / `public class C : IDisposable` / `{` / `    #region IDisposable` / `    public void Dispose()` / `    {` / `    }` / `    #endregion // IDisposable` / `    private int[] _a = new[]` / `    {` / `        #region Values` / `        1,` / `        #endregion // Values` / `    };` / `}`.

**Real output:**
- RH7309 reports `#region Values`.
- The fix swaps the two blocks. It moves `#region Values` / `1,` / `#endregion // Values` into the class body and `Dispose` into the initializer, and the result does not parse.

**Expected under SHARED-FIX-003:** no fix is offered for this input. The diagnostic follows the answer to the RH7303 question.

## `SDD open questions: sdd/specs/analyzer/RH7310.md`

### Q1 — The fix leaves a blank line next to a brace

**Candidate for "Decided by principle P10 (#912)".** The lead weighs it against P9. Same question as Q4 of the RH7303 issue (link).

**Touches:** RH7310-004.

**Input:** `class C` / `{` / `    int _a;` / `` / `    #region Methods` / `` / `    #endregion // Methods` / `}`.

**Real output:** `class C` / `{` / `    int _a;` / `` / `}`. `scripts/format.sh` then removes the blank line before `}`.

## `SDD open questions: sdd/specs/analyzer/RH7311.md`

### Q1 — The same description in an `#if` and an `#else` branch

**Touches:** RH7311-001, SHARED-TYPEREGION-006.

**Input:** `class C` / `{` / `#if DEBUG` / `    #region Fields` / `    int _a;` / `    #endregion` / `#else` / `    #region Fields` / `    int _b;` / `    #endregion` / `#endif` / `}`.

**Real output:** the second `#region Fields` is reported.

**Why it looks wrong:** in every configuration only one `Fields` region is compiled.

**Readings:**
- (a) Do not compare the regions of mutually exclusive branches.
- (b) Keep the current behavior: the editor shows both regions.

### Q2 — A region inside a nested enum is compared with the outer type's regions

Same mechanism as Q2 of the RH7309 issue (link).

**Touches:** RH7311-001, SHARED-TYPEREGION-006.

**Input:** `class C` / `{` / `    #region Values` / `    int _a;` / `    #endregion` / `    enum E` / `    {` / `        #region Values` / `        A,` / `        #endregion` / `    }` / `}`.

**Real output:** the enum's `#region Values` is reported as a repeat of the class's region (confirmed by running the analyzer).

**Readings:**
- (a) The enum's regions belong to the enum. Nothing is reported.
- (b) Keep the current behavior.

## For `shared/using-directive-ordering.md` (lead decides whether to reopen #916 or open a new issue)

### Q1 — An alias of a type that is not a name

**Touches:** SHARED-USING-004, SHARED-USING-005, RH7204, RH7207. Also the formatter: `UsingGrouping` delegates to `UsingDirectiveOrderingUtilities`.

**Input:** `using A = System.Text;` / blank line / `using B = int;` / blank line / `class C` / `{` / `}`.

**Real output:** RH7207 reports it, and the fix gives `using B = int;` / blank line / `using A = System.Text;`. A tuple target behaves the same way.

**Why it looks wrong:**
- The root of such an alias is `""`. Its group order key equals that of `System`, and the ordinal tie-break places it first, which goes against SHARED-USING-005.
- SHARED-USING-004 defines no root for a target that is not a name.
- The RH7204 and RH7207 specs leave the case out. RH7207-003 states only where the diagnostic is.

# Decisions needed

_None._
