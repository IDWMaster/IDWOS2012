///<reference path="IDWOS_core.js" />
///IDWOS-LINKER-INCLUDE:IDWOS_core.js
//Program running in Ring 1
function onRecv(event) {
    if (event.OPCODE == undefined) {
    //Init script
        IDWOS.Threading.ThreadContext.Initialize(event);
        
        IDWOS.Threading.ThreadContext.SendMsg({ opcode: IDWOS.Kernel.OPCODE.OUTPUT_PRINT, text: '$ ' });
        function readlinecomplete(msg) {
            var handled = false;
            if (msg == 'exit') {
              
                Kill();
                handled = true;
            }
            var params = msg.split(' ');
            if (params[0] == 'malloc') {
                malloc(parseInt(params[1]));
                handled = true;
            }

            IDWOS.Threading.ThreadContext.SendMsg({ opcode: IDWOS.Kernel.OPCODE.OUTPUT_PRINT, text: '$ ' });

            IDWOS.Input.ReadLine(readlinecomplete);
            if (!handled) {
                var args = new Array();
                for (var i = 1; i < params.length; i++) {
                    args.push(params[i]);
                }
                IDWOS.Threading.ThreadContext.SendMsg({ opcode: 4, sourcefile: params[0]+'.js', arguments: args });

            }
        }
        IDWOS.Input.ReadLine(readlinecomplete);

    }else {
    //Message loop
        IDWOS.Input.DispatchMessage(event);
    }

}
setRecvDgate(onRecv);
InitEventLoop();