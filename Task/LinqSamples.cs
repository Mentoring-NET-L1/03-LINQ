// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return return all presented in market products")]

		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

        [Category("Tasks")]
        [Title("Task 1")]
        [Description("Sum of all orders is begger than X")]
        public void Linq01()
        {
            int minOrdersSum = 100000;
            var customers = dataSource.Customers
                .Where(customer => customer.Orders.Sum(order => order.Total) > minOrdersSum);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }

            Console.WriteLine(new string('-', 120));

            minOrdersSum = 10000;
            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("Tasks")]
        [Title("Task 2")]
        [Description("Group suppliers by location of customer")]
        public void Linq02()
        {
            var suppliersGroupedByCustomerLocation = dataSource.Customers.GroupJoin(
                dataSource.Suppliers,
                customer => new { Country = customer.Country, City = customer.City },
                supplier => new { Country = supplier.Country, City = supplier.City },
                (customer, suppliers) => new
                {
                    Country = customer.Country,
                    City = customer.City,
                    Customer = customer.CompanyName,
                    Suppliers = suppliers.Select(s => s.SupplierName),
                });

            foreach (var item in suppliersGroupedByCustomerLocation)
            {
                ObjectDumper.Write(item);
                foreach (var supplier in item.Suppliers)
                {
                    Console.WriteLine($"\t{supplier}");
                }
            }
        }

        [Category("Tasks")]
        [Title("Task 3")]
        [Description("Find customers with order's price bigger than X")]
        public void Linq03()
        {
            int minOrderTotal = 10000;
            var customers = dataSource.Customers
                .Where(customer => customer.Orders.Any(order => order.Total > minOrderTotal));

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
                foreach (var order in customer.Orders.Where(order => order.Total > minOrderTotal))
                {
                    Console.Write('\t');
                    ObjectDumper.Write(order);
                }
            }
        }

        [Category("Tasks")]
        [Title("Task 4")]
        [Description("Find first order date of each customer")]
        public void Linq04()
        {
            var customers = dataSource.Customers
                .Select(customer => new
                {
                    CustomerName = customer.CompanyName,
                    DateOfFirstOrder = customer.Orders.Select(order => (DateTime?)order.OrderDate).Min(),
                });

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("Tasks")]
        [Title("Task 5")]
        [Description("Order customers")]
        public void Linq05()
        {
            var customers = dataSource.Customers
                .Select(customer => new
                {
                    CustomerName = customer.CompanyName,
                    DateOfFirstOrder = customer.Orders.Select(order => (DateTime?)order.OrderDate).Min(),
                    Turnover = customer.Orders.Sum(order => order.Total),
                })
                .OrderBy(customer => customer.DateOfFirstOrder)
                .ThenByDescending(customer => customer.Turnover)
                .ThenBy(customer => customer.CustomerName);

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("Tasks")]
        [Title("Task 6")]
        [Description("Filter customers by validity of postal code, region and phone.")]
        public void Linq06()
        {
            var customers = dataSource.Customers
                .Where(
                    customer => !int.TryParse(customer.PostalCode, out _) ||
                    customer.Region == null ||
                    customer.Phone[0] != '(')
                .Select(customer => new
                {
                    customer.CompanyName,
                    customer.PostalCode,
                    customer.Region,
                    customer.Phone,
                });

            foreach (var customer in customers)
            {
                ObjectDumper.Write(customer);
            }
        }

        [Category("Tasks")]
        [Title("Task 7")]
        [Description("Double group products")]
        public void Linq07()
        {
            var groups = dataSource.Products.GroupBy(
                product => product.Category,
                (category, categorizedProducts) => new
                {
                    Category = category,
                    Groups = categorizedProducts.GroupBy(
                        p => p.UnitsInStock > 0,
                        (isInStock, products) => new
                        {
                            IsInStock = isInStock,
                            Products = products.OrderBy(product => product.UnitPrice),
                        }),
                });

            foreach (var group in groups)
            {
                Console.WriteLine(group.Category);
                foreach (var subGroup in group.Groups)
                {
                    Console.Write('\t');
                    Console.WriteLine(subGroup.IsInStock ? "In stock" : "Out of stock");
                    foreach (var product in subGroup.Products)
                    {
                        Console.Write("\t\t");
                        ObjectDumper.Write(product);
                    }
                }
            }
        }

        public enum PriceCategory { Chip, Medium, Expensive };

        [Category("Tasks")]
        [Title("Task 8")]
        [Description("Price category for products")]
        public void Linq08()
        {
            int medium = 10;
            int expensice = 100;

            var groups = dataSource.Products.GroupBy(
                product =>
                {
                    if (product.UnitPrice < medium)
                        return PriceCategory.Chip;
                    else if (product.UnitPrice < expensice)
                        return PriceCategory.Medium;
                    else
                        return PriceCategory.Expensive;
                });

            foreach (var group in groups)
            {
                Console.WriteLine(group.Key);
                foreach (var product in group)
                {
                    Console.Write('\t');
                    ObjectDumper.Write(product);
                }
            }
        }

        [Category("Tasks")]
        [Title("Task 9")]
        [Description("Get statistic of each city")]
        public void Linq09()
        {
            var cityStatisrics = dataSource.Customers.GroupBy(
                customer => customer.City,
                (city, customers) => new
                {
                    City = city,
                    AverageProfit = customers
                                        .SelectMany(customer => customer.Orders)
                                        .Average(order => order.Total),
                    AverageIntensity = customers
                                        .Select(customer => customer.Orders.Count())
                                        .Average(),
                });

            foreach (var statistic in cityStatisrics)
            {
                ObjectDumper.Write(statistic);
            }
        }

        [Category("Tasks")]
        [Title("Task 10")]
        [Description("Order count statistics")]
        public void Linq10()
        {
            var statisticByYear = dataSource.Customers
                .SelectMany(customer => customer.Orders)
                .GroupBy(
                    order => order.OrderDate.Year,
                    (year, orders) => new
                    {
                        Year = year,
                        OrderCount = orders.Count(),
                    });

            Console.WriteLine("Statistic By Year");
            foreach (var item in statisticByYear)
            {
                ObjectDumper.Write(item);
            }

            var statisticByMonth = dataSource.Customers
                .SelectMany(customer => customer.Orders)
                .GroupBy(
                    order => order.OrderDate.Month,
                    (month, orders) => new
                    {
                        Month = month,
                        OrderCount = orders.Count(),
                    });

            Console.WriteLine("Statistic By Month");
            foreach (var item in statisticByMonth)
            {
                ObjectDumper.Write(item);
            }

            var statisticByYearMonth = dataSource.Customers
                .SelectMany(customer => customer.Orders)
                .GroupBy(
                    order => new { order.OrderDate.Year, order.OrderDate.Month },
                    (date, orders) => new
                    {
                        Year = date.Year,
                        Month = date.Month,
                        OrderCount = orders.Count(),
                    });

            Console.WriteLine("Statistic By Year and Month");
            foreach (var item in statisticByYearMonth)
            {
                ObjectDumper.Write(item);
            }
        }
    }
}
