
// Compiler.cs
using System;
using System.IO;

namespace Custom
{
	class Compiler
	{
	    public static void EmitError(string message, string file, int line, int colm)
	    {
			Console.Error.WriteLine(message + " " + file + ":" + line + ":" + colm);
	    }

	    public static void Main(string[] args)
	    {
	        if(args.Length == 0) {
	            Console.Error.WriteLine("No input file(s)");
	            return;
	        }

	        string fileName = args[0];
	        Console.WriteLine(fileName);

	        TokenStream tokenStream = new TokenStream();
	        tokenStream.TokenizeFile(fileName);

			Parser parser = new Parser (tokenStream);
			AstNode rootNode = parser.ParseStream ();

			StreamWriter fout = new StreamWriter ("out.c", false, new System.Text.UTF8Encoding());
			fout.Write ("#include \"stdio.h\"\nint main(int argc, int **argv)\n");
			rootNode.EmitCode (fout);
			fout.Flush ();
	    }
	}
}

// AstNode.cs
using System;
using System.IO;
using System.Collections.Generic;
namespace Custom
{
	public abstract class AstNode
	{
		private string _file;
		private int _line;
		private int _colm;

		public string File {
			get { return _file;}
		}

		public int Line {
			get { return _line;}
		}

		public int Colm {
			get { return _colm;}
		}

		public AstNode (string file, int line, int colm)
		{
			_file = file;
			_line = line;
			_colm = colm;
		}

		public abstract void EmitCode (StreamWriter fout);
	}

	public class AstDeclaration : AstNode
	{
		private string _identifier;
		private string _type;

		public AstDeclaration (string file, int line, int colm, string identifier, string type) :
			base(file, line, colm)
		{
			_identifier = identifier;
			_type = type;
		}

		public override void EmitCode (StreamWriter fout)
		{
			fout.Write (_type + ' ' + _identifier);
		}
	}

	public class AstAssignment : AstNode
	{
		private string _identifier;
		private AstNode _expression;

		public AstAssignment (string file, int line, int colm, string identifier, AstNode expression) :
			base(file, line, colm)
		{
			_identifier = identifier;
			_expression = expression;
		}

		public override void EmitCode (StreamWriter fout)
		{
			fout.Write (_identifier + " = ");
			_expression.EmitCode (fout);
		}
	}

	public class AstFunctionCall : AstNode
	{
		private string _identifier;
		private List<AstNode> _arguments;

		public AstFunctionCall (string file, int line, int colm, string identifier, List<AstNode> arguments) :
			base(file, line, colm)
		{
			_identifier = identifier;
			_arguments = arguments;
		}

		public override void EmitCode (StreamWriter fout)
		{
			fout.Write (_identifier + "(");
			for (int i = 0; i < _arguments.Count; ++i) {
				_arguments [i].EmitCode (fout);
				if (i < _arguments.Count - 1)
					fout.Write (',');
			}
			fout.Write (")");
		}
	}

	public class AstNumber : AstNode
	{
		private int _value;
		public AstNumber (string file, int line, int colm, int value) :
			base(file, line, colm)
		{
			_value = value;
		}

		public override void EmitCode (StreamWriter fout)
		{
			fout.Write (_value);
		}
	}

	public class AstString : AstNode
	{
		private string _value;

		public AstString (string file, int line, int colm, string value) :
			base(file, line, colm)
		{
			_value = value;
		}

		public override void EmitCode (StreamWriter fout)
		{
			fout.Write ("\"" + _value + "\"");
		}
	}

	public class AstVariable : AstNode
	{
		private string _identifier;

		public AstVariable (string file, int line, int colm, string identifier) :
			base(file, line, colm)
		{
			_identifier = identifier;
		}

		public override void EmitCode (StreamWriter fout)
		{
			fout.Write (_identifier);
		}
	}

	public class AstBinOperator : AstNode
	{
		private Token.TagType _tag;
		private AstNode _lhs;
		private AstNode _rhs;

