/** TODO:
 *  - Everything under Watch It!
 *  - Average land value
 *  - City attractiveness
 *  - Disaster logs
 *  - Game limits
 *  - Localized text
 */

class App {
    constructor() {
        this.updateInterval = 2000; //msec
        this._isInit        = false;
        this.viewModel      = null;
        this.currentDate    = null;
        this.budget         = new Budget(this);
        this.chirper        = new Chirper(this);
        this.population     = new Population(this);
        this.problems       = new Problems(this);
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
        //this.population.run();
        this.problems.run();
        this.transit.run();

        $('#main').masonry({
            itemSelector: '.box',
            columnWidth: '.grid-sizer',
            //horizontalOrder: true,
            //percentPosition: true,
            transitionDuration: 0,
            initLayout: true,
        });

        setTimeout(() => {
            $('#main').masonry('layout');
        }, 500);
        this._refresh();

        $.getJSON('/Limits', (data) => { this.limits = data });
    }

    makeNameColor(name) {
        //Try to match game colors
        if(name.startsWith('Residential_Low'))  return '#80FF00';
        if(name.startsWith('Residential_High')) return '#40C000';
        if(name.startsWith('Commercial_Low'))   return '#0080FF';
        if(name.startsWith('Commercial_High'))  return '#0040C0';
        if(name.startsWith('Industrial'))       return '#FF8000';
        if(name.startsWith('Office'))           return '#00C0C0';
        let hue = 0, sat = 0, light = 0;
        for(let i=0; i<name.length; i++) {
            let c = name.charCodeAt(i);
            hue   += (c ^ 0xE2); //arbitrary constants
            sat   += (c ^ 0x79);
            light += (c ^ 0xA5);
        }
        hue %= 360;
        sat = (sat % 50) + 50; //range 50-100
        light = (light % 50) + 25; //range 25-75
        return `hsl(${hue}, ${sat}%, ${light}%)`;
    }

    _updateClock(data) {
        //build displayed date string
        let year   = this.currentDate.getFullYear();
        let month  = this.monthNames[this.currentDate.getMonth()];
        let day    = String(this.currentDate.getDate()).padStart(2, '0');
        let hour   = String(this.currentDate.getHours()).padStart(2, '0');
        let minute = String(this.currentDate.getMinutes()).padStart(2, '0');
        let second = String(this.currentDate.getSeconds()).padStart(2, '0');
        let night  = data.isNight ? '☽' : '☀';

        this.data.friendlyDate =
            `${year} ${month} ${day} · ${hour}:${minute}:${second} ${night}`;
        $('#clock').toggleClass('game-paused', data.isPaused)
    }

    _refresh() {
        $.getJSON('/CityInfo', (data) => {
            //console.log("CityInfo:", data);
            $('#navbar-error').hide();
            this.data = data;
            this.currentDate = new Date(this.data.Time);
            this._updateClock(data);

            document.title = this.data.Name;
            $('#city-name').attr('title',
                `Map: ${data.mapName}\n` +
                `Climate: ${data.environment}`);

            if(!this._isInit) {
                this.viewModel = ko.mapping.fromJS(this.data);
                ko.applyBindings(this.viewModel);

                //chart = initializeChart(viewModel);
                this._isInit = true;
                return;
            }

            this.population.update(data);
            ko.mapping.fromJS(this.data, this.viewModel);
            $('#main').masonry('layout');
        }).fail(() => {
            $('#navbar-error').text("Not connected to game").show();
        });
    }
}

$(() => { //run when window loaded
    const app = new App();
    app.run();
});
