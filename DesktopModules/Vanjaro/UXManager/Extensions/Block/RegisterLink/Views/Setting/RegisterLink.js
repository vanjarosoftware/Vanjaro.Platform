app.controller('setting_registerlink', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.Loaded = false;

    $scope.onInit = function () {
        $(".uxmanager-modal .modal-title", parent.document).html('[L:SettingTitle]');
        $scope.CurrentRegistrationLink = window.parent.VjEditor.getSelected();
        if ($scope.CurrentRegistrationLink != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentRegistrationLink.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {
                $scope.ui.data.ShowRegisterLink.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showregisterlink'] == "false" ? false : true;
                $scope.ui.data.ShowAvatar.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showavatar'] == "false" ? false : true;
                $scope.ui.data.ShowNotification.Value = $scope.ui.data.GlobalConfigs.Options['data-block-shownotification'] == "false" ? false : true;
            }
            else {
                $scope.ui.data.ShowRegisterLink.Value = $scope.CurrentRegistrationLink.attributes.attributes['data-block-showregisterlink'] == "false" ? false : true;
                $scope.ui.data.ShowAvatar.Value = $scope.CurrentRegistrationLink.attributes.attributes['data-block-showavatar'] == "false" ? false : true;
                $scope.ui.data.ShowNotification.Value = $scope.CurrentRegistrationLink.attributes.attributes['data-block-shownotification'] == "false" ? false : true;
            }
        }
        $scope.Loaded = true;
    };

    $scope.ApplyChanges = function (registered) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('registerlink/update', '', registered.attributes.attributes).success(function () {
                window.parent.RenderBlock(registered);
            });
        }
        else
            window.parent.RenderBlock(registered);
    };

    $scope.$watch('ui.data.ShowRegisterLink.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-showregisterlink': 'true' });
            else
                registered.addAttributes({ 'data-block-showregisterlink': 'false' });
            $scope.ApplyChanges(registered);
        }
    });

    $scope.$watch('ui.data.Global.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-global': 'true' });
            else
                registered.addAttributes({ 'data-block-global': 'false' });
            $scope.ApplyChanges(registered);
        }
    });

    $scope.$watch('ui.data.ShowAvatar.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-showavatar': 'true' });
            else
                registered.addAttributes({ 'data-block-showavatar': 'false' });
            $scope.ApplyChanges(registered);
        }
    });

    $scope.$watch('ui.data.ShowNotification.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-shownotification': 'true' });
            else
                registered.addAttributes({ 'data-block-shownotification': 'false' });
            $scope.ApplyChanges(registered);
        }
    });
});