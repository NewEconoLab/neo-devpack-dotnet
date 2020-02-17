using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace Neo.Compiler.MSIL.TestClasses
{
    public class Contract_Blockchain : SmartContract.Framework.SmartContract
    {
        public static uint GetHeight()
        {
            return Blockchain.GetHeight();
        }

        public static BigInteger GetTransactionHeight(byte[] hash)
        {
            return Blockchain.GetTransactionHeight(hash);
        }

        public static object GetBlockByHash(byte[] hash, string whatReturn)
        {
            var block = Blockchain.GetBlock(hash);
            return GetBlockInfo(block, whatReturn);
        }

        public static object GetBlockByIndex(uint index, string whatReturn)
        {
            var block = Blockchain.GetBlock(index);
            return GetBlockInfo(block, whatReturn);
        }

        private static object GetBlockInfo(Block block, string whatReturn)
        {
            if (block == null)
            {
                Runtime.Log("NULL Block");
                return null;
            }

            if (whatReturn == "Hash") return block.Hash;
            if (whatReturn == "Index") return block.Index;
            if (whatReturn == "MerkleRoot") return block.MerkleRoot;
            if (whatReturn == "NextConsensus") return block.NextConsensus;
            if (whatReturn == "PrevHash") return block.PrevHash;
            if (whatReturn == "Timestamp") return block.Timestamp;
            if (whatReturn == "TransactionsCount") return block.TransactionsCount;
            if (whatReturn == "Version") return block.Version;

            throw new Exception("Uknown property");
        }

        public static object GetTxByHash(byte[] hash, string whatReturn)
        {
            var tx = Blockchain.GetTransaction(hash);
            return GetTxInfo(tx, whatReturn);
        }

        public static object GetTxByBlockHash(byte[] blockHash, int txIndex, string whatReturn)
        {
            var tx = Blockchain.GetTransactionFromBlock(blockHash, txIndex);
            return GetTxInfo(tx, whatReturn);
        }

        public static object GetTxByBlockIndex(uint blockIndex, int txIndex, string whatReturn)
        {
            var tx = Blockchain.GetTransactionFromBlock(blockIndex, txIndex);
            return GetTxInfo(tx, whatReturn);
        }

        private static object GetTxInfo(Transaction tx, string whatReturn)
        {
            if (tx == null)
            {
                Runtime.Log("NULL Tx");
                return null;
            }

            if (whatReturn == "Hash") return tx.Hash;
            if (whatReturn == "NetworkFee") return tx.NetworkFee;
            if (whatReturn == "Nonce") return tx.Nonce;
            if (whatReturn == "Script") return tx.Script;
            if (whatReturn == "Sender") return tx.Sender;
            if (whatReturn == "SystemFee") return tx.SystemFee;
            if (whatReturn == "ValidUntilBlock") return tx.ValidUntilBlock;
            if (whatReturn == "Version") return tx.Version;

            throw new Exception("Uknown property");
        }

        public static object GetContract(byte[] hash, string whatReturn)
        {
            var contract = Blockchain.GetContract(hash);
            return GetContractInfo(contract, whatReturn);
        }

        private static object GetContractInfo(Contract contract, string whatReturn)
        {
            if (contract == null)
            {
                Runtime.Log("NULL contract");
                return null;
            }

            if (whatReturn == "HasStorage") return contract.HasStorage;
            if (whatReturn == "IsPayable") return contract.IsPayable;
            if (whatReturn == "Script") return contract.Script;

            throw new Exception("Uknown property");
        }
    }
}
