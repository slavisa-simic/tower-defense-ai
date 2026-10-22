# Phase 3A-1: master visual reference brief

Status updated 2026-09-19: master v001 was revised to v002 for projection/readability. User selected v002 by replying "continue" after the displayed revision and approval question. Packs use that immutable reference. Scope is recorded in `ASSET_LIST_PHASE_3A1.md`: master first, then environment, props, base/fortifications and basic UI. Enemy/defense production and animation remain excluded.

The next explicitly requested art task should create a master visual reference for Border Defenders: gritty, readable pixel art, strict perpendicular overhead view, alternate post-nuclear Balkan Danube quarantine line. Show water on the left, muddy/concrete riverbank defenses, ground combat space, and the base on the right. Include visual examples of a Crawler, MG emplacement, wire, mine, sniper emplacement, and radiation-resistant inflatable/improvised river craft in a coherent design language. These are reference examples, not a production sprite sheet or new gameplay features.

Use `ART_PRODUCTION_RULES.md`. Preserve the prototype's 32 x 18 map composition and existing gameplay. Do not derive perspective or lighting from the legacy isometric scene. Record which current screenshot was used as a layout reference; a gameplay screenshot supplies composition, not final style.

Deliverable: `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v001.png`, with an actual-path/prompt record in `Docs/Art/Batches/p3a1_master_001.md`. Save a different version if that filename already exists. Use generated output's actual native dimensions and inspect it before copying the selected file. Do not insert the reference into the gameplay scene.

Review the master for:

- Overhead roofs, shoulders and craft; no isometric or side-facing assets.
- Coherent material palette and pixel cluster density; muted terrain and readable units/defenses.
- Distinct Crawler, defense and base silhouettes when viewed small.
- Clear quarantine/military/Danube identity without anachronistic glossy interfaces.
- Compatible neutral lighting for rotating local-+X units and weapons.
- A stated pixel-density proposal and exact swatches sampled from the selected reference. The current 64-pixel-cell / 51.2-PPU proposal follows the user's Phase 3A-1 brief and supersedes the unapproved setup proposal.

Record approval evidence and the immutable reference version in the register. If the user requests revisions, make focused changes and keep earlier versions identifiable. Only after selection proceed to the specifically authorized environment pack, starting with water/shore/grass checks before expanding. The current request supersedes the setup's generic Crawler/defense pilot order for this phase. A good-looking reference alone does not prove alpha quality, tile seams, animation continuity, or Unity integration.
