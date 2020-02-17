namespace Neo.SmartContract.Framework.Services.Neo
{
    public class Account
    {
        [Syscall("System.Contract.IsStandard")]
        public static extern bool IsStandard(byte[] scripthash);
    }
}
