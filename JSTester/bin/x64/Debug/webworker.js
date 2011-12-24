///<reference path="IDWOS_core.js" />
///IDWOS-LINKER-INCLUDE:IDWOS_core.js
commands = new Array();
//In regular browsers, this is onmessage = function(event)
//In a browser, to get actual event data
//we wouldneed to ddeclare input = event.data instead of input = event;
setRecvDgate(function (event) {
    var input = event;
    //Periodic rotation
   
    if (input.cmd == 0) {
       
        var X = input.X;
        var Y = input.Y;
        var Z = input.Z;
        var cx = input.cx;
        var cy = input.cy;
        var cz = input.cz;
        function dorotate() {
            X += cx;
            Y += cy;
            Z += cz;
            var output = {
                cmd: 1,
                x: X,
                y: Y,
                z: Z,
                ID: event.ID
            }
            output.ThreadID = input.ThreadID;
            postMessage(output);
        }
        var ival = setInterval(dorotate, input.period);
        var outputdata = {
            cmd: 2,
            value: ival,
            ID: event.ID

        };
        postMessage(outputdata);
    }
    //End periodic rotation
});
InitEventLoop();