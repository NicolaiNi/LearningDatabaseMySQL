using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseConsole {
    public class Bill {

        private int Id { get; set; }
        public int Number { get; set; }
        public double Sum { get; set; }
        public string Title { get; set; }
        private int Customer_Id { get; set; }

        public List<InvoiceItem> InvoiceItems = new List<InvoiceItem>(); 

        public int ReturnId() {
            return this.Id; 
        }

        public void SetCustomer_Id(int id) {
            this.Customer_Id = id; 
        }
        internal void Insert(MySqlConnection myConnection, string table, int customer_id) {
            PropertyInfo[] properties = GetProperties(this);

            StringBuilder sb = new StringBuilder();
            sb.Append($"INSERT INTO {table} (");
            foreach (var prop in properties) {
                sb.Append($"{prop.Name.ToLower()}");
                if(prop != properties.Last()) {
                    sb.Append(",");
                } else {
                    sb.Append(",customer_id) VALUES (");
                }
            }
            foreach (var prop in properties) {
                sb.Append($"@{prop.Name.ToLower()}");
                if (prop != properties.Last()) {
                    sb.Append(",");
                } else {
                    sb.Append($",(SELECT id FROM customer WHERE id=@id))");
                }
            }
            string sql = sb.ToString();
            using (var myCommand = new MySqlCommand(sql, myConnection)) {
                foreach (var prop in properties) {
                    myCommand.Parameters.AddWithValue($"@{prop.Name.ToLower()}", prop.GetValue(this));
                }
                myCommand.Parameters.AddWithValue($"@id", customer_id);
                myCommand.ExecuteNonQuery();
                int id = (int)myCommand.LastInsertedId;
                this.Id = id;
            }
        }

        public static Bill SelectById(int id, MySqlConnection myConnection) {
            var sql = $"SELECT * FROM rechnung WHERE id = @id LIMIT 1";

            using (var myCommand = new MySqlCommand(sql, myConnection)) {
                myCommand.Parameters.AddWithValue($"@{nameof(Id).ToLower()}", id);
                using (var reader = myCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        var rechnung = new Bill();
                        rechnung.Id = reader.GetInt32(nameof(Id).ToLower());
                        rechnung.Number = reader.GetInt32(nameof(Number).ToLower());
                        rechnung.Title = reader.GetString(nameof(Title).ToLower());
                        return rechnung;
                    }
                }
            }
            return null;
        }

        private PropertyInfo[] GetProperties<T>(T obj) {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties;
        }

        public void SetSum() {
            double sum = 0;
            foreach (InvoiceItem item in InvoiceItems) {
                //update item Total
                item.SetPriceTotal();
                sum += item.PriceTotal;
            }
            this.Sum = sum;
        }
    }
}

/* var sql = $"INSERT INTO {table} (" +
          $"{nameof(Number).ToLower()}, {nameof(Title).ToLower()}" +
          $") VALUES (" +
          $"@{nameof(Number).ToLower()}, @{nameof(Title).ToLower()}" +
          $")"; */


//SELECT bill.number, customer.firstname, customer.lastname FROM `bill` INNER JOIN customer ON bill.customer_id = customer.id
//SELECT bill.number, customer.firstname, customer.lastname FROM `bill` RIGHT JOIN customer ON bill.customer_id = customer.id