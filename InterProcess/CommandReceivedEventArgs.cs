using System;

namespace InterProcess
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public string Command { get; }

        public CommandReceivedEventArgs(string command)
        {
            Command = command;
        }
    }
}