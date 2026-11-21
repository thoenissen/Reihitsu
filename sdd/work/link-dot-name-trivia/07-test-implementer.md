Role: test implementer
Commit: d0f92bdf
Verdict: tests written

Inputs read: `00-lead.md` (*Decisions taken*, rows 3–5), `04-test-implementer.md`, `05-implementer.md` (*Disputed tests or scope*), `06-spec-author.md`, the change doc's *Checked and kept* entry for the same-line condition, FMT-INDENT-029. The prompt carried no intended fix, cause, or verdict.

## Tests

File `src/Reihitsu.Formatter.Test/Regression/Indentation/MethodChainAlignmentTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentBetweenDotBehindInitializerCloseBraceAndMemberNameIsUnsupported` (was `ConditionalAccessAnchorOnLaterLineThanInitializerCloseBraceStaysIdempotent`; same input, now refused at `// keep`; summary no longer claims the comment is the only way to reach the unequal-line branch) | FMT-UNSUP-016 | `AssertUnsupportedFor` (LF, CRLF) | passes |
| `AnchorBehindMultiLineIndexAfterInitializerCloseBraceKeepsItsOwnColumn` (new; the spec author's verbatim input `}.Map[@"a` ⏎ `b"].Trim()` ⏎ `.ToString();`, statement wrongly indented; expected: statement at column 8, braces under `new` at 16, `b"].Trim()` at 0, `.ToString();` at 3 under `.Trim()`) | FMT-INDENT-029; FC-003 (unequal-line side of the kept condition) | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | passes |

Watched for the right reason: in a scratch worktree of d0f92bdf with `&& LayoutComputer.GetLine(anchorDot) == LayoutComputer.GetLine(firstDot)` removed from `MethodChainAlignmentContributor.GetChainAnchorColumn`, `MethodChainAlignmentTests` ran 104 passed, 1 failed — the new test, on its expected-versus-actual assertion. The worktree was removed; no production code was changed in the branch.

The other side of the condition (anchor on the first dot's line, correction applied) stays covered by the unchanged `ConditionalAccessAfterInitializerCloseBraceAndPropertyAlignsToInvokedLink` and `TrailingDotAfterInitializerCloseBraceRejoinsItsMemberName`, both passing.

## Other existing tests whose input holds the shape (item 3)

None besides the one rewritten above. Searches over `src/*Test*/`, widened from the earlier ones to any token in front of the dot (the earlier `[\w\)\]\?!]\.` missed `}` and analyzer markup such as `{|RH3101:base|}.`) and to blank lines between the dot and the trivia:

- `rg -n --pcre2 '(?<![\w\)\]\?!\.])\.[ \t]*(//|/\*)' src/*Test*/`
- `rg -n -U --pcre2 '^(?![ \t]*///).*(?<![\.\d])\.[ \t]*(\r?\n[ \t]*)+(//(?!/)|/\*|#(if|else|elif|endif|region|endregion|pragma|nullable|line|define|undef|error|warning))' src/*Test*/`

Hits, each checked:

| Hit | Disposition |
|---|---|
| `MethodChainAlignmentTests` (old line 1616) | rewritten above |
| `UnsupportedLinkDotNameCommentTests`, `UnsupportedLinkOperatorCommentTests:520`, `ChainGapJoinTests:489`, `ChainSplitMemberNameRejoinTests:294/323` | already refusal tests from `04-test-implementer.md` |
| `RH3101…AnalyzerTests:784/836` (`base /* a */ . /* b */ M()`, `base. // b`), `RH3105…AnalyzerTests:499/849` (`this` likewise) | left: `VerifyNoCodeFixIsOffered` under RH3101-006 / RH3105's own guard; neither code fix calls `ReihitsuFormatter` (`grep -c ReihitsuFormatter` = 0 in both providers), so the precheck does not reach them |
| `RH6016…AnalyzerTests:727/799` | left, as in `04-test-implementer.md`: the RH6016 fix never calls the formatter |
| `RH2008…AnalyzerTests:141`, `RH7109…ExcludedCodeTests:2796` | not member accesses (a comment ending in `.`, disabled text) |

A test that asserts an unchanged output (`AssertRuleResult(input)` with no expected, `VerifyFormatter(source)`) would pass vacuously under the refusal and not show up as a failure; the searches above are what rules those out, not the green suite.

## Not covered, and why

- Raw-string and interpolated multi-line index variants of the kept condition: the spec author's evidence shows they produce the same output with and without the condition, so they would not guard it; the verbatim input is the one that does.
- Node-level entry points for the kept condition: the condition is kept because a whole document reaches it (`06-spec-author.md`); a node-level test adds no side.
- Other failure classes: unchanged from `04-test-implementer.md`; this round changes no production code and adds no condition.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~MethodChainAlignmentTests"
scripts/test.sh --project formatter
```

Results: `MethodChainAlignmentTests` 105 passed; formatter project 6827 passed, 0 failed.

## Follow-up candidates

_None._
