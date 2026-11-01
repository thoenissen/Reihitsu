Role: spec author
Commit: 43ce9cd001e161b8dfeca7c4bfb5a764a46da5b4
Verdict: change doc written

## Issue statements checked against `main` (43ce9cd)

- Holds: the line-break phase's `AttributeTargetFormattingRewriter` splits (`SplitAttributeLists`, which builds new lists in `BuildSplitAttributeLists`) and merges (`MergeAttributeLists` → Core's `AttributeTargetUtilities.MergeAttributeListGroup`), then places (`ApplyPlacement`). The issue's `ApplyListShape` does not exist; the entry is `FormatAttributeLists`. Statement corrected, not dropped.
- Holds by grep: no other rewriter under `Pipeline/LineBreaks/Rewriter` builds new tokens. The others rebuild lists of existing nodes.
- Holds: the B3 contract exists as FMT-STRUCT-026 (#1488 closed), and FMT-BREAK-031 records the merge's space as an exception "until the merge moves" (#1492).
- **Does not hold for accessors:** the issue's premise is that the shape can move with output unchanged. That is true for declarations, parameters and type parameters, where the shape (`AttributeTargetFormattingShared.ResolveListShapeMode`) depends on no layout. For accessor lists, `ShouldKeepAccessorListsSingleLine` reads `SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists` on the tree *after* the earlier line-break rewriters (`LineBreakAssignmentRewriter`, `PropertyLayoutLineBreakRewriter`, `DeclarationBraceLineBreakRewriter` run before it in `LineBreakPhase.CreateRewriters`). Measured with `scripts/format.sh` / `scripts/trace.sh`:
  - `int P { [A, B] get => 1; }` (one line as written) → split, accessor list expanded;
  - `int S { [A] [B] get; set; } =` + newline + `1;` (two lines as written) → `int S { [A, B] get; set; } = 1;` (merged);
  - `X P { [A, B] get; set; } = new X { A = 1 };` (one line) → expanded and split.
  A structural transform sees the written layout and would decide all three the other way. On the next run it would see the line-break layout and flip again. The only ways to keep the output are to leave accessor shape in the line-break phase or to have the structural phase predict that phase's layout. Hence `Internal choices` 1.
- The issue's FMT-BREAK-027 example: today's output matches the issue's "Desired Formatted Output" exactly (checked with `scripts/format.sh`).

## Changed requirements

- FMT-BREAK-027 — Attribute lists of declarations start their own line — changed (placement only; adds the FMT-BREAK-024 one-line-accessor exception and a coverage sentence for a list with a comment or directive between its brackets)
- FMT-BREAK-031 — A join writes the minimal separator… — changed (its recorded merge exception now names accessor merges, FMT-BREAK-032, and drops the issue link)
- FMT-BREAK-032 — The attribute lists of an accessor follow its accessor list's layout — added (clarification of existing behavior: the one-line merge was unstated)
- FMT-STRUCT-028 — A declaration's attribute list holds one attribute — added
- FMT-STRUCT-029 — The lists of one target on a parameter are merged — added
- SHARED-ATTR-013 — Why now cites FMT-STRUCT-028; spec intro and `Uses` updated
- `structural-transforms.md` — intro, one non-goal, `Uses`; `line-breaks.md` and `attribute-targets.md` — `Uses`
- Change doc: `sdd/changes/2026-11/attribute-list-structural.md` (Draft). Visible output changes: none.

`scripts/test.sh --project architecture` (spec and requirement filters): everything passes except `EveryRequirementOfAnEnforcedSpecIsCovered`, which names FMT-BREAK-032, FMT-STRUCT-028, and FMT-STRUCT-029 as uncovered. The test implementer adds those tests.

## Examples for the tests

FMT-STRUCT-028, FMT-STRUCT-029, FMT-BREAK-032, FMT-BREAK-027 (unchanged example). The negative cases of FMT-STRUCT-028 (`[A /* c */, B]`, `[unknown: A, B]`) are written in prose. Today's tests tagged FMT-BREAK-027 are in `OneLineClassBodyMembersFullPipelineTests` and `ParameterAttributeTargetLayoutTests`. They may need the new tags where they assert shape rather than placement.

## Output questions

_None._ The change doc lists no visible output change. Every example was taken from today's output on 43ce9cd.

## Internal choices

1. Accessor attribute-list shape stays in the line-break phase (FMT-BREAK-032). This drops part of the issue's first "Done when" item ("No line-break rewriter changes the token sequence"), which then holds for every owner except accessors. Alternatives: (B) the structural phase predicts the line-break phase's accessor-list layout, which keeps the output but adds a prediction (an architecture finding by `squad/README.md`); (C) decide on the written layout, which changes output and is not stable from one run to the next. Recommend A. Nothing visible is in question for A.
2. FMT-BREAK-027 keeps its ID as the placement requirement and is changed rather than retired, so the analyzer and shared links that cite it for placement stay valid.
3. The moved transforms follow FMT-STRUCT-026 with no exception: no line break between created lists, no space after the merged list or its commas, no elastic trivia. The code fixes' own output (SHARED-ATTR-017, SHARED-ATTR-018) stays as specified. Alternative: an exception for reusing the Core merge as-is, like the field split's.
4. Code owners: the new transform types do not exist yet, and `SpecConsistencyTests` rejects names of types that do not exist, so the FMT-STRUCT code-owner row in the spec and in `sdd/specs/README.md` is added once they exist. The `AGENTS.md` phase-ownership rows (StructuralTransformPhase: attribute-list split and merge; LineBreakPhase: attribute placement and accessor-list attribute shape) change at the same point. The change doc records both decisions.
5. FMT-STRUCT-028 leaves lambda attribute lists unspecified (coverage sentence). Today they are split and placed on their own lines, which looks wrong (Follow-up 1). The change doc still requires that the output stay unchanged for every input.
6. The transforms run through every entry point, as FMT-STRUCT-016 already states. FMT-PIPE-011 is unchanged.

## Follow-up candidates

1. **Lambda attribute lists are placed on lines of their own.** Reached by whole-document formatting. FMT-BREAK-027 / FMT-STRUCT-028. Input:
   ```csharp
   class C
   {
       void M()
       {
           var f = [A, B] (int x) => x;
       }
   }
   ```
   Real output (main):
   ```csharp
           var f = [A]
           [B]
           (int x) => x;
   ```
   Why it looks wrong: a lambda is an expression, not a declaration. The first list stays behind `=` and the rest start lines at the statement's column. Pass 2 of `trace.sh` changes the line-break and indentation phases and reverts them. Reading A (recommended): the lists stay on the lambda's line, as on a parameter (`var f = [A, B] (int x) => x;`; whether to split is part of the question). Reading B: today's output.
2. **An accessor attribute expands a one-line auto-property whose initializer the phase lays out over several lines** (FMT-BREAK-024). Input: `X P { [A] get; set; } = new X { A = 1 };`. Real output: the accessor list is expanded (`{` / `[A]` / `get;` / `set;` / `}` on separate lines, then `= new X` with the initializer). Without `[A]`, `X P { get; set; } = new X` stays on one line with the initializer below it. Why it looks wrong: FMT-BREAK-024 says an accessor list with an accessor attribute stays on its line when the declaration is on one line. Reading A (recommended): `X P { [A] get; set; } = new X` with the initializer laid out as without `[A]`. Reading B: today's output, with FMT-BREAK-024 reworded. Through FMT-BREAK-032 this also decides whether `[A, B]` on such an accessor is split.

## Files

- `/home/user/Reihitsu/sdd/changes/2026-11/attribute-list-structural.md`
- `/home/user/Reihitsu/sdd/specs/formatter/structural-transforms.md`
- `/home/user/Reihitsu/sdd/specs/formatter/line-breaks.md`
- `/home/user/Reihitsu/sdd/specs/shared/attribute-targets.md`
- `/home/user/Reihitsu/sdd/specs/requirement-ids.txt`
