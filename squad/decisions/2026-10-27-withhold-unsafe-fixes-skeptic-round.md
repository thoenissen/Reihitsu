# Withheld fixes: RH7306 architecture finding and the open questions after the skeptic

| | |
|---|---|
| Date | 2026-10-27 |
| Escalation | 1, 5, 12 |
| Run | issues #1136, #1134, #1135, #1262, #1332, #1336, #1340 |

## Question

1. Architecture finding: the RH7306 analyzer and its code fix each decide "ends with the word `implementation`" with a private predicate, on different texts (description versus whole directive line). A — fix narrowly in this run: the fix judges the description through one shared predicate; B — restructure all RH73xx description rules first; C — stop the run.
2. RH7309, layout-sequential struct: do auto-property and field-like-event backing fields count as instance fields? A — no; B — yes, the fix is withheld.
3. RH7303, the mirror of the straddling region (`#region` in a method body, `#endregion` at type level, only `#region` reported): A — offer the fix, both lines go; B — withhold it like the decided shape.
4. RH7309, regions that `RegionDirectiveUtilities.GetTopLevelRegions` counts although they belong to a nested enum or an expression: A — fix the shared helper in this run; B — withhold only the RH7309 fix; C — leave the instance out of the run and file a follow-up (helper and the SHARED-TYPEREGION-005 gap).
5. RH7309, a comment above a block the fix moves, separated from its `#region` by a blank line: A — withhold; B — offer.
6. RH7309, a comment directly above a block that keeps its position: A — offer; B — withhold whenever any block has such a comment.
7. RH7306, the `#endregion` partner left as `#endregion //`: A — keep today's output; B — withhold the fix when nothing of the partner's description would remain; C — remove the `//` with the word.
8. RH7105/RH7106, a comment between the last modifier and the next token: A — withhold the fix, as for directives; B — offer it without the inserted blank line.

## Answer

1A, 2B, 3B, 4C, 5A, 6A, 7B, 8A — the lead's recommendation on every question. The maintainer also asked the lead to pause after every role from here on, because the run's budget is limited.

## Reason

None given.
