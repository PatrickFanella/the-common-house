# Converted-office home base

Direction confirmed by the owner on 2026-09-29; decisions D-28, D-38, and D-39 in the
[register](decisions-and-open-questions.md). This document owns environment
direction. [Issue #87](https://git.subcult.tv/PatrickFanella/the-common-house/issues/87)
owns the first implementation slice. Room uses below are provisional and can
change as the writing develops.

The home occupies a converted telecommunications office building. The owner's image supplies a
large modern shell: pale masonry wings, wide curtain-glass fronts, repeated
vertical fins, deep entrance recesses, and horizontal facade bands. It is an
architectural reference, not an imported game asset or a confirmed real-world
location. The image stays in the owner's attachment storage; its original
building, architect, and image license are unknown.

## Attitude and materials

DedSec in Watch Dogs 2 is a strong influence on the collective's visual
attitude: DIY equipment, graphic experimentation, irreverence, and shared
spaces for making things. Ubisoft's [reveal article](https://news.ubisoft.com/en-us/article/2lxgl1Nhk2dRAs8VYCx5OL/watch-dogs-2-reveal-heres-what-you-need-to-know-about-ctos-bay-area-invasion)
provides the collective's context. Environment artists' portfolio entries
offer reference leads: [Hugo Deschamps's hackerspace](https://hugo.artstation.com/projects/nLlK4)
and [Matt Kazan's bunker hackerspace](https://maxx0rz.artstation.com/projects/q9gNvP).
The portfolio search listings identify the work; their full pages were not
retrievable during this pass. No portfolio or Ubisoft artwork is copied into
the asset set.

Our interpretation combines an orderly office shell with visible household
adaptations. Pale masonry, aluminum framing, tinted window bands, carpet,
suspended ceilings, door frames, and old service storage retain the building's
prior use. Plywood repairs, exposed conduit, salvaged terminals, parts bins,
portable privacy screens, patched upholstery, and communal work surfaces show
how it has been made useful again.

The first implemented study uses teal, magenta, and yellow to mark signs and repaired equipment against bone white,
charcoal, and worn neutrals. Graphic panels use original bitmap lettering,
offset print blocks, stripes, and original interrupted broadcast-ring prints. Broad architectural shapes and readable
color patches should survive the existing PS1 surfaces and soft PS2 frame.
DedSec's exact logos, slogans, cast, and world are not proposed story content.

Domestic comfort belongs beside equipment: somewhere to sit, eat, rest, and
work with another person. Clutter should have a purpose and keep walking paths
clear. A sleeping area has no assigned beds or character belongings yet.

## First ground-floor layout

The working conversion keeps the existing Case 01 rooms in place. Its south
wall opens into a former office corridor that connects the new spaces.

```text
                 north / existing case rooms
  +----------------+-----------------+----------------+
  | radio room     | shared kitchen  | archive /      |
  |                |                 | private alcove |
  +------door------+------door-------+------door------+
  |                 office concourse                  |
  +------door------+------door-------+------door------+
  |                |                 | shared sleeping|
  | repair         | reception       +------door------+
  | hackerspace    | commons         | utilities      |
  +----------------+-----------------+----------------+
                 south / glazed front
```

| Location | Earlier office use | Conversion study |
|---|---|---|
| Radio room | Communications office | Existing Case 01 receiver and routing desk |
| Kitchen | Staff break room | Existing shared table and care interactions |
| Archive/private alcove | Records and maintenance office | Existing clues and private conversation |
| Office concourse | Internal circulation | Wayfinding tape, notice wall, lockers, surface wiring |
| Reception commons | Reception and meeting area | Reused counter, mismatched sofas, communal table |
| Repair hackerspace | Open-plan office bay | Workbenches, terminals, toolboard, components and parts storage |
| Communal sleeping | Meeting-room bay | Unassigned cots, lockers, portable privacy divider |
| Utilities | Server/service room | Rewired racks, battery bank, shared maintenance storage |

This slice adds a multi-storey exterior shell so the building's scale and
architecture are visible in review. Upper floors have no playable interiors.
The closed upper-floor access panel suggests future space without opening a
route. Present-day play stays inside the home base; the exterior landing is
visual context, not an expedition area.

## Asset and location sequence

First establish a walkable ground floor with recognizable office forms and a
small reusable prop kit. Inspect native renders and preserve the existing case
flows. Subsequent bounded slices can refine the facade, build a stairwell and
one upper-floor zone, improve domestic facilities, or add more environmental
variation. Those are follow-up candidates, not implemented locations or queued
tasks.

The incident occurred a few months before the player arrives (D-30). Exact
geography, calendar, former operator, infrastructure condition, resident
ownership of spaces, and character-specific decoration remain open. The
[graphic reference study](graphic-language.md) proposes how the supplied
DedSec images can inform later environment assets. Nothing in this study establishes new case evidence,
resident histories, or a release scope.
