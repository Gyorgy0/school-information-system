function showRegister() {
    document.getElementById('login').classList.add('hidden');
    document.getElementById('register').classList.remove('hidden');
    document.getElementById('showRegisterBtn').classList.add('hidden');
}

function showLogin() {
    document.getElementById('register').classList.add('hidden');
    document.getElementById('login').classList.remove('hidden');
    document.getElementById('showRegisterBtn').classList.remove('hidden');
}
    
function register() {
    const username = $('#regUsername').val();
    const password = $('#regPassword').val();
    const role = $('#role').val();

    $.post('/User/Create', { username, password, role })
        .done(function (response) {
            alert(response);
            $("#regUsername").val('');
            $("#regPassword").val('');
            showLogin();
        })
        .fail(function (xhr) {
            alert('Hiba a regisztráció során: ' + xhr.responseText);
        });
}

function login() {
    const username = $('#username').val();
    const password = $('#password').val();
    const role = $('#role').val();

    $.post('/User/Login', { username, password, role })
        .done(function (response) {
            alert(response.message);
            window.location.assign('/main');
        })
        .fail(function (xhr) {
            alert(xhr.responseText);
        });
}
