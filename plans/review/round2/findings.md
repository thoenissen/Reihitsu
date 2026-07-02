# 1.0.0 RC Code Review (Round 2) — Findings

Format: `- [Critical|Major|Minor] path:line — problem. Suggested direction for fix.`
Severity scale and criteria: see `criteria.md`. Files without findings are fine and are not listed. Coverage is tracked in `coverage.md`.

Sessions append their findings under a per-session subsection (see `session-prompts.md`). Before adding a finding, check whether it is an instance of an already-reported systemic issue — if so, extend that finding's affected-file list instead of duplicating it.

## Summary

*(Compiled during the closeout session: counts per severity and per assembly, plus the recommended fix order.)*

## Open cross-checks

*(Sessions add entries here when a suspicion in the current scope can only be verified in an area covered by a later session — e.g. "does the code fix inherit this formatter defect?". Each entry names the suspicion, the file/line it originates from, and the session expected to resolve it. The closeout session resolves or explicitly closes every entry.)*

## Reihitsu.Core

## Reihitsu.Cli

## Reihitsu.Formatter

## Reihitsu.Analyzer

## Reihitsu.Analyzer.CodeFixes
