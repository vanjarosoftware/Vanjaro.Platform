app.controller('setting_add', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);

    $scope.SiteTemplate;

    $scope.onInit = function () {
        $(window.parent.document.body).find('.modal-dialog:last').find('.defaultdesign-hidden').remove();
        $scope.SiteTemplate = $scope.GetSbTemplate("vj_sbtemplate");
        if ($scope.SiteTemplate != undefined && $scope.SiteTemplate.length > 0) {
            $scope.SiteTemplate = JSON.parse($scope.SiteTemplate);
            $scope.ui.data.CreatePortalRequest.Options.SiteName = $scope.SiteTemplate.Template;
        }
        document.cookie = "vj_sbtemplate=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/";
    };

    $scope.GetSbTemplate = function (cname) {
        var name = cname + "=";
        var decodedCookie = decodeURIComponent(document.cookie);
        var ca = decodedCookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length);
            }
        }
        return null;
    };

    $scope.Click_Update = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_add') && IsValidate()) {
            var formData = {
                PortalRequest: $scope.ui.data.CreatePortalRequest.Options,
                SiteTemplate: $scope.SiteTemplate
            };
            common.webApi.post('add/create', '', formData).then(function (Response) {
                if (Response.data.IsSuccess) {
                    $scope.Click_Cancel();
                }
                else if (Response.data.Errors["SiteDomainError"] != 'undefined') {
                    CommonSvc.SweetAlert.swal(Response.data.Errors["SiteDomainError"].Message);
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