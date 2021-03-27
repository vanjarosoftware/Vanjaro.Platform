app.controller('settings_about', function ($scope, $attrs, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $('#signingOut').on("click", function () {          
            $('#signingOut').html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"> </span>Signing Out...');          
        }); 

    };

    $scope.Logout = function () {
        $.get($scope.ui.data.LogoutUrl.Value, function (data, status) {
            if ($scope.ui.data.RedirectAfterLogout.Value == "-1")
                window.parent.location.reload();
            else
                window.parent.location.href = $scope.ui.data.LogoutUrl.Value;
        });
    };

    $scope.EnableMode = function () {
        var messag = "[L:EnableModeText]";
        if (!$scope.ui.data.EnableMode.Options)
            messag = "[L:DisableModeText]";

        window.parent.swal({
            title: "[LS:AreYouSure]",
            text: messag,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('About/EnableMode', 'IsEnabled=' + !$scope.ui.data.EnableMode.Options).then(function (data) {
                        window.parent.location.reload();
                    });
                }
            });
        
    };

    $scope.ClearCache = function () {
        window.parent.swal({
            title: "[LS:AreYouSure]",
            text: "[L:ClearCacheText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('About/ClearCache').then(function (data) {
                        window.parent.location.reload();
                    });
                }
            });
    };

    $scope.IncrementCRMVersion = function () {
        window.parent.swal({
            title: "[LS:AreYouSure]",
            text: "[L:IncrementVersionText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('About/IncrementCRMVersion').then(function (data) {
                        window.parent.location.reload();
                    });
                }
            });
        
    };

    $scope.RestartApp = function () {
        window.parent.swal({
            title: "[LS:AreYouSure]",
            text: "[L:RestartAppText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('About/RestartApplication').then(function (data) {
                        window.parent.location.reload();
                    });
                }
            });
    };
});