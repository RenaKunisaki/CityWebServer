class App {
    constructor() {
        this.updateInterval = 2000; //msec
        this._isInit        = false;
        this.viewModel      = null;
        this.currentDate    = null;
        this.budget         = new Budget(this);
        this.chirper        = new Chirper(this);
        this.transit        = new Transit(this);

        this.monthNames = [ //XXX get from game for localization
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
        ];
    }

    run() {
        window.setInterval(() => {
            this._refresh();
        }, this.updateInterval);

        $('#chirper').append(this.chirper.element);
        $('#transit').append(this.transit.element);

        this.budget.run();
        this.chirper.run();
        this.transit.run();
    }

    _refresh() {
        $.getJSON('/CityInfo', (data) => {
            this.data = data;
            this.currentDate = new Date(this.data.Time);
            document.title = this.data.Name;

            //build displayed date string
            let day = this.currentDate.getDay();
            if(day < 10) day = '0' + day;
            let month = this.monthNames[this.currentDate.getMonth()-1];
            this.data.friendlyDate =
                this.currentDate.getFullYear() + ' ' + month + ' ' + day;

            if(!this._isInit) {
                this.viewModel = ko.mapping.fromJS(this.data);
                ko.applyBindings(this.viewModel);

                //chart = initializeChart(viewModel);
                this._isInit = true;
                return;
            }

            ko.mapping.fromJS(this.data, this.viewModel);
        });
    }
}

$(() => { //run when window loaded
    const app = new App();
    app.run();
});
