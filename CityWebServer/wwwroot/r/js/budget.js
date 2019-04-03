class Budget {
    constructor(app) {
        this.app = app;
        this.updateInterval = 5000; //msec
        this._isInit = false;

        //for some reason the in-game bank names
        //just return BankA, BankB, BankC. XXX fix this.
        this.bankNames = [
            "Silver Sunset Bank",
            "Global Credit Inc",
            "Pyramid Capital",
        ];

        this.groups = {
            Residential:     {color:'#80FF00'},
            Commercial:      {color:'#0080FF'},
            Industrial:      {color:'#FF8000'},
            Office:          {color:'#00C0C0'},
            Transit:         {},
            Parks:           {},
            Roads:           {},
            Electricity:     {},
            Water:           {},
            Garbage:         {},
            UniqueBuildings: {},
            Health:          {},
            Education:       {},
            Police:          {},
            Fire:            {},
            Disaster:        {},
            LoanPayments:    {},
            Policies:        {},
            Unknown:         {color: '#FF0000'},
        };
    }

    _init(data) {
        this._makeCharts(data);
    }

    makeNameColor(name) {
        //Try to match game colors
        if(name.startsWith('Residential_Low'))  return '#80FF00';
        if(name.startsWith('Residential_High')) return '#40C000';
        if(name.startsWith('Commercial_Low'))   return '#0080FF';
        if(name.startsWith('Commercial_High'))  return '#0040C0';
        if(name.startsWith('Industrial'))       return '#FF8000';
        if(name.startsWith('Office'))           return '#00C0C0';
        return this.app.makeNameColor(name);
    }

    _makeCharts(data) {
        const legend = $('<table class="legend">').append(
            $('<tr>').append(
                $('<th class="name">').text("Service"),
                $('<th class="income">').text("Income ₡"),
                $('<th class="expense">').text("Cost ₡"),
                $('<th class="net">').text("Net ₡"),
            ),
        );
        const options = {
            layout: {
                padding: { left: 0, right: 0, bottom: 0, top: 0 },
            },
            legend: { display: false },
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

        data.groups = {};

        const items = Object.entries(data.economy.incomesAndExpenses);
        let totalIncome=0, totalExpense=0;
        for(const [name, item] of items) {
            totalIncome += item.Income;
            totalExpense += item.Expense;
        }
        //Add a row for any discrepancies if totals don't match up
        items.push(["Unknown", {
            Income:  data.totalIncome   - totalIncome,
            Expense: data.totalExpenses - totalExpense,
        }]);

        for(const [name, item] of items) {
            let groupName = name.substring(0, name.indexOf('_'));
            if(groupName == '') groupName = name;
            let group = this.groups[groupName];
            if(data.groups[groupName] == undefined) {
                group.income  = item.Income;
                group.expense = item.Expense;
                data.groups[groupName] = group;
            }
            else {
                group.income  += item.Income;
                group.expense += item.Expense;
            }
        }

        for(const [name, group] of Object.entries(data.groups)) {
            labels.push(name);
            if(group.color == undefined) group.color = this.makeNameColor(name);
            bgColors.push(group.color);

            const cellIncome = $(`<td id="income-${name}" class="money income">`);
            const cellCost = $(`<td id="cost-${name}" class="money expense">`);
            const cellNet = $(`<td id="net-${name}" class="money net">`);
            cellIncome.number(group.income / 100);
            cellCost.number(group.expense / 100);
            cellNet.number((group.income - group.expense) / 100);
            cellNet.toggleClass('negative', group.income < group.expense);

            let row = $(`<tr id="legend-row-${name}">`).append(
                $('<td>').append(
                    $('<div class="legend-box">')
                        .css('background-color', group.color),
                    $('<span class="label">').text(name),
                ),
                cellIncome, cellCost, cellNet,
            );
            if(group.income == 0 && group.expense == 0) {
                row.addClass('zero');
            }
            legend.append(row);
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
                    borderWidth: 0,
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
                    borderWidth: 0,
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
        data.groups = {};
        for(const [name, item] of Object.entries(data.economy.incomesAndExpenses)) {
            //XXX are we sure these will always be in the same order?
            let groupName = name.substring(0, name.indexOf('_'));
            if(groupName == '') groupName = name;
            let group = this.groups[groupName];
            if(data.groups[groupName] == undefined) {
                group.income  = item.Income;
                group.expense = item.Expense;
                data.groups[groupName] = group;
            }
            else {
                group.income  += item.Income;
                group.expense += item.Expense;
            }
        }

        for(const [name, group] of Object.entries(data.groups)) {
            incomeData.push(group.income);
            expenseData.push(group.expense);
            $(`#income-${name}`).number(group.income / 100);
            $(`#cost-${name}`).number(group.expense / 100);
            $(`#net-${name}`).number((group.income - group.expense) / 100)
                .toggleClass('negative', group.income < group.expense);
            $(`#legend-row-${name}`).toggleClass('zero',
                (group.income == 0 && group.expense == 0));
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
            //console.log("Budget data:", data);
            if(!this._isInit) {
                this._init(data);
                this._isInit = true;
            }

            const net = data.totalIncome - data.totalExpenses;

            $('#income-chart .total').number(data.totalIncome / 100)
                .toggleClass('negative', data.totalIncome <= 0);

            $('#expense-chart .total').number(data.totalExpenses / 100)
                .toggleClass('negative', data.totalExpenses <= 0);

            $('#net-income').number(net / 100)
                .toggleClass('negative', net <= 0);

            $('#current-cash').number(data.currentCash / 100)
                .toggleClass('negative', data.currentCash <= 0);

            const taxes = Object.entries(data.economy.taxRates);
            for(const [name, rate] of taxes) {
                $(`#tax-${name}`).number(rate);
            }

            for(let i=0; i < data.loans.length; i++) {
                const loan = data.loans[i];
                $(`#loan${i}`).empty().append(
                    //$('<td>').text(loan.BankName),
                    $('<td>').text(this.bankNames[i]),
                    $('<td class="money">').number(loan.PaymentLeft / 100),
                    $('<td class="number">').text('XXX'), //time left
                    $('<td class="number">').text(
                        (loan.InterestRate / 100).toFixed(0) + '%'),
                    $('<td class="money">').text('XXX'), //weekly cost

                    //Length: number of weeks of repayment total
                    //No idea how to get number of weeks left or start date,
                    //or cost per week
                    //Should be 408 and 442.31
                );
            }

            this._updateCharts(data);
        });
    }
}
