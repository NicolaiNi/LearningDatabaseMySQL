using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DataBaseConsole {
    class Program {
        static void Main(string[] args) {

            var myConnectionString = "Database=billingsystem;Data Source=localhost;User Id=root;Password=";
            MySqlConnection myConnection = new MySqlConnection(myConnectionString);
            myConnection.Open();

            // Refresh Database
            SqlBaseMethods.SetForeignKeyChecks(0, myConnection);
            SqlBaseMethods.DropAllTables(myConnection); 
            SqlBaseMethods.SetForeignKeyChecks(1, myConnection);

            // Create Tables
            SqlBaseMethods.CreateTableCustomer(myConnection);
            SqlBaseMethods.CreateTableBill(myConnection);
            SqlBaseMethods.CreateTableInvoiceItem(myConnection);

            //DataTable metaDataCollection = myConnection.GetSchema("Tables");
            //SqlBaseMethods.DisplayData(metaDataCollection);

            string currentDirectory = Directory.GetCurrentDirectory();

            //Generate Data
            string filePathData = Path.Combine(currentDirectory, "Data");
            List<Customer> customers = new List<Customer>();
            List<Bill> bills = new List<Bill>(); 

            foreach (var filepath in Directory.GetFiles(filePathData)) {
                StreamReader sr = new StreamReader(filepath);
                if (filepath.Contains("Customers")) {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Customer>), new XmlRootAttribute("Customers"));
                    customers = (List<Customer>)serializer.Deserialize(sr);
                }
                if (filepath.Contains("Bills")) {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Bill>), new XmlRootAttribute("Bills"));
                    bills = (List<Bill>)serializer.Deserialize(sr);
                }
            }

            List<int> customersIDs = new List<int>();
            // Insert Customers and Bills into the database
            foreach (var customer in customers) {
                customer.Insert(myConnection, nameof(customer));
                customersIDs.Add(customer.ReturnId());
            }

            var random = new Random();
            foreach (var bill in bills) {
                var ranIndex = random.Next(customersIDs.Count);
                var ranCustomerId = customersIDs[ranIndex];
                bill.Insert(myConnection, nameof(bill), ranCustomerId);
                bill.SetCustomer_Id(ranCustomerId);
            }

            // Connect each Bill with random customer
            //var rand = new Random();

            //foreach (var bill in bills) {
            //    var ranCustomer = rand.Next(1, customers.Count);
            //    int currentId = bill.ReturnId();
            //    var updateBill_CustomerIdSql = $"UPDATE `bill` SET `customer_id` =@customer_id WHERE `id` = {currentId}";
            //    MySqlCommand updateBill_CustomerId = new MySqlCommand(updateBill_CustomerIdSql, myConnection);
            //    updateBill_CustomerId.Parameters.AddWithValue("@customer_id", ranCustomer); 
            //    updateBill_CustomerId.ExecuteNonQuery();
            //    bill.SetCustomer_Id(ranCustomer);    
            //}

            string filePathDataInvoice = Path.Combine(filePathData, "BillContents");
            List<string> pathInvoiceFiles = Directory.GetFiles(filePathDataInvoice).ToList();

            //create a invoiceItem List for each bill and update database
            foreach (var bill in bills) {
                var pathOfInvoiceFile = pathInvoiceFiles.Where(f => f.Contains(bill.Number.ToString())).FirstOrDefault();
                if (pathOfInvoiceFile != null) {
                    StreamReader sr = new StreamReader(pathOfInvoiceFile);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<InvoiceItem>), new XmlRootAttribute("InvoiceItems"));
                    List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
                    invoiceItems = (List<InvoiceItem>)serializer.Deserialize(sr);

                    //add InvoiceItems to Bill object
                    bill.InvoiceItems = invoiceItems;
                    //update Bill object
                    bill.SetSum();

                    //update bill.sum in Database
                    int currentId = bill.ReturnId();
                    var updateBill_SumSql = $"UPDATE `bill` SET `sum` =@sum WHERE `id` = {currentId}";
                    MySqlCommand updateBill_Sum = new MySqlCommand(updateBill_SumSql, myConnection);
                    updateBill_Sum.Parameters.AddWithValue("@sum", bill.Sum);
                    updateBill_Sum.ExecuteNonQuery();

                    //insert invoiceItems in Database
                    foreach (var invoiceItem in bill.InvoiceItems) {

                        invoiceItem.Insert(myConnection, nameof(invoiceItem).ToLower());

                        // and update Database Reference to Bill
                        int currentIditem = invoiceItem.ReturnId();
                        var updateInvoiceItem_BillIdSql = $"UPDATE `invoiceitem` SET `bill_id` =@bill_id WHERE `id` = {currentIditem}";
                        MySqlCommand updateInvoiceItem_BillId = new MySqlCommand(updateInvoiceItem_BillIdSql, myConnection);
                        updateInvoiceItem_BillId.Parameters.AddWithValue("@bill_id", bill.ReturnId());
                        updateInvoiceItem_BillId.ExecuteNonQuery();

                        invoiceItem.SetBill_Id(bill.ReturnId());
                    }
                }
            }

            myConnection.Close();
        }
    }
}

//var rechnung2 = Bill.SelectById(3, myConnection);
//var customers = new List<Customer>() { 
//    new Bill() { Number = 1, Title = "Bestellung Buch" },
//    new Bill() { Number = 2, Amount = 12.99, Title = "Bestellung Jacke"},
//};