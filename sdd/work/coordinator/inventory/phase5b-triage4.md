# Phase 5b triage — #1693–#1696

Triaged without reproducing; assessed from issue text and the code it names.

| Issue | Title (short) | Class | Proposed milestone | Labels | Run | One-line reason |
|---|---|---|---|---|---|---|
| #1693 | sdd/README: shared specs "apply without `Uses`" vs SpecStructureTests | internal | Maintenance (#4) | (has maintenance, documentation) | — | Rulebook text only; contradiction confirmed (`FindUsesProblems` has no shared-spec exemption, RH2002 lists SHARED-DIAG IDs), though the cited examples RH4001/RH4009/RH2003/RH2109 actually have `Uses —` and reference no SHARED-DIAG ID. |
| #1694 | SHARED-DIAG-002: compare names by value incl. Unicode escapes | internal | Maintenance (#4) | (has maintenance, documentation, analyzer) | — | Spec clarification + tagged tests; overlaps #1105 (using rules compare by value, 1.2.2) and #1132 (param-name matching); any rule found non-compliant becomes its own bug issue. |
| #1695 | Node-in-document write-back of trailing trivia: line ending, trailing whitespace | minor | none (deferred) | (has bug, formatter) | — | Reachable via every formatter-delegating fix, but nothing is damaged: the trailing whitespace was already behind the original last token (kept, not introduced), and in an all-lone-`\r` document the written-back `\r` is the detected ending — only mixed documents differ. |
| #1696 | RH8402 fix: inserted header uses first line's ending, not detected | minor | none (deferred) | (has bug, analyzer) | — | Confirmed in code (`NormalizeLineEndings`/`GetPreferredLineEnding` take the first line break, `"\n"` fallback); all-lone-`\r` case is overstated (first ending = detected ending), real only for mixed docs; neighbours #1353, #1450, #1346. |

No labels need adding; all four already carry the group label and surface labels. No issue is a duplicate; none is unreachable.

## Doubts

1. **#1694 class.** If checking "every rule that compares a name" finds a rule that compares by spelling, fixing it changes what users see.
   - A — Maintenance (#4) as filed; any non-compliant rule found is split into its own bug issue.
   - B — vNext (#2) + `enhancement`, treating the issue as a behavior change across rules.
   - Recommendation: A — the issue's deliverable is spec text and tests; behavior changes, if any, get their own triage.
