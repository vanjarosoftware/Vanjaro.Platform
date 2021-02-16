app.controller('setting_searchinput', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);
    $scope.loaded = false;
    $scope.CurrentSearch;

    $scope.onInit = function () {
        $(".uxmanager-modal .modal-title", parent.document).html('[L:SettingTitle]');
        $scope.CurrentSearch = window.parent.VjEditor.getSelected();
        if ($scope.CurrentSearch != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentSearch.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {
                $scope.ui.data.Template.Value = $scope.ui.data.GlobalConfigs.Options["data-block-template"];
            }
            else {
                $scope.ui.data.Template.Value = $scope.CurrentSearch.attributes.attributes["data-block-template"];
            }
        }

        $scope.ui.data.IsAdmin.Value = ($scope.ui.data.IsAdmin.Value != '' && $scope.ui.data.IsAdmin.Value == 'true') ? true : false;
        $scope.loaded = true;
    };

    $scope.ApplyChanges = function (searchinput) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('search/update', '', searchinput.attributes.attributes).success(function () {
                window.parent.RenderBlock(searchinput);
            });
        }
        else
            window.parent.RenderBlock(searchresult);
    };

    $scope.$watch('ui.data.Template.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var searchinput = window.parent.VjEditor.getSelected();
            searchinput.addAttributes({ 'data-block-template': newValue });
            $scope.ApplyChanges(searchinput);
        }
    });

    $scope.$watch('ui.data.Global.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var searchinput = window.parent.VjEditor.getSelected();
            if (newValue)
                searchinput.addAttributes({ 'data-block-global': 'true' });
            else
                searchinput.addAttributes({ 'data-block-global': 'false' });
            $scope.ApplyChanges(searchinput);
        }
    });


});