$(document).ready(function () { 
    //if (localStorage.RememberMe && localStorage.RememberMe != '' && localStorage.RememberMe == "true") {
    //    $('#chkRememberMe').prop('checked', true);
    //    var usrDc = CryptoJS.AES.decrypt(CookiesController.setCookie("username") , "JHKJHKAHFKSHFKHLKHFSKLHFSKLHFKLHSFKLSFHLKHFKSHFKlhF");
    //    var pssDc = CryptoJS.AES.decrypt(CookiesController.setCookie("passkey"), "JHKJHKAHFKSHFKHLKHFSKLHFSKLHFKLHSFKLSFHLKHFKSHFKlhF"); 
    //    $('#txtUserName').val(usrDc.toString(CryptoJS.enc.Utf8));
    //    $('#txtPassword').val(pssDc.toString(CryptoJS.enc.Utf8));
    //} else {
    //    $('#chkRememberMe').prop('checked', false);
    //    $('#txtUserName').val("");
    //    $('#txtPassword').val("");
    //}

    $("#txtPassword").bind("keyup", function (event) {
        var code = (event.keyCode ? event.keyCode : event.which);
        if (code == 13) {
            event.preventDefault();
            OnAuthentication();
        }
    });
});
function OnAuthentication() { 
    var loginModel = {
        Username: $("#txtUserName").val(),
        Password: $("#txtPassword").val()
    };

    if (loginModel.Username == null || loginModel.Username == "") { 
        MessageController.Error("Pleas Enter 'Username'", 'Login Faild'); 
        return;
    }
    if (loginModel.Password == null || loginModel.Password == "") { 
        MessageController.Error("Pleas Enter 'Password'", 'Login Faild'); 
        return;
    }
    MessageController.BlockUI({ boxed: true, target: '#frm-login' });
    $.ajax({
        type: "POST",
        url: "/Account/Login",
        data: loginModel,
        success: function (response) { 
            if (response.Code == 200) { 
                //if ($("#chkRememberMe").prop('checked') == true) {
                //    CookiesController.setCookie("username") = CryptoJS.AES.encrypt($('#txtUserName').val(), "JHKJHKAHFKSHFKHLKHFSKLHFSKLHFKLHSFKLSFHLKHFKSHFKlhF");
                //    CookiesController.setCookie("passkey") = CryptoJS.AES.encrypt($('#txtPassword').val(), "JHKJHKAHFKSHFKHLKHFSKLHFSKLHFKLHSFKLSFHLKHFKSHFKlhF"); 
                //    localStorage.RememberMe = true; 
                //} else {
                //    $('#chkRememberMe').prop('checked', false);
                //    CookiesController.setCookie("username") = null;
                //    CookiesController.setCookie("passkey") = null;
                //    localStorage.RememberMe = false;
                //}
                localStorage.menuId = null;
                MessageController.UnblockUI('#frm-login');
                window.location.href = response.Data;
            } else { 
                MessageController.UnblockUI('#frm-login');
                MessageController.Error("Pleas Enter 'Password'", 'Login Faild'); 
            }
        },
        failure: function (response) { 
            MessageController.UnblockUI('#frm-login');
            if (response.responseJSON != null && response.responseJSON != undefined && response.responseJSON.Errors.length > 0) {
                MessageController.Error(response.responseJSON.Errors[0], "Login Faild"); 
            } else {
                MessageController.Error("Invalid Account", "Login Faild");
            }
        },
        error: function (response) { 
            MessageController.UnblockUI('#frm-login');
            MessageController.Error(response.responseJSON.Errors[0], "Login Faild"); 
        }
    });  
}