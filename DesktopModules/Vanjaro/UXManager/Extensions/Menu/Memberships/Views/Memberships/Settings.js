app.controller('memberships_settings', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.ShowMembershipsTab = true;
    //Init Scope
    $scope.onInit = function () {
       
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'Memberships') {
            $('#Memberships a.nav-link').addClass("active");
            $scope.ShowMembershipsTab = true;
        }
    };

    $scope.Click_Save = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'memberships_settings')) {
            var settingData = {
                UpdateRegistrationSettingsRequest: $scope.ui.data.UserRegistration.Options.Settings,
                UpdateBasicLoginSettingsRequest: $scope.ui.data.UpdateBasicLoginSettingsRequest.Options,
            };
            common.webApi.post('memberships/UpdateSettings', '', settingData).then(function (Response) {
                if (Response.data.IsSuccess) {
                    if (Response.data.IsRedirect) {
                        window.parent.location.href = Response.data.RedirectURL;
                    }
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                }
                else {
                    window.parent.ShowNotification('[LS:SecuritySettings]', Response.data.Message, 'error');
                }
            });
        };
    }
});