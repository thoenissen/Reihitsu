Role: skeptic
Commit: 7dd2523
Verdict: #966: proceed (one spec example needs amending first); #1289: proceed (one wording amendment needed; one structural recommendation)

This pass challenges the spec diff `eeafb94..7dd2523` of `sdd/specs/` and `sdd/changes/` against the code and the earlier handovers. My prompt carried no intended fix or hoped-for verdict. Fixtures were written to the scratchpad and deleted afterwards.

## thoenissen/Reihitsu#966

### Premise

Unchanged from `01-skeptic.md`: real, reachable through RH7201–RH7207, their code fixes, and the formatter's using phase; demanded by SHARED-USING-009. The clarified SHARED-USING-009 now states both qualifiers explicitly (token-wise reach, comment text is not a directive), each with an example on its side (FC-002). I checked the new text against the one owner of the decision, `UsingDirectiveOrderingSafety.CanSafelyReorder` (`src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs`): it is the only reorder-safety check every surface calls (`rg CanSafelyReorder`: RH7201, RH7202, RH7204, RH7206, RH7207 analyzers and the rest through the same helper, `UsingDirectiveCodeFixUtilities`, `UsingDirectiveOrderingCodeFixProviderBase`, the RH7207 provider, `UsingDirectiveOrderingRewriter`), so the sentence "Only a preprocessor directive makes the block unsafe" claims nothing the dispatch does not show.

Checking the two new examples, both run with `scripts/format.sh` (two passes) and `scripts/apply-fix.sh RH7203` (LF and CRLF):

- **`# Header` example — correct.** With the `#` (today): `no-diagnostic` under LF and CRLF, and the formatter keeps the order; this is the bug. The control without the `#` (`Header` as the comment line): `fixed (1 action(s), 1 iteration(s))` under LF and CRLF, output `/*`/`Header`/`*/`/`using System;`/`using System.Linq;` — header kept on top, exactly the example's After; the formatter produces the same. The reported span `using {|RH7203:System|};` follows the spec's existing RH7203 convention (`using-directive-ordering.md:588`).
- **B1 example — wrong as written.** The spec says the block "is left unchanged". The formatter leaves the order alone but moves `    .Text;` to column 0:

  ```text
  -    .Text;
  +.Text;
  ```

  The second pass is stable. "Left unchanged" in SHARED-USING-009's other examples means formatter output = input, so a formatter test derived from this example fails. Amendment (spec author): write the input in the formatter's stable shape (`.Text;` at column 0), or replace "is left unchanged" by "is not reordered and not reported". The column of a continuation line behind a directive is the indentation phase's existing output, not something this run changes, so nothing visible is in question.

### Mechanism and candidates

Unchanged from `01-skeptic.md`. No new candidate from the spec text.

### Breaking inputs

`01-skeptic.md` matrices A (A1–A8) and B (B1–B3) stand. One member to add to matrix A, derived from the specs, not run (it is blocked today, and the test implementer must compile it):

- **A9 — the `#`-led comment line is a line another build reads as a directive.** A namespace-scoped block (block or file-scoped namespace) whose `/* */` comment holds a line `#else`, `#elif …`, or `#endif`, where the comment starts inside a conditional group opened before the `namespace` and closed after the last using. Only a namespace scope reaches this: in the compilation unit, every directive before the first using is in that using's leading trivia, so the block stays unsafe. The block's trivia then holds no directive, so the clarified SHARED-USING-009 demands the diagnostic; the document holds a hiding carrier, so SHARED-FIX-012 withholds every fix (enforced centrally in `ReihitsuCodeFixProviderBase`, which `UsingDirectiveOrderingCodeFixProviderBase` and the RH7207 provider derive from), and FMT-PIPE-033 makes whole-document formatting return the document unchanged. Expected: reported, no fix offered, formatter output unchanged. This is how every unordered block in a hiding-carrier document behaves today, so it is no conflict with SHARED-USING-009's `Why:`; it needs a test because the regex was the only thing that silenced this member, and a reorder there would move a line another build reads as `#else`.

