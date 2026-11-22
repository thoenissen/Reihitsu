Role: spec author
Commit: a0c3bef (spec diff and change doc committed as 8cbdaea)
Verdict: change doc written

The prompt held no intended fix or hoped-for verdict. The maintainer's answers 1–6 in `00-lead.md` are written into the spec and the change doc as binding.

## Changed requirements

- PLAY-002 — Input on the left, read-only output on the right — changed: the output becomes a read-only code view with selectable, exactly-copied text; the input keeps a text area's own editing, selection, and undo; spell checking is required only in the input.
- PLAY-006 — Input with syntax errors is shown unchanged with a message — changed: the sentence stays and every parser error is listed below it as `line <line>, column <column> — <ID>: <message>`, ordered by position, duplicates kept, warnings not listed.
- PLAY-010 — Both panes are highlighted by the C# syntax tree — added.
- PLAY-011 — The input is highlighted while the user types — added.
- PLAY-012 — Lines do not wrap — added.
- PLAY-013 — The message is an alert box — added (covers the messages of PLAY-006, PLAY-008, PLAY-009; `role="alert"`-type announcement).
- PLAY-014 — Error locations are marked in the input — added.

PLAY-004, PLAY-008, and PLAY-009 keep their text; their marks and box come from PLAY-013 and PLAY-014. Two non-goals added: semantic highlighting; marks in the output and for warnings. `Uses` and `Code owners` are unchanged. Change doc: `sdd/changes/2026-11/playground-highlighting.md` (`Status: Draft`). Ledger updated. `scripts/test.sh --project architecture` fails only on `EveryRequirementOfAnEnforcedSpecIsCovered` for PLAY-010 to PLAY-014, which the test implementer covers.

## Examples for the tests

PLAY-002, PLAY-006 (both examples — verified against Roslyn: `public class Sample{ int x }` gives `line 1, column 28 — CS1002: ; expected`, span `[27..28)`, the `}`; `…return a+b}` gives `line 1, column 59 — CS1002` span `[58..59)` and `line 1, column 60 — CS1513` empty span `[59..59)`), PLAY-010 (both examples; the second parses without error under PLAY-005's options, `DEBUG` undefined, so `inactive text` is `DisabledTextTrivia`; token kinds checked), PLAY-011, PLAY-012, PLAY-013, PLAY-014 (the three examples; the PLAY-008 example's reported span is `[41..48)`, exactly `/* c */`).

The issue's illustrative `line 1, column 26 — CS1002` named no input; the spec's numbers are the measured ones.

Notes for the test implementer and implementer (not decisions):
- Today's tests read the output through `textarea` `value`; the output is no longer a text area (PLAY-002), so those assertions move to the code view's text.
- `FormattingRunsWithoutServicesOrJavaScriptInterop` (PLAY-001) asserts no JS interop call during formatting. PLAY-010 forbids only loading a library or anything from another origin; keeping the scroll alignment of PLAY-011 free of JS interop (a CSS overlay where the text area and the highlighted copy scroll in one container) keeps that test as is.
- Parser messages must be English regardless of the test host's culture (format with the invariant culture).
- New types that implement PLAY behavior (a highlighter, an error-list model) belong under `Code owners` of `playground/playground.md` and its row in `sdd/specs/README.md`; I could not add them before they exist.

## Output questions

**1 · Playground · error marks after the input is edited (PLAY-014)**

Input, formatted with "Format", then the user types `int y;` at the start of the text:

```csharp
public class Sample{public int Add(int a,int b){return a+b}
```

Before (`main`): no marks exist.

After, A (recommended): the marks disappear at the first edit and come back only with the next "Format"; the alert box stays until the next format, as today.

After, B: the marks stay on the same character offsets until the next "Format" — after the insertion above they sit six characters left of the `}` they were about.

After, C: syntax-error marks are recomputed on every keystroke from the live parse (the unsupported-shape mark still only on "Format"), so the marks and the alert box's list disagree while typing.

Why A: a mark that no longer points at the code it names is worse than none, and C brings back the flashing errors PLAY-004 avoids.

**2 · Playground · colour of `var`, `nameof`, `dynamic`, `value` (PLAY-010)**

Input:

```csharp
class C { int P { set { var x = nameof(value); } } }
```

Before (`main`): no colours.

After, A (recommended): `var`, `nameof`, and `value` in the identifier colour — the parser gives them identifier tokens, and the Playground has no semantic model.

After, B: `var`, `nameof`, `dynamic`, and `value` in the keyword colour where a cheap syntactic check of their position says they act as a keyword (a declaration's type `var`, an invocation `nameof(…)`, `value` inside a setter).

Why A: the token kind is the one rule for every token; B needs per-word position rules, and gets it wrong for a type or variable actually named `var` or `value`.

Neither answer blocks the tests of the other requirements; PLAY-010 and PLAY-014 carry a coverage sentence for the open case, which the lead replaces with the answer.

The other visible decisions are listed in the change doc for the lead's output questions: the seven categories with punctuation, operators, and whitespace in the normal text colour; error ordering by position with duplicates kept and warnings omitted; an empty error span marked one character wide at its position; marks in the input only.

## Internal choices

- PLAY-002 drops the spell-check requirement for the output: a read-only code view has none to turn off. Alternative: keep it as an attribute requirement on the code view.
- PLAY-013 is one requirement for the box of all three messages instead of editing "below the button" in PLAY-006, PLAY-008, PLAY-009 each. Alternative: restate the box in each.
- PLAY-010 states each category by token or trivia kind (cause), with the keyword rule "the parser gives the token a keyword kind", instead of a list of words.
- The colours themselves stay styling (existing non-goal); PLAY-010 requires a distinct colour per category in each colour scheme the page supports.

## Follow-up candidates

_None._
