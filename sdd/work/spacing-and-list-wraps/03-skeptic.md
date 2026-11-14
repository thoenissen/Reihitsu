Role: skeptic
Commit: 952e41e
Verdict: #1642: nothing to do; #1635: proceed narrower (amend the FMT-BREAK-012 exception before tests are written)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It told me to check the spec diff and the change doc, not the premise again, and I did that.

## thoenissen/Reihitsu#1642

Nothing to do: the spec diff does not touch horizontal spacing, and decision 2 (characterization test only, `Refs #1642`) stands.

## thoenissen/Reihitsu#1635

### Premise

The premise was settled in 01-skeptic. I checked only whether the spec diff (`sdd/specs/formatter/line-breaks.md` against `origin/main`) and the change doc (`sdd/changes/2026-11/list-wrap-by-elements.md`) carry out the maintainer's answers and nothing more.

| Answer / decision | Where in the diff | Verdict |
|---|---|---|
| Q1 → C: `Foo(1⏎, 2, 3)` gives `Foo(1,⏎2,⏎3)`, attribute argument lists too | FMT-BREAK-012, comma paragraph and second example; change doc entries 1 and 4 | carried out |
| Q2 → like C: `Foo(1⏎, 2,⏎3)` | FMT-BREAK-012, second example; entry 2 | carried out |
| Clarification: elements decide; `M(⏎a, b)` is single-line and joined | FMT-BREAK-012, first two bullets and first example | carried out |
| Non-goal: the commas leave #956, the closing parenthesis stays | Non-goals bullet | carried out |
| Decision 7: the comment-pinned comma is a rare class, rewritten only where a cheap check shows the move is safe, otherwise left as written | FMT-BREAK-012, last paragraph: the **whole list** is left as written | **goes further than the decision**, see *Breaking inputs* A |
| Entry 6, the indexer wrapped only behind `[`, is now joined | FMT-BREAK-012, last example; entry 6 | covered by the clarification. The maintainer stated the definition for "a list", with no exception for a list kind. The split behind `[` was a lead decision of #1644 (decision 14 in `squad/notes/2026-11-07-formatter-layout-fixes.md`), not a maintainer answer, so no earlier answer is overturned. No question needed |

One structural point follows from the diff. Decision 4 asked for a "wrapped behind the opening delimiter" signal shared by all four list visitors. Under the new definition that signal is not wanted anywhere, so the indexer's copy (`isWrappedAfterOpener`, `LineBreakListRewriter.cs:504–546, 772–779`) goes away instead of being shared. Decision 4 is therefore moot. The existing test `IndexerParameterListLayoutTests.FirstIndexerParameterJoinsOpeningBracket` (line 110) flips, as 02 says.

### Mechanism and candidates

_N/A_: this is a behavior change, not a bug. The list kinds the spec diff covers are the dispatch of 01-skeptic:
- `ArgumentListSyntax`, under every owner;
- `AttributeArgumentListSyntax`;
- `ParameterListSyntax`;
- `BracketedParameterListSyntax`.

The comma paragraph applies only to the first two. The parameter-list kinds already get their commas from FMT-BREAK-011.

### Breaking inputs

Every observation below comes from `scripts/format.sh`, run twice, and `scripts/apply-fix.sh` for RH5101, RH5102, RH5107, and RH5109, under LF and CRLF. All runs were at 952e41e, which has no production change against `main`. Fixtures were in the scratchpad and are deleted.

**A. The whole-list exception conflicts with SHARED-DIAG-010 and with FMT-BREAK-037 (FC-013, FC-016)**

The last paragraph of FMT-BREAK-012 leaves the *whole list* as written when one comma is held by a comment or a directive. It adds no line break between any of the list's elements and moves none of its commas.

That makes these lists shapes that "a requirement of the formatter leaves unchanged". RH5102's fix delegates to the formatter (RH5102-003), so SHARED-DIAG-010 now says RH5102 MUST NOT report them. Today it reports them and withholds the fix (RH5102-004). Observed:

