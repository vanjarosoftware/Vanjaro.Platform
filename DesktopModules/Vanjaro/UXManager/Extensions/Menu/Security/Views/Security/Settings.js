app.controller('security_settings', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.InputMaxUploadSize = 28;
    //Init Scope
    $scope.onInit = function () {
        $scope.ui.data.Picture_DefaultFolder.Value = parseInt($scope.ui.data.Picture_DefaultFolder.Value);
        $scope.ui.data.Video_DefaultFolder.Value = parseInt($scope.ui.data.Video_DefaultFolder.Value);
        $scope.ui.data.Picture_MaxUploadSize.Value = parseInt($scope.ui.data.Picture_MaxUploadSize.Value);
        $scope.ui.data.Video_MaxUploadSize.Value = parseInt($scope.ui.data.Video_MaxUploadSize.Value);


        if ($scope.ui.data.IsSuperUser.Value == "True")
            $scope.ui.data.IsSuperUser.Value = true;
        else
            $scope.ui.data.IsSuperUser.Value = false;

        if ($scope.ui.data.IsSuperUser.Value) {
            $scope.ui.data.AutoAccountUnlockDuration.Value = parseInt($scope.ui.data.AutoAccountUnlockDuration.Value);
            $scope.ui.data.AsyncTimeout.Value = parseInt($scope.ui.data.AsyncTimeout.Value);
            $scope.ui.data.MaxUploadSize.Value = parseInt($scope.ui.data.MaxUploadSize.Value);
            $scope.InputMaxUploadSize = parseInt($scope.ui.data.MaxUploadSize.Value);
        }
        if (!$scope.ui.data.IsSuperUser.Value) {
            $scope.Click_ShowTab('SecurityLogin');
        }
        if ($scope.ui.data.UserRegistration.Options.Settings.UserRegistration != null)
            $scope.ui.data.UserRegistration.Options.Settings.UserRegistration = $scope.ui.data.UserRegistration.Options.Settings.UserRegistration.toString();
       
        $scope.ShowGeneralTab = true;
        $scope.Show_Tab = false;
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'General') {
            $('#General a.nav-link').addClass("active");
            $('#SecurityLogin a.nav-link').removeClass("active");
            $('#Registeration a.nav-link').removeClass("active");
            $('#Media a.nav-link').removeClass("active");
            $('#SSL a.nav-link').removeClass("active");
            $scope.ShowGeneralTab = true;
            $scope.ShowLoginTab = false;
            $scope.Registeration = false;
            $scope.ShowMediaTab = false;
            $scope.ShowSSLModule = false;
        }
        else if (type == 'SecurityLogin') {
            $('#General a.nav-link').removeClass("active");
            $('#SecurityLogin a.nav-link').addClass("active");
            $('#Registeration a.nav-link').removeClass("active");
            $('#Media a.nav-link').removeClass("active");
            $('#SSL a.nav-link').removeClass("active");
            $scope.ShowGeneralTab = false;
            $scope.ShowLoginTab = true;
            $scope.Registeration = false;
            $scope.ShowMediaTab = false;
            $scope.ShowSSLModule = false;
        }
        else if (type == 'Registeration') {
            $('#General a.nav-link').removeClass("active");
            $('#SecurityLogin a.nav-link').removeClass("active");
            $('#Registeration a.nav-link').addClass("active");
            $('#Media a.nav-link').removeClass("active");
            $('#SSL a.nav-link').removeClass("active");
            $scope.ShowGeneralTab = false;
            $scope.ShowLoginTab = false;
            $scope.Registeration = true;
            $scope.ShowMediaTab = false;
            $scope.ShowSSLModule = false;
        }
        else if (type == 'Media') {
            $('#General a.nav-link').removeClass("active");
            $('#SecurityLogin a.nav-link').removeClass("active");
            $('#Registeration a.nav-link').removeClass("active");
            $('#Media a.nav-link').addClass("active");
            $('#SSL a.nav-link').removeClass("active");
            $('#collapseFour').addClass('show');
            $('#collapseFive').removeClass('show');
            $scope.ShowGeneralTab = false;
            $scope.ShowLoginTab = false;
            $scope.Registeration = false;
            $scope.ShowMediaTab = true;
            $scope.ShowSSLModule = false;
        }
        else if (type == 'SSL') {
            $('#General a.nav-link').removeClass("active");
            $('#SecurityLogin a.nav-link').removeClass("active");
            $('#Registeration a.nav-link').removeClass("active");
            $('#Media a.nav-link').removeClass("active");
            $('#SSL a.nav-link').addClass("active");
            $scope.ShowGeneralTab = false;
            $scope.ShowLoginTab = false;
            $scope.Registeration = false;
            $scope.ShowMediaTab = false;
            $scope.ShowSSLModule = true;
        }
    };

    $scope.Click_Save = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'security_settings')) {
            if ($scope.ui.data.MaxUploadSize.Value == undefined)
                $scope.ui.data.MaxUploadSize.Value = $scope.InputMaxUploadSize;

            if ($scope.ui.data.MaxUploadSize.Value != undefined && $scope.ui.data.MaxUploadSize.Value > $scope.InputMaxUploadSize) {
                window.parent.swal('[L:maxUploadSizeValidation]' + $scope.InputMaxUploadSize);
            }
            else {
                var settingData = {
                    UpdateSslSettingsRequest: $scope.ui.data.UpdateSslSettingsRequest.Options,
                    UpdateRegistrationSettingsRequest: $scope.ui.data.UserRegistration.Options.Settings,
                    UpdateBasicLoginSettingsRequest: $scope.ui.data.UpdateBasicLoginSettingsRequest.Options,
                    Picture_DefaultFolder: $scope.ui.data.Picture_DefaultFolder.Value,
                    Picture_MaxUploadSize: $scope.ui.data.Picture_MaxUploadSize.Value,
                    Picture_AllowableFileExtensions: $scope.ui.data.Picture_AllowableFileExtensions.Value,
                    Video_DefaultFolder: $scope.ui.data.Video_DefaultFolder.Value,
                    Video_MaxUploadSize: $scope.ui.data.Video_MaxUploadSize.Value,
                    Video_AllowableFileExtensions: $scope.ui.data.Video_AllowableFileExtensions.Value,
                    AutoAccountUnlockDuration: $scope.ui.data.IsSuperUser.Value ? $scope.ui.data.AutoAccountUnlockDuration.Value : null,
                    AsyncTimeout: $scope.ui.data.IsSuperUser.Value ? $scope.ui.data.AsyncTimeout.Value : null,
                    MaxUploadSize: $scope.ui.data.IsSuperUser.Value ? $scope.ui.data.MaxUploadSize.Value : null,
                    FileExtensions: $scope.ui.data.IsSuperUser.Value ? $scope.ui.data.FileExtensions.Value : null,
                    DefaultEndUserExtensionWhitelist: $scope.ui.data.IsSuperUser.Value ? $scope.ui.data.DefaultEndUserExtensionWhitelist.Value : null
                };
                $scope.ui.data.UpdateSslSettingsRequest.Options.SSLEnforced = $scope.ui.data.UpdateSslSettingsRequest.Options.SSLEnabled;
                common.webApi.post('security/UpdateSettings', '', settingData).then(function (Response) {
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
            }
        };
    }
});