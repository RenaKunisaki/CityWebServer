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
					Name       = "ResidentialLow",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialLow,
					Levels     = 5,
				},
				new IncomeExpenseGroup {
					Name       = "ResidentialHigh",
					Service    = ItemClass.Service.Residential,
					SubService = ItemClass.SubService.ResidentialHigh,
					Levels     = 5,
				},
				new IncomeExpenseGroup {
					Name       = "CommercialLow",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialLow,
					Levels     = 3,
				},
				new IncomeExpenseGroup {
					Name       = "CommercialHigh",
					Service    = ItemClass.Service.Commercial,
					SubService = ItemClass.SubService.CommercialHigh,
					Levels     = 3,
				},
				//XXX Leisure, Tourism, Organic and Local Produce
				new IncomeExpenseGroup {
					Name       = "Industrial",
					Service    = ItemClass.Service.Industrial,
					Levels     = 3,
				},
				//XXX specific industries
				new IncomeExpenseGroup {
					Name       = "Transit",
					Service    = ItemClass.Service.PublicTransport,
				},
				//XXX specific transit types
				new IncomeExpenseGroup {
					Name       = "Office",
					Service    = ItemClass.Service.Office,
					Levels     = 3,
				},
				//XXX IT Cluster
				new IncomeExpenseGroup {
					Name       = "Parks",
					Service    = ItemClass.Service.Beautification, //XXX verify
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
				/* new IncomeExpenseGroup {
					Name       = "Policies",
					Service    = ItemClass.Service.???, //XXX
				}, */
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
			BudgetInfo budget = new BudgetInfo();
			//LogMessage("Getting budget info.");

			//Get overview
			economyManager.GetIncomeAndExpenses(new ItemClass(),
				out budget.TotalIncome, out budget.TotalExpenses);
			budget.CurrentCash = economyManager.LastCashAmount;

			//Get loan info
			List<Loan> loans = new List<Loan>(EconomyManager.MAX_LOANS);
			//for(int i = 0; i < economyManager.CountLoans(); i++) {
			for(int i = 0; i < EconomyManager.MAX_LOANS; i++) {
				economyManager.GetLoan(i, out EconomyManager.Loan loan);
				loans.Add(new Loan {
					//economyManager.GetLoanExpenses()  - useful?
					//economyManager.GetPolicyExpenses()
					//LocaleFormatter
					//bank names aren't the ones shown in-game,
					//they're just BankA, BankB, BankC. WTF?
					BankName = economyManager.GetBankName(i),
					Amount = loan.m_amountTaken,
					PaymentLeft = loan.m_amountLeft,
					InterestRate = loan.m_interestRate,
					InterestPaid = loan.m_interestPaid,
					Length = loan.m_length,
					//XXX how to get weekly cost, weeks left,
					//correct bank name?
				});
			}
			budget.loans = loans.ToArray();

			//Get income/expenses for each group
			//LogMessage("Getting Income/Expense info.");
			Dictionary<String, IncomeExpense> incomeExpenses = new Dictionary<String, IncomeExpense>();
			foreach(IncomeExpenseGroup group in expenseGroups) {
				if(group.Levels > 0) {
					for(int i=1; i<=group.Levels; i++) {
						economyManager.GetIncomeAndExpenses(
							group.Service, group.SubService, (ItemClass.Level)i,
							out long income, out long expense);
						incomeExpenses[$"{group.Name}_Lv{i}"] = new IncomeExpense {
							Income  = income,
							Expense = expense,
							Level   = i,
						};
					}
				}
				else { //This group doesn't have levels
					economyManager.GetIncomeAndExpenses(
						group.Service, group.SubService,
						ItemClass.Level.None,
						out long income, out long expense);
					incomeExpenses[group.Name] = new IncomeExpense {
						Income  = income,
						Expense = expense,
						Level   = 0,
					};
				}
			}

			//Get tax rates for each group
			//LogMessage("Getting tax info.");
			Dictionary<String, int> taxRates = new Dictionary<string, int>();
			foreach(TaxRateGroup group in taxGroups) {
				//LogMessage($"Getting tax info for {group.Name}.");
				taxRates[group.Name] = economyManager.GetTaxRate(
					group.Service, group.SubService, ItemClass.Level.None);
			}

			//LogMessage("Setting up response.");
			budget.Economy = new Economy {
				IncomesAndExpenses = incomeExpenses,
				taxRates = taxRates,
			};

			//LogMessage("Sending response.");
			return JsonResponse(budget);
		}

		private new void LogMessage(string msg) {
			IntegratedWebServer.LogMessage(msg);
		}
	}
}