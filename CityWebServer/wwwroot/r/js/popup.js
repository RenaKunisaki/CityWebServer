class Popup {
    constructor(params) {
        const closeBtn = $('<button class="close">')
        .on('click', e => this.close());
        this.element = $('<div class="popup">').addClass(params.classes).append(
            $('<h1>').text(params.title).append(
                $('<div class="buttons">').append(closeBtn),
            ),
            params.body,
        );
    }

    show() {
        $('body').append(this.element);
    }

    close() {
        this.element.remove();
    }
}
