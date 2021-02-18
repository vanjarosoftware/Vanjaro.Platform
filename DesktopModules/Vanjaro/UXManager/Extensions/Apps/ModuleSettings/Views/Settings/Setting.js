app.controller('settings_setting', function ($scope, $attrs, $http, CommonSvc) {
    var common = CommonSvc.getData($scope);
    $scope.ShowSchedulingTab = false;
    $scope.ShowAdvanceTab = false;
    $scope.ShowPermissionTab = true;
    $scope.chkNewTabs = false;
    $scope.EnableScheduling = false;
    $scope.ShowAppSettings = false;
    $scope.onInit = function () {
        var startdate = null;
        var enddate = null;
        if ($scope.ui.data.StartDate.Options !== null && $scope.ui.data.StartDate.Options !== "0001-01-01T00:00:00") {
            startdate = new Date($scope.ui.data.StartDate.Options);
            $scope.ui.data.StartDate.Options = ((startdate.getMonth() + 1) < 10 ? '0' + (startdate.getMonth() + 1) : (startdate.getMonth() + 1)) + "/" + (startdate.getDate() < 10 ? '0' + startdate.getDate() : startdate.getDate()) + "/" + startdate.getFullYear();
        }
        else {
            $scope.ui.data.StartDate.Options = '';
        }
        if ($scope.ui.data.EndDate.Options !== null && $scope.ui.data.EndDate.Options !== "0001-01-01T00:00:00") {
            enddate = new Date($scope.ui.data.EndDate.Options);
            $scope.ui.data.EndDate.Options = ((enddate.getMonth() + 1) < 10 ? '0' + (enddate.getMonth() + 1) : (enddate.getMonth() + 1)) + "/" + (enddate.getDate() < 10 ? '0' + enddate.getDate() : enddate.getDate()) + "/" + enddate.getFullYear();
        }
        else {
            $scope.ui.data.EndDate.Options = '';
        }
        $scope.EnableScheduling = ($scope.ui.data.EndDate.Options !== '' || $scope.ui.data.StartDate.Options !== '');
        if (!$scope.EnableScheduling) {
            $scope.ui.data.EndDate.Options = '';
            $scope.ui.data.StartDate.Options = '';
        }
        if (parent.window.$('[data-settingsmid=' + $scope.ui.data.ModuleID.Options.toString() + ']').length > 0)
            $scope.ShowAppSettings = true;
    };

    $scope.ChangeTab = function (type) {
        if (type === 'scheduling') {
            $('#scheduling').addClass('active');
            $('#appsetting').removeClass('active');
            $('#advance').removeClass('active');
            $('#apps').removeClass('active');
            $scope.ShowSchedulingTab = true;
            $scope.ShowAdvanceTab = false;
            $scope.ShowPermissionTab = false;
        }
        else if (type === 'advance') {
            $('#advance').addClass('active');
            $('#scheduling').removeClass('active');
            $('#appsetting').removeClass('active');
            $('#apps').removeClass('active');
            $scope.ShowSchedulingTab = false;
            $scope.ShowAdvanceTab = true;
            $scope.ShowPermissionTab = false;
        }
        else if (type === 'permissions') {
            $scope.ShowPermissionTab = true;
            $scope.ShowSchedulingTab = false;
            $scope.ShowAdvanceTab = false;
            $('#appsetting').addClass('active');
            $('#scheduling').removeClass('active');
            $('#advance').removeClass('active');
            $('#apps').removeClass('active');
        }
        else if (type === 'apps') {
            window.parent.OpenPopUp(null, '100%', '', 'App Settings', $scope.ui.data.AppSettingUrl.Value, '', '', false, true);
            setTimeout(function () { window.location.reload(); }, 500);
        }
    };

    $scope.Click_Save = function () {
        var PermissionsUsers = [];
        var PermissionsRoles = [];
        var valid = true;
        if ($scope.ui.data.StartDate.Options !== null && $scope.ui.data.EndDate.Options !== null && $scope.ui.data.StartDate.Options !== "" && $scope.ui.data.EndDate.Options !== "") {
            valid = new Date($scope.ui.data.StartDate.Options) <= new Date($scope.ui.data.EndDate.Options);
        }
        if (valid) {
            var data = {
                PermissionsRoles: $scope.PermissionsRoles,
                PermissionsUsers: $scope.PermissionsUsers,
                PermissionsInherit: $scope.PermissionsInherit,
                StartDate: $scope.EnableScheduling === true ? $scope.ui.data.StartDate.Options : '',
                EndDate: $scope.EnableScheduling === true ? $scope.ui.data.EndDate.Options : '',
                chkAllTabs: $scope.ui.data.chkAllTabs.Options,
                chkNewTabs: $scope.ui.data.chkAllTabs.Options === false ? false : $scope.chkNewTabs
            };
            common.webApi.post('setting/save', 'moduleid=' + $scope.ui.data.ModuleID.Options, data).success(function (Response) {
                if (Response.IsSuccess) {
                    $scope.chkNewTabs = false;
                    $scope.Click_Cancel('update');
                }
                else {
                    window.parent.ShowNotification('', Response.Message, 'error');
                }
            });
        }
        else {
            swal('[L:InvalidDateText]');
        }
    };

    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };

});