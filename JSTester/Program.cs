using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using JSLib;
using System.IO;
using System.Threading;
using _3DAPI;
using _3DLib_OpenGL;
using System.Drawing;
using System.Reflection;
using DirectXLib;
namespace JSTester
{
   
    class Program
    {
        #region Interop helpers
        public static MethodInfo ResolveMethod(string name)
        {
            return typeof(Program).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Static);
        }
        #endregion
        #region 3D interop
        static Renderer CreateRenderer()
        {

            Renderer renderer;
            try
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    throw new Exception("For debugging purposes");
                }
                renderer = new DirectEngine();
              
            }
            catch (Exception er)
            {
                renderer = new GLRenderer();
            }
            renderer.cameraPosition.Z = -5;
            return renderer;
        }
        static Bitmap CreateBitmap(string filename)
        {
            return new Bitmap(filename);
        }
        static void ConvertFloat(object[] mray)
        {
            
            for (int i = 0; i < mray.Length; i++)
            {
                if (mray[i].GetType() == typeof(int))
                {
                    mray[i] = (float)(int)(mray[i]);
                }
                else
                {
                    mray[i] = (float)(double)(mray[i]);
                }
            }
        }
        static VirtualThread createThread(string src)
        {
            return new VirtualThread(src);
        }
        static VertexBuffer CreateVertexBuffer(int rptr, object[] vertices, object[] texcoords, object[] normals)
        {
            Renderer renderer = mnul.objPtrs[rptr] as Renderer;
            List<Vector3D> v = new List<Vector3D>();
            List<Vector2D> t = new List<Vector2D>();
            List<Vector3D> n = new List<Vector3D>();
            ConvertFloat(vertices);
            ConvertFloat(texcoords);
            ConvertFloat(normals);
            for (int i = 0; i < vertices.Length; i += 3)
            {
                v.Add(new Vector3D((float)vertices[i],(float)vertices[i+1],(float)vertices[i+2]));

            }
            for (int i = 0; i < texcoords.Length; i += 2)
            {
                t.Add(new Vector2D((float)texcoords[i], (float)texcoords[i + 1]));
            }
            for (int i = 0; i < normals.Length; i += 3)
            {
                n.Add(new Vector3D((float)normals[i], (float)normals[i + 1], (float)normals[i + 2]));

            }
            return renderer.CreateVertexBuffer(v.ToArray(), t.ToArray(), n.ToArray());
            
        }
        static void RotateBuffer(int ptr,double X, double Y, double Z)
        {
            VertexBuffer vertbuffer = mnul.objPtrs[ptr] as VertexBuffer;
            vertbuffer.rotation = new Vector3D((float)X, (float)Y, (float)Z);
        }
        static void SetCameraPosition(int ptr, double X, double Y, double Z)
        {
            Renderer vertbuffer = mnul.objPtrs[ptr] as Renderer;
            vertbuffer.cameraPosition = new Vector3D((float)X, (float)Y, (float)Z);
        }
        #endregion
        static JavaScriptVM vm;
        static Kernel mnul = new Kernel();
        static string Link(string code)
        {
            string linkedcode = code;
            string linkstr = "IDWOS-LINKER-INCLUDE:";
            for (int i = 0; i < linkedcode.Length; )
            {
                int offset = linkedcode.IndexOf("IDWOS-LINKER-INCLUDE:",i);
                if (offset < 0)
                {
                   
                    break;
                }
                else
                {
                    int endoffset = linkedcode.IndexOf("\n", offset) - (offset + linkstr.Length);
                    string resolvepath = linkedcode.Substring(offset + linkstr.Length, endoffset);
                    StreamReader mreader = new StreamReader(resolvepath.Replace("\n","").Replace("\r",""));
                    linkedcode = linkedcode.Replace(linkstr + resolvepath, Link(mreader.ReadToEnd()));
                    mreader.Dispose();
                    i = endoffset-5;
                }
            }
            return linkedcode;
        }
        class VirtualThread
        {
            Kernel mkernl;
            void thetar(object sender)
            {
                StreamReader mreader = new StreamReader(sender as String);
                string code = Link(mreader.ReadToEnd());
                mreader.Dispose();
                mkernl = new Kernel();
                mkernl.Initialize();
                mkernl.Run(code);
            }
            public VirtualThread(string src)
            {
                System.Threading.Thread mthread = new Thread(thetar);
                mthread.Start(src);
            }
            public void postMessage(object data)
            {
                throw new NotImplementedException("TODO: Implement this");
            }
        }
       
        static void Main(string[] args)
        {
            Console.WriteLine("IDOWS 2012 - Secure Execution Environment");
            foreach (MethodInfo et in typeof(Program).GetMethods())
            {
                Console.WriteLine(et);
            }
            StreamReader mreader = new StreamReader("IDWOS.js");

            mnul.TranslatedFunctions.AddRange(new MethodInfo[] { ResolveMethod("CreateRenderer"), ResolveMethod("CreateBitmap"),ResolveMethod("CreateVertexBuffer"), ResolveMethod("RotateBuffer"), ResolveMethod("SetCameraPosition"),ResolveMethod("createThread") });
            mnul.Initialize();
            vm = mnul.vm;
            string code = Link(mreader.ReadToEnd() + "\nmain();");
            
            mnul.Run(code);
            
        }
    }
    
}
