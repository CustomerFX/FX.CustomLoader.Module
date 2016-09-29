# FX.CustomLoader.Module
A module for Infor CRM to dynamically load custom javascript and style files without the need to modify ant master pages. See [this article](http://customerfx.com/article/loading-custom-javascript-and-style-files-in-infor-crm-saleslogix-web-without-modifying-master-pages/) for complete details and instructions.

In a nutshell, this is how it works. There are folders you place your scripts and stylesheets in and this module will load them all at runtime for you. No need to modify the master pages ever again.

#Steps to Use
1. [Download the bundle](https://github.com/CustomerFX/FX.CustomLoader.Module/raw/master/Deliverables/Custom%20Loader%20Module.zip)
2. Install the bundle in Application Architect
3. Look in the SlxClient/SupportFiles and you'll see a folder called "Custom" which contains two subfolders named "Scripts" and "Style"
4. Drop any javascript files in the SupportFiles/Custom/Scripts folder (you can organize things with subfolders if you'd like)
5. Drop any CSS style files in the SupportFiles/Custom/Style folder (you can organize things with subfolders if you'd like)
