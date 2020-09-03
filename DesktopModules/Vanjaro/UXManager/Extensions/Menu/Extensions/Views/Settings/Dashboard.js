app.controller('settings_dashboard', function ($scope, $attrs, $http, CommonSvc) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
        $scope.Extensions = $scope.ui.data.Extensions.Options
        $scope.Search_Extensions = $scope.Extensions;
        $scope.avlinstall = false;
    };
    $scope.Click_CreateModule = function () {
        location.href = "#/createmodule";
    };
    $scope.Click_IsInstall = function (IsInstall) {
        if (IsInstall) {
            $scope.avlinstall = false;
        } else {
            $scope.avlinstall = true;
        }
        common.webApi.get('dashboard/Extensions', 'isinstall=' + IsInstall).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.Extensions = Response.Data;
                $scope.Search_Extensions = $scope.Extensions;
            }
            else {
                $scope.Extensions = [];
                $scope.Search_Extensions = $scope.Extensions;
            }
        });
    };
    $scope.Click_CreateExtension = function () {
        location.href = "#/createextension";
    };
    $scope.Click_InstallExtension = function () {
        parent.OpenPopUp(null, 600, 'center', 'Install Extension', "#/install", 700);
    };
    $scope.Click_Edit = function (row) {
        parent.OpenPopUp(null, 750, 'right', 'Manage Extension', "#/edit/pid/" + row.PackageId);
    };
    $scope.Click_Delete = function (row) {
        parent.OpenPopUp(null, 600, 'center', 'Delete Extension', "#/delete/pid/" + row.PackageId, 300);
    };
    $scope.ShowInUse = function (val) {
        if (!val) {
            return false;
        }
        return true;
    };
    $scope.Click_IsInUse = function (row) {
        parent.OpenPopUp(null, 600, 'right', 'In Use', "#/usage/pid/" + row.PackageId);
    };
    $scope.Click_Install = function (row) {
        if (row.FileName) {
            parent.OpenPopUp(null, 600, 'center', 'Install Extension', "#/install/type/" + row.Type + "/name/" + row.FileName, 700);
        }
        else if (row.Type === 'CoreLanguagePack') {
            common.webApi.get('dashboard/ParseLanguagePackage', 'cultureCode=' + row.Description).success(function (Response) {
                if (Response.IsSuccess && Response.Data.success) {
                    common.webApi.get('dashboard/GetAvailablePackages').success(function (Response) {
                        if (Response.IsSuccess) {
                            parent.OpenPopUp(null, 600, 'center', 'Install Extension', "#/install/type/" + row.Type + "/name/installlanguage.resources", 700);
                        }
                    });
                }
            });
        }
    };
    $scope.showInstall = function (row) {
        if (row.FileName)
            return true;
        else
            return false;
    };
    $scope.Click_Download = function (row) {
        if (row.FileName) {
            $http({
                method: 'GET',
                url: window.location.origin + jQuery.ServicesFramework($scope.$parent.moduleid).getServiceRoot('Extensions') + "dashboard/DownloadPackage?FileName=" + row.FileName + "&Type=" + row.Type,
                responseType: 'arraybuffer',
                headers: {
                    'ModuleId': parseInt($scope.$parent.moduleid),
                    'TabId': parseInt($.ServicesFramework(parseInt($scope.$parent.moduleid)).getTabId()),
                    'RequestVerificationToken': $.ServicesFramework(parseInt($scope.$parent.moduleid)).getAntiForgeryValue()
                },
            }).success(function (data, status, headers) {
                headers = headers();
                var filename = headers['x-filename'];
                var contentType = headers['content-type'];
                var linkElement = document.createElement('a');
                try {
                    var blob = new Blob([data], {
                        type: contentType
                    });
                    var url = window.URL.createObjectURL(blob);
                    linkElement.setAttribute('href', url);
                    linkElement.setAttribute("download", filename);
                    var clickEvent = new MouseEvent("click", {
                        "view": window,
                        "bubbles": true,
                        "cancelable": false
                    });
                    linkElement.dispatchEvent(clickEvent);
                } catch (ex) {
                    alert(ex);
                }
            }).error(function (data) {
                alert(data);
            });
        }
        else if (row.Type === 'CoreLanguagePack') {
            $http({
                method: 'GET',
                url: window.location.origin + jQuery.ServicesFramework($scope.$parent.moduleid).getServiceRoot('Extensions') + "dashboard/DownloadLanguagePackage?cultureCode=" + row.Description,
                responseType: 'arraybuffer',
                headers: {
                    'ModuleId': parseInt($scope.$parent.moduleid),
                    'TabId': parseInt($.ServicesFramework(parseInt($scope.$parent.moduleid)).getTabId()),
                    'RequestVerificationToken': $.ServicesFramework(parseInt($scope.$parent.moduleid)).getAntiForgeryValue()
                },
            }).success(function (data, status, headers) {
                headers = headers();
                var filename = headers['x-filename'];
                var contentType = headers['content-type'];
                var linkElement = document.createElement('a');
                try {
                    var blob = new Blob([data], {
                        type: contentType
                    });
                    var url = window.URL.createObjectURL(blob);
                    linkElement.setAttribute('href', url);
                    linkElement.setAttribute("download", filename);
                    var clickEvent = new MouseEvent("click", {
                        "view": window,
                        "bubbles": true,
                        "cancelable": false
                    });
                    linkElement.dispatchEvent(clickEvent);
                } catch (ex) {
                    alert(ex);
                }
            }).error(function (data) {
                alert(data);
            });
        }
    };

    $scope.Click_InstallPackage = function () {
        parent.OpenPopUp(null, 1200, 'center', 'Install Extension', "~UXManager/Library/Resources/extension.html", '800');
    };
});