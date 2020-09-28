@echo off
xcopy /e /i ..\DesktopModules\Vanjaro\Core\Library\Themes\* ..\Portals\_default\vThemes\*  /Y >NUL
setlocal
:PROMPT
SET /P AREYOUSURE=Build Packages (Y/[N])?: 
CLS
IF /I "%AREYOUSURE%" NEQ "Y" GOTO END

SET /P Version=Please Enter Package Version: 
CLS
echo Creating Extension Packages...
xcopy /e /i ..\DesktopModules\Vanjaro\Common\Install\* ..\DesktopModules\Vanjaro\Core\Library\Install\*  /Y >NUL
xcopy /e /i ..\DesktopModules\Vanjaro\URL\Install\* ..\DesktopModules\Vanjaro\Core\Library\Install\*  /Y >NUL
xcopy /e /i ..\DesktopModules\Vanjaro\UXManager\Extensions\Migrate\Install\* ..\DesktopModules\Vanjaro\Core\Library\Install\*  /Y >NUL

cd ..\DesktopModules\Vanjaro\UXManager\Library\ >NUL
rd bin /s /q >NUL
md bin >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Library.dll bin\  >NUL

copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.About.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.Icon.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.Image.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.Link.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.ModuleSettings.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.Video.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.LogsSettings.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.BlockLanguage.dll bin\  >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.BreadCrumb.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.Custom.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.Login.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.LoginLink.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.Logo.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.Menu.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.Profile.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.Register.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.RegisterLink.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.SearchInput.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Block.SearchResult.dll bin\ >NUL

copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Azure.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.EmailServiceProvider.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.GoogleAnalytics.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Pixabay.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.YouTube.dll bin\ >NUL

copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Assets.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.CustomCSS.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Domain.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Extensions.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Help.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Languages.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.LogoAndTitle.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Logs.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Pages.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Roles.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Scheduler.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Security.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.SEO.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.SiteGroups.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Sites.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.SQLConsole.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Theme.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Users.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Workflow.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.Privacy.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Menu.MemberProfile.dll bin\ >NUL

copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.DeviceMode.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.Fullscreen.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.Language.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.Navigator.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.PageSetting.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.Preview.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.Redo.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.Undo.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.UXManager.Extensions.Toolbar.ViewLayout.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.Migrate.dll bin\ >NUL


copy ..\..\..\..\bin\Vanjaro.Core.Extensions.Workflow.Review.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.Core.Extensions.Notification.Notification.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.Core.Providers.Authentication.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.Core.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.Skin.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.Container.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.Common.dll bin\ >NUL
copy ..\..\..\..\bin\Mandeeps.DNN.Modules.Licensing.dll bin\ >NUL
copy ..\..\..\..\bin\Vanjaro.URL.dll bin\ >NUL

copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Vanjaro\bin\HtmlAgilityPack.dll bin\ >NUL

copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Vanjaro\bin\LibSassHost.dll bin\ >NUL
copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Vanjaro\bin\AdvancedStringBuilder.dll bin\ >NUL
copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Vanjaro\bin\System.Buffers.dll bin\ >NUL
copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Vanjaro\bin\ImageProcessor.dll bin\ >NUL
copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Vanjaro\bin\ImageProcessor.Plugins.WebP.dll bin\ >NUL

del library-uxmanager-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a library-uxmanager-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Extensions\Apps\About\ >NUL
del apps-about-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a apps-about-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Icon\ >NUL
del apps-icon-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a apps-icon-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Image\ >NUL
del apps-image-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a apps-image-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Link\ >NUL
del apps-link-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a apps-link-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\ModuleSettings\ >NUL
del apps-modulesettings-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a apps-modulesettings-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Video\ >NUL
del apps-video-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a apps-video-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\Block\Language >NUL
del block-language-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-language-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\BreadCrumb >NUL
del block-breadcrumb-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-breadcrumb-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Custom >NUL
del block-custom-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-custom-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Login >NUL
del block-login-resources.zip >NUL
"C:\Program Files\7-Zip\7z.exe" a block-login-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\LoginLink >NUL
del block-loginlink-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-loginlink-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Logo >NUL
del block-logo-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-logo-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Menu >NUL
del block-menu-resources.zip >NUL
"C:\Program Files\7-Zip\7z.exe" a block-menu-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Profile >NUL
del block-profile-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-profile-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Register >NUL
del block-register-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-register-resources.zip @Resources.txt -xr!?svn\ >NUL


