app.controller('settings_deleteextension', function ($scope, $attrs, $http, CommonSvc, $sce) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
        $scope.DeleteFiles = false;
        $scope.SiteURL = $scope.ui.data.packageDetail.Options.url;
        $scope.DescriptionTitle = $scope.ui.data.packageDetail.Options.description ? String($scope.ui.data.packageDetail.Options.description).replace(/<[^>]+>/gm, '') : '';
        $scope.Email = $scope.ui.data.packageDetail.Options.email;

    };
    $scope.Click_Email = function () {
        window.location.href = "mailto:" + $scope.Email;
    };
    $scope.Click_Delete = function () {
        $scope.ui.data.deletePackage.Options.deleteFiles = $scope.DeleteFiles;
        window.parent.swal({
            title: "[LS:Confirm]",
            text: "[L:DeleteText]" + $scope.ui.data.packageDetail.Options.friendlyName,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('DeleteExtension/deletepackage', '', $scope.ui.data.deletePackage.Options).success(function (Response) {
                        if (Response.IsSuccess) {
                            window.parent.LoadApps();
                            $scope.Click_Cancel();
                            window.parent.ShowNotification($scope.ui.data.packageDetail.Options.friendlyName, '[L:DeletedSuccessfully]', 'success');
                        }
                        else if (Response.HasErrors)
                            window.parent.swal(Response.Message);
                    });
                }
            });
    };

    $scope.Click_Cancel = function () {
        if (typeof parent.document.getElementById("iframe").contentWindow.angular != 'undefined') {
            var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
            Parentscope.Click_IsInstall(true);
        }
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };
});