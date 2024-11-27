<--------------------------------------------------------------------------------Step One-------------------------------------------------------------------------------->
Clone the repo to the local project folder



<--------------------------------------------------------------------------------Step Two-------------------------------------------------------------------------------->
Update the Visual Studio from VS Installer to LTS version



<--------------------------------------------------------------------------------Step Three-------------------------------------------------------------------------------->
Update or install the project required SDK version from the Windows SDK store online



<--------------------------------------------------------------------------------Step Four-------------------------------------------------------------------------------->
If {you} have not done the code first migration before then run this command to install the EF Core CLI Tool globally
dotnet tool install --global dotnet-ef
-----------------------------------------------------------
if you have it, then update to the latest one
dotnet tool update --global dotnet-ef



<--------------------------------------------------------------------------------Step Five-------------------------------------------------------------------------------->
Chnage the  app setting details
1. DB name
2. Login/password
3. Gmail Pass Key
4. Sender and Receiver Emails

