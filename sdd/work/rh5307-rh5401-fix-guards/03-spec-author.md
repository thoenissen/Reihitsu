Role: spec author
Commit: fde7f0e
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions 3–5, which I applied.

## Changed requirements

- RH5307-004 — The fix joins the reported argument list — changed (amended): the normative text now names the whole class by its cause — which list is joined MUST NOT depend on the receiver (element access, element binding `c?[0]`, member access starting with one, `this`) or on what contains the access (the bracketed list of another element access, element binding, or implicit element access; a parenthesized argument list; a collection-expression element). New sentence: where the receiver's list is reported too, each diagnostic's fix joins its own list, and fixing all diagnostics joins both. Examples: `p` (`x[/* c */ m[1,⏎2]]`) added to the second example; new third example (`c?[0][1,⏎2]` per decision 5, `n?[m[…]]`, `a[0].P[0][…]`, `this[0][…]`, initializer `[m[…]] = 1`); new fourth example (`j[0,⏎1][1,⏎2]`, two diagnostics, one fix at a time and Fix All give `j[0, 1][1, 2]`); new fifth example for the members fixed today that stay fixed (`N(m[…])`, `[m[…]]` collection element, `(j[0])[…]`, `c?.P[…]`).
- RH5401-003 — The fix puts the property on one line — changed (decision 5 wording): "the attribute lists of the property as FMT-BREAK-027 places them"; the absolute "moves onto a line of its own" statement is gone (false for `[Obsolete /* c */] int E`, whose placement FMT-BREAK-027 leaves unspecified). New example: a comment between the attribute lists and the property (written on Q2 A, see below).
- RH5401-004 — The fix is withheld when the property stays on several lines — clarified: the property's own code excludes the trivia in front of its first token and behind its `;`; neither the attribute lists nor a comment between them and the own code count, "wherever FMT-BREAK-027 places them".
- RH5401 `Uses` gains FMT-BLANK-011 (referenced by the new example).
- Change doc `sdd/changes/2026-11/rh5307-rh5401-fix-the-reported-construct.md` (still `Draft`): *What* states the RH5307 class by cause (receiver's first token inside another bracketed argument list or another element access / element binding) and its three before-outcomes (withheld, no-op action, both fixes on the receiver's list); *Visible output changes* now has the whole class with real before/after, the `j[0,⏎1][1,⏎2]` case (one fix and Fix All), the members fixed today that stay fixed, and the RH5401 attribute-on-line, comment-between, and comment-in-brackets cases; new decision "The RH5307 fix covers the whole class"; RH5401 decision reworded per decision 5.
- Rule pages unchanged in this round; the RH5401 sentence is question 2 below.

No new requirement IDs. `scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

- RH5307-004: all five examples. Each line of the second and third examples is a test of its own; the fourth is both a single-fix test and the natural Fix All scenario (nested spans, FC-008); the fifth covers the stays-fixed members, including parents with the same span as the access (`ArgumentSyntax`, collection element).
- RH5401-003: the attributed example and the new comment-between example. RH5401-004: the existing withheld example.
- Two existing tests assert today's withheld fix and must change (from 02): `VerifyFixIsNotOfferedForElementAccessInsideAnotherArgumentList`, `VerifyFixIsNotOfferedWhenReceiverStartsWithElementAccess`.

How verified: "Before" from `scripts/apply-fix.sh` on the current tree (fde7f0e), one fixture per member, LF and CRLF identical. "After" from a scratch worktree (deleted) with throwaway providers: RH5307 resolves the list from `root.FindNode(span, getInnermostNodeForTie: true)` (element access → its list, bracketed list → itself); RH5401 measures the formatted property's text from the first token behind its attribute lists. Every RH5307-004 and RH5401-003/-004 example ran as a generated `Verify`/`VerifyNoCodeFixIsOffered` test (markup spans, iterative and Fix All): 10/10 passed. Every change-doc input ran through `apply-fix.sh` and matched its after block. Today's Fix All on `j[0,⏎1][1,⏎2]` was observed separately: `j[0, 1][1,⏎2]`, outer list still reported. The throwaway implementations are no proposal for the implementer.

## Output questions

Four questions for the batch. Option labels of the earlier drafts are unchanged.

**1 · RH5401 · Expression style get-only properties should be single lined — attribute list on the property's line** (01 Q3)

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

Why A: the property's own code fits on one line, and the attribute moves to where the formatter puts every attribute list.

**2 · RH5401 · Expression style get-only properties should be single lined — rule page sentence** (02 Q1)

Input:
```csharp
[Obsolete /* c */] int E
    => 1;
