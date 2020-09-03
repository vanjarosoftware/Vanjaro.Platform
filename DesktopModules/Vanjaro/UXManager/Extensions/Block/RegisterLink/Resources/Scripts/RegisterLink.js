//var Register = {
//    User: function (data_Selector) {
//        var sf = $.ServicesFramework(-1);
//        var EventTarget = $(event.target);
//        if (this.DoValidationAndSubmit(data_Selector)) {
//            var data = {
//                'UserName': $(EventTarget).closest('#Register').find('input.username').val(),
//                'Password': $(EventTarget).closest('#Register').find('input.password').val(),
//                'ConfirmPassword': $(EventTarget).closest('#Register').find('input.confirmpassword').val(),
//                'DisplayName': $(EventTarget).closest('#Register').find('input.displayname').val(),
//                'Email': $(EventTarget).closest('#Register').find('input.email').val()
//            };
//            $.ajax({
//                type: "POST",
//                url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Register") + "Register/Index",
//                data: data,
//                headers: {
//                    'ModuleId': parseInt(sf.getModuleId()),
//                    'TabId': parseInt(sf.getTabId()),
//                    'RequestVerificationToken': sf.getAntiForgeryValue()
//                },
//                success: function (response) {
//                    if (response.IsSuccess) {
//                        if (response.Data != null && response.Data.RedirectURL != null) {
//                            window.location.href = response.Data.RedirectURL;
//                        }
//                        else if (response.Message != null && response.Message != "") {
//                            if (response.Message.length > 75) {
//                                swal({
//                                    title: '',
//                                    text: response.Message,
//                                    html: true
//                                });
//                            }
//                            else {

//                                window.parent.ShowNotification('Done!', response.Message, 'success');
//                            }
//                            $(EventTarget).closest('#Register').find('input').val('');
//                        }
//                    }
//                    else {
//                        if (response.Message.length > 75) {
//                            swal({
//                                title: '',
//                                text: response.Message,
//                                html: true
//                            });
//                        }
//                        else
//                            window.parent.ShowNotification('Error!', response.Message, 'error');
//                    }
//                }
//            });
//        }
//    },
//    DoValidationAndSubmit: function (sender) {
//        var group;
//        if (sender != undefined && sender.length > 0)
//            group = $(sender);

//        var isValid = true;
//        $(group).validate();
//        $(group).find(':input').each(function (i, item) {
//            if ($(item).is(':visible') && !$(item).valid()) {
//                isValid = false;
//            }
//        });
//        return isValid;
//    },
//};

//$.validator.addMethod('comparepassword', function (value, element) {
//    var password = $(element).closest('.Register').find('input[type=password].password').val();
//    if (password !== value)
//        return false;
//    else
//        return true;
//}, "Password not match");


//$.validator.addMethod('emailrequired', function (value, element) {
//    var link = $(element).closest('.Register').find('input[type=email]').val();
//    if (!validateEmail(link))
//        return false;
//    else
//        return true;
//}, "Please enter a valid email address.");


//function validateEmail(sEmail) {
//    var filter = /^[\w\-\.\+]+\@[a-zA-Z0-9\.\-]+\.[a-zA-z0-9]{2,4}$/;
//    if (filter.test(sEmail)) {
//        return true;
//    }
//    else {
//        return false;
//    }
//}




