///<reference path="IDWOS_platform.js" />
///IDWOS-LINKER-INCLUDE:IDWOS_platform.js
//BEGIN IDWOS CORE SPECIFICATION
CreateShaderFunction = null;
LoadTexture = null;
DrawVertexBuffer = null;
DrawShader = null;
DrawTexture = null;
PostMsg = null;
currentID = 0;
IDWOS = {
    Threading: {
    ///<summary>Provides multitasking functionality in a JavaScript environment</summary>
        Thread: function (scriptURL, threadSecurity) {
            ///<summary>Creates a new thread</summary>
            ///<param name="scriptURL">The relative URL of the script to execute</param>
            ///<param name="threadSecurity">true if the thread handles potentially sensitive information and needs to run on the local machine, false if the thread should be allowed to run on any machine.</param>

            this.ptr = InvokeMethod(5, scriptURL);
            var threadID = currentID;
            currentID++;

            this.Start = function (data) {
                ///<summary>Instructs the operating system to transition the thread to an executing state</summary>
                ///<param name="data">Serializable data to send to the remote thread</param>
                data.ThreadID = threadID;
                if (PostMsg == null) {
                    PostMsg = ResolveMethod(this.ptr,'postMessage');
                }
                InvokeDynamicMethod(this.ptr, PostMsg, data);
            }
            var mvent = null;
            this.SetNtfyDataReceived = function (callback) {
                ///<summary>Sets a callback function to be called when data is received</summary>
                ///<param name="callback">The callback function</param>
                mvent = callback;

            }
            
            this.ptr.onmessage = function (data) {
                if (data.data.ThreadID == threadID) {
                    mvent(data.data);
                }
            }
        }
    },
    Graphics: {
        Camera: function (_renderer) {
            this.renderer = _renderer;

        },
        Renderer: function () {
            this.ptr = InvokeMethod(0);
            this.Camera = new IDWOS.Graphics.Camera(this);

        },
        Bitmap: function (filename) {
            this.ptr = InvokeMethod(1, filename);
        },
        Shader: function (renderer) {
            if (CreateShaderFunction == null) {
                CreateShaderFunction = ResolveMethod(renderer.ptr, 'createBasicShader');
            }
            this.ptr = InvokeDynamicMethod(renderer.ptr, CreateShaderFunction);
        },
        Texture2D: function (renderer, bitmap) {
            if (LoadTexture == null) {
                LoadTexture = ResolveMethod(renderer.ptr, 'createTextureFromBitmap');

            }
            this.ptr = InvokeDynamicMethod(renderer.ptr, LoadTexture, bitmap.ptr);
        },
        VertexBuffer: function (renderer, vertices, texcoords, normals) {
            this.ptr = InvokeMethod(2, renderer.ptr, vertices, texcoords, normals);
        }
    }

}
IDWOS.Graphics.VertexBuffer.prototype.Draw = function () {
    if (DrawVertexBuffer == null) {
        DrawVertexBuffer = ResolveMethod(this.ptr, 'Draw');
    }
    InvokeDynamicMethod(this.ptr, DrawVertexBuffer);
}
IDWOS.Graphics.Shader.prototype.Draw = function () {
    if (DrawShader == null) {
        DrawShader = ResolveMethod(this.ptr, 'Draw');
    }
    InvokeDynamicMethod(this.ptr, DrawShader);
}
IDWOS.Graphics.Camera.prototype.SetPosition = function (X, Y, Z) {
    InvokeMethod(4, this.renderer.ptr, X, Y, Z);
}
IDWOS.Graphics.Texture2D.prototype.Draw = function () {
    if (DrawTexture == null) {
        DrawTexture = ResolveMethod(this.ptr, 'Draw');

    }
    InvokeDynamicMethod(this.ptr, DrawTexture);
}
IDWOS.Graphics.VertexBuffer.prototype.SetRotation = function (X, Y, Z) {
    InvokeMethod(3, this.ptr, X, Y, Z);
}
//END IDWOS CORE SPECIFICATION
