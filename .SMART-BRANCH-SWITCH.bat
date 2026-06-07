::Disable command echoes...
@ECHO OFF
::Enable UTF-8
chcp 65001
::Enable the support for ANSI colors on this Windows CMD script
for /f "tokens=2 delims=[" %%a in ('ver') do for /f "tokens=1 delims=]" %%b in ("%%a") do set "winver=%%b"
for /f "tokens=1,2 delims=." %%a in ("%winver%") do set "major=%%a" & set "minor=%%b"
::Defines the invisible ESC character
for /f %%a in ('echo prompt $E ^| cmd') do set "ESC=%%a"
::Set the formation variables
set "GRAY=%ESC%[90m"
set "RED=%ESC%[91m"
set "GREEN=%ESC%[92m"
set "YELLOW=%ESC%[93m"
set "BLUE=%ESC%[94m"
set "CYAN=%ESC%[96m"
set "RESET=%ESC%[0m"
::Clear the current logs
cls



::Display the title
call :DRAW_TITLE "Running starting checks..."
echo:
::Warn about Git installation check
echo - Checking Git installation on Windows system...
::If the Git is not installed, stop here...
where git >nul 2>nul
if %errorlevel% neq 0 (
    echo - %RED%ERROR!%RESET% It's not possible to find Git installed on your system. Please install it and try
    echo          again. If Git is already installed, try restarting your computer.
    echo:
    pause
    exit /b
)
::Get the installed Git version in safe way
for /f "delims=" %%a in ('"git version"') do set "gitCheckResult=%%a"
set "gitVersionNumber=%gitCheckResult:git version =%"
::Inform the found version of Git
echo - A Git installation was found: "%GRAY%%gitVersionNumber%%RESET%".
::Check if this Batch script is inside of a Git repository folder
echo - Checking Git repository...
::If this Batch script is not in a Git repository folder, stop here
set "DOTGITFOLDER=%~dp0.git"
if exist "%DOTGITFOLDER%\" (
    echo - This is a valid Git repository folder.
) else (
    echo - %RED%ERROR!%RESET% It appears that this Batch script is not running within the root directory of any
    echo          Git repository on your machine. Please place this script in the root directory of
    echo          a Git repository and try again.
    echo:
    pause
    exit /b
)
::Clear the logs until now
cls



::If don't have the Smart Branch Switch installed, go to installation automatically, if is installed, go to Menu
set "START_SBS_EXE_PATH=%~dp0.git\hooks\Smart-Branch-Switch.exe"
if exist "%START_SBS_EXE_PATH%" (
    goto MENU
) else (
    goto INSTALL_OR_UPDATE_SBS
)
::Pause to wait input from user
pause
::Exit from programm, as last step
exit /b




















::--------- Start of MENU block ---------
:MENU
::Clear the old text
cls
::Draw the title
call :DRAW_TITLE "Main Menu"
echo:
echo Please, choose a option number:
echo:
echo 1 - Install/Update Smart Branch Switch in this Local Repository
echo 2 - Read last execution Log
echo 3 - Clear Smart Branch Switch Limbo and ignored files
echo 0 - Uninstall Smart Branch Switch of this Local Repository
echo:
set /P menu_choose=Type the number of choosed option: %=%
::Send the user to desired option
if "%menu_choose%"=="1" goto INSTALL_OR_UPDATE_SBS
if "%menu_choose%"=="2" goto READ_LAST_LOG
if "%menu_choose%"=="3" goto CLEAR_LIMBO_AND_IGNORED_FILES
if "%menu_choose%"=="0" goto UNINSTALL_SBS
::As fallback, send the user back to the menu
goto MENU
::Pause to wait interaction of user
pause
::Exit from programm, as last step
exit /b
::---------- End of MENU block ----------





