#ifndef ROPE_H_
#define ROPE_H_

#include "types.h"

/*
 * cache
 * store a string that will live for the entire life of the program
 * whilst ensuring realatively good cache cohernency
 */
c8*
cache_string(const c8* cstr);

void
kill_text_buffer();

#endif
