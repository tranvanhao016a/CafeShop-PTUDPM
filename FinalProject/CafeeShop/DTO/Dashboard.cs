﻿using CafeeShop.Db;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardApp.Models
{
    public struct RevenueByDate
    {
        public string Date { get; set; }
        public float TotalAmount { get; set; }
    }

    public class Dashboard : DbConnection
    {

        //Fields & Properties
        private DateTime startDate;
        private DateTime endDate;
        private int numberDays;

        public int NumEmployees { get; private set; }
        public int NumTableFood { get; private set; }
        public int NumFood { get; private set; }

        public int NumBill { get; private set; }

        public List<KeyValuePair<string, int>> TopProductsList { get; private set; }
        public List<KeyValuePair<string, int>> UnderstockList { get; private set; }
        public List<RevenueByDate> GrossRevenueList { get; private set; }
        public int NumOrders { get; set; }
        public float TotalRevenue { get; set; }
        public float TotalProfit { get; set; }

        //Constructor
        public Dashboard()
        {

        }

        //Private methods
        private void GetNumberItems()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    
                    command.CommandText = "select count(id) from Employees";
                    NumEmployees = (int)command.ExecuteScalar();

                    
                    command.CommandText = "select count(id) from TableFood";
                    NumTableFood = (int)command.ExecuteScalar();

                    
                    command.CommandText = "select count(id) from Food";
                    NumFood = (int)command.ExecuteScalar();

                    command.CommandText = "select count(id) from Bill";
                    NumBill = (int)command.ExecuteScalar();
                  
                    command.CommandText = @"select count(id) from [Bill]" +
                                            "where DateCheckIn between  @fromDate and @toDate";
                    command.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    command.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    NumOrders = (int)command.ExecuteScalar();
                }
            }
        }
        private void GetProductAnalisys()
        {
            TopProductsList = new List<KeyValuePair<string, int>>();
            UnderstockList = new List<KeyValuePair<string, int>>();
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    SqlDataReader reader;
                    command.Connection = connection;
                    //Get Top 5 products
                    command.CommandText = @"select top 5 F.name as Name , sum(I.count)
                                          as T from  BillInfo I inner 
                                          join Food F on I.idFood = F.id 
                                          group by F.name 
                                          order by sum(I.count)  ";
                    command.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    command.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        TopProductsList.Add(
                            new KeyValuePair<string, int>(reader[0].ToString(), (int)reader[1]));
                    }
                    reader.Close();

                    //Get Understock
                    //command.CommandText = @"select ProductName, Stock
                    //                        from Product
                    //                        where Stock <= 6 and IsDiscontinued = 0";
                    //reader = command.ExecuteReader();
                    //while (reader.Read())
                    //{
                    //    UnderstockList.Add(
                    //        new KeyValuePair<string, int>(reader[0].ToString(), (int)reader[1]));
                    //}
                    //reader.Close();
                }
            }
        }
        private void GetOrderAnalisys()
        {
            GrossRevenueList = new List<RevenueByDate>();
            TotalProfit = 0;
            TotalRevenue = 0;

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"select DateCheckIn, sum(Total) 
                                            from[Bill]
                                            where DateCheckIn between @fromDate and @toDate
                                            group by DateCheckIn";
                    command.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    command.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    var reader = command.ExecuteReader();
                    var resultTable = new List<KeyValuePair<DateTime, float>>();
                    while (reader.Read())
                    {
                        resultTable.Add(
                            new KeyValuePair<DateTime, float>((DateTime)reader[0], (float)Convert.ToDouble(reader[1])));
                        TotalRevenue += (float)Convert.ToDouble(reader[1]);
                    }
                    TotalProfit = NumBill;
                    //TotalProfit = TotalRevenue * 0.2m;//20%
                    reader.Close();

                    //Group by Hours
                    if (numberDays <= 1)
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("hh tt")
                                           into order
                                            select new RevenueByDate
                                            {
                                                Date = order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }
                    //Group by Days
                    else if (numberDays <= 30)
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("dd MMM")
                                           into order
                                            select new RevenueByDate
                                            {
                                                Date = order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }

                    //Group by Weeks
                    else if (numberDays <= 92)
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                                orderList.Key, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                                           into order
                                            select new RevenueByDate
                                            {
                                                Date = "Week " + order.Key.ToString(),
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }

                    //Group by Months
                    else if (numberDays <= (365 * 2))
                    {
                        bool isYear = numberDays <= 365 ? true : false;
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("MMM yyyy")
                                           into order
                                            select new RevenueByDate
                                            {
                                                Date = isYear ? order.Key.Substring(0, order.Key.IndexOf(" ")) : order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }

                    //Group by Years
                    else
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("yyyy")
                                           into order
                                            select new RevenueByDate
                                            {
                                                Date = order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }
                }
            }
        }

        //Public methods
        public bool LoadData(DateTime startDate, DateTime endDate)
        {
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day,
                endDate.Hour, endDate.Minute, 59);
            if (startDate != this.startDate || endDate != this.endDate)
            {
                this.startDate = startDate;
                this.endDate = endDate;
                this.numberDays = (endDate - startDate).Days;

                GetNumberItems();
                GetProductAnalisys();
                GetOrderAnalisys();
                Console.WriteLine("Refreshed data: {0} - {1}", startDate.ToString(), endDate.ToString());
                return true;
            }
            else
            {
                Console.WriteLine("Data not refreshed, same query: {0} - {1}", startDate.ToString(), endDate.ToString());
                return false;
            }
        }
    }
}



