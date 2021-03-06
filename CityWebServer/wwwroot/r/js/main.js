/** TODO:
 *  - District:
 *    - CremateCapacity
 *    - DeadCount (is this how many people are waiting to be moved?)
 *    - ImportAmount, ExportAmount (units?)
 *      - Overall natural resource amounts
 *    - GarbageAccumulation, GarbagePiles, IncinerationCapacity
 *    - IncomeAccumulation (what is this?)
 *  - DistrictManager: m_districtCount, m_parkCount
 *  - EventManager
 *  - Road wetness
 *  - Busiest roads by name
 *  - PathManager: m_pathUnitCount
 *  - SimulationManager: m_realTimer (what is this?)
 *  - VehicleManager: m_maxTrafficFlow, m_totalTrafficFlow
 *     what are these? node IDs?
 *  - Weather, wind
 *  - ZoneManager: demand
 *  - Noise pollution
 *  - Availability/coverage of police, fire, healthcare
 *  - Fire hazard rate
 *  - Crime rate
 *  - Grab info from StatsManager or whatever
 *  - Cyclists, Tourists
 *  - Happiness, Health, Leisure
 *  - Average land value
 *  - City attractiveness
 *  - Road/Park maintenance
 *  - Disaster logs, escape route coverage, radio coverage, detection
 *  - Postal stuff
 *  - Localized text
 *  - Figure out how to get updated game limits, eg trees
 *
 * Ideas:
 * - Have map zoom to camera height (RenderManager.CameraHeight)
 *   - Have it rotate
 * - A district selection dropdown in the top navbar, to show stats for
 *   that district instead of whole city
 */

class App {
    constructor() {
        this._isInit        = false;
        this.viewModel      = null;
        this.currentDate    = null;
        this.chirper        = new Chirper(this);
        //this.heightMap      = new HeightMap(this);
        this.budget         = new Budget(this);
        this.population     = new Population(this);
        this.limits         = new Limits(this);
        this.problems       = new Problems(this);
        this.resources      = new Resources(this);
        this.stats          = new Stats(this);
        //this.transit        = new Transit(this);

        this.monthNames = [ //XXX get from game for localization
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
        ];
        this.messageHandlers = {};
        this.data = {};
        this._registerjQueryExtensions();
    }

    _registerjQueryExtensions() {
        function isNumber(val) {
            return (val != undefined) && (val != null) &&
            !isNaN(val) && isFinite(val);
        }
        jQuery.fn.extend({
            money: function(num) {
                //$(...).money(num)
                //formats num (in cents) as amount of money
                //doesn't add currency symbol, does toggle "negative" class
                if(!isNumber(num)) $(this).text("-");
                else {
                    $(this).text(Math.round(num / 100).toLocaleString());
                    $(this).toggleClass('negative', num < 0);
                    $(this).toggleClass('zero', Math.round(num/100) == 0);
                }
                return this;
            },
            percent: function(num) {
                if(!isNumber(num)) $(this).text("-");
                else $(this).text(num.toFixed(0)+'%');
                return this;
            },
            //permyriad: percent of a percent
            //used because the game gives some percentages this way
            //eg 15% = 1500
            permyriad: function(num) {
                if(!isNumber(num)) $(this).text("-");
                else $(this).text((num / 100).toFixed(2)+'%');
                return this;
            },
            //there's also number() from jquery.number.min.js
        });
    }

    run() {
        //Create socket to receive data from game.
        this.socket = new WebSocket(`ws://${window.location.host}/Socket`);
        console.log("Created socket", this.socket);

        this.socket.onopen = (event) => {
            console.log("Socket opened");
            this.socket.send("{hello_there_server_is_this_message_long_enough_for_you:42}");
        };
        this.socket.onclose = (event) => {
            console.log("Socket closed");
            $('#navbar-error').text("Not connected to game").show();
        };
        this.socket.onerror = (event) => {
            console.log("Socket error", event);
            $('#navbar-error').text("Not connected to game").show();
        };
        this.socket.onmessage = (event) => {
            this._onMessage(event.data);
        };

        //Set up some default handlers.
        this.registerMessageHandler("Tick", data => this._onTick(data));
        this.registerMessageHandler("CityInfo", data => this._updateCityInfo(data));


        //Set up layout.
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
    }

    registerMessageHandler(name, handler) {
        /** Register a callback for a message from the game.
         */
        if(this.messageHandlers[name] == undefined) {
            this.messageHandlers[name] = [];
        }
        this.messageHandlers[name].push(handler);
    }

