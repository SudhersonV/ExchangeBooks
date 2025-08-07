using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Models;
using Xamarin.Forms;
using ExchangeBooks.Core.ViewModels;

namespace ExchangeBooks.ViewModels
{
    public class BookImageViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IDialogService _dialogService;
        private readonly IPostDataService _postDataService;
        private readonly IEventTracker _eventTracker;
        private List<BookImage> _bookImages = new List<BookImage>();
        #endregion
        #region Properties
        public ObservableCollection<Image> BookImages { get; set; } = new ObservableCollection<Image>();
        public Image CurrentBookImage { get; set; }
        public ICommand NextClick => new Command(OnNext);
        public ICommand CameraClick => new Command(OnTakePhoto);
        public ICommand AttachClick => new Command(OnAttachPhoto);
        public ICommand RemoveImage => new Command<Image>((image) => OnRemoveImage(image));
        #endregion

        #region Constructor
        public BookImageViewModel(IDialogService dialogService, IPostDataService postDataService
            , IEventTracker eventTracker, IAuthenticationService authenticationService) : base(authenticationService, dialogService)
        {
            _dialogService = dialogService;
            _postDataService = postDataService;
            _eventTracker = eventTracker;
            Title = "Title";
            Init();
        }
        #endregion

        #region Public Methods
        public async Task ShowAddBookError()
        {
            await _dialogService.Alert("Please add atleast 1 image", "Error", "Ok");
        }
        #endregion

        #region Private Methods
        private async void OnNext()
        {
            _eventTracker.SendEvent("BookImages", "OnNext", "Click");
            if (BookImages == null || !BookImages.Any())
            {
                //await ShowAddBookError();
                //return;
            }
            _postDataService.CurrentBook.Images = _bookImages;
            var paramDictionary = new Dictionary<string, string> {
                { "bookImagesCount", _bookImages.Count.ToString() } };
            _eventTracker.SendEvent("BookImages", paramDictionary);
            await Shell.Current.GoToAsync("//post/post");
        }

        private void Init()
        {
            _eventTracker.SetCurrentScreen("BookImages", nameof(BookImageViewModel));
        }

        private async void OnTakePhoto()
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await _dialogService.Alert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }
            var media = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "AddBooks",
                Name = "book.jpg",
                AllowCropping = true,
                SaveMetaData = true
            });
            if (media == null)
                return;
            //await _dialogService.ShowDialog("File Location", media.Path, "OK");
            var image = new Image();
            using (var stream = media.GetStream())
            {
                image.Source = AddPhotoToBookImages(stream);
                media.Dispose();
            }
            BookImages.Add(image);
            CurrentBookImage = image;
            RefreshImages();
        }

        private string AddPhotoToBookImages(Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            var base64String = Convert.ToBase64String(bytes);
            _bookImages.Add(new BookImage { Content = base64String });
            return base64String ?? string.Empty;
        }

        private async void OnAttachPhoto()
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await _dialogService.Alert("Oops!!", ":( Pick photo not avaialble.", "OK");
                return;
            }
            var mediaFiles = await CrossMedia.Current.PickPhotosAsync(new PickMediaOptions
            {
                SaveMetaData = true,
                PhotoSize = PhotoSize.Medium
            });
            if (mediaFiles == null)
                return;
            mediaFiles.ToList().ForEach(mf =>
            {
                var image = new Image();
                using (var stream = mf.GetStream())
                {
                    image.Source = AddPhotoToBookImages(stream);
                    mf.Dispose();
                }
                BookImages.Add(image);
                CurrentBookImage = image;
            });
            RefreshImages();
        }

        private void OnRemoveImage(Image image)
        {
            var imgIndex = BookImages.IndexOf(image);
            if (imgIndex < 0) return;
            BookImages.RemoveAt(imgIndex);
            _bookImages.RemoveAt(imgIndex);
            RefreshImages();
        }

        private void RefreshImages()
        {
            OnPropertyChanged(nameof(CurrentBookImage));
            OnPropertyChanged(nameof(BookImages));
        }
        #endregion
    }
}