| Input | Formatter at 952e41e (and under the new spec) | RH5102 |
|---|---|---|
| `Foo(1, 2⏎, 3 // keep⏎, 4);` | unchanged | reported, `no-fix-offered` |
| `Foo(1, 2⏎, 3, 4 // keep⏎, 5);` | unchanged | reported, `no-fix-offered` |
| `Foo(1, 2⏎#if X⏎, 3⏎#endif⏎);` | unchanged | reported, `no-fix-offered` |

On `main`, keeping these lists was a formatter defect against FMT-BREAK-012, which demanded the split. The spec diff makes the keep deliberate, so the disagreement moves to the analyzer. Two things would resolve it:
- RH5102 stops reporting these lists. That is a diagnostic change, so it is an output question, and the analyzer gains a whole-list special case.
- Or the formatter does more here.

The diff also gives up safe ground in parameter lists:
- Change doc entry 7, input `void N(int a⏎, int b // keep⏎, int c)`.
- On `main`, the free first comma is moved.
- Under the diff, nothing is moved. RH5107 then reports the free comma in the formatter's output, and its fix gives `void N(int a,⏎int b // keep⏎, int c)` (observed).
- RH5107's fix is not a formatter delegation, so SHARED-DIAG-010 is not strictly violated.
- But the CLI's output keeps a diagnostic that the formatter cleared on `main`. This is the "guard's scope" defect in `AGENTS.md`, *Code-fix coverage*.

The same spec already decides the same shape the other way. FMT-BREAK-037 (constraint clauses) refuses per gap: "a comma kept off the previous line stays at the start of the line of the constraint behind it", while the rest of the clause is laid out one constraint per line. Its example `where T : class // c⏎, new()` keeps exactly the held comma. Under FMT-PIPE-014's family rule, two comma-separated lists that refuse differently are the drift FC-016 names.

The change doc's reason for rejecting the per-gap option is that it "leaves a mix of leading and trailing commas, or a comma alone on its line (entry 7, before)". The lone comma is `main`'s defect, not a property of refusing per gap. FMT-BREAK-037 shows the per-gap layout without one.

Per-gap outputs, which follow from the maintainer's answer C plus FMT-BREAK-001, for comparison. Each is clean for RH5102, RH5107 (SHARED-JOIN-001 exempts the held comma), and RH5109:

```csharp
// Foo(1, 2⏎, 3 // keep⏎, 4);
Foo(1,
    2,
    3 // keep
    , 4);

// void N(int a⏎, int b // keep⏎, int c)  (entry 7; equals RH5107's fix result)
void N(int a,
       int b // keep
       , int c)

// Foo(1, 2⏎#if X⏎, 3⏎#endif⏎);
Foo(1,
    2
#if X
    , 3
#endif
    );
```

Recommendation (internal, rare class, the lead decides): narrow the exception to the held gap. A comma that a comment, directive, or disabled text keeps off the previous element's line stays at the start of the line of the element behind it, as in FMT-BREAK-037. Every other comma and element of the list is laid out as the rest of FMT-BREAK-012 and FMT-BREAK-011 say. This puts no output in question:
- the held gap is left as written (a rare class, RH5035/RH5038);
- the rest is the maintainer's answer.

The FMT-BREAK-011 exception clause ("except in a list FMT-BREAK-012 leaves as written") becomes "except a comma that a comment or directive keeps off the line (FMT-BREAK-001)".

Change doc impact:
- entry 7's "after" becomes the RH5107 result above;
- the "Alternatives" item and the third "Decisions" subsection swap;
- new entries for the three argument-list inputs in the table above (before: unchanged; after: as shown).

If the lead keeps the whole-list exception instead, RH5102 must stop reporting those lists. That is a diagnostic change and therefore an output question. Otherwise the spec diff ships a SHARED-DIAG-010 violation.

**B. A comment behind a leading comma that moves is not decided (FC-004, FC-021)**

The comma paragraph says that a leading comma "moves behind the element in front of it". It does not say where a comment behind that comma, on the comma's line, goes. The exception paragraph covers only a comment *in front of* the comma.

