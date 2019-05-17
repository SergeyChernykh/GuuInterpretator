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
        CancellationTokenSource _tokenSource;
        public Form1()
        {
            InitializeComponent();

            SourceCodeFastColoredTextBox.Text = "sub main\n\tset a 1\n\tcall foo\n\tprint a\nsub foo\n\tset a 2";
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            OutputRichTextBox.Clear();
            //Lexer lexer = new Lexer(SourceCodeFastColoredTextBox.Text);
            //Lex lex = lexer.GetLex();
            //while (lex != null)
            //{
            //    OutputRichTextBox.Text += lex.ToString() + '\n';
            //    lex = lexer.GetLex();
            //}

            InnerViewBuilder innerViewBuilder = new InnerViewBuilder(SourceCodeFastColoredTextBox.Text);
            InnerView innerView = innerViewBuilder.Build();
            Executer executer = new Executer(innerView);

            _tokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = _tokenSource.Token;

            OutputRichTextBox.Text += await Task.Run(() => executer.Execute(cancelToken), cancelToken);

        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
        }
    }
}
