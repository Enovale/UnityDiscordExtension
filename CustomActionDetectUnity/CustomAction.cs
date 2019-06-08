using Microsoft.Deployment.WindowsInstaller;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DetectUnity
{
    [RunInstaller(true)]
    public partial class DetectUnity : System.Configuration.Install.Installer
    {
        public DetectUnity()
        {
            // STUB
            int processId = Process.GetCurrentProcess().Id;
            string message = string.Format("Please attach the debugger (elevated on Vista or Win 7) to process [{0}].", processId);
            MessageBox.Show(message, "Debug");
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
        }
        // EditorApplication.applicationPath
    }
}