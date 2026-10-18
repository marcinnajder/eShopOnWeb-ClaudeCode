# Level 2 Prompt Template: Refactor Generated Playwright .NET Test

You are refactoring a generated Playwright .NET test into maintainable xUnit test code.

## Non-negotiable requirements

1. Preserve user intent and scenario behavior.
2. Prefer role-based locators (GetByRole first).
3. Extract helpers only when readability improves.
4. Avoid implementation-detail assertions.
5. Keep assertions user-observable and intent-based.

## Repo conventions

- Language: C#
- Test framework: xUnit
- Browser automation: Microsoft.Playwright
- Existing fixture patterns should be reused when appropriate.

## Inputs

Scenario intent:

<replace with one-sentence intent>

Generated test draft:

<replace with generated C# test code>

Optional constraints:

<replace with constraints, if any>

## Required output

1. Refactored xUnit test code (C#).
2. Any helper/page extraction only if it improves readability.
3. A concise rationale:

- Which locators were improved and why.
- Which assertions were changed and why.
- What was intentionally not extracted and why.
- Any assumptions that need human confirmation.

## Additional guidance

- Keep code straightforward for teaching/demo use.
- Prefer explicit, descriptive names over clever abstractions.
- If a semantic locator is unavailable, explain fallback locator choice.
