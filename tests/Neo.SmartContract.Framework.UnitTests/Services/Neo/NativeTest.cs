using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Compiler.MSIL.Utils;
using Neo.VM;
using Neo.VM.Types;

namespace Neo.SmartContract.Framework.UnitTests.Services.Neo
{
    [TestClass]
    public class NativeTest
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine();
            _engine.AddEntryScript("./TestClasses/Contract_Native.cs");
            ((TestSnapshot)_engine.Snapshot).SetPersistingBlock(new Network.P2P.Payloads.Block()
            {
                Index = 0,
                ConsensusData = new Network.P2P.Payloads.ConsensusData(),
                Transactions = new Network.P2P.Payloads.Transaction[0],
                Witness = new Network.P2P.Payloads.Witness()
                {
                    InvocationScript = new byte[0],
                    VerificationScript = new byte[0]
                },
                NextConsensus = UInt160.Zero,
                MerkleRoot = UInt256.Zero,
                PrevHash = UInt256.Zero
            });

            // Deploy native contracts
            Assert.Fail("neo 3 has not Neo_Native_Deploy.");
            //using (var script = new ScriptBuilder())
            //{
            //    script.EmitSysCall(InteropService.Neo_Native_Deploy);
            //    _engine.LoadScript(script.ToArray());
            //    _engine.Execute();
            //}
        }

        [TestMethod]
        public void Test_NEO()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("NEO_Decimals");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0, item.GetBigInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_Name");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            Assert.AreEqual("NEO", item.GetString());
        }

        [TestMethod]
        public void Test_GAS()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("GAS_Decimals");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(8, item.GetBigInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("GAS_Name");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            Assert.AreEqual("GAS", item.GetString());
        }

        [TestMethod]
        public void Test_Policy()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("policy_GetFeePerByte");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(1000L, item.GetBigInteger());
        }
    }
}
