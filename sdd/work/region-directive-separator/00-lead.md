Role: lead
Commit: 50a9d79
Verdict: lane claimed

# Run: region-directive-separator

## Issues

- thoenissen/Reihitsu#1486 — "FMT-REGION: one space separates #region and #endregion from their text". Labels: `enhancement`, `formatter`, `agent-filed` (goals are proposals). Package F1 of thoenissen/Reihitsu#1482; its blocker thoenissen/Reihitsu#1485 is closed by thoenissen/Reihitsu#1508.
- Draft PR: thoenissen/Reihitsu#1578. Branch: `claude/confident-brown-k65bv5` (the session's designated branch; see Decisions taken #1).

## Lane

Behavior change. Reason: the issue requests changed formatter output (a new FMT-REGION requirement) and requires a change doc under `sdd/changes/`.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped. The issue manager runs before publishing if follow-up candidates exist.

## Maintainer clarifications

None so far.

## Maintainer answers

Batch 1 (before the test implementer):

1. Several spaces before a capitalized name (`#region    Bar`) → A: written `#region Bar`.
2. Tab separator (`#region<tab>Bar`) → A: written `#region Bar` (the preparation phase first turns the tab into four spaces, the region phase then writes one space). A tab inside the description stays. The maintainer asked whether such tabs are not removed by the tab phase; answered: the tab phase replaces them with four spaces (FMT-TAB-001), it does not decide separators.
3. Inside `#if false` → A: `#region Bar` … `#endregion // Bar`.
4. `#endregion` of a region without a description → B: one space between `#endregion` and its text (`#endregion    // Stale` → `#endregion // Stale`; `#endregion//Stale` → `#endregion //Stale`).
5. Trailing whitespace on a `#region` line with a description → C: every such line is written as `#region`, one space, the trimmed description.
6. Code fixes that format a node holding a region pair → B: the general rule applies that a code fix changes only what its diagnostic is about (SHARED-FIX-006). A fix such as RH7105 has nothing to do with regions and must not adjust them; a user who does not use the region rules would otherwise see their region layout changed by an unrelated fix.

Batch 1, second message:

