# vs-2017-ftp-upload
## Uploads FTP files directly from Visual Studio 2017

*Adds an FTP Command menu item to VS 2017 Solution Explorer that lets you FTP individual files to a Web Server*

You can modify the code to do anything really.

1. Install NcFTP ftp://ftp.ncftp.com/ncftp/binaries/Setup%20NcFTP%203.2.6.msi (or find binary here - https://www.ncftp.com/download/ )
1. Download this and run in VS 2017 or get the VISX package in the root that might work.
1. Create a ftp.ext.settings as a plain text file at the root of the solution that maps with your FTP location

Syntax is each of these on a new line (FTP Details)\
\
[server]\
[username]\
[password]\
[remote folder]


So for example, if you put that file in

c:\dev\myproject\ with a /www/myproject/ as the remote folder

and you edit c:\dev\myproject\js\main.js in Visual Studio it will upload to /www/myproject/js/



If you are still not sure read the code here, this is the meat of the implementation - https://github.com/garazy/vs-2017-ftp-upload/blob/82dddb54e36c911e30bffbcd746477657389966f/FTPUpload/FTPCommand.cs#L113
