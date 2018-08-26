using ConfigMaker.Csgo.Config;
using ConfigMaker.Mvvm.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
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

        public int SelectedTab
        {
            get => this.Model.SelectedTab;
            set => this.Model.SelectedTab = value;
        }
        
        public AttachmentsViewModel KeyDownAttachmentsVM { get; }
        public AttachmentsViewModel KeyUpAttachmentsVM { get; }
        public AttachmentsViewModel SolidAttachmentsVM { get; }

        public ComboBoxItemsViewModel StateBindingItemsVM { get; }

        public ICommand SelectTabCommand { get; }
        public ICommand SelectAttachmentsCommand { get; }
        public ICommand OpenCfgCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ToggleCommand { get; }
        
        public MainViewModel(): base(new MainModel())
        {
            this.SelectTabCommand = new DelegateCommand((obj) =>
            {
                this.SelectedTab = int.Parse(obj.ToString());
            });

            this.SelectAttachmentsCommand = new DelegateCommand((obj) =>
            {
                int attachmentsBorderTag = (int)obj;

                if (attachmentsBorderTag == 0) this.StateBinding = EntryStateBinding.KeyDown;
                if (attachmentsBorderTag == 1) this.StateBinding = EntryStateBinding.KeyUp;

            });

            this.OpenCfgCommand = new DelegateCommand((obj) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Config Maker Config (*.cmc)|*.cmc",
                    InitialDirectory = this.Model.GetTargetFolder()
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    XmlSerializer cfgSerializer = new XmlSerializer(typeof(ConfigManager));

                    FileInfo fi = new FileInfo(openFileDialog.FileName);
                    string cfgName = fi.Name.Replace(".cmc", "");
                    this.CustomCfgName = cfgName;

                    try
                    {
                        using (FileStream fs = File.OpenRead(openFileDialog.FileName))
                        {
                            ConfigManager cfgManager = (ConfigManager)cfgSerializer.Deserialize(fs);
                            this.Model.LoadCfgManager(cfgManager);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleException("Файл поврежден", ex);
                    }
                }
            });

            this.SaveCommand = new DelegateCommand((obj) =>
            {
                // Определим путь к файлу и передедим его на обработку модели
                string path = Path.Combine(this.Model.GetTargetFolder(), $"{this.CustomCfgName}.cmc");
                this.Model.SaveCfgManager(path);
                // И через модель представления выделим файл в проводнике
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
            });

            this.GenerateCommand = new DelegateCommand((obj) =>
            {
                string cfgPath = Path.Combine(this.Model.GetTargetFolder(), $"{this.CustomCfgName}.cfg");
                this.Model.GenerateConfig(cfgPath);

                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{cfgPath}\"");
            });

            this.AboutCommand = new DelegateCommand((obj) =>
            {
                new AboutWindow().ShowDialog();
            });

            this.ToggleCommand = new DelegateCommand((obj) =>
            {
                this.Model.SetToggleCommand(obj.ToString());
            });

            this.KeyDownAttachmentsVM = new AttachmentsViewModel(this.Model.KeyDownAttachments) { Tag = 0 };
            this.KeyUpAttachmentsVM = new AttachmentsViewModel(this.Model.KeyUpAttachments) { Tag = 1 };
            this.SolidAttachmentsVM = new AttachmentsViewModel(this.Model.SolidAttachments) { Tag = 2 };
            
            this.Model.PropertyChanged += (_, arg) =>
            {
                if (arg.PropertyName == nameof(MainModel.StateBinding))
                {
                    HandleStateBinding(this.StateBinding);
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

            // Разово вызовем метод для обновления моделей представления, 
            // т.к. PropertyChanged был вызван давно в конструкторе модели
            HandleStateBinding(this.StateBinding);

        }

        void HandleStateBinding(EntryStateBinding state)
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

        public void ClickButton(string button, VirtualKeyboard.SpecialKey flags)
        {
            this.Model.ClickButton(button, flags);
        }

        void HandleException(string userMsg, Exception ex)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine(userMsg);

            Exception currentException = ex;

            do
            {
                builder.AppendLine($"{currentException.Message}");
                currentException = currentException.InnerException;
            } while (currentException != null);

            builder.Append($"StackTrace: {ex.StackTrace}");
            MessageBox.Show(builder.ToString());
        }
    }
}
