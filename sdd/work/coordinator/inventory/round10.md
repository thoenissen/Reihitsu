# Inventory — round 10 (questions filed by the round-9 runs #1331, #1380, #1403)

Read 2026-10-23 via GitHub MCP (list/read only, no writes). `⏎` = line break, `␍` = lone CR.

## 1. Run status

| PR | Issue | Merged | Issue closed | Filed by the run | Proposed follow-ups / items for the maintainer (PR body; PRs have no comments and no reviews) |
|---|---|---|---|---|---|
| #1404 | #1331 (P25–P31 + P18/P22 clar. + 2 wording points) | yes 2026-10-23 12:39 | yes (completed) | #1407 (Q1 body, Q2–Q3 comment), #1408 Q1, bug #1412 | Decisions taken in the run (`squad/decisions/2026-10-22-1331-p2*`): P26 reach = brace fixes yes, expression-body conversion and using organizers no; P30 reach = RH8301–RH8303 stay gated by doc mode; P28 limited to RH7104–RH7106 (partial members stay P15, #1287). SHARED-FIX-003 narrowed (missing fix never removes the report; not-reported inputs -> DIAG-008/-010/JOIN-001) and P11 closing sentence -> P31: both #1331 wording points done. P18/P22 needed no text. Spec examples show decided-but-unimplemented behavior (tracked #1334, #1335, #1339, #1345, #1346, #1347, #1287, #1407, #1408). Weak coverage: SHARED-DIAG-009 bullet-1 test cannot fail until #1334; DIAG-004 partial-member paragraph untestable until #1287; RH2004-001/-005 contradict the partial-property example (#1287). Commented on #1335, #969. Two extra repair rounds approved by maintainer. |
| #1405 | #1380 (P32–P37 + P12 clar.) | yes 2026-10-23 10:52 | yes (completed) | #1408 comment (Q-1380-1, Q-1380-2) | Maintainer decisions in run: P37 recorded formatter-side only (FMT-BLANK-016), shared side deferred to #1385; SHARED-COMMENT-011 keeps rule-own exemptions (SHARED-CHAIN-004/RH5112); P34 keeps per-construct layouts (FMT-BREAK-014/-021/-022/-026, switch-arm non-goal). Partly addresses #1081 item 2 (lone `\r` in FMT-PIPE-007). **Two spec-author readings not decided by the maintainer:** FMT-STRUCT-024 reads "move a comment" as moving it next to other code (because FMT-STRUCT-002 itself moves comments into the new block); SHARED-COMMENT-012 names the conditions under which a rule reports the token behind a comment tail. P12 example (RH5110) does not reproduce -> spec uses RH5204; RH5110 case is a #1147 instance. Contradictions left on purpose: FMT-STRUCT-022 vs -024 (#1384, #1395), SHARED-INIT-001 vs SHARED-COMMENT-012 (#1382), FMT-BLANK-011 vs -016 (#1385). Commented on #963, #1147, #1381, #1382, #1384, #1385. **Unfiled follow-up candidate:** a switch-expression arm wrapped before `=>` keeps its break but `=> 2,` is moved left of the arm ("may belong to #953"). |
| #1406 | #1403 (multi-issue squad runs) | yes 2026-10-23 12:10 | yes | none | Differs from issue: an escalation stops the whole run; every issue of a run must share ONE lane (`2026-10-23-1403-same-lane-batches.md`), not "narrowest lane admitting all". Soft limit 8 issues (escalation 11). `sdd/changes/2026-10/establish-squad.md` left unchanged on purpose. Follow-ups: none. Consequence: #1401+#1377 (docs) = one run; #1402 (tooling) separate. |

All three merged, all three issues closed. No open PR.

## 2. Issues and PRs with number > #1403

