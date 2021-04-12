app.controller('setting_settings', function ($scope, $attrs, $routeParams, $http, CommonSvc, $compile) {

    var common = CommonSvc.getData($scope);

    $scope.HasChanges = false;

    function sliderInputChange(){
        $('#dvMarkUp').find('[guid]').on("change", function () {
            $(this).siblings("input[type='range']").val(this.value);
            $scope.ApplyChanges(this);
        });
    }

    $scope.onInit = function () {
        sliderInputChange();
        $scope.ApplyColorPicker();
        setTimeout(function () {
            $compile($('#dvMarkUp'))($scope);
            $('#dvMarkUp').find('[guid]').on("change", function () {
                $scope.ApplyChanges(this);
            });
            $('#dvMarkUp').find('[guid]').on("input", function () {
                $(this).siblings("input[type='number']").val(this.value);            
                $scope.ApplyChanges(this);
            });
        }, 500);


        $('.subtMenu').on("click", function (e) {
            $(this).next('ul').toggle();
            return false;
        });
        $scope.themeMenuToggle();
    };

    $scope.themeMenuToggle = function () {
        //$("#dvMarkUp .firstblock").first().find('.mainblocks .fa').removeClass('fa-caret-right').addClass('fa-caret-down');
        //$("#dvMarkUp .firstblock").not(":first-child").children().not('.mainblocks').hide();
        $("#dvMarkUp .firstblock").children().not('.mainblocks').hide();
        $(".mainblocks").click(function () {
            $(this).siblings().slideToggle().delay(100);
            $(this).find('.fa').toggleClass("fa-caret-right fa-caret-down");
            $(this).parent().siblings().children().not('.mainblocks').slideUp();
            $(this).parent().siblings().find('.mainblocks .fa').removeClass("fa-caret-down").addClass("fa-caret-right");
        });
    };

    $scope.ApplyColorPicker = function (e) {
        $('#dvMarkUp').find('.color').spectrum({
            showInput: true,
            preferredFormat: "hex",
            showPalette: false,
            showAlpha: true,
            palette: [],
            chooseText: "Ok",
            show: function (color) {

                var body = this.closest('body');

                $(body).click(function (event) {
                    event.stopPropagation();
                });
            },
            move: function (color) {
                $(this).val(color.toRgbString());
                $scope.ApplyChanges(this);
            },
            change: function (color) {
                $(this).val(color.toRgbString());
                $scope.ApplyChanges(this);
            },
            hide: function (color) {
                $(this).val(color.toRgbString());
                $scope.ApplyChanges(this);
            }
        });
    };

    $scope.ApplyChanges = function (e) {
        $scope.HasChanges = true;
        var id = $(e).attr('guid');
        var css = $('#' + id).attr('prevcss');
        var sass = $('#' + id).attr('sass');
        var variable = $('#' + id).attr('value');
        if (css != undefined && css != '') {
            css = css.split(variable).join($(e).val());
            $(window.parent.document.body).find('.gjs-frame').contents().find('head').find('style[data-sass="' + id + '"]').remove();
            $(window.parent.document.body).find('.gjs-frame').contents().find('head').append('<style pvcss="true" data-sass="' + id + '">' + css + '</style>');

            $(window.parent.document.head).find('style[data-sass="' + id + '"]').remove();
            $(window.parent.document.head).append('<style pvcss="true" data-sass="' + id + '">' + css + '</style>');
        }
        if (sass != undefined && sass != '') {

            var iframeexists = false;
            $.each($(window.parent.document.body).find('.gjs-frame').contents().find('head>style'), function () {
                if ($($(window.parent.document.body).find('.gjs-frame').contents().find('head>style')[0]).html() == sass)
                    iframeexists = true;
            });

            var exists = false;
            $.each($(window.parent.document.body).find('.gjs-frame').contents().find('head>style'), function () {
                if ($($(window.parent.document.body).find('.gjs-frame').contents().find('head>style')[0]).html() == sass)
                    exists = true;
            });

            if (!iframeexists)
                $(window.parent.document.body).find('.gjs-frame').contents().find('head').append('<style pvcss="true">' + sass + '</style>');

            if (!exists)
                $(window.parent.document.head).append('<style pvcss="true">' + sass + '</style>');
        }
    };

    $scope.SaveTheme = function () {
        event.preventDefault();
        var ResetMarkUp = $('#dvMarkUp').html();
        var data = [];
        $.each($('#dvMarkUp').find('[guid]'), function (k, v) {
            var obj = {
                Guid: $(v).attr('guid'),
                Value: $(v).val()
            };
            data.push(obj);
        });
        var formdata = {
            ThemeEditorValues: data,
            Css: ''
        };

        window.parent.swal({
            title: "[L:WarningTitle]",
            text: "[L:WarningText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('settings/save', 'guid=' + $scope.ui.data.Guid.Value, formdata).then(function (response) {
                        if (response.data.Message == undefined) {
                            window.parent.ShowNotification('[L:Theme]', '[L:SaveSuccess]', 'success');
                            window.parent.location.reload();
                        }
                        else {
                            window.parent.ShowNotification('[L:Theme]', response.data.Message, 'error');
                            $scope.ResetMarkUp(ResetMarkUp, formdata);
                        }
                    });
                }
                else
                    $scope.ResetMarkUp(ResetMarkUp, formdata);
            });
    };

    $scope.ResetMarkUp = function (Data, FormData) {
        $scope.ui.data.MarkUp.Value = Data;
        setTimeout(function () {
            $compile($('#dvMarkUp'))($scope);
            $scope.ApplyColorPicker();
            $('#dvMarkUp').find('[guid]').on("change", function () {
                $scope.ApplyChanges(this);
            });
            $('#dvMarkUp').find('[guid]').on("input", function () {
                $(this).siblings("input[type='number']").val(this.value);
                $scope.ApplyChanges(this);
            });
            $('.subtMenu').on("click", function (e) {
                $(this).next('ul').toggle();
                return false;
            });
            $scope.themeMenuToggle();
            if (FormData != undefined && FormData.ThemeEditorValues != undefined) {
                $.each(FormData.ThemeEditorValues, function (k, v) {
                    $('[guid="' + v.Guid + '"]').val(v.Value);
                });
            }
            sliderInputChange();
        }, 100);
    };

    $scope.GetCss = function () {
        var result = [];
        $.each($('#dvMarkUp').find('[guid]'), function (k, v) {
            var id = $(v).attr('guid');
            var css = $('#' + id).attr('css');
            var sass = $('#' + id).attr('sass');
            var variable = $('#' + id).attr('value');
            if (css != undefined && css != '') {
                css = css.split(variable).join($(v).val());
                result.push(css + ';');
            }
            else if (variable != undefined && variable != '' && variable.startsWith("$")) {
                result.push(variable + ":" + $(v).val() + ' !default;');
            }

            if (sass != undefined && sass != '') {
                result.push(sass + ';');
            }
        });
        return result;
    };

    $scope.BackTo_Theme = function () {
        event.preventDefault();
        if ($scope.HasChanges) {
            var ResetMarkUp = $('#dvMarkUp').html();
            var data = [];
            $.each($('#dvMarkUp').find('[guid]'), function (k, v) {
                var obj = {
                    Guid: $(v).attr('guid'),
                    Value: $(v).val()
                };
                data.push(obj);
            });
            var formdata = {
                ThemeEditorValues: data,
                Css: ''
            };
            window.parent.swal({
                title: "[L:WarningTitle]",
                text: "[L:WarningBackText]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
                cancelButtonText: "[LS:No]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        window.location.hash = '#!/categories/' + $scope.ui.data.ThemeName.Value;
                    }
                    else
                        $scope.ResetMarkUp(ResetMarkUp, formdata);
                });
        }
        else
            window.location.hash = '#!/categories/' + $scope.ui.data.ThemeName.Value;
    };

    $scope.OpenPopUp = function () {
        event.preventDefault();
        var url = window.location.href.split('#')[0];
        url = url + "#!/manage/" + $scope.ui.data.Guid.Value;
        parent.OpenPopUp(event, 900, 'right', '[LS:ThemeBuilder]', url);
        window.location.reload();
    };

    $scope.ResetTheme = function () {
        event.preventDefault();        
        $scope.ResetMarkUp($scope.ui.data.MarkUp.Value, null);
        $(window.parent.document.body).find('.gjs-frame').contents().find('head').find('style[pvcss="true"]').remove();
        $(window.parent.document.head).find('style[pvcss="true"]').remove();
        $scope.HasChanges = false;
    };
});