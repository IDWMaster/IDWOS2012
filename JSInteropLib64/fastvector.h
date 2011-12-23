#ifndef FASTVECT
#define FASTVECT
#include "mempool.h"
#include <string.h>
template <class T>
class FastVector {
private:
	MemAllocator* ilock;
	int64_t capacity_bytes;
	int64_t count; 
	T* buffer;
public:
	FastVector() {
	ilock = new MemAllocator(1024*1024*5);
	count = 0;
	buffer = (T*)ilock->allocMem(1024);
	capacity_bytes = 1024;
	};
	FastVector(MemAllocator* allocator) {
	ilock = allocator;
	count = 0;
	buffer = (T*)ilock->allocMem(1024);
	capacity_bytes = 1024;
	};
	int64_t size() {
	return count;
	};
	void clear() {
	count = 0;
	capacity_bytes = 1024;
	ilock->unallocMem(buffer);
	buffer = (T*)ilock->allocMem(1024);
	}
	T* data() {
	return buffer;
	};
	void resize(int64_t len) {
	count = len;
	int64_t prevval = capacity_bytes;
	bool haschanged = false;
	while(capacity_bytes<count*sizeof(T)) {
		capacity_bytes+=1024;
		haschanged = true;
	};
	if(haschanged) {
	T* prevbuffer = buffer;
	//NOW for the big huge copy operation
	buffer = (T*)ilock->allocMem(capacity_bytes);
	memcpy(&buffer,prevbuffer,prevval);
	ilock->unallocMem(prevbuffer);
	}

	};
	void push_back(T item) {
	count++;
	int64_t prevval = capacity_bytes;
	bool haschanged = false;
	while(capacity_bytes<count*sizeof(T)) {
		capacity_bytes+=1024;
		haschanged = true;
	};
	if(haschanged) {
	T* prevbuffer = buffer;
	//NOW for the big huge copy operation
	buffer = (T*)ilock->allocMem(capacity_bytes);
	memcpy(&buffer,prevbuffer,prevval);
	ilock->unallocMem(prevbuffer);
	}
	memcpy(buffer+(sizeof(T)*count),&item,sizeof(T));

	};
	~FastVector() {
	ilock->unallocMem(buffer);
	};
	//TODO: Array operator
	//T& operator[](int index) {
	
//	};

};
#endif