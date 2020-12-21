app.controller('settings_installpackage', function ($scope, $attrs, $http, CommonSvc, $sce) {
    var common = CommonSvc.getData($scope);
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
            if (!v.checked){
                val = false;
                CommonSvc.SweetAlert.swal("[L:CheckLicences]");
            }
        });
        if (val) {
            common.webApi.get('InstallPackage/install').success(function (Response) {
                if (Response.Data.length===0) {
                    var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
                    Parentscope.Click_IsInstall(true);
                    $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                }
                else {
                    $scope.ui.data.PackageErrorList.Options = Response.Data;
                    $scope.PackageError();
                }
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
