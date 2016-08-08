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
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

/*
 * Syslog Listener library
 * by David Husselmann
 * for Fantail Technology Ltd
 * 
 * Implements an RFC3164 compliant syslog listener which parses syslog messages
 * sent to any interface the class is bound to.  To use, instantiate the class:
 * 
 *   SyslogListener sl = new SyslogListener(IPAddress.Any);
 * 
 * Attach a suitable event handler using the MessageReceived event:
 * 
 *  sl.MessageReceived += new MessageReceivedEventHandler(delegate(object sender, MessageReceivedEventArgs e) {
 *    Console.WriteLine("Got a message: ------------\n" + e.SyslogMessage.ToString());
 *  });
 * 
 * And finally call the Start() and Stop() methods to control the class:
 * 
 *  sl.Start();
 *  Console.ReadLine();
 *  sl.Stop();
 * 
 */

/* CHANGELOG
 * 20070813 DFH Initial version.
 */

namespace Fantail.Libraries.Syslog {

  /// <summary>
  /// Encapsulates a single syslog message, as received from a remote host.
  /// </summary>
  public struct SyslogMessage {
    /// <summary>
    /// Creates a new instance of the SyslogMessage class.
    /// </summary>
    /// <param name="priority">Specifies the encoded PRI field, containing the facility and severity values.</param>
    /// <param name="timestamp">Specifies the timestamp, if present in the packet.</param>
    /// <param name="hostname">Specifies the hostname, if present in the packet.  The hostname can only be present if the timestamp is also present (RFC3164).</param>
    /// <param name="message">Specifies the textual content of the message.</param>
    public SyslogMessage(int? priority, DateTime timestamp, string hostname, string message) {
      if (priority.HasValue) {
        this.facility = (int)Math.Floor((double)priority.Value / 8);
        this.severity = priority % 8;
      } else {
        this.facility = null;
        this.severity = null;
      }
      this.timestamp = timestamp;
      this.hostname = hostname;
      this.message = message;
    }

    private int? facility;
    /// <summary>
    /// Returns an integer specifying the facility.  The following are commonly used:
    ///       0             kernel messages
    ///       1             user-level messages
    ///       2             mail system
    ///       3             system daemons
    ///       4             security/authorization messages (note 1)
    ///       5             messages generated internally by syslogd
    ///       6             line printer subsystem
    ///       7             network news subsystem
    ///       8             UUCP subsystem
    ///       9             clock daemon (note 2)
    ///      10             security/authorization messages (note 1)
    ///      11             FTP daemon
    ///      12             NTP subsystem
    ///      13             log audit (note 1)
    ///      14             log alert (note 1)
    ///      15             clock daemon (note 2)
    ///      16             local use 0  (local0)
    ///      17             local use 1  (local1)
    ///      18             local use 2  (local2)
    ///      19             local use 3  (local3)
    ///      20             local use 4  (local4)
    ///      21             local use 5  (local5)
    ///      22             local use 6  (local6)
    ///      23             local use 7  (local7)
    /// </summary>
    public int? Facility {
      get { return facility; }
    }

    private int? severity;
    /// <summary>
    /// Returns an integer number specifying the severity.  The following values are commonly used:
    ///       0       Emergency: system is unusable
    ///       1       Alert: action must be taken immediately
    ///       2       Critical: critical conditions
    ///       3       Error: error conditions
    ///       4       Warning: warning conditions
    ///       5       Notice: normal but significant condition
    ///       6       Informational: informational messages
    ///       7       Debug: debug-level messages
    /// </summary>
    public int? Severity {
      get { return severity; }
    }

    private DateTime timestamp;
    /// <summary>
    /// Returns a DateTime specifying the moment at which the event is known to have happened.  As per RFC3164,
    /// if the host does not send this value, it may be added by a relay.
    /// </summary>
    public DateTime Timestamp {
      get { return timestamp; }
    }

    private string hostname;
    /// <summary>
    /// Returns the DNS hostname where the message originated, or the IP address if the hostname is unknown.
    /// </summary>
    public string Hostname {
      get { return hostname; }
      set { hostname = value; }
    }

    private string message;
    /// <summary>
    /// Returns a string indicating the textual content of the message.
    /// </summary>
    public string Message {
      get { return message; }
      set { message = value; }
    }
	
