app.controller('setting_setting', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    $scope.rid = $routeParams["rid"];
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $scope.ShowCookieConsentTab = true;
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'CookieConsent') {
            $('#CookieConsent a.nav-link').addClass("active");
            $('#DataConsent a.nav-link').removeClass("active");
            $('#ImprovementProgram a.nav-link').removeClass("active");
            $scope.ShowCookieConsentTab = true;
            $scope.ShowDataConsentTab = false;
            $scope.ShowImprovementProgramTab = false;
        }
        else if (type == 'DataConsent') {
            $('#CookieConsent a.nav-link').removeClass("active");
            $('#DataConsent a.nav-link').addClass("active");
            $('#ImprovementProgram a.nav-link').removeClass("active");
            $scope.ShowCookieConsentTab = false;
            $scope.ShowDataConsentTab = true;
            $scope.ShowImprovementProgramTab = false;
        }
        else if (type == 'ImprovementProgram') {
            $('#CookieConsent a.nav-link').removeClass("active");
            $('#DataConsent a.nav-link').removeClass("active");
            $('#ImprovementProgram a.nav-link').addClass("active");
            $scope.ShowCookieConsentTab = false;
            $scope.ShowDataConsentTab = false;
            $scope.ShowImprovementProgramTab = true;
        }
    };

    $scope.Click_ResetTerms = function () {
        window.parent.swal({
            title: "",
            text: "[L:ResetTermsCondition]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
            cancelButtonText: "[L:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('setting/ResetTermsAgreement', '', '').success(function (data) {
                        if (data.IsSuccess) {
                            if (data.Message != null && data.Message == "")
                                window.parent.swal(data.Message);
                        }
                        else {
                            window.parent.document.callbacktype = type;
                            $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                        }
                    });
                }
            });
    };

    $scope.Click_Save = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            $scope.ui.data.UpdatePrivacy.Options.ShowCookieConsent = $scope.ui.data.GetPrivacy.Options.Settings.ShowCookieConsent;
            $scope.ui.data.UpdatePrivacy.Options.CheckUpgrade = $scope.ui.data.GetPrivacy.Options.Settings.CheckUpgrade;
            //$scope.ui.data.UpdatePrivacy.Options.DisplayCopyright = $scope.ui.data.GetPrivacy.Options.Settings.DisplayCopyright;
            $scope.ui.data.UpdatePrivacy.Options.CookieMoreLink = $scope.ui.data.GetPrivacy.Options.Settings.CookieMoreLink;
            $scope.ui.data.UpdatePrivacy.Options.DataConsentActive = $scope.ui.data.GetPrivacy.Options.Settings.DataConsentActive;
            $scope.ui.data.UpdatePrivacy.Options.DataConsentUserDeleteAction = $scope.ui.data.UserDelete.Value;
            $scope.ui.data.UpdatePrivacy.Options.DataConsentConsentRedirect = $scope.ui.data.PageRedirect.Value;
            $scope.ui.data.UpdatePrivacy.Options.DataConsentDelay = $scope.ui.data.GetPrivacy.Options.Settings.DataConsentDelay;
            $scope.ui.data.UpdatePrivacy.Options.DataConsentDelayMeasurement = $scope.ui.data.HardDelete.Value;
            var customSettings = {
                VJImprovementProgram: $scope.ui.data.GetPrivacy.Options.Settings.VJImprovementProgram,
            };
            var settingsData = {
                PrivacySettingsRequest: $scope.ui.data.UpdatePrivacy.Options,
                CustomSettingsRequest: customSettings
            };
            common.webApi.post('setting/UpdatePrivacySettings', '', settingsData).success(function (data) {
                if (data.IsSuccess) {
                    //window.parent.document.callbacktype = type;
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                }
                else {
                    window.parent.swal(data.Message);
                }
            });
        }
    };
});


