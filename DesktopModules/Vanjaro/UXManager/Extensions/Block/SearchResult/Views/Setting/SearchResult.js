app.controller('setting_searchresult', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.Loaded = false;
    $scope.onInit = function () {
        $(".uxmanager-modal .modal-title", parent.document).html('[L:SettingTitle]');
        if ($scope.ui.data.IsAdmin.Value)
            $scope.ui.data.IsAdmin.Value = $scope.ui.data.IsAdmin.Value == 'true' ? true : false;
        $scope.CurrentSearchresult = window.parent.VjEditor.getSelected();
        if ($scope.CurrentSearchresult != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentSearchresult.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {
                $scope.ui.data.LinkTarget.Value = $scope.ui.data.GlobalConfigs.Options['data-block-linktarget'] == "false" ? false : true;
                $scope.ui.data.Template.Value = $scope.ui.data.GlobalConfigs.Options["data-block-template"];
            }
            else {
                $scope.ui.data.LinkTarget.Value = $scope.CurrentSearchresult.attributes.attributes['data-block-linktarget'] == "false" ? false : true;
                $scope.ui.data.Template.Value = $scope.CurrentSearchresult.attributes.attributes["data-block-template"];
            }
        }
        $scope.Loaded = true;
    };

    $scope.ApplyChanges = function (searchresult) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('searchresult/update', '', searchresult.attributes.attributes).then(function () {
                window.parent.RenderBlock(searchresult);
            });
        }
        else
            window.parent.RenderBlock(searchresult);
    };

    $scope.$watch('ui.data.Template.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var searchresult = window.parent.VjEditor.getSelected();
            searchresult.addAttributes({ 'data-block-template': newValue });
            $scope.ApplyChanges(searchresult);
        }
    });

    $scope.$watch('ui.data.Global.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var searchresult = window.parent.VjEditor.getSelected();
            if (newValue)
                searchresult.addAttributes({ 'data-block-global': 'true' });
            else
                searchresult.addAttributes({ 'data-block-global': 'false' });
            $scope.ApplyChanges(searchresult);
        }
    });

    $scope.$watch('ui.data.LinkTarget.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var searchresult = window.parent.VjEditor.getSelected();
            if (newValue)
                searchresult.addAttributes({ 'data-block-linktarget': 'true' });
            else
                searchresult.addAttributes({ 'data-block-linktarget': 'false' });
            $scope.ApplyChanges(searchresult);
        }
    });

});