| Input | `main` |
|---|---|
| `Foo(1⏎, /* c */ 2, 3);` | unchanged |
| `Foo(1⏎, // c⏎2, 3);` | `Foo(1⏎, // c⏎2,⏎3);` |
| parameter list `void P(int a⏎, /* c */ int b)` | `void P(int a, /* c */⏎int b)`: the comment travels with the comma |
| parameter list `void Q(int a⏎, // c⏎int b)` | `void Q(int a, // c⏎int b)`: the comment travels with the comma |

Two formatter precedents decide it the same way, by FMT-PIPE-014 (one layout per family):
- the parameter-list output above;
- FMT-BREAK-037: "a comment behind a comma stays behind it" (example `R`).

So the argument-list outputs are `Foo(1, /* c */⏎2,⏎3);` and `Foo(1, // c⏎2,⏎3);`. I read this as decided rather than open. Its gap is a spec gap: FMT-BREAK-012 needs one sentence and one example, and the change doc needs these two inputs as entries, since both outputs change. If the lead does not accept the precedent reading, the case is an output question with these options:
- (A) the comment travels with the comma;
- (B) the list is left as written (a rare-class refusal, which needs no question).

The RH5107 fix leaves such a comment in front of the parameter (`int a,⏎/* c */ int b`, observed), which disagrees with the formatter. That is pre-existing; see *Follow-up candidates*.

**C. The counting in the exception paragraph (wording, rare)**

The first paragraph counts lines as SHARED-BLANK-008 does. The exception paragraph ("that comma is on a later line than the element's last token") does not say how it counts.

The two readings give different output for `Foo(1 /* a⏎ b */, 2,⏎3)`:
- text lines: the list is held and left as written;
- SHARED-BLANK-008: the comma is on the element's line, so it is not held, and the list is laid out.

Recommendation: state SHARED-BLANK-008 counting there too, as FMT-BREAK-037 does. This is an internal wording fix that follows the requirement's own first paragraph.

**D. What a held list still gets (wording)**

The exception says only "no line break between its elements, no comma moved". Under A's narrowing this disappears. If the whole-list form stays, the spec should say that the first element still joins the opening parenthesis and that FMT-BREAK-015 still joins element interiors. RH5101 reports `Foo(⏎1⏎, 2 // keep⏎, 3)` on `1`, so leaving that gap would keep a diagnostic. This is the literal reading of the text, so it is internal.

**Checked and holding**

These held under both passes:
- **A multi-line raw string as an element** (`Foo(1, """⏎x⏎""", 2)`): split on `main`. This matches "an element spans several lines" (FMT-BREAK-013).
- **A block comment between an element and a comma on the next line** (`void R(int a /* x */⏎, int b)`, `Foo(1 /* x */⏎, 2, 3)`): `main` already keeps it, so this is no change.
- **The single-line join** (`Foo(⏎a, b)`): consistent with RH5101 and RH5102. Both report it, and their fixes delegate to the formatter's join.
- **Other specs that cite FMT-BREAK-011 or FMT-BREAK-012:** RH5101, RH5102, RH5105, RH5106, RH5107, RH5108, RH5109, `pipeline.md` FMT-PIPE-014's example, and `switch-case-braces.md`:251.
  - None contradicts the diff except RH5109 (below) and A.
  - RH5102's non-goal ("the position of a comma … in an argument list") still holds: the rule reports nothing new. A formatter that moves commas no rule reports is P11.

