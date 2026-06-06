This page is designed to guide you and provide guidelines on what to do and how to do it if you wish to Contribute to this repository. But before we continue...

# ☕ Are you a Regular User of this Product?

If you are a Regular User of this Repository's Product and are not interested in Contributing technically, but need help, want to report a bug, ask a question, suggest a new feature, or something similar, go to the `Issues` tab of this Repository. After that, click on `New Issue` to create a topic for your request.

You can also `💖 Sponsor` this project to help keep it going!

# 🤝 Do you want to Contribute technically?

If you want to Contribute technically to this Repository, proposing Code Changes and things like that, this is the topic for you! Here you will see the guidelines for Contributions, how to make Contributions, etc.

Just know that every Contribution is appreciated, even the smallest ones. This topic outlines a set of guidelines and best practices for Contributing. Following these guidelines helps us maintain a clean, organized, and high-performance Repository. Thank you for investing your time in Contributing to our project! ⭐

## 🌱 How to Contribute

Technically, you can Contribute to this Repository in the following ways:

- **📌 Testing the Product from this Repository:** You can do this simply by using the Product from this Repository, and as you see necessary, execute the next items on this list.
- **📌 Reporting Bugs:** This can be done through the `Issues` tab of this Repository.
- **📌 Suggesting Enhancements or giving Feedback:** This can be done through the `Issues` tab of this Repository.
- **📌 Proposing Code Changes:** This can be done through `Pull Requests` (this will be discussed in more detail below). It's great if you want to get your hands-on and work on the Source Code of this Repository, implementing new Features, Improvements, Optimizations, Fixes, and Refactorings yourself.
- **📌 Proposing Documentation/ReadMe Changes:** This can be done through `Pull Requests` (this will be discussed in more detail below). It's great if you've seen some incorrect information in the Documentation/ReadMe, or seen something that could be improved, or removed because it's outdated, and you want to get your hands-on and do it yourself.

### Roles

Before we continue, it's helpful to differentiate between the different Repository Roles, as each Role has different responsibilities and permissions:

- **👑 Owner:** Owns the Repository. Has full control and can perform any action.
- **🕵️‍♂️ Admin:** Has almost all permissions of the Owner. Can manage nearly everything. Acts as a secondary Owner.
- **📋 Maintainer:** Manages the project on a daily basis. Reviews Pull Requests, manages Issues, defines the Roadmap, approves/reject Merges and ensures overall project health.
- **🤝 Contributor:** Anyone outside the core team who participates by submitting Code Changes, Documentation/ReadMe Changes, suggestions, bug reports, or other Contributions. The Maintainer+ member can also make Contributions, but they have direct write access to the Repository.

### How do I send Pull Requests?

So, you want to get your hands-on and implement new Features, Improvements, Optimizations, Fixes, Refactorings or updates on Documentation/ReadMe, yourself? Perfect! Here you'll find everything you need to know!

<details>
  <summary> It's important that you understand the basics of Branches before continue. To learn more, expand this section!</summary>

---

Branches are where the Source Code of Repositories resides. On GitHub, you'll find Repositories with two types of Source Code organization, see below:

- **Case 1, everything in the "main", "develop" or "master" Branch:** In this case, all the project's Source Code is in this Branch. The Source Code files may be located in the root of this Branch, or they may be contained within a Folder or different Folders, one Folder for each Source Code of each sub-project that makes up the entire project.
- **Case 2, Source Code separated into Branches:** Depending on the project, the dev/team may want to separate the Source Code into Branches. Let's use a Minecraft mod as an example. The same mod can exist for Minecraft 1.12 and Minecraft 1.16, but they will be different Source Codes that will never be mixed/merged. Therefore, many devs/teams create a Branch for each Source Code of each version.
 
It's important to understand this separation of how Branches can be used to organize the project, because this organization will vary from Repository to Repository, and from project to project, but generally, these two cases, are the cases for 99% of Repositories.
 
