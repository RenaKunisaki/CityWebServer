using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
using CityWebServer.Models;
using ColossalFramework;

namespace CityWebServer.RequestHandlers {
	public class BudgetRequestHandler: RequestHandlerBase {
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
				//XXX "Organic and Local Produce"
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
				new IncomeExpenseGroup {
					Name       = "Citizens", //XXX what is this?
					Service    = ItemClass.Service.Citizen,
				},
				new IncomeExpenseGroup {
					Name       = "Tourists",
					Service    = ItemClass.Service.Tourism,
				},
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
					Service    = ItemClass.Service.Monument, //XXX verify
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
				new IncomeExpenseGroup {
					Name       = "Natural", //natural resources?
					Service    = ItemClass.Service.Natural,
				},
				new IncomeExpenseGroup {
					Name       = "Vehicles", //city service vehicles?
					Service    = ItemClass.Service.Vehicles,
				},
				//Policy and Loan expenses are handled differently
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

		public BudgetRequestHandler(IWebServer server)
			: base(server, new Guid("87205a0d-1b53-47bd-91fa-9cddf0a3bd9e"),
				"Budget", "Rychard", 100, "/Budget") {
		}

		public override IResponseFormatter Handle(HttpListenerRequest request) {
			// TODO: Expand upon this to expose substantially more information.
			var economyManager = Singleton<EconomyManager>.instance;
			BudgetInfo budget  = this.GetOverview(economyManager);
			budget.loans       = this.GetLoans(economyManager).ToArray();
			budget.economy     = new Economy {
				incomesAndExpenses = this.GetIncomesAndExpenses(economyManager),
				taxRates           = this.GetTaxRates(economyManager),
			};

			//LogMessage("Sending response.");
			return JsonResponse(budget);
		}

		public BudgetInfo GetOverview(EconomyManager economyManager) {
			BudgetInfo budget = new BudgetInfo();
			economyManager.GetIncomeAndExpenses(new ItemClass(),
				out budget.totalIncome, out budget.totalExpenses);
			budget.currentCash    = economyManager.LastCashAmount;
			return budget;
		}

		public List<Loan> GetLoans(EconomyManager economyManager) {
			List<Loan> loans = new List<Loan>(EconomyManager.MAX_LOANS);
			//for(int i = 0; i < economyManager.CountLoans(); i++) {
			for(int i = 0; i < EconomyManager.MAX_LOANS; i++) {
				economyManager.GetLoan(i, out EconomyManager.Loan loan);
				loans.Add(new Loan {
					//bank names aren't the ones shown in-game,
					//they're just BankA, BankB, BankC. WTF?
					//can LocaleFormatter give us the names?
					BankName     = economyManager.GetBankName(i),
					Amount       = loan.m_amountTaken,
					PaymentLeft  = loan.m_amountLeft,
					InterestRate = loan.m_interestRate,
					InterestPaid = loan.m_interestPaid,
					Length       = loan.m_length,
					//XXX how to get weekly cost, weeks left,
					//correct bank name?
				});
			}
			return loans;
		}

		public Dictionary<String, IncomeExpense> GetIncomesAndExpenses(EconomyManager economyManager) {
			//Get income/expenses for each group
			//LogMessage("Getting Income/Expense info.");
			Dictionary<String, IncomeExpense> incomeExpenses = new Dictionary<String, IncomeExpense>();
			foreach(IncomeExpenseGroup group in expenseGroups) {
				if(group.Levels > 0) {
					for(int i = 1; i <= group.Levels; i++) {
						economyManager.GetIncomeAndExpenses(
							group.Service, group.SubService, (ItemClass.Level)(i-1),
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

		public Dictionary<String, int> GetTaxRates(EconomyManager economyManager) {
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

		private new void LogMessage(string msg) {
			IntegratedWebServer.LogMessage(msg);
		}
	}
}