    /// <summary>
    /// Returns a textual representation of the syslog message, for debugging purposes.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
      return "Facility: " + this.facility + "\nSeverity: " + this.severity +
        "\nTimestamp: "+this.timestamp+"\nHostname: "+this.hostname+"\nMessage: "+this.message;
    }
  }

  public delegate void MessageReceivedEventHandler(MessageReceivedEventArgs e);

  public class MessageReceivedEventArgs : EventArgs {
    private SyslogMessage syslogMessage;
    /// <summary>
    /// Returns the syslog message as received from the remote host.
    /// </summary>
    public SyslogMessage SyslogMessage {
      get { return syslogMessage; }
    }
	
    /// <summary>
    /// Creates a new instance of the MessageReceivedEventArgs class.
    /// </summary>
    public MessageReceivedEventArgs(SyslogMessage sm) : base() {
      this.syslogMessage = sm;
    }
  }

  /// <summary>
  /// Implements a syslog message listener which is RFC3164 compliant.
  /// </summary>
  public class SyslogListener {
    private IPAddress listenAddress;
    private Socket sock;
    private const int SYSLOG_PORT = 514;
    private const int RECEIVE_BUFFER_SIZE = 1024;

    private byte[] receiveBuffer = new Byte[RECEIVE_BUFFER_SIZE];
    private EndPoint remoteEndpoint = null;
    private Regex msgRegex = new Regex(@"
(\<(?<PRI>\d{1,3})\>){0,1}
(?<HDR>
  (?<TIMESTAMP>
    (?<MMM>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s
    (?<DD>[ 0-9][0-9])\s
    (?<HH>[0-9]{2})\:(?<MM>[0-9]{2})\:(?<SS>[0-9]{2})
  )\s
  (?<HOSTNAME>
    [^ ]+?
  )\s
){0,1}
(?<MSG>.*)
", RegexOptions.IgnorePatternWhitespace);

    /// <summary>
    /// Creates a new instance of the SyslogListener class.
    /// </summary>
    /// <param name="listenAddress">Specifies the address to listen on.  IPAddress.Any will bind the listener to all available interfaces.</param>
    public SyslogListener(IPAddress listenAddress) {
      this.listenAddress = listenAddress;
      this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    /// <summary>
    /// Starts listening for syslog packets.
    /// </summary>
    public void Start() {
      if (sock.IsBound) return;
      sock.Bind(new IPEndPoint(listenAddress, SYSLOG_PORT));
      SetupReceive();
    }

    private void SetupReceive() {
      remoteEndpoint = new IPEndPoint(IPAddress.None, 0);
      sock.BeginReceiveFrom(receiveBuffer, 0, RECEIVE_BUFFER_SIZE, SocketFlags.None, ref remoteEndpoint, new AsyncCallback(DoReceiveData), sock);
    }

    public event MessageReceivedEventHandler MessageReceived;

    /// <summary>
    /// This internal method processes an async receive as set up by SetupReceive()
    /// </summary>
    private void DoReceiveData(IAsyncResult r) {
      Socket sock = (Socket)r.AsyncState;
      //int count = sock.EndReceive(r, out errorCode);
      EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
      int count = 0;
      try {
        count = sock.EndReceiveFrom(r, ref ep);
      } catch (SocketException) {
        //ignore buffer overruns; .NET handles them.
      } catch (ObjectDisposedException) {
        //if the socket is disposed, we're shutting down, so return
        return;
      }

      string packet = System.Text.Encoding.ASCII.GetString(receiveBuffer, 0, count);

      Match m = msgRegex.Match(packet);
      //ignore invalid messages
      if (m != null && !string.IsNullOrEmpty(packet)) {

        //parse PRI section into priority
        int pri;
        int? priority = int.TryParse(m.Groups["PRI"].Value, out pri) ? new int?(pri) : null;

        //parse the HEADER section - contains TIMESTAMP and HOSTNAME
        string hostname = null;
        Nullable<DateTime> timestamp = null;
        if (!string.IsNullOrEmpty(m.Groups["HDR"].Value)) {
          if (!string.IsNullOrEmpty(m.Groups["TIMESTAMP"].Value)) {
            try {
              timestamp = new DateTime(
                DateTime.Now.Year,
                MonthNumber(m.Groups["MMM"].Value),
                int.Parse(m.Groups["DD"].Value),
                int.Parse(m.Groups["HH"].Value),
                int.Parse(m.Groups["MM"].Value),
                int.Parse(m.Groups["SS"].Value)
                );
            } catch (ArgumentException) {
              //ignore invalid timestamps
            }
          }

          if (!string.IsNullOrEmpty(m.Groups["HOSTNAME"].Value)) {
            hostname = m.Groups["HOSTNAME"].Value;
          }
        }

        if (!timestamp.HasValue) {
          //add timestamp as per RFC3164
          timestamp = DateTime.Now;
        }
        if (string.IsNullOrEmpty(hostname)) {
          IPEndPoint ipe = (IPEndPoint)ep;
          IPHostEntry he = Dns.GetHostEntry(ipe.Address);
          if (he != null && !string.IsNullOrEmpty(he.HostName))
            hostname = he.HostName;
          else
            hostname = ep.ToString();
        }

        string message = m.Groups["MSG"].Value;

        SyslogMessage sm = new SyslogMessage(priority, timestamp.Value, hostname, message);
        if (MessageReceived != null) {
          MessageReceived(new MessageReceivedEventArgs(sm));
        }
      }

      //after we're done processing, ready the socket for another receive.
      SetupReceive();
    }

    /// <summary>
    /// Stops listening and reporting syslog message packets.
    /// </summary>
    public void Stop() {
      if (sock.IsBound) {
        try {
          sock.Close();
        } catch (Exception) { }
      }
    }

    private static int MonthNumber(string monthName) {
      switch (monthName.ToLower().Substring(0,3)) {
        case "jan": return 1;
        case "feb": return 2;
        case "mar": return 3;
        case "apr": return 4;
        case "may": return 5;
        case "jun": return 6;
        case "jul": return 7;
        case "aug": return 8;
        case "sep": return 9;
        case "oct": return 10;
        case "nov": return 11;
        case "dec": return 12;
        default:
          throw new Exception("Unrecognised month name: " + monthName);
      }
    }
  }
}
