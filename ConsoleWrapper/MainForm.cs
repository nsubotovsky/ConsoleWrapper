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

        public MainForm()
        {
            InitializeComponent();
            StartProcess();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //_commandProcessWrapper.Send(this.InputBox.Text);
        }

        private void StartProcess()
        {
            _commandProcessWrapper = new CommandProcessWrapper(@"C:\programData\Anaconda3\python.exe", @"C:\Users\Luxor\PycharmProjects\dm_textmining\testRead.py");

            _commandProcessWrapper.NewLine += AddNewLine;
            _commandProcessWrapper.ProcessEnded += ProcessEnded;
            _commandProcessWrapper.Start();
        }

        private void ProcessEnded(object sender, EventArgs e)
        {
            DisableInputBox();
            AddLine(EndedProcessMessage);
        }

        private void DisableInputBox()
        {
            if (InputBox.InvokeRequired) InputBox.Invoke((Action)DisableInputBox);
            InputBox.Text = EndedProcessMessage;
            InputBox.Enabled = false;
        }

        private void AddNewLine(object sender, string e)
        {
            AddLine(e);
        }

        private void AddLine(string e)
        {
            if (OutputBox.InvokeRequired) OutputBox.Invoke((Action<string>) (AddLine), new object[] {e});
            else OutputBox.AppendText(e + Environment.NewLine);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _commandProcessWrapper?.Kill();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                this.OutputBox.AppendText( "> " + this.InputBox.Text + Environment.NewLine );
                _commandProcessWrapper.Send(this.InputBox.Text);
                this.InputBox.Text = "";
            }
        }
    }
}
