# Contributing to dotnet-azure-starter

Thank you for your interest in contributing. This guide covers everything you need to open a clean pull request.

---

## Getting started

Clone the repo and follow the setup instructions in [README.md](README.md#getting-started). The quickest path is:

```bash
git clone https://github.com/ibuenuel/dotnet-azure-starter.git
cd dotnet-azure-starter
cp .env.example .env
docker compose up --build
```

---

## Branch naming

| Type | Pattern | Example |
|---|---|---|
| New feature | `feat/<short-description>` | `feat/auth-middleware` |
| Bug fix | `fix/<short-description>` | `fix/pagination-off-by-one` |
| Chore / docs | `chore/<short-description>` | `chore/update-readme` |

Branch off `main`. PRs target `main`.

---

## Commit messages

Use the imperative mood and keep the subject line under 72 characters:

```
Add FluentValidation for CreateTodoDto
Fix pagination returning duplicate items on last page
Update CONTRIBUTING.md with branch naming conventions
```

No ticket numbers in subjects. Put context in the PR description.

---

## Before opening a PR

**1. Build must be warning-free:**

```bash
dotnet build
```

All projects have `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`. A missing XML doc comment on a public API is a build failure.

**2. All 72 tests must pass:**

```bash
dotnet test
```

| Suite | Count | Requires Docker |
|---|---|---|
| Unit tests — `Core.Tests` | 57 | No |
| Architecture tests — `Core.Tests` | 5 | No |
| Integration tests — `Api.Tests` | 10 | Yes |

Integration tests spin up a dedicated SQL Server container via Testcontainers — Docker running is sufficient.

**3. No new vulnerable packages:**

```bash
dotnet list package --vulnerable
```

---

## Code style

Enforced by `.editorconfig` — no manual formatting needed. Key rules:

- 4-space indent (no tabs)
- Allman braces
- `var` when type is obvious from the right-hand side
- Private fields: `_camelCase` prefix
- Interfaces: `I` prefix
- Async methods: `Async` suffix
- One class per file; file name matches class name
- No `#region`

---

## Architecture rules

Layer dependency rules are enforced by architecture tests (NetArchTest). The forbidden directions are:

```
Core → Infrastructure   ❌
Core → Api              ❌
Infrastructure → Api    ❌
```

A misplaced `using` will be caught by `dotnet test` — not by a code review comment.

---

## PR guidelines

- One concern per PR. A PR that adds a feature and refactors unrelated code is harder to review and harder to revert.
- Keep PRs small. If a change spans many files, split it into logical steps.
- Include a brief description of *why* the change is needed, not just what it does.
- Link to an issue if one exists.

---

## Replacing the Todo domain

This boilerplate uses Todo items as a **reference implementation**. If you're adapting it for a real domain, follow the [template guide in README.md](README.md#using-this-as-a-template) — the migration takes about 30 minutes.

---

*Author: Ismail Bünül — Senior Software Engineer & Deputy Head of IT*
