app.controller('setting_settings', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
    };

    $scope.GetAllConnector = function () {
        common.webApi.get('Settings/GetAllConnector').success(function (Response) {
            $scope.ui.data.Connectors.Options = Response.Data;
        });
    };

    $scope.Click_Delete = function (row) {
        window.parent.swal({
            title: "",
            text: "[L:DeleteConfirm]" + row.DisplayName,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('Settings/DeleteConnection', 'id=' + row.Id).success(function (Response) {
                        if (Response.IsSuccess)
                            $scope.ui.data.Connectors.Options = Response.Data;
                        else if (Response.HasErrors)
                            CommonSvc.SweetAlert.swal(Response.Message);
                    });
                }
            });
    };

    $scope.Click_New = function () {
        parent.OpenPopUp(null, 700, 'right', 'Azure', "#/add");
    };

    $scope.Click_Edit = function (row) {
        parent.OpenPopUp(null, 700, 'right', 'Azure', "#/add/" + row.Id);
    };
});