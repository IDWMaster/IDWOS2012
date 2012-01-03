///<reference path="IDWOS_core.js" />
///IDWOS-LINKER-INCLUDE:IDWOS_core.js
//Program running in Ring 1
function onRecv(event) {
    if (event.OPCODE == undefined) {
        //Init script
        IDWOS.Threading.ThreadContext.Initialize(event);
        IDWOS.Threading.ThreadContext.SendMsg({ opcode: 5 });
    }else {
        //Message loop
        if (event.OPCODE == 6) {
            var filestr = '';
            for (var i = 0; i < event.files.length; i++) {
                filestr += event.files[i] + '\n';
            }

            IDWOS.Threading.ThreadContext.SendMsg({ opcode: 0, text: filestr+event.files.length.toString()+' files found.\n$ ' });
            Kill();
        }
        IDWOS.Input.DispatchMessage(event);
    }
}
setRecvDgate(onRecv);
InitEventLoop();