Dropped (no caller reaches it): a shebang `#!` or a `#:` file-based-app directive before the first using. Both `scripts/format.sh` ("Skipped 2 file(s) with syntax errors") and `scripts/apply-fix.sh RH7203` (`parse-error`, LF and CRLF) see a syntax error under the default parse options.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/using-directive-ordering.md` SHARED-USING-009 | must change: B1 example amendment (above) |
| Everything else | as in `01-skeptic.md` |

### Refuse option

Unchanged from `01-skeptic.md`: refusing would keep a guard that gives away safe ground for ordinary input (a Markdown-like header comment); declaring a shape unsupported deletes no special case and is not an option.

### Output questions

_None._ Matrix A's new reports, fixes, and reorders are demanded by SHARED-USING-009; A9 is decided by SHARED-USING-009, SHARED-FIX-012, and FMT-PIPE-033; matrix B and the B1 example keep today's output.

### Internal choices

1. B1 example: write it in the formatter's stable shape (recommended: the example then still reads "is left unchanged", like its neighbours) or reword it to "is not reordered and not reported". Either way, no visible output changes.

### Issue goals

No change against `01-skeptic.md` and decision 3.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1289

### Premise

The amended RH2109-001 now states today's behavior; I checked every new sentence against `FindCodeDirectives` (`src/Reihitsu.Analyzer/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs`):

- Reported direction: any `@` that `StartsLine` (only spaces and tabs before it) and matches `\G@code\s*\{` is reported; the scanner keeps no C# state. Example 1 (lines 1, 3, 7) is the shape of the existing `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported` (reported on 1, 3, 7), with the single-line `"@code{"` string replaced by a nested `@code {`/`}` pair in the comment; it holds.
- Converse direction: `@*` with `StartsLine` skips to the next `*@`, `<!--` with `StartsLine` skips to the next `-->`, each to the end of the text when its closer is missing (`SkipPast`). Example 2 holds: line 3 `    @* not a Razor comment` starts its line after spaces, no `*@` follows, so the `@code` on line 6 is not reported. No test covers it yet.
- Existing tests tagged RH2109-001 that put an opener inside C# (`VerifyHtmlCommentOpenerInCSharpCommentOfControlFlowBlockDoesNotHideDirective`, `VerifyRazorCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirective`, `VerifyHtmlCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirective`, `VerifyCommentAndStringStartsInsideLiteralsStartNothing`) all place it behind other text on its line, so they agree with the new paragraph; none contradicts it.

### Mechanism and candidates

_N/A_ (no defect; the amendment records shipped behavior). Candidate axes as in `01-skeptic.md`.

### Breaking inputs

**Wording defect in the converse bullet — two readings, different output.** The bullet says a line-starting `@*` or `<!--` hides a `@code` "up to the next `*@` or `-->`". The code pairs each opener with its own closer only. The spec text also admits "whichever of the two comes first", and that reading gives different output:

```razor
@code {
    /*
    @* x
    */
}
<!-- -->
@code {
}
```

Code (and the maintainer's answer, which records today's behavior): `@*` runs to a `*@` that never comes, so only line 1 is reported. "Either closer" reading: the comment ends at `-->` on line 6, so line 7 is reported too. This is no output question — the maintainer chose to record today's miss — but the spec has to say it unambiguously. Amendment (spec author): "up to the `*@` that closes an `@*`, or the `-->` that closes an `<!--`, or up to the end of the file when that closer does not follow". The change doc's *What* bullet carries the same phrase and needs the same fix. Test candidate: the input above (only line 1 reported), so the pairing is pinned (FC-002).

Test candidates for the decided result, besides retagging the characterization tests `01-skeptic.md` lists: example 2 (`@*` variant); its `<!--` variant; the pairing input above; the issue's own shape (FC-034: `@code {` starting its own line inside the literal, as in `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported`, line 7); CRLF for at least one of them (FC-033).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/analyzer/RH2109.md` | must change: closer wording (above); see *Internal choices* 1 |
| `sdd/changes/2026-11/rh2109-csharp-lines-read-as-markup.md` | must change: same closer wording; affected requirements follow *Internal choices* 1 |
| `sdd/specs/shared/diagnostics.md` SHARED-DIAG-003 | must stay as amended; see *Internal choices* 2 |
| RH2109 analyzer | must stay (no production change) |
| Rule page, `general-notes.md` | not involved |
| Tests | characterization tests become requirement-tagged; new tests above |

### Refuse option

Already chosen: the maintainer answered A (the class stays reported, the converse miss is recorded). No production change, no new type. Declaring a shape unsupported is not an option (the formatter never receives `.razor` text).

### Output questions

_None._ The closer-pairing ambiguity is resolved by the maintainer's answer (record today's behavior), so it is a wording fix, not a question.

### Internal choices

1. **One requirement or two.** The new paragraphs carry their own `Why:` (the cost of lexing C# in Razor text) inside RH2109-001, whose existing `Why:` is a different reason (behavior belongs in a code-behind file). `sdd/README.md`, *Spec format*: "A requirement is one decision with its own `Why:`. Two decisions with different reasons are two requirements." Recommendation: move the new paragraphs, their `Why:`, and both examples into a new requirement RH2109-004 ("A line inside a multi-line C# comment or literal is read like markup"), add the ID to `sdd/specs/requirement-ids.txt`, point RH2109-001's first paragraph at it ("RH2109-004 decides instead"), list RH2109-004 under the change doc's affected requirements, and tag the retagged characterization tests and the new tests with RH2109-004. Consequence of keeping it in RH2109-001: the spec checker is likely to flag the two `Why:`s. No visible output either way.
2. **SHARED-DIAG-003's pointer sentence.** It reads as description ("RH2109 reads a Razor file as markup …; RH2109-001 states how …") next to a MUST that RH2109 now departs from for one class. Recommendation: say plainly that the rule's requirement is an exception to this one for that class (e.g. "RH2109 is an exception for a line inside a multi-line C# comment or literal of a Razor file: RH2109-00N states how it reads such a line."). Wording only.

### Issue goals

No change against `01-skeptic.md`: "Required output: not reported" is dropped by the maintainer's answer A; "Done when the input has a regression test with the decided result" is kept and met by retagging `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported`.

### Architecture findings

_None._

### Follow-up candidates

The spec author's candidate (a real HTML or Razor comment opened behind other text on its line does not hide a following `@code {`) stands; I checked it against the code: `FindCodeDirectives` treats an opener as one only when `StartsLine` holds, and its summary states that decision; the spec records it nowhere. No further candidates.
