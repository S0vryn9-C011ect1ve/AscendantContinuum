# Ascendant Continuum AI Constitution

## Purpose
This constitution defines how the coding agent must operate in this repository.

## Core Principles
1. **Ask Before Creating New Systems**
   - Do not introduce a new architecture, subsystem, framework, pipeline, or service without explicit approval.
   - Prefer extending existing systems over inventing parallel ones.

2. **Single Source of Truth (SSOT)**
   - Every concept, config, and rule should have one canonical home.
   - Avoid duplicate logic and conflicting definitions across files.

3. **Connect, Don’t Create**
   - Integrate with existing modules, assets, and patterns before creating new components.
   - Reuse and compose what already exists.

4. **Run Tests Before Committing**
   - No commit-ready change is complete until relevant tests pass.
   - Prefer targeted tests first, then broader suites as needed.

## TDD Is Mandatory
Test-Driven Development is the default implementation method.

### TDD Loop (Red → Green → Refactor)
1. **Red**: Write or update a failing test first.
2. **Green**: Write the minimum production code needed to make that test pass.
3. **Refactor**: Improve design while keeping all tests green.
4. Repeat in small increments.

## Working Rules
- Start each non-trivial change by identifying the test to write first.
- If no test exists, create one before implementation.
- Keep tests deterministic, fast, and focused on behavior.
- Do not broaden scope beyond the failing test requirement.
- When blocked, ask for clarification rather than creating a speculative system.

## Definition of Done
A task is done only when:
- Requested behavior is implemented.
- New/updated tests were written first and now pass.
- Existing relevant tests pass.
- No duplicate source of truth was introduced.
- No unauthorized new system was created.
