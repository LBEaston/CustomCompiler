
#include "code_emission.h"
#include "stretchy_buffer.h"
#include "Compiler.h"

void
emit_code_for_block         (FILE *out, Ast_Node *root);
void
emit_code_for_declaration   (FILE *out, Ast_Node *root);
void
emit_code_for_assignment    (FILE *out, Ast_Node *root);
void
emit_code_for_function_call (FILE *out, Ast_Node *root);
void
emit_code_for_variable      (FILE *out, Ast_Node *root);
void
emit_code_for_number        (FILE *out, Ast_Node *root);
void
emit_code_for_string        (FILE *out, Ast_Node *root);

void
emit_code_node(FILE *out, Ast_Node *node)
{
    switch(node->type)
    {
    case N_Block         : emit_code_for_block         (out, node); break;
    case N_Declaration   : emit_code_for_declaration   (out, node); break;
    case N_Assignment    : emit_code_for_assignment    (out, node); break;
    case N_Function_Call : emit_code_for_function_call (out, node); break;
    case N_Variable      : emit_code_for_variable      (out, node); break;
    case N_Number        : emit_code_for_number        (out, node); break;
    case N_String        : emit_code_for_string        (out, node); break;
    default: emit_error("Codegen: Unknown AST Node type", 0, 0, 0); break;
    }
}

void
emit_code(FILE *out, Ast_Node *root)
{
    fprintf(out, "int main() {\n");
    emit_code_node(out, root);
    fprintf(out, "return 0; }\n\n");
}

void
emit_code_for_block         (FILE *out, Ast_Node *node)
{
    fprintf(out, "{\n");
    for(s32 i = 0; i < sb_count(node->block.statements); ++i) {
        emit_code_node(out, node->block.statements[i]);
        fprintf(out, ";\n");
    }
    fprintf(out, "}\n");
}

void
emit_code_for_declaration   (FILE *out, Ast_Node *node)
{
    fprintf(out, "%s %s", node->declaration.type, node->declaration.identifier);
}

void
emit_code_for_assignment    (FILE *out, Ast_Node *node)
{
    fprintf(out, "%s = ", node->assignment.identifier);
    emit_code_node(out, node->assignment.expression);
}

void
emit_code_for_function_call (FILE *out, Ast_Node *node)
{
    fprintf(out, "%s(", node->function_call.identifier);
    for(s32 i = 0; i < sb_count(node->function_call.arguments); ++i) {
        emit_code_node(out, node->function_call.arguments[i]);
        if(i < sb_count(node->function_call.arguments)-1)
            fprintf(out, ", ");
    }
    fprintf(out, ")");
}

void
emit_code_for_variable      (FILE *out, Ast_Node *node)
{
    fprintf(out, "%s", node->variable.identifier);
}

void
emit_code_for_number        (FILE *out, Ast_Node *node)
{
    fprintf(out, "%li", node->number.value);
}

void
emit_code_for_string        (FILE *out, Ast_Node *node)
{
    fprintf(out, "\"%s\"", node->string.value);
}