::--------- Start of INSTALL_OR_UPDATE_SBS block ---------
:INSTALL_OR_UPDATE_SBS
::Clear the old text
cls
::Draw the title
call :DRAW_TITLE "Install"
::---------
::Speaks about the Smart Branch Switch
echo:
echo %CYAN%What is the Smart Branch Switch?%RESET%
echo:
echo Smart Branch Switch is a utility that runs whenever your Local Repository switches the active
echo Branch. It's called by the Git binary that you're using to interact with your Local Repository.
echo After the Branch is switched, Smart Branch Switch executes some hooks, running useful 
echo functions. Among these functions, the main one is to retrieve files ignored by the previous
echo Branch and place them in a Limbo exclusive to that old Branch, keeping your Local Repository
echo clean and organized, maintaining only the files that truly belong to the new active Branch.
echo When you revert to the old Branch, the files in its Limbo are restored, as if nothing had
echo happened. No more ignored files mixing across Branches and contaminating everything!
echo:
echo Smart Branch Switch focuses on the scope of your Git Local Repository. It only runs on your
echo machine, within the scope of your Local Repository, and of course, only if you choose to
echo install it in your Local Repository. Furthermore, when you install it, Git doesn't track it,
echo which means that when you Commit and Push, on your Local Repository, nothing from the Smart
echo Branch Switch goes to the Remote Repository.
echo - Learn more at: %GRAY%https://github.com/marcos4503/smart-branch-switch%RESET%
echo:
::Confirm before continue...
choice /C SN /M "Do you want to install/update the Smart Branch Switch in this Local Repository"
if errorlevel 2 exit /b
::If "Yes", continue...
echo:
echo - Starting installation or update (if already is installed)...
::Check if the Smart Branch Switch already is installed...
set "RUN_GIT_CLEAN=false"
set "INSTALL_SBS_EXE_PATH=%~dp0.git\hooks\Smart-Branch-Switch.exe"
if exist "%INSTALL_SBS_EXE_PATH%" (
    ::If is already installed, warn it
    echo - The Smart Branch Switch already is installed. Updating it...
) else (
    ::If not installed yet, prompt before delete all untracked and ignored files existing on repository
    echo:
    echo %YELLOW%WARNING%RESET%
    echo:
    echo Since the Smart Branch Switch is being installed from scratch in your Local Repository, it is
    echo necessary to delete all ignored files that currently exist in your Local Repository. Without
    echo performing this step, the Smart Branch Switch may have trouble tracking ignored files correctly,
    echo which can affect its operation. This step will be done automatically by this tool, on end of 
    echo the install, and will not affect ANYTHING that is tracked by Git. Before continuing, make sure 
    echo you have committed anything you were working on!
    echo:
    choice /C SN /M "Continue"
    if errorlevel 2 exit /b
    echo:
    set "RUN_GIT_CLEAN=true"
)
::Create the "sbs_install" temporary folder inside the ".git" folder
echo - Preparing temporary files
set "SBS_INSTALL_PATH=%~dp0.git\sbs_install"
if not exist "%SBS_INSTALL_PATH%" (
    mkdir "%SBS_INSTALL_PATH%"
)
::Download the "post-checkout" file
echo - Starting download of Git Bash Script of Hook of Smart Branch Switch...
set "SH_DOWNLOAD_PATH=%~dp0.git\sbs_install\post-checkout"
curl -L -f --progress-bar -o "%SH_DOWNLOAD_PATH%" "https://marcos4503.github.io/smart-branch-switch/Repository-Pages/metadata/windows/post-checkout.sh"
if %errorlevel%==0 (
    echo - Download finished at: "%SH_DOWNLOAD_PATH%"
) else (
    echo - Can't download a file. Please check your internet connection and try again.
    echo:
    pause
    goto INSTALL_OR_UPDATE_SBS_END
)
::Download the "bin-download-url.txt" file
echo - Starting download of Metadata files for Windows...
set "BINURL_DOWNLOAD_PATH=%~dp0.git\sbs_install\bin-url.txt"
curl -L -f --progress-bar -o "%BINURL_DOWNLOAD_PATH%" "https://marcos4503.github.io/smart-branch-switch/Repository-Pages/metadata/windows/bin-download-url.txt"
if %errorlevel%==0 (
    echo - Download finished at: "%BINURL_DOWNLOAD_PATH%"
) else (
    echo - Can't download a file. Please check your internet connection and try again.
    echo:
    pause
    goto INSTALL_OR_UPDATE_SBS_END
)
::Download the "Smart-Branch-Switch.exe" file (the URL is extracted from "bin-download-url.txt" file)
echo - Starting download of Core Binary of Smart Branch Switch for Windows...
set "BINARY_DOWNLOAD_PATH=%~dp0.git\sbs_install\Smart-Branch-Switch.exe"
set "BIN_URL_CONTENT_FILE="
for /f "usebackq delims=" %%A in ("%BINURL_DOWNLOAD_PATH%") do (
    set "BIN_URL_CONTENT_FILE=%%A"
)
curl -L -f --progress-bar -o "%BINARY_DOWNLOAD_PATH%" "%BIN_URL_CONTENT_FILE%"
if %errorlevel%==0 (
    echo - Download finished at: "%BINARY_DOWNLOAD_PATH%"
) else (
    echo - Can't download a file. Please check your internet connection and try again.
    echo:
    pause
    goto INSTALL_OR_UPDATE_SBS_END
)
::Check if the "hooks" folder exists inside ".git" folder
echo - Checking if "hooks" folder exists...
set "DOTGITHOOKSFOLDER=%~dp0.git\hooks"
if exist "%DOTGITHOOKSFOLDER%\" (
    echo - The "hooks" folder was found!
) else (
    echo - Can't install the Smart Branch Switch in this Local Repository.
    echo:
    pause
    goto INSTALL_OR_UPDATE_SBS_END
)
::Move the downloaded files
set "SH_FINAL_PATH=%~dp0.git\hooks\post-checkout"
move /Y "%SH_DOWNLOAD_PATH%" "%SH_FINAL_PATH%"
set "BINARY_FINAL_PATH=%~dp0.git\hooks\Smart-Branch-Switch.exe"
move /Y "%BINARY_DOWNLOAD_PATH%" "%BINARY_FINAL_PATH%"
::Inform that the installation was finished
if exist "%BINARY_FINAL_PATH%" (
    echo:
    echo %CYAN%======================================================================================================%RESET%
    echo %CYAN%Installation/update complete! Switch active Branch in your repository to test the Smart Branch Switch!%RESET%
    echo %CYAN%======================================================================================================%RESET%
    echo:
)
::Delete the untracked or ignored files existing on Repository, if desired
if "%RUN_GIT_CLEAN%"=="true" (
    echo - Deleting untracked and ignored files from Local Repository...
    git clean -d -x -f
)
::Delete the "sbs_install" temporary folder inside the ".git" folder
:INSTALL_OR_UPDATE_SBS_END
echo - Deleting temporary files...
if exist "%SBS_INSTALL_PATH%\" (
    rmdir /S /Q "%SBS_INSTALL_PATH%"
)
echo:
::---------
::Pause to wait interaction of user
pause
::As fallback, send the user back to the menu
goto MENU
::Exit from programm, as last step
exit /b
::---------- End of INSTALL_OR_UPDATE_SBS block ----------





