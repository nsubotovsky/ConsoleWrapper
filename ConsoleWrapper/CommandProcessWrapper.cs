using System;
using System.Diagnostics;
using System.Threading;

namespace ConsoleWrapper
{
    public class CommandProcessWrapper : IDisposable
    {
        private Process _process;
        private readonly ProcessStartInfo _processStartInfo;

        public event EventHandler ProcessEnded;
        public event EventHandler<string> NewLine;

        public bool IsProcessRunning { get; private set; }

        public CommandProcessWrapper(string commandLine, string arguments)
        {
            _processStartInfo = new ProcessStartInfo
            {
                FileName = commandLine,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            IsProcessRunning = false;
        }


        public void Start()
        {
            IsProcessRunning = true;
            _process = Process.Start(_processStartInfo);
            PrepareProcess();
        }

        private void PrepareProcess()
        {
            _process.OutputDataReceived += DataReceived;
            _process.Exited += OnProcessEnded;

            _process.BeginOutputReadLine();
            _process.EnableRaisingEvents = true;
        }


        private void OnProcessEnded(object sender, EventArgs e)
        {
            IsProcessRunning = false;
            DissociateEvents();
            _process.Dispose();
            _process = null;
            if (ProcessEnded != null) ProcessEnded(sender, EventArgs.Empty);
        }

        private void DissociateEvents()
        {
            _process.OutputDataReceived -= DataReceived;
            _process.Exited -= OnProcessEnded;
        }


        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                OnNewLine(e.Data);
            }
        }

        private void OnNewLine(string newLine)
        {
            if (NewLine != null) NewLine(this, newLine);
        }


        public void Kill()
        {
            if (_process != null)
            {
                if (!_process.HasExited) _process.Kill();
                OnProcessEnded(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            Thread.Sleep(100);
            Kill();
        }

        public void Send(string text)
        {
            _process.StandardInput?.WriteLineAsync(text);
        }
    }
}