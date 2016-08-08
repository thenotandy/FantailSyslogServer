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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using Fantail.Libraries.Syslog;
using System.Net;

namespace Fantail.SyslogServer {
  public partial class SyslogService : ServiceBase {

      /// <summary>
      ///   This is where all the action really starts!
      /// 
      /// The sequence of events is as follows:
      /// 
      /// Create the SyslogListener object - this will listen on port 514 for syslog messages
      /// Create the SqlUpdate object - this will save messages to your SQL database
      /// 
      /// NOTE that you need to edit the new SqlUpdater for your database settings
      /// 
      /// The MemoryBuffer holds the messages recieved by the listener and then calls the SQL update
      /// in batches, so reduce load on the SQL server
      /// 
      /// Setup event handlers for receiving new syslog messages and for handling SQL exceptions.
      /// 
      /// and so on with starting the Fantail.SyslogServer service!
      /// 
      /// </summary>
    public SyslogService() {
      InitializeComponent();
    }

    private SyslogListener syslogListener;
    private SqlUpdater sqlUpdater;
    private MemoryBuffer memoryBuffer;

    protected override void OnStart(string[] args) {
      syslogListener = new SyslogListener(IPAddress.Any);

        /// 
        /// the next line needs to be edited.  instead of hardcoding these details into the application
        /// it would be better to store them in an XML config file or something...
        /// 
      sqlUpdater = new SqlUpdater("server=<your-server>;user=<db-user>;password=<db-password>;initial catalog=<db-name>", "your-table-name");
      
        
      memoryBuffer = new MemoryBuffer(TimeSpan.FromSeconds(30), sqlUpdater);

      syslogListener.MessageReceived += new MessageReceivedEventHandler(delegate(MessageReceivedEventArgs e) {
        memoryBuffer.PushMessage(e.SyslogMessage);
      });

      sqlUpdater.ExceptionThrown += new ExceptionThrownEventHandler(delegate(ExceptionThrownEventArgs e) {
        EventLog.WriteEntry(EventLog.Source, e.Exception.ToString());
      });

      syslogListener.Start();
    }

    protected override void OnStop() {
      syslogListener.Stop();
      memoryBuffer.Flush();
    }
  }
}
