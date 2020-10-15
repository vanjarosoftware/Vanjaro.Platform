app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    var Data = "";
    $scope.onInit = function () {

    };

    $scope.Click_Update = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            if ($scope.ui.data.ApplyTo.Options) {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Host_SiteKey: $scope.ui.data.Host_SiteKey.Value,
                    Host_SecretKey: $scope.ui.data.Host_SecretKey.Value
                };
            }
            else {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Site_SiteKey: $scope.ui.data.Site_SiteKey.Value,
                    Site_SecretKey: $scope.ui.data.Site_SecretKey.Value
                };
            }
            common.webApi.post('Setting/save', '', Data).success(function (Response) {
                if (Response)
                    $scope.Click_Cancel();
                else {
                    CommonSvc.SweetAlert.swal("[L:Invalid]");
                }
            });
        }
    };
    $scope.Click_Delete = function () {
        common.webApi.post('Setting/delete', '', $scope.ui.data.ApplyTo.Options).success(function (Response) {
            if ($scope.ui.data.ApplyTo.Options) {
                $scope.ui.data.Host_SiteKey.Value = Response;
                $scope.ui.data.Host_SecretKey.Value = Response;
                $scope.ui.data.Site_HasSiteKey.Options = false;
            }
            else {
                $scope.ui.data.Site_SiteKey.Value = Response;
                $scope.ui.data.Site_SecretKey.Value = Response;
                $scope.ui.data.Site_HasSiteKey.Options = false;
            }
        });
    };
    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});