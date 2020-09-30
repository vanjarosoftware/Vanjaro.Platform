app.controller('settings_installextension', function ($scope, $attrs, $http, CommonSvc, FileUploader, $sce) {
    var common = CommonSvc.getData($scope);
    $scope.UploadExtension = new FileUploader();
    $scope.SelectedPackage = null;
    $scope.CheckLicence = false;
    $scope.UploadExtension.onBeforeUploadItem = function (fileItem) {
        $scope.SelectedPackage = $('[uploader="UploadExtension"]input').prop('files')[0];
        if ($scope.SelectedPackage === undefined)
            $scope.SelectedPackage = fileItem._file;
    };

    $scope.UploadExtension.onCompleteAll = function () {
        if ($scope.UploadExtension.progress === 100) {
            $.each($scope.UploadExtension.queue, function (key, value) {
                $scope.FileName = value.file.name;
                var form_data = new FormData();
                form_data.append('file', $scope.SelectedPackage);
                var sf = $.ServicesFramework(-1);
                $.ajax({
                    type: "POST",
                    url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Extensions") + "InstallExtension/ParsePackage",
                    data: form_data,
                    contentType: false,
                    processData: false,
                    cache: false,
                    headers: {
                        'ModuleId': parseInt(sf.getModuleId()),
                        'TabId': parseInt(sf.getTabId()),
                        'RequestVerificationToken': sf.getAntiForgeryValue()
                    },
                    success: function (response) {
                        if (response.IsSuccess) {                            
                            $scope.$apply(function () {
                                $scope.showupload = false;
                                if (response.Data.success) {
                                    if (response.Data.noManifest) {
                                        $scope.showupload = true;
                                        window.parent.ShowNotification($scope.FileName, '[L:NoManifest]', 'error');
                                    }
                                    else {
                                        $scope.showpackagelog = true;
                                        $scope.PackageIcon = response.Data.packageIcon;
                                        $scope.PackageType = response.Data.packageType;
                                        $scope.Name = response.Data.friendlyName;
                                        $scope.Version = response.Data.version;
                                        $scope.Description = $sce.trustAsHtml(response.Data.description);
                                        $scope.DescriptionTitle = response.Data.description ? String(response.Data.description).replace(/<[^>]+>/gm, '') : '';
                                        $scope.Organization = response.Data.organization;
                                        $scope.ReleaseLog = $sce.trustAsHtml(response.Data.releaseNotes);
                                        $scope.Email = response.Data.email;
                                        $scope.SiteURL = response.Data.url;
                                        $scope.License = response.Data.license;
                                        $scope.Finish = response.Data.alreadyInstalled ? "[L:Repair]" : "[L:Finish]";
                                        setTimeout(function () { $("[data-toggle='tooltip']").tooltip('enable'); }, 100);
                                    }
                                }
                                else {
                                    $scope.showerrorlog = true;
                                    $scope.Logs = response.Data.logs;
                                }
                            });
                        }
                        else {
                            window.parent.ShowNotification($scope.FileName, '[L:PackageParseError]', 'error');
                        }
                    }
                });
            });
            $scope.UploadExtension.queue = [];
        }
    };
    $scope.UploadExtension.onErrorItem = function (item, response, status, headers) {
        CommonSvc.SweetAlert.swal(response.ExceptionMessage);
        $scope.UploadExtension.progress = 0;
    };

    $scope.onInit = function () {
        $('#extentionInstall').on("click", function () {
            var isChecked = $("#extensionCheckbox").is(":checked");
            if (isChecked) {
                $('#extentionInstall').html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span> Installing...');
            }
        }); 
        $scope.FileName;
        $scope.showupload = true;
        $scope.showerrorlog = false;
        $scope.showpackagelog = false;
        $scope.Logs = [];
        if ($scope.ui.data.ParsePackageFile && $scope.ui.data.ParsePackageFile.Options) {
            $scope.showupload = false;
            if ($scope.ui.data.ParsePackageFile.Options.success) {
                if ($scope.ui.data.ParsePackageFile.Options.noManifest) {
                    $scope.showupload = true;
                    window.parent.ShowNotification($scope.FileName, '[L:PackageParseError]', 'error');
                }
                else {
                    $scope.PackageIcon = $scope.ui.data.ParsePackageFile.Options.packageIcon;
                    $scope.PackageType = $scope.ui.data.ParsePackageFile.Options.packageType;
                    $scope.Name = $scope.ui.data.ParsePackageFile.Options.friendlyName;
                    $scope.Version = $scope.ui.data.ParsePackageFile.Options.version;
                    $scope.Description = $sce.trustAsHtml($scope.ui.data.ParsePackageFile.Options.description);
                    $scope.DescriptionTitle = $scope.ui.data.ParsePackageFile.Options.description ? String($scope.ui.data.ParsePackageFile.Options.description).replace(/<[^>]+>/gm, '') : '';
                    $scope.Organization = $scope.ui.data.ParsePackageFile.Options.organization;
                    $scope.ReleaseLog = $sce.trustAsHtml($scope.ui.data.ParsePackageFile.Options.releaseNotes);
                    $scope.Email = $scope.ui.data.ParsePackageFile.Options.email;
                    $scope.SiteURL = $scope.ui.data.ParsePackageFile.Options.url;
                    $scope.License = $scope.ui.data.ParsePackageFile.Options.license;
                    $scope.Finish = $scope.ui.data.ParsePackageFile.Options.alreadyInstalled ? "[L:Repair]" : "[L:Finish]";
                    $scope.showpackagelog = true;

                }
            }
            else {
                $scope.showerrorlog = true;
                $scope.Logs = $scope.ui.data.ParsePackageFile.Options.logs;
            }
        }

        //Tooltip
        $('[data-toggle="tooltip"]').tooltip('disable');
    };

    $scope.Click_Finish = function () {
        if ($scope.CheckLicence) {
            $scope.DisableFinish = true;
            if ($scope.ui.data.ParsePackageFile && $scope.ui.data.ParsePackageFile.Options) {
                common.webApi.get('InstallExtension/InstallAvailablePackage', 'FileName=' + $scope.ui.data.FileName.Value + '&Type=' + $scope.ui.data.Type.Value).success(function (response) {
                    $scope.showpackagelog = false;
                    if (response.Data.success) {
                        
                        $scope.Click_Cancel();
                        window.parent.ShowNotification($scope.Name, '[L:InstalledSuccessfully]', 'success');
                    }
                    else {
                        $scope.$apply(function () {
                            $scope.showerrorlog = true;
                            $scope.Logs = response.Data.logs;
                            $('#extentionInstall').html('[L:Finish]');
                            $scope.CheckLicence = false;
                        });
                    }
                    $scope.DisableFinish = false;
                });
            } else {
                var form_data = new FormData();
                form_data.append('file', $scope.SelectedPackage);
                var sf = $.ServicesFramework(-1);
                $.ajax({
                    type: "POST",
                    url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Extensions") + "InstallExtension/InstallPackage",
                    data: form_data,
                    contentType: false,
                    processData: false,
                    cache: false,
                    headers: {
                        'ModuleId': parseInt(sf.getModuleId()),
                        'TabId': parseInt(sf.getTabId()),
                        'RequestVerificationToken': sf.getAntiForgeryValue()
                    },
                    success: function (response) {
                        $scope.showpackagelog = false;
                        if (response.Data.success) {
                            $scope.Click_Cancel();
                            window.parent.ShowNotification($scope.Name, '[L:InstalledSuccessfully]', 'success');
                        }
                        else {
                            $scope.$apply(function () {
                                $scope.showerrorlog = true;
                                $scope.Logs = response.Data.logs;
                                $('#extentionInstall').html('[L:Finish]');
                                $scope.CheckLicence = false;
                            });
                        }
                        $scope.DisableFinish = false;
                    }
                });
            }
        }
        else {
            CommonSvc.SweetAlert.swal("[L:CheckLicences]");
        }
    };

    $scope.Click_Cancel = function (type) {
        var Parentscope = parent.document.getElementById("iframe").contentWindow.angular.element(".menuextension").scope();
        Parentscope.Click_IsInstall(true);
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };

    $scope.Click_Email = function () {
        window.location.href = "mailto:" + $scope.Email;
    };

    $scope.Click_ErrorCancel = function () {
        $scope.showupload = true;
        $scope.showerrorlog = false;
    };

    $scope.Click_ShowLicense = function () {
        window.parent.swal({
            customClass: 'sweet-customwidth',
            width: '700px',
            title: '[L:TermsCenter]',
            text: "<div class='sweetheight'>" + $scope.License + "</div>",
            html: true,
        });
    };
});