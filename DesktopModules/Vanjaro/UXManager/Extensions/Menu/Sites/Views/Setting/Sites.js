app.controller('setting_sites', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {

    };
    $scope.Click_New = function () {
        parent.OpenPopUp(null, 800, 'right', 'Add New Site', "#/add");
    };
    $scope.Click_SiteGroup = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.SiteGroupUrl.Value;
    };

    $scope.RefreshGrid = function () {
        if ($scope.PortalsTableState != undefined) {
            $scope.PortalsTableState.pagination.start = 0;
            $scope.PortalsTableState.pagination.numberOfPages = 20;
            $scope.Pipe_PortalsPagging($scope.PortalsTableState);
        }
    };
    $scope.Click_Delete = function (row) {
        window.parent.swal({
            title: "[LS:Confirm]",
            text: "[L:DeleteConfirm]" + row.PortalName,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('sites/delete', 'PortalID=' + row.PortalID).success(function (Response) {
                        if (Response.IsSuccess) {
                            $scope.RefreshGrid();
                            window.parent.ShowNotification('[LS:Sites]', '[L:Success_DeleteMessage]', 'success');
                        }
                    });
                }
            });
    };
    $scope.Pipe_PortalsPagging = function (tableState) {
        $scope.PortalsTableState = tableState;
        common.webApi.get('sites/getportals', 'pageSize=' + tableState.pagination.number + '&pageIndex=' + (tableState.pagination.start / tableState.pagination.number)).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.Portals = Response.Data.Items;
                $scope.PortalsTableState.pagination.numberOfPages = Response.Data.NumberOfPages;
            }
        });
    };
    $scope.Click_Export = function (row) {
        window.parent.swal({
            title: "[LS:Confirm]",
            text: "[L:ExportMessage]" + row.PortalName,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (IsConfirm) {
                if (IsConfirm) {
                    $http({
                        method: 'GET',
                        url: window.location.origin + jQuery.ServicesFramework(-1).getServiceRoot('Sites') + "Sites/Export?PortalID=" + row.PortalID + "&Name=" + row.PortalName,
                        responseType: 'arraybuffer',
                        headers: {
                            'ModuleId': -1,
                            'TabId': parseInt($.ServicesFramework(-1).getTabId()),
                            'RequestVerificationToken': $.ServicesFramework(-1).getAntiForgeryValue()
                        }
                    }).success(function (data, status, headers) {
                        headers = headers();
                        var filename = headers['x-filename'];
                        var contentType = headers['content-type'];
                        var linkElement = document.createElement('a');
                        try {
                            var blob = new Blob([data], { type: contentType });
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
            });
    };
});