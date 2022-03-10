app.controller('settings_installpackage', function ($scope, $attrs, $http, CommonSvc, $sce) {
    var common = CommonSvc.getData($scope);
    $scope.DisableFinish = false;
    $scope.showInstall = true;
    $scope.onInit = function () {
        $scope.PackageError();
    };

    $scope.PackageError = function () {
        if ($scope.ui.data.PackageErrorList.Options.length === 0)
            $scope.showInstall = true;
        else
            $scope.showInstall = false;
    };

    $scope.Click_Install = function () {
        var val = true;
        $.each($("input[type=checkbox]"), function (i,v) {
            if (!v.checked && val){
                val = false;
                CommonSvc.SweetAlert.swal("", "[L:CheckLicences]", "warning");
            }
        });
        if (val) {
            $scope.DisableFinish = true;
            common.webApi.get('InstallPackage/install').then(function (Response) {
                if (Response.data.Data.length === 0) {
                    window.parent.ShowNotification('[L:Products]','[L:InstalledSuccessfully]', 'success');
                    var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
                    Parentscope.Click_IsInstall(true);
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                }
                else {
                    $scope.ui.data.PackageErrorList.Options = Response.data.Data;
                    $scope.PackageError();
                }
                $scope.DisableFinish = false;
            });
        }
    };

    $scope.Click_Email = function (email) {
        window.location.href = "mailto:" + email;
    };

    $scope.Click_ShowLicense = function (license) {
        window.parent.swal({
            customClass: 'sweet-customwidth',
            width: '700px',
            title: '[L:TermsCenter]',
            text: "<div class='sweetheight'>" + license + "</div>",
            html: true,
        });
    };

    $scope.Description = function (description) {
        return $sce.trustAsHtml(description);
    };    

});
