app.controller('setting_setting', function ($scope, $attrs, $routeParams, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.ChangeTheme = function (Theme) {

        window.parent.swal({
            title: "[LS:ApplyThemeTitle]",
            text: "[LS:ApplyThemeText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('setting/update', 'theme=' + Theme).then(function (data) {
                        window.parent.location.reload();
                    });
                }
            });
    };

    $scope.Open_ThemeBuilder = function () {

        event.stopImmediatePropagation();

        var url = $scope.ui.data.ThemeBuilderUrl.Value;
        if (url.indexOf("?") == -1)
            url = url + "?v=" + (new Date()).getTime() + '#categories/' + $scope.ui.data.Theme.Options[0].Value;
        else
            url = url + "&v=" + (new Date()).getTime() + '#categories/' + $scope.ui.data.Theme.Options[0].Value;
        window.location.href = url;
    };
});