# Coordinator state

Active milestone: **1.5.0** (milestone 7). Branch: `coordinator`. Theme: size and complexity metrics (RH2008/RH2111 defaults, RH2009), member-access chains and the joins of attached parts, expression-bodied and get-only members, unsupported shapes FMT-UNSUP-013..016, formatter performance.

Milestone numbers: 1.5.0 = 7, 1.6.0 = 14, 1.7.0 = 15, 1.8.0 = 16, 1.9.0 = 17, 1.10.0 = 18, 1.11.0 = 19, vNext = 2, Maintenance = 4. Released: v1.4.0.

## Decisions

- 2026-11-21 · #1913 · closed as not planned: the build catches it once 1.5.0 is bumped.
- 2026-11-21 · #1915 → 1.5.0 (RH2009 roll-out: scan the repository, test the default limit 15, fix or propose a new default, enforce it; runs alone); #1912 → 1.6.0; #1914 → 1.9.0 (with #1864); #1916 → 1.7.0.
- 2026-11-21 · #1906 · squad process changes accepted (wording lane, coverage-first tests, local self-hosting rows, fewer pushes); PR subscription only until the final commit's CI is decided. Merged as PR #1908.
- 2026-11-18 · #1865 · binding scope correction: default limits only, no custom limits, fix every violation, test the defaults; bad defaults → output question with new defaults.

Earlier decisions of 1.5.0 live on their issues.

## Plan

Open in 1.5.0: #1915.

Runs in progress: `/squad #1915` (claim PR #1917, branch `claude/gifted-hawking-3q8b4p`), alone.

Start now: nothing.

After merges:
- after #1915 merges → `/coordinator check` (maintainer starts it). Scope of the check — what 1.5.0 ships: RH2008/RH2111 with new defaults and repository enablement, RH2009, RH3209/RH3210, RH5419, RH5201 wording and first-call change, RH5112/RH5048 wording, the joins of argument lists, element access, `?[`, and `!` (#895, #1877), the unsupported shapes FMT-UNSUP-013..016, performance (#1862, #1803), CLI stack-overflow handling (#1887), `Release.ruleset` completeness (#1890).
- after the v1.5.0 tag → `/squad #1880` (1.6.0; carried into `roadmap.md`).

## Status

Status 2026-11-21 (b): maintainer: /bump-version sets no tag (no right to); the GitHub release creates it; release notes run to the merged bump commit; check reuses the release-notes scope definition. Commands changed in the draft.
Status 2026-11-21 (a): state carried over from `claude/sdd-coordinator` (roadmap condensed into `roadmap.md`; Phase 5–7 inventories not carried). `/coordinator` drafted on `claude/bold-dijkstra-n2wcif`, waiting for the maintainer's go-ahead to publish it as a PR to `main`. #1915 running.

## Next

When #1915 merges: `/coordinator batch` (records the merge; the milestone is then empty), then `/coordinator check`. After the check: `/bump-version 1.5.0`; after its merge `/generate-release-notes 1.5.0 v1.4.0`; publish the GitHub release with tag `v1.5.0` on the bump commit, `/coordinator cleanup`, `/coordinator start 1.6.0`.
