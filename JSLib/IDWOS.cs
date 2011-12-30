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
            object[] realargs = vm.Deserialize(len,args);
            Console.Write(realargs[0]);

            
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

            JSMarshaller marshal = new JSMarshaller();
            marshal.ptr = mp.function;
            marshal.args = new object[0];
                interruptfunctions.Add(marshal);
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
        class JSMarshaller
        {
            public JSFunctionPtr ptr;
            public object[] args;
            public bool DeleteAfterExecution = true;
        }
        List<JSMarshaller> interruptfunctions = new List<JSMarshaller>();
        List<JSFunctionPtr> deletememes = new List<JSFunctionPtr>();
        public JSFunctionPtr recvDgate;
        IntPtr setRecvDgate(int len, IntPtr args)
        {
            if (recvDgate != null)
            {
                vm.DisposeFunctionPtr(recvDgate);
                recvDgate = null;
            }
            recvDgate = vm.Deserialize(len, args)[0] as JSFunctionPtr;
            return IntPtr.Zero;
            
        }
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
            JSMarshaller func = interruptfunctions[0];
            if (func.ptr == null)
            {
                vm.InvokeMethod(recvDgate, this, func.args);
            
            }
            else
            {
                vm.InvokeMethod(func.ptr, this, func.args);
            }
            interruptfunctions.RemoveAt(0);
            if (func.DeleteAfterExecution)
            {
                deletememes.Add(func.ptr);
            }
            return IntPtr.Zero;    
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
                    if (rameters[i].ParameterType != typeof(int) & rameters[i].ParameterType !=typeof(float))
                    {
                        targs[i] = objPtrs[(int)targs[i]];
                    }
                }
            }
            IntPtr retval = IntPtr.Zero;
           
                return vm.Serialize(this,mfo.Invoke(objPtrs[(int)args[0]], targs));
               
          
        }
        bool eventthreadrunning = false;
        class JSEvent
        {
            public int timeout;
            public JSMarshaller marshaller;
            public bool repeating = false;
        }
        List<JSEvent> events = new List<JSEvent>();
        void eventthread()
        {
            while (true)
            {
                if (events.Count > 0)
                {
                    JSEvent tvent = null;
                    int min = int.MaxValue;
                    for (int i = 0; i < events.Count; i++)
                    {
                        if (events[i].timeout <= min)
                        {
                            min = events[i].timeout;
                            tvent = events[i];
                        }
                    }
                    if (!tvent.repeating)
                    {
                        events.Remove(tvent);
                    }
                    eventTimer.WaitOne(tvent.timeout);
                    interruptfunctions.Add(tvent.marshaller);
                    mvent.Set();
                    eventTimer.Reset();
                }
                else
                {
                    eventTimer.Reset();
                    eventTimer.WaitOne();
                }
            }
        }
        ManualResetEvent eventTimer = new ManualResetEvent(false);
        Dictionary<int, JSFunctionPtr> intervals = new Dictionary<int, JSFunctionPtr>();
        int ivalid = 0;
        IntPtr setInterval(int count, IntPtr args)
        {
            object[] data = vm.Deserialize(count, args);
            JSFunctionPtr ptr = data[0] as JSFunctionPtr;
            if (!eventthreadrunning)
            {
                eventthreadrunning = true;
                System.Threading.Thread mthread = new Thread(eventthread);
                mthread.Start();
            }
            JSEvent tt = new JSEvent();
            JSMarshaller marshal = new JSMarshaller();
            marshal.args = new object[0];
            marshal.DeleteAfterExecution = false;
            marshal.ptr = ptr;
            tt.marshaller = marshal;
            tt.timeout = (int)data[1];
            tt.repeating = true;
            events.Add(tt);
            eventTimer.Set();
            intervals.Add(ivalid, ptr);
            ivalid++;
            return vm.Serialize(this, ivalid-1);

        }
        /// <summary>
        /// Dispatches a JavaScript function to the main thread
        /// </summary>
        /// <param name="ptr">The function pointer</param>
        /// <param name="args">The arguments to pass to the function</param>
        /// <param name="deleteAfterExec">Whether or not this pointer should be deleted after the function has been called</param>
        public void DispatchFunction(JSFunctionPtr ptr,bool deleteAfterExec, params object[] args)
        {
            
            JSMarshaller marshal = new JSMarshaller();
            marshal.args = args;
            marshal.DeleteAfterExecution = deleteAfterExec;
            marshal.ptr = ptr;
            interruptfunctions.Add(marshal);
            mvent.Set();
        }
        public Kernel parent;
        IntPtr postMessage(int count, IntPtr msg)
        {
            parent.DispatchFunction(null, false, vm.Deserialize(count, msg));
            return IntPtr.Zero;
        }
        IntPtr free(int count, IntPtr args)
        {
            object[] ptrs = vm.Deserialize(count, args);
            foreach (int et in ptrs)
            {
                Type mtype = objPtrs[et].GetType();
                MethodInfo dmthd = mtype.GetMethod("Dispose");
                if (dmthd != null)
                {
                    dmthd.Invoke(objPtrs[et], null);
                }
                objPtrs.Remove(et);
            }
            return IntPtr.Zero;
        }
        IntPtr weaken(int count, IntPtr args)
        {
            object[] ptrs = vm.Deserialize(count, args);
            foreach (int et in ptrs)
            {
              
                objPtrs.Remove(et);
            }
            return IntPtr.Zero;

        }
        #endregion
        public void Run(string code)
        {
            vm.Run(new JSFunction[] { Write,KernelSpinWait, FireAt, InvokeMethod, ResolveMethodPtr, ResolveMethod,InvokeDynamicMethod, setRecvDgate, setInterval, postMessage,free, weaken},code);
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
