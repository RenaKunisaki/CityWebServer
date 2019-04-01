class Budget {
    constructor(app) {
        this.app = app;
        this.updateInterval = 5000; //msec
        this.txtIncome = $('<span class="money income">');
        this.txtExpenses = $('<span class="money expenses">');
        this.txtDelta = $('<span class="money delta">');
        this.txtCash = $('<span class="money cash">');
        this.txtNumLoans = $('<span class="number numLoans">');
        this.element = $('<div class="budget">').append(
            this.txtCash,
            this.txtDelta,
            this.txtIncome,
            this.txtExpenses,
            this.txtNumLoans,
        );
    }

    run() {
        window.setInterval(() => {this._refresh()}, this.updateInterval);
        console.log("Budget online.")
    }

    _refresh() {
        $.getJSON('/Budget', (data) => {
            this.txtIncome.number(data.income / 100);
            this.txtExpenses.number(data.expenses / 100);
            this.txtDelta.number((data.income - data.expenses) / 100);
            this.txtCash.number(data.cash / 100);
            this.txtNumLoans.number(data.numLoans);
            console.log("Budget data:", data);
        });
    }
}
