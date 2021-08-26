@echo off
echo starting copy       
timeout 5 
echo start copy 
xcopy /s /e /y /c "%temp%" "%localDir%"
echo end copy
timeout 3
start %launchpadExecutable%