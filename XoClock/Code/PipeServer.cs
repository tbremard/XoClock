using System;
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
        private TimerModel _viewModel;

        NamedPipeServerStream _pipeServer;

        public PipeServer(TimerModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public bool OpenPort()
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
                _log.Debug("Rx cmd: '{0}' on thread[{1}] by user: {2}",
                                    command, threadId, _pipeServer.GetImpersonationUserName());
                DispatchToModel(command);
            }
            catch (IOException e)
            {
                _log.Debug("ERROR: {0}", e.Message);
            }
            return true;
        }

        public bool Close()
        {
            _pipeServer.Close();
            _pipeServer.Dispose();
            return true;
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
                _viewModel.SetMode(ClockMode.Chrono);
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