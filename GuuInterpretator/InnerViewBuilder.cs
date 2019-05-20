using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuuInterpreter 
{
    public class InnerView
    {
        public Dictionary<string, int> Variables { get; set; }
        public Dictionary<string, Instruction[]> Functions { get; set; }

        public InnerView()
        {
            Variables = new Dictionary<string, int>();
            Functions = new Dictionary<string, Instruction[]>();
        }
    }

    class InnerViewBuilder
    {
        //объект выдающий лексемы
        private Lexer mLexer;
        private Lex mCurrentLex;

        private string mCurrentFunction;

        //если переменная или функция была использована, 
        //но еще не определена, то она попадает в этот список
        private List<string> ExpectToDefVars { get; set; }
        private List<string> ExpectToDefFuncs { get; set; }

        private List<Instruction> Instructions { get; set; }

        private InnerView mInnerView;
        
        public InnerViewBuilder(string sourceCode)
        {
            mLexer = new Lexer(sourceCode);
            ExpectToDefVars = new List<string>();
            ExpectToDefFuncs = new List<string>();
            mInnerView = new InnerView();
            Instructions = new List<Instruction>();

        }

        //запуск рекурсивного спуска
        public InnerView Build()
        {
            mCurrentLex = mLexer.GetLex();
            while (mCurrentLex != null)
            {
                FuncDefenition();
            }

            //проверка на то, что все переменные и функции объявлены
            CheckExpectedDefList(ExpectToDefVars);
            CheckExpectedDefList(ExpectToDefFuncs);
            return mInnerView;
        }

        //обработка заголовка функции
        private void FuncDefenition()
        {
            CheckLexType(mCurrentLex, LexTypes.SUB);

            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (mInnerView.Functions.ContainsKey(mCurrentLex.Value))
            {
                throw new Exception("Redeclare function " 
                    + mCurrentLex.Value + ".");
            }
            mInnerView.Functions.Add(mCurrentLex.Value, null);
            mCurrentFunction = mCurrentLex.Value;

            if (ExpectToDefFuncs.Contains(mCurrentLex.Value))
                ExpectToDefFuncs.Remove(mCurrentLex.Value);

            EndLine();

            FuncBody();
        }

        //обработка тела функции
        private void FuncBody()
        {
            while (mCurrentLex != null && mCurrentLex.Type != LexTypes.SUB)
            {
                switch (mCurrentLex.Type)
                {
                    case LexTypes.CALL:
                        Call();
                        break;

                    case LexTypes.PRINT:
                        Print();
                        break;

                    case LexTypes.SET:
                        Set();
                        break;

                    case LexTypes.ENDLINE:
                        mCurrentLex = mLexer.GetLex();
                        break;

                    default:
                        throw new Exception(
                           SyntaxisErrorMessageInOper(mCurrentLex)
                        );
                }
            }
            if (Instructions.Count == 0)
            {
                throw new Exception($"{mCurrentFunction}'s body is empty.");
            }
            else
            {
                mInnerView.Functions[mCurrentFunction] = Instructions.ToArray();
                Instructions.Clear();
            }
        }

        //обработка инструкции set
        private void Set()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            string varName = mCurrentLex.Value;

            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.DIGIT);
            int val = Int32.Parse(mCurrentLex.Value);

            if (!mInnerView.Variables.ContainsKey(varName))
            {
                mInnerView.Variables.Add(varName,0);
                if (ExpectToDefVars.Contains(varName))
                    ExpectToDefVars.Remove(varName);
            }

           Instructions.Add(new SetInst(varName, val, mCurrentLex.LineNo));

            EndLine();
        }

        //обработка инструкции print
        private void Print()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (!mInnerView.Variables.ContainsKey(mCurrentLex.Value))
            {
                if (!ExpectToDefVars.Contains(mCurrentLex.Value))
                    ExpectToDefVars.Add(mCurrentLex.Value);
            }

            Instructions.Add(new PrintInst(mCurrentLex.Value, mCurrentLex.LineNo));

            EndLine();
        }

        //обработка инструкции call
        private void Call()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (!mInnerView.Functions.ContainsKey(mCurrentLex.Value))
            {
                if (!ExpectToDefFuncs.Contains(mCurrentLex.Value))
                    ExpectToDefFuncs.Add(mCurrentLex.Value);
            }

            Instructions.Add(new CallInst(mCurrentLex.Value, mCurrentLex.LineNo));

            EndLine();
        }

        //обработка конца строки
        private void EndLine()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.ENDLINE);
            mCurrentLex = mLexer.GetLex();
        }

        //проверка списка, на то что все переменные или функции объявлены объявлены
        private void CheckExpectedDefList(List<String> list)
        {
            if (list.Count != 0)
            {
                string names = "";
                foreach (string name in list)
                {
                    names += name + (name == list.Last() ? "" : ", ");
                }
                string type;
                if (list == ExpectToDefFuncs)
                    type = "Function(s)";
                else
                    type = "Variable(s)";
                throw new Exception(type + ": " + names
                    + "doesn't(don't) exist in current context.");
            }
        }

        //проверка типа лексемы
        private void CheckLexType(Lex lex, LexTypes type)
        {
            if (lex.Type != type)
            {
                throw new Exception(SyntaxisErrorMessage(lex, type));
            }
        }

        //построение сообщения о ошибке
        private string SyntaxisErrorMessageInOper(Lex lex)
        {
            return $"Syntaxis error in line {lex.LineNo}.\n" 
                + "Expected SET, CALL or PRINT, but get " +
               lex.Type.ToString() + ".";
        }

        //построение сообщения о ошибке
        private string SyntaxisErrorMessage(Lex lex, LexTypes expect)
        {
            return $"Syntaxis error in line {lex.LineNo}.\n"
                + "Expected " + expect.ToString() + ", but get " 
                + lex.Type.ToString() + ".";
        }
    }


}