When a developer starts working on a project, whether to add a feature, fix a bug, etc. They will create a new Branch, which we'll call as `Example Branch`. This Branch will always have a Source Branch, which is the Branch where the developer wants to apply the Code Changes. After the `Example Branch` is created, it will inherit a copy of all the files from the Source Branch. The developer will work on the `Example Branch`, and when finished, they will Merge this `Example Branch` back into their Source Branch (through a Pull Request, for example). After that, the Source Branch will receive all the Changes. Once this is done, the developer can delete the `Example Branch` if they want, as it has already fulfilled its purpose.
 
Now that you have the basic context of Branches, understanding how to send Pull Requests becomes easy!

---

</details>

ℹ️ To submit a Pull Request with your Code Changes or Documentation/ReadMe Changes, simply follow these steps:

- **If you are or want to be a `🤝 Contributor`**
  - ☑️ It all starts with you creating an Issue to discuss the change you want to make to the Project. You can do this by going to the `Issues` tab of this Repository, and then creating an Issue using the `Code Change Proposal` or `Documentation/ReadMe Change Proposal` option. **You are encouraged to create this discussion Issue before you begin working** because it's good to have a debate about what will be changed, so there's an understanding of how it will be done, why it will be done, the benefits, etc. **If the change you intend to make, is small, creating this Issue is not mandatory, but if the change is considerable, then it becomes indispensable**, and if you don't create the Issue, your Pull Request may be rejected. If you create an Issue for discussion, only start working if the **Maintainers+** agree with you are proposing, and give the green light.
  - ☑️ Now, Fork this Repository. This will "copy" it to your GitHub Account. Then, clone the Fork to your PC.
  - ☑️ On the Repository cloned on your PC, create a new Branch to start working on what you want to implement. Remember to select the Source Branch that contains the Source Code that you want to modify!
  - ☑️ Within this new Branch you created, work on the modifications you want to implement in this project.
