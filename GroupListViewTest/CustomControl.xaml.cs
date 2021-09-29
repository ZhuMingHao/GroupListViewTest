using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GroupListViewTest
{
    public enum MyEnum
    {
        A,
        B,
        C
    }
    public sealed partial class CustomControl : UserControl
    {
        public CustomControl()
        {
            this.InitializeComponent();
        }

        public MyEnum Ownenum
        {
            get { return (MyEnum)GetValue(OwnenumProperty); }
            set { SetValue(OwnenumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Ownenum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OwnenumProperty =
            DependencyProperty.Register("Ownenum", typeof(MyEnum), typeof(CustomControl), new PropertyMetadata(0, new PropertyChangedCallback(CallBack)));

        private static void CallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {   // process logic
            var control = d as CustomControl;
            switch ((MyEnum)e.NewValue)
            {
                case MyEnum.A:

                    break;
                case MyEnum.B:

                    break;
                case MyEnum.C:

                    break;
                default:
                    break;
            }
        }
    }
}
