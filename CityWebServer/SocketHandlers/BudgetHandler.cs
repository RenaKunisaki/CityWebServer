using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Callbacks;
using CityWebServer.Extensibility;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;
using UnityEngine;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Pushes budget info to client.
	/// </summary>
	public class BudgetHandler: SocketHandlerBase {
		public static readonly IList<IncomeExpenseGroup> expenseGroups = new System.Collections.ObjectModel.ReadOnlyCollection<IncomeExpenseGroup>(
			new List<IncomeExpenseGroup> {
				//Tax income from zones
				new IncomeExpenseGroup {
					Name       = "Residential_Low",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialLow,
					Levels     = 5,
				},
				new IncomeExpenseGroup {
					Name       = "Residential_High",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialHigh,
					Levels     = 5,
				},
				new IncomeExpenseGroup {
					Name       = "Residential_LowEco",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialLowEco,
					Levels     = 5,
				},
				new IncomeExpenseGroup {
					Name       = "Residential_HighEco",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialHighEco,
					Levels     = 5,
				},
				new IncomeExpenseGroup {
					Name       = "Commercial_Low",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialLow,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Commercial_High",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialHigh,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					//"Organic and Local Produce"
					Name       = "Commercial_Eco",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialEco,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Commercial_Leisure",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialLeisure,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Commercial_Tourist",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialTourist,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Industrial_Generic",
					Service    = ItemClass.Service.Industrial,
					SubService = ItemClass.SubService.IndustrialGeneric,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Industrial_Oil",
					Service    = ItemClass.Service.Industrial,
					SubService = ItemClass.SubService.IndustrialOil,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Industrial_Ore",
					Service    = ItemClass.Service.Industrial,
					SubService = ItemClass.SubService.IndustrialOre,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Industrial_Farming",
					Service    = ItemClass.Service.Industrial,
					SubService = ItemClass.SubService.IndustrialFarming,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Industrial_Forestry",
					Service    = ItemClass.Service.Industrial,
					SubService = ItemClass.SubService.IndustrialForestry,
					Levels     = 3,
				},
				//Areas zoned as Industrial are counted differently
				//from the specific industries above (including Generic).
				new IncomeExpenseGroup {
					Name       = "Industrial_Zoned",
					Service    = ItemClass.Service.PlayerIndustry,
					Levels     = 0,
				},

				new IncomeExpenseGroup {
					Name       = "Office_Generic",
					Service    = ItemClass.Service.Office,
					SubService = ItemClass.SubService.OfficeGeneric,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Office_Hightech",
					Service    = ItemClass.Service.Office,
					SubService = ItemClass.SubService.OfficeHightech,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Bus",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportBus,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Post",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportPost,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Ship",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportShip,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Taxi",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTaxi,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Tram",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTram,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Metro",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportMetro,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Plane",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportPlane,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Tours",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTours,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Train",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTrain,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_CableCar",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportCableCar,
				},
				new IncomeExpenseGroup {
					Name       = "Transit_Monorail",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportMonorail,
				},
				new IncomeExpenseGroup {
					Name       = "Parks",
					Service    = ItemClass.Service.Beautification,
				},
				new IncomeExpenseGroup {
					Name       = "Roads",
					Service    = ItemClass.Service.Road,
					//Income is from tolls
				},
				//XXX Small/Large Vehicle tolls

				//These are grand totals, don't include them.
				/* new IncomeExpenseGroup {
					Name       = "Citizens",
					Service    = ItemClass.Service.Citizen,
				},
				new IncomeExpenseGroup {
					Name       = "Tourists",
					Service    = ItemClass.Service.Tourism,
				}, */
				//XXX specific tourist types

				//Upkeep expenses
				//Roads is already covered above, due to tolls
				new IncomeExpenseGroup {
					Name       = "Electricity",
					Service    = ItemClass.Service.Electricity,
				},
				new IncomeExpenseGroup {
					Name       = "Water",
					Service    = ItemClass.Service.Water,
				},
				new IncomeExpenseGroup {
					Name       = "Garbage",
					Service    = ItemClass.Service.Garbage,
				},
				new IncomeExpenseGroup {
					Name       = "UniqueBuildings",
					Service    = ItemClass.Service.Monument,
				},
				new IncomeExpenseGroup {
					Name       = "Health",
					Service    = ItemClass.Service.HealthCare,
				},
				new IncomeExpenseGroup {
					Name       = "Education",
					Service    = ItemClass.Service.Education,
				},
				new IncomeExpenseGroup {
					Name       = "Police",
					Service    = ItemClass.Service.PoliceDepartment,
				},
				new IncomeExpenseGroup {
					Name       = "Fire",
					Service    = ItemClass.Service.FireDepartment,
				},
				new IncomeExpenseGroup {
					Name       = "Disaster",
					Service    = ItemClass.Service.Disaster,
				},

				//These always return 0, so probably there aren't any
				//incomes or expenses associated with them.
				/* new IncomeExpenseGroup {
					Name       = "Natural", //natural resources?
					Service    = ItemClass.Service.Natural,
				},
				new IncomeExpenseGroup {
					Name       = "Vehicles", //city service vehicles?
					Service    = ItemClass.Service.Vehicles,
				}, */

				//Policy and Loan expenses are handled differently,
				//as they're not consider services.
				//Loan payments are a lump sum, so not weekly income.
			}
		);

		public static readonly IList<TaxRateGroup> taxGroups = new System.Collections.ObjectModel.ReadOnlyCollection<TaxRateGroup>(
			new List<TaxRateGroup> {
				new TaxRateGroup {
					Name       = "ResidentialLow",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialLow,
				},
				new TaxRateGroup {
					Name       = "ResidentialHigh",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialHigh,
				},
				new TaxRateGroup {
					Name       = "CommercialLow",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialLow,
				},
				new TaxRateGroup {
					Name       = "CommercialHigh",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialHigh,
				},
				new TaxRateGroup {
					Name    = "Industrial",
					Service = ItemClass.Service.Industrial,
				},
				new TaxRateGroup {
					Name    = "Office",
					Service = ItemClass.Service.Office,
				},
			}
		);

		public static readonly IList<BudgetGroup> budgetGroups = new System.Collections.ObjectModel.ReadOnlyCollection<BudgetGroup>(
			new List<BudgetGroup> {
				new BudgetGroup {
					Name    = "Electricity",
					Service = ItemClass.Service.Electricity,
				},
				new BudgetGroup {
					Name    = "Water",
					Service = ItemClass.Service.Water,
				},
				new BudgetGroup {
					Name    = "Garbage",
					Service = ItemClass.Service.Garbage,
				},
				new BudgetGroup {
					Name    = "Health",
					Service = ItemClass.Service.HealthCare,
				},
				new BudgetGroup {
					Name    = "Fire",
					Service = ItemClass.Service.FireDepartment,
				},
				new BudgetGroup {
					Name    = "Police",
					Service = ItemClass.Service.PoliceDepartment,
				},
				new BudgetGroup {
					Name    = "Education",
					Service = ItemClass.Service.Education,
				},
				new BudgetGroup {
					Name    = "Parks",
					Service = ItemClass.Service.Beautification,
				},
				new BudgetGroup {
					Name    = "UniqueBuildings",
					Service = ItemClass.Service.Monument,
				},
				new BudgetGroup {
					Name    = "Industry",
					Service = ItemClass.Service.PlayerIndustry,
				},
				new BudgetGroup {
					Name    = "Roads",
					Service = ItemClass.Service.Road,
				},
				new BudgetGroup {
					Name       = "Transit_Bus",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportBus,
				},
				new BudgetGroup {
					Name       = "Transit_Tram",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTram,
				},
				new BudgetGroup {
					Name       = "Transit_Metro",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportMetro,
				},
				new BudgetGroup {
					Name       = "Transit_Train",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTrain,
				},
				new BudgetGroup {
					Name       = "Transit_Ship",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportShip,
				},
				new BudgetGroup {
					Name       = "Transit_Plane",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportPlane,
				},
				new BudgetGroup {
					Name       = "Transit_Monorail",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportMonorail,
				},
				new BudgetGroup {
					Name       = "Transit_CableCar",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportCableCar,
				},
				new BudgetGroup {
					Name       = "Transit_Post",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportPost,
				},
				new BudgetGroup {
					Name       = "Transit_Taxi",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTaxi,
				},
				new BudgetGroup {
					Name       = "Transit_Tours",
					Service    = ItemClass.Service.PublicTransport,
					SubService = ItemClass.SubService.PublicTransportTours,
				},
			}
		);

		protected EconomyManager economyManager;

		public BudgetHandler(SocketRequestHandler handler) :
		base(handler, "Budget") {
			economyManager = EconomyManager.instance;
			server.dailyCallbacks.Register(Update);
			SendAll();
			handler.RegisterMessageHandler("Budget", OnClientMessage);
		}

		public void OnClientMessage(ClientMessage msg) {
			string action = msg.GetString("action");
			switch(action) {
				case "gimme":
					economyManager.AddResource(
						EconomyManager.Resource.RewardAmount,
						msg.GetInt("amount"), new ItemClass());
					break;
				case "payLoan": {
						var r = economyManager.PayLoanNow(msg.GetInt("id"));
						if(r == null) Log($"PayLoanNow returns null");
						else Log($"PayLoanNow returns: {r}");
						break;
					}
				//case "setBudget": {
				//		//TODO, also set tax rate
				//		//economyManager.SetBudget();
				//		break;
				//	}
				default:
					throw new ArgumentException($"Invalid method {action}");
			}
		}

		/// <summary>
		/// Send new data to client.
		/// </summary>
		/// <param name="param">Callback parameters.</param>
		protected void Update(DailyCallbackParam param) {
			SendAll();
		}

		/// <summary>
		/// Send all budget info to client.
		/// </summary>
		protected void SendAll() {
			BudgetInfo budget = this.GetOverview();
			budget.loans = this.GetLoans().ToArray();
			budget.economy = new Economy {
				incomesAndExpenses = this.GetIncomesAndExpenses(),
				taxRates = this.GetTaxRates(),
				budgetRates = this.GetBudgetRates(),
			};
			SendJson(budget);
		}

		/// <summary>
		/// Get basic budget info.
		/// </summary>
		/// <returns>The overview.</returns>
		public BudgetInfo GetOverview() {
			BudgetInfo budget = new BudgetInfo();
			economyManager.GetIncomeAndExpenses(
				(ItemClass)ScriptableObject.CreateInstance("ItemClass"),
				out budget.totalIncome, out budget.totalExpenses);
			budget.currentCash = economyManager.LastCashAmount;
			return budget;
		}

		/// <summary>
		/// Get list of active loans.
		/// </summary>
		/// <returns>The loans.</returns>
		public List<Loan> GetLoans() {
			List<Loan> loans = new List<Loan>(EconomyManager.MAX_LOANS);
			//for(int i = 0; i < economyManager.CountLoans(); i++) {
			for(int i = 0; i < EconomyManager.MAX_LOANS; i++) {
				economyManager.GetLoan(i, out EconomyManager.Loan loan);
				if(loan.m_amountLeft == 0) continue;

				//Compute how long ago the loan was taken.
				//thanks to Vectorial1024 for the formula.
				float remain = loan.m_amountLeft;
				float initial = loan.m_amountTaken;
				//rate is 52 weeks * 100%
				//m_interestRate is per 10,000, ie 0.01%, so we
				//further divide it by 100.
				float rate = 1 + (loan.m_interestRate / 520000);
				float cost = loan.m_amountTaken / loan.m_length; //weekly payment
				double weeks = (Math.Log(remain + cost / (1 - rate)) -
					Math.Log(initial + cost / (1 - rate))) / Math.Log(rate);

				loans.Add(new Loan {
					//bank names aren't the ones shown in-game,
					//they're just BankA, BankB, BankC. WTF?
					//can LocaleFormatter give us the names?
					//BankName = economyManager.GetBankName(i),
					BankName = economyManager.m_properties.m_banks[i].m_bankName,
					Amount = loan.m_amountTaken,
					PaymentLeft = loan.m_amountLeft,
					InterestRate = loan.m_interestRate,
					InterestPaid = loan.m_interestPaid,
					Length = loan.m_length,
					TimeLeft = weeks,
					WeeklyPayment = (long)cost,
				});
			}
			return loans;
		}

		/// <summary>
		/// Get income and expense details for each service (see expenseGroups).
		/// </summary>
		/// <returns>Dictionary of name => IncomeExpense.</returns>
		public Dictionary<String, IncomeExpense> GetIncomesAndExpenses() {
			//LogMessage("Getting Income/Expense info.");
			Dictionary<String, IncomeExpense> incomeExpenses = new Dictionary<String, IncomeExpense>();
			foreach(IncomeExpenseGroup group in expenseGroups) {
				if(group.Levels > 0) {
					for(int i = 1; i <= group.Levels; i++) {
						economyManager.GetIncomeAndExpenses(
							group.Service, group.SubService, (ItemClass.Level)(i - 1),
							out long income, out long expense);
						incomeExpenses[$"{group.Name}_Lv{i}"] = new IncomeExpense {
							Income = income,
							Expense = expense,
							Level = i,
						};
					}
				}
				else { //This group doesn't have levels
					economyManager.GetIncomeAndExpenses(
						group.Service, group.SubService,
						ItemClass.Level.None,
						out long income, out long expense);
					incomeExpenses[group.Name] = new IncomeExpense {
						Income = income,
						Expense = expense,
						Level = 0,
					};
				}
			}

			//These aren't considered services
			incomeExpenses["LoanPayments"] = new IncomeExpense {
				Income = 0,
				Expense = economyManager.GetLoanExpenses(),
				Level = 0,
			};
			incomeExpenses["Policies"] = new IncomeExpense {
				Income = 0,
				Expense = economyManager.GetPolicyExpenses(),
				Level = 0,
			};

			return incomeExpenses;
		}

		/// <summary>
		/// Get tax rates for each zone. (See taxGroups)
		/// </summary>
		/// <returns>Dictionary of name => rate.</returns>
		/// <remarks>Rate is a percentage.</remarks>
		public Dictionary<String, int> GetTaxRates() {
			//Get tax rates for each group
			//LogMessage("Getting tax info.");
			Dictionary<String, int> taxRates = new Dictionary<string, int>();
			foreach(TaxRateGroup group in taxGroups) {
				//LogMessage($"Getting tax info for {group.Name}.");
				taxRates[group.Name] = economyManager.GetTaxRate(
					group.Service, group.SubService, ItemClass.Level.None);
			}
			return taxRates;
		}

		/// <summary>
		/// Get the funding setting for each service.
		/// </summary>
		/// <returns>Dictionary of name => funding.</returns>
		/// <remarks>Funding is a percentage, and can be more than 100%.
		/// In-game range is 50% to 200%.</remarks>
		public Dictionary<String, int> GetBudgetRates() {
			Dictionary<String, int> budgetRates = new Dictionary<string, int>();
			foreach(BudgetGroup group in budgetGroups) {
				budgetRates[$"{group.Name}_Day"] =
					economyManager.GetBudget(group.Service, group.SubService, false);
				budgetRates[$"{group.Name}_Night"] =
					economyManager.GetBudget(group.Service, group.SubService, true);
			}
			return budgetRates;
		}
	}
}