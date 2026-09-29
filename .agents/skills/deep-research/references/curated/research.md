# Research

Curated from a previously merged skill. This file keeps the useful task guidance without preserving the old skill as an active nested skill.

## Extracted Resources

- `scripts/research/`

## Guidance

### From `research-skill.md`

_Source topic: research_

**Purpose:** Diagnose research quality and guide systematic query expansion. Use when starting research on any topic, when stuck in research, or when unsure if research is complete.

# Research Skill

Tool-assisted research with Tavily integration. Transforms basic questions into comprehensive search strategies using AI-optimized web search.

## Setup

This skill includes a bundled Tavily CLI script at `scripts/tavily-cli.ts`.

### Requirements

1. **Deno** - Install from https://deno.land
2. **Tavily API Key** - Get one at https://tavily.com (free tier available)

### Configuration

```bash
export TAVILY_API_KEY="your-key-here"
```

```bash

# Adjust path to where this skill is installed
alias tavily='deno run --allow-net --allow-env /path/to/skills/research/scripts/tavily-cli.ts'
```

Or run directly:
```bash
deno run --allow-net --allow-env ./scripts/tavily-cli.ts "your query"
```

Commands below use `tavily` assuming the alias is configured.

## Phase 0: Analysis

**Goal**: Structure topic before searching. Prevents unfocused searches and scope mismatch.

### Scope Calibration

Before searching, assess stakes:

| Decision Type | Confidence Needed | Research Depth |
|---------------|-------------------|----------------|
| Reversible, low-stakes | 60-70% | Quick scan (minutes) |
| Reversible, moderate | 75-85% | Working knowledge |
| Irreversible, moderate | 85-90% | Solid grounding |
| Irreversible, high | 90-95% | Deep expertise |
...
```markdown

# Research Analysis: [Topic]

## Core Concepts
- **Primary terms:** [Key terms requiring definition]
- **Terminology variants:** [Synonyms, jargon, historical terms]
- **Ambiguous terms:** [Terms with multiple meanings]

## Stakeholders
- **Primary actors:** [Who is directly involved?]
- **Affected groups:** [Who bears consequences?]
- **Opposing interests:** [Who benefits from different outcomes?]

## Temporal Scope
- **Historical origins:** [When did this begin?]
- **Key transitions:** [What changed and when?]
- **Current state:** [What's happening now?]

## Domains
- **Primary field:** [Main discipline]
- **Adjacent fields:** [Related disciplines]

## Controversies
- **Active debates:** [What's contested?]
- **Competing frameworks:** [Different ways of understanding]
```

### Phase 0 Checklist

- [ ] Identified primary terms
- [ ] Listed potential stakeholders
- [ ] Assessed decision stakes
- [ ] Determined appropriate research depth

## Phase 2: Foundational Search

**Goal**: Build foundational understanding with authoritative sources.

### Question Pattern → Tavily Command

| Question Pattern | Strategy | Command |
