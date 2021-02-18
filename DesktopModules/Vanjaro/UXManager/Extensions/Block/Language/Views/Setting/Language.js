app.controller('setting_language', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);
    $scope.loaded = false;
    $scope.CurrentLanguage;

    $scope.onInit = function () {
        $(".uxmanager-modal .modal-title", parent.document).html('[L:SettingTitle]');
        $scope.CurrentLanguage = window.parent.VjEditor.getSelected();
        if ($scope.CurrentLanguage != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentLanguage.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {               
                $scope.ui.data.Template.Value = $scope.ui.data.GlobalConfigs.Options["data-block-template"];                
            }
            else {               
                $scope.ui.data.Template.Value = $scope.CurrentLanguage.attributes.attributes["data-block-template"];                
            }
        }
        
        $scope.ui.data.IsAdmin.Value = ($scope.ui.data.IsAdmin.Value != '' && $scope.ui.data.IsAdmin.Value == 'true') ? true : false;
        $scope.loaded = true;
    };

    $scope.ApplyChanges = function (language) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('language/update', '', language.attributes.attributes).success(function () {
                window.parent.RenderBlock(language);
            });
        }
        else
            window.parent.RenderBlock(language);
    };

    $scope.$watch('ui.data.Template.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var language = window.parent.VjEditor.getSelected();
            language.addAttributes({ 'data-block-template': newValue });
            $scope.ApplyChanges(language);
        }
    });

    $scope.$watch('ui.data.Global.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var language = window.parent.VjEditor.getSelected();
            if (newValue)
                language.addAttributes({ 'data-block-global': 'true' });
            else
                language.addAttributes({ 'data-block-global': 'false' });
            $scope.ApplyChanges(language);
        }
    });
});