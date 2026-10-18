Role: skeptic
Commit: a729d6cfbbf369e3cb62a363a1cfedd198a75449
Verdict: proceed narrower

The lead's prompt carried no intended fix and no hoped-for verdict; nothing had to be ignored. GitHub was not accessed (lead's instruction). Duplicates against thoenissen/Reihitsu#957–#960 are judged from the subjects recorded in `00-lead.md`, `01-spec-author.md`, and the FMT-CLEAN/FMT-EOL non-goals: #957 `fixed(` spacing, #959 whitespace and line breaks inside comments and disabled text, #960 whitespace at the end of a directive line and behind the last token of a file without a final line break. No repository file records the subject of #958 — the lead checks it before filing anything below.

Evidence commands: `scripts/apply-fix.sh RH#### <dir>` (analyzer, fix, re-analysis; LF and CRLF arms) and `scripts/format.sh <dir>` over copies, with fixtures under the session scratchpad, outside the repository. `scripts/apply-fix.sh` normalizes every fixture to LF and to CRLF, so it cannot show RH5604 behavior; RH5604 statements below come from code reading, and the formatter half from `scripts/format.sh`.

## Premise

The specs mostly state what the code does. Spot checks that hold, beyond the spec author's harness: the shared guard (`RemoveWhitespaceRunCodeFixProviderBase.TryGetReplacement` picks the gap from the diagnostic start and the token boundary; `CommentSafeSpanReplacementCodeFixProviderBase` withholds on any comment or directive in it) matches SHARED-WS-002; RH6004's line scan with `GetNonFormattableLineIndices` plus inactive directives matches RH6004-002; RH6008 reports only when the previous token ends on the `]` line, as RH6008-001 says; RH6011/RH6012 skip structured trivia, as -002 says; RH6005 registers only operator and conversion operator declarations.

### A. Requirements that contradict the code or decide a filed question

These have to be corrected before tests are tagged. A normative sentence that covers a case the handover calls "left out" or files as a question does not leave it out: it either contradicts the code or decides the question.

