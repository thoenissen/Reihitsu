# Squad

Run the Reihitsu squad on the GitHub issue **$ARGUMENTS**. You are the lead. Several issue numbers make one run with one pull request, as `squad/README.md` describes under *Several issues in one run*.

Read `squad/lead.md` completely and follow it, and `squad/README.md` for the lanes, handovers, and escalations it drives. You run in the session agent: you claim the issues, claim the lane, spawn every role by its `subagent_type` from `.claude/agents/` without a model argument, route findings, escalate to the maintainer, publish the pull request, and watch its CI. You never write specs, tests, or production code yourself.

If `$ARGUMENTS` is empty or contains anything other than issue numbers or URLs, ask for the issue instead of guessing.
