app.controller('setting_setting', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
    };

    $scope.Click_TestSMTP = function () {
        var Data = {
            Server: $scope.ui.data.Server.Value,
            Username: $scope.ui.data.Username.Value,
            Password: $scope.ui.data.Password.Value,
            EnableSSL: $scope.ui.data.EnableSSL.Options
        }
        common.webApi.post('Setting/SendTestEmail', '', Data).success(function (data) {
            if (data.IsSuccess) {
                window.parent.ShowNotification('[LS:EmailServiceProvider]', data.Data, 'success');
            }
            else if (data.HasErrors) {
                window.parent.swal(data.Message);
            }
        });
    };

    $scope.Click_Update = function () {
        var Data = {
            Server: $scope.ui.data.Server.Value,
            Username: $scope.ui.data.Username.Value,
            Password: $scope.ui.data.Password.Value,
            EnableSSL: $scope.ui.data.EnableSSL.Options
        }
        common.webApi.post('Setting/update', '', Data).success(function (data) {
            if (data.IsSuccess) {
                $scope.Click_Cancel();
            }
            else if (data.HasErrors) {
                window.parent.swal(data.Message);
            }
        });
    }
    $scope.Click_Cancel = function () {
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});