| # | Kind | Title | State | Milestone | Labels | Gist | Filed by |
|---|---|---|---|---|---|---|---|
| #1404 | PR | Record principles P25–P31 in the shared specs | merged | – | – | see §1 | run #1331 |
| #1405 | PR | Record principles P32–P37 and the P12 clarification … | merged | – | – | see §1 | run #1380 |
| #1406 | PR | Let one squad run implement several issues of the same lane | merged | – | – | see §1 | run #1403 |
| #1407 | issue | SDD open questions: sdd/specs/shared/element-documentation.md | open | none | none | 3 Q: RH8305/RH8307 fixes move behind-code `///` (P29); move fix leaves behind-code `///` in doc mode `None`; directive between behind-code `///` and moved declaration (RH7309) | run #1331 (Q1 body 2026-10-22, Q2–Q3 comment 2026-10-23) |
| #1408 | issue | SDD open questions: sdd/specs/shared/code-fixes.md | open | none | none | 3 Q: brace fixes reformat wrapped statement (P26); RH5011/RH8304 write lone `\r` in all-`\r` doc (P35); kept breaks of a formatter-laid-out fix get mixed endings | run #1331 (Q1), run #1380 (Q-1380-1/-2 comment) |
| #1409 | PR | Withhold RH3103 fix for dotted Nullable inside nameof | merged 2026-10-23 | – | – | external contributor (biggdawg320); fixes #1258, adds RH3103-006 | **not ours** |
| #1410 | PR | Skip only the Sonar steps for fork pull requests | merged 2026-10-23 | – | – | CI workflow, Refs #1409 | **not ours** (maintainer session) |
| #1411 | issue | [ANALYZER BUG] RH3103 reports Nullable where the shorthand T? cannot stand | open | Triage | analyzer | Asks to stop reporting `Nullable<T>` where `T?` is not valid C# (dotted `nameof`, member-access receiver, `typeof(Nullable<>)`); reverses the report part of P23 (#1231, #1281); explicitly needs a maintainer decision | **not ours** (maintainer, from review of PR #1409) — but SDD-relevant, see §5 item 4 |
| #1412 | issue | [ANALYZER BUG] RH7105 and RH7106 do not report the declaring part of a partial constructor | open | none | bug, analyzer | Pre-existing gap found by the #1331 auditor; bug lane; related #1338, #1287, SHARED-DIAG-004 (P28) | run #1331 (PR #1404 "Follow-up work") |

