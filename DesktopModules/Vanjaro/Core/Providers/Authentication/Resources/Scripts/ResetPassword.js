var ResetPassword = {
    User: function (data_Selector, obj) {
        var sf = $.ServicesFramework(-1);
        var EventTarget = $(event.target);
        if (mnValidationService.DoValidationAndSubmit(data_Selector)) {
            var data =
            {
                Username: $(EventTarget).closest('#ResetPassword').find('input.Username').val(),
                Password: $(EventTarget).closest('#ResetPassword').find('input.newPassword').val(),
                ConfirmPassword: $(EventTarget).closest('#ResetPassword').find('input.confirmPassword').val(),
                ResetToken: parent.GetParameterByName("resettoken", location.href) ?? null
            };
            $(obj).closest('.ResetPassword').find('.show-message').remove();
            var resetText = $($(obj).closest('.ResetPassword').find('#btnChangePassword')).attr('attr-localize-resetPassword-text');
            if ($.trim(resetText) == "" || resetText == null)
                resetText = "Password Changed";

            $(obj).closest('.ResetPassword').find('#btnChangePassword').html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span> ' + resetText);
            $(obj).closest('.ResetPassword').find('#btnChangePassword').addClass('disabled');
            $.ajax({
                type: "POST",
                url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Authentication") + "ResetPassword/Index",
                data: data,
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
                success: function (response) {
                    if (response.IsSuccess) {
                        $('<div class="show-message alert alert-success">' + response.Message + '</div>').insertAfter($(obj).closest('.ResetPassword').find('.Resethead'));
                        $(obj).closest('.ResetPassword').find('.changepassword').hide();
                        $(obj).closest('.ResetPassword').find('.loginbtn').show();

                        if (response.IsRedirect != undefined && response.RedirectURL != undefined && response.IsRedirect && response.RedirectURL.length > 0) {
                            window.location.href = response.RedirectURL;
                        }
                        else {
                            $(obj).closest('.ResetPassword').find('#btnChangePassword').html(resetText);
                            $(obj).closest('.ResetPassword').find('#btnChangePassword').removeClass('disabled');
                        }
                    }
                    else {
                        $(obj).closest('.ResetPassword').find('#btnChangePassword').html(resetText);
                        $(obj).closest('.ResetPassword').find('#btnChangePassword').removeClass('disabled');
                        $('<div class="show-message alert alert-danger">' + response.Message + '</div>').insertAfter($(obj).closest('.ResetPassword').find('.Resethead'));
                    }
                }
            });
        }
    }
};