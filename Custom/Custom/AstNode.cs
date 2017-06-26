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

