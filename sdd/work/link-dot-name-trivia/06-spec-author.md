Role: spec author
Commit: 8c79ce97
Verdict: change doc written

Inputs read: `00-lead.md` (*Decisions taken*, rows 3 and 4), `05-implementer.md` (*Disputed tests or scope*, item 2), `01-spec-author.md`, `03-spec-author.md`, the change doc, `MethodChainAlignmentContributor`, `FluentChain.GetAnchorLink`. The prompt carried no intended fix or verdict.

Written: change doc `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md`, *Special cases / Checked and kept*, a new entry for the same-line condition `LayoutComputer.GetLine(anchorDot) == LayoutComputer.GetLine(firstDot)` in `MethodChainAlignmentContributor.GetChainAnchorColumn`. **Kept**, not deleted. No spec edit.

## Finding

The anchor (`FluentChain.GetAnchorLink`) is the first invoked link that does not start a line, or the last non-invoked link in front of the first line-starting link. Its dot never starts a line, but a link in front of it can span lines. With the shape refused and every other dot-to-name gap joined (FMT-BREAK-020), the only remaining way is something multi-line inside a non-invoked link before the anchor: an element access whose index spans lines. No comment is needed.

## Evidence

All runs in a scratch worktree of 8c79ce97 (`<scratchpad>/wt`). Nothing was changed in the branch's code.

1. **Probe.** The unequal-line branch was replaced by `throw new InvalidOperationException("PROBE-UNEQUAL-LINE")` (prevToken is an initializer's `}` that is first on its line, and the anchor is on another line than the first dot). Whole-document runs through `scripts/format.sh`:

   | Input (behind `new Holder { … }` with `}` on its own line) | Probe hit |
   |---|---|
   | `}.Items[` ⏎ `i].ToString();` | no (line-break phase joins the index onto `[`) |
   | `}.Items[Pick(i,` ⏎ `i)].ToString();` | **yes** |
   | `new[] { … }[0][` ⏎ `i].ToString();` | no (no dot follows the brace) |
   | `}.Map["""` ⏎ `key` ⏎ `"""].ToString();` | **yes** |
   | `}.Items![` ⏎ `i]!.ToString();` | no |
   | `}.Items?[` ⏎ `i].ToString();` | no |
   | `}.Map[@"a` ⏎ `b"].ToString();` | **yes** |
   | `}.Map[$"{i` ⏎ `}"].ToString();` | **yes** |

2. **Without the condition.** With the condition deleted (always rebasing), the same inputs plus a wrapped later link (`… ].Trim()` ⏎ `.ToString();`), also with the statement indented wrongly so the initializer must move:

   ```csharp
   class C
   {
       void M(Dictionary<string, string> d)
       {
                   var x = new Holder
                           {
                               Map = d
                           }.Map[@"a
   b"].Trim()
      .ToString();
       }
   }
   ```

   HEAD (with the condition) leaves `.ToString()` under `.Trim()` (column 3, counting from 0), and a second pass changes nothing. Without the condition the first pass puts `.ToString();` at the start of its line, and the second pass moves it back under `.Trim()`: not idempotent. The multi-line-argument and raw-string variants gave the same output either way, so the verbatim input is the one that shows the condition is needed.

3. **Existing tests.** The formatter project with the probe in place (Debug): 6825 passed, 1 failed, and the failure is `ConditionalAccessAnchorOnLaterLineThanInitializerCloseBraceStaysIdempotent`, which the precheck now refuses, so the probe does not decide it. **No other test reaches the unequal-line branch.** Once that test is rewritten to expect the refusal (*Decisions taken*, row 3), the condition has no test.

Node-level inputs: not tried separately. The condition is kept because a whole document reaches it, and a node-level input cannot make a kept condition deletable.

## Changed requirements

None. The condition's behavior is FMT-INDENT chain alignment already specified, and the change doc records the decision.

## Examples for the tests

For the test implementer, along with the rewrite in row 3: one regression test in `MethodChainAlignmentTests` keeps the branch covered. Input: the verbatim block above. Expected: today's output, with columns counted from 0: the statement at column 8, the initializer under `new`, `}.Map[@"a` at column 16, `b"].Trim()` at column 0, and `.ToString();` at column 3 (under `.Trim()`). It should assert idempotency, as the replaced test did. The test name and summary should not claim that a comment is the only way to reach the branch. That claim in the old test's summary is wrong now.

## Output questions

_None._ Keeping the condition changes no output.

## Internal choices

- Listed under *Checked and kept* with the verbatim-string input as the reason. Alternative: delete it, which this evidence rules out because the output is no longer idempotent.

## Follow-up candidates

_None._
