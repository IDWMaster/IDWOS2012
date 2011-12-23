using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace JSLib
{
    /// <summary>
    /// Represents a pointer to a JavaScript function
    /// </summary>
    public class JSFunctionPtr
    {
        internal int value;
        internal JSFunctionPtr(int _val)
        {
            value = _val;
        }
    }
    public delegate IntPtr JSFunction(int len,IntPtr args);
    public class JavaScriptVM:IDisposable
    {
        IntPtr inst;
        [DllImport("JSInteropLib64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern void LoadVM(IntPtr instance,long count, IntPtr[] callbacks, string[] methnames, string source);
        [DllImport("JSInteropLib64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern void __ClosePtr(IntPtr instance, IntPtr handle);
        [DllImport("JSInteropLib64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern void __CloseFuncPtr(IntPtr instance, int handle);
        [DllImport("JSInteropLib64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr NativeAlloc(IntPtr instance, int bytes);
        [DllImport("JSInteropLib64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr CreateVM();
        public IntPtr Malloc(int bytes)
        {
            return NativeAlloc(inst,bytes);
        }
        public void DisposeFunctionPtr(JSFunctionPtr ptr)
        {
            __CloseFuncPtr(inst,ptr.value);
        }
        public void DeletePtr(IntPtr ptr)
        {
            __ClosePtr(inst,ptr);
        }
        
        
        
        Thread getCurrentExecutionContext
        {
            get
            {
                return Thread.CurrentThread;
            }
        }
        bool TypeCompare(Type a, Type b)
        {
            return a == b;
        }
        void SerializeManaged(BinaryWriter mwriter, Kernel kernel, object value)
        {
            if (value == null)
            {
                mwriter.Write((byte)2);
                mwriter.Write((int)0);
                return;
            }
            Type reflectedtype = value.GetType();
            if (reflectedtype == typeof(double))
            {
                mwriter.Write((byte)5);
                mwriter.Write((double)value);
                return;

            }
            if (reflectedtype == typeof(int))
            {
                mwriter.Write((byte)2);
                mwriter.Write((int)value);
                return;
            }
            if (reflectedtype == typeof(object[]))
            {
                mwriter.Write((byte)1);
                object[] mray = value as object[];
                mwriter.Write((uint)mray.Length);
                foreach (object et in mray)
                {
                    SerializeManaged(mwriter,kernel, et);
                }
                return;
            }
            if (reflectedtype == typeof(string))
            {
                mwriter.Write((byte)3);
                byte[] strdat = Encoding.UTF8.GetBytes(value as string);
                mwriter.Write(strdat.Length);
                mwriter.Write(strdat);
                return;

            }
            if (reflectedtype == typeof(JSFunctionPtr))
            {
                mwriter.Write((byte)4);
                mwriter.Write((value as JSFunctionPtr).value);
                return;
            }
            //No matches!
            kernel.objPtrs.Add(kernel.cpointer, value);
            mwriter.Write((byte)2);
            mwriter.Write(kernel.cpointer);
            kernel.cpointer++;
        }
        public IntPtr Serialize(Kernel kern,params object[] args)
        {
            MemoryStream mstream = new MemoryStream();
            BinaryWriter mwriter = new BinaryWriter(mstream);
            mwriter.Write(args.Length);
         
            foreach(object et in args) {
                SerializeManaged(mwriter,kern, et);
            }
            byte[] data = mstream.ToArray();
            IntPtr retval = NativeAlloc(inst,data.Length);
            Marshal.Copy(data, 0, retval, data.Length);
            return retval;
        }
        public object[] Deserialize(int count, IntPtr args)
        {
            byte[] data = new byte[count];
            System.Runtime.InteropServices.Marshal.Copy(args, data, 0, data.Length);
            DeletePtr(args);
            MemoryStream mstream = new MemoryStream(data);
            List<object> mstrs = new List<object>();
            while (mstream.Position != mstream.Length)
            {
                mstrs.Add(DeserializeManaged(mstream));
            }
            return mstrs.ToArray();
        }
        object DeserializeManaged(Stream data)
        {
            BinaryReader mreader = new BinaryReader(data);
            object baseobj = null;
            byte opcode = mreader.ReadByte();
            if (opcode == 0)
            {
                int count = mreader.ReadInt32();
                Dictionary<string, object> mstr = new Dictionary<string, object>();
                for (int i = 0; i < count; i++)
                {
                    int clen = mreader.ReadInt32();
                    mstr.Add(Encoding.UTF8.GetString(mreader.ReadBytes(clen)), DeserializeManaged(data));
                }
                baseobj = mstr;
            }
            if (opcode == 1)
            {
                baseobj = new object[mreader.ReadUInt32()];
                for (int i = 0; i < (baseobj as object[]).Length; i++)
                {
                    (baseobj as object[])[i] = DeserializeManaged(data);
                }
                
            }
            if (opcode == 2)
            {
                baseobj = mreader.ReadInt32();
            }
            if (opcode == 3)
            {
                baseobj = Encoding.UTF8.GetString(mreader.ReadBytes(mreader.ReadInt32()));
            }
            if (opcode == 4)
            {
                baseobj = new JSFunctionPtr(mreader.ReadInt32());
            }
            if (opcode == 5)
            {
                baseobj = mreader.ReadDouble();
            }
            return baseobj;
        }
        public void Run(Delegate[] registers, string source)
        {
            inst = CreateVM();
            List<string> regnames = new List<string>();
            List<IntPtr> callbacks = new List<IntPtr>();
            foreach (Delegate et in registers)
            {
                
                callbacks.Add(Marshal.GetFunctionPointerForDelegate(et));
                regnames.Add(et.Method.Name);
            }
            
            LoadVM(inst,registers.Length, callbacks.ToArray(), regnames.ToArray(), source);
        }
       unsafe struct NativeArray
        {
            
            public IntPtr[] values;
            public int count;
        }
        IntPtr cptr;
        Thread activeThread;
        public JavaScriptVM()
        {
            activeThread = Thread.CurrentThread;
            
        }


        public void Dispose()
        {
            
        }
    }
}
