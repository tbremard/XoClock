﻿using InterProcess;
using System;
using System.Windows;

namespace XoClock
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App():base()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 3)
            {
                if (args[1] == "/c")
                {
                    string command = args[2];
                    Execute(command);
                    Shutdown();
                }
            }
        }

        private void Execute(string commandText)
        {
            string upperCommand = commandText.Trim().ToUpper();
            if (RemoteProcess.Instance.Connect())
            {
                RemoteProcess.Instance.SendCommand(upperCommand);
            }
        }
    }
}
