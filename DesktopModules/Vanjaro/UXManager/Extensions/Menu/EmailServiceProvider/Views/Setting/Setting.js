app.controller('setting_setting', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
    };
    var Data = "";
    $scope.Click_TestSMTP = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            if ($scope.ui.data.SMTPmode.Options) {
                Data = {
                    Server: $scope.ui.data.Host_Server.Value,
                    Port: $scope.ui.data.Host_Port.Value,
                    Username: $scope.ui.data.Host_Username.Value,
                    Password: $scope.ui.data.Host_Password.Value,
                    EnableSSL: $scope.ui.data.Host_EnableSSL.Options
                }
            }
            else {
                Data = {
                    Server: $scope.ui.data.Portal_Server.Value,
                    Port: $scope.ui.data.Portal_Port.Value,
                    Username: $scope.ui.data.Portal_Username.Value,
                    Password: $scope.ui.data.Portal_Password.Value,
                    EnableSSL: $scope.ui.data.Portal_EnableSSL.Options
                }
            }
            common.webApi.post('Setting/SendTestEmail', '', Data).then(function (data) {
                if (data.data.IsSuccess) {
                    window.parent.ShowNotification('[LS:EmailServiceProvider]', data.data.Data, 'success');
                }
                else if (data.data.HasErrors) {
                    window.parent.swal(data.data.Message);
                }
            });
        }
    };


    $scope.Click_Update = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            if ($scope.ui.data.SMTPmode.Options) {

                Data = {
                    SMTPmode: $scope.ui.data.SMTPmode.Options,
                    Host_Server: $scope.ui.data.Host_Server.Value,
                    Host_Port: $scope.ui.data.Host_Port.Value,
                    Host_Username: $scope.ui.data.Host_Username.Value,
                    Host_Password: $scope.ui.data.Host_Password.Value,
                    Host_Email: $scope.ui.data.Host_Email.Value,
                    Host_EnableSSL: $scope.ui.data.Host_EnableSSL.Options,
                    Host_PurgeLogsAfter: $scope.ui.data.Host_PurgeLogsAfter.Value
                }
            }
            else {
                Data = {
                    SMTPmode: $scope.ui.data.SMTPmode.Options,
                    Portal_Server: $scope.ui.data.Portal_Server.Value,
                    Portal_Port: $scope.ui.data.Portal_Port.Value,
                    Portal_Username: $scope.ui.data.Portal_Username.Value,
                    Portal_Password: $scope.ui.data.Portal_Password.Value,
                    Portal_EnableSSL: $scope.ui.data.Portal_EnableSSL.Options
                }
            }
            common.webApi.post('Setting/update', '', Data).then(function (data) {
                if (data.data.IsSuccess) {
                    $scope.Click_Cancel();
                }
                else if (data.data.HasErrors) {
                    window.parent.swal(data.data.Message);
                }
            });
        }
    };

    $scope.Click_Cancel = function () {
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };
});