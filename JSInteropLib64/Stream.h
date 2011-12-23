#ifndef STREAM_H
#define STREAM_H
#include "stdafx.h"
#include <v8.h>
namespace System {
namespace IO {
class Stream {
public:
	virtual void Write(char* buffer, int32_t offset, int32_t length) = 0;
	virtual int32_t Read(char* buffer, int32_t offset, int32_t length) = 0;
	virtual void Position_Set(int32_t value) = 0;
	virtual int Position_Get() = 0;
};
};
};
#endif