Role: spec author
Commit: bdcb9bf
Verdict: spec written

Repair round 2, for the spec and change-doc side of audit findings 1–4 (`11-auditor.md`) as lead decisions 16–19 settle them. My prompt named the change asked for, which is those recorded decisions; it carried no intended fix, suspected cause, or hoped-for verdict beyond them. I worked from `00-lead.md`, `01-`, `03-`, `06-`, `09-spec-author.md`, `11-auditor.md`, the specs, and `AttributeTargetFormattingShared` at `bdcb9bf`. Outputs I cite as `main`'s are the auditor's measurements; I measured none myself. The change doc stays `Accepted` and lists no visible output beyond decisions 4 and 18 and maintainer answer 1.

## Changed requirements

- FMT-STRUCT-028 — A declaration's attribute list holds one attribute — changed (decisions 17, 18):
  - whitespace and line breaks behind the original `[` are dropped, so the first created list holds nothing between `[` and its attribute (`[⏎ A, B]` → `[A][B]`); only what stands in front of `]` stays outside the requirement;
  - a `param:`/`typevar:` list is the exception to "a known non-default specifier is split like one without": it keeps the parameter shape on every owner and is never split;
  - Why extended; the second example gains `void P()` with a line break behind `[`.
- FMT-STRUCT-029 — The lists of one target on a parameter are merged — changed (decisions 16, 17, 19):
  - new paragraph: `param:` lists and `typevar:` lists are merged (and never split) on the compilation unit, a type, a delegate, a member, an enum member, or a local function too; the other lists of such an owner are split by FMT-STRUCT-028; accessors are FMT-BREAK-032's; lambdas stay unspecified, as in FMT-STRUCT-028;
  - the kept line break: the stretch now runs from the first list's `[` (where the comment/directive guard starts) to the token behind the last list. A line break directly behind that `[` stays where it is; otherwise any line break in the stretch — inside the first list included — gives one line break between the merged list and the token behind the last list; where the merged list ends up on a non-parameter owner is not specified;
  - Why extended; the line-break example gains `void S([A,⏎ C] [B] int x, int y)` → `void S([A, C, B] int x,⏎ int y)` (the auditor's `main` output); a new example shows a method with `[param: A] [param: B] [typevar: C, D] [E, F]` (CS0657 input).
- FMT-BREAK-032 — The attribute lists of an accessor follow its accessor list's layout — changed (decisions 16, 17): in an accessor list over several lines, `param:`/`typevar:` lists are not split and their lists of one target are merged, as FMT-STRUCT-029 states; in either layout an accessor merge keeps no line break and its space is FMT-BREAK-031's exception (that exception's text already covers every accessor merge, unchanged). New example: the setter `[param: A] [param: B] set => _v = value;` in a multi-line accessor list → `[param: A, B] set => _v = value;` (auditor finding 1's `main` output).
- FMT-BREAK-027 — clarified: it leaves the placement of a `param:`/`typevar:` list on an owner other than a parameter or type parameter unspecified. `main` keeps such a list on its owner's line (`AttributeTargetFormattingShared.ResolvePlacementMode`), contrary to the requirement's "each list of an accessor … MUST start its own line"; see internal choice 2.
- Change doc `sdd/changes/2026-11/attribute-list-structural.md`:
  - *What*: the split bullet covers what stands behind `[`; new bullet "`param:` and `typevar:` lists keep the parameter shape on every owner"; the merge-line-break bullet names the stretch from the first `[`; the FMT-BREAK-027 bullet names the placement gap;
  - *Visible output changes*: change 1 is retitled "A line break behind the comma, or behind the `[`, settles in one pass" and gains the `[⏎ A, B] void P()` case (`main` first pass `[        A]⏎[B]`, after `[A]⏎[B]` in one pass; same for accessors and lambdas), as decision 18 asks; the closing sentence keeps only `[A, B⏎]` outside the change;
  - *Decisions*: "A created list holds only its attribute" covers the `[` case; "The merge keeps one line break of its stretch" names the first list's inside and the `[` boundary; new decision "A `param:` or `typevar:` list keeps the parameter shape on every owner".

Requirement IDs unchanged. `scripts/test.sh --project architecture`: Passed 92, Failed 0.

## Examples for the tests

- FMT-STRUCT-028: the `void P()` row (`[⏎        A, B]`) — final output in one pass, and the phase output `[A][B]`; also the accessor (`[⏎ C, D] get;` in a multi-line accessor list) and lambda members the change doc names.
- FMT-STRUCT-029: `void S([A,⏎ C] [B] int x, int y)`; the auditor's further members `[A⏎ , C] [B]`, `[A⏎ ] [B]` on a method, a record parameter list, and a local function; the CS0657 method example (`[param: A, B]`, `[typevar: C, D]`, `[E]`⏎`[F]`) and the auditor's other non-accessor owners (property, local function, delegate, class). The placement of these lists on a non-parameter owner is unspecified, so tests for them assert `main`'s output as characterization.
- FMT-BREAK-032: the setter example; the auditor's `init`, indexer setter, and event `remove` members in a multi-line accessor list.

## Output questions

_None._ Decision 18 settles the one open output (`[⏎ A, B]`), recorded in the change doc as part of visible change 1.

## Internal choices

1. Where the merge of an accessor's `param:`/`typevar:` lists runs: FMT-BREAK-032 (the accessor merge of the line-break phase, keeping no line break, with FMT-BREAK-031's space), not FMT-STRUCT-029, so it matches `main`'s merge in that phase and the auditor's suggested change. Alternative: the structural merge on accessors too, which keeps a line break and could change where `main` puts the list.
2. FMT-BREAK-027 now leaves the placement of a `param:`/`typevar:` list on a non-parameter owner unspecified instead of stating it. The setter example of FMT-BREAK-032 shows `main`'s placement, but I have measured no method or type case and did not want to write placement I have not seen. Alternative: state that such a list stays on its owner's line, as `ResolvePlacementMode` says.
3. Lambdas are left out of FMT-STRUCT-029's `param:`/`typevar:` owners, consistent with FMT-STRUCT-028 and decision 10. The implementation may still merge them as `main` did (decision 16 says every owner).
4. Where the kept line break stands on a non-parameter owner is not specified (as decision 9 left it open for the final output).

## Follow-up candidates

1. Placement of a `param:`/`typevar:` attribute list on an owner other than a parameter or type parameter is unspecified (FMT-BREAK-027 now says so). Spec gap only, no output in question that I know of: `main` keeps such a list on its owner's line. Reachable through whole-document formatting (`int P { get; [param: A] set; }` over several lines).
