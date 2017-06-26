#include <stdio.h>

#include "Compiler.h"
#include "Rope.h"

#include "Token_Stream.h"
#include "Ast_Node.h"
#include "Parser.h"
#include "code_emission.h"

#include "stretchy_buffer.h"

#define CHAIN_BUFFER_IMPLEMENTATION
#include "Chain_Buffer.h"


void
emit_error(const c8 *message, c8 *file_name, s32 line, s32 colm)
{
    fprintf(stderr, "%s\n\t%s(%i:%i)\n", message, file_name, line, colm);

    const s32 MAX_LINE_LEN = 120;
    if(colm > MAX_LINE_LEN)
        fprintf(stderr, "Error line is too long, not printing\n");

    FILE *file = fopen(file_name, "r");
    go_to_line(file, line);

    c8 line_buffer[MAX_LINE_LEN];
    fgets(line_buffer, MAX_LINE_LEN, file);
    fprintf(stderr, "%s", line_buffer);

    for(s32 i = 0; i < colm - 2; ++i)
        fputc('~', stderr);

    fprintf(stderr, "^\n");
}

void
go_to_line (FILE *file, s32 line)
{
    c8 c;
    for(s32 i = 0; i < line - 1; ++i)
        do c = fgetc(file);
        while (c != '\n');
}

int
main(s32 argc, c8 **argv)
{
    if(argc == 1) {
        fprintf(stderr, "No input file(s)\n");
        return -1;
    }

    c8 *file_name = cache_string(argv[1]);

    Token_Stream token_stream;
    token_stream.tokens = 0;
    token_stream.current= 0;

    tokenize_file(&token_stream, file_name);
    Ast_Node *root_node = parse_stream(&token_stream);

    emit_code(stdout, root_node);

    for(s32 i = 0; i < sb_count(token_stream.tokens); ++i) {
        Token *token = &token_stream.tokens[i];
        print_token(token);
    }

    return 0;
}
