using CoroutinesDotNet;
using CoroutinesForWpf;
using MarcosTomaz.ATS;
using Smart_Branch_Switch.Controls.ListItems;
using Smart_Branch_Switch.Scripts;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Shell;

namespace Smart_Branch_Switch
{
    public partial class MainWindow : Window
    {
        //Public enums
        public enum ErrorHandleResponse
        {
            Unknown,
            SolveErrors,
            SkipErrors
        }
        public enum OverwriteHandleResponse
        {
            AskEverything,
            OverwriteAll,
            OverwriteNothing
        }
        public enum RunNotificationTier
        {
            Regular,
            Warning,
            Critical
        }

        //Public classes
        public class RunNotification
        {
            //Public variables
            public RunNotificationTier tier = RunNotificationTier.Regular;
            public string message = "";

            //Public methods

            public RunNotification(RunNotificationTier tier, string message)
            {
                //Store data
                this.tier = tier;
                this.message = message;
            }
        }

        //Private constants
        private const int LOGS_EXPAND_WINDOW_TO_SIZE = 512;
        private const int LOGS_EXPAND_SLIDERFX_TO_SIZE = 496;

        //Private cache variables
        private bool canCloseProgram = false;
        private string[] startArguments = null;
        private float defaultProgressSliderFxWidth = 0.0f;
        private string oldBranchName = "";
        private string currentBranchName = "";

        //Private variables
        private string currentPath = "";
        private string currentRunLogPath = "";
        private string currentRepositoryRootPath = "";
        private string currentRepositoryName = "";
        private bool isBranchChange = false;
        private string oldBranchHash = "";
        private string currentBranchHash = "";

        //Public variables
        public ErrorHandleResponse toLimbofilesErrorHandleDiagResponse = ErrorHandleResponse.Unknown;
        public OverwriteHandleResponse fromLimboFilesOverwriteHandleDiagResponse = OverwriteHandleResponse.AskEverything;
        public List<RunNotification> runNotificationsList = new List<RunNotification>();
        public List<LogItem> instantiatedLogItems = new List<LogItem>();

        //Core methods

