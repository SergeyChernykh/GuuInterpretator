using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuuInterpretator
{
    public partial class Form1 : Form
    {
        CancellationTokenSource TokenSource;
        Executer Executer;
        Style lineStyle = new TextStyle(Brushes.Black, Brushes.Green, FontStyle.Bold);
        Range PreviusRange;

        public Form1()
        {
            InitializeComponent();

            SourceCodeFastColoredTextBox.Text = "sub main\n\tset a 1\n\tcall foo\n\tprint a\nsub foo\n\tset a 2";
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            OutputRichTextBox.Clear();

            try
            {
                using (Executer = new Executer(SourceCodeFastColoredTextBox.Text))
                {
                    using (TokenSource = new CancellationTokenSource())
                    {
                        StopButton.Enabled = true;
                        RunButton.Enabled = false;
                        CancellationToken cancelToken = TokenSource.Token;
                        OutputRichTextBox.Text += await Task.Run(() => Executer.Execute(cancelToken), cancelToken);
                        StopButton.Enabled = false;
                        RunButton.Enabled = true;
                    }
                    TokenSource = null;
                }
            }
            catch (Exception exp)
            {
                OutputRichTextBox.Text += exp.Message;
            }

        }

        private void StepRunButton_Click(object sender, EventArgs e)
        {
            try
            {
                OutputRichTextBox.Clear();
                SourceCodeFastColoredTextBox.Enabled = false;
                StepRunButton.Enabled = false;
                RunButton.Enabled = false;
                StopButton.Enabled = true;
                Executer = new Executer(SourceCodeFastColoredTextBox.Text);
                StepIntoButton.Enabled = true;
                StepOverButton.Enabled = true;
                int line = Executer.FindMain().LineNo - 1;
                ChangeLineColor(line);
            }
            catch (Exception exp)
            {
                OutputRichTextBox.Text += exp.Message;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            TokenSource?.Cancel();
            EndOfStepExecute();
        }

        private void StepIntoButton_Click(object sender, EventArgs e)
        {
            OneStep(true);
        }

        private void StepOverButton_Click(object sender, EventArgs e)
        {
            OneStep(false);
        }

        private async void OneStep(bool stepInto)
        {
            try
            {
                using (TokenSource = new CancellationTokenSource())
                {
                    CancellationToken cancelToken = TokenSource.Token;
                    OutputMessage message = await Task.Run(() => Executer.ExecuteNextInst(stepInto, cancelToken), cancelToken);

                    int line = message.LineNo - 1;
                    ChangeLineColor(line);
                    OutputRichTextBox.Text += message.Output;
                    StackTraceRichTextBox.Text = message.StackTrace;
                    VariablesRichTextBox.Text = message.Variables;
                    if (line < 0)
                    {
                        EndOfStepExecute();
                    }
                }
                TokenSource = null;
            }
            catch (Exception e)
            {
                OutputRichTextBox.Text += e.Message;
                EndOfStepExecute();
            }
        }

        private void EndOfStepExecute()
        {
            ChangeLineColor(-1);
            StepIntoButton.Enabled = false;
            StepOverButton.Enabled = false;
            StopButton.Enabled = false;
            StepRunButton.Enabled = true;
            RunButton.Enabled = true;
            SourceCodeFastColoredTextBox.Enabled = true;
            Executer.Dispose();
            return;
        }

        private void ChangeLineColor(int line)
        {
            PreviusRange?.ClearStyle(lineStyle);
            if (line >= 0)
            {
                PreviusRange = SourceCodeFastColoredTextBox.GetLine(line);
                PreviusRange.SetStyle(lineStyle);
            }
        }


    }
}
