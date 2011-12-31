///<reference path="IDWOS_core.js" />
///IDWOS-LINKER-INCLUDE:IDWOS_core.js
commands = new Array();
//In regular browsers, this is onmessage = function(event)
//In a browser, to get actual event data
//we wouldneed to ddeclare input = event.data instead of input = event;

function onRecv(event) {
   
    IDWOS.Threading.ThreadContext.Initialize(event);

    IDWOS.Threading.ThreadContext.SendMsg({ opcode: 0, text: 'Hello world (from a running program)!' });
}
setRecvDgate(onRecv);
InitEventLoop();