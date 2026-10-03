#Requires -Version 5.1
<#+
Modern Civilization Survival
01 - Initialize/update the project and push it to GitHub.

Default local project:
  C:\MinecraftServer\Modern Civilization\Project

Default repository:
  RockyRoadGames/Modern-Civilization-Survival

This script never commits .jar files. It is safe to run repeatedly.
#>
[CmdletBinding()]
param(
    [string]$ProjectRoot = "C:\MinecraftServer\Modern Civilization\Project",
    [string]$RepoOwner = "RockyRoadGames",
    [string]$RepoName = "Modern-Civilization-Survival",
    [ValidateSet("public","private")]
    [string]$Visibility = "public"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found. Install Git and GitHub CLI (gh), then rerun this script."
    }
}

function Section([string]$Message) {
    Write-Host ""
    Write-Host "==== $Message ====" -ForegroundColor Cyan
}

Require-Command "git"
Require-Command "gh"

New-Item -ItemType Directory -Force -Path $ProjectRoot | Out-Null
foreach ($d in @("modpack","docs","reports","scripts")) {
    New-Item -ItemType Directory -Force -Path (Join-Path $ProjectRoot $d) | Out-Null
}
Set-Location $ProjectRoot

Section "Checking GitHub authentication"
& gh auth status
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run 'gh auth login' once, then rerun this script."
}

Section "Writing project metadata"

@'
# Generated Minecraft files / runtime folders
Server/world/
Server/logs/
Server/crash-reports/
Server/config/
Server/defaultconfigs/
Server/versions/
Server/libraries/
Server/neoforge-*-installer.jar
Server/*.log

# Client runtime data
Client/logs/
Client/crash-reports/
Client/config/
Client/versions/

# Never commit binary mod distributions
**/*.jar
**/*.mrpack
**/*.zip

# OS/editor
Thumbs.db
Desktop.ini
.vscode/
.idea/
'@ | Set-Content -Path (Join-Path $ProjectRoot ".gitignore") -Encoding UTF8

$readme = @'
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
'@
$readme | Set-Content -Path (Join-Path $ProjectRoot "README.md") -Encoding UTF8

$research = @'
# Research Notes — Modern Civilization Survival
# Verified from current Modrinth/NeoForged pages on 2026-10-03

## Foundation decision

Minecraft 1.21.1 + NeoForge is the selected test foundation. NeoForged's documentation requires Java 21 for Minecraft 1.20.5 and newer, and the 1.21.1 ecosystem remains large and widely supported. The official Create page shows Create 6.0.10 for Minecraft 1.21.1 on NeoForge, client and server.

## Core systems researched

Create — mechanical automation and logistics base.

Create: New Age — electrical generation, motors, wires, heat systems and optional reactor/solar systems. 1.21.1 NeoForge release exists.

Create: Crafts & Additions — Create/electricity bridge. 1.21.1 NeoForge release exists.

Create: Diesel Generators — diesel engines, industrial features and crude-oil refinery. 1.21.1 NeoForge release exists.

Create: The Factory Must Grow — heavy engineering/oil for Create. 1.21.1 NeoForge releases exist, including release builds later than the initial port.

Create: Connected — Create QoL blocks. 1.21.1 NeoForge release exists.

Create: Copycats+ — additional copycat building pieces. 1.21.1 NeoForge support exists.

Create Deco — industrial/urban decoration, shipping containers, metal sets, windows, catwalks and signage-like decals. 1.21.1 NeoForge support exists.

Create: Design n' Decor — additional Create decoration/QoL blocks. 1.21.1 NeoForge releases exist.

Create: Power Loader — Create-themed chunk loaders, including train support. 1.21.1 is explicitly supported.

Steam 'n' Rails Neoforge — unofficial 1.21.1 NeoForge port. Current research shows release 0.2.1, client and server, with Create 6 compatibility fixes.

Create: Numismatics — functional Create-styled currency, vendors and shop-oriented systems. 1.21.1 NeoForge release exists.

