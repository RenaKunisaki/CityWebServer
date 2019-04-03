using System;
using System.Collections.Generic;

namespace CityWebServer.Models {
	public class BudgetInfo {
		public long totalIncome;
		public long totalExpenses;
		public long currentCash;
		public long loanExpenses;
		public long policyExpenses;
		public Loan[] loans;
		public Economy economy;
	}

	public class IncomeExpenseGroup {
		public String Name;
		public ItemClass.Service Service;
		public ItemClass.SubService SubService = ItemClass.SubService.None;
		public int Levels = 0;
	}

	public class TaxRateGroup {
		public String Name;
		public ItemClass.Service Service;
		public ItemClass.SubService SubService = ItemClass.SubService.None;
	}

	public class Loan {
		public String BankName;
		public long Amount;
		public long PaymentLeft;
		public int InterestRate;
		public long InterestPaid;
		public long Length;
	}

	public class Economy {
		public Dictionary<String, IncomeExpense> incomesAndExpenses;
		public Dictionary<String, int> taxRates;
	}

	public class TaxRate {
		public int Rate { get; set; }

		// Tax Rate: Low-Density Residential
		// Tax Rate: High-Density Residential
		// Tax Rate: Low-Density Commercial
		// Tax Rate: High-Density Commercial
		// Tax Rate: Industry
		// Tax Rate: Offices
	}

	public class IncomeExpense {
		public long Income;
		public long Expense;
		public int Level;

		// Tax Income: Low-Density Residential
		// Tax Income: High-Density Residential
		// Tax Income: Low-Density Commercial
		// Tax Income: High-Density Commercial
		// Tax Income: Industry
		// Tax Income: Offices

		// Income: Citizens
		// Income: Tourists

		// Income: Bus/Train/Metro?

		// Upkeep Expense: Roads
		// Upkeep Expense: Electricity
		// Upkeep Expense: Water
		// Upkeep Expense: Garbage
		// Upkeep Expense: Unique Buildings
		// Upkeep Expense: Healthcare
		// Upkeep Expense: Education
		// Upkeep Expense: Police
		// Upkeep Expense: Firefighters
		// Upkeep Expense: Parks
		// Upkeep Expense: Bus/Train/Metro?
		// Upkeep Expense: Taxes???
		// Upkeep Expense: Policy
	}
}