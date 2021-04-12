app.controller('setting_edit', function ($scope, $attrs, $routeParams, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.EditControltype = '';
    $scope.EditControl = false;
    $scope.EditMarkup = '';
    $scope.EditKey = '';
    $scope.HideCategory = false;

    $scope.onInit = function () {
        $(".Sass label").css('display', 'none');

        var StylesheetEditorID = $("textarea.Sass").attr("id");
        $scope.SassStyle = CodeMirror.fromTextArea(document.getElementById(StylesheetEditorID), {
            lineNumbers: true,
            mode: "css",
            matchBrackets: true,
            autocorrect: true
        });
        $scope.SassStyle.setSize(600, 600);

        var CustomCSSStyleEditorID = $("textarea.CustomCSS").attr("id");
        $scope.CustomCSSStyle = CodeMirror.fromTextArea(document.getElementById(CustomCSSStyleEditorID), {
            lineNumbers: true,
            mode: "css",
            matchBrackets: true,
            autocorrect: true
        });
        $scope.CustomCSSStyle.setSize(600, 400);

        var PreviewCSSStyleEditorID = $("textarea.PreviewCSS").attr("id");
        $scope.PreviewCSSStyle = CodeMirror.fromTextArea(document.getElementById(PreviewCSSStyleEditorID), {
            lineNumbers: true,
            mode: "css",
            matchBrackets: true,
            autocorrect: true
        });
        $scope.PreviewCSSStyle.setSize(600, 400);

        setTimeout(function () {
            if ($scope.ui.data.IsNew.Value == 'true')
                $(window.parent.document.body).find('.modal-title').text("[L:NewSetting]");
            else
                $(window.parent.document.body).find('.modal-title').text("[L:EditSetting]");
            $scope.SassStyle.setValue($scope.ui.data.ThemeEditor.Options.Sass);
        }, 100);
        $scope.InitEditors();

        if ($scope.ui.data.Category.Value.length > 0)
            $scope.ui.data.ThemeEditor.Options.Category = $scope.ui.data.Category.Value;

        if ($scope.ui.data.Type.Value.length > 0 && $scope.ui.data.Type.Value == 'newsub') {
            $scope.HideCategory = true;
        }
        if ($scope.ui.data.Category.Value.length == 0 && $scope.ui.data.Type.Value.length == 0 && $scope.ui.data.ThemeEditor.Options.Title != undefined && $scope.ui.data.ThemeEditor.Options.Title.length > 0) {
            $scope.HideCategory = true;
        }
        if ($scope.ui.data.DeveloperMode.Value == 'true')
            $scope.ui.data.DeveloperMode.Value = true;
        else
            $scope.ui.data.DeveloperMode.Value = false;
    };

    $scope.InitEditors = function () {
        setTimeout(function () {
            $scope.SassStyle.refresh();
            $scope.CustomCSSStyle.refresh();
            $scope.PreviewCSSStyle.refresh();
        }, 200);
    };

    $scope.Click_AddControl = function (row) {
        $scope.EditControl = true;
        $scope.EditControltype = row;
        $scope.Reset();
        $scope.InitEditors();
    };

    $scope.Click_Cancel = function () {
        $scope.EditControl = false;
        $scope.EditControltype = '';
        $scope.Reset();
    };

    $scope.Reset = function () {
        $scope.EditKey = '';
        $('input.Title').val('');
        $('input.DefaultValue').val('');
        $('input.Suffix').val('');
        $('input.RangeMin').val(0);
        $('input.RangeMax').val(100);
        $('input.Increment').val(5);
        $('textarea.Options').val('');
        $scope.CustomCSSStyle.setValue('');
        $scope.PreviewCSSStyle.setValue('');
        $('input.LessVariable').val('');
        $('input.JavascriptVariable').val('');
    };

    $scope.Click_Edit = function (row) {
        $scope.EditControl = true;
        $scope.EditControltype = row.Type;
        if (row.Guid != '')
            $scope.EditKey = row.Guid;
        else if (row.$$hashKey != undefined)
            $scope.EditKey = row.$$hashKey;
        $scope.InitEditors();
        if ($scope.EditControltype == "Slider") {
            $('input.Title').val(row.Title != undefined ? row.Title : '');
            $('input.DefaultValue').val(row.DefaultValue != undefined ? row.DefaultValue : '');
            $('input.Suffix').val(row.Suffix != undefined ? row.Suffix : '');
            $('input.RangeMin').val(row.RangeMin != undefined ? row.RangeMin : 0);
            $('input.RangeMax').val(row.RangeMax != undefined ? row.RangeMax : 100);
            $('input.Increment').val(row.Increment != undefined ? row.Increment : 5);
            $scope.CustomCSSStyle.setValue(row.CustomCSS != undefined ? row.CustomCSS : '');
            $scope.PreviewCSSStyle.setValue(row.PreviewCSS != undefined ? row.PreviewCSS : '');
            $('input.LessVariable').val(row.LessVariable != undefined ? row.LessVariable : '');
            $('input.JavascriptVariable').val(row.JavascriptVariable != undefined ? row.JavascriptVariable : '');
        }
        else if ($scope.EditControltype == "Dropdown") {
            $('input.Title').val(row.Title != undefined ? row.Title : '');
            $('input.DefaultValue').val(row.DefaultValue != undefined ? row.DefaultValue : '');
            $('input.Suffix').val(row.Suffix != undefined ? row.Suffix : '');
            var Options = '';
            if (row.Options != undefined) {
                $.each(row.Options, function (k, v) {
                    Options += Object.keys(v)[0] + '|' + v[Object.keys(v)[0]] + '\n';
                });
            }
            $('textarea.Options').val(Options.trimEnd('\n'));
            $scope.CustomCSSStyle.setValue(row.CustomCSS != undefined ? row.CustomCSS : '');
            $scope.PreviewCSSStyle.setValue(row.PreviewCSS != undefined ? row.PreviewCSS : '');
            $('input.LessVariable').val(row.LessVariable != undefined ? row.LessVariable : '');
            $('input.JavascriptVariable').val(row.JavascriptVariable != undefined ? row.JavascriptVariable : '');
        }
        else if ($scope.EditControltype == "Color Picker") {
            $('input.Title').val(row.Title != undefined ? row.Title : '');
            $('input.DefaultValue').val(row.DefaultValue != undefined ? row.DefaultValue : '');
            $('input.Suffix').val(row.Suffix != undefined ? row.Suffix : '');
            $scope.CustomCSSStyle.setValue(row.CustomCSS != undefined ? row.CustomCSS : '');
            $scope.PreviewCSSStyle.setValue(row.PreviewCSS != undefined ? row.PreviewCSS : '');
            $('input.LessVariable').val(row.LessVariable != undefined ? row.LessVariable : '');
            $('input.JavascriptVariable').val(row.JavascriptVariable != undefined ? row.JavascriptVariable : '');
        }
        else if (row.Type == "Fonts") {
            $('input.Title').val(row.Title != undefined ? row.Title : '');
            $('select.DefaultValue').val(row.DefaultValue != undefined ? row.DefaultValue : '');
            $('input.Suffix').val(row.Suffix != undefined ? row.Suffix : '');
            $scope.CustomCSSStyle.setValue(row.CustomCSS != undefined ? row.CustomCSS : '');
            $scope.PreviewCSSStyle.setValue(row.PreviewCSS != undefined ? row.PreviewCSS : '');
            $('input.LessVariable').val(row.LessVariable != undefined ? row.LessVariable : '');
            $('input.JavascriptVariable').val(row.JavascriptVariable != undefined ? row.JavascriptVariable : '');
        }
    };

    $scope.Click_Delete = function (row) {
        $scope.ui.data.ThemeEditor.Options.Controls = $scope.ui.data.ThemeEditor.Options.Controls.filter(function (s) {
            return s !== row;
        })
    };

    $scope.Click_Save = function (sender) {
        if (mnValidationService.DoValidationAndSubmit(sender)) {
            $scope.ui.data.ThemeEditor.Options.Sass = $scope.SassStyle.getValue();
            common.webApi.post('edit/Update', 'catguid=' + $scope.ui.data.CatGuid.Value, $scope.ui.data.ThemeEditor.Options).then(function (response) {
                if (response.data != 'Failed') {
                    var parentScope = parent.document.getElementById("iframe").contentWindow.angular.element("#dvMarkUp").scope();
                    parentScope.ui.data.MarkUp.Value = response.data;
                    window.parent.document.getElementById("iframe").contentWindow.location.reload();
                    window.location.hash = '#!/manage/' + $scope.ui.data.CatGuid.Value;
                }
                else
                    swal(response.data);
            });
        }
    };

    $scope.Click_CancelEdit = function () {
        window.location.hash = '#!/manage/' + $scope.ui.data.CatGuid.Value;
    };

    $scope.AddUpdateControl = function (type, control) {
        if ($scope.EditKey == '')
            $scope.ui.data.ThemeEditor.Options.Controls.push(control);
        else {
            var index = -1;
            if ($scope.IsGuid($scope.EditKey)) {
                $.each($scope.ui.data.ThemeEditor.Options.Controls, function (k, v) {
                    if (v.Guid == $scope.EditKey)
                        index = k;
                });
            }
            else {
                $.each($scope.ui.data.ThemeEditor.Options.Controls, function (k, v) {
                    if (v.$$hashKey == $scope.EditKey)
                        index = k;
                });
            }
            if (index > -1) {
                if ($scope.EditControltype == "Slider") {
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Title = control.Title;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].DefaultValue = control.DefaultValue;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Suffix = control.Suffix;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].RangeMin = control.RangeMin;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].RangeMax = control.RangeMax;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Increment = control.Increment;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].CustomCSS = control.CustomCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].PreviewCSS = control.PreviewCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].LessVariable = control.LessVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].JavascriptVariable = control.JavascriptVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Type = control.Type;
                }
                else if ($scope.EditControltype == "Dropdown") {
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Title = control.Title;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].DefaultValue = control.DefaultValue;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Suffix = control.Suffix;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].CustomCSS = control.CustomCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].PreviewCSS = control.PreviewCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].LessVariable = control.LessVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].JavascriptVariable = control.JavascriptVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Type = control.Type;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Options = control.Options;
                }
                else if ($scope.EditControltype == "Color Picker") {
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Title = control.Title;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].DefaultValue = control.DefaultValue;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Suffix = control.Suffix;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].CustomCSS = control.CustomCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].PreviewCSS = control.PreviewCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].LessVariable = control.LessVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].JavascriptVariable = control.JavascriptVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Type = control.Type;
                }
                else if ($scope.EditControltype == "Fonts") {
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Title = control.Title;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].DefaultValue = control.DefaultValue;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Suffix = control.Suffix;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].CustomCSS = control.CustomCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].PreviewCSS = control.PreviewCSS;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].LessVariable = control.LessVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].JavascriptVariable = control.JavascriptVariable;
                    $scope.ui.data.ThemeEditor.Options.Controls[index].Type = control.Type;
                }
            }
        }
    };

    $scope.IsGuid = function (value) {
        var regex = /[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}/i;
        var match = regex.exec(value);
        return match != null;
    };

    $scope.Click_SaveControl = function (sender) {
        if (mnValidationService.DoValidationAndSubmit(sender)) {
            //var LessVariables = $('input.LessVariable').val().split(" ");
            //var CustomCSS = $('textarea.CustomCSS').val();
            var IsValid = true;
            //if (LessVariables != undefined && LessVariables.length > 0) {
            //    $.each(LessVariables, function (k, v) {
            //        if (CustomCSS.indexOf(v) < 0)
            //            IsValid = false;
            //    });
            //}
            if (IsValid) {
                if ($scope.EditControltype == "Slider") {
                    var slider = {
                        Guid: '',
                        Title: $('input.Title').val(),
                        DefaultValue: $('input.DefaultValue').val(),
                        Suffix: $('input.Suffix').val(),
                        RangeMin: $('input.RangeMin').val(),
                        RangeMax: $('input.RangeMax').val(),
                        Increment: $('input.Increment').val(),
                        CustomCSS: $scope.CustomCSSStyle.getValue(),
                        PreviewCSS: $scope.PreviewCSSStyle.getValue(),
                        LessVariable: $('input.LessVariable').val(),
                        JavascriptVariable: $('input.JavascriptVariable').val(),
                        Type: $scope.EditControltype
                    };
                    $scope.AddUpdateControl($scope.EditControltype, slider);
                }
                else if ($scope.EditControltype == "Dropdown") {
                    var dropdown = {
                        Guid: '',
                        Title: $('input.Title').val(),
                        DefaultValue: $('input.DefaultValue').val(),
                        Suffix: $('input.Suffix').val(),
                        CustomCSS: $scope.CustomCSSStyle.getValue(),
                        PreviewCSS: $scope.PreviewCSSStyle.getValue(),
                        LessVariable: $('input.LessVariable').val(),
                        JavascriptVariable: $('input.JavascriptVariable').val(),
                        Type: $scope.EditControltype,
                        Options: $scope.GetOptions($('textarea.Options').val())
                    };
                    $scope.AddUpdateControl($scope.EditControltype, dropdown);
                }
                else if ($scope.EditControltype == "Color Picker") {
                    var colorpicker = {
                        Guid: '',
                        Title: $('input.Title').val(),
                        DefaultValue: $('input.DefaultValue').val(),
                        Suffix: $('input.Suffix').val(),
                        CustomCSS: $scope.CustomCSSStyle.getValue(),
                        PreviewCSS: $scope.PreviewCSSStyle.getValue(),
                        LessVariable: $('input.LessVariable').val(),
                        JavascriptVariable: $('input.JavascriptVariable').val(),
                        Type: $scope.EditControltype
                    };
                    $scope.AddUpdateControl($scope.EditControltype, colorpicker);
                }
                else if ($scope.EditControltype == "Fonts") {
                    var fonts = {
                        Guid: '',
                        Title: $('input.Title').val(),
                        DefaultValue: $('select.DefaultValue').val(),
                        Suffix: $('input.Suffix').val(),
                        CustomCSS: $scope.CustomCSSStyle.getValue(),
                        PreviewCSS: $scope.PreviewCSSStyle.getValue(),
                        LessVariable: $('input.LessVariable').val(),
                        JavascriptVariable: $('input.JavascriptVariable').val(),
                        Type: $scope.EditControltype
                    };
                    $scope.AddUpdateControl($scope.EditControltype, fonts);
                }
                $scope.Click_Cancel();
            }
            else
                window.parent.swal('[L:LessVariableError]');
        }
    };

    $scope.GetOptions = function (Value) {
        var result = [];
        if (Value != undefined) {
            $.each(Value.split("\n"), function (index, Option) {
                Option = jQuery.trim(Option);
                var optval = Option.split("|");
                var txt = optval[0];
                var val = optval[0];
                if (optval[1] != undefined)
                    val = optval[1];
                var obj = {};
                obj[txt] = val;
                result.push(obj);
            });
        }
        return result;
    };

    $scope.dragdone = function () {
        setTimeout(function () {
            $scope.Controls = [];
            $(".sortguid").each(function () {
                var GUID = $(this).attr('guid');
                $.each($scope.ui.data.ThemeEditor.Options.Controls, function (index, value) {
                    if (value.Guid == GUID) {
                        $scope.Controls.push(value);
                    }
                });
            });

            $scope.ui.data.ThemeEditor.Options.Controls = $scope.Controls;
        }, 200);
    }

});