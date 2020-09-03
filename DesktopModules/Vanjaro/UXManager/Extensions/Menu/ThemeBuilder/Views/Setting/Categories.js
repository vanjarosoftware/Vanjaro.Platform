app.controller('setting_categories', function ($scope, $attrs, $routeParams, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.OpenSettings = function (option) {
        event.preventDefault();
        var url = window.location.href.split('#')[0];
        window.location.href = url + "#/settings/" + $scope.ui.data.Theme.Value + "/" + option.Name + "/" + option.Guid;
    };

    $scope.BackTo_Theme = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.ThemeUrl.Value;
    };

});