cd ..\RegisterLink >NUL
del block-registerlink-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-registerlink-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\SearchInput >NUL
del block-searchinput-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-searchinput-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\SearchResult >NUL
del block-searchresult-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a block-searchresult-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\Menu\Azure >NUL
del menu-integrations-azure-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-integrations-azure-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\EmailServiceProvider >NUL
del menu-integrations-emailserviceprovider-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-integrations-emailserviceprovider-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\GoogleAnalytics >NUL
del menu-integrations-googleanalytics-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-integrations-googleanalytics-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Pixabay >NUL
del menu-integrations-pixabay-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-integrations-pixabay-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\YouTube >NUL
del menu-integrations-youtube-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-integrations-youtube-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Assets >NUL
del menu-assest-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-assest-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\CustomCSS >NUL
del menu-customecss-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-customecss-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Domain >NUL
del menu-domain-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-domain-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Extensions >NUL
del menu-extensions-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-extensions-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Help >NUL
del menu-help-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-help-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Languages >NUL
del menu-languages-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-languages-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\LogoAndTitle >NUL
del menu-logoandtitle-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-logoandtitle-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Logs >NUL
del menu-logs-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-logs-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\LogsSettings >NUL
del menu-logssettings-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-logssettings-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Pages >NUL
del menu-pages-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-pages-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Roles >NUL
del menu-roles-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-roles-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Scheduler >NUL
del menu-scheduler-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-scheduler-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Security >NUL
del menu-security-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-security-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\SEO >NUL
del menu-seo-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-seo-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\SiteGroups >NUL
del menu-sitegroups-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-sitegroups-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Sites >NUL
del menu-sites-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-sites-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\SQLConsole >NUL
del menu-sqlconsole-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-sqlconsole-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Theme >NUL
del menu-theme-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-theme-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\ThemeBuilder >NUL
del menu-themebuilder-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-themebuilder-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Users >NUL
del menu-users-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-users-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Workflow >NUL
del menu-workflow-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-workflow-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Privacy >NUL
del menu-privacy-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-privacy-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\MemberProfile >NUL
del menu-memberprofile-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a menu-memberprofile-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\Toolbar\DeviceMode >NUL
del toolbar-devicemode-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-devicemode-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Fullscreen >NUL
del toolbar-fullscreen-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-fullscreen-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Language >NUL
del toolbar-language-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-language-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Navigator >NUL
del toolbar-navigator-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-navigator-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\PageSetting >NUL
del toolbar-pagesetting-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-pagesetting-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Preview >NUL
del toolbar-preview-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-preview-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Redo >NUL
del toolbar-redo-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-redo-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Undo >NUL
del toolbar-undo-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-undo-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\VersionManagement >NUL
del toolbar-versionmanagement-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-versionmanagement-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\ViewLayout >NUL
del toolbar-viewlayout-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a toolbar-viewlayout-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\Migrate >NUL
del migrate-vanjaro.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a migrate-vanjaro.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\..\..\..\DesktopModules\Vanjaro\Core\Extensions\Notification\Notification >NUL
del core-extensions-notification-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a core-extensions-notification-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\Workflow\Review\ >NUL
del core-extensions-review-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a core-extensions-review-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\..\Library\ >NUL
del core-library-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a core-library-resources.zip @Resources.txt -xr!?svn\ >NUL

