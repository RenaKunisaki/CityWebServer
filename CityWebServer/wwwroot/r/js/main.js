class App {
    constructor() {
        this.updateInterval = 2000; //msec
        this._isInit        = false;
        this.viewModel      = null;
        this.currentDate    = null;
        this.budget         = new Budget(this);
        this.chirper        = new Chirper(this);
        this.transit        = new Transit(this);
    }

    run() {
        window.setInterval(() => {
            this._refresh();
        }, this.updateInterval);
        
        $('#budget').append(this.budget.element);
        $('#chirper').append(this.chirper.element);
        $('#transit').append(this.transit.element);
        
        this.budget.run();
        this.chirper.run();
        this.transit.run();
    }

    _refresh() {
        $.getJSON('/CityInfo', (data) => {
            console.log("Got response", data);
            if(!this._isInit) {
                this.viewModel = ko.mapping.fromJS(data);
                ko.applyBindings(this.viewModel);
                //chart = initializeChart(viewModel);
                this.currentDate = this.viewModel.Time();
                this._isInit = true;
                return;
            }

            ko.mapping.fromJS(data, this.viewModel);
            if (this.currentDate !== this.viewModel.Time()) {
                //updateChart(viewModel, chart);
                this.currentDate = this.viewModel.Time();
            } else {
                //console.log("Same Date: " + lastDate);
            }
        });
    }
}

$(() => { //run when window loaded
    const app = new App();
    app.run();
});
