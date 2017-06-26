
#ifndef CHAIN_BUFFER_HEADER
#define CHAIN_BUFFER_HEADER

#define CHAIN_BUFFER_MIN_SIZE 0x100000

#include <stddef.h>

#define chain_push(item)     chain_push_psize((void*)&item, sizeof(item))
#define chain_reserve(type)  chain_push_psize(0,            sizeof(type))

void*
chain_push_psize(void* data, size_t size);

#endif /*CHAIN_BUFFER_HEADER*/

#ifdef CHAIN_BUFFER_IMPLEMENTATION

#include <memory.h>
#include "stretchy_buffer.h"

typedef struct Chain_Buffer
{
    void*  data;
    size_t size;
    size_t capacity;
} Chain_Buffer;

static Chain_Buffer*
new_chain_buffer(Chain_Buffer *buffers, size_t min_size)
{
    size_t true_size = (min_size > CHAIN_BUFFER_MIN_SIZE) ? min_size : CHAIN_BUFFER_MIN_SIZE;
    Chain_Buffer *new_buffer = stb_sb_add(buffers, 1);
    new_buffer->data = malloc(true_size);
    new_buffer->size = 0;
    new_buffer->capacity = true_size;
    return buffers;
}

void*
chain_push_psize(void* data, size_t size)
{
    static Chain_Buffer *buffers = 0;
    if(!buffers)
        buffers = new_chain_buffer(buffers, size);

    Chain_Buffer *current = &buffers[sb_count(buffers)-1];
    if(current->size + size > current->capacity) {
        buffers = new_chain_buffer(buffers, size);
        current = &buffers[sb_count(buffers)-1];
    }

    if(data) memcpy(current->data + current->size, data, size);

    void* result = current->data + current->size;
    current->size += size;
    return result;
}

#endif
