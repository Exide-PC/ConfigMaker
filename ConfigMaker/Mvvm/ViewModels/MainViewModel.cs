using ConfigMaker.Csgo.Config;
using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static ConfigMaker.Mvvm.Models.MainModel;
using Res = ConfigMaker.Properties.Resources;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class MainViewModel : ViewModelBase<MainModel>
    {
        public ObservableCollection<BindableBase> ActionTabItemViewModels { get; } =
            new ObservableCollection<BindableBase>();
        
        public BuyMenuViewModel BuyMenuViewModel { get; }

        public ObservableCollection<SettingsCategoryViewModel> GameSettingsCategoryViewModels { get; } =
            new ObservableCollection<SettingsCategoryViewModel>();

        public EntryStateBinding StateBinding
        {
            get => this.Model.StateBinding;
            set => this.Model.StateBinding = value;
        }

        #region Directly exposed from model. TODO: REMOVE
        public KeySequence KeySequence => this.Model.KeySequence;
        public Dictionary<KeySequence, List<Csgo.Config.Entries.BindEntry>> BoundEntries => this.Model.BoundEntries;
        #endregion

        public string CustomCfgPath
        {
            get => this.Model.CustomCfgPath;
            set => this.Model.CustomCfgPath = value;
        }

        public string CustomCfgName
        {
            get => this.Model.CustomCfgName;
            set => this.Model.CustomCfgName = value.Trim();
        }
        
        public AttachmentsViewModel KeyDownAttachmentsVM { get; }
        public AttachmentsViewModel KeyUpAttachmentsVM { get; }
        public AttachmentsViewModel SolidAttachmentsVM { get; }

        public ComboBoxItemsViewModel StateBindingItemsVM { get; }

        public ICommand SelectAttachmentsCommand { get; }
        public ICommand SaveCommand { get; }

        public MainViewModel(): base(new MainModel())
        {
            this.SelectAttachmentsCommand = new DelegateCommand((obj) =>
            {
                int attachmentsBorderTag = (int)obj;

                if (attachmentsBorderTag == 0) this.StateBinding = EntryStateBinding.KeyDown;
                if (attachmentsBorderTag == 1) this.StateBinding = EntryStateBinding.KeyUp;

            });

            this.KeyDownAttachmentsVM = new AttachmentsViewModel(this.Model.KeyDownAttachments) { Tag = 0 };
            this.KeyUpAttachmentsVM = new AttachmentsViewModel(this.Model.KeyUpAttachments) { Tag = 1 };
            this.SolidAttachmentsVM = new AttachmentsViewModel(this.Model.SolidAttachments) { Tag = 2 };

            this.Model.PropertyChanged += (_, arg) =>
            {
                if (arg.PropertyName == nameof(MainModel.StateBinding))
                {
                    // Зададим панелям значение IsSelected
                    this.KeyDownAttachmentsVM.IsSelected = this.StateBinding == EntryStateBinding.KeyDown;
                    this.KeyUpAttachmentsVM.IsSelected = this.StateBinding == EntryStateBinding.KeyUp;
                    this.SolidAttachmentsVM.IsSelected = this.StateBinding != EntryStateBinding.InvalidState;

                    // Определим выбранный индекс в комбобоксе
                    switch (this.Model.StateBinding)
                    {
                        case EntryStateBinding.Default:
                            {
                                this.StateBindingItemsVM.SelectedIndex = 0;
                                break;
                            }
                        case EntryStateBinding.KeyDown:
                        case EntryStateBinding.KeyUp:
                            {
                                this.StateBindingItemsVM.SelectedIndex = 1;
                                break;
                            }
                        case EntryStateBinding.Alias:
                            {
                                this.StateBindingItemsVM.SelectedIndex = 2;
                                break;
                            }
                    }
                }
            };

            foreach (BindableBase item in this.Model.ActionTabItems)
            { 
                if (item is TextModel)
                {
                    TextModel textModel = (TextModel)item;

                    this.ActionTabItemViewModels.Add(new TextViewModel()
                    {
                        Text = textModel.Text,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }
                else if (item is ActionModel)
                {
                    ActionModel actionModel = (ActionModel)item;
                    this.ActionTabItemViewModels.Add(new ActionViewModel(actionModel));
                }
            }

            this.BuyMenuViewModel = new BuyMenuViewModel(this.Model.BuyMenuModel);

            foreach (SettingsCategoryModel category in this.Model.SettingsCategoryModels)
            {
                this.GameSettingsCategoryViewModels.Add(new SettingsCategoryViewModel(category));
            }

            
            this.StateBindingItemsVM = new ComboBoxItemsViewModel()
            {
                Items = new ObservableCollection<string>()
                {
                    Res.Default_Binding,
                    Res.Keyboard_Binding,
                    Res.Alias_Binding
                }
            };
            this.StateBindingItemsVM.PropertyChanged += (_, arg) =>
            {
                if (arg.PropertyName == nameof(ComboBoxItemsViewModel.SelectedIndex))
                {
                    switch (this.StateBindingItemsVM.SelectedIndex)
                    {
                        case 0:
                            {
                                this.StateBinding = EntryStateBinding.Default;
                                break;
                            }
                        case 1:
                            {
                                this.StateBinding = EntryStateBinding.KeyDown;
                                break;
                            }
                        case 2:
                            {
                                this.StateBinding = EntryStateBinding.Alias;
                                break;
                            }
                    }
                }
            };
        }

        public void ClickButton(string button, VirtualKeyboard.SpecialKey flags)
        {
            this.Model.ClickButton(button, flags);
        }
    }
}
