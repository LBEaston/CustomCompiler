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

        // Parse a {} block
        // { statement0 statement1 ... }
		private AstNode ParseBlock()
		{
            // Match opening curly bracket
			Token openingBracket = _ts.Match (Token.TagType.tag_lcurlybrack);

			List<AstNode> statements = new List<AstNode> ();
            // Parse internal nodes until closing curly bracket is found
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

        // Parse a variable declaration
        // id : type
		private AstNode ParseDeclaration()
		{
			Token id = _ts.Match (Token.TagType.tag_id);
			_ts.Match (Token.TagType.tag_colon);
			Token type = _ts.Match (Token.TagType.tag_id);
			AstNode result = new AstDeclaration (id.File, id.Line, id.Colm, id.Lexeme, type.Lexeme);
			return result;
		}

        // Parse an assignment
        // id = value
		private AstNode ParseAssignment()
		{
			Token id = _ts.Match (Token.TagType.tag_id);
			_ts.Match (Token.TagType.tag_equal);
			AstNode expr = ParseExpression ();
			AstNode result = new AstAssignment (id.File, id.Line, id.Colm, id.Lexeme, expr);
			return result;
		}

        // Parse a function call
        // id(arg0, arg1, ...)
		private AstNode ParseFuntionCall()
		{
			Token id = _ts.Match (Token.TagType.tag_id);
			List<AstNode> arguments = new List<AstNode> ();
			_ts.Match (Token.TagType.tag_lbrack);
            // Parse all the arguments until a closing brace is found
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

        // Parse an expression
		private AstNode ParseExpression()
		{
			Token peek = _ts.Peek ();
            // Found an ID
			if (peek.Tag == Token.TagType.tag_id) {
                // If there's a following opening bracket then it's a function call
				if (_ts.Lookahead (1).Tag == Token.TagType.tag_lbrack)
					return ParseFuntionCall ();
                // Else it's a variable
				AstNode result = new AstVariable (peek.File, peek.Line, peek.Colm, peek.Lexeme);
				_ts.Eat ();
				return result;
			}
            // Found a number
			if (peek.Tag == Token.TagType.tag_number) {
				AstNode result = new AstNumber (peek.File, peek.Line, peek.Colm, peek.Number);
				_ts.Eat ();
				return result;
			}
            // Found a string
			if (peek.Tag == Token.TagType.tag_string) {
				AstNode result = new AstString (peek.File, peek.Line, peek.Colm, peek.Lexeme);
				_ts.Eat ();
				return result;
			}

			Compiler.EmitError ("Parser: Unexpected token in expression", peek.File, peek.Line, peek.Colm);
			_ts.Eat ();
			return null;
		}

        // Parse a Statement
		private AstNode ParseStatement()
		{
			Token peek = _ts.Peek ();
            // If there's an opening curly bracket then parse a block
			if (peek.Tag == Token.TagType.tag_lcurlybrack)
				return ParseBlock ();
            // Else it's either a declaration, assignment, or expression
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

