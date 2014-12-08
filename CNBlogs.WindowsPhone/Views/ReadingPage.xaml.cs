﻿using CNBlogs.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CNBlogs.DataHelper.DataModel;
using CNBlogs.DataHelper.Helper;
using Windows.System;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CNBlogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReadingPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Post post = null;
        private News news = null;
        private string content = string.Empty;

        public Author Author { get; set; }


        public ReadingPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            string commentsCount = string.Empty;
            var pageFile = string.Empty;

            if (e.Parameter is Post)
            {
                this.post = e.Parameter as Post;
                this.Author = post.Author;
                commentsCount = post.CommentsCount;
                CNBlogs.DataHelper.CloudAPI.PostContentDS pcDS = new DataHelper.CloudAPI.PostContentDS(post.ID);
                if (await pcDS.LoadRemoteData())
                {
                    //this.wv_Post.NavigateToString(pcDS.Content);
                    content = pcDS.Content;
                }

                pageFile = "ms-appx-web:///HTML/post.html";
            }
            else if (e.Parameter is News)
            {
                this.news = e.Parameter as News;
                commentsCount = news.CommentsCount;
                CNBlogs.DataHelper.CloudAPI.NewsContentDS ncDS = new DataHelper.CloudAPI.NewsContentDS(news.ID);
                if (await ncDS.LoadRemoteData())
                {
                    //this.wv_Post.NavigateToString(ncDS.News.Content);
                    content = ncDS.News.Content;
                }

                pageFile = "ms-appx-web:///HTML/news.html";

            }

            UpdateUI(commentsCount);

            this.wv_WebContent.Navigate(new Uri(pageFile));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void wv_WebContent_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            try
            {
                // fill post content using javascript
                await this.wv_WebContent.InvokeScriptAsync("setContent", new[] { content });
            }
            catch (Exception ex)
            {
                // invoke script may cause exception
                System.Diagnostics.Debug.WriteLine("exception when set post content", ex.Message);
            }

            Functions.HideProgressBar(this.pb_Top);
        }

        private void btn_Comment_Click(object sender, RoutedEventArgs e)
        {
            if (this.post != null)
            {
                this.Frame.Navigate(typeof(CommentReadingPage), this.post);
            }
            else if (this.news != null)
            {
                this.Frame.Navigate(typeof(CommentReadingPage), this.news);
            }
        }

        private void btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private void btn_MorePost_Click(object sender, RoutedEventArgs e)
        {
            if (this.post != null && !string.IsNullOrEmpty(this.post.BlogApp))
            {
                this.Frame.Navigate(typeof(BloggerPage), this.post.BlogApp);
            }
        }

        private void UpdateUI(string commentsCount)
        {
            if (commentsCount == "0")
            {
                this.btn_Comment.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.btn_Comment.Label = string.Format("{0}条评论", commentsCount);
            }
        }

        private async void wv_WebContent_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            // in this page, we only need to load start page, and will using IE to load other links
            if (args.Uri.Scheme != "ms-appx-web")
            {
                args.Cancel = true;

                // using launcher to open links in IE
                await Launcher.LaunchUriAsync(args.Uri);
            }
        }
    }
}