cd Themes\ >NUL
del ..\themes-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a ..\themes-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Packager\Licenses\ >NUL
del ..\..\licenses-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a ..\..\licenses-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\..\Providers\Authentication >NUL
del providers-authentication-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a providers-authentication-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\..\..\..\..\portals\_default\skins\vanjaro >NUL
del skin-resource.zip >NUL 2>&1
"c:\program files\7-zip\7z.exe" a skin-resource.zip @resources.txt -xr!?svn\ >NUL

cd ..\..\containers\vanjaro\ >NUL
del container-resource.zip >NUL 2>&1
"c:\program files\7-zip\7z.exe" a container-resource.zip @resources.txt -xr!?svn\ >NUL

cd ..\..\..\..\DesktopModules\Vanjaro\Common\ >NUL
del common-library-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a common-library-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\URL\ >NUL
del url-resources.zip >NUL 2>&1
"C:\Program Files\7-Zip\7z.exe" a url-resources.zip @Resources.txt -xr!?svn\ >NUL

cd ..\Core\Library\ >NUL


del ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x64_Install.zip >NUL 2>&1
del ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x64_Install.zip >NUL 2>&1
del ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x86_Install.zip >NUL 2>&1
del ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x86_Install.zip >NUL 2>&1
del ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x64_Upgrade.zip >NUL 2>&1
del ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x86_Upgrade.zip >NUL 2>&1


"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x64_Install.zip @PackageList.txt >NUL
"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x86_Install.zip @PackageList.txt >NUL

copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x64_Install.zip ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x64_Install.zip >NUL

"C:\Program Files\7-Zip\7z.exe" d -r ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x64_Install.zip @DistrubtionRemoveList.txt >NUL

"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x64_Install.zip @DistrubtionAddList.txt >NUL

"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x64_Install.zip Packager\Vanjaro\bin\x64\*.dll >NUL
"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x64_Install.zip Packager\Vanjaro\bin\x64\*.dll >NUL

"C:\Program Files\7-Zip\7z.exe" rn ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x64_Install.zip Packager\Vanjaro\bin\x64\libsass.dll bin\libsass.dll >NUL
"C:\Program Files\7-Zip\7z.exe" rn ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x64_Install.zip Packager\Vanjaro\bin\x64\libsass.dll bin\libsass.dll >NUL

copy ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x86_Install.zip ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x86_Install.zip >NUL

"C:\Program Files\7-Zip\7z.exe" d -r ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x86_Install.zip @DistrubtionRemoveList.txt >NUL

"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x86_Install.zip @DistrubtionAddList.txt >NUL

"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x86_Install.zip Packager\Vanjaro\bin\x86\*.dll >NUL
"C:\Program Files\7-Zip\7z.exe" a ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x86_Install.zip Packager\Vanjaro\bin\x86\*.dll >NUL


"C:\Program Files\7-Zip\7z.exe" rn ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_Platform_"%Version%"_x86_Install.zip Packager\Vanjaro\bin\x86\libsass.dll bin\libsass.dll >NUL
"C:\Program Files\7-Zip\7z.exe" rn ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\Releases\Vanjaro_For_DNN_"%Version%"_x86_Install.zip Packager\Vanjaro\bin\x86\libsass.dll bin\libsass.dll >NUL


cd ..\..\..\..\DesktopModules\Vanjaro\Core\Library\Packager\ 
del Temp_DNN /Q >NUL 2>&1
mkdir Temp_DNN >NUL
xcopy /e /i "DNN Install"\* Temp_DNN\*  /Y >NUL

::Need to copy the Core DLL for Vanjaro Installer
copy ..\..\..\..\..\bin\Vanjaro.Core.dll Temp_DNN\bin >NUL

cd Temp_DNN\



echo Optimizing DNN Install...

del DNN.ico /Q >NUL 2>&1
del favicon.ico /Q >NUL 2>&1
del compilerconfig.json /Q >NUL 2>&1

