#define RUN_UNIT_TESTS
//#define JSON_PARSER_ONLY

// On GitHub:
// https://github.com/ysharplanguage/FastJsonParser

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// For the JavaScriptSerializer
using System.Web.Script.Serialization;

// JSON.NET 5.0 r8
using Newtonsoft.Json;

// ServiceStack 3.9.59
using ServiceStack.Text;

// Our stuff
using System.Text.Json;

namespace Test
{
    public class E
    {
        public object zero { get; set; }
        public int one { get; set; }
        public int two { get; set; }
        public List<int> three { get; set; }
        public List<int> four { get; set; }
    }

    public class F
    {
        public object g { get; set; }
    }

    public class E2
    {
        public F f { get; set; }
    }

    public class D
    {
        public E2 e { get; set; }
    }

    public class C
    {
        public D d { get; set; }
    }

    public class B
    {
        public C c { get; set; }
    }

    public class A
    {
        public B b { get; set; }
    }

    public class H
    {
        public A a { get; set; }
    }

    public class HighlyNested
    {
        public string a { get; set; }
        public bool b { get; set; }
        public int c { get; set; }
        public List<object> d { get; set; }
        public E e { get; set; }
        public object f { get; set; }
        public H h { get; set; }
        public List<List<List<List<List<List<List<object>>>>>>> i { get; set; }
    }

    class ParserTests
    {
        private static readonly string OJ_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}_oj-highly-nested.json.txt", Path.DirectorySeparatorChar);
        private static readonly string BOON_SMALL_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}boon-small.json.txt", Path.DirectorySeparatorChar);
        private static readonly string TINY_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}tiny.json.txt", Path.DirectorySeparatorChar);
        private static readonly string DICOS_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}dicos.json.txt", Path.DirectorySeparatorChar);
        private static readonly string SMALL_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}small.json.txt", Path.DirectorySeparatorChar);
        private static readonly string FATHERS_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}fathers.json.txt", Path.DirectorySeparatorChar);
        private static readonly string HUGE_TEST_FILE_PATH = string.Format(@"..{0}..{0}TestData{0}huge.json.txt", Path.DirectorySeparatorChar);

