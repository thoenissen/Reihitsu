Role: spec author
Commit: 14119f0
Verdict: spec written

Follow-up round for lead decision 3. My prompt named the change asked for, which is that recorded decision. It carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, `01-spec-author.md`, `03-spec-author.md`, `05-implementer.md`, and the code at `44e6f8c`.

## Changed requirements

No requirement text changed. Code owners and ownership documentation:

- FMT-STRUCT code owners (`formatter/structural-transforms.md` header and `sdd/specs/README.md`): added `AttributeListSplitTransform`, `AttributeListMergeTransform`, `AttributeListShapeUtilities`.
- SHARED-ATTR code owners (`shared/attribute-targets.md` header and `sdd/specs/README.md`): added `AttributeListMergeSeparators`, the Core enum the shared merge (`AttributeTargetUtilities.MergeAttributeListGroup`) takes. SHARED-ATTR-018 and FMT-STRUCT-029 both depend on it.
- FMT-BREAK code owners: unchanged. `AttributeTargetFormattingRewriter` and `AttributeTargetFormattingShared` keep placement and the accessor shape (FMT-BREAK-027, FMT-BREAK-032). No FMT-BREAK owner type was removed.
- `AGENTS.md` phase-ownership table:
  - The `StructuralTransformPhase` row now owns the shape of attribute lists that depends on no layout (one attribute per list on declarations; merged lists of one target on parameters and type parameters). Its guards name `AttributeListShapeUtilities.SplitAttributeLists`/`MergeAttributeLists`/`UsesMergedListShape`.
  - The `LineBreakPhase` row's "attributes" now names the placement of every list and the accessor list shape. Its guards name `AttributeTargetFormattingRewriter.StandsInOneLineProperty`.
- Change doc `sdd/changes/2026-11/attribute-list-structural.md`, *Code owners and phase ownership*: names the created types and the SHARED-ATTR owner.

`scripts/test.sh --project architecture`: Passed 92, Failed 0.

## Examples for the tests

_None new._

## Output questions

_None._

## Internal choices

1. `AttributeListShapeUtilities` is listed under FMT-STRUCT only, although the line-break phase's accessor shape (FMT-BREAK-032) also calls it. No type is a code owner of two specs today. The type lives under `StructuralTransforms/Utilities`, and its main callers are the two transforms. Alternative: list it under FMT-BREAK as well.
2. `AttributeListMergeSeparators` goes to SHARED-ATTR beside `AttributeTargetUtilities`, the type that consumes it. Alternative: FMT-STRUCT, whose FMT-STRUCT-029 drives the `None` value. I rejected that because the enum lives in Core with the shared merge.

## Follow-up candidates

_None._
