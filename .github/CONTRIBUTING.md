# Contributing to [Nome do Projeto]

Thank you for investing your time in contributing to our project! ✨

This document outlines a set of guidelines and best practices for contributing. Following these rules helps us maintain a clean, organized, and high-performance repository.

---

## 🗺️ Table of Contents

1. [Code of Conduct](#-code-of-conduct)
2. [How Can I Contribute?](#-how-can-i-contribute)
   - [Reporting Bugs](#reporting-bugs)
   - [Suggesting Enhancements](#suggesting-enhancements)
   - [Pull Requests](#pull-requests)
3. [Branch Naming Conventions](#%EF%B8%8F-branch-naming-conventions)
4. [Style Guides & Commit Messages](#-style-guides--commit-messages)

---

## 📜 Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct. Please be respectful, constructive, and welcoming to all contributors.

---

## 🤝 How Can I Contribute?

### Reporting Bugs
If you find a bug, please open an **Issue** and include:
* A clear and descriptive title.
* Steps to reproduce the behavior.
* Your environment details (OS version, Java/Git version, Minecraft/Forge version if applicable).
* Expected vs. actual behavior.
* Logs or screenshots (if any).

### Suggesting Enhancements
Enhancement suggestions are tracked as Issues. When creating one, please:
* Explain the main goal of the feature.
* Provide a step-by-step description of how it should work.
* Explain why this enhancement would be useful to most users.

### Pull Requests
1. Fork the repository and create your branch from the correct base branch.
2. Ensure your code follows the project's style and all tests pass.
3. Open a Pull Request targeting the appropriate branch (never target `main` for source code changes).
4. Describe your changes clearly in the PR description.

---

## 🌿 Branch Naming Conventions

To keep our repository history clean and predictable, we use a strict semantic hierarchy. All branch names must follow these patterns:

| Branch Type            | Naming Pattern                                    | Example                                     |
| :--------------------- | :------------------------------------------------ | :------------------------------------------ |
| **Production/Landing** | `main`                                            | `main`                                      |
| **Source Environment** | `source-<mc-version>-<loader>`                    | `source-1.20.1-forge`                       |
| **Bug Fixes**          | `source-<version>-<loader>/fix-<description>`     | `source-1.20.1-forge/fix-hud-alignment`     |
| **Features**           | `source-<version>-<loader>/feature-<description>` | `source-1.20.1-forge/feature-sound-effects` |

> ⚠️ **Important:** Branches that do not follow this convention will be blocked or closed.

---

## 💬 Style Guides & Commit Messages

We prefer short, descriptive, and imperative commit messages (e.g., `Add visual hitmarkers`, not `Fixed some bugs`).

* **Code Style:** Keep your code clean, well-indented, and comment whenever you write a complex logic.
* **UI/UX:** For visual tweaks, ensure elements are perfectly aligned and text strings fit appropriately within buttons or text fields.

---

## ❓ Need Help?

If you have questions or need assistance, feel free to open a discussion or contact the maintainers at [Seu Link de Contato/GitHub Profile].