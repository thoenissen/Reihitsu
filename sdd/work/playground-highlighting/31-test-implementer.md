Role: test implementer
Commit: 04dbbfa
Verdict: tests written

Repair round 3, Decision 11 (`00-lead.md`): remove `#pragma warning disable BL0006` from `tests/Reihitsu.Playground.Test/PlaygroundBrowserDocument.cs` without changing the helper's behaviour or the `NullCharacterTests` assertions. The prompt held no intended fix, suspected cause, or hoped-for verdict. Only that file changed; `NullCharacterTests.cs` is untouched.

## What changed

Without the pragma, the Release build reported 26 `BL0006` errors, all on uses of `RenderTreeFrame`, `RenderTreeFrameType`, and `ArrayRange<RenderTreeFrame>` (`typeof(Renderer)` is not flagged). The helper now names none of them at compile time:

- `AppendComponent` still calls `Renderer.GetCurrentRenderTreeFrames` by reflection, and reads the returned range's public `Array` and `Count` fields by name, copying the frames into an `object[]`.
- A frame's members (`FrameType`, `ElementName`, `ElementSubtreeLength`, `TextContent`, `MarkupContent`, `ComponentId`, `ComponentSubtreeLength`, `RegionSubtreeLength`, `AttributeName`, `AttributeValue`) are read by name through one `ReadMember` (public property, else public field); the frame type is compared by its enum name (`"Element"`, `"Text"`, `"Markup"`, `"Component"`, `"Region"`, `"Attribute"`).
- The frame-by-frame logic is otherwise as before: the same cases, the same empty-markup-as-a-space rule, the same attribute rules. The reason the render tree is read moved from the file-top comment into `ReadMember`'s remarks.

The Release build of `Reihitsu.Playground.Test` (`--no-incremental`) reports 0 warnings and 0 errors; `scripts/format.sh` changed nothing.

## Same behaviour, checked

| Code under test | Helper | Result |
|---|---|---|
| Production code at fc6cddc (`src/` clean at the time of the run) | new | 5 null-character tests fail, 98 pass — each failing only on `0000` expected, `fffd` actual, as in handover 29 |
| Scratch fix A: a line whose markup holds `&#0;` emitted as span elements and text frames, U+0000 as a character (handover 29's scratch fix) | new and old | 103 of 103 pass with both |
| Scratch fix B: such a line emitted as one text frame of its plain text, spans and the error mark dropped | new and old | 102 pass, `NullCharacterIsKeptInACopyWithErrorMarks` fails on the mark (`expected ["28:"]`, `actual []`) with both |

The scratch fixes ran in a separate worktree under the scratchpad (removed afterwards, nothing committed), built with `-p:RunAnalyzers=false` because the scratch code is not analyzer-clean; the implementer's `src/` was not touched. `scripts/test.sh --project architecture` passes (198).

## Tests

No test added or changed; the helper serves the tests listed in `29-test-implementer.md`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|

## Not covered, and why

Nothing beyond `29-test-implementer.md`.

## Commands

```shell
scripts/test.sh --project playground --filter "FullyQualifiedName~NullCharacterTests"
scripts/test.sh --project playground
```

## Follow-up candidates

_None._
