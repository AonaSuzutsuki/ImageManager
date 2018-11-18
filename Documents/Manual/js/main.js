function createMenu() {
    var base = $('<div id="sub-toc" />');

    base.append('<h2>Functions</h2>');

    var inbase = $('<div class="method-elem" />');
    insertContentsMenu(inbase, '#properties');
    insertContentsMenu(inbase, '#constractors');
    insertContentsMenu(inbase, '#statics');
    insertContentsMenu(inbase, '#methods');
    base.append(inbase);

    return base;
}

function createSubMenu() {
    var ol = $('<ol class="menu" />');
    var body = $('body').find('h2');
    body.each(function (index, elem) {
        var e = $(elem);
        var id = e.parent()[0].id;
        ol.append('<li><a href="#' + id + '">' + e.text() + '</a></li>');
    });
    return ol;
}

function insertNewLine(base) {
    base.append('<br />');
}

function insertContentsMenu(base, id) {
    var elem = createContentsMenu(id);
    if (elem !== null) {
        insertNewLine(base);
        base.append(elem);
    }
}

function createContentsMenu(id) {
    if ($(id).length) {
        var ol = $('<table class="functions" />');
        var body = $('body').find(id);
        var h2 = body.find('h2');
        var methodElem = body.find('.method-elem');
        ol.append('<caption>' + h2.text() + '</caption>');

        methodElem.each(function (index, elem) {
            var tr = $('<tr />');
            var h3 = $(elem).find('h3')[0];
            var id = h3.id;
            var content = $(elem).find('.elem-content');

            tr.append('<th><a href="#' + id + '">' + h3.innerText + '</a></th><td>' + content.text() + '</td>');
            ol.append(tr);
        });
        return ol;
    }
    return null;
}