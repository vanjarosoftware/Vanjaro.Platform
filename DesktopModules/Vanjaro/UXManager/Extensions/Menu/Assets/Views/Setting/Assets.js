app.controller('setting_assets', function ($scope, $attrs, FileUploader, $http, CommonSvc, SweetAlert, $compile) {

    var common = CommonSvc.getData($scope);

    $scope.UploadFile = new FileUploader();
    $scope.Icons = new FileUploader();
    $scope.FolderTypes = { Options: [], Value: -1 };
    $scope.SelectedFolder = -1;
    $scope.ExtractFiles = [];
    $scope.ExistingFileIds = [];

    $scope.onInit = function () {
        setTimeout(function () {
            $scope.IconsClick_FileUpoad('browse');
        }, 10);
        $scope.CopyFileValue = false;
        $scope.MoveFileValue = false;
        $scope.MoveFolderValue = false;
        $('[identifier="setting_assets"] div.esc').remove();
        $scope.ShowErrorMessage = false;
        $scope.ChangeFolderType(true);
        if ($scope.ui.data.IsList.Value === "true" || $scope.ui.data.IsList.Value === "TRUE")
            $scope.ui.data.IsList.Value = true;
        else
            $scope.ui.data.IsList.Value = false;
        $scope.BindFolderEvents();
    };

    $scope.BindFolderEvents = function (fo) {
        if (fo === undefined) {
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

    $scope.ChangeAssetMode = function () {
        $scope.ui.data.AssetType.Value = !$scope.ui.data.AssetType.Value;
        if ($scope.ui.data.AssetType.Value) {
            $("#defaultModal .modal-title", parent.document).html('[L:SiteAssetsTitle]');
            $('.global').attr('data-original-title', '[L:GlobalAssetsTitle]');
        }
        else {
            $("#defaultModal .modal-title", parent.document).html('[L:GlobalAssetsTitle]');
            $('.global').attr('data-original-title', '[L:SiteAssetsTitle]');
        }
    };

    $scope.ChangeFolderType = function (islocal) {
        $scope.FolderTypes = { Options: [], Value: -1 };
        if (islocal) {
            $.each($scope.ui.data.FolderType.Options, function (key, value) {
                if (value.PortalID !== -1)
                    $scope.FolderTypes.Options.push(value);
            });
        }
        else if (islocal === false) {
            $.each($scope.ui.data.FolderType.Options, function (key, value) {
                if (value.PortalID === -1)
                    $scope.FolderTypes.Options.push(value);
            });
        }
        $scope.FolderTypes.Value = $scope.FolderTypes.Options[0].Value;
    };

    $scope.UploadFile.onBeforeUploadItem = function (item) {
        //$('[identifier="setting_assets"]').find('[ng-show="UploadFile.queue.length"]').remove();
        item.formData[0].folder = $('[identifier="setting_assets"]').find('.folders[style="font-weight: bold;"]').attr('id');
    };

    $scope.UploadFile.onCompleteAll = function () {
        if ($scope.UploadFile.progress === 100) {
            window.parent.ShowNotification('', '[L:FileUploadedSuccessfullyTo]' + $('[identifier="setting_assets"]').find('.folders[style="font-weight: bold;"]').text() + ' [L:Folder]', 'success');
            $scope.Syncfolder(false, parseInt($('[identifier="setting_assets"]').find('.folders[style="font-weight: bold;"]').attr('id').replace(/[^0-9]/gi, '')));
            $scope.ExtractFiles = [];
            $.each($scope.UploadFile.queue, function (k, v) {
                if (v.file.name.split('fileid')[0].indexOf('.zip') > 0)
                    $scope.ExtractFiles.push(v.file.name.split('fileid')[1]);
            });
            $scope.UploadFile.queue = [];
            if ($scope.ExtractFiles.length > 0)
                $scope.ProcessExtractFiles();
            else
                $scope.Pipe_IconsPagging($scope.IconsTableState);
        }
    };

    $scope.UploadFile.onErrorItem = function (item, response, status, headers) {
        window.parent.ShowNotification('', response.ExceptionMessage, 'error');
        $scope.UploadFile.progress = 0;
    };

    $scope.$watch('ui.data.AssetType.Value', function (newValue, oldValue) {
        if (newValue !== undefined && oldValue !== undefined) {
            $scope.GetFolderAndFiles(newValue);
        }
    });

    $scope.$watch('ui.data.IsList.Value', function (newValue, oldValue) {
        if (newValue !== undefined && oldValue !== undefined) {
            $scope.IconsIsList = true;
            $scope.ui.data.IsList.Value = true;
        }
    });

    $scope.ProcessExtractFiles = function () {
        swal({
            title: "[L:ExtractZip]",
            text: "[L:ExtractZipText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:YesButton]",
            cancelButtonText: "[LS:NoButton]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    $('[type="search"]').focus();
                    common.webApi.post('Upload/ExtractFiles', 'fileids=' + $scope.ExtractFiles.join(',')).success(function (response) {
                        $scope.ExtractFiles = [];
                        $scope.Pipe_IconsPagging($scope.IconsTableState);
                    });
                }
                else {
                    $scope.ExtractFiles = [];
                    $scope.Pipe_IconsPagging($scope.IconsTableState);
                }
            });

    };

    $scope.GetFolderAndFiles = function (AssetValue) {
        common.webApi.get('Assets/GetFolderAndFiles', 'IsGlobal=' + !AssetValue).success(function (data) {
            $scope.IconsFolders = data;
            $scope.BindFolderEvents();
            $scope.ChangeFolderType(AssetValue); //false means global
            setTimeout(function () {
                $('#folders' + $scope.IconsFolders[0].Value).click();
            }, 500);
        });
    };

    $scope.Click_Cancel = function () {
        window.parent.swal.close();
    };

    $scope.OpenPermissionPopUp = function (folder) {
        //$('uiengine #permDiv').modal({ keyboard: false, backdrop: false });
        //$('uiengine #permDiv .modal-title').text(folder.Text);
        //$('uiengine .PermissionFrame').attr('id', folder.Value);
        //$('uiengine .PermissionFrame').attr('src', window.location.href + '#/permission/' + folder.Value).load(function () { });
        //$("uiengine #permDiv").on("hidden.bs.modal", function (e) {
        //    $('uiengine .PermissionFrame').attr('src', 'about:blank');
        //});
        window.parent.OpenPopUp(null, 700, 'centre', 'Permissions', window.location.href + '#/permission/' + folder.Value, 850, '');
    };

    $scope.RenameFolder = function (folderid, foldername) {
        var FolderID = folderid;
        var FolderName = foldername;
        swal(
            {
                title: "[L:RenameFolder]", text: "[L:LetsGiveNewName]", inputValue: FolderName, type: "input", confirmButtonText: "[LS:Ok]", cancelButtonText: "[LS:Cancel]", showCancelButton: true, closeOnConfirm: false, animation: "slide-from-top", inputPlaceholder: "[L:FolderName]"
            },
            function (inputValue) {
                if (inputValue === false)
                    return false;
                if (inputValue === "") {
                    swal.showInputError("[L:EnterFolderName]");
                    return false;
                }
                if (/^[^\:/*?"<>|.]+$/.test(inputValue) === false) {
                    swal.showInputError("[L:WrongFolderName]");
                    return false;
                }
                if (inputValue === $('[identifier="setting_assets"]').find('.folders[style="font-weight: bold;"]').text()) {
                    swal.close();
                    return true;
                }
                var folderName = inputValue;
                common.webApi.get('Upload/RenameFolder', 'folderid=' + FolderID + '&newFolderName=' + encodeURIComponent(folderName)).success(function (data) {
                    if (data === 'Success') {
                        $.each($scope.IconsFolders, function (key, value) {
                            RenameTree(folderName, value, FolderID);
                        });
                        swal.close();
                    }
                    else {
                        swal.showInputError(data);
                        return false;
                    }
                });
            });
    };

    $scope.RenameFile = function (fileid, foldername) {
        swal(
            {
                title: "[L:RenameFile]", text: "[L:LetsGiveNewNameFile]", inputValue: foldername, type: "input", confirmButtonText: "[LS:Ok]", cancelButtonText: "[LS:Cancel]", showCancelButton: true, closeOnConfirm: false, animation: "slide-from-top", inputPlaceholder: "[L:FileName]"
            },
            function (inputValue) {
                if (inputValue === false)
                    return false;
                if (inputValue === "") {
                    swal.showInputError("[L:EnterFileName]");
                    return false;
                }
                if (/[a-zA-Z0-9]/.test(inputValue) === false) {
                    swal.showInputError("[L:WrongFolderName]");
                    return false;
                }
                if (inputValue === $('[identifier="setting_assets"]').find('.folders[style="font-weight: bold;"]').text()) {
                    swal.close();
                    return true;
                }
                var ChangeFileName = inputValue;
                common.webApi.get('Upload/RenameFile', 'fileID=' + fileid + '&newFileName=' + encodeURIComponent(ChangeFileName)).success(function (data) {
                    if (data === 'Success') {
                        $scope.Syncfolder(false, parseInt($('[identifier="setting_assets"]').find('.folders[style="font-weight: bold;"]').attr('id').replace(/[^0-9]/gi, '')));
                        swal.close();
                    }
                    else {
                        swal.showInputError(data);
                        return false;
                    }
                });
            });
    };

    $scope.CreateFolder = function () {
        $scope.ShowErrorMessage = false;
        var FolderType = $scope.FolderTypes.Value;
        var FolderName = $scope.NewFolderName;
        if (FolderName === "" || FolderName === undefined) {
            $scope.ShowErrorMessage = true;
            $scope.ErrorMessage = "[L:EnterFolderName]";
            $('.ShowErrorMessage').show();
        }
        else if (/^[^\:/*?"<>|.]+$/.test(FolderName) === false) {
            $scope.ShowErrorMessage = true;
            $scope.ErrorMessage = "[L:WrongFolderName]";
            $('.ShowErrorMessage').show();
        }
        if (!$scope.ShowErrorMessage) {
            common.webApi.post('Upload/CreateNewFolder', 'folderparentID=' + folderid + '&folderName=' + encodeURIComponent(FolderName) + '&FolderType=' + FolderType).success(function (data) {
                if (data === 'Success') {
                    $scope.Syncfolder(true, folderid);
                    $('[data-dismiss="modal"]').click();
                }
                else {
                    $scope.ShowErrorMessage = true;
                    $scope.ErrorMessage = data;
                    $('.ShowErrorMessage').show();
                }
            });
        }
    };

    $scope.MoveFolderPopUp = function (fid) {
        folderid = fid;
        $scope.MoveFolderValue = true;
        $scope.OpenPopUp();
        $scope.ClosePopUp();
    };

    $scope.MoveFilePopUp = function (fid) {
        fileid = fid;
        $scope.MoveFileValue = true;
        $scope.OpenPopUp();
        $scope.ClosePopUp();
    };

    $scope.CopyFilePopUp = function (fid) {
        fileid = fid;
        $scope.CopyFileValue = true;
        $scope.OpenPopUp();
        $scope.ClosePopUp();
    };

    $scope.OpenPopUp = function () {
        if ($scope.MoveFolderValue || $scope.MoveFileValue)
            $('.cpymv').text('[LS:Move]');
        else
            $('.cpymv').text('[LS:Copy]');
        $('uiengine #movefolder').modal({ keyboard: false });
        $("uiengine #movefolder").on("shown.bs.modal", function (e) {
            var DestinationFolderID = -1;
            if ($scope.IconsFolders !== undefined && $scope.IconsFolders[0] !== undefined)
                DestinationFolderID = $scope.IconsFolders[0].Value;
            setTimeout(function () { $('[identifier="setting_assets"]').find('#movefolder').find('#folders' + DestinationFolderID).closest('li').children('span.fas fa-caret-right').click(); }, 100);
            $('[identifier="setting_assets"]').find('#movefolder').find('.folders').css('font-weight', '');
            $('[identifier="setting_assets"]').find('#movefolder').find('#folders' + DestinationFolderID).css('font-weight', 'bold');
        });
    };

    $scope.ResetFolders = function () {
        if ($scope.IconsFolders !== undefined && $scope.IconsFolders[0] !== undefined) {
            $.each($scope.IconsFolders[0].children, function (k, v) {
                var $this = $('[identifier="setting_assets"]').find('.Iconsfoldersdiv').find('#folders' + v.Value).closest('li').children('span.fas fa-caret-down');
                if ($this !== undefined && $this.length > 0) {
                    $($this[0]).click();
                    $($this[0]).removeClass('fas fa-caret-down').addClass('fas fa-caret-right');
                }
            });
            setTimeout(function () { $('[identifier="setting_assets"]').find('.Iconsfoldersdiv').find('#folders' + $scope.IconsFolders[0].Value).click(); }, 100);
        }
    };

    $scope.ClosePopUp = function () {
        $("uiengine #movefolder").on("hidden.bs.modal", function (e) {
            $scope.MoveFileValue = false;
            $scope.CopyFileValue = false;
            $scope.MoveFolderValue = false;
            if ($scope.IconsFolders !== undefined && $scope.IconsFolders[0] !== undefined) {
                $.each($scope.IconsFolders[0].children, function (k, v) {
                    var $this = $('[identifier="setting_assets"]').find('#movefolder').find('#folders' + v.Value).closest('li').children('span.fas fa-caret-down');
                    if ($this !== undefined && $this.length > 0) {
                        $($this[0]).click();
                        $($this[0]).removeClass('fas fa-caret-down').addClass('fas fa-caret-right');
                    }
                    $this = $('[identifier="setting_assets"]').find('.Iconsfoldersdiv').find('#folders' + v.Value).closest('li').children('span.fas fa-caret-down');
                    if ($this !== undefined && $this.length > 0) {
                        $($this[0]).click();
                        $($this[0]).removeClass('fas fa-caret-down').addClass('fas fa-caret-right');
                    }
                });
            }
        });
    };

    $scope.MoveCopyToFolder = function (DestinationFolderID) {
        $scope.SelectedFolder = DestinationFolderID;
        setTimeout(function () { $('[identifier="setting_assets"]').find('#movefolder').find('#folders' + DestinationFolderID).closest('li').children('span.fas fa-caret-right').click(); }, 100);
        $('[identifier="setting_assets"]').find('#movefolder').find('.folders').css('font-weight', '');
        $('[identifier="setting_assets"]').find('#movefolder').find('#folders' + DestinationFolderID).css('font-weight', 'bold');
    };

    $scope.SelectUpdateFolder = function () {
        $scope.ExistingFileIds = [];
        var fileids = [];
        if ($scope.MoveFileValue || $scope.CopyFileValue) {
            fileids.push(fileid);
            if ($scope.IconsIsList) {
                $.each($('.chkfid:checked'), function (k, v) {
                    fileids.push(parseInt($(v).attr('cbfid')));
                });
            }
            else {
                $.each($('.thumb-info-wrapper-selected'), function (k, v) {
                    fileids.push(parseInt($(v).attr('cbfid')));
                });
            }
        }

        if (fileids.length <= 1) {
            var method;
            if ($scope.MoveFolderValue)
                method = "Upload/MoveFolder?folderId=" + folderid;
            else if ($scope.MoveFileValue)
                method = "Upload/MoveFile?fileid=" + fileid + '&overwrite=' + false;
            else if ($scope.CopyFileValue)
                method = "Upload/CopyFile?fileid=" + fileid + '&overwrite=' + false;
            common.webApi.get(method + '&destinationFolderId=' + $scope.SelectedFolder).success(function (Data) {
                $(window.document.body).find('[data-dismiss="modal"]').click();
                $scope.ClosePopUp();
                if (Data === "Success") {
                    $scope.Syncfolder(true, $scope.IconsFolders[0].Value);
                    $scope.ResetFolders();
                }
                else if (Data === "Exist") {
                    swal({
                        title: "[L:DuplicateExist]",
                        text: "[L:SureReplace]",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Replace]",
                        cancelButtonText: "[LS:Cancel]",
                        closeOnConfirm: true,
                        closeOnCancel: true
                    },
                        function (isConfirm) {
                            if (isConfirm) {
                                method = method.replace(false, true);
                                common.webApi.get(method + '&destinationFolderId=' + $scope.SelectedFolder).success(function (Data) {
                                    if (Data === "Success") {
                                        $scope.Syncfolder(true, $scope.IconsFolders[0].Value);
                                        $scope.ResetFolders();
                                    }
                                    else {
                                        window.parent.ShowNotification('', Data, 'error');
                                    }
                                });
                            }
                        });
                }
                else {
                    window.parent.ShowNotification('', Data, 'error');
                }
            });
        }
        else {
            var Type;
            var method;
            if ($scope.MoveFileValue) {
                method = "Upload/MoveFiles?fileids=" + fileids.join(',') + '&overwrite=' + false;
                Type = 'Move';
            }
            else if ($scope.CopyFileValue) {
                method = "Upload/CopyFiles?fileids=" + fileids.join(',') + '&overwrite=' + false;
                Type = 'Copy';
            }
            common.webApi.get(method + '&destinationFolderId=' + $scope.SelectedFolder).success(function (Data) {
                $(window.document.body).find('[data-dismiss="modal"]').click();
                $scope.ClosePopUp();
                if (Data === "Success") {
                    $scope.Syncfolder(true, $scope.IconsFolders[0].Value);
                    $scope.ResetFolders();
                }
                else if (Data != undefined && Data.Error != undefined && Data.Error == "Exist") {
                    if (Data.ExistingFiles.length > 0)
                        $scope.ProcessExistingFile(Data.ExistingFiles[0], Data.ExistingFiles, Type);
                }
                else {
                    window.parent.ShowNotification('', Data, 'error');
                }
            });
        }
    };

    $scope.ProcessExistingFile = function (v, data, type) {
        swal({
            title: "[L:DuplicateExist]",
            text: v.Name.replace(/^.*[\\\/]/, '') + "[L:SureReplaceFile]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Replace]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    $scope.ExistingFileIds.push(v.FileId);
                }
                data = data.filter(function (s) {
                    return s.FileId !== v.FileId;
                });
                if (data.length > 0)
                    setTimeout(function () { $scope.ProcessExistingFile(data[0], data, type); }, 500);
                else {
                    if ($scope.ExistingFileIds.length > 0) {
                        var method;
                        if (type == 'Move')
                            method = "Upload/MoveFiles?fileids=" + $scope.ExistingFileIds.join(',') + '&overwrite=' + true;
                        else if (type == 'Copy')
                            method = "Upload/CopyFiles?fileids=" + $scope.ExistingFileIds.join(',') + '&overwrite=' + true;
                        common.webApi.get(method + '&destinationFolderId=' + $scope.SelectedFolder).success(function (Data) {
                            if (Data == "Success") {
                                $scope.Syncfolder(true, $scope.IconsFolders[0].Value);
                                $scope.ResetFolders();
                            }
                            else {
                                window.parent.ShowNotification('', Data, 'error');
                            }
                        });
                    }
                }
            });
    };

    $scope.BaseFolderClick_GetSubFolders = function (event, folder) {
        var $this = $(event.currentTarget);
        if ($this.hasClass("fas fa-caret-right") && $this.parent().find('> .rootfolder li').length <= 0) {
            common.webApi.get('Upload/GetSubFolders', 'identifier=setting_assets&folderid=' + folder.Value).success(function (data) {
                if (data !== undefined && data !== null) {
                    folder.children = data;
                    setTimeout(function () {
                        $this.toggleClass('fas fa-caret-right fas fa-caret-down');
                        if ($this.hasClass('fas fa-caret-down'))
                            $this.parent().find('> .rootfolder').addClass('show');
                        else $this.parent().find('> .rootfolder').removeClass('show');
                    }, 2)
                }
            });
        }
        else {
            if ($this.parent().find('> .rootfolder li').length <= 0)
                return;
            $this.toggleClass('fas fa-caret-right fas fa-caret-down');
            if ($this.hasClass('fas fa-caret-down'))
                $this.parent().find('> .rootfolder').addClass('show');
            else
                $this.parent().find('> .rootfolder').removeClass('show');
        }
    };

    $scope.Deletefolder = function (folderid) {
        swal({
            title: "[LS:AreYouSure]",
            text: "[LS:NotRecover]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('Upload/DeleteItems', 'folderid=' + folderid).success(function (Data) {
                        if (Data === "Success") {
                            $scope.Syncfolder(true, $scope.IconsFolders[0].Value);
                            $scope.ResetFolders();
                        }
                        else {
                            swal.showInputError(Data);
                            return false;
                        }
                    });
                }
            });
    };

    $scope.Syncfolder = function (value, id) {
        var folderID;
        var sync;
        if (event !== undefined && event.target !== undefined && $(event.target).hasClass("sync"))
            sync = true;
        else
            sync = false;
        if (id !== undefined)
            folderID = id;
        else
            folderID = $('[identifier="setting_assets"]').find('.folders[style="font-weight: bold;"]').attr('id').replace(/[^0-9]/gi, '');
        common.webApi.get('Upload/SynchronizeFolder', 'folderid=' + folderID + '&recursive=' + value).success(function (data) {
            if (data.IsSuccess) {
                common.webApi.get('Upload/GetSubFolders', 'folderid=' + folderID).success(function (Data) {
                    if (Data !== undefined) {
                        AddTree(Data, $scope.IconsFolders[0], folderID);
                        $scope.BindFolderEvents();
                    }
                });
                common.webApi.get('Upload/GetFiles', 'folderId=' + folderID + '&uid=' + null + '&skip=' + 0 + '&pagesize=' + 20 + '&keyword=').success(function (data) {
                    if (data !== undefined) {
                        $scope.IconsFiles = data.Files;
                    }
                });
                if (sync)
                    window.parent.ShowNotification(data.FolderName, '[LS:SyncSuccessMessage]', 'success');
            }
            else
                window.parent.ShowNotification(data.FolderName, data.Message, 'error');
        });
    };

    $scope.DeleteFile = function (fileid) {
        swal({
            title: "[LS:AreYouSure]",
            text: "[LS:NotRecover]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    var fileids = [];
                    fileids.push(fileid);
                    if ($scope.IconsIsList) {
                        $.each($('.chkfid:checked'), function (k, v) {
                            fileids.push(parseInt($(v).attr('cbfid')));
                        });
                    }
                    else {
                        $.each($('.thumb-info-wrapper-selected'), function (k, v) {
                            fileids.push(parseInt($(v).attr('cbfid')));
                        });
                    }
                    common.webApi.delete('Upload/DeleteFiles', 'fileIDs=' + fileids.join(',')).success(function (Data) {
                        if (Data === "Success") {
                            $scope.Pipe_IconsPagging($scope.IconsTableState);
                        }
                        else {
                            swal.showInputError(Data);
                            return false;
                        }
                    });
                }
            });
    };

    $scope.GetURL = function (fileid) {
        common.webApi.post('Upload/GetUrl', 'fileid=' + fileid).success(function (data) {
            if (data.Status === 'Success') {
                var Link = "";
                if (data.Url.startsWith('http') || data.Url.startsWith('//') || data.Url.startsWith('www'))
                    Link = data.Url;
                else
                    Link = window.location.origin + data.Url;
                var $temp = $("<input>");
                $("body").append($temp);
                $temp.val(Link).select();
                document.execCommand("copy");
                $temp.remove();
                window.parent.ShowNotification('', '[L:LinkCopied]', 'success');
            }
            else {
                swal.showInputError(data);
                return false;
            }
        });
    };

    $scope.DownloadFile = function (fileid, filename) {
        if (fileid > 0) {
            var sf = $.ServicesFramework(-1);
            $http({
                method: 'GET',
                url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Assets") + "Upload/Download?fileId=" + fileid + '&forceDownload=' + true,
                responseType: 'arraybuffer',
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
            }).success(function (data, status, headers) {
                headers = headers();
                var filename = headers['x-filename'];
                var contentType = headers['content-type'];
                var linkElement = document.createElement('a');
                try {
                    var blob = new Blob([data], { type: contentType });
                    var url = window.URL.createObjectURL(blob);
                    linkElement.setAttribute('href', url);
                    linkElement.setAttribute("download", filename);
                    var clickEvent = new MouseEvent("click", {
                        "view": window,
                        "bubbles": true,
                        "cancelable": false
                    });
                    linkElement.dispatchEvent(clickEvent);
                } catch (ex) {
                    alert(ex);
                }
            }).error(function (data) {
                alert(data);
            });
        }
    };

    $scope.CreateFolderPopUp = function (fid) {
        folderid = fid;
        $scope.NewFolderName = "";
        $scope.FolderTypes.Value = $scope.FolderTypes.Options[0].Value;
        $scope.ShowErrorMessage = false;
        $('.ShowErrorMessage').hide();
        $('.NewFolderName').val("");
        $('.FolderType').val("0");
        $('uiengine #CreateFolder').modal({ keyboard: false });
    };

    $scope.FolderType = function (Type) {
        $scope.FolderType = Type;
    };

    $scope.ClickInfoWrapper = function (obj) {
        if ($(obj.currentTarget).find('.thumb-info-wrapper').hasClass('thumb-info-wrapper-selected'))
            $(obj.currentTarget).find('.thumb-info-wrapper').removeClass('thumb-info-wrapper-selected');
        else
            $(obj.currentTarget).find('.thumb-info-wrapper').addClass('thumb-info-wrapper-selected');
        $scope.ShowHideMenuItem();
    };

    $scope.ShowHideMenuItem = function (all) {
        if ($scope.IconsIsList) {
            if (all) {
                if ($('.chkfidall:checked').length > 0)
                    $('.chkfid').prop("checked", true);
                else
                    $('.chkfid').removeAttr('checked');
            }
            if ($('.chkfid:checked').length <= 1)
                $('.vjShowHideMenuItem').show();
            else
                $('.vjShowHideMenuItem').hide();
        }
        else {
            if (all) {
                if ($('.chkfidall:checked').length > 0)
                    $('.thumb-info-wrapper').addClass('thumb-info-wrapper-selected');
                else
                    $('.thumb-info-wrapper').removeClass('thumb-info-wrapper-selected');
            }
            if ($('.thumb-info-wrapper-selected').length <= 1)
                $('.vjShowHideMenuItem').show();
            else
                $('.vjShowHideMenuItem').hide();
        }
    };

    function AddTree(Data, element, matchingTitle) {
        if (element.Value === matchingTitle) {
            element.children = Data;
            element.childrenCount = Data.length;
            return true;
        } else if (element.children !== null) {
            var i;
            var result = null;
            for (i = 0; result === null && i < element.children.length; i++) {
                if (AddTree(Data, element.children[i], matchingTitle))
                    return true;
            }
        }
    }

    function RenameTree(Data, element, matchingTitle) {
        if (element.Value === matchingTitle) {
            element.Text = Data;
            return true;
        } else if (element.children !== null) {
            var i;
            var result = null;
            for (i = 0; result === null && i < element.children.length; i++) {
                if (RenameTree(Data, element.children[i], matchingTitle))
                    true;
            }
        }
    };

    function findparent(parentid, element, matchingID) {
        if (element.Value === matchingID) {
            return true;
        } else if (element.children !== null) {
            var i;
            var result = null;
            for (i = 0; result === null && i < element.children.length; i++) {
                if (findparent(element.Value, element.children[i], matchingID))
                    break;
            }
            return parentid;
        }
    };
});