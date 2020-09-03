app.controller('setting_menu', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);

    $scope.loaded = false;

    $scope.CurrentMenu;

    $scope.onInit = function () {
        $("#defaultModal .modal-title", parent.document).html('[L:SettingTitle]');
        $scope.CurrentMenu = window.parent.VjEditor.getSelected();
        if ($scope.CurrentMenu != undefined) {
            $scope.ui.data.Global.Value = $scope.CurrentMenu.attributes.attributes["data-block-global"] == "false" ? false : true;
            if ($scope.ui.data.Global.Value) {
                $scope.ui.data.IncludeHiddenPageNo.Value = $scope.ui.data.GlobalConfigs.Options["data-block-includehidden"] == "false" ? false : true;
                if ($scope.ui.data.GlobalConfigs.Options["data-block-nodeselector"] != "*") {
                    $scope.ui.data.DisplayAllPages.Value = false;
                    var nodeItems = $scope.ui.data.GlobalConfigs.Options["data-block-nodeselector"].split(',');
                    if (nodeItems[0] == "*")
                        $scope.ui.data.RootPages.Value = "0";
                    else if (nodeItems[0] == "+0")
                        $scope.ui.data.RootPages.Value = "1";
                    else if (nodeItems[0] == "0")
                        $scope.ui.data.RootPages.Value = "2";
                    else {
                        $scope.ui.data.RootPages.Value = "3";
                        $scope.ui.data.Pages.Value = nodeItems[0];
                    }

                    if (nodeItems[1] != undefined && nodeItems[1] != "0") {
                        $scope.ui.data.SkipPages.Value = true;
                        $scope.ui.data.NoOfPagesSkip.Value = parseInt(nodeItems[1]);
                    }

                    if (nodeItems[2] != undefined && nodeItems[2] != "0") {
                        $scope.ui.data.LimitDepth.Value = true;
                        $scope.ui.data.NoOfDepth.Value = parseInt(nodeItems[2]);
                    }
                }
            }
            else {
                $scope.ui.data.IncludeHiddenPageNo.Value = $scope.CurrentMenu.attributes.attributes["data-block-includehidden"] == "false" ? false : true;
                if ($scope.CurrentMenu.attributes.attributes["data-block-nodeselector"] != "*") {
                    $scope.ui.data.DisplayAllPages.Value = false;
                    var nodeItems = $scope.CurrentMenu.attributes.attributes["data-block-nodeselector"].split(',');
                    if (nodeItems[0] == "*")
                        $scope.ui.data.RootPages.Value = "0";
                    else if (nodeItems[0] == "+0")
                        $scope.ui.data.RootPages.Value = "1";
                    else if (nodeItems[0] == "0")
                        $scope.ui.data.RootPages.Value = "2";
                    else {
                        $scope.ui.data.RootPages.Value = "3";
                        $scope.ui.data.Pages.Value = nodeItems[0];
                    }

                    if (nodeItems[1] != undefined && nodeItems[1] != "0") {
                        $scope.ui.data.SkipPages.Value = true;
                        $scope.ui.data.NoOfPagesSkip.Value = parseInt(nodeItems[1]);
                    }

                    if (nodeItems[2] != undefined && nodeItems[2] != "0") {
                        $scope.ui.data.LimitDepth.Value = true;
                        $scope.ui.data.NoOfDepth.Value = parseInt(nodeItems[2]);
                    }
                }
            }
        }
        $scope.loaded = true;
        $scope.ui.data.IsAdmin.Value = ($scope.ui.data.IsAdmin.Value != '' && $scope.ui.data.IsAdmin.Value == 'true') ? true : false;
    };

    $scope.$watch('ui.data.Global.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var menu = window.parent.VjEditor.getSelected();
            if (newValue)
                menu.addAttributes({ 'data-block-global': 'true' });
            else
                menu.addAttributes({ 'data-block-global': 'false' });
            $scope.ApplyChanges(menu);
        }
    });

    $scope.$watch('ui.data.DisplayAllPages.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined)
            $scope.SetNodeSelector();
    });

    $scope.$watch('ui.data.RootPages.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined)
            $scope.SetNodeSelector();
    });

    $scope.$watch('ui.data.Pages.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined)
            $scope.SetNodeSelector();
    });

    $scope.$watch('ui.data.SkipPages.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined)
            $scope.SetNodeSelector();
    });

    $scope.$watch('ui.data.NoOfPagesSkip.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined)
            $scope.SetNodeSelector();
    });

    $scope.$watch('ui.data.LimitDepth.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined)
            $scope.SetNodeSelector();
    });

    $scope.$watch('ui.data.NoOfDepth.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined)
            $scope.SetNodeSelector();
    });

    $scope.$watch('ui.data.IncludeHiddenPageNo.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            var menu = window.parent.VjEditor.getSelected();
            if (newValue)
                menu.addAttributes({ 'data-block-includehidden': 'true' });
            else
                menu.addAttributes({ 'data-block-includehidden': 'false' });
            $scope.ApplyChanges(menu);
        }
    });

    $scope.SetNodeSelector = function () {
        var menu = window.parent.VjEditor.getSelected();
        if ($scope.ui.data.DisplayAllPages.Value)
            menu.addAttributes({ 'data-block-nodeselector': '*' });
        else {
            var markup = "";
            if ($scope.ui.data.RootPages.Value == "0")
                markup += "*";
            else if ($scope.ui.data.RootPages.Value == "1")
                markup += "+0";
            else if ($scope.ui.data.RootPages.Value == "2")
                markup += "0";
            else
                markup += $scope.ui.data.Pages.Value;

            if ($scope.ui.data.SkipPages.Value && $scope.ui.data.NoOfPagesSkip.Value != "")
                markup += "," + $scope.ui.data.NoOfPagesSkip.Value;
            if ($scope.ui.data.LimitDepth.Value && $scope.ui.data.NoOfDepth.Value != "") {
                if (markup.indexOf(',') > 0)
                    markup += "," + $scope.ui.data.NoOfDepth.Value;
                else
                    markup += ",0," + $scope.ui.data.NoOfDepth.Value;
            }
            menu.addAttributes({ 'data-block-nodeselector': markup });
        }
        $scope.ApplyChanges(menu);
    };

    $scope.ApplyChanges = function (menu) {
        if ($scope.ui.data.Global.Value) {
            common.webApi.post('menu/update', '', menu.attributes.attributes).success(function () {
                window.parent.RenderBlock(menu);
            });
        }
        else
            window.parent.RenderBlock(menu);
    };
});