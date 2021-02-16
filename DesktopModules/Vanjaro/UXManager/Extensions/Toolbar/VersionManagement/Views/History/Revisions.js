app.controller('history_revisions', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);
    $scope.BlockGuid;
    $scope.onInit = function () {
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
    }


    $scope.SelectedVersion;
    $scope.Click_SelectVersion = function (event, Version, StateName) {
        if ($(event.target).parents('.badge').length > 0)
            return;
        event.preventDefault();
        if (StateName != 'Draft') {
            $scope.SelectedVersion = Version;
            var html = '<img class="revisionloader revisionloaderimg" src="' + window.parent.VjDefaultPath + 'loading.gif">';
            $(window.parent.document.body).append(html);
            if ($scope.BlockGuid != '') {
                common.webApi.get('Revisions/GetBlockVersion', 'Version=' + $scope.SelectedVersion + '&BlockGuid=' + $scope.BlockGuid).success(function (response) {
                    if (response != undefined) {
                        $(window.parent.document.body).find('.revisionloaderimg').remove();
                        window.parent.VjEditor.getSelected().components([]);
                        window.parent.VjEditor.getSelected().append('<style>' + response.Css + '</style');
                        window.parent.VjEditor.getSelected().append(response.Html)

                    }
                });

            }
            else

            $.get(window.parent.CurrentTabUrl + '?revisionversion=' + $scope.SelectedVersion, function (data) {
                if (data != null) {
                    var htmldom = $.parseHTML(data);
                    var dom = $(data);
                    var Scripts_Links = $(data).find('script');
                    $.each(dom, function (i, v) {
                        Scripts_Links.push(v);
                    });
                    common.webApi.get('Revisions/GetVersion', 'Version=' + $scope.SelectedVersion + '&Locale=' + window.parent.VJLocal).success(function (response) {
                        if (response != undefined) {
                            $scope.ui.data.Versions.Options = response.Version;
                            $(window.parent.document.body).find('.revisionloaderimg').remove();
                            var mids = [];
                            $.each($(data).find('[mid]'), function (key, val) {
                                mids.push($(val).attr('mid'));
                            }).promise().done(function () {
                                var Css = '';
                                $.each($(data).find('style'), function (sk, sv) {
                                    if (Css.indexOf(sv.innerHTML) <= 0)
                                        Css += sv.innerHTML;
                                }).promise().done(function () {
                                    window.parent.window.VjEditor.setComponents(eval(response.components));
                                    window.parent.window.VjEditor.setStyle(eval(response.style));
                                    if (Css.length > 0)
                                        window.parent.window.VjEditor.addComponents('<style>' + Css + '</style>');
                                    if (mids.length > 0) {
                                        $.each(mids, function (midkey, mid) {
                                            var existinghtml = $(window.parent.window.VjEditor.Canvas.getDocument()).find('[mid=' + mid + ']>[vjmod]')[0];
                                            if (existinghtml != undefined) {
                                                if ($($(data).find('#dnn_vj_' + mid)[0]).hasClass('DNNEmptyPane'))
                                                    $($(window.parent.window.VjEditor.Canvas.getDocument()).find('[mid=' + mid + ']>[vjmod]')[0]).closest('[dmid]').remove();
                                                else
                                                    $($(window.parent.window.VjEditor.Canvas.getDocument()).find('[mid=' + mid + ']>[vjmod]')[0]).html($(data).find('#dnn_vj_' + mid)[0].outerHTML);
                                            }
                                        });
                                    }
                                    if (response.BlocksMarkUp != undefined) {
                                        var blockdom = $(response.BlocksMarkUp);
                                        $.each(blockdom, function (k, v) {
                                            if ($(v).attr('data-block-guid') != undefined) {
                                                var blocksattributes = "";
                                                $.each(v.attributes, function (k, v) {
                                                    blocksattributes += "[" + v.nodeName + '=' + '"' + v.nodeValue + '"' + ']';
                                                });
                                                var existinghtmls = $(window.parent.window.VjEditor.Canvas.getDocument()).find(blocksattributes);
                                                $.each(existinghtmls, function (ind, va) {
                                                    if ($(va).attr('data-block-type') == "Logo") {
                                                        $scope.BuildLogo(JSON.parse(response.components), $(va).attr('id'), va, v);
                                                    }
                                                    if ($(va).attr('data-block-type') == "global") {
                                                        var GlobalMarkUp = $(v.innerHTML);
                                                        $.each(GlobalMarkUp.find('[data-block-type="Logo"]'), function (k, v) {
                                                            var style = $(v).attr('data-style');
                                                            if (style != undefined) {
                                                                $(v).find('img').attr('style', style);
                                                            }
                                                        });
                                                        $(va).html(GlobalMarkUp);
                                                    }
                                                    else
                                                        $(va).html(v.innerHTML);
                                                });
                                            }
                                        });
                                    }
                                    if (response.Scripts != undefined) {
                                        $.each(response.Scripts, function (k, v) {
                                            var script = $(window.parent.window.VjEditor.Canvas.getDocument().createElement('script'))[0];
                                            script.type = 'text/javascript';
                                            script.src = v;
                                            script.src = script.src.replace('~/', '');
                                            $(window.parent.window.VjEditor.Canvas.getDocument()).find('head')[0].appendChild(script);
                                        });
                                    }
                                    if (response.Styles != undefined) {
                                        $.each(response.Styles, function (k, v) {
                                            var link = $(window.parent.window.VjEditor.Canvas.getDocument().createElement('link'))[0];
                                            link.rel = 'stylesheet';
                                            link.type = 'text/css';
                                            link.href = v;
                                            link.href = link.href.replace('~/', '');
                                            $(window.parent.window.VjEditor.Canvas.getDocument()).find('head')[0].appendChild(link);
                                        });
                                    }
                                    if (response.Script != undefined) {
                                        var script_tag = $(window.parent.window.VjEditor.Canvas.getDocument().createElement('script'))[0];
                                        script_tag.type = 'text/javascript';
                                        script_tag.text = response.Script;
                                        $(window.parent.window.VjEditor.Canvas.getDocument()).find('head')[0].appendChild(script_tag);
                                    }
                                    $.each(window.parent.getAllComponents(), function (key, model) {
                                        if (model.attributes != undefined && model.attributes.type != undefined && model.attributes.type == "blockwrapper")
                                            model.attributes.name = model.attributes.attributes["data-block-type"];
                                        else if (model.attributes != undefined && model.attributes.type != undefined && model.attributes.type == "globalblockwrapper") {
                                            var Name = window.parent.GetGlobalBlockName(model.attributes.attributes["data-guid"]);
                                            if (Name != undefined && Name != '') {
                                                model.attributes.name = "Global: " + Name;
                                                window.parent.StyleGlobal(model);
                                            }
                                        }
                                        else if (model.attributes != undefined && model.attributes.type != undefined && model.attributes.type == "modulewrapper") {
                                            var classes = $(model.view.el).find('.dnnmodule').attr('class');
                                            if (classes != undefined) {
                                                $.each(classes.split(' '), function (k, v) {
                                                    if (v.toLowerCase().startsWith('dnnmodule-') && !$.isNumeric(v.toLowerCase().split('dnnmodule-')[1]))
                                                        model.attributes.name = "App: " + v.replace('dnnmodule-', '').replace('DnnModule-', '');
                                                });
                                            }
                                        }
                                    });
                                    window.parent.window.VjEditor.LayerManager.render();
                                });
                            });
                            window.parent.window.VjEditor.runCommand("save");
                        }
                    });
                }
            });
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