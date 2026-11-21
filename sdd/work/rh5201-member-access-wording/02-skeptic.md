Role: skeptic
Commit: f0c3cd3
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing ignored.

## Premise

Real. RH5201 reports links of wrapped chains that hold no invocation, and its user-facing text names a "method chain":

- Text today: `RH5201Title` and `RH5201MessageFormat` = "Method chains should be aligned." (`src/Reihitsu.Analyzer/AnalyzerResources.resx:708-713`); code-fix `RH5201Title` = "Align method chain" (`src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx:330-332`), used as the `CodeAction` title in `RH5201MethodChainsShouldBeAlignedCodeFixProvider.cs:56`.
- Call-less chains reported today, by existing tests: `VerifySplitConditionalOperatorOfCallLessChainIsReported` (`src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs:1929`) and `VerifyMisalignedLinkOfCallLessChainIsReportedAndAlignedUnderTheLastLink` (`src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5201FluentChainLinkFormatterTests.cs:440`); requirements RH5201-002 and RH5201-003 cover them.
- Caller: the analyzer itself (every build with the package) and the RH5201 code fix. Run, `scripts/apply-fix.sh RH5201 <dir>` over two fixtures (LF and CRLF, 4 fixed, 0 other):

```csharp
internal sealed class Example
{
    private static object Run(dynamic order)
    {
        return order.Customer.Address
                    .City;
    }
}
```

fixed in 1 action to `.City` under `.Address` (column of `.Address`);

```csharp
#nullable enable

internal sealed class Example2
{
    private Example2? Next { get; set; }

    private static object? Run(Example2 d)
    {
        return d.Next?
                 .Next;
    }
}
```

fixed in 1 action to `return d.Next` / `?.Next` with `?` under the `.` of `.Next` in the first line. Both chains hold no call, so the diagnostic text "Method chains should be aligned." and the action "Align method chain" describe neither. The script prints no message text; the text is the resource value above.

- Spec wrong instead? No. RH5201's spec already says "member-access chain" throughout; only its title said "Method chains". No requirement has a `Why:` that the new wording contradicts.
- Conflicts: none. thoenissen/Reihitsu#893 (next RH5201 run) widens what RH5201 reports; it does not narrow the rule back to calls, so the new wording stays right after it. No SHARED-DIAG-010/P11 question: no reporting condition or fix output changes.

## Mechanism and candidates

_N/A._ Wording change; no defect mechanism.

## Breaking inputs

No failure class from `squad/failure-classes.md` reaches a text-only change (no reporting condition, location, guard, or rewrite changes).

Every place that carries or pins the old text (`rg -i "method chains? should be aligned|align method chain"`, excluding `sdd/work`, `sdd/changes`):

| Place | Kind | Pinned by |
|---|---|---|
| `src/Reihitsu.Analyzer/AnalyzerResources.resx:709,712` | message, title | `AnalyzerTitleResourcesMatchRuleDocumentationTitles`, `MessageFormatResourcesMatchTitleWhenNoPlaceholders` (`src/Reihitsu.Analyzer.Test/SelfHosting/AnalyzerPackageMetadataTests.cs:244,284`) |
| `src/Reihitsu.Analyzer.Package/README.MD:178` | package README row | `PackageReadmeDescriptionsMatchRuleDocumentationTitles` (same file, :204) |
| `documentation/rules/RH5201.md:1` | heading (already changed at 8aed075) | the two tests above |
| `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx:331` | code-fix title | **no test today** |
| `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs:159` | hard-coded old message in an expected diagnostic | itself; its expected value changes under the maintainer's answer |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs:10` | `///` summary | none (internal) |

No other resource file (no localized `.resx`), no `AnalyzerReleases` file, and `Release.ruleset:252` names only the ID. `binaries/` is rebuilt by the version bump, not here.

Tests the test implementer must cover, following the precedent in `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs:1763-1840` (`VerifyTitleNamesFirstMemberAccess`, `VerifyWrappedPropertyFirstLinkIsReportedWithMemberAccessMessage`, `VerifyCodeFixTitleNamesFirstWrappedMemberAccess`):

1. The RH5201 descriptor's title equals "Member-access chains should be aligned.".
2. A call-less chain (the first fixture above, `{|#0:.|}City`) is reported with the literal message "Member-access chains should be aligned." and fixed to `.City` under `.Address` — the literal string, not `MessageFormat`, since `MessageFormat` reads the resource and cannot fail on a wording regression.
3. The code action offered on that diagnostic has the title "Align member-access chain" and is the only action. Without this, the code-fix title — one of the three answered outputs — stays unpinned.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer title and message (`AnalyzerResources.resx`) | must change |
| Analyzer reporting condition and location | must stay |
| Code-fix title (`CodeFixResources.resx`) | must change |
| Code-fix rewrite and equivalence key (`nameof(RH5201MethodChainsShouldBeAlignedCodeFixProvider)`) | must stay |
| Fix All | must stay |
| Formatter | not involved |
| `Reihitsu.Core` | not involved |
| Rule page `documentation/rules/RH5201.md` | heading must change (done); body is the open output question (change 4) |
| Package README row | must change |
| Spec `sdd/specs/analyzer/RH5201.md` | title changed (done); requirements stay |
| Tests | one hard-coded expectation must change; three wording tests added (above) |
| Other rules' pages saying "method chain" (RH5035/5038/5039/5043/5044, RH5115/5116, RH3007, RH5102) and FMT-INDENT-029 | must stay: RH5035-family sentences mean a later *call* (SHARED-COMMENT-009 keeps a comment before a later invoked link valid, not before a non-invoked one), RH5115/5116 describe their own fixes, RH3007/RH5102 mean method syntax and arguments |

## Refuse option

Option B of the issue (leave the text) — zero cost, and the maintainer already rejected it by choosing A. No unsupported-syntax shape is involved (no comment, directive, or layout decision). Nothing simpler than a resource-and-doc edit exists.

## Output questions

The spec author's open question on the rule page body (change doc change 4) is a genuine output question and still open; I concur with its recommendation A. Checked the rest of the page: the Description bullets and "How to fix it" say "the first call" where they state the anchor rule (SHARED-CHAIN-007: aligned with the first call, else with the last member access in front of the first wrapped line), which is a call; they stay correct under A. The example is a chain with calls and stays.

No other open output question.

## Internal choices

- Test 2 above asserts the literal message rather than `MessageFormat`. Recommendation: literal, as the precedent did; consequence of `MessageFormat` instead: a later wording regression passes unnoticed.
- The `///` summary of `RH5201MethodChainsShouldBeAlignedAnalyzer` follows the new title (the change doc says so); type names stay. Agree with the spec author.

## Issue goals

thoenissen/Reihitsu#1900 is `agent-filed`. Its "Done when" (resources, heading and every place the title test pins, code-fix title; title-matching tests pass) serves the purpose and stays. One gap, no visible change: "title-matching tests pass" does not cover the code-fix title, which no test pins — test 3 above closes it. The issue does not mention the rule page body; that is change 4, asked separately.

## Architecture findings

_None._

## Follow-up candidates

_None._
