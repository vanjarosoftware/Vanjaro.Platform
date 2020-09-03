app.controller('setting_addgroup', function ($scope, $routeParams, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.gid = $routeParams["gid"];
    $scope.onInit = function () {
        $scope.isDisabled = false;
    };

    $scope.Click_SaveRoleGroup = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_addgroup')) {
            $scope.isDisabled = true;
            common.webApi.post('rolegroup/SaveRoleGroup', '', $scope.ui.data.Working_RoleGroupDto.Options).success(function (data) {
                if (data.IsSuccess) {
                    var ParentScope = parent.document.getElementById("iframe").contentWindow.angular;
                    if (ParentScope != undefined && ParentScope.element(".menuextension").scope() != undefined && ParentScope.element(".menuextension").scope().ui.data.RoleGroup != undefined) {
                        ParentScope.element(".menuextension").scope().ui.data.RoleGroup.Options = data.Data.AllRoleGroup;
                        ParentScope.element(".menuextension").scope().$apply();
                    }
                    $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                    window.parent.ShowNotification($scope.ui.data.Working_RoleGroupDto.Options.name, '[L:RoleGroupUpdatedSuccess]', 'success');
                }
                else {
                    window.parent.ShowNotification('[LS:Roles]', data.Message, 'error');
                }
                $scope.isDisabled = false;
            });
        }
    };

});