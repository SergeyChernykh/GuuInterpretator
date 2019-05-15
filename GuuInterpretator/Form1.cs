using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuuInterpretator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
                InnerView innerView = new InnerView(SourceCodeFastColoredTextBox.Text);
                OutputRichTextBox.Text = "SUCCESS!!!";
            } 
            catch(Exception exp)
            {
                OutputRichTextBox.Text = exp.Message;
            }




        }

    }
}
