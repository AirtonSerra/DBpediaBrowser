var appIndex = {
    data: null,
    init: function () {
        appIndex.setCanvasHeight();
        appIndex.buttonFunctions();
        appIndex.hideRigthClickMenu();
        appIndex.autocompleteSearch();
    },
    setCanvasHeight: function () {
        $(window).resize(function () {
            appIndex.resizeCanvas();
        }).trigger('resize');
    },
    resizeCanvas: () => {
        if ($('#mynetwork').length) {
            $('#mynetwork').css("height", 0);
            let height = ($('#mynetwork').offset().top - $('footer').offset().top) * -1;
            $('#mynetwork').css("height", height);
        }
    },
    buttonFunctions: () => {
        $("#btnSearch").click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            var source = $("#inpSearch").val().trim();

            if (source !== "") {
                var objPost = {
                    pesquisa: "http://dbpedia.org/resource/" + source,
                    qtdRerouces: $("#impResources").val(),
                    qtdLiterais: $("#impLiterais").val(),
                    refresh: $("#checkRefresh").is(":checked")
                }

                $("#caminhoClick").html("<span>" + source + "</span>").css({ display: 'block' });
                appIndex.searchPost(objPost);
                $("#checkRefresh").prop("checked", false);
            }
        });

        $("#inpSearch").on('keyup', function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                $("#btnSearch").click();
            }
        });

        appIndex.modalConfirmacao(removeNode);

        $("#remove").click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            //TODO: Investigar o por que o preloader não fecha se chamando nesse contexto.
            $("#mi-modal").modal('show');
        });

        function removeNode() {

            let id = $("#remove").attr("nodeid");
            let nodeLabel = $("#remove").attr("nodeLabel");

            $.post(HomeAction + "/Home/RemoveNode",
                {
                    id: id
                },
                function (result) {
                    appIndex.buildChart(result);

                    $("#menu").css({
                        opacity: "0"
                    });
                    setTimeout(function () {
                        $("#menu").css({
                            visibility: "hidden"
                        });
                    }, 501);

                    let partialLabel = $("#caminhoClick").html();
                    $("#caminhoClick").html(partialLabel + "<b> > </b>" + "<span style=\"text-decoration: line-through\">" + nodeLabel + "</span>");

                    app.preloader("off");
                })
                .fail(function (result) {
                    console.log("Ocorreu um erro ao Remover.");
                    app.preloader("off");
                });
        }
    },
    searchPost: function (objPost, endpoint = "Search") {
        app.preloader("on");

        $.post(HomeAction + "/Home/" + endpoint,
            { filterModel: objPost },
            function (result) {
                appIndex.buildChart(result);
                appIndex.resizeCanvas();
                app.preloader("off");
            })
            .fail(function (result) {
                console.log("Ocorreu um erro ao pesquisar.");
                app.preloader("off");
            });

    },
    buildChart: function (data) {
        if (data != null) {
            appIndex.data = data;

            // Destaques para nodes Literais.
            data.nodes.forEach(function (node) {
                if (node.shape == "box") {
                    let background = node.color;

                    node.borderWidth = 2;
                    node.color = {
                        border: background,
                        background: "#FFF",
                        highlight: {
                            border: background,
                            background: "#FFF"
                        },
                        hover: {
                            border: background,
                            background: "#FFF"
                        }
                    }
                }
            });


            var container = document.getElementById("mynetwork");
            var options = {};
            var network = new vis.Network(container, data, options);

            network.on('click', function (properties) {
                var nodeid = properties.nodes[0];
                if (nodeid > 0) {
                    clickedNode = appIndex.data.nodes.find(obj => { return obj.id === nodeid });
                    if (clickedNode.source.includes("resource/") && !clickedNode.clicked) {
                        clickedNode.clicked = true;
                        let objPost = {
                            pesquisa: clickedNode.source,
                            qtdRerouces: $("#impResources").val(),
                            qtdLiterais: $("#impLiterais").val()
                        }

                        let partialLabel = $("#caminhoClick").html();
                        $("#caminhoClick").html(partialLabel + "<b> > </b>" + "<span>" + clickedNode.label + "</span>");

                        appIndex.searchPost(objPost, "ExpandChart");
                    }
                    console.log(clickedNode);
                }
            });

            network.on("oncontext", function (params) {
                params.event.preventDefault();
                let nodeid = network.getNodeAt(params.pointer.DOM);
                var clickedNode = appIndex.data.nodes.find(obj => { return obj.id === nodeid });
                if (clickedNode) {
                    if (clickedNode.source.includes("resource/")) {
                        $("#menu #redirectDbpedia").attr("href", clickedNode.source);
                        $("#menu #redirectDbpedia").css({ display: "block" });
                    }
                    else {
                        $("#menu #redirectDbpedia").css({ display: "none" });
                    }

                    $("#menu #remove").attr("nodeid", clickedNode.id);

                    $("#menu #remove").attr("nodeLabel", clickedNode.label);

                    $("#menu").css({
                        top: params.event.pageY + "px",
                        left: params.event.pageX + "px",
                        visibility: "visible",
                        opacity: "1"
                    });
                }
                else {
                    $("#menu").css({
                        opacity: "0"
                    });
                    setTimeout(function () {
                        $("#menu").css({
                            visibility: "hidden"
                        });
                    }, 501);
                }
            });
        }
    },
    hideRigthClickMenu: function () {

        if ($('#menu').length) {
            var i = document.getElementById("menu").style;
            var canvas = document.getElementById("mynetwork");

            if (document.addEventListener) {
                canvas.addEventListener('click', function (e) {
                    i.opacity = "0";
                    setTimeout(function () {
                        i.visibility = "hidden";
                    }, 501);
                }, false);
            } else {
                canvas.attachEvent('onclick', function (e) {
                    i.opacity = "0";
                    setTimeout(function () {
                        i.visibility = "hidden";
                    }, 501);
                });
            }
        }
    },
    modalConfirmacao: function (run) {

        $("#modal-btn-yes").on("click", function () {
            run();
            $("#mi-modal").modal('hide');
        });

        $("#modal-btn-no").on("click", function () {
            $("#mi-modal").modal('hide');
        });
    },
    autocompleteSearch: function () {
        //$("#inpSearch").on("input", function (e) {
        //    e.stopPropagation();
        //    e.preventDefault();
        //    debugger;

        //    let search = $(this).val();
        //    if (search.length > 3) {


        //        $.get("Home/AutoCompleteSearch",
        //            { search: search },
        //            function (source) {
        //                debugger;
        //                $("#inpSearch").autocomplete({
        //                    source: source
        //                });
        //                $("#inpSearch").autocomplete('search');
        //            }
        //        );
        //    }
        //});

        let source = $("#autocompleteSource").val().split(", ");

        $("#inpSearch").autocomplete({
            source: source
        });
    }
};


(function () {
    appIndex.init();
})();