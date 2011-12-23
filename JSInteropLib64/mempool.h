#ifndef MEMPOOL_H
#define MEMPOOL_H
#define platformptr long long
#include <string>
#include <vector>
#include <v8stdint.h>
struct memregion {
	size_t length;
	size_t offset;
	bool allocated;
};
class MemAllocator {
public:
	char* data;
	std::vector<memregion> memregions;

	MemAllocator(size_t dsize) {
		memregion defaultregion;
		defaultregion.allocated = false;
		defaultregion.length = dsize;
		defaultregion.offset = 0;
		data = (char*)malloc(dsize);
		memregions.push_back(defaultregion);
	};
	void unallocMem(void* ptr) {
	for(int i = 0;i<memregions.size();i++) {
		platformptr calculatedoffset = ((platformptr)data)+memregions[i].offset;
		//platformptr calculatedend = calculatedoffset+memregions[i].length;
		if((platformptr)ptr == calculatedoffset) {
			memregions[i].allocated = false;
			return;
		}
	}
	throw "MemoryNotFound";
	}
	void compact() {
	//Look for any free space fragments which can "linked"
		for(int i = 0;i<memregions.size()+1;i+=2) {
			if(!memregions[i].allocated & !memregions[i+1].allocated) {
				
			}
		}
	}
	void* allocMem(size_t size) {
		
		bool hascompacted = false;
		mcplr:
		for(int i = 0;i<memregions.size();i++) {
			if(!memregions[i].allocated & memregions[i].length>=size) {
				size_t freedmem = memregions[i].length-size;
				memregions[i].allocated = true;
				memregions[i].length = size;
				if(freedmem !=0) {
				//Create a new block representing the free memory
				memregion freedregion;
				freedregion.allocated = false;
				freedregion.length = freedmem;
				
				freedregion.offset = memregions[i].offset+memregions[i].length;
				memregions.push_back(freedregion);
				}
				return data+memregions[i].offset;
			}
		}
		if(!hascompacted) {
			compact();
			hascompacted = true;
			goto mcplr;
		}
		throw "InsufficientMemory";
	}

	
};
template <class T>
class FastAllocator {
public:
	MemAllocator* ilock;
	FastAllocator(MemAllocator* internalloc) {
	ilock = internalloc;
	};
	T* allocate(size_t mlen) {
		//Allocate in chunks of 1024 bytes
		//starting at 1024
		size_t count = mlen*sizeof(T);
		int64_t blocksize = 1024;
		while(blocksize<count) {
			blocksize+=1024;
		}
		return (T*)ilock->allocMem(count);
	}
	void deallocate(T* ptr, size_t num) {
		ilock->unallocMem(ptr);
	}
};
#endif
