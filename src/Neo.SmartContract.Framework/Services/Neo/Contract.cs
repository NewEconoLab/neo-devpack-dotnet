﻿namespace Neo.SmartContract.Framework.Services.Neo
{
    public class Contract
    {
        public extern byte[] Script
        {
            [Syscall("Neo.Contract.GetScript")]
            get;
        }

        public extern bool IsPayable
        {
            [Syscall("Neo.Contract.IsPayable")]
            get;
        }

        public extern StorageContext StorageContext
        {
            [Syscall("Neo.Contract.GetStorageContext")]
            get;
        }

        [Syscall("Neo.Contract.Create")]
        public static extern Contract Create(byte[] script, byte[] parameter_list, byte return_type, ContractPropertyState contract_property_state, string name, string version, string author, string email, string description);

        [Syscall("Neo.Contract.Migrate")]
        public static extern Contract Migrate(byte[] script, byte[] parameter_list, byte return_type, ContractPropertyState contract_property_state, string name, string version, string author, string email, string description);

        [Syscall("Neo.Contract.Destroy")]
        public static extern void Destroy();
    }
}
