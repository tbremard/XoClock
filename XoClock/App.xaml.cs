using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

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
                }
                this.Shutdown();
            }
        }

        private void Execute(string commandText)
        {
            var commandTable = new Dictionary<string, ICommand>();
            commandTable.Add("STARTCHRONO", new StartChronoCommand());
            string upper = commandText.Trim().ToUpper();
            if(commandTable.ContainsKey(upper))
            {
                ICommand command = commandTable[upper];
                command.Execute(null);
            }
        }
    }
}
