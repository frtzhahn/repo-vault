<div align="center">

<img src="https://github.com/user-attachments/assets/55d7af5b-1ab6-4d63-b154-b57562bfb740" />

<sub style="font-weight: bold;"> Local Git archive server and academic repository catalog for CSD students from UCC built with C# WPF (.NET 8 LTS, Gruvbox Theme) and MySQL/MariaDB (XAMPP) </sub>
</div>
<br><br>

## Overview

Repo Vault provides a local area Git HTTP server and desktop workstation for archiving, inspecting, and grading student programming repositories without relying on cloud hosting. Students can register accounts, push and clone repositories over LAN using standard Git commands, browse source trees, and leave peer review feedback. Faculty members can manage course catalogs, inspect code metrics (source, comment, and blank lines), lock repositories after deadlines, soft-delete archives via the Repo Bin, and record rubric evaluations.

## Prerequisites

Before cloning and building the project, make sure the following tools are installed on your machine:

1. **.NET 8 LTS SDK** (`8.0` or newer) - verify with `dotnet --version` or `dotnet --info`
2. **XAMPP** (with MySQL/MariaDB service running on port `3306`)
3. **Git CLI** (`git.exe` on Windows or `git` on Linux) added to your system PATH
4. **Visual Studio 2022** (with .NET Desktop Development workload, open `RepoVault.sln`) or **Neovim / VS Code**

## Project Structure (monorepo)

- `src/RepoVault.Core` - Database access (MariaDB), RBAC authentication, Git HTTP server (`HttpListener`), and static code metrics scanner
- `src/RepoVault.Desktop` - Native WPF desktop client (`.NET 8`, Gruvbox Light/Dark dynamic theme, Version B Classroom UX)
- `tests/RepoVault.Tests` - Unit tests for database queries, line counting, and server handlers.
- `database/schema.sql` - SQL script to initialize the `repo_vault_db` schema in phpMyAdmin or MariaDB CLI.

## Notes and Roles

> [!IMPORTANT]
> Update your row in your pull request when starting or finishing a task/submitting PR

```mermaid
kanban
  [Todo]
    [mock ups]@{ assigned: 'TBD' }
  [Opened Issues]
    [ui: winforms screen mockups]@{ assigned: 'TBD', ticket: '#22' }
    [ui: desktop session and xampp health check]@{ assigned: 'Aldrin', ticket: '#23' }
    [ui: team and repo editor dialogs]@{ assigned: 'TBD', ticket: '#24' }
    [ui: login, register, student explorer]@{ assigned: 'TBD', ticket: '#12' }
    [ui: faculty admin, rubric, repo bin]@{ assigned: 'TBD', ticket: '#13' }
    [desktop: core wiring and lan testing]@{ assigned: 'Aldrin', ticket: '#14' }
  [In Progress]
    [Live LAN Smoke Test & Final Demo Prep]@{ assigned: 'Aldrin' }
  [Closed Issues]
    [desktop: winforms net10.0 scaffold]@{ assigned: 'Aldrin', ticket: '#1' }
    [db: mariadb schema and entity models]@{ assigned: 'Aldrin', ticket: '#3' }
    [core: pbkdf2 auth and rbac service]@{ assigned: 'Aldrin', ticket: '#5' }
    [core: 6-domain crud and repo bin]@{ assigned: 'Aldrin', ticket: '#7' }
    [git: smart http server and push queue]@{ assigned: 'Aldrin', ticket: '#9' }
    [metrics: static loc and language scanner]@{ assigned: 'Aldrin', ticket: '#10' }
    [metrics: fix trailing newline phantom blank]@{ assigned: 'Aldrin', ticket: '#17' }
    [core: bare repo file tree and blob reader]@{ assigned: 'Aldrin', ticket: '#18' }
    [core: view dtos and joined read queries]@{ assigned: 'Aldrin', ticket: '#19' }
    [git: lan ipv4 resolver and urlacl script]@{ assigned: 'Aldrin', ticket: '#20' }
    [db: academic seed data script]@{ assigned: 'Duites/Alciso', ticket: '#11' }
    [db: bare git repo workspace seeder]@{ assigned: 'Aldrin', ticket: '#21' }
    [core: classroom tasks and access tickets]@{ assigned: 'Aldrin', ticket: '#31' }
    [desktop: student workspace control and coding interface]@{ assigned: 'Aldrin', ticket: '#33' }
    [desktop: faculty workspace, rubric grading, and repo bin]@{ assigned: 'Aldrin', ticket: '#35' }
    [desktop: migrate solution to .net 8 and rebuild ui in wpf with gruvbox theme]@{ assigned: 'Aldrin', ticket: '#37' }
  [Done]
    [Project Skeleton & Desktop Scaffold]@{ assigned: 'Aldrin' }
    [Database Schema & Entity Models]@{ assigned: 'Aldrin' }
    [PBKDF2 Auth & RBAC Security Engine]@{ assigned: 'Aldrin' }
    [6-Domain CRUD Services & Repo Bin Engine]@{ assigned: 'Aldrin' }
    [In-Process Git Smart HTTP Server]@{ assigned: 'Aldrin' }
    [Deterministic Code Metrics Scanner]@{ assigned: 'Aldrin' }
    [Core Read Layer, File Browser & LAN Helper]@{ assigned: 'Aldrin' }
    [Database Seed Data & Demo Workspace Seeder]@{ assigned: 'Aldrin' }
    [WinForms Auth & Main Shell]@{ assigned: 'Aldrin' }
    [Classroom Coding Tasks & Access Tickets]@{ assigned: 'Aldrin' }
    [Student Workspace UI Implementation]@{ assigned: 'Aldrin' }
    [Faculty Admin & Grading UI Implementation]@{ assigned: 'Aldrin' }
    [WPF Gruvbox Workstation & .NET 8 LTS Migration]@{ assigned: 'Aldrin' }
```

## Contribution Guide for Members

Please FOLLOW and READ these steps when picking up a task so we wont overwrite some random files:

### 1. Opening an Issue

> [!NOTE]
> please follow this guide accordingly to keep the project workflow in order thank you in advance

1. Go to the [issue](https://github.com/frtzhahn/repo-vault/issues) tab on GitHub and click **New issue**.
2. Title it with the module name, for example: `[ui] add faculty rubric grading form`.
3. Briefly list what files or forms you plan to add or edit, then assign the issue to yourself or to someone assigned on the specific field.

### 2. Creating a Branch

> [!WARNING]
> Never commit or push directly to `master`. Always pull the latest changes and create a feature branch first:

```bash
git checkout master
git pull origin master
git checkout -b feat/short-task-name
```
