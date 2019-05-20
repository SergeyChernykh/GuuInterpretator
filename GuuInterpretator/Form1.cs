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

namespace GuuInterpreter 
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
        }

        //выполнение программы
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
                        //нужен для остановки выполнения программы
                        CancellationToken cancelToken = TokenSource.Token;
                        //запускаем выполнение программы в отдельной задаче
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

        //запуск пошагового выполнения программы
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

                //выделяем первую строку в main
                ChangeLineColor(Executer.FindMain().LineNo - 1);
            }
            catch (Exception exp)
            {
                OutputRichTextBox.Text += exp.Message;
            }
        }

        //шаг с заходом
        private void StepIntoButton_Click(object sender, EventArgs e)
        {
            OneStep(true);
        }

        //шаг с обходом
        private void StepOverButton_Click(object sender, EventArgs e)
        {
            OneStep(false);
        }

        //выполнение шага
        private async void OneStep(bool stepInto)
        {
            try
            {
                using (TokenSource = new CancellationTokenSource())
                {
                    //нужен для остановки выполнения программы
                    CancellationToken cancelToken = TokenSource.Token;
                    //запускаем выполнение инструкции в отдельной задаче
                    OutputMessage message = await Task.Run(() => Executer.ExecuteCurrentInst(stepInto, cancelToken), cancelToken);

                    //выделение следующей инструкции
                    int line = message.LineNo - 1;
                    ChangeLineColor(line);
                    
                    //вывод информации
                    OutputRichTextBox.Text = message.Output;
                    StackTraceRichTextBox.Text = message.StackTrace;
                    VariablesRichTextBox.Text = message.Variables;

                    
                    if (line < 0)
                    {
                        //была выполнена последняя инструкция
                        EndExecute();
                        Executer.Dispose();
                    }
                }
                TokenSource = null;
            }
            catch (Exception e)
            {
                OutputRichTextBox.Text += e.Message;
                EndExecute();
                Executer.Dispose();
            }
        }

        //остановка выполнения
        private void StopButton_Click(object sender, EventArgs e)
        {
            TokenSource?.Cancel();
            EndExecute();
        }

        //остановка выполнения
        private void EndExecute()
        {
            ChangeLineColor(-1);
            StepIntoButton.Enabled = false;
            StepOverButton.Enabled = false;
            StopButton.Enabled = false;
            StepRunButton.Enabled = true;
            RunButton.Enabled = true;
            SourceCodeFastColoredTextBox.Enabled = true;
            return;
        }

        //выделение инструкции
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