rmdir admin\Containers /S/Q >NUL 2>&1
rmdir admin\Sales /S/Q >NUL 2>&1

mkdir admin\SecurityTemp >NUL
move admin\Security\App_LocalResources\PasswordReset.ascx.resx admin\SecurityTemp >NUL
del admin\Security\* /S/Q >NUL 2>&1
mkdir admin\Security\App_LocalResources >NUL 2>&1
move admin\SecurityTemp\PasswordReset.ascx.resx admin\Security\App_LocalResources\ >NUL
rmdir admin\SecurityTemp >NUL 2>&1

mkdir admin\MenusTemp >NUL
move admin\Menus\ModuleActions\ModuleActions.ascx admin\MenusTemp >NUL
del admin\Menus\* /S/Q >NUL 2>&1
mkdir admin\Menus\ModuleActions >NUL 2>&1
move admin\MenusTemp\ModuleActions.ascx admin\Menus\ModuleActions >NUL
rmdir admin\MenusTemp >NUL 2>&1
rmdir admin\Menus\DNNActions >NUL 2>&1
rmdir admin\Menus\DNNAdmin >NUL 2>&1
rmdir admin\Menus\ModuleActions\images >NUL 2>&1

mkdir admin\SkinsTemp >NUL
move admin\Skins\modulemessage.ascx admin\SkinsTemp >NUL
del admin\Skins\* /S/Q >NUL 2>&1
rmdir admin\Skins\App_LocalResources /S/Q >NUL 2>&1
move admin\SkinsTemp\modulemessage.ascx admin\Skins\ >NUL
rmdir admin\SkinsTemp /S/Q >NUL 2>&1
rmdir admin\App_LocalResources /S/Q >NUL 2>&1
rmdir admin\Tabs /S/Q >NUL 2>&1
rmdir admin\Users /S/Q >NUL 2>&1
:: rmdir controls /S/Q
rmdir icons\sigma /S/Q >NUL 2>&1
rmdir images /S/Q >NUL 2>&1
mkdir images\Branding >NUL
del Install\AuthSystem\* /S/Q >NUL 2>&1


mkdir Install\JavaScriptLibraryTEMP >NUL
move Install\JavaScriptLibrary\jQuery_* Install\JavaScriptLibraryTEMP\ >NUL
move Install\JavaScriptLibrary\jQueryMigrate_* Install\JavaScriptLibraryTEMP\ >NUL
del Install\JavaScriptLibrary\* /S/Q >NUL 2>&1
move Install\JavaScriptLibraryTEMP\jQuery_* Install\JavaScriptLibrary\ >NUL
move Install\JavaScriptLibraryTEMP\jQueryMigrate_* Install\JavaScriptLibrary\ >NUL
rmdir Install\JavaScriptLibraryTEMP /S/Q >NUL 2>&1

del Install\Library\* /S/Q >NUL 2>&1

mkdir Install\ModuleTEMP >NUL
move Install\Module\Newtonsoft* Install\ModuleTEMP\ >NUL
:: move Install\Module\DNNCE_Azure* Install\ModuleTEMP\
move Install\Module\DNN.Persona* Install\ModuleTEMP\ >NUL
:: move Install\Module\GoogleAnalytics* Install\ModuleTEMP\
del Install\Module\* /S/Q >NUL 2>&1
move Install\ModuleTemp\* Install\Module\ >NUL
rmdir Install\ModuleTemp /S/Q >NUL 2>&1


mkdir Install\ProviderTEMP >NUL
move Install\Provider\DNNCE_* Install\ProviderTEMP\ >NUL
del Install\Provider\* /S/Q >NUL 2>&1
move Install\ProviderTEMP\* Install\Provider\ >NUL
rmdir Install\ProviderTEMP /S/Q >NUL 2>&1

