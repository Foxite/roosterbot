@echo off
time /T >> C:\temp\appstart.2.log
RoosterBot\bin\Debug\RoosterBot.exe
echo done >> C:\temp\appstart.2.log
time /T >> C:\temp\appstart.2.log
exit
