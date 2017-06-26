#ifndef COMPILER_HPP_
#define COMPILER_HPP_

#include <stdio.h>

#include "types.h"

#define INLINE     __attribute__((always_inline))
#define CONST      __attribute__((const))
#define PURE       __attribute__((pure))
#define FLATTEN    __attribute__((flatten))
#define HOT        __attribute__((hot))
#define COLD       __attribute__((cold))
#define NEVER_NULL __attribute__((return_nonnull))

void
emit_error(const c8 *message, c8 *file, s32 line, s32 colm);

void
go_to_line (FILE *file, s32 line);

#endif
