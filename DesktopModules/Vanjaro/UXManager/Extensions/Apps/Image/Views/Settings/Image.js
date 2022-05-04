app.controller('settings_image', function ($scope, FileUploader, $attrs, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.UploadFile = new FileUploader();
    $scope.FileAttachments = new FileUploader();
    $scope.dblclickFired = false;
    $scope.targetParent;

    $scope.onInit = function () {
        window.parent.window.VJIsSaveCall = true;
        $('.uiengine-wrapper a[data-target="#!/admin"]').addClass("active");
        $('.uiengine-wrapper a[data-target="#!/imageonline"]').removeClass("active");
        setTimeout(function () {
            $scope.FileAttachmentsClick_FileUpoad('browse');
            $('[identifier="settings_image"]').find('.col-sm-12.esc').remove();
            $('[identifier="settings_image"]').find('.url-add .choosefile').parent().css({ 'float': 'right', 'margin-bottom': '10px' });
            $('[identifier="settings_image"]').find('.url-add .choosefile').html('<span style="font-family:Arial, Helvetica, sans-serif;">[L:Upload]</span>');
            $('[identifier="settings_image"]').find('.url-add .choosefile').addClass('fas fa-plus');
        }, 10);
        if ($(window.parent.document.body).find('[data-bs-dismiss="modal"]').length <= 0)
            $(window.parent.document.body).find('.btn-close').attr('data-bs-dismiss', 'modal');
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').on("click", function (e) {
            window.parent.window.VJIsSaveCall = false;
            window.parent.VjEditor.runCommand("save");
        });
        $scope.BindFolderEvents();

        if ($scope.ui.data.Folders.Value != undefined && $scope.ui.data.Folders.Value != null && parseInt($scope.ui.data.Folders.Value) > 0) {
            setTimeout(function () {               
                $('#folders' + $scope.ui.data.Folders.Value).trigger('click');
            }, 200);
        }
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

    $scope.UploadFile.onCompleteAll = function () {
        if ($scope.UploadFile.progress == 100) {
            if ($scope.UploadFile.queue[0].file.name.includes("not uploaded")) {
                window.parent.ShowNotification('Error', "[L:CheckSiteLogs]", 'error');
            }
            else {
                window.parent.ShowNotification('', '[L:FileUploadedSuccessfullyTo]' + $('[identifier="settings_image"]').find('.folders[style="font-weight: bold;"]').text() + ' [L:Folder]', 'success');
                $scope.UploadFile.queue = [];
                $scope.Pipe_FileAttachmentsPagging($scope.FileAttachmentsTableState);
            }
        }
    };

    $scope.$watch('FileAttachmentsFiles', function (newValue, oldValue) {
        if (newValue != undefined && newValue != null && newValue.length > 0) {
            setTimeout(function () {
                $('[fid]').on("dblclick", function (e) {
                    $scope.dblclickFired = true;
                });
            }, 100);
        }
    });

    $scope.$watch('FileAttachments.selectqueue', function (newValue, oldValue) {
        if (newValue != undefined && newValue.length > 0) {
            var fileid = newValue[0].fileid;
            $scope.GetURL(fileid);
            $scope.FileAttachments.selectqueue = [];
            $scope.FileAttachmentsClick_FileUpoad('browse');
        }
    });

    $scope.UploadFile.onBeforeUploadItem = function (item) {
        //$('[identifier="setting_assets"]').find('[ng-show="UploadFile.queue.length"]').remove();
        item.formData[0].folder = $('[identifier="settings_image"]').find('.folders[style="font-weight: bold;"]').attr('id');
    };

    $scope.UploadFile.onErrorItem = function (item, response, status, headers) {
        SweetAlert.swal(response.ExceptionMessage);
        $scope.UploadFile.progress = 0;
    };

    $scope.GetURL = function (fileid) {
        window.parent.window.VJIsSaveCall = true;
        var absolutelink = false;

        if (window.parent.GetParameterByName('absolutelink', location.href) == "true")
            absolutelink = true;

        common.webApi.post('Upload/GetUrl', 'fileid=' + fileid + '&absolutelink=' + absolutelink.toString()).then(function (data) {
            if (data.data.Status == 'Success') {
                var Link = data.data.Url;

                var target = window.parent.document.vj_image_target;

                var url = Link;

                //checked background image has not changed
                if (typeof target != 'undefined') {

                    if (target.attributes.optimize) {

                        target.set('src', url);

                        if ($scope.targetParent == undefined)
                            $scope.targetParent = target.parent();

                        if (data.data.Urls.length)
                            parent.ChangeToWebp($scope.targetParent, data.data.Urls);
                        else {
                            target.removeStyle('max-width');
                            $($scope.targetParent.components().models).each(function (index, component) {
                                if (component.getName() == "Source")
                                    component.remove();
                            });
                        }
                    }
                    else {

                        if (data.data.Urls.length)
                            url = data.data.Urls.find(v => v.Type == 'webp').Url;

                        target.set('src', url);
                    }
                }
                else {
                    var background = window.parent.VjEditor.StyleManager.getProperty('background_&_shadow', 'background');

                    if (data.data.Urls.length)
                        url = data.data.Urls.find(v => v.Type == 'webp').Url;

                    background.getSelectedLayer().prop.attributes.properties.models.find(m => m.id == 'background-image-sub').setValue(url);
                }
            }
            else {
                window.parent.ShowNotification('Error', data.data.Status, 'error');
            }
            if ($scope.dblclickFired)
                $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
        });
    };
});