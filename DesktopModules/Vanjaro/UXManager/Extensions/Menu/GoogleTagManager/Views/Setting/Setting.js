app.controller('setting_setting', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    var Data = "";
    $scope.onInit = function () {

    };

    $scope.Click_Update = function () {
            if ($scope.ui.data.ApplyTo.Options) {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Host_Head: $scope.ui.data.Host_Head.Value,
                    Host_Body: $scope.ui.data.Host_Body.Value
                }
            }
            else {
                Data = {
                    ApplyTo: $scope.ui.data.ApplyTo.Options,
                    Site_Head: $scope.ui.data.Site_Head.Value,
                    Site_Body: $scope.ui.data.Site_Body.Value,
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

    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});