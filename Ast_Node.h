#ifndef AST_NODE_HPP
#define AST_NODE_HPP

#include "Token_Stream.h"

struct Ast_Node;

typedef struct Ast_Declaration {
    c8 *identifier;
    c8 *type;
} Ast_Declaration;

typedef struct Ast_Assignment {
    c8              *identifier;
    struct Ast_Node *expression;
} Ast_Assignment;

typedef struct Ast_Function_Call {
    c8              *identifier;
    struct Ast_Node **arguments;
} Ast_Function_Call;

typedef struct Ast_Number {
    s32 value;
} Ast_Litteral_Number;

typedef struct Ast_String {
    c8 *value;
} Ast_Litteral_String;

typedef struct Ast_Variable {
    c8 *identifier;
} Ast_Variable;

typedef struct Ast_Bin_Operator {
    enum Tag  tag;
    struct Ast_Node *lhs;
    struct Ast_Node *rhs;
} Ast_Operator;

typedef struct Ast_Block {
    struct Ast_Node **statements;
} Ast_Block;

typedef struct Ast_Node {
    enum Node_Type {
        N_None = 0,
        N_Declaration,
        N_Assignment,
        N_Function_Call,
        N_Number,
        N_String,
        N_Variable,
        N_Bin_Operator,
        N_Block,
    }Node_Type;

    union {
        struct Ast_Declaration   declaration;
        struct Ast_Assignment    assignment;
        struct Ast_Function_Call function_call;
        struct Ast_Number        number;
        struct Ast_String        string;
        struct Ast_Variable      variable;
        struct Ast_Bin_Operator  bin_operator;
        struct Ast_Block         block;
    };

    enum Node_Type type;
    c8 *file;
    s32 line;
    s32 colm;
} Ast_Node;

#endif
