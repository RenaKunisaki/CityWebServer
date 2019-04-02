class Budget {
    constructor(app) {
        this.app = app;
        this.updateInterval = 5000; //msec
    }

    _makeCharts() {
        let ctx = $('#income-chart canvas')[0].getContext('2d');
        const options = {
            legend: {
                display: false,
            },
        };
        const labels = ['Foo', 'Bar', 'Baz'];
        const bgColors = [
            '#800000',
            '#008000',
            '#000080',
        ];

        this.chartIncome = new Chart(ctx, {
            type: 'pie',
            options: options,
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: bgColors,
                    data: [],
                }],
            },
        });

        ctx = $('#expense-chart canvas')[0].getContext('2d');
        this.chartExpenses = new Chart(ctx, {
            type: 'pie',
            options: options,
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: bgColors,
                    data: [],
                }],
            },
        });
    }

    _updateCharts(data) {
        //XXX get breakdown data
        this.chartIncome.data.datasets[0].data = [10, 20, 30];
        this.chartExpenses.data.datasets[0].data = [30, 20, 10];
        this.chartIncome.update();
        this.chartExpenses.update();
    }

    run() {
        window.setInterval(() => {this._refresh()}, this.updateInterval);
        //make charts here, now that the elements have hopefully
        //been added to the DOM.
        this._makeCharts();
        //$('#budget-summary').append(this.element);
        console.log("Budget online.")
    }

    _refresh() {
        $.getJSON('/Budget', (data) => {
            const net = data.income - data.expenses;
            $('#income-chart .total').number(data.income / 100)
                .toggleClass('negative', data.income <= 0);
            $('#expense-chart .total').number(data.expenses / 100)
                .toggleClass('negative', data.expenses <= 0);
            $('#net-income').number(net / 100)
                .toggleClass('negative', net <= 0);
            $('#current-cash').number(data.cash / 100)
                .toggleClass('negative', data.cash <= 0);
            $('#num-loans').number(data.numLoans);
            //console.log("Budget data:", data);
            this._updateCharts(data);
        });
    }
}
