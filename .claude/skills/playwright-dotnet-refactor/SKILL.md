---
name: playwright-dotnet-refactor
description: Refactor generated Playwright .NET test drafts into readable xUnit tests with role-based locators, intent-based assertions, and minimal helper extraction. Use when a generated Playwright test is noisy, brittle, or hard to maintain and needs cleanup without changing user intent.
metadata:
  author: nimblepros
  version: "1.0"
  spec: agentskills.io
---

# Playwright .NET Refactor Skill

## Purpose

Turn generated Playwright .NET test code into maintainable xUnit tests while preserving the original user journey and intent.

## Use This Skill When

- You have generated Playwright C# code from codegen and want production-quality tests.
- Selectors are brittle (CSS-heavy, position-dependent, implementation-coupled).
- Assertions are missing, weak, or coupled to internals instead of user-visible outcomes.
- Test flow is hard to read and would benefit from light structure.

## Inputs To Gather Before Refactor

1. Raw generated Playwright C# test draft.
2. Intended user scenario in one sentence.
3. Target location for test (file/class name).
4. Existing fixture and conventions in the repo (for this repo, prefer xUnit and BrowserFixture patterns).
5. Any known app constraints (auth state, seed data, environment, flaky selectors).

## Refactor Rules

1. Preserve user intent.

- Keep the same user-visible workflow and expected behavior.
- Do not silently add or remove meaningful scenario steps.

2. Prefer role-based locators.

- Prioritize GetByRole, then GetByLabel/GetByPlaceholder, then GetByText, then GetByTestId.
- Avoid brittle CSS/XPath selectors unless no better semantic locator exists.

3. Produce readable xUnit tests.

- Use clear test names describing behavior and expected outcome.
- Keep Arrange/Act/Assert intent obvious through structure and naming.
- Minimize incidental complexity in the main test body.

4. Extract helpers only when readability improves.

- Extract repeated or noisy interaction sequences.
- Keep helpers small and intention-revealing.
- Do not introduce abstractions that hide simple steps.

5. Avoid implementation-detail assertions.

- Assert only user-observable behavior (visible text, accessible state, navigation outcome, enabled/disabled controls, etc.).
- Do not assert internal IDs, hidden DOM structure, CSS class internals, or framework-specific plumbing unless user-visible behavior depends on them.

6. Keep tests deterministic.

- Prefer Playwright assertions and explicit waits tied to user-visible conditions.
- Avoid arbitrary sleeps.

## Output Contract

Return:

1. Refactored C# xUnit test code.
2. Optional helper/page object code only if it materially improves readability.
3. Short rationale list covering:

- Locator improvements made.
- Assertion improvements made.
- Helper extraction decisions.
- Any uncertainty or assumptions.

## Quality Gate Checklist

- User intent preserved.
- Locators are semantic and stable.
- Assertions are user-observable.
- Test reads clearly top-to-bottom.
- No unnecessary abstraction introduced.
- No arbitrary waits or timing hacks.

## Prompt Starter

Use [assets/refactor-prompt-template.md](assets/refactor-prompt-template.md) as the reusable Level 2 prompt.
