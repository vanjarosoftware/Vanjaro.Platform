app.controller('setting_logSetting', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);
    $scope.Delete = function (row) {
        swal({
            title: "[L:AreYouSure]",
            text: "[L:DeleteLog]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('LogSetting/DeleteLogSetting', 'LogTypeConfigId=' + row.ID).then(function (data) {
                        if (data.data != undefined && data.data.Status == 'Success')
                            $scope.RefreshGrid();
                        else
                            window.parent.ShowNotification('[LS:Error]', data.data.Status, 'error');
                    })
                }
            });
    };

    $scope.ChangeStatus = function (row, type) {
        if (type == 'Log')
            row.LoggingIsActive ^= true;
        else
            row.EmailNotificationIsActive ^= true;
        if (row != undefined) {
            var LogType = {
                ID: row.ID,
                LoggingIsActive: row.LoggingIsActive,
                LogTypeKey: row.LogTypeKey,
                LogTypePortalID: row.LogTypePortalID,
                KeepMostRecent: row.KeepMostRecent,
                EmailNotificationIsActive: row.EmailNotificationIsActive,
                MailFromAddress: row.MailFromAddress,
                NotificationThreshold: row.NotificationThreshold,
                NotificationThresholdTime: row.NotificationThresholdTime,
                NotificationThresholdTimeType: row.NotificationThresholdTimeType,
                MailToAddress: row.MailToAddress,
            };
            common.webApi.post('LogSetting/UpdateLogSetting', '', LogType).then(function (data) {
                if (data.data != undefined && data.data.Status == 'Success')
                    $scope.RefreshGrid();
                else
                    window.parent.ShowNotification('[LS:Error]', data.data.Status, 'error');
            })
        }
    };

    $scope.OpenPopUp = function (row) {
        window.location.hash = '#!/editlogsetting/' + row.ID;
    };

    $scope.RefreshGrid = function () {
        if ($scope.LogSettingTableState != undefined) {
            $scope.Pipe_LogSettingPagging($scope.LogSettingTableState);
        };
    };

    $scope.Click_CancelLogSettings = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.LogsUrl.Value;
    };

    $scope.Pipe_LogSettingPagging = function (tableState) {
        $scope.LogSettingTableState = tableState;
        var str = '';
        if (tableState != undefined && tableState.search != undefined && tableState.search.predicateObject != undefined && tableState.search.predicateObject.$ != undefined)
            str = tableState.search.predicateObject.$;
        common.webApi.get('LogSetting/GetLogSettings', 'pageSize=' + tableState.pagination.number + '&pageIndex=' + (tableState.pagination.start / tableState.pagination.number) + '&search=' + str).then(function (data) {
            if (data.data != undefined && data.data.Status == 'Success') {
                $scope.LogSetting = data.data.Types;
                $scope.LogSettingTableState.pagination.numberOfPages = data.data.NumberOfPages;
            }
            else
                window.parent.ShowNotification('[LS:Error]', data.data.Status, 'error');
        })
    };
});