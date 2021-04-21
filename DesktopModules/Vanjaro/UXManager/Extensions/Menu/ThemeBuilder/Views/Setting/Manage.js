app.controller('setting_manage', function ($scope, $attrs, $routeParams, $http, CommonSvc, $compile) {

    var common = CommonSvc.getData($scope);
    $scope.LoadingPath = window.parent.VjDefaultPath + 'loading.svg';

    $scope.CompileMarkup = function (toggle) {
        setTimeout(function () {
            $('.RightSideMenu').show();
            $('.RightSideMenuLoader').hide();
            $compile($('#dvMarkUp'))($scope);
            if (toggle)
                $scope.rightThemeBuilderToggle();
        }, 1000);
    };

    $scope.onInit = function () {
        $scope.GoogleFontOption = false;
        $scope.ShowGeneralTab = true;
        $scope.CompileMarkup(true);
        if ($scope.ui.data.Fonts.Options.length == 0)
            $scope.AddNewFonts = true;

        $("#General").click(function () {
            $scope.rightThemeBuilderToggle();
        });
        if ($scope.ui.data.DeveloperMode.Value == 'true')
            $scope.ui.data.DeveloperMode.Value = true;
        else
            $scope.ui.data.DeveloperMode.Value = false;

    };

    $scope.rightThemeBuilderToggle = function () {
        $("#dvMarkUp .firstblock").children().not('.mainblocks').hide();
        $(".mainblocks").click(function () {
            if ($(this).siblings('.child-wrapper').children().length) {
                $(this).siblings().slideToggle().delay(100);
                $(this).find('.fa').toggleClass("fa-caret-right fa-caret-down");
                $(this).parent().siblings().children().not('.mainblocks').slideUp();
                $(this).parent().siblings().find('.mainblocks .fa').removeClass("fa-caret-down").addClass("fa-caret-right");
            }
        });

        $('#dvMarkUp .firstblock').each(function () {
            var $this = $(this);
            if (!$this.find('.child-wrapper').children().length) {
                $this.find('.mainblocks .fa').css('visibility', 'hidden');
            }
            if ($this.find('.child-wrapper').children().length) {
                $this.find('.mainblocks label').css('cursor', 'pointer');
            }


        });
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'General') {
            $('#General a.nav-link').addClass("active");
            $('#Fonts a.nav-link').removeClass("active");
            $scope.ShowGeneralTab = true;
            $scope.CompileMarkup(false);
        }
        else {
            $('#General a.nav-link').removeClass("active");
            $('#Fonts a.nav-link').addClass("active");
            $scope.ShowGeneralTab = false;
            common.webApi.get('settings/getfonts', 'Guid=' + $scope.ui.data.Guid.Value).then(function (response) {
                $scope.ui.data.Fonts.Options = response.data;
            });
        }
    };

    $scope.AddNewFonts = false;
    $scope.Change_AddNew = function () {
        if ($scope.AddNewFonts == true)
            $scope.AddNewFonts = false;
        else {
            $scope.AddNewFonts = true;
            $scope.GoogleFontOption = false;
        }
        $scope.Click_Reset();
    }

    $scope.Click_CancelEdit = function () {
        $scope.Change_AddNew();
    }

    $scope.Click_Reset = function () {
        $scope.ui.data.Font.Options.Guid = '';
        $scope.ui.data.Font.Options.Name = '';
        $scope.ui.data.Font.Options.Family = '';
        $scope.ui.data.Font.Options.Css = '';
        $scope.ui.data.FontOptions.Value = true;
        $("#GoogleFonts").val($("#GoogleFonts option:first").val());
    }

    $scope.Click_Save = function (sender) {
        if ($scope.ui.data.FontOptions.Value) {
            $scope.Change_GoogleFont();
        }
        else {
            if (mnValidationService.DoValidationAndSubmit(sender)) {
                var formdata = {
                    Guid: $scope.ui.data.Font.Options.Guid != undefined && $scope.ui.data.Font.Options.Guid != '' ? $scope.ui.data.Font.Options.Guid : '',
                    Name: $scope.ui.data.Font.Options.Name,
                    Family: $scope.ui.data.Font.Options.Family,
                    Css: $scope.ui.data.Font.Options.Css
                };
                common.webApi.post('settings/updatefont', 'Guid=' + $scope.ui.data.Guid.Value, formdata).then(function (response) {
                    if (response.data != undefined && response.data.IsSuccess) {
                        $scope.ui.data.Fonts.Options = response.data.Data.Fonts;
                        $scope.AddNewFonts = false;
                    }
                });
            }
        }
    };

    $scope.Click_DeleteFont = function (data) {
        swal({
            title: "[L:DeleteFontTitle]",
            text: "[L:DeleteFontText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteButton]",
            cancelButtonText: "[L:CancelButton]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('settings/deletefont', 'Guid=' + $scope.ui.data.Guid.Value, data).then(function (response) {
                        if (response.data != undefined && response.data.IsSuccess) {
                            $scope.ui.data.Fonts.Options = response.data.Data.Fonts;
                        }
                    });
                }
            });

    }

    $scope.Click_EditFont = function (data) {
        $scope.ui.data.Font.Options.Guid = data.Guid;
        $scope.ui.data.Font.Options.Name = data.Name;
        $scope.ui.data.Font.Options.Family = data.Family;
        $scope.ui.data.Font.Options.Css = data.Css;
        $scope.ui.data.FontOptions.Value = false;
        $scope.GoogleFontOption = false;
        if ($scope.IsGoogleFont($scope.ui.data.Font.Options.Name)) {
            $(elem).attr('selected', 'selected');
            $scope.ui.data.FontOptions.Value = true;
            $scope.GoogleFontOption = true;
        }
        $("#GoogleFonts option:selected").length > 0
        $scope.AddNewFonts = true;

    }

    $scope.Delete = function (Category) {
        window.parent.swal({
            title: "[L:DeleteTitle]",
            text: "[L:DeleteText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteButton]",
            cancelButtonText: "[L:CancelButton]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('settings/delete', 'Category=' + Category + '&Guid=' + $scope.ui.data.Guid.Value).then(function (response) {
                        if (response.data != undefined && response.data.IsSuccess) {
                            $(window.parent.document.body).find('#iframe')[0].contentWindow.location.reload();
                            $scope.ui.data.MarkUp.Value = response.data.ManageMarkup;
                            $scope.CompileMarkup(true);
                        }
                    });
                }
                else
                    $scope.CompileMarkup(true);
            });
    };

    $scope.DeleteSubCategory = function (Category, SubCategory) {
        window.parent.swal({
            title: "[L:DeleteTitle]",
            text: "[L:DeleteText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteButton]",
            cancelButtonText: "[L:CancelButton]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('settings/delete', 'Category=' + Category + '&SubCategory=' + SubCategory + '&Guid=' + $scope.ui.data.Guid.Value).then(function (response) {
                        if (response.data != undefined && response.data.IsSuccess) {
                            $(window.parent.document.body).find('#iframe')[0].contentWindow.location.reload();
                            $scope.ui.data.MarkUp.Value = response.data.ManageMarkup;
                            $scope.CompileMarkup(true);
                        }
                    });
                }
                else
                    $scope.CompileMarkup(true);
            });
    };

    $scope.OpenPopUp = function (hash) {
        window.location.hash = hash;
    };

    $scope.Change_GoogleFont = function () {
        var FontCategory = $('#GoogleFonts').val();
        var FontName = $("#GoogleFonts option:selected").html();
        if (FontCategory != null && FontCategory != "" && FontCategory != "NoneSpecified") {
            $.get("https://fonts.googleapis.com/css2?family=" + FontName + ":ital,wght@0,100;0,300;0,400;0,500;0,700;0,900;1,100;1,300;1,400;1,500;1,700;1,900&display=swap", function (data, status) {
                var formdata = {
                    Guid: $scope.ui.data.Font.Options.Guid != undefined && $scope.ui.data.Font.Options.Guid != '' ? $scope.ui.data.Font.Options.Guid : '',
                    Name: FontName,
                    Family: "'" + FontName + "'," + FontCategory + "",
                    Css: data
                };
                common.webApi.post('settings/updatefont', 'Guid=' + $scope.ui.data.Guid.Value, formdata).then(function (response) {
                    if (response.data != undefined && response.data.IsSuccess) {
                        $scope.ui.data.Fonts.Options = response.data.Data.Fonts;
                        $scope.AddNewFonts = false;
                    }
                });
            });
        }
        else {
            swal('[LS:InvalidGoogleFont]');
        }
    };

    $scope.AddEditSetting = function (hash) {
        event.preventDefault();
        window.location.hash = hash + '/' + $scope.ui.data.Guid.Value;
    };

    $scope.IsGoogleFont = function (FontName) {
        var flag = false;
        $("#GoogleFonts option").each(function (index, elem) {
            if ($(elem).html() == FontName) {
                flag = true;
            }
        });
        return flag;
    }

});