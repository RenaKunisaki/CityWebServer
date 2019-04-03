class Transit {
    constructor(app) {
        this.app = app;
        this.updateInterval = 10000; //msec
        this.element = $('<table class="transit">');
        this.header = $('<tr class="header">').append(
            $('<th>').text("Route") .attr('title', "Route Name"),
            $('<th>').text("Stops") .attr('title', "Number of stops"),
            $('<th>').text("Vhcls") .attr('title', "Number of vehicles"),
            $('<th>').text("Riders").attr('title', "Number of passengers"),
        );
    }

    run() {
        window.setInterval(() => {this._refresh()}, this.updateInterval);
        console.log("Transit online.")
        this._refresh();
    }

    _refresh() {
        $.getJSON('/Transport', (data) => {
            const rows = [];
            const totals = {};

            for(const route of data) {
                const row = $('<tr>');
                //rows.push(row);

                const name = route.Name;
                let cls = '';
                if     (name.indexOf("Blimp") == 0) cls = 'blimp';
                else if(name.indexOf("Bus")   == 0) cls = 'bus';
                else if(name.indexOf("Ferry") == 0) cls = 'ferry';
                else if(name.indexOf("Metro") == 0) cls = 'metro';
                else if(name.indexOf("Train") == 0) cls = 'train';
                row.addClass(cls);

                let numPassengers = 0;
                for(let passType of route.Passengers) {
                    numPassengers += passType.Amount;
                }

                if(totals[cls] == undefined) {
                    totals[cls] = {
                        type:       cls,
                        routes:     0,
                        stops:      0,
                        vehicles:   0,
                        passengers: 0,
                    };
                }
                totals[cls].routes++;
                totals[cls].stops      += route.StopCount;
                totals[cls].vehicles   += route.VehicleCount;
                totals[cls].passengers += numPassengers;

                /* row.append(
                    $('<td class="name">').text(route.Name),
                    $('<td class="stops">').number(route.StopCount),
                    $('<td class="vehicles">').number(route.VehicleCount),
                    $('<td class="passengers">').number(numPassengers),
                ); */
            }

            for(const cls in totals) {
                const total = totals[cls];
                rows.unshift(
                    $('<tr class="total">').append(
                        $('<td class="name">').text(
                            `${total.type} (${total.routes})`),
                        $('<td class="stops">').number(total.stops),
                        $('<td class="vehicles">').number(total.vehicles),
                        $('<td class="passengers">').number(total.passengers),
                    )
                );
            }

            this.element.empty().append(this.header, rows);
        });
    }
}
