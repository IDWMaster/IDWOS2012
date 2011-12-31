///<reference path="IDWOS_platform.js" />
///IDWOS-LINKER-INCLUDE:IDWOS_platform.js
//BEGIN IDWOS CORE SPECIFICATION
CreateShaderFunction = null;
LoadTexture = null;
DrawVertexBuffer = null;
DrawShader = null;
DrawTexture = null;
PostMsg = null;
ClearDrawing = null;
currentID = 0;
var threads = { hasInitialized : false };
IDWOS = {
    Threading: {
    ///<summary>Provides multitasking functionality in a JavaScript environment</summary>
        ThreadContext: {
        
            
            Initialize: function (data) {
            ///<summary>Initializes a threading context for a worker thread</summary>
            ///<param name="data">The data parameter passed into this thread's starter function</param>
                KernelThreadID = data.ThreadID;
            },
            SendMsg: function (data) {
            ///<summary>Sends a notification to the kernel</summary>
            ///<param name="data">Data to be sent to the kernel</param>
                data.ThreadID = KernelThreadID;
                postMessage(data);
            }
            },
    Thread: function (scriptURL, threadSecurity) {
            ///<summary>Creates a new thread</summary>
            ///<param name="scriptURL">The relative URL of the script to execute</param>
            ///<param name="threadSecurity">true if the thread handles potentially sensitive information and needs to run on the local machine, false if the thread should be allowed to run on any machine.</param>
            this.OnDataReceived = function (data) {
                Write('ERR: No interceptor defined');
            };
            this.ptr = InvokeMethod(5, scriptURL);
            var threadID = currentID;
            currentID++;
            threads[threadID] = this;
            if (!threads.hasInitialized) {
                setRecvDgate(function (data) {
                   
                    threads[data.ThreadID].OnDataReceived(data);
                    
                });
                threads.hasInitialized = true;
            }
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
            
        }
    },
    Graphics: {
        DrawingContext:function(bitmap) {
        ///<summary>Creates a drawing context from a specified bitmap</summary>
        ///<param name="bitmap" type="IDWOS.Graphics.Bitmap">The bitmap to create the context from</param>
            this.ptr = InvokeMethod(6, bitmap.ptr);
        },
        Camera: function (_renderer) {
            this.renderer = _renderer;

        },
        Renderer: function () {
            this.ptr = InvokeMethod(0);
            this.Camera = new IDWOS.Graphics.Camera(this);

        },
        Bitmap: function (filename) {
        ///<summary>Creates a bitmap from a specified filename in system storage, or from a specified width and height</summary>
        ///<param name="filename">The filename, or a valid width and height</param>
            if (arguments.length == 1) {
                this.ptr = InvokeMethod(1, filename);
            }else {
             
                this.ptr = InvokeMethod(7, arguments[0], arguments[1]);
            }
        },
        Shader: function (renderer) {
            if (CreateShaderFunction == null) {
                CreateShaderFunction = ResolveMethod(renderer.ptr, 'createBasicShader');
            }
            this.ptr = InvokeDynamicMethod(renderer.ptr, CreateShaderFunction);
        },
        Texture2D: function (renderer, bitmap) {
        ///<summary>Creates a Texture2D from a bitmap</summary>
        ///<param name="renderer">The renderer</param>
        
          
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
UploadBitmapFunction = null;
IDWOS.Graphics.Texture2D.prototype.UploadBitmap = function (bitmap) {
///<summary>Uploads a bitmap to the GPU and stores it in this texture</summary>
///<param name="bitmap" type="IDWOS.Graphics.Bitmap">The bitmap to upload</param>
    if (UploadBitmapFunction == null) {
        UploadBitmapFunction = ResolveMethod(this.ptr, 'UploadBitmap');
    }
    InvokeDynamicMethod(this.ptr, UploadBitmapFunction, bitmap.ptr);
}
DrawImageFunction = null;
IDWOS.Graphics.DrawingContext.prototype.DrawImage = function (bitmap, x, y, width, height) {
    if (DrawImageFunction == null) {
        DrawImageFunction = ResolveMethod(this.ptr, 'DrawImage');
    }
    InvokeDynamicMethod(this.ptr,DrawImageFunction, bitmap.ptr, x, y, width, height);
}
IDWOS.Graphics.DrawingContext.prototype.Clear = function (a, r, g, b) {
///<summary>Clears the drawing canvas to the specified color (values between 0-255)</summary>
///<param name="a">The alpha channel</param>
///<param name="r">The red channel</param>
///<param name="g">The green channel</param>
///<param name="b">The blue channel</param>

    if (ClearDrawing == null) {
        ClearDrawing = ResolveMethod(this.ptr, 'Clear');
    }
    InvokeDynamicMethod(this.ptr, ClearDrawing,a,r,g,b);
}
DrawStringFunction = null;
IDWOS.Graphics.DrawingContext.prototype.DrawString = function (text, fontsize, a, r, g, b, x, y) {
    if (DrawStringFunction == null) {
        DrawStringFunction = ResolveMethod(this.ptr, 'DrawString');
    }
    InvokeDynamicMethod(this.ptr,DrawStringFunction, a, r, g, b, text, fontsize, x, y);
}
IDWOS.Graphics.VertexBuffer.prototype.Draw = function () {
    ///<summary>Draws the vertex buffer</summary>
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