1. **RH6001-002** says that keywords not listed in RH6001-001 "MUST NOT be checked". `fixed` is not listed, but it is checked. `fixed(int* p = arr)` is reported and fixed (apply-fix: `fixed`), because `SyntaxKind.FixedKeyword` is in the analyzer's keyword list. Leaving `fixed` out of 001 is fine (#957). A universal 002 then denies current behavior.
2. **RH5601-001** says "every tab … in indentation, between two tokens". Tabs in the leading or trailing trivia of a string, character, raw, or UTF-8 literal token are not reported. Each of `string s =⏎→"abc";`, `char c =⏎→'x';`, `string s =⏎→"""…""";`, and `M("a"→, "b");` gives `no-diagnostic`. `$"…"` is reported, because its start token is not in the list. Mechanism: `root.FindToken(index)` returns the token whose *full* span, trivia included, holds the tab, and `IsStringContentToken` is applied to that token. The code fix repeats the same check. The rule page says tabs must not appear outside literals. The formatter removes the trailing-trivia tab (FMT-SPACE-004) and rebuilds indentation. → new open question (Q-new-1 below); RH5601-001 must not claim these tabs.
3. **RH5601-002** says the analyzer "MUST NOT report a tab inside … a comment (`//` …)". A comment on an active directive line is reported, and the fix rewrites it: `#pragma warning disable CS0168 // a→b` and `#if DEBUG // a→b` both give `fixed`. Mechanism: `SyntaxTriviaUtilities.IsInsideCommentOrDisabledText` calls `FindTrivia(position)` without `findInsideTrivia`, so every position on a directive line resolves to the directive trivia, and the comment check never sees the comment. This is the same mechanism as the spec author's RH5601 Q1 (directive free text). The formatter keeps the tab (FMT-CLEAN-004; `format.sh` output still has it), so formatted code raises RH5601, and the rule page exempts comments. → extend Q1 with this input; RH5601-002 must not claim comments on directive lines.
4. **RH5602-001**'s first sentence covers every line. The code does not report two kinds of line: lines touched by a string, UTF-8, or interpolated literal (`string s = "a";···` and `string t = $"{1}";··` give `no-diagnostic`; the author's Q1), and whitespace at the end of a `//` comment (`int x; // note···` gives `no-diagnostic`; left out per #959). The sentence also asserts the report at the end of a directive line (`#pragma warning disable CS0168···` gives `fixed`; left out per #960). The examples avoid all three, but the sentence does not.
5. **RH5603-001** says "whatever the last line before them holds — code or a comment". "Whatever" also covers a final directive or disabled text, which is the author's RH5603 Q1, and so decides it for the current analyzer behavior.
6. **RH6005-002** says the analyzer "MUST NOT report an `operator` keyword that is followed by whitespace". That includes `operator  -`, which is the author's RH6005 Q1, so the requirement decides it. The handover says RH6005 deliberately specifies no width; this sentence specifies any width.
7. **RH5604-001** starts "When a file contains more than one kind of line ending", which covers lone `\r`. The handover says RH5604 states nothing about it. Read literally, the predominant-ending clause ("`\r\n` when it occurs at least as often as `\n`") gives `\r\n` for the pipeline question's input (one `\r\n`, zero `\n`). `LineEndingUtilities.DetectEndOfLine` gives `\n`, because it counts every length-1 terminator as `\n`.
8. **RH6003-001, RH6011-001, and RH6012-001** report the run "when that run follows code or a comment on the same line", which includes a comment on a continuation line. Examples: `int value = 1⏎    /* keep */ ;`, `List⏎    /* keep */ <int>`, `List<int⏎    /* keep */ >`. The analyzers report these, and the fix is withheld; existing tests pin this (`VerifyFixIsNotOfferedForContinuationLine…WithPrecedingComment` in the RH6003, RH6011, and RH6012 test classes). The formatter keeps that whitespace, because FMT-SPACE-001 leaves a gap that holds a line break unchanged. Verified: after `format.sh`, apply-fix still reports `no-fix-offered` for RH6003, RH6011, and RH6012 on the formatted files. RH6008 gives `no-diagnostic`, because it needs the previous token on the `]` line. So the three Why claims ("formatted code raises no RH6003/RH6011/RH6012") are false for this input. The handover files the same disagreement as RH6006 and RH6007 Q1: the formatter keeps the whitespace between a comment and the token, the analyzer reports it, and no fix is offered. For RH6003, RH6011, and RH6012, the spec writes it in as intended. Only one of the two treatments can be right. The consistent course is to leave the continuation-comment case out of these three requirements and file it with the RH6006/RH6007 questions; whether it counts as "looks wrong" is the lead's call to route.

### B. Text errors

- RH6012-001 cites `(Dictionary< ,>)` as a `>` after the comma of an unbound type. That is whitespace *after* `<`, which RH6012 does not report (`typeof(Dictionary< ,>)` gives `no-diagnostic`). The example block shows `Dictionary<, >`.
- RH5604-001's example caption says "three `\r\n` against one `\n`", but the block has two (`class C`, `int x;`) against one (`{`). The intro convention says "every other line with `\n`", but the last line of each block has no line break; otherwise the first example would be a tie and `}` would be reported.
- The Why of RH6006-001 and RH6007-001 credits FMT-SPACE-004 with removing the end-of-line run. FMT-SPACE-001 excludes gaps that hold a line break; FMT-CLEAN-001 removes that run. RH5601-001's Why credits FMT-CLEAN-002 for directive interiors, which FMT-CLEAN-003 owns.
- Report only, existing spec, no change without approval: RH6002-003's Why says "RH6007, RH6008, and RH6012 report the spaces next to the brackets". No rule reports the space after `<` in `Dictionary< ,>` (RH6011's new non-goal; RH6012 `no-diagnostic` above).

### C. Shared spec design

- `WhitespaceSpanRemovalCodeFixProviderBase` is also the base, through `BlankLineSpanRemovalCodeFixProviderBase`, of the RH5022–RH5028 fixes. Those fixes delete blank lines, so their spans contain line breaks. A type has one owner, so the future specs of those rules will have to use SHARED-WS-001. The normative sentence fits them ("delete the whitespace of the reported span … no other text"). The Why does not ("A reported run never contains a line break"), and neither does the intro ("a run of whitespace"). The lead asked for reuse by RH6013–RH6024; this is a further consumer of the same type.
- Leaving `CommentSafeSpanReplacementCodeFixProviderBase` out of the code owners is defensible. `RemoveWhitespaceRunCodeFixProviderBase` chooses the guard span, and the base serves other guard spans (RH5402, RH6016, RH6021, `CollapseTokenGapCodeFixProviderBase`).