Create: Numismatics Utils — bank meter and portable bank terminal utilities for Numismatics. 1.21.1 NeoForge release exists.

Ender Mail Reborn — packages, coordinates/named lockers, delivery by Ender Mailman. 1.21.1 NeoForge, client and server.

Refined Storage — network storage and automation. 1.21.1 NeoForge is supported, client and server.

Create Ore Excavation — infinite hidden ore veins/resources powered by Create rotational force. 1.21.1 NeoForge release exists.

Farmer's Delight — farming/cooking system with kitchens and many real food preparation chains. 1.21.1 NeoForge release exists.

Create: Food — expands food content around Create/Farmer's Delight; 1.21.1 NeoForge release exists, including a current release line.

Handcrafted — 250+ furniture pieces, desks, tables, seating and interiors. 1.21.1 NeoForge release exists.

Supplementaries — functional Vanilla+ blocks including signposts, faucets, lights, storage and automation. 1.21.1 NeoForge support exists.

Macaw's Doors — modern/garage/metal doors and wood variants. 1.21.1 NeoForge support exists.

Macaw's Windows — windows, blinds, shutters, curtains and one-way glass. 1.21.1 NeoForge support exists.

Macaw's Lights & Lamps — street lamps, ceiling lights, wall lamps and other practical lighting. 1.21.1 NeoForge support exists.

KubeJS — custom recipes/items/world/server scripting. 1.21.1 NeoForge support exists.

EMI — item/recipe viewer with a 1.21.1 NeoForge release and server compatibility fixes.

Jade — information overlay with 1.21.1 NeoForge releases; client and server.

ModernFix — performance/memory fixes with 1.21.1 NeoForge support.

## Experimental layers

Create Factory Logistics — fluid logistics jars/bottlers for Create 6; 1.21.1 NeoForge, but explicitly early access.

PneumaticCraft: Repressurized — separate compressed-air automation/power system. 1.21.1 NeoForge exists, but it is intentionally not part of the first core boot because it overlaps the main industrial stack.

Chemica — Create + TFMG chemistry and industrial material chain. 1.21.1 NeoForge exists, but the project describes itself as early in development.

CC: Tweaked — useful for future computer-controlled city systems, but it is kept optional until the core pack is stable.

Vehicles — both Automobility and Ultimate Car Mod have 1.21.1 NeoForge support, but both describe the 1.21.x vehicle branch as beta/alpha. Vehicle testing is therefore opt-in and kept out of the mandatory core.

## Non-core decision

Mekanism is deliberately not in the first build. It is a very large second technology stack and would duplicate power/material-processing roles that the initial Create/New Age/TFMG design already covers. It can be tested later if a concrete city requirement needs it.

## Engineering target

The modpack supplies systems and components; the actual modern-city simulation will be built with Create automation, KubeJS, redstone and later computer logic. That is how we will implement city addresses, utility grids, banking workflows, mail sorting, shops, jobs, traffic control and similar systems without requiring one monolithic "city simulator" mod.
'@
$research | Set-Content -Path (Join-Path $ProjectRoot "docs\RESEARCH.md") -Encoding UTF8

Section "Writing curated mod manifest"
$manifest = [ordered]@{
    project = "Modern Civilization Survival"
    minecraft = "1.21.1"
    loader = "neoforge"
    java = "21"
    versionPolicy = "Latest Modrinth release matching Minecraft + loader, then recursively resolve required Modrinth dependencies. Exact results are locked."
    core = @(
        "create",
        "create-new-age",
        "createaddition",
        "create-diesel-generators",
        "create-tfmg",
        "create-connected",
        "copycats",
        "create-deco",
        "create-design-n-decor",
        "create-power-loader",
        "create-steam-n-rails-1.21.1",
        "numismatics",
        "create-numismatics-utils",
        "ender-mail-reborn",
        "refined-storage",
        "create-ore-excavation",
        "farmers-delight",
        "create-food",
        "handcrafted",
        "supplementaries",
        "macaws-doors",
        "macaws-windows",
        "macaws-lights-and-lamps",
        "kubejs",
        "emi",
        "jade",
        "modernfix"
    )
    experimental = @(
        "create_factory_logistics",
        "pneumaticcraft-repressurized",
        "chemica",
        "cc-tweaked"
    )
    vehicles = [ordered]@{
        automobility = "automobility"
        ultimateCarMod = "ultimate-car-mod"
    }
    clientOnly = @(
        "embeddium",
        "immediatelyfast",
        "entityculling",
        "dynamic-fps",
        "xaeros-minimap",
        "xaeros-world-map"
    )
}
$manifest | ConvertTo-Json -Depth 20 | Set-Content -Path (Join-Path $ProjectRoot "modpack\manifest.json") -Encoding UTF8

