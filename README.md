# vs-2017-ftp-upload
## Uploads FTP files directly from Visual Studio 2017

*Adds an FTP Command menu item to VS 2017 Solution Explorer that lets you FTP individual files to a Web Server*

You can modify the code to do anything really.

1. Install NcFTP ftp://ftp.ncftp.com/ncftp/binaries/Setup%20NcFTP%203.2.6.msi (or find binary here - https://www.ncftp.com/download/ )
1. Download this and run in VS 2017 or get the VISX package in the root that might work.
1. Create a ftp.ext.settings at the root of the solution that maps with your FTP location

Syntax is\
\
[server]\
[username]\
[password]\
[remoteFolder]


So if you put that file in

c:\dev\myproject\ with a folder of /www/myproject/ as the remote folder

and you edit c:\dev\myproject\js\main.js in Visual Studio it will upload to /www/myproject/js/




