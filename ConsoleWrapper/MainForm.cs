using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleWrapper
{
    public partial class MainForm : Form
    {
        private CommandProcessWrapper _commandProcessWrapper=null;
        private const string EndedProcessMessage = "<PROCESS ENDED>";

        private const string ShortcutStringHeader = "NEXT>";
        private const string ShortcutStringSeparator = "|";
        private const string ShortcutDescriptionSeparator = ":";

        private List<Keys> _activeShortcuts = new List<Keys>();

        public MainForm()
        {
            InitializeComponent();
            StartProcess();
        }

        private void StartProcess()
        {
            _commandProcessWrapper = new CommandProcessWrapper(@"C:\programData\Anaconda3\python.exe", @"C:\Users\Luxor\source\repos\ConsoleWrapper\PythonApi\PythonApi.py");

            _commandProcessWrapper.NewLine += AddNewLine;
            _commandProcessWrapper.ProcessEnded += ProcessEnded;
            _commandProcessWrapper.Start();
        }

        private void ProcessEnded(object sender, EventArgs e)
        {
            DisableInputBox();
            ProcessOutputLine(EndedProcessMessage);
        }

        private void DisableInputBox()
        {
            if (InputBox.InvokeRequired) InputBox.Invoke((Action)DisableInputBox);
            InputBox.Text = EndedProcessMessage;
            InputBox.Enabled = false;
        }

        private void AddNewLine(object sender, string e)
        {
            ProcessOutputLine(e);
        }

        private void ProcessOutputLine(string outputLine)
        {
            if (OutputBox.InvokeRequired) OutputBox.Invoke((Action<string>) ProcessOutputLine, outputLine);
            else
            {
                OutputBox.AppendText(outputLine + Environment.NewLine);
                ProcessShortcuts(outputLine);
            }
        }

        private void ProcessShortcuts(string outputLine)
        {
            if (outputLine.StartsWith(ShortcutStringHeader))
            {
                var availableShortctus = outputLine
                    .Substring(ShortcutStringHeader.Length)
                    .Split(Convert.ToChar(ShortcutStringSeparator))
                    .Select(s => s.Trim())
                    .Select(s =>
                    {
                        var parts = s.Split(new[] { ShortcutDescriptionSeparator }, 2, StringSplitOptions.None);
                        return new { Shortcut = parts[0], Description = parts[1] };
                    })
                    .ToList();

                _activeShortcuts = availableShortctus
                    .Select(asc => asc.Shortcut)
                    .Select(s => Enum.Parse(typeof(Keys), s))
                    .Cast<Keys>().ToList();

                Shortcuts.Text = string.Join(Environment.NewLine, availableShortctus.Select(s => $"{s.Shortcut} : {s.Description}"));
            }
            

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _commandProcessWrapper?.Kill();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(this.InputBox.Text))
            {
                OutputBox.AppendText("> " + InputBox.Text + Environment.NewLine);
                _commandProcessWrapper.Send(InputBox.Text);
                InputBox.Text = "";
                e.SuppressKeyPress = true;
            } else if (_activeShortcuts.Contains(e.KeyCode))
            {
                OutputBox.AppendText(ShortcutStringHeader + " " + e.KeyCode + Environment.NewLine);
                _commandProcessWrapper.Send(e.KeyCode.ToString());
                e.SuppressKeyPress = true;
            }
        }
    }
}
