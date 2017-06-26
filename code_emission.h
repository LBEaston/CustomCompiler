#ifndef CODE_EMISSION_H_
#define CODE_EMISSION_H_

#include <stdio.h>
#include "Ast_Node.h"

void
emit_code(FILE *out, Ast_Node *root);
#endif

