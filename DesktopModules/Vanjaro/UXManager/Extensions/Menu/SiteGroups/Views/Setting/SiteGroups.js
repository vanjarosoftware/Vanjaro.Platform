app.controller('setting_sitegroups', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {

    };
    $scope.Click_Back = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.SitesUrl.Value;
    };
    $scope.Click_New = function () {
        parent.OpenPopUp(null, 800, 'right', '[L:SiteGroups]', $scope.ui.data.SiteGroupUrl.Value + "#!/add");
    };

    $scope.Click_Edit = function (row) {
        parent.OpenPopUp(null, 800, 'right', row.MasterPortal.PortalName, $scope.ui.data.SiteGroupUrl.Value + "#!/add/" + row.PortalGroupId);
    };

    $scope.Click_Delete = function (row) {
        window.parent.swal({
            title: "[LS:Confirm]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('sitegroups/delete', 'PortalGroupId=' + row.PortalGroupId).success(function (Response) {
                        window.parent.ShowNotification('[L:SiteGroups]', row.PortalGroupName + ' [L:Success_DeleteMessage]', 'success');
                        $scope.ui.data.SiteGroups.Options = Response;
                    });
                }
            });
    };

    $scope.RefreshGrid = function () {
        common.webApi.get('sitegroups/getall').success(function (Response) {
            $scope.ui.data.SiteGroups.Options = Response;
        });
    };
    
});