		public AstBinOperator (string file, int line, int colm, Token.TagType tag, AstNode lhs, AstNode rhs) :
			base(file, line, colm)
		{
			_tag = tag;
			_lhs = lhs;
			_rhs = rhs;
		}

		public override void EmitCode (StreamWriter fout)
		{
			throw new NotImplementedException ();
		}
	}

	public class AstBlock : AstNode
	{
		private List<AstNode> _statements;
		public AstBlock (string file, int line, int colm, List<AstNode> statements) :
			base(file, line, colm)
		{
			_statements = statements;
		}

		public override void EmitCode (StreamWriter fout)
		{
			fout.Write("{\n");
			foreach (AstNode statement in _statements) {
				statement.EmitCode (fout);
				fout.Write(";\n");
			}
			fout.Write("}\n");
		}
	}

}

// Parser.cs
using System;
using System.Collections.Generic;

namespace Custom
{
	public class Parser
	{
		private TokenStream _ts;
		public Parser (TokenStream ts)
		{
			_ts = ts;
		}

		public AstNode ParseStream()
		{
			AstNode root = ParseBlock ();
			return root;
		}

		private AstNode ParseBlock()
		{
			Token openingBracket = _ts.Match (Token.TagType.tag_lcurlybrack);

			List<AstNode> statements = new List<AstNode> ();
			while (_ts.Peek().Tag != Token.TagType.tag_rcurlybrack) {
				if (_ts.Peek ().Tag == Token.TagType.tag_eof) {
					Compiler.EmitError ("Parser: Unmatched curly bracket '{'",
					                    openingBracket.File, openingBracket.Line, openingBracket.Colm);
				}
				AstNode statement = ParseStatement ();
				statements.Add (statement);
			}
			AstNode result = new AstBlock(openingBracket.File, openingBracket.Line, openingBracket.Colm, statements);
			return result;
		}

		private AstNode ParseDeclaration()
		{
			Token id = _ts.Match (Token.TagType.tag_id);
			_ts.Match (Token.TagType.tag_colon);
			Token type = _ts.Match (Token.TagType.tag_id);
			AstNode result = new AstDeclaration (id.File, id.Line, id.Colm, id.Lexeme, type.Lexeme);
			return result;
		}

		private AstNode ParseAssignment()
		{
			Token id = _ts.Match (Token.TagType.tag_id);
			_ts.Match (Token.TagType.tag_equal);
			AstNode expr = ParseExpression ();
			AstNode result = new AstAssignment (id.File, id.Line, id.Colm, id.Lexeme, expr);
			return result;
		}

		private AstNode ParseFuntionCall()
		{
			Token id = _ts.Match (Token.TagType.tag_id);
			List<AstNode> arguments = new List<AstNode> ();
			_ts.Match (Token.TagType.tag_lbrack);
			while (true) {
				AstNode expr = ParseExpression ();
				arguments.Add (expr);
				if (_ts.Peek ().Tag == Token.TagType.tag_comma) {
					_ts.Eat ();
					continue;
				}
				if (_ts.Peek ().Tag == Token.TagType.tag_rbrack)
					break;
			}
			_ts.Match (Token.TagType.tag_rbrack);
			AstNode result = new AstFunctionCall (id.File, id.Line, id.Colm, id.Lexeme, arguments);
			return result;
		}

		private AstNode ParseExpression()
		{
			Token peek = _ts.Peek ();
			if (peek.Tag == Token.TagType.tag_id) {
				if (_ts.Lookahead (1).Tag == Token.TagType.tag_lbrack)
					return ParseFuntionCall ();

				AstNode result = new AstVariable (peek.File, peek.Line, peek.Colm, peek.Lexeme);
				_ts.Eat ();
				return result;
			}

			if (peek.Tag == Token.TagType.tag_number) {
				AstNode result = new AstNumber (peek.File, peek.Line, peek.Colm, peek.Number);
				_ts.Eat ();
				return result;
			}

			if (peek.Tag == Token.TagType.tag_string) {
				AstNode result = new AstString (peek.File, peek.Line, peek.Colm, peek.Lexeme);
				_ts.Eat ();
				return result;
			}

			Compiler.EmitError ("Parser: Unexpected token in expression", peek.File, peek.Line, peek.Colm);
			_ts.Eat ();
			return null;
		}

