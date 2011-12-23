// Win32JSInterop.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <v8.h>
#pragma comment(lib, "wsock32.lib")
using namespace v8;
Handle<Value> callback(const Arguments& args) {
	String::AsciiValue mvalue(args[0]);
	printf(*mvalue);
	
	return Int32::New(0);
};
int _tmain(int argc, _TCHAR* argv[])
{
	 // Create a stack-allocated handle scope.
  HandleScope handle_scope;

  //Define external functions
    Handle<ObjectTemplate> temphndle  = ObjectTemplate::New();
  
  temphndle->Set("printf",FunctionTemplate::New(&callback));
  //Create VM execution context
  Persistent<Context> context = Context::New(NULL,temphndle);
  
  // Enter the created context for compiling and
  // running the script.
  Context::Scope context_scope(context);

  // Create a string containing the JavaScript source code.
  Handle<String> source = String::New("function helloworld() {\nprintf('Hello world!');\n}\nhelloworld();");

  // Compile the source code.
  Handle<Script> script = Script::Compile(source);
  
  // Run the script to get the result.
  Handle<Value> result = script->Run();
  

  // Dispose the persistent context.
  context.Dispose();
  
  return 0;
}

