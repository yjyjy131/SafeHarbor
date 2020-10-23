
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

    if(idDoubleChk(uesrid))
        console.log('???');

    return true;
}

function idDoubleChk(userid) {
    var form = document.loginForm;
    $("#idChkBox").click(function(){
        $.ajax({
            url: '/signUp',
            type: 'GET',
            dataType: "json",
            data: {"userid" : form.userid.value},
            success: function(result){
                console.log('결과는' + result)
                if (result){
                 $('#idChkInfo').text('아이디 생성 가능');
                 $('#idChkInfo').css("color", "#ffffff");
                 }

                if(!result){
                 $('#idChkInfo').text('이미 존재하는 아이디 입니다!');
                 $('#idChkInfo').css("color", "#dfba06");
                }
            }
        })
        return false;
    })
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