#include <string.h>
#include <malloc.h>

#include "Rope.h"

typedef struct Rope_Buffer {
  u32                  size;
  c8*                  cstr;
  c8*                  curs;
  struct Rope_Buffer*  prev;
} Rope_Buffer;

static Rope_Buffer* text = 0;

static void
new_buff(const u32 min_size) {
  static const u32 default_min_size = 4096;

  Rope_Buffer* prev = text;
  text         = malloc(sizeof(Rope_Buffer));
  text->size   = min_size > default_min_size ? min_size : default_min_size;
  text->cstr   = malloc(sizeof(c8) * text->size);
  text->curs   = text->cstr;
  text->prev   = prev;
}

static void
kill_buff(Rope_Buffer* dead) {
  if(dead->prev) kill_buff(dead->prev);
  free(dead->cstr);
  free(dead);
}

void
kill_text_buffer() {
  kill_buff(text);
}

c8*
cache_string(const c8* cstr) {
  u32 size = (u32)strlen(cstr) + 1;
  if(size)++size;
  if(!text || text->curs - text->cstr > text->size - size)
    new_buff(size);
  c8* to_return = strcpy(text->curs, cstr);
  text->curs += size;
  return to_return;
}