### D. Inputs the code decides that the specs do not mention (not defects)

- RH6007: each of these is reported and fixed: indexer declaration `this[ int i]`, implicit element access in an initializer `{ [ 0] = 1 }`, function-pointer calling-convention list `delegate* unmanaged[ Cdecl]<void>`, and `stackalloc[ ]`. RH6007-001 lists the bracket owners as if the list were complete, and it omits these.
- RH5601: a tab between the tokens of an *inactive* directive is reported and fixed (`#if NEVER` / `#pragma→warning …` / `#endif`; also `#if→NESTED`, `#region→R`). The formatter replaces it too, so the two surfaces agree. RH6004-002 treats inactive directives the opposite way, and RH5601 says nothing about them.
- RH5604: a line break inside an interpolation hole (`$"{a +⏎ b}"`, and in `$@"…"`) is end-of-line trivia, so it is counted and converted. RH5604-003 ("inside a string literal — … interpolated verbatim") reads as excluding it. Established by code reading only.
- RH6001: `#if(DEBUG)` is not reported, because directive tokens are not visited. This is consistent with "statement keyword".
- RH6009: a delimited doc comment directly before `{` (`void M()/** y */{`) is reported. The formatter leaves `/** y */{` glued, after BlankLinePhase moves the mid-line `/** */` onto its own line. FMT-SPACE-014 decides only the no-space case, so formatted code raises RH6009. This is a low-priority question candidate for `sdd/specs/formatter/horizontal-spacing.md`, not for RH6009.

## Mechanism and candidates

_N/A._ No bug fix in this lane. The mechanisms behind the contradictions are stated under Premise A2 and A3.

## Breaking inputs

For the test implementer. A case that is left out or under question is never tagged with a requirement.

- **FC-019, FC-020, FC-021** (comment and directive in the gap): every SHARED-WS-002 user needs a directive in the gap (none has one today besides `RemoveWhitespaceRunCodeFixProviderBaseTests`), a same-line comment, a next-line comment, and a trailing comment after the second token (fix offered). RH6001/RH6005 need a comment after the keyword, before `(` or the symbol (`if/* c */(`, `operator/* c */-`). RH6009/RH6010 need a comment before the brace.
- **FC-003** (both sides of each boundary): same line vs. a continuation line (RH6003, RH6008, RH6011, RH6012); a run vs. no run; a comment inside vs. outside the gap; one space vs. several (RH6001, RH6009, RH6010 — "any width"); a tab vs. a space.
- **FC-022** (literals): RH5601 — a tab inside each literal kind. RH5602 — a multi-line verbatim and a raw literal. RH5604 — verbatim, interpolated verbatim, and raw literals with a differing line break. The RH6004-002 comment/literal lines.
- **FC-027** (other owners and scopes): the RH6007 bracket owners in Premise D; the RH6009/RH6010 brace owners (accessor list, initializer, empty pair `{}`, property pattern, switch expression, lambda body, anonymous object); RH6011/RH6012 on a type parameter list, a method type argument list, a nested `>`, and unbound `<, >`; RH6001 on each keyword in its list, including the `while` of `do`.
- **FC-033 / FC-017** (line endings): RH5603 under CRLF and with several line breaks; RH5604 needs exact before/after text with explicit `\r\n` in the source string, because the fixture runner normalizes it away.
- **FC-013**: the Why parity claims hold only for the inputs that remain specified after Premise A8.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH5601–RH5604, RH6001, RH6003–RH6012 | must stay (no production change in this lane) |
| Code fixes, Fix All, `RemoveWhitespaceRunCodeFixProviderBase`, `WhitespaceSpanRemovalCodeFixProviderBase` | must stay |
| `Reihitsu.Core` | must stay |
| Formatter | not involved; FMT-CLEAN, FMT-SPACE, FMT-INDENT, FMT-PIPE, and FMT-EOL are only referenced |
| Specs (16 files) | must change: Premise A, B, and C |
| Rule pages | must stay; RH5601 (comments exempt), RH5603 ("any blank lines"), RH6004 (regions not reported), and RH6005 ("exactly one space") disagree with the code, which the questions carry |
| Tests | must change: tags and focused tests per `01-spec-author.md`, minus the cases taken out above |

