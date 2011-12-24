// JSInteropLib64.cpp : Defines the exported functions for the DLL application.
#include "stdafx.h"
#pragma comment(lib, "wsock32.lib")
//Could'a used a.....
#include <v8.h>//!!!!
#include <vector>
#include <string>
#include <map>
#include "mempool.h"
#include "fastvector.h"
#include <mutex>
using namespace v8;
using namespace std;
typedef void* (*ManagedFuncPtr)(int len,void* data);

struct VMObject {
	//Stupid Microsoft jokes! What is a 'long long' anyways? Don't they mean a 'really long long'?
	///Represents the number of function calls defined here
	int64_t count;
	///Pointers to the functions to be available as the "root" object
	ManagedFuncPtr* functions;
	///The names of each function
	uint16_t** methodnames;
	};
	struct funcptr {
		Persistent<Function> mfunc;
	};
	struct EmbeddedFuncPtr {
	ManagedFuncPtr function;
	void* instance;
	};
	static vector<EmbeddedFuncPtr> funcPtrs;
	static std::mutex mtx;
	static HANDLE LockFuncPtrs() {
		mtx.lock();
	return NULL;	
		HANDLE mutex = CreateMutex(NULL,TRUE,L"{8F0C9ED4-9B9A-4105-8FC5-8FC925E63A00}");
		   WaitForSingleObject(mutex,-1);
		   return mutex;
};
	
	static void UnlockFuncPtrs(HANDLE mutex) {
		mtx.unlock();
	ReleaseMutex(mutex);
		   CloseHandle(mutex);
	};
	static int currentPtr = 0;
class VMInstance {
public:
	
	MemAllocator* memallocator;
	
	
	int32_t cptr_int;
    map< int32_t, funcptr> jsFuncPtrs;
	
	
	
	void* FastAlloc(int64_t count) {
	//Allocate in chunks of 1024 bytes
		//starting at 1024
		
		int64_t blocksize = 1024;
		while(blocksize<count) {
			blocksize+=1024;
		}
		return memallocator->allocMem(blocksize);
	}
		void WriteData(vector<char>& destination, void* source, int len) {
			destination.resize(destination.size()+len);
			memcpy((destination.data()+destination.size())-len,source,len);
		
		}
	void SerializeValue(Local<Value> value, std::vector<char>& data) {
			
			if(value->IsNumber() & !value->IsInt32()) {
				data.push_back(5);
				double numbval = value->NumberValue();
				WriteData(data,&numbval,sizeof(double));
				return;
			}
			if(value->IsFunction()) {
				data.push_back(4);
				funcptr fval;
				fval.mfunc = Persistent<Function>::New(Handle<Function>::Cast(value));
				jsFuncPtrs[cptr_int] = fval;
				WriteData(data,&cptr_int,sizeof(int32_t));
				cptr_int++;
				return;
			}
			if(value->IsInt32()) {
				data.push_back(2);
				//Datatype = 2
				int32_t val = value->Int32Value();
				WriteData(data,&val,sizeof(int32_t));
				return;
			}
			if(value->IsString()) {
				data.push_back(3);
				//Datatype = 3
				int32_t stlen = value->ToString()->Utf8Length();
				WriteData(data,&stlen,sizeof(int32_t));
				char* strval = new char[stlen];
				value->ToString()->WriteUtf8(strval);
				WriteData(data,strval,stlen);
				return;
			}
			if(value->IsArray()) {
				//Datatype = 1
					data.push_back(1);
					//Size
					Local<Array> mray = Local<Array>::Cast(value);
					data.resize(data.size()+sizeof(uint32_t));
					uint32_t len = mray->Length();
					memcpy((data.data()+data.size())-sizeof(uint32_t),(void*)&len,sizeof(uint32_t));
					//Begin array elements
					int32_t ccount = mray->Length();
					for(int i = 0;i<ccount;i++) {
					
						SerializeValue(mray->Get(i),data);
					}
					return;
				}
			
			if(value->IsObject()) {
				//Datatype = 0
				data.push_back(0);
				Local<Object> obj = value->ToObject();
				Local<Array> propertynames = obj->GetPropertyNames();
				int32_t count = propertynames->Length();
				data.resize(data.size()+sizeof(int32_t));
				memcpy((data.data()+data.size())-sizeof(int32_t),&count,sizeof(int32_t));

			for(int i = 0;i<count;i++) {
				Local<Value> tval = obj->GetRealNamedProperty(propertynames->Get(i)->ToString());
				//Object name (prefixed with int)
				int32_t stlen = propertynames->Get(i)->ToString()->Utf8Length();
				data.resize(data.size()+sizeof(int32_t));
				memcpy((data.data()+data.size())-sizeof(int32_t),&stlen,sizeof(int32_t));
			    char* mdat = new char[stlen];
				propertynames->Get(i)->ToString()->WriteUtf8(mdat);
				data.resize(data.size()+stlen);
				memcpy((data.data()+data.size())-stlen,mdat,stlen);
				SerializeValue(tval,data);
				
			}
			}
		}

