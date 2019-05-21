using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace GuuInterpreter 
{
    public class TextBoxStreamWriter : TextWriter
    {
        RichTextBox _output = null;

        public TextBoxStreamWriter(RichTextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            _output.AppendText(value.ToString()); // Когда символ записывается в поток, добавляем его в textbox.
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}