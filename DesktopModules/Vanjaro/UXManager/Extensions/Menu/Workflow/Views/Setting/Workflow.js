app.controller('setting_workflow', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
        $scope.WorkflowID = $scope.ui.data.WorkflowId.Values;
        $('#workflow').parent('ul').find('li').removeClass('active');
        $('#advanced').parent('ul').find('li').removeClass('active');
        $('#workflow').addClass('active');
        //$('#advanced').addClass('active');
        $scope.Click_ShowTab('workflow');
        $('[data-target="#collapseFour"]').on('click', function () {
            if ($('#collapseFive').hasClass('show')) {
                $('[data-target="#collapseFive"]').click();
            }
        });
    };

    $scope.ShowWorkflowTab = true;
    //$scope.ShowAdvancedTab = true;
    $scope.ShowAccordion = true;
    $scope.Click_ShowTab = function (TabName) {
        $('#workflow').parent('ul').find('li a').removeClass('active');
        $('#advanced').parent('ul').find('li a').removeClass('active');
        if (TabName == 'workflow') {
            $scope.ShowWorkflowTab = true;
            $scope.ShowAdvancedTab = false;
            $scope.ShowEditWorkflow = false;
            $scope.ShowState = false;
            $scope.ShowPermission = false;
            $scope.ShowAccordion = true;
            $('#workflow a').addClass('active');
            setTimeout(function () {
                $('#collapseFour').addClass('show');
                $('[data-target="#collapseFour"]').on('click', function () {
                    if ($('#collapseFive').hasClass('show')) {
                        $('[data-target="#collapseFive"]').click();
                    }
                });
            }, 1);
        }
        else if (TabName == 'advanced') {
            $scope.ShowAdvancedTab = true;
            $scope.ShowWorkflowTab = false;
            $scope.ShowState = false;
            $scope.ShowEditWorkflow = false;
            $scope.ShowAccordion = false;
            $('#advanced a').addClass('active');
            setTimeout(function () {
                $('#collapseSix').addClass('show');
            }, 1);
        }
    }

    $scope.OpenAccordion = function () {
        setTimeout(function () {
            $scope.Click_ShowTab('workflow');
        }, 1);
    };

    $scope.Change_WorkflowStateInfo = function () {

        if ($scope.ui.data.ddlWorkFlows.Value) {
            $.each($scope.ui.data.ddlWorkFlows.Options, function (key, w) {
                if ($scope.ui.data.ddlWorkFlows.Value == w.Value) {
                    $scope.ui.data.WorkflowStateInfo.Value = w.Content;
                    return;
                }

            });
        }
    };


    $scope.Change_UpdateAdvance = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_workflow')) {
            var data = {
                MaxRevisions: $scope.ui.data.MaxRevisions.Value
            }
            common.webApi.post('workflow/UpdateAdvance', '', data).success(function (data) {
                if (data != null) {
                    $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                }
            });
        };
    };

    $scope.Change_UpdateWorkID = function () {
        var data = {
            WorkflowID: $scope.ui.data.ddlWorkFlows.Value,
        }
        common.webApi.post('workflow/UpdateDefault', '', data).success(function (data) {
            if (data != null) {
                $(window.parent.document.body).find('[data-dismiss="modal"]').click();
            }
        })
    }

    $scope.ShowEditWorkflow = false;

    $scope.Change_Workflow = function (workflowid) {
        $scope.WorkflowID = workflowid;
        common.webApi.get('workflow/getworkflow', 'workflowid=' + parseInt($scope.WorkflowID)).success(function (data) {
            $scope.ui.data.Workflow.Options = data.Data.Workflow;
            $scope.ui.data.WorkflowStates.Options = data.Data.WorkflowStates;
            //$scope.ui.data.Permissions.Options.Permissions = data.Data.workflowPermission.Permissions
            $scope.PermissionsRoles = data.Data.workflowPermission.Permissions.RolePermissions;
            $scope.PermissionsUsers = data.Data.workflowPermission.Permissions.UserPermissions;
            //$scope.workflowPermissionRoles = data.Data.RolePermissions;
            //$scope.workflowPermissionUsers = data.Data.UserPermissions;
            $scope.ShowPermission = true;

            $scope.ShowEditWorkflow = true;
            $scope.ShowWorkflowTab = false;
            $scope.ShowAccordion = false;
        });
    };

    $scope.DeleteWorkflow = function (workflow) {
        CommonSvc.SweetAlert.swal({
            title: "[L:DeleteWorkflowTitle]",
            text: "[L:MessageWorkflowText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {

                if (isConfirm) {
                    $scope.ui.data.Workflow.Options = workflow;
                    $scope.ui.data.Workflow.Options.IsDeleted = true;
                    common.webApi.post('workflow/delete', '', $scope.ui.data.Workflow.Options).success(function (data) {
                        if ($scope.WorkflowID != '0' || $scope.WorkflowID == "") {
                            if (!data.Data.IsDeleted == true)
                                window.parent.ShowNotification('[L:Warning]', '[L:WorkflowUsing]', 'warning');
                            $scope.ui.data.Workflows.Options = data.Data.Workflows;
                            $scope.ui.data.ddlWorkFlows.Options = data.Data.ddlWorkFlows;
                        }
                    });
                }
            });

    }

    var IsNull = function (value) {
        if (value == null || value == undefined)
            return true;
        return false;
    };
    $scope.Click_Add = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'setting_workflow')) {
            var data = {
                Workflow: $scope.ui.data.Workflow.Options,
                PermissionsRoles: $scope.PermissionsRoles,
                PermissionsUsers: $scope.PermissionsUsers,
            }
            common.webApi.post('workflow/add', '', data).success(function (data) {
                if ($scope.WorkflowID == '-1') {
                    $scope.ShowEditWorkflow = true;
                }
                else {
                    $scope.Click_Cancel();
                    //$(window.parent.document.body).find('[data-dismiss="modal"]').click();
                }

                if ($scope.WorkflowID != '0' || $scope.WorkflowID == "") {
                    $scope.ui.data.Workflows.Options = data.Data.Workflows;
                    $scope.WorkflowID = data.Data.WorkflowID.toString();
                    $scope.ui.data.WorkflowStates.Options = data.Data.WorkflowStates;
                    $scope.ui.data.Workflow.Options = data.Data.Workflow;
                    $scope.ui.data.ddlWorkFlows.Options = data.Data.ddlWorkFlows;
                }

            });
        }
    };
    var Validate = function () {
        var isval = true;
        if (!$scope.ui.data.Workflow.Options.Name) {
            isval = false
            CommonSvc.SweetAlert.swal("[L:IsValName]");
        }
        else if (!$scope.ui.data.Workflow.Options.Description) {
            isval = false;
            CommonSvc.SweetAlert.swal("[L:IsValDescription]");
        }
        return isval;
    }

    $scope.Click_Cancel = function () {
        $scope.ShowWorkflowTab = true;
        $scope.ShowAdvancedTab = false;
        $scope.ShowEditWorkflow = false;
        $scope.ShowAccordion = true;
        $scope.OpenAccordion();
    };

    $scope.CheckIsReview = function (row) {
        if (row != undefined) {
            var isavl = false;
            $.each(row.Permissions, function (key, p) {
                if (p.PermissionName == "Review Content") {
                    p.AllowAccess = !p.AllowAccess;
                    isavl = true;
                }
            });
            if (!isavl) {
                var per = $.extend(true, {}, $scope.ui.data.TemplatePermission.Options);
                row.Permissions.push(per);
            }

        }
    };
    $scope.ReviewContent = function (row) {
        var IsReview = "unchecked";
        if (row != undefined) {
            $.each(row.Permissions, function (key, p) {
                if (p.PermissionName == "Review Content" && p.AllowAccess) {
                    IsReview = "check";
                }
            });
        }
        return IsReview;
    };

    $scope.Click_NewState = function () {
        $scope.StateID = 0;
        $scope.Name = "";
        $scope.Notify = false;
        $scope.ShowEditWorkflow = false;
        $scope.ShowWorkflowTab = false;
        $scope.IsActive = true;
        $scope.ShowState = true;
        $scope.ShowPermission = true;
        common.webApi.get('workflow/statepermission', 'stateid=' + $scope.StateID).success(function (data) {
            $scope.workflowPermissionRoles = data.Data.RolePermissions;
            $scope.workflowPermissionUsers = data.Data.UserPermissions;
        });
    };


    $scope.Click_EditState = function (row) {
        $scope.StateID = row.StateID;
        $scope.Name = row.Name;
        $scope.Notify = row.Notify;
        $scope.IsActive = row.IsActive;
        $scope.ShowEditWorkflow = false;
        $scope.ShowWorkflowTab = false;
        $scope.ShowState = true;
        $scope.ShowNotifyToggel = true;
        $scope.ShowworkflowPermissionRole = false;
        $scope.ShowworkflowPermissionUser = false;

        $scope.ShowActiveToggel = row.WorkflowID == 1 || row.IsFirst || row.IsLast ? false : true;
        if (row.IsFirst || row.IsLast)
            $(".Active  button").prop('disabled', true);

        else
            $(".Active  button").prop('disabled', false);

        if (row.WorkflowID == 1 && (row.IsFirst || row.IsLast))
            $scope.ShowNotifyToggel = false;

        if (row.WorkflowID != 1 && row.IsFirst)
            $scope.ShowNotifyToggel = false;

        if ((row.WorkflowID != 1 && row.IsLast))
            $scope.ShowNotifyToggel = true;

        if (row.IsFirst || row.IsLast)
            $scope.ShowPermission = false;
        else {
            common.webApi.get('workflow/statepermission', 'stateid=' + row.StateID).success(function (data) {
                $scope.workflowPermissionRoles = data.Data.RolePermissions;
                $scope.workflowPermissionUsers = data.Data.UserPermissions;
                $scope.ShowPermission = true;
            });
        }
    };
    $scope.Click_UpdateState = function () {
        if (ValidateState()) {
            var Data = {
                StateID: $scope.StateID,
                Name: $scope.Name,
                Notify: $scope.Notify,
                IsActive: $scope.IsActive,
                RolePermissions: $scope.workflowPermissionRoles,
                UserPermissions: $scope.workflowPermissionUsers
            };
            common.webApi.post('workflow/updatestate', 'WorkflowID=' + parseInt($scope.WorkflowID), Data).success(function (data) {
                $scope.ui.data.Workflow.Options = data.Data.Workflow;
                $scope.ui.data.WorkflowStates.Options = data.Data.WorkflowStates;
                $scope.Click_CancleState();
            });
        }
    };
    var ValidateState = function () {
        var isval = true;
        if (!$scope.Name) {
            isval = false
            window.parent.ShowNotification('[L:Warning]', '[L:IsValStateName]', 'warning');
        }
        else {
            $.each($scope.ui.data.WorkflowStates.Options, function (key, s) {
                if (s.StateID != $scope.StateID && s.Name == $scope.Name) {
                    isval = false;
                    window.parent.ShowNotification('[L:Warning]', '[L:IsValStateExitName]', 'warning');
                }
            });
        }
        return isval;
    }
    $scope.Click_DeleteState = function (row) {
        CommonSvc.SweetAlert.swal({
            title: "[L:DeleteStateTitle]",
            text: "[L:MessageStateText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('workflow/delete', 'stateid=' + row.StateID).success(function (data) {
                        if (data.Data.IsDeleted) {
                            $scope.ui.data.WorkflowStates.Options = $scope.ui.data.WorkflowStates.Options.filter(function (s) {
                                return s.StateID !== row.StateID;
                            });
                        }
                        else {
                            window.parent.ShowNotification('[L:Warning]', '[L:StateUsing]', 'warning');
                        }
                    });
                }
            });
    };
    $scope.Click_CancleState = function () {
        $scope.ShowState = false;
        $scope.ShowEditWorkflow = true;
        $scope.ShowWorkflowTab = false;
        return false;
    };
});