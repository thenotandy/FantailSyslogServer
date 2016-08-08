Fantail.SyslogServer
====================

Fantail Technology Ltd ( www.fantail.net.nz )

Designed by Chris Guthrey & David Husselmann
Developed by David Husselmann for Fantail Technology Ltd

chris@fantail.net.nz
david@tamix.com

Copyright (c) 2007, Fantail Technology Ltd


This project is by no means fully complete.  Fantail Technology Ltd has released this
project as it is under a BSD-style license so that it may be of help or use to anyone
else needing to build a C# Syslog to SQL service.

The basic steps for building this project are:

1) extract the ZIP file to :

	C:\Projects\Fantail\SyslogServer\SyslogServer 

  If you want to stick it elsewhere, feel free to, but the solution files and such probably need to be updated.


2) Setup an SQL database

   2a) If you don't already have a Microsoft SQL Server handy, you may want to download MSDE or SQLExpress

	http://www.microsoft.com/sql/prodinfo/previousversions/msde/prodinfo.mspx
	or
	http://msdn2.microsoft.com/en-us/express/aa718378.aspx

	Note that I haven't actually tested this code against SQLExpress, but since
	it is all simple stuff, it should just work...


   2b) Create a database, if you can't think of a name for it, called it "FantailSysLog"


   2c) Create a table. Run the following SQL Statement to create the one table (just one!) that you need:

		CREATE TABLE FantailSysLog (
			Id int identity(1,1), 
			Facility int, 
			Severity int, 
			Timestamp datetime,
			Hostname varchar(255), 
			Message varchar(1024)
		)

	(change "FantailSysLog" to a different table name if you wish)


   2d) Create a database user, and grant that user full permissions on your database and table


3) edit the file 

	C:\Projects\Fantail\SyslogServer\SyslogServer\Fantail.SyslogServer\SyslogService.cs 


   and change the line:

      sqlUpdater = new SqlUpdater("server=<your-server>;user=<db-user>;password=<db-password>;initial catalog=<db-name>", "<your-table-name>");

   as follows:
	<your-server> 	this should be the network name or IP address of the machine that hosts your MS SQL Server
			(if you are going to install the Fantail.SyslogService on the same machine as the SQL Server 
			then you can simply set this to "." or "(local)"

	<db-user>	this is the sql database user you created in step 2d) above

	<db-password>	the password you assigned to above user

	<db-name>	the database you created in step 2b)

	<your-table-name>	the name of the table you created in step 2c) above

 	
4) build the project. This was written using Visual Studio 2005.  If you don't have VS2005, you could probably 
   use SharpDevelop without a great deal of modification. I haven't tested this with SharpDevelop, but seeing that the
   code is straightforward, it should just work...

5) install the Service.
   You will need a .NET 2 Utility called "InstallUtil".
   Since the machine that you want to run this service on must already have the .NET2 framework installed, all you simply need do is

   5a) copy your newly built executable "Fantail.SyslogServer.exe" to a location where you want to run it from.  I recommend you 
       create the folder:  C:\Program Files\Fantail
       and copy it there

   5b) open a Command Prompt  (click Start menu, click on "Run...", then enter "CMD" and click the OK button)

   5c) change directory to the location where you copied your executable to, ie:

        CD "\Program Files\Fantail"

   5d) use InstallUtil to install the executable as a Windows Service:

        "C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe" Fantail.SyslogServer.exe

       you should get a message to the effect that the service has been installed successfully.

   5f) time to test it out! type:

          NET START "Fantail Syslog Server"

   5g) if you get an error message, check the eventlog for clues as to what went wrong...

          EVENTVWR


Enjoy!

If I get requested to, I will remove the ugly hard-coded SQL connection details, and move those settings into a Config File.
This means that a generic EXE can be used to talk to any SQL server. 

Otherwise if anyone else cares to tidy up and finish off the rough edges, please send me a copy!

Cheers!
Chris Guthrey - chris@fantail.net.nz
Fantail Technology Ltd
www.fantail.net.nz

===================================================================

Copyright (c) 2007, Fantail Technology Ltd

All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of Fantail Technology Ltd nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE

===================================================================
