using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuuInterpreter 
{

    public enum LexTypes
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

    public class Lex
    {
        public LexTypes Type { get; set; }
        public string Value { get; set; }
        public int LineNo { get; set; }

        public override string ToString()
        {
            return "<" + Type.ToString() + "; " + Value + "; " + LineNo + ">";
        }
    }

    public class Lexer
    {
        //хранение исходного текста по словам
        private string[][] mSourseCodeByWord;
        int mCurrentString;
        int mCurrentWord;

        public Lexer(string sourceCode)
        {
            sourceCode = sourceCode.Replace("\r\n", "\n");
            string[] strs = sourceCode.Split('\n');
           
            mSourseCodeByWord = new string[strs.Length][];
            int i = 0;
            foreach (string str in strs)
            {
                mSourseCodeByWord[i++] = str.Split(' ')
                    .Where(it => !String.IsNullOrEmpty(it))
                    .ToArray();
            }

            mCurrentWord = 0;
            mCurrentString = 0;
        }

        //полечение лексемы
        public Lex GetLex()
        {
            Lex lex = GetNewLex();
            if (lex == null)
            {
                for (int i = 0; i < mSourseCodeByWord.Length; ++i)
                {
                    mSourseCodeByWord[i] = null;
                }
                mSourseCodeByWord = null;
            }
            return lex;
        }

        //построение лексемы и проверка условий
        private Lex GetNewLex()
        {
            if (mSourseCodeByWord == null)
                return null;
            if (mCurrentString >= mSourseCodeByWord.Length)
                return null;
            if (mCurrentString == mSourseCodeByWord.Length
                && mCurrentWord >= mSourseCodeByWord.Last().Length)
                return null;
            if (mCurrentWord >= mSourseCodeByWord[mCurrentString].Length)
            {
                mCurrentString++;
                mCurrentWord = 0;

                return new Lex
                {
                    Type = LexTypes.ENDLINE,
                    Value = "\n",
                    LineNo = mCurrentString
                };
            }
            while (mSourseCodeByWord[mCurrentString].Length == 0)
            {
                mCurrentString++;
                mCurrentWord = 0;
                if (mCurrentString >= mSourseCodeByWord.Length)
                    return null;

            }

            Lex lex = BuildNewLex();

            mCurrentWord++;

            return lex;
        }

        //построение лексемы по слову
        private Lex BuildNewLex()
        {
            Lex lex = new Lex();

            string word = mSourseCodeByWord[mCurrentString][mCurrentWord];
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
                    }
                    else if (isIdent(word))
                    {
                        lex.Type = LexTypes.INDENT;
                    }
                    else
                    {
                        lex.Type = LexTypes.ERROR;
                        throw new Exception("Unexpected lexem " + word +
                            " in line " + (mCurrentString + 1) + ".");
                    }

                    break;
            }
            lex.Value = word;
            lex.LineNo = mCurrentString + 1;
            return lex;
        }

        //является ли слово идентификатором
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

        //является ли слово числом
        private bool isDigit(string word)
        {
            int n = 0;

            if (String.IsNullOrEmpty(word))
                return false;

            return Int32.TryParse(word, out n);
        }
    }
}
