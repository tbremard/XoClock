using NLog;
using System;
using System.IO.Pipes;
using System.Security.Principal;

namespace XoClock
{
    public class PipelineClientService
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        public static PipelineClientService Instance = new PipelineClientService();
        NamedPipeClientStream _pipeClient;
        StreamString _stream;
        public bool IsConnected = false;

        private PipelineClientService()
        {
            //Singleton
        }
        public bool ConnectToServer()
        {
            _pipeClient = new NamedPipeClientStream(PipeConst.LOCAL_SERVER_NAME, PipeConst.XOCLOCK_PIPE_NAME,
                                                    PipeDirection.InOut, PipeOptions.None,
                                                    TokenImpersonationLevel.Impersonation);
            int timeout = 1000;
            try
            {
                _log.Debug("Connecting to server...\n");
                _pipeClient.Connect(timeout);
                _stream = new StreamString(_pipeClient);
            }
            catch (Exception)
            {
                return false;
            }
            if (_stream.ReadString() == PipeConst.XOCLOCK_SERVER_ID)
            {
                IsConnected = true;
                return true;
            }
            else
            {
                _log.Debug("Server could not be verified.");
                return false;
            }
        }

        public bool SendCommand(string cmd)
        {
            if (!IsConnected)
                return false;
            // The client security token is sent with the first write.
            _stream.WriteString(cmd);
            return true;
        }

        public bool Disconnect()
        {
            _pipeClient.Close();
            IsConnected = false;
            return true;
        }
    }
}