using System.Runtime.InteropServices.JavaScript;
using TypeScriptExport;

namespace DotnetWasmTypescript.InteropGenerator.SampleClasses
{
    [TsExport]
    public class PersonX
    {
        public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }

        internal PersonNameX Name { get; set; }

        internal int Age { get; set; }

        
        public string GetName()
        {
            return Name.Value;
        }

        public void SetName(PersonNameX name)
        {
            this.Name = name;
        }

        public void SetProperties(PersonNameX name, int age)
        {
            this.Name = name;
            Age = age;
        }

        public void SetProperties2(int age, PersonNameX name)
        {
            this.Name = name;
            Age = age;
        }

        // TODO: run experiments with collections
        //public void SetPropertiesNoisy(int age, PersonNameX name, PersonNameX[] names)//, List<PersonX> people)
        //{
        //    this.Name = name;
        //    Age = age;
        //}
    }

    [TsExport]
    public partial class PersonNameX
    {
        public static implicit operator PersonNameX(string value) => new() { Value = value };
        public static implicit operator PersonNameX(JSObject jsObj) => new()
        {
            Value = jsObj.GetPropertyAsString(nameof(Value)) ?? throw new ArgumentException($"JSObject is not a valid {nameof(PersonNameX)}", nameof(jsObj))
        };

        public string Value { get; set; }
    }
}