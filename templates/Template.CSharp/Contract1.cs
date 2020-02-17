using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace Test
{
	public class Contract1 : SmartContract
    {
        public static object Main(string operation, object[] args)
        {
            if (operation == "put")
            {
                Storage.Put("Hello", "World");
            }
            if (operation == "get")
            {
                return Storage.Get("Hello");
            }
            if (operation == "name")
            {
                return "test2";
            }
            return false;
        }
    }
}