del Install\Skin\* /S/Q >NUL 2>&1
del Install\Template\* /S/Q >NUL 2>&1
del Install\InstallWizard.aspx.cs >NUL 2>&1
del Install\InstallWizard.aspx >NUL 2>&1
::mkdir jsTEMP
::move js\Microsoft* jsTEMP\
::del js\* /S/Q
::move jsTEMP\* js\
::rmdir jsTEMP /S/Q
::rmdir js\Debug /S/Q

echo Creating Platform Install Packages...
cd ..\vanjaro\ 
copy "Blank Website.template" ..\Temp_DNN\Portals\_default\ >NUL
copy "Default Website.template" ..\Temp_DNN\Portals\_default\ >NUL
copy "Default Website.template.resources" ..\Temp_DNN\Portals\_default\ >NUL
copy "DotNetNuke.install.config.resources" ..\Temp_DNN\Install\ >NUL
copy "InstallWizard.aspx.cs.resources" ..\Temp_DNN\Install\InstallWizard.aspx.cs >NUL
copy "InstallWizard.aspx.resources" ..\Temp_DNN\Install\InstallWizard.aspx >NUL
copy Install\Install.aspx.cs ..\Temp_DNN\Install\Install.aspx.cs >NUL
copy Install\Install.aspx.designer.cs ..\Temp_DNN\Install\Install.aspx.designer.cs >NUL
copy Install\Install.htm ..\Temp_DNN\Install\Install.htm >NUL
copy "vcustom.css" ..\Temp_DNN\Resources\Shared\stylesheets\ >NUL
copy Images\*.* ..\Temp_DNN\Images >NUL
copy "Images\Branding\Vanjaro_logo.png" ..\Temp_DNN\Images\Branding\ >NUL

cd ..\Releases >NUL
copy Vanjaro_Platform_"%Version%"_x64_Install.zip ..\Temp_DNN\Install\Module\ >NUL
copy Vanjaro_Platform_"%Version%"_x86_Install.zip ..\Temp_DNN\Install\Module\ >NUL

cd ..\Temp_DNN 
del Vanjaro_Platform_"%Version%"_x64_Install.zip >NUL 2>&1
del Vanjaro_Platform_"%Version%"_x86_Install.zip >NUL 2>&1


"C:\Program Files\7-Zip\7z.exe" a Vanjaro_Platform_"%Version%"_x64_Install.zip -xr!?svn >NUL
"C:\Program Files\7-Zip\7z.exe" a Vanjaro_Platform_"%Version%"_x86_Install.zip -xr!?svn >NUL

"C:\Program Files\7-Zip\7z.exe" d -r Vanjaro_Platform_"%Version%"_x86_Install.zip Vanjaro_Platform_"%Version%"_x64_Install.zip >NUL

"C:\Program Files\7-Zip\7z.exe" d -r Vanjaro_Platform_"%Version%"_x64_Install.zip Install\Module\Vanjaro_Platform_"%Version%"_x86_Install.zip >NUL
"C:\Program Files\7-Zip\7z.exe" d -r Vanjaro_Platform_"%Version%"_x86_Install.zip Install\Module\Vanjaro_Platform_"%Version%"_x64_Install.zip >NUL
move Vanjaro_Platform_"%Version%"_x64_Install.zip ..\Releases\ >NUL
move Vanjaro_Platform_"%Version%"_x86_Install.zip ..\Releases\ >NUL



cd ..\..\Packager\

del UpgradeTemp_DNN /Q >NUL 2>&1
mkdir UpgradeTemp_DNN >NUL
xcopy /e /i "DNN Upgrade"\* UpgradeTemp_DNN\*  /Y >NUL

::Need to copy the Core DLL to Upgrade Package for Upgrade Page (Upgrade.aspx) to kick off
copy ..\..\..\..\..\bin\Vanjaro.Core.dll UpgradeTemp_DNN\bin >NUL

cd UpgradeTemp_DNN\

echo Optimizing DNN Upgrade...
del DNN.ico /Q >NUL 2>&1
del compilerconfig.json /Q >NUL 2>&1



