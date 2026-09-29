# Research Workflow

Curated from a previously merged skill. This file keeps the useful task guidance without preserving the old skill as an active nested skill.

## Extracted Resources

- `assets/research-workflow/`

## Guidance

### From `research-workflow-skill.md`

_Source topic: research-workflow_

**Purpose:** Guide agents through structured research including planning, multi-query execution, source analysis, and synthesis. Use for comprehensive topic research, deep investigation, or creating research reports. Keywords: research, investigate, deep dive, comprehensive, analysis, synthesis, report.

# Research Workflow

A structured methodology for conducting comprehensive research. This skill guides you through planning, executing, analyzing, and synthesizing research on any topic.

## When to Use This Skill

Use this skill when:
- The user needs comprehensive research on a topic
- Multiple search queries are needed to fully answer a question
- Source credibility and synthesis matter
- A research report or documented findings are expected
- Keywords mentioned: research, investigate, deep dive, comprehensive analysis

Do NOT use this skill when:
- A single quick search will suffice (use web-search instead)
- The user just wants a simple fact lookup
- No synthesis or analysis is needed
- Time is extremely limited

## Prerequisites

- **Web search capability** is available (web-search skill, WebSearch tool, or similar)
- Sufficient time for multi-phase research process
- Clear understanding of the research question or topic

## Research Phases Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    RESEARCH WORKFLOW                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. PLANNING          2. EXECUTION                          │
│  ┌──────────────┐    ┌──────────────┐                       │
│  │ Define       │    │ Run searches │                       │
│  │ questions    │───>│ Evaluate     │                       │
│  │ Plan queries │    │ sources      │                       │
│  └──────────────┘    └──────────────┘                       │
│         │                   │                               │
│         v                   v                               │
│  3. ANALYSIS          4. SYNTHESIS                          │
│  ┌──────────────┐    ┌──────────────┐                       │
│  │ Organize     │    │ Create       │                       │
│  │ findings     │───>│ coherent     │                       │
│  │ Find patterns│    │ output       │                       │
│  └──────────────┘    └──────────────┘                       │
...
```

## Phase 1: Planning
...

# Current news
web-search "AI healthcare regulations 2024" --topic news --time month
```

### Step 2: Document Each Search

For each search, record:
- Query used
- Number of results reviewed
- Key findings (2-3 bullet points)
- Notable sources
- New questions raised

### Step 3: Evaluate Sources

Use the checklist at assets/source-evaluation-checklist.md to assess:

**Credibility Indicators**:
- Author/organization expertise
- Publication reputation
- Date of publication
...
```

1. What versioning strategies exist?
2. What are pros/cons of each?
3. What do major companies use?
4. What do experts recommend?

- "API versioning strategies comparison"
- "REST API versioning best practices 2024"
- "API versioning header vs URL vs query parameter"
- "large companies API versioning approach"
```

**Phase 2 - Execution**:

### From `research-plan-template.md`

_Source topic: research-plan-template_

# Research Plan: [Topic]

## Research Question

[Clear statement of what you want to discover]

## Sub-Questions

1. [Specific question that contributes to the main question]
2. [Another specific question]
3. [Another specific question]
4. [Another specific question]

## Scope

### In Scope
- [What will be covered]
- [What will be covered]

### Out of Scope
- [What will NOT be covered]
- [What will NOT be covered]

## Search Strategy

### Broad Queries (Start Here)
- "[topic] overview"
- "[topic] introduction guide"
- "[topic] current state [year]"

### Specific Queries (Dive Deeper)
- "[topic] [specific aspect 1]"
- "[topic] [specific aspect 2]"
- "[topic] case study examples"

### Verification Queries (Confirm Findings)
...


### From `research-report-template.md`

_Source topic: research-report-template_

# Research Report: [Topic]

## Executive Summary

[2-3 paragraphs summarizing the most important findings and recommendations. Write this last after completing the full report.]

## Research Question

## Methodology

- **Queries executed**: [Number]
- **Sources reviewed**: [Number]
- **Time period**: [When research was conducted]
- **Search tools used**: [Tools/skills used]
- **Time range filter**: [If any time restrictions applied]

## Key Findings

### Finding 1: [Title]

### Finding 2: [Title]

### Finding 3: [Title]

## Source Analysis

