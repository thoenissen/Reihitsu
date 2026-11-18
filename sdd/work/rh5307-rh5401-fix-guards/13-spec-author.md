Role: spec author
Commit: c3c9de9
Verdict: spec written

My prompt carried no intended fix or hoped-for verdict. It pointed at the auditor's suggestion (finding 1) and said it is only a suggestion. I worked the class out again from new probes. It is wider than the auditor's list, and the condition I wrote is not the auditor's "whole-region count only" (see *Internal choices*).

## Repair round 1

**Finding 1, spec side.** RH5401-004 said that a gap the formatter creates when it splits an attribute list (FMT-STRUCT-028) "holds no blank line" and does not withhold the fix. That is false. When the formatter splits a list, it drops every blank line written inside that list, not only the one between two attributes. My probes (`apply-fix.sh`, LF and CRLF, current code `c3c9de9`; whole-document `format.sh` gives the same result) show this:

- Dropped, fix offered today: a blank line behind the comma (`N`), in front of the comma (`O`), behind the `[` of a list that is split (`P`), and inside the argument list of an attribute in a list that is split (`[Obsolete(⏎⏎ "a"), CLSCompliant(false)]`, `R`). The auditor did not have `P` and `R`. Its `q01`/`q02` probes were single-attribute lists, which the formatter does not split.
- Kept, fix offered: a blank line inside a list that is not split (`[Obsolete("a",⏎⏎ false)]`, `Q`, the auditor's `q01`), and a blank line in front of the `]` of a list that is split (not used as an example; see *Follow-up candidates*).
- Withheld today already: `S` = `N` plus `// note` in front of the property. Here the formatter drops the blank line inside the list and inserts one in front of the comment, so the total stays the same. The per-gap condition is what withholds it.

The new RH5401-004 sentence: the fix MUST NOT be offered when the formatted property holds, in all, a different number of blank lines than the property as written, counted from the `[` of its first attribute list through its first token behind its attribute lists. The per-gap condition behind each list stays as it was. The requirement says each condition catches a case the other misses (example `S`). The false sentence about split gaps is replaced. Example `L` (a split list with no blank line) and the new example `Q` are the offered side. A new withheld example holds `N`, `O`, `P`, `R`, `S`. RH5401-003: the condition sentence now says "among and behind the attribute lists", and the example of kept blank lines gains `Q`. In `Why:`, "between the attributes and the property" becomes "among the attributes and between them and the property".

**Finding 3, change doc** (`Status: Accepted` kept). The RH5401 before/after block now holds `L` and `Q`; neither got a fix on `main`. The *unchanged* block gains `N` and a sentence naming the other positions inside a list that is split. *What* names FMT-STRUCT-028 and the dropped blank line. The decision "judges the property's own lines" names FMT-STRUCT-028 for the split. The question-6 decision states that the maintainer's rule also covers a blank line inside a list that is split, and that the fix compares the total too.

Rule page `documentation/rules/RH5401.md`: unchanged. The new no-fix inputs are a withheld fix in a corner case.

Verified:
- Every RH5401 spec example and the change doc's RH5401 blocks, through `apply-fix.sh`. The offered examples match the spec's output exactly (`Q` included). The new withheld `N`, `O`, `P`, `R` compile and are reported, but the current code still offers a fix for them. Withholding them is the implementer's job. `S` is withheld already.
- A fixture that holds all five gives `no-fix-offered` as a whole, because the runner stops at `S`. Run them one by one, as I did.
- `scripts/test.sh --project architecture`: 198 passed.

## Changed requirements

- RH5401-004 — The fix is withheld when the property stays on several lines — changed (whole-region blank-line count; false sentence about split gaps replaced; new example `N`, `O`, `P`, `R`, `S`)
- RH5401-003 — The fix puts the property on one line — clarified (wording "among and behind"; example `Q` added)

## Examples for the tests

- RH5401-004: new example `N`, `O`, `P`, `R` (withheld, offered today: these are the reproductions) and `S` (withheld today, a guard for the per-gap condition when the total is unchanged). One test per member, with `VerifyNoCodeFixIsOffered`.
- RH5401-003: `Q` (offered; the blank line inside a list that is not split stays).

## Output questions

_None._ Withholding the fix for this rare class is the rule maintainer answer 6 already set (the fix does not change these blank lines). On `main` these inputs got no fix either.

## Internal choices

- The condition is the total count over the attribute region **in addition to** the per-gap count behind each list, not instead of it. With the total alone, `S` would get the fix, and the fix would move a blank line from inside the list to in front of the comment. Alternative: withhold whenever a list with several attributes holds a blank line, a syntactic check that does not depend on the formatter's output. Rejected: it also refuses the case where the blank line is in front of `]`, which keeps the blank line (safe ground given away), and it no longer matches the count comparison the maintainer chose.
- Example `R` (blank line inside an attribute's arguments in a list that is split) is in the spec, so the implementer's guard covers it. It shows that "between attributes" would be too narrow a reading.

## Follow-up candidates

- **Formatter, an attribute list with several attributes and a blank line in front of its `]`** (rare shape; reached through whole-document formatting and the CLI). Input:
  ```csharp
  [Obsolete, CLSCompliant(false)

  ]
  int E => 1;
  ```
  Real output (`format.sh`):
  ```csharp
  [Obsolete]
  [CLSCompliant(false)

   ]
  int E => 1;
  ```
  It looks wrong because the `]` ends up one column right of its `[`, while every other split list closes on the attribute's line. Reading A: `[Obsolete]⏎[CLSCompliant(false)]` (recommended, the same as every other list that is split). Reading B: keep the blank line and put `]` under `[`. Requirement touched: FMT-STRUCT-028 (and the indentation of a `]` on its own line). The RH5401 fix offers this output today, because the blank-line count is unchanged. That is consistent with the spec as written.
