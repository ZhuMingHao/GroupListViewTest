using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GroupListViewTest
{

    public class IncrementalGroupedListViewHelper
    {
        public static T MyFindDataGridChildOfType<T>(DependencyObject root) where T : class
        {
            var MyQueue = new Queue<DependencyObject>();
            MyQueue.Enqueue(root);
            while (MyQueue.Count > 0)
            {
                DependencyObject current = MyQueue.Dequeue();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    MyQueue.Enqueue(child);
                }
            }
            return null;
        }

        private readonly ListView _listView;
        private readonly ISupportIncrementalLoading _supportIncrementalLoading;

        private ScrollViewer _scrollViewer;
        private ItemsStackPanel _itemsStackPanel;
        public IncrementalGroupedListViewHelper(ListView listView, ISupportIncrementalLoading supportIncrementalLoading)
        {
            _listView = listView;
            _supportIncrementalLoading = supportIncrementalLoading;
            _listView.Loaded += ListViewOnLoaded;
        }
        private async void ListViewOnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(_listView.ItemsPanelRoot is ItemsStackPanel itemsStackPanel)) return;
            _itemsStackPanel = itemsStackPanel;
            _scrollViewer = MyFindDataGridChildOfType<ScrollViewer>(_listView);
            // This event handler loads more items when scrolling.
            _scrollViewer.ViewChanged += async (o, eventArgs) =>
            {
                if (eventArgs.IsIntermediate) return;
                double distanceFromBottom = itemsStackPanel.ActualHeight - _scrollViewer.VerticalOffset - _scrollViewer.ActualHeight;
                if (distanceFromBottom < 10) // 10 is an arbitrary number
                {
                    await LoadMoreItemsAsync(itemsStackPanel);
                }
            };
            // This event handler loads more items when size is changed and there is more
            // room in ListView.
            itemsStackPanel.SizeChanged += async (o, eventArgs) =>
            {
                if (itemsStackPanel.ActualHeight <= _scrollViewer.ActualHeight)
                {
                    await LoadMoreItemsAsync(itemsStackPanel);
                }
            };
            await LoadMoreItemsAsync(itemsStackPanel);
        }
        private async Task LoadMoreItemsAsync(ItemsStackPanel itemsStackPanel)
        {
            if (!_supportIncrementalLoading.HasMoreItems) return;
            // This is to handle the case when the InternalLoadMoreItemsAsync
            // does not fill the entire space of the ListView.
            // This event is needed untill the desired size of itemsStackPanel 
            // is less then the available space.
            itemsStackPanel.LayoutUpdated += OnLayoutUpdated;
            await InternalLoadMoreItemsAsync();
        }
        private async void OnLayoutUpdated(object sender, object e)
        {
            if (_itemsStackPanel.DesiredSize.Height <= _scrollViewer.ActualHeight)
            {
                await InternalLoadMoreItemsAsync();
            }
            else
            {
                _itemsStackPanel.LayoutUpdated -= OnLayoutUpdated;
            }
        }
        private async Task InternalLoadMoreItemsAsync()
        {
            await _supportIncrementalLoading.LoadMoreItemsAsync(5); // 5 is an arbitrary number
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private double _sliderValue;
        public double SliderValue { get { return _sliderValue; } set { _sliderValue = value; NotifyPropertyChanged(); } }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IncremetalStringGroups _availableGroups { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            _availableGroups = new IncremetalStringGroups();

        }
        public static T MyFindDataGridChildOfType<T>(DependencyObject root) where T : class
        {
            var MyQueue = new Queue<DependencyObject>();
            MyQueue.Enqueue(root);
            while (MyQueue.Count > 0)
            {
                DependencyObject current = MyQueue.Dequeue();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    MyQueue.Enqueue(child);
                }
            }
            return null;
        }
        private ObservableCollection<Person> _people;
        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _people = new ObservableCollection<Person>();

            for (int i = 1; i <= 200; i++)
            {
                var p = new Person { Name = "Person " + i };
                _people.Add(p);
            }

            var collection = new IncrementalLoadingCollection<PeopleSource, Person>();

            // PeopleListView.ItemsSource = collection;
           // new IncrementalGroupedListViewHelper(PeopleListView, collection);

            var result =
                from t in _people
                group t by t.Name.Length into g
                orderby g.Key
                select g;

            groupInfoCVS.Source = result;
    
            PeopleListView.ItemsSource = collection;
           
           
        }

        private void PeopleListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(PeopleListView.ItemsPanelRoot is ItemsStackPanel itemsStackPanel)) return;
            var _itemsStackPanel = itemsStackPanel;
            var _scrollViewer = MyFindDataGridChildOfType<ScrollViewer>(PeopleListView);
            // This event handler loads more items when scrolling.
           
            _scrollViewer.ViewChanged += (o, eventArgs) =>
            {
                if (eventArgs.IsIntermediate) return;
                double distanceFromBottom = itemsStackPanel.ActualHeight - _scrollViewer.VerticalOffset - _scrollViewer.ActualHeight;

                System.Diagnostics.Debug.WriteLine($"---------{_scrollViewer.VerticalOffset}---------");
                if (distanceFromBottom < 10) // 10 is an arbitrary number
                {
                    //for (int i = 1; i <= 50; i++)
                    //{
                    //    var p = new Person { Name = "Person " + i };
                    //    _people.Add(p);
                    //    var result =
                    //                from t in _people
                    //                group t by t.Name.Length into g
                    //                orderby g.Key
                    //                select g;

                    //    groupInfoCVS.Source = result;
                    //    PeopleListView.ItemsSource = groupInfoCVS.View;
                    //}
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyModel.SliderValue += 10;
        }
    }

    public class StringGroup : ObservableCollection<string>
    {
        public readonly string Key;
        public StringGroup(string key)
        {
            Key = key;
        }
    }
    public class IncremetalStringGroups : ObservableCollection<StringGroup>, ISupportIncrementalLoading
    {
        private readonly Random _random = new Random();

        private char _nextLetter = 'A';
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var stringGroup = new StringGroup(_nextLetter.ToString());
            int countToAdd = _random.Next(5, 10);
            for (var counter = 0; counter < countToAdd; counter++)
            {
                stringGroup.Add($"{_nextLetter} {counter}");
            }
            Add(stringGroup);
            _nextLetter = (char)(_nextLetter + 1);
            return AsyncInfo.Run(ct => Task.FromResult(new LoadMoreItemsResult { Count = (uint)Count }));
        }
        public bool HasMoreItems => _nextLetter == 'z' + 1;
    }


    public class Person
    {
        public string Name { get; set; }
    }

    public class PeopleSource : IIncrementalSource<Person>
    {
        private readonly ObservableCollection<Person> _people;

        public PeopleSource()
        {
            // Creates an example collection.
            _people = new ObservableCollection<Person>();

            for (int i = 1; i <= 200; i++)
            {
                var p = new Person { Name = "Person " + i };
                _people.Add(p);
            }
        }



        public async Task<IEnumerable<Person>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var result = (from p in _people
                          select p).Skip(pageIndex * pageSize).Take(pageSize);

            // Simulates a longer request...
            await Task.Delay(1000);

            return result;
        }
    }

    // IncrementalLoadingCollection can be bound to a GridView or a ListView. In this case it is a ListView called PeopleListView.




}
