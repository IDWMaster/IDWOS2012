///<reference path="IDWOS_core.js" />
commands = new Array();
onmessage = function (event) {
    var input = event.data;
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
                ID: event.data.ID
            }
            output.ThreadID = input.ThreadID;
            postMessage(output);
        }
        var ival = setInterval(dorotate, input.period);
        var outputdata = {
            cmd: 2,
            value: ival,
            ID: event.data.ID

        };
        postMessage(outputdata);
    }
    //End periodic rotation
}