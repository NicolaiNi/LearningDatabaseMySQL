using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseConsole {
    public class SqlBaseMethods {

        public static void CreateTableBill(MySqlConnection myConnection) {
            var createTableSql = $"CREATE TABLE IF NOT EXISTS bill (" +
                "id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                "number INT NOT NULL DEFAULT 0, " +
                "sum DOUBLE NOT NULL DEFAULT 0, " +
                "title VARCHAR(100), " +
                "customer_id BIGINT, " +
                "CONSTRAINT fk_customer FOREIGN KEY (customer_id) REFERENCES customer(id)" +
                ")";

            MySqlCommand createTable = new MySqlCommand(createTableSql, myConnection);
            createTable.ExecuteNonQuery();
        }

        public static void CreateTableInvoiceItem(MySqlConnection myConnection) {
            var createTableSql = $"CREATE TABLE IF NOT EXISTS invoiceitem (" +
                "id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                "description VARCHAR(100), " +
                "priceitem DOUBLE NOT NULL DEFAULT 0, " +
                "number INT NOT NULL DEFAULT 0, " +
                "pricetotal DOUBLE NOT NULL DEFAULT 0, " +
                "bill_id BIGINT, " +
                "CONSTRAINT fk_bill FOREIGN KEY (bill_id) REFERENCES bill(id)" +
                ")";

            MySqlCommand createTable = new MySqlCommand(createTableSql, myConnection);
            createTable.ExecuteNonQuery();
        }

        public static void CreateTableCustomer(MySqlConnection myConnection) {
            var createTableSql = $"CREATE TABLE IF NOT EXISTS customer (" +
                "id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                "firstname VARCHAR(100), " +
                "lastname VARCHAR(100) " +
                ")";

            MySqlCommand createTable = new MySqlCommand(createTableSql, myConnection);
            createTable.ExecuteNonQuery();
        }




        public static void DisplayData(System.Data.DataTable table) {
            foreach (System.Data.DataRow row in table.Rows) {
                foreach (System.Data.DataColumn col in table.Columns) {
                    Console.WriteLine("{0} = {1}", col.ColumnName, row[col]);
                }
                Console.WriteLine("============================");
            }
        }

        public static void SetForeignKeyChecks(int check, MySqlConnection myConnection) {
            var setForeignKeyZeroSql = $"SET FOREIGN_KEY_CHECKS = {check}";
            MySqlCommand setForeignKeyZero = new MySqlCommand(setForeignKeyZeroSql, myConnection);
            setForeignKeyZero.ExecuteNonQuery();
        }

        public static void DropAllTables(MySqlConnection myConnection) {
            DataTable Tables = myConnection.GetSchema("Tables");
            List<string> tables = new List<string>();
            foreach (DataRow row in Tables.Rows) {
                string tablename = (string)row[2];
                tables.Add(tablename);
            }
            foreach (var table in tables) {
                var dropTableSql = $"DROP TABLE IF EXISTS {table}";
                MySqlCommand dropTable = new MySqlCommand(dropTableSql, myConnection);
                dropTable.ExecuteNonQuery();
            }
        }



    }
}
