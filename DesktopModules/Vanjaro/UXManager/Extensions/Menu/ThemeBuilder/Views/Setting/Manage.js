app.controller('setting_manage', function ($scope, $attrs, $routeParams, $http, CommonSvc, $compile) {

    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $scope.ShowGeneralTab = true;
        setTimeout(function () { $compile($('#dvMarkUp'))($scope); $scope.rightThemeBuilderToggle(); }, 500);
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
                $this.find('.mainblocks label').css('cursor','pointer');
            }
              
            
        });
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'General') {
            $('#General a.nav-link').addClass("active");
            $('#Fonts a.nav-link').removeClass("active");
            $scope.ShowGeneralTab = true;
            setTimeout(function () { $compile($('#dvMarkUp'))($scope); }, 500);
        }
        else {
            $('#General a.nav-link').removeClass("active");
            $('#Fonts a.nav-link').addClass("active");
            $scope.ShowGeneralTab = false;
            common.webApi.get('settings/getfonts','Guid=' + $scope.ui.data.Guid.Value).success(function (response) {
                $scope.ui.data.Fonts.Options = response;
            });
        }
    };

    $scope.AddNewFonts = false;
    $scope.Change_AddNew = function () {
        if ($scope.AddNewFonts == true)
            $scope.AddNewFonts = false;
        else
            $scope.AddNewFonts = true;
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
    }

    $scope.Click_Save = function (sender) {
        if (mnValidationService.DoValidationAndSubmit(sender)) {
            var formdata = {
                Guid: $scope.ui.data.Font.Options.Guid != undefined && $scope.ui.data.Font.Options.Guid != '' ? $scope.ui.data.Font.Options.Guid : '',
                Name: $scope.ui.data.Font.Options.Name,
                Family: $scope.ui.data.Font.Options.Family,
                Css: $scope.ui.data.Font.Options.Css
            };
            common.webApi.post('settings/updatefont', 'Guid=' + $scope.ui.data.Guid.Value, formdata).success(function (response) {
                if (response != undefined && response.IsSuccess) {
                    $scope.ui.data.Fonts.Options = response.Data.Fonts;
                    $scope.AddNewFonts = false;
                }
            });
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
                    common.webApi.post('settings/deletefont', 'Guid=' + $scope.ui.data.Guid.Value, data).success(function (response) {
                        if (response != undefined && response.IsSuccess) {
                            $scope.ui.data.Fonts.Options = response.Data.Fonts;
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
                    common.webApi.post('settings/delete', 'Category=' + Category + '&Guid=' + $scope.ui.data.Guid.Value).success(function (response) {
                        if (response != undefined && response.IsSuccess) {
                            $(window.parent.document.body).find('#iframe')[0].contentWindow.location.reload();
                            $scope.ui.data.MarkUp.Value = response.ManageMarkup;
                            setTimeout(function () { $compile($('#dvMarkUp'))($scope); $scope.rightThemeBuilderToggle(); }, 500);
                        }
                    });
                }
                else
                    setTimeout(function () { $compile($('#dvMarkUp'))($scope); $scope.rightThemeBuilderToggle(); }, 500);
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
                    common.webApi.post('settings/delete', 'Category=' + Category + '&SubCategory=' + SubCategory + '&Guid=' + $scope.ui.data.Guid.Value).success(function (response) {
                        if (response != undefined && response.IsSuccess) {
                            $(window.parent.document.body).find('#iframe')[0].contentWindow.location.reload();
                            $scope.ui.data.MarkUp.Value = response.ManageMarkup;
                            setTimeout(function () { $compile($('#dvMarkUp'))($scope); $scope.rightThemeBuilderToggle(); }, 500);
                        }
                    });
                }
                else
                    setTimeout(function () { $compile($('#dvMarkUp'))($scope); $scope.rightThemeBuilderToggle(); }, 500);
            });
    };

    $scope.OpenPopUp = function (hash) {
        window.location.hash = hash;
    };

    $scope.AddEditSetting = function (hash) {
        event.preventDefault();
        window.location.hash = hash + '/' + $scope.ui.data.Guid.Value;
    };

});