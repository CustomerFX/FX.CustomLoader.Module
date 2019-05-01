using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using Sage.Platform.Application;
using Sage.Platform.Application.UI;
using Sage.Platform.WebPortal.Workspaces;
using Sage.Platform.WebPortal.Workspaces.Tab;
using FX.CustomLoader.Configuration;

namespace FX.CustomLoader
{
    public class Module : Sage.Platform.Application.IModule
    {
        [ServiceDependency(Type = typeof(WorkItem))]
        public UIWorkItem PageWorkItem { get; set; }

        public Module()
        {
            this.ScriptFolder = "~/Custom/Scripts";
            this.ModulesFolder = "~/Custom/Modules";
            this.StyleFolder = "~/Custom/Style";
            this.FormsFolder = "~/Custom/Forms";
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

        [DisplayName("Forms Folder")]
        [Description("The root folder where SmartPart definition files are located. Default value is ~/Custom/Forms")]
        public string FormsFolder { get; set; }

        private MainContentWorkspace _mainworkspace = null;
        private MainContentWorkspace MainContent
        {
            get { return _mainworkspace ?? (_mainworkspace = PageWorkItem.Workspaces["MainContent"] as MainContentWorkspace); }
        }

        private TabWorkspace _tabworkspace = null;
        private TabWorkspace TabWorkspace
        {
            get { return _tabworkspace ?? (_tabworkspace = PageWorkItem.Workspaces["TabControl"] as TabWorkspace); }
        }

        private TaskPaneWorkspace _taskworkspace = null;
        private TaskPaneWorkspace TaskPane
        {
            get { return _taskworkspace ?? (_taskworkspace = PageWorkItem.Workspaces["TaskPane"] as TaskPaneWorkspace); }
        }

        private DialogWorkspace _dialogworkspace = null;
        private DialogWorkspace DialogWorkspace
        {
            get { return _dialogworkspace ?? (_dialogworkspace = PageWorkItem.Workspaces["DialogWorkspace"] as DialogWorkspace); }
        }

        public void Load()
        {
            // do not load on dialog pages (such as groupbuilder.aspx)
            if (!string.IsNullOrEmpty(this.CurrentPage.MasterPageFile) && this.CurrentPage.MasterPageFile.ToLower().Contains("dialog.master")) return;

            this.LoadModules();
            this.LoadScripts();
            this.LoadStyles();
            this.LoadForms();
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

        /*
         * Sample Config (can be named anything.json)
         * {
         *      "ID": "mySmartPart",
         *      "SmartPart": "~/SmartParts/Account/MySmartPart.ascx",
         *      "Workspace": "DialogWorkspace",
         *      "Title": "Title",  <- optional
         *      "ShowInMode": "",  <- optional
         *      "EntityTypes": ["Account", "Contact"]  <- optional, omit to add form on all pages
         * }
         */
        private void LoadForms()
        {
            if (CurrentPage == null) return;

            var files = GetFiles(FormsFolder, "json");
            foreach (var file in files)
            {
                var config = FormConfiguration.LoadFromFile(file);
                if (config != null)
                {
                    if (string.IsNullOrEmpty(config.SmartPart) || string.IsNullOrEmpty(config.ID) || string.IsNullOrEmpty(config.Workspace)) continue;
                    if (!File.Exists(HttpContext.Current.Server.MapPath(config.SmartPart))) continue;

                    if (config.EntityTypes != null && config.EntityTypes.Length > 0 && !config.EntityTypes.Contains(CurrentEntityType)) continue;

                    Control control;
                    try
                    {
                        control = CurrentPage.LoadControl(config.SmartPart);
                    }
                    catch (Exception ex)
                    {
                        control = new LiteralControl(ex.Message);
                    }

                    control.ID = config.ID;
                    if (string.IsNullOrEmpty(config.Title)) config.Title = config.ID;

                    var workspace = GetWorkspaceFromName(config.Workspace);
                    if (workspace != null) workspace.Show(control, new SmartPartInfo(config.Title, config.Title));
                }
            }
        }

        private IWorkspace GetWorkspaceFromName(string WorkspaceName)
        {
            var prop = this.GetType().GetProperty(WorkspaceName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop == null) return null;

            return (IWorkspace)prop.GetValue(this, null);
        }

        private List<string> GetFiles(string VirtualPath, string Extension)
        {
            if (string.IsNullOrEmpty(VirtualPath) || !Directory.Exists(GetServerPath(VirtualPath))) return new List<string>();
            return Directory.GetFiles(GetServerPath(VirtualPath), "*." + Extension, SearchOption.AllDirectories).ToList();
        }

        private List<string> GetSubDirectories(string VirtualPath)
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

        private string CurrentEntityType
        {
            get
            {
                var entityName = Path.GetFileNameWithoutExtension(HttpContext.Current.Request.Url.LocalPath);
                return entityName.Substring(0, 1).ToUpper() + entityName.Substring(1).ToLower();
            }
        }
    }
}