#if RUN_UNIT_TESTS
        static object UnitTest<T>(string input, Func<string, T> parse) { return UnitTest(input, parse, false); }

        static object UnitTest<T>(string input, Func<string, T> parse, bool errorCase)
        {
            object obj;
            Console.WriteLine();
            Console.WriteLine(errorCase ? "(Error case)" : "(Nominal case)");
            Console.WriteLine("\tTry parse: {0} ... as: {1} ...", input, typeof(T).FullName);
            try { obj = parse(input); }
            catch (Exception ex) { obj = ex; }
            Console.WriteLine("\t... result: {0}{1}", (obj != null) ? obj.GetType().FullName : "(null)", (obj is Exception) ? " (" + ((Exception)obj).Message + ")" : String.Empty);
            Console.WriteLine();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
            return obj;
        }

        static void UnitTests()
        {
            object obj;
            Console.Clear();
            Console.WriteLine("Press ESC to skip the unit tests or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;

            // A few nominal cases
            obj = UnitTest("null", s => new JsonParser().Parse<object>(s));
            System.Diagnostics.Debug.Assert(obj == null);

            obj = UnitTest("true", s => new JsonParser().Parse<object>(s));
            System.Diagnostics.Debug.Assert(obj is bool && (bool)obj);

            obj = UnitTest(@"""\z""", s => new JsonParser().Parse<char>(s));
            System.Diagnostics.Debug.Assert(obj is char && (char)obj == 'z');

            obj = UnitTest(@"""\t""", s => new JsonParser().Parse<char>(s));
            System.Diagnostics.Debug.Assert(obj is char && (char)obj == '\t');

            obj = UnitTest(@"""\u0021""", s => new JsonParser().Parse<char>(s));
            System.Diagnostics.Debug.Assert(obj is char && (char)obj == '!');

            obj = UnitTest(@"""\u007D""", s => new JsonParser().Parse<char>(s));
            System.Diagnostics.Debug.Assert(obj is char && (char)obj == '}');

            obj = UnitTest(@" ""\u007e"" ", s => new JsonParser().Parse<char>(s));
            System.Diagnostics.Debug.Assert(obj is char && (char)obj == '~');

            obj = UnitTest(@" ""\u202A"" ", s => new JsonParser().Parse<char>(s));
            System.Diagnostics.Debug.Assert(obj is char && (int)(char)obj == 0x202a);

            obj = UnitTest("123", s => new JsonParser().Parse<int>(s));
            System.Diagnostics.Debug.Assert(obj is int && (int)obj == 123);

            obj = UnitTest("\"\"", s => new JsonParser().Parse<string>(s));
            System.Diagnostics.Debug.Assert(obj is string && (string)obj == String.Empty);

            obj = UnitTest("\"Abc\\tdef\"", s => new JsonParser().Parse<string>(s));
            System.Diagnostics.Debug.Assert(obj is string && (string)obj == "Abc\tdef");

            obj = UnitTest("[null]", s => new JsonParser().Parse<object[]>(s));
            System.Diagnostics.Debug.Assert(obj is object[] && ((object[])obj).Length == 1 && ((object[])obj)[0] == null);

            obj = UnitTest("[true]", s => new JsonParser().Parse<IList<bool>>(s));
            System.Diagnostics.Debug.Assert(obj is IList<bool> && ((IList<bool>)obj).Count == 1 && ((IList<bool>)obj)[0]);

            obj = UnitTest("[1,2,3,4,5]", s => new JsonParser().Parse<int[]>(s));
            System.Diagnostics.Debug.Assert(obj is int[] && ((int[])obj).Length == 5 && ((int[])obj)[4] == 5);

            obj = UnitTest("123.456", s => new JsonParser().Parse<decimal>(s));
            System.Diagnostics.Debug.Assert(obj is decimal && (decimal)obj == 123.456m);

            obj = UnitTest("{\"a\":123,\"b\":true}", s => new JsonParser().Parse<object>(s));
            System.Diagnostics.Debug.Assert(obj is IDictionary<string, object> && (((IDictionary<string, object>)obj)["a"] as string) == "123" && ((obj = ((IDictionary<string, object>)obj)["b"]) is bool) && (bool)obj);

            obj = UnitTest("1", s => new JsonParser().Parse<Status>(s));
            System.Diagnostics.Debug.Assert(obj is Status && (Status)obj == Status.Married);

            obj = UnitTest("\"Divorced\"", s => new JsonParser().Parse<Status>(s));
            System.Diagnostics.Debug.Assert(obj is Status && (Status)obj == Status.Divorced);

            obj = UnitTest("{\"Name\":\"Peter\",\"Status\":0}", s => new JsonParser().Parse<Person>(s));
            System.Diagnostics.Debug.Assert(obj is Person && ((Person)obj).Name == "Peter" && ((Person)obj).Status == Status.Single);

            obj = UnitTest("{\"Name\":\"Paul\",\"Status\":\"Married\"}", s => new JsonParser().Parse<Person>(s));
            System.Diagnostics.Debug.Assert(obj is Person && ((Person)obj).Name == "Paul" && ((Person)obj).Status == Status.Married);

            obj = UnitTest("{\"History\":[{\"key\":\"1801-06-30T00:00:00Z\",\"value\":\"Birth date\"}]}", s => new JsonParser().Parse<Person>(s));
            System.Diagnostics.Debug.Assert(obj is Person && ((Person)obj).History[new DateTime(1801, 6, 30, 0, 0, 0, DateTimeKind.Utc)] == "Birth date");

            obj = UnitTest(@"{""Items"":[
                {
                    ""__type"": ""Test.ParserTests+Stuff, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                    ""Id"": 123, ""Name"": ""Foo""
                },
                {
                    ""__type"": ""Test.ParserTests+Stuff, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                    ""Id"": 456, ""Name"": ""Bar""
                }]}", s => new JsonParser().Parse<StuffHolder>(s));
            System.Diagnostics.Debug.Assert
            (
                obj is StuffHolder && ((StuffHolder)obj).Items.Count == 2 &&
                ((Stuff)((StuffHolder)obj).Items[1]).Name == "Bar"
            );

            string configTestInputVendors = @"{
                ""ConfigItems"": {
                    ""Vendor1"": {
                        ""__type"": ""Test.ParserTests+SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 100,
                        ""Content"": ""config content for vendor 1""
                    },
                    ""Vendor3"": {
                        ""__type"": ""Test.ParserTests+SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 300,
                        ""Content"": ""config content for vendor 3""
                    }
                }
            }";

            string configTestInputIntegers = @"{
                ""ConfigItems"": {
                    ""123"": {
                        ""__type"": ""Test.ParserTests+SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 123000,
                        ""Content"": ""config content for key 123""
                    },
                    ""456"": {
                        ""__type"": ""Test.ParserTests+SampleConfigItem, Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"",
                        ""Id"": 456000,
                        ""Content"": ""config content for key 456""
                    }
                }
            }";

            obj = UnitTest(configTestInputVendors, s => new JsonParser().Parse<SampleConfigData<VendorID>>(s));
            System.Diagnostics.Debug.Assert
            (
                obj is SampleConfigData<VendorID> &&
                ((SampleConfigData<VendorID>)obj).ConfigItems.ContainsKey(VendorID.Vendor3) &&
                ((SampleConfigData<VendorID>)obj).ConfigItems[VendorID.Vendor3] is SampleConfigItem &&
                ((SampleConfigItem)((SampleConfigData<VendorID>)obj).ConfigItems[VendorID.Vendor3]).Id == 300
            );

            obj = UnitTest(configTestInputIntegers, s => new JsonParser().Parse<SampleConfigData<int>>(s));
            System.Diagnostics.Debug.Assert
            (
                obj is SampleConfigData<int> &&
                ((SampleConfigData<int>)obj).ConfigItems.ContainsKey(456) &&
                ((SampleConfigData<int>)obj).ConfigItems[456] is SampleConfigItem &&
                ((SampleConfigItem)((SampleConfigData<int>)obj).ConfigItems[456]).Id == 456000
            );

            obj = UnitTest(@"{
                ""OwnerByWealth"": {
                    ""15999.99"":
                        { ""Id"": 1,
                          ""Name"": ""Peter"",
                          ""Assets"": [
                            { ""Name"": ""Car"",
                              ""Price"": 15999.99 } ]
                        },
                    ""250000.05"":
                        { ""Id"": 2,
                          ""Name"": ""Paul"",
                          ""Assets"": [
                            { ""Name"": ""House"",
                              ""Price"": 250000.05 } ]
                        }
                },
                ""WealthByOwner"": [
                    { ""key"": { ""Id"": 1, ""Name"": ""Peter"" }, ""value"": 15999.99 },
                    { ""key"": { ""Id"": 2, ""Name"": ""Paul"" }, ""value"": 250000.05 }
                ]
            }", s => new JsonParser().Parse<Owners>(s));
            Owner peter, owner;
            System.Diagnostics.Debug.Assert
            (
                (obj is Owners) &&
                (peter = ((Owners)obj).WealthByOwner.Keys.
                    Where(person => person.Name == "Peter").FirstOrDefault()
                ) != null &&
                (owner = ((Owners)obj).OwnerByWealth[15999.99m]) != null &&
                (owner.Name == peter.Name) &&
                (owner.Assets.Count == 1) &&
                (owner.Assets[0].Name == "Car")
            );

            // Support for JSON.NET's "$type" pseudo-key (in addition to ServiceStack's "__type"):
            Person jsonNetPerson = new Person { Id = 123, Abc = '#', Name = "Foo", Scores = new[] { 100, 200, 300 } };

            // (Expected serialized form shown in next comment)
            string jsonNetString = JsonConvert.SerializeObject(jsonNetPerson, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
            // => '{"$type":"Test.ParserTests+Person, Test","Id":123,"Name":"Foo","Status":0,"Address":null,"Scores":[100,200,300],"Data":null,"History":null,"Abc":"#"}'

            // (Note the Parse<object>(...))
            object restoredObject = UnitTest(jsonNetString, s => new JsonParser().Parse<object>(jsonNetString));
            System.Diagnostics.Debug.Assert
            (
                restoredObject is Person &&
                ((Person)restoredObject).Name == "Foo" &&
                ((Person)restoredObject).Abc == '#' &&
                ((IList<int>)((Person)restoredObject).Scores).Count == 3 &&
                ((IList<int>)((Person)restoredObject).Scores)[2] == 300
            );

            // A few error cases
            obj = UnitTest("\"unfinished", s => new JsonParser().Parse<string>(s), true);
            System.Diagnostics.Debug.Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad string"));

            obj = UnitTest("[123]", s => new JsonParser().Parse<string[]>(s), true);
            System.Diagnostics.Debug.Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad string"));

            obj = UnitTest("[null]", s => new JsonParser().Parse<short[]>(s), true);
            System.Diagnostics.Debug.Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad number (short)"));

            obj = UnitTest("[123.456]", s => new JsonParser().Parse<int[]>(s), true);
            System.Diagnostics.Debug.Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Unexpected character at 4"));

            obj = UnitTest("\"Unknown\"", s => new JsonParser().Parse<Status>(s), true);
            System.Diagnostics.Debug.Assert(obj is Exception && ((Exception)obj).Message.StartsWith("Bad enum value"));

            Console.Clear();
            Console.WriteLine("... Unit tests done.");
            Console.WriteLine();
            Console.WriteLine("Press a key to start the speed tests...");
            Console.ReadKey();
        }