rmdir admin\Containers /S/Q >NUL 2>&1
rmdir admin\Sales /S/Q >NUL 2>&1

mkdir admin\SecurityTemp >NUL
move admin\Security\App_LocalResources\PasswordReset.ascx.resx admin\SecurityTemp >NUL
del admin\Security\* /S/Q >NUL 2>&1
mkdir admin\Security\App_LocalResources >NUL 2>&1
move admin\SecurityTemp\PasswordReset.ascx.resx admin\Security\App_LocalResources\ >NUL
rmdir admin\SecurityTemp >NUL 2>&1



mkdir admin\MenusTemp >NUL
move admin\Menus\ModuleActions\ModuleActions.ascx admin\MenusTemp >NUL
del admin\Menus\* /S/Q >NUL 2>&1
mkdir admin\Menus\ModuleActions >NUL 2>&1
move admin\MenusTemp\ModuleActions.ascx admin\Menus\ModuleActions >NUL
rmdir admin\MenusTemp >NUL 2>&1
rmdir admin\Menus\DNNActions >NUL 2>&1
rmdir admin\Menus\DNNAdmin >NUL 2>&1
rmdir admin\Menus\ModuleActions\images >NUL 2>&1



mkdir admin\SkinsTemp >NUL
move admin\Skins\modulemessage.ascx admin\SkinsTemp >NUL
del admin\Skins\* /S/Q >NUL 2>&1
rmdir admin\Skins\App_LocalResources /S/Q >NUL 2>&1
move admin\SkinsTemp\modulemessage.ascx admin\Skins\ >NUL
rmdir admin\SkinsTemp /S/Q >NUL 2>&1
rmdir admin\App_LocalResources /S/Q >NUL 2>&1
rmdir admin\Tabs /S/Q >NUL 2>&1
rmdir admin\Users /S/Q >NUL 2>&1
rmdir icons\sigma /S/Q >NUL 2>&1
rmdir images /S/Q >NUL 2>&1
mkdir images\Branding >NUL
del Install\AuthSystem\* /S/Q >NUL 2>&1


mkdir Install\JavaScriptLibraryTEMP >NUL
move Install\JavaScriptLibrary\jQuery_* Install\JavaScriptLibraryTEMP\ >NUL
move Install\JavaScriptLibrary\jQueryMigrate_* Install\JavaScriptLibraryTEMP\ >NUL
del Install\JavaScriptLibrary\* /S/Q >NUL 2>&1
move Install\JavaScriptLibraryTEMP\jQuery_* Install\JavaScriptLibrary\ >NUL
move Install\JavaScriptLibraryTEMP\jQueryMigrate_* Install\JavaScriptLibrary\ >NUL
rmdir Install\JavaScriptLibraryTEMP /S/Q >NUL 2>&1

del Install\Library\* /S/Q >NUL 2>&1

mkdir Install\ModuleTEMP >NUL
move Install\Module\Newtonsoft* Install\ModuleTEMP\ >NUL
move Install\Module\DNN.Persona* Install\ModuleTEMP\ >NUL
del Install\Module\* /S/Q >NUL 2>&1
move Install\ModuleTemp\* Install\Module\ >NUL
rmdir Install\ModuleTemp /S/Q >NUL 2>&1


mkdir Install\ProviderTEMP >NUL
move Install\Provider\DNNCE_* Install\ProviderTEMP\ >NUL
del Install\Provider\* /S/Q >NUL 2>&1
move Install\ProviderTEMP\* Install\Provider\ >NUL
rmdir Install\ProviderTEMP /S/Q >NUL 2>&1

del Install\Skin\* /S/Q >NUL 2>&1
del Install\Template\* /S/Q >NUL 2>&1
del Install\InstallWizard.aspx.cs >NUL 2>&1
del Install\InstallWizard.aspx >NUL 2>&1

