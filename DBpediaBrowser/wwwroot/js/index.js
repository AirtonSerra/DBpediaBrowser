var appIndex = {
    data: null,
    network: null,
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

        $("#btnExport").click(function () {
            appIndex.exportGraph();
        });
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

            if (data.nodes.length > 0)
                $("#btnExport").show();
            else
                $("#btnExport").hide();

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
            appIndex.network = new vis.Network(container, data, options);

            appIndex.network.on('click', function (properties) {
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

            appIndex.network.on("oncontext", function (params) {
                params.event.preventDefault();
                let nodeid = appIndex.network.getNodeAt(params.pointer.DOM);
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
    },
    exportGraph: function () {
        //Gera manualmente o SVG
        //O vis mantém um canvas interno inacessível

        if (!appIndex.network || !appIndex.data)
            return;

        //Largura svg
        const positions = appIndex.network.getPositions();

        let minX = Infinity;
        let minY = Infinity;
        let maxX = -Infinity;
        let maxY = -Infinity;
        
        appIndex.data.nodes.forEach(node => {
            const p = positions[node.id];
            if (!p)
                return;

            const shape = appIndex.network.body.nodes[node.id].shape;
            const box = shape.boundingBox;

            minX = Math.min(minX, p.x + box.left);
            minY = Math.min(minY, p.y + box.top);

            maxX = Math.max(maxX, p.x + box.right);
            maxY = Math.max(maxY, p.y + box.bottom);

        });
        
        const margin = 40;

        minX -= margin;
        minY -= margin;
        maxX += margin;
        maxY += margin;

        const width = maxX - minX;
        const height = maxY - minY;

        let svg = [];

        svg.push(`<svg xmlns="http://www.w3.org/2000/svg"
            width="${width}"
            height="${height}"
            viewBox="${minX} ${minY} ${width} ${height}">`
        );

        svg.push(`<style>
            text{
                font-family: Arial;
                font-size:14px;
            }
            </style>`
        );

        // Arestas

        appIndex.data.edges.forEach(edge => {

            const from = positions[edge.from];
            const to = positions[edge.to];

            if (!from || !to)
                return;

            svg.push(`<line
                    x1="${from.x}"
                    y1="${from.y}"
                    x2="${to.x}"
                    y2="${to.y}"
                    stroke="${edge.color}"
                    stroke-width="1.0"
                />`
            );

            if (edge.label) {
                const mx = (from.x + to.x) / 2;
                const my = (from.y + to.y) / 2;

                svg.push(`
                <text
                    x="${mx}"
                    y="${my-4}"
                    text-anchor="middle"
                    fill="#444">
                ${edge.label}
                </text>`);
            }

        });

        // Nós
        appIndex.data.nodes.forEach(node => {
            const p = positions[node.id];
            if (!p)
                return;

            const internalNode = appIndex.network.body.nodes[node.id];
            const shape = internalNode.shape;
            const width = shape.width;
            const height = shape.height;

            if (node.shape === "box") {
                svg.push(`<rect
                        x="${p.x - width / 2}"
                        y="${p.y - height / 2}"
                        width="${width}"
                        height="${height}"
                        rx="4"
                        fill="${typeof node.color === "object"
                                ? node.color.background
                                : node.color}"
                        stroke="${typeof node.color === "object"
                                    ? node.color.border
                                    : node.color}"
                        stroke-width="1.5"
                    />`);
            }
            else {
                svg.push(`<ellipse
                    cx="${p.x}"
                    cy="${p.y}"
                    rx="${width / 2}"
                    ry="${height / 2}"
                    fill="${typeof node.color === "object"
                                ? node.color.background
                                : node.color}"
                    stroke="${typeof node.color === "object"
                                    ? node.color.border
                                    : node.color}"
                    stroke-width="1.0"
                    />`
                );
            }

            svg.push(`<text
                x="${p.x}"
                y="${p.y + shape.textSize.height / 2 - 2}"
                text-anchor="middle"
                font-family="${internalNode.options.font.face}"
                font-size="${internalNode.options.font.size}"
                fill="${internalNode.options.font.color}">
                    ${node.label}
                </text>`
            );
        });

        svg.push("</svg>");

        //Download

        const blob = new Blob(
            [svg.join("")],
            { type: "image/svg+xml;charset=utf-8" }
        );

        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");

        a.href = url;
        a.download = "graph.svg";

        document.body.appendChild(a);

        a.click();

        document.body.removeChild(a);

        URL.revokeObjectURL(url);
    }
};


(function () {
    appIndex.init();
})();