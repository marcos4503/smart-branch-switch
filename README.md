<p align="center" style="font-size: 2px;">
    <img src=".github/assets/cover.png" />
    <br>
    Get the .bat/.sh script that Installs/Uninstalls the Smart Branch Switch in your Git Local Repository, on <a href="https://github.com/marcos4503/smart-branch-switch/releases">Releases</a> page.
</p>
<p align="center" style="font-size: 2px;">
    <b>Main Branches of the Project</b>
    <br>
    <a href="https://github.com/marcos4503/smart-branch-switch">Main</a>
    •
    <a href="https://github.com/marcos4503/smart-branch-switch/tree/source-windows-wpf">Windows WPF Source Code</a>
</p>
<hr>

# 🔀 What is the Smart Branch Switch?

Smart Branch Switch (or "SBS") is a tool whose main purpose is to help you work in an organized and clean way, while switching between active Branches (a.k.a. Branch Checkout) in your Git Local Repository, on your PC.

You know when you're on a Branch, and there are several files that are ignored there by the Branch's `.gitignore` file, but then you switch Branches and all those junk files ignored by the previous Branch follow you to the new Branch? **Smart Branch Switch solves that!**

You know when you have multiple Branches and end up forgetting to do things like configuring the `.gitignore` file or keeping it synchronized across all Branches, or you forget that some Branch has a file like `CONTRIBUTING.md` missing? **Smart Branch Switch solves that!**

The Smart Branch Switch can help you with these and other things in a non-destructive, non-intrusive, and high-performance way. The SBS helps you in an automated way, but without taking away your control in any way; that is, it doesn't make you dependent on the tool. It doesn't act like a completely invisible ghost that makes you forget how to solve problems manually. It doesn't make your knowledge obsolete.

### How does the Smart Branch Switch work?

Every Git Repository has a hidden folder called `.git` located in the Repository's root directory. If you decide to use SBS, it will be installed inside the `.git` folder of your Local Repository. What happens next is simple: whenever a Git binary interacts with your Local Repository to switch the active Branch, it will look for `Hooks` to execute automatically, once the Branch switch is complete. It will then find SBS and call it automatically. The SBS will ran and will fulfill its functions. This applies to any Git binary that interacts with your Local Repository, whether it's Git Bash, your IDE's Git, GitHub Desktop, etc.