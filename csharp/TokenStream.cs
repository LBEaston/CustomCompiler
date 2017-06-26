
using System;
using System.IO;
using System.Collections.Generic;

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

        // Keywords
        tag_key_true,
        tag_key_false,
        tag_key_if,
        tag_key_elif,
        tag_key_else,
        tag_key_each,
        tag_key_while,
        tag_key_loop,
        tag_key_match,
        tag_key_enum,
        tag_key_return,
        tag_key_goto,
        tag_key_default,
        tag_key_uninit,
        tag_key_global,
        tag_key_internal,

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

class TokenStream
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
            for(;;next = file[++i], ++colm) {
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
                char[] buffer = new char[256];
                int currentChar = 0;
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
                    buffer[currentChar++] = next;
                }
                buffer[currentChar] = '\0';

                Token result = new Token(Token.TagType.tag_string,
                                         fileName,
                                         line,
                                         stringStartColm,
                                         new string(buffer));
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
                char[] buffer = new char[256];
                int currentChar = 0;
                while(true) {
                    buffer[currentChar++] = next;
                    next = file[i+1];
                    if(!(char.IsLetter(next) || char.IsDigit(next)) && next != '_') break;
                    ++i;
                    ++colm;
                }
                buffer[currentChar] = '\0';

                Token result = new Token(Token.TagType.tag_id,
                                         fileName,
                                         line,
                                         identifierStartColm,
                                         new string(buffer));
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
