using System.Text;

namespace LaundryQueueSystem
{
    // INTERFACES
    public interface ICalculable
    {
        double CalculateFee(ServiceRate rate);
    }

    public interface ITrackable
    {
        string GetStatus();
    }

    // CUSTOMER CLASS
    public class Customer
    {
        public string Name { get; set; }
        public string ContactNumber { get; set; }

        public Customer(string name, string contactNumber)
        {
            Name = name;
            ContactNumber = contactNumber;
        }

        public string GetCustomerDetails()
        {
            return $"{Name} ({ContactNumber})";
        }
    }

    // SERVICE RATE CLASS
    public class ServiceRate
    {
        public double WashRate = 20;
        public double DryRate = 10;
        public double FoldRate = 15;
        public double ComboRate = 40;

        public double ComputeFee(double weightKg, string serviceType)
        {
            return serviceType.ToLower() switch
            {
                "wash" => weightKg * WashRate,
                "dry" => weightKg * DryRate,
                "fold" => weightKg * FoldRate,
                "combo" => weightKg * ComboRate,
                _ => 0
            };
        }

        public bool IsValidServiceType(string serviceType)
        {
            return serviceType.ToLower() switch
            {
                "wash" or "dry" or "fold" or "combo" => true,
                _ => false
            };
        }
    }

    // LAUNDRY ORDER CLASS
    public class LaundryOrder : ICalculable, ITrackable
    {
        public int OrderID { get; private set; }
        public Customer Customer { get; private set; }
        public double WeightKg { get; private set; }
        public string ServiceType { get; private set; }
        public string Status { get; private set; }

        public LaundryOrder(int id, Customer customer, double weightKg, string serviceType)
        {
            OrderID = id;
            Customer = customer;
            WeightKg = weightKg;
            ServiceType = serviceType;
            Status = "For Washing";
        }

        public void UpdateStatus(string newStatus) => Status = newStatus;

        public string GetStatus() => Status;

        public double CalculateFee(ServiceRate rate) => rate.ComputeFee(WeightKg, ServiceType);
    }

    // FILE HANDLER CLASS (AUTO-SAVE + AUTO-LOAD)
    public class FileHandler
    {
        private string filePath = @"C:\Users\Public\LaundryQueueData.csv";

        public void SaveOrders(List<LaundryOrder> orders)
        {
            try
            {
                using StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8);
                writer.WriteLine("OrderID,Name,Contact,Weight,Service,Status");
                foreach (var order in orders)
                {
                    writer.WriteLine($"{order.OrderID},{order.Customer.Name},{order.Customer.ContactNumber},{order.WeightKg},{order.ServiceType},{order.Status}");
                }
                Console.WriteLine($"💾 Queue saved to: {filePath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("🔒 ACCESS DENIED: Cannot save to file. Check file permissions.");
                Console.WriteLine($"Trying alternative location...");
                SaveToAlternativeLocation(orders);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"💾 File error: {ex.Message}");
                Console.WriteLine("Data not saved. Please try again.");
            }
        }

        public List<LaundryOrder> LoadOrders()
        {
            List<LaundryOrder> orders = new();

            try
            {
                if (!File.Exists(filePath)) return orders;

                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        string[] parts = lines[i].Split(',');
                        if (parts.Length < 6) continue;

                        if (!int.TryParse(parts[0], out int id))
                        {
                            Console.WriteLine($"⚠️ Skipping invalid OrderID: {parts[0]}");
                            continue;
                        }

                        string name = parts[1];
                        string contact = parts[2];

                        if (!double.TryParse(parts[3], out double weight))
                        {
                            Console.WriteLine($"⚠️ Skipping invalid weight: {parts[3]}");
                            continue;
                        }

                        string service = parts[4];
                        string status = parts[5];

                        Customer c = new(name, contact);
                        LaundryOrder order = new(id, c, weight, service);
                        order.UpdateStatus(status);
                        orders.Add(order);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error parsing line {i}: {ex.Message}");
                        continue;
                    }
                }

