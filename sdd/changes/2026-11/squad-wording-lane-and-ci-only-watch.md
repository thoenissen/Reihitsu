# The squad runs a wording lane, checks coverage first, checks line limits locally, pushes less, and watches a pull request only until CI is decided

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | none — process change |
| Depends on | [The squad refuses by cause, checks its own text, and drops the strict documentation proof](squad-refuses-by-cause-and-checks-its-text.md) |

## What

Five rules, each accepted by the maintainer at triage of thoenissen/Reihitsu#1906; the fifth was added by the maintainer.

- **A wording lane.** A run whose diff changes only what a diagnostic or a code fix says — diagnostic titles and messages and code-fix titles — together with the text that repeats them, runs the lane `wording`: spec author, spec checker, validator. No skeptic, test implementer, implementer, cleaner, or auditor. The spec author writes every path of the diff, the resource texts included. A run that turns out to need a test — a new one, or an existing test that quotes a text the run changes — needs a wider lane, and the lead widens.
  - `scripts/lane-check.sh` gains the lane `wording` and the path category *Wording*: `src/Reihitsu.Analyzer/AnalyzerResources.resx` and `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx`. Every other `*.resx` stays production.
  - The lane the diff needs is decided by ordered rules: production first, as today; then tooling, which next to wording needs `behavior-change` with a change doc and admits no lane without one; then wording, which next to a test path needs `behavior-change` with a change doc and `tests` without one, and otherwise needs `wording`, with or without a change doc; then the rules of today for a change doc, tests, and text.
  - Lanes widen along docs → wording → tests → bug fix → behavior change, and docs → tests → tooling → behavior change; `tooling` does not cover `wording`.
  - The lane check runs the text-only proof without the wording paths, through a new `--exclude <path>` option of `scripts/verify-text-only.sh`, so a C# comment that repeats a changed title stays text. A proof line that excluded paths says how many.
  - In the docs and wording lanes, which have no test implementer and no auditor, the one batch of output questions is asked after the spec author, before the spec checker. New texts are an output question unless a maintainer answer already covers them.
  - `scripts/README.md`, *The lane check*, is the one statement of the classification and the lane rules; `squad/README.md` and the role files point there.
- **Coverage first.** Before a role asks for a test or writes one, it checks whether an existing test already pins the value. A pinned value gets no new test; a changed pinned value updates the existing test. "No new test" is a valid outcome, and the handover names the pinning test. `squad/README.md`, *Coverage first*, states the rule; the skeptic and the test implementer apply it.
- **Line limits checked locally.** When a run's diff adds lines to a C# file of a test project, the validator runs the self-hosting scan's line-count rows after the default run: `scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"`. `scripts/README.md`, *The line-count rows*, states which rows these are and why the default run and the build miss them.
- **Fewer pushes.** The lead pushes only at stage boundaries: the claim, before every stop that waits for the maintainer, before ending a run that cannot go on, the trigger commit, and each CI repair. Handover commits between them stay local. `squad/lead.md`, *Pushes*, states the list.
- **The pull request is watched only until CI is decided.** The lead subscribes to the published pull request's activity only for the CI result of its final commit. Green: it unsubscribes at once and the run ends. Red: it repairs as before, pushes, waits for the next result, and unsubscribes once it is green, or once every remaining failure also exists on `main` and is decided as a follow-up. Review comments, the merge, and closed issues no longer reach the run's session; the maintainer sends further changes in the chat. `squad/lead.md` section 9 and the squad paragraph of `CLAUDE.md` state it.

## Why

- **Two wording runs found nothing in their heavy stages.** The runs that changed the RH5112/RH5048 and the RH5201 member-access wording each ran the full behavior-change lane. The skeptic and the auditor, both on the large tier, found nothing in either. In the RH5201 run they read about 4.6 million cached tokens of the run's 36.5 million for a five-line text change. The lane check could not admit a shorter lane: it classified the two resource files as production.
- **Tests were requested by precedent.** The RH5201 run added three wording tests; two duplicated `AnalyzerPackageMetadataTests`, which already pins each rule's title against its rule-page heading and package README row, and its message against its title. The maintainer had them removed, at the cost of a repair round.
- **A line limit failed only in CI.** The same run grew an analyzer test class to 3015 lines, past the 3000-line limit RH2111 applies to test types. The build loads the analyzers from `binaries/` without the root `reihitsu.json` (`Directory.Build.props`), and the default run leaves out `Reihitsu.SelfHosting.Test`, so only CI saw it, and it cost a full repair round. The two line-count rows take under a minute locally after a build; the rows of every analyzer had not finished after nine minutes.
- **Pushed handover commits only make noise.** Every pushed commit fires check-suite events on the pull request, `[skip ci]` or not, and the lead had to read each one.
- **Later pull-request events wake sessions with nothing left to do.** After CI is green, the run is over; a review comment, a merge, or a closed issue woke a cold session whose context had to be paid again before it could do anything, and the maintainer can reach that session in the chat anyway.