        public MainWindow()
        {
            //Initialize the Window
            InitializeComponent();

            //Set the position for this window
            this.Top = 8;
            this.Left = 8;

            //Get the start arguments of this program
            startArguments = Environment.GetCommandLineArgs();

            //Get the current directory path for this program
            currentPath = (new DirectoryInfo(AppContext.BaseDirectory)).FullName.FixToWins();
            //Prepare the log path for this run
            currentRunLogPath = Path.Combine(currentPath, (Path.GetFileNameWithoutExtension(Environment.ProcessPath) + ".log")).FixToWins();
            //Try to get the root directory path for the owner repository...
            if ((new DirectoryInfo(currentPath)).Parent.Name.ToLower() == ".git")
                currentRepositoryRootPath = (new DirectoryInfo(currentPath)).Parent.Parent.FullName.FixToWins();

            //Reset the log file
            if (File.Exists(currentRunLogPath) == true)
                File.Delete(currentRunLogPath);
            //Initialize the log file
            File.WriteAllText(currentRunLogPath, (DateTime.Now.ToString("HH:mm:ss") + " - Starting Smart Branch Switch execution log."));

            //Inform paths
            SendLog("This log path: \"" + currentRunLogPath + "\"");
            SendLog("Smart Branch Switch executable directory path: \"" + currentPath + "\"");

            //If was not found a repository path, stop here
            if (currentRepositoryRootPath == "")
            {
                //Warn about the problem
                MessageBox.Show("The Context Repository could not be found.", "Smart Branch Switch: Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //Stop the execution of this instance
                Application.Current.Shutdown();
                //Cancel the execution
                return;
            }

            //Get the current repository name
            currentRepositoryName = (new DirectoryInfo(currentRepositoryRootPath)).Name;

            //Inform the repository root path
            SendLog("This repository root path: \"" + currentRepositoryRootPath + "\"");
            //Inform the repository name
            SendLog("This repository name: \"" + currentRepositoryName + "\"");

            //If is not possible to run Git commands, stop here
            try
            {
                //Try to run a command...
                Process cmdProcess = Process.Start(new ProcessStartInfo { FileName = "git", Arguments = "--version", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true });
                if (cmdProcess == null)
                    throw new Exception("Process start error.");
                bool wasRunnedWithoutFreezing = cmdProcess.WaitForExit(3000);
                if (wasRunnedWithoutFreezing == false)
                {
                    cmdProcess.Kill();
                    throw new Exception("Process start error.");
                }
                cmdProcess.Dispose();
            }
            catch (Exception e)
            {
                //Warn about the problem
                MessageBox.Show("Can't run Git commands on machine. Try checking Windows Environment Variables.", ("Smart Branch Switch: Error on \"" + currentRepositoryName + "\""), MessageBoxButton.OK, MessageBoxImage.Error);
                //Stop the execution of this instance
                Application.Current.Shutdown();
                //Cancel the execution
                return;
            }

            //Inform the system Git version found
            string gitVersion = RunGitCommandAndGetResponse("--version").Replace("git version", "").Replace("\r", "").Replace("\n", " ").Replace(" ", "");
            SendLog("Using Git version \"" + gitVersion + "\"");

            //If don't have enough arguments, cancel here
            if (startArguments.Length < 4)
            {
                //Warn about the problem
                MessageBox.Show("Not enough arguments were provided for start.", ("Smart Branch Switch: Error on \"" + currentRepositoryName + "\""), MessageBoxButton.OK, MessageBoxImage.Error);
                //Stop the execution of this instance
                Application.Current.Shutdown();
                //Cancel the execution
                return;
            }

            //Read the arguments
            if (startArguments[1] == "0")
                isBranchChange = false;
            if (startArguments[1] == "1")
                isBranchChange = true;
            oldBranchHash = startArguments[2];
            currentBranchHash = startArguments[3];

            //Inform the arguments received
            SendLog("Is the Branch changed now? \"" + isBranchChange + "\".");
            SendLog("Old Branch Hash: \"" + oldBranchHash + "\".");
            SendLog("Current Branch Hash: \"" + currentBranchHash + "\".");

            //Prepare the UI
            progressSlider.Value = 0.0d;
            defaultProgressSliderFxWidth = (float)progressSliderFx.Width;
            progressSliderFx.Width = 0.0f;
            repoName.Text = currentRepositoryName;
            logsPanel.Visibility = Visibility.Collapsed;
            switchBranchContext.Visibility = Visibility.Collapsed;

            //Override the closing code, to allow close only if is allowed by the program
            this.Closing += (s, e) =>
            {
                //Cancel the default event running
                e.Cancel = true;

                //If is not allowed to close now, do warning
                if (canCloseProgram == false)
                    MessageBox.Show("It's not possible to close the Smart Branch Switch now! Please wait.", ("Smart Branch Switch: Error on \"" + currentRepositoryName + "\""), MessageBoxButton.OK, MessageBoxImage.Error);
                //If is allowed to close now, then close
                if (canCloseProgram == true)
                    Application.Current.Shutdown();
            };

            //Inform that was started the Branch Switch processment
            SendLog("Running Hooks of Branch Switching...");
            //Start the Branch Switch processment
            RunHooksOfBranchSwitching();
        }

        //Private methods

        private void RunHooksOfBranchSwitching()
        {
            //Start the Hooks thread
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(this, new string[] { });
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Store result of success limbed files
                int limbedOkFilesCount = 0;
                //Store result of error of locked files
                int limbedErrorFilesCount = 0;
                //Store result of success unlimbed files
                int unlimbedOkFilesCount = 0;
                //Store result of error unlimbed files
                int unlimbedErrorFilesCount = 0;
                //Store result of was found ".gitignore" with different rules on root of all Branches
                bool diffGitIgnoresAcrossBranches = false;
                //Store result of was found files that are NOT ignored and NOT tracked from the last Branch
                string lastBranchLeakedFiles = "";
                //Store result of names of Branches that exists on Local, but not on Remote
                string branchesOnLocalOnly = "";
                //Store result of names of Branches that exists on Remote, but not on Local
                string branchesOnRemoteOnly = "";

                //Try to run the hooks...
                try
                {
                    //Wait some time...
                    threadTools.MakeThreadSleep(1500);

                    //If is a real Branch change, continue for Hooks..
                    if (isBranchChange == true)
                    {
                        ///========================== LIMBO MANAGEMENT ==========================///

                        //If the Old and Current Branches have the same Hash, means that one of the Branches is recently created...
                        if (oldBranchHash == currentBranchHash)
                        {
                            //Load the name of the Old and Current Branches accessing the logs and checking the symbolic-ref
                            oldBranchName = RunGitCommandAndGetResponse(("reflog -n 1 --format=\"%gs\"")).Replace("\r", "").Replace("\n", "").Split(" from ")[1].Split(" to ")[0].Trim();
                            currentBranchName = RunGitCommandAndGetResponse(("symbolic-ref --short HEAD")).Replace("\r", "").Replace("\n", "").Trim();
                        }
                        //If the Old and Current Branches have different Hashes, so both have Commits and is not recently created...
                        if (oldBranchHash != currentBranchHash)
                        {
                            //Load the name of the Old and Current Branches
                            oldBranchName = RunGitCommandAndGetResponse(("name-rev --name-only --refs=\"refs/heads/*\" " + oldBranchHash)).Replace("\r", "").Replace("\n", "").Trim();
                            currentBranchName = RunGitCommandAndGetResponse(("name-rev --name-only --refs=\"refs/heads/*\" " + currentBranchHash)).Replace("\r", "").Replace("\n", "").Trim();
                        }

                        //Report the names found for Old and Current Branches
                        SendLog("Old Branch Name: \"" + oldBranchName + "\".");
                        SendLog("Current Branch Name: \"" + currentBranchName + "\".");
                        //Update the UI to show the Branches switching
                        Application.Current.Dispatcher.Invoke(new Action(() => { switchBranchContext.Text = (oldBranchName + " > " + currentBranchName); switchBranchContext.Visibility = Visibility.Visible; }));

                        //Report progress
                        threadTools.ReportNewProgress((0.05f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Prepare the all Branches existing dictionary (format of: branch-name, bransh-name-hash)
                        Dictionary<string, string> existingBranchNames = new Dictionary<string, string>();
                        //Load the lines of all Branches existing in this repository
                        foreach (string branchName in RunGitCommandAndGetResponse("for-each-ref --format=\"%(refname:short)\" refs/heads/").Replace("\r", "").Split("\n"))
                            if (branchName != null && branchName != "")
                                if (existingBranchNames.ContainsKey(branchName) == false)
                                    existingBranchNames.Add(branchName, Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(branchName))).Replace("-", "").ToLower().Substring(0, 8));

                        //Report all the names of all Branches found in this repository
                        List<string> reportBranchNames = new List<string>();
                        foreach (var entry in existingBranchNames)
                            reportBranchNames.Add((entry.Key + " (" + entry.Value + ")"));
                        SendLog("Branchs existing in repository: " + GetStringArray(reportBranchNames.ToArray()));

                        //Report progress
                        threadTools.ReportNewProgress((0.10f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Prepare the paths for each base directory of Smart Branch Switch
                        string dirSbs = Path.Combine(currentRepositoryRootPath, @".git\sbs").FixToWins();
                        string dirLimbo = Path.Combine(currentRepositoryRootPath, @".git\sbs\limbo").FixToWins();
                        string dirTmpGi = Path.Combine(currentRepositoryRootPath, @".git\sbs\tmpgi").FixToWins();
                        //Ensure that the base directory structure of the Smart Branch Switch is created
                        if (Directory.Exists(dirSbs) == false)
                            Directory.CreateDirectory(dirSbs);
                        if (Directory.Exists(dirLimbo) == false)
                            Directory.CreateDirectory(dirLimbo);
                        if (Directory.Exists(dirTmpGi) == true)
                            Directory.Delete(dirTmpGi, true);
                        Directory.CreateDirectory(dirTmpGi);

                        //Report the structure creation
                        SendLog("Smart Branch Switch directories structure, validated on \".git\" directory in this repository.");

                        //Inform that is syncing the Limbo folders with existing Branches
                        SendLog("Syncing Limbo Folders with the existing Branches...");

                        //Create Limbo Folders for existing Branches that don't have
                        foreach (var entry in existingBranchNames)
                        {
                            //Prepare the path of Limbo Folder for current Branch...
                            string targetFolderPath = Path.Combine(dirLimbo, GetLimboFolderName(entry.Key, entry.Value)).FixToWins();
                            //If the Limbo Folder don't exists, create it and notify on log
                            if (Directory.Exists(targetFolderPath) == false)
                            {
                                Directory.CreateDirectory(targetFolderPath);
                                SendLog("Limbo Folder created at: \"" + targetFolderPath + "\"");
                            }
                        }
                        //Get a list of Limbo Folders that should exists in the Limbo, using currently existing Branches on repository
                        List<string> limboFoldersThatShouldExists = new List<string>();
                        foreach (var entry in existingBranchNames)
                            limboFoldersThatShouldExists.Add(GetLimboFolderName(entry.Key, entry.Value));
                        //Delete Limbo Folders that points to inexisting Branches
                        foreach (string folderPath in Directory.GetDirectories(dirLimbo))
                        {
                            //Get folder Name, with expected style of "branchname-hashedname"
                            string folderName = Path.GetFileName(folderPath);
                            //If the Limbo Folder exists, but dont have a Branch with the name, delete it and notify on log
                            if (limboFoldersThatShouldExists.Contains(folderName) == false)
                            {
                                Directory.Delete(folderPath, true);
                                SendLog("Limbo Folder removed at: \"" + folderPath + "\"");
                            }
                        }

                        //Report progress
                        threadTools.ReportNewProgress((0.15f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Inform that is searching by ".gitignore" files on the last Branch
                        SendLog("Searching by \".gitignore\" files of the \"" + oldBranchName + "\" Branch...");

                        //Prepare a list of files paths of ".gitignore" of last Branch
                        List<string> gitIgnoreFilesPath = new List<string>();
                        //Get a list of files path of the last Branch
                        string[] lastBranchFilesPathList = RunGitCommandAndGetResponse(("ls-tree -r --name-only -z --full-name " + oldBranchHash)).Split('\0');
                        //Fill the list of files of ".gitignore" of last Branch
                        foreach (string filePath in lastBranchFilesPathList)
                            if (filePath != null && filePath != "")
                                if (filePath.EndsWith(".gitignore", StringComparison.OrdinalIgnoreCase) == true)
                                    gitIgnoreFilesPath.Add(filePath);

                        //Report all the ".gitignore" files, found for the last Branch
                        SendLog(("Found " + gitIgnoreFilesPath.Count + "x \".gitignore\" files on \"" + oldBranchName + "\" Branch: " + GetStringArray(gitIgnoreFilesPath.ToArray())));

                        //Inform that is retrieving the ".gitignore" files of last Branch
                        SendLog("Retrieving \".gitignore\" files of \"" + oldBranchName + "\" Branch...");

                        //Prepare the store for the retrieved ".gitignore" files path
                        List<string> retrievedGitIgnoreFilesPath = new List<string>();
                        //Retrieve all ".gitignore" files of the last Branch, to the temp gitignores folder
                        float retrievedGiFilesCount = 0;
                        foreach (string giFilePath in gitIgnoreFilesPath)
                        {
                            //Get the current gitignore file content
                            string giFileContent = RunGitCommandAndGetResponse(("--no-pager show " + oldBranchHash + ":\"" + giFilePath.FixToUnix() + "\""));
                            //Prepare the final path for this gitignore file inside the temp gitignores folder
                            string giFinalPath = Path.Combine(dirTmpGi, giFilePath).FixToWins();
                            string giFinalDir = Path.GetDirectoryName(giFinalPath).FixToWins();
                            //Create the needed directories inside the temp gitignores folder, if not exists
                            if (Directory.Exists(giFinalDir) == false)
                                Directory.CreateDirectory(giFinalDir);
                            //Store the retrieved file
                            File.WriteAllText(giFinalPath, giFileContent, Encoding.UTF8);
                            //Add this gitignore file path to the list of retrieveds
                            retrievedGitIgnoreFilesPath.Add(giFinalPath);
                            //Increase the retrieved files count
                            retrievedGiFilesCount += 1.0f;
                            //Report progress
                            threadTools.ReportNewProgress((0.15f + ((retrievedGiFilesCount / (float)gitIgnoreFilesPath.Count) * 0.05f)).ToString());
                            //Wait time
                            threadTools.MakeThreadSleep(50);
                        }
                        //Leave a last Branch signature file
                        File.WriteAllText(Path.Combine(dirTmpGi, ("from-" + oldBranchName.Replace("/", "_").Replace("\\", "_"))).FixToWins(), "old_branch");

                        //Report progress
                        threadTools.ReportNewProgress((0.20f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Inform that is mapping all ignored files by last Branch
                        SendLog("Mapping all files of this Branch, ignored by \".gitignore\" files of the \"" + oldBranchName + "\" Branch and which of these files is also used/tracked by current Branch.");

                        //Prepare the list containing all the files existing on the current Branch, but is ignored by any ".gitignore" of the old Branch
                        HashSet<string> currentBranchFilesIgnoredByLastBranch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        //Feed the list of files...
                        foreach (string lastGitIgnoreFilePath in retrievedGitIgnoreFilesPath)
                        {
                            //Get the relative path for the current ".gitignore" file of last Branch
                            string giFileRelPath = lastGitIgnoreFilePath.Replace(currentRepositoryRootPath, "").FixToUnix().TrimStart('/');
                            //Get a list of files of the current Branch, that is ignored by the current ".gitignore" file of last Branch...
                            string[] thisBranchIgnoredFiles = RunGitCommandAndGetResponse("-c core.excludesFile=\"" + giFileRelPath + "\" ls-files -z -i -c -o --exclude-per-directory=\"" + giFileRelPath + "\"").Split('\0');
                            //Iterate through all returned files of current Branch
                            foreach (string ignoredFile in thisBranchIgnoredFiles)
                                if (ignoredFile != null && ignoredFile != "")
                                    currentBranchFilesIgnoredByLastBranch.Add(Path.Combine(currentRepositoryRootPath, ignoredFile).FixToWins());
                        }
                        //Get a list of all files being tracked by the current Branch...
                        string[] thisBranchTrackedFilesPath = RunGitCommandAndGetResponse("ls-files -z --cached").Split('\0');
                        //Prepare a list containing all the files existing on the current Branch, but is ignored by any ".gitignore" of the old Branch AND is used/tracked by the current Branch too!
                        HashSet<string> currentBranchFilesIgnoredByLastBranchAndAlsoUsedByThisBranch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        //Feed the list of files...
                        foreach (string filePath in thisBranchTrackedFilesPath)
                            if (filePath != null && filePath != "")
                            {
                                //Get the absolute path for current tracked file
                                string absoluteFilePath = Path.Combine(currentRepositoryRootPath, filePath).FixToWins();
                                //If the tracked file of current Branch, exists in the list of files ignored by any ".gitignore" of the old Branch...
                                if (currentBranchFilesIgnoredByLastBranch.Contains(absoluteFilePath) == true)
                                {
                                    //Remove this file from the list of files ignored by any ".gitignore" of the old Branch. This is because the files in this list, will be moved for the Limbo of the old Branch
                                    currentBranchFilesIgnoredByLastBranch.Remove(absoluteFilePath);
                                    //Add this file for the list of files ignored by any ".gitignore" of the old Branch, and is USED by this current Branch. Files in this list will be just copyied for the Limbo of the old Branch
                                    currentBranchFilesIgnoredByLastBranchAndAlsoUsedByThisBranch.Add(absoluteFilePath);
                                }
                            }

                        //Inform the mapping result
                        SendLog("Mapping finished!");
                        SendLog("Found " + currentBranchFilesIgnoredByLastBranch.Count + "x files in this Branch that is just ignored by \"" + oldBranchName + "\" Branch.");
                        SendLog("Found " + currentBranchFilesIgnoredByLastBranchAndAlsoUsedByThisBranch.Count + "x files in this Branch that is ignored by \"" + oldBranchName + "\" Branch and used by current Branch too!");

                        //Report progress
                        threadTools.ReportNewProgress((0.30f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Detect the absolute path for the Limbo Folder of the old Branch...
                        string oldBranchAbsolutePath = Path.Combine(dirLimbo, GetLimboFolderName(oldBranchName, existingBranchNames[oldBranchName])).FixToWins();
                        string oldBranchFlagAbsolutePath = Path.Combine(oldBranchAbsolutePath, "!branch_limbo.sbscheck").FixToWins();
                        //Inform the detected Limbo path of the old Branch
                        SendLog(("Detected Limbo Folder path for \"" + oldBranchName + "\" Branch: \"" + oldBranchAbsolutePath + "\""));
                        //If the Limbo Folder of the old Branch don't exists, stop here
                        if (Directory.Exists(oldBranchAbsolutePath) == false)
                            throw new Exception("Limbo Folder of old Branch was not found!");
                        //Prepare information storage..
                        int totalFilesToBeHandled = (currentBranchFilesIgnoredByLastBranch.Count + currentBranchFilesIgnoredByLastBranchAndAlsoUsedByThisBranch.Count);
                        int successfullyMovedFiles = 0;
                        int errorMovedFiles = 0;
                        Dictionary<string, string> filesErrorsOnMoveToLimboProcedure = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); //<- Key: current file path, Value: target file path on limbo
                        Dictionary<string, string> filesErrorsOnCopyToLimboProcedure = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); //<- Key: current file path, Value: target file path on limbo
                        //If on the Limbo Folder already have the flag file, warn it
                        if (File.Exists(oldBranchFlagAbsolutePath) == true)
                        {
                            totalFilesToBeHandled = 0;
                            SendLog("The task of saving files ignored by \"" + oldBranchName + "\", on Limbo, already was runned!");
                        }
                        //If is possible to save the content ignored by the old Branch, on the Limbo of old Branch, do it
                        if (File.Exists(oldBranchFlagAbsolutePath) == false)
                        {
                            //Handle the files that are ignored by the old Branch only...
                            foreach (string ignoredFile in currentBranchFilesIgnoredByLastBranch)
                            {
                                //Get the ignored file current absolute path
                                string currentIgnoredFilePath = ignoredFile;
                                //Get the target path and directory for this file, in the limbo
                                string targetIgnoredFilePath = Path.Combine(oldBranchAbsolutePath, currentIgnoredFilePath.Replace(currentRepositoryRootPath, "").FixToWins().TrimStart('\\')).FixToWins();
                                string targetIgnoredDirPath = Path.GetDirectoryName(targetIgnoredFilePath).FixToWins();
                                //Create the needed directory structury inside the Limbo
                                if (Directory.Exists(targetIgnoredDirPath) == false)
                                    Directory.CreateDirectory(targetIgnoredDirPath);
                                //If the current Branch Hash and the old Branch Hash is different: Means that are Branchs with a different evolution. Because of this, the ignored files of the old Branch, should be MOVED to Limbo
                                if (currentBranchHash != oldBranchHash)
                                {
                                    //Try to move this ignored file to the final path on Limbo. If fail, add this file in the Dictionary of fails...
                                    try /*************/ { File.Move(currentIgnoredFilePath, targetIgnoredFilePath, true); /***************************/ successfullyMovedFiles += 1; }
                                    catch (Exception e) { filesErrorsOnMoveToLimboProcedure.TryAdd(currentIgnoredFilePath, targetIgnoredFilePath); /**/ errorMovedFiles += 1; }
                                }
                                //If the current Branch Hash and the old Branch Hash is equal: Means that are equal Branches (or some is recently created). As they are equals, ignored files of old Branch should be COPIED to Limbo
                                if (currentBranchHash == oldBranchHash)
                                {
                                    //Try to copy this ignored file to the final path on Limbo. If fail, add this file in the Dictionary of fails...
                                    try /*************/ { File.Copy(currentIgnoredFilePath, targetIgnoredFilePath, true); /***************************/ successfullyMovedFiles += 1; }
                                    catch (Exception e) { filesErrorsOnCopyToLimboProcedure.TryAdd(currentIgnoredFilePath, targetIgnoredFilePath); /**/ errorMovedFiles += 1; }
                                }
                                //Get the total handled files until now
                                int totalHandledFilesUntilNow = (successfullyMovedFiles + errorMovedFiles);
                                //Update the progress...
                                threadTools.ReportNewProgress((0.30f + (((float)totalHandledFilesUntilNow / (float)totalFilesToBeHandled) * 0.30f)).ToString());
                                //Wait time for each 5 moved files...
                                if ((totalHandledFilesUntilNow % 5) == 0)
                                    threadTools.MakeThreadSleep(15);
                            }
                            //Handle the files that are ignored by the old Branch AND used/tracked by the current Branch
                            foreach (string ignoredAndUsedFile in currentBranchFilesIgnoredByLastBranchAndAlsoUsedByThisBranch)
                            {
                                //Get the ignored and used file current absolute path
                                string currentIgnoredAndUsedFilePath = ignoredAndUsedFile;
                                //Get the target path and directory for this file, in the limbo
                                string targetIgnoredAndUsedFilePath = Path.Combine(oldBranchAbsolutePath, currentIgnoredAndUsedFilePath.Replace(currentRepositoryRootPath, "").FixToWins().TrimStart('\\')).FixToWins();
                                string targetIgnoredAndUsedDirPath = Path.GetDirectoryName(targetIgnoredAndUsedFilePath).FixToWins();
                                //Create the needed directory structury inside the Limbo
                                if (Directory.Exists(targetIgnoredAndUsedDirPath) == false)
                                    Directory.CreateDirectory(targetIgnoredAndUsedDirPath);
                                //Try to copy this ignored and used file to the final path on Limbo. If fail, add this file in the Dictionary of fails...
                                try /*************/ { File.Copy(currentIgnoredAndUsedFilePath, targetIgnoredAndUsedFilePath, true); /***************************/ successfullyMovedFiles += 1; }
                                catch (Exception e) { filesErrorsOnCopyToLimboProcedure.TryAdd(currentIgnoredAndUsedFilePath, targetIgnoredAndUsedFilePath); /**/ errorMovedFiles += 1; }
                                //Get the total handled files until now
                                int totalHandledFilesUntilNow = (successfullyMovedFiles + errorMovedFiles);
                                //Update the progress...
                                threadTools.ReportNewProgress((0.30f + (((float)totalHandledFilesUntilNow / (float)totalFilesToBeHandled) * 0.30f)).ToString());
                                //Wait time for each 5 moved files...
                                if ((totalHandledFilesUntilNow % 5) == 0)
                                    threadTools.MakeThreadSleep(15);
                            }
                        }
                        //Create the flag file on the Limbo of the old Branch, to ensure that this task don't overwrite the saved data
                        if (File.Exists(oldBranchFlagAbsolutePath) == false)
                            File.WriteAllText(oldBranchFlagAbsolutePath, "lock");
                        //If have files that was experienced erros while copying or moving, handle it now...
                        if (errorMovedFiles > 0)
                        {
                            //Prepare the error messages to show in dialogs
                            string diagMsg = "Error on handle residual files from the previous Branch, to Limbo. Left %EC% error files! Close any processes that might be using these files, wait and click \"Yes\" to try again.";
                            string diagTlt = ("Smart Branch Switch: Problem on \"" + currentRepositoryName + "\"");
                            //Start the loop to handle the move error files...
                            while (filesErrorsOnMoveToLimboProcedure.Count > 0)
                            {
                                //If don't have a response for this error resolution
                                if (toLimbofilesErrorHandleDiagResponse == ErrorHandleResponse.Unknown)
                                {
                                    //Wait time...
                                    threadTools.MakeThreadSleep(500);
                                    //Show a sync dialog and receive the response...
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        MessageBoxResult mbr = MessageBox.Show(diagMsg.Replace("%EC%", filesErrorsOnMoveToLimboProcedure.Count.ToString()), diagTlt, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                        if (mbr == MessageBoxResult.Yes)
                                            toLimbofilesErrorHandleDiagResponse = ErrorHandleResponse.SolveErrors;
                                        if (mbr != MessageBoxResult.Yes)
                                            toLimbofilesErrorHandleDiagResponse = ErrorHandleResponse.SkipErrors;
                                    }));
                                    //Continue to next iteration
                                    continue;
                                }
                                //If was desired to solve the errors...
                                if (toLimbofilesErrorHandleDiagResponse == ErrorHandleResponse.SolveErrors)
                                {
                                    //Try to move the file again. If have a error, remove the agreed response of user, to show the error again
                                    try
                                    {
                                        string fileCurrentPath = filesErrorsOnMoveToLimboProcedure.Keys.FirstOrDefault();
                                        string fileTargetPath = filesErrorsOnMoveToLimboProcedure[fileCurrentPath];
                                        if (File.Exists(fileCurrentPath) == true)
                                            File.Move(fileCurrentPath, fileTargetPath, true);
                                        errorMovedFiles -= 1;
                                        successfullyMovedFiles += 1;
                                        filesErrorsOnMoveToLimboProcedure.Remove(fileCurrentPath);
                                    }
                                    catch (Exception e) { toLimbofilesErrorHandleDiagResponse = ErrorHandleResponse.Unknown; }
                                }
                                //If was desired to skip error solve, break this loop...
                                if (toLimbofilesErrorHandleDiagResponse == ErrorHandleResponse.SkipErrors)
                                    break;
                            }
                            //Start the loop to handle the copy error files...
                            while (filesErrorsOnCopyToLimboProcedure.Count > 0)
                            {
                                //If don't have a response for this error resolution
                                if (toLimbofilesErrorHandleDiagResponse == ErrorHandleResponse.Unknown)
                                {
                                    //Wait time...
                                    threadTools.MakeThreadSleep(500);
                                    //Show a sync dialog and receive the response...
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        MessageBoxResult mbr = MessageBox.Show(diagMsg.Replace("%EC%", filesErrorsOnCopyToLimboProcedure.Count.ToString()), diagTlt, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                        if (mbr == MessageBoxResult.Yes)
                                            toLimbofilesErrorHandleDiagResponse = ErrorHandleResponse.SolveErrors;
                                        if (mbr != MessageBoxResult.Yes)
                                            toLimbofilesErrorHandleDiagResponse = ErrorHandleResponse.SkipErrors;
                                    }));
                                    //Continue to next iteration
                                    continue;
                                }
                                //If was desired to solve the errors...
                                if (toLimbofilesErrorHandleDiagResponse == ErrorHandleResponse.SolveErrors)
                                {
                                    //Try to copy the file again. If have a error, remove the agreed response of user, to show the error again
                                    try
                                    {
                                        string fileCurrentPath = filesErrorsOnCopyToLimboProcedure.Keys.FirstOrDefault();
                                        string fileTargetPath = filesErrorsOnCopyToLimboProcedure[fileCurrentPath];
                                        if (File.Exists(fileCurrentPath) == true)
                                            File.Copy(fileCurrentPath, fileTargetPath, true);
                                        errorMovedFiles -= 1;
                                        successfullyMovedFiles += 1;
                                        filesErrorsOnCopyToLimboProcedure.Remove(fileCurrentPath);
                                    }
                                    catch (Exception e) { toLimbofilesErrorHandleDiagResponse = ErrorHandleResponse.Unknown; }
                                }
                                //If was desired to skip error solve, break this loop...
                                if (toLimbofilesErrorHandleDiagResponse == ErrorHandleResponse.SkipErrors)
                                    break;
                            }
                        }

                        //Inform the Limbo Folder handling result
                        SendLog(("The task of saving files ignored by \"" + oldBranchName + "\", on Limbo, is finished!"));
                        SendLog(("Total of " + successfullyMovedFiles + " files was moved/copied to Limbo of \"" + oldBranchName + "\" Branch. Files used/tracked by \"" + currentBranchName + "\" Branch is just copied."));
                        SendLog(("Total of " + errorMovedFiles + " files was NOT moved/copied to Limbo of \"" + oldBranchName + "\" Branch. They presented operational errors. Possibly because some process is using them."));
                        SendLog(("Total of " + totalFilesToBeHandled + " files was processed in this send to Limbo task."));

                        //Report progress
                        threadTools.ReportNewProgress((0.60f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Inform that is removing empty Folders of the current Branch
                        SendLog("Deleting empty Folders of \"" + currentBranchName + "\" Branch.");

                        //Delete all empty folders of current Branch, and get the count of deleted folders
                        int deletedFolders = DeleteEmptyFoldersAt(currentRepositoryRootPath);

                        //Inform the deleted Folders count
                        SendLog("A total of " + deletedFolders + "x empty folders was deleted.");

                        //Detect the absolute path for the Limbo Folder of the current Branch...
                        string currentBranchAbsolutePath = Path.Combine(dirLimbo, GetLimboFolderName(currentBranchName, existingBranchNames[currentBranchName])).FixToWins();
                        string currentBranchFlagAbsolutePath = Path.Combine(currentBranchAbsolutePath, "!branch_limbo.sbscheck").FixToWins();
                        //Inform the detected Limbo path of the current Branch
                        SendLog(("Detected Limbo Folder path for \"" + currentBranchName + "\" Branch: \"" + currentBranchAbsolutePath + "\""));
                        //If the Limbo Folder of the current Branch don't exists, stop here
                        if (Directory.Exists(currentBranchAbsolutePath) == false)
                            throw new Exception("Limbo Folder of current Branch was not found!");
                        //Prepare information storage..
                        int successfullyRestoredFiles = 0;
                        int errorRestoredFiles = 0;
                        //If on the Limbo Folder don't have the flag file, warn it
                        if (File.Exists(currentBranchFlagAbsolutePath) == false)
                            SendLog("There are no files registry to be restored from the Branch \"" + currentBranchName + "\" Limbo.");
                        //If is possible to restore the content from the Limbo of current Branch, do it
                        if (File.Exists(currentBranchFlagAbsolutePath) == true)
                        {
                            //Prepare the overwrite solve dialog messages
                            string overwriteDiagMsg1 = ("The following file already exists in the current Branch, and therefore the version of Limbo cannot be restored:\n\n" +
                                                       "- %FILEP%\n\n" +
                                                       "What do you want to do with this file of the current Branch?\n\n" +
                                                       "- Yes: Overwrite with the Limbo version.\n" +
                                                       "- No: Do not overwrite and ignore the Limbo version.\n" +
                                                       "- Cancel: Cancel this operation to choose what to do with this and next occurrences of this type.");
                            string overwriteDiagMsg2 = ("What do you want to do with any conflicts that might occur during this Limbo file restoration for this current Branch?\n\n" +
                                                        "- Yes: Overwrite all occurrences of conflict of files in the current Branch, with their respective Limbo versions.\n" +
                                                        "- No: Do not overwrite anything and ignore Limbo versions in case of conflict.");
                            string overwriteDiagTlt = ("Smart Branch Switch: Problem on \"" + currentRepositoryName + "\"");
                            //Get a list of all files existing on the Limbo of current Branch
                            string[] limboFilesPaths = Directory.GetFiles(currentBranchAbsolutePath, "*", SearchOption.AllDirectories);
                            //Iterate through all files to restore all...
                            foreach (string limboFilePath in limboFilesPaths)
                            {
                                //If is the flag file, ignore it
                                if (limboFilePath.EndsWith("!branch_limbo.sbscheck") == true)
                                    continue;
                                //Get the limbo file absolute path
                                string currentFileAbsolutePath = limboFilePath.FixToWins();
                                //Get the target path and directory for this file of the limbo
                                string targetFilePath = Path.Combine(currentRepositoryRootPath, currentFileAbsolutePath.Replace(currentBranchAbsolutePath, "").FixToWins().TrimStart('\\')).FixToWins();
                                string targetDirPath = Path.GetDirectoryName(targetFilePath).FixToWins();
                                //If the target file already exists on current Branch...
                                if (File.Exists(targetFilePath) == true)
                                {
                                    //If is desired to ask every conflict...
                                    if (fromLimboFilesOverwriteHandleDiagResponse == OverwriteHandleResponse.AskEverything)
                                    {
                                        //Wait time...
                                        threadTools.MakeThreadSleep(500);
                                        //Show a sync dialog and receive the response...
                                        Application.Current.Dispatcher.Invoke(new Action(() =>
                                        {
                                            //Show the first dialog of what to do with the current conflict
                                            MessageBoxResult mbr = MessageBox.Show(overwriteDiagMsg1.Replace("%FILEP%", targetFilePath), overwriteDiagTlt, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                            //If was desired to overwrite...
                                            if (mbr == MessageBoxResult.Yes)
                                                if (File.Exists(targetFilePath) == true)
                                                    try { File.Delete(targetFilePath); } catch (Exception ex) { errorRestoredFiles += 1; } //<- If fail, register this as a restore error
                                            //If was desired to not overwrite
                                            if (mbr == MessageBoxResult.No)
                                                errorRestoredFiles += 1;       //<- Register this as a restore error
                                            //If was desired to see more options...
                                            if (mbr == MessageBoxResult.Cancel)
                                            {
                                                //Show second dialog of what to do to all ocurrences
                                                MessageBoxResult mbr2 = MessageBox.Show(overwriteDiagMsg2, overwriteDiagTlt, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                                //Store the persistent response
                                                if (mbr2 == MessageBoxResult.Yes)
                                                    fromLimboFilesOverwriteHandleDiagResponse = OverwriteHandleResponse.OverwriteAll;
                                                if (mbr2 != MessageBoxResult.Yes)
                                                    fromLimboFilesOverwriteHandleDiagResponse = OverwriteHandleResponse.OverwriteNothing;
                                            }
                                        }));
                                    }
                                    //If is desired to solve all conflicts automatically, delete the already existing file...
                                    if (fromLimboFilesOverwriteHandleDiagResponse == OverwriteHandleResponse.OverwriteAll)
                                        if (File.Exists(targetFilePath) == true)
                                            try { File.Delete(targetFilePath); } catch (Exception ex) { errorRestoredFiles += 1; } //<- If fail, register this as a restore error
                                    //If is desired to overwrite nothing
                                    if (fromLimboFilesOverwriteHandleDiagResponse == OverwriteHandleResponse.OverwriteNothing)
                                    {
                                        //Register this as a restore error
                                        errorRestoredFiles += 1;
                                    }
                                }
                                //If the target file don't exists on current Branch...
                                if (File.Exists(targetFilePath) == false)
                                {
                                    //Try to do the operation...
                                    try
                                    {
                                        //Create the needed directory structury inside the Branch
                                        if (Directory.Exists(targetDirPath) == false)
                                            Directory.CreateDirectory(targetDirPath);
                                        //Do the movement of restoration of Limbo, for this file
                                        File.Move(currentFileAbsolutePath, targetFilePath, true);
                                        successfullyRestoredFiles += 1;
                                    }
                                    catch (Exception e)
                                    {
                                        //Register this as a restore error
                                        errorRestoredFiles += 1;
                                    }
                                }
                                //Get the total handled files until now
                                int totalHandledFilesUntilNow = (successfullyRestoredFiles + errorRestoredFiles);
                                //Update the progress...
                                threadTools.ReportNewProgress((0.60f + (((float)totalHandledFilesUntilNow / (float)limboFilesPaths.Length) * 0.30f)).ToString());
                                //Wait time for each 5 moved files...
                                if ((totalHandledFilesUntilNow % 5) == 0)
                                    threadTools.MakeThreadSleep(15);
                            }
                        }
                        //Delete the flag file on the Limbo of the current Branch, to leave a signal that now, the Limbo can receive files again!
                        if (File.Exists(currentBranchFlagAbsolutePath) == true)
                            File.Delete(currentBranchFlagAbsolutePath);
                        //Delete the Limbo Folder of current Branch and create it again, to ensure that the Limbo is totally cleared
                        if (Directory.Exists(currentBranchAbsolutePath) == true)
                            Directory.Delete(currentBranchAbsolutePath, true);
                        threadTools.MakeThreadSleep(250);
                        Directory.CreateDirectory(currentBranchAbsolutePath);

                        //Inform the Limbo Folder handling result
                        SendLog(("The task of restoring files of \"" + currentBranchName + "\" Limbo was finished!"));
                        SendLog(("Total of " + successfullyRestoredFiles + " files was moved from Limbo, back to the \"" + currentBranchName + "\" Branch."));
                        SendLog(("Total of " + errorRestoredFiles + " files was NOT moved from Limbo, back to \"" + currentBranchName + "\" Branch. Possible conflicts with files already on Branch, or a process was using them."));

                        //Report progress
                        threadTools.ReportNewProgress((0.90f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Store the result informations for the Hook of Limbo handling
                        limbedOkFilesCount = successfullyMovedFiles;
                        limbedErrorFilesCount = errorMovedFiles;
                        unlimbedOkFilesCount = successfullyRestoredFiles;
                        unlimbedErrorFilesCount = errorRestoredFiles;

                        ///========================== DIFFERENT .GITIGNORE FILES ACROSS BRANCHES, DETECTION ==========================///

                        //Inform that is analyzing the ".gitignore" at the root of all Branches
                        SendLog("Analyzing differences between \".gitignore\" files across roots of all Branches.");

                        //Prepare the response of different .gitignore found
                        bool wasFoundDifferentGitIgnoreAcrossBranchesRoots = false;

                        //Prepare a dictionary to store content of all ".gitignore" files on root of all Branches
                        Dictionary<string, HashSet<string>> existingBranchesGitIgnoreOnRoot = new Dictionary<string, HashSet<string>>();
                        //Load ".gitignore" content of all Branches root and load rules
                        foreach (var entry in existingBranchNames)
                        {
                            //Try to load the content of ".gitignore" on root of this Branch...
                            string giFileLoadResult = RunGitCommandAndGetResponse(("--no-pager show " + entry.Key + ":\".gitignore\""));
                            //If was not successfully, stop here
                            if (giFileLoadResult.Replace("\"", "'").Contains("fatal: path '.gitignore' does not exist in") == true)
                            {
                                //Add this empty to the existing branches info Dictionary
                                existingBranchesGitIgnoreOnRoot.TryAdd(entry.Key, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                                //Skip to next Branch...
                                continue;
                            }
                            //Prepare a Hashset of rules of this ".gitignore" file of root of this Branch
                            HashSet<string> giFileRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            //Get all lines of the ".gitignore" of root of this Branch...
                            string[] giFileLines = giFileLoadResult.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                            //Iterate through all lines...
                            foreach (string giLine in giFileLines)
                            {
                                //Remove unnecessary characters of line
                                string trimmedLine = giLine.Trim();
                                //Ignore this line if is empty or if is a commentary
                                if (string.IsNullOrEmpty(trimmedLine) == true || trimmedLine.StartsWith("#") == true)
                                    continue;
                                //Add this line to
                                giFileRules.Add(trimmedLine);
                            }
                            //Add this to the existing branches info Dictionary
                            existingBranchesGitIgnoreOnRoot.TryAdd(entry.Key, giFileRules);
                        }
                        //If have more than one branches...
                        if (existingBranchesGitIgnoreOnRoot.Keys.Count > 1)
                        {
                            //Get all rules of all Branches, into a array of HashSets
                            HashSet<string>[] allBranchesRootsGitIgnore = existingBranchesGitIgnoreOnRoot.Values.ToArray();
                            //Get the rules of first Branch
                            HashSet<string> baseBranchRootGitIgnore = allBranchesRootsGitIgnore[0];
                            //Compare the ".gitignore" rules on root of all Branches, with the ".gitignore" of the first Branch
                            for (int i = 1; i < allBranchesRootsGitIgnore.Length; i++)
                                if (allBranchesRootsGitIgnore[i].SetEquals(baseBranchRootGitIgnore) == false)
                                {
                                    //Inform that was found differences
                                    wasFoundDifferentGitIgnoreAcrossBranchesRoots = true;
                                    //Stop the loop here
                                    break;
                                }
                        }

                        //Inform the result..
                        if (wasFoundDifferentGitIgnoreAcrossBranchesRoots == true)
                            SendLog("There are Branches with different \".gitignore\" files in the root.");
                        if (wasFoundDifferentGitIgnoreAcrossBranchesRoots == false)
                            SendLog("All Branches have identical \".gitignore\" files in their roots.");

                        //Report progress
                        threadTools.ReportNewProgress((0.92f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Store the result informations for the Hook of .gitignore differences detection
                        diffGitIgnoresAcrossBranches = wasFoundDifferentGitIgnoreAcrossBranchesRoots;

                        ///========================== NOT TRACKED/COMMITED FILES WARNING ==========================///

                        //Inform that is searching by files not tracked/commited existing in the current Branch
                        SendLog("Searching by files not tracked/commited existing in current Branch.");

                        //Prepare the response of not tracked/commited files existing on current Branch
                        List<string> notTrackedOrCommitedFilesPathInThisBranch = new List<string>();

                        //Fill the list with files not tracked/commited, existing on current Branch
                        foreach (string filePath in RunGitCommandAndGetResponse("ls-files -o --exclude-standard -z").Split('\0'))
                        {
                            //If this line is empty, skip it
                            if (filePath == null || filePath == "")
                                continue;
                            //Add this file path to the list
                            notTrackedOrCommitedFilesPathInThisBranch.Add(filePath.FixToWins());
                        }

                        //Inform the result
                        SendLog("Found " + notTrackedOrCommitedFilesPathInThisBranch.Count + "x files not tracked/commited in the \"" + currentBranchName + "\" Branch. This don't include excluded files in \".gitignore\" files.");

                        //Report progress
                        threadTools.ReportNewProgress((0.94f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Store the result informations for the Hook of not tracked or commited files detection
                        lastBranchLeakedFiles = GetRawStringArray(notTrackedOrCommitedFilesPathInThisBranch.ToArray());

                        ///========================== UPDATE THE BRANCHES REFERENCES ==========================///

                        //Inform that is updating only existing Branches references on Local Repository
                        SendLog("Updating only the references of existing Branches on Local Repository and Remote Repository...");

                        //Update the existing Branches references
                        RunGitCommandAndGetResponse("fetch --prune");

                        ///========================== BRANCHES ONLY ON LOCAL WARNING ==========================///

                        //Inform that is searching by Branches that only exists on local repository
                        SendLog("Searching by Branches that only exists on Local Repository.");

                        //Prepare the response for Branches that only exists on local
                        HashSet<string> branchesExistingOnlyOnLocal = new HashSet<string>();

                        //Fill the list with name of Branches that exists only on local repository
                        foreach (string branchNameAndRemoteInfo in RunGitCommandAndGetResponse("branch --format=\"%(refname:short)|%(upstream)|%(upstream:track)\"").Replace("\r", "").Split('\n'))
                            if (branchNameAndRemoteInfo != null && branchNameAndRemoteInfo != "")
                            {
                                //Get the splitted info
                                string[] branchDataParts = branchNameAndRemoteInfo.Split("|");
                                string branchName = branchDataParts[0];
                                string branchRemoteRef = ((branchDataParts.Length >= 2) ? branchDataParts[1] : "");
                                string branchRemoteSts = ((branchDataParts.Length >= 3) ? branchDataParts[2] : "");
                                //If don't have remote info for this Branch, add it to the list
                                if (branchRemoteRef == null || branchRemoteRef == "")
                                    branchesExistingOnlyOnLocal.Add(branchName);
                                //If have status of "gone" on remote, add it to the list
                                if (branchRemoteSts != null && branchRemoteSts.Contains("gone") == true)
                                    branchesExistingOnlyOnLocal.Add(branchName);
                            }

                        //Inform the result
                        SendLog("Found " + branchesExistingOnlyOnLocal.Count + "x Branches that exists only on Local Repository: " + GetStringArray(branchesExistingOnlyOnLocal.ToArray()));

                        //Report progress
                        threadTools.ReportNewProgress((0.98f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Store the result informations for the Hook of branches existing only on local
                        branchesOnLocalOnly = GetRawStringArray(branchesExistingOnlyOnLocal.ToArray());

                        ///========================== BRANCHES ONLY ON REMOTE WARNING ==========================///

                        //Inform that is searching by Branches that only exists on remote repository
                        SendLog("Searching by Branches that only exists on Remote Repository.");

                        //Prepare the response for Branches that only exists on remote
                        HashSet<string> branchesExistingOnlyOnRemote = new HashSet<string>();

                        //Fill the list with name of Branches that exists only on remote repository
                        foreach (string rawBranchName in RunGitCommandAndGetResponse("branch -r --format=\"%(refname:short)\"").Replace("\r", "").Split('\n'))
                            if (rawBranchName != null && rawBranchName != "")
                            {
                                //Get the trimmed raw name
                                string trimmedRawName = rawBranchName.Trim();
                                //If is not a expected branch name, skip this
                                if (trimmedRawName == "origin" || trimmedRawName.ToLower().Contains("head") == true)
                                    continue;
                                //Get the formated branch name
                                string formatedBranchName = trimmedRawName;
                                if (formatedBranchName.StartsWith("origin/") == true)
                                    formatedBranchName = formatedBranchName.Substring("origin/".Length);
                                //If the formated branch name don't exists on the dictionary of local branches, add it to the list
                                if (existingBranchNames.ContainsKey(formatedBranchName) == false)
                                    branchesExistingOnlyOnRemote.Add(formatedBranchName);
                            }

                        //Inform the result
                        SendLog("Found " + branchesExistingOnlyOnRemote.Count + "x Branches that exists only on Remote Repository: " + GetStringArray(branchesExistingOnlyOnRemote.ToArray()));

                        //Report progress
                        threadTools.ReportNewProgress((1.0f).ToString());
                        //Wait time
                        threadTools.MakeThreadSleep(25);

                        //Store the result informations for the Hook of branches existing only on remote
                        branchesOnRemoteOnly = GetRawStringArray(branchesExistingOnlyOnRemote.ToArray());
                    }

                    //Wait some time...
                    threadTools.MakeThreadSleep(1500);
                }
                catch (Exception exception)
                {
                    //Inform the error and stop here
                    return new string[]
                    {   "error", (exception.Message + "\n\n" + exception.StackTrace),
                        limbedOkFilesCount.ToString(), limbedErrorFilesCount.ToString(), unlimbedOkFilesCount.ToString(), unlimbedErrorFilesCount.ToString(),
                        diffGitIgnoresAcrossBranches.ToString(), lastBranchLeakedFiles, branchesOnLocalOnly, branchesOnRemoteOnly
                    };
                }

                //Return the result of this background code...
                return new string[] {
                    "success", "",
                    limbedOkFilesCount.ToString(), limbedErrorFilesCount.ToString(), unlimbedOkFilesCount.ToString(), unlimbedErrorFilesCount.ToString(),
                    diffGitIgnoresAcrossBranches.ToString(), lastBranchLeakedFiles, branchesOnLocalOnly, branchesOnRemoteOnly
                };
            };
            asyncTask.onNewProgress_RunMainThread += (callerWindow, newProgress) =>
            {
                //Get the message
                float progress = float.Parse(newProgress);
                //Update the progress
                progressSlider.Value = (progress * 100.0f);
                progressSliderFx.Width = (progress * defaultProgressSliderFxWidth);
                taskbarProgress.ProgressState = TaskbarItemProgressState.Normal;
                taskbarProgress.ProgressValue = progress;
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Get the thread response
                string threadTaskResponse = backgroundResult[0];
                string errorMessage = backgroundResult[1];
                int successLimbedFilesQty = int.Parse(backgroundResult[2]);
                int errorLimbedFilesQty = int.Parse(backgroundResult[3]);
                int successUnlimbedFilesQty = int.Parse(backgroundResult[4]);
                int errorUnlimbedFilesQty = int.Parse(backgroundResult[5]);
                bool foundDiffGitIgnoresAcrossBranches = bool.Parse(backgroundResult[6]);
                string[] lastBranchLeakedFilesPath = backgroundResult[7].Split("|||");
                string[] branchesOnLocalOnly = backgroundResult[8].Split("|||");
                string[] branchesOnRemoteOnly = backgroundResult[9].Split("|||");

                //If have a error, inform it
                if (threadTaskResponse != "success")
                {
                    SendLog(("Error on \"" + currentRepositoryName + "\": " + errorMessage));
                    MessageBox.Show(errorMessage, ("Smart Branch Switch: Error on \"" + currentRepositoryName + "\""), MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //Inform that is finishing
                SendLog("Finishing Smart Branch Switch execution log.");

                //If exists different ".gitignore" files across root of Branches...
                if (foundDiffGitIgnoresAcrossBranches == true)
                    if (File.Exists(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnDiffGitIgnore.flag")) == false)
                    {
                        //Show the warning message about different ".gitignore" files
                        MessageBoxResult mbr = MessageBox.Show(("Branches were found where the root contains \".gitignore\" files with different rules. Please check if all Branches " +
                                                                "have \".gitignore\" files in their root. If they do, ensure that these files contain the same rules across all Branches.\n\n" +
                                                                "These files are essential for Git to know what it should or shouldn't track/commit, preventing your repository from being " +
                                                                "bloated with unnecessary files. Furthermore, these files are the cornerstone used by Smart Branch Switch to determine what " +
                                                                "should be saved to Limbo, or not, after an Branch switch.\n\n" +
                                                                "Although Smart Branch Switch can handle different \".gitignore\" files in each Branch, still efficiently cleaning up ignored " +
                                                                "files, having identical \".gitignore\" files in the root of all Branches ensures the most predictable operation of Smart Branch " +
                                                                "Switch, keeping your local repository organized according to the Branch you are currently on. Furthermore, ensuring " +
                                                                "identical \".gitignore\" files in the roots of all Branches, guarantees that other people who clone your repository on their " +
                                                                "PCs, and prefer not to use Smart Branch Switch, will not suffer from a mess of ignored files when switching Branches. In " +
                                                                "addition, it improves the functionality and predictability of Git itself, avoiding unexpected behaviors.\n\n" +
                                                                "Click \"Cancel\" to avoid receiving this warning again for this repository."),
                                                                ("Smart Branch Switch: Warning on \"" + currentRepositoryName + "\""),
                                                                MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        //If was desired to not receive warning abot this again, set a flag
                        if (mbr == MessageBoxResult.Cancel)
                            File.WriteAllText(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnDiffGitIgnore.flag"), "flag");
                        //Send a notification
                        runNotificationsList.Add(new RunNotification(RunNotificationTier.Critical, "Found Branches were root not contains a \".gitignore\" file or contains \".gitignore\" with different rules across Branches."));
                    }

                //If not exists a "README.md" on root of the current Branch...
                if (File.Exists(Path.Combine(currentRepositoryRootPath, "README.md")) == false)
                    if (File.Exists(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnReadme.flag")) == false)
                    {
                        //Show the warning message
                        MessageBoxResult mbr = MessageBox.Show(("Can't find a \"README.md\" file on root of the current \"" + currentBranchName + "\" Branch. " +
                                                                "You might want to create one to keep the Branch organized with a redirect link to another Branch, extra instructions, issue reference, checklist, etc.\n\n" +
                                                                "Click \"Cancel\" to avoid receiving this warning again for this repository."),
                                                                ("Smart Branch Switch: Warning on \"" + currentRepositoryName + "\""),
                                                                MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        //If was desired to not receive warning abot this again, set a flag
                        if (mbr == MessageBoxResult.Cancel)
                            File.WriteAllText(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnReadme.flag"), "flag");
                        //Send a notification
                        runNotificationsList.Add(new RunNotification(RunNotificationTier.Regular, "The file \"README.md\" was not found in \"" + currentBranchName + "\" Branch."));
                    }

                //If not exists a valid "CONTRIBUTING.md" on the current Branch...
                string contributingFilePath0 = Path.Combine(currentRepositoryRootPath, "CONTRIBUTING.md");
                string contributingFilePath1 = Path.Combine(currentRepositoryRootPath, @".github\CONTRIBUTING.md");
                string contributingFilePath2 = Path.Combine(currentRepositoryRootPath, @"docs\CONTRIBUTING.md");
                if (File.Exists(contributingFilePath0) == false && File.Exists(contributingFilePath1) == false && File.Exists(contributingFilePath2) == false)
                    if (File.Exists(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnContribute.flag")) == false)
                    {
                        //Show the warning message
                        MessageBoxResult mbr = MessageBox.Show(("Can't find a valid \"CONTRIBUTING.md\" file on the current \"" + currentBranchName + "\" Branch. " +
                                                                "You may want to create this file to keep the contribution instructions/rules for this Branch, very clear.\n\n" +
                                                                "Click \"Cancel\" to avoid receiving this warning again for this repository."),
                                                                ("Smart Branch Switch: Warning on \"" + currentRepositoryName + "\""),
                                                                MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        //If was desired to not receive warning abot this again, set a flag
                        if (mbr == MessageBoxResult.Cancel)
                            File.WriteAllText(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnContribute.flag"), "flag");
                        //Send a notification
                        runNotificationsList.Add(new RunNotification(RunNotificationTier.Regular, "The file \"CONTRIBUTING.md\" was not found in \"" + currentBranchName + "\" Branch."));
                    }

                //If not exists a valid "PULL_REQUEST_TEMPLATE" on the current Branch...
                string prtFilePath0 = Path.Combine(currentRepositoryRootPath, "PULL_REQUEST_TEMPLATE.md");
                string prtFilePath1 = Path.Combine(currentRepositoryRootPath, @".github\PULL_REQUEST_TEMPLATE.md");
                string prtFilePath2 = Path.Combine(currentRepositoryRootPath, @"docs\PULL_REQUEST_TEMPLATE.md");
                string prtDirPath0 = Path.Combine(currentRepositoryRootPath, @".github\PULL_REQUEST_TEMPLATE");
                if (File.Exists(prtFilePath0) == false && File.Exists(prtFilePath1) == false && File.Exists(prtFilePath2) == false && Directory.Exists(prtDirPath0) == false)
                    if (File.Exists(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnPullReqTemp.flag")) == false)
                    {
                        //Show the warning message
                        MessageBoxResult mbr = MessageBox.Show(("Can't find a valid \"PULL_REQUEST_TEMPLATE\" file/directory on the current \"" + currentBranchName + "\" Branch. " +
                                                                "You may want to create this file/directory to help, better guiding the Pull Request processes that this Branch may receive.\n\n" +
                                                                "Click \"Cancel\" to avoid receiving this warning again for this repository."),
                                                                ("Smart Branch Switch: Warning on \"" + currentRepositoryName + "\""),
                                                                MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        //If was desired to not receive warning abot this again, set a flag
                        if (mbr == MessageBoxResult.Cancel)
                            File.WriteAllText(Path.Combine(currentRepositoryRootPath, @".git\sbs\dontWarnOnPullReqTemp.flag"), "flag");
                        //Send a notification
                        runNotificationsList.Add(new RunNotification(RunNotificationTier.Regular, "The file/directory \"PULL_REQUEST_TEMPLATE\" was not found in \"" + currentBranchName + "\" Branch."));
                    }

                //If have errors when limbing the files, send a notification
                if (errorLimbedFilesQty > 0)
                    runNotificationsList.Add(new RunNotification(RunNotificationTier.Regular, (errorLimbedFilesQty + " files could not be saved to the Limbo of Branch \"" + oldBranchName + "\". This is possibly because " +
                                                                                               "they were being used by another process, no longer exists, or there was an I/O error.")));
                //If have errors when unlimbing the files, send a notification
                if (errorUnlimbedFilesQty > 0)
                    runNotificationsList.Add(new RunNotification(RunNotificationTier.Regular, (errorUnlimbedFilesQty + " files could not be retrieved from the Limbo of Branch \"" + currentBranchName + "\". This is possibly " +
                                                                                               "due to conflicts with already existing files in current Branch, or an I/O error. Files that remained in Limbo were discarded.")));
                //If have leaked files from last Branch, send a notification
                if (lastBranchLeakedFilesPath.Length > 0)
                    foreach (string filePath in lastBranchLeakedFilesPath)
                        if (filePath != null && filePath != "")
                            runNotificationsList.Add(new RunNotification(RunNotificationTier.Critical, ("The file \"" + filePath + "\" exists in the current Branch, but it is not tracked by Git. It may be a leaked file from " +
                                                                                                        "the previous Branch, \"" + oldBranchName + "\".")));
                //If have Branches existing only on the local repository
                if (branchesOnLocalOnly.Length > 0)
                    foreach (string branchName in branchesOnLocalOnly)
                        if (branchName != null && branchName != "")
                            runNotificationsList.Add(new RunNotification(RunNotificationTier.Warning, ("The Branch \"" + branchName + "\" only exists in your Local Repository, on your machine. Perhaps you haven't published " +
                                                                                                       "it yet, or if it also existed in the Remote Repository, it was deleted.")));
                //If have Branches existing only on the remote repository
                if (branchesOnRemoteOnly.Length > 0)
                    foreach (string branchName in branchesOnRemoteOnly)
                        if (branchName != null && branchName != "")
                            runNotificationsList.Add(new RunNotification(RunNotificationTier.Warning, ("The Branch \"" + branchName + "\" only exists in the Remote Repository. Perhaps you haven't yet fetched it for the " +
                                                                                                       "Local Repository, or you deleted it Locally and forgot to delete it Remotely.")));

                //Update the logs display
                UpdateLogsDisplay();

                //If don't have logs...
                if (runNotificationsList.Count == 0)
                    CloseApplication();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        public void UpdateLogsDisplay()
        {
            //If don't have logs to display, stop here
            if (runNotificationsList.Count == 0)
                return;

            //Render the logs
            foreach (RunNotification notification in runNotificationsList)
            {
                //Create the log item to display
                LogItem newLogItem = new LogItem(this);
                logsList.Children.Add(newLogItem);
                instantiatedLogItems.Add(newLogItem);
                //Configure it
                newLogItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                newLogItem.VerticalAlignment = VerticalAlignment.Top;
                newLogItem.Width = double.NaN;
                newLogItem.Height = double.NaN;
                newLogItem.Margin = new Thickness(0, 0, 0, 8);
                //Inform the data about the log
                newLogItem.SetLogIcon(notification.tier);
                newLogItem.SetMessage(notification.message);
            }

            //Enable the logs panel
            logsPanel.Visibility = Visibility.Visible;
            windowRoot.Width = LOGS_EXPAND_WINDOW_TO_SIZE;
            progressSliderFx.Width = LOGS_EXPAND_SLIDERFX_TO_SIZE;
            progressSliderFx.Visibility = Visibility.Collapsed;
            this.Title = "Smart Branch Switch";

            //Setup the dismiss button
            dismissBtn.Click += (s, e) => { CloseApplication(); };

            //Start the timer Coroutine
            IDisposable autoCloseTimerRoutine = Coroutine.Start(AutoCloseTimer());
        }

        private IEnumerator AutoCloseTimer()
        {
            //Prepare the information
            int totalSeconds = 30;
            int remaingSeconds = totalSeconds;
            bool highlightBlinkEnabled = false;

            //Force scroll view to down
            logsScrollView.ScrollToEnd();

            //Enter the loop of seconds update
            while (true)
            {
                //Update the button text
                dismissBtn.Content = ("Dismiss (" + remaingSeconds + ")");
                //If was reached zero, break the loop
                if (remaingSeconds <= 0)
                    break;
                //Wait 1 seconds
                yield return new WaitForSeconds(1.0f);
                //Decrease the seconds
                remaingSeconds -= 1;
                //Change the highlight blink state
                highlightBlinkEnabled = !highlightBlinkEnabled;
                //Update the highlight blink state
                if (highlightBlinkEnabled == false)
                    highlightBlink.Visibility = Visibility.Collapsed;
                if (highlightBlinkEnabled == true)
                    highlightBlink.Visibility = Visibility.Visible;
                //Force scroll view to down, if is the first second
                if (remaingSeconds == (totalSeconds - 1))
                    logsScrollView.ScrollToEnd();
            }

            //Close the application
            CloseApplication();
        }

        public void CloseApplication()
        {
            //Inform that can close the program
            canCloseProgram = true;
            //Stop the execution of this instance
            Application.Current.Shutdown();
        }

        //Private auxiliar methods

        private void SendLog(string message)
        {
            //Send a normal log on Console
            Console.WriteLine(message);
            //Send debug log
            Debug.WriteLine(message);
            //Append the message on log file
            File.WriteAllText(currentRunLogPath, (File.ReadAllText(currentRunLogPath) + "\n" + DateTime.Now.ToString("HH:mm:ss") + " - " + message));
        }

        private string RunGitCommandAndGetResponse(string commandWithArguments)
        {
            //Prepare the response to return
            string toReturn = "";

            //Inform in the log
            SendLog("Running Git command: \"git " + commandWithArguments + "\".");

            //Prepare the Process Start Info
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "git";
            processStartInfo.Arguments = commandWithArguments;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.WorkingDirectory = currentRepositoryRootPath;

            //Start the process with "using" to ensure that resources of this process is cleaned in any scenario. Be a crash, or a normal execution
            using (Process gitProcess = Process.Start(processStartInfo))
            {
                //If the process is null, stop here
                if (gitProcess == null)
                {
                    //Show the error
                    SendLog("Impossible to start the Git process.");
                    //Warn in log
                    SendLog("Stopping execution...");
                    //Throw a exception
                    throw new Exception("Error on running a command of Git.");
                }

                //Prepare the task of reading error asynchronous. This is useful to keep the "error output" buffer cleared, while this thread is reading the "standard output" buffer
                Task<string> readErrorTask = gitProcess.StandardError.ReadToEndAsync();
                //Keep reading the standard output
                string standardOutput = gitProcess.StandardOutput.ReadToEnd();
                //Store the standard error result
                string standardError = readErrorTask.Result;

                //Wait the git process finishes
                gitProcess.WaitForExit();

                //Inform the response
                toReturn = standardOutput;

                //If have a error, stop the program
                if (gitProcess.ExitCode != 0)
                {
                    //Show the error
                    SendLog("Git command error: " + standardError.Replace("\r", "").Replace("\n", " ").Trim());
                    //Warn in log
                    SendLog("Stopping execution...");
                    //Throw a exception
                    throw new Exception(("Error on running a command of Git. Exit Code: " + gitProcess.ExitCode + "."));
                }
            }

            //Return the response
            return toReturn;
        }

        private string GetStringArray(string[] elements)
        {
            //Prepare the value to return
            string toReturn = "[ ";

            //Build the elements of string array
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string item in elements)
            {
                //If not is the first entry being added, add a comma...
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(", ");
                //Add this entry in the array
                stringBuilder.Append(("\"" + item + "\""));
            }
            //Add the builded string, to the value to return
            toReturn += stringBuilder.ToString();

            //Return the value
            return (toReturn + " ]");
        }

        private string GetRawStringArray(string[] elements)
        {
            //Prepare the value to return
            string toReturn = "";

            //Build the elements of string array
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string item in elements)
            {
                //If not is the first entry being added, add a comma...
                if (stringBuilder.Length > 0)
                    stringBuilder.Append("|||");
                //Add this entry in the array
                stringBuilder.Append(item);
            }
            //Add the builded string, to the value to return
            toReturn = stringBuilder.ToString();

            //Return the value
            return toReturn;
        }

        private string GetLimboFolderName(string branchName, string branchNameHashed)
        {
            //Prepare the response
            string toReturn = "";

            //Inform the response
            toReturn = (branchName.Replace("/", "_").Replace("\\", "_") + "-" + branchNameHashed);

            //Return the response
            return toReturn;
        }

        private int DeleteEmptyFoldersAt(string targetDirectoryPath)
        {
            //Prepare the value to return
            int toReturn = 0;

            //Try to run the code...
            try
            {
                //Iterate through all directories of target directory
                foreach (var directory in Directory.GetDirectories(targetDirectoryPath))
                {
                    //Iterate through all subdirectories of the current directory
                    toReturn += DeleteEmptyFoldersAt(directory);

                    //After iterated through all subdirectories of the current directory, check if the current directory is now empty
                    if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                        if (directory.EndsWith(".git", StringComparison.OrdinalIgnoreCase) == false &&
                            directory.Contains(@".git\", StringComparison.OrdinalIgnoreCase) == false &&
                            directory.Contains(".git/", StringComparison.OrdinalIgnoreCase) == false)
                        {
                            //Delete the current directory, not recursively
                            Directory.Delete(directory, false);
                            toReturn += 1;
                        }
                }
            }
            catch (Exception ex) { }

            //Return the value
            return toReturn;
        }
    }
}