                Console.WriteLine($"📁 Loaded {orders.Count} orders from file.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("🔒 ACCESS DENIED: Cannot read data file.");
                Console.WriteLine("Starting with empty order list...");
                return orders; // Return empty list, don't crash
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("📁 Data file not found. Starting fresh.");
                return orders;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"💾 File error: {ex.Message}");
                Console.WriteLine("Starting with empty order list...");
                return orders;
            }

            return orders;
        }

        public void DeleteQueueFile()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("🗑️ Queue file deleted.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("🔒 Cannot delete file: Access denied.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"⚠️ Error deleting file: {ex.Message}");
            }
        }

        // PRIVATE HELPER METHOD
        private void SaveToAlternativeLocation(List<LaundryOrder> orders)
        {
            try
            {
                // Try saving to My Documents instead
                string altPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "LaundryQueueData.csv");

                using StreamWriter writer = new StreamWriter(altPath, false, Encoding.UTF8);
                writer.WriteLine("OrderID,Name,Contact,Weight,Service,Status");
                foreach (var order in orders)
                {
                    writer.WriteLine($"{order.OrderID},{order.Customer.Name},{order.Customer.ContactNumber},{order.WeightKg},{order.ServiceType},{order.Status}");
                }

                Console.WriteLine($"✅ Data saved to alternative location: {altPath}");
                Console.WriteLine("Please update your file permissions to use the default location.");
            }
            catch
            {
                Console.WriteLine("❌ Could not save to any location. Data may be lost.");
            }
        }
    }

    // RECEIPT GENERATOR
    public class ReceiptGenerator
    {
        public void GenerateReceipt(LaundryOrder order, double fee)
        {
            string filePath = @"C:\Users\Public\Receipt_Order_{order.OrderID}.txt";
            using StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("===== LAUNDRY EXPRESS RECEIPT =====");
            writer.WriteLine($"Date: {DateTime.Now}");
            writer.WriteLine("Address: CEBU INSTITUTE OF TECHNOLOGY  UNIVERSITY");
            writer.WriteLine("-------------------------------------------");
            writer.WriteLine($"Order ID: {order.OrderID}");
            writer.WriteLine($"Customer: {order.Customer.Name}");
            writer.WriteLine($"Contact: {order.Customer.ContactNumber}");
            writer.WriteLine($"Service: {order.ServiceType}");
            writer.WriteLine($"Weight: {order.WeightKg} kg");
            writer.WriteLine($"Status: {order.Status}");
            writer.WriteLine("-------------------------------------------");
            writer.WriteLine($"Total Fee: ₱{fee:F2}");
            writer.WriteLine("Thank you for trusting Laundry Express!");
            Console.WriteLine($"\nReceipt generated: {filePath}");
        }
    }

    public class SalesReport
    {
        private string directory = @"C:\Users\Public\SalesReport";
        private string filePath;

        public SalesReport()
        {
            Directory.CreateDirectory(directory);
            filePath = $@"{directory}\SalesReport_{DateTime.Now:yyyyMMdd}.csv";
            if (!File.Exists(filePath))
            {
                using StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8);
                writer.WriteLine("OrderID,Customer,Service,Weight(kg),Fee(₱),DateCompleted");
            }
        }

        public void RecordFinishedOrder(LaundryOrder order, double fee)
        {
            using StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.WriteLine($"{order.OrderID},{order.Customer.Name},{order.ServiceType},{order.WeightKg},{fee:F2},{DateTime.Now}");
        }

        public void GenerateSalesReport(List<LaundryOrder> orders, ServiceRate rate)
        {
            double totalSales = 0;
            string summaryFile = $@"{directory}\SalesSummary_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            using StreamWriter writer = new StreamWriter(summaryFile, false, Encoding.UTF8);
            writer.WriteLine("===== LAUNDRY EXPRESS SALES REPORT =====");
            writer.WriteLine($"Date Generated: {DateTime.Now}");
            writer.WriteLine("-----------------------------------------------");
            writer.WriteLine("OrderID | Customer | Service | Fee (₱)");
            writer.WriteLine("-----------------------------------------------");

            foreach (var order in orders)
            {
                double fee = order.CalculateFee(rate);
                writer.WriteLine($"{order.OrderID,6} | {order.Customer.Name,-15} | {order.ServiceType,-8} | ₱{fee,8:F2}");
                totalSales += fee;
            }

            writer.WriteLine("-----------------------------------------------");
            writer.WriteLine($"TOTAL SALES: ₱{totalSales:F2}");
            Console.WriteLine($"\nSales summary saved: {summaryFile}");
        }
    }

    //QUEUE MANAGER
    public class QueueManager
    {
        private List<LaundryOrder> orderList;
        private int nextID = 1;

        public QueueManager(List<LaundryOrder> loadedOrders)
        {
            orderList = loadedOrders ?? new();
            if (orderList.Count > 0)
                nextID = orderList.Max(o => o.OrderID) + 1;
        }

        public void AddOrder(Customer customer, double weightKg, string serviceType)
        {
            var order = new LaundryOrder(nextID++, customer, weightKg, serviceType);
            orderList.Add(order);
            Console.WriteLine("\n✅ Order Added Successfully!");
        }

        public void ViewQueue(ServiceRate rate)
        {
            Console.WriteLine("\n================== CURRENT QUEUE ==================");
            Console.WriteLine($"{"OrderID",-8} {"Customer",-15} {"Weight(kg)",-10} {"Service",-10} {"Status",-20} {"Fee (₱)",-10}");
            Console.WriteLine("---------------------------------------------------");

            if (orderList.Count == 0)
            {
                Console.WriteLine("No orders available.");
                return;
            }

            foreach (var order in orderList)
            {
                Console.WriteLine($"{order.OrderID,-8} {order.Customer.Name,-15} {order.WeightKg,-10:F1} {order.ServiceType,-10} {order.Status,-20} ₱{order.CalculateFee(rate),-10:F2}");
            }
        }

        public void UpdateOrderStatus(int orderID, string newStatus)
        {
            var order = orderList.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
            {
                Console.WriteLine("\n⚠️ Order Not Found!");
                return;
            }
            order.UpdateStatus(newStatus);
            Console.WriteLine("\n✅ Status Updated Successfully!");
        }

        public void MarkOrderAsFinished(int orderID, ServiceRate rate, ReceiptGenerator receiptGenerator, SalesReport salesReport)
        {
            var order = orderList.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
            {
                Console.WriteLine("\n⚠️ Order Not Found!");
                return;
            }

            order.UpdateStatus("Finished");
            double fee = order.CalculateFee(rate);

            receiptGenerator.GenerateReceipt(order, fee);
            salesReport.RecordFinishedOrder(order, fee);

            orderList.Remove(order);

            Console.WriteLine("\n✅ Order marked as finished, saved to sales report, and removed from queue!");
        }

        public void ViewClientOrders(string identifier, ServiceRate rate)
        {
            var results = orderList.Where(o =>
                o.Customer.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase)
                || o.Customer.ContactNumber == identifier).ToList();

            if (results.Count == 0)
            {
                Console.WriteLine("\n⚠️ No laundry found under that name/contact.");
                return;
            }

            Console.WriteLine("\n===== YOUR LAUNDRY STATUS =====");
            foreach (var order in results)
            {
                Console.WriteLine($"Order #{order.OrderID} | {order.ServiceType} | {order.WeightKg}kg | Status: {order.Status}");
                if (order.Status == "Ready for Pickup")
                {
                    Console.WriteLine($"Total to Pay: ₱{order.CalculateFee(rate):F2}");
                }
            }
        }

        public List<LaundryOrder> GetOrders() => orderList;

        public void ClearOrders()
        {
            orderList.Clear();
            Console.WriteLine("\n🧾 All orders cleared for a new batch.");
        }
    }

    // BANNER DISPLAY CLASS
    public static class BannerDisplay
    {
        public static void DisplayBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ██╗      █████╗ ██╗   ██╗███╗   ██╗██████╗ ██████╗ ██╗   ██╗    ███████╗██╗  ██╗██████╗ ██████╗ ███████╗███████╗███████╗");
            Console.WriteLine("  ██║     ██╔══██╗██║   ██║████╗  ██║██╔══██╗██╔══██╗╚██╗ ██╔╝    ██╔════╝╚██╗██╔╝██╔══██╗██╔══██╗██╔════╝██╔════╝██╔════╝");
            Console.WriteLine("  ██║     ███████║██║   ██║██╔██╗ ██║██║  ██║██████╔╝ ╚████╔╝     █████╗   ╚███╔╝ ██████╔╝██████╔╝█████╗  ███████╗███████╗");
            Console.WriteLine("  ██║     ██╔══██║██║   ██║██║╚██╗██║██║  ██║██╔══██╗  ╚██╔╝      ██╔══╝   ██╔██╗ ██╔═══╝ ██╔══██╗██╔══╝  ╚════██║╚════██║");
            Console.WriteLine("  ███████╗██║  ██║╚██████╔╝██║ ╚████║██████╔╝██║  ██║   ██║       ███████╗██╔╝ ██╗██║     ██║  ██║███████╗███████║███████║");
            Console.WriteLine("  ╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═════╝ ╚═╝  ╚═╝   ╚═╝       ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝╚══════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void DisplayFooter()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Excellence in Every Wash");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void DisplayServicePrices(ServiceRate rate)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n  ┌───────────────── SERVICE PRICES ────────────────┐");
            Console.WriteLine($"  │ Wash Service  : ₱{rate.WashRate,2}/kg                          │");
            Console.WriteLine($"  │ Dry Service   : ₱{rate.DryRate,2}/kg                          │");
            Console.WriteLine($"  │ Fold Service  : ₱{rate.FoldRate,2}/kg                          │");
            Console.WriteLine($"  │ Combo Service : ₱{rate.ComboRate,2}/kg (All-in-one)             │");
            Console.WriteLine("  └─────────────────────────────────────────────────┘");
            Console.ResetColor();
        }
    }

    // INPUT VALIDATION CLASS
    public static class InputValidator
    {
        public static double GetValidWeight()
        {
            while (true)
            {
                Console.Write("  Enter Laundry Weight (kg): ");
                string input = Console.ReadLine();

                if (double.TryParse(input, out double weight) && weight > 0)
                {
                    return weight;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ❌ Invalid weight! Please enter a valid positive number.");
                    Console.ResetColor();
                }
            }
        }

        public static string GetValidServiceType(ServiceRate rate)
        {
            while (true)
            {
                Console.Write("  Enter Service Type (wash/dry/fold/combo): ");
                string serviceType = Console.ReadLine()?.ToLower().Trim();

                if (rate.IsValidServiceType(serviceType))
                {
                    return serviceType;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ❌ Invalid service type! Please choose from: wash, dry, fold, or combo.");
                    Console.ResetColor();
                    BannerDisplay.DisplayServicePrices(rate);
                }
            }
        }

        public static int GetValidOrderId()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out int id) && id > 0)
                {
                    return id;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  ❌ Invalid Order ID! Please enter a valid positive number: ");
                    Console.ResetColor();
                }
            }
        }
    }

    // LEFT-ALIGNED MENU HELPER
    public static class MenuHelper
    {
        public static int DisplayMenu(string title, string[] options)
        {
            int index = 0;
            ConsoleKey key;

            do
            {
                BannerDisplay.DisplayBanner();

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"  {title}");
                Console.WriteLine($"  {new string('═', title.Length)}");
                Console.ResetColor();
                Console.WriteLine();

                for (int i = 0; i < options.Length; i++)
                {
                    string option = (i == index) ? $"> {options[i]}" : $"  {options[i]}";

                    if (i == index)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"  {option}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {option}");
                    }
                }

                BannerDisplay.DisplayFooter();
                Console.Write("  Use ↑↓ arrows to navigate, ENTER to select: ");

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow && index > 0)
                    index--;
                else if (key == ConsoleKey.DownArrow && index < options.Length - 1)
                    index++;

            } while (key != ConsoleKey.Enter);

            return index;
        }
    }

    // MAIN PROGRAM
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Laundry Express Management System";

            FileHandler fileHandler = new();
            List<LaundryOrder> loadedOrders = fileHandler.LoadOrders();
            QueueManager manager = new(loadedOrders);
            ServiceRate rate = new();
            ReceiptGenerator receiptGenerator = new();
            SalesReport salesReport = new();

            bool exit = false;
            while (!exit)
            {
                int mainChoice = MenuHelper.DisplayMenu(
                    "MAIN MENU",
                    new[] { "Admin Login", "Client Services", "Exit System" });

                switch (mainChoice)
                {
                    case 0: // Admin
                        Console.Write("\n  Enter Admin Password: ");
                        if (Console.ReadLine() == "laundryexpress123")
                        {
                            bool adminExit = false;
                            while (!adminExit)
                            {
                                int adminChoice = MenuHelper.DisplayMenu(
                                    "ADMIN DASHBOARD",
                                    new[]
                                    {
                                        "Add New Order",
                                        "View Current Queue",
                                        "Update Order Status",
                                        "Mark Order as Finished",
                                        "Generate Sales Report",
                                        "Clear All Orders",
                                        "Return to Main Menu"
                                    });

                                switch (adminChoice)
                                {
                                    case 0:
                                        BannerDisplay.DisplayBanner();
                                        Console.WriteLine("  ADD NEW ORDER");
                                        Console.WriteLine("  " + new string('─', 50));
                                        Console.Write("  Enter Customer Name: ");
                                        string name = Console.ReadLine();

                                        Console.Write("  Enter Contact Number: ");
                                        string contact = Console.ReadLine();

                                        // Use validated weight input
                                        double weight = InputValidator.GetValidWeight();

                                        // Display service prices
                                        BannerDisplay.DisplayServicePrices(rate);

                                        // Use validated service type input
                                        string service = InputValidator.GetValidServiceType(rate);

                                        manager.AddOrder(new Customer(name, contact), weight, service);
                                        fileHandler.SaveOrders(manager.GetOrders());
                                        Console.WriteLine("  Press any key to continue...");
                                        Console.ReadKey();
                                        break;

                                    case 1:
                                        BannerDisplay.DisplayBanner();
                                        manager.ViewQueue(rate);
                                        Console.WriteLine("\n  Press any key to continue...");
                                        Console.ReadKey();
                                        break;

                                    case 2:
                                        BannerDisplay.DisplayBanner();
                                        Console.Write("  Enter Order ID to Update: ");
                                        int id1 = InputValidator.GetValidOrderId();
                                        Console.Write("  Enter New Status (For Washing / Drying / Ready for Pickup): ");
                                        string status = Console.ReadLine();
                                        manager.UpdateOrderStatus(id1, status);
                                        fileHandler.SaveOrders(manager.GetOrders());
                                        Console.WriteLine("  Press any key to continue...");
                                        Console.ReadKey();
                                        break;

                                    case 3:
                                        BannerDisplay.DisplayBanner();
                                        Console.Write("  Enter Order ID to Mark as Finished: ");
                                        int id2 = InputValidator.GetValidOrderId();
                                        manager.MarkOrderAsFinished(id2, rate, receiptGenerator, salesReport);
                                        fileHandler.SaveOrders(manager.GetOrders());
                                        Console.WriteLine("  Press any key to continue...");
                                        Console.ReadKey();
                                        break;

                                    case 4:
                                        BannerDisplay.DisplayBanner();
                                        salesReport.GenerateSalesReport(manager.GetOrders(), rate);
                                        Console.WriteLine("  Press any key to continue...");
                                        Console.ReadKey();
                                        break;

                                    case 5:
                                        BannerDisplay.DisplayBanner();
                                        manager.ClearOrders();
                                        fileHandler.DeleteQueueFile();
                                        Console.WriteLine("  Press any key to continue...");
                                        Console.ReadKey();
                                        break;

                                    case 6:
                                        adminExit = true;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("  ❌ Incorrect password!");
                            Console.WriteLine("  Press any key to continue...");
                            Console.ReadKey();
                        }
                        break;

                    case 1: // Client
                        bool clientExit = false;
                        while (!clientExit)
                        {
                            int clientChoice = MenuHelper.DisplayMenu(
                                "CLIENT SERVICES",
                                new[] { "Check My Laundry Status", "Return to Main Menu" });

                            switch (clientChoice)
                            {
                                case 0:
                                    BannerDisplay.DisplayBanner();
                                    Console.Write("  Enter your Name or Contact Number: ");
                                    string identifier = Console.ReadLine();
                                    manager.ViewClientOrders(identifier, rate);
                                    Console.WriteLine("\n  Press any key to continue...");
                                    Console.ReadKey();
                                    break;

                                case 1:
                                    clientExit = true;
                                    break;
                            }
                        }
                        break;

                    case 2:
                        exit = true;
                        break;
                }
            }

            fileHandler.SaveOrders(manager.GetOrders());
            BannerDisplay.DisplayBanner();
            Console.WriteLine("  💾 Queue saved automatically. Thank you for using Laundry Express!");
            Console.WriteLine("  Press any key to exit...");
            Console.ReadKey();
        }
    }
}