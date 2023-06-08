using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class RankTable : Grid
    {
        public int NumberOfColumns { get; private set; }
        public int NumberOfRows { get; private set; }
        ICommand tapCommand;
        public ICommand TapCommand => tapCommand;
        private string selectedRow;
        public string SelectedRow {
            get => selectedRow;
            set
            {
                if (selectedRow == value)
                    return;
                selectedRow = value;

                OnPropertyChanged();
            }
        }

        public RankTable() : base()
        {
            tapCommand = new Command(OnTapped);
        }
        public RankTable(List<string> columnNames, Collection<List<string>> rowItems) : base()
        {
            NumberOfColumns = 0;
            NumberOfRows = 0;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                this.ColumnSpacing = 10;
            }
            else
            {
                this.ColumnSpacing = 15;
                this.RowSpacing = 20;
            }
                

            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition { Height = 1 });

            foreach (var name in columnNames)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = 1 });
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var titleLabel = new Label { TextColor = Color.Black, FontAttributes = FontAttributes.Bold, Text = name, HorizontalOptions = LayoutOptions.CenterAndExpand };
                Children.Add(titleLabel, (NumberOfColumns*2)+1, 0);
                NumberOfColumns += 1;
            }

            //Add header seperator
            var line = new BoxView { BackgroundColor = Color.Black, HeightRequest =1,VerticalOptions=LayoutOptions.End, HorizontalOptions = LayoutOptions.FillAndExpand};
            Children.Add(line, 0, ColumnDefinitions.Count, 1,2);

            foreach (var lineItem in rowItems)
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var columnIndex = 0;

                var commandParameter = lineItem[0];
                foreach (var item in lineItem)
                {
                    var itemLabel = new Label { TextColor = Color.Black, Text = item, HorizontalOptions = LayoutOptions.CenterAndExpand };

                    var tapGestureRecognizer = new TapGestureRecognizer
                    {
                        Command = TapCommand,
                        CommandParameter = commandParameter,
                        NumberOfTapsRequired = 1,
                        
                    };
                    
                    itemLabel.GestureRecognizers.Add(tapGestureRecognizer);

                    Children.Add(itemLabel, (columnIndex * 2) + 1, NumberOfRows + 2);

                    columnIndex++;
                }
                NumberOfRows += 1;
            }

            for (int i=2; i < ColumnDefinitions.Count; i+=2)
            {
                Children.Add(GetLine(), i,i+1,1,RowDefinitions.Count);
            }
        }

        public event PropertyChangedEventHandler SelectedRowPropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string name = "")
        {
            var changed = SelectedRowPropertyChanged;
            if (changed == null)
                return;
            changed(this, new PropertyChangedEventArgs(name));
        }
        void OnTapped(object s)
        {
            this.SelectedRow = s as string;
        }
        private BoxView GetLine()
        {
            return new BoxView { BackgroundColor = Color.Black, WidthRequest = 1, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.End };
        }
    }
}
