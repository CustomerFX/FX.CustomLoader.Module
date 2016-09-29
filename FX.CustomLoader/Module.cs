using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace FX.CustomLoader
{
    public class Module : Sage.Platform.Application.IModule
    {
        public Module()
        {
            this.ScriptFolder = "~/Custom/Scripts";
            this.StyleFolder = "~/Custom/Style";
        }

        [DisplayName("Script Folder")]
        [Description("The root folder where custom javascript files are located. Default value is ~/Custom/Scripts")]
        public string ScriptFolder { get; set; }

        [DisplayName("Style Folder")]
        [Description("The root folder where custom CSS style files are located. Default value is ~/Custom/Style")]
        public string StyleFolder { get; set; }

        public void Load()
        {
            this.LoadScripts();
            this.LoadStyles();
        }

        private void LoadScripts()
        {
            if (CurrentPage == null) return;

            var files = GetFiles(ScriptFolder, "js");
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                CurrentPage.ClientScript.RegisterClientScriptInclude(CurrentPage.GetType(), fi.Name.Replace(fi.Extension, "") + "_Script", GetVirtualPath(file));
            }
        }

        private void LoadStyles()
        {
            if (CurrentPage == null) return;

            var files = GetFiles(StyleFolder, "css");
            foreach (var file in files)
            {
                CurrentPage.Header.Controls.Add(new LiteralControl(string.Format("<link rel='stylesheet' type'text/css' href='{0}'/>", file)));
            }
        }

        public List<string> GetFiles(string VirtualPath, string Extension)
        {
            if (string.IsNullOrEmpty(VirtualPath) || !Directory.Exists(GetServerPath(VirtualPath))) return new List<string>();
            return Directory.GetFiles(GetServerPath(VirtualPath), "*." + Extension, SearchOption.AllDirectories).ToList();
        }

        private string GetServerPath(string VirtualPath)
        {
            return HttpContext.Current.Server.MapPath(VirtualPath);
        }

        private string GetVirtualPath(string PhysicalPath)
        {
            return PhysicalPath.Replace(HttpContext.Current.Server.MapPath("~/"), "~/").Replace(@"\", "/");
        }

        private Page CurrentPage
        {
            get { return HttpContext.Current.Handler as Page; }
        }
    }
}
