app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    var Data = "";
    $scope.onInit = function () {

    };

    $scope.Click_Update = function () {
            if ($scope.ui.data.ApplyTo.Options) {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Host_Tag: $scope.ui.data.Host_Tag.Value
                }
            }
            else {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Site_Tag: $scope.ui.data.Site_Tag.Value
                }
            }
            common.webApi.post('Setting/save', '', Data).success(function (Response) {
                if (Response)
                    $scope.Click_Cancel();
                else {
                    CommonSvc.SweetAlert.swal("[L:Invalid]");
                }
            });
        
    };
    $scope.Click_Delete = function () {
        common.webApi.post('Setting/delete', '', $scope.ui.data.ApplyTo.Options).success(function (Response) {
            if ($scope.ui.data.ApplyTo.Options)
                $scope.ui.data.Host_Tag.Value = Response;
            else
                $scope.ui.data.Site_Tag.Value = Response;
        });
    };
    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});