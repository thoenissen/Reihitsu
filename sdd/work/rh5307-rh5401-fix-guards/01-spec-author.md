Role: spec author
Commit: 37dc984
Verdict: change doc written

## Lane

A change doc is required: RH5307-004 and RH5401-003/-004 scoped the fix to the inputs it handles today ("not inside another bracketed argument list and whose receiver does not start with an element access"; "for a property without attribute lists"), so the issue's inputs are not bugs against the spec but requirement changes that offer a fix where none was offered — a visible code-fix change (`sdd/README.md`, *When a change doc is required*; precedent `sdd/changes/2026-11/rh7304-fix-for-comment-led-region-line.md`). The lane widens to **behavior change**. The maintainer already answered every case on #1217 (Q1, Q3) and #1219 (Q1), 2026-10-20; the change doc records those answers under `Decisions`. Status stays `Draft` for the lead to set.

Change doc: `sdd/changes/2026-11/rh5307-rh5401-fix-the-reported-construct.md`.

## Changed requirements

- RH5307-001 — A wrapped element access is reported — clarified: the argument list of `c?[…]` (an element binding expression) MUST NOT be reported; negative example added.
- RH5307-004 — The fix joins the reported argument list — changed: the fix joins the list the diagnostic is located on (RH5307-003), whatever the receiver is and whatever contains it; a list in the receiver or around the access is not joined and does not withhold the fix.
- RH5401-003 — The fix puts the property on one line — changed: no longer limited to properties without attribute lists; attribute lists go on lines of their own (FMT-BREAK-027, added to `Uses`).
- RH5401-004 — The fix is withheld when the property stays on several lines — changed: judges the property's own code, from the first token behind its attribute lists through its `;`.
- Rule pages: `documentation/rules/RH5307.md` (one sentence: `target?[...]` is not checked — the maintainer's decision #1217 Q1 asks for it on the page); `documentation/rules/RH5401.md` ("whole declaration fits on one line" was no longer true; now "the property fits on one line; attributes keep lines of their own above it").

No new requirement IDs; `requirement-ids.txt` unchanged.

## Examples for the tests

- RH5307-001 (new `?[ ]` negative example), RH5307-004 (second example: `x[m[1,⏎2]]`, `j[0][1,⏎2]`, `j[0 /* c */][1,⏎2]`, `j[N(1,⏎2)][1,⏎2]`; each line is a test of its own).
- RH5401-003 (new attributed example: two attribute lists on own lines; attribute sharing the property's line), RH5401-004 (new attributed withheld case `E`).

Checked: today's code gives no fix for `x[m…]`, `j[0 /* c */]…`, `j[N(…)]…`, all RH5401 attributed inputs; `c?[1,⏎2]` is not reported (`scripts/apply-fix.sh`). New find within the issue's cause: `j[0][1,⏎2]` (plain element-access receiver) gets a fix offered that **changes nothing** (`no-progress`) — the fix resolves `[0]`. The example outputs were verified by running every spec example through a throwaway copy of the two providers with the guard narrowed as specified (scratch worktree, deleted; no repository code touched): all RH5307 examples and RH5401-003 examples fix to the stated outputs under LF and CRLF, the RH5401-004 example stays `no-fix-offered`. Architecture tests and `RuleDocumentationConventionTests` pass.

## Output questions

The three listed visible changes are answered by the maintainer's decisions; each is listed so the lead can record it. Question 3 is the one detail the decisions do not name explicitly.

**1 · RH5307 · Indexer bracketed arguments should be single lined — reported list inside another list or behind an element-access receiver** — answered by #1217 Q3.

Input:
```csharp
var a = x[m[1,
            2]];
var b = j[0][1,
             2];
var c = j[0 /* c */][1,
                     2];
```

Before (`main`): a and c no fix offered; b fix offered, changes nothing.

After, A (recommended, decided):
```csharp
var a = x[m[1, 2]];
var b = j[0][1, 2];
var c = j[0 /* c */][1, 2];
```

After, B: unchanged from today.

Why A: the maintainer's decision; the fix joins the list it reports.

**2 · RH5401 · Expression style get-only properties should be single lined — property with attribute lists on their own lines** — answered by #1219 Q1.

Input:
```csharp
[Obsolete]
int E
    => 1;
```

Before (`main`): no fix offered.

After, A (recommended, decided):
```csharp
[Obsolete]
int E => 1;
```

After, B: no fix offered.

Why A: the maintainer's decision; the guard judges the property's own lines.

**3 · RH5401 · Expression style get-only properties should be single lined — attribute list on the property's line**

Input:
```csharp
[Obsolete] int E
    => 1;
```

Before (`main`): no fix offered.

After, A (recommended):
```csharp
[Obsolete]
int E => 1;
```

After, B: no fix offered; the diagnostic stays.

Why A: the attribute is inside the reported span (RH5401-002), the fix's output is the formatter's layout for it, and the property's own code fits on one line, which is what #1219 Q1 makes the test. The spec and change doc are written on A.

## Internal choices

- `?[ ]` stated as a MUST NOT sentence plus negative example in RH5307-001 rather than a non-goal, so the test implementer has an example; alternative: a non-goal bullet.
- RH5307 rule page gets the `?[ ]` sentence although the create-rule-doc convention (added 2026-11-15) keeps corner cases off pages: the maintainer's decision #1217 Q1 (2026-10-20) names the page explicitly, and the sentence states the rule's scope like RH5112 and RH5201 do for `?[0]`. Alternative: spec only.
- RH5401 rule page sentence edited because "the whole declaration fits on one line" becomes false for every attributed property, a normal case.
- FMT-BREAK-014 not touched in this run (see follow-up).

## Follow-up candidates

**FMT-BREAK-014 · formatter · null-conditional element access** (reachable: whole-document formatting and the CLI). FMT-BREAK-014 says "a wrapped bracketed argument list (`m[1, 2]`) … MUST be joined", which literally includes the list of `c?[…]`, but the formatter leaves it:

Input:
```csharp
var a = c?[1,
           2];
```

Before (`main`): unchanged.

After, A: `var a = c?[1, 2];` (the formatter joins it like `c[1,⏎2]`; RH5307 stays silent, P11).

After, B: unchanged; FMT-BREAK-014 is clarified to exclude an element binding's list.

Recommendation A: the list is the same construct as an element access's and reads the same; the maintainer only decided RH5307's scope (#1217 Q1). Not decided in this run because the issue asks only for RH5307's spec and page.

The ignored-prompt note: my prompt carried no intended fix or hoped-for verdict.
