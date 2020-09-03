app.controller('setting_setting', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert, FileUploader) {
    var common = CommonSvc.getData($scope);
    $scope.LogoFile = new FileUploader();
    $scope.FavIcon = new FileUploader();
    $scope.SocialSharingLogo = new FileUploader();
    $scope.HomeScreenIcon = new FileUploader();
    $scope.LogoFileDetails = [];
    $scope.FavIconDetails = [];
    $scope.SocialSharingLogoDetails = [];
    $scope.ShowLogo_TitleTab = false;
    $scope.ShowIconTab = false;
    $scope.Show_Tab = false;
    $scope.ShowFileManager = false;
    $scope.LogoFileUpload = new FileUploader();
    $scope.FavIconUpload = new FileUploader();
    $scope.SocialSharingLogoUpload = new FileUploader();
    $scope.HomeScreenIconUpload = new FileUploader();
    $scope.ActiveUpload = '';

    $scope.onInit = function () {
        if ($scope.ui.data.IconFolders.Value != '')
            $scope.ui.data.IconFolders.Value = $scope.ui.data.SiteSettings.Options.IconSet.toString();
        if ($scope.ui.data.FavIcon.Value == "") {
            $scope.ui.data.FavIcon.Value = $scope.ui.data.DefaultFavIcon.Value;
        }
        $("#defaultModal .modal-title", parent.document).html('[LS:LogoAndTitle]');
        $scope.Click_ShowTab('Logo_Title');

        $.each($('input[type="file"]'), function (key, element) {
            if ($(element).attr('uploader') != undefined) {
                var data = {
                    ControlName: $(element).attr('uploader'),
                    UserID: $scope.uid
                }
                setTimeout(function () {
                    $.extend($scope[data.ControlName].formData[0], data);
                }, 3);
            }
        });

        $('.file-manager-12 .buttons').remove();
        $(".buttons").hide();

        $('#LogoFileShowFileBrowseSpan,#FavIconShowFileBrowseSpan,#SocialSharingLogoShowFileBrowseSpan,#HomeScreenIconShowFileBrowseSpan').click(function () {
            $('.my-drop-zone .buttons').removeAttr("style");
            $(".mainLogo,#logomanager").removeClass('ng-hide');
            $(".selectLogo").addClass('ng-hide');
            $(".buttons").hide();
            $scope.ShowFileManager = false;
        });

        $(document).keyup(function (e) {
            var code = e.keyCode || e.which;
            if (code == 27) {
                e.preventDefault();
                $scope.ShowFileManager = false;
                $(".mainLogo,#logomanager").removeClass('ng-hide');
                $('.uploadinput-button span.btn').hide();
            }
        });

        $('.logoBox').click(function () {
            $scope.ShowFileManager = true;
            $(".selectLogo").removeClass('ng-hide');
            $(".mainLogo,#logomanager").addClass('ng-hide');
            $(".buttons").hide();
            $('.buttons [ng-show="LogoFileControlEnable(\'browse\')"]').click();
            $('.uploadinput-button span.btn').show();
            $scope.AssignClickToFolders();
        })
        $('.FaviconBox').click(function () {
            $scope.ShowFileManager = true;
            $(".selectLogo").removeClass('ng-hide');
            $(".mainLogo,#logomanager").addClass('ng-hide');
            $(".buttons").hide();
            $('.buttons [ng-show="FavIconControlEnable(\'browse\')"]').click();
            $('.uploadinput-button span.btn').show();
            $scope.AssignClickToFolders();
        })
        $('.SocialSharingBox').click(function () {
            $scope.ShowFileManager = true;
            $(".selectLogo").removeClass('ng-hide');
            $(".mainLogo,#logomanager").addClass('ng-hide');
            $(".buttons").hide();
            $('.buttons [ng-show="SocialSharingLogoControlEnable(\'browse\')"]').click();
            $('.uploadinput-button span.btn').show();
            $scope.AssignClickToFolders();
        })
        $('.HomeScreenBox').click(function () {
            $scope.ShowFileManager = true;
            $(".selectLogo").removeClass('ng-hide');
            $(".mainLogo,#logomanager").addClass('ng-hide');
            $(".buttons").hide();
            $('.buttons [ng-show="HomeScreenIconControlEnable(\'browse\')"]').click();
            $('.uploadinput-button span.btn').show();
            $scope.AssignClickToFolders();
        })

        setTimeout(function () {
            $('[ng-click="LogoFileClick_FileUpoad(\'browse\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "LogoFile";
                $('.LogoFilefoldersdiv .pagedir .folders:first').click();
                $('[identifier="setting_setting"]').find('.col-sm-12.esc').remove();
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').parent().css({ 'float': 'right', 'margin-bottom': '10px' });
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').html('<span style="font-family:Arial, Helvetica, sans-serif;">[L:Upload]</span>');
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').addClass('fas fa-plus');
                $scope.ActiveUpload = "Uploader_One";
                setTimeout(function () {
                    $('.fa-folder').on('click',
                        function () {
                            if ($('.file-manager-12 .LogoFilefoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .LogoFilefoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first')).click();
                            };
                        });

                    $('.folders').on('click',
                        function () {
                            if ($('.file-manager-12 .LogoFilefoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .LogoFilefoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first')).click();
                            };
                        });
                }, 100);
            });

            $('[ng-click="LogoFileClick_FileUpoad(\'upload\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "LogoFile";
                $scope.LogoFile.url = $scope.LogoFile.url + '&uid=' + $scope.ui.data.Uid.Value;
            });

            $('[ng-click="FavIconClick_FileUpoad(\'browse\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "FavIcon";
                $('.FavIconfoldersdiv .pagedir .folders:first').click();
                $('[identifier="setting_setting"]').find('.col-sm-12.esc').remove();
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').parent().css({ 'float': 'right', 'margin-bottom': '10px' });
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').html('<span style="font-family:Arial, Helvetica, sans-serif;">[L:Upload]</span>');
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').addClass('fas fa-plus');
                $scope.ActiveUpload = "Uploader_Two";
                setTimeout(function () {
                    $('.fa-folder').on('click',
                        function () {
                            if ($('.file-manager-12 .FavIconfoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .FavIconfoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first')).click();
                            };
                        });

                    $('.folders').on('click',
                        function () {
                            if ($('.file-manager-12 .FavIconfoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .FavIconfoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first')).click();
                            };
                        });
                }, 100);
            });
            $('[ng-click="FavIconClick_FileUpoad(\'upload\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "FavIcon";
                $scope.FavIcon.url = $scope.FavIcon.url + '&uid=' + $scope.ui.data.Uid.Value;
            });

            $('[ng-click="SocialSharingLogoClick_FileUpoad(\'browse\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "SocialSharingLogo";
                $('.SocialSharingLogofoldersdiv .pagedir .folders:first').click();
                $('[identifier="setting_setting"]').find('.col-sm-12.esc').remove();
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').parent().css({ 'float': 'right', 'margin-bottom': '10px' });
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').html('<span style="font-family:Arial, Helvetica, sans-serif;">[L:Upload]</span>');
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').addClass('fas fa-plus');
                $scope.ActiveUpload = "Uploader_Three";
                setTimeout(function () {
                    $('.fa-folder').on('click',
                        function () {
                            if ($('.file-manager-12 .SocialSharingLogofoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .SocialSharingLogofoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first')).click();
                            };
                        });

                    $('.folders').on('click',
                        function () {
                            if ($('.file-manager-12 .SocialSharingLogofoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .SocialSharingLogofoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first')).click();
                            };
                        });
                }, 100);
            });
            $('[ng-click="SocialSharingLogoClick_FileUpoad(\'upload\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "SocialSharingLogo";
                $scope.SocialSharingLogo.url = $scope.SocialSharingLogo.url + '&uid=' + $scope.ui.data.Uid.Value;
            });


            $('[ng-click="HomeScreenIconClick_FileUpoad(\'browse\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "SocialSharingLogo";
                $('.HomeScreenIconfoldersdiv .pagedir .folders:first').click();
                $('[identifier="setting_setting"]').find('.col-sm-12.esc').remove();
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').parent().css({ 'float': 'right', 'margin-bottom': '10px' });
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').html('<span style="font-family:Arial, Helvetica, sans-serif;">[L:Upload]</span>');
                $('[identifier="setting_setting"]').find('.uploadbtn .choosefile').addClass('fas fa-plus');
                $scope.ActiveUpload = "Uploader_Four";
                setTimeout(function () {
                    $('.fa-folder').on('click',
                        function () {
                            if ($('.file-manager-12 .HomeScreenIconfoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .HomeScreenIconfoldersdiv').find('#' + $(this).next().attr('id')).parent().prevAll('span:first')).click();
                            };
                        });

                    $('.folders').on('click',
                        function () {
                            if ($('.file-manager-12 .HomeScreenIconfoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
                                $($('.file-manager-12 .HomeScreenIconfoldersdiv').find('#' + $(this).attr('id')).parent().prevAll('span:first')).click();
                            };
                        });
                }, 100);
            });
            $('[ng-click="HomeScreenIconClick_FileUpoad(\'upload\')"]').on("click", function () {
                $scope.ui.data.Uid.Value = "HomeScreenIcon";
                $scope.HomeScreenIcon.url = $scope.HomeScreenIcon.url + '&uid=' + $scope.ui.data.Uid.Value;
            });
        }, 100);
    };

    $scope.AssignClickToFolders = function () {
        //setTimeout(function () {
        //    $('.fa-folder').on('click',
        //        function () {
        //            if ($('#' + $(this).next().attr('id')).parent().prevAll('span:first').hasClass('fa-caret-right')) {
        //                $($('#' + $(this).next().attr('id')).parent().prevAll('span:first')).click();
        //            };
        //        });
        //}, 100);
    };

    $scope.LogoFileUpload.onBeforeUploadItem = function (item) {
        item.formData[0].logotype = $scope.ui.data.Uid.Value;
    };

    $scope.SocialSharingLogoUpload.onBeforeUploadItem = function (item) {
        item.formData[0].logotype = $scope.ui.data.Uid.Value;
    };

    $scope.HomeScreenIconUpload.onBeforeUploadItem = function (item) {
        item.formData[0].logotype = $scope.ui.data.Uid.Value;
    };

    $scope.FavIconUpload.onBeforeUploadItem = function (item) {
        item.formData[0].logotype = $scope.ui.data.Uid.Value;
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'Logo_Title') {
            $('#Logo_Title a.nav-link').addClass("active");
            $('#Icon a.nav-link').removeClass("active");
            $scope.ShowLogo_TitleTab = true;
            $scope.ShowIconTab = false;
        }
        else if (type == 'Icon') {
            $('#Logo_Title a.nav-link').removeClass("active");
            $('#Icon a.nav-link').addClass("active");
            $scope.ShowLogo_TitleTab = false;
            $scope.ShowIconTab = true;
        }
    };

    $scope.Click_Save = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            if ($scope.ui.data.LogoFile.Options != null) {
                $scope.ui.data.SiteSettings.Options.LogoFile.fileId = $scope.ui.data.LogoFile.Options.fileId;
                $scope.ui.data.SiteSettings.Options.LogoFile.fileName = $scope.ui.data.LogoFile.Options.fileName;
            }

            if ($scope.ui.data.FavIcon.Options != null) {
                $scope.ui.data.SiteSettings.Options.FavIcon.fileId = $scope.ui.data.FavIcon.Options.fileId;
                $scope.ui.data.SiteSettings.Options.FavIcon.fileName = $scope.ui.data.FavIcon.Options.fileName;
            }
            $scope.ui.data.SiteSettings.Options.IconSet = $scope.ui.data.IconFolders.Value;
            common.webApi.post('setting/updatePortalSettings', 'FileId=' + $scope.ui.data.SocialSharingLogo.Options.fileId.toString() + '&HomeIcon=' + $scope.ui.data.HomeScreenIcon.Options.fileId.toString(), $scope.ui.data.SiteSettings.Options).success(function (data) {
                if (data.Message != null && data.Message.length > 0) {
                    window.parent.swal(data.Message);
                }
                else {
                    window.parent.document.callbacktype = type;
                    if (parent.VjEditor != null && parent.VjEditor.getComponents() != null && parent.VjEditor.getComponents().models != null && typeof parent.VjEditor.getComponents().models != 'undefined') {
                        $.each(parent.VjEditor.getComponents().models, function (key, value) {
                            $scope.BindLogo(value, data.Data);
                        });
                    }
                    $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                }
            });
        }
    };

    $scope.BindLogo = function (value, path) {
        if (value.attributes != null && value.attributes.name != null && value.attributes.name == "Logo") {
            //value.addStyle({ 'width': '200px', 'height': '200px' });
            //var htmldom = $.parseHTML(value.attributes.content);
            //$(htmldom).find('img').attr('src', path.FileUrl);
            //value.attributes.content = htmldom[0].outerHTML;
            $(value.view.$el).find('img').attr('src', path.FileUrl);
        }
        if (value.attributes != null && value.attributes.components != null && value.attributes.components.models != null) {
            $.each(value.attributes.components.models, function (k, mod) {
                $scope.BindLogo(mod, path);
            });
        }
    };

    $scope.LogoFileUpload.onCompleteAll = function () {
        if ($scope.LogoFileUpload.progress == 100) {
            var FileIds = [];
            $.each($scope.LogoFileUpload.queue, function (key, value) {
                if (value.file.name != "File/s not uploaded successfully!")
                    FileIds.push(parseInt(value.file.name.split('fileid')[1]));
            });
            if (FileIds.length > 0) {
                common.webApi.get('Upload/GetMultipleFileDetails', 'fileids=' + FileIds.join()).success(function (response) {
                    $.each(response, function (key, value) {
                        if (value.Name != null) {
                            var Title = (value.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.LogoFile.Value = value.FileUrl;
                        var data = {
                            "fileName": value.Name,
                            "fileId": value.FileId,
                            "Title": Title,
                            "KBSize": value.Size,
                            "Size": (value.Size / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.LogoFile.Options = [];
                        $scope.ui.data.LogoFile.Options = data;
                    });
                    $scope.LogoFileUpload.queue = [];
                });
            }
        }
    };

    $scope.$watch('LogoFile.selectqueue', function (newValue, oldValue) {
        if (newValue != undefined && newValue.length > 0) {
            $.each(newValue, function (key, value) {
                var FileId = parseInt(value.fileid);
                if (FileId > 0) {
                    common.webApi.get('Upload/GetFile', 'fileid=' + FileId).success(function (response) {
                        if (response.Name != null) {
                            var Title = (response.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.LogoFile.Value = response.FileUrl;
                        var data = {
                            "fileName": response.Name,
                            "fileId": FileId,
                            "Title": Title,
                            "KBSize": 0,
                            "Size": (0 / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.LogoFile.Options = [];
                        $scope.ui.data.LogoFile.Options = data;
                    });
                    $scope.LogoFile.selectqueue = [];
                    $scope.ShowFileManager = false;
                }
            });
        }
        $('.my-drop-zone .buttons').removeAttr("style");
    });

    $scope.FavIconUpload.onCompleteAll = function () {
        if ($scope.FavIconUpload.progress == 100) {
            var FileIds = [];
            $.each($scope.FavIconUpload.queue, function (key, value) {
                if (value.file.name != "File/s not uploaded successfully!")
                    FileIds.push(parseInt(value.file.name.split('fileid')[1]));
            });
            if (FileIds.length > 0) {
                common.webApi.get('Upload/GetMultipleFileDetails', 'fileids=' + FileIds.join()).success(function (response) {
                    $.each(response, function (key, value) {
                        if (value.Name != null) {
                            var Title = (value.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.FavIcon.Value = value.FileUrl;
                        var data = {
                            "fileName": value.Name,
                            "fileId": value.FileId,
                            "Title": Title,
                            "KBSize": value.Size,
                            "Size": (value.Size / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.FavIcon.Options = [];
                        $scope.ui.data.FavIcon.Options = data;
                    });
                    $scope.FavIconUpload.queue = [];
                });
            }
        }
    };

    $scope.$watch('FavIcon.selectqueue', function (newValue, oldValue) {
        if (newValue != undefined && newValue.length > 0) {
            $.each(newValue, function (key, value) {
                var FileId = parseInt(value.fileid);
                if (FileId > 0) {
                    common.webApi.get('Upload/GetFile', 'fileid=' + FileId).success(function (response) {
                        if (response.Name != null) {
                            var Title = (response.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.FavIcon.Value = response.FileUrl;
                        var data = {
                            "fileName": response.Name,
                            "fileId": FileId,
                            "Title": Title,
                            "KBSize": 0,
                            "Size": (0 / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.FavIcon.Options = [];
                        $scope.ui.data.FavIcon.Options = data;
                    });
                    $scope.FavIcon.selectqueue = [];
                    $scope.ShowFileManager = false;
                }
            });
        }
        $('.my-drop-zone .buttons').removeAttr("style");
    });

    $scope.SocialSharingLogoUpload.onCompleteAll = function () {
        if ($scope.SocialSharingLogoUpload.progress == 100) {
            var FileIds = [];
            $.each($scope.SocialSharingLogoUpload.queue, function (key, value) {
                if (value.file.name != "File/s not uploaded successfully!")
                    FileIds.push(parseInt(value.file.name.split('fileid')[1]));
            });
            if (FileIds.length > 0) {
                common.webApi.get('Upload/GetMultipleFileDetails', 'fileids=' + FileIds.join()).success(function (response) {
                    $.each(response, function (key, value) {
                        if (value.Name != null) {
                            var Title = (value.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.SocialSharingLogo.Value = value.FileUrl;
                        var data = {
                            "fileName": value.Name,
                            "fileId": value.FileId,
                            "Title": Title,
                            "KBSize": value.Size,
                            "Size": (value.Size / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.SocialSharingLogo.Options = [];
                        $scope.ui.data.SocialSharingLogo.Options = data;
                    });
                    $scope.SocialSharingLogoUpload.queue = [];
                });
            }
        }
    };

    $scope.$watch('SocialSharingLogo.selectqueue', function (newValue, oldValue) {
        if (newValue != undefined && newValue.length > 0) {
            $.each(newValue, function (key, value) {
                var FileId = parseInt(value.fileid);
                if (FileId > 0) {
                    common.webApi.get('Upload/GetFile', 'fileid=' + FileId).success(function (response) {
                        if (response.Name != null) {
                            var Title = (response.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.SocialSharingLogo.Value = response.FileUrl;
                        var data = {
                            "fileName": response.Name,
                            "fileId": FileId,
                            "Title": Title,
                            "KBSize": 0,
                            "Size": (0 / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.SocialSharingLogo.Options = [];
                        $scope.ui.data.SocialSharingLogo.Options = data;
                    });
                    $scope.SocialSharingLogo.selectqueue = [];
                    $scope.ShowFileManager = false;
                }
            });
        }
        $('.my-drop-zone .buttons').removeAttr("style");
    });

    $scope.HomeScreenIconUpload.onCompleteAll = function () {
        //$('#ShowLogo_BasicTab').hide();
        if ($scope.HomeScreenIconUpload.progress == 100) {
            var FileIds = [];
            $.each($scope.HomeScreenIconUpload.queue, function (key, value) {
                if (value.file.name != "File/s not uploaded successfully!")
                    FileIds.push(parseInt(value.file.name.split('fileid')[1]));
            });
            if (FileIds.length > 0) {
                common.webApi.get('Upload/GetMultipleFileDetails', 'fileids=' + FileIds.join()).success(function (response) {
                    $.each(response, function (key, value) {
                        if (value.Name != null) {
                            var Title = (value.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.HomeScreenIcon.Value = value.FileUrl;
                        var data = {
                            "fileName": value.Name,
                            "fileId": value.FileId,
                            "Title": Title,
                            "KBSize": value.Size,
                            "Size": (value.Size / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.HomeScreenIcon.Options = [];
                        $scope.ui.data.HomeScreenIcon.Options = data;
                    });
                    $scope.HomeScreenIconUpload.queue = [];
                });
            }
        }
    };

    $scope.$watch('HomeScreenIcon.selectqueue', function (newValue, oldValue) {
        //$('#ShowLogo_BasicTab').hide();
        if (newValue != undefined && newValue.length > 0) {
            $.each(newValue, function (key, value) {
                var FileId = parseInt(value.fileid);
                if (FileId > 0) {
                    common.webApi.get('Upload/GetFile', 'fileid=' + FileId).success(function (response) {
                        if (response.Name != null) {
                            var Title = (response.Name.split('/').pop()).split('.')[0];
                        }
                        $scope.ui.data.HomeScreenIcon.Value = response.FileUrl;
                        var data = {
                            "fileName": response.Name,
                            "fileId": FileId,
                            "Title": Title,
                            "KBSize": 0,
                            "Size": (0 / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.HomeScreenIcon.Options = [];
                        $scope.ui.data.HomeScreenIcon.Options = data;
                    });
                    $scope.HomeScreenIcon.selectqueue = [];
                    $scope.ShowFileManager = false;
                }
            });
        }
        $('.my-drop-zone .buttons').removeAttr("style");
    });

    $scope.Click_Back = function () {
        parent.Click_Back();
    };
});