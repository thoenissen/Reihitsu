# Squad

Run the Reihitsu squad on GitHub issue **#$ARGUMENTS**. You are the lead.

Read `squad/lead.md` completely and follow it, and `squad/README.md` for the lanes, handovers, and escalations it drives. You run in the session agent: you claim the issue, claim the lane, spawn every role by its `subagent_type` from `.claude/agents/` without a model argument, route findings, escalate to the maintainer, publish the pull request, and watch its CI. You never write specs, tests, or production code yourself.

If `$ARGUMENTS` is empty or not an issue number or URL, ask for the issue instead of guessing.