#endif

        static void LoopTest<T>(string parserName, Func<string, T> parseFunc, string testFile, int count)
        {
            Console.Clear();
            Console.WriteLine("Parser: {0}", parserName);
            Console.WriteLine();
            Console.WriteLine("Loop Test File: {0}", testFile);
            Console.WriteLine();
            Console.WriteLine("Iterations: {0}", count.ToString("0,0"));
            Console.WriteLine();
            Console.WriteLine("Deserialization: {0}", (typeof(T) != typeof(object)) ? "POCO(s)" : "loosely-typed");
            Console.WriteLine();
            Console.WriteLine("Press ESC to skip this test or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;

            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);

            var json = System.IO.File.ReadAllText(testFile);
            var st = DateTime.Now;
            var l = new List<object>();
            for (var i = 0; i < count; i++)
                l.Add(parseFunc(json));
            var tm = (int)DateTime.Now.Subtract(st).TotalMilliseconds;

            System.Threading.Thread.MemoryBarrier();
            var finalMemory = System.GC.GetTotalMemory(true);
            var consumption = finalMemory - initialMemory;

            System.Diagnostics.Debug.Assert(l.Count == count);

            Console.WriteLine();
            Console.WriteLine("... Done, in {0} ms. Throughput: {1} characters / second.", tm.ToString("0,0"), (1000 * (decimal)(count * json.Length) / (tm > 0 ? tm : 1)).ToString("0,0.00"));
            Console.WriteLine();
            Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
            Console.WriteLine();

            GC.Collect();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
        }

        static void Test<T>(string parserName, Func<string, T> parseFunc, string testFile)
        {
            Console.Clear();
            Console.WriteLine("Parser: {0}", parserName);
            Console.WriteLine();
            Console.WriteLine("Test File: {0}", testFile);
            Console.WriteLine();
            Console.WriteLine("Deserialization: {0}", (typeof(T) != typeof(object)) ? "POCO(s)" : "loosely-typed");
            Console.WriteLine();
            Console.WriteLine("Press ESC to skip this test or any other key to start...");
            Console.WriteLine();
            if (Console.ReadKey().KeyChar == 27)
                return;

            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);

            var json = System.IO.File.ReadAllText(testFile);
            var st = DateTime.Now;
            var o = parseFunc(json);
            var tm = (int)DateTime.Now.Subtract(st).TotalMilliseconds;

            System.Threading.Thread.MemoryBarrier();
            var finalMemory = System.GC.GetTotalMemory(true);
            var consumption = finalMemory - initialMemory;

            Console.WriteLine();
            Console.WriteLine("... Done, in {0} ms. Throughput: {1} characters / second.", tm.ToString("0,0"), (1000 * (decimal)json.Length / (tm > 0 ? tm : 1)).ToString("0,0.00"));
            Console.WriteLine();
            Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
            Console.WriteLine();

            if (o is FathersData)
            {
                Console.WriteLine("Fathers : {0}", ((FathersData)(object)o).fathers.Length.ToString("0,0"));
                Console.WriteLine();
            }
            GC.Collect();
            Console.WriteLine("Press a key...");
            Console.WriteLine();
            Console.ReadKey();
        }

        public class BoonSmall
        {
            public string debug { get; set; }
            public IList<int> nums { get; set; }
        }

        public enum Status
        {
            Single,
            Married,
            Divorced
        }

        public interface ISomething
        {
            int Id { get; set; }
            // Notice how "Name" isn't introduced here yet, but
            // instead, only in the implementation class "Stuff"
            // below:
        }

        public class Stuff : ISomething
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class StuffHolder
        {
            public IList<ISomething> Items { get; set; }
        }

        public class Asset
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        public class Owner : Person
        {
            public IList<Asset> Assets { get; set; }
        }

        public class Owners
        {
            public IDictionary<decimal, Owner> OwnerByWealth { get; set; }
            public IDictionary<Owner, decimal> WealthByOwner { get; set; }
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }

            // Both string and integral enum value representations can be parsed:
            public Status Status { get; set; }

            public string Address { get; set; }

            // Just to be sure we support that one, too:
            public IEnumerable<int> Scores { get; set; }

            public object Data { get; set; }

            // Generic dictionaries are also supported; e.g.:
            // '{
            //    "Name": "F. Bastiat", ...
            //    "History": [
            //       { "key": "1801-06-30", "value": "Birth date" }, ...
            //    ]
            //  }'
            public IDictionary<DateTime, string> History { get; set; }

            // 1-char-long strings in the JSON can be deserialized into System.Char:
            public char Abc { get; set; }
        }

        public enum SomeKey
        {
            Key0, Key1, Key2, Key3, Key4,
            Key5, Key6, Key7, Key8, Key9
        }

        public class DictionaryData
        {
            public IList<IDictionary<SomeKey, string>> Dictionaries { get; set; }
        }

        public class DictionaryDataAdaptJsonNetServiceStack
        {
            public IList<
                IList<KeyValuePair<SomeKey, string>>
            > Dictionaries { get; set; }
        }

        public class FathersData
        {
            public Father[] fathers { get; set; }
        }

        public class Father
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool married { get; set; }
            // Lists...
            public List<Son> sons { get; set; }
            // ... or arrays for collections, that's fine:
            public Daughter[] daughters { get; set; }
        }

        public class Son
        {
            public int age { get; set; }
            public string name { get; set; }
        }

        public class Daughter
        {
            public int age { get; set; }
            public string name { get; set; }
        }

        public enum VendorID
        {
            Vendor0,
            Vendor1,
            Vendor2,
            Vendor3,
            Vendor4,
            Vendor5
        }

        public class SampleConfigItem
        {
            public int Id { get; set; }
            public string Content { get; set; }
        }

        public class SampleConfigData<TKey>
        {
            public Dictionary<TKey, object> ConfigItems { get; set; }
        }

        static void SpeedTests()
        {
#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, OJ_TEST_FILE_PATH, 10000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, OJ_TEST_FILE_PATH, 10000);
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, OJ_TEST_FILE_PATH, 10000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, OJ_TEST_FILE_PATH, 10000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<HighlyNested>, OJ_TEST_FILE_PATH, 10000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<HighlyNested>, OJ_TEST_FILE_PATH, 10000);
            LoopTest("ServiceStack", new JsonSerializer<HighlyNested>().DeserializeFromString, OJ_TEST_FILE_PATH, 10000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<HighlyNested>, OJ_TEST_FILE_PATH, 10000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, OJ_TEST_FILE_PATH, 100000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, OJ_TEST_FILE_PATH, 100000);
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, OJ_TEST_FILE_PATH, 100000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, OJ_TEST_FILE_PATH, 100000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<HighlyNested>, OJ_TEST_FILE_PATH, 100000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<HighlyNested>, OJ_TEST_FILE_PATH, 100000);
            LoopTest("ServiceStack", new JsonSerializer<HighlyNested>().DeserializeFromString, OJ_TEST_FILE_PATH, 100000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<HighlyNested>, OJ_TEST_FILE_PATH, 100000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);
            LoopTest("ServiceStack", new JsonSerializer<BoonSmall>().DeserializeFromString, BOON_SMALL_TEST_FILE_PATH, 1000000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 1000000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);
            LoopTest("ServiceStack", new JsonSerializer<BoonSmall>().DeserializeFromString, BOON_SMALL_TEST_FILE_PATH, 10000000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<BoonSmall>, BOON_SMALL_TEST_FILE_PATH, 10000000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 10000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 10000);
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 10000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 10000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 100000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 100000);
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 100000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 100000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<Person>, TINY_TEST_FILE_PATH, 1000000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<Person>, TINY_TEST_FILE_PATH, 1000000);
            LoopTest("ServiceStack", new JsonSerializer<Person>().DeserializeFromString, TINY_TEST_FILE_PATH, 1000000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<Person>, TINY_TEST_FILE_PATH, 1000000);

