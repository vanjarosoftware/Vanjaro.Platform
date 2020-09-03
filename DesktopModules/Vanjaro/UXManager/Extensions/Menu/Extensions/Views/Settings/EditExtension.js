app.controller('settings_editextension', function ($scope, $attrs, $http, CommonSvc) {

    //Initailize Variable
    var common = CommonSvc.getData($scope);
    $scope.ShowAdvanceTab = false;
    $scope.ShowPermissionTab = false;
    $scope.ModuleType = false;
    $scope.Auth_SystemType = false;
    $scope.ContainerType = false;
    $scope.JavaScript_LibraryType = false;
    $scope.ControlType = -1;
    $scope.ControlIcon = "";
    $scope.onInit = function () {
        var Version = $scope.ui.data.packageDetail.Options.version.split('.');
        if (Version[0] !== undefined)
            $scope.MajorVersion = Version[0] > 9 ? Version[0] : '0' + Version[0];
        else
            $scope.MajorVersion = "00";
        if (Version[1] !== undefined)
            $scope.MinorVersion = Version[1] > 9 ? Version[1] : '0' + Version[1];
        else
            $scope.MinorVersion = "00";

        if (Version[2] !== undefined)
            $scope.BuildVersion = Version[2] > 9 ? Version[2] : '0' + Version[2];
        else
            $scope.BuildVersion = "00";

        if ($scope.ui.data.packageDetail.Options.packageType === 'Module') {
            $scope.ShowAdvanceTab = true;
            $scope.ShowPermissionTab = true;
            $scope.ModuleType = true;
            $scope.ModuleDefinitions = $scope.ui.data.packageDetail.Options.moduleDefinitions;
            $scope.ModuleControls = [];
            GetAssignMoodule();
        }
        else if ($scope.ui.data.packageDetail.Options.packageType === 'Auth_System') {
            $scope.ShowAdvanceTab = true;
            $scope.Auth_SystemType = true;

        }
        else if ($scope.ui.data.packageDetail.Options.packageType === 'Container') {
            $scope.ShowAdvanceTab = true;
            $scope.ContainerType = true;
        }
        else if ($scope.ui.data.packageDetail.Options.packageType === 'JavaScript_Library') {
            $scope.ShowAdvanceTab = true;
            $scope.JavaScript_LibraryType = true;
        }


        $scope.Versions = [];
        GetVersions();
        $scope.Click_Extensions();
        $scope.ShowModuleDefinition = false;
        $scope.ShowModuleControl = false;
    };

    //Render Extension UI

    var GetAssignMoodule = function () {
        $scope.Assigns = [];
        $($scope.ui.data.packageDetail.Options.assignedPortals).each(function (index, Value) {
            var temp = {
                Id: Value.id,
                Name: Value.name,
                Value: true
            };
            $scope.Assigns.push(temp);
        });
        $($scope.ui.data.packageDetail.Options.unassignedPortals).each(function (index, Value) {
            var temp = {
                Id: Value.id,
                Name: Value.name,
                Value: false
            };
            $scope.Assigns.push(temp);
        });
    };

    var GetVersions = function () {
        for (var i = 0; i < 100; i++) {
            if (i < 10)
                $scope.Versions.push('0' + i);
            else
                $scope.Versions.push(i.toString());
        }
    };

    $scope.Click_Extensions = function (val) {
        $scope.ShowBasic = false;
        $scope.ShowAdvance = false;
        $scope.ShowPermission = false;
        $scope.Click_CancelModuleDef();
        $('.Extensions li.advance a ').removeClass('active');
        $('.Extensions li.permission a').removeClass('active');
        $('.Extensions li.basic a').removeClass('active');
        if (val === 'advance') {
            $('.Extensions li.advance a').addClass('active');
            $scope.ShowAdvance = true;
        } else if (val === 'permission') {
            $('.Extensions li.permission a').addClass('active');
            $scope.ShowPermission = true;
        }
        else {
            $('.Extensions li.basic a').addClass('active');
            $scope.ShowBasic = true;
        }
    };

    // add edit delete Module Definitions

    $scope.ShowDefinitionName = function () {
        if ($scope.DefinitionId === -1)
            return false;
        return true;
    };
    $scope.Click_AddModuleDef = function () {
        $scope.ShowModuleDefinition = true;
        $scope.DefinitionId = -1;
        $scope.DefinitionName = "";
        $scope.FriendlyName = "";
        $scope.DefaultCacheTime = 0;
        $scope.ModuleControls = [];
    };
    $scope.Click_EditModuleDef = function (row) {
        $scope.ShowModuleDefinition = true;
        $scope.DefinitionId = row.id;
        $scope.DefinitionName = row.name;
        $scope.FriendlyName = row.friendlyName;
        $scope.DefaultCacheTime = row.cacheTime;
        $scope.ModuleControls = row.controls;
    };
    $scope.Click_DeleteModuleDef = function (row) {
        CommonSvc.SweetAlert.swal({
            title: "[LS:Confirm]",
            text: "[L:DeleteAppDefinition]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    MapSettingsData('deletedefinition', row.id);
                    common.webApi.post('editextension/save', '', $scope.ui.data.packageSettings.Options).success(function (Response) {
                        if (Response.IsSuccess) {
                            $scope.ui.data.packageDetail.Options = Response.Data;
                            $scope.ModuleDefinitions = $scope.ui.data.packageDetail.Options.moduleDefinitions;
                            $scope.Click_CancelModuleControl();
                            $scope.Click_CancelModuleDef();
                        }
                        else if (Response.HasErrors)
                            CommonSvc.SweetAlert.swal(Response.Message);
                    });
                }
            });
    };
    $scope.Click_SaveModuleDef = function () {
        if (IsValidateModuleDef()) {
            MapSettingsData('definition');
            common.webApi.post('editextension/save', '', $scope.ui.data.packageSettings.Options).success(function (Response) {
                if (Response.IsSuccess) {
                    $scope.ui.data.packageDetail.Options = Response.Data;
                    $scope.ModuleDefinitions = $scope.ui.data.packageDetail.Options.moduleDefinitions;
                    $scope.Click_CancelModuleDef();
                }
                else if (Response.HasErrors)
                    CommonSvc.SweetAlert.swal(Response.Message);
            });
        }
    };
    $scope.Click_CancelModuleDef = function () {
        $scope.ShowModuleDefinition = false;
        $scope.Click_CancelModuleControl();
    };
    var IsValidateModuleDef = function () {
        var isval = true;
        if (!$scope.FriendlyName) {
            isval = false;
            CommonSvc.SweetAlert.swal("[L:ErrorMDef_FriendlyName]");
        }
        if (!$scope.DefinitionName) {
            isval = false;
            CommonSvc.SweetAlert.swal("[L:ErrorDefinitionName]");
        }
        return isval;
    };
    // add edit delete Module Controls

    $scope.Click_AddModuleControl = function () {
        $scope.ShowModuleControl = true;
        $scope.ControlId = -1;
        $scope.ControlKey = "";
        $scope.ControlTitle = "";
        $scope.ControlSourceFolder = "";
        $scope.ControlSourceFile = "";
        $scope.ControlType = -1;
        $scope.ControlViewOrder = 0;
        $scope.ControlIcon = "";
        $scope.ControlHelpURL = "";
        $scope.ControlSupportsPopups = true;
        $scope.ControlSupportsPartialRendering = true;
        common.webApi.get('editextension/GetSourceFolders').success(function (Response) {
            if (Response.IsSuccess) {
                $scope.ui.data.ControlSourceFolder.Options = Response.Data;
            }
        });
        $scope.Change_ControlSourceFolder();
    };
    $scope.Click_EditModuleControl = function (row) {
        $scope.ShowModuleControl = true;
        $scope.ControlId = row.id;
        $scope.DefinitionId = row.definitionId;
        $scope.ControlKey = row.key;
        $scope.ControlTitle = row.title;
        $scope.ControlSourceFolder = row.source.split("/").slice(0, -1).join("/").toLowerCase();
        $scope.ControlSourceFile = row.source.toLowerCase();
        $scope.ControlType = row.type;
        $scope.ControlViewOrder = row.order;
        $scope.ControlIcon = row.icon.toLowerCase();
        $scope.ControlHelpURL = row.helpUrl;
        $scope.ControlSupportsPopups = row.supportPopups;
        $scope.ControlSupportsPartialRendering = row.supportPartialRendering;
        common.webApi.get('editextension/GetSourceFolders').success(function (Response) {
            if (Response.IsSuccess)
                $scope.ui.data.ControlSourceFolder.Options = Response.Data;
        });
        $scope.Change_ControlSourceFolder(row);
    };
    $scope.Click_DeleteModuleControl = function (row) {
        CommonSvc.SweetAlert.swal({
            title: "[LS:Confirm]",
            text: "[L:DeleteAppControl]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    MapSettingsData('deletecontrol', row.id);
                    common.webApi.post('editextension/save', '', $scope.ui.data.packageSettings.Options).success(function (Response) {
                        if (Response.IsSuccess) {
                            $scope.ui.data.packageDetail.Options = Response.Data;
                            $scope.ModuleDefinitions = $scope.ui.data.packageDetail.Options.moduleDefinitions;
                            $scope.Click_CancelModuleControl();
                            $scope.Click_CancelModuleDef();
                        }
                        else if (Response.HasErrors)
                            CommonSvc.SweetAlert.swal(Response.Message);
                    });
                }
            });
    };
    $scope.Click_SaveModuleControl = function () {
        if (IsValidateModuleControl()) {
            MapSettingsData('control');
            common.webApi.post('editextension/save', '', $scope.ui.data.packageSettings.Options).success(function (Response) {
                if (Response.IsSuccess) {
                    $scope.ui.data.packageDetail.Options = Response.Data;
                    $scope.ModuleDefinitions = $scope.ui.data.packageDetail.Options.moduleDefinitions;
                    $scope.Click_CancelModuleControl();
                    $scope.Click_CancelModuleDef();
                }
                else if (Response.HasErrors)
                    CommonSvc.SweetAlert.swal(Response.Message);
            });
        }
    };
    $scope.Click_CancelModuleControl = function () {
        $scope.ShowModuleControl = false;
    };
    var IsValidateModuleControl = function () {
        var isval = true;
        if (!$scope.ControlSourceFile) {
            isval = false;
            CommonSvc.SweetAlert.swal("[L:ErrorControlSourceFile]");
        }
        return isval;
    };
    $scope.Change_ControlSourceFolder = function (row) {
        common.webApi.get('editextension/GetSourceFiles', 'root=' + encodeURIComponent($scope.ControlSourceFolder)).success(function (Response) {
            if (Response.IsSuccess)
                $scope.ui.data.ControlSourceFile.Options = Response.Data;
            if (row === undefined)
                $scope.ControlSourceFile = "";
        });
        common.webApi.get('editextension/GetIcons', 'controlpath=' + encodeURIComponent($scope.ControlSourceFolder)).success(function (Response) {
            if (Response.IsSuccess)
                $scope.ui.data.ControlIcon.Options = Response.Data;
            if (row === undefined)
                $scope.ControlIcon = "";
        });
    };
    // save Extensions

    $scope.Click_Save = function () {
        if (IsValidate()) {
            MapSettingsData();
            common.webApi.post('editextension/save', '', $scope.ui.data.packageSettings.Options).success(function (Response) {
                if (Response.IsSuccess)
                    $scope.Click_Cancel();
                else if (Response.HasErrors)
                    CommonSvc.SweetAlert.swal(Response.Message);
            });
        }
    };
    var IsValidate = function () {
        var isval = true;
        if (!$scope.ui.data.packageDetail.Options.friendlyName) {
            isval = false;
            CommonSvc.SweetAlert.swal("Please specify [L:ErrorfriendlyName] > friendlyName");
        }
        return isval;
    };
    var MapSettingsData = function (Type,ID) {

        var Version = parseInt($scope.MajorVersion) + '.' + parseInt($scope.MinorVersion) + '.' + parseInt($scope.BuildVersion);
        $scope.ui.data.packageSettings.Options.settings = {
            Description: $scope.ui.data.packageDetail.Options.description,
            Email: $scope.ui.data.packageDetail.Options.email,
            FriendlyName: $scope.ui.data.packageDetail.Options.friendlyName,
            License: $scope.ui.data.packageDetail.Options.license,
            Name: $scope.ui.data.packageDetail.Options.name,
            Organization: $scope.ui.data.packageDetail.Options.organization,
            Owner: $scope.ui.data.packageDetail.Options.owner,
            ReleaseNotes: $scope.ui.data.packageDetail.Options.releaseNotes,
            Url: $scope.ui.data.packageDetail.Options.url,
            Version: Version
        };
        if (Type === "definition") {
            $scope.ui.data.packageSettings.Options.editorActions = {
                savedefinition: '{"id": ' + $scope.DefinitionId + ',"desktopModuleId": ' + $scope.ui.data.packageDetail.Options.desktopModuleId + ', "name": "' + $scope.DefinitionName + '","friendlyName": "' + $scope.FriendlyName + '", "cacheTime": ' + $scope.DefaultCacheTime + '}'
            };

        }
        else if (Type === "deletedefinition") {
            $scope.ui.data.packageSettings.Options.editorActions = {
                deletedefinition: ID
            };
        }
        else if (Type === "control") {
            $scope.ui.data.packageSettings.Options.editorActions = {
                savemodulecontrol: '{"id": ' + $scope.ControlId + ',"definitionId": ' + $scope.DefinitionId + ',"key": "' + $scope.ControlKey + '","title": "' + $scope.ControlTitle + '","source": "' + $scope.ControlSourceFile + '","type": ' + $scope.ControlType + ',"order": ' + $scope.ControlViewOrder + ',"icon": "' + $scope.ControlIcon + '","helpUrl": "' + $scope.ControlHelpURL + '","supportPopups": ' + $scope.ControlSupportsPopups + ',"supportPartialRendering": ' + $scope.ControlSupportsPartialRendering + '}'
            };
        }
        else if (Type === "deletecontrol") {
            $scope.ui.data.packageSettings.Options.editorActions = {
                deletemodulecontrol: ID
            };
        }
        else {
            if ($scope.ModuleType) {
                var permissionDefinitions = '';
                var rolePermissions = '';
                var userPermissions = '';
                $($scope.PermissionsDefinitions).each(function (index, Def) {
                    if (permissionDefinitions !== '')
                        permissionDefinitions += ',';
                    permissionDefinitions += '{"permissionId":' + Def.PermissionId + ',"permissionName":"' + Def.permissionName + '","permissionKey":null,"permissionCode":null,"fullControl":false,"view":false,"allowAccess":false}';
                });

                $($scope.PermissionsRoles).each(function (index, Role) {
                    var allowaccess = false;
                    var rpermission = '{"roleId":' + Role.RoleId + ',"roleName":"' + Role.RoleName + '",';
                    var permissions = '';
                    $(Role.Permissions).each(function (index, permission) {
                        if (permissions !== '')
                            permissions += ',';
                        permissions += '{"permissionId":' + permission.PermissionId + ',"permissionName":"' + permission.PermissionName + '","permissionKey":null,"permissionCode":null,"fullControl":false,"view":false,"allowAccess":' + permission.AllowAccess + '}';
                        if (!allowaccess && permission.AllowAccess)
                            allowaccess = true;
                    });
                    rpermission += '"permissions":[' + permissions + '],"locked":' + Role.IsDefault + ',"default":' + Role.Locked + '}';
                    if (allowaccess) {
                        if (rolePermissions !== '')
                            rolePermissions += ',';
                        rolePermissions += rpermission;
                    }
                });

                $($scope.PermissionsUsers).each(function (index, User) {
                    var allowaccess = false;
                    var upermission = '{"userId":' + User.UserId + ',"displayName":"' + User.DisplayName + '",';
                    var permissions = '';
                    $(User.Permissions).each(function (index, permission) {
                        if (permissions !== '')
                            permissions += ',';
                        permissions += '{"permissionId":' + permission.PermissionId + ',"permissionName":"' + permission.PermissionName + '","permissionKey":null,"permissionCode":null,"fullControl":false,"view":false,"allowAccess":' + permission.AllowAccess + '}';
                        if (!allowaccess && permission.AllowAccess)
                            allowaccess = true;
                    });
                    upermission += '"permissions":[' + permissions + ']}';
                    if (allowaccess) {
                        if (userPermissions !== '')
                            userPermissions += ',';
                        userPermissions += upermission;
                    }
                });
                
                var assignPortal = '';
                var unassignPortal = '';
                $($scope.Assigns).each(function (index, Value) {
                    if ($('input[id=assign_' + Value.Id + ']').is(':checked')) {
                        assignPortal += '{"id":' + Value.Id + ',"name":"' + Value.Name + '"},';
                    }
                    else {
                        unassignPortal += '{"id":' + Value.Id + ',"name":"' + Value.Name + '"},';
                    }
                });
                $scope.ui.data.packageSettings.Options.editorActions = {
                    permissions: '{"desktopModuleId":' + $scope.ui.data.packageDetail.Options.desktopModuleId + ',"permissionDefinitions":[' + permissionDefinitions + '],"rolePermissions":[' + rolePermissions + '],"userPermissions":[' +userPermissions + ']}',
                    assignPortal: '[' + assignPortal + ']',
                    businessController: $scope.ui.data.packageDetail.Options.businessController,
                    category: $scope.ui.data.packageDetail.Options.category,
                    dependencies: $scope.ui.data.packageDetail.Options.dependencies,
                    folderName: $scope.ui.data.packageDetail.Options.folderName,
                    hostPermissions: $scope.ui.data.packageDetail.Options.hostPermissions,
                    premiumModule: $scope.ui.data.packageDetail.Options.premiumModule,
                    shareable: $scope.ui.data.packageDetail.Options.shareable,
                    unassignPortal: '[' + unassignPortal + ']'
                };
            }
            else if ($scope.Auth_SystemType) {
                $scope.ui.data.packageSettings.Options.editorActions = {
                    appEnabled: $scope.ui.data.packageDetail.Options.appEnabled,
                    appId: $scope.ui.data.packageDetail.Options.appId,
                    appSecret: $scope.ui.data.packageDetail.Options.appSecret,
                    authenticationType: $scope.ui.data.packageDetail.Options.authenticationType,
                    enabled: $scope.ui.data.packageDetail.Options.enabled,
                    loginControlSource: $scope.ui.data.packageDetail.Options.loginControlSource,
                    logoffControlSource: $scope.ui.data.packageDetail.Options.logoffControlSource,
                    settingsControlSource: $scope.ui.data.packageDetail.Options.settingsControlSource
                };
            }
            else if ($scope.ContainerType) {
                $scope.ui.data.packageSettings.Options.editorActions = {
                    themePackageName: $scope.ui.data.packageDetail.Options.themePackageName
                };
            }
            else if ($scope.JavaScript_LibraryType) {
                $scope.ui.data.packageSettings.Options.editorActions = {
                    customCdn: $scope.ui.data.packageDetail.Options.customCdn
                };
            }
            else {
                $scope.ui.data.packageSettings.Options.editorActions = {};
            }
        }
    };

    $scope.Click_Cancel = function () {
        var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
        Parentscope.Click_IsInstall(true);
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };

});