///<reference path="IDWOS_platform.js"/>
///<reference path="IDWOS_core.js"/>
///IDWOS-LINKER-INCLUDE:IDWOS_core.js





//Kernel running in ring 0





currentID = 0;
function main() {
    renderer = new IDWOS.Graphics.Renderer();
    renderer.Camera.SetPosition(0, 0, 0);
    var shader = new IDWOS.Graphics.Shader(renderer);
    shader.Draw();
    var bitmap = new IDWOS.Graphics.Bitmap(512, 512);
    var drawingcontext = new IDWOS.Graphics.DrawingContext(bitmap);
    drawingcontext.Clear(255, 0, 0, 0);
    drawingcontext.DrawString('Hello world!', 12, 255, 255, 255, 255, 0, 60);
    
    free(drawingcontext.ptr);
    var kerneltexture = new IDWOS.Graphics.Texture2D(renderer, bitmap);
    
    kerneltexture.Draw();
    var vertices = [
    -1, -1, -1,
    1, -1, -1,
    1, 1, -1,
    //Triangle 2
    -1, -1, -1,
    -1, 1, -1,
    1, 1, -1
    ];
    var texcoords = [
    0, 0,
    1, 0,
    1, 1,
    0, 0,
    0, 1,
    1, 1
    ];
    var normals = new Array();
    for (var i = 0; i < vertices.length; i++) {
        normals.push(1);
    }
    var vertbuffer = new IDWOS.Graphics.VertexBuffer(renderer, vertices, texcoords, normals);
    vertbuffer.Draw();
    var X = 0.0;
    var Y = 0.0;
    var Z = 0.0;
    var dorender = function () {
        vertbuffer.SetRotation(X, Y, Z);
        Y += .01;
        FireAt(10, dorender);

    }
  
    var intervalID = null;
    var ourID = currentID;
  
    //BEGIN KERNEL INPUT/OUTPUT LOGIC
    
   
    var ltxt = '=====================================================\nIDWOS 2012 Runtime Environment\n=====================================================';
    ltxt += '\nDISCLAIMER: THIS SYSTEM CURRENTLY SUPPORTS ONLY ENGLISH KEYBOARDS.\nI do not currently have the budget to hire people who could implement this for other keyboard types.\nIf you are offended by the lack of support for your keyboard, please\ncontribute your keyboard layout at https://github.com/IDWMaster/IDWOS2012\nor e-mail webadm@elcnet.servehttp.com.\n';
    var currentFS = new IDWOS.IO.FileSystem();
    var ison = true;
    var doupdate = function () {
        var kernelIOBitmap = new IDWOS.Graphics.Bitmap(1024, 1024);
        var graphicsContext = new IDWOS.Graphics.DrawingContext(kernelIOBitmap);
        graphicsContext.Clear(255, 0, 0, 0);
        if (ison) {
            ison = false;
            graphicsContext.DrawString(ltxt + '_', 12, 255, 255, 255, 255, 0, 95);
        }else {
            graphicsContext.DrawString(ltxt, 12, 255, 255, 255, 255, 0, 95);
            ison = true;
        }
        free(graphicsContext.ptr);
        kerneltexture.UploadBitmap(kernelIOBitmap);

        weaken(kernelIOBitmap.ptr);
        
    };
    setInterval(doupdate, 200);
    var workerthread = new IDWOS.Threading.Thread('webworker.js', true);
    var keyDownInterrupt = function (keyname) {
        var keyval = '';
        var signal = false;
        if (keyname == 'Back') {
            signal = true;
            keyval = '\b';
        }
        if (keyname == 'Space') {
            signal = true;
            keyval = ' ';
        }
        if (keyname == 'Enter') {
            signal = true;
            keyval = '\n';
        }
        if (keyname.indexOf('0') > -1) {
            signal = true;
            keyval = '0';
        }
        if (keyname.indexOf('1') > -1) {
            signal = true;
            keyval = '1';
        }
        if (keyname.indexOf('2') > -1) {
            signal = true;
            keyval = '2';
        }
        if (keyname.indexOf('3') > -1) {
            signal = true;
            keyval = '3';
        }
        if (keyname.indexOf('4') > -1) {
            signal = true;
            keyval = '4';
        }
        if (keyname.indexOf('5') > -1) {
            signal = true;
            keyval = '5';
        }
        if (keyname.indexOf('6') > -1) {
            signal = true;
            keyval = '6';
        }
        if (keyname.indexOf('7') > -1) {
            signal = true;
            keyval = '7';
        }
        if (keyname.indexOf('8') > -1) {
            signal = true;
            keyval = '8';
        }
        if (keyname.indexOf('9') > -1) {
            signal = true;
            keyval = '9';
        }
        if (keyname.toLowerCase().indexOf('shift') > -1) {
            signal = true;
            shiftkeydown = true;
            
        }
        if (!signal) {
            if (shiftkeydown) {
                keyval = keyname.toUpperCase();
            }else {
                keyval = keyname.toLowerCase();
               
            }
        }
        doupdate();

        workerthread.SendMsg({ OPCODE: IDWOS.Kernel.OPCODE.INPUT_KEYDOWN, inputdata: keyval });
    }
    var keyUpInterrupt = function (keyname) {
        var signal = false;
        if (keyname.toLowerCase().indexOf('shift') > -1) {
            shiftkeydown = false;
            signal = true;
        }
    }
    InvokeMethod(8, keyDownInterrupt);
    InvokeMethod(9, keyUpInterrupt);
    //END KERNEL INPUT/OUTPUT LOGIC
   
 
    var datrecv = function (data) {
        //Kernel request -- Write to console

        if (data.opcode == 0) {
            if (data.text == '\b') {
                ltxt = ltxt.substring(0, ltxt.length - 1);
                doupdate();
                return;
            }
            ltxt += data.text;
            doupdate();
        }
        if (data.opcode == 3) {

            delete threads[data.ThreadID];
            threads.length -= 1;
            if (threads.length == 0) {
                Shutdown();
            }
        }
        if (data.opcode == 4) {
            var files = currentFS.GetFiles();
            var found = false;
            for (var i = 0; i < files.length; i++) {
                if (files[i] == data.sourcefile) {
                    found = true;
                    break;
                }
            }
            if (found) {
                var tthread = new IDWOS.Threading.Thread(data.sourcefile);
                tthread.OnDataReceived = datrecv;
                tthread.Start({ arguments: data.arguments });
            }else {
                ltxt += 'ERR: Program '+data.sourcefile+' not found.\n$ ';
            }
        }
        if (data.opcode == 5) {
            var files = currentFS.GetFiles();
            
            threads[data.ThreadID].SendMsg({ OPCODE: 6, files: files });


        }
        if (data.opcode == 7) {
            currentFS.CreateFile(data.filename);
        }
    }
    workerthread.OnDataReceived = datrecv;
    
    workerthread.Start({ arguments: 0 });
  
    InitEventLoop();
}
shiftkeydown = false;
