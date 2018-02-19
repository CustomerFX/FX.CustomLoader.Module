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
            this.ModulesFolder = "~/Custom/Modules";
            this.StyleFolder = "~/Custom/Style";
        }

        [DisplayName("Script Folder")]
        [Description("The root folder where custom javascript files are located. Default value is ~/Custom/Scripts")]
        public string ScriptFolder { get; set; }

        [DisplayName("Modules Folder")]
        [Description("The root folder where custom javascript module files are located. These are javascript files that are loaded as AMD modules. Default value is ~/Custom/Modules")]
        public string ModulesFolder { get; set; }

        [DisplayName("Style Folder")]
        [Description("The root folder where custom CSS style files are located. Default value is ~/Custom/Style")]
        public string StyleFolder { get; set; }

        public void Load()
        {
            // do not load on dialog pages (such as groupbuilder.aspx)
            if (!string.IsNullOrEmpty(this.CurrentPage.MasterPageFile) && this.CurrentPage.MasterPageFile.ToLower().Contains("dialog.master")) return;

            this.LoadModules();
            this.LoadScripts();
            this.LoadStyles();
        }

        private void LoadModules()
        {
            if (CurrentPage == null) return;

            var subDirectories = GetSubDirectories(ModulesFolder);
            foreach (var subDirectory in subDirectories)
            {
                var di = new DirectoryInfo(subDirectory);
                var script = string.Format("require({{ packages: [{{ name: '{0}', location: '{1}'}}] }}", di.Name, GetVirtualPath(subDirectory));
                if (di.GetFiles("*.js", SearchOption.TopDirectoryOnly).Count(x => x.Name.ToLower() == "main.js") > 0) script += string.Format(", ['{0}/main']", di.Name);
                script += "); ";

                CurrentPage.ClientScript.RegisterStartupScript(CurrentPage.GetType(), di.Name + "_Script", script, true);
            }
        }

        private void LoadScripts()
        {
            if (CurrentPage == null) return;

            var files = GetFiles(ScriptFolder, "js");
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                var script = string.Format("<script src=\"{0}\" type=\"text/javascript\"></script>", GetVirtualPath(file));
                CurrentPage.ClientScript.RegisterStartupScript(CurrentPage.GetType(), fi.Name.Replace(fi.Extension, "") + "_Script", script, false);
            }
        }

        private void LoadStyles()
        {
            if (CurrentPage == null) return;

            var files = GetFiles(StyleFolder, "css");
            foreach (var file in files)
            {
                CurrentPage.Header.Controls.Add(new LiteralControl(string.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />", GetVirtualPath(file))));
            }
        }

        public List<string> GetFiles(string VirtualPath, string Extension)
        {
            if (string.IsNullOrEmpty(VirtualPath) || !Directory.Exists(GetServerPath(VirtualPath))) return new List<string>();
            return Directory.GetFiles(GetServerPath(VirtualPath), "*." + Extension, SearchOption.AllDirectories).ToList();
        }

        public List<string> GetSubDirectories(string VirtualPath)
        {
            if (string.IsNullOrEmpty(VirtualPath) || !Directory.Exists(GetServerPath(VirtualPath))) return new List<string>();
            return Directory.GetDirectories(GetServerPath(VirtualPath), "*", SearchOption.TopDirectoryOnly).ToList();
        }

        private string GetServerPath(string VirtualPath)
        {
            return HttpContext.Current.Server.MapPath(VirtualPath);
        }

        private string GetVirtualPath(string PhysicalPath)
        {
            return PhysicalPath.Replace(HttpContext.Current.Server.MapPath("~/"), HttpContext.Current.Request.ApplicationPath + "/").Replace(@"\", "/");
        }

        private Page CurrentPage
        {
            get { return HttpContext.Current.Handler as Page; }
        }
    }
}
