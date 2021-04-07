app.controller('setting_adduser', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
        //$scope.ui.data.UserTemplate.Options.authorize = true;
        //$scope.ui.data.UserTemplate.Options.notify = false;
        //$scope.ui.data.UserTemplate.Options.randomPassword = true;
    };

    $scope.Click_Save = function (Sender) {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_adduser')) {
            if ($scope.ui.data.UseEmailAsUsername.Options)
                $scope.ui.data.UserTemplate.Options.userName = $scope.ui.data.UserTemplate.Options.email;
            if ($scope.ui.data.UserTemplate.Options.password == $scope.ui.data.UserTemplate.Options.confirmPassword) {
                common.webApi.post('user/createuser', '', $scope.ui.data.UserTemplate.Options).then(function (data) {
                    if (data.data != null && data.data.Data != null && data.data.IsSuccess) {
                        var ParentScope = parent.document.getElementById("iframe").contentWindow.angular;
                        if (ParentScope != undefined && ParentScope.element(".menuextension").scope() != undefined && has(ParentScope.element(".menuextension").scope(), 'ui.data.AllUsers')) {
                            ParentScope.element(".menuextension").scope().ui.data.AllUsers.Options.push(data.data.Data);
                            ParentScope.element(".menuextension").scope().$apply();
                        }

                        if (has(data.data.Data, 'displayName'))
                            window.parent.ShowNotification(data.data.Data.displayName, '[L:UserCreatedSuccess]', 'success');

                        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                        //parent.OpenPopUp(null, 800, 'right', '', $scope.ui.data.RedirectUrl.Value + '#!/setting?uid=' + data.Data.userId);
                    }
                    else {
                        parent.ShowNotification('[LS:Shortcut_Title]', data.data.Message, 'error');
                    }
                });
            }
            else {
                parent.ShowNotification('[L:AddUser]', '[LS:PasswordMismatch]', 'error');
            }
        }
    };

    has = function (obj, key) {
        return key.split(".").every(function (x) {
            if (typeof obj != "object" || obj === null || !x in obj || typeof obj[x] === "undefined")
                return false;
            obj = obj[x];
            return true;
        });
    };
});