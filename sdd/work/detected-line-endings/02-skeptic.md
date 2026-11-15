Role: skeptic
Commit: cf433bfa7b33fb2e697697147c8a3f4b2011c4b0
Verdict: #1695: proceed (line ending, one more failing member found) + output question (whitespace, reframed); #1696: output question; #1707: proceed narrower (RH3003 only, RH3005 does not reproduce); #1721: do not make the change (specified, already pinned by existing tests)

My prompt carried no intended fix or verdict. The spec author's handover carries recommendations; I checked them against code and runs and say below where I differ. All probes ran outside the repository (fixtures in the session scratchpad, deleted afterwards): `scripts/apply-fix.sh` for LF/CRLF code-fix behavior, `scripts/format.sh` for whole documents, and, for lone-`\r` and mixed documents that `apply-fix.sh` normalizes away (FC-033), a throwaway file-based app that called `ReihitsuFormatter.FormatNodeInDocumentAsync` / `FormatNodeInDocumentWithContextAsync` with the node a code fix passes (allowed as a check by `AGENTS.md`, *Supported inputs*, last bullet).

## thoenissen/Reihitsu#1695

### Premise

Real, both parts, reached through code fixes.

- **Line ending.** `ReihitsuFormatter.WriteBackTrailingTrivia` (`src/Reihitsu.Formatter/ReihitsuFormatter.cs:544`) writes `originalLastToken.TrailingTrivia` back verbatim; the general path of `FormatNodeInDocumentWithContextAsync` does the same (`ReihitsuFormatter.cs`, `formattedLastToken.WithTrailingTrivia(originalLastToken.TrailingTrivia)`, ~line 368); the split-field path `FormatSplitFieldInContext` does it too when the last field does not share its line (`trailingTrivia = ... : originalLastToken.TrailingTrivia`, ~line 777; the shared-line branch already writes `DetectEndOfLine(root)`). Observed:
  - CRLF document, `\n` behind property `P`, `FormatNodeInDocumentAsync(P)`: every line of `P` is `\r\n`, the line behind its `}` stays `\n` (the spec example's "before").
  - Lone-`\r` document, same target: `P`'s lines are `\n`, the line behind its `}` stays `\r`.
  - Same with a trailing comment: `} // c\n` keeps `\n`.
  - `FormatNodeInDocumentWithContextAsync`, enum member `B =\r\n2,\n` in a CRLF enum (RH5111 shape): result `B = 2,\n`.
  Reachability: every formatter-delegating fix (26 files under `src/Reihitsu.Analyzer.CodeFixes`, 33 call sites) in a document with mixed or lone-`\r` line breaks. The class is rare.
- **Whitespace.** `scripts/apply-fix.sh RH3202` on `public int GetValue() => _value;···` (LF and CRLF): result ends `}···`. `RH5404` on `class D { int x; }···` (nested type, original last token kept): result ends `}···` too. Whole-document formatting removes both.
- **Severity, line ending.** Overstated in the issue. In a mixed document the fix leaves as many foreign line breaks as there were (the same one, at the end of the target's last line); in a lone-`\r` document the result is mixed with or without the change, because the lines outside the target keep `\r` (FMT-PIPE-010). The value of the change is conformance with SHARED-FIX-008, which already names "its trailing trivia included" and the lone-`\r` case; it is cheap.
- **Severity, whitespace.** Also overstated: the whitespace was already on the target's line before the fix, and RH5602 (*Code must not contain trailing whitespace*) reports it there before and after the fix. The fix moves an existing finding from one line to another; it creates none.

### Mechanism and candidates

Mechanism (one class for both parts): the node-level entry points join text the phases wrote — every line break in the detected line ending, no whitespace before a line break — with text they keep from the document at the target's boundary, without bringing the kept text to what the phases would write; the trailing trivia is written back after the last phase, so neither the line-ending preparation nor the cleanup phase sees it.

Dispatch: the three write-back sites above, and at the leading boundary the join of the preceding token's trailing trivia (kept) with the leading trivia the phases write for a target of `FormatNodeInDocumentAsync` that starts its line and is not a direct child of the root (`keepsLeadingTrivia == false`).

### Breaking inputs

FC-033 (harness normalizes line endings: `VerifyCodeFixWithEndOfLine` and `apply-fix.sh` write one uniform ending, so mixed and lone-`\r` cases need `ApplyCodeFixAsync` on exact text or a formatter unit test on the entry point), FC-017 (LF-only assertions), FC-016 (sibling write-back sites), FC-028.

Class matrix — boundary × entry point × document shape (observed unless marked):

| # | Member | Result today |
|---|---|---|
| T1 | Trailing, `FormatNodeInDocumentAsync`, CRLF document with `\n` behind the target | fails (`}\n`) |
| T2 | Trailing, `FormatNodeInDocumentAsync`, all-lone-`\r` document | fails (`}\r`) |
| T3 | Trailing, as T1 with a comment behind the target (`} // c\n`) | fails |
| T4 | Trailing, `FormatNodeInDocumentWithContextAsync` general path, CRLF document with `\n` behind the target | fails (`B = 2,\n`) |
| T5 | Trailing, `FormatSplitFieldInContext`, last field not sharing its line | fails by code (same verbatim write; not run) — reached by the RH7101 fix |
| T6 | Trailing, LF document with `\r\n` behind the target | fails by code (not run) |
| T7 | Trailing trivia without a line break (target does not end its line) | not a member |
| **L1** | **Leading, `FormatNodeInDocumentAsync`, target starts its line, preceding line ends in a lone `\r`, blank line above the target** | **fails: the blank line is lost.** Lone-`\r` class with `int P;\r\r    int Q { ... }`, target `Q`: result text `int P;\r\n    int Q\n...` — the phases write the blank line as `\n`, which joins the preceding `\r` into one `\r\n` line break (the hazard FMT-PIPE-019's `Why:` names). Same with a method target (RH3202 shape, `int _v;\r\r    int M() { ... }`) and with an `if` statement target behind `int a = 1;\r\r` (what the RH5403 fix passes, `RH5403...CodeFixProvider.cs:42`). Also in an LF-majority document whose preceding line ends in a lone `\r` (`int P;\r\n` written as `\r` + blank `\n`). Violates SHARED-FIX-006 (blank lines between the node and its neighbours stay) and FMT-PIPE-010's "decides the target's blank lines". |
| L2 | Leading, as L1 but a comment line above the target and no blank line | the LF counterpart inserts a blank line above the comment (see *Follow-up candidates* 1); in the lone-`\r` document that inserted `\n` fuses with the preceding `\r`, so the result shows no blank line and the preceding line ends in `\r\n` |
| L3 | Leading, as L1 without a blank line above the target | holds |
| L4 | Leading, `FormatNodeInDocumentWithContextAsync` (leading trivia kept verbatim, FMT-PIPE-018), lone-`\r` enum with a blank line above the target | holds |
| L5 | Leading, preceding line ends in a lone `\r` and the detected ending is `\r\n` | not a member (`\r` + `\r\n` is two line breaks) |

L1 is not in the issue's text, but it is a failing member of the issue's class reached by RH5403/RH5404/RH3202 and the other `FormatNodeInDocumentAsync` callers in a lone-`\r` document; the lead decides whether it joins the run (*Scope*) or becomes a follow-up. It puts no output in question: SHARED-FIX-006 already demands the blank line.

Whitespace members (RH3202 via `apply-fix.sh`, compared with whole-document formatting of the same file):

| # | Shape behind the target's last token | Fix today | Document formatting |
|---|---|---|---|
| W1 | `}···` (no comment) | `}···` | `}` |
| W2 | `}···// c` (whitespace before a `//` comment) | kept | kept |
| W3 | `} // c···` (whitespace is part of the `//` comment's text) | kept | kept (comment text, FMT-PIPE-002) |
| W4 | `} /* c */···` | kept | removed |
| W5 | `}···` where the target's last token is kept (RH5404 nested type) | kept | removed |

So the decisive notion is "whitespace before the line break that ends the trailing trivia, outside a `//` comment's text" — what FMT-CLEAN-001 removes — not "whitespace behind the last token". W4 must be covered by whatever answer Q1 gets; the issue names only W1.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ReihitsuFormatter` node-level write-back (three trailing sites; leading join for L1 if taken in) | must change |
| Formatter phases | must stay |
| Code fixes calling the entry points | must stay (they inherit the change) |
| Analyzers, rule pages, `Reihitsu.Core` | not involved |
| Spec `sdd/specs/formatter/pipeline.md` FMT-PIPE-010 | changed by ec7c0c6; whitespace sentence pending Q1 |
| Tests | formatter unit tests on both entry points and the split-field path with exact text; one code-fix test per entry point with mixed and lone-`\r` text via `ApplyCodeFixAsync` |

### Refuse option

- Withhold the formatter-delegating fixes where the target's trailing line break is not the detected ending (a cheap check exists: `LineEndingUtilities.HoldsOtherLineBreak`). Costs more than the requested change: the entry point cannot refuse alone (SHARED-FIX-005 forbids an offered fix that changes nothing), so 26 fixes would need the check, against a few lines at three write-back sites. The input class is rare. Not recommended.
- Whitespace: keep it (Q1 option C), zero cost.
- Declaring a shape unsupported: not an option. A document with mixed or lone-`\r` line breaks is normalized by FMT-PIPE-007 in every phase; declaring it would refuse whole files the CLI formats today and would delete no special case — the preparation phase's normalization stays for documents.

### Output questions

**Q1 (whitespace) — I agree it is an output question, but the spec author's A/B split does not hold as stated.** A ("remove only where formatting appended tokens behind the original last token") is claimed to be demanded by SHARED-FIX-007 and B to be a behavior change. But in W5 (case 1b, RH5404) the kept `}` also stands on a line the fix creates — the one-line body became four lines — so SHARED-FIX-007 ("the code a fix changes MUST be laid out as the formatter lays it out … so that formatting the result changes nothing in that code") applies to 1b as much as to 1a. Under A, 1b would violate SHARED-FIX-007 just as today. The case that SHARED-FIX-007 does not cover is a target whose last line the formatting leaves unchanged. So the options should be:

**1 · Code fixes that format their node (e.g. RH3202 · Expression-style methods should not be used, RH5404 · Element must not be on a single line) — whitespace at the end of the fixed code's last line**

Input (RH3202, `·` is a space):
```csharp
class C
{
    private int _value;

    public int GetValue() => _value; /* c */···
}
```

Before (`main`):
```csharp
    public int GetValue()
    {
        return _value;
    } /* c */···
```

After, A' — remove where the fix's formatting changed the target's last line (RH3202 above and RH5404 `class D { int x; }···` both lose it; a target whose last line the fix leaves as written keeps it):
```csharp
    public int GetValue()
    {
        return _value;
    } /* c */
```

After, B (recommended) — remove at the end of every fixed target's last line, as document formatting does: same output for both examples, and also removed where the target's last line is otherwise unchanged.

After, C — keep (today's output).

Why B: one rule for the end of the target's last line, matching the line-ending clarification just made (the trailing trivia is part of the fix's span, SHARED-FIX-008) and the issue's "Done when"; the only extra case over A' is whitespace RH5602 already reports and the formatter removes. What would change the pick: the maintainer reading SHARED-FIX-006 as forbidding any edit on a line the fix's layout leaves unchanged (then A'). A' needs the entry point to tell whether the last line changed, a comparison of the phases' result, which B does not.

Whether B needs a change doc is the lead's internal call: it is a clarification in the same way the line-ending sentence is, if SHARED-FIX-007's "code a fix changes" is read with SHARED-FIX-008's span.

### Internal choices

- **Spec diff (ec7c0c6) — accept.** It states what SHARED-FIX-008 already requires and the code contradicts. Two wording notes, neither changes output: (a) "MUST write the line break that ends it" presumes the trailing trivia ends in a line break; T7 (no line break) is implicitly outside — "when it ends in a line break" would make that explicit; (b) the sentence "A trailing trivia whose line break already is the detected line ending is written back as it stands" is redundant with "keep the target's trailing trivia" and can stay or go. The lone-`\r` prose of the new example is correct as observed (target lines `\n`, outside lines `\r`) for its input; it would not hold for L1-shaped input (blank line above the target) until L1 is fixed.
- The line-ending rewrite need not touch U+2028/U+0085/U+2029 specially: "whatever line break the document has there" already covers a one-character terminator that FMT-PIPE-007 counts as `\n`.

### Issue goals

- "FMT-PIPE-010 states this instead of 'keep the trailing trivia' for these two cases" — done for the line ending; whitespace depends on Q1.
- "a regression test per case (whitespace behind an appended token, a lone-`\r` and a mixed document)" — keep, and widen: both entry points and the split-field path (T4, T5), a trailing comment (T3), and W4. No visible change.
- "the node-level entry points return the same text as document formatting for the end of the target's last line" — that is option B; keep or drop according to Q1. Note it is not true in general of the line ending in a mixed document (document formatting would also rewrite the lines outside the target), so word it as the end of the target's last line only.
- "Affected: … 29 files" — today 26 files, 33 call sites; immaterial.

### Architecture findings

`ReihitsuFormatter` writes the target's trailing trivia back after the last phase, so two decisions the phases own for every other line of the target are made outside them for that trivia: its line ending (preparation phase, `PreparationPhase/LineEndings`, FMT-EOL) and the whitespace before its line break (`CleanupPhase`, FMT-CLEAN-001). Fixing #1695 at the write-back sites restates both rules in the entry point — the "copies another's behavior" kind (`README.md`, *Architecture findings*). The line-ending part is small and has a precedent in the same file (`FormatSplitFieldInContext` already writes `SyntaxFactory.EndOfLine(DetectEndOfLine(root))` on its shared-line branch); the whitespace part (if Q1 is A' or B) replays FMT-CLEAN-001. The lead decides narrow fix vs structural change; I propose no design.

### Follow-up candidates

1. **`FormatNodeInDocumentAsync` callers insert a blank line above a comment that leads their target** (LF and CRLF; observed with RH5403 and RH5404 through `apply-fix.sh`). FMT-PIPE-010/FMT-PIPE-024 say this entry point decides the target's blank lines; SHARED-FIX-006 says a fix MUST NOT insert a blank line before the node it changes. Outside this run's issues.

   **RH5403 · Statement must not be on a single line — comment line above the statement**

   Input:
   ```csharp
   int a = 1;
   // c
   if (a > 0) { a++; }
   ```
   Before (`main`):
   ```csharp
   int a = 1;

   // c
   if (a > 0)
   {
       a++;
   }
   ```
   After, A (recommended): no blank line inserted above `// c`. After, B: as today.
   Why A: SHARED-FIX-006 forbids it, and FMT-PIPE-018 already keeps the leading trivia for the other entry point.
2. If the lead does not take L1 into the run: the leading-boundary fusion above, as its own bug against SHARED-FIX-006 (no output question).
3. The spec author's "leading trivia written back with foreign line endings" candidate (in `00-lead.md`): FMT-PIPE-018 already decides it for `FormatNodeInDocumentWithContextAsync` ("The write-back also restores the tabs and line endings the preparation phase rewrites"), and L4 confirms the code does that. It is not open for that entry point; for `FormatNodeInDocumentAsync` with kept leading trivia it is the same reading of FMT-PIPE-010. I would drop it, or file it only as a spec-consistency note between SHARED-FIX-008's span and FMT-PIPE-018.

## thoenissen/Reihitsu#1696

### Premise

The issue's description of today's behavior follows from the code (`RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs:51` uses `CopyrightHeaderTemplateResolver.NormalizeLineEndings`, which takes the first line break found in the file, `CopyrightHeaderTemplateResolver.cs:78`; `GetPreferredLineEnding`, `:105`, takes the first line's break or `\n`). It is not run end to end: `apply-fix.sh` normalizes line endings, and the fix needs an `AdditionalFiles` configuration. The test implementer reproduces it with `ApplyCodeFixAsync` on exact text.

But the premise "this is a bug against SHARED-FIX-008" is half right: RH8402-002 and RH8402-005 specify the first line's ending explicitly, as the spec author found. Two unchanged requirements, two outputs: an output question.

Severity — the mixed case is worse than the issue says. Today's header can flip the file's predominant line ending. I formatted today's fix result for `namespace N;\nclass C\r\n{\r\n}\r\n` (first line LF, three CRLF): the header adds three LF, so FMT-PIPE-007 detects LF and `scripts/format.sh` rewrote every CRLF of the file to LF. The one-line-file case is invisible on Linux (`Environment.NewLine` is `\n`) and differs only on Windows. The lone-`\r` case is rare.

### Mechanism and candidates

The fix chooses the header's line ending from the first line break of the file (two private helpers), not from the detection FMT-PIPE-007 and every other fix use (`LineEndingUtilities.DetectEndOfLine`). Candidates — document shape × configured header ending in a line break or not × what starts the file (code; comment run with `copyright`; other comments; directive or documentation comment):

| Document shape | First-line ending vs detected | Differs today |
|---|---|---|
| all LF / all CRLF | equal | no |
| mixed, first line in the majority ending | equal | no |
| mixed, first line in the minority ending | differ | yes |
| all lone `\r` | `\r` vs `\n` | yes |
| no line break | `\n` vs `Environment.NewLine` | only on Windows |

The other two axes change which line breaks are written (header lines, separator, header's own last line break) but not the choice; each failing shape should be tested with a header that ends in a line break and one that does not.

### Breaking inputs

FC-033 (the verifier's `VerifyCodeFixWithEndOfLine` cannot express mixed or lone-`\r` text; use `ApplyCodeFixAsync`), FC-015 (on B, the fix's private detection becomes a copy of `LineEndingUtilities.DetectEndOfLine` that must go), FC-017 (a one-line-file test must compute `Environment.NewLine`, not hard-code `\n`, or it cannot fail on Linux).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8402 code fix | must change on B; must stay on A |
| RH8402 analyzer and its use of `NormalizeLineEndings` | must stay (after a B fix, the file's first line break is the header's own, so the comparison in the first line's ending still matches) |
| RH8402 spec (RH8402-002 fix part, RH8402-005) | must change on B, with a change doc (lane widens) |
| Rule page | not involved (no line-ending text) |
| Formatter, `Reihitsu.Core` | not involved (the detection exists) |

### Refuse option

Withhold the RH8402 fix in a document whose line breaks are not uniform (cheap check exists). Cost similar to B, but it withholds a whole header insertion for a rare but harmless shape; B is better. No unsupported shape applies (no formatter phase is involved).

### Output questions

The spec author's Q2 stands; I agree with recommendation B, with two additions for the question's *Why*: (1) A can flip a mixed file's predominant line ending, after which the formatter rewrites every line of the file (observed above); (2) RH8402-005's `Why:` gives no reason for the first line's ending — RH8402-002's `Why:` justifies comparing in the file's own ending, which B keeps for the analyzer. For the one-line file, the *After, B* line should say "the line ending of the machine the fix runs on", so the maintainer sees that B makes that output machine-dependent where A writes `\n` everywhere.

### Internal choices

On B, the analyzer keeps RH8402-002's first-line comparison (recommended; it still matches after the fix and changes no diagnostic).

### Issue goals

- "the `GetPreferredLineEnding` copy is replaced by the shared detection" — a prescribed solution; on B keep it (also `NormalizeLineEndings` as called by the fix, not by the analyzer); on A drop it and link `Refs`.
- One-line-file regression test "shows the detected ending" — only meaningful with `Environment.NewLine` computed in the test.

### Architecture findings

_None_ beyond FC-015 above, which exists only once B is chosen.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1707

### Premise

- **RH3003: real.** `scripts/apply-fix.sh RH3003` under CRLF: `return a +\r\n            /* x */ "";` gives `CRLF=7, LF=1` (`[line-ending-drift]`), as the issue says. The LF arm holds — on this Linux host the code action's clean-up writes `\n`; on a host whose newline option is `\r\n` the LF arm would drift instead.
- **RH3005: does not reproduce.** Ten shapes under LF and CRLF, none drifts: comment in front of the comparison on the next line (`return\r\n    /* x */ 0 < a;`), comment line above it, comment behind `return`, comment behind the comparison before `&&`, comment after the comparison on its line, inside `if (/* x */ 0 < a)`, behind `&&` with and without a line break, and without a comment. By construction the only SyntaxFactory default trivia the RH3005 fix leaves is the operator token's leading trivia, which sits between the right operand's explicit trailing space and the operator, a gap that cannot hold a line break.
- Reachability: RH3003 through its code fix and Fix All (BatchFixer), in an IDE whose newline option differs from the document's.

### Mechanism and candidates

Mechanism (RH3003): the fix builds `string.Empty` with `SyntaxFactory` default (elastic) trivia on `string`'s trailing and `.`/`Empty`'s trivia, and copies the literal's trivia only to the outside; Roslyn's code-action clean-up then re-formats the gap in front of the new expression, and when the literal's own leading trivia holds a comment and that gap holds a line break, every line break in that gap is rewritten with the host's newline. Membership (observed):

| Shape (CRLF) | Drifts |
|---|---|
| `a +\r\n    /* x */ ""` (line break before, comment in the literal's leading trivia) | yes |
| `a +\r\n    // c\r\n    ""` (comment line between) | yes, two line breaks (`LF=2`: the one behind `+` and the one ending `// c`) |
| `var s =\r\n    /* x */ ""` | yes |
| `M(\r\n    /* x */ "", b)` | yes |
| `a + // c\r\n    /* x */ ""` | yes |
| `a +\r\n    ""` (no comment) | no |
| `a +\r\n\r\n    ""` (blank line, no comment) | no |
| `a + /* x */ ""` / `a\r\n    + /* x */ ""` (no line break in the gap) | no |
| `a + /* x */\r\n    ""` (comment in `+`'s trailing trivia, not the literal's) | no |
| `""` followed by a comment (`"" /* x */;`, `"" // c\r\n, b`) | no |
| `#if DEBUG … #else\r\n    ""` (directive and disabled text in front) | no |

The `// c` member shows the change reaches the literal's own leading trivia, which RH3003-004 says to keep, besides SHARED-FIX-006.

Sibling fixes (FC-016): the code fixes that build nodes with `SyntaxFactory` and do not go through the formatter are RH3001, RH3003, RH3005, RH3006, RH3104, RH7104. RH3001, RH3006, and RH7104 build their tokens with explicit trivia; RH3104 parses its replacement (`ParseExpression`). `apply-fix.sh` with a comment-plus-line-break shape in front of the replaced node holds for RH3001 and RH3104. The issue's claim that RH3003 and RH3005 are the only remaining candidates holds; only RH3003 fails.

### Breaking inputs

FC-033 and FC-017: the test must run under both LF and CRLF (`VerifyCodeFixWithEndOfLine`), because which arm fails depends on the host's newline. FC-021 (comment before the replaced node), FC-019. The test implementer should cover at least the first five failing rows and one holding row from each group.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH3003 code fix | must change |
| RH3005 code fix | must stay; characterization test only |
| Analyzers, formatter, specs, rule pages | not involved (SHARED-FIX-006 and RH3003-004 decide it, as the spec author found) |

### Refuse option

Withholding the RH3003 fix when the literal's leading trivia holds a comment is a cheap check, but it refuses common code (a comment in front of `""` is ordinary) to avoid a fix that is itself small — the RH3006 repair (explicit trivia on the new tokens) shows its size. Not recommended. No unsupported shape applies.

### Output questions

_None._ SHARED-FIX-006 and RH3003-004 demand the unchanged line breaks.

### Internal choices

_None._

### Issue goals

- RH3005 "reproduced first … if it does not reproduce, its test stays as a characterization test" — keep; it does not reproduce, so RH3005 production code stays as is and the PR states that the RH3005 half did not reproduce.
- "Each fix keeps CRLF in a test run under LF and CRLF" — keep for RH3003; for RH3005 the same test is a characterization test.
- The issue's statement of SHARED-FIX-006 ("the line ending of the gaps next to the replaced node stays the document's own") is not the requirement's text; the requirement and RH3003-004 still decide it.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1721

### Premise

No defect. `scripts/format.sh` on the issue's input (all lone `\r`, one multi-line raw string) and on a second input (raw string with an indented content line, plus a method): output has `\n` outside the literal and lone `\r` inside it, the closing quotes aligned under the opening ones, the second pass changes nothing, and the output equals the output of the LF copy with `\r` → `\n`. That is what FMT-PIPE-007 (lone `\r` counts as `\n`, its `Why:`) and FMT-EOL-004/FMT-PIPE-019 (literal line breaks keep their characters) specify; the issue's first reading contradicts an unchanged requirement with a good `Why:`. I also checked the class for the FMT-PIPE-019 fusion hazard (a kept `\r` followed by a written `\n`): lone-`\r` documents with disabled text before `#endif` and `#else`, a verbatim string, a multi-line block comment, a region, and a blank line after `#endif` all give the LF copy's output, with no `\r\n` in the result.

Reachability: the CLI and whole-document formatting.

The "Done when" ("a formatter test pins the result for the all-lone-`\r` document") is already met on `main`: `ReihitsuFormatterLineEndingIndependenceTests.KeptLineBreaksInsideLiteralsCommentsAndDisabledTextAreTheSameInTheCopy` has a row with a multi-line raw string in an all-lone-`\r` file (`§` and `¤` both `\r`), asserts the output equals the LF copy's and keeps the `\r` inside the literal; `ReihitsuFormatterTests.FormatSyntaxTreeWritesLineFeedWhenEveryLineBreakIsALoneCarriageReturn` pins the detection.

### Mechanism and candidates

_N/A._

### Breaking inputs

None found; the members above hold.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, specs, tests | must stay |
| Everything else | not involved |

### Refuse option

_None_ — nothing to refuse.

### Output questions

_None._

### Internal choices

- No new test (recommended): the existing row is the issue's shape. Alternative: one test that pins the issue's exact input and bytes as a characterization test; it adds nothing the existing row does not assert.

### Issue goals

Both goals are met by existing behavior and tests; the PR can say so with the observed output and the existing test names. Nothing visible changes.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- Lane: bug fix holds for #1695 (if Q1 is not answered in a way the lead judges a behavior change), #1707, #1721. #1696 on B widens the run to behavior change (change doc for RH8402).
- Output questions for the batch before the test implementer: Q1 (reframed above) and Q2 (with the additions above).
- No build or test run beyond the probes named above.
