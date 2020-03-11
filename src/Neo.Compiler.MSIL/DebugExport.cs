using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using vmtool;

namespace Neo.Compiler
{
    public static class DebugExport
    {
        private static MyJson.JsonNode_Array GetSequencePoints(IEnumerable<NeoCode> codes, IDictionary<string, int> docMap)
        {
            var points = codes
                .Where(code => code.sequencePoint != null)
                .Select(code => (code.addr, code.sequencePoint));

            var outjson = new MyJson.JsonNode_Array();

            foreach (var (address, sequencePoint) in points)
            {
                var value = string.Format("{0}[{1}]{2}:{3}-{4}:{5}",
                    address,
                    docMap[sequencePoint.Document.Url],
                    sequencePoint.StartLine,
                    sequencePoint.StartColumn,
                    sequencePoint.EndLine,
                    sequencePoint.EndColumn);
                outjson.Add(new MyJson.JsonNode_ValueString(value));
            }

            return outjson;
        }

        private static string ConvertType(string type)
        {
            if (type == "System.Object")
                return string.Empty;

            return FuncExport.ConvType(type);
        }

        private static MyJson.JsonNode_Array ConvertParamList(IList<NeoParam> @params)
        {
            var paramsJson = new MyJson.JsonNode_Array();
            foreach (var param in @params)
            {
                var value = string.Format("{0},{1}", param.name, ConvertType(param.type));
                paramsJson.Add(new MyJson.JsonNode_ValueString(value));
            }

            return paramsJson;
        }

        private static MyJson.JsonNode_Array GetMethods(NeoModule module, IDictionary<string, int> docMap)
        {
            var outjson = new MyJson.JsonNode_Array();

            foreach (var method in module.mapMethods.Values)
            {
                var name = string.Format("{0},{1}",
                    method._namespace, method.displayName);

                var range = string.Format("{0}-{1}",
                    method.body_Codes.Values.First().addr,
                    method.body_Codes.Values.Last().addr);

                var methodJson = new MyJson.JsonNode_Object();
                methodJson.SetDictValue("id", method.name);
                methodJson.SetDictValue("name", name);
                methodJson.SetDictValue("range", range);
                methodJson.SetDictValue("params", ConvertParamList(method.paramtypes));
                methodJson.SetDictValue("return", ConvertType(method.returntype));
                methodJson.SetDictValue("variables", ConvertParamList(method.body_Variables));
                methodJson.SetDictValue("sequence-points", GetSequencePoints(method.body_Codes.Values, docMap));
                outjson.Add(methodJson);
            }
            return outjson;
        }

        private static MyJson.JsonNode_Array GetEvents(NeoModule module)
        {
            var outjson = new MyJson.JsonNode_Array();
            foreach (var @event in module.mapEvents.Values)
            {
                var name = string.Format("{0},{1}",
                    @event._namespace, @event.displayName);

                var eventJson = new MyJson.JsonNode_Object();
                eventJson.SetDictValue("id", @event.name);
                eventJson.SetDictValue("name", name);
                eventJson.SetDictValue("params", ConvertParamList(@event.paramtypes));
                outjson.Add(eventJson);
            }
            return outjson;
        }

        private static IDictionary<string, int> GetDocumentMap(NeoModule module)
        {
            return module.mapMethods.Values
                .SelectMany(m => m.body_Codes.Values)
                .Where(code => code.sequencePoint != null)
                .Select(code => code.sequencePoint.Document.Url)
                .Distinct()
                .Select((d, i) => (d, i))
                .ToDictionary(t => t.d, t => t.i);
        }

        private static MyJson.JsonNode_Array GetDocuments(IDictionary<string, int> docMap)
        {
            var outjson = new MyJson.JsonNode_Array();
            foreach (var doc in docMap.OrderBy(kvp => kvp.Value))
            {
                System.Diagnostics.Debug.Assert(outjson.Count == doc.Value);
                outjson.Add(new MyJson.JsonNode_ValueString(doc.Key));
            }
            return outjson;
        }

        public static MyJson.JsonNode_Object Export(NeoModule am)
        {
            var docMap = GetDocumentMap(am);

            var outjson = new MyJson.JsonNode_Object();
            outjson.SetDictValue("entrypoint", am.mainMethod);
            outjson.SetDictValue("documents", GetDocuments(docMap));
            outjson.SetDictValue("methods", GetMethods(am, docMap));
            outjson.SetDictValue("events", GetEvents(am));
            return outjson;
        }
    }
}
