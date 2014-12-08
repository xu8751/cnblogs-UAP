﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace CNBlogs
{
    public sealed class NewsTitleTextControl : Control
    {
        public NewsTitleTextControl()
        {
            this.DefaultStyleKey = typeof(NewsTitleTextControl);
            this.Width = CNBlogs.DataHelper.Helper.Functions.GetWindowsWidth();

        }
    }
}
