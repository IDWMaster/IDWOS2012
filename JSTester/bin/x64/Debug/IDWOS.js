///<reference path="WebGLLibs.js"/>
///<reference path="IDWOS_platform.js"/>
///<reference path="IDWOS_core.js"/>
///IDWOS-LINKER-INCLUDE:IDWOS_platform.js
///IDWOS-LINKER-INCLUDE:IDWOS_core.js











currentID = 0;
function main() {
    renderer = new IDWOS.Graphics.Renderer();
    renderer.Camera.SetPosition(0, 0, -2);
    var shader = new IDWOS.Graphics.Shader(renderer);
    shader.Draw();
    var bitmap = new IDWOS.Graphics.Bitmap('pic.jpg');
    var texture = new IDWOS.Graphics.Texture2D(renderer, bitmap);
    texture.Draw();
    var vertices = [
    0, 0, 0,
    1.5, 0, 0,
    1.5, 1, 0,
    //Triangle 2
    0, 0, 0,
    0, 1, 0,
    1.5, 1, 0
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
    var workerthread = new IDWOS.Threading.Thread('webworker.js',true);
    
    var intervalID = null;
    var ourID = currentID;
    workerthread.SetNtfyDataReceived(function (data) {
        
        if (data.ID == ourID) {
            if (data.cmd == 2) {
                intervalID = event.data.value;
            }else {
                vertbuffer.SetRotation(data.x, data.y, data.z);
            }
        }
    });
    workerthread.Start({ ID: ourID, cmd: 0, X: 0.0, Y: 0.0, Z: 0.0, cx: 0.0, cy: 0.01, cz: 0.0 });

    currentID++;
    InitEventLoop();
}