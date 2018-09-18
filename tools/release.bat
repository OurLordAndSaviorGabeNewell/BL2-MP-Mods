@ECHO OFF

set bintray-key=

call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"

cd ../

IF NOT EXIST Patcher.sln (
	echo Project file is invalid or missing!
	EXIT 2
)

set /p version="Version: "
set /p patchnotes="Patchnote: "

git diff --exit-code
IF ERRORLEVEL NOT 0 (
	CHOICE /M "You have uncommitted changed. Do you want to commit and push any changes first"
	IF ERRORLEVEL NOT 1 Goto SkipGit

	git add *
	set /p commitmessage="Commit message: "
	git commit -a -m %commitmessage%
	git push
)

:SkipGit

msbuild Patcher.sln /p:Configuration=Release /p:VersionNumber=%version%
IF ERRORLEVEL NOT 0 (
	echo The project failed to build!
	EXIT 1
)

curl -T src\bin\Release\CoopPatcherV%version%.zip -urobeth:%bintray-key% https://api.bintray.com/content/robeth/BL2-MP-Mods/BL2-MP-Mods/%version%/