var Register = {
    User: function (data_Selector) {
        var sf = $.ServicesFramework(-1);
        var EventTarget = $(event.target);
        var Form = $(EventTarget).closest('#Register');
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
            var registerText = $(Form.find('#submit')).attr('attr-localize-register-text');
            if ($.trim(registerText) == "" || registerText == null)
                registerText = "Reset Password";

            Form.find('#submit').html('<span class="spinner-grow spinner-grow-sm" role="status" aria-hidden="true"></span> ' + registerText);
            Form.find('#submit').addClass('disabled');
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
                            Form.find('#submit').html(registerText);
                            Form.find('#submit').removeClass('disabled');
                        }
                    }
                    else {
                        Form.find('#submit').html(registerText);
                        $('<div class="show-message alert alert-danger">' + response.Message + '</div>').insertAfter(Form.find('.Registerhead'));
                        Form.find('#submit').removeClass('disabled');
                    }
                },
                error: function (Error) {
                    console.log(Error);
                }
            });
        }
    }
};




