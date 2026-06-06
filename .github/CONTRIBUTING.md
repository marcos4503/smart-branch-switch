This page is designed to guide you and provide guidelines on what to do and how to do it if you wish to Contribute to this repository. But before we continue...

# ☕ Are you a Regular User of this Product?

If you are a Regular User of this Repository's Product and are not interested in Contributing technically, but need help, want to report a bug, ask a question, suggest a new feature, or something similar, go to the `Issues` tab of this Repository. After that, click on `New Issue` to create a topic for your request. You can also `💖 Sponsor` this project to help keep it going!

# 🤝 Do you want to Contribute technically?

If you want to Contribute technically to this Repository, proposing Code Changes and things like that, this is the topic for you! Here you will see the guidelines for Contributions, how to make Contributions, etc.

Just know that every Contribution is appreciated, even the smallest ones. This topic outlines a set of guidelines and best practices for Contributing. Following these guidelines helps us maintain a clean, organized, and high-performance Repository. Thank you for investing your time in Contributing to our project! ✨

## How to Contribute

Technically, you can Contribute to this Repository in the following ways:

- **Testing the Product from this Repository:** You can do this simply by using the Product from this Repository, and as you see fit, execute the next items on this list.
- **Reporting Bugs:** This can be done through the `Issues` tab of this Repository.
- **Suggesting Enhancements or giving your Feedback:** This can be done through the `Issues` tab of this Repository.
- **Proposing Code Changes:** This can be done through `Pull Requests` (this will be discussed in more detail below). It's great if you want to get your hands dirty and work on the Source Code of this Repository, implementing new Features, Improvements, Optimizations, Fixes, and Refactorings yourself.
- **Proposing Documentation/ReadMe Changes:** This can be done through `Pull Requests` (this will be discussed in more detail below). It's great if you've seen some incorrect information in the Documentation/ReadMe, or seen something that could be improved, or removed because it's outdated, and you want to get your hands dirty and do it yourself.

### Roles

Before we continue, it's helpful to differentiate between the different Repository roles, as each role has different responsibilities and permissions:

- **Owner:** Owns the Repository. Has full control and can perform any action.
- **Admin:** Has almost all the permissions of the Owner and can manage nearly everything. Acts as a secondary owner.
- **Maintainer:** Manages the project on a daily basis. Reviews Pull Requests, manages Issues, defines the Roadmap, approves merges and ensures overall project health.
- **Contributor:** Anyone outside the core team who participates by submitting Code Changes, Documentation/ReadMe Changes, suggestions, bug reports, or other Contributions. The Maintainer+ members can also make Contributions, but they have direct write access to the Repository.

### How do I send Pull Requests?

So, you want to get your hands dirty and you want to implement new Features, Improvements, Optimizations, Fixes, Refactorings or updates on Documentation/ReadMe, yourself? Perfect! Here you'll find everything you need to know! Let's start by talking about Branches.

<details>
  <summary>If you want to understand the basics of Branches, expand this section!</summary>

Branches are where the Source Code of Repositories resides. On GitHub, you'll find Repositories with two types of Source Code organization, see below:

- **Case 1, everything in the "main", "develop" or "master" Branch:** In this case, all the project's Source Code is in this Branch. The Source Code files may be located in the root of this Branch, or they may be contained within a folder.
- **Case 2, Source Code separated into Branches:** Depending on the project, the developer may want to separate the Source Code into Branches. Let's use a Minecraft mod as an example. The same mod can exist for Minecraft 1.12 and Minecraft 1.16, but they will be different Source Codes that will never be mixed/merged. Therefore, many developers create a Branch for each Source Code of each version.

It's important to understand this separation of how Branches can be used to organize the project, because this organization will vary from Repository to Repository, and from Project to Project, but generally, this is the cases for 99% of Repositories.

When a developer starts working on a project, whether to add a feature, fix a bug, etc. They will create a new branch, which we'll call the "Example Branch". This Branch will always have an Source Branch, which is the Branch where the developer wants to apply the Code Changes. After the "Example Branch" is created, it will inherit a copy of all the files from the Source Branch. The developer will work on the "Example Branch", and when finished, they will Merge this "Example Branch" back into their Source Branch (through a Pull Request, for example). After that, the Source Branch will receive all the Changes. Once this is done, the developer can delete the "Example Branch" if they want, as it has already fulfilled its purpose.

Now that you have the basic context of Branches, understanding how to send Pull Requests becomes easy!
</details>

To submit a Pull Request with your Code Changes or Documentation/ReadMe Changes, simply follow these steps:

- If you are or want to be a **Contributor**
  - It all starts with you creating an Issue to discuss the change you want to make to the Project. You can do this by going to the `Issues` tab of this Repository, and then creating an Issue using the `Code Change Proposal` or `Documentation/ReadMe Change Proposal` option. You are encouraged to create this discussion issue before you begin working because it's good to have a debate about what will be changed, so there's an understanding of how it will be done, why it will be done, the benefits, etc. If the change you intend to make is small, creating this issue is not mandatory, but if the change is considerable, then it becomes indispensable, and if you don't create the discussion issue, your Pull Request may be rejected.
- If you are a **Mantainer+**

1. If you are a Contributor, Fork this Repository. This will "copy" it to your GitHub Account. From there, simply clone the Fork to your PC. If you are a Maintainer+, just clone this Repository to your PC, because you already have write permissions.
2. Create a new Branch to start working on what you want to implement. Remember to select the Source Branch that contains the Source Code that you want to modify! If you are a Maintainer+, remember to follow the Branch creation guidelines when creating a Branch. They are located below!
3. Within this new Branch you created, work on the modifications you want to implement in this project!
4. 











- In this case, when someone implements a Feature or Modification in the project, they will create a Branch that uses this Branch as its source. The new Branch will have a copy of all the files from this Branch, and the developer will work in this new Branch. When they finish, they will merge the new Branch with the original Branch (through a Pull Request, for example). All the work they did in the new Branch will be sent to the original Branch, and they can delete the new Branch they just created.








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