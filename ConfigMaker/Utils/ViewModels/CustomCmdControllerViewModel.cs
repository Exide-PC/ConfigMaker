using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    public class CustomCmdControllerViewModel: EntryViewModel
    {
        string _cmdName;
        bool _addButtonEnabled = false;
        bool _deleteButtonEnabled = false;

        public ObservableCollection<BubbleViewModel> Bubbles { get; } =
            new ObservableCollection<BubbleViewModel>();

        public CustomCmdControllerViewModel()
        {
            this.Bubbles.CollectionChanged += (_, arg) =>
            {
                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            // Сбросим выделения у всех пузырьков
                            this.Bubbles.Where(b => b.IsSelected).ToList().ForEach(b =>
                            {
                                b.IsSelected = false;
                            });
                            // И выделим первый добавленный
                            ((BubbleViewModel)arg.NewItems[0]).IsSelected = true;
                            this.DeleteButtonEnabled = true;

                            foreach (object bubble in arg.NewItems)
                                ((BubbleViewModel)bubble).Click += BubbleClickHandler;
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            this.Bubbles.Where(b => b.IsSelected).ToList()
                                .ForEach(b => b.IsSelected = false);

                            if (this.Bubbles.Count > 0)
                                this.Bubbles[0].IsSelected = true;
                            else
                                this.DeleteButtonEnabled = false;
                            
                            // TODO: исправить маленькую утечку памяти при удалении элементов
                            break;
                        }
                }
            };
        }

        void BubbleClickHandler(object sender, EventArgs args)
        {
            foreach (BubbleViewModel b in this.Bubbles)
                b.IsSelected = false;

            BubbleViewModel bubble = (BubbleViewModel)sender;
            bubble.IsSelected = true;
        }
        
        public string CmdName
        {
            get => this._cmdName;
            set => this.SetProperty(ref _cmdName, value);
        }

        public bool AddButtonEnabled
        {
            get => this._addButtonEnabled;
            set => this.SetProperty(ref _addButtonEnabled, value);
        }

        public bool DeleteButtonEnabled
        {
            get => this._deleteButtonEnabled;
            set => this.SetProperty(ref _deleteButtonEnabled, value);
        }
    }
}
