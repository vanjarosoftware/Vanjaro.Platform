app.controller('settings_setting', function ($scope, $attrs, $http, CommonSvc, $sce) {
    var common = CommonSvc.getData($scope);

    //Init Scope
    $scope.onInit = function () {
        if ($scope.ui.data.IconFolders.Value != '')
            $scope.ui.data.IconFolders.Value = $scope.ui.data.DefaultIconLocation.Value.toString();
        $("#icon-search").keydown(function (e) {
            if (e.keyCode == 13) {
                $scope.Search_Icon();
                event.preventDefault();
            }
        });

        $(".close-iconbtn").click(function () {
            $(".icon-search-block input").val("");
            $(".icon-search-block em.fa-times").hide();
            $scope.Search_Icon();
        });

        $scope.Search_Icon();
        setTimeout(function () { $('input#icon-search').focus() }, 500)
    };
    $scope.Paging = {
        PageSize: 60,
        Skip: 0,
        Total_Icon: 0,
    };
    $scope.NoIconFound = true;
    $scope.Parse = function (option) {
        return $sce.trustAsHtml(option.SVG);
    }

    $scope.selectIcon = function (option) {
        var compSelected = window.parent.VjEditor.getSelected();

        if (location.href.indexOf('ignoregj') > 0)
            $(window.parent.document.body).find('.uxmanager-modal #UXpagerender').contents().find('svg').replaceWith(option.SVG);
        else {
            var style = compSelected.components().models[0].getStyle();
            compSelected.components(option.SVG);
            compSelected.components().models[0].addStyle(style);
        }

    };

    $scope.doubleclick = function () {
        if (location.href.indexOf('ignoregj') > 0)
            $(window.parent.document.body).find('.uxmanager-modal [data-bs-dismiss="modal"]').click();
        else
            $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };

    $(document).ready(function () {
        //Start-Icon Search
        function debounce(func, wait, immediate) {
            var timeout;
            return function () {
                var context = this, args = arguments;
                var later = function () {
                    timeout = null;
                    if (!immediate) func.apply(context, args);
                };
                var callNow = immediate && !timeout;
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
                if (callNow) func.apply(context, args);
            };
        };
        var Icon_Search = debounce(function () {
            var inputval = $(".icon-search-block input").val();
            if (inputval == "") {
                $(".icon-search-block em.fa-times").hide();
            }
            else {
                $(".icon-search-block em.fa-times").show();
            }
            $scope.Paging.Skip = 0;
            $scope.Search_Icon();

        }, 500);
        window.addEventListener('keyup', Icon_Search);
        //End-Icon Search  
    });


    $scope.Search_Icon = function () {
        var Keyword = $('#icon-search').val();
        if (typeof Keyword == 'undefined' || Keyword == null || Keyword == "")
            Keyword = "";

        common.webApi.post('icon/Search', 'Keyword=' + Keyword + '&index=' + $scope.Paging.Skip + '&size=' + $scope.Paging.PageSize + '&iconfolder=' + $scope.ui.data.IconFolders.Value, '').success(function (Response) {
            if (Response.IsSuccess) {
                var scrolliconspage = $('.vj-icon-wrapper');
                if ($scope.Paging.Skip == 0) {
                    $scope.ui.data.All_Icons.Options = Response.Data.All_Icons;
                }
                else {
                    $.each(Response.Data.All_Icons, function (key, value) {
                        $scope.ui.data.All_Icons.Options.push(value);
                    });
                    scrolliconspage.animate({ scrollTop: scrolliconspage.prop("scrollHeight") });
                }
                $scope.Paging.Total_Icon = Response.Data.Total_Icon;
                $scope.Paging.Skip = $scope.ui.data.All_Icons.Options.length >= $scope.Paging.PageSize ? $scope.ui.data.All_Icons.Options.length : 0;
                $scope.NoIconFound = $scope.ui.data.All_Icons.Options.length > 0 ? true : false;
            }
            if (Response.HasErrors) {
                window.parent.ShowNotification('[L:IconError]', Response.Message, 'error');
            }
        });
    };

    $scope.Change_IconFolder = function () {
        $scope.Paging.Skip = 0;
        $scope.Search_Icon();
    };
});