using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.Collections.Generic;

namespace Neo.Compiler.MSIL.Utils
{
    public class TestEngine : ApplicationEngine
    {
        public const int MaxStorageKeySize = 64;
        public const int MaxStorageValueSize = ushort.MaxValue;

        static IDictionary<string, BuildScript> scriptsAll = new Dictionary<string, BuildScript>();

        public readonly IDictionary<string, BuildScript> Scripts;

        public BuildScript ScriptEntry { get; private set; }

        public TestEngine(TriggerType trigger = TriggerType.Application, IVerifiable verificable = null, StoreView snapshot = null)
            : base(trigger, verificable, snapshot == null ? new TestSnapshot() : snapshot, 0, true)
        {
            Scripts = new Dictionary<string, BuildScript>();
        }

        public BuildScript Build(string filename, bool releaseMode = false)
        {
            if (scriptsAll.ContainsKey(filename) == false)
            {
                scriptsAll[filename] = NeonTestTool.BuildScript(filename, releaseMode);
            }

            return scriptsAll[filename];
        }

        public void AddEntryScript(string filename, bool releaseMode = false)
        {
            ScriptEntry = Build(filename, releaseMode);
            Reset();
        }

        public void Reset()
        {
            this.State = VMState.BREAK; // Required for allow to reuse the same TestEngine
            this.InvocationStack.Clear();
            this.LoadScript(ScriptEntry.finalNEF);
        }

        public class ContractMethod
        {
            readonly TestEngine engine;
            readonly string methodname;

            public ContractMethod(TestEngine engine, string methodname)
            {
                this.engine = engine;
                this.methodname = methodname;
            }

            public StackItem Run(params StackItem[] _params)
            {
                return this.engine.ExecuteTestCaseStandard(methodname, _params).Pop();
            }
        }

        public ContractMethod GetMethod(string methodname)
        {
            return new ContractMethod(this, methodname);
        }

        public EvaluationStack ExecuteTestCaseStandard(string methodname, params StackItem[] args)
        {
            this.InvocationStack.Peek().InstructionPointer = 0;
            this.Push(new VM.Types.Array(this.ReferenceCounter, args));
            this.Push(methodname);
            while (true)
            {
                var bfault = (this.State & VMState.FAULT) > 0;
                var bhalt = (this.State & VMState.HALT) > 0;
                if (bfault || bhalt) break;

                Console.WriteLine("op:[" +
                    this.CurrentContext.InstructionPointer.ToString("X04") +
                    "]" +
                this.CurrentContext.CurrentInstruction.OpCode);
                this.ExecuteNext();
            }
            return this.ResultStack;
        }

        public EvaluationStack ExecuteTestCase(params StackItem[] args)
        {
            //var engine = new ExecutionEngine();
            this.InvocationStack.Peek().InstructionPointer = 0;
            if (args != null)
            {
                for (var i = args.Length - 1; i >= 0; i--)
                {
                    this.CurrentContext.EvaluationStack.Push(args[i]);
                }
            }
            while (true)
            {
                var bfault = (this.State & VMState.FAULT) > 0;
                var bhalt = (this.State & VMState.HALT) > 0;
                if (bfault || bhalt) break;

                Console.WriteLine("op:[" +
                    this.CurrentContext.InstructionPointer.ToString("X04") +
                    "]" +
                this.CurrentContext.CurrentInstruction.OpCode);
                this.ExecuteNext();
            }
            return this.ResultStack;
        }
        static Dictionary<uint, InteropDescriptor> callmethod;
        
        protected override bool OnSysCall(uint method)
        {
            if(callmethod==null)
            {
                callmethod = new Dictionary<uint, InteropDescriptor>();
                foreach(var m in InteropService.SupportedMethods())
                {
                    callmethod[m] = m;
                }
            }
            if(callmethod.ContainsKey(method)==false)
            {
                throw new Exception($"Syscall not found: {method.ToString("X2")} (using base call)");
            }
            else
            {
                return base.OnSysCall(method);
            }
        }

        public bool CheckAsciiChar(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToLower().ToCharArray()[i];
                if ((!((c >= 97 && c <= 123) || (c >= 48 && c <= 57) || c == 45 || c == 46 || c == 64 || c == 95)))
                    return false;
            }
            return true;
        }

        private void DumpItemShort(StackItem item, int space = 0)
        {
            var spacestr = "";
            for (var i = 0; i < space; i++) spacestr += "    ";
            var line = NeonTestTool.Bytes2HexString(item.GetSpan().ToArray());

            if (item is Neo.VM.Types.ByteArray)
            {
                var str = item.GetString();
                if (CheckAsciiChar(str))
                {
                    line += "|" + str;
                }
            }
            Console.WriteLine(spacestr + line);
        }

        private void DumpItem(StackItem item, int space = 0)
        {
            var spacestr = "";
            for (var i = 0; i < space; i++) spacestr += "    ";
            Console.WriteLine(spacestr + "got Param:" + item.GetType().ToString());

            if (item is VM.Types.Array || item is Neo.VM.Types.Struct)
            {
                var array = item as Neo.VM.Types.Array;
                for (var i = 0; i < array.Count; i++)
                {
                    var subitem = array[i];
                    DumpItem(subitem, space + 1);
                }
            }
            else if (item is Neo.VM.Types.Map)
            {
                var map = item as Neo.VM.Types.Map;
                foreach (var subitem in map)
                {
                    Console.WriteLine("---Key---");
                    DumpItemShort(subitem.Key, space + 1);
                    Console.WriteLine("---Value---");
                    DumpItem(subitem.Value, space + 1);
                }
            }
            else
            {
                Console.WriteLine(spacestr + "--as num:" + item.GetBigInteger());
                Console.WriteLine(spacestr + "--as bin:" + NeonTestTool.Bytes2HexString(item.GetSpan().ToArray()));

                if (item is Neo.VM.Types.ByteArray)
                {
                    var str = item.GetString();
                    if (CheckAsciiChar(str))
                    {
                        Console.WriteLine(spacestr + "--as str:" + item.GetString());
                    }
                }
            }
        }
    }
}