- **If you are a `📋 Mantainer+`**
  - ☑️ It all starts with you discussing with other **Maintainers+**, about what you intend to change. Just like a **Contributor** would, but you likely have other communication channels with other **Maintainers+** besides the `Issues` tab. If you get the green light, proceed.
  - ☑️ Just clone this Repository to your PC, because you already have write permissions.
  - ☑️ On the Repository cloned on your PC, create a new Branch to start working on what you want to implement. Remember to select the Source Branch that contains the Source Code that you want to modify, but, **remember to follow the Branch creation guidelines** when creating a Branch. [They are located below!](#for-mantainers+-only)
  - ☑️ Within this new Branch you created, work on the modifications you want to implement in this project.
- **Continuing, for `🤝 Contributor` or `📋 Maintainer+`...**
  - ☑️ Once your work is finished on your PC, `Commit` it, followed by a `Push` to send your work to the Remote Repository. If you are a **Contributor** your work will be sent to your Fork in your GitHub Account. If you are a **Maintainer+**, your work will be sent to the Original Repository.
  - ☑️ Now, to create the Pull Request, go to the GitHub website, open your Forked Repository (if you are a **Contributor**) or the Original Repository (if you are a **Mantainer+**), and go to the `Pull Request` tab. There, you will click on `New Pull Request`.
  - ☑️ On this screen, to configure properly, at the top of the interface, under "Base Repository," select the Original Repository and the Branch to which you want to apply the changes. Select carefully. Under "Head Repository," select your Fork Repository (if you are a **Contributor**) or the Original Repository (if you are a **Mantainer+**) and the Branch you created to work with. View the changes. Now you can compare what has changed from your Branch of work, to the Branch that you want to apply the changes. 
  - ☑️ If you are sure that everything is correct, click `Create Pull Request`. When doing this, you will need to provide a `Title` and `Description` for your Pull Request. You will likely receive a Description already filled in with a Template, and you will only need to fill in the fields correctly. After doing this, simply click `Create Pull Request` again.
  - ☑️ After doing this, your Pull Request will be sent to the Original Repository, but it will remain linked to the Branch you created, so you can work on it. Any changes you make to that Branch on your PC, followed by a Commit/Push, will automatically be sent to the Pull Request as well, while it is open. While it is open, you can leave comments within the Pull Request, and the Maintainers+ can too. They can ask you questions, request changes, etc. In the end, they can accept the Pull Request (Merged), or reject it (Closed Without Merge).
    - **If the Pull Request is accepted or `✅ Merged`...**
      - The Commits from your work Branch are applied to the Branch that you want to apply the changes.
      - The Pull Request is automatically closed.
      - If the Pull Request was linked to an Issue with `Closes #<number>`, that Issue is closed automatically.
    - **If the Pull Request is rejected or `❌ Closed Without Merge`...**
      - No commits from your work Branch are applied.
      - The Pull Request is automatically closed.
  - ☑️ That's all! When a Pull Request is closed, regardless of whether it was Accepted or Rejected, the working Branch you created in the Fork Repository (if you are a **Contributor**) or Original Repository (if you are a **Mantainer+**) remains. You can delete it or continue working on it to create another Pull Request later.

Pull Requests are the heart of collaboration. They allow you to propose changes, discuss them openly, and improve the project together with the community. ❤️

### Code/Documentation/ReadMe Changes Submission Guidelines

Currently, these are the guidelines you should follow to submit changes to this repository:

- Comments in Source Code or Text files should be in English.
- In Source Code, any logic that is more complex should have clear comments explaining it.
- For UI/UX/Code/Documentation/ReadMe changes, try to follow the same organizational and visual standards as the rest of the project.

### Need Help?

If you have questions or need help, feel free to open a Issue or contact the maintainers at trough the `Issues` tab!

## 📋 For Maintainers+ Only

For **Maintainers+**, there are a few other things...

### 🚩 General Branch Creation Guidelines

  - For all Branches other than **main**, create a `README.md` at the root of the Branch, to redirect to Branch **main**. **You can skip this step if the Branch you are creating is a Branch that will be merged with another Branch later.**
  - For all Branches (except **gh-pages**), make sure they have a folder called `.github` and that this folder contains the files `CONTRIBUTING.md` and `PULL_REQUEST_TEMPLATE.md`, as well as the **main** Branch. If you created your Branch from an existing one, it most likely already inherited those files.

### 🚩 Source Code Branches That Never Will Be Merged Creation Guidelines

  - The names of these Source Code or Project Branches must follow the naming format `source-<operational-system-or-platform>-<framework-or-technology>`. For example: **source-1.20.1-forge**, **source-linux-javafx**, **source-web-vanilla.js**, **source-web-package.ts**, **source-game-modloader** or **source-any-unity**.

### 🚩 Branches That Always Will Be Merged Creation Guidelines

For Branches that will always be merged with another Branch, they should follow the naming pattern below. **Branches that do not follow this convention will be deleted!**

| Branch Type            | Naming Pattern                               | Example                                     |
| :--------------------- | :------------------------------------------- | :------------------------------------------ |
| **For Bug Fixes Work** | `<source-branch-name>/fix-<description>`     | `source-1.20.1-forge/fix-hud-alignment`     |
| **For Features Work**  | `<source-branch-name>/feature-<description>` | `source-1.20.1-forge/feature-sound-effects` |

### 🚩 Branch Maintenance Conventions

- Keep the same `.gitignore` file in all Branches. This prevents one Branch from accidentally considering a file ignored by another Branch, which can cause a mess, and also helps maintain consistency across Branches, while working on Local Repository.

> [!NOTE]
> For managing multiple Branches, it is recommended to install **Smart Branch Switch** in the Local Repository. This will help keep the work environment clean and organized, and will also provide alerts if anything is out of the ordinary, such as the absence of the files mentioned above. For more information about the **Smart Branch Switch**, click <a href="https://github.com/marcos4503/smart-branch-switch">here</a>.