using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using ExchangeBooks.Core.Navigation;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Interfaces.Repository;
using ExchangeBooks.Repository;
using ExchangeBooks.Services.Data;
using ExchangeBooks.Services.Framework;
using ExchangeBooks.Services.Http;
using ExchangeBooks.ViewModels;
using ExchangeBooks.Views;
using Xamarin.Forms;

namespace ExchangeBooks.Bootstrap
{
    public class AppContainer
    {
        private static IContainer _container;

        public static Dictionary<string, Type> Routes { get; } = new Dictionary<string, Type>();

        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            //ViewModels
            builder.RegisterType<PostDetailViewModel>().SingleInstance();
            builder.RegisterType<BookDetailViewModel>().SingleInstance();
            builder.RegisterType<BookImageViewModel>().SingleInstance();
            builder.RegisterType<AddPostViewModel>().SingleInstance();
            builder.RegisterType<ProfileViewModel>().SingleInstance();
            builder.RegisterType<SearchViewModel>().SingleInstance();
            builder.RegisterType<FilterViewModel>().SingleInstance();
            builder.RegisterType<MessageViewModel>().SingleInstance();
            builder.RegisterType<ChatViewModel>().SingleInstance();
            builder.RegisterType<NotificationViewModel>().SingleInstance();
            builder.RegisterType<MyPostsViewModel>().SingleInstance();
            builder.RegisterType<ViewPostViewModel>().SingleInstance();
            builder.RegisterType<EulaViewModel>().SingleInstance();
            builder.RegisterType<HidePostViewModel>().SingleInstance();
            builder.RegisterType<FlagPostViewModel>().SingleInstance();
            builder.RegisterType<ViewBookViewModel>();

            //Services - http
            builder.RegisterType<BooksService>().As<IBooksService>().SingleInstance();
            builder.RegisterType<MessagesService>().As<IMessagesService>().SingleInstance();

            //Services - data
            builder.RegisterType<PostDataService>().As<IPostDataService>().SingleInstance();
            builder.RegisterType<MyPostsDataService>().As<IMyPostsDataService>().SingleInstance();
            builder.RegisterType<SearchBooksDataService>().As<ISearchBooksDataService>().SingleInstance();
            builder.RegisterType<HidePostsDataService>().As<IHidePostsDataService>().SingleInstance();
            builder.RegisterType<FlagPostsDataService>().As<IFlagPostsDataService>().SingleInstance();

            //Services - framework
            builder.RegisterType<GenericRepository>().As<IGenericRepository>();
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>().SingleInstance();
            builder.RegisterType<MessagingCenterService>().As<IMessagingCenterService>().SingleInstance();
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterInstance(DependencyService.Get<IEventTracker>()).As<IEventTracker>();
            builder.RegisterInstance(DependencyService.Get<IFcmUtility>()).As<IFcmUtility>();
            builder.RegisterType<PopupNavigation>().As<IPopupNavigation>().SingleInstance();

            //HttpClient
            builder.Register(c => new HttpClient()).As<HttpClient>().SingleInstance();

            //Pages
            builder.RegisterType<EulaPage>();
            builder.RegisterType<HidePostPage>();
            builder.RegisterType<FlagPostPage>();

            _container = builder.Build();
        }

        public static object Resolve(Type typeName)
        {
            return _container.Resolve(typeName);
        }

        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public static void RegisterRoutes()
        {
            if (!Routes.ContainsKey("chat"))
                Routes.Add("chat", typeof(ChatPage));
            if (!Routes.ContainsKey("viewPost"))
                Routes.Add("viewPost", typeof(ViewPostPage));
            if (!Routes.ContainsKey("viewBook"))
                Routes.Add("viewBook", typeof(ViewBookPage));
            foreach (var item in Routes)
            {
                Routing.RegisterRoute(item.Key, item.Value);
            }
        }

        public static void InitializeMessenger()
        {
            var postViewModel = Resolve<AddPostViewModel>();
            postViewModel.InitializeMessenger();
        }
    }
}
