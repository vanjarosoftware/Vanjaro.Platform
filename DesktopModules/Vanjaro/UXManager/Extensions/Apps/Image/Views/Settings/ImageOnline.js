app.controller('settings_imageonline', function ($scope, $attrs, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);
    $scope.Images = [];
    $scope.SearchKeyword = "";
    $scope.ImageProvidersLink = "";
    $scope.ImageProvidersImage = "";
    $scope.PictureId;

    $scope.onInit = function () {
        window.parent.window.VJIsSaveCall = true;
        $('.uiengine-wrapper a[data-target="#!/admin"]').removeClass("active");
        $('.uiengine-wrapper a[data-target="#!/imageonline"]').addClass("active");
        $scope.ChangeImageProviders();
        if ($(window.parent.document.body).find('[data-bs-dismiss="modal"]').length <= 0)
            $(window.parent.document.body).find('.btn-close').attr('data-bs-dismiss', 'modal');
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').on("click", function (e, popup) {
            if (typeof popup === 'undefined') {
                e.stopImmediatePropagation();
                $scope.SaveImage();
            }
        });

        $('#VjImageKeyword').keypress(function (event) {
            var keycode = (event.keyCode ? event.keyCode : event.which);
            if (keycode == '13') {
                if ($scope.ui.data.keyword != undefined && $scope.ui.data.keyword.Value.length >= 3) {
                    $scope.DebounceSearch();
                }
                event.preventDefault();
            }
        });
        $('.AllImages tfoot tr').hide();
    };

    var AutoSaveTimeOutid;
    $scope.DebounceSearch = function () {
        if (AutoSaveTimeOutid) {
            clearTimeout(AutoSaveTimeOutid);
        }
        AutoSaveTimeOutid = setTimeout(function () {
            $scope.RefreshGrid();
        }, 500)
    }

    $scope.Pipe_ImagePagging = function (tableState) {
        $scope.ImageTableState = tableState;
        if ($scope.ui.data.keyword != undefined && $scope.ui.data.keyword.Value.length >= 3) {
            $scope.Images = [];
            $scope.SearchKeyword = $scope.ui.data.keyword.Value;
            common.webApi.get('Image/Search', 'source=' + $scope.ui.data.ImageProviders.Value + '&keyword=' + $scope.ui.data.keyword.Value + '&PageNo=' + Math.round(($scope.ImageTableState.pagination.start / $scope.ImageTableState.pagination.number) + 1)).then(function (data) {
                var imgjson = JSON.parse(data.data);
                $.each(imgjson.hits, function (key, value) {
                    $scope.Images.push(value);
                });
                $scope.ImageTableState.pagination.numberOfPages = Math.round(imgjson.totalHits / 35);
                if ($scope.ImageTableState.pagination.numberOfPages > 1)
                    $('.AllImages tfoot tr').show();
            });
        }
        else {
            $scope.Images = [];
            $('.AllImages tfoot tr').hide();
            $scope.$apply();
        }
    };

    $scope.SaveOndblClick = function () {
        $scope.SaveImage();
    };

    $scope.SaveImage = function () {
        if ($(window.parent.document.body).find('[data-bs-dismiss="modal"]').length <= 0)
            $(window.parent.document.body).find('.btn-close').attr('data-bs-dismiss', 'modal');
        if ($scope.PictureId != undefined) {

            $(window.parent.document.body).find('.uxmanager-modal .modal-dialog').find('iframe').css("pointer-events", "none");
            if (!$(window.parent.document.body).find('.uxmanager-modal .modal-dialog').find('.modal-backdrop').length)
                $(window.parent.document.body).find('.uxmanager-modal .modal-dialog').append('<div class="modal-backdrop fade show"></div>');

            var svPath = '';
            var target = window.parent.document.vj_image_target;
            if (typeof target != 'undefined') {
                svPath = target.attributes["src"];
            }
            else {
                var background = window.parent.VjEditor.StyleManager.getProperty('background_&_shadow', 'background');
                svPath = background.getSelectedLayer().prop.attributes.properties.models.find(m => m.id == 'background-image-sub').attributes.value;
            }
            common.webApi.post('Image/Save', 'path=' + svPath + '&id=' + $scope.PictureId).then(function (data) {
                if (data.data != "failed") {
                    window.parent.window.VJIsSaveCall = false;
                    $scope.SelectImage(data.data, true);
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').trigger('click', 'close');
                }
            });
        }
        else
            $(window.parent.document.body).find('[data-bs-dismiss="modal"]').trigger('click', 'close');
    };

    $scope.ChangeImageProviders = function () {
        $.each($scope.ui.data.ImageProviders.Options, function (k, v) {
            if (v.Value == $scope.ui.data.ImageProviders.Value) {
                if (v.ShowLogo) {
                    $scope.ImageProvidersLink = v.Link;
                    $scope.ImageProvidersImage = v.Logo;
                }
                else {
                    $scope.ImageProvidersLink = "";
                    $scope.ImageProvidersImage = "";
                }
                $scope.Images = [];
                $scope.RefreshGrid();
            }
        });
    };

    $scope.SelectImage = function (URL, save) {
        if (save == undefined)
            window.parent.window.VJIsSaveCall = true;
        var target = window.parent.document.vj_image_target;
        var url = null;

        if (typeof URL.fullHDURL != 'undefined') {
            url = URL.fullHDURL;
            $scope.PictureId = URL.id;
        }
        else if (typeof URL.largeImageURL != 'undefined') {
            url = URL.largeImageURL;
            $scope.PictureId = URL.id;
        }
        else {
            if (typeof (URL) == "object")
                url = URL.Url
            else
                url = URL;
        }

        //checked background image has not changed
        if (typeof target != 'undefined') {

            $(target.parent().components().models).each(function (index, component) {
                if (component.getName() == "Source")
                    component.remove();
            });
            target.set('src', url);

            if (save != undefined && save == true) {

                if (URL.Urls.length) {

                    if (target.attributes.optimize)
                        parent.ChangeToWebp(target.parent(), URL.Urls);
                    else
                        target.set('src', URL.Urls.find(v => v.Type == 'webp').Url);
                }
                else {
                    target.removeStyle('max-width');
                    $(target.parent().components().models).each(function (index, component) {
                        if (component.getName() == "Source")
                            component.remove();
                    });
                }

                window.parent.VjEditor.runCommand("save");
            }
        }
        else {

            var background = window.parent.VjEditor.StyleManager.getProperty('background_&_shadow', 'background');

            if (save != undefined && save && URL.Urls.length)
                url = URL.Urls.find(v => v.Type == 'webp').Url;

            background.getSelectedLayer().prop.attributes.properties.models.find(m => m.id == 'background-image-sub').setValue(url);
        }
        if (save == undefined)
            $(window.parent.document.body).find('[data-bs-dismiss="modal"]').removeAttr('data-bs-dismiss');
    };

    $scope.RefreshGrid = function () {
        if ($scope.ImageTableState != undefined) {
            $scope.Images = [];
            $scope.ImageTableState.pagination.start = 1;
            $scope.ImageTableState.pagination.numberOfPages = 35;
            $scope.Pipe_ImagePagging($scope.ImageTableState);
        };
    };
});