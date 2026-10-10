# Working conventions for the formatter specs (#902)

Temporary working doc. Delete `sdd/work/902-formatter-specs/` before the PR is marked ready.

Read first: `sdd/README.md`, `sdd/templates/spec.md`, `sdd/specs/formatter/pipeline.md` (the finished reference spec — copy its style).

## Files, prefixes, owners of the writing

| File | ID prefix | Phase / policy |
|---|---|---|
| `sdd/specs/formatter/pipeline.md` | `FMT-PIPE` | done |
| `sdd/specs/formatter/structural-transforms.md` | `FMT-STRUCT` | `StructuralTransformPhase` |
| `sdd/specs/formatter/region-formatting.md` | `FMT-REGION` | `RegionFormattingPhase` |
| `sdd/specs/formatter/documentation-comments.md` | `FMT-DOC` | `DocumentationCommentFormattingPhase` |
| `sdd/specs/formatter/using-directives.md` | `FMT-USING` | `UsingDirectiveOrderingPhase` |
| `sdd/specs/formatter/blank-lines.md` | `FMT-BLANK` | `BlankLinePhase` |
| `sdd/specs/formatter/line-breaks.md` | `FMT-BREAK` | `LineBreakPhase` |
| `sdd/specs/formatter/switch-case-braces.md` | `FMT-SWITCH` | `SwitchCaseBracePhase` |
| `sdd/specs/formatter/horizontal-spacing.md` | `FMT-SPACE` | `HorizontalSpacingPhase` |
| `sdd/specs/formatter/indentation.md` | `FMT-INDENT` | `IndentationPhase` |
| `sdd/specs/formatter/raw-string-alignment.md` | `FMT-RAW` | `RawStringAlignmentPhase` |
| `sdd/specs/formatter/cleanup.md` | `FMT-CLEAN` | `CleanupPhase` |
| `sdd/specs/formatter/line-endings.md` | `FMT-EOL` | `LineEndingNormalizationPhase` |
| `sdd/specs/shared/blank-line-spacing.md` | `SHARED-BLANK` | `BlankLineSpacingPolicy` (+ the Core types it uses), only the rules analyzer and formatter share |
| `sdd/specs/shared/using-directive-ordering.md` | `SHARED-USING` | `UsingDirectiveOrderingSafety`, `UsingDirectiveOrderingUtilities`, `UsingDirectiveOrderingGroup` |
| `sdd/specs/shared/fluent-chain-layout.md` | `SHARED-CHAIN` | `FluentChain`, `FluentChainUtilities` and friends, only if the line-break author confirms it is a real shared policy |

Do not edit `pipeline.md`, `sdd/specs/README.md`, or another author's file. The lead adds the index rows.

## Format rules (from `sdd/README.md`, applied)

- Header table exactly as the template: `ID prefix`, `Status` (`Active`), `Code owners`, `Uses`.
- Code owners: type names only, in backticks, no paths, no line numbers. Every name must exist — check with `rg -n "(class|record|struct|enum|interface) <Name>\b" src`.
- `Uses`: IDs this spec relies on, e.g. `FMT-PIPE-002`, `SHARED-BLANK-001`. `—` when none.
- One or two sentences of scope under the header, then `## Non-goals`, then `## Requirements`.
- One requirement per `### <ID> — <Short title>` heading, numbered from `-001` without gaps.
- Normative sentence first, `The formatter MUST …` / `MUST NOT` / `SHOULD`. Then `Why:` (always). Then `Example:` with two ```` ```csharp ```` blocks: input first, output second — no `// before`/`// after` comment inside the blocks; the block content is the exact harness input and output. For a guard (input left unchanged), give one block followed by "is left unchanged." Labels, when needed, go outside the blocks as plain text (see FMT-PIPE-008).
- Do not restate the pipeline invariants (non-destructive, idempotent, syntax-error/generated skip, line-ending detection) — reference `FMT-PIPE-00x` in `Uses` or in a sentence.
- Describe what the phase decides at the level a maintainer needs to judge "is this a bug?": the canonical shape, and every guard that makes the phase leave code alone (comments, directives, disabled text, unsafe rewrites). Not a catalog of every test case. Typical size: 6–20 requirements per phase.
- Name a type in prose only when it helps navigation; behavior belongs in the sentence.
- Terminology: "the formatter" (surface), "phase", "line break", "blank line", "trivia", "directive", "disabled text", "documentation comment" (`///`), "comment", "token", "indentation unit" (four spaces).
- English only. No GitHub issue or PR numbers anywhere in a spec.

## Every example is verified — never invented

Harness (already built):

```shell
H=/tmp/claude-0/-home-user-Reihitsu/df219eee-70d1-5bb8-8f2c-2e5e070f8df2/scratchpad/fmt/run.sh
mkdir -p <your-scratch-dir>; cd <your-scratch-dir>   # use a fresh dir under the scratchpad, e.g. .../scratchpad/cases/<phase>
# write one input per file: case01.cs, case02.cs, ...
$H .            # formats every *.cs, writes *.out.cs, prints the output
```

- Output line: `=== file: CHANGED|UNCHANGED, idempotent|NOT-IDEMPOTENT; phases: <phases that changed the text>`, then the formatted text (`␍` marks `\r`, `→` marks a tab).
- File name markers: `x.lang6.cs` parses as C# 6 (any number); `x.node.cs` formats only the first member of the first type through `FormatNodeInDocumentAsync`.
- The full pipeline runs, so write inputs that are already formatted except for the behavior under test, and check that the `phases:` list contains your phase (ideally alone, `Cleanup` aside). The formatter removes the final newline of a file (FMT-CLEAN) — do not let that distract you.
- Copy input and output into the spec byte for byte (apart from the final-newline). If a block cannot be shown faithfully in Markdown (trailing whitespace, tabs), describe the input in prose and say so.
- If the formatter's real output looks wrong, NOT-IDEMPOTENT, or contradicts the code's comments: do NOT write it into a requirement as intended behavior and do NOT write the behavior you think is right. Leave the requirement out (or specify only the part that is clearly right) and record an open question.

## Open questions

Write them to `sdd/work/902-formatter-specs/questions/<file-stem>.md`, one `## Q<n> — <title>` each, with: the exact input block, the exact output block, the phase list from the harness, why it looks wrong, and which requirement (if any) it touches. The lead copies them into the PR description.

## Shared policies

The policies analyzer and formatter share live in `sdd/specs/shared/`. A phase spec references a shared requirement by ID and does not restate it. A shared spec states the policy once, surface-neutral ("Analyzer and formatter MUST …"), names the Core types as code owners, and lists the analyzer rules that use it in prose. Its examples are verified through the formatter the same way.

## Your report back to the lead

- Files written, requirement count per file, and every ID with its one-line title.
- Every external ID you reference.
- Open questions written (count, titles).
- Anything you believe belongs in another phase's spec or in `shared/` (the lead routes it).
