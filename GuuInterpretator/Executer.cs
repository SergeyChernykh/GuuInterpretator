using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuuInterpretator
{
    class Executer
    {
        RichTextBox mOutputRichTextBox;
        InnerView mInnerView;
        Stack<Range> mStackTrace;


        public Executer(RichTextBox output, InnerView innerView)
        {
            mOutputRichTextBox = output;
            mInnerView = innerView;
            mStackTrace = new Stack<Range>();
        }

        public void Execute()
        {
            if (!mInnerView.Functions.ContainsKey("main"))
                throw new Exception("main is missing.");
            ExecuteFuncion("main");
            
        }

        private void ExecuteFuncion(string funcName)
        {
            mStackTrace.Push(
                new Range {
                    FuncName = funcName,
                    LineNo = mInnerView.Functions[funcName].First().LineNo
                }
            );

            foreach (var inst in mInnerView.Functions[funcName])
            {
                switch (inst.Type)
                {
                    case InstType.PRINT:
                        ExecutePrint(inst);
                        break;

                    case InstType.SET:
                        ExecuteSet(inst);
                        break;

                    case InstType.CALL:
                        ExecuteCall(inst);
                        break;
                    default:
                        throw new Exception("Unexpected instruction.");
                }
            }

            mStackTrace.Pop();
        }

        private void ExecuteCall(Instruction inst)
        {
            CallInst callInst = (CallInst)inst;
            ExecuteFuncion(callInst.CallerFunc);
        }

        private void ExecuteSet(Instruction inst)
        {
            SetInst setInst = (SetInst)inst;
            mInnerView.Variables[setInst.Variable] = setInst.Value;
        }

        private void ExecutePrint(Instruction inst)
        {
            PrintInst printInst = (PrintInst)inst;
            Console.Write(printInst.Variable 
                + "=" + mInnerView.Variables[printInst.Variable]+"\n");
        }
    }

    internal class Range
    {
       public string FuncName { get; set; }
       public int LineNo { get; set; }
    }
}
