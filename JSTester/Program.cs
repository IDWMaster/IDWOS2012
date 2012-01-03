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
using IC80v3;
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
            renderer.defaultKeyboard.onKeyDown += new keyboardeventargs(defaultKeyboard_onKeyDown);
            renderer.defaultKeyboard.onKeyUp += new keyboardeventargs(defaultKeyboard_onKeyUp);
            
            renderer.cameraPosition.Z = -5;
            return renderer;
        }
        static JSFunctionPtr dgate_keyup;
        static void defaultKeyboard_onKeyUp(string KeyName)
        {
            if (dgate_keyup != null)
            {
                mnul.DispatchFunction(dgate_keyup, false, KeyName);
            }
        }
        static JSFunctionPtr dgate_keydown;
        static void defaultKeyboard_onKeyDown(string KeyName)
        {
            if (dgate_keydown != null)
            {
                mnul.DispatchFunction(dgate_keydown, false, KeyName);
            }
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
        class ManagedGraphics : IDisposable
        {
            Bitmap imap;
            public ManagedGraphics(Bitmap mmap)
            {
                imap = mmap;
                mfix = Graphics.FromImage(mmap);
            }
            public void Clear(int a, int r, int b, int g)
            {
                mfix.Clear(Color.FromArgb(a, r, g, b));
            }
            public void DrawString(int a, int r, int g, int b, string text, float size, int x, int y)
            {
                Font mfont = new Font(FontFamily.GenericMonospace, size);
                SizeF msize = mfix.MeasureString(text+"_", mfont);
                msize.Height += y;
                int cy = y;
                if (msize.Height > imap.Height-95)
                {
                    cy -= ((int)msize.Height-imap.Width)+95;
                }
                
                SolidBrush mbrush = new SolidBrush(Color.FromArgb(a, r, g, b));
                mfix.DrawString(text, mfont, mbrush, new PointF(x, cy));
                mbrush.Dispose();
                mfont.Dispose();

            }
            public void FillRect(int a, int r, int g, int b, int x, int y, int w, int h)
            {
                SolidBrush mbrush = new SolidBrush(Color.FromArgb(a, r, g, b));
                mfix.FillRectangle(mbrush, new Rectangle(x, y, w, h));
                mbrush.Dispose();
            }
            public void DrawImage(Bitmap mmap, int x, int y, int width, int height)
            {
                mfix.DrawImage(mmap, new Rectangle(x, y, width, height));

            }
            Graphics mfix;
            public void Dispose()
            {
                mfix.Dispose();
            }
        }
        /// <summary>
        /// OPCODE 6
        /// </summary>
        /// <param name="mmap"></param>
        /// <returns></returns>

        static ManagedGraphics CreateGraphics(int ptr)
        {
            return new ManagedGraphics(mnul.objPtrs[ptr] as Bitmap);

        }
        /// <summary>
        /// OPCODE 7
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        static Bitmap createBitmapFromWidthHeight(int width, int height)
        {
            return new Bitmap(width, height);
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
            ManualResetEvent mvent = new ManualResetEvent(false);
            void thetar(object sender)
            {
                StreamReader mreader = new StreamReader(sender as String);
                string code = Link(mreader.ReadToEnd());
                mreader.Dispose();
                mkernl = new Kernel();
                mkernl.parent = mnul;
                mkernl.Initialize();
                mvent.Set();
                mkernl.Run(code);
            }
            public VirtualThread(string src)
            {
                System.Threading.Thread mthread = new Thread(thetar);
                mthread.Start(src);
                mvent.WaitOne();
            }
            public void postMessage(object data)
            {
                mkernl.DispatchFunction(null,false, data);
            }
            
            
        }
        /// <summary>
        /// OPCODE 8
        /// </summary>
        /// <param name="funcptr"></param>
        static void onKeyPress(JSFunctionPtr funcptr)
        {
            dgate_keydown = funcptr;
        }
        /// <summary>
        /// OPCODE 9
        /// </summary>
        /// <param name="funcptr"></param>
        static void onKeyUp(JSFunctionPtr funcptr)
        {
            dgate_keyup = funcptr;

        }
        class ManagedStream
        {
            Stream _underlyingstream;
            public ManagedStream(Stream internstream)
            {
                _underlyingstream = internstream;

            }
            public void Dispose()
            {
                _underlyingstream.Dispose();
            }
        }
        class ManagedFS
        {
            IndexedFS underlyingFS;
            public ManagedFS()
            {
                
                underlyingFS = new IndexedFS(new Filesystem(File.Open("filesystem", FileMode.OpenOrCreate, FileAccess.ReadWrite), 1024 * 1024 * 5, 1024 * 1024 * 50));
                
            }
            public void Dispose()
            {
                underlyingFS.Dispose();
            }
            public void CreateFile(string filename)
            {
                underlyingFS.CreateFile(filename);

            }
            public ManagedStream OpenFile(string filename)
            {
                return new ManagedStream(underlyingFS.OpenFile(filename));
            }
            public object[] GetDirectories()
            {
                List<object> directories = new List<object>();
                foreach (string et in underlyingFS.Directories)
                {
                    directories.Add(et);
                }
                return directories.ToArray();

            }
            public object[] GetFiles()
            {
                List<object> files = new List<object>();
                foreach (string et in underlyingFS.Files)
                {
                    files.Add(et);
                }
                foreach (string et in Directory.GetFiles(Environment.CurrentDirectory))
                {
                    files.Add(et.Substring(et.LastIndexOf("\\")+1));
                }
                return files.ToArray();
            }
        }
        /// <summary>
        /// OPCODE 10
        /// </summary>
        /// <returns></returns>
        static ManagedFS CreateFS()
        {
            return new ManagedFS();
        }
        static void Main(string[] args)
        {
            Console.WriteLine("DistVM - Secure Execution Environment");
            
            StreamReader mreader = new StreamReader("IDWOS.js");

            mnul.TranslatedFunctions.AddRange(new MethodInfo[] { ResolveMethod("CreateRenderer"), ResolveMethod("CreateBitmap"),ResolveMethod("CreateVertexBuffer"), ResolveMethod("RotateBuffer"), ResolveMethod("SetCameraPosition"),ResolveMethod("createThread"), ResolveMethod("CreateGraphics"),ResolveMethod("createBitmapFromWidthHeight"), ResolveMethod("onKeyPress"),ResolveMethod("onKeyUp"),ResolveMethod("CreateFS") });
            mnul.Initialize();
            vm = mnul.vm;
            string code = Link(mreader.ReadToEnd() + "\nmain();");
      
            mnul.Run(code);
            
        }
    }
    
}
