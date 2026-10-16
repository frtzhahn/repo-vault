@echo off
REM Repo Vault LAN setup, run this file as Administrator on the host machine.
REM Reserves the HTTP prefix for the in-process Git server and opens TCP 5050.

netsh http add urlacl url=http://+:5050/ user=Everyone
netsh advfirewall firewall add rule name="RepoVault Git HTTP 5050" dir=in action=allow protocol=TCP localport=5050

echo Done. Start RepoVault.Desktop and share your LAN IP with git clone.
pause