#if !JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 10000);//(Can't deserialize properly)
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 10000);
            LoopTest("ServiceStack", new JsonSerializer<DictionaryDataAdaptJsonNetServiceStack>().DeserializeFromString, DICOS_TEST_FILE_PATH, 10000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<DictionaryData>, DICOS_TEST_FILE_PATH, 10000);

#if !JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 100000);//(Can't deserialize properly)
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 100000);
            LoopTest("ServiceStack", new JsonSerializer<DictionaryDataAdaptJsonNetServiceStack>().DeserializeFromString, DICOS_TEST_FILE_PATH, 100000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<DictionaryData>, DICOS_TEST_FILE_PATH, 100000);

#if !JSON_PARSER_ONLY
            //LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().Deserialize<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 1000000);//(Can't deserialize properly)
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<DictionaryDataAdaptJsonNetServiceStack>, DICOS_TEST_FILE_PATH, 1000000);
            LoopTest("ServiceStack", new JsonSerializer<DictionaryDataAdaptJsonNetServiceStack>().DeserializeFromString, DICOS_TEST_FILE_PATH, 1000000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<DictionaryData>, DICOS_TEST_FILE_PATH, 1000000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, SMALL_TEST_FILE_PATH, 10000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, SMALL_TEST_FILE_PATH, 10000);
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, SMALL_TEST_FILE_PATH, 10000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, SMALL_TEST_FILE_PATH, 10000);

