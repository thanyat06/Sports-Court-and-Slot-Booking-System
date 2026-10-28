using System;
using System.Collections.Generic;
using System.Text;
using SportsCourtBooking.Models;
using SportsCourtBooking.Repositories;
using SportsCourtBooking.Services;

namespace SportsCourtBooking;

internal class Program
{

    static async Task Main(string[] args)
    {
        BookingService bookingService = new BookingService(new BookingRepository());
        SlotService slotService = new SlotService(new SlotRepository());
        CourtService courtService = new CourtService(new CourtRepository());
        PaymentService paymentService = new PaymentService(new PaymentRepository());

        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("====================================");
            Console.WriteLine("  SPORTS COURT & SLOT BOOKING SYSTEM");
            Console.WriteLine("====================================");
            Console.WriteLine("1. Manage Courts");
            Console.WriteLine("2. Manage Slots");
            Console.WriteLine("3. Manage Bookings");
            Console.WriteLine("4. Manage Payments");
            Console.WriteLine("5. Exit");
            Console.WriteLine("====================================");

            string choice = ReadText("Enter your choice: ");

            switch (choice)
            {
                case "1":
                    await ManageCourtsAsync(courtService);
                    break;

                case "2":
                    await ManageSlotsAsync(slotService);
                    break;

                case "3":
                    await ManageBookingsAsync(bookingService);
                    break;

                case "4":
                    await ManagePaymentsAsync(paymentService);
                    break;

                case "5":
                    exit = true;
                    Console.WriteLine("Exiting application...");
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    break;
            }
        }
    }

    private static async Task ManageCourtsAsync(CourtService service)
    {
        bool back = false;

        while (!back)
        {
            Console.Clear();
            Console.WriteLine("========== COURT MANAGEMENT ==========");
            Console.WriteLine("1. Add Court");
            Console.WriteLine("2. View All Courts");
            Console.WriteLine("3. Search Court by ID");
            Console.WriteLine("4. Update Court");
            Console.WriteLine("5. Back to Main Menu");
            Console.WriteLine("======================================");

            string choice = ReadText("Enter your choice: ");

            try
            {
                switch (choice)
                {
                    case "1":
                        await AddCourtAsync(service);
                        break;

                    case "2":
                        await ViewAllCourtsAsync(service);
                        break;

                    case "3":
                        await SearchCourtAsync(service);
                        break;

                    case "4":
                        await UpdateCourtAsync(service);
                        break;

                    case "5":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            if (!back)
                Pause();
        }
    }

    private static async Task AddCourtAsync(CourtService service)
    {
        Console.WriteLine("\n--- Add Court ---");

        Court court = new Court
        {
            CourtName = ReadText("Court name: "),
            SportType = ReadSportType(),
            Location = ReadOptionalText("Location (optional): "),
            HourlyRate = ReadDecimal("Hourly rate: "),
            IsActive = true
        };

        int courtId = await service.AddAsync(court);

        Console.WriteLine($"Court added successfully. Court ID: {courtId}");
    }

    private static async Task ViewAllCourtsAsync(CourtService service)
    {
        Console.WriteLine("\n--- All Courts ---");

        List<Court> courts = await service.GetAllAsync();

        if (courts.Count == 0)
        {
            Console.WriteLine("No courts found.");
            return;
        }

        foreach (Court court in courts)
        {
            DisplayCourt(court);
        }
    }

    private static async Task SearchCourtAsync(CourtService service)
    {
        Console.WriteLine("\n--- Search Court ---");

        int courtId = ReadInt("Enter court ID: ");
        Court? court = await service.GetByIdAsync(courtId);

        if (court is null)
        {
            Console.WriteLine("Court not found.");
            return;
        }

        DisplayCourt(court);
    }

    private static async Task UpdateCourtAsync(CourtService service)
    {
        Console.WriteLine("\n--- Update Court ---");

        int courtId = ReadInt("Enter court ID to update: ");
        Court? court = await service.GetByIdAsync(courtId);

        if (court is null)
        {
            Console.WriteLine("Court not found.");
            return;
        }

        Console.WriteLine("Current court details:");
        DisplayCourt(court);

        Console.WriteLine("\nEnter the updated details:");

        court.CourtName = ReadText("Court name: ");
        court.SportType = ReadSportType();
        court.Location = ReadOptionalText("Location (optional): ");
        court.HourlyRate = ReadDecimal("Hourly rate: ");
        court.IsActive = ReadYesNo("Should this court be active? (y/n): ");

        bool updated = await service.UpdateAsync(court);

        Console.WriteLine(updated
            ? "Court updated successfully."
            : "Court could not be updated.");
    }

    private static void DisplayCourt(Court court)
    {
        Console.WriteLine("--------------------------------------");
        Console.WriteLine($"Court ID   : {court.CourtId}");
        Console.WriteLine($"Name       : {court.CourtName}");
        Console.WriteLine($"Sport      : {court.SportType}");
        Console.WriteLine($"Location   : {court.Location ?? "Not specified"}");
        Console.WriteLine($"Hourly Rate: {court.HourlyRate:C}");
        Console.WriteLine($"Status     : {(court.IsActive ? "Active" : "Inactive")}");
        Console.WriteLine("--------------------------------------");
    }

    private static string ReadSportType()
    {
        while (true)
        {
            Console.Write("Sport type (Turf/Badminton): ");
            string value = (Console.ReadLine() ?? "").Trim();

            if (value.Equals("Turf", StringComparison.OrdinalIgnoreCase))
                return "Turf";

            if (value.Equals("Badminton", StringComparison.OrdinalIgnoreCase))
                return "Badminton";

            Console.WriteLine("Enter either Turf or Badminton.");
        }
    }

    private static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = (Console.ReadLine() ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(value))
                return value;

            Console.WriteLine("This field is required.");
        }
    }

    private static string? ReadOptionalText(string prompt)
    {
        Console.Write(prompt);
        string value = (Console.ReadLine() ?? "").Trim();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                return value;

            Console.WriteLine("Enter a valid positive integer.");
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (decimal.TryParse(Console.ReadLine(), out decimal value)
                && value >= 0)
                return value;

            Console.WriteLine("Enter a valid non-negative amount.");
        }
    }

    private static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = (Console.ReadLine() ?? "").Trim();

            if (value.Equals("y", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.Equals("n", StringComparison.OrdinalIgnoreCase))
                return false;

            Console.WriteLine("Enter y or n.");
        }
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }


    private static async Task ManageSlotsAsync(SlotService service)
    {
        bool back = false;

        while (!back)
        {
            Console.Clear();
            Console.WriteLine("========== SLOT MANAGEMENT ==========");
            Console.WriteLine("1. Create Slot");
            Console.WriteLine("2. View All Slots");
            Console.WriteLine("3. Search Slot by ID");
            Console.WriteLine("4. View Available Slots");
            Console.WriteLine("5. Back to Main Menu");
            Console.WriteLine("=====================================");

            string choice = ReadText("Enter your choice: ");

            try
            {
                switch (choice)
                {
                    case "1":
                        await CreateSlotAsync(service);
                        break;

                    case "2":
                        await ViewAllSlotsAsync(service);
                        break;

                    case "3":
                        await SearchSlotAsync(service);
                        break;

                    case "4":
                        await ViewAvailableSlotsAsync(service);
                        break;

                    case "5":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            if (!back)
                Pause();
        }
    }

    private static async Task CreateSlotAsync(SlotService service)
    {
        Console.WriteLine("\n--- Create Slot ---");

        int courtId = ReadInt("Enter court ID: ");
        DateTime slotDate = ReadSlotDate();
        int startHour = ReadStartHour();

        Slot slot = new Slot
        {
            CourtId = courtId,
            SlotDate = slotDate.Date,
            StartTime = TimeSpan.FromHours(startHour),
            EndTime = TimeSpan.FromHours(startHour + 1),
            SlotStatus = "Available",
            BookingId = null
        };

        int slotId = await service.CreateAsync(slot);

        Console.WriteLine($"Slot created successfully. Slot ID: {slotId}");
    }

    private static async Task ViewAllSlotsAsync(SlotService service)
    {
        Console.WriteLine("\n--- All Slots ---");

        List<Slot> slots = await service.GetAllAsync();

        if (slots.Count == 0)
        {
            Console.WriteLine("No slots found.");
            return;
        }

        foreach (Slot slot in slots)
            DisplaySlot(slot);
    }

    private static async Task SearchSlotAsync(SlotService service)
    {
        Console.WriteLine("\n--- Search Slot ---");

        int slotId = ReadInt("Enter slot ID: ");
        Slot? slot = await service.GetByIdAsync(slotId);

        if (slot is null)
        {
            Console.WriteLine("Slot not found.");
            return;
        }

        DisplaySlot(slot);
    }

    private static async Task ViewAvailableSlotsAsync(SlotService service)
    {
        Console.WriteLine("\n--- Available Slots ---");

        int courtId = ReadInt("Enter court ID: ");
        DateTime slotDate = ReadSlotDate();

        List<Slot> slots =
            await service.GetAvailableAsync(courtId, slotDate);

        if (slots.Count == 0)
        {
            Console.WriteLine("No available slots found for this court and date.");
            return;
        }

        foreach (Slot slot in slots)
            DisplaySlot(slot);
    }

    private static void DisplaySlot(Slot slot)
    {
        Console.WriteLine("-------------------------------------");
        Console.WriteLine($"Slot ID   : {slot.SlotId}");
        Console.WriteLine($"Court ID  : {slot.CourtId}");
        Console.WriteLine($"Date      : {slot.SlotDate:dd-MM-yyyy}");
        Console.WriteLine($"Start Time: {DateTime.Today.Add(slot.StartTime):hh:mm tt}");
        Console.WriteLine($"End Time  : {DateTime.Today.Add(slot.EndTime):hh:mm tt}");
        Console.WriteLine($"Status    : {slot.SlotStatus}");
        Console.WriteLine($"Booking ID: {slot.BookingId?.ToString() ?? "None"}");
        Console.WriteLine("-------------------------------------");
    }

    private static DateTime ReadSlotDate()
    {
        while (true)
        {
            Console.Write("Enter date (dd-MM-yyyy): ");
            string input = Console.ReadLine() ?? "";

            if (DateTime.TryParseExact(
                    input,
                    "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime date)
                && date.Date >= DateTime.Today)
            {
                return date.Date;
            }

            Console.WriteLine(
                "Enter a valid date today or later, e.g. 30-10-2026.");
        }
    }

    private static int ReadStartHour()
    {
        while (true)
        {
            Console.Write("Enter start hour (0-22; e.g. 18 for 6 PM): ");

            if (int.TryParse(Console.ReadLine(), out int hour)
                && hour >= 0 && hour <= 22)
            {
                return hour;
            }

            Console.WriteLine(
                "Enter a whole-hour value from 0 to 22.");
        }
    }


    private static async Task ManageBookingsAsync(BookingService service)
    {
        bool back = false;

        while (!back)
        {
            Console.Clear();
            Console.WriteLine("========== BOOKING MANAGEMENT ==========");
            Console.WriteLine("1. Create Booking");
            Console.WriteLine("2. View All Bookings");
            Console.WriteLine("3. Search Booking by ID");
            Console.WriteLine("4. Cancel Booking");
            Console.WriteLine("5. Back to Main Menu");
            Console.WriteLine("========================================");

            string choice = ReadText("Enter your choice: ");

            try
            {
                switch (choice)
                {
                    case "1":
                        await CreateBookingAsync(service);
                        break;

                    case "2":
                        await ViewAllBookingsAsync(service);
                        break;

                    case "3":
                        await SearchBookingAsync(service);
                        break;

                    case "4":
                        await CancelBookingAsync(service);
                        break;

                    case "5":
                        back = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            if (!back)
                Pause();
        }
    }

    private static async Task CreateBookingAsync(BookingService service)
    {
        Console.WriteLine("\n--- Create Booking ---");

        int courtId = ReadInt("Enter court ID: ");
        string customerName = ReadText("Customer name: ");
        string customerPhone = ReadText("Customer phone: ");
        DateTime bookingDate = ReadSlotDate();

        int startHour = ReadStartHour();
        int slotCount;

        while (true)
        {
            slotCount = ReadInt("Number of consecutive hours: ");

            if (startHour + slotCount <= 23)
                break;

            Console.WriteLine(
                "The booking must end by 11:00 PM or earlier. Try a shorter duration.");
        }

        Booking booking = new Booking
        {
            CourtId = courtId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            BookingDate = bookingDate.Date,
            StartTime = TimeSpan.FromHours(startHour),
            EndTime = TimeSpan.FromHours(startHour + slotCount),
            SlotCount = slotCount
        };

        int bookingId = await service.CreateAsync(booking);

        Console.WriteLine("\nBooking created successfully!");
        Console.WriteLine($"Booking ID : {bookingId}");
        Console.WriteLine($"Customer   : {booking.CustomerName}");
        Console.WriteLine($"Total      : {booking.TotalAmount:C}");
        Console.WriteLine($"Status     : {booking.BookingStatus}");
    }

    private static async Task ViewAllBookingsAsync(BookingService service)
    {
        Console.WriteLine("\n--- All Bookings ---");

        List<Booking> bookings = await service.GetAllAsync();

        if (bookings.Count == 0)
        {
            Console.WriteLine("No bookings found.");
            return;
        }

        foreach (Booking booking in bookings)
            DisplayBooking(booking);
    }

    private static async Task SearchBookingAsync(BookingService service)
    {
        Console.WriteLine("\n--- Search Booking ---");

        int bookingId = ReadInt("Enter booking ID: ");
        Booking? booking = await service.GetByIdAsync(bookingId);

        if (booking is null)
        {
            Console.WriteLine("Booking not found.");
            return;
        }

        DisplayBooking(booking);
    }

    private static async Task CancelBookingAsync(BookingService service)
    {
        Console.WriteLine("\n--- Cancel Booking ---");

        int bookingId = ReadInt("Enter booking ID to cancel: ");

        bool cancelled = await service.CancelAsync(bookingId);

        Console.WriteLine(cancelled
            ? "Booking cancelled successfully. Its reserved slots have been released."
            : "Booking could not be cancelled. Check its ID, status, and payment status.");
    }

    private static void DisplayBooking(Booking booking)
    {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Booking ID : {booking.BookingId}");
        Console.WriteLine($"Court ID   : {booking.CourtId}");
        Console.WriteLine($"Customer   : {booking.CustomerName}");
        Console.WriteLine($"Phone      : {booking.CustomerPhone}");
        Console.WriteLine($"Date       : {booking.BookingDate:dd-MM-yyyy}");
        Console.WriteLine($"Start      : {DateTime.Today.Add(booking.StartTime):hh:mm tt}");
        Console.WriteLine($"End        : {DateTime.Today.Add(booking.EndTime):hh:mm tt}");
        Console.WriteLine($"Slot Count : {booking.SlotCount}");
        Console.WriteLine($"Total      : {booking.TotalAmount:C}");
        Console.WriteLine($"Status     : {booking.BookingStatus}");
        Console.WriteLine($"Created At : {booking.CreatedAt}");
        Console.WriteLine("----------------------------------------");
    }

    static async Task ManagePaymentsAsync(PaymentService service)
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("===== PAYMENT MANAGEMENT =====");
            Console.WriteLine("1. View All Payments");
            Console.WriteLine("2. View Payment by Booking ID");
            Console.WriteLine("3. Check Payment Status");
            Console.WriteLine("4. Collect Cash");
            Console.WriteLine("5. Back to Main Menu");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine()?.Trim() ?? "";

            switch (choice)
            {
                case "1":
                    await ViewAllPaymentsAsync(service);
                    break;

                case "2":
                    await ViewPaymentByBookingIdAsync(service);
                    break;

                case "3":
                    await CheckPaymentStatusAsync(service);
                    break;

                case "4":
                    await CollectCashAsync(service);
                    break;

                case "5":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    Pause();
                    break;
            }
        }
    }

    private static async Task ViewAllPaymentsAsync(PaymentService service)
    {
        Console.WriteLine("\n--- All Payments ---");

        List<Payment> payments = await service.GetAllAsync();

        if (payments.Count == 0)
        {
            Console.WriteLine("No payment records found.");
            Pause();
            return;
        }

        foreach (Payment payment in payments)
        {
            DisplayPayment(payment);
        }

        Pause();
    }

    private static async Task ViewPaymentByBookingIdAsync(PaymentService service)
    {
        Console.WriteLine("\n--- Search Payment by Booking ID ---");

        int bookingId = ReadInt("Enter booking ID: ");
        Payment? payment = await service.GetByBookingIdAsync(bookingId);

        if (payment is null)
        {
            Console.WriteLine("No payment found for this booking.");
        }
        else
        {
            DisplayPayment(payment);
        }

        Pause();
    }

    private static async Task CheckPaymentStatusAsync(PaymentService service)
    {
        Console.WriteLine("\n--- Check Payment Status ---");

        int bookingId = ReadInt("Enter booking ID: ");
        string? status = await service.GetPaymentStatusAsync(bookingId);

        if (string.IsNullOrWhiteSpace(status))
        {
            Console.WriteLine("Payment record not found.");
        }
        else
        {
            Console.WriteLine($"Booking ID: {bookingId}");
            Console.WriteLine($"Payment Status: {status}");
        }

        Pause();
    }

    private static async Task CollectCashAsync(PaymentService service)
    {
        Console.WriteLine("\n--- Collect Cash Payment ---");

        int bookingId = ReadInt("Enter booking ID: ");
        string? notes = ReadOptionalText("Enter notes (optional): ");

        bool success = await service.CollectCashAsync(bookingId, notes);

        Console.WriteLine(success
            ? "Cash payment collected successfully."
            : "Payment could not be collected. Check the booking and payment status.");

        Pause();
    }

    private static void DisplayPayment(Payment payment)
    {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Payment ID    : {payment.PaymentId}");
        Console.WriteLine($"Booking ID    : {payment.BookingId}");
        Console.WriteLine($"Amount        : {payment.Amount:C}");
        Console.WriteLine($"Payment Method: {payment.PaymentMethod}");
        Console.WriteLine($"Payment Status: {payment.PaymentStatus}");
        Console.WriteLine($"Collected At  : {payment.CollectedAt?.ToString("dd-MM-yyyy hh:mm tt") ?? "Not collected"}");
        Console.WriteLine($"Notes         : {payment.Notes ?? "None"}");
        Console.WriteLine("----------------------------------------");
    }
}
