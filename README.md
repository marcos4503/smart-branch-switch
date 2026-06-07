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

Every Git Repository has a hidden folder called `.git` located in the Repository's root directory. If you decide to use SBS, it will be installed inside the `.git` folder of your Local Repository. What happens next is simple: whenever a Git binary interacts with your Local Repository to switch the active Branch, it will look for `Hooks` to execute automatically, once the Branch switch is complete. It will then find SBS and call it automatically. The SBS will ran and will fulfill its functions. This applies to any Git binary that interacts with your Local Repository, whether it's Git Bash, your IDE's Git, GitHub Desktop, etc.