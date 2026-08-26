# Game Implementation Plan

## Target engine

Unity + C# is the selected implementation target for the Android-first mobile RPG described by the master specification.

## Phase 0 — Foundation

- Establish Unity project structure
- Define game state machine contracts
- Define run state contracts
- Define centralized RNG contracts
- Define dice contracts
- Define board/node contracts
- Keep gameplay logic separate from presentation
- Prepare test/debug boundaries

## Phase 1 — Playable Core

Implement and verify:

1. Start run
2. Choose a starter team
3. Enter board
4. Roll a six-sided die
5. Move through real board nodes
6. Resolve normal and enemy tiles
7. Resolve a basic encounter
8. Produce a real win/lose state

## Rules

- No fake buttons or fake gameplay
- No duplicated RNG logic
- No hidden dice manipulation
- No placeholder feature presented as complete
- Keep configuration/data separate from gameplay code
- Validate state transitions
- Test every completed phase before expansion

## Immediate implementation slice

The first code slice should compile as a Unity project and provide a deterministic, testable domain layer for the board/dice loop. Rendering and UI are layered on top of the domain rather than becoming the source of truth.
