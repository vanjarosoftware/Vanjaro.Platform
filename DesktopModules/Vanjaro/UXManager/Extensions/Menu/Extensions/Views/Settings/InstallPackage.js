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

});
