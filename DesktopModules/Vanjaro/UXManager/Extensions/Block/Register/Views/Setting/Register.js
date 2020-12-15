app.controller('setting_register', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);
    $scope.Loaded = false;

    //Init Scope
    $scope.onInit = function () {
        $(".uxmanager-modal .modal-title", parent.document).html('[L:SettingTitle]');
        $scope.CurrentRegistration = window.parent.VjEditor.getSelected();
        if ($scope.CurrentRegistration != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentRegistration.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {
                $scope.ui.data.ShowLabel.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showlabel'] == "false" ? false : true;
                $scope.ui.data.TermsPrivacy.Value = $scope.ui.data.GlobalConfigs.Options['data-block-termsprivacy'] == "false" ? false : true;
                $scope.ui.data.ButtonAlign.Value = $scope.ui.data.GlobalConfigs.Options['data-block-buttonalign'];
            }
            else {
                $scope.ui.data.ShowLabel.Value = $scope.CurrentRegistration.attributes.attributes['data-block-showlabel'] == "false" ? false : true;
                $scope.ui.data.TermsPrivacy.Value = $scope.CurrentRegistration.attributes.attributes['data-block-termsprivacy'] == "false" ? false : true;
                $scope.ui.data.ButtonAlign.Value = $scope.CurrentRegistration.attributes.attributes['data-block-buttonalign'];
            }
        }
        $scope.Loaded = true;
        $scope.ui.data.IsAdmin.Value = ($scope.ui.data.IsAdmin.Value != '' && $scope.ui.data.IsAdmin.Value == 'true') ? true : false;
    };

    $scope.ApplyChanges = function (registered) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('register/update', '', registered.attributes.attributes).success(function () {
                window.parent.RenderBlock(registered);
            });
        }
        else
            window.parent.RenderBlock(registered);
    };

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

    $scope.$watch('ui.data.ShowLabel.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-showlabel': 'true' });
            else
                registered.addAttributes({ 'data-block-showlabel': 'false' });
            $scope.ApplyChanges(registered);
        }
    });

    $scope.$watch('ui.data.TermsPrivacy.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            if (newValue)
                registered.addAttributes({ 'data-block-termsprivacy': 'true' });
            else
                registered.addAttributes({ 'data-block-termsprivacy': 'false' });
            $scope.ApplyChanges(registered);
        }
    });

    $scope.$watch('ui.data.ButtonAlign.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var registered = window.parent.VjEditor.getSelected();
            registered.addAttributes({ 'data-block-buttonalign': newValue.toString() });
            $scope.ApplyChanges(registered);
        }
    });

});



