using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataBaseConsole {
    public class InvoiceItem {

        private int Id { get; set; }
        public string Description { get; set; }
        public int Number { get; set; }
        public double PriceItem { get; set; }
        public double PriceTotal { get; set;} 
        private int Bill_Id { get; set; }

        public void SetPriceTotal() {
            double priceTotal = this.Number * this.PriceItem;
            this.PriceTotal = priceTotal; 
        }

        internal void Insert(MySqlConnection myConnection, string table) {
            PropertyInfo[] properties = GetProperties(this);

            StringBuilder sb = new StringBuilder();
            sb.Append($"INSERT INTO {table} (");
            foreach (var prop in properties) {
                sb.Append($"{prop.Name.ToLower()}");
                if (prop != properties.Last()) {
                    sb.Append(",");
                } else {
                    sb.Append(") VALUES (");
                }
            }
            foreach (var prop in properties) {
                sb.Append($"@{prop.Name.ToLower()}");
                if (prop != properties.Last()) {
                    sb.Append(",");
                } else {
                    sb.Append(")");
                }
            }
            string sql = sb.ToString();
            using (var myCommand = new MySqlCommand(sql, myConnection)) {
                foreach (var prop in properties) {
                    myCommand.Parameters.AddWithValue($"@{prop.Name.ToLower()}", prop.GetValue(this));
                }
                myCommand.ExecuteNonQuery();
                int id = (int)myCommand.LastInsertedId;
                this.Id = id;
            }
        }
        private PropertyInfo[] GetProperties<T>(T obj) {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties;
        }
        public int ReturnId() {
            return this.Id;
        }
        public void SetBill_Id(int id) {
            this.Bill_Id = id;
        }


    }
}
