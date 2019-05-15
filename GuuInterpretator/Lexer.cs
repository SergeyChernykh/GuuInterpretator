using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuuInterpretator
{

    enum LexTypes
    {
        SUB,
        SET,
        CALL,
        PRINT,
        INDENT,
        DIGIT,
        ERROR,
        ENDLINE
    }

    class Lex
    {
        public LexTypes Type { get; set; }
        public string Value { get; set; }
        public int LineNo { get; set; }

        public override string ToString()
        {
            return "<" + Type.ToString() + "; " + Value + "; " + LineNo + ">";
        }
    }

    class Lexer
    {
        private string[][] mSourseCodeByStrings;
        int mCurrentString;
        int mCurrentWord;

        public Lexer(string sourceCode)
        {
            sourceCode = sourceCode.Replace("\r\n", "\n");
            string[] strs = sourceCode.Split('\n');
           

            mSourseCodeByStrings = new string[strs.Length][];
            int i = 0;
            foreach (string str in strs)
            {
                mSourseCodeByStrings[i++] = str.Split(' ')
                    .Where(it => !String.IsNullOrEmpty(it))
                    .ToArray();
            }

            mCurrentWord = 0;
            mCurrentString = 0;
        }

        public Lex GetLex()
        {
            if (mSourseCodeByStrings == null)
                return null;
            if (mCurrentString >= mSourseCodeByStrings.Length)
                return null;
            if (mCurrentString == mSourseCodeByStrings.Length
                && mCurrentWord >= mSourseCodeByStrings.Last().Length)
                return null;
            if (mCurrentWord >= mSourseCodeByStrings[mCurrentString].Length)
            {
                mCurrentString++;
                mCurrentWord = 0;

                return new Lex {
                    Type = LexTypes.ENDLINE,
                    Value ="\n",
                    LineNo = mCurrentString
                };
            }
            while (mSourseCodeByStrings[mCurrentString].Length == 0)
            { 
                mCurrentString++;
                mCurrentWord = 0;
                if (mCurrentString >= mSourseCodeByStrings.Length)
                    return null;
                
            }
                

            Lex lex = new Lex();

            string word = mSourseCodeByStrings[mCurrentString][mCurrentWord];
            switch (word)
            {
                case "sub":
                    lex.Type = LexTypes.SUB;
                    break;
                case "set":
                    lex.Type = LexTypes.SET;
                    break;
                case "print":
                    lex.Type = LexTypes.PRINT;
                    break;
                case "call":
                    lex.Type = LexTypes.CALL;
                    break;
                default:
                    if (isDigit(word))
                    {
                        lex.Type = LexTypes.DIGIT;
                    } else if (isIdent(word))
                    {
                        lex.Type = LexTypes.INDENT;
                    }
                    else
                    {
                        lex.Type = LexTypes.ERROR;
                        throw new Exception("Unexpected lexem " + word +
                            " in line " + (mCurrentString+1) +".");
                    }
                
                    break;
            }
            lex.Value = word;
            lex.LineNo = mCurrentString+1;

            mCurrentWord++;
            
            return lex;
        }

        private bool isIdent(string word)
        {
            if (String.IsNullOrEmpty(word))
                return false;
       
            if (Char.IsDigit(word[0]))
            {
                return false;
            }
            foreach (char c in word)
            {
                Char.IsDigit(c);
                if (!Char.IsDigit(c) &&
                    !Char.IsLetter(c) &&
                    c != '_')
                    return false;
            }
            return true;
        }

        private bool isDigit(string word)
        {
            int n = 0;

            if (String.IsNullOrEmpty(word))
                return false;

            return Int32.TryParse(word, out n);
        }
    }
}
