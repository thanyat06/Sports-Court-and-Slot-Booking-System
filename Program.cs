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
