using System;
using System.IO;
using FX.CustomLoader.Utility;

namespace FX.CustomLoader.Configuration
{
    internal class FormConfiguration
    {
        public string ID { get; set; }
        public string SmartPart { get; set; }
        public string Title { get; set; }
        public string Workspace { get; set; }
        public string ShowInMode { get; set; }
        public string[] EntityTypes { get; set; }

        public static FormConfiguration LoadFromFile(string FileName)
        {
            if (!File.Exists(FileName)) return null;
            return FormConfiguration.LoadFromJson(File.ReadAllText(FileName));
        }

        public static FormConfiguration LoadFromJson(string Json)
        {
            return Json.FromJson<FormConfiguration>();
        }
    }
}
