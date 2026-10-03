# Research Notes â€” Modern Civilization Survival
# Verified from current Modrinth/NeoForged pages on 2026-10-03

## Foundation decision

Minecraft 1.21.1 + NeoForge is the selected test foundation. NeoForged's documentation requires Java 21 for Minecraft 1.20.5 and newer, and the 1.21.1 ecosystem remains large and widely supported. The official Create page shows Create 6.0.10 for Minecraft 1.21.1 on NeoForge, client and server.

## Core systems researched

Create â€” mechanical automation and logistics base.

Create: New Age â€” electrical generation, motors, wires, heat systems and optional reactor/solar systems. 1.21.1 NeoForge release exists.

Create: Crafts & Additions â€” Create/electricity bridge. 1.21.1 NeoForge release exists.

Create: Diesel Generators â€” diesel engines, industrial features and crude-oil refinery. 1.21.1 NeoForge release exists.

Create: The Factory Must Grow â€” heavy engineering/oil for Create. 1.21.1 NeoForge releases exist, including release builds later than the initial port.

Create: Connected â€” Create QoL blocks. 1.21.1 NeoForge release exists.

Create: Copycats+ â€” additional copycat building pieces. 1.21.1 NeoForge support exists.

Create Deco â€” industrial/urban decoration, shipping containers, metal sets, windows, catwalks and signage-like decals. 1.21.1 NeoForge support exists.

Create: Design n' Decor â€” additional Create decoration/QoL blocks. 1.21.1 NeoForge releases exist.

Create: Power Loader â€” Create-themed chunk loaders, including train support. 1.21.1 is explicitly supported.

Steam 'n' Rails Neoforge â€” unofficial 1.21.1 NeoForge port. Current research shows release 0.2.1, client and server, with Create 6 compatibility fixes.

Create: Numismatics â€” functional Create-styled currency, vendors and shop-oriented systems. 1.21.1 NeoForge release exists.

Create: Numismatics Utils â€” bank meter and portable bank terminal utilities for Numismatics. 1.21.1 NeoForge release exists.

Ender Mail Reborn â€” packages, coordinates/named lockers, delivery by Ender Mailman. 1.21.1 NeoForge, client and server.

Refined Storage â€” network storage and automation. 1.21.1 NeoForge is supported, client and server.

Create Ore Excavation â€” infinite hidden ore veins/resources powered by Create rotational force. 1.21.1 NeoForge release exists.

Farmer's Delight â€” farming/cooking system with kitchens and many real food preparation chains. 1.21.1 NeoForge release exists.

Create: Food â€” expands food content around Create/Farmer's Delight; 1.21.1 NeoForge release exists, including a current release line.

Handcrafted â€” 250+ furniture pieces, desks, tables, seating and interiors. 1.21.1 NeoForge release exists.

Supplementaries â€” functional Vanilla+ blocks including signposts, faucets, lights, storage and automation. 1.21.1 NeoForge support exists.

Macaw's Doors â€” modern/garage/metal doors and wood variants. 1.21.1 NeoForge support exists.

Macaw's Windows â€” windows, blinds, shutters, curtains and one-way glass. 1.21.1 NeoForge support exists.

Macaw's Lights & Lamps â€” street lamps, ceiling lights, wall lamps and other practical lighting. 1.21.1 NeoForge support exists.

KubeJS â€” custom recipes/items/world/server scripting. 1.21.1 NeoForge support exists.

EMI â€” item/recipe viewer with a 1.21.1 NeoForge release and server compatibility fixes.

Jade â€” information overlay with 1.21.1 NeoForge releases; client and server.

ModernFix â€” performance/memory fixes with 1.21.1 NeoForge support.

## Experimental layers

Create Factory Logistics â€” fluid logistics jars/bottlers for Create 6; 1.21.1 NeoForge, but explicitly early access.

PneumaticCraft: Repressurized â€” separate compressed-air automation/power system. 1.21.1 NeoForge exists, but it is intentionally not part of the first core boot because it overlaps the main industrial stack.

Chemica â€” Create + TFMG chemistry and industrial material chain. 1.21.1 NeoForge exists, but the project describes itself as early in development.

CC: Tweaked â€” useful for future computer-controlled city systems, but it is kept optional until the core pack is stable.

Vehicles â€” both Automobility and Ultimate Car Mod have 1.21.1 NeoForge support, but both describe the 1.21.x vehicle branch as beta/alpha. Vehicle testing is therefore opt-in and kept out of the mandatory core.

## Non-core decision

Mekanism is deliberately not in the first build. It is a very large second technology stack and would duplicate power/material-processing roles that the initial Create/New Age/TFMG design already covers. It can be tested later if a concrete city requirement needs it.

## Engineering target

The modpack supplies systems and components; the actual modern-city simulation will be built with Create automation, KubeJS, redstone and later computer logic. That is how we will implement city addresses, utility grids, banking workflows, mail sorting, shops, jobs, traffic control and similar systems without requiring one monolithic "city simulator" mod.
