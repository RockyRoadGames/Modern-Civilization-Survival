# Modern Civilization Survival

A multiplayer Minecraft 1.21.1 NeoForge survival project built around a functioning modern city.

## Vision

The city should be more than a collection of buildings. We want systems that actually operate:

- Homes and apartments connected to utilities
- Banks, ATMs and player-run commerce
- Fast food, groceries, offices and other businesses
- Factories, warehouses and delivery networks
- Roads, parking garages, rail and public transit
- Electricity generation, distribution and industrial power
- Oil/fuel production and service stations
- Mailboxes, packages, sorting and delivery
- Vehicles as an experimental system
- Computer-controlled infrastructure later
- Multiplayer ownership, jobs and city services later

## Foundation

Minecraft 1.21.1 + NeoForge + Create 6.x.

The initial build uses one main system per problem rather than stacking several competing technology mods. Exact versions are resolved from Modrinth at install time and locked in `modpack/resolved-mods.json`.

## Test progression

1. Boot test: server + client.
2. Create mechanical automation.
3. Electricity: Create New Age + Crafts & Additions + Diesel Generators.
4. Heavy industry/oil: TFMG.
5. Storage/logistics: Refined Storage + Create logistics.
6. Economy: Numismatics + Numismatics Utils.
7. Mail: Ender Mail Reborn.
8. Rail/transit: Steam 'n' Rails.
9. Modern building/interiors.
10. Optional experimental systems.
11. Multiplayer load/stability test.

Do not open the server to friends until the core systems pass the test list above.

## Important

The server and client are intentionally separate from the existing Secret-Facility and Redstone Learning projects.

Mod JARs are not committed to Git. The resolved lockfile contains project IDs, exact Modrinth version IDs, filenames, download URLs and hashes so the same files can be reproduced.