**SDD open-questions issues:** 2 (#1407, #1408), **6 questions**. **Other:** #1411 (decision needed), #1412 (plain bug, no question).

## 3. Question inventory

Principles used: P1 unsafe fix (SHARED-FIX-003); P8 doc = what the compiler produces; P9 fix scope; P10 fix layout; P13 comment kinds alike / `///` documenting nothing = ordinary comment; P14a no blank line before changed node; P23 report, fix withheld where a fix would break code; P24 fix keeps document EOL; P26 moved code keeps its layout (SHARED-FIX-007 moved-code bullet); P29 doc comment behind code (SHARED-DOC-004); P30 doc-mode gate only for doc rules; P35 fix EOL = FMT-PIPE-007 detection (SHARED-FIX-008). Precedent round 7b item 4 (#1185 Q1): fix withheld when a comment stands directly above a region block.

| Issue | Q | Rule/spec | Input → real output | Why wrong | Req IDs | Principle | Duplicates / related open | Kind + recommendation | Fully decided? |
|---|---|---|---|---|---|---|---|---|---|
| #1407 | Q1 | RH8305, RH8307 fixes | `int A; /// <summary>Value</summary>⏎int B;` (RH8305) and `int A; /// <summary>Total⏎/// sum</summary>⏎int B;` (RH8307) → fix moves comment to own line after an inserted blank line (`int A;⏎⏎/// <summary>…`) | SHARED-DOC-004: only the RH8303 fix moves a behind-code `///`; other fixes leave it (P9); also inserts a blank line (P14a) | SHARED-DOC-004, SHARED-FIX-006, RH8305, RH8307 | P29 (claim **verified**: SHARED-DOC-004 on main says "A code fix that moves neither the declaration … nor the code … MUST leave the comment behind that code …, except the RH8303 fix") + P14a | #1342 (RH8305/RH8307 fixes format only the reported comment, P9/P14d — same fixes), #1345 (P29 for RH8301–RH8303), #1241 (RH8305/RH8307 end tags) | Bug. Extend #1342 (same fixes; retitle "+ leave a behind-code `///` in place (P29)"), cross-link #1345 | Yes |
| #1407 | Q2 | RH7103 (also RH7102, RH7109) move fixes; doc mode `None` | `public void Run()⏎{⏎} /// <summary>Creates</summary>⏎public static void Create()…` (mode `None`) → `Create` moved in front of `Run`, `} /// <summary>Creates</summary>` stays behind `Run`; `// note` between comment and `Create` travels; mode `Parse` carries the `///` | Moved declaration loses its documentation | SHARED-DOC-004 (mode `None` explicitly not covered), SHARED-ORDER-002 | none decides. P8/P13 favor (b): in mode `None` the compiler parses `///` as ordinary trailing trivia of `}` (stated in SHARED-DOC-004 Why). P30 (doc-mode gate only for doc rules) could be read for (a) | #1335 (organization fixes P26), #969 (RH7103 whole-type formatting) | Clarification. Recommend (b): in mode `None` a behind-code `///` is an ordinary trailing comment of the code in front and stays with it, like `// c` behind `}`; write it into SHARED-DOC-004 | No |
| #1407 | Q3 | RH7309 fix (modes `Parse` and `None`) | `#region IDisposable⏎public void Dispose()⏎{⏎} /// <summary>M</summary>⏎⏎#endregion // IDisposable⏎⏎#region Methods⏎public void M()…` → `Methods` region moved in front; `} /// <summary>M</summary>` stays in front of `#endregion // IDisposable`. Counter: RH7103 with `#if DEBUG`/`#endif` between comment and `Create` (mode `Parse`) carries comment and directives | `M` loses its documentation; in Parse mode the stray `///` can then document whatever follows (silently wrong doc) | SHARED-DOC-004 (directive case not covered), SHARED-ORDER-002, SHARED-FIX-003 | P1 (fix that changes what a comment documents relocates its meaning) + precedent 7b item 4 (#1185 Q1, withhold when a comment stands directly above a region block) | #1262 (region fixes withheld when they break code), #1264 (RH7309 region ownership) | Recommend (c) withhold the RH7309 fix when a behind-code `///` that documents a moved declaration is separated from it by a directive that starts the moved block (modes `Parse`/`Diagnose`); in mode `None` as Q2. Extend #1262 | No |
| #1408 | Q1 | RH5405, RH5406, RH5407 brace fixes | `if (a)⏎    M(a,b);` → braces added and body re-laid out to `M(a, b);` | SHARED-FIX-007 moved-code bullet names "the embedded statement of an `if`, `else`, or loop that a brace fix encloses": keeps layout, only re-indented | SHARED-FIX-007, SHARED-BRACE-002 (states the opposite: "laying out the `if` statement … as the formatter does"), RH5405–RH5407 | P26 (claim **verified**: SHARED-FIX-007 text on main and #1404 run decision "P26 reach: brace fixes yes") | #1335 (P26 for RH7004/RH7101/RH7104–RH7108 only), #1261 (RH5405–RH5407 comment/directive text), #1388 (RH5405 EOL), #1144 (P10) | Behavior change (SHARED-BRACE-002 amended + code). New "Brace fixes RH5405–RH5407 keep the wrapped statement's layout; SHARED-BRACE-002 aligned with SHARED-FIX-007 (P26)" — or extend #1261 (same fixes). Recommend new | Yes |
| #1408 | Q-1380-1 | RH5011, RH8304 fixes | all-`␍` doc: `a++;␍break;␍a++;␍` → `break;␍␍a++;`; `/// <summary>Runs</summary><remarks>Remarks</remarks>␍` → created break `␍` | SHARED-FIX-008: created break must be `\n` in an all-`␍` doc; RH5011-004/RH8304-004 state today's `␍` | SHARED-FIX-008, FMT-PIPE-007, RH5011-004, RH8304-004 | P35 (claim **verified**: SHARED-FIX-008 "in a document whose line breaks are all a lone `\r`, every line break the fix creates MUST be `\n`") | #1388 (RH5405 fix + `FormatNode` EOL, P24), #1377 (cites SHARED-FIX-008; lists RH5011-005 but not RH5011-004/RH8304-004) | Bug + amend RH5011-004/RH8304-004. Extend #1388 (code); note on #1377 to add RH5011-004, RH8304-004 to its list | Yes |
| #1408 | Q-1380-2 | formatter-delegating fixes (RH5406, RH5405) | all-`␍` doc: `if (a) M(a,␍                 b);` → `if (a)⏎{⏎M(a,⏎b);⏎}␍` (kept break inside `M(…)` becomes `\n`, kept break behind `}` stays `␍`) | Kept breaks treated differently; no stated rule | SHARED-FIX-008 (CRLF/LF: "every line break the fix creates **or keeps**"; lone-CR clause: only "creates"), SHARED-FIX-007, FMT-PIPE-007 | P35 covers created breaks only; P26 removes this exact shape for brace fixes (wrapped statement not laid out) but not for other formatter-laid-out fixes | #1388, #1377 | Clarification + small bug. Recommend (b) symmetric with the CRLF/LF clause | No |

**Counts:** 6 questions in 2 issues. Fully decided: 3 (#1407 Q1 by P29, #1408 Q1 by P26, Q-1380-1 by P35; all three "Decided by principle" claims verified against main). Needs maintainer: 3 (#1407 Q2, Q3; #1408 Q-1380-2). Plus non-question decisions: #1411, two #1405 spec-author readings, one unfiled follow-up candidate.

## 4. Decided → proposed follow-up action

1. **Extend #1342** (RH8305/RH8307 fixes): the fixes leave a behind-code `///` where it stands and insert no blank line (P29, P14a) — #1407 Q1. Retitle e.g. "Documentation layout fixes format only the reported comment and leave a behind-code `///` in place: RH8305, RH8307, RH8309, RH8402 (P9, P14d, P29)". Cross-link #1345.
2. **New: "Brace fixes RH5405–RH5407 keep the layout of the statement they wrap (P26)"** — #1408 Q1. Behavior-change lane, change doc; amends SHARED-BRACE-002 to match SHARED-FIX-007; tests pinning `M(a, b);` change. Cross-link #1261, #1388, #1335. (Alternative: extend #1335 and retitle to "Organization and brace fixes …".)
3. **Extend #1388** (RH5405 fix + `FormatNode` EOL): RH5011 and RH8304 fixes write `\n` in an all-`\r` document; amend RH5011-004 and RH8304-004 to cite SHARED-FIX-008 (P35) — #1408 Q-1380-1. Comment on #1377 that RH5011-004 and RH8304-004 join its list (or are handled by #1388; avoid double edit).
4. Close #1407 / #1408 once §5 is answered and the follow-ups exist; list both on #912 (see §6).
5. **Unfiled run follow-up (#1405):** switch-expression arm wrapped before `=>`: `=> 2,` moved left of the arm — comment on #953 (continuation lines without alignment owner) after a quick check, or ask (see §5 item 6).

## 5. Needs maintainer decision

**1. #1407 Q2 — behind-code `///` in documentation mode `None`, member move.**
Input (`DocumentationMode.None`, RH7103):
`public class C⏎{⏎    public void Run()⏎    {⏎    } /// <summary>Creates</summary>⏎    public static void Create()⏎    {⏎    }⏎}`
Today: `Create` moves in front of `Run`; `} /// <summary>Creates</summary>` stays behind `Run` (mode `Parse`: carried with `Create`).
(a) carry it with `Create`, as in mode `Parse` (fix behaves independently of doc mode, P30 reading);
(b) leave it: in mode `None` the compiler parses it as an ordinary trailing comment of `}`, and a trailing comment stays with its line — clarification, today's behavior;
(c) withhold the fix.
**Recommend (b)** (P8: as the compiler parses; no code change; doc mode `None` is rare and gates every doc rule anyway).

**2. #1407 Q3 — directive between behind-code `///` and the moved declaration (RH7309).**
Input (modes `Parse`/`None`): `#region IDisposable⏎public void Dispose()⏎{⏎} /// <summary>M</summary>⏎⏎#endregion // IDisposable⏎⏎#region Methods⏎public void M()⏎{⏎}⏎#endregion // Methods`
Today: `Methods` region moved in front; the `///` stays behind `Dispose()` in front of `#endregion` → `M` undocumented, the stray comment documents whatever follows.
(a) carry the comment across `#endregion` with `M` (into another region);
(b) leave it (today; directive starts the moved block);
(c) withhold the RH7309 fix in modes `Parse`/`Diagnose` when such a comment would be separated from its declaration (P1; like 7b item 4, #1185 Q1); mode `None` follows item 1.
**Recommend (c).** Follow-up: extend #1262.

**3. #1408 Q-1380-2 — kept line breaks of a formatter-laid-out fix in an all-`\r` document.**
Input (all `␍`): `class C␍{␍    void M(bool a, bool b)␍    {␍        if (a) M(a,␍                 b);␍    }␍}␍`, RH5406.
Today: `if (a)⏎{⏎M(a,⏎b);⏎}␍` — break inside `M(…)` becomes `\n`, break behind `}` stays `␍`.
(a) a fix changes only the breaks it creates; every kept break stays as written (P9 literal; result mixes `␍` and `\n` by rule);
(b) as for CRLF/LF documents: every break the fix creates **or keeps** in the code it changes is the detected ending (`\n`) — SHARED-FIX-008 lone-CR clause gets "or keeps";
(c) leave unspecified (non-goal; lone-CR documents are rare).
**Recommend (b)** (symmetric with the existing CRLF/LF clause; small fix in the formatter-delegating path). Follow-up: extend #1388.

**4. #1411 — RH3103 reports `Nullable<T>` where `T?` cannot be written.**
Input: `string A() => nameof(System.Nullable<int>);`, `bool D() => Nullable<int>.Equals(1, 2);`, `Type E() => typeof(Nullable<>);`
Today: reported, fix withheld (P23 via #1231 Q1/Q2, #1281; PR #1409 added RH3103-006).
(a) keep P23: report, no fix; rule page names the positions (documentation-only);
(b) do not report where the language forbids `T?` (dotted `nameof` argument, member-access receiver, `typeof` of an unbound generic) — extends the existing RH3103-002 exemption for unqualified `nameof(Nullable<int>)`; using alias reported only from C# 12 (`using N = int?;` valid there);
(c) (b) without the alias part.
**Recommend (b)** (candidate P38: a report needs a correction the language allows at that position; P23 stays for positions where the form is legal but the automatic fix is unsafe). #1258 is closed by #1409; #1281's RH3103 part shrinks.

**5. #1405 spec-author readings (confirm).**
(i) FMT-STRUCT-024 (P36) "move a comment" = move it next to other code; FMT-STRUCT-002 moving a comment into the new block is not a move. Input: `if (a) // c⏎    Foo();` → braced with `// c` inside the block stays allowed. Confirm yes/no. **Recommend yes.**
(ii) SHARED-COMMENT-012 (P33) conditions under which a rule reports the token behind a comment tail (author's list). **Recommend: confirm as written**; any change goes through #1382.

**6. #1405 unfiled candidate.** Switch-expression arm wrapped before `=>`: `x switch⏎{⏎    1⏎        => 2,⏎}` → `=> 2,` placed left of the arm column. (a) comment on #953 (continuation without alignment owner); (b) own formatter bug issue; (c) drop. **Recommend (a)** after the bookkeeping agent reproduces it.

## 6. Principle candidates (continue at P38)

- **P38 — A rule reports only what the language lets the user write instead.** Where the form the rule asks for is not valid C# at that position, the construct is not reported; P23 (report, fix withheld) stays for positions where the form is valid but the automatic rewrite is unsafe. Decides #1411; consistent with RH3103-002.
- **P39 — A fix that cannot move a declaration together with its documentation is withheld.** Where a move would separate a declaration from the `///` the compiler attaches to it (a directive between them that starts the moved block), the fix is withheld (P1 instance; 7b item 4 precedent). Decides #1407 Q3.
- **P35 clarification (not new):** in every document the fix writes the detected line ending for each break it creates or keeps in the code it changes (lone-CR documents included). Decides #1408 Q-1380-2 (b).
- **P29 clarification (not new):** in documentation mode `None` a behind-code `///` is ordinary trailing trivia of the code in front (P8). Decides #1407 Q2 (b).

## 7. #912 tracking

Last #912 comment: round-9 bookkeeping 2026-10-22 20:00 (lists #1362–#1379 and follow-ups). **#1407 and #1408 (created 20:51) are not tracked on #912** — no sub-issue (limit 100/100 reached; both bodies say so) and no listing comment. #1411 and #1412 are not question issues; #1411 is in milestone Triage, #1412 has no milestone. #1407/#1408 carry no milestone (earlier follow-ups: `SDD`).

## 8. Other findings for the coordinator

- #1331 wording points (SHARED-FIX-003, P11 closing sentence) are done by #1404 — nothing left from round 9 item 29 except #1401.
- #1081 item 2 partly addressed by #1405 (FMT-PIPE-007 lone `\r` count); check whether #1081 can be narrowed/closed.
- Weak-coverage notes from #1404 belong to #1334 and #1287 (tests that cannot fail yet; RH2004-001/-005 contradiction) — mention on #1287 if not already there.
- Next runs per #1406 rule: #1401 + #1377 (docs lane, one run), #1402 (tooling, separate), then #998.
