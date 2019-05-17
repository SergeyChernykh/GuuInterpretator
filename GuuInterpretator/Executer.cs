using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuuInterpretator
{
    class Executer
    {
        InnerView mInnerView;
        Stack<Range> mStackTrace;
        String mOutput;
        CancellationToken mCancelToken;

        public Executer(InnerView innerView)
        {
            mInnerView = innerView;
            mStackTrace = new Stack<Range>();
        }

        public string Execute(CancellationToken cancelToken)
        {
            if (!mInnerView.Functions.ContainsKey("main"))
                throw new Exception("main is missing.");
            mCancelToken = cancelToken;
            try
            {
                ExecuteFuncion("main");

                mCancelToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                mOutput += "Cancel\n";
            }
            catch (Exception e)
            {
                mOutput += $"{e.Message}\n";
            }
           return mOutput;
           
        }

        private void ExecuteFuncion(string funcName)
        {
            try
            {
                mStackTrace.Push(
                    new Range
                    {
                        FuncName = funcName,
                        LineNo = mInnerView.Functions[funcName].First().LineNo
                    }
                );
            }
            catch(Exception e)
            {
                mOutput += $"{e.Message}";
                throw new Exception();
            }

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
                mCancelToken.ThrowIfCancellationRequested();
                
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

            mOutput += printInst.Variable
                + "=" + mInnerView.Variables[printInst.Variable] + "\n";
        }
    }

    internal class Range
    {
       public string FuncName { get; set; }
       public int LineNo { get; set; }
    }
}
