///<reference path="IDWOS_platform.js"/>
///<reference path="IDWOS_core.js"/>
///IDWOS-LINKER-INCLUDE:IDWOS_core.js











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
    
    var keyDownInterrupt = function (keyname) {
        
        var signal = false;
        if (keyname == 'Back') {
            signal = true;
            ltxt = ltxt.substring(0, ltxt.length - 1);
        }
        if (keyname == 'Space') {
            signal = true;
            ltxt += ' ';
        }
        if (keyname == 'Enter') {
            signal = true;
            ltxt += '\n';
        }
        if (!signal) {
            ltxt += keyname.toLowerCase();
        }
        doupdate();
    }
    var keyUpInterrupt = function (keyname) {

    }
    InvokeMethod(8, keyDownInterrupt);
    
    //END KERNEL INPUT/OUTPUT LOGIC
   
    var workerthread = new IDWOS.Threading.Thread('webworker.js', true);
    workerthread.OnDataReceived = function (data) {
        //Kernel request -- Write to console

        if (data.opcode == 0) {
            ltxt += data.text;
            doupdate();
        }
    }
    workerthread.Start({ arguments: 0 });
    
    InitEventLoop();
}