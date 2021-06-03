app.controller('settings_video', function ($scope, FileUploader, $attrs, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.UploadFile = new FileUploader();
    $scope.FileAttachments = new FileUploader();

    $scope.onInit = function () {
        window.parent.window.VJIsSaveCall = true;
        $('.uiengine-wrapper a[data-target="#!/admin"]').addClass("active");
        $('.uiengine-wrapper a[data-target="#!/videoonline"]').removeClass("active");
        setTimeout(function () {
            $scope.FileAttachmentsClick_FileUpoad('browse');
            $('[identifier="settings_video"]').find('.col-sm-12.esc').remove();
            $('[identifier="settings_video"]').find('.url-add .choosefile').parent().css({ 'float': 'right', 'margin-bottom': '10px' });
            $('[identifier="settings_video"]').find('.url-add .choosefile').html('<span style="font-family:Arial, Helvetica, sans-serif;">[L:Upload]</span>');
            $('[identifier="settings_video"]').find('.url-add .choosefile').addClass('fa fa-plus');
        }, 10);
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').on("click", function (e) {
            window.parent.window.VJIsSaveCall = false;
            window.parent.VjEditor.runCommand("save");
        });
        $scope.BindFolderEvents();
    };

    $scope.BindFolderEvents = function (fo) {
        if (fo == undefined) {
            setTimeout(function () {
                $('.fa-folder').on('click',
                    function () {
                        if ($('#' + $(this).next().attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                            $($('#' + $(this).next().attr('id')).parent().prevAll('span:first')).click();
                        };
                    });

                $('.folders').on('click',
                    function () {
                        if ($('#' + $(this).attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                            $($('#' + $(this).attr('id')).parent().prevAll('span:first')).click();
                        };
                    });
            }, 100);
        }
        else {
            var nestedli = $('#folders' + fo).parent().next('ul').find('li');
            $.each(nestedli, function (k, v) {
                $(v).find('.fa-folder').on('click',
                    function () {
                        if ($('#' + $(this).next().attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                            $($('#' + $(this).next().attr('id')).parent().prevAll('span:first')).click();
                        };
                    });

                $(v).find('.folders').on('click',
                    function () {
                        if ($('#' + $(this).attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                            $($('#' + $(this).attr('id')).parent().prevAll('span:first')).click();
                        };
                    });
            });
        }
    };

    $scope.UploadFile.onBeforeUploadItem = function (item) {
        //$('[identifier="setting_assets"]').find('[ng-show="UploadFile.queue.length"]').remove();
        item.formData[0].folder = $('[identifier="settings_video"]').find('.folders[style="font-weight: bold;"]').attr('id');
    };

    $scope.UploadFile.onCompleteAll = function () {
        if ($scope.UploadFile.progress == 100) {
            if ($scope.UploadFile.queue[0].file.name.includes("not uploaded")) {
                window.parent.ShowNotification('Error', "[L:CheckSiteLogs]", 'error');
            }
            else {
                window.parent.ShowNotification('', '[L:FileUploadedSuccessfullyTo]' + $('[identifier="settings_video"]').find('.folders[style="font-weight: bold;"]').text() + ' [L:Folder]', 'success');
                $scope.UploadFile.queue = [];
                $scope.Pipe_FileAttachmentsPagging($scope.FileAttachmentsTableState);
            }
        }
    };

    $scope.UploadFile.onErrorItem = function (item, response, status, headers) {
        window.parent.ShowNotification('', response.ExceptionMessage, 'error');
        $scope.UploadFile.progress = 0;
    };

    $scope.$watch('FileAttachments.selectqueue', function (newValue, oldValue) {
        if (newValue != undefined && newValue.length > 0) {
            var fileid = newValue[0].fileid;
            $scope.GetURL(fileid);
            $scope.FileAttachments.selectqueue = [];
            $scope.FileAttachmentsClick_FileUpoad('browse');
        }
    });

    $scope.$watch('FileAttachmentsFiles', function (newValue, oldValue) {
        if (newValue != undefined && newValue != null && newValue.length > 0) {
            setTimeout(function () {
                $('[fid]').on("dblclick", function (e) {
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                });
            }, 100);
        }
    });

    $scope.FileAttachments.onErrorItem = function (item, response, status, headers) {
        SweetAlert.swal(response.ExceptionMessage);
        $scope.FileAttachments.progress = 0;
    };

    $scope.GetURL = function (fileid) {
        window.parent.window.VJIsSaveCall = true;
        common.webApi.post('Upload/GetUrl', 'fileid=' + fileid).then(function (data) {
            if (data.data.Status == 'Success') {
                var Link = data.data.Url;
                var target = window.parent.document.vj_video_target;
                if (target != undefined) {
                    target.set('provider', 'so');
                    target.attributes.videoId = fileid;
                    target.addStyle({ 'height': '400px' });
                    target.set('src', Link);
                }
            }
            else {
                window.parent.ShowNotification('Error', data.data.Status, 'error');
            }
        });
    };
});