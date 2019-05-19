using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuuInterpretator
{
    class Executer : IDisposable
    {
        InnerView mInnerView;
        Stack<StackTraceItem> mStackTrace;
        OutputMessage mOutputMessage;
        CancellationToken mCancelToken;
        int mCurrentInstNo;
        string mCurrentFunc;
        private Instruction mCurrentInst;

        public Executer(String sourceCode)
        {
            mInnerView = new InnerViewBuilder(sourceCode).Build();
            if (!mInnerView.Functions.ContainsKey("main"))
                throw new Exception("main is missing.");
            mStackTrace = new Stack<StackTraceItem>();
            mCurrentInstNo = 0;
            mCurrentFunc = "main";
            mCurrentInst = mInnerView.Functions[mCurrentFunc][mCurrentInstNo];
            mOutputMessage = new OutputMessage();
        }

        public string Execute(CancellationToken cancelToken)
        {
            mOutputMessage.Output = "";
            mCancelToken = cancelToken;
            PushNewFuncIntoStack("main");

            try
            {
                ExecuteFuncion("main");

                mCancelToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                mOutputMessage.Output += "Cancel\n";
            }
            catch (Exception e)
            {
                mOutputMessage.Output += $"{e.Message}\n";
            }
           return mOutputMessage.Output;
           
        }

        private void ExecuteFuncion(string funcName)
        {
            foreach (var inst in mInnerView.Functions[funcName])
            {
                ExecuteInstruction(inst);
            }
             mStackTrace.Pop();
        }

        private void PushNewFuncIntoStack(string funcName, int currInstNo = -1)
        {
            if (mStackTrace.Count >= 3000)
            {
                throw new Exception("StackOverflowExeption");
            }
            mStackTrace.Push(
                new StackTraceItem
                {
                    FuncName = funcName,
                    LineNo = mInnerView.Functions[funcName].First().LineNo,
                    InstNo = currInstNo
                }
            );
        }

        private void ExecuteInstruction(Instruction inst)
        {
            Thread.Sleep(500);
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

        private void ExecuteCall(Instruction inst)
        {
            CallInst callInst = (CallInst)inst;
            PushNewFuncIntoStack(callInst.CallerFunc);
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
            mOutputMessage.Output += printInst.Variable
                + "=" + mInnerView.Variables[printInst.Variable] + "\n";
        }

        public OutputMessage ExecuteNextInst(bool stepInto, CancellationToken cancellationToken)
        {
            mCancelToken = cancellationToken;
            try
            {
                if (stepInto && mCurrentInst.Type == InstType.CALL)
                {
                    CallInst callInst = (CallInst)mCurrentInst;
                    PushNewFuncIntoStack(callInst.CallerFunc, mCurrentInstNo);
                    mCurrentFunc = callInst.CallerFunc;
                    mCurrentInstNo = -1;
                }
                else
                {
                    ExecuteInstruction(mCurrentInst);
                }
                mCurrentInstNo++;
                if (mInnerView.Functions[mCurrentFunc].Count <= mCurrentInstNo)
                {
                    if (mStackTrace.Peek().FuncName != "main")
                    {
                        mCurrentInstNo = mStackTrace.Pop().InstNo + 1;
                        mCurrentFunc = mStackTrace.Peek().FuncName;
                    }
                    else
                    {
                        mOutputMessage.LineNo = -1;
                        return mOutputMessage;
                    }
                }
                mCurrentInst = mInnerView.Functions[mCurrentFunc][mCurrentInstNo];
                StackTraceItem stackItem = mStackTrace.Pop();
                stackItem.LineNo = mCurrentInst.LineNo;
                mStackTrace.Push(stackItem);

                string stackTrace = "";
                foreach (var item in mStackTrace)
                {
                    stackTrace += item.ToString();
                }

                string variables = "";
                foreach (var item in mInnerView.Variables)
                {
                    variables += $"{item.Key} = {item.Value}\n";
                }
                mOutputMessage.LineNo = mCurrentInst.LineNo;
                mOutputMessage.StackTrace = stackTrace;
                mOutputMessage.Variables = variables;
            }
            catch (OperationCanceledException)
            {
                mOutputMessage.Output += "Cancel\n";
            }
            catch (Exception e)
            {
                mOutputMessage.Output += $"{e.Message}\n";
            }
            return mOutputMessage;
        }

        public OutputMessage FindMain()
        {
            PushNewFuncIntoStack("main");
            return new OutputMessage { LineNo = mCurrentInst.LineNo };
        }
        public void Dispose()
        {
            mStackTrace.Clear();
            mInnerView.Functions.Clear();
            mInnerView.Variables.Clear();
        }
    }

    public class OutputMessage
    {
        public string Output { get; set; }
        public string StackTrace { get; set; }
        public string Variables { get; set; }
        public int LineNo { get; set; }
    }

    internal class StackTraceItem
    {
       public string FuncName { get; set; }
       public int InstNo { get; set; }
       public int LineNo { get; set; }

        public override string ToString()
        {
            return $"{FuncName}, {LineNo}\n";
        }
    }
}
