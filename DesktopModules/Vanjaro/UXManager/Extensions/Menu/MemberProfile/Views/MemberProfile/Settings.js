app.controller('memberprofile_settings', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.ShowUserProfileSettingsTab = true;
    //Init Scope
    $scope.onInit = function () {

    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'UserProfileSettings') {
            $('#UserProfileSettings a.nav-link').addClass("active");
            $scope.ShowUserProfileSettingsTab = true;
        }
    };

    $scope.Click_Save = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'memberprofile_settings')) {
            common.webApi.post('memberprofile/UpdateProfileSettings', '', $scope.ui.data.ProfileSettings.Options).success(function (Response) {
                if (Response.IsSuccess) {
                    $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                }
                else {
                    window.parent.ShowNotification('[LS:UserProfileSettings]', Response.Message, 'error');
                }
            });
        };
    }
});