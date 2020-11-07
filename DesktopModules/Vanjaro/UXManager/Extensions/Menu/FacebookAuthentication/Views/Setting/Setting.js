app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    var Data = "";
    $scope.onInit = function () {

    };

    $scope.Click_Update = function () {
        Data = {
            AppID: $scope.ui.data.AppID.Value,
            AppSecret: $scope.ui.data.AppSecret.Value,
            Enabled: $scope.ui.data.Enabled.Options
        };
        common.webApi.post('Setting/save', '', Data).success(function (Response) {
                $scope.Click_Cancel();
        });
    };

    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});