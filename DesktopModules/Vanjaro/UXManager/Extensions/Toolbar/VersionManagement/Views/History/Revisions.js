app.controller('history_revisions', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);
    $scope.BlockGuid = '';

    $scope.onInit = function () {
        if ($scope.ui.data.BlockGuid != undefined)
            $scope.BlockGuid = $scope.ui.data.BlockGuid.Value;
        $scope.LoadVersion();
    };

    $scope.LoadVersion = function () {
        common.webApi.get('Revisions/GetDate', 'Locale=' + window.parent.VJLocal + '&BlockGuid=' + $scope.BlockGuid).success(function (data) {
            if (data != null) {
                $scope.ui.data = data;
                $scope.ui.data.MaxVersion.Value = parseInt($scope.ui.data.MaxVersion.Value);
                $scope.ui.data.PublicVersion.Value = parseInt($scope.ui.data.PublicVersion.Value);
            }
        });
    };

    $scope.SelectedVersion;
    $scope.Click_SelectVersion = function (event, Version, StateName) {
        if ($(event.target).parents('.badge').length > 0)
            return;
        event.preventDefault();
        if (StateName != 'Draft') {
            $scope.SelectedVersion = Version;
            window.parent.IsVJCBRendered = true;
            if (!$(window.parent.document.body).find('.optimizing-overlay').length)
                $(window.parent.document.body).find('.vj-wrapper').prepend('<div class="optimizing-overlay"><h1><img class="centerloader" src="' + window.parent.VjDefaultPath + 'loading.svg" />Please wait</h1></div>');
            if ($scope.BlockGuid != '') {
                common.webApi.get('Revisions/GetBlockVersion', 'Version=' + $scope.SelectedVersion + '&BlockGuid=' + $scope.BlockGuid).success(function (response) {
                    if (response != undefined) {
                        parent.setCookie("vj_UXLoad", window.location.href);
                        if (window.parent.VjEditor.getSelected() != undefined)
                            parent.setCookie("vj_UX_BlockRevision_Id", window.parent.VjEditor.getSelected().ccid);
                        window.parent.location.reload();
                    }
                });
            }
            else {
                common.webApi.get('Revisions/GetVersion', 'Version=' + $scope.SelectedVersion + '&Locale=' + window.parent.VJLocal).success(function (response) {
                    if (response != undefined) {
                        parent.setCookie("vj_UXLoad", window.location.href);
                        window.parent.location.reload();
                    }
                });
            }
        }
        else {
            return;
        }
    };

    $scope.Click_ViewLogs = function (Data) {
        if (Data.IsLogsExist)
            window.parent.OpenPopUp(null, 600, 'right', 'Logs', window.parent.CurrentExtTabUrl + '&guid=33d8efed-0f1d-471e-80a4-6a7f10e87a42#moderator?version=' + Data.Version + '&entity=Page&entityid=' + Data.TabID);
    };

    $scope.BuildLogo = function (components, id, existinghtml, blockdom) {
        $.each(components, function (k, v) {
            if (v.attributes != undefined && v.attributes.id != undefined && v.attributes.id == id) {
                var style = $(v.content).find('img').attr('style');
                var contentdom = $(blockdom.innerHTML);
                $(contentdom).find('img').attr('style', style);
                $.each(window.parent.window.VjEditor.getComponents().models, function (key, model) {
                    if (model.attributes.components.length == 0 && model.attributes.type == "blockwrapper" && model.attributes.attributes != undefined && model.attributes.attributes.id != undefined && model.attributes.attributes.id == id) {
                        model.attributes.content = contentdom[0].outerHTML;
                        $(existinghtml).html(contentdom[0].outerHTML);
                    }
                    else {
                        $.each(model.findType('blockwrapper'), function (i, m) {
                            if (m.attributes.attributes != undefined && m.attributes.attributes.id != undefined && m.attributes.attributes.id == id) {
                                m.attributes.content = contentdom[0].outerHTML;
                                $(existinghtml).html(contentdom[0].outerHTML);
                            }
                        });
                    }
                });
            }
            else if (v.components != undefined) {
                $scope.BuildLogo(v.components, id, existinghtml, blockdom);
            }
        });
    };

    $scope.Click_RollBack = function () {
        if ($scope.SelectedVersion != null && $scope.ui.data.MaxVersion.Value !== $scope.SelectedVersion) {
            window.parent.swal({
                title: "[L:RollBackTitle]",
                text: "[L:RollBackMessageText]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
                cancelButtonText: "[L:No]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        common.webApi.post('Revisions/rollback', 'Version=' + $scope.SelectedVersion + '&Locale=' + window.parent.VJLocal).success(function (data) {
                            if (data != null) {
                                $scope.ui.data = data;
                            }
                        });
                    }
                });
        }
    };

    $scope.Click_DeleteVersion = function () {

        if ($scope.SelectedVersion != null && $scope.ui.data.MaxVersion.Value !== $scope.SelectedVersion) {
            window.parent.swal({
                title: "[L:DeleteTitle]",
                text: "[L:DeleteMessageText]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
                cancelButtonText: "[L:No]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        common.webApi.post('Revisions/delete', 'Version=' + $scope.SelectedVersion + '&Locale=' + window.parent.VJLocal).success(function (data) {
                            if (data != null) {
                                $scope.ui.data = data;
                            }
                        });
                    }
                });
        }

    };
});