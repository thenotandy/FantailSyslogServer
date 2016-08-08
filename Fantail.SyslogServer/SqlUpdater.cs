///
/// Fantail.SyslogServer
/// ====================
/// 
/// Fantail Technology Ltd ( www.fantail.net.nz )
/// 
/// Designed by Chris Guthrey & David Husselmann
/// Developed by David Husselmann for Fantail Technology Ltd
///
/// chris@fantail.net.nz
/// david@tamix.com
/// 
/// Copyright (c) 2007, Fantail Technology Ltd
/// 
/// All rights reserved.
/// 
/// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
/// 
///     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
///     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
///     * Neither the name of Fantail Technology Ltd nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
/// 
/// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
/// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
/// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
/// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
/// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
/// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
/// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
/// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
/// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
/// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
/// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE
/// 
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Fantail.Libraries.Syslog;
using System.Threading;

namespace Fantail.Libraries.Syslog {
  /// <summary>
  /// Takes care of inserting SyslogMessage records into a database.
  /// </summary>
  public class SqlUpdater : IDisposable {
    private const string CHECK_SCHEMA_SQL = @"
declare @err varchar(2000)

if not exists(select 1 from sysobjects where name=@TableName and xtype='U') begin
  set @err = 'A table by the name '+isnull(@TableName,'NULL')+' does not exist in catalog '+DB_NAME()+''
  raiserror(@err,16,1)
  return
end

if exists(
	select 1
	from (
	  select 'Id' col, 'int' typ
	  union select 'Facility', 'int'
	  union select 'Severity', 'int'
	  union select 'Timestamp', 'datetime'
	  union select 'Hostname', 'varchar'
	  union select 'Message', 'varchar'
	) cols
	left join information_schema.columns ic on cols.col=ic.column_name and cols.typ=ic.data_type and ic.table_name=@TableName
	where ic.column_name is null
) begin
  set @err = 'The table schema does not conform to the recommended structure.
The following SQL will create it for you:

CREATE TABLE '+quotename(@TableName)+' (
  Id int identity(1,1), Facility int, Severity int, Timestamp datetime,
  Hostname varchar(255), Message varchar(1024)
)
'
  raiserror(@err,16,2)
end
";

    private SqlConnection conn;
    private string tableName;

    public SqlUpdater(string connectionString, string tableName) {
      this.tableName = tableName;
      conn = new SqlConnection(connectionString);

      //try opening the connection a couple times, if we're in a boot process
      int tries = 10;
      do {
        conn.Open();
        if (tries != 10) Thread.Sleep(10000);
      } while (tries-- > 0 && conn.State != System.Data.ConnectionState.Open);

      using (SqlCommand cmd = new SqlCommand(CHECK_SCHEMA_SQL, conn)) {
        try {
          cmd.Parameters.AddWithValue("@TableName", tableName);
          cmd.ExecuteNonQuery();
        } catch (Exception ex) {
          throw new Exception("Error checking the schema for the message table.", ex);
        }
      }
    }

    /// <summary>
    /// Submits the given syslog message to the database table.
    /// </summary>
    /// <param name="sm">The syslog message to save.</param>
    /// <returns>True if the operation was successful.</returns>
    public bool SaveMessage(SyslogMessage sm) {
      List<SyslogMessage> l = new List<SyslogMessage>();
      l.Add(sm);
      return SaveMessage(l);
    }

    /// <summary>
    /// Submits the given syslog messages to the database table in a single transaction.
    /// </summary>
    /// <param name="syslogMessages">The syslog messages to save.</param>
    /// <returns>True if the operation was successful.</returns>
    public bool SaveMessage(List<SyslogMessage> syslogMessages) {
      //DFH Note that this method gets called a lot, so be careful about
      //disposing of stuff properly.

      if (syslogMessages.Count == 0) return true;

      StringBuilder sb = new StringBuilder();
      try {
        //build the sql string
        //Yep, it defies belief that using this rather than parameters is actually faster.
        foreach (SyslogMessage sm in syslogMessages) {
          sb.AppendFormat("INSERT INTO [" + tableName + "] (Facility, Severity, Timestamp, Hostname, Message) VALUES ({0},{1},'{2}','{3}','{4}')\n",
            sm.Facility, sm.Severity, sm.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff"),
            sm.Hostname.Replace("'", "''"),
            sm.Message.Replace("'", "''")
          );
        }

        //check the connection
        if (!PrepareForCommand()) return false;

        //submit the sql
        using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn)) {
          try {
            cmd.ExecuteNonQuery();
          } catch (Exception ex) {
            DoExceptionThrown(ex);
            return false;
          }
        }

      } finally {
        sb = null;
      }

      //if we get here, everything worked.
      return true;
    }

    private bool PrepareForCommand() {
      if (conn.State != System.Data.ConnectionState.Open) {
        //broken connection?  try resetting it
        try {
          conn.Close();
        } catch (Exception) {
        }

        try {
          conn.Open();
        } catch (Exception ex) {
          //error trying to open the connection; we better terminate.
          DoExceptionThrown(new Exception("Error trying to reopen a failed SQL connection.",ex));
          return false;
        }
      }
      return true;
    }

    private void DoExceptionThrown(Exception e) {
      if (ExceptionThrown != null) {
        ExceptionThrown(new ExceptionThrownEventArgs(e));
      } else {
        throw e;
      }
    }

    /// <summary>
    /// This event gets invoked if an exception is thrown while talking to the database.
    /// </summary>
    public event ExceptionThrownEventHandler ExceptionThrown;

    /// <summary>
    /// Called on disposing of this class.  This will close the SqlConnection if it is still open.
    /// </summary>
    public void Dispose() {
      if (conn.State != System.Data.ConnectionState.Closed) {
        try {
          conn.Close();
        } catch (Exception) {
        }
      }
    }
  }

  public delegate void ExceptionThrownEventHandler(ExceptionThrownEventArgs e);
  /// <summary>
  /// Encapsulates the exception thrown by internal code.
  /// </summary>
  public class ExceptionThrownEventArgs : EventArgs {
    private Exception exception;

    public Exception Exception {
      get { return exception; }
    }
    public ExceptionThrownEventArgs(Exception exception) {
      this.exception = exception;
    }
  }
}