echo Creating Platform Upgrade Packages...
cd ..\vanjaro\ 
copy "Blank Website.template" ..\UpgradeTemp_DNN\Portals\_default\ >NUL
copy "Default Website.template" ..\UpgradeTemp_DNN\Portals\_default\ >NUL
copy "Default Website.template.resources" ..\UpgradeTemp_DNN\Portals\_default\ >NUL
copy "DotNetNuke.install.config.resources" ..\UpgradeTemp_DNN\Install\ >NUL
copy Install\Install.aspx.cs ..\UpgradeTemp_DNN\Install\Install.aspx.cs >NUL
copy Install\Install.aspx.designer.cs ..\UpgradeTemp_DNN\Install\Install.aspx.designer.cs >NUL
copy Install\Install.htm ..\UpgradeTemp_DNN\Install\Install.htm >NUL
copy "vcustom.css" ..\UpgradeTemp_DNN\Resources\Shared\stylesheets\ >NUL

copy Images\*.* ..\UpgradeTemp_DNN\Images >NUL
copy "Images\Branding\Vanjaro_logo.png" ..\UpgradeTemp_DNN\Images\Branding\ >NUL

cd ..\Temp_DNN\
copy Install\Module\Vanjaro_Platform_"%Version%"_x64_Install.zip ..\UpgradeTemp_DNN\Install\Module\ >NUL
copy Install\Module\Vanjaro_Platform_"%Version%"_x86_Install.zip ..\UpgradeTemp_DNN\Install\Module\ >NUL

cd ..\UpgradeTemp_DNN\


"C:\Program Files\7-Zip\7z.exe" a Vanjaro_Platform_"%Version%"_x64_Upgrade.zip -xr!?svn >NUL
"C:\Program Files\7-Zip\7z.exe" a Vanjaro_Platform_"%Version%"_x86_Upgrade.zip -xr!?svn >NUL

"C:\Program Files\7-Zip\7z.exe" d -r Vanjaro_Platform_"%Version%"_x86_Upgrade.zip Vanjaro_Platform_"%Version%"_x64_Upgrade.zip >NUL

"C:\Program Files\7-Zip\7z.exe" d -r Vanjaro_Platform_"%Version%"_x64_Upgrade.zip Install\Module\Vanjaro_Platform_"%Version%"_x86_Install.zip >NUL
"C:\Program Files\7-Zip\7z.exe" d -r Vanjaro_Platform_"%Version%"_x86_Upgrade.zip Install\Module\Vanjaro_Platform_"%Version%"_x64_Install.zip >NUL

move Vanjaro_Platform_"%Version%"_x64_Upgrade.zip ..\Releases\ >NUL
move Vanjaro_Platform_"%Version%"_x86_Upgrade.zip ..\Releases\ >NUL

cd ..\
rmdir UpgradeTemp_DNN /s /q >NUL 2>&1
rmdir Temp_DNN /s /q >NUL 2>&1


"C:\Program Files\7-Zip\7z.exe" a Releases\Vanjaro_Platform_"%Version%"_x64_Upgrade.zip Vanjaro\Install\Upgrade.aspx >NUL
"C:\Program Files\7-Zip\7z.exe" rn Releases\Vanjaro_Platform_"%Version%"_x64_Upgrade.zip Vanjaro\install\Upgrade.aspx Install/Upgrade.aspx >NUL

"C:\Program Files\7-Zip\7z.exe" a Releases\Vanjaro_Platform_"%Version%"_x86_Upgrade.zip Vanjaro\Install\Upgrade.aspx >NUL
"C:\Program Files\7-Zip\7z.exe" rn Releases\Vanjaro_Platform_"%Version%"_x86_Upgrade.zip Vanjaro\install\Upgrade.aspx Install/Upgrade.aspx >NUL


echo.
echo.
echo. 
echo All Packages Created!
echo. 
SET /P AnyKeyExit=Please any key to terminate

:END
endlocal
exit