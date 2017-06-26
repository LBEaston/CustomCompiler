#ifndef PARSER_H_
#define PARSER_H_

#include "Token_Stream.h"
#include "Ast_Node.h"

Ast_Node*
parse_stream(Token_Stream *stream);

Ast_Node*
parse_block(Token_Stream *stream);

Ast_Node*
parse_declaration(Token_Stream *stream);

Ast_Node*
parse_statement(Token_Stream *stream);

Ast_Node*
parse_expression(Token_Stream *stream);
#endif
