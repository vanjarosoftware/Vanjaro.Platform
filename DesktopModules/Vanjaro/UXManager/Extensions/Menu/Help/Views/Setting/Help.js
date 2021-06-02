app.controller('setting_help', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.HeaderText = "[L:Help]";

    $scope.onInit = function () {
    };

    $scope.Click_Back = function () {
        parent.Click_Back();
    };

    $scope.Click_Videos = function () {
        window.location.href = '#!/videos';
    };
});


