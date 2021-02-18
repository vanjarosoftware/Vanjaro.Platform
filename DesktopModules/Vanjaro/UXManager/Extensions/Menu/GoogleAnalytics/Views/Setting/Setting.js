app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        
    };

    $scope.Click_Update = function () {
        common.webApi.post('Setting/save', '', $scope.ui.data.Settings.Options).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.Click_Cancel();
            }
            else if (Response.HasErrors)
                CommonSvc.SweetAlert.swal(Response.Message);
        });
    };

    $scope.Click_Delete = function () {
        common.webApi.get('Setting/delete').success(function (Response) {
            $scope.ui.data.Settings.Options = Response;
            $scope.ui.data.HasConfig.Options = false;
        });
    };

    $scope.Click_Cancel = function () {
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };
});