app.controller('seo_settings', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    //Init Scope
    $scope.onInit = function () {
        $scope.ShowSiteTab = true;
        $scope.tag = null;
        $scope.Group = null;
        BindSitemapPriorityList();
        if ($scope.ui.data.SitemapSettings.Options.SitemapCacheDays != '')
            $scope.ui.data.SitemapSettings.Options.SitemapCacheDays = $scope.ui.data.SitemapSettings.Options.SitemapCacheDays.toString();

        $scope.Ignoretags = $scope.ui.data.IgnoreWord.Options.StopWords.split(",");
        $.each($scope.ui.data.AllSynonymsGroups.Options, function (key, object) {
            if (typeof object.SynonymsTags == "string") {
                var Tags = new Array();
                $.each(object.SynonymsTags.split(","), function (k, v) {
                    Tags.push({ text: v });
                });
                object.SynonymsTags = Tags;
            }
        });
        $scope.ui.data.BasicSettings.Options.SearchCustomAnalyzer = $scope.ui.data.CustomAnalyzer.Value;
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'Site') {
            $('#Site a.nav-link').addClass("active");
            $('#URLs a.nav-link').removeClass("active");
            $('#Sitemap a.nav-link').removeClass("active");
            $('#SiteSearch a.nav-link').removeClass("active");
            $scope.ShowSiteTab = true;
            $scope.ShowURLsTab = false;
            $scope.ShowSitemapTab = false;
            $scope.SiteSearchTab = false;
        }
        else if (type == 'URLs') {
            $('#Site a.nav-link').removeClass("active");
            $('#URLs a.nav-link').addClass("active");
            $('#Sitemap a.nav-link').removeClass("active");
            $('#SiteSearch a.nav-link').removeClass("active");
            $scope.ShowSiteTab = false;
            $scope.ShowURLsTab = true;
            $scope.ShowSitemapTab = false;
            $scope.SiteSearchTab = false;
        }
        else if (type == 'Sitemap') {
            $('#Site a.nav-link').removeClass("active");
            $('#URLs a.nav-link').removeClass("active");
            $('#Sitemap a.nav-link').addClass("active");
            $('#SiteSearch a.nav-link').removeClass("active");
            $scope.ShowSiteTab = false;
            $scope.ShowURLsTab = false;
            $scope.ShowSitemapTab = true;
            $scope.SiteSearchTab = false;
        }
        else if (type == 'SiteSearch') {
            $('#Site a.nav-link').removeClass("active");
            $('#URLs a.nav-link').removeClass("active");
            $('#Sitemap a.nav-link').removeClass("active");
            $('#SiteSearch a.nav-link').addClass("active");
            $scope.ShowSiteTab = false;
            $scope.ShowURLsTab = false;
            $scope.ShowSitemapTab = false;
            $scope.SiteSearchTab = true;
        }
    };

    $scope.Click_Save = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'seo_settings')) {
            $scope.ui.data.UrlRedirectSettings.Options.DeletedTabHandlingType = $scope.ui.data.DeletedPageHandling.Value == true ? "Do301RedirectToPortalHome" : "Do404Error";
            $scope.ui.data.SitemapSettings.Options.SitemapExcludePriority = $scope.ui.data.SitemapExcludePriority.Value;
            $scope.ui.data.SitemapSettings.Options.SitemapMinPriority = $scope.ui.data.SitemapMinPriority.Value;

            $.each($scope.ui.data.AllSynonymsGroups.Options, function (key, object) {
                if (typeof object.SynonymsTags != "string") {
                    var tags = new Array();
                    $.each(object.SynonymsTags, function (k, v) {
                        tags.push(v.text);
                    });
                    object.SynonymsTags = tags.join();
                }
            });
            var ITags = new Array();
            $.each($scope.Ignoretags, function (k, v) {
                ITags.push(v.text);
            });
            $scope.ui.data.IgnoreWords.Options.StopWords = ITags.join();
            $scope.ui.data.IgnoreWords.Options.StopWordsId = $scope.ui.data.IgnoreWord.Options.StopWordsId;
            var SynonymsGroups = jQuery.grep(JSON.parse(angular.toJson($scope.ui.data.AllSynonymsGroups.Options)), function (v) {
                return !(v.SynonymsGroupId == 0 && v.SynonymsTags == "")
            });

            var UpdateSearchData = {
                "BasicSettings": $scope.ui.data.BasicSettings.Options,
                "AllSynonymsGroups": SynonymsGroups,
                "IgnoreWord": $scope.ui.data.IgnoreWords.Options,
            };

            var requestSettings = {
                UpdateGeneralSettingsRequest: $scope.ui.data.UrlRedirectSettings.Options,
                UpdateRegexSettingsRequest: $scope.ui.data.RegexSettings.Options,
                SitemapSettingsRequest: $scope.ui.data.SitemapSettings.Options,
                SiteTitle: $scope.ui.data.SiteTitle.Value,
                HTMLPageHeader: $scope.ui.data.HTMLPageHeader.Value,
                UpdateSearchData: UpdateSearchData,
                Description: $scope.ui.data.Description.Value,
                Keywords: $scope.ui.data.Keywords.Value,
            };

            common.webApi.post('seo/UpdateSettings', '', requestSettings).then(function (Response) {
                if (Response.data.IsSuccess) {
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                }
                else {
                    window.parent.ShowNotification('[L:SEOSettings]', Response.data.Message, 'error');
                }
            });
        };
    };

    $scope.Click_TestUrl = function () {
        if ($scope.ui.data.PageToTest.Value != '' && $scope.ui.data.PageToTest.Value != "-1") {
            common.webApi.post('seo/TestUrl', 'pageId=' + $scope.ui.data.PageToTest.Value + '&queryString=' + $scope.ui.data.AddQueryString.Value + '&customPageName=' + $scope.ui.data.CustomPageName.Value, '').then(function (Response) {
                if (Response.data.IsSuccess) {
                    $scope.ui.data.ResultingURLs.Value = Response.data.Data.Urls[0];
                }
                else {
                    window.parent.ShowNotification('[L:SEOSettings]', Response.data.Message, 'error');
                }
            });
        }
    };

    $scope.Click_URLToTest = function () {
        if ($scope.ui.data.URLToTest.Value != '') {
            common.webApi.post('seo/TestUrlRewrite', 'uri=' + $scope.ui.data.URLToTest.Value, '').then(function (Response) {
                if (Response.data.IsSuccess) {
                    $scope.ui.data.UrlRewritingResult.Options = Response.data.Data;
                }
                else {
                    window.parent.ShowNotification('[L:SEOSettings]', Response.data.OperationMessages, 'error');
                }
            });
        }
    };

    $scope.Click_Clearcache = function () {
        common.webApi.post('seo/ResetCache', '', '').then(function (Response) {
            if (Response.data.IsSuccess) {
                window.parent.ShowNotification('[L:SEOSettings]', '[L:CacheClear]', 'success');
            }
            else {
                window.parent.ShowNotification('[L:SEOSettings]', Response.data.Message, 'error');
            }
        });
    };

    var BindSitemapPriorityList = function () {
        var priorityOptions = [];
        priorityOptions.push({ "value": 1, "label": "1" });
        priorityOptions.push({ "value": 0.9, "label": "0.9" });
        priorityOptions.push({ "value": 0.8, "label": "0.8" });
        priorityOptions.push({ "value": 0.7, "label": "0.7" });
        priorityOptions.push({ "value": 0.6, "label": "0.6" });
        priorityOptions.push({ "value": 0.5, "label": "0.5" });
        priorityOptions.push({ "value": 0.4, "label": "0.4" });
        priorityOptions.push({ "value": 0.3, "label": "0.3" });
        priorityOptions.push({ "value": 0.2, "label": "0.2" });
        priorityOptions.push({ "value": 0.1, "label": "0.1" });
        priorityOptions.push({ "value": 0, "label": "0" });

        $scope.ui.data.SitemapExcludePriority.Options = priorityOptions;
        $scope.ui.data.SitemapMinPriority.Options = priorityOptions;

        if ($scope.ui.data.SitemapExcludePriority.Value != null)
            $scope.ui.data.SitemapExcludePriority.Value = parseFloat($scope.ui.data.SitemapExcludePriority.Value);
        if ($scope.ui.data.SitemapMinPriority.Value != null)
            $scope.ui.data.SitemapMinPriority.Value = parseFloat($scope.ui.data.SitemapMinPriority.Value);
    }

    $scope.$watch('ui.data.URLToTest.Value', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            if (!newValue)
                $scope.ui.data.UrlRewritingResult.Options = [];
        }
    });

    $scope.Click_Add = function () {
        $scope.ui.data.AllSynonymsGroups.Options.push($.extend(true, new Object, $scope.ui.data.Working_SynonymsGroup.Options));
    };

    $scope.Click_ReIndex = function () {
        window.parent.swal({
            title: "",
            text: "[L:ReIndexConfirm]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
            cancelButtonText: "[L:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('seo/portalSearchReindex', '&PortalId=' + $scope.ui.data.IgnoreWord.Options.PortalId).then(function (data) {
                        if (data.data.IsSuccess) {
                            if (data.data.Message != null && data.data.Message == "")
                                window.parent.swal(data.data.Message);
                        }
                        else {
                            window.parent.document.callbacktype = type;
                            $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                        }
                    });
                }
            });
    };

    $scope.Click_CompactIndex = function () {
        window.parent.swal({
            title: "",
            text: "[L:CompactIndexConfirm]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
            cancelButtonText: "[L:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('seo/CompactSearchIndex', '', '').then(function (data) {
                        if (data.data.IsSuccess) {
                            if (data.data.Message != null && data.data.Message == "")
                                window.parent.swal(data.data.Message);
                        }
                        else {
                            window.parent.document.callbacktype = type;
                            $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                        }
                    });
                }
            });
    };

    $scope.Click_HostSearchIndex = function () {
        window.parent.swal({
            title: "",
            text: "[L:HostSearchIndexConfirm]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
            cancelButtonText: "[L:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('seo/HostSearchReindex', '', '').then(function (data) {
                        if (data.data.IsSuccess) {
                            if (data.data.Message != null && data.data.Message == "")
                                window.parent.swal(data.data.Message);
                        }
                        else {
                            window.parent.document.callbacktype = type;
                            $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                        }
                    });
                }
            });
    };
});