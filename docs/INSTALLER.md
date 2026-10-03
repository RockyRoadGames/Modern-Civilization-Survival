# Modern Civilization Installer

This repository now includes a native self-contained Windows installer.

## Why an EXE

The installer does not require PowerShell execution policy changes and does not depend on a globally installed .NET runtime. GitHub Actions builds the release on a Windows runner.

The build pipeline performs:
1. C# compile
2. Local JSON/environment self-tests
3. Live Modrinth resolution against Minecraft 1.21.1 + NeoForge
4. Windows self-contained single-file publish
5. Artifact upload

The installer downloads and checksum-validates every JAR into a staging area first. It only promotes the files into the final server/client folders after the whole download set has passed validation.

## Usage

Core test install:

Modern-Civilization-Installer.exe

Validation only:

Modern-Civilization-Installer.exe validate

Experimental:

Modern-Civilization-Installer.exe --experimental

Custom locations:

Modern-Civilization-Installer.exe --server-mods "C:\MinecraftServer\Modern Civilization\Server\mods" --client-mods "C:\Users\...\Modern Civilization\mods"

The installer never touches the Secret-Facility or Redstone Learning projects.
