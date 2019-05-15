using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuuInterpretator
{
    public enum InstType
    {
        CALL,
        PRINT,
        SET
    }

    public abstract class Instruction
    {
        public InstType Type { get; set; }
        public int LineNo { get; set; }
    }

    public class CallInst : Instruction
    {
        public string CallerFunc { get; set; }

        public CallInst(string callerFunc, int lineNo)
        {
            Type = InstType.CALL;
            CallerFunc = callerFunc;
            LineNo = lineNo;
        }
    }

    public class PrintInst : Instruction
    {
        public string Variable { get; set; }

        public PrintInst(string variable, int lineNo)
        {
            Type = InstType.PRINT;
            Variable = variable;
            LineNo = lineNo;
        }
    }

    public class SetInst : Instruction
    {
        public string Variable { get; set; }
        public int Value { get; set; }

        public SetInst(string variable, int value, int lineNo)
        {
            Type = InstType.SET;
            Variable = variable;
            Value = value;
            LineNo = lineNo;
        }
    }

}
