app.controller('setting_login', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.Loaded = false;
    //Init Scope
    $scope.onInit = function () {
        $(".uxmanager-modal .modal-title", parent.document).html('[L:SettingTitle]');
        $scope.CurrentLogin = window.parent.VjEditor.getSelected();
        if ($scope.CurrentLogin != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentLogin.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {
                $scope.ui.data.ShowResetPassword.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showresetpassword'] == "false" ? false : true;
                $scope.ui.data.ShowRememberPassword.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showrememberpassword'] == "false" ? false : true;
                $scope.ui.data.ShowLabel.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showlabel'] == "false" ? false : true;
                $scope.ui.data.ButtonAlign.Value = $scope.ui.data.GlobalConfigs.Options['data-block-buttonalign'];
                $scope.ui.data.ResetPassword.Value = $scope.ui.data.GlobalConfigs.Options['data-block-resetpassword'] == "false" ? false : true;
                $scope.ui.data.ShowRegister.Value = $scope.ui.data.GlobalConfigs.Options['data-block-showregister'] == "false" ? false : true;
            }
            else {
                $scope.ui.data.ShowResetPassword.Value = $scope.CurrentLogin.attributes.attributes['data-block-showresetpassword'] == "false" ? false : true;
                $scope.ui.data.ShowRememberPassword.Value = $scope.CurrentLogin.attributes.attributes['data-block-showrememberpassword'] == "false" ? false : true;
                $scope.ui.data.ShowLabel.Value = $scope.CurrentLogin.attributes.attributes['data-block-showlabel'] == "false" ? false : true;
                $scope.ui.data.ButtonAlign.Value = $scope.CurrentLogin.attributes.attributes['data-block-buttonalign'];
                $scope.ui.data.ResetPassword.Value = $scope.CurrentLogin.attributes.attributes['data-block-resetpassword'] == "false" ? false : true;
                $scope.ui.data.ShowRegister.Value = $scope.CurrentLogin.attributes.attributes['data-block-showregister'] == "false" ? false : true;
            }
        }
        $scope.Loaded = true;
        $scope.ui.data.IsAdmin.Value = ($scope.ui.data.IsAdmin.Value != '' && $scope.ui.data.IsAdmin.Value == 'true') ? true : false;
    };

    $scope.ApplyChanges = function (login) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('login/update', '', login.attributes.attributes).success(function () {
                window.parent.RenderBlock(login);
            });
        }
        else
            window.parent.RenderBlock(login);
    };

    $scope.$watch('ui.data.ShowResetPassword.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var login = window.parent.VjEditor.getSelected();
            if (newValue)
                login.addAttributes({ 'data-block-showresetpassword': 'true' });
            else
                login.addAttributes({ 'data-block-showresetpassword': 'false' });
            $scope.ApplyChanges(login);
        }
    });

    $scope.$watch('ui.data.ShowRememberPassword.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var login = window.parent.VjEditor.getSelected();
            if (newValue)
                login.addAttributes({ 'data-block-showrememberpassword': 'true' });
            else
                login.addAttributes({ 'data-block-showrememberpassword': 'false' });
            $scope.ApplyChanges(login);
        }
    });

    $scope.$watch('ui.data.ShowLabel.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var login = window.parent.VjEditor.getSelected();
            if (newValue)
                login.addAttributes({ 'data-block-showlabel': 'true' });
            else
                login.addAttributes({ 'data-block-showlabel': 'false' });
            $scope.ApplyChanges(login);
        }
    });

    $scope.$watch('ui.data.ButtonAlign.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var login = window.parent.VjEditor.getSelected();
            login.addAttributes({ 'data-block-buttonalign': newValue.toString() });
            $scope.ApplyChanges(login);
        }
    });

    $scope.$watch('ui.data.ResetPassword.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var login = window.parent.VjEditor.getSelected();
            if (newValue)
                login.addAttributes({ 'data-block-resetpassword': 'true' });
            else
                login.addAttributes({ 'data-block-resetpassword': 'false' });
            $scope.ApplyChanges(login);
        }
    });

    $scope.$watch('ui.data.ShowRegister.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var login = window.parent.VjEditor.getSelected();
            if (newValue)
                login.addAttributes({ 'data-block-showregister': 'true' });
            else
                login.addAttributes({ 'data-block-showregister': 'false' });
            $scope.ApplyChanges(login);
        }
    });      

    $scope.$watch('ui.data.Global.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var login = window.parent.VjEditor.getSelected();
            if (newValue)
                login.addAttributes({ 'data-block-global': 'true' });
            else
                login.addAttributes({ 'data-block-global': 'false' });
            $scope.ApplyChanges(login);
        }
    });


});