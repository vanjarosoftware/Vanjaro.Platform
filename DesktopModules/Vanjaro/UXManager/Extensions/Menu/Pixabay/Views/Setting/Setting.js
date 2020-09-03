app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {

    };

    $scope.Click_Update = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            common.webApi.post('Setting/save', '', $scope.ui.data.ApiKey.Value).success(function (Response) {
                if (Response)
                    $scope.Click_Cancel();
                else {
                    CommonSvc.SweetAlert.swal("[L:Invalid]");
                }
            });
        }
    };
    $scope.Click_Delete = function () {
        common.webApi.get('Setting/delete').success(function (Response) {
            $scope.ui.data.ApiKey.Value = Response;
            $scope.ui.data.HasApiKey.Options = false;
        });
    };
    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});