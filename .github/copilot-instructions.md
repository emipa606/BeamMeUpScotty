# GitHub Copilot Instructions for Beam me up Scotty (Continued)

Welcome to the **Beam me up Scotty (Continued)** mod for RimWorld! This mod is an update to the original **Gouda quiches** mod, introducing advanced teleportation features, alien races, and enhanced gameplay mechanics. Below are the detailed instructions to help you effectively use GitHub Copilot for contributing to this mod.

## Mod Overview and Purpose

The **Beam me up Scotty (Continued)** mod enhances the RimWorld experience by allowing players to travel through space at light speed and teleport pawns, objects, and even wounded individuals. The mod also introduces new alien races and technologies to expand the universe of RimWorld.

## Key Features and Systems

### Teleportation System

- **Teleport Workstation**: A central hub that manages teleportation spots.
- **Mini Station**: A smaller version of the teleport workstation with limited capacity.
- **Teleport Spot**: Locations where pawns or objects can be teleported to or from. It can be configured for automatic teleportation and prioritizing storage.
- **Teleport Catcher**: A wooden building that acts as a storage and teleportation endpoint.
- **Teleport Bed**: Allows for the teleportation of wounded, incapacitated, or captured pawns.
- **Teleport Box**: A specialized teleporter for pets and other small objects.

### Inter-Map Travel

- **Linking Spots**: Connect teleport spots across different maps.
- **Power Management**: Uninstall power-intensive spots during travel and carry them in caravans.

### Alien Races

- **Clone Bay**: Allows for cloning pawns.
- **Mindcontrol**: Ability to control other pawns' minds.
- **Crystal Race**: A new alien race with unique abilities.
- **Engi Race**: Technologically advanced aliens.
- **Lanius Race**: Predatory aliens.
- **Mantis Race**: Insect-like aliens.
- **Rockmen Race**: Rock-dwelling aliens.
- **Zoltan Race**: Psychic aliens.

## Coding Patterns and Conventions

### General Coding Standards

- **Naming Conventions**: Use PascalCase for class names and camelCase for method and variable names. Prefix private fields with an underscore.
- **Code Comments**: Provide meaningful comments for complex logic or important code sections.
- **Code Readability**: Keep the code clean and maintainable, avoiding unnecessary complexity.

### XML Integration

- **Def Files**: XML files define various game entities such as buildings, research projects, and mote effects. Use consistent naming and structure for easier maintenance.
- **Def Loading**: Ensure that all XML definitions are correctly loaded and registered within the mod's initialization process.

### Harmony Patching

- **Harmony Patches**: Use Harmony patches to modify vanilla game behavior. Follow the naming convention `Patch_` followed by the class and method name being patched.
- **Patch Order**: Ensure that patches are applied in the correct order to avoid conflicts with other mods or patches.

## Suggestions for Copilot

### Using Copilot for XML Files

1. **Create New Def Files**: Use Copilot to generate new XML definitions for buildings, research projects, or mote effects. Start with a basic template and let Copilot suggest completions.
2. **Edit Existing Defs**: When modifying existing XML files, use Copilot to suggest changes or additions to ensure consistency with other definitions.

### Using Copilot for C# Files

1. **Implement New Features**: Use Copilot to scaffold new classes and methods for implementing new features. Start with method signatures and let Copilot provide the implementation details.
2. **Refactor Existing Code**: When refactoring existing code, use Copilot to suggest improvements or optimizations. Ensure that the refactored code adheres to the coding conventions.

### Using Copilot for Harmony Patches

1. **Create Patches**: Use Copilot to generate new Harmony patches for modifying vanilla game behavior. Start with the patch class and method names and let Copilot provide the patch logic.
2. **Debug Patches**: When debugging Harmony patches, use Copilot to suggest common troubleshooting steps or alternative approaches.

## Conclusion

The **Beam me up Scotty (Continued)** mod offers an exciting new dimension to RimWorld, with advanced teleportation mechanics and alien races. By following the coding patterns and conventions outlined above, and leveraging GitHub Copilot effectively, you can contribute to the development of this mod and enhance the gameplay experience for all players.

If you encounter any issues or have suggestions, feel free to reach out to the mod developers on the Discord channel or contribute directly to the GitHub repository. Happy coding!

---

**Mod Name:** Beam me up Scotty (Continued)  
**Author:** Gouda quiche  
**PackageId:** Mlie.BeamMeUpScotty  
**Tags:** teleportation, technology

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