**Candidate inputs the test implementer must cover** (each under LF and CRLF, with a second pass), in addition to 02's examples:
- the three argument-list inputs of A, and entry 7, with whatever the lead decides;
- both B inputs;
- `Foo(⏎1⏎, 2, 3)`, through the formatter, the RH5101 fix, and the RH5102 fix (see *Surfaces*);
- C's input, once the wording is settled.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `LineBreakListRewriter` | must change. The indexer-only `isWrappedAfterOpener` is deleted rather than shared |
| `IndentationPhase` | must stay. Its leading-comma alignment still serves held commas |
| RH5101 fix | no code change, but its **result changes**, and the change doc omits it. `Foo(⏎1⏎, 2, 3)` gives `Foo(1⏎, 2, 3)` today (observed). After the change it gives `Foo(1,⏎2,⏎3)` (RH5101-003 delegates). Add an entry next to entry 8 |
| RH5102 fix | result changes (entry 8). `Foo(⏎1⏎, 2, 3)` is `no-progress` today (observed) and converges afterwards. Diagnostic: see A |
| RH5107, RH5109 analyzers and fixes | must stay. RH5107 is affected only through A (entry 7) |
| RH6002, RH5203, RH5105, RH5106, RH5108 | not involved |
| `line-breaks.md` | amend: A, B, C, D |
| Change doc | amend: entry 7, the new entries from A and B, the RH5101 fix entry, and the Decisions/Alternatives swap |
| Rule pages, `Reihitsu.Core` | not involved |

### Refuse option

The run already contains a refusal: leaving a held comma as written, which is a rare class (RH5035/RH5038). What is open is its **scope**:
- whole list (the diff): it conflicts with SHARED-DIAG-010 for RH5102, and is less than `main` for parameter lists;
- held gap (FMT-BREAK-037's form): no conflict.

Declaring the shape unsupported ("a comment, directive, or disabled text between a list element and the comma behind it, from the element's last token through the comma") is an option to name. It is not recommended:
- it would delete no special case, because FMT-BREAK-001's guard serves every join;
- it would refuse whole files with the leading-comma-plus-comment style, which is rare but legitimate.

The lead may not want to escalate it under escalation 10. If so, it needs no question, because no one proposes declaring it.

Simpler options: _None_ beyond A's narrowing.

### Output questions

_None_, provided the lead takes A's per-gap narrowing (rare-class refusal plus the maintainer's answer) and B's precedent reading. Two choices would open questions:
- keeping the whole-list exception opens one, on whether RH5102 stops reporting those lists;
- rejecting B's precedent opens the question in B.

### Internal choices

1. **Scope of the held-comma refusal** (A). Per gap or whole list. Recommendation: per gap.
2. **The counting rule in the exception** (C). Recommendation: SHARED-BLANK-008.
3. **Entry 6** (the indexer join behind `[`). Treat it as covered by the clarification; there is no indexer-only exception.

### Issue goals

Unchanged from 01-skeptic and decisions 3 and 7. The maintainer's answers have replaced the issue's "keep leading commas" option, and the second comment's W1 observation is now settled by the clarification: the list is joined.

### Architecture findings

_None new._ Decision 4's finding is resolved by deleting the indexer copy, as the definition no longer needs the signal.

### Follow-up candidates

These are pre-existing, outside the run's issues, and not changed by the diff:
- **RH5107's fix and the formatter disagree on a comment behind a leading comma in a parameter list** (FC-013).
  - Input `void P(int a⏎, /* c */ int b)`.
  - The formatter gives `int a, /* c */⏎int b`.
  - RH5107's fix gives `int a,⏎/* c */ int b`.
  - Output question for the issue: which of the two the fix should produce.
- **No rule reports a leading comma in an argument list.** RH5107's non-goal excludes argument lists, and RH5102's excludes commas. After this change the formatter moves them, which P11 allows. This is a new-rule candidate: an argument-list counterpart of RH5107. Check whether thoenissen/Reihitsu#956 already plans it before filing.
- **RH5109 W1** (02-spec-author's candidate, confirmed). RH5109-003's own example `void N(⏎int a, int b)` → `int a,⏎int b` now contradicts FMT-BREAK-012, which RH5109's Why cites as its basis, while the formatter joins it.

## Whole run

- Commands, at 952e41e:
  - `scripts/format.sh` over 12 scratch fixtures;
  - `scripts/apply-fix.sh` for RH5101, RH5102, RH5107, and RH5109 over the same fixtures, under LF and CRLF.
- The RH5102 run exits 1 only because of `Foo(⏎1⏎, 2, 3)` (`no-progress`), which this run fixes.
- Fixtures were deleted afterwards. No repository file was changed except this handover.
