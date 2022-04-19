using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using NLog;

namespace InterProcess
{
    public class CommandDispatcher
    {
        public event EventHandler<CommandReceivedEventArgs> CommandReceived;
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        NamedPipeServerStream _pipeServer;

        public CommandDispatcher()
        {
        }

        public bool Listen()
        {
            bool ret = true;
            try
            {
                _pipeServer = new NamedPipeServerStream(PipeConst.XOCLOCK_PIPE_NAME, PipeDirection.InOut);
                _log.Debug($"Pipe [{PipeConst.XOCLOCK_PIPE_NAME}] is open: you can control via cmd line (refer to UserGuide.txt)");
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                ret = false;
            }
            return ret;
        }

        public bool HandleClient()
        { 
            _log.Debug("Waiting for client connection...");
            _pipeServer.WaitForConnection();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            _log.Debug("Client connected on thread[{0}].", threadId);
            try
            {
                var streamString = new StreamString(_pipeServer);
                streamString.WriteString(PipeConst.XOCLOCK_SERVER_ID);
                string command = streamString.ReadString();
                string user = _pipeServer.GetImpersonationUserName();
                _log.Debug($"Rx cmd: '{command}' by user: {user}");
                OnCommandReceived(command);
            }
            catch (IOException e)
            {
                _log.Debug("ERROR: {0}", e.Message);
            }
            return true;
        }

        private void OnCommandReceived(string command)
        {
            if (CommandReceived != null)
            {
                CommandReceived(this, new CommandReceivedEventArgs(command));
            }
        }

        public bool Close()
        {
            _pipeServer.Close();
            _pipeServer.Dispose();
            return true;
        }
    }
}