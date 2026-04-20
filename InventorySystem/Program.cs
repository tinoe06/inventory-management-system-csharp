using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;


namespace InventorySystem
{
    internal class Program
    {
        //Connection to your database
        static string connection = @"Server=.\SQLEXPRESS;Database=InventoryDB;Integrated Security=True;";


        static void Main(string[] args)
        {

            while (true)
            {
                Console.Clear();
                Console.WriteLine("========== INVENTORY SYSTEM ==========");
                Console.WriteLine("1. Add Product");
                Console.WriteLine("2. View Product");
                Console.WriteLine("3. Update Product");
                Console.WriteLine("4. Delete Product");
                Console.WriteLine("5 Record Sale");
                Console.WriteLine("6. Exit");
                
                //menu driven statement
                Console.Write("Choose Option: ");
                int choice =int.Parse(Console.ReadLine());
                switch(choice)
                {
                    case 1: AddProduct(); break;
                    case 2: ViewProduct(); break;
                    case 3: UpdateProduct(); break;
                    case 4: DeleteProduct(); break;
                    case 5: RecordSale(); break;
                    case 6: return;
                    default: Console.WriteLine("Invalid Option (Try Again)"); break;
                }

                Console.Write("Press Any Key...");
                Console.ReadKey();

            }
        }

      

        static void AddProduct()
        {
            try
            {

                Console.Write("Enter Product Name: ");
                string prodname = Console.ReadLine();
                Console.Write("Enter Product Price: $");
                double price = double.Parse(Console.ReadLine());
                Console.Write("Enter Quantity: ");
                int qty = int.Parse(Console.ReadLine());

                string query = "INSERT INTO Products (Name, Price, Quantity) VALUES (@Name, @Price, @Qty);";

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", prodname);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@Qty", qty);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("Product Added Successfully!");

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
         
        }

        static void ViewProduct()
        {
            try
            {
                string query = "SELECT * FROM Products;";

                    using (SqlConnection conn = new SqlConnection(connection)) 
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    Console.WriteLine("\nID     |   Name     |    Price    |    Quantity");
                    Console.WriteLine("---------------------------------------------------");

                    while(reader.Read())
                    {
                        Console.WriteLine($"{reader["ProductID"]} |  {reader["Name"]}         |  {reader["Price"]}         |  {reader["Quantity"]}");
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void UpdateProduct()
        {
            try
            {
                Console.Write("Enter Product ID: ");
                int id = int.Parse(Console.ReadLine());

                Console.Write("New Name: ");
                string name = Console.ReadLine();

                Console.Write("New Price: $");
                double price = double.Parse(Console.ReadLine());

                Console.Write("New Quantity: ");
                int qty = int.Parse(Console.ReadLine());

                string query = "UPDATE Products SET Name=@Name, Price=@Price, Quantity=@Qty WHERE ProductID=@Id";

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@Qty", qty);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        Console.WriteLine("Product Updated Successfully");
                    else
                        Console.WriteLine("Incorrect Method!!!");

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Wrong Input" + ex.Message);
            }
        }

        static void DeleteProduct()
        {
            
           
                Console.Write("Enter Product ID: ");
                int id = int.Parse(Console.ReadLine());
                Console.Write("Are you sure? (y/n): ");
                string confirm = Console.ReadLine();

                if (confirm.ToLower() != "y")
                {
                    Console.Write("Cancelled");
                    return;
                }

                // Delete from sales
                
                
            using (SqlConnection conn = new SqlConnection(connection))
            {
                try
                {
                    conn.Open();
                    string delSalesquery = "DELETE FROM Sales WHERE ProductID=@Id;";
                    SqlCommand cmd = new SqlCommand(delSalesquery, conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();

                    string delProdQuery = "DELETE FROM Products WHERE ProductID=@Id";
                    SqlCommand cmd1 = new SqlCommand(delProdQuery, conn);
                    cmd1.Parameters.AddWithValue("@Id", id);
                    cmd1.ExecuteNonQuery();

                    Console.WriteLine("Product deleted Successfully!");
                }

               catch (Exception ex)
                {
                Console.WriteLine("Invalid Option" + ex.Message);
                 }

            }
            
            
            
        }

        static void RecordSale()
        {
            try
            {
                Console.Write("Enter Product ID: ");
                int productId = int.Parse(Console.ReadLine());

                Console.Write("Quantity Sold: ");
                int qtySold = int.Parse(Console.ReadLine());

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();

                    // check current stock
                    string checkquery = "SELECT Quantity FROM Products WHERE ProductID=@Id;";
                    SqlCommand cmd = new SqlCommand(checkquery, conn);
                    cmd.Parameters.AddWithValue("@Id", productId);
                    object result = cmd.ExecuteScalar();

                    if(result==null)
                    {
                        Console.Write("Product Not Found.");
                        return;
                    }

                    int currentStock = Convert.ToInt32(result);

                    // validating stock
                    if(qtySold>currentStock)
                    {
                        Console.Write("Stock Not Enough");
                        return;
                    }

                    // insert stock
                    string saleQuery = "INSERT INTO Sales (ProductID, QuantitySold) VALUES (@Id, @Qty);";
                    SqlCommand saleCmd = new SqlCommand(saleQuery, conn);
                    saleCmd.Parameters.AddWithValue("@Id", productId);
                    saleCmd.Parameters.AddWithValue("@Qty", qtySold);
                    saleCmd.ExecuteNonQuery();

                    //UPdate Stock
                    string updateQuery = "UPDATE Products SET Quantity = Quantity - @Qty WHERE ProductID=@Id;";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@Qty", qtySold);
                    updateCmd.Parameters.AddWithValue("@Id", productId);
                    updateCmd.ExecuteNonQuery();

                    Console.WriteLine("Sale Recorded and Stock Updated");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }



    }
}
