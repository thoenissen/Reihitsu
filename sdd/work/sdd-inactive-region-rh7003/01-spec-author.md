Role: spec author
Commit: cef925e
Verdict: #1240: text written; #1591: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1240

No change doc: the edit narrows a non-goal, and no requirement changes (`sdd/README.md`, *When a change doc is required*).

Premise checked: the old non-goal in `sdd/specs/shared/diagnostics.md` said that no rule can report anything in an inactive `#if` branch. Existing tests show that region rules do report region directives there: `RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests` (`{|RH7302:#region test|}` inside `#if false`), `RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzerTests`, and `RH7301RegionsShouldMatchAnalyzerTests` (`{|RH7301:#endregion // Members|}` inside `#if false`). `sdd/specs/analyzer/RH7301.md` and `RH7303.md` specify it, and `documentation/rules/general-notes.md` already states the exception. The issue's list of rules is not fully accurate: `sdd/specs/analyzer/RH7309.md` (RH7309-005 example) says a region of an excluded branch is not part of the outline it judges. For that reason the new text names no rule and leaves the behavior to each rule's spec.

The new wording follows principle P22 from the issue comment and agrees with SHARED-DIAG-008 ("code that such a directive excludes counts as absent; the directive itself counts as a directive line").

### Changed requirements

None. Non-goal changed, `sdd/specs/shared/diagnostics.md`:

- old: "Code in an inactive `#if` branch. The compiler does not hand it to any analyzer, so no rule can report it."
- new: "Code that an inactive `#if` branch excludes. The compiler does not hand that code to any analyzer, so no rule can report it. The directives inside such a branch are not excluded code: they stay in the syntax tree, so a rule may report a `#region` or `#endregion` there, and whether it does is that rule's own spec."

### Examples for the tests

None. No requirement was added. The existing tests named above already cover the behavior.

### Output questions

_None._

### Internal choices

- The non-goal names no rule and no directive list beyond `#region`/`#endregion`. It defers to each rule's spec instead of the issue's proposed "except its `#region` and `#endregion` directives, which the compiler still reads". Alternative: list the reporting rules, which would go stale, and RH7309 already differs from the issue's list.
- `.claude/commands/create-rule-doc.md` line 29 ("Code in an inactive `#if` branch is not analyzed.") is left as it is. It summarizes what `general-notes.md` owns, and `general-notes.md` already states the region exception. Alternative: repeat the exception there.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1591

No change doc: what is reported is unchanged.

`documentation/rules/RH7003.md`: the `## Exceptions` section is removed, and what it said is folded into `## Description`. The rule reports a block-scoped namespace only when it is the one declaration in its file and declares no namespace inside it, and only from C# 10 on. A file with several namespaces or with a type outside the namespace is not reported; split it first, as RH7001 and RH7002 ask. Each statement matches `sdd/specs/analyzer/RH7003.md` (RH7003-001, -002, -003, -005), which is unchanged. The footer is kept.

### Changed requirements

None.

### Examples for the tests

None. The page examples are unchanged, and the rule has no code fix.

### Output questions

_None._ The page now describes the existing behavior more plainly; what is reported does not change.

### Internal choices

- The description also names the nested-namespace condition (RH7003-005), which the old Exceptions section left out. It is part of "can be converted" in the normal case. Alternative: leave it out, as before.

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`: Passed 7, Failed 0.
- `scripts/test.sh --project architecture`: Passed 198, Failed 0.
- No C# file changed, so `scripts/verify-text-only.sh` does not apply.
- `scripts/lane-check.sh docs --head worktree`: `LANE CHECK: confirmed — claimed docs, diff needs docs`; `TEXT-ONLY PROOF: PASS` (3 non-compiled files, 0 C#).
