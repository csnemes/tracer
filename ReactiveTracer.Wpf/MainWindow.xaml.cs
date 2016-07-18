using System;
using System.Diagnostics;
using System.Reactive;
using System.Windows;
using ReactiveUI;
using Tracer.Observable.Adapters;
using TracerAttributes;

namespace ReactiveTracer.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();

            InitializeComponent();
        }

        public void ExecuteMethod(object sender, RoutedEventArgs e)
        {

        }
    }

    [NoTrace]
    public class MainWindowViewModel : ReactiveObject
    {
       
        public ReactiveList<string> ReactiveTracer { get; } = new ReactiveList<string>();

        public MainWindowViewModel()
        {

            IObserver<string> observer = new AnonymousObserver<string>(s =>
            {
                Debug.WriteLine("xx" + s);
                ReactiveTracer.Add(s);
            });

            LoggerAdapter.TracerSubject.Subscribe(observer);

        }
    }
}