	VMInstance() {
	memallocator = new MemAllocator(1024*1024*5);
	cptr_int = 0;
	}

	vector<char> data;
	vector<char> realstream;
	Handle<Value> DeserializeObject(char* input, int64_t* offsetptr) {
		char objID = input[*offsetptr];
		*offsetptr +=1;
		if(objID == 5) {
		double mval;
		memcpy(&mval,input+(*offsetptr),sizeof(double));
		*offsetptr+=sizeof(double);
		return Number::New(mval);
		}
		if(objID == 4) {
			int32_t* intptr = (int32_t*)(input+(*offsetptr));
			funcptr mptr = jsFuncPtrs[(*intptr)];
			*offsetptr+=sizeof(int32_t);
			return mptr.mfunc;
		}
		if(objID == 3) {
		int32_t stlen;
		memcpy(&stlen,input+(*offsetptr),sizeof(int32_t));
		(*offsetptr)+=sizeof(int);
		char* txt = new char[stlen];
		memcpy(txt,input+(*offsetptr),stlen);
		*offsetptr+=stlen;
		return String::New(txt,stlen);
		}
		if(objID == 2) {
		//int32_t
			int32_t val;
			memcpy(&val,input+(*offsetptr),sizeof(int32_t));
			(*offsetptr)+=sizeof(int32_t);
			return Int32::New(val);

		}
		if(objID == 1) {
			uint32_t arraylen;
			memcpy(&arraylen,input+(*offsetptr),sizeof(uint32_t));
			Local<Array> mray = Array::New(arraylen);
			(*offsetptr)+=sizeof(uint32_t);
			for(int i = 0;i<arraylen;i++) {
				mray->Set(i,DeserializeObject(input,offsetptr));
			}
			return mray;
		}
		if(objID == 0) {
			int32_t count;
			
			Handle<ObjectTemplate> rootobj = ObjectTemplate::New();
			memcpy(&count,input+(*offsetptr),sizeof(int32_t));
			(*offsetptr)+=sizeof(int32_t);
		for(int i = 0;i<count;i++) {
			int32_t stlen;
			memcpy(&stlen,input+(*offsetptr),sizeof(int32_t));
			(*offsetptr)+=sizeof(int);
			char* str = new char[stlen];
			memcpy(str,input+(*offsetptr),stlen);
			(*offsetptr)+=stlen;
			Handle<Value> tval = DeserializeObject(input,offsetptr);
			
			rootobj->Set(String::New(str,stlen),tval);
		}
		return rootobj->NewInstance();
		}
		
		}
	static Handle<Value> InvokeManagedDelegate(const Arguments& args) {
		//TODO: Get fastvector to work
		HANDLE mutex = LockFuncPtrs();
		EmbeddedFuncPtr ptr = funcPtrs[args.Data()->Int32Value()];
		UnlockFuncPtrs(mutex);
		VMInstance* instance = ((VMInstance*)(ptr.instance));
		instance->realstream.clear();
		
		
		int count = args.Length();
		
		for(int i = 0;i<count;i++) {
		instance->data.clear();
		
			instance->SerializeValue(args[i],instance->data);
			instance->realstream.resize(instance->realstream.size()+instance->data.size());
			memcpy(instance->realstream.data()+(instance->realstream.size()-instance->data.size()),instance->data.data(),instance->data.size()*sizeof(char));
			
		}
		char* expstr = (char*)instance->FastAlloc(instance->realstream.size());
		memcpy(expstr,instance->realstream.data(),instance->realstream.size()*sizeof(char));
		void* retval = ptr.function(instance->realstream.size(),expstr);
		if(retval == NULL) {
		return Int32::New(0);
		}else {
			int64_t offset = sizeof(int32_t);
			char* chptr = (char*)retval;
			Handle<Value> mval = instance->DeserializeObject(chptr,&offset);
			instance->memallocator->unallocMem(retval);
			//delete retval;
			//LocalFree(retval);
			return mval;
		}
	}


