using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GuuInterpreter.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void subLex()
        {
            Lexer lexer = new Lexer("sub");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("SUB", lex.Type.ToString());
        }

        [TestMethod]
        public void setLex()
        {
            Lexer lexer = new Lexer("set");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("SET", lex.Type.ToString());
        }

        [TestMethod]
        public void printLex()
        {
            Lexer lexer = new Lexer("print");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("PRINT", lex.Type.ToString());
        }
        [TestMethod]
        public void callLex()
        {
            Lexer lexer = new Lexer("call");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("CALL", lex.Type.ToString());
        }

        [TestMethod]
        public void identLex1()
        {
            Lexer lexer = new Lexer("a");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("INDENT", lex.Type.ToString());
        }

        [TestMethod]
        public void identLex2()
        {
            Lexer lexer = new Lexer("asd2");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("INDENT", lex.Type.ToString());
        }

        [TestMethod]
        public void identLex3()
        {
            Lexer lexer = new Lexer("add_2as");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("INDENT", lex.Type.ToString());
        }

        [TestMethod]
        public void digitLex()
        {
            Lexer lexer = new Lexer("123");
            Lex lex = lexer.GetLex();
            Assert.AreEqual("DIGIT", lex.Type.ToString());
        }

        [TestMethod]
        public void errorLex()
        {
            Lexer lexer = new Lexer("2a");
            try
            {
                Lex lex = lexer.GetLex();
                Assert.AreEqual("ERROR", lex.Type.ToString());
            }
            catch (Exception e)
            {
                Assert.AreEqual("Unexpected lexem 2a in line 1.", e.Message);
            }
        }

        [TestMethod]
        public void innerViewBuildtest1()
        {
            InnerViewBuilder innerViewBuilder = new InnerViewBuilder("sub main\ncall main");
            InnerView innerView = innerViewBuilder.Build();
            bool actual = innerView.Functions.ContainsKey("main");
            Assert.AreEqual(true, actual);
            CallInst inst = (CallInst)innerView.Functions["main"][0];
            Assert.AreEqual("CALL", inst.Type.ToString());
        }

        [TestMethod]
        public void innerViewBuildtest2()
        {
            InnerViewBuilder innerViewBuilder = new InnerViewBuilder("sub main\nset a 2");
            InnerView innerView = innerViewBuilder.Build();
            bool actual = innerView.Functions.ContainsKey("main");
            Assert.AreEqual(true, actual);
            SetInst inst = (SetInst)innerView.Functions["main"][0];
            Assert.AreEqual("SET", inst.Type.ToString());
        }

        [TestMethod]
        public void innerViewBuildtest3()
        {
            InnerViewBuilder innerViewBuilder = new InnerViewBuilder("sub main\nset a 2\nprint a");
            InnerView innerView = innerViewBuilder.Build();
            bool actual = innerView.Functions.ContainsKey("main");
            Assert.AreEqual(true, actual);
            SetInst inst = (SetInst)innerView.Functions["main"][0];
            Assert.AreEqual("SET", inst.Type.ToString());
            PrintInst prInst = (PrintInst)innerView.Functions["main"][1];
            Assert.AreEqual("PRINT", prInst.Type.ToString());
        }

        [TestMethod]
        public void innerViewBuildtest4()
        {
            InnerViewBuilder innerViewBuilder
                = new InnerViewBuilder("sub boo\nset a 3\nsub main\nset a 2\nprint a");
            InnerView innerView = innerViewBuilder.Build();
            bool actual = innerView.Functions.ContainsKey("main");
            Assert.AreEqual(true, actual);
            actual = innerView.Functions.ContainsKey("boo");
            Assert.AreEqual(true, actual);
            SetInst inst = (SetInst)innerView.Functions["boo"][0];
            Assert.AreEqual("SET", inst.Type.ToString());
        }

        [TestMethod]
        public void executerTest1()
        {
            Executer executer = new Executer("sub main\nset a 2\nprint a");
            string output = executer.Execute(new System.Threading.CancellationToken());
            Assert.AreEqual("a=2\n", output);
        }

        [TestMethod]
        public void executerTest2()
        {
            Executer executer = new Executer("sub main\nset a 2\nprint a\nset a 3\nprint a");
            string output = executer.Execute(new System.Threading.CancellationToken());
            Assert.AreEqual("a=2\na=3\n", output);
        }

        [TestMethod]
        public void executerTest3()
        {
            Executer executer = new Executer("sub main\nset b 2\nprint b\nset a 3\nprint a");
            string output = executer.Execute(new System.Threading.CancellationToken());
            Assert.AreEqual("b=2\na=3\n", output);
        }

        [TestMethod]
        public void executerTest4()
        {
            Executer executer 
                = new Executer("sub main\nset a 2\nprint a\ncall foo\nsub foo\nset a 3\ncall boo\nsub boo\nprint a");
            string output = executer.Execute(new System.Threading.CancellationToken());
            Assert.AreEqual("a=2\na=3\n", output);
        }

        [TestMethod]
        public void executerTest5()
        {
            Executer executer = new Executer("sub main\ncall main");
            string output = executer.Execute(new System.Threading.CancellationToken());
            Assert.AreEqual("StackOverflowExeption\n", output);
        }
    }
}
