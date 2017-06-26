#ifndef TOKEN_STREAM_HPP
#define TOKEN_STREAM_HPP

#include "Compiler.h"

typedef struct Token
{
    enum Tag {
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
    }Tag;

    enum Tag  tag;
    c8        *file;
    s32       line, colm;

    union
    {
        c8 *lexeme;
        s32 number;
    };
} Token;

typedef struct Token_Stream
{
    struct Token *tokens;
    s32 current;
} Token_Stream;

void
print_token(Token *token);

inline Token* INLINE
match_token(Token_Stream *stream, enum Tag tag)
{
    Token *result = stream->tokens + stream->current++;
    if(result->tag != tag) emit_error("Parser: unexpected token", result->file, result->line, result->colm);
    return result;
}

inline Token* INLINE
eat_token(Token_Stream *stream)
{
    return stream->tokens + stream->current++;
}

inline Token* INLINE
peek_token(Token_Stream *stream)
{
    return stream->tokens + stream->current;
}

inline Token* INLINE
lookahead_token(Token_Stream *stream, s32 count)
{
    return stream->tokens + stream->current + count;
}

void
tokenize_file(struct Token_Stream *stream, c8 *file_name);

#endif
