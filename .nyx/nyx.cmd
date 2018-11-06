@echo off

@powershell -NoProfile -ExecutionPolicy unrestricted -Command "New-Item -ItemType directory -Path .nyx\ -Force; (New-Object System.Net.WebClient).DownloadFile('https://raw.githubusercontent.com/Elders/Nyx/master/.nyx/.nyx.zip','.nyx\.nyx.zip')"
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "Expand-Archive '.nyx\.nyx.zip' -DestinationPath '.nyx\' -Force"
