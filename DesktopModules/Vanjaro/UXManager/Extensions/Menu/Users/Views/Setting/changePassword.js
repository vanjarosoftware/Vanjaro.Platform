app.controller('setting_changepassword', function ($scope, $routeParams, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.uid = $routeParams["uid"];
    $scope.uname = $routeParams["uname"];
    $scope.onInit = function () {
        $scope.ConfirmPassword = null;
        $scope.PasswordChangeError = null;
    };

    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };

    $scope.Click_Change = function (change, ConfirmPassword) {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_changepassword')) {
            $scope.ui.data.ChangePasswordTemplate.Options.userId = $scope.uid;
            if ($scope.ui.data.ChangePasswordTemplate.Options.password == ConfirmPassword) {
                common.webApi.post('user/changepassword', '', $scope.ui.data.ChangePasswordTemplate.Options).then(function (data) {
                    if (data != null && data.data.IsSuccess && !data.data.HasErrors) {
                        $scope.Click_Cancel(change);
                        window.parent.ShowNotification($scope.uname, '[L:Success_ChangePasswordMessage]', 'success');
                    }
                    else {
                        window.parent.ShowNotification($scope.uname, data.data.Message, 'error');
                    }
                });
            }
            else {
                window.parent.ShowNotification($scope.uname, '[LS:PasswordMismatch]', 'error');
            }
            ConfirmPassword = null;
        }
    };
});