Role: spec author
Commit: 24e0131
Verdict: spec written

Follow-up round. My inputs were the maintainer's answers in `00-lead.md` (*Maintainer answers* 2 and 3) and *Decisions taken* rows 1–4. The prompt carried no intended fix or hoped-for verdict. I carried the answers into the specs and the change doc and set the change doc to `Status: Accepted`, as the prompt asked.

## Changed requirements

- **SHARED-USING-009** — changed again (answer 5B).
  - New title: *Blocks with preprocessor directives among their directives or disabled text in front of them are not reordered*.
  - A block is now also unsafe when the leading trivia of the first token of its first directive holds disabled text. I took that boundary from the text of option B as it was offered: "disabled text in front of the block would count as unsafe".
  - The conditional-group example is now negative: reported, no fix, in both builds.
  - New positive example: a group that encloses the block (`#if !LEGACY` in front, `#endif` behind). It leaves no disabled text in front, so it is reordered. This is the other side of the qualifier (FC-002).
  - *Why* gives the reason for the disabled-text rule.
- **FMT-USING-012** — rewritten (answers 3A and 4B, decision 4).
  - New title: *The leading trivia of a block with a directive in front stays at the top*.
  - When the leading trivia of the original first directive holds a preprocessor directive, all of it stays at the top: directives, comments and blank lines. FMT-USING-009's blank-line split does not apply to it.
  - A leading trivia without a directive keeps FMT-USING-009 unchanged.
  - Examples: comment header + blank + directive; directive + blank (skeptic suggestion, decision 3); comment directly below the directive (3A); comment behind a blank line below the directive (4B); `#region` in front.
  - Removed: the conditional-group example and the old "split behind the last directive" text.
  - The wording item "that using directive" is applied (decision 3).
- **FMT-USING-009** — clarified with one sentence: when the leading trivia holds a preprocessor directive, FMT-USING-012 applies instead.
- **FMT-USING-004** — example added: a conditional group with disabled text in front, and a shared line, so only the line is split. Normative text unchanged.
- **RH7201-001 … RH7207-001** — "between its directives" is now "among its directives" (decision 3).
- **RH7207-001** — the header sentence adds "with no disabled text there".
- **RH7201-004 … RH7207-004** — the whole leading trivia of the original first directive is kept at the top when it holds a preprocessor directive (FMT-USING-012). The no-fix sentence adds "or with disabled text in front of its first directive". The fixed-block sentence adds "with no disabled text there".
- **Change doc** `sdd/changes/2026-11/using-header-directive-reorder.md` — now `Accepted`.
  - *Affected requirements* adds FMT-USING-009 (clarified).
  - *What* and *Why* are updated.
  - Case 5's text is updated. Case 6's after is now 4B.
  - Case 7 is now "nothing visible changes".
  - New case 8: the enclosing group.
  - *Decisions* now has: the batch answers; the reading of "after its usings" (row 1); disabled text in front; the whole leading trivia; RH7207's blank lines; not declaring the shape unsupported (6A).
  - *Alternatives* is updated to match.

Rule pages are unchanged, for the same reason as in round 01.

## Examples for the tests

- SHARED-USING-009: the changed conditional-group example, now negative in both builds, and the new enclosing-group example.
- FMT-USING-012: all five examples. It is still the only failure of `scripts/test.sh --project architecture`: "FMT-USING-012: no test covers it".
- FMT-USING-004: the new disabled-text example.
- RH7201–RH7207 -004: the header example at the end of each spec. RH7207-001: the header block that is now reported.

Checks I ran on 24e0131:
- `scripts/apply-fix.sh RH7203` on the enclosing group and on the FMT-USING-004 example: `no-fix-offered` today.
- `scripts/format.sh` on both: today the formatter keeps the enclosing block, and it splits the FMT-USING-004 shared line exactly as the new example shows.
- The new expected outputs are stable under today's formatter: FMT-USING-012 directive + blank, the 4B output, and the enclosing-group reorder.

## Output questions

_None blocking._ The lead should decide whether one case needs asking:

**Change doc case 8 · RH7201–RH7207 fixes and formatter — a conditional group that encloses the block.** No question asked this case. I wrote it as covered by answer 1 and decision row 1: the `#endif` stands behind the block, and nothing in front of the block is disabled text.

Input, in a build that does not define `LEGACY`:
```csharp
#if !LEGACY
using System.Linq;
using System;
#endif
```
Before (`main`): reported, no fix offered, the formatter keeps the block.

After, as specified:
```csharp
#if !LEGACY
using System;
using System.Linq;
#endif
```
The other reading, "every conditional directive in front of the block is unsafe", gives: unchanged, no fix offered.

Why the spec's reading: option B as offered named disabled text as the criterion, and this group excludes nothing in front of the block.

## Internal choices

- I took the 5B boundary as "disabled text in the leading trivia of the first token of the first directive", from the offered option's text. The alternative was "any conditional directive in front of the block". It differs only for groups without disabled text in front, such as case 8.
- I read 4B as "the whole leading trivia stays at the top", which is the simplest rule that gives the 3A and 4B outputs ("don't make it complicated"). The alternative was a split at a second blank line behind the directive. No answered case tells the two apart.
- I added no negative disabled-text example to each RH72xx-004. They reference SHARED-USING-009, which holds it.

## Follow-up candidates

_None._
