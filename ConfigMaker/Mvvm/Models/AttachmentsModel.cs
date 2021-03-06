﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class AttachmentsModel: BindableBase
    {
        public ObservableCollection<ItemModel> Items => this.itemsHolder.Items;

        ItemCollectionModel itemsHolder = new ItemCollectionModel();
        string _hint = string.Empty;

        public AttachmentsModel()
        {
            this.itemsHolder.Items.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(Items));
        }

        public string Hint
        {
            get => this._hint;
            set => this.SetProperty(ref _hint, value);
        }

        public int SelectedIndex
        {
            get => this.itemsHolder.SelectedIndex;
            set => this.itemsHolder.SelectedIndex = value;
        }
    }
}
