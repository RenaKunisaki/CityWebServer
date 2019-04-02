/**
 * Shuffles array in place.
 * @param {Array} a items An array containing the items.
 */
function shuffle(a) {
    var j, x, i;
    for (i = a.length - 1; i > 0; i--) {
        j = Math.floor(Math.random() * (i + 1));
        x = a[i];
        a[i] = a[j];
        a[j] = x;
    }
    return a;
}

﻿class Budget {
    constructor(app) {
        this.app = app;
        this.updateInterval = 5000; //msec
        this._isInit = false;
    }

    _init(data) {
        this._makeCharts(data);
    }

    _makeCharts(data) {
        const legend = $('<ul class="legend">');
        const options = {
            layout: {
                padding: { left: 0, right: 0, bottom: 0, top: 0 },
            },
            legend: {
                /* position: 'right',
                labels: {
                    boxWidth: 20,
                    fontColor: '#FFF',
                    padding:  0,
                }, */
                display: false,
            },
            tooltips: {
                callbacks: {
                    label: (item, data) => {
                        const label = data.labels[item.index];
                        const dset  = data.datasets[item.datasetIndex];
                        const value = dset.data[item.index];
                        const amount = (value / 100).toFixed(0);
                        //XXX format amount as number (commas)
                        return `${label}: ₡${amount}`;
                    },
                },
            },
        };
        const labels      = [];
        const bgColors    = [];
        const incomeData  = [];
        const expenseData = [];

        //XXX color by some hash of the name so it's consistent
        const items = Object.entries(data.Economy.IncomesAndExpenses);
        for(const [name, item] of items) {
            labels.push(name);
            const hue = (incomeData.length / items.length) * 360;
            bgColors.push(`hsl(${hue}, 100%, 50%)`);
            incomeData.push(item.Income);
            expenseData.push(item.Expense);
        }
        shuffle(bgColors);

        for(let i=0; i<items.length; i++) {
            legend.append($('<li>').append(
                $('<div class="legend-box">').css('background-color', bgColors[i]),
                $('<span class="label">').text(items[i][0]),
            ));
        }


        let ctx = $('#income-chart canvas')[0].getContext('2d');
        this.chartIncome = new Chart(ctx, {
            type: 'pie',
            options: options,
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: bgColors,
                    hoverBackgroundColor: bgColors,
                    borderWidth: 1,
                    borderColor: '#000',
                    data: incomeData,
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
                    hoverBackgroundColor: bgColors,
                    borderWidth: 1,
                    borderColor: '#000',
                    data: expenseData,
                }],
            },
        });
        $('#budget-legend').append(legend);
    }

    _updateCharts(data) {
        const incomeData  = [];
        const expenseData = [];
        for(const [name, item] of Object.entries(data.Economy.IncomesAndExpenses)) {
            //XXX are we sure these will always be in the same order?
            //XXX why are these dollars, not cents?
            incomeData.push(item.Income);
            expenseData.push(item.Expense);
        }

        this.chartIncome.data.datasets[0].data = incomeData;
        this.chartExpenses.data.datasets[0].data = expenseData;
        this.chartIncome.update();
        this.chartExpenses.update();
    }

    run() {
        window.setInterval(() => {this._refresh()}, this.updateInterval);
        console.log("Budget online.")
        this._refresh();
    }

    _refresh() {
        $.getJSON('/Budget', (data) => {
            console.log("Budget data:", data);
            if(!this._isInit) {
                this._init(data);
                this._isInit = true;
            }

            const net = data.TotalIncome - data.TotalExpenses;

            $('#income-chart .total').number(data.TotalIncome / 100)
                .toggleClass('negative', data.TotalIncome <= 0);

            $('#expense-chart .total').number(data.TotalExpenses / 100)
                .toggleClass('negative', data.TotalExpenses <= 0);

            $('#net-income').number(net / 100)
                .toggleClass('negative', net <= 0);

            $('#current-cash').number(data.CurrentCash / 100)
                .toggleClass('negative', data.CurrentCash <= 0);

            this._updateCharts(data);
        });
    }
}
