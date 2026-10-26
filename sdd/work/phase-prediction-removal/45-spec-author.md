Role: spec author
Commit: 75cbd71
Verdict: text written

This round is a documentation repair of finding #12 in `44-auditor.md`. The lead routed it under `lead.md` section 6 (`00-lead.md`, "Audit 4 — pass with text cleanup").

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked only that the recorded gap cover the member the auditor shows, with no requirement IDs and no behavior change.

The auditor's member:
- **Target and context.** The argument `Wrap(…)` of `        var x = Call(1, Wrap("""` / `    a` / `    """));`, with the statement as context.
- **What happens.** The phases put the argument on a line of its own. That added line break is not written back, so `Wrap(` stays on the first line. The raw string's content line and closing marker take the phases' column 26, while the quotes stand at 29.
- **Against `main`.** The output is the same as on `main`, so this is the behavior decision `audit-3` keeps.

## Changed requirements

- **FMT-PIPE-017 — clarified (text only).** The "Recorded gap" paragraph is rewritten.
  - **Old wording.** It defined the gap as "a target that starts a later line of its context". It also claimed that a target that does not start its line is outside the gap, and that claim was false.
  - **New definition.** The gap is now a target whose first token the phases place on another line or in another column than it keeps in the document. The paragraph states the mechanism: only the target is written back, with its leading trivia as in the document, and everything in front of it on its line stays (FMT-PIPE-010, FMT-PIPE-018).
  - **Two known cases**, as a list:
    - a later-line target whose line the phases give another column;
    - a target that does not start its line, where the phases break the line in front of it.
  - **The other side of the qualifier.** A target whose first token the phases leave on its line and in its column is not in the gap. Two examples are given: a target that starts the context's first line, and a mid-line target in front of which the phases change nothing on that line.
  - **New example** for the second case, the auditor's member with its columns:
    - from the auditor: quotes at 29, content and closing marker at 26;
    - derived by me from the input: the phases put `Wrap(` under `1` at 21, and the quotes at 26.
  - **The declarator example** is kept and labeled "the first case".
- **SHARED-FIX-007 — clarified (text only).** The recorded-gap bullet now also covers changed code that does not start its line when the formatter breaks the line in front of it. Its sentence about what is outside the gap is narrowed to match: "does not start its line and in front of which the formatter changes nothing on that line".
- **RH5111-004 — unchanged.** Its "not in the gap" sentence names only the first parameter behind `(` and the first declarator behind its type. The auditor found that the RH5111 fix does not split in front of them, so the sentence holds.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` stays **`Status: Accepted`**. What changed:
- **What, node-level bullet.** The gap is now stated as "keeps its first token's line and column in the document, also where the phases place it elsewhere". The bullet names both cases.
- **Decision "The node-level entry points lay out their node at its own column", last paragraph.**
  - The sentence that was false is replaced with the general statement and both known cases. The raw-string argument is the example, with the same output as today.
  - The RH3208 sentence now says the phases keep the local function on the label's line.
  - New sentences: the RH5111 fix does not split in front of a first declarator or parameter, and no code fix was found that reaches the second case.

## Not changed

- **`AGENTS.md`** (outside "spec/change-doc Markdown"). Its sentence names the context entry point's write-back as the one recorded exception, which covers #12. Its "so a target that starts a later line of its context keeps that line's column" names one consequence, and that consequence is true.
- **The `<remarks>` of `FormatNodeInDocumentWithContextAsync`.** The auditor reports that they are true as they stand.
- **No test.** The member is outside every requirement and belongs to the follow-up for restructuring the context entry point.

## Proof

```
TEXT-ONLY PROOF: PASS — base=75cbd71 head=worktree — 3 changed file(s) (3 non-compiled, 0 C# comment/layout-only); xml-doc impact: none
```

(`scripts/verify-text-only.sh --base HEAD --head worktree --strict-docs`.)

`scripts/test.sh --configuration Debug --project architecture` gives `Passed: 89, Failed: 0`.

## Examples for the tests

_None._ This is a text repair, and the new FMT-PIPE-017 example describes the recorded gap.

## Open questions for the maintainer

_None._ Follow-up candidate, an addition to the restructuring of the context entry point: a mid-line target in front of which the phases break the line (#12).