## Visible output changes

None — no output an end user sees changes. The change is to the squad's process and to two repository scripts, `scripts/lane-check.sh` and `scripts/verify-text-only.sh`.

## Decisions

### The wording category names the two resource files

Every value in the two files is a diagnostic title, a message format, or a code-fix title, apart from four sample entries of the resource template that no code reads. Naming the files keeps the category to the texts the maintainer named. A resource file added later, for example for CLI messages, stays production until a change classifies it. A content check — only `<value>` texts of existing entries changed — would need the diff's content, not its paths, in the classifier. The spec author's role file states that rule instead, and the validator's run catches a removed or renamed entry through `AnalyzerResourcesTests` and `CodeFixResourcesTests`.

### No test path in the wording lane

About twenty test files quote a diagnostic message as a literal instead of reading the analyzer's `MessageFormat`; a wording change to one of those rules changes a test. The lane check sees paths, not what changed in them, so a wording lane that admitted test paths would also admit new tests, and nobody would derive them from a spec. A diff with a test path needs a wider lane: `tests` without a change doc, `behavior-change` with one, as a change doc and a test do today. The cause is the quoted literals, so removing them is a follow-up, not a lane rule.

### The proof excludes the wording paths

The text-only proof answers whether a change can affect compiled behavior, and a resource text does, so it must not pass the resource files. Without an exclusion, a wording diff that also edits a C# comment quoting the old title, as the RH5201 run did, fails the proof and turns that comment edit into production. The lane check classifies the wording paths itself and passes them to the proof as exclusions; the proof stays unchanged for every other caller.

### A wording diff may carry a change doc

`sdd/README.md` keeps the diagnostic message and the code-fix title out of requirements, so a wording change often needs no change doc. Both earlier wording runs wrote one anyway, and whether one is required stays the lead's decision. The lane admits both, so the decision does not move the run into another lane. A change doc without a wording path still needs `behavior-change`, as before.

### Tooling and wording without a change doc admit no lane

This mirrors production next to tooling. The tooling lane runs no spec author, so it would change visible text with no one writing the text and the rule pages that repeat it.

### The line-count rows only, and only for test files

The maintainer's rule names test files and the matching row. The filter `Name~RH2008|Name~RH2111` selects the two analyzer rows and the tests of the scan's line-count setup, 18 tests that took 41 seconds after a Release build. The other analyzer rows stay with CI, as before. Production files are judged by the same two rows, but the trigger stays as the maintainer stated it; widening it is a follow-up candidate.

### Stage boundaries are the points where the branch has to hold the run

A run on the branch survives a lost session only if it was pushed. The points where a session can be left waiting or ended — a stop for the maintainer, a run that cannot go on — are pushed. The claim and the publishing pushes are what the pull request needs. Between them a lost session loses at most one stage's local commits, which a fresh session redoes from the last pushed handovers.

## Alternatives considered

- **Classify every `*.resx` under `src/` as wording** — rejected: a resource file a later change adds might hold text that is not a title or message, and the lane check would admit it without anyone deciding.
- **Admit a test path in the wording lane when it changes only a string literal** — rejected: it needs a content check of C# test files in the lane check, a proof of its own beside the text-only proof, for a case the follow-up removes.
- **Let the proof treat the resource files as text** — rejected: a resource text is compiled into the assembly and changes what users see; the proof would then pass a change that is not text-only.
- **Run the whole self-hosting scan locally when a test file grows** — rejected: the rows of every analyzer had not finished after nine minutes, and the failure the evidence shows is a line limit.
- **Keep the subscription until the pull request is merged or closed** — rejected by the maintainer: those events wake a session that has nothing left to do.
