app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    var Data = "";
    $scope.onInit = function () {

    };

    $scope.Click_Update = function () {
        if ($scope.ui.data.ApplyTo.Options) {
            Data = {
                ApplyTo: $scope.ui.data.ApplyTo.Options,
                Host_SiteKey: $scope.ui.data.Host_SiteKey.Value,
                Host_SecretKey: $scope.ui.data.Host_SecretKey.Value,
                Host_Enabled: $scope.ui.data.Host_Enabled.Options
            };
        }
        else {
            Data = {
                ApplyTo: $scope.ui.data.ApplyTo.Options,
                Site_SiteKey: $scope.ui.data.Site_SiteKey.Value,
                Site_SecretKey: $scope.ui.data.Site_SecretKey.Value,
                Site_Enabled: $scope.ui.data.Site_Enabled.Options
            };
        }
        common.webApi.post('Setting/save', '', Data).success(function (Response) {
            $scope.Click_Cancel();
        });

    };
    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };
});