    unregisterMessageHandler(name, handler) {
        if(this.messageHandlers[name] == undefined) return;
        this.messageHandlers[name].splice(
            this.messageHandlers[name].indexOf(handler, 1));
    }

    query(message, response) {
        const self = this;
        let _resolve = null;
        const promise = new Promise(resolve => {
            _resolve = resolve;
        });
        function handler(data) {
            self.unregisterMessageHandler(message, handler);
            _resolve(data);
        }
        this.registerMessageHandler(response, handler);
        this.socket.send(JSON.stringify(message));
        return promise;
    }

    makeNameColor(name) {
        /** Generate a color based on a string.
         */
        //Try to match game colors
        if(name.startsWith('Residential_Low'))  return '#80FF00';
        if(name.startsWith('Residential_High')) return '#40C000';
        if(name.startsWith('Commercial_Low'))   return '#0080FF';
        if(name.startsWith('Commercial_High'))  return '#0040C0';
        if(name.startsWith('Industrial'))       return '#FF8000';
        if(name.startsWith('Office'))           return '#00C0C0';
        if(name.startsWith('Water'))            return '#009DF3';
        if(name.startsWith('Sewer'))            return '#802010';
        if(name.startsWith('Electricity'))      return '#C0C000';
        if(name.startsWith('Heat'))             return '#C000C0';
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

    formatTimestamp(time) {
        let year   = time.getFullYear();
        let month  = this.monthNames[time.getMonth()];
        let day    = String(time.getDate()).padStart(2, '0');
        let hour   = String(time.getHours()).padStart(2, '0');
        let minute = String(time.getMinutes()).padStart(2, '0');
        let second = String(time.getSeconds()).padStart(2, '0');
        return `${year} ${month} ${day} · ${hour}:${minute}:${second}`
    }

    _onTick(Tick) {
        /** Callback for Tick message.
         */
        //build displayed date string
        this.currentDate = new Date(Tick.Time);
        let night = Tick.isNight ? ' ☽' : ' ☀';

        this.data.friendlyDate = this.formatTimestamp(this.currentDate) + night;
        $('#clock').toggleClass('game-paused', Tick.isPaused)

        //Update layout
        $('#main').masonry('layout');
    }

    _updateCityInfo(CityInfo) {
        /** Callback for CityInfo message.
         */
        document.title = CityInfo.Name;
        /* $('#city-name').attr('title',
            `Map: ${CityInfo.mapName}\n` +
            `Climate: ${CityInfo.environment}`); */
    }

    _init() {
        //this.heightMap.run();
        this.chirper.run();
        this.budget.run();
        this.population.run();
        this.limits.run();
        this.problems.run();
        this.resources.run();
        this.stats.run();
        //this.transit.run();
        //$('#transit').append(this.transit.element);
        this._isInit = true;
    }

    _onMessage(message) {
        /** Called when a new message arrives on the socket.
         */
        if(!this._isInit) this._init();
        $('#navbar-error').hide();
        message = JSON.parse(message);
        this._callMessageHandlers(message);

        //do this after calling handlers so that changes they make
        //are reflected in the result.
        for(const [name, field] of Object.entries(message)) {
            //Merge new data into existing data
            let keys = name.split('.');
            let target = this.data;
            for(const key of keys) {
                if(target[key] == undefined) target[key] = {};
                target = target[key];
            }
            if(field.ID == undefined) merge(target, field, true);
            else {
                target[parseInt(field.ID)] = field;
            }
        }
        this._updateBoundElements();

    }

    _callMessageHandlers(message) {
        for(const [name, field] of Object.entries(message)) {
            const handlers = this.messageHandlers[name];
            if(handlers) {
                for(const handler of handlers) {
                    try { handler(field); }
                    catch(ex) {
                        console.error("Error handling message", name, ex);
                    }
                }
            }
        }
    }

    _updateBoundElements() {
        $("[data-bind]").each((idx, elem) => {
            const binding = $(elem).attr('data-bind').split(':');
            const prop = binding[0];
            const fields = binding[1].split('.');
            let value = this.data;
            for(const field of fields) {
                value = value[field];
                if(value == undefined) break;
            }
            $(elem)[prop](value);
        });
    }

    _refresh() {
        //XXX this is old and no longer used.
        $.getJSON('/CityInfo', (data) => {
            //console.log("CityInfo:", data);
            //XXX only do this if GameAreaManager.m_areaCount changed
            this.heightMap.updateLockedTiles(data);
            this.population.update(data);

        }).fail(() => {

        });
    }
}

$(() => { //run when window loaded
    window.app = new App();
    window.app.run();
});
