app.controller('setting_add', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
    };
    $scope.Click_GeAll = function () {
        $scope.IsNativeName = !$scope.IsNativeName;
        common.webApi.get('add/getall', 'IsNativeName=' + $scope.IsNativeName).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.ui.data.Languages.Options = Response.Data;
            }
        });
    };
    $scope.Click_Update = function () {
        common.webApi.get('add/update', 'code=' + $scope.ui.data.Languages.Value).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.Click_Cancel();
                window.parent.location.reload();
                //$scope.RenderMarkup();
            }
        });
    };

    $scope.RenderMarkup = function () {
        $.each(window.parent.getAllComponents().filter(function (component) {
            return (component.attributes.name === "Language" || component.attributes["custom-name"] !== undefined && component.attributes["custom-name"] === "Language");
        }), function (index, value) {
            window.parent.RenderBlock(value);
        });
    }
    $scope.Click_Cancel = function () {
        //var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
        //Parentscope.GetLanguages();
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };    
});