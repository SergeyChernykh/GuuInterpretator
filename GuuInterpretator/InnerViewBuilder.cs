using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuuInterpretator
{
    class InnerView
    {
        private Lexer mLexer;
        private Lex mCurrentLex;

        public List<string> Variables { get; set; }
        public List<string> ExpectToDefVars { get; set; }
        public Dictionary<string, List<Instruction>> Functions { get; set; }
        public List<string> ExpectToDefFuncs { get; set; }
        
        public InnerView(string sourceCode)
        {
            mLexer = new Lexer(sourceCode);
            Variables = new List<string>();
            ExpectToDefVars = new List<string>();

            Functions = new Dictionary<string, List<Instruction>>();
            ExpectToDefFuncs = new List<string>();

            mCurrentLex = mLexer.GetLex();
            while (mCurrentLex != null)
            {
                FuncDefenition();
            }

            CheckExpectedDefList(ExpectToDefVars);
            CheckExpectedDefList(ExpectToDefFuncs);

        }


        private void FuncDefenition()
        {
            CheckLexType(mCurrentLex, LexTypes.SUB);

            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (Functions.ContainsKey(mCurrentLex.Value))
            {
                throw new Exception("Redeclare function " 
                    + mCurrentLex.Value + ".");
            }
            Functions.Add(mCurrentLex.Value, new List<Instruction>());
            if (ExpectToDefFuncs.Contains(mCurrentLex.Value))
                ExpectToDefFuncs.Remove(mCurrentLex.Value);

            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.ENDLINE);
            mCurrentLex = mLexer.GetLex();

            FuncBody();
        }

        private void FuncBody()
        {
            if (mCurrentLex == null || mCurrentLex.Type == LexTypes.SUB)
                throw new Exception("Function body is empty.");

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
        }

        private void Set()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            string varName = mCurrentLex.Value;

            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.DIGIT);
            int val = Int32.Parse(mCurrentLex.Value);

            if (!Variables.Contains(varName))
            {
                Variables.Add(varName);
                if (ExpectToDefVars.Contains(varName))
                    ExpectToDefVars.Remove(varName);
            }

            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.ENDLINE);
            mCurrentLex = mLexer.GetLex();
        }

        private void Print()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (!Variables.Contains(mCurrentLex.Value))
            {
                if (!ExpectToDefVars.Contains(mCurrentLex.Value))
                    ExpectToDefVars.Add(mCurrentLex.Value);
            }
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.ENDLINE);
            mCurrentLex = mLexer.GetLex();
        }

        private void Call()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (!Functions.ContainsKey(mCurrentLex.Value))
            {
                if (!ExpectToDefFuncs.Contains(mCurrentLex.Value))
                    ExpectToDefFuncs.Add(mCurrentLex.Value);
            }

            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.ENDLINE);
            mCurrentLex = mLexer.GetLex();
        }



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
                    + " don't(doesn't) exist in current context.");
            }
        }

        private void CheckLexType(Lex lex, LexTypes type)
        {
            if (lex.Type != type)
            {
                throw new Exception(SyntaxisErrorMessage(lex, type));
            }
        }

        private string SyntaxisErrorMessageInOper(Lex lex)
        {
            return "Syntaxis error in line " + lex.LineNo + ".\n" 
                + "Expected SET, CALL or PRINT, but get " +
               lex.Type.ToString() + ".";
        }

        private string SyntaxisErrorMessage(Lex lex, LexTypes expect)
        {
            return "Syntaxis error in line " + lex.LineNo + ".\n" 
                + "Expected " + expect.ToString() + ", but get " 
                + lex.Type.ToString() + ".";
        }
    }

    public class Instruction
    {
    }
}
