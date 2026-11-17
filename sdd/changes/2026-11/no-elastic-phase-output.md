# No phase adds elastic trivia

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-034 (added), FMT-STRUCT-026 (changed: its last sentence on elastic trivia now points to FMT-PIPE-034), FMT-BREAK-032 (clarified: the commas of an accessor's merged attribute list carry no trivia), FMT-BREAK-031 (clarified: its recorded exception covers no space inside the merged list), the non-goals of `pipeline.md` (extended) |
| Depends on | [Every phase returns a tree its text parses back to](../2026-10/phase-tree-round-trip.md) |

## What

No formatter phase adds elastic trivia (FMT-PIPE-034). Elastic trivia stands in a phase's output only where the phase keeps trivia of its input that is already elastic. For every input this repository's callers pass to `FormatDocumentAsync` and `FormatSyntaxTree` (`AGENTS.md`, *Supported inputs*), the tree these entry points pass through the phases is parsed from text, so no phase output carries any elastic trivia at all.

The line-break phase's merge of an accessor's attribute lists writes every comma between two attributes of the merged list with no trivia, and the horizontal-spacing phase writes the space behind it (FMT-BREAK-032).

FMT-STRUCT-026 already required this of the tokens the structural transforms create and said explicitly that it decided nothing for the later phases. Its last sentence now points to FMT-PIPE-034, which states the rule once for every phase.

Elastic trivia that a node passed to a node-level entry point already carries, such as a node a code fix built through `SyntaxFactory`, is not decided: FMT-PIPE-034 requires only that the phases add none, not that they keep or drop what the node brings along. The attribute-list merge as the code fixes call it is no phase and is not covered; SHARED-ATTR-018 keeps deciding its output.

## Why

An elastic marker is whitespace to the spacing phase but no character of the text, so a tree that carries one is spaced differently from its own text wherever no spacing rule decides the gap (FMT-SPACE-002). FMT-PIPE-021's Why rules that difference out, but its comparison does not compare whitespace and cannot see it. The [phase round-trip change](../2026-10/phase-tree-round-trip.md) fixed the structural transforms and recorded the pipeline-wide rule as a follow-up candidate; this change is that follow-up (thoenissen/Reihitsu#1516).

Today the formatter test fixtures show elastic trivia in phase output in one place: the commas the line-break phase writes when it merges an accessor's attribute lists (FMT-BREAK-032). They stay in the output of every later phase, up to the cleanup phase. Their gaps are decided by spacing rules (FMT-SPACE-004, FMT-SPACE-008), so the output does not change; it is correct because those rules happen to cover the gaps, not because the tree and its text agree.

## Visible output changes

None — no output an end user sees changes. The final text of every phase is the text it has today; only trivia without a character of the text is removed from the tree.

- The formatter's output is unchanged. Example (FMT-PIPE-034), formatted the same before and after:

```csharp
class C
{
    int P { get; [param: A] [param: B] set; }
}
```

```csharp
class C
{
    int P { get; [param: A, B] set; }
}
```

- Code-fix results are unchanged. The code fixes that format the affected node through the formatter (every fix that formats a property, indexer, or event whose accessor list carries several `param:` or `typevar:` lists, or several lists of one target on the declaration's line) receive the same text as today. The attribute-list merge fixes (RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5524, RH5526, RH5528, RH5529, RH5531) do not run a phase; their output stays as SHARED-ATTR-018 states, `[A, B]` above.
- No diagnostic, rule page, or CLI behavior changes.

## Decisions

### The rule is stated as "adds", not "carries"

FMT-PIPE-034 forbids a phase to add elastic trivia rather than forbidding elastic trivia in its output. For the document entry points the two are the same for every input this repository's callers pass, because that tree is parsed from text and carries none. For the node-level entry points, "carries" would require the phases to strip elastic trivia a code fix's node brings along, which would change what the spacing phase does with those gaps in code-fix results — a visible output the issue does not ask about. Stated as "adds", the requirement covers every entry point without deciding that.

### The code fixes' merge keeps its output

The accessor merge of the line-break phase and the merge of the attribute-list code fixes share one implementation and, until now, one set of separators (FMT-BREAK-031, [attribute-list change](attribute-list-structural.md)). FMT-PIPE-034 applies to the phase's merge only. The code fixes' output is decided by SHARED-ATTR-018 and does not change; whatever the phase's merge writes in place of an elastic comma must leave that output as it is.

### The phase's merge writes its commas without whitespace

In place of the elastic comma, the line-break phase's accessor merge writes each comma between two attributes of the merged list with no trivia on either side, a comma the first list already held included, and leaves the space behind it to the horizontal-spacing phase (FMT-SPACE-008). FMT-BREAK-032 states it, and FMT-BREAK-031's recorded exception — the spaces the shared merge writes behind the merged list's `]` and behind a token it leaves touching a joined token — stays as it is and does not grow to these commas. Writing an explicit space behind each comma gives the same final output, but it would make the line-break phase decide a same-line space that FMT-SPACE owns and widen the recorded exception. The code fixes' merge is not affected: SHARED-ATTR-018 keeps deciding its output, `[A, B]`.

### One pipeline requirement, not one per phase

The rule is the same for every phase and has the same reason, so it is stated once in the pipeline spec, as FMT-PIPE-021 and FMT-PIPE-022 are, and FMT-STRUCT-026 points to it instead of repeating it.

### The issue's goals

thoenissen/Reihitsu#1516 is `agent-filed`, so its goals are proposals; they are adjusted on the evidence of the run:

- "No phase returns elastic trivia" becomes "no phase adds elastic trivia" (*The rule is stated as "adds", not "carries"*): for the document entry points the two are the same, and "returns" would make the phases strip elastic trivia from the nodes code fixes pass, a code-fix output change nobody asked for.
- The per-phase round-trip check asserts the rule on every formatter test fixture: kept. The fixtures are parsed from text, so there "adds" is checked as "carries none".
- The formatter test suite keeps its outputs: kept, and widened to the code-fix suites — the attribute-list merge fixes (SHARED-ATTR-018) and the fixes that format a member through the formatter keep their results too.
- The issue's example, a created `get=>""` that formats to `get=> ""`, is not used: FMT-STRUCT-026 already removed that source, and FMT-SPACE-017 decides the spaces around `=>`. The accessor example of FMT-PIPE-034 replaces it; the commas of the line-break phase's accessor merge were the only elastic trivia left in phase output on the formatter fixtures.

## Alternatives considered

- **No phase output carries elastic trivia, for every entry point** — rejected: it would make the phases strip elastic trivia from the nodes code fixes pass, which changes how the spacing phase treats those gaps in code-fix results without a case that asks for it.
- **Leave the rule to each phase spec** — rejected: the same rule in thirteen places can drift apart, which an architecture finding names as a defect.
- **The phase's merge writes an explicit space behind each comma** — rejected: see *The phase's merge writes its commas without whitespace*.
- **Leave it unspecified, since no output changes** — rejected: whether an elastic marker changes the output depends on whether a spacing rule covers its gap, so a phase that adds one is correct only by luck, and FMT-PIPE-021's comparison cannot catch it.
