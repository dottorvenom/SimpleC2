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