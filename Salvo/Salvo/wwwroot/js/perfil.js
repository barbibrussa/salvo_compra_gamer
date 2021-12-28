var app = new Vue({
    el: '#app',
    data: {
        name: "",
        email: "",
        password: "",
        newPassword: "",
        newPasswordRepeat: "",
        avatar: "",
        permisse: "",
        wins: "",
        winsPercent: "",
        defeats: "",
        defeatsPercent: "",
        ties: "",
        tiesPercent: "",
        totalGamesPlayed: ""
    },
    mounted() {
        this.GetPlayer();
        this.GetPermisses();
        this.GetInfoPlayer();
    },
    methods: {
        changeName: function() {
            $('#inputPerfilName').show();
            $('#inputPerfilEmail').hide();
            $('#inputNewPassword').hide();
            $('#inputNewPassword2').hide();

            $('#changeName-btn').show();

            $('#changeMail-btn').hide();
            $('#changePassword-btn').hide();

            $('#titleForm').text("Cambiar Nombre");
        },
        changeEmail: function() {
            $('#inputPerfilName').hide();
            $('#inputPerfilEmail').show();
            $('#inputNewPassword').hide();
            $('#inputNewPassword2').hide();

            $('#changeMail-btn').show();

            $('#changeName-btn').hide();
            $('#changePassword-btn').hide();

            $('#titleForm').text("Cambiar Email");
        },
        changePassword: function() {
            $('#inputPerfilName').hide();
            $('#inputPerfilEmail').hide();
            $('#inputNewPassword').show();
            $('#inputNewPassword2').show();

            $('#changePassword-btn').show();

            $('#changeName-btn').hide();
            $('#changeMail-btn').hide();

            $('#titleForm').text("Cambiar Contraseña");
        },
        GetPlayer: function() {
            axios.get('/api/settings')
                .then(result => {
                    this.name = result.data.name,
                        this.email = result.data.email,
                        this.avatar = result.data.avatar
                })
                .catch(error => {
                    console.log("error, código de estatus: " + error.response.status);
                });
        },
        ChangeName: function() {
            axios.post('/api/settings/name', {
                    name: this.name,
                    password: this.password
                })
                .then(result => {
                    this.name = result.data.name;
                    password = "";
                    const message = $('.messageNone');
                    message.text("Modificado");
                    message.removeClass('messageNone');
                    message.addClass('message');

                    setTimeout(() => {
                        message.removeClass('message');
                        message.addClass('messageNone');
                    }, 3000);

                })
                .catch(error => {
                    const message = $('.messageNone');
                    message.text(error.response.data);
                    message.removeClass('messageNone');
                    message.addClass('messageError');

                    setTimeout(() => {
                        message.removeClass('messageError');
                        message.addClass('messageNone');
                    }, 3000);
                })
        },

        ChangeMail: function() {
            axios.post('/api/settings/mail', {
                    Email: this.email,
                    password: this.password
                })
                .then(result => {
                    this.mail = result.data.email;
                    password = "";
                    const message = $('.messageNone');
                    message.text("Modificado");
                    message.removeClass('messageNone');
                    message.addClass('message');

                    setTimeout(() => {
                        message.removeClass('message');
                        message.addClass('messageNone');
                    }, 3000);
                })
                .catch(error => {
                    const message = $('.messageNone');
                    message.text(error.response.data);
                    message.removeClass('messageNone');
                    message.addClass('messageError');

                    setTimeout(() => {
                        message.removeClass('messageError');
                        message.addClass('messageNone');
                    }, 3000);
                })
        },
        ChangePassword: function() {
            axios.post('/api/settings/password', {
                    password: this.password,
                    newPassword: this.newPassword,
                    newPasswordRepeat: this.newPasswordRepeat,
                })
                .then(result => {
                    password = "";
                    newPassword = "";
                    newPasswordRepeat = "";
                    const message = $('.messageNone');
                    message.text("Modificado");
                    message.removeClass('messageNone');
                    message.addClass('message');

                    setTimeout(() => {
                        message.removeClass('message');
                        message.addClass('messageNone');
                    }, 3000);
                })
                .catch(error => {
                    const message = $('.messageNone');
                    message.text(error.response.data);
                    message.removeClass('messageNone');
                    message.addClass('messageError');

                    setTimeout(() => {
                        message.removeClass('messageError');
                        message.addClass('messageNone');
                    }, 3000);
                })
        },
        GetPermisses: function() {
            axios.get('/api/settings/permisse')
                .then(result => {
                    this.permisse = result.data;
                })
                .catch(error => {
                    this.permisse = error.data;
                    window.location.href = '/index.html';
                })
        },

        ChangeAvatar: function() {
            let input = $("#avatar-form input[type='radio']:checked").val();
            axios.put('/api/settings/avatar', {
                    avatar: input
                })
                .then(result => {
                    this.avatar = input;
                    setTimeout(() => {
                        $('.close').click();
                    }, 200);
                })
                .catch(error => {
                    console.log("Error, Código de status: " + error.response.status);
                })
        },
        GetInfoPlayer: function() {
            axios.get('/api/games/infoPlayer')
                .then(result => {
                    this.wins = result.data.wins + ' Victorias';
                    this.winsPercent = Math.round((result.data.wins * 100) / result.data.totalGamesPlayed);
                    this.defeats = result.data.defeats + ' Derrotas';
                    this.defeatsPercent = Math.round((result.data.defeats * 100) / result.data.totalGamesPlayed);
                    this.ties = result.data.ties + ' Empates';
                    this.tiesPercent = Math.round((result.data.ties * 100) / result.data.totalGamesPlayed);
                    this.totalGamesPlayed = result.data.totalGamesPlayed;

                    $('.wins').width(this.winsPercent + '%');
                    $('.ties').width(this.tiesPercent + '%');
                    $('.defeats').width(this.defeatsPercent + '%');

                    $('.winCount').width(this.winsPercent + '%');
                    $('.tiesCount').width(this.tiesPercent + '%');
                    $('.defeatsCount').width(this.defeatsPercent + '%');
                })
                .catch(error => {
                    console.log("Error, Código de status: " + error.response.status);
                })
        },

    }

})