let user = null;

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
            const role = response.role;
            user = { name: username, role: role };
            document.getElementById('login').classList.add('hidden');
            document.getElementById('userInfo').innerText = `Bejelentkezve: ${username}(${role})`;
            document.getElementById('logoutBtn').classList.remove('hidden');
            document.getElementById('showRegisterBtn').classList.add('hidden');

            if (role === 'student') {
            } else if (role === 'teacher') {
            } else if (role === 'admin') {
            }

            document.getElementById('chatInputBox').classList.remove('hidden');
        })
        .fail(function (xhr) {
            alert('Hibás felhasználónév vagy jelszó!');
        });
}

function logout() {
    $.post('/User/Logout')
        .done(function (response) {
            user = null;
            document.getElementById('login').classList.remove('hidden');
            document.getElementById('userInfo').innerText = '';
            document.getElementById('logoutBtn').classList.add('hidden');
            document.getElementById('showRegisterBtn').classList.remove('hidden');
            document.getElementById('username').value = '';
            document.getElementById('password').value = '';
            document.getElementById('role').value = 'student';
        })
        .fail(function (xhr) {
            alert('Hiba a kijelentkezés során: ' + xhr.responseText);
        });
}

function setRoleBasedView(role) {
    if (role === 'student') {
        document.getElementById('teacherMenu').classList.add('hidden');
        document.getElementById('studentMenu').classList.remove('hidden');
        document.getElementById('adminMenu').classList.add('hidden');
        document.getElementById('gradeInput').classList.add('hidden');
        document.getElementById('hwInput').classList.add('hidden');
        document.getElementById('chatInputBox').classList.remove('hidden');
        document.getElementById('newTimetableEntry').classList.add('hidden');
    } else if (role === 'teacher') {
        document.getElementById('teacherMenu').classList.remove('hidden');
        document.getElementById('studentMenu').classList.add('hidden');
        document.getElementById('adminMenu').classList.add('hidden');
        document.getElementById('gradeInput').classList.remove('hidden');
        document.getElementById('hwInput').classList.remove('hidden');
        document.getElementById('chatInputBox').classList.remove('hidden');
        document.getElementById('newTimetableEntry').classList.add('hidden');
    } else if (role === 'admin') {
        document.getElementById('teacherMenu').classList.add('hidden');
        document.getElementById('studentMenu').classList.add('hidden');
        document.getElementById('adminMenu').classList.remove('hidden');
        document.getElementById('gradeInput').classList.add('hidden');
        document.getElementById('hwInput').classList.remove('hidden');
        document.getElementById('chatInputBox').classList.add('hidden');
        document.getElementById('newTimetableEntry').classList.remove('hidden');
    }
}