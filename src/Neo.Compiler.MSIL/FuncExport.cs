using Neo.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vmtool
{
    public class FuncExport
    {
        static string ConvType(string _type)
        {
            switch (_type)
            {
                case "__Signature":
                    return "Signature";

                case "System.Boolean":
                    return "Boolean";

                case "System.Byte":
                case "System.SByte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Numerics.BigInteger":
                    return "Integer";

                case "__Hash160":
                    return "Hash160";

                case "__Hash256":
                    return "Hash256";

                case "System.Byte[]":
                    return "ByteArray";

                case "__PublicKey":
                    return "PublicKey";

                case "System.String":
                    return "String";

                case "System.Object[]":
                    return "Array";

                case "__InteropInterface":
                case "IInteropInterface":
                    return "InteropInterface";

                case "System.Void":
                    return "Void";

                case "System.Object":
                    return "ByteArray";
            }
            if (_type.Contains("[]"))
                return "Array";

            return "Unknown:" + _type;
        }

        public static MyJson.JsonNode_Object Export(NeoModule module, byte[] script)
        {
            var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash256 = sha256.ComputeHash(script);
            var ripemd160 = new Neo.Cryptography.RIPEMD160Managed();
            var hash = ripemd160.ComputeHash(hash256);

            var outjson = new MyJson.JsonNode_Object();

            //hash
            StringBuilder sb = new StringBuilder();
            sb.Append("0x");
            foreach (var b in hash.Reverse().ToArray())
            {
                sb.Append(b.ToString("x02"));
            }
            outjson.SetDictValue("hash", sb.ToString());

            //entrypoint
            var entryPoint = "main";
            var mainmethod = module.mapMethods[module.mainMethod];
            if (mainmethod != null)
            {
                entryPoint = mainmethod.displayName;
            }

            //functions
            var methods = new MyJson.JsonNode_Array();
            outjson["methods"] = methods;

            List<string> names = new List<string>();
            foreach (var function in module.mapMethods)
            {
                var mm = function.Value;
                if (mm.inSmartContract == false)
                    continue;
                if (mm.isPublic == false)
                    continue;

                var funcsign = new MyJson.JsonNode_Object();
                if (function.Value.displayName == entryPoint)
                {
                    // This is the entryPoint
                    outjson.SetDictValue("entryPoint", funcsign);
                }
                else
                {
                    methods.Add(funcsign);
                }
                funcsign.SetDictValue("name", function.Value.displayName);
                if (names.Contains(function.Value.displayName))
                {
                    throw new Exception("abi not allow same name functions");
                }
                names.Add(function.Value.displayName);
                MyJson.JsonNode_Array funcparams = new MyJson.JsonNode_Array();
                funcsign["parameters"] = funcparams;
                if (mm.paramtypes != null)
                {
                    foreach (var v in mm.paramtypes)
                    {
                        var ptype = ConvType(v.type);
                        var item = new MyJson.JsonNode_Object();
                        funcparams.Add(item);

                        item.SetDictValue("name", v.name);
                        item.SetDictValue("type", ptype);
                    }
                }

                var rtype = ConvType(mm.returntype);
                funcsign.SetDictValue("returnType", rtype);
            }

            //events
            var eventsigns = new MyJson.JsonNode_Array();
            outjson["events"] = eventsigns;
            foreach (var events in module.mapEvents)
            {
                var mm = events.Value;
                var funcsign = new MyJson.JsonNode_Object();
                eventsigns.Add(funcsign);

                funcsign.SetDictValue("name", events.Value.displayName);
                MyJson.JsonNode_Array funcparams = new MyJson.JsonNode_Array();
                funcsign["parameters"] = funcparams;
                if (mm.paramtypes != null)
                {
                    foreach (var v in mm.paramtypes)
                    {
                        var ptype = ConvType(v.type);
                        var item = new MyJson.JsonNode_Object();
                        funcparams.Add(item);

                        item.SetDictValue("name", v.name);
                        item.SetDictValue("type", ptype);
                    }
                }
                //event do not have returntype in nep3
                //var rtype = ConvType(mm.returntype);
                //funcsign.SetDictValue("returntype", rtype);
            }

            return outjson;
        }
    }
}