# Copy the installer scripts into the repo when they have been downloaded next to this script.
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
foreach ($name in @("01-Save-Modern-Civilization-To-GitHub.ps1","02-Install-Modern-Civilization-Test.ps1")) {
    $source = Join-Path $scriptDirectory $name
    if (Test-Path $source) {
        Copy-Item -LiteralPath $source -Destination (Join-Path $ProjectRoot "scripts\$name") -Force
    }
}

Section "Initializing Git"
if (-not (Test-Path (Join-Path $ProjectRoot ".git"))) {
    & git init | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "git init failed." }
}
& git branch -M main | Out-Host
& git config core.autocrlf true | Out-Host

$repoFullName = "$RepoOwner/$RepoName"
$repoUrl = "https://github.com/$repoFullName.git"

Section "Ensuring GitHub repository exists"
& gh repo view $repoFullName --json name,url | Out-Null
$repoExists = ($LASTEXITCODE -eq 0)

if (-not $repoExists) {
    Write-Host "Creating GitHub repository $repoFullName..." -ForegroundColor Yellow
    $visibilitySwitch = if ($Visibility -eq "public") { "--public" } else { "--private" }
    & gh repo create $repoFullName $visibilitySwitch --description "Modern Civilization Survival - functional multiplayer modern-city Minecraft 1.21.1 NeoForge project" | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "GitHub repository creation failed." }
}

$origin = (& git remote get-url origin 2>$null)
$hasOrigin = ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace(($origin -as [string])))
if (-not $hasOrigin) {
    & git remote add origin $repoUrl | Out-Host
} elseif ($origin -ne $repoUrl) {
    & git remote set-url origin $repoUrl | Out-Host
}

Section "Committing local project"
& git add -A | Out-Host
& git diff --cached --quiet
$hasStagedChanges = ($LASTEXITCODE -ne 0)
if ($hasStagedChanges) {
    $message = "Update Modern Civilization project $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    & git commit -m $message | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "git commit failed." }
} else {
    Write-Host "No new local changes to commit." -ForegroundColor DarkGray
}

Section "Syncing with GitHub"
& git fetch origin main | Out-Host
if ($LASTEXITCODE -ne 0) { throw "git fetch failed." }

$remoteRef = (& git rev-parse --verify refs/remotes/origin/main 2>$null)
if ($LASTEXITCODE -eq 0 -and $remoteRef) {
    $localHead = (& git rev-parse HEAD)
    if ($localHead -ne $remoteRef) {
        & git pull --rebase origin main | Out-Host
        if ($LASTEXITCODE -ne 0) {
            throw "GitHub has changes that could not be rebased automatically. Resolve the Git conflict, then rerun this script."
        }
    }
}

& git push -u origin main | Out-Host
if ($LASTEXITCODE -ne 0) { throw "git push failed." }

Section "Saved"
Write-Host "Local project : $ProjectRoot" -ForegroundColor Green
Write-Host "GitHub repo   : https://github.com/$repoFullName" -ForegroundColor Green
Write-Host "Manifest      : $(Join-Path $ProjectRoot 'modpack\manifest.json')" -ForegroundColor Green
Write-Host ""
Write-Host "Run Script 02 next. After it resolves/installs the exact mods, run this script again to push the lockfile/report." -ForegroundColor Cyan
