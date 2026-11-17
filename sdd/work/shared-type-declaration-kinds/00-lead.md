# Lead — shared-type-declaration-kinds

- Issue: thoenissen/Reihitsu#1788 — "Analyzers, Core, formatter: the type-declaration kinds a rule selects come from one shared set". Label `agent-filed`: its goals, options, and "Done when" are proposals.
- Slug: `shared-type-declaration-kinds`
- Branch: `claude/squad-1788-96lw0x` (session-designated). Draft PR: thoenissen/Reihitsu#1841.
- Lane: **bug fix**. The issue text says "Lane: tooling", but every path it names is production (`src/Reihitsu.Analyzer`, `src/Reihitsu.Core`, the formatter) or test (`src/Reihitsu.ArchitectureTests`); the lane check classifies production without a change doc as bug fix. The issue states that no output changes, so no change doc is expected. Either of the issue's options fits this lane (an architecture test alone would need only the tests lane, which bug fix is wider than).
- Roles run: skeptic (premise, reachability, which option), test implementer, implementer, cleaner, spec checker, auditor, validator. Spec author skipped unless a clarification is needed.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane, given the issue says tooling | Bug fix | Named paths are production/test, not tooling; no output change planned | Lane choice is internal | claim |
| 2 | Skeptic | Which option of the issue to build | Neither: drop the shared-set goal, the "one edit" goal, and the exclusion-table architecture test; build one canary test in `Reihitsu.ArchitectureTests` that pins every type-declaration syntax type and its SyntaxKinds as Roslyn has them, keeping RH5404's existing canary | Skeptic 01: no single kind set exists (at least six different sets, each matching its spec); an exclusion table would repeat every spec's list a third time; the canary catches the event that caused the issue (a Roslyn bump adding a kind, also on an existing type) | Test-only; no production, spec, or rule-doc change | e6c97ce |
| 3 | Skeptic | PR link | `Closes #1788`, with the departure from the issue stated in the Summary | The issue's purpose — a new type-declaration kind does not slip through unnoticed — is met by the canary | Internal | e6c97ce |
| 4 | Skeptic | Implementer and spec author | Skipped: the diff is test-only and covers existing behavior; no spec clarification needed | Lane bug fix admits a tests diff | Internal | — |

## Follow-up candidates

From `01-skeptic.md` (each carries an output question, written there):
1. RH5108 does not report class/struct primary-constructor parameter lists that the formatter joins.
2. RH5110 does not report `record struct` constraint clauses that the formatter breaks.
3. Formatter leaves extension-block braces where they are (`DeclarationBraceLineBreakRewriter` lacks `ExtensionBlockDeclarationSyntax`; FMT-BREAK-002 silent on extension blocks; RH5402/RH5404 do not report).
4. RH7103 and other member-order rules do not check extension-block members.

## Worklist / next step

- Skeptic done (01, a070ff2). Test implementer done (02, e6c97ce). Implementer skipped (decision 4). Lane check: confirmed — claimed bug-fix, diff needs only tests. Cleaner clean (03). Spec checker consistent (04). Auditor pass (05) on audited SHA 4e7be9f1d0d5c340f538422456c440c03a198f2c. Issue manager (07): created #1843, #1844, #1845; extended #1785. Next: validator, then note + publish.
