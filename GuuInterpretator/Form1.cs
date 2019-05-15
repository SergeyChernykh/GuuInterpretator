using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuuInterpretator
{
    public partial class Form1 : Form
    {
        TextWriter _writer = null;

        public Form1()
        {
            InitializeComponent();

            SourceCodeFastColoredTextBox.Text = "sub main\n\tset a 1\n\tcall foo\n\tprint a\nsub foo\n\tset a 2";
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            OutputRichTextBox.Clear();
            //Lexer lexer = new Lexer(SourceCodeFastColoredTextBox.Text);
            //Lex lex = lexer.GetLex();
            //while (lex != null)
            //{
            //    OutputRichTextBox.Text += lex.ToString() + '\n';
            //    lex = lexer.GetLex();
            //}

            try
            {
                InnerViewBuilder innerViewBuilder = new InnerViewBuilder(SourceCodeFastColoredTextBox.Text);
                InnerView innerView = innerViewBuilder.Build();
                Executer executer = new Executer(OutputRichTextBox, innerView);
                executer.Execute();
            }
            catch (Exception exp)
            {
                OutputRichTextBox.Text = exp.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _writer = new TextBoxStreamWriter(OutputRichTextBox);
            Console.SetOut(_writer);
        }
    }
}
