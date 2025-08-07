using System.Threading.Tasks;

namespace ExchangeBooks.Interfaces.Framework
{
    public interface IDialogService
    {
        Task Alert(string message, string title, string okText);
        void Toast(string message);
        Task<bool> Confirm(string message, string title, string okText, string cancelText);
        void ShowLoading();
        void HideLoading();
    }
}
