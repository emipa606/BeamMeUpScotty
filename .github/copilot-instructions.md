# GitHub Copilot Instructions for "Beam me up Scotty (Continued)" Mod

## Mod Overview and Purpose
"Beam me up Scotty (Continued)" is a RimWorld mod originating from Gouda quiche's work and continued as part of the "Faster Than Light for RimWorld" series. This mod enhances the RimWorld experience by integrating advanced teleportation technology, allowing players to move pawns, items, and even teleport across maps. It’s particularly compatible with the "Save Our Ship 2" mod, enriching space travel and colony logistics with futuristic teleportation capabilities.

## Key Features and Systems
- **Teleport Workstation & Mini Station**: These are essential structures required to command teleport spots. While the workstation allows for more extensive operations, the mini station offers reduced capacity for smaller-scale tasks.
- **Teleport Spot**: Operates as both a teleport location and a storage unit. It can be set up to teleport items or pawns in various modes: in, out, swap.
- **Teleport Catcher**: A cost-effective wooden structure linked to teleport spots that facilitates item or pawn teleportation.
- **Teleport Bed**: Specialized for teleporting injured or incapacitated characters.
- **Teleport Box**: Simplistic design to ensure no beloved animals (like pets) are left behind.
- **Travel Between Maps**: A mechanic to transport teleport spots via caravans, facilitating inter-map teleportation logistics.

## Coding Patterns and Conventions
- **C# Structure**: The mod employs a structured approach, where components, such as `CompProperties_MatrixOverlay` and `CompProperties_LTF_TpSpot`, are clearly defined with related attributes and methods.
- **Consistent Naming**: Types and members follow a clear naming convention making it easier to understand the purpose and functionality.
- **Modular Design**: The code is structured modularly, allowing for easy updates and maintenance.

## XML Integration
- XML files are used for defining various elements such as `ThingDef` for in-game objects and `ResearchProjectDef` for unlocking features through research.
- XML files define the visual and research components and are critical in linking the teleportation mechanics with the game’s UI and progression systems.

## Harmony Patching
- Harmony is used to create runtime modifications within the game's methods. This allows the mod to override or extend the vanilla game behavior without altering the original game files.
- Standard practices should be followed, such as ensuring all patches apply only when necessary and are thoroughly tested to prevent conflicts with other patches.

## Suggestions for Copilot
1. **Autocomplete Suggestions**: Take advantage of Copilot's ability to suggest repetitive code structures, such as getter methods or repetitive initialization logic within components.
2. **Complex Logic**: Let Copilot assist in drafting complex teleportation logic or conditions by understanding similar structures in existing code.
3. **Error Handling**: Use Copilot to improve exception handling in code by providing suggestions based on community best practices.
4. **XML Definition Autofill**: Copilot can help in auto-filling XML structure based on similar patterns, reducing manual input errors.
5. **Refactoring Assistance**: When improving code structure or performance, use Copilot to suggest refactoring patterns or identify optimization opportunities.

## Known Issues and Recommendations
- **Animal Box Bug**: There's an acknowledged issue with the animal teleport box, with a noted fix pending an update.
- **Testing Best Practices**: To ensure compatibility, recommend isolating this mod and its requirements before integrating with others, followed by incremental mod additions.
- **Community Support**: Encourage users to report any issues through the designated Discord channel for real-time support and avoid using discussion threads for error reporting.

This document serves as an instructive guide for contributors and developers working on or with the "Beam me up Scotty (Continued)" mod, leveraging GitHub Copilot to enhance development efficiency and code quality.

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

