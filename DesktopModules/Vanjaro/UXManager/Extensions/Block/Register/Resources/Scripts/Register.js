var Register = {
    User: function (data_Selector) {
        var sf = $.ServicesFramework(-1);
        var obj = $('#registerbtn');
        var Form = obj.closest('#Register');
        Form.find('.show-message').remove();
        if (mnValidationService.DoValidationAndSubmit(data_Selector)) {
            var Username = Form.find('input.username').length > 0 ? Form.find('input.username').val() : Form.find('input.email').val();
            var data = {
                'UserName': Username,
                'Password': Form.find('input.password').val(),
                'ConfirmPassword': Form.find('input.password').val(),
                'DisplayName': Form.find('input.displayname').val(),
                'Email': Form.find('input.email').val()
            };
            var registerText = obj.attr('attr-localize-register-text');
            if ($.trim(registerText) == "" || registerText == null)
                registerText = "Reset Password";

            obj.html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span> ' + registerText);
            obj.addClass('disabled');
            $.ajax({
                type: "POST",
                url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Register") + "Register/Index",
                data: data,
                headers: {
                    'ModuleId': parseInt(sf.getModuleId()),
                    'TabId': parseInt(sf.getTabId()),
                    'RequestVerificationToken': sf.getAntiForgeryValue()
                },
                success: function (response) {
                    if (response.IsSuccess) {
                        if (response.Data != null && response.Data.RedirectURL != null) {
                            window.location.href = response.Data.RedirectURL;
                        }
                        else if (response.Message != null && response.Message != "") {
                            $('<div class="show-message alert alert-success">' + response.Message + '</div>').insertAfter(Form.find('.Registerhead'));
                            Form.find('input').val('');
                            Form.find('#checktermcondition').prop("checked", false);
                        }
                        else {
                            obj.html(registerText);
                            obj.removeClass('disabled');
                        }
                    }
                    else {
                        obj.html(registerText);
                        $('<div class="show-message alert alert-danger">' + response.Message + '</div>').insertAfter(Form.find('.Registerhead'));
                        obj.removeClass('disabled');                        
                    }

                    if (response.Errors["ProfileUpdate"] != null && typeof response.Errors["ProfileUpdate"] != "undefined") {
                        $(obj).closest('.Register').find('#registerbtn').html(registerText);
                        $(obj).closest('.Register').find('#registerbtn').removeClass('disabled');
                        if (response.Data != null && response.Data.UserExtensionURL != null) {
                            parent.OpenPopUp(null, 800, 'right', null, response.Data.UserExtensionURL);
                        }
                    }
                },
                error: function (Error) {                    
                    console.log(Error);
                }
            });
        }
    }
};



