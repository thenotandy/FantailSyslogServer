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
namespace Fantail.SyslogServer
{
  partial class ProjectInstaller {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
        this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
        this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
        // 
        // serviceProcessInstaller1
        // 
        this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalService;
        this.serviceProcessInstaller1.Password = null;
        this.serviceProcessInstaller1.Username = null;
        // 
        // serviceInstaller1
        // 
        this.serviceInstaller1.Description = "SYSLOG server which saves log entries to an SQL database.";
        this.serviceInstaller1.DisplayName = "Fantail Syslog Server";
        this.serviceInstaller1.ServiceName = "Fantail.SyslogServer";
        this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        this.serviceInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
        // 
        // ProjectInstaller
        // 
        this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.serviceInstaller1});

    }

    #endregion

    private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
    private System.ServiceProcess.ServiceInstaller serviceInstaller1;
  }
}