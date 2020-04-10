using Neo.Compiler.MSIL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Compiler
{
    class Debug
    {
        public static void DebugOutput(string outpath, NeoModule module, byte[] avm,string onlyname)
        {
            if (System.IO.Directory.Exists(outpath) == false)
            {
                System.IO.Directory.CreateDirectory(outpath);
            }
            string mapInfo = null;

            {//gen debug info
                Neo.Compiler.MyJson.JsonNode_Array arr = new Neo.Compiler.MyJson.JsonNode_Array();
                foreach (var m in module.mapMethods)
                {
                    Neo.Compiler.MyJson.JsonNode_Object item = new Neo.Compiler.MyJson.JsonNode_Object();
                    arr.Add(item);
                    item.SetDictValue("name", m.Value.displayName);
                    item.SetDictValue("addr", m.Value.funcaddr.ToString("X04"));
                    Neo.Compiler.MyJson.JsonNode_Array infos = new Neo.Compiler.MyJson.JsonNode_Array();
                    item.SetDictValue("map", infos);
                    foreach (var c in m.Value.body_Codes)
                    {
                        if (c.Value.debugline > 0)
                        {
                            infos.AddArrayValue(m.Value.funcaddr.ToString("X04") + "-" + (c.Value.debugline - 2).ToString());
                            break;
                        }
                    }
                    foreach (var c in m.Value.body_Codes)
                    {
                        if (c.Value.debugline > 0)
                        {
                            infos.AddArrayValue(c.Value.addr.ToString("X04") + "-" + c.Value.debugline.ToString());
                        }
                    }
                }
                mapInfo = arr.ToString();
            }


            var outfile = System.IO.Path.Combine(outpath, onlyname);
            System.IO.File.WriteAllText(outfile + ".map.json", mapInfo);
            new DefLogger().Log("write:" + onlyname + ".map.json");
        }
    }
}
