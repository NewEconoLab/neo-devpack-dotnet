using Neo.SmartContract.Framework.Services.Neo;

namespace Neo.Compiler.MSIL.TestClasses
{
    public class Contract_Account : SmartContract.Framework.SmartContract
    {
        public static bool AccountIsStandard(byte[] scripthash)
        {
            return Account.IsStandard(scripthash);
        }
    }
}
