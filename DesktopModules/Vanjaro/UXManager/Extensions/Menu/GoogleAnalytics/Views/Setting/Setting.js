app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        
    };

    $scope.Click_Update = function () {
        common.webApi.post('Setting/save', '', $scope.ui.data.Settings.Options).then(function (Response) {
            if (Response.data.IsSuccess) {
                $scope.Click_Cancel();
            }
            else if (Response.HasErrors)
                CommonSvc.SweetAlert.swal(Response.data.Message);
        });
    };

    $scope.Click_Delete = function () {
        common.webApi.get('Setting/delete').then(function (Response) {
            $scope.ui.data.Settings.Options = Response.data;
            $scope.ui.data.HasConfig.Options = false;
        });
    };

    $scope.Click_Cancel = function () {
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };
});