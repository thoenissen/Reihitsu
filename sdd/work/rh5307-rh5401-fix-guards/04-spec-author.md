Role: spec author
Commit: 63edd66
Verdict: open question

My prompt carried no intended fix, suspected cause, or hoped-for verdict; it gave the maintainer's reply and the task to carry it out.

## Answers carried out

- **Q1 A** (attribute list on the property's line gets the fix, `[Obsolete]⏎int E => 1;`): already the spec's and change doc's text (RH5401-003 example `B`); recorded under the change doc's `Decisions`.
- **Q2 B**: `documentation/rules/RH5401.md` now says "The code fix is offered when the property, apart from its attributes, fits on one line."; change doc *Visible output changes* (rule page) and `Decisions` updated.
- **Q3** ("the fix should not insert a blank line, that is not part of its scope"): output `[Obsolete]⏎// note⏎int E => 1;` written into RH5401-003's example and the change doc's `D` after-block; `Decisions` records it and that neither asked option was chosen; two rejected alternatives added.
- **Q4 A**: already the spec's and change doc's text (RH5307-004); recorded under `Decisions`.
- Change doc `sdd/changes/2026-11/rh5307-rh5401-fix-the-reported-construct.md`: `Status: Accepted`. Every visible output change it lists has an answer; the open case below is not listed in it (see *Output questions*). If the lead prefers the doc stay `Draft` until that answer, revert the one line.

## Q3 reachability

**Not reachable from the requirements the run otherwise leaves unchanged.** RH5401-003 says the fix formats the property "as the formatter formats it in place", and FMT-BLANK-011 inserts a blank line in front of a comment on its own line whose preceding token (`]`) opens nothing. SHARED-FIX-006 limits blank lines only *before* the node and *between* it and its neighbours, and keeps only the node's own leading trivia; `// note` is the leading trivia of `int`, inside the node, so neither clause covers it.

Verified the same way as before: scratch worktree (deleted), throwaway RH5401 guard measuring from the first token behind the attribute lists, `scripts/apply-fix.sh RH5401` on fixtures under LF and CRLF (identical). Observed:

| Input (attribute region) | Fix output with today's formatting |
|---|---|
| `[Obsolete]⏎// note⏎int E` | blank line **inserted** in front of `// note` |
| `[Obsolete]⏎[CLSCompliant(false)]⏎// note⏎int E`, `[Obsolete] // why⏎// note⏎int E`, `// lead⏎[Obsolete]⏎/* note */⏎int E` | blank line inserted |
| `[Obsolete]⏎// note⏎[CLSCompliant(false)]⏎int E` (comment between two lists) | blank line inserted |
| `[Obsolete]⏎⏎// note⏎int E` | kept as written |
| `[Obsolete]⏎#if DEBUG⏎// note⏎#endif⏎int E` | nothing inserted (directive precedes) |
| `[Obsolete]⏎⏎int E`, `[Obsolete]⏎⏎[CLSCompliant(false)]⏎int E` | kept |
| `[Obsolete]⏎⏎⏎// note⏎int E`, `[Obsolete]⏎⏎⏎int E`, `[Obsolete]⏎⏎⏎[CLSCompliant(false)]⏎int E` | **collapsed to one** blank line |

So the run changes **RH5401-003** to reach the answer: an exception that the fix MUST NOT insert a blank line inside the property where the formatter would insert one in front of a comment on its own line behind an attribute list (in front of the next list or the property's own code); a comment that already has a blank line above it is unaffected. New example in RH5401-003 for a comment that keeps its existing blank line and a comment between two attribute lists (both outputs match the table above minus the insertion). How the requirement is worded is an internal choice (below); the decided output is the maintainer's.

The answer leaves one case open — the removal of blank lines the author wrote (last table row). It is the output question below.

## Changed requirements

- RH5401-003 — The fix puts the property on one line — changed: exception "MUST NOT insert a blank line inside the property" (FMT-BLANK-011 case); the comment-between example now has no blank line; new example (comment with an existing blank line kept; comment between two attribute lists). `Uses` unchanged (FMT-BLANK-011 still referenced).
- Rule page `documentation/rules/RH5401.md`: one sentence (Q2 B).

No requirement IDs added or removed. `scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

- RH5401-003: the comment-between example (no blank line inserted) and the new example (`E` keeps its one blank line, `G` gets none inserted between the lists). Everything listed in 03-spec-author.md stands.
- Tests for several blank lines in the attribute region wait on the output question.

## Output questions

**1 · RH5401 · Expression style get-only properties should be single lined — several blank lines between the attributes and the property**

Input:
```csharp
[Obsolete]


// note
int E
    => 1;
```

Before (`main`): no fix offered.

After, A (recommended):
```csharp
[Obsolete]


// note
int E => 1;
```

After, B:
```csharp
[Obsolete]

// note
int E => 1;
```

Why A: it follows the answer that blank lines are not part of what this fix is for, in both directions; B removes a blank line the user wrote, which the fix does not report.

(The same holds without the comment, `[Obsolete]⏎⏎⏎int E`, and between two attribute lists. Until answered, RH5401-003 decides only insertion; the implementer can build everything else.)

## Internal choices

- The Q3 exception is stated in RH5401-003 only, not in SHARED-FIX-006, because the maintainer answered for this rule; alternative: widen SHARED-FIX-006 *Blank lines* to blank lines inside the formatted node, which would bind every formatter-delegating fix and is beyond the issue.
- The exception names its cause (FMT-BLANK-011, a comment on its own line behind an attribute list) rather than "the fix keeps every blank line inside the property as written", which would decide the open question.
- `Status: Accepted` set although one output question is open, because that case is not among the doc's listed changes; once answered it is added to *Visible output changes* and `Decisions`.

## Follow-up candidates

_None._ (FMT-BREAK-014 and `c?[1,⏎2]` stays as recorded in `00-lead.md`.)
