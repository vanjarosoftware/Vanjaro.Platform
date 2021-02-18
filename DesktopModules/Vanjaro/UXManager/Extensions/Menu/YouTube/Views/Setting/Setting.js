app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {

    };

    $scope.Click_Update = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            if ($scope.ui.data.ApplyTo.Options) {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Host_ApiKey: $scope.ui.data.Host_ApiKey.Value
                }
            }
            else {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Site_ApiKey: $scope.ui.data.Site_ApiKey.Value
                }
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
                $scope.ui.data.Host_ApiKey.Value = Response;
                $scope.ui.data.Host_HasApiKey.Options = false;
            }
            else {
                $scope.ui.data.Site_ApiKey.Value = Response;
                $scope.ui.data.Site_HasApiKey.Options = false;
            }
        });
    };
    $scope.Click_Cancel = function () {
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };
});