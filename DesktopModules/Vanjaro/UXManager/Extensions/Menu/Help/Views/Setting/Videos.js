app.controller('setting_videos', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.HeaderText = "[L:Videos]";

    $scope.onInit = function () {     
        $('#videosFrame').attr('src', $scope.ui.data.AuthenticatedURL.Value  + '?v=' + new Date().getTime());
    };

    $scope.Click_Back = function () {
        window.location.href = '#!/help';
    };       

    window.addEventListener('message', event => {
        if (typeof event.data != 'undefined' && event.data.action == "Help.Videos" && event.origin.includes($scope.ui.data.OriginURL.Value)) {
            parent.parent.OpenPopUp(event, event.data.width, event.data.position, event.data.title, event.data.url);
        }
    });
});


