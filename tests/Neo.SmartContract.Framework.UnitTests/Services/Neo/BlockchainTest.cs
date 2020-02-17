using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Compiler.MSIL.Utils;
using Neo.IO;
using Neo.Ledger;
using Neo.VM;
using Neo.VM.Types;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Neo.SmartContract.Framework.UnitTests.Services.Neo
{
    [TestClass]
    public class BlockchainTest
    {
        private Network.P2P.Payloads.Block _block;
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            var _ = TestBlockchain.TheNeoSystem;
            var snapshot = Blockchain.Singleton.GetSnapshot();

            _block = Blockchain.GenesisBlock;
            _engine = new TestEngine(snapshot: snapshot.Clone());
            _engine.AddEntryScript("./TestClasses/Contract_Blockchain.cs");
        }

        [TestMethod]
        public void Test_GetHeight()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("getHeight");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(_block.Index, item.GetBigInteger());
        }

        [TestMethod]
        public void Test_GetTransactionHeight()
        {
            // Not found

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("getTransactionHeight", new ByteArray(UInt256.Zero.ToArray()));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(BigInteger.MinusOne, item.GetBigInteger());

            // Found

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getTransactionHeight", new ByteArray(_block.Transactions[0].Hash.ToArray()));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(_block.Index, item.GetBigInteger());
        }

        [TestMethod]
        public void Test_GetBlockByHash()
        {
            Test_GetBlock("getBlockByHash", new ByteArray(_block.Hash.ToArray()), new ByteArray(UInt256.Zero.ToArray()));
        }

        [TestMethod]
        public void Test_GetTxByHash()
        {
            Test_GetTransaction("getTxByHash", new StackItem[] { new ByteArray(_block.Transactions[0].Hash.ToArray()) },
                new StackItem[] { new ByteArray(UInt256.Zero.ToArray()) },
                true);
        }

        [TestMethod]
        public void Test_GetTxByBlockIndex()
        {
            Test_GetTransaction("getTxByBlockIndex", new StackItem[] {
                new Integer(_block.Index), new Integer(0) },
                new StackItem[] { new Integer(_block.Index), new Integer(_block.Transactions.Length + 1) },
                false);
        }

        [TestMethod]
        public void Test_GetTxByBlockHash()
        {
            Test_GetTransaction("getTxByBlockHash", new StackItem[] {
                new ByteArray(_block.Hash.ToArray()), new Integer(0) },
                new StackItem[] { new ByteArray(_block.Hash.ToArray()), new Integer(_block.Transactions.Length + 1) },
                false);
        }

        public void Test_GetTransaction(string method, StackItem[] foundArgs, StackItem[] notFoundArgs, bool expectedNullAsNotFound)
        {
            // Not found

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard(method, Concat(notFoundArgs, new ByteArray(new byte[0])));

            if (expectedNullAsNotFound)
            {
                Assert.AreEqual(VMState.HALT, _engine.State);
                Assert.AreEqual(1, result.Count);
                Assert.IsInstanceOfType(result.Pop(), typeof(Null));
            }
            else
            {
                Assert.AreEqual(VMState.FAULT, _engine.State);
                Assert.AreEqual(0, result.Count);
            }

            var tx = _block.Transactions[0];

            // Hash

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("Hash"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            CollectionAssert.AreEqual(tx.Hash.ToArray(), item.GetSpan().ToArray());

            // NetworkFee

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("NetworkFee"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(tx.NetworkFee, item.GetBigInteger());

            // Nonce

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("Nonce"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(tx.Nonce, item.GetBigInteger());

            // SystemFee

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("SystemFee"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(tx.SystemFee, item.GetBigInteger());

            // ValidUntilBlock

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("ValidUntilBlock"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(tx.ValidUntilBlock, item.GetBigInteger());

            // Version

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("Version"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(tx.Version, item.GetBigInteger());

            // Script

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("Script"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            CollectionAssert.AreEqual(tx.Script, item.GetSpan().ToArray());

            // Sender

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, Concat(foundArgs, new ByteArray(Encoding.UTF8.GetBytes("Sender"))));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            CollectionAssert.AreEqual(tx.Sender.ToArray(), item.GetSpan().ToArray());
        }

        private StackItem[] Concat(StackItem[] a, ByteArray b)
        {
            return a.Concat(new StackItem[] { b }).ToArray();
        }

        [TestMethod]
        public void Test_GetBlockByIndex()
        {
            Test_GetBlock("getBlockByIndex", new Integer(_block.Index), new Integer(_block.Index + 100));
        }

        public void Test_GetBlock(string method, StackItem foundArg, StackItem notFoundArg)
        {
            // Not found

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard(method, notFoundArg, new ByteArray(new byte[0]));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Null));

            // Hash

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("Hash")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            CollectionAssert.AreEqual(_block.Hash.ToArray(), item.GetSpan().ToArray());

            // Index

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("Index")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(_block.Index, item.GetBigInteger());

            // MerkleRoot

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("MerkleRoot")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            CollectionAssert.AreEqual(_block.MerkleRoot.ToArray(), item.GetSpan().ToArray());

            // NextConsensus

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("NextConsensus")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            CollectionAssert.AreEqual(_block.NextConsensus.ToArray(), item.GetSpan().ToArray());

            // PrevHash

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("PrevHash")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteArray));
            CollectionAssert.AreEqual(_block.PrevHash.ToArray(), item.GetSpan().ToArray());

            // Timestamp

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("Timestamp")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(_block.Timestamp, item.GetBigInteger());

            // TransactionsCount

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("TransactionsCount")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(_block.Transactions.Length, item.GetBigInteger());

            // Version

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("Version")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(_block.Version, item.GetBigInteger());

            // Uknown property

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard(method, foundArg, new ByteArray(Encoding.UTF8.GetBytes("ASD")));
            Assert.AreEqual(VMState.FAULT, _engine.State);
        }

        [TestMethod]
        public void GetContract()
        {
            var contract = new ContractState()
            {
                Script = new byte[] { 0x01, 0x02, 0x03 },
                Manifest = new Manifest.ContractManifest()
                {
                    Features = Manifest.ContractFeatures.HasStorage
                }
            };
            _engine.Snapshot.Contracts.GetOrAdd(contract.ScriptHash, () => contract);

            // Not found

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("getContract", new ByteArray(UInt160.Zero.ToArray()), new ByteArray(new byte[0]));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Null));

            // Found + HasStorage

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getContract", new ByteArray(contract.ScriptHash.ToArray()), new ByteArray(Encoding.UTF8.GetBytes("HasStorage")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.Boolean));
            Assert.AreEqual(contract.HasStorage, item.ToBoolean());

            // Found + IsPayable

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getContract", new ByteArray(contract.ScriptHash.ToArray()), new ByteArray(Encoding.UTF8.GetBytes("IsPayable")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.Boolean));
            Assert.AreEqual(contract.Payable, item.ToBoolean());

            // Found + IsPayable

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getContract", new ByteArray(contract.ScriptHash.ToArray()), new ByteArray(Encoding.UTF8.GetBytes("Script")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteArray));
            CollectionAssert.AreEqual(contract.Script, item.GetSpan().ToArray());

            // Found + Uknown property

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getContract", new ByteArray(contract.ScriptHash.ToArray()), new ByteArray(Encoding.UTF8.GetBytes("ASD")));
            Assert.AreEqual(VMState.FAULT, _engine.State);
        }
    }
}
