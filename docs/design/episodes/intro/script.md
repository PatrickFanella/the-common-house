---
title: Intro: playable script
type: documentation
status: draft
created: 2026-09-29
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Intro: playable script

[Package guide](README.md) · [Scene contract](case-contract.md). All lines are draft. IN denotes this intro, not an engine API. Use explicit interactions to begin directed exchanges. Pause, close, and readable replay are available throughout; essential learning is acknowledged separately from playback.

## IN-S00: the lit entrance

**PLAY.** Entrance frontage inside the refuge, or threshold fallback. The company sign is worn; a planter and upstairs laundry show habitation. A short visible route leads to an openable entrance. The player's goal is to find someone to ask where they are. Look and move remain under player control.

| Line | Speaker | Text |
|---|---|---|
| IN-L001 | Ren, inside | There's someone by the steps. |
| IN-L002 | Ivo | Someone from where? |
| IN-L003 | Ren | I don't know. Get the door. |

The exchange starts on approaching the entrance interaction, not a background timer. If skipped, a persistent "Speak to the resident at the entrance" lead remains. No actor movement must be watched to proceed. An unimplemented exterior is staging to build, not a claim that this geography already exists.

## IN-S01: another arrival

**INTERACT + DIRECTED.** Ren comes to the entrance. Nobody requires a name or explanation before offering shelter.

| Line | Speaker | Text |
|---|---|---|
| IN-L010 | Ren | Come up here. Are you hurt? |
| IN-L011 | Ren | Was there anyone with you? |

IN-Q01 offers "I was following a path," "I thought this was somewhere else," "I don't know how I got here," and "Can I come inside?" These express the player's immediate account without a backstory assignment. Respond with the corresponding lines:

| Line | Speaker | Text |
|---|---|---|
| IN-L012 | Ren, following a path | You can show us from the doorway. Come in first. |
| IN-L013 | Ren, wrong place | That happens. I can tell you what we know. |
| IN-L014 | Ren, uncertain | All right. You don't have to work it out standing there. |
| IN-L015 | Ren, shelter request | Yes. There's water inside. |

All fold back to entry and water. "I need a minute" closes the interaction and leaves the door/help available. The script neither grants an injury nor forces one. Actual injury, companions, or a medical sequence require separate writing.

## IN-S02: from here

**INTERACT + CHOICE.** At the threshold, Ren addresses the player's wish to return. No one takes them past the visible refuge. The outside scene offers no demonstrated safe route.

| Line | Speaker | Text |
|---|---|---|
| IN-L020 | Player option | How do I get back? |
| IN-L021 | Ren | Can you show me where you came from? |
| IN-L022 | Player option | I think so. |
| IN-L023 | Ren | From here. Show me from here. |
| IN-L024 | Ren | The paths change between places like this. Instruments don't give us a route we can trust. |
| IN-L025 | Ren | You might get to that fence. You might turn around and find us behind you. |
| IN-L026 | Player option | And if I don't? |
| IN-L027 | Ren | That's why I'm asking you to stay where you can see the building. |
| IN-L028 | Ren | You can be all right out there and still not find your way back. |
| IN-L029 | Player option | Has that happened to someone here? |
| IN-L030 | Ren | People have left. We haven't heard from all of them. That doesn't tell us where they are. |

Data menu IDs IN-Q04 and IN-Q05 identify the return questions and pointing alternatives. IN-Q06 identifies address choices, IN-Q07 chronology questions, IN-Q08 network-location questions, and IN-Q03 the final shift offer. These supplement IN-Q01 and IN-Q02 without changing line IDs.

Optional pointing is an expression, not a navigation minigame or a verified arrival route. IN-Q05 pairs "I think so" with "I can't show you." If the player cannot describe the path, omit IN-L022 and continue the explanation. Ren's accounts are attributed experience, not omniscient geography.

**Exit/recovery.** Explicit acknowledgment records IN-C01, the warning. Closing grants no acknowledged learning. The conversation can be reopened from reception. Turning away never teleports the player into safety or starts an unplanned lost-in-the-disruption sequence. Final playable boundaries need scene review; the threshold-only staging avoids offering an unsupported travel mechanic.

## IN-S03: reception

**PLAY + INTERACT.** Old customer counter, ledger, bedding, service-area map. The map has station names, question marks, and crossed-out distances. IN-D01's readable description preserves those marks without supplying coordinates to refuges.

| Line | Speaker | Text |
|---|---|---|
| IN-L040 | Ivo | What should I put down? |
| IN-L041 | Player option | My name. |
| IN-L042 | Player option | Leave it blank for now. |
| IN-L043 | Ivo | All right. We can fill it in later. |
| IN-L044 | Ren | Do we still have the spare bedding? |
| IN-L045 | Ivo | Which spare bedding? |
| IN-L046 | Ren | The bedding we keep saying is spare. |
| IN-L047 | Player option | Does this happen often? |
| IN-L048 | Ivo | At first, every day. Sometimes several people. It's been a while now. |
| IN-L049 | Player option | How long since it happened? |
| IN-L050 | Ivo | A few months. We keep the dates here, but they don't always help with someone who's just arrived. |

IN-L041 means ask how the player would like to be addressed. Exact input UI is open; a text-first draft can leave the name undecided. Neither choice grants a prescribed identity. Offer bedding in both routes.

**Record.** IN-C02 is Ivo's chronology/arrival account; IN-D01 is the map observation. The ledger does not contain a conveniently complete history of the incident. A date discrepancy is possible without claiming that the player's memory is false.

