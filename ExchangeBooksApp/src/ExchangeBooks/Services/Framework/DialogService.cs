using System.Threading.Tasks;
using Acr.UserDialogs;
using ExchangeBooks.Interfaces.Framework;

namespace ExchangeBooks.Services.Framework
{
    public class DialogService : IDialogService
    {
        public Task Alert(string message, string title, string okText)
        {
            return UserDialogs.Instance.AlertAsync(message, title, okText);
        }

        public Task<bool> Confirm(string message, string title, string okText, string cancelText)
        {
            return UserDialogs.Instance.ConfirmAsync(message, title, okText, cancelText);
        }

        public void HideLoading()
        {
            UserDialogs.Instance.HideLoading();
        }

        public void ShowLoading()
        {
            UserDialogs.Instance.ShowLoading(title: string.Empty, maskType: MaskType.Clear);
        }

        public void Toast(string message)
        {
            UserDialogs.Instance.Toast(message);
        }
    }
}