| Source | Type | Credibility | Key Contribution |
|--------|------|-------------|------------------|
| [Name/URL] | [Academic/Industry/News/Blog] | [High/Medium/Low] | [What it provided] |
| [Name/URL] | [Type] | [Level] | [Contribution] |
| [Name/URL] | [Type] | [Level] | [Contribution] |

### Source Consensus

- [Topic where multiple sources agree]
- [Topic where multiple sources agree]
...


### From `source-evaluation-checklist.md`

_Source topic: source-evaluation-checklist_

# Source Evaluation Checklist

Use this checklist to assess the credibility and quality of sources during research.

## Quick Assessment

For each source, answer these questions:

### Authority
- [ ] Author or organization is identified
- [ ] Author has relevant expertise or credentials
- [ ] Organization is reputable in this field
- [ ] Contact information or about page is available

### Accuracy
- [ ] Information can be verified elsewhere
- [ ] Claims are specific and testable
- [ ] Data sources are cited
- [ ] No obvious factual errors

### Currency
- [ ] Publication date is provided
- [ ] Information is recent enough for the topic
- [ ] Updates or revisions are noted
- [ ] Links and references are still active

### Purpose
- [ ] Purpose of the content is clear
- [ ] Commercial interests are disclosed
- [ ] Sponsorship or funding is transparent
- [ ] Content is not purely promotional

### Objectivity
- [ ] Multiple perspectives are considered
- [ ] Counterarguments are addressed
- [ ] Language is professional, not sensational
...


### From `methodology.md`

_Source topic: methodology_

# Research Methodology

Detailed background on the research methodology used in this workflow.

## Why Structured Research?

Unstructured research often leads to:
- Incomplete coverage of the topic
- Confirmation bias (finding what you expect)
- Poor source evaluation
- Unfocused or rambling outputs

## The Four-Phase Model

### Phase 1: Planning

- Transform vague topics into specific questions
- Identify knowledge gaps to fill
- Plan query sequence from broad to specific
- Set success criteria

- Skipping directly to searching
- Asking only one question
- Not planning for verification queries

### Phase 2: Execution

- Execute queries in planned order
- Document findings immediately
- Evaluate source credibility
- Identify emerging questions

1. **Funnel Pattern**: Start broad, narrow down
   - Overview → Specifics → Details

2. **Multi-perspective Pattern**: Cover different viewpoints
...


### From `output-formats.md`

_Source topic: output-formats_

# Output Formats

Detailed specifications for research output formats.

## Choosing the Right Format

| Format | Best For | Length | Time |
|--------|----------|--------|------|
| Executive Summary | Quick decisions | 1-2 paragraphs | 5 min |
| Key Findings | Status updates | 5-10 bullet points | 10 min |
| Full Report | Documentation | 1-5 pages | 30+ min |
| Comparison Table | Option evaluation | 1 page | 15 min |
| Action Plan | Implementation | 1-2 pages | 20 min |

## Executive Summary Format

**When to use**: Stakeholders need quick overview for decisions

```markdown
## Executive Summary: [Topic]

**Key Finding**: [One sentence main conclusion]

**Supporting Points**:
- [Point 1]
- [Point 2]
- [Point 3]

**Recommendation**: [What to do next]

**Confidence**: [High/Medium/Low]
```

**Example**:
```markdown
## Executive Summary: API Rate Limiting Approaches

**Key Finding**: Token bucket algorithm is the industry standard
for its balance of simplicity and fairness.

**Supporting Points**:
- Used by major providers (Stripe, GitHub, AWS)
- Handles burst traffic gracefully
- Easy to implement and explain to users

**Recommendation**: Implement token bucket with per-user quotas

**Confidence**: High (strong consensus across sources)
```

## Key Findings Format
...

# Research Report: [Topic]

## Executive Summary
[Condensed version for quick reading]

## Research Question
[What you set out to discover]

## Methodology
- Queries executed: [count]
- Sources reviewed: [count]
- Time period: [dates]
- Search tools used: [tools]

## Findings

### [Theme 1]

| Source | Type | Credibility | Key Contribution |
|--------|------|-------------|------------------|
| [Name] | [Type] | [Level] | [What it added] |

### [Theme 2]

## Analysis

### Consensus Points

### Conflicts

### Patterns

## Limitations

## Recommendations
1. [Recommendation with rationale]
...