```

Before (`main`): the fix gives `[Obsolete /* c */] int E => 1;`. The page says: "The code fix is offered when the whole declaration fits on one line."

After, A: the fix gives the same output. The page says: "The code fix is offered when the property fits on one line; attributes keep lines of their own above it."

After, B (recommended): the fix gives the same output. The page says: "The code fix is offered when the property, apart from its attributes, fits on one line."

Why B: it states when the fix is offered without promising an attribute layout that this input does not get.

**3 · RH5401 · Expression style get-only properties should be single lined — comment between the attributes and the property** (02 Q2)

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

After, B: no fix offered; the diagnostic stays.

Why A: it is the layout whole-document formatting gives this property, and the property's own code fits on one line.

**4 · RH5307 · Indexer bracketed arguments should be single lined — further receivers and containers of the reported index** (new; the members #1217 Q3 did not name)

Input:
```csharp
var vd = j[N(1,
             2)][1,
                 2];
var vp = x[/* c */ m[1,
                     2]];
var ve = c?[0][1,
               2];
var vh = n?[m[1,
              2]];
var vi = a[0].P[0][1,
                   2];
var vj = this[0][1,
                 2];
var vm = new Dictionary<int, int>
         {
             [m[1,
                2]] = 1,
         };
var vn = j2[0,
            1][1,
               2];
```

Before (`main`): `vd`, `vp`, `ve`, `vh`, `vm` no fix offered; `vi`, `vj` a fix is offered and changes nothing; `vn` both reports' fixes give
```csharp
var vn = j2[0, 1][1,
                  2];
```
and the remaining report's fix changes nothing.

After, A (recommended):
```csharp
var vd = j[N(1,
             2)][1, 2];
var vp = x[/* c */ m[1, 2]];
var ve = c?[0][1, 2];
var vh = n?[m[1, 2]];
var vi = a[0].P[0][1, 2];
var vj = this[0][1, 2];
var vm = new Dictionary<int, int>
         {
             [m[1, 2]] = 1,
         };
var vn = j2[0, 1][1, 2];
```

After, B: no fix offered for any of them; the diagnostics stay.

Why A: it is the same rule the maintainer chose for `x[m[1,⏎2]]` and `j[0][1,⏎2]` — the fix joins the index it reports — and B still needs a new check to stop offering the actions that change nothing.

## Internal choices

- RH5307-004 states the class by its cause (receiver kinds, container kinds) with the members as examples, rather than a list of members only; alternative: one example per matrix row without the normative enumeration.
- RH5401 spec and change doc are written on Q1 A and Q3 A, and keep page text A in the change doc's *Visible output changes* until Q2 is answered (B changes only the one sentence).
- The comment between attributes and property's own code is stated in RH5401-004 as "not counted" (trivia in front of the first token), not as a separate requirement; alternative: a new RH5401-005.
- Q4 includes `vd` (from the 01 change doc) and `vp` (skeptic row `p`) besides decision 4's list, since I cannot confirm #1217 Q3 named them; drop them from the question if the lead finds them covered.
- Q4's option B is "no fix offered" rather than today's mix, because today's action that changes nothing is a defect, not an output to choose.

## Follow-up candidates

- FMT-BREAK-014 and `c?[1,⏎2]`: unchanged from 01 (already in `00-lead.md`).
- None other.
