app.controller('setting_choosetemplate', function ($scope, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.CreatePortalRequest = {
        SiteName: '',
        SiteTemplate: '',
        SiteTemplateHash: ''
    };

    $scope.VjDefaultPath = window.parent.parent.VjDefaultPath + 'loading.svg';

    $scope.onInit = function () {
        $(window.parent.document.body).find('.modal-dialog:last').parent().removeAttr('data-edit');
        $('.defaultdesign').show();
        $(window.parent.document.body).find('.modal-dialog:last').find('.btn-close').before($('.defaultdesign-hidden'));
        var $FrameUrl = $('uiengine #FrameUrl');
        $FrameUrl.attr('src', $scope.ui.data.LibraryMidUrl.Value).load(function () { $(this).prev('.loader').hide(); });
    };

    $scope.Click_ContinueDefault = function () {
        document.cookie = "vj_sbtemplate=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/";
        window.location.hash = '#!/add';
        $scope.HideModal();
    };

    $scope.HideModal = function () {
        $(window.parent.document.body).find('.modal-dialog:last').parent().removeAttr('data-edit');
        $(window.parent.document.body).find('.modal-dialog:last>[ng-controller="Controller"]').removeClass('container-fluid').addClass('container');
    };

    $scope.CreateSite = function () {
        if ($scope.CreatePortalRequest.SiteTemplate.length > 0) {
            var storagedata = {
                Template: $scope.CreatePortalRequest.SiteName,
                TemplatePath: $scope.CreatePortalRequest.SiteTemplate,
                TemplateHash: $scope.CreatePortalRequest.SiteTemplateHash
            };
            var date = new Date();
            date.setTime(date.getTime() + (60 * 1000));
            var expires = "; expires=" + date.toUTCString();
            document.cookie = "vj_sbtemplate=" + JSON.stringify(storagedata) + expires + "; path=/";
        }
        window.location.hash = '#!/add';
        $scope.HideModal();
    };

    window.addEventListener('message', event => {
        if (event.origin.startsWith($scope.ui.data.LibraryUrl.Value) && event.data != undefined) {
            if (typeof event.data.path != 'undefined') {
                if (!event.origin.startsWith($scope.ui.data.LibraryUrl.Value)) {
                    $scope.CreatePortalRequest.SiteTemplate = $scope.ui.data.LibraryUrl.Value + event.data.path;
                    $scope.CreatePortalRequest.SiteName = event.data.name;
                    $scope.CreatePortalRequest.SiteTemplateHash = event.data.hash;
                }
                else {
                    $scope.CreatePortalRequest.SiteTemplate = event.data.path;
                    $scope.CreatePortalRequest.SiteName = event.data.name;
                    $scope.CreatePortalRequest.SiteTemplateHash = event.data.hash;
                }
                $scope.CreateSite();
            }
        }
    });
});