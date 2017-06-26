using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace Custom
{
	[TestFixture()]
	public class Tests
	{
		[Test()]
		public void TestDeclaration ()
		{
			StreamWriter sw = new StreamWriter ("testfileDeclaration");
			AstNode node = new AstDeclaration ("test", 1, 1, "varName", "type");
			node.EmitCode (sw);
			sw.Flush ();
			sw.Close ();

			StreamReader sr = new StreamReader ("testfileDeclaration");
			string wholeFile = sr.ReadToEnd ();

			Assert.AreEqual ("type varName", wholeFile);
		}

		[Test()]
		public void TestAssignment ()
		{
			StreamWriter sw = new StreamWriter ("testfileAssignment");
			AstNode node = new AstAssignment ("test", 1, 1, "lhs", new AstVariable ("test", 1, 1, "var"));
			node.EmitCode (sw);
			sw.Flush ();
			sw.Close ();

			StreamReader sr = new StreamReader ("testfileAssignment");
			string wholeFile = sr.ReadToEnd ();

			Assert.AreEqual ("lhs = var", wholeFile);
		}

		[Test()]
		public void TestFunctionCall ()
		{
			StreamWriter sw = new StreamWriter ("testfileFunctionCall");
			AstNode node = new AstFunctionCall ("test", 1, 1, "function", new List<AstNode> ());
			node.EmitCode (sw);
			sw.Flush ();
			sw.Close ();

			StreamReader sr = new StreamReader ("testfileFunctionCall");
			string wholeFile = sr.ReadToEnd ();

			Assert.AreEqual ("function()", wholeFile);
		}

		[Test()]
		public void TestNumber ()
		{
			StreamWriter sw = new StreamWriter ("testfileNumber");
			AstNode node = new AstNumber ("test", 1, 1, 42);
			node.EmitCode (sw);
			sw.Flush ();
			sw.Close ();

			StreamReader sr = new StreamReader ("testfileNumber");
			string wholeFile = sr.ReadToEnd ();

			Assert.AreEqual ("42", wholeFile);
		}

		[Test()]
		public void TestString ()
		{
			StreamWriter sw = new StreamWriter ("testfileString");
			AstNode node = new AstString ("test", 1, 1, "StringValue");
			node.EmitCode (sw);
			sw.Flush ();
			sw.Close ();

			StreamReader sr = new StreamReader ("testfileString");
			string wholeFile = sr.ReadToEnd ();

			Assert.AreEqual ("\"StringValue\"", wholeFile);
		}

		[Test()]
		public void TestVariable ()
		{
			StreamWriter sw = new StreamWriter ("testfileVariable");
			AstNode node = new AstVariable("test", 1, 1, "VarName");
			node.EmitCode (sw);
			sw.Flush ();
			sw.Close ();

			StreamReader sr = new StreamReader ("testfileVariable");
			string wholeFile = sr.ReadToEnd ();

			Assert.AreEqual ("VarName", wholeFile);
		}

		[Test()]
		public void TestBinOperator ()
		{
			StreamWriter sw = new StreamWriter ("testfileBinOperator");
			AstNode variable = new AstVariable ("test", 1, 1, "VAR");
			AstNode node = new AstBinOperator("test", 1, 1, Token.TagType.tag_plus, variable, variable);
		
			TestDelegate lambda = () => {
				node.EmitCode (sw);
				};

			Assert.Catch (lambda);

			sw.Flush ();
			sw.Close ();
		}
	}
}

