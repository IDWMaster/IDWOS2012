using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace JSLib
{
    public delegate IntPtr ExpFunction(IntPtr[] args);
    public class NativeBoundary
    {
        static bool isInit = false;
        class RemotingBoundary
        {
            public string source;
        }
        static bool isrunning = true;
        
        static void beginsession(object sender)
        {
            try
            {
                Stream session = sender as Stream;
                if (taskpool.Count > 0)
                {
                    string task;
                    lock (taskpool)
                    {
                        task = taskpool[0];
                        taskpool.RemoveAt(0);
                    }
                    BinaryWriter mwriter = new BinaryWriter(session);
                    mwriter.Write(task);
                    mwriter.Flush();
                    while (isrunning)
                    {
                        BinaryReader mreader = new BinaryReader(session);
                        byte opcode = mreader.ReadByte();
                        if (opcode == 0)
                        {
                            Console.Write(mreader.ReadString());
                        }
                        if (opcode == 1)
                        {
                            CreateThread(mreader.ReadString());
                        }
                    }
                }
                else
                {
                    session.Close();
                }
            }
            catch (Exception er)
            {
            }
        }
        static List<string> taskpool = new List<string>();
        static void serverthread()
        {
            Console.WriteLine("Connecting to Distributed Supercomputer Network.....");
            TcpListener mlist = new TcpListener(IPAddress.Any,6553);
            mlist.Start();
            while (isrunning)
            {
                Stream session = mlist.AcceptTcpClient().GetStream();
                System.Threading.ThreadPool.QueueUserWorkItem(beginsession, session);
            }
        }
        static void thetar(object sender)
        {
            RemotingBoundary mbnd = sender as RemotingBoundary;
            Process mproc = new Process();
            mproc.StartInfo.FileName = "remotingWorker.exe";
            lock (taskpool)
            {
                taskpool.Add(mbnd.source);
            }
            if (!isInit)
            {
                isInit = true;
                System.Threading.Thread mthread = new System.Threading.Thread(serverthread);
                mthread.Start();
                System.Threading.Thread.Sleep(500);
            }
            mproc.Start();
        }
        public static void CreateThread(string source)
        {
            
            RemotingBoundary boundary = new RemotingBoundary();
            boundary.source = source;
            
            System.Threading.Thread mthread = new System.Threading.Thread(thetar);
            mthread.Start(boundary);
        }
    }
    public class Kernel
    {
        public static Stream ExecutionContext = null;
        /// <summary>
        /// OPCODE 0
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        IntPtr Write(int len, IntPtr args)
        {
            Console.Write(vm.Deserialize(len,args));

            
            return IntPtr.Zero;
            
        }
        Dictionary<int, IntPtr> registers = new Dictionary<int, IntPtr>();
        
        /// <summary>
        /// OPCODE 1
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        IntPtr CreateKernel(int len,IntPtr args)
        {
            string code = vm.Deserialize(len, args)[0] as string;
            if (ExecutionContext == null)
            {
                NativeBoundary.CreateThread(code);
            }
            else
            {
                BinaryWriter mwriter = new BinaryWriter(ExecutionContext);
                mwriter.Write((byte)1);
                mwriter.Write(code);
                mwriter.Flush();
            }
            return IntPtr.Zero;
        }
        ManualResetEvent mvent = new ManualResetEvent(false);
        void firetarget(object sender)
        {
            mpass mp = sender as mpass;
            System.Threading.Thread.Sleep(mp.timeout);
            
                interruptfunctions.Add(mp.function);
                mvent.Set();
            
        }
        class mpass
        {
            public int timeout;
            public JSFunctionPtr function;
        }
        IntPtr FireAt(int len,IntPtr args)
        {

            mpass mp = new mpass();
            object[] mjects = vm.Deserialize(len, args);
            mp.function = mjects[1] as JSFunctionPtr;
            mp.timeout = (int)mjects[0];
            
            ThreadPool.QueueUserWorkItem(firetarget, mp);
            return IntPtr.Zero;
            
        }
        List<JSFunctionPtr> interruptfunctions = new List<JSFunctionPtr>();
        List<JSFunctionPtr> deletememes = new List<JSFunctionPtr>();
        IntPtr KernelSpinWait(int len,IntPtr args)
        {
            vm.Deserialize(len, args);
            foreach (JSFunctionPtr et in deletememes)
            {
                vm.DisposeFunctionPtr(et);
            }
            deletememes.Clear();
            if (interruptfunctions.Count < 1)
            {
                mvent.Reset();
                mvent.WaitOne();
            }
            JSFunctionPtr func = interruptfunctions[0];
            interruptfunctions.RemoveAt(0);
            deletememes.Add(func);
            return vm.Serialize(this,func);    
        }
        #region External functions
        public List<ExpFunction> Funcmappings = new List<ExpFunction>();
        public List<MethodInfo> TranslatedFunctions = new List<MethodInfo>();
        #endregion
        #region Managed interop
        IntPtr InvokeMethod(int len, IntPtr args)
        {
            object[] targs = vm.Deserialize(len, args);
            int methodPtr = (int)targs[0];
            List<Object> methodArguments = new List<object>();
            for (int i = 1; i < targs.Length; i++)
            {
                methodArguments.Add(targs[i]);
            }
            
           return vm.Serialize(this,TranslatedFunctions[methodPtr].Invoke(null, methodArguments.ToArray()));
        }
        IntPtr ResolveMethodPtr(int len, IntPtr args)
        {
            string methid = vm.Deserialize(len, args)[0] as string;
            int i = 0;
            foreach (MethodInfo et in TranslatedFunctions)
            {
                if (et.Name == methid)
                {
                    return vm.Serialize(this,i);
                }
                i++;
            }
            return IntPtr.Zero;
        }
        #endregion
        #region JS reflection
        public int cpointer = 0;
        public Dictionary<int, object> objPtrs = new Dictionary<int, object>();
        IntPtr ResolveMethod(int count,IntPtr va)
        {
            object[] args = vm.Deserialize(count, va);
            string methodname = args[1] as string;
            object reflectedobject = objPtrs[(int)args[0]];
            objPtrs.Add(cpointer,reflectedobject.GetType().GetMethod(methodname));
            cpointer++;
            return vm.Serialize(this,cpointer - 1);
        }
        IntPtr InvokeDynamicMethod(int count, IntPtr va)
        {
            object[] args = vm.Deserialize(count, va);
            MethodInfo mfo = objPtrs[(int)args[1]] as MethodInfo;
            object[] targs = new object[args.Length-2];

            for(int i = 2;i<args.Length;i++) {
            targs[i-2] = args[i];
            }
            ParameterInfo[] rameters = mfo.GetParameters();
            for (int i = 0; i < targs.Length; i++)
            {
                if (targs[i].GetType() == typeof(int))
                {
                    if (rameters[i].ParameterType != typeof(int))
                    {
                        targs[i] = objPtrs[(int)targs[i]];
                    }
                }
            }
            IntPtr retval = IntPtr.Zero;
           
                return vm.Serialize(this,mfo.Invoke(objPtrs[(int)args[0]], targs));
               
          
        }
        #endregion
        public void Run(string code)
        {
            vm.Run(new JSFunction[] { Write,KernelSpinWait, FireAt, InvokeMethod, ResolveMethodPtr, ResolveMethod,InvokeDynamicMethod },code);
        }
       public JavaScriptVM vm;
        public void Initialize()
        {
            vm = new JavaScriptVM();
        }
        /// <summary>
        /// Creates a new virtual kernel
        /// </summary>
        public Kernel()
        {
            
        }
    }
}