## IN-S04: people on the network

**MEDIA + PLAY.** An authored local broadcast sequence on the radio desk. The player may listen, read, pause, and replay without tuning skill or a deadline. Channels and names below are fictional working choices.

| Source/line | Speaker | Text |
|---|---|---|
| IN-T01 / IN-L060 | Music presenter | This one's for the night shift at Orchard. Your request finally reached us. |
| IN-T02 / IN-L061 | News reader | South Relay reports that their upper tanks are working again. We haven't heard directly from the households downstream. |
| IN-T03 / IN-L062 | Remote repair operator | I didn't say cut it. I said disconnect it. |
| IN-T03 / IN-L063 | Second repair operator | Well, it's disconnected. |
| IN-T04 / IN-L064 | Orchard operator | Is Ren there? Tell them the recording was terrible. I'm sending it back. |
| IN-L065 | Ren | They asked for something cheerful. |
| IN-T04 / IN-L066 | Orchard operator | And who's new? I can hear you finding another chair. |
| IN-L067 | Ivo | We've had an arrival. We'll check in later. |

IN-T04 is a present-day live radio exchange, unlike the later recorded greeting. Outgoing scope is limited to IN-L067. The desk microphone is visibly active for that exchange; its status is described in text. The newcomer is not identified, asked to speak, or broadcast automatically. The audible chair gives the remote operator an ordinary basis for the question.

| Line | Speaker | Text |
|---|---|---|
| IN-L068 | Player option | Where are they? |
| IN-L069 | Ivo | They call their place Orchard. |
| IN-L070 | Player option | How far away? |
| IN-L071 | Ivo | We don't know. |
| IN-L072 | Ivo | Some stations speak for whole districts. Some are one person in a house. Radio is how we reach each other. |

**Record.** IN-C03 is the network explanation. The program's reports are attributed, with uncertainty intact. Music need not be a licensed track or final asset; captions and spoken introduction suffice for the first slice.

## IN-S05: a place for tonight

**PLAY + DIRECTED.** Former meeting room repurposed as kitchen. Tablecloth, projector bracket, and mismatched labels show different residents' modifications. The player receives food and bedding without a work obligation.

| Line | Speaker | Text |
|---|---|---|
| IN-L080 | Ren | That mug leaks. Take the other one. |
| IN-L081 | Ivo | We can put the bedding in the archive for tonight. |
| IN-L082 | Ren | There's room. We'll move the boxes. |
| IN-L083 | Ivo | Let's check before we promise them a room. |
| IN-L084 | Ren | Somewhere to sleep, then. |
| IN-L085 | Ren, to player | You don't have to decide anything tonight. |
| IN-L086 | Player option | Decide what? |
| IN-L087 | Ren | Whether you're staying. |

**Direction.** Ivo briefly looks toward the entrance after IN-L087. It conveys unease without adding a factual clue. Text alone must establish the shelter offer. Exact sleeping spot is staging, not permanent ownership or entry to private rooms. Jo may later take the mug line as a reviewed cameo; her episode history is not disclosed here.

IN-Q02 offers "Thank you," "I still want to find a way back," and "I'd rather be on my own for a bit," with the corresponding responses below. All grant shelter. No gratitude or meal input is required to progress.

| Line | Speaker | Text |
|---|---|---|
| IN-L088 | Ren, thanks | We'll show you where things are. |
| IN-L089 | Ren, wanting return | I understand. We can talk about it tomorrow. |
| IN-L090 | Ren, requesting solitude | All right. The food will be here. |

## IN-S06: our office is closed

**INTERACT + MEDIA.** A receiver indicator marks a new recorded fragment. Start on explicit inspection, not an unavoidable interruption of another panel. IN-T05 is a telephone greeting, not a live household announcement.

| Line | Speaker | Text |
|---|---|---|
| IN-L100 | Recorded voice | Thank you for calling. Our office is currently closed. Please call again during normal business hours. |
| IN-L101 | Recorded voice | This is the local service office of <former company name>. |
| IN-L102 | Ivo | That was our greeting. |
| IN-L103 | Ren | Do you know where it came from? |
| IN-L104 | Ivo | No sender I can identify. I'll keep it in the log. |
| IN-L105 | Ren | Has it happened before? |
| IN-L106 | Ivo | I've heard that recording before. Coming through like this? No. |

The former company name is an explicit open text field, not an invitation for an implementer to invent final branding. A first draft can display "the local service office" until it is selected. The recording includes no personal information about the player. It does not repeat an unrecorded household conversation or reveal the intelligence.

The source's uncertainty remains in the log. Ivo's recognition is IN-C04. Playback and acknowledgment acquire the greeting IN-T05 without granting an explanation. Ren and Ivo's reactions indicate different experience; neither knows the AI survived.

## IN-S07: tomorrow

**DIRECTED + CHOICE.** Return to ordinary household life. Ren offers orientation to the radio desk on a later shift.

| Line | Speaker | Text |
|---|---|---|
| IN-L110 | Ren | Eat first. Tomorrow I can show you the desk. |
| IN-L111 | Player option | I'd like that. |
| IN-L112 | Player option | I'll think about it. |
| IN-L113 | Ren | All right. Get some rest. |

Both choices complete the intro after explicit acknowledgment. Record readiness to settle in, not an obligation to labor. Case start remains a separate explicit action. The first episode connector will be drafted after its story is selected.
