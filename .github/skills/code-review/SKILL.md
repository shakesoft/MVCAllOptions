---
name: code-review
description: "ABP Framework code review for this MVCAllOptions project. Use when: reviewing unstaged changes, staged changes, commits on a branch, or a named diff; checking code against ABP best practices, DDD patterns, layer dependency rules, EF Core conventions, MVC/Razor Pages UI patterns, permission/authorization patterns, multi-tenancy rules, and testing patterns defined in .cursor/rules/; validating that entities, repositories, application services, DTOs, mappers, EF Core configuration, and UI page models all follow the project's architecture. Triggers: 'review my changes', 'review this branch', 'code review', 'check my code', 'review staged', 'review commits', 'ABP review'."
argument-hint: "Specify the scope: 'unstaged', 'staged', 'branch', 'last N commits', or paste a diff."
---

# Code Review — MVCAllOptions (ABP Framework, Layered DDD)

## Purpose

This skill performs a structured, rule-driven code review of changes in the `MVCAllOptions` repository. It checks conformance to:
- **Copilot instructions**: `.github/copilot-instructions.md`
- **All `.mdc` rule files** under `.cursor/rules/` (canonical source of truth — takes precedence when conflicting with the instructions file)

---

## When to Use

- "Review my unstaged / staged / committed changes"
- "Review the changes on this branch vs `main`"
- "Check that my new feature follows ABP conventions"
- "Make sure I haven't violated any DDD or layer dependency rules"

---

## Step-by-Step Procedure

### Step 1 — Determine Scope

Ask the user (or infer from their message) which diff to review:

| Scope | Command to run |
|-------|---------------|
| Unstaged changes | `git diff` |
| Staged changes | `git diff --cached` |
| Current branch vs default (main) | `git diff main...HEAD` |
| Last N commits | `git diff HEAD~N HEAD` |
| Specific commit | `git show <sha>` |
| Specific file(s) | add `-- path/to/file` to any command above |

Run the appropriate command to collect the diff.

### Step 2 — Load Rule Sources

Read every rule file before analysing — they are the authoritative checklist:

```
.github/copilot-instructions.md
.cursor/rules/template/app.mdc
.cursor/rules/framework/common/abp-core.mdc
.cursor/rules/framework/common/application-layer.mdc
.cursor/rules/framework/common/authorization.mdc
.cursor/rules/framework/common/ddd-patterns.mdc
.cursor/rules/framework/common/dependency-rules.mdc
.cursor/rules/framework/common/development-flow.mdc
.cursor/rules/framework/common/infrastructure.mdc
.cursor/rules/framework/common/multi-tenancy.mdc
.cursor/rules/framework/data/ef-core.mdc
.cursor/rules/framework/testing/patterns.mdc
.cursor/rules/framework/ui/mvc.mdc
```

> `.mdc` files take precedence over `copilot-instructions.md` when they conflict.

### Step 3 — Analyse the Diff

Work through the checklist in `./references/review-checklist.md`.

For each changed file, identify which layers/categories it belongs to and apply the relevant sections of the checklist. Flag every violation and every area of concern, rated by severity:

| Severity | Meaning |
|----------|---------|
| 🔴 **CRITICAL** | Violates a hard rule — must be fixed (e.g. wrong layer dependency, `DateTime.Now`, public entity setter, business logic in controller) |
| 🟠 **MAJOR** | Significant deviation from ABP/DDD conventions — strongly recommended to fix |
| 🟡 **MINOR** | Style, naming, or optional best-practice gap — worth addressing |
| 🟢 **GOOD** | Notable correct patterns worth calling out |

### Step 4 — Produce a Structured Review Report

Output the review in this structure:

```
## Code Review Report
**Scope**: <what was reviewed>
**Files changed**: N
**Total findings**: X critical, Y major, Z minor

---

### <File or Feature Name>

#### 🔴 CRITICAL
- [<Rule>] Description of violation + line reference + suggested fix

#### 🟠 MAJOR
- ...

#### 🟡 MINOR
- ...

#### 🟢 GOOD
- ...

---

### Summary & Next Steps
Short paragraph summarising the overall quality and the ordered list of things to fix.
```

### Step 5 — Offer to Fix

After delivering the report, ask:
> "Would you like me to fix any of these findings? If so, which ones?"

Apply fixes only for items the user confirms, following the same rule files.

---

## Scope Decision Matrix

```
User says "unstaged"      → git diff
User says "staged"        → git diff --cached
User says "branch"        → git diff main...HEAD   (or ask for base branch)
User says "last N commits" → git diff HEAD~N HEAD
User says "commit <sha>"  → git show <sha>
User says "compare to PR" → git diff origin/main...HEAD
No scope specified        → ask user, default to "all local changes" (staged + unstaged)
```

---

## Key Anti-Patterns to Catch (Quick Reference)

| Pattern | Rule File | Severity |
|---------|-----------|----------|
| `DateTime.Now` / `DateTime.UtcNow` | `abp-core.mdc` | 🔴 |
| `DbContext` injected in Application Services | `dependency-rules.mdc` | 🔴 |
| Public setters on entities | `ddd-patterns.mdc` | 🔴 |
| No parameterless protected constructor on entity | `ddd-patterns.mdc` | 🔴 |
| GUID generated inside entity constructor | `ddd-patterns.mdc` | 🔴 |
| Referencing other aggregates by navigation (not ID) | `ddd-patterns.mdc` | 🔴 |
| Repository for non-aggregate-root entity | `ddd-patterns.mdc` | 🔴 |
| `AddScoped/AddTransient/AddSingleton` manually | `abp-core.mdc` | 🔴 |
| Minimal APIs | `abp-core.mdc` | 🔴 |
| MediatR usage | `abp-core.mdc` | 🔴 |
| Hardcoded role checks | `authorization.mdc` | 🔴 |
| Business logic in Controllers/PageModels | `application-layer.mdc` | 🔴 |
| `includeAllEntities: true` in `AddDefaultRepositories` | `ef-core.mdc` | 🟠 |
| Missing `b.ConfigureByConvention()` in EF config | `ef-core.mdc` | 🟠 |
| Entity exposed in App Service (not DTO) | `application-layer.mdc` | 🟠 |
| ID inside UpdateDto | `application-layer.mdc` | 🟡 |
| Async method without `Async` suffix | `abp-core.mdc` | 🟡 |
| `IFormFile` / `Stream` in App Service | `application-layer.mdc` | 🟡 |
| Missing permission attribute on App Service method | `authorization.mdc` | 🟠 |
| `TenantId` manually filtered in queries | `multi-tenancy.mdc` | 🔴 |
| Mock-based unit tests instead of integration tests | `patterns.mdc` | 🟠 |
| Test name not following `Should_X_When_Y` | `patterns.mdc` | 🟡 |
| Missing localization — hardcoded strings in UI | `mvc.mdc` | 🟡 |

---

## References

- [Full Review Checklist](./references/review-checklist.md)
- [ABP Docs](https://abp.io/docs/latest)
- [Layered Template Docs](https://abp.io/docs/latest/solution-templates/layered-web-application)
- [DDD Guide](https://abp.io/docs/latest/framework/architecture/domain-driven-design)
