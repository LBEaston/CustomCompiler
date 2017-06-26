
#include <ctype.h>
#include "stretchy_buffer.h"

#include "Token_Stream.h"
#include "Rope.h"

u8
fpeekc(FILE *file)
{
    u8 result = fgetc(file);
    ungetc(result, file);
    return result;
}

void
print_token(Token *token)
{
    switch(token->tag) {
        default:
            /* printf("%s", token->lexeme); */
            break;
    }
}

void
tokenize_file(Token_Stream *stream, c8 *file_name)
{
    FILE *file = fopen(file_name, "r");

    s32 line = 1;
    s32 colm = 1;

    while(!feof(file)) {
        c8 next = fgetc(file);
        ++colm;

        // Ignore Whitespace and Comments
        for(;;next = fgetc(file), ++colm) {
            if(next == ' ' || next == '\t') {
                ++colm;
                continue;
            }
            else if(next == '\n') {
                ++line;
                colm = 1;
            }
            // Ignore until newline if '//' is reached
            else if(next == '/' && fpeekc(file) == '/') {
                do
                    next = fgetc(file);
                while(next != '\n');
                colm = 0;
            }
            else break;
        }

        // string litteral
        if(next == '\"') {
            s32 string_start_colm = colm;
            c8 buffer[256] = {0};
            s32 current_char = 0;
            while(1) {
                next = fgetc(file);
                ++colm;
                if(next == '\"') break;
                if(next == '\n') {
                    emit_error("Lexer: Newline encountered before end of string",
                            file_name, line, string_start_colm);
                    return;
                }
                if(feof(file)) {
                    emit_error("Lexer: End of file reached inside of string!",
                            file_name, line, string_start_colm);
                    return;
                }
                buffer[current_char++] = next;
            }
            buffer[current_char] = 0;

            Token result;
            result.tag    = tag_string;
            result.file   = file_name;
            result.lexeme = cache_string(buffer);
            result.line   = line;
            result.colm   = string_start_colm;
            sb_push(stream->tokens, result);

            goto NEXT_TOKEN;
        }

        // number litteral
        if(isdigit(next)) {
            s32 number_start_colm = colm;
            s32 value = 0;

            while(1) {
                value *= 10;
                value += next - '0';
                next = fpeekc(file);
                if(!isdigit(next)) break;
                fgetc(file);
                ++colm;
            }

            Token result;
            result.tag  = tag_number;
            result.file = file_name;
            result.number = value;
            result.line = line;
            result.colm = number_start_colm;
            sb_push(stream->tokens, result);

            goto NEXT_TOKEN;
        }

        // identifier
        if(isalpha(next)) {
            s32 identifier_start_colm = colm;
            c8 buffer[256] = {0};
            s32 current_char = 0;
            while(1) {
                buffer[current_char++] = next;
                next = fpeekc(file);
                if(!isalnum(next) && next != '_') break;
                fgetc(file);
                ++colm;
            }
            buffer[current_char] = 0;

            Token result;
            result.tag    = tag_id;
            result.file   = file_name;
            result.lexeme = cache_string(buffer);
            result.line   = line;
            result.colm   = identifier_start_colm;
            sb_push(stream->tokens, result);

            goto NEXT_TOKEN;
        }

        // symbol
        if(!feof(file)) {
            Token result;
            result.tag    = next;
            result.file   = file_name;
            result.line   = line;
            result.colm   = colm;
            sb_push(stream->tokens, result);
        } else {
            Token result;
            result.tag    = tag_eof;
            result.file   = file_name;
            result.line   = line;
            result.colm   = colm;
            sb_push(stream->tokens, result);
        }
NEXT_TOKEN: ;
    }
    fclose(file);
}