	void DoVMLoad(long count, ManagedFuncPtr* functions, uint16_t** names, uint16_t* source) {
	VMObject vmData;
	vmData.count = count;
	vmData.functions = functions;
	vmData.methodnames = names;
	v8::Isolate* mlate = Isolate::New();
	mlate->Enter();
	HandleScope handle_scope;
	
	
	Handle<ObjectTemplate> roothandle = ObjectTemplate::New();
	   for(int64_t i = 0;i<vmData.count;i++) {
		   //Sneaky way to pass pointers -- as doubles
		   
		   Handle<Value> vhand = Int32::New(currentPtr);
		   EmbeddedFuncPtr fptr;
		   fptr.function = functions[i];
		   fptr.instance = this;
		   HANDLE fhandle = LockFuncPtrs();
		   funcPtrs.push_back(fptr);
		   
		   currentPtr++;
		   UnlockFuncPtrs(fhandle);
		   
		   roothandle->Set(String::New(vmData.methodnames[i]),FunctionTemplate::New(InvokeManagedDelegate,vhand));
		   
	   }
	   
	   Persistent<Context> context = Context::New(NULL,roothandle);
	   Context::Scope context_scope(context);
	   Handle<String> srccode = String::New(source);
	   Handle<Script> script = Script::Compile(srccode);
	   script->Run();
	   
	   context.Dispose();
	}
};




//BEGIN EXTERN functions
extern "C" {
		
		//TODO: Fix memory leak on Deserialize
		
		
		
		__declspec(dllexport) void __cdecl __ClosePtr(VMInstance* instance,void* mray) {
			instance->memallocator->unallocMem(mray);
			
		}
		__declspec(dllexport) void __cdecl __CloseFuncPtr(VMInstance* instance,int ptr) {
			instance->jsFuncPtrs[ptr].mfunc.Dispose();
			instance->jsFuncPtrs.erase(ptr);
		}
		
	__declspec(dllexport) void* __cdecl NativeAlloc(VMInstance* instance,int32_t count) {
		//Allocate in chunks of 1024 bytes
		//starting at 1024
		int64_t blocksize = 1024;
		while(blocksize<count) {
			blocksize+=1024;
		}
		return instance->memallocator->allocMem(blocksize);
	//return malloc(count);
	}
		_declspec(dllexport) void __cdecl CallFunction(VMInstance* instance, char* args, int ptr) {
			int32_t count = *(int32_t*)args;
			int64_t offset = sizeof(int32_t);
			
			Handle<Value>* cray = (Handle<Value>*)NativeAlloc(instance,sizeof(Handle<Value>)*count);
			
			for(int32_t i = 0;i<count;i++) {
				cray[i] = instance->DeserializeObject(args,&offset);
			}
		
			Local<Context> ctxt = Context::GetCurrent();
			instance->jsFuncPtrs[ptr].mfunc->Call(ctxt->Global(),count,cray);
			for(int32_t i = 0;i<count;i++) {
				cray[i]->~Value();
			}
			
			__ClosePtr(instance,args);
			
			__ClosePtr(instance,cray);
			

		}




	struct ArrayPtr {
	void* data;
	int32_t count;
	};
	
	struct pHndl {
	Persistent<Context> context;
	};
	
	_declspec(dllexport) void* __cdecl CreateVM() {
		return new VMInstance();
	}
__declspec(dllexport) void __cdecl LoadVM(VMInstance* instance, long count, ManagedFuncPtr* functions, uint16_t** names, uint16_t* source) {
	
	instance->DoVMLoad(count,functions,names,source);
	   
};
__declspec(dllexport) void __cdecl Shutdown(pHndl* handle) {
	
};
}