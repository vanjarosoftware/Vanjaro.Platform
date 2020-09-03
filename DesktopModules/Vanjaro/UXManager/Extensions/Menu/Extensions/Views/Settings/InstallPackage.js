app.controller('settings_installpackage', function ($scope, $attrs, $http, CommonSvc, $sce) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {

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

    $scope.Click_Download = function () {
        var List = {
            Uri: ["https://www.mandeeps.com/Portals/0/Mandeeps.png","https://www.mandeeps.com/Downloads/Latest/Modules/Install/Live%20Articles_v3.3.4_Extract%20Me.zip"]
        };

        common.webApi.get('InstallPackage/download', 'Data=' + JSON.stringify(List)).success(function (Response) {

            if (Response === Response.Success) {
                $scope.Click_InstallPackage();
            }
            else {
                CommonSvc.SweetAlert.swal(Response.Error);
            }
        });
    }

    $scope.Click_InstallPackage = function () {
        parent.OpenPopUp(null, 600, 'center', 'Install', "#/installpackage", 600);
    };   

});
