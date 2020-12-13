using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows;
using NLog;

namespace XoClock
{
    internal class PipeServer
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        private MainViewModel _viewModel;

        public PipeServer(MainViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public void OpenPort()
        {
            var pipeServer = new NamedPipeServerStream(PipeConst.XOCLOCK_PIPE_NAME, PipeDirection.InOut);
            _log.Debug("Waiting for client connection...");
            pipeServer.WaitForConnection();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            _log.Debug("Client connected on thread[{0}].", threadId);
            try
            {
                // Read the request from the client. Once the client has
                // written to the pipe its security token will be available.
                StreamString ss = new StreamString(pipeServer);
                // Verify our identity to the connected client using a
                // string that the client anticipates.
                ss.WriteString(PipeConst.XOCLOCK_SERVER_ID);
                string command = ss.ReadString();
                // Display the name of the user we are impersonating.
                _log.Debug("received command: '{0}' on thread[{1}] as user: {2}.",
                                    command, threadId, pipeServer.GetImpersonationUserName());
                DispatchToModel(command);
            }
            catch (IOException e)
            {
                _log.Debug("ERROR: {0}", e.Message);
            }
            pipeServer.Close();
            pipeServer.Dispose();
        }

        private void DispatchToModel(string command)
        {
            if (command == XoClockCommand.START_CHRONO.ToString())
            {
                _viewModel.StartChrono();
            }
            if (command == XoClockCommand.STOP_CHRONO.ToString())
            {
                _viewModel.StopChrono();
            }
            if (command == XoClockCommand.RESET_CHRONO.ToString())
            {
                _viewModel.ResetChrono();
            }
            if (command == XoClockCommand.MODE_CHRONO.ToString())
            {
                _viewModel.SetMode(ClockMode.Chronometer);
            }
            if (command == XoClockCommand.MODE_CLOCK.ToString())
            {
                _viewModel.SetMode(ClockMode.Clock);
            }
            if (command == XoClockCommand.KILL.ToString())
            {
                Application.Current.Dispatcher.InvokeShutdown();
            }
        }
    }
}