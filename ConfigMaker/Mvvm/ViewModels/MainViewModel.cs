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
        public ObservableCollection<CategoryViewModel> ActionTabItemViewModels { get; } =
            new ObservableCollection<CategoryViewModel>();
        
        public BuyMenuViewModel BuyMenuViewModel { get; }

        public ObservableCollection<CategoryViewModel> GameSettingsCategoryViewModels { get; } =
            new ObservableCollection<CategoryViewModel>();

        public ObservableCollection<EntryViewModel> ExtraControllerViewModels { get; } =
            new ObservableCollection<EntryViewModel>();
        
        public AliasSetViewModel AliasSetViewModel { get; }

        public VirtualKeyboardViewModel KeyboardViewModel { get; }
        public SearchViewModel SearchViewModel { get; }

        public EntryStateBinding StateBinding
        {
            get => this.Model.StateBinding;
            set => this.Model.StateBinding = value;
        }

        public string CsgoPath
        {
            get => this.Model.CsgoPath;
            set => this.Model.CsgoPath = value;
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

        double _maxActionHeight = -1;

        public double MaxActionHeight
        {
            get => this._maxActionHeight;
            set => this.SetProperty(ref _maxActionHeight, value);
        }

        public AttachmentsViewModel KeyDownAttachmentsVM { get; }
        public AttachmentsViewModel KeyUpAttachmentsVM { get; }
        public AttachmentsViewModel SolidAttachmentsVM { get; }

        public ComboBoxItemsViewModel StateBindingItemsVM { get; }

        public ICommand SelectTabCommand { get; }
        public ICommand SelectAttachmentsCommand { get; }
        public ICommand OpenCfgCommand { get; }
        public ICommand SaveCfgCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ToggleCommand { get; }
        public ICommand SaveAppCommand { get; }
        public ICommand ClickVirtualKey { get; }
        public ICommand GenerateCrosshairsCommand { get; }
        public ICommand AddUnknownCommand { get; }
        public ICommand MoveEntryCommand { get; }
       
        
        public MainViewModel(): base(new MainModel())
        {
            this.KeyboardViewModel = new VirtualKeyboardViewModel(this.Model.KeyboardModel);
            this.SearchViewModel = new SearchViewModel(this.Model.SearchModel);

            this.ClickVirtualKey = new DelegateCommand((obj) =>
            {
                SpecialKey flags = 0;

                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    flags |= SpecialKey.Ctrl;
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    flags |= SpecialKey.Shift;
                if (Keyboard.IsKeyDown(Key.LeftAlt))
                    flags |= SpecialKey.Alt;

                this.Model.ClickButton(obj as string, flags);
            });

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
                    InitialDirectory = this.Model.TargetFolder
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

            this.SaveCfgCommand = new DelegateCommand((obj) =>
            {
                // Определим путь к файлу и передедим его на обработку модели
                string path = Path.Combine(this.Model.TargetFolder, $"{this.CustomCfgName}.cmc");
                this.Model.SaveCfgManager(path);
            });

            this.GenerateCommand = new DelegateCommand((obj) =>
            {
                string cfgPath = Path.Combine(this.Model.TargetFolder, $"{this.CustomCfgName}.cfg");
                this.Model.GenerateConfig(cfgPath);
            });

            this.AboutCommand = new DelegateCommand((obj) =>
            {
                new AboutWindow().ShowDialog();
            });

            this.ToggleCommand = new DelegateCommand((obj) =>
            {
                this.Model.SetToggleCommand(obj.ToString());
            });

            this.SaveAppCommand = new DelegateCommand((obj) => this.Model.SaveRegistry());

            this.GenerateCrosshairsCommand = new DelegateCommand((obj) =>
            {
                this.Model.GenerateRandomCrosshairs();
            });

            this.AddUnknownCommand = new DelegateCommand((obj) => this.Model.AddUnknownCommand());

            this.MoveEntryCommand = new DelegateCommand((obj) =>
            {
                object[] arg = obj as object[];
                int panelNumber = int.Parse(arg[0].ToString());
                bool moveLeft = int.Parse(arg[1].ToString()) == 0;

                AttachmentsModel targetModel = null;

                if (panelNumber == 0) targetModel = this.KeyDownAttachmentsVM.Model;
                else if (panelNumber == 1) targetModel = this.KeyUpAttachmentsVM.Model;
                else targetModel = this.SolidAttachmentsVM.Model;

                this.Model.MoveEntry(targetModel, moveLeft);
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
                else if (arg.PropertyName == nameof(MainModel.KeyboardModel))
                {
                    this.KeyboardViewModel.Raise();
                }
            };

            foreach (CategoryModel item in this.Model.ActionTabItems)
            {
                this.ActionTabItemViewModels.Add(new CategoryViewModel(item));
            }

            this.BuyMenuViewModel = new BuyMenuViewModel(this.Model.BuyMenuModel);

            foreach (CategoryModel category in this.Model.SettingsCategoryModels)
            {
                this.GameSettingsCategoryViewModels.Add(new CategoryViewModel(category));
            }

            foreach (EntryModel model in this.Model.ExtraControllerModels)
            {
                if (model is CustomCmdModel customCmdModel)
                    this.ExtraControllerViewModels.Add(new CustomCmdViewModel(customCmdModel));
                else if (model is CycleCrosshairModel chLoopModel)
                    this.ExtraControllerViewModels.Add(new CycleCrosshairViewModel(chLoopModel));
                else if (model is VolumeRegulatorModel volumeModel)
                    this.ExtraControllerViewModels.Add(new VolumeRegulatorViewModel(volumeModel));
            }

            this.AliasSetViewModel = new AliasSetViewModel(this.Model.AliasSetModel);
            
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