::--------- Start of READ_LAST_LOG block ---------
:READ_LAST_LOG
::Clear the old text
cls
::Draw the title
call :DRAW_TITLE "Read Log"
::---------
::Try to open the log of last run of Smart Branch Switch
set "LOG_PATH=%~dp0.git\hooks\Smart-Branch-Switch.log"
if exist "%LOG_PATH%" (
    echo:
    echo Opening Smart Branch Switch last execution Log...
    echo:
    start "" "%LOG_PATH%"
) else (
    echo:
    echo No Log file were found. It appears the Smart Branch Switch hasn't run yet. Try switching
    echo Branches to run it.
    echo:
)
::---------
::Pause to wait interaction of user
pause
::As fallback, send the user back to the menu
goto MENU
::Exit from programm, as last step
exit /b
::---------- End of READ_LAST_LOG block ----------





::--------- Start of CLEAR_LIMBO_AND_IGNORED_FILES block ---------
:CLEAR_LIMBO_AND_IGNORED_FILES
::Clear the old text
cls
::Draw the title
call :DRAW_TITLE "Clear Limbo And Ignored Files"
::---------
::Show a warning
echo:
echo %YELLOW%WARNING%RESET%
echo:
echo This action will delete all files that the Smart Branch Switch stores in the Limbo of all
echo Branches. It will also delete files that Git ignores or not track in the current Branch.
echo:
choice /C SN /M "Continue"
if errorlevel 2 goto CLEAR_LIMBO_AND_IGNORED_FILES_END
echo:
::Delete the Limbo folder
echo - Deleting Limbo folder...
set "LIMBO_PATH=%~dp0.git\sbs\limbo"
if exist "%LIMBO_PATH%\" (
    rmdir /S /Q "%LIMBO_PATH%"
)
::Delete the untracked or ignored files existing on Repository, if desired
echo - Deleting untracked and ignored files from Local Repository...
git clean -d -x -f
::Show a final space
:CLEAR_LIMBO_AND_IGNORED_FILES_END
echo:
::---------
::Pause to wait interaction of user
pause
::As fallback, send the user back to the menu
goto MENU
::Exit from programm, as last step
exit /b
::---------- End of CLEAR_LIMBO_AND_IGNORED_FILES block ----------





