#include "Chain_Buffer.h"
#include "Parser.h"
#include "stretchy_buffer.h"


Ast_Node*
parse_stream(Token_Stream *ts)
{
    Ast_Node *root = parse_block(ts);
    return root;
}

Ast_Node*
parse_block(Token_Stream *ts)
{
    Token *opening_bracket = match_token(ts, tag_lcurlybrack);
    Ast_Node *result = chain_reserve(Ast_Node);
    result->type             = N_Block;
    result->block.statements = 0;
    while(peek_token(ts)->tag != tag_rcurlybrack) {
        if(peek_token(ts)->tag == tag_eof) {
            emit_error("Parser: Unmatched curly bracket '{'",
                       opening_bracket->file, opening_bracket->line, opening_bracket->colm);
        }
        Ast_Node *statement = parse_statement(ts);
        sb_push(result->block.statements, statement);
    }
    return result;
}

Ast_Node*
parse_declaration(Token_Stream *ts)
{
    Ast_Node *result = chain_reserve(Ast_Node);
    result->type = N_Declaration;
    result->declaration.identifier = match_token(ts, tag_id)->lexeme;
    match_token(ts, tag_colon);
    result->declaration.type       = match_token(ts, tag_id)->lexeme;
    return result;
}

Ast_Node*
parse_assignment(Token_Stream *ts)
{
    Ast_Node *result = chain_reserve(Ast_Node);
    result->type = N_Assignment;
    result->assignment.identifier  = match_token(ts, tag_id)->lexeme;
    match_token(ts, tag_equal);
    result->assignment.expression  = parse_expression(ts);
    return result;
}

Ast_Node*
parse_function_call(Token_Stream *ts)
{
    Ast_Node *result = chain_reserve(Ast_Node);
    result->type = N_Function_Call;
    result->function_call.identifier = match_token(ts, tag_id)->lexeme;
    result->function_call.arguments  = 0;
    match_token(ts, tag_lbrack);
    while(1) {
        sb_push(result->function_call.arguments, parse_expression(ts));
        if(peek_token(ts)->tag == tag_comma) {eat_token(ts); continue;}
        if(peek_token(ts)->tag == tag_rbrack) break;
    }
    match_token(ts, tag_rbrack);
    return result;
}

Ast_Node*
parse_expression(Token_Stream *ts)
{
    Token *peek = peek_token(ts);
    if(peek->tag == tag_id) {
        if(lookahead_token(ts, 1)->tag == tag_lbrack) return parse_function_call(ts);

        Ast_Node *result = chain_reserve(Ast_Node);
        result->type = N_Variable;
        result->variable.identifier = peek->lexeme;
        eat_token(ts);
        return result;
    }

    if(peek->tag == tag_number) {
        Ast_Node *result = chain_reserve(Ast_Node);
        result->type = N_Number;
        result->number.value = peek->number;
        eat_token(ts);
        return result;
    }

    if(peek->tag == tag_string) {
        Ast_Node *result = chain_reserve(Ast_Node);
        result->type = N_String;
        result->string.value = peek->lexeme;
        eat_token(ts);
        return result;
    }

    emit_error("Parser: Unexpected token in expression", peek->file, peek->line, peek->colm);
    eat_token(ts);
    return 0;
}

Ast_Node*
parse_statement(Token_Stream *ts)
{
    Token *peek = peek_token(ts);
    if(peek->tag == tag_lcurlybrack) return parse_block(ts);

    if(peek->tag == tag_id) {
        if(lookahead_token(ts, 1)->tag == tag_colon)  return parse_declaration  (ts);
        if(lookahead_token(ts, 1)->tag == tag_equal)  return parse_assignment   (ts);
        return parse_expression(ts);
    }
    eat_token(ts);
    emit_error("Parser: Unexpected token in statement", peek->file, peek->line, peek->colm);
    return 0;
}
