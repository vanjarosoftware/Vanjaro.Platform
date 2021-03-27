app.controller('settings_usageextension', function ($scope, $attrs, $http, CommonSvc, $sce) {
    var common = CommonSvc.getData($scope);
    $scope.Pages = [];
    $scope.onInit = function () {
        $scope.ui.data.Portals.Value = parseInt($scope.ui.data.Portals.Value);
        ParseMarkup();
    }
    var ParseMarkup = function () {
        $scope.Pages = [];
        $.each($scope.ui.data.Pages.Options, function (index) {
            $scope.Pages.push($sce.trustAsHtml($scope.ui.data.Pages.Options[index]));
        });
    };
    $scope.Change_Portals = function () {
        common.webApi.get('UsageExtension/GetPages', 'PortalID=' + $scope.ui.data.Portals.Value + '&PackageID=' + $scope.ui.data.PackageID.Options).then(function (response) {
            if (response.data.IsSuccess) {
                $scope.ui.data.Pages.Options = response.data.Data;
                ParseMarkup();
            }
        });
    }; 

});