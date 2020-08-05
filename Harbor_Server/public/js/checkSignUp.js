const { format } = require("sequelize/types/lib/utils");

// idInput passwordInput passChkInput opSysy
function allChecked() {
    var form = document.loginForm;

    if (! checkUserid(form.userid.value))
        return false;

    if (!checkPassword(form.password.value, form.passwordCheck.value))    
        return false;

    return true;
}

function checkEmpty (value) {
    if(value == "") {
        alert("공백이 있습니다.");
        return false;
    }

    return true;
}

function checkUserid(userid) {
    if(!checkEmpty(userid))
    return false;
    
    var idRestrict = /^[a-zA-Z0-9]{4,12}$/;
    if (!idRestrict.test(userid)){
        alert("아이디는 영문 대소문자와 숫자 4~12자리로 입력해야합니다.");
        return false;
    }
    return true;
}

function checkPassword (password1, password2) {
  
    if (!checkEmpty(password1))
        return false;

    if (!checkEmpty(password2))
        return false;
    
    var passwordRestrict = /^[a-zA-Z0-9]{4,12}$/;
    if (!passwordRestrict.test(password1)){ 
        alert("비밀번호는 영문 대소문자와 4~12자리로 입력해야합니다.");
        return false;
    }

    if(password1 != password2){
        alert("두 비밀번호가 일치하지 않습니다.");
        return false;
    }

    return true;
    
}