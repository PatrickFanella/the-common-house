---
name: deep-research
description: "Research a topic thoroughly in this repo and return a structured summary with file references. Use when you need to understand how something works, find patterns across modules, or audit implementations."
metadata:
  blacktower-pack: engineering
  blacktower-kind: workflow
  blacktower-tier: standard
  blacktower-capabilities: dev-workflows,deep,research
  stage: tool
  tags: research
allowed-tools: Read Grep Glob
---

# Deep Research (Explore)

Research $ARGUMENTS thoroughly.

## Required output

1. **Findings**: 3-10 bullets, each backed by specific file references
2. **Key code locations**: a short list of the most important paths to read next
3. **Open questions / unknowns**: what you could not confirm from the repo

## Method

- Start broad: use Glob to find likely modules.
- Then go deep: use Grep to locate exact identifiers and call sites.
- Read the top-ranked files and follow references until the flow is clear.
- Stop when you hit diminishing returns (two additional searches yield no new information).

## Depth guidelines

- **Quick scan** (< 5 files): Answer a specific question about one module or function.
- **Standard research** (5-15 files): Trace a flow across modules, map dependencies.
- **Deep audit** (15+ files): Full cross-module investigation, pattern analysis, consistency check.

Default to standard depth. Escalate to deep audit when the user says "thorough", "audit", or "comprehensive".

## Rules

- Be evidence-driven: always cite file paths.
- Do not implement changes; this skill is for investigation only.
- Prefer Grep with specific patterns over broad Glob sweeps.
- When tracing a call chain, follow imports and function references, not just text matches.
- If the repo has tests, check them too — tests often reveal intended behavior better than implementation.

## Resources

Load these only when the active task needs the extra detail; keep `SKILL.md` as the activation workflow.

### Curated references

Former merged-skill dumps were distilled into these lookup files:

- `references/curated/research.md` — Research guidance.
- `references/curated/research-workflow.md` — Research Workflow guidance.


### Extracted resource directories

- `references/` — curated resources extracted from prior merged skill material.
- `scripts/` — curated resources extracted from prior merged skill material.
- `assets/` — curated resources extracted from prior merged skill material.
