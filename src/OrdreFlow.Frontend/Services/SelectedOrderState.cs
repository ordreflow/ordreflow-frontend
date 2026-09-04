using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

// Holds the order picked on the selection screen so the time-registration form (next up) can read it.
public class SelectedOrderState
{
    public Order? SelectedOrder { get; private set; }

    public event Action? Changed;

    public void Select(Order order)
    {
        SelectedOrder = order;
        Changed?.Invoke();
    }
}
