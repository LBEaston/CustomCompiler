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

