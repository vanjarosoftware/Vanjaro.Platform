app.controller('notification_tasks', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $('.Texttab ').addClass('active');
        $('.Messagetab').removeClass('active');
    };
});