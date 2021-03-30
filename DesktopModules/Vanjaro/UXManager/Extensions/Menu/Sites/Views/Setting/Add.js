app.controller('setting_add', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
    };

    $scope.Click_Update = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_add') && IsValidate()) {
            common.webApi.post('add/create', '', $scope.ui.data.CreatePortalRequest.Options).then(function (Response) {
                if (Response.data.IsSuccess) {
                    $scope.Click_Cancel();
                }
            });
        }
    };
    $scope.Click_Cancel = function (type) {
        var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
        Parentscope.RefreshGrid();
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };
    var IsValidate = function () {
        var isval = true;
        if (!$scope.ui.data.CreatePortalRequest.Options.UseCurrentUserAsAdmin) {
            if ($scope.ui.data.CreatePortalRequest.Options.Password !== $scope.ui.data.CreatePortalRequest.Options.PasswordConfirm) {
                CommonSvc.SweetAlert.swal("[L:ConfirmPasswordError]");
                isval = false;
            }
        }
        return isval;
    };
});