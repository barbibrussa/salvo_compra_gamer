var app = new Vue({
    el: '#app',
    data: {
        games: [],
        scores: [],
        topLocations: [],
        topTypes: [],
        name: "",
        email: "",
        password: "",
        modal: {
            tittle: "",
            message: ""
        },
        currentPage: 1,
        perPage: 7,
        player: null,
        avatar: null,
        vuetify: new Vuetify()
    },
    mounted() {
        this.getGames();
        this.getTopLocations();
        this.getTopTypes();
    },
    methods: {
        joinGame(gId) {
            var gpId = null;
            axios.post('/api/games/' + gId + '/players')
                .then(response => {
                    gpId = response.data;
                    window.location.href = '/game.html?gp=' + gpId;
                })
                .catch(error => {
                    alert("erro al unirse al juego");
                });
        },
        createGame() {
            var gpId = null;
            axios.post('/api/games')
                .then(response => {
                    gpId = response.data;
                    window.location.href = '/game.html?gp=' + gpId;
                })
                .catch(error => {
                    alert("erro al obtener los datos");
                });
        },
        returnGame(gpId) {
            window.location.href = '/game.html?gp=' + gpId;
        },
        getGames: function() {
            this.showLogin(false);
            axios.get('/api/games')
                .then(response => {
                    this.player = response.data.email;
                    this.games = response.data.games;
                    this.avatar = response.data.avatar;
                    this.getScores(this.games)
                    if (this.player == "Guest")
                        this.showLogin(true);
                })
                .catch(error => {
                    alert("erro al obtener los datos");
                });
        },
        showModal: function(show) {
            if (show)
                $("#infoModal").modal('show');
            else
                $("#infoModal").modal('hide');
        },
        showLogin: function(show) {
            if (show) {
                $('#botonLogin').show();
                $('#botonRegister').show();
                this.email = "";
                this.password = "";
            } else {
                $('#exampleModal').modal('hide');
                $('#botonLogin').hide();
                $('#botonRegister').hide();
                /*$("#botonLogin").css({
                    display: "none",
                    visibility: "hidden"
                });*/
            }
        },


        mostrarFormLogin: function() {
            $('#signin-btn').hide();
            $('#login-btn').show();
            $('#titleForm').text("Login");
            $('#inputLoginName').hide();
        },

        mostrarFormRegister: function() {
            $('#login-btn').hide();
            $('#signin-btn').show();
            $('#titleForm').text("Register");
            $('#inputLoginName').show();
        },



        getTopLocations: function() {
            axios.get('/api/games/topLocations')
                .then(response => {
                    this.topLocations = response.data;
                })
                .catch(error => {
                    alert("error al obtener los datos");
                });
        },
        getTopTypes: function() {
            axios.get('/api/games/topTypes')
                .then(response => {
                    this.topTypes = response.data;
                })
                .catch(error => {
                    alert("error al obtener los datos");
                });
        },

        moveToPerfil: function() {
            window.location.href = '/perfil.html';
        },




        logout: function() {
            axios.post('/api/auth/logout')
                .then(result => {
                    if (result.status == 200) {
                        this.showLogin(true);
                        this.getGames();
                    }
                })
                .catch(error => {
                    alert("Ocurrió un error al cerrar sesión");
                });
        },
        login: function(event) {
            axios.post('/api/auth/login', {
                    email: this.email,
                    password: this.password
                })
                .then(result => {
                    if (result.status == 200) {
                        this.showLogin(false);
                        this.getGames();
                    }
                })
                .catch(error => {

                    const message = $('.messageNone');
                    message.text("Email o contraseña invalidos");
                    message.removeClass('messageNone');
                    message.addClass('messageError');

                    setTimeout(() => {
                        message.removeClass('messageError');
                        message.addClass('messageNone');
                    }, 4000);
                });
        },
        signin: function(event) {
            axios.post('/api/players', {
                    name: this.name,
                    email: this.email,
                    password: this.password
                })
                .then(result => {
                    if (result.status == 201) {
                        this.login();
                    }
                })
                .catch(error => {
                    const message = $('.messageNone');
                    let messageFront = VerifyForm(this.name, this.email, this.password);
                    if (messageFront === "Ok") {
                        message.text(error.response.data);
                    } else {
                        message.text(messageFront);
                    }
                    message.removeClass('messageNone');
                    message.addClass('messageError');

                    setTimeout(() => {
                        message.removeClass('messageError');
                        message.addClass('messageNone');
                    }, 4000);
                });
        },
        getScores: function(games) {
            var scores = [];
            games.forEach(game => {
                game.gamePlayers.forEach(gp => {
                    var index = scores.findIndex(sc => sc.email == gp.player.email)
                    if (index < 0) {
                        var score = { email: gp.player.email, win: 0, tie: 0, lost: 0, total: 0 }
                        switch (gp.point) {
                            case 1:
                                score.win++;
                                break;
                            case 0:
                                score.lost++;
                                break;
                            case 0.5:
                                score.tie++;
                                break;
                        }
                        score.total += gp.point;
                        scores.push(score);
                    } else {
                        switch (gp.point) {
                            case 1:
                                scores[index].win++;
                                break;
                            case 0:
                                scores[index].lost++;
                                break;
                            case 0.5:
                                scores[index].tie++;
                                break;
                        }
                        scores[index].total += gp.point;
                    }
                })
            })

            scores.sort(function(a, b) {
                // a and b will here be two objects from the array 
                // thus a[1] and b[1] will equal the names 
                // if they are equal, return 0 (no sorting) 
                if (a['total'] == b['total']) { return 0; }
                if (a['total'] > b['total']) {
                    // if a should come after b, return 1 
                    return -1;
                } else { // if b should come after a, return -1 
                    return 1;
                }
            });
            console.log("Score despues de cortar");

            app.scores = scores.slice(0, 5);
        },
        getImgUrl(image) {
            return `../images/${image}.png`
        },


    },
    filters: {
        dateFormat(date) {
            return moment(date).format('DD-MM-YYYY');
        }
    },
    computed: {
        lists() {
            const items = this.games;
            // Return just page of items needed
            return items.slice(
                (this.currentPage - 1) * this.perPage,
                this.currentPage * this.perPage
            )
        },
        totalRows() {
            return this.games.length
        },

    },
    created() {
        this.getGames();
    }
})

VerifyForm = (name = "", mail = "", password = "") => {
    if (name === "") {
        return "Debe ingresar un nombre";
    }
    if (mail === "") {
        return "Debe ingresar un email";
    }
    if (password === "") {
        return "Debe ingresar una contraseña";
    }
    return "Ok";
};