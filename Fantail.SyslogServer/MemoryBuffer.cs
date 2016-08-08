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
using Fantail.Libraries.Syslog;
using System.Timers;
using System.Threading;

namespace Fantail.Libraries.Syslog {
  /// <summary>
  /// This class takes care of buffering incoming syslog messages in memory
  /// and submitting them to the SqlUpdater in batches, rather than keeping the database
  /// occupied with drip-fed messages.
  /// </summary>
  public class MemoryBuffer {
    private List<SyslogMessage> buffer = new List<SyslogMessage>();
    private TimeSpan commitInterval;
    private System.Timers.Timer timer = new System.Timers.Timer();
    private SqlUpdater sqlUpdater;
    private Mutex bufferLock = new Mutex(false);

    /// <summary>
    /// Creates a new instance of the MemoryBuffer class.
    /// </summary>
    /// <param name="commitInterval">Specifies the interval at which message batches should be committed to the database.</param>
    /// <param name="sqlUpdater">The pre-initialised SqlUpdater to which messages should be submitted.</param>
    public MemoryBuffer(TimeSpan commitInterval, SqlUpdater sqlUpdater) {
      this.commitInterval = commitInterval;
      this.timer.Interval = commitInterval.TotalMilliseconds;
      this.sqlUpdater = sqlUpdater;
      this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
      this.timer.Enabled = true;
    }

    void timer_Elapsed(object sender, ElapsedEventArgs e) {
      Flush();
    }

    /// <summary>
    /// Posts a new message to the buffer, for committing to the database when the internal timer elapses.
    /// </summary>
    /// <param name="sm">The syslog message to be submitted.</param>
    public void PushMessage(SyslogMessage sm) {
      bufferLock.WaitOne();
      buffer.Add(sm);
      bufferLock.ReleaseMutex();
    }

    /// <summary>
    /// Flushes the buffer.  This method can be called by the user to ensure that final in-memory entries are
    /// written to the database.
    /// </summary>
    public void Flush() {
      bufferLock.WaitOne();
      try {
        if (sqlUpdater.SaveMessage(buffer)) {
          buffer.Clear();
        }
      } finally {
        bufferLock.ReleaseMutex();
      }
    }
  }
}
