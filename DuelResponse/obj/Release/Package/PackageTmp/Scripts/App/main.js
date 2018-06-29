(function (EMP, $, undefined) {
    EMP.BlockData = {
        "msg": [
            { "seq": 1, "title": "No - Click Me!", "outcome": "Check your email...", "playWav": "/Content/youGotmail.wav", "url": "/Home/SendMail" },
            { "seq": 2, "title": "LuCKy NuMb3r 2?", "outcome": "Better schedule that appointment...", "url": "/Home/Appointment", "newWindow": true },
            { "seq": 3, "title": "PLEASE click me", "outcome": "We see your 3D imaging and raise you web-invoked multithreading", "url": "/Home/CallThem" },
            { "seq": 4, "title": "WARNING - not for clicking", "outcome": "Wrong move - NO SCREEN FOR U!", "playWav": "/Content/nosoup.mp3" },
            { "seq": 5, "title": "?", "outcome": "No dice - try again"},
            {
                "seq": 6, "title": "Don't even think about it", "outcome": "Just be sure not to forward that one...", "url": "/Home/MailHarper",
                    "method": "POST", "data": {
                        "subject": "Hey Baby!",
                        "body": "Saturday night was <span style='font-style:italic; color: #FF0000;'>unforgettable</span>.  Is your wife still out of town?"
                    }
            },
            { "seq": 7, "title": "Whaa?", "outcome": "-", "playWav": "/Content/buzz.wav" },
            { "seq": 8, "title": "Give me a try", "outcome": "Better get to that community service", "url": "/Home/CallHarper"},
            { "seq": 9, "title": "Uh oh, really???", "outcome": "Hmmmm", "url": "/Home/GetRandomPair", "useResponse": true }
        ]
    };

    EMP.GoFullscreen = function () {
        if (screenfull.enabled) {
            screenfull.request($('body')[0]);
        } else {
            // Ignore or do something else
        }
    };

    EMP.RandomColor = function () {
        var letters = '0123456789ABCDEF';
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    };

    EMP.AnimateShow = function (word, delay) {
        setTimeout(function () {
            $('#intro').html(word);
        }, delay);
    };

    EMP.ShowIntro = function (onComplete) {
        var ITEM_DELAY = 600;
        var words = ['Shall', 'we', 'play', 'a', 'game?', '--', '5elect ur fate'];
        for (var i = 0; i < words.length; i++) {
            EMP.AnimateShow(words[i], i * ITEM_DELAY);
        }
        setTimeout(function () {
            $('#intro').toggle("bounce", { times: 3 }, "slow");
            setTimeout(function () {
                $('#intro').hide();
                onComplete();
            }, 3000);
        }, (words.length + 1) * ITEM_DELAY);
    };

    EMP.RenderRandomBlocks = function () {
        // using handlebars templating client-side
        var source = $("#box-template").html();
        var template = Handlebars.compile(source);
        var htmlBlocks = template(EMP.BlockData);
        $('div#page-content').append(htmlBlocks);
        $('div.selection div').tooltip();
        $('div.selection').each(function (i, v) {
            $(this).animate({ "background-color": EMP.RandomColor() }, "slow");
        });
    };
/*
    EMP.DisableEnterKey = function () {
        $("input").keypress(function (evt) {
            var charCode = evt.charCode || evt.keyCode;
            if (charCode === 13) return false;
        });
    };

    EMP.ScrollToView = function (element) {
        if (element === null || element === undefined) return;
        var oset = element.offset();
        if (oset === null || oset === undefined) return;
        var offset = oset.top;
        var visible_area_start = $(window).scrollTop();
        var visible_area_end = visible_area_start + window.innerHeight;
        if (offset < visible_area_start || offset > visible_area_end) {
            // Not in view so scroll to it
            $('html,body').animate({ scrollTop: offset - window.innerHeight / 3 }, 1000);
        }
    };
*/

    EMP.PlayWav = function(path) {
        var audio = new Audio(path);
        audio.play();
    };

    EMP.GetMessageBySeq = function (seq) {
        for (var i = 0; i < EMP.BlockData.msg.length; i++) {
            if (EMP.BlockData.msg[i].seq === seq) return EMP.BlockData.msg[i];
        }
        return null;
    };

    EMP.ShowMessage = function (seq, text) {
        $('div.selection[data-seq="' + seq + '"]')
            .unbind('click')
            .html('<div style="font-size: 20pt; padding-top: 5px;">' + text + '</div>');
        setTimeout(function () {
            $('div.selection[data-seq="' + seq + '"]').remove();
        }, 5000);
    };

    EMP.BindSelectionClick = function () {
        $('div.selection').click(function () {
            var seq = parseInt($(this).attr('data-seq'));
            var message = EMP.GetMessageBySeq(seq);
            if (message.playWav) {
                // any actions can have a sound accompanying
                EMP.PlayWav(message.playWav);
            }
            if (message.newWindow) {
                // invoke a server action in a new window
                window.open(message.url);
                EMP.ShowMessage(message.seq, message.outcome);
            } else if (message.url) {
                // invoke a server action and then show the outcome
                var httpMethod = (message.method)
                    ? message.method
                    : 'GET';
                $.ajax({
                    type: httpMethod,
                    data: message.data,
                    url: message.url,
                    success: function (result) {
                        if (result.success) {
                            if (message.useResponse) {
                                EMP.ShowMessage(message.seq, result.Message);
                            } else {
                                EMP.ShowMessage(message.seq, message.outcome);
                            }
                        }
                    }
                });
            } else {
                // no server call, just show the outcome
                EMP.ShowMessage(message.seq, message.outcome);
            }
        });
    };
}(window.EMP = window.EMP || {}, jQuery));