## Simpler option

_None._

## Questions for the maintainer

None blocks the run (escalation 1 does not apply; these were found in passing and are filed). Verdicts on the spec author's list, plus new questions:

| Question | Reproduced | Verdict |
|---|---|---|
| RH5601 Q1 — tab in directive free text | yes (`#region Na→me`, `#warning a→b` gives `fixed`; formatter keeps both and copies the region name into `#endregion // Na→me`) | survives. Extend it with Premise A3 (`#if DEBUG // a→b`): same mechanism, and it contradicts the rule page's comment exemption |
| RH5601 Q-new-1 — tab in a literal token's trivia not reported | yes (Premise A2) | new, survives; same issue as Q1 |
| RH5602 Q1 — lines touched by a literal skipped | yes (`no-diagnostic`; formatter removes both runs) | survives; not #959/#960 (those concern comments, directive lines, end of file) |
| RH5603 Q1 — final directive or disabled text reported | yes (fix removes the directive's own line break; formatter keeps it, FMT-CLEAN-007) | survives |
| RH5603 Q2 — whitespace-only last line survives the fix | yes (`}⏎···⏎` becomes `}⏎···`; formatter removes the whole tail) | survives, low severity: one pass clears RH5603's own diagnostic; the conflict is with the rule page's "any blank lines" and the RH5602/RH5603 split. Near #960 but distinct (the whitespace is on its own line) |
| pipeline.md Q1 — lone `\r` counts as `\n` | analyzer by code reading; formatter verified (`class C\r\n{\r…` becomes all `\n`) | survives. FMT-PIPE-007 read literally gives `\r\n`, so it is ambiguous, not specified; FMT-EOL-001's lone-`\r` example does not decide detection |
| RH6004 Q1 — `# region` not treated as a region | yes (both lines `fixed`; formatter treats them as regions and appends `// Spaced`) | survives; also contradicts the rule page's exception |
| RH6005 Q1 — several spaces not reported | yes (`no-diagnostic`; formatter writes one space) | survives, but only once RH6005-002 stops deciding it (Premise A6) |
| RH6005 Q2 — `operator checked+` | yes (`no-diagnostic`; formatter keeps it) | **does not survive** as an RH6005 question. Analyzer and formatter agree, and the `operator` keyword is followed by a space. RH6005's own non-goal excludes "spacing around the operator symbol", and FMT-SPACE-002 decides the pair today. This is a request for new behavior; if kept at all, it belongs to `sdd/specs/formatter/horizontal-spacing.md` or a feature issue |
| RH6006 Q1, RH6007 Q1 — whitespace between the bracket and a same-line comment | yes (`no-fix-offered`; formatter keeps it, FMT-SPACE-013 example `M( /* four */a)`) | survive |
| RH6003, RH6011, RH6012 — whitespace between a continuation-line comment and the token | yes (Premise A8) | new, same disagreement as the RH6006/RH6007 questions; file it the same way, or route the whole group to the maintainer as one decision |
| RH6009 Q1, RH6010 Q1 — braces inside an interpolation expression unchecked | yes (`no-diagnostic`; formatter writes `new int[] {1 }` / `{ 1 }`) | survive, low severity (analyzer laxer; formatter output not flagged); the rule pages exempt only the interpolation's own braces |
| horizontal-spacing.md — `/** */` glued to `{` after formatting | yes (Premise D, RH6009) | new, optional, low priority |

Follow-up candidate (formatter, FMT-INDENT-009): survives. `#if NEVER ⏎ x++; ⏎ #else ⏎ x--; ⏎ #endif`, indented: `#if` and `#endif` move to column 0, and `#else` stays indented. With `#elif ALSO_NEVER`, `#endif` stays indented too, because it also follows disabled text. The rule is that an active directive directly after disabled text keeps its indentation. RH6004 reports the formatted output (apply-fix on the input: `fixed` after 3 iterations). By the recorded subjects it is not one of #957, #959, or #960; #958 is unchecked.

Further out-of-scope candidate (tooling): `scripts/trace.sh` is committed with mode `100644` (`git ls-files -s`), so `scripts/trace.sh …` as documented in `AGENTS.md` fails with "Permission denied". It runs with `bash scripts/trace.sh`.
