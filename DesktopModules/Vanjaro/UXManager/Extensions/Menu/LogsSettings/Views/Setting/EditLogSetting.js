app.controller('setting_editlogsetting', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);

    $scope.LoggingIsActiveDisabled = true;
    $scope.EmailNotificationDisabled = true;

    $scope.onInit = function () {
        if ($scope.ui.data !== undefined) {
            if ($scope.ui.data.LoggingIsActive.Value === true)
                $scope.LoggingIsActiveDisabled = false;
            if ($scope.ui.data.EmailNotificationIsActive.Value === true)
                $scope.EmailNotificationDisabled = false;
            $scope.ui.data.Email.Value = $scope.ui.data.MailToAddress.Value;
        }
    };

    $scope.Click_Save = function (sender) {
        if (mnValidationService.DoValidationAndSubmit(sender)) {
            var LogType = {
                ID: $scope.ui.data.ID.Value,
                LoggingIsActive: $scope.ui.data.LoggingIsActive.Value,
                LogTypeKey: $scope.ui.data.GetLogTypes.Value,
                KeepMostRecent: $scope.ui.data.GetKeepMostRecentOptions.Value,
                EmailNotificationIsActive: $scope.ui.data.EmailNotificationIsActive.Value,
                NotificationThreshold: $scope.ui.data.NotificationThreshold.Value,
                NotificationThresholdTime: $scope.ui.data.NotificationTimes.Value,
                NotificationThresholdTimeType: $scope.ui.data.NotificationTimeType.Value,
                MailToAddress: $scope.ui.data.Email.Value,
                LogTypePortalID: $scope.ui.data.LogTypePortalID.Value,
                MailFromAddress: $scope.ui.data.MailFromAddress.Value
            };
            if ($scope.ui.data.ID.Value === "0") {
                common.webApi.post('LogSetting/AddLogSetting', '', LogType).then(function (data) {
                    if (data.data.Status !== 'Success')
                        window.parent.parent.ShowNotification('[LS:Error]', data.data.Status, 'error');
                    else
                        $scope.Click_Cancel();
                })
            }
            else {
                common.webApi.post('LogSetting/UpdateLogSetting', '', LogType).then(function (data) {
                    if (data.data.Status !== 'Success')
                        window.parent.parent.ShowNotification('[LS:Error]', data.data.Status, 'error');
                    else
                        $scope.Click_Cancel();
                });
            }
        }
    };

    $scope.Click_Cancel = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.LogSettingUrl.Value;
    };

    $scope.ChangeLoggingIsActive = function () {
        if ($scope.ui.data.LoggingIsActive.Value === true)
            $scope.LoggingIsActiveDisabled = false;
        else
            $scope.LoggingIsActiveDisabled = true;
    };

    $scope.ChangeEmailNotificationIsActive = function () {
        if ($scope.ui.data.EmailNotificationIsActive.Value === true)
            $scope.EmailNotificationDisabled = false;
        else
            $scope.EmailNotificationDisabled = true;
    };
});