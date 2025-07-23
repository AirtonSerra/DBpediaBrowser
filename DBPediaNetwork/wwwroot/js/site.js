var app = {
    init: function () {
        app.removeSomeeAd();
    },
    preloader: function (action, ms = 500) {
        var display = $('.preloader').css('display');

        if (action == "on" && (display == "none" || display == undefined)) {
            $('.modal.preloader').modal('show');
        } else if (action == "off") {

            setTimeout(function () {
            $('.modal.preloader').modal('hide');
            }, ms);
        }
    },
    removeSomeeAd: () => {
        $("center").remove();
        $("footer ~ div").remove();

        window.removead1 = setInterval(function () {
            if ($("center")) {
                $("center").remove();
                $("footer ~ div").remove();
                clearInterval(window.removead1);
            }
        }, 1);

        window.removead2 = setInterval(function () {
            if ($("footer ~ div:not(.modal)")) {
                $("footer ~ div:not(.modal)").remove();
            }
        }, 1);

        setTimeout(function () {
            clearInterval(window.removead1);
            clearInterval(window.removead2);
        }, 3000);
    }
};

(function () {
    app.init();
})();
