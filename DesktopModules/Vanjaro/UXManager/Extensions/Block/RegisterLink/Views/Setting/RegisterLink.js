app.controller('setting_registerlink', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.Loaded = false;

    $scope.onInit = function () {
        $(".uxmanager-modal .modal-title", parent.document).html('[L:SettingTitle]');
        $scope.CurrentRegistrationLink = window.parent.VjEditor.getSelected();
        if ($scope.CurrentRegistrationLink != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentRegistrationLink.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {
                $scope.ui.data.ShowSignInLink.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showsigninlink'] == "false" ? false : true;
                $scope.ui.data.ShowProfileLink.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showprofilelink'] == "false" ? false : true;
                $scope.ui.data.ShowAvatar.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showavatar'] == "false" ? false : true;
                $scope.ui.data.ShowNotification.Value = $scope.ui.data.GlobalConfigs.Options['data-block-shownotification'] == "false" ? false : true;
                $scope.ui.data.Template.Value = $scope.ui.data.GlobalConfigs.Options["data-block-template"];
            }
            else {
                $scope.ui.data.ShowSignInLink.Value = $scope.CurrentRegistrationLink.attributes.attributes['data-block-showsigninlink'] == "false" ? false : true;
                $scope.ui.data.ShowProfileLink.Value = $scope.CurrentRegistrationLink.attributes.attributes['data-block-showprofilelink'] == "false" ? false : true;
                $scope.ui.data.ShowAvatar.Value = $scope.CurrentRegistrationLink.attributes.attributes['data-block-showavatar'] == "false" ? false : true;
                $scope.ui.data.ShowNotification.Value = $scope.CurrentRegistrationLink.attributes.attributes['data-block-shownotification'] == "false" ? false : true;
                $scope.ui.data.Template.Value = $scope.CurrentRegistrationLink.attributes.attributes["data-block-template"];
            }
        }
        $scope.Loaded = true;
    };

    $scope.ApplyChanges = function (registered) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('registerlink/update', '', registered.attributes.attributes).then(function () {
                window.parent.RenderBlock(registered);
            });
        }
        else
            window.parent.RenderBlock(registered);
    };

    $scope.$watch('ui.data.Template.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            registered.addAttributes({ 'data-block-template': newValue });
            $scope.ApplyChanges(registered);
        }
    });

    $scope.$watch('ui.data.ShowSignInLink.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-showsigninlink': 'true' });
            else
                registered.addAttributes({ 'data-block-showsigninlink': 'false' });
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

    $scope.$watch('ui.data.ShowProfileLink.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-showprofilelink': 'true' });
            else
                registered.addAttributes({ 'data-block-showprofilelink': 'false' });
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