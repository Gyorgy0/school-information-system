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