7. Whitespace behind the text of an `#endregion` whose `#region` has no description → B: removed (`#endregion    // Stale···` → `#endregion // Stale`). The maintainer added as a general statement that trailing whitespace is always removed.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/issue-1486-<slug>` (lead.md) vs. the session's designated branch | Use `claude/confident-brown-k65bv5` | The session environment requires all pushes to that branch | Branch names are internal | 50a9d79 |
| 2 | Skeptic | Does FMT-REGION-007 apply to every `#region` line, or only to directives the phase pairs (FMT-REGION-003)? | Only to paired directives; the spec author states the pairing scope | Matches the code's ownership; keeps `main`'s output for a `#region` whose `#endregion` lies outside a formatted node | `main`'s output is kept for the unpaired case, so nothing changes there (skeptic output question 4 dissolves) | |
| 3 | Skeptic | Where does the disabled-branch qualifier live (FMT-REGION-006 vs 007)? | In FMT-REGION-007; FMT-REGION-006 stays unchanged | One decision per requirement, each with its own `Why:` | Spec structure only | |
| 4 | Skeptic | FMT-TAB-002's example still capitalizes `foo` | Keep naming FMT-REGION-001 in FMT-TAB-002's prose and `Uses` beside FMT-REGION-007 | The example relies on both rules | Spec wording only | |
| 5 | Skeptic | Architecture finding (spec level): the one-space line form is stated in FMT-REGION-001 and FMT-REGION-007 | Change the structure in this run: the line form is stated once, in FMT-REGION-007, and FMT-REGION-001 points to it | A narrow fix would leave a second statement of the rule; the change stays within the region spec | Spec wording only; the code already has one writer (`BuildRegionTrivia`) | |
| 6 | Skeptic | Change doc says code fixes are unchanged | The spec author corrects the change doc: code fixes keep region directive text (maintainer answer 6) | The skeptic reproduced the change with RH7105/RH7104/RH8305 | Decided by the maintainer (answer 6) | |
| 7 | Maintainer answer 6 | Fixes unrelated to regions already rewrite region directives on `main` (capitalize names, write `#endregion // Name`), a pre-existing departure from SHARED-FIX-006 in the mechanism this run changes | Take it into the run: a code fix leaves the text of region directives in its formatted node unchanged (separator, capitalization, `#endregion` comment), so FMT-REGION applies to whole-document formatting only; the spec author states the scope, the skeptic's architecture concern (FC-028, a node-level exception for one sub-rule) is avoided by excluding the whole region rewrite, not only the new separator | An unchanged requirement (SHARED-FIX-006) demands it and the maintainer confirmed the rule; exempting only the new separator would add a node-level exception for one sub-rule and keep the existing departure | Demanded by unchanged SHARED-FIX-006 and covered by maintainer answer 6 | |
| 8 | Test implementer | A region directive without a description followed only by trailing whitespace (`#region···`, bare `#endregion···`): does FMT-REGION-004's "left alone" keep the whitespace? | Remove it: the line becomes `#region` / `#endregion`. FMT-REGION-004 is amended accordingly | Maintainer answer 7 states that trailing whitespace is always removed; for region lines that answer is in this run's scope | Covered by maintainer answer 7 | |
| 9 | Test implementer | FMT-REGION-006 names only 001 and 002 for disabled branches; does FMT-REGION-004's normalization apply inside `#if false`? | Yes; the spec author states it explicitly (in FMT-REGION-004, as for 007) | Same rule as FMT-REGION-007 inside `#if false` (maintainer answer 3) | Covered by maintainer answer 3 | |
| 10 | Test implementer | `03-spec-author.md` claims RH7103 withholds its fix for a type with a region | Claim is wrong; the change doc's list of affected fixes includes RH7103 | An existing RH7103 test applies the fix inside regions | Change-doc text only; the RH7103 output change is covered by maintainer answer 6 | |
| 11 | Implementer | Four existing tests (`RegionFormattingTests.TrailingOpeningWhitespaceWithCanonicalCloseIsStable`, `.MismatchedCloseUsesNormalizedOpeningDescription`, `RH7301RegionsShouldMatchFormatterTests.VerifyFormatterPreservesTrailingOpeningWhitespaceWhenCloseIsCanonical`, `.VerifyFormatterUsesNormalizedOpeningDescriptionForMismatchedClose`) expect `#region Fields···` to keep its trailing whitespace | The tests are wrong; the test implementer changes their expected output to `#region Fields` and their names/summaries | FMT-REGION-007 as amended, change-doc visible change 4 | Covered by maintainer answer 5 (option C) | |
| 12 | Implementer | Follow-up candidate 1 (a fix writes half a region pair) | Dropped as follow-up: absorbed by FMT-PIPE-028; covered by `FormatNodeInDocumentAsyncKeepsBothHalvesOfRegionOpenedInFrontOfTopLevelType` and the RH7104 top-level test | Fixed in this run | Covered by maintainer answer 6 | ac171f5 |

## Follow-up candidates

1. (Absorbed in this run — decision 12; not filed.) A code fix whose target's parent is the root restores the target's leading trivia after the region phase paired its `#region` with an `#endregion` inside the target, so only half the pair is rewritten (e.g. RH7104 on a top-level `partial class C` preceded by `#region    bar`: writes `#endregion // Bar`, keeps `#region    bar`). Pre-existing on `main`. — 02-skeptic.md
2. Trailing whitespace behind the text of directives other than `#region`/`#endregion` is kept (`#warning x···`, `#pragma ...···`). The maintainer stated in answer 7 that trailing whitespace is always removed, so the follow-up carries that direction. — 01-spec-author.md, 02-skeptic.md, maintainer answer 7
3. Whitespace between `#` and the directive keyword (`#  region Bar`) is kept. Low priority. — 01-spec-author.md, 02-skeptic.md
