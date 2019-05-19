using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuuInterpretator
{
    public class InnerView
    {
        public Dictionary<string, int> Variables { get; set; }
        public Dictionary<string, List<Instruction>> Functions { get; set; }

        public InnerView()
        {
            Variables = new Dictionary<string, int>();
            Functions = new Dictionary<string, List<Instruction>>();
        }
    }

    class InnerViewBuilder
    {
        private Lexer mLexer;
        private Lex mCurrentLex;

        private string mCurrentFunction;

        private List<string> ExpectToDefVars { get; set; }
        private List<string> ExpectToDefFuncs { get; set; }

        private InnerView mInnerView;
        
        public InnerViewBuilder(string sourceCode)
        {
            mLexer = new Lexer(sourceCode);
            ExpectToDefVars = new List<string>();
            ExpectToDefFuncs = new List<string>();
            mInnerView = new InnerView();

        }

        public InnerView Build()
        {
            mCurrentLex = mLexer.GetLex();
            while (mCurrentLex != null)
            {
                FuncDefenition();
            }

            CheckExpectedDefList(ExpectToDefVars);
            CheckExpectedDefList(ExpectToDefFuncs);
            return mInnerView;
        }


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
            mInnerView.Functions.Add(mCurrentLex.Value, new List<Instruction>());
            mCurrentFunction = mCurrentLex.Value;

            if (ExpectToDefFuncs.Contains(mCurrentLex.Value))
                ExpectToDefFuncs.Remove(mCurrentLex.Value);

            EndLine();

            FuncBody();
        }

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
            if (mInnerView.Functions[mCurrentFunction].Count == 0)
            {
                throw new Exception($"{mCurrentFunction}'s body is empty.");
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

            if (!mInnerView.Variables.ContainsKey(varName))
            {
                mInnerView.Variables.Add(varName,0);
                if (ExpectToDefVars.Contains(varName))
                    ExpectToDefVars.Remove(varName);
            }

            mInnerView.Functions[mCurrentFunction]
                .Add(new SetInst(varName, val, mCurrentLex.LineNo));

            EndLine();
        }

        private void Print()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (!mInnerView.Variables.ContainsKey(mCurrentLex.Value))
            {
                if (!ExpectToDefVars.Contains(mCurrentLex.Value))
                    ExpectToDefVars.Add(mCurrentLex.Value);
            }

            mInnerView.Functions[mCurrentFunction]
                .Add(new PrintInst(mCurrentLex.Value, mCurrentLex.LineNo));

            EndLine();
        }

        private void Call()
        {
            mCurrentLex = mLexer.GetLex();
            CheckLexType(mCurrentLex, LexTypes.INDENT);
            if (!mInnerView.Functions.ContainsKey(mCurrentLex.Value))
            {
                if (!ExpectToDefFuncs.Contains(mCurrentLex.Value))
                    ExpectToDefFuncs.Add(mCurrentLex.Value);
            }

            mInnerView.Functions[mCurrentFunction]
                .Add(new CallInst(mCurrentLex.Value, mCurrentLex.LineNo));

            EndLine();
        }

        private void EndLine()
        {
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


}