#if !JSON_PARSER_ONLY
            LoopTest(typeof(JavaScriptSerializer).FullName, new JavaScriptSerializer().DeserializeObject, SMALL_TEST_FILE_PATH, 100000);
            LoopTest("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, SMALL_TEST_FILE_PATH, 100000);//(JSON.NET: OutOfMemoryException)
            //LoopTest("ServiceStack", new JsonSerializer<object>().DeserializeFromString, SMALL_TEST_FILE_PATH, 100000);
#endif
            LoopTest(typeof(JsonParser).FullName, new JsonParser().Parse<object>, SMALL_TEST_FILE_PATH, 100000);

#if !JSON_PARSER_ONLY
            var msJss = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            Test(typeof(JavaScriptSerializer).FullName, msJss.Deserialize<FathersData>, FATHERS_TEST_FILE_PATH);
            Test("JSON.NET 5.0 r8", JsonConvert.DeserializeObject<FathersData>, FATHERS_TEST_FILE_PATH);
            Test("ServiceStack", new JsonSerializer<FathersData>().DeserializeFromString, FATHERS_TEST_FILE_PATH);
#endif
            Test(typeof(JsonParser).FullName, new JsonParser().Parse<FathersData>, FATHERS_TEST_FILE_PATH);

            if (File.Exists(HUGE_TEST_FILE_PATH))
            {
#if !JSON_PARSER_ONLY
                Test(typeof(JavaScriptSerializer).FullName, msJss.DeserializeObject, HUGE_TEST_FILE_PATH);
                Test("JSON.NET 5.0 r8", JsonConvert.DeserializeObject, HUGE_TEST_FILE_PATH);//(JSON.NET: OutOfMemoryException)
                //Test("ServiceStack", new JsonSerializer<object>().DeserializeFromString, HUGE_TEST_FILE_PATH);
#endif
                Test(typeof(JsonParser).FullName, new JsonParser().Parse<object>, HUGE_TEST_FILE_PATH);
            }

            StreamTest();
        }

        static void StreamTest()
        {
            Console.Clear();
            System.Threading.Thread.MemoryBarrier();
            var initialMemory = System.GC.GetTotalMemory(true);
            using (var reader = new System.IO.StreamReader(FATHERS_TEST_FILE_PATH))
            {
                Console.WriteLine("\"Fathers\" Test... streamed (press a key)");
                Console.WriteLine();
                Console.ReadKey();
                var st = DateTime.Now;
                var o = new JsonParser().Parse<FathersData>(reader);
                var tm = (int)DateTime.Now.Subtract(st).TotalMilliseconds;

                System.Threading.Thread.MemoryBarrier();
                var finalMemory = System.GC.GetTotalMemory(true);
                var consumption = finalMemory - initialMemory;

                System.Diagnostics.Debug.Assert(o.fathers.Length == 30000);
                Console.WriteLine();
                Console.WriteLine("... {0} ms", tm);
                Console.WriteLine();
                Console.WriteLine("\tMemory used : {0}", ((decimal)finalMemory).ToString("0,0"));
                Console.WriteLine();
            }
            Console.ReadKey();
        }

        public static void Run()
        {
#if RUN_UNIT_TESTS
            UnitTests();
#endif
            SpeedTests();
        }
    }
}
