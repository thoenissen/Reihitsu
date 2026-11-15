Role: skeptic
Commit: d0df346ba8e62640e9a2b33d89d8e6b0e286e2b3
Verdict: proceed

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's own *Affected* section names a cause; it was checked against the code below, not adopted.

## Premise

Real, reached, and already decided by the maintainer.

- **Caller.** The CLI (`reihitsu-format <file>`, whole-document formatting through `FormatCommandHandler`) on any `.cs` file whose last line ends with whitespace and has no final line break. Confirmed with `scripts/format.sh` over fixtures in a scratch directory (deleted afterwards): `namespace N;\n\nclass D\n{\n    int x;\n}` followed by three spaces and no line break is reported as unchanged and keeps the three spaces, LF and CRLF alike, with and without a byte order mark. A second pass changes nothing (the defect is stable, not oscillating).
- **The same text with a final line break loses the whitespace.** `}` + three spaces + `\n` becomes `}` (FMT-CLEAN-001 removes the spaces, FMT-CLEAN-005 the line break). So today the output depends on whether the input had a final line break.
- **Decision on record.** thoenissen/Reihitsu#923, comment 6232535812, Q3: "bug fix — trailing whitespace after the last token of the file is removed whether or not a final line break follows; both inputs give the same output." `sdd/specs/formatter/cleanup.md` records it in its last non-goal ("decided ... behind the last token as a bug, tracked in thoenissen/Reihitsu#960"). The visible output is therefore not in question.
- **Duplicate.** thoenissen/Reihitsu#960 (open, `agent-filed`) item 2 is the same defect; its item 1 (whitespace at the end of a directive line, #923 Q2) is a different defect in different trivia and is not part of this run. A triage comment on #960 reclassified it as an extension because the spec lists it as a non-goal; that contradicts the #923 Q3 decision, which the non-goal text itself cites.
- **Analyzer conflict today (FC-013 / SHARED-DIAG-010).** `scripts/apply-fix.sh RH5602` on the formatter's current output reports RH5602 (*Code must not contain trailing whitespace*) on all three end-of-file shapes I tried (`}···`, `} /* c */···`, `/* end */···` on its own line) and its fix removes the whitespace (6 of 6 fixed, LF and CRLF). So the CLI's output today is flagged by a layout rule whose fix already produces the target output. The bug fix removes the conflict; RH5602 itself is right.
- **Cause, checked against the code.** `CleanupPhase.CleanTrailingTrivia` removes trailing whitespace only through `CleanWhitespaceBeforeEndOfLine` (whitespace directly followed by `EndOfLineTrivia` in the same list) and `EndOfLineWhitespace.StripBeforeFollowingLineBreak` (the next token's leading trivia starts with `EndOfLineTrivia`). `CleanLeadingTrivia` of the `EndOfFileToken` uses only the first. At the end of a file without a final line break neither condition holds. The issue's *Affected* text is accurate on this point.

## Mechanism and candidates

Mechanism: cleanup removes a whitespace trivia only when a line break follows it, so a whitespace trivia that is the last trivia of the text — the end of the file without a final line break — is never removed, in whichever trivia list it lies.

Dispatch: the text's last trivia lies in exactly one of two lists, decided by Roslyn's trivia attachment:

1. the trailing trivia of the last real token (no line break in it, `EndOfFileToken` leading trivia empty) — `CleanTrailingTrivia`;
2. the leading trivia of the `EndOfFileToken` (after a line break, a comment, or a directive) — `CleanLeadingTrivia`.

Members, observed with `scripts/format.sh` (input tail → output tail):

| # | Input end of file | List | Today | Fails |
|---|---|---|---|---|
| 1 | `}···` (issue case), LF | 1 | `}···` | yes |
| 2 | `}···`, CRLF file | 1 | `}···` | yes |
| 3 | `}···` with byte order mark at the start of the file | 1 | `}···` | yes |
| 4 | `}` + tab, no line break | 1 | `}` + four spaces (tab replaced, then kept) | yes |
| 5 | `} /* c */···` | 1 | `} /* c */···` | yes |
| 6 | `}  /* a */  /* b */  ` | 1 | unchanged | yes |
| 7 | `}\n\n/* end */···` | 2 | `/* end */···` | yes |
| 8 | `}\n\n/* a */\n/* b */···` | 2 | unchanged | yes |
| 9 | `/* only */···` (file holds no other token) | 2 | unchanged | yes, rare |
| 10 | `}\n···`, `}\n\n···`, `}\n···\n···`, CRLF `}\r\n···`, `}\n` + tab | 2 | `}` | holds |
| 11 | `} /* c */\n···` | 1+2 | `} /* c */` | holds |
| 12 | `}\n\n// end\n···` | 2 | `// end` | holds |
| 13 | `}\n\n#pragma warning restore CS1591\n···` | 2 | directive keeps its line break, `···` removed | holds |
| 14 | `}···\n`, `/* end */  \n`, `/* only */···\n` (final line break present) | – | whitespace and line break removed | holds |
| 15 | `···` only, byte order mark + `···` (no token) | 2 | `` / byte order mark kept | holds |

Not members (whitespace that is not a whitespace trivia, out of scope and unchanged by this run):

- `}\n\n// end···` — the spaces are part of the `//` comment's text; with a final line break they stay too (`// end···\n` → `// end···`). That is thoenissen/Reihitsu#959 (#923 Q1).
- `#endif···` at the end of the file, and `/// <summary/>···` — whitespace inside directive or documentation-comment structure; it stays with a final line break too. Directive lines are #960 item 1 (#923 Q2); documentation lines are FMT-DOC.

Target for every failing member, from #923 Q3 ("both inputs give the same output"): the output equals the output of the same input with a final line break appended — member 1 → `}`, 5 → `} /* c */`, 7 → `/* end */`, 9 → `/* only */`. RH5602's fix produces exactly this for 1, 5, and 7.

## Breaking inputs

Failure classes that reach the changed code:

- **FC-001 / FC-002 — the qualifier "end of the text" must be an explicit condition.** The trailing trivia of every token on a line that continues also ends in whitespace with nothing behind it in the same list, and the next token's leading trivia is empty there. Inputs that must keep their single space: `int x; int y;` gaps, `} /* c */` with code following on the line, `a = b;` etc. In the node-level path, `EndOfLineWhitespace.StripBeforeFollowingLineBreak` is also applied to the token in front of a code-fix target (`ReihitsuFormatter.cs:259`); a target that shares its line with the preceding token (`int a;   int b;`, target `int b;`) must keep the gap. That helper has two consumers (FC-015): its behavior for the node-level consumer must stay.
- **FC-016 — sibling shapes.** All failing members 1–9 above, in both trivia lists, not only member 1.
- **FC-019 / FC-021 — comments in the gap.** Members 5–9: `/* */` comments on the token's line and on their own line; a `//` comment at the end must keep its text (FMT-CLEAN-004, #959 out of scope).
- **FC-020 — directives.** Member 13 must keep the directive's line break (FMT-CLEAN-007); `#endif···` without line break must stay as it is (whitespace inside directive structure, #960 item 1). Disabled text at the end of the file (`#if NEVER ... #endif`) must stay unchanged.
- **FMT-CLEAN-009 — byte order mark.** Member 3, and a file holding only a byte order mark and whitespace (member 15, today `﻿` kept): the byte order mark must survive.
- **FC-010 / FC-017 — idempotency on CRLF.** Each failing member formatted twice, LF and CRLF; a second pass must change nothing.
- **FC-033 — harness normalization.** The reproduction must be built from explicit strings (`"...}   "`), not a raw string literal or a file an editor trims, and must not append a line break; the assertion must compare the full text including its end.
- **FC-028 — detached vs document.** See *Internal choices* 2: no production caller observes a node-level root's trailing whitespace, so the node-level entry points must not be part of the reproduction.

Candidate inputs for the test implementer: members 1, 2, 4, 5, 7, 9 (failing), and 10, 13, 14, plus a same-line gap such as `class D { int x; int y; }`-shaped code formatted where a token's trailing trivia ends in whitespace before code on the same line (holding).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `CleanupPhase` (`src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs`) | Must change |
| `EndOfLineWhitespace.StripBeforeFollowingLineBreak` | Must stay for its node-level consumer (`ReihitsuFormatter.cs:259`); a change to it would also change the gap in front of a code-fix target |
| Other formatter phases | Not involved |
| Node-level entry points (`FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`) | Not involved: a non-root target keeps its trailing trivia (FMT-PIPE-010) through `WriteBackTrailingTrivia`; a compilation-unit target runs the same pipeline as the document and gets the fix through `CleanupPhase` |
| CLI (`Reihitsu.Cli`) | Not involved in code; its output changes as a result (files of the failing members are now reported/rewritten by `--check`/format) |
| RH5602 analyzer and code fix | Must stay; they already report and remove the whitespace |
| RH5603 and other analyzers, code fixes, Fix All | Not involved |
| `Reihitsu.Core` | Not involved |
| Rule docs | Not involved |
| `sdd/specs/formatter/cleanup.md` | Must change: the last non-goal still says the phase leaves "whitespace behind the last token of a file that has no final line break" in place; it must stop saying so for this case (the comment and directive-line parts stay), and a requirement must state the removal (see *Internal choices* 1) |
| Tests | Must change: regression tests in `src/Reihitsu.Formatter.Test/Regression/Cleanup/`; no existing test asserts the current end-of-file whitespace (searched for test strings ending in spaces) |

## Refuse option

- **Leave the construct as written (today's behavior).** Costs nothing in production code, but contradicts the maintainer's #923 Q3 decision, keeps the formatter's output depending on whether the input had a final line break, and keeps the CLI's output flagged by RH5602 (FC-013). The class is uncommon (whitespace behind the last code of a file without a final line break) but not exotic: editors that strip the final newline but not trailing spaces produce it. The requested change is a cheap trivia check in one phase, with no new type or mechanism expected. Refusing is not recommended.
- **Declare the shape unsupported.** Not an option: the shape costs no special case anywhere (no guard in any phase or code fix exists for it that a declaration would delete), and refusing whole files for trailing whitespace would refuse common, harmless code. A shape that deletes nothing is not declared.
- **Narrower change.** Fixing only member 1 (whitespace directly behind the last token) and leaving members 5–9 is narrower but leaves FC-016 siblings that #923 Q3 ("both inputs give the same output") and RH5602 both cover; not recommended.

## Output questions

_None._ #923 Q3 decides every failing member: each must give the output of the same input with a final line break, which is observed today (member 14) and matches RH5602's fix.

## Internal choices

1. **Lane and spec shape.** `sdd/README.md` says extending behavior into a non-goal is a behavior change needing a change doc, but also that a non-goal never lists a known defect — and this non-goal lists a decided bug. Options: (A) bug-fix lane; the spec author, as a clarification, removes the end-of-file part from the non-goal and states the removal in a requirement — either FMT-CLEAN-001 extended to whitespace that ends the text, or FMT-CLEAN-005 widened from "line breaks and whitespace-only lines" to "whitespace" after the last token; (B) behavior-change lane with a change doc. No visible output differs. Recommend A: #923 Q3 is a bug-fix decision, and #923 Q1 set the precedent that the implementing PR adds the requirement and removes the non-goal without a change doc. Whichever requirement the spec author picks is the tag of the new tests.
2. **Scope with respect to detached roots (FMT-CLEAN-008).** Whitespace at the end of a node-level root's last token (no next token) could be treated like the end of a file or left alone. No caller observes the difference: `FormatNode`'s only production caller (RH5401's `CanCollapse`) reads `ToString()`, which drops trailing trivia, and the document entry points overwrite the target's last trailing trivia (FMT-PIPE-010). Recommend keeping the change at the end of a file (compilation unit), so FMT-CLEAN-008 stays as written.

## Issue goals

- "with a formatter test carrying the FMT-CLEAN-005 tag" — change: the tag follows the requirement the spec author writes (*Internal choices* 1); FMT-CLEAN-005 today covers only line breaks and whitespace-only lines, and the non-goal excludes this case. No visible output change.
- "Whole-document formatting removes the whitespace behind the last token" — widen to the class: whitespace that ends the file, also behind a trailing `/* */` comment on the token's line or on its own line (members 5–9). Covered by #923 Q3; it changes visible output only as that decision already demands.
- "formatting the result changes nothing" — keep, and test it on CRLF too (FC-010).
- *Affected*: "The node-level entry points match this behavior today for a target at the end of such a file" — accurate but irrelevant: non-root targets keep their trailing trivia by FMT-PIPE-010; nothing to do there.
- The PR should `Closes #1747` and reference thoenissen/Reihitsu#960 with `Refs`, because #960 item 1 (directive lines) stays open.

## Architecture findings

_None._ `EndOfLineWhitespace` already owns the "whitespace before a line break carried by the next token" policy for both consumers; the defect is a missing case in one phase, not a shared decision.

## Follow-up candidates

- **Update thoenissen/Reihitsu#960** after this run: drop item 2 (done here), keep item 1 (directive-line whitespace, #923 Q2); its triage comment calling it an extension contradicts #923 Q2/Q3 (bug fix). Text change of an issue only.
- **FMT-CLEAN-007 vs a directive-only file with a trailing whitespace line.** Out of the run's issue. FMT-CLEAN-007 says "A file that contains no token besides directives MUST be left unchanged", but the CLI changes such a file:

  Input (`···` are three spaces, no final line break):
  ```csharp
  #pragma warning disable CS1591
  ···
  ```
  Before (`main`): the whitespace-only last line is removed; the file ends with the directive's line break.
  ```csharp
  #pragma warning disable CS1591

  ```
  After, A (recommended): spec says so — the whitespace-only line after the last directive is removed, as for a file with code (FMT-CLEAN-007's own example). No code change.
  After, B: code changes to keep the file byte-identical, `···` included.

  Why A: the whitespace carries nothing, and A matches the files that hold code. Rare; a spec-wording follow-up, not a defect of this run.