::--------- Start of UNINSTALL_SBS block ---------
:UNINSTALL_SBS
::Clear the old text
cls
::Draw the title
call :DRAW_TITLE "Uninstall"
::---------
::Show a warning
echo:
echo %YELLOW%WARNING%RESET%
echo:
echo This action will delete all files that the Smart Branch Switch stores in the Limbo of all
echo Branches and it cannot be undone. You can always reinstall the Smart Branch Switch using
echo this script whenever you prefer.
echo:
choice /C SN /M "Continue"
if errorlevel 2 goto UNINSTALL_SBS_END
echo:
::Delete "sbs" folder of Smart Branch Switch...
echo - Deleting Smart Branch Switch data...
set "SBS_DATA_PATH=%~dp0.git\sbs"
if exist "%SBS_DATA_PATH%\" (
    rmdir /S /Q "%SBS_DATA_PATH%"
)
::Delete the Git Hooks
echo - Deleting Smart Branch Switch Git Bash Hook...
set "SBS_SH_SCRIPT=%~dp0.git\hooks\post-checkout"
if exist "%SBS_SH_SCRIPT%" (
    del /F /Q "%SBS_SH_SCRIPT%"
)
::Delete the Log
echo - Deleting Smart Branch Switch Log...
set "SBS_LOG=%~dp0.git\hooks\Smart-Branch-Switch.log"
if exist "%SBS_LOG%" (
    del /F /Q "%SBS_LOG%"
)
::Delete the Core Binary
echo - Deleting Smart Branch Switch Core Binary...
set "SBS_BIN=%~dp0.git\hooks\Smart-Branch-Switch.exe"
if exist "%SBS_BIN%" (
    del /F /Q "%SBS_BIN%"
)
::Inform that the uninstall was finished
echo:
echo %RED%===================%RESET%
echo %RED%Uninstall finished!%RESET%
echo %RED%===================%RESET%
echo:
pause
exit /b
::Show a final space
:UNINSTALL_SBS_END
echo:
::---------
::Pause to wait interaction of user
pause
::As fallback, send the user back to the menu
goto MENU
::Exit from programm, as last step
exit /b
::---------- End of UNINSTALL_SBS block ----------




















::---------- Script methods ----------
::Prevent this script from running the methods below directly
exit /b

::Start of DRAW_TITLE method (arg1 = subtitle)
:DRAW_TITLE
    set "ARG1=%~1"
    echo ===========================================================================================
    echo =                                   Smart Branch Switch                                   =
    echo ===========================================================================================
    echo - %ARG1%
goto :eof
::End of DRAW_TITLE method

::------------------------------------