		private AstNode ParseStatement()
		{
			Token peek = _ts.Peek ();
			if (peek.Tag == Token.TagType.tag_lcurlybrack)
				return ParseBlock ();

			if (peek.Tag == Token.TagType.tag_id) {
				if (_ts.Lookahead (1).Tag == Token.TagType.tag_colon)
					return ParseDeclaration ();
				if (_ts.Lookahead (1).Tag == Token.TagType.tag_equal)
					return ParseAssignment ();
				return ParseExpression ();
			}

			_ts.Eat ();
			Compiler.EmitError ("Parser: Unexpected token in statement", peek.File, peek.Line, peek.Colm);
			return null;
		}
	}
}

// Tests.cs
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

// TokenStream.csd

using System;
using System.IO;
using System.Collections.Generic;
namespace Custom
{
	public class Token
	{
	    public enum TagType {
	        tag_none = 0,
	        tag_id,
	        tag_number,
	        tag_string,

	        tag_newline        = 0xA,
	        tag_carriagereturn = 0xD,
	        tag_bang           = '!',
	        tag_quote          = '"',
	        tag_hash           = '#',
	        tag_dollar         = '$',
	        tag_percent        = '%',
	        tag_ampersand      = '&',
	        tag_apostrophe     = '\'',
	        tag_lbrack         = '(',
	        tag_rbrack         = ')',
	        tag_astrix         = '*',
	        tag_plus           = '+',
	        tag_comma          = ',',
	        tag_minus          = '-',
	        tag_fullstop       = '.',
	        tag_slash          = '/',
	        tag_colon          = ':',
	        tag_semicolon      = ';',
	        tag_lessthan       = '<',
	        tag_equal          = '=',
	        tag_greaterthan    = '>',
	        tag_questionmark   = '?',
	        tag_at             = '@',
	        tag_lsquarebrack   = '[',
	        tag_backslash      = '\\',
	        tag_rsquarebrack   = ']',
	        tag_caret          = '^',
	        tag_underscore     = '_',
	        tag_backtick       = '`',
	        tag_lcurlybrack    = '{',
	        tag_pipe           = '|',
	        tag_rcurlybrack    = '}',
	        tag_tilde          = '~', // 127

	        // Digraphs
	        tag_safe_nav = 200,
	        tag_lshift,
	        tag_rshift,
	        tag_arrow,
	        tag_and,
	        tag_or,
	        tag_lessthanequal,
	        tag_greaterthanequal,
	        tag_isequal,
	        tag_notequal,
	        tag_timesequal,
	        tag_divideequal,
	        tag_modequal,
	        tag_plusequal,
	        tag_minusequal,

	        tag_eof = 255,
	    }

	    private TagType _tag;
	    private string  _file;
	    private int     _line, _colm;

	    private string  _lexeme;
	    private int     _number;

	    public Token(TagType tag, string file, int line, int colm, string lexeme)
	    {
	        _tag = tag;
	        _file = file;
	        _line = line;
	        _colm = colm;
	        _lexeme = lexeme;

	        _number = 0xcdcd;
	    }

	    public Token(TagType tag, string file, int line, int colm, int number)
	    {
	        _tag = tag;
	        _file = file;
	        _line = line;
	        _colm = colm;
	        _lexeme = "Unassigned";

	        _number = number;
	    }

	    public TagType Tag
	    {
	        get {return _tag;}
	    }

	    public string File
	    {
	        get {return _file;}
	    }

	    public int Line
	    {
	        get {return _line;}
	    }
	    
	    public int Colm
	    {
	        get {return _colm;}
	    }

	    public string Lexeme
	    {
	        get {return _lexeme;}
	    }

	    public int Number
	    {
	        get {return _number;}
	    }
	}

	public class TokenStream
	{
	    private List<Token> _tokens = new List<Token>();
	    private int _currentToken   = 0;
	    
	    public Token Match(Token.TagType tag)
	    {
	        Token result = _tokens[_currentToken++];
	        if(result.Tag != tag) Compiler.EmitError("Parser: unexpected token", result.File, result.Line, result.Colm);
	        return result;
	    }

	    public Token Eat()
	    {
	        return _tokens[_currentToken++];
	    }

	    public Token Peek()
	    {
	        return _tokens[_currentToken];
	    }

	    public Token Lookahead(int count)
	    {
	        return _tokens[_currentToken + count];
	    }

	    public void TokenizeFile(string fileName)
	    {
	        string file = File.ReadAllText(fileName);

	        int line = 1;
	        int colm = 1;

	        for(int i = 0; i < file.Length; ++i)
	        {
	            char next = file[i];
	            ++colm;

	            // Ignore Whitespace and Comments
	            for(;i+1 < file.Length;next = file[++i], ++colm) {
	                if(next == ' ' || next == '\t') {
	                    ++colm;
	                    continue;
	                }
	                else if(next == '\n') {
	                    ++line;
	                    colm = 1;
	                }
	                // Ignore until newline if '//' is reached
	                else if(next == '/' && file[i+1] == '/') {
	                    do
	                        next = file[++i];
	                    while(next != '\n');
	                    colm = 0;
	                }
	                else break;
	            }

	            // string litteral
	            if(next == '\"') {
	                int stringStartColm = colm;
					string buffer = "";
	                while(true) {
	                    next = file[++i];
	                    ++colm;
	                    if(next == '\"') break;
	                    if(next == '\n') {
	                        Compiler.EmitError("Lexer: Newline encountered before end of string",
	                                           fileName, line, stringStartColm);
	                        return;
	                    }
	                    if(i >= file.Length) {
	                        Compiler.EmitError("Lexer: End of file reached inside of string!",
	                                fileName, line, stringStartColm);
	                        return;
	                    }
						buffer += next;
	                }

	                Token result = new Token(Token.TagType.tag_string,
	                                         fileName,
	                                         line,
	                                         stringStartColm,
	                                         buffer);
	                _tokens.Add(result);

	                continue;
	            }

	            // number litteral
	            if(char.IsDigit(next)) {
	                int numberStartColm = colm;
	                int val= 0;

	                while(true) {
	                    val*= 10;
	                    val+= next - '0';
	                    next = file[i+1];
	                    if(!char.IsDigit(next)) break;
	                    ++i;
	                    ++colm;
	                }

	                Token result = new Token(Token.TagType.tag_number,
	                                         fileName,
	                                         line,
	                                         numberStartColm,
	                                         val);
	                _tokens.Add(result);

	                continue;
	            }

	            // identifier
	            if(char.IsLetter(next)) {
	                int identifierStartColm = colm;
					string buffer = "";
	                while(true) {
						buffer += next;
	                    next = file[i+1];
	                    if(!(char.IsLetter(next) || char.IsDigit(next)) && next != '_') break;
	                    ++i;
	                    ++colm;
	                }

	                Token result = new Token(Token.TagType.tag_id,
	                                         fileName,
	                                         line,
	                                         identifierStartColm,
	                                         buffer);
	                _tokens.Add(result);

	                continue;
	            }

	            // symbol
	            {
	                Token result = new Token((Token.TagType)next,
	                                     fileName,
	                                     line,
	                                     colm,
	                                     0);
	                _tokens.Add(result);
	            }
	        }
	        {
	            Token result = new Token(Token.TagType.tag_eof,
	                                     fileName,
	                                     line,
	                                     colm,
	                                     0);

	            _tokens.Add(result);
	        }
	    }
	}
}
