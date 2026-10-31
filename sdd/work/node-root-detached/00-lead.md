Role: lead
Commit: ccb6669
Verdict: lane claimed; implementer done

# Run: thoenissen/Reihitsu#1489

- Issue: thoenissen/Reihitsu#1489 — FMT-PIPE: node-level roots are detached, and their input facts are specified plain values (package B4 of thoenissen/Reihitsu#1482). Not `agent-filed` (filed before the label existed; goals treated as binding).
- Slug: `node-root-detached`
- Branch: `claude/sleepy-goldberg-pli2uv` (the session's designated branch; the harness allows pushes only there).
- Draft PR: thoenissen/Reihitsu#1520
- Base: `origin/main` `e7853b6` at the claim; `origin/main` `c24bc1d` (new squad process) merged at `ccb6669`. The run follows the new process from here; the `squad/decisions/` entries it had written were removed at the maintainer's request, and their content lives in the sections below.

## Lane

Behavior change: the issue requests changed behavior of the node-level entry points and requires a change doc. Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing. None skipped.

## Maintainer clarifications

- Only end-user-visible output reaches the maintainer (now the rule on `main`, #1528).
- The node-level entry points are used only by this repository's code fixes; a node-level output no code fix reaches is not a visible output.
- No `squad/decisions/` entries in this run.
- The formatter's public methods (`FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`, and the rest) are not a public API — like every assembly of the repository (`squad/README.md`). They must work for the inputs this repository's own callers — the code fixes, the CLI, whole-document formatting — pass, not for every input their signature admits. A case no such caller reaches is neither a defect nor a follow-up. The maintainer wants this rule written down in this PR (process text in `AGENTS.md` and `squad/`, with a process change doc).

## Maintainer answers

1. Change doc (first version): approved as written.
2. Member-body target whose `{` stays on the declaration's line: follow-up thoenissen/Reihitsu#1521, behavior change (write back the line break).
3. Node-in-document outputs that changed because decisions read outside a detached root: amend the change doc so FMT-PIPE-024 carries plain-value facts for them.
4. Two owners of the leading trivia of a target that does not start its line: narrow fix in this run (the `else if` fact keeps the comment); the structural change is folded into #1521 (widened).
5. RH5405 comment drop on `main`: separate follow-up thoenissen/Reihitsu#1527.
6. Depth of a target behind a multi-line comment (zero offset): the indentation phase's depth; those outputs change.
7. Amended change doc: approved as written.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | test implementer | Column of the elements of an assigned collection initializer under `FormatNodeInDocumentAsync` (same column as `{`) | Not changed in this run; follow-up candidate | The run does not change this output | Output identical before and after the run | — |
| 2 | test implementer | Column of `/* note` before an embedded `if` target | Left unpinned by tests | No spec gives a derivable column; the run does not change it | Output identical before and after | — |
| 3 | test implementer | Uncovered list-element, `and`/`or` pattern, and context-entry-point cases | Left as recorded in `04-test-implementer.md` for the auditor | Covered by the tab-equivalence rule; reasons in the handover | No asserted output changes | — |
| 4 | implementer | 88 `FormatNodeInDocumentAsync` outputs differ from `main`, wider than the change doc says | Spec author corrects the change doc's What | They follow FMT-PIPE-023/024 (a target with a tab gets the output of the same target with spaces) | Node-level outputs reach users only through code fixes; no analyzer or code-fix test changed | pending |
| 5 | implementer | One-line-property flag read more narrowly than the spec row (root holds no line break) | Spec row aligned with the implementation | Keeps today's output for those targets | Output equals `main` | pending |
| 6 | implementer | FMT-PIPE-024 sentence "every phase answers what stands in front of the root's first token from the preceding-token fact" not implemented by some line-break and spacing readers | Narrow the sentence to the readers that use the fact; the remaining approximations become a follow-up candidate | No output difference found; implementing adds about eight production paths without a failing test | No output difference | pending |
| 7 | implementer | Architecture finding: chain outermost-ness has two owners for an inner chain-part target (line-break phase reads the fact, `MethodChainAlignmentContributor` does not) | Narrow: no change in this run; structural change is a follow-up candidate; change doc names the case as out of scope | The chain code fixes (RH5112, RH5201) pass the outermost chain node (`FluentChainCodeFixHelper.FindChainNode`), so no code fix formats an inner chain part | Not reachable through any code fix; whole-document formatting unaffected | pending |
| 9 | lead | The six follow-up candidates of the handovers (initializer column, preceding-token approximations, chain outermost-ness, Core's one-line-property copy, Core's depth rule for attribute placement, blank line above an embedded target) | Dropped; none is filed | No code fix, CLI path, or whole-document formatting was shown to reach any of them, and the maintainer wants no follow-ups for cases nobody needs | Nothing filed, nothing changed | — |
| 8 | implementer | `ReihitsuFormatterHelpers.ComputeBaseIndentLevel` wrapper has no production caller, only its five tests | Adjacent fix: remove the wrapper and its tests | "Done when": the formatter no longer calls `ComputeBaseIndentLevel` | Dead code; no output | pending |

## Follow-up candidates

None. Every candidate the handovers named was dropped under the maintainer's clarification (decision 9).

Already filed during the run: thoenissen/Reihitsu#1521, thoenissen/Reihitsu#1527.
