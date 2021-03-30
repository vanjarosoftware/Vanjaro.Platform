app.controller('common_controls_url', function ($scope, $attrs, $compile, $element, $http, $location, CommonSvc, SweetAlert, FileUploader) {

    var common = CommonSvc.getData($scope);

    $scope.UploadFile = new FileUploader();
    $scope.Attachment = new FileUploader();
    $scope.SelectedPage = -1;
    $scope.onInit = function () {
        $scope.ShowLink = true;
        $('.FileLink').addClass('ms-active');
        $('.PageLink').removeClass('ms-active');
        setTimeout(function () {
            $scope.AttachmentClick_FileUpoad('browse');
            $('[identifier="common_controls_url"]').find('.ms-col-sm-12.esc').remove();
            $('[identifier="common_controls_url"]').find('.url-add .choosefile').parent().css({ 'float': 'right', 'margin-bottom': '10px' });
            $('[identifier="common_controls_url"]').find('.url-add .choosefile').html('<span style="font-family:Arial, Helvetica, sans-serif;">[L:Upload]</span>');
            $('[identifier="common_controls_url"]').find('.url-add .choosefile').addClass('fas fa-plus');
        }, 10);
        if ($scope.ui.data.FilebrowserBrowseUrl.Value == "True" || $scope.ui.data.FilebrowserBrowseUrl.Value == "true")
            $scope.ui.data.FilebrowserBrowseUrl.Value = true;
        else
            $scope.ui.data.FilebrowserBrowseUrl.Value = false;
    };
    $scope.FileLink = function () {
        $scope.ShowLink = true;
        $('.FileLink').addClass('ms-active');
        $('.PageLink').removeClass('ms-active');
    };
    $scope.PageLink = function () {
        $scope.ShowLink = false;
        $('.FileLink').removeClass('ms-active');
        $('.PageLink').addClass('ms-active');
    };
    $scope.UploadFile.onBeforeUploadItem = function (item) {
        //$('[identifier="common_controls_url"]').find('[ng-show="UploadFile.queue.length"]').remove();
        item.formData[0].folder = $('[identifier="common_controls_url"]').find('.folders[style="font-weight: bold;"]').attr('id');
    };

    $scope.UploadFile.onCompleteAll = function () {
        if ($scope.UploadFile.progress == 100) {
            SweetAlert.swal('File uploaded successfully to ' + $('[identifier="common_controls_url"]').find('.folders[style="font-weight: bold;"]').text() + ' Folder');
            $scope.UploadFile.queue = [];
            $scope.Pipe_AttachmentPagging($scope.AttachmentTableState);
        }
    };

    $scope.UploadFile.onErrorItem = function (item, response, status, headers) {
        SweetAlert.swal(response.ExceptionMessage);
        $scope.UploadFile.progress = 0;
    };

    $scope.UpdateBrowser = function (sender) {
        if ($scope.Attachment.selectqueue != undefined && $scope.Attachment.selectqueue[0] != undefined && $scope.Attachment.selectqueue[0].fileurl != undefined && $scope.Attachment.selectqueue[0].fileurl != '') {
            var FuncNum = $scope.GetParameterByName('CKEditorFuncNum');
            var Opnr = window.top.opener;
            var fileurl = $scope.Attachment.selectqueue[0].fileurl;
            if ($scope.Attachment.selectqueue[0].fileid != undefined)
                fileurl = $scope.Attachment.selectqueue[0].fileid;
            var urltype = 0;
            if ($scope.ui.data.Types.Value == false)
                urltype = 1;
            common.webApi.get('upload/getlink', 'fileurl=' + fileurl + '&urltype=' + urltype).then(function (data) {
                Opnr.CKEDITOR.tools.callFunction(FuncNum, data.data, '');
                self.close();
            });
        }
        else if ($scope.Attachment.queue != undefined && $scope.Attachment.queue[0] != undefined && $scope.Attachment.queue[0].formData[0] != undefined && $scope.Attachment.queue[0].formData[0].fileDirectory != undefined && $scope.Attachment.queue[0].file != undefined) {
            var FuncNum = $scope.GetParameterByName('CKEditorFuncNum');
            var Opnr = window.top.opener;
            var fileurl = $scope.Attachment.queue[0].formData[0].fileDirectory + $scope.Attachment.queue[0].file.name;
            if ($scope.Attachment.queue[0].fileid != undefined)
                fileurl = $scope.Attachment.queue[0].fileid;
            var urltype = 0;
            if ($scope.ui.data.Types.Value == false)
                urltype = 1;
            common.webApi.get('upload/getlink', 'fileurl=' + fileurl + '&urltype=' + urltype).then(function (data) {
                Opnr.CKEDITOR.tools.callFunction(FuncNum, data.data, '');
                self.close();
            });
        }
        else
            window.close();
    };

    $scope.UpdatePageLink = function (sender) {

        if (parseInt($scope.SelectedPage) == -1) {
            CommonSvc.SweetAlert.swal("[L:SelectPageError]");
        }
        else {

            if ($scope.SelectedPage != undefined || $scope.SelectedPage != '') {
                var FuncNum = $scope.GetParameterByName('CKEditorFuncNum');
                var Opnr = window.top.opener;
                var urltype = 0;
                if ($scope.ui.data.TypesPages.Value == false)
                    urltype = 1;
                common.webApi.get('upload/getlink', 'fileurl=pages' + $scope.SelectedPage + '&urltype=' + urltype + '&language=' + $scope.ui.data.Languages.Value + '&anchorlist=' + $scope.ui.data.PageAnchors.Value).then(function (data) {
                    Opnr.CKEDITOR.tools.callFunction(FuncNum, data.data, '');
                    self.close();
                });
            }
        }
    };

    $scope.CancelBrowser = function () {
        window.close();
    };

    $scope.GetParameterByName = function (name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    };

    $scope.Click_GetSubPages = function (event, page) {
        var $this = $(event.currentTarget);
        if ($this.parent().find('> .rootfolder li').length <= 0)
            return;
        $this.toggleClass('fa-caret-right fa-caret-down');
        if ($this.hasClass('ms-glyphicon-triangle-bottom'))
            $this.parent().find('> .rootfolder').addClass('show');
        else
            $this.parent().find('> .rootfolder').removeClass('show');
    };

    $scope.Click_GetPageId = function (event) {
        var $this = $(event.currentTarget);
        $scope.SelectedPage = $($this).attr('id');
        $scope.SelectedPage = $scope.SelectedPage.replace('pages', '');
        if ($scope.SelectedPage != undefined) {
            $.each($('[identifier="common_controls_url"]').find('[data-name="DnnPagesDiv"]').find('.folders'), function (key, value) {
                $(value).css('font-weight', '');
                if ($(value).attr('id') == ("pages" + $scope.SelectedPage))
                    $(value).css('font-weight', 'bold');
            });
            common.webApi.get('~PageLink/GetPageAnchors', 'pageid=' + $scope.SelectedPage).then(function (data) {
                if (data) {
                    $scope.ui.data.PageAnchors.Options = data.data;
                    $scope.ui.data.PageAnchors.Value = "0";
                }
            });
        }
    };
});