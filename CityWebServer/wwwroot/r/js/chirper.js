class Chirper {
    constructor(app) {
        this.app = app;
        this.updateInterval = 5000; //msec
        this.messages = {};
        this.element = $('<ul class="chirper">');
    }

    run() {
        window.setInterval(() => {this._refresh()}, this.updateInterval);
        console.log("Chirper online.")
        this._refresh();
    }

    _refresh() {
        $.getJSON('/Messages', (messages) => {
            //debugger;
            for(const msg of messages) {
                let key = `${msg.SenderID}:${msg.Text}`;
                if(this.messages[key] == undefined) {
                    this._onNewMessage(msg);
                    this.messages[key] = msg;
                }
            }
        });
    }

    _onNewMessage(msg) {
        //console.log("Chirp!", msg);
        this.element.prepend(
            $('<li class="chirp">').append(
                $('<span class="name">').text(msg.SenderName).css(
                    'color', this.app.makeNameColor(msg.SenderName)
                ),
                $('<span class="text">').text(msg.Text),
            )
        );
    }
}
