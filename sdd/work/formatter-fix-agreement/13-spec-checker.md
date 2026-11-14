Role: spec checker
Commit: b0d712c
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Repair round 1; scope: the delta 01672a1..HEAD (pipeline.md, the #1529 change doc, FluentChain, FluentChainBoundary, one deleted test), plus the earlier verdict for the unchanged rest.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. ok: the change doc stays Accepted, lists its Visible output changes, gains FMT-PIPE-024 under Affected requirements and a Decisions subsection for the deletion (no output change, so no new maintainer answer); `node-root-detached.md` untouched as history; no task list.
2. ok: the FMT-PIPE-024 *Conditional-access branch* row is gone and the SHARED-CHAIN-001 bullet names only the chain-continuation fact; it matches the diff (flag, constructor parameter, `From` argument and `IsConditionalAccessBranch` deleted). `rg` finds no remaining mention of the fact in `src`, `sdd/specs`, `documentation`; the only mentions are the history doc and the change doc.
3. ok: no ID added, changed, or removed in the round (FMT-PIPE-024 changed in place).
4. ok: `FluentChainBoundary` still exists and stays a code owner of `pipeline.md`; `sdd/specs/README.md` unchanged and valid.
5. ok: no rule page affected.
6. ok: no open output question; the follow-up candidate stays in `00-lead.md`.
7. ok: `scripts/test.sh --project architecture`: Passed 117, Failed 0 of 117. The deleted test was the only one tagged for the removed fact; FMT-PIPE-023/024 keep other tagged tests (`11-test-implementer.md`).
8. ok: no test added in the round; the earlier verdict for tagging stands.
9. ok: `sdd/work/` holds only this run's directory.
