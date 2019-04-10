class Chirper {
    constructor(app) {
        this.app = app;
        this.messages = {};
        this.element = $('<ul class="chirper">');
    }

    run() {
        $('#chirper').append(this.element);
        this.app.registerMessageHandler("Chirper", data => this.update(data));
        console.log("Chirper online.")
    }

    update(messages) {
        if(!Array.isArray(messages)) messages = [messages];
        for(const msg of messages) {
            let key = `${msg.SenderID}:${msg.Time}:${msg.Text}`;
            if(this.messages[key] == undefined) {
                this._onNewMessage(msg);
                this.messages[key] = msg;
            }
        }
    }

    _onNewMessage(msg) {
        //console.log("Chirp!", msg);
        msg.Time = new Date(msg.Time);
        this.element.prepend(
            $('<li class="chirp">').append(
                $('<span class="name">').text(msg.SenderName).css(
                    'color', this.app.makeNameColor(msg.SenderName)
                ),
                $('<span class="time">').text(
                    this.app.formatTimestamp(msg.Time)),
                $('<span class="text">').text(msg.Text),
            )
        );
    }
}
