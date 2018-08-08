/*
MIT License

Copyright (c) 2018 Exide-PC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using ConfigMaker.Csgo.Commands;
using ConfigMaker.Csgo.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;
using ConfigMaker.Csgo.Config.Entries;
using ConfigMaker.Utils.Converters;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;
using System.Windows.Documents;
using Res = ConfigMaker.Properties.Resources;
using ConfigMaker.Csgo.Config.Enums;
using ConfigMaker.Csgo.Config.Entries.interfaces;
using ConfigMaker.Utils;
using System.Collections.ObjectModel;
using ConfigMaker.Utils.ViewModels;
using System.Windows.Input;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties/fields/DependencyProperties
        public enum EntryStateBinding
        {
            KeyDown,
            KeyUp,
            Default,
            Alias,
            InvalidState
        }

        ConfigManager cfgManager = new ConfigManager();
        KeySequence currentKeySequence = null;
        AppConfig cfg = null;
        string cfgPath = $"{nameof(AppConfig)}.xml";
        //const string extraAliasSetEntryKey = "ExtraAliasSet";
        
        ObservableCollection<EntryControllerV2> entryV2Controllers = new ObservableCollection<EntryControllerV2>();

        AliasControllerViewModel aliasSetVM = null;
        AttachmentsViewModel keyDownAttachmentsVM = null;
        AttachmentsViewModel keyUpAttachmentsVM = null;
        AttachmentsViewModel solidAttachmentsVM = null;

        public EntryStateBinding StateBinding
        {
            get => (EntryStateBinding) GetValue(StateBindingProperty);
            set => SetValue(StateBindingProperty, value);
        }

        public string CsgoCfgPath
        {
            get => (string)GetValue(CsgoCfgPathProperty);
            set => SetValue(CsgoCfgPathProperty, value);
        }

        public string CfgName
        {
            get => (string)GetValue(CfgNameProperty);
            set => SetValue(CfgNameProperty, value);
        }

        public static readonly DependencyProperty StateBindingProperty;
        public static readonly DependencyProperty CfgNameProperty;
        public static readonly DependencyProperty CsgoCfgPathProperty;

        static MainWindow()
        {
            StateBindingProperty = DependencyProperty.Register(
                "StateBinding",
                typeof(EntryStateBinding),
                typeof(MainWindow),
                new PropertyMetadata(EntryStateBinding.Default));

            CsgoCfgPathProperty = DependencyProperty.Register(
                "CsgoCfgPath",
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata(" "));

            // Локальный метод по валидации имени конфига, который передаётся в метаданные свойства
            object CoerceCfgName(DependencyObject _, object value)
            {
                return ((string)value).Trim();
            };

            CfgNameProperty = DependencyProperty.Register(
                "CfgName",
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata("Config", null, new CoerceValueCallback(CoerceCfgName)));
        }
        #endregion

        #region UI
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Делаем поиск по ресурсам регистронезависимым
            Res.ResourceManager.IgnoreCase = true;

            // Возьмем данные из конфига, если такой есть. Иначе создадим
            XmlSerializer ser = new XmlSerializer(typeof(AppConfig));

            if (!File.Exists(this.cfgPath))
            {
                using (FileStream fs = File.OpenWrite(this.cfgPath))
                {
                    AppConfig defaultCfg = new AppConfig()
                    {
                        CfgName = this.CfgName,
                        CsgoCfgPath = this.CsgoCfgPath
                    };
                    ser.Serialize(fs, defaultCfg);
                }
            }
            // К этому моменту у нас гарантированно создан конфигурационный файл
            using (FileStream fs = File.OpenRead(this.cfgPath))
            {
                
                this.cfg = (AppConfig)ser.Deserialize(fs);

                this.CfgName = this.cfg.CfgName;
                this.CsgoCfgPath = this.cfg.CsgoCfgPath;
            }

            // Добавляем слушателя на нажатие виртуальной клавиатуры
            this.kb.OnKeyboardKeyDown += KeyboardKeyDownHandler;

            // Пусть коллекция сама добавляет слушатель нажатия новым элементам
            this.entryV2Controllers.CollectionChanged += (_, arg) =>
            {
                if (arg.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    EntryControllerV2 controller = (EntryControllerV2)arg.NewItems[0];
                    EntryViewModel entryVM = controller.AttachedViewModel;
                    entryVM.Click += (__, ___) => HandleEntryClick(entryVM.Key);
                }
            };

            keyDownAttachmentsVM = new AttachmentsViewModel();
            keyUpAttachmentsVM = new AttachmentsViewModel();
            solidAttachmentsVM = new AttachmentsViewModel()
            {
                Hint = Localize(Res.CommandsByDefault_Hint)
            };

            // Инициализируем панели
            this.keyDownAttachmentsContentControl.Content = keyDownAttachmentsVM;
            this.keyUpAttachmentsContentControl.Content = keyUpAttachmentsVM;
            this.solidAttachmentsContentControl.Content = solidAttachmentsVM;


            // Инициализируем интерфейс
            InitActionTab();
            InitBuyTab();
            InitGameSettingsTab();
            InitAliasController();
            InitExtra();

            // Проверим, что в словаре нет одинаковых ключей в разном регистре
            // Для этого сгруппируем все ключи, переведя их в нижний регистр,
            // и найдем группу, где больше 1 элемента
            var duplicatedKeyGroup = this.entryV2Controllers.GroupBy(c => c.AttachedViewModel.Key.ToLower())
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatedKeyGroup != null) throw new Exception($"Duplicate key: {duplicatedKeyGroup.Key}");

            // Зададим привязку по умолчанию
            this.SetStateAndUpdateUI(EntryStateBinding.Default);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Удалим прошлый файл
            File.Delete(this.cfgPath);
            // И создадим новый
            using (FileStream fs = File.OpenWrite(this.cfgPath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(AppConfig));
                this.cfg.CfgName = this.CfgName;
                this.cfg.CsgoCfgPath = this.CsgoCfgPath;
                ser.Serialize(fs, this.cfg);
            }
        }

        private void KeyboardKeyDownHandler(object sender, VirtualKeyboard.KeyboardClickRoutedEvtArgs args)
        {
            // Определим новую последовательность
            string key = args.Key.ToLower();

            // Убедимся, что в комбобоксе выделен нужный элемент
            this.iKeyboard.IsSelected = true;

            if (currentKeySequence == null || currentKeySequence.Keys.Length == 2 ||
                currentKeySequence.Keys.Length == 1 && !args.SpecialKeyFlags.HasFlag(VirtualKeyboard.SpecialKey.Shift))
            {
                // Теперь создаем новую последовательность с 1 клавишей
                currentKeySequence = new KeySequence(key);
                this.SetStateAndUpdateUI(EntryStateBinding.KeyDown);

                // Отредактируем текст у панелей
                this.keyDownAttachmentsVM.Hint = string.Format(Res.KeyDown1_Format, currentKeySequence[0].ToUpper());
                this.keyUpAttachmentsVM.Hint = string.Format(Res.KeyUp1_Format, currentKeySequence[0].ToUpper());
            }
            else if (currentKeySequence.Keys.Length == 1)
            {
                // Иначе в последовательности уже есть 1 кнопка и надо добавить вторую
                // Проверяем, что выбрана не та же кнопка
                if (currentKeySequence[0] == key) return;

                currentKeySequence = new KeySequence(currentKeySequence[0], key);
                this.SetStateAndUpdateUI(EntryStateBinding.KeyDown);

                string key1Upper = currentKeySequence[0].ToUpper();
                string key2Upper = currentKeySequence[1].ToUpper();

                this.keyDownAttachmentsVM.Hint = string.Format(Res.KeyDown2_Format, key2Upper, key1Upper);
                this.keyUpAttachmentsVM.Hint = string.Format(Res.KeyUp2_Format, key2Upper, key1Upper);
            }

            ColorizeKeyboard();

            // Обновляем интерфейс под новую последовательность
            this.UpdateAttachmentPanels();
        }

        private void AttachmentsBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Нажатие на панель возможно когда еще не указана последовательность
            if (this.currentKeySequence == null)
                return;

            bool isKeyDownBinding = ((FrameworkElement)sender).DataContext == this.keyDownAttachmentsVM;
            this.SetStateAndUpdateUI(isKeyDownBinding ? EntryStateBinding.KeyDown : EntryStateBinding.KeyUp);

            UpdateAttachmentPanels();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)e.Source;
            ComboBoxItem selectedItem = (ComboBoxItem)cbox.SelectedItem;

            if (selectedItem.Name == iKeyboard.Name)
            {
                // При выборе клавиатуры по умолчанию не выбрана последовательность
                this.SetStateAndUpdateUI(EntryStateBinding.InvalidState);
            }
            else
            {
                if (solidAttachmentsVM == null) return;

                EntryStateBinding selectedState = selectedItem.Name == iDefault.Name ?
                    EntryStateBinding.Default :
                    EntryStateBinding.Alias;

                solidAttachmentsVM.Hint = selectedState == EntryStateBinding.Default ?
                    Res.CommandsByDefault_Hint :
                    Res.CommandsInAlias_Hint;

                this.currentKeySequence = null;
                this.ColorizeKeyboard();

                if (selectedState == EntryStateBinding.Alias)
                {
                    // И при этом если ни одной команды не создано, то задаем неверное состояние
                    selectedState =
                        this.aliasSetVM.Items.Count == 0 ?
                        EntryStateBinding.InvalidState :
                        EntryStateBinding.Alias;
                }

                this.SetStateAndUpdateUI(selectedState);
            }

            UpdateAttachmentPanels();
        }

        private void AddAliasButton_Click(object sender, RoutedEventArgs e)
        {
            this.AddAliasCommand.Execute(null);
        }

        private void DeleteAliasButton_Click(object sender, RoutedEventArgs e)
        {
            this.DeleteAliasCommand.Execute(null);
        }

        private void OpenCfgButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Config Maker Config (*.cmc)|*.cmc",
                InitialDirectory = GetTargetFolder()
            };

            if (openFileDialog.ShowDialog() == true)
            {
                XmlSerializer cfgSerializer = new XmlSerializer(typeof(ConfigManager));

                FileInfo fi = new FileInfo(openFileDialog.FileName);
                string cfgName = fi.Name.Replace(".cmc", "");
                this.CfgName = cfgName;

                try
                {
                    using (FileStream fs = File.OpenRead(openFileDialog.FileName))
                    {
                        this.cfgManager = (ConfigManager)cfgSerializer.Deserialize(fs);
                    }
                    UpdateCfgManager();
                }
                catch (Exception ex)
                {
                    HandleException("Файл поврежден", ex);
                }
            }
        }

        private void SaveCfgButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CfgName.Length == 0)
                this.CfgName = (string) CfgNameProperty.DefaultMetadata.DefaultValue;
            
            string cfgManagerPath = Path.Combine(GetTargetFolder(), $"{this.CfgName}.cmc");

            if (File.Exists(cfgManagerPath)) File.Delete(cfgManagerPath);
            using (FileStream fs = File.OpenWrite(cfgManagerPath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(ConfigManager));
                ser.Serialize(fs, this.cfgManager);
            }
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{cfgManagerPath}\"");
        }
        
        private void SearchCmdTextbox(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            string input = textbox.Text.Trim().ToLower();
            
            DynamicEntryViewModel[] elements = ((IEnumerable<BindableBase>)gameSettingsItemsControl.ItemsSource)
                .Where(item => item is DynamicEntryViewModel).Cast<DynamicEntryViewModel>().ToArray();

            // Выводим все элементы, если ничего не ищем
            if (input.Length == 0)
            {
                foreach (DynamicEntryViewModel element in elements)
                    element.IsVisible = true;
                addUnknownCmdButton.Visibility = Visibility.Hidden;
            }
            else
            {
                int foundCount = 0;

                foreach (DynamicEntryViewModel element in elements)
                {
                    if (element.Key != null)
                    {
                        string entryKey = element.Key;
                        if (entryKey.ToLower().Contains(input))
                        {
                            element.IsVisible = true;
                            foundCount++;
                        }
                        else
                        {
                            element.IsVisible = false;
                        }
                    }
                    else
                        element.IsVisible = false;
                }

                // Если ничего не выведено - предалагем добавить
                if (foundCount == 0)
                {
                    addUnknownCmdButton.Content = string.Format(Res.UnknownCommandExecution_Format, input);
                    addUnknownCmdButton.Visibility = Visibility.Visible;
                }
                else
                    addUnknownCmdButton.Visibility = Visibility.Hidden;
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        #region Proxy methods
        private void CmdInputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //this.HandleCmdNameCommand.Execute(null);
        }

        private void AddCmdButton_Click(object sender, RoutedEventArgs e)
        {
            this.AddCmdCommand.Execute(null);
        }

        private void DeleteCmdButton_Click(object sender, RoutedEventArgs e)
        {
            this.DeleteCmdCommand.Execute(null);
        }

        private void GenerateRandomCrosshairsButton_Click(object sender, RoutedEventArgs e)
        {
            this.GenerateCrosshairsCommand.Execute(null);
        }
        #endregion
        #endregion

        #region Filling UI with config entry managers
        void InitActionTab()
        {
            ObservableCollection<BindableBase> actionTabItems = new ObservableCollection<BindableBase>();
            actionItemsControl.ItemsSource = actionTabItems;

            //// Локальный метод для подготовки и настройки нового чекбокса-контроллера
            ActionViewModel PrepareAction(string cmd, bool isMeta)
            {
                ActionViewModel actionVM = new ActionViewModel()
                {
                    Content = Localize(cmd),
                    Key = cmd,
                    ToolTip = isMeta ? $"+{cmd.ToLower()}" : $"{cmd.ToLower()}"
                };
                
                actionTabItems.Add(actionVM);
                return actionVM;
            }

            // Локальный метод для добавления действий
            void AddAction(string cmd, bool isMeta)
            {
                ActionViewModel actionVM = PrepareAction(cmd, isMeta);

                EntryControllerV2 entryController = new EntryControllerV2
                {
                    AttachedViewModel = actionVM,
                    // генерируем каждый раз новый элемент во избежание замыкания
                    Generate = () =>
                    {
                        return new Entry()
                        {
                            PrimaryKey = cmd,
                            Cmd = new SingleCmd(cmd),
                            IsMetaScript = isMeta,
                            Type = EntryType.Static
                        };
                    },
                    UpdateUI = (entry) => actionVM.UpdateIsChecked(true),
                    Focus = () =>
                    {
                        actionTabButton.IsChecked = true;
                        actionVM.IsFocused = true;
                    },
                    Restore = () => actionVM.UpdateIsChecked(false),
                    HandleState = (state) =>
                    {
                        actionVM.IsEnabled =
                            state != EntryStateBinding.InvalidState
                            && (!isMeta || state == EntryStateBinding.KeyDown);
                    }                    
                };
                entryV2Controllers.Add(entryController);
            };

            // Метод для добавления новой категории.
            void AddActionGroupHeader(string text)
            {
                TextViewModel headerVM = new TextViewModel()
                {
                    Text = text,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                actionTabItems.Add(headerVM);
            };

            AddActionGroupHeader(Res.CategoryCommonActions);
            AddAction("attack", true);
            AddAction("attack2", true);
            AddAction("reload", true);
            AddAction("drop", false);
            AddAction("use", true);
            AddAction("showscores", true);

            AddActionGroupHeader(Res.CategoryMovement);
            AddAction("forward", true);
            AddAction("back", true);
            AddAction("moveleft", true);
            AddAction("moveright", true);
            AddAction("jump", true);
            AddAction("duck", true);
            AddAction("speed", true);

            AddActionGroupHeader(Res.CategoryEquipment);
            AddAction("slot1", false);
            AddAction("slot2", false);
            AddAction("slot3", false);
            AddAction("slot4", false);
            AddAction("slot5", false);
            AddAction("slot6", false);
            AddAction("slot7", false);
            AddAction("slot8", false);
            AddAction("slot9", false);
            AddAction("slot10", false);
            AddAction("slot11", false);
            AddAction("invprev", false);
            AddAction("invnext", false);
            AddAction("lastinv", false);
            AddAction("cl_show_team_equipment", true);

            AddActionGroupHeader(Res.CategoryBuy);
            AddAction("buymenu", false);
            AddAction("autobuy", false);
            AddAction("rebuy", false);
            
            AddActionGroupHeader(Res.CategoryCommunication);
            AddAction("voicerecord", true);
            AddAction("radio1", false);
            AddAction("radio2", false);
            AddAction("radio3", false);
            AddAction("messagemode2", false);
            AddAction("messagemode", false);

            AddActionGroupHeader(Res.CategoryWarmup);
            AddAction("god", false);
            AddAction("noclip", false);
            AddAction("impulse 101", false);
            AddAction("mp_warmup_start", false);
            AddAction("mp_warmup_end", false);
            AddAction("mp_swapteams", false);
            AddAction("bot_add_t", false);
            AddAction("bot_add_ct", false);
            AddAction("bot_place", false);

            AddActionGroupHeader(Res.CategoryOther);
            AddAction("lookatweapon", true);
            AddAction("spray_menu", true);
            AddAction("r_cleardecals", false);
            AddAction("callvote", false);
            AddAction("teammenu", false);
            AddAction("toggleconsole", false);
            AddAction("clear", false);
            AddAction("cl_clearhinthistory", false);
            AddAction("screenshot", false);
            AddAction("pause", false);
            AddAction("disconnect", false);
            AddAction("quit", false);

            AddActionGroupHeader(Res.CategoryDemo);
            AddAction("demo_resume", false);
            AddAction("demo_togglepause", false);

            AddActionGroupHeader(Res.CategoryBonusScripts);

            // jumpthrow script
            const string jumpthrowEntryKey = "Jumpthrow";
            ActionViewModel jumpthrowVM = PrepareAction(jumpthrowEntryKey, true);

            this.entryV2Controllers.Add(new EntryControllerV2()
            {
                AttachedViewModel = jumpthrowVM,
                Focus = () =>
                {
                    actionTabButton.IsChecked = true;
                    jumpthrowVM.IsFocused = true;
                },
                Generate = () =>
                {
                    MetaCmd metaCmd = new MetaCmd(
                        jumpthrowEntryKey,
                        Executable.SplitCommands("+jump; -attack; -attack2"),
                        Executable.SplitCommands("-jump"));

                    return new Entry()
                    {
                        PrimaryKey = jumpthrowEntryKey,
                        Cmd = new SingleCmd(jumpthrowEntryKey),
                        IsMetaScript = true,
                        Type = EntryType.Static,
                        Dependencies = metaCmd
                    };
                },
                UpdateUI = (entry) => jumpthrowVM.UpdateIsChecked(true),
                Restore = () => jumpthrowVM.UpdateIsChecked(false),
                HandleState = (state) => jumpthrowVM.IsEnabled = state == EntryStateBinding.KeyDown
            });

            // DisplayDamageOn
            const string displayDamageOnEntryKey = "DisplayDamage_On";
            ActionViewModel displayDamageOnVM = PrepareAction(displayDamageOnEntryKey, false);

            this.entryV2Controllers.Add(new EntryControllerV2()
                {
                    AttachedViewModel = displayDamageOnVM,
                    Focus = () =>
                    {
                        actionTabButton.IsChecked = true;
                        displayDamageOnVM.IsFocused = true;
                    },
                    Generate = () =>
                    {
                        string[] stringCmds = new string[]
                        {
                            "developer 1",
                            "con_filter_text \"Damage Given\"",
                            "con_filter_text_out \"Player:\"",
                            "con_filter_enable 2"
                        };
                        CommandCollection cmds = new CommandCollection(
                            stringCmds.Select(cmd => new SingleCmd(cmd)));

                        return new Entry()
                        {
                            PrimaryKey = displayDamageOnEntryKey,
                            Cmd = new SingleCmd(displayDamageOnEntryKey),
                            IsMetaScript = false,
                            Type = EntryType.Static,
                            Dependencies = new CommandCollection(new AliasCmd(displayDamageOnEntryKey, cmds))
                        };
                    },
                    UpdateUI = (entry) => displayDamageOnVM.UpdateIsChecked(true),
                    Restore = () => displayDamageOnVM.UpdateIsChecked(false),
                    HandleState = (state) => displayDamageOnVM.IsEnabled = state != EntryStateBinding.InvalidState
                });

            // DisplayDamageOff
            const string displayDamageOffEntryKey = "DisplayDamage_Off";
            ActionViewModel displayDamageOffVM = PrepareAction(displayDamageOffEntryKey, false);

            this.entryV2Controllers.Add(new EntryControllerV2()
                {
                    AttachedViewModel = displayDamageOffVM,
                    Focus = () =>
                    {
                        actionTabButton.IsChecked = true;
                        displayDamageOffVM.IsFocused = true;
                    },
                    Generate = () =>
                    {
                        string[] stringCmds = new string[]
                        {
                            "con_filter_enable 0",
                            "developer 0",
                            "echo \"Display damage: Off!\""
                        };
                        CommandCollection cmds = new CommandCollection(
                            stringCmds.Select(cmd => new SingleCmd(cmd)));

                        return new Entry()
                        {
                            PrimaryKey = displayDamageOffEntryKey,
                            Cmd = new SingleCmd(displayDamageOffEntryKey),
                            IsMetaScript = false,
                            Type = EntryType.Static,
                            Dependencies = new CommandCollection(new AliasCmd(displayDamageOffEntryKey, cmds))
                        };
                    },
                    UpdateUI = (entry) => displayDamageOffVM.UpdateIsChecked(true),
                    Restore = () => displayDamageOffVM.UpdateIsChecked(false),
                    HandleState = (state) => displayDamageOffVM.IsEnabled = state != EntryStateBinding.InvalidState
                });
        }

        void InitBuyTab()
        {
            const string buyScenarioEntryKey = "BuyScenario";

            BuyViewModel buyVM = new BuyViewModel
            {
                Content = Localize(buyScenarioEntryKey),
                Key = buyScenarioEntryKey
            };
            // Зададим контент в лице самой модели представления из которой будет формироваться интерфейс закупки
            buyTabContentControl.Content = buyVM;

            // Локальный метод для получения всего оружия
            List<EntryViewModel> GetWeaponViewModels()
            {
                return buyVM.Categories.SelectMany(c => c.Weapons).ToList();
            };

            // Обработчик интерфейса настроек закупки
            this.entryV2Controllers.Add(new EntryControllerV2()
            {
                AttachedViewModel = buyVM,
                Focus = () => buyTabButton.IsChecked = true,
                UpdateUI = (entry) =>
                {
                    IParametrizedEntry<string[]> extendedEntry = (IParametrizedEntry<string[]>)entry;
                    string[] weapons = extendedEntry.Arg;

                    // зададим состояние чекбоксов согласно аргументам
                    GetWeaponViewModels().ForEach(weaponVM => weaponVM.IsChecked = weapons.Contains((weaponVM.Key)));
                    // не забываем про главный чекбокс
                    buyVM.UpdateIsChecked(true);
                },
                Generate = () =>
                {
                    string[] weaponsToBuy = GetWeaponViewModels()
                        .Where(weaponVM => weaponVM.IsChecked)
                        .Select(weaponVM => weaponVM.Key).ToArray();

                    Executable cmd = null;
                    CommandCollection dependencies = null;

                    List<SingleCmd> buyCmds = weaponsToBuy.Select(weapon => new SingleCmd($"buy {weapon}")).ToList();

                    if (weaponsToBuy.Length == 1)
                    {
                        cmd = buyCmds[0];
                        dependencies = new CommandCollection();
                    }
                    else
                    {
                        AliasCmd buyAlias = new AliasCmd($"{GeneratePrefix()}_{buyScenarioEntryKey}", buyCmds);

                        cmd = buyAlias.Name;
                        dependencies = new CommandCollection(buyAlias);
                    }

                    return new ParametrizedEntry<string[]>()
                    {
                        PrimaryKey = buyScenarioEntryKey,
                        Type = EntryType.Dynamic,
                        Dependencies = dependencies,
                        Cmd = cmd,
                        Arg = weaponsToBuy,
                        IsMetaScript = false
                    };
                },
                Restore = () =>
                {
                    buyVM.UpdateIsChecked(false);
                    GetWeaponViewModels().ForEach(c => c.IsChecked = false);
                },
                HandleState = (state) => buyVM.IsEnabled = state != EntryStateBinding.InvalidState
            });

            //StackPanel currentPanel = null;
            WeaponCategoryViewModel currentCategory = null;

            void AddWeapon(string weaponId, string localizedName)
            {
                EntryViewModel weaponVM = new EntryViewModel
                {
                    Content = localizedName,
                    Key = weaponId
                };

                weaponVM.PropertyChanged += (_, arg) =>
                {
                    if (arg.PropertyName == nameof(EntryViewModel.IsChecked))
                        this.AddEntry(buyScenarioEntryKey, true);
                };

                currentCategory.Weapons.Add(weaponVM);
            };

            // Метод для добавления новой категории. Определяет новый stackpanel и создает текстовую метку
            void AddGroupSeparator(string text)
            {
                currentCategory = new WeaponCategoryViewModel() { Name = text };
                buyVM.Categories.Add(currentCategory);
            };

            AddGroupSeparator(Res.Pistols);
            AddWeapon("glock", Res.Pistol1);
            AddWeapon("elite", Res.Pistol2);
            AddWeapon("p250", Res.Pistol3);
            AddWeapon("fn57", Res.Pistol4);
            AddWeapon("deagle", Res.Pistol5);

            AddGroupSeparator(Res.Heavy);
            AddWeapon("nova", Res.Heavy1);
            AddWeapon("xm1014", Res.Heavy2);
            AddWeapon("mag7", Res.Heavy3);
            AddWeapon("m249", Res.Heavy4);
            AddWeapon("negev", Res.Heavy5);

            AddGroupSeparator(Res.SMGs);
            AddWeapon("mac10", Res.Smg1);
            AddWeapon("mp7", Res.Smg2);
            AddWeapon("ump45", Res.Smg3);
            AddWeapon("p90", Res.Smg4);
            AddWeapon("bizon", Res.Smg5);

            AddGroupSeparator(Res.Rifles);
            AddWeapon("famas", Res.Rifle1);
            AddWeapon("ak47", Res.Rifle2);
            AddWeapon("ssg08", Res.Rifle3);
            AddWeapon("aug", Res.Rifle4);
            AddWeapon("awp", Res.Rifle5);
            AddWeapon("g3sg1", Res.Rifle6);

            AddGroupSeparator(Res.Gear);
            AddWeapon("vest", Res.Gear1);
            AddWeapon("vesthelm", Res.Gear2);
            AddWeapon("taser", Res.Gear3);
            AddWeapon("defuser", Res.Gear4);

            AddGroupSeparator(Res.Grenades);
            AddWeapon("molotov", Res.Grenade1);
            AddWeapon("decoy", Res.Grenade2);
            AddWeapon("flashbang", Res.Grenade3);
            AddWeapon("hegrenade", Res.Grenade4);
            AddWeapon("smokegrenade", Res.Grenade5);
        }
        
        void InitGameSettingsTab()
        {
            ObservableCollection<BindableBase> gameSettingsVMColl = new ObservableCollection<BindableBase>();
            gameSettingsItemsControl.ItemsSource = gameSettingsVMColl;

            DynamicEntryViewModel RegisterEntry(string cmd, BindableBase controllerViewModel, bool needToggle)
            {
                DynamicEntryViewModel entryVM = new DynamicEntryViewModel(controllerViewModel)
                {
                    Key = cmd,
                    NeedToggle = needToggle
                };

                gameSettingsVMColl.Add(entryVM);
                return entryVM;
            }
            
            void AddIntervalCmdController(string cmd, double from, double to, double step, double defaultValue)
            {
                // Определим целочисленный должен быть слайдер или нет
                bool isInteger = from % 1 == 0 && to % 1 == 0 && step % 1 == 0;
                // И создадим модель представления
                IntervalControllerViewModel sliderVM = new IntervalControllerViewModel()
                {
                    From = from,
                    To = to,
                    Step = step
                };

                // Получим базовую модель представления
                DynamicEntryViewModel entryVM = RegisterEntry(cmd, sliderVM, true);
                
                void HandleSliderValue(double value)
                {
                    string formatted = Executable.FormatNumber(value, isInteger);
                    Executable.TryParseDouble(formatted, out double fixedValue);
                    fixedValue = isInteger ? ((int)fixedValue) : fixedValue;

                    entryVM.Content = new SingleCmd($"{cmd} {formatted}").ToString();
                    entryVM.Arg = fixedValue;
                    //resultCmdBlock.Tag = fixedValue;

                    //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                    this.AddEntry(cmd, true);
                }

                sliderVM.PropertyChanged += (sender, arg) =>
                {
                    if (arg.PropertyName == nameof(IntervalControllerViewModel.Value))
                        HandleSliderValue(sliderVM.Value);
                };
                
                // обработчик интерфейса
                this.entryV2Controllers.Add(new EntryControllerV2()
                {
                    AttachedViewModel = entryVM,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        entryVM.IsFocused = true;
                    },
                    Restore = () =>
                    {
                        // Сперва сбрасываем чекбокс, это важно
                        entryVM.UpdateIsChecked(false);
                        sliderVM.Value = defaultValue;
                        entryVM.Arg = defaultValue;
                    },
                    Generate = () =>
                    {
                        if (entryVM.Arg is double)
                        {
                            return new ParametrizedEntry<double>()
                            {
                                PrimaryKey = cmd,
                                Cmd = new SingleCmd(entryVM.Content),
                                IsMetaScript = false,
                                Type = EntryType.Dynamic,
                                Arg = (double)entryVM.Arg
                            };
                        }
                        else
                        {
                            return new ParametrizedEntry<double[]>()
                            {
                                PrimaryKey = cmd,
                                Cmd = new SingleCmd(entryVM.Content),
                                IsMetaScript = false,
                                Type = EntryType.Dynamic,
                                Arg = (double[])entryVM.Arg
                            };
                        }
                    },
                    UpdateUI = (entry) =>
                    {
                        entryVM.UpdateIsChecked(true);
                        if (entry is IParametrizedEntry<double>)
                        {
                            IParametrizedEntry<double> extendedEntry = (IParametrizedEntry<double>)entry;
                            sliderVM.Value = extendedEntry.Arg;
                            entryVM.Arg = extendedEntry.Arg;
                        }
                        else
                        {
                            IParametrizedEntry<double[]> extendedEntry = (IParametrizedEntry<double[]>)entry;
                            entryVM.Content = Executable.GenerateToggleCmd(
                                cmd, extendedEntry.Arg, isInteger).ToString();
                            entryVM.Arg = extendedEntry.Arg;
                        }
                    },
                    HandleState = (state) => entryVM.IsEnabled = state != EntryStateBinding.InvalidState
                });

                // Задаем начальное значение и тут же подключаем обработчика интерфейса
                sliderVM.Value = defaultValue;
                // Вручную вызовем метод для обновления выводимой команды, если стандартное значение равно 0
                if (defaultValue == 0)
                    HandleSliderValue(defaultValue);
            };
            
            void AddComboboxCmdController(string cmd, string[] names, int defaultIndex, bool isIntegerArg, int baseIndex = 0)
            {
                ComboBoxControllerViewModel comboboxVM = new ComboBoxControllerViewModel();
                DynamicEntryViewModel entryVM = RegisterEntry(cmd, comboboxVM, isIntegerArg);
                
                // Если надо предусмотреть функцию toggle, то расширяем сетку и добавляем кнопку
                if (isIntegerArg)
                {
                    //toggleButton.Click += (_, __) =>
                    //{
                    //    ToggleWindow toggleWindow = new ToggleWindow(true, 0, names.Length - 1);
                    //    if ((bool)toggleWindow.ShowDialog())
                    //    {
                    //        int[] values = toggleWindow.GeneratedArg.Split(' ').Select(n => int.Parse(n)).ToArray();
                    //        resultCmdBlock.Text = Executable.GenerateToggleCmd(cmd, values).ToString();
                    //        // Сохраним аргумент в теге
                    //        resultCmdBlock.Tag = values;

                    //        //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                    //        this.AddEntry(cmd, true);
                    //    }
                    //    else
                    //    {

                    //    }
                    //};
                }

                // Зададим элементы комбобокса
                names.ToList().ForEach(name => comboboxVM.Items.Add(name));

                // Создадим обработчика пораньше, т.к. он понадобится уже при задании начального индекса комбобокса
                entryV2Controllers.Add(new EntryControllerV2()
                {
                    AttachedViewModel = entryVM,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        entryVM.IsFocused = true;
                    },
                    Restore = () =>
                    {
                        // Сначала сбрасываем чекбокс, ибо дальше мы с ним сверяемся
                        entryVM.UpdateIsChecked(false);
                        // искусственно сбрасываем выделенный элемент
                        comboboxVM.SelectedIndex = -1;
                        // и гарантированно вызываем обработчик SelectedIndexChanged
                        comboboxVM.SelectedIndex = defaultIndex; 
                    },
                    Generate = () =>
                    {
                        SingleCmd resultCmd = new SingleCmd(entryVM.Content);

                        if (entryVM.Arg is int)
                        {
                            return new ParametrizedEntry<int>()
                            {
                                PrimaryKey = cmd,
                                Type = EntryType.Dynamic,
                                IsMetaScript = false,
                                Cmd = resultCmd,
                                Arg = (int)entryVM.Arg
                            };
                        }
                        else
                        {
                            return new ParametrizedEntry<int[]>()
                            {
                                PrimaryKey = cmd,
                                Type = EntryType.Dynamic,
                                IsMetaScript = false,
                                Cmd = resultCmd,
                                Arg = (int[])entryVM.Arg
                            };
                        }
                    },
                    UpdateUI = (entry) =>
                    {
                        entryVM.UpdateIsChecked(true);

                        if (entry is IParametrizedEntry<int>)
                        {
                            IParametrizedEntry<int> extendedEntry = (IParametrizedEntry<int>)entry;
                            comboboxVM.SelectedIndex = extendedEntry.Arg;
                            entryVM.Arg = extendedEntry.Arg;
                        }
                        else
                        {
                            IParametrizedEntry<int[]> extendedEntry = (IParametrizedEntry<int[]>)entry;
                            entryVM.Content = Executable.GenerateToggleCmd(cmd, extendedEntry.Arg).ToString();
                            entryVM.Arg = extendedEntry.Arg;
                        }
                    },
                    HandleState = (state) => entryVM.IsEnabled = state != EntryStateBinding.InvalidState
                });

                comboboxVM.PropertyChanged += (_, arg) =>
                {
                    if (arg.PropertyName == nameof(ComboBoxControllerViewModel.SelectedIndex))
                    {
                        if (comboboxVM.SelectedIndex == -1) return;

                        string resultCmdStr;
                        if (isIntegerArg)
                            resultCmdStr = $"{cmd} {comboboxVM.SelectedIndex + baseIndex}";
                        else
                            resultCmdStr = $"{cmd} {comboboxVM.SelectedItem}";

                        entryVM.Content = new SingleCmd(resultCmdStr).ToString();
                        entryVM.Arg = comboboxVM.SelectedIndex;
                        //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                        this.AddEntry(cmd, true);
                    }
                };
                
                // Команда по умолчанию обновится, т.к. уже есть обработчик
                comboboxVM.SelectedIndex = defaultIndex;                
            };

            void AddTextboxNumberCmdController(string cmd, double defaultValue, bool asInteger)
            {
                string formattedDefaultStrValue = Executable.FormatNumber(defaultValue, asInteger);
                double coercedDefaultValue = Executable.CoerceNumber(defaultValue, asInteger);

                TextboxControllerViewModel textboxVM = new TextboxControllerViewModel();
                DynamicEntryViewModel entryVM = RegisterEntry(cmd, textboxVM, true);

                //toggleButton.Click += (_, __) =>
                //{
                    //ToggleWindow toggleWindow = new ToggleWindow(asInteger, double.MinValue, double.MaxValue);
                    //if ((bool)toggleWindow.ShowDialog())
                    //{
                    //    double[] values = toggleWindow.GeneratedArg.Split(' ').Select(value =>
                    //    {
                    //        Executable.TryParseDouble(value, out double parsedValue);
                    //        return parsedValue;
                    //    }).ToArray();

                    //    resultCmdBlock.Text = Executable.GenerateToggleCmd(cmd, values, asInteger).ToString();
                    //    // Сохраним аргумент в теге
                    //    resultCmdBlock.Tag = values;

                    //    //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                    //    this.AddEntry(cmd, true);
                    //}
                    //else
                    //{

                    //}
                //};

                textboxVM.PropertyChanged += (_, arg) =>
                {
                    if (arg.PropertyName == nameof(TextboxControllerViewModel.Text))
                    {
                        if (!Executable.TryParseDouble(textboxVM.Text.Trim(), out double fixedValue))
                            return;
                        // Обрезаем дробную часть, если необходимо
                        fixedValue = asInteger ? (int)fixedValue : fixedValue;

                        // сохраним последнее верное значение в тег текстового блока
                        entryVM.Arg = fixedValue;

                        string formatted = Executable.FormatNumber(fixedValue, asInteger);
                        entryVM.Content = new SingleCmd($"{cmd} {formatted}").ToString();

                        //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                        AddEntry(cmd, true);
                    }
                };

                this.entryV2Controllers.Add(new EntryControllerV2()
                {
                    AttachedViewModel = entryVM,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        entryVM.IsFocused = true;
                    },
                    Restore = () =>
                    {
                        entryVM.UpdateIsChecked(false);
                        textboxVM.Text = formattedDefaultStrValue;
                        entryVM.Arg = coercedDefaultValue;
                    },
                    Generate = () =>
                    {
                        SingleCmd generatedCmd = new SingleCmd(entryVM.Content);

                        if (entryVM.Arg is double)
                        {
                            return new ParametrizedEntry<double>()
                            {
                                PrimaryKey = cmd,
                                Cmd = generatedCmd,
                                Type = EntryType.Dynamic,
                                IsMetaScript = false,
                                Arg = (double)entryVM.Arg // Подтягиваем аргумент из тега
                            };
                        }
                        else
                        {
                            return new ParametrizedEntry<double[]>()
                            {
                                PrimaryKey = cmd,
                                Cmd = generatedCmd,
                                Type = EntryType.Dynamic,
                                IsMetaScript = false,
                                Arg = (double[])entryVM.Arg // Подтягиваем аргумент из тега
                            };
                        }
                    },
                    UpdateUI = (entry) =>
                    {
                        entryVM.UpdateIsChecked(true);

                        if (entry is IParametrizedEntry<double>)
                        {
                            IParametrizedEntry<double> extendedEntry = (IParametrizedEntry<double>)entry;
                            textboxVM.Text = Executable.FormatNumber(extendedEntry.Arg, asInteger);
                            entryVM.Arg = extendedEntry.Arg;
                        }
                        else
                        {
                            IParametrizedEntry<double[]> extendedEntry = (IParametrizedEntry<double[]>)entry;
                            double[] values = extendedEntry.Arg;

                            entryVM.Content = Executable.GenerateToggleCmd(cmd, values, asInteger).ToString();
                            entryVM.Arg = extendedEntry.Arg;                            
                        }                        
                    },
                    HandleState = (state) => entryVM.IsEnabled = state != EntryStateBinding.InvalidState
                });

                // Начальное значение
                textboxVM.Text = formattedDefaultStrValue;
            };

            void AddTextboxStringCmdController(string cmd, string defaultValue)
            {
                TextboxControllerViewModel textboxVM = new TextboxControllerViewModel();
                DynamicEntryViewModel entryVM = RegisterEntry(cmd, textboxVM, false);

                textboxVM.PropertyChanged += (_, arg) =>
                {
                    // Обернем в команду только название команды, т.к. аргументы в нижнем регистре не нужны
                    entryVM.Content = $"{new SingleCmd(cmd)} {textboxVM.Text}";

                    // Добавляем в конфиг только если это сделал сам пользователь
                    AddEntry(cmd, true);
                };

                this.entryV2Controllers.Add(new EntryControllerV2()
                {
                    AttachedViewModel = entryVM,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        entryVM.IsFocused = true;
                    },
                    Restore = () =>
                    {
                        entryVM.UpdateIsChecked(false);
                        textboxVM.Text = defaultValue;
                    },
                    Generate = () =>
                    {
                        SingleCmd generatedCmd = new SingleCmd(entryVM.Content, false);

                        return new ParametrizedEntry<string>()
                        {
                            PrimaryKey = cmd,
                            Cmd = generatedCmd,
                            Type = EntryType.Dynamic,
                            IsMetaScript = false,
                            Arg = (string)textboxVM.Text // Подтягиваем аргумент из тега
                        };
                    },
                    UpdateUI = (entry) =>
                    {
                        entryVM.UpdateIsChecked(true);
                        IParametrizedEntry<string> extendedEntry = (IParametrizedEntry<string>)entry;
                        textboxVM.Text = extendedEntry.Arg;
                    },
                    HandleState = (state) => entryVM.IsEnabled = state != EntryStateBinding.InvalidState
                });

                // Начальное значение
                textboxVM.Text = defaultValue;
            };

            void AddGroupHeader(string text)
            {
                TextViewModel headerVM = new TextViewModel()
                {
                    Text = text,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (gameSettingsVMColl.Count != 0)
                    headerVM.Margin = new Thickness(0, 5, 0, 0);

                gameSettingsVMColl.Add(headerVM);
            };

            string[] toggleStrings = new string[] { Res.Off, Res.On };
            string[] primaryWeapons = new string[] { "weapon_m4a4", "weapon_ak47", "weapon_awp" };
            string[] secondaryWeapons = new string[] { "weapon_hkp2000", "weapon_glock", "weapon_deagle" };
            
            AddGroupHeader(Res.CategoryMouseSettings);
            AddTextboxNumberCmdController("sensitivity", 2.5, false);
            AddTextboxNumberCmdController("zoom_sensitivity_ratio_mouse", 1, false);
            AddComboboxCmdController("m_rawinput", toggleStrings, 1, true);
            AddComboboxCmdController("m_customaccel", toggleStrings, 0, true);
            AddIntervalCmdController("m_customaccel_exponent", 0.05, 10, 0.05, 1.05);

            AddGroupHeader(Res.CategoryClientCommands);
            AddComboboxCmdController("cl_autowepswitch", toggleStrings, 1, true);
            AddIntervalCmdController("cl_bob_lower_amt", 5, 30, 1, 21);
            AddIntervalCmdController("cl_bobamt_lat", 0.1, 2, 0.1, 0.4);
            AddIntervalCmdController("cl_bobamt_vert", 0.1, 2, 0.1, 0.25);
            AddIntervalCmdController("cl_bobcycle", 0.1, 2, 0.01, 0.98);
            AddTextboxNumberCmdController("cl_clanid", 0, true);
            AddComboboxCmdController("cl_color", new string[] { Res.Yellow, Res.Purple, Res.Green, Res.Blue, Res.Orange }, 0, true);
            AddComboboxCmdController("cl_dm_buyrandomweapons", toggleStrings, 1, true);
            AddComboboxCmdController("cl_draw_only_deathnotices", toggleStrings, 1, true);
            AddComboboxCmdController("cl_hud_color",
                new string[] { Res.White, Res.LightBlue, Res.Blue, Res.Purple, Res.Red, Res.Orange, Res.Yellow, Res.Green, Res.Aqua, Res.Pink, Res.Default }, 0, true, baseIndex: 1);
            AddComboboxCmdController("cl_hud_healthammo_style", new string[] { Res.Health_Style0, Res.Health_Style1 }, 0, true);
            AddIntervalCmdController("cl_hud_radar_scale", 0.8, 1.3, 0.1, 1);
            AddIntervalCmdController("cl_hud_background_alpha", 0, 1, 0.1, 1);
            AddComboboxCmdController("cl_hud_playercount_pos", new string[] { Res.Top, Res.Bottom }, 0, true);
            AddComboboxCmdController("cl_hud_playercount_showcount", new string[] { Res.ShowAvatars, Res.ShowCount}, 0, true);
            AddComboboxCmdController("cl_mute_enemy_team", toggleStrings, 0, true);
            AddTextboxNumberCmdController("cl_pdump", -1, true);
            AddComboboxCmdController("cl_radar_always_centered", toggleStrings, 0, true);
            AddIntervalCmdController("cl_radar_icon_scale_min", 0.4, 1, 0.01, 0.7);
            AddIntervalCmdController("cl_radar_scale", 0.25, 1, 0.01, 0.7);
            AddComboboxCmdController("cl_righthand", toggleStrings, 0, true);
            AddComboboxCmdController("cl_showfps", toggleStrings, 0, true);
            AddComboboxCmdController("cl_show_clan_in_death_notice", toggleStrings, 0, true);
            AddComboboxCmdController("cl_showpos", toggleStrings, 0, true);
            AddComboboxCmdController("cl_teammate_colors_show", toggleStrings, 0, true);
            AddComboboxCmdController("cl_teamid_overhead_always", toggleStrings, 0, true);
            AddIntervalCmdController("cl_timeout", 4, 30, 1, 30);
            AddComboboxCmdController("cl_use_opens_buy_menu", toggleStrings, 1, true);
            AddIntervalCmdController("cl_viewmodel_shift_left_amt", 0.5, 2, 0.05, 1.5);
            AddIntervalCmdController("cl_viewmodel_shift_right_amt", 0.25, 2, 0.05, 0.75);
            AddComboboxCmdController("cl_cmdrate", new string[] { "64", "128" }, 1, false);
            AddComboboxCmdController("cl_updaterate", new string[] { "64", "128" }, 1, false);

            AddGroupHeader(Res.CategoryCrosshair);
            AddComboboxCmdController("cl_crosshair_drawoutline", toggleStrings, 0, true);
            AddIntervalCmdController("cl_crosshair_dynamic_maxdist_splitratio", 0, 1, 0.1, 0.35);
            AddIntervalCmdController("cl_crosshair_dynamic_splitalpha_innermod", 0, 1, 0.01, 1);
            AddIntervalCmdController("cl_crosshair_dynamic_splitalpha_outermod", 0.3, 1, 0.01, 0.5);
            AddTextboxNumberCmdController("cl_crosshair_dynamic_splitdist", 7, true);
            AddIntervalCmdController("cl_crosshair_outlinethickness", 0.1, 3, 0.1, 1);
            AddTextboxNumberCmdController("cl_crosshair_sniper_width", 1, false);
            AddIntervalCmdController("cl_crosshairalpha", 0, 255, 1, 200);
            AddComboboxCmdController("cl_crosshairdot", toggleStrings, 1, true);
            AddComboboxCmdController("cl_crosshairgap", toggleStrings, 1, true);
            AddComboboxCmdController("cl_crosshair_t", toggleStrings, 1, true);
            AddComboboxCmdController("cl_crosshairgap_useweaponvalue", toggleStrings, 1, true);
            AddTextboxNumberCmdController("cl_crosshairsize", 5, false);
            AddIntervalCmdController("cl_crosshairstyle", 0, 5, 1, 2);
            AddTextboxNumberCmdController("cl_crosshairthickness", 0.5, false);
            AddComboboxCmdController("cl_crosshairusealpha", toggleStrings, 1, true);
            AddTextboxNumberCmdController("cl_fixedcrosshairgap", 3, false);

            AddGroupHeader(Res.CategoryOther);
            AddTextboxStringCmdController("say", "vk.com/exideprod");
            AddTextboxStringCmdController("say_team", "Hello world!");
            AddTextboxStringCmdController("exec", "config");
            AddTextboxNumberCmdController("rate", 786432, true);
            AddTextboxStringCmdController("connect", "12.34.56.78:27015");
            AddTextboxStringCmdController("map", "de_mirage");
            AddTextboxStringCmdController("echo", "blog.exideprod.com");
            AddTextboxStringCmdController("record", "demo_name");
            AddComboboxCmdController("developer", toggleStrings, 0, true);
            AddComboboxCmdController("gameinstructor_enable", toggleStrings, 0, true);
            AddComboboxCmdController("r_drawothermodels", new string[] { "Off", "Normal", "Wireframe" }, 1, true);
            AddTextboxStringCmdController("host_writeconfig", "config_name");
            AddIntervalCmdController("mat_monitorgamma", 1.6, 2.6, 0.1, 2.2);
            AddTextboxNumberCmdController("mm_dedicated_search_maxping", 150, true);
            AddTextboxNumberCmdController("host_timescale", 1, false);
            AddTextboxNumberCmdController("demo_timescale", 1, false);

            AddGroupHeader(Res.CategoryServer);
            AddTextboxNumberCmdController("sv_airaccelerate", 12, true);
            AddTextboxNumberCmdController("sv_accelerate", 5.5, false);
            AddComboboxCmdController("sv_showimpacts", new string[] { "Off", "Server/client", "Client only" }, 0, true);
            AddTextboxNumberCmdController("mp_restartgame", 1, true);
            AddComboboxCmdController("mp_solid_teammates", toggleStrings, 0, true);
            AddComboboxCmdController("mp_ct_default_primary", primaryWeapons, 0, false);
            AddComboboxCmdController("mp_t_default_primary", primaryWeapons, 1, false);
            AddComboboxCmdController("mp_ct_default_secondary", secondaryWeapons, 0, false);
            AddComboboxCmdController("mp_t_default_secondary", secondaryWeapons, 1, false);
            
            AddGroupHeader(Res.CategoryVoiceAndSound);
            AddIntervalCmdController("volume", 0, 1, 0.05, 1);
            AddComboboxCmdController("voice_enable", toggleStrings, 1, true);
            AddComboboxCmdController("voice_loopback", toggleStrings, 1, true);
            AddIntervalCmdController("voice_scale", 0, 1, 0.05, 1);
            AddIntervalCmdController("snd_roundstart_volume", 0, 1, 0.05, 1);
            AddIntervalCmdController("snd_roundend_volume", 0, 1, 0.05, 1);
            AddIntervalCmdController("snd_tensecondwarning_volume", 0, 1, 0.05, 1);
            AddIntervalCmdController("snd_deathcamera_volume", 0, 1, 0.05, 1);

            AddGroupHeader(Res.CategoryNetGraphSettings);
            AddComboboxCmdController("net_graph", toggleStrings, 0, true);
            AddTextboxNumberCmdController("net_graphheight", 64, true);
            AddTextboxNumberCmdController("net_graphpos", 1, true);
            AddComboboxCmdController("net_graphproportionalfont", toggleStrings, 0, true);

            AddGroupHeader(Res.CategoryBots);
            AddComboboxCmdController("bot_stop", toggleStrings, 0, true);
            AddComboboxCmdController("bot_mimic", toggleStrings, 0, true);
            AddComboboxCmdController("bot_crouch", toggleStrings, 0, true);
            AddIntervalCmdController("bot_mimic_yaw_offset", 0, 180, 5, 0);
        }

        public ICommand AddCmdCommand { get; private set; }
        public ICommand DeleteCmdCommand { get; private set; }
        public ICommand GenerateCrosshairsCommand { get; private set; }

        void InitExtra()
        {
            // --- custom command execution ---
            const string execCustomCmdsEntryKey = "ExecCustomCmds";

            CustomCmdControllerViewModel customCmdVM = new CustomCmdControllerViewModel((cmd) => cmd.Trim().Length > 0)
            {
                Content = this.Localize(execCustomCmdsEntryKey),
                Key = execCustomCmdsEntryKey
            };
            extraItemsControl.Items.Add(customCmdVM);

            // Зададим действия по клику на кнопки в этом методе, чтобы ключ нигде кроме как здесь не упоминался
            // Повесим действие по добавлению новой команды на кнопку
            this.AddCmdCommand = new DelegateCommand(() =>
            {
                customCmdVM.Items.Add(new ItemViewModel() { Text = customCmdVM.Input });
                this.AddEntry(execCustomCmdsEntryKey, false);
            });
            // Обработчик нажатия на кнопку добавления неизвестной программе команды
            addUnknownCmdButton.Click += (sender, args) =>
            {
                // Перейдем в клатке с соответствующим контроллером
                this.GetController(execCustomCmdsEntryKey).Focus();
                //this.entryControllers[execCustomCmdsEntryKey].
                customCmdVM.IsChecked = true;

                // Добавим указанную пользователем команду в контроллер ExecCustomCmds
                string cmd = searchCmdBox.Text.Trim();
                customCmdVM.Input = cmd;

                // И выполним команду по добавлению
                this.AddCmdCommand.Execute(null);
                
            };

            // А так же повесим действие на кнопку удаления команды
            this.DeleteCmdCommand = new DelegateCommand(() =>
            {
                int firstSelectedIndex = customCmdVM.Items
                    .IndexOf(customCmdVM.Items.First(b => b.IsSelected));

                customCmdVM.Items.RemoveAt(firstSelectedIndex);
                AddEntry(execCustomCmdsEntryKey, false);
            });

            this.entryV2Controllers.Add(new EntryControllerV2()
            {
                AttachedViewModel = customCmdVM,
                Focus = () => extraTabButton.IsChecked = true,
                HandleState = (state) => customCmdVM.IsEnabled = state != EntryStateBinding.InvalidState,
                Restore = () =>
                {
                    customCmdVM.Items.Clear();

                    customCmdVM.Input = string.Empty;
                    customCmdVM.UpdateIsChecked(false);
                },
                Generate = () =>
                {
                    // Получим все указанные пользователем команды
                    string[] cmds = customCmdVM.Items.Select(b => b.Text).ToArray();

                    SingleCmd cmd = null;
                    CommandCollection dependencies = null;

                    // Если указана только одна команда - просто выписываем её в конфиг напрямую
                    if (cmds.Length == 1)
                    {
                        cmd = new SingleCmd(cmds[0].Trim());
                        dependencies = new CommandCollection();
                    }
                    else
                    {
                        // Иначе генерируем специальный алиас и привязываемся к нему
                        string aliasName = $"{GeneratePrefix()}_exec";
                        AliasCmd execCmdsAlias = new AliasCmd(aliasName, cmds.Select(strCmd => new SingleCmd(strCmd)));

                        cmd = new SingleCmd(aliasName);
                        dependencies = new CommandCollection(execCmdsAlias);
                    }

                    return new ParametrizedEntry<string[]>()
                    {
                        PrimaryKey = execCustomCmdsEntryKey,
                        Cmd = cmd,
                        IsMetaScript = false,
                        Type = EntryType.Dynamic,
                        Dependencies = dependencies,
                        Arg = cmds
                    };
                },
                UpdateUI = (entry) =>
                {
                    customCmdVM.UpdateIsChecked(true);

                    IParametrizedEntry<string[]> extendedEntry = (IParametrizedEntry<string[]>)entry;
                    string[] cmds = extendedEntry.Arg;

                    customCmdVM.Items.Clear();

                    foreach (string cmd in cmds)
                    {
                        customCmdVM.Input = cmd;
                        this.AddCmdCommand.Execute(null);
                    }
                }
            });

            // cycle crosshairs
            const string cycleCrosshairEntryKey = "CycleCrosshair";

            CycleCrosshairViewModel cycleChVM = new CycleCrosshairViewModel()
            {
                Content = Localize(cycleCrosshairEntryKey),
                Key = cycleCrosshairEntryKey
            };
            extraItemsControl.Items.Add(cycleChVM);

            this.GenerateCrosshairsCommand = new DelegateCommand(() =>
            {
                GenerateRandomCrosshairs(cycleChVM.CrosshairCount);
            });

            this.entryV2Controllers.Add(new EntryControllerV2()
            {
                AttachedViewModel = cycleChVM,
                Focus = () =>
                {
                    extraTabButton.IsChecked = true;
                    cycleChVM.IsFocused = true;
                },
                Generate = () =>
                {
                    int crosshairCount = cycleChVM.CrosshairCount;
                    string prefix = GeneratePrefix();
                    string scriptName = $"{prefix}_crosshairLoop";

                    // Зададим имена итерациям
                    string[] iterationNames = new string[crosshairCount];

                    for (int i = 0; i < crosshairCount; i++)
                        iterationNames[i] = $"{scriptName}{i + 1}";

                    List<CommandCollection> iterations = new List<CommandCollection>();

                    for (int i = 0; i < crosshairCount; i++)
                    {
                        CommandCollection currentIteration = new CommandCollection()
                        {
                            new SingleCmd($"exec {prefix}_ch{i + 1}"),
                            new SingleCmd($"echo \"Crosshair {i + 1} loaded\"")
                        };
                        iterations.Add(currentIteration);
                    }

                    CycleCmd crosshairLoop = new CycleCmd(scriptName, iterations, iterationNames);

                    // Задаем начальную команду для алиаса
                    CommandCollection dependencies = new CommandCollection();

                    // И добавим в конец все итерации нашего цикла
                    foreach (Executable iteration in crosshairLoop)
                        dependencies.Add(iteration);

                    return new ParametrizedEntry<int>()
                    {
                        PrimaryKey = cycleCrosshairEntryKey,
                        Cmd = new SingleCmd(scriptName),
                        Type = EntryType.Dynamic,
                        IsMetaScript = false,
                        Arg = crosshairCount,
                        Dependencies = dependencies
                    };
                },
                UpdateUI = (entry) =>
                {
                    cycleChVM.UpdateIsChecked(true);
                    int crosshairCount = (entry as IParametrizedEntry<int>).Arg;

                    cycleChVM.CrosshairCount = crosshairCount;
                },
                Restore = () =>
                {
                    cycleChVM.UpdateIsChecked(false);
                    cycleChVM.CrosshairCount = cycleChVM.MinimumCount;
                },
                HandleState = (state) => cycleChVM.IsEnabled =
                    state != EntryStateBinding.Default && state != EntryStateBinding.InvalidState
            });

            cycleChVM.PropertyChanged += (_, arg) =>
            {
                //if ((bool)cycleChHeaderCheckbox.IsChecked == true)
                if (arg.PropertyName == nameof(CycleCrosshairViewModel.CrosshairCount))
                    this.AddEntry(cycleCrosshairEntryKey, true);
            };


            // volume regulator
            const string volumeRegulatorEntryKey = "VolumeRegulator";

            VolumeRegulatorControllerViewModel volumeVM = new VolumeRegulatorControllerViewModel()
            {
                Content = Localize(volumeRegulatorEntryKey),
                Key = volumeRegulatorEntryKey
            };
            extraItemsControl.Items.Add(volumeVM);

            volumeVM.PropertyChanged += (_, arg) =>
            {
                string prop = arg.PropertyName;

                if (prop == nameof(VolumeRegulatorControllerViewModel.Mode))
                {
                    this.AddEntry(volumeRegulatorEntryKey, true);
                    return;
                }
                else if (prop == nameof(VolumeRegulatorControllerViewModel.From))
                {
                    volumeVM.ToMinimum = volumeVM.From + 0.01;
                }
                else if (prop == nameof(VolumeRegulatorControllerViewModel.To))
                {
                    volumeVM.FromMaximum = volumeVM.To - 0.01;
                }
                else if (prop == nameof(VolumeRegulatorControllerViewModel.Step))
                {
                    
                }

                // Определим дельту
                double delta = volumeVM.To - volumeVM.From;
                volumeVM.StepMaximum = delta;

                // Обновим регулировщик в конфиге, если изменение было сделано пользователем
                this.AddEntry(volumeRegulatorEntryKey, true);
            };

            this.entryV2Controllers.Add(new EntryControllerV2()
            {
                AttachedViewModel = volumeVM,
                Focus = () =>
                {
                    extraTabButton.IsChecked = true;
                    volumeVM.IsFocused = true;
                },
                Generate = () =>
                {
                    double minVolume = Math.Round(volumeVM.From, 2);
                    double maxVolume = Math.Round(volumeVM.To, 2);
                    double volumeStep = Math.Round(volumeVM.Step, 2);
                    volumeStep = volumeStep == 0 ? 0.01 : volumeStep;
                    bool volumeUp = volumeVM.Mode == 1;

                    // Определяем промежуточные значения от максимума к минимуму
                    List<double> volumeValues = new List<double>();

                    double currentValue = maxVolume;

                    while (currentValue >= minVolume)
                    {
                        volumeValues.Add(currentValue);
                        currentValue -= volumeStep;
                        string formatted = Executable.FormatNumber(currentValue, false);
                        Executable.TryParseDouble(formatted, out currentValue);
                    }
                    // Если минимальное значение не захватилось, то добавим его вручную
                    if (volumeValues.Last() != minVolume)
                        volumeValues.Add(minVolume);

                    // Теперь упорядочим по возрастанию
                    volumeValues.Reverse();

                    // Создаем цикл
                    string volumeUpCmd = "volume_up";
                    string volumeDownCmd = "volume_down";

                    SingleCmd[] iterationNames = volumeValues
                        .Select(v => new SingleCmd($"volume_{Executable.FormatNumber(v, false)}")).ToArray();

                    CommandCollection dependencies = new CommandCollection();

                    for (int i = 0; i < volumeValues.Count; i++)
                    {
                        double value = volumeValues[i];
                        string formattedValue = Executable.FormatNumber(value, false);

                        CommandCollection iterationCmds = new CommandCollection();

                        // Задаем звук на текущей итерации с комментарием в консоль
                        SingleCmd volumeCmd = new SingleCmd($"volume {formattedValue}");
                        iterationCmds.Add(volumeCmd);
                        iterationCmds.Add(new SingleCmd($"echo {volumeCmd.ToString()}"));

                        if (i == 0)
                        {
                            iterationCmds.Add(
                                new AliasCmd(volumeDownCmd, new SingleCmd("echo Volume: Min")));
                            iterationCmds.Add(
                                new AliasCmd(volumeUpCmd, iterationNames[i + 1]));
                        }
                        else if (i == volumeValues.Count - 1)
                        {
                            iterationCmds.Add(
                                new AliasCmd(volumeUpCmd, new SingleCmd("echo Volume: Max")));
                            iterationCmds.Add(
                                new AliasCmd(volumeDownCmd, iterationNames[i - 1]));
                        }
                        else
                        {
                            iterationCmds.Add(
                                new AliasCmd(volumeDownCmd, iterationNames[i - 1]));
                            iterationCmds.Add(
                                new AliasCmd(volumeUpCmd, iterationNames[i + 1]));
                        }

                        // Добавим зависимость
                        dependencies.Add(new AliasCmd(iterationNames[i].ToString(), iterationCmds));
                    }

                    // По умолчанию будет задано минимальное значение звука
                    dependencies.Add(iterationNames[0]);

                    return new ParametrizedEntry<double[]>()
                    {
                        PrimaryKey = volumeRegulatorEntryKey,
                        Cmd = volumeUp ? new SingleCmd(volumeUpCmd) : new SingleCmd(volumeDownCmd),
                        Type = EntryType.Semistatic,
                        IsMetaScript = false,
                        Dependencies = dependencies,
                        Arg = new double[] { minVolume, maxVolume, volumeStep }
                    };
                },
                UpdateUI = (entry) =>
                {
                    volumeVM.UpdateIsChecked(true);
                    double[] args = ((IParametrizedEntry<double[]>)entry).Arg;

                    volumeVM.From = args[0];
                    volumeVM.To = args[1];
                    volumeVM.Step = args[2];
                    volumeVM.Mode = entry.Cmd.ToString() == "volume_up" ? 1 : 0;
                },
                Restore = () =>
                {
                    volumeVM.UpdateIsChecked(false);
                    volumeVM.Mode = 0;
                },
                HandleState = (state) => volumeVM.IsEnabled =
                    state != EntryStateBinding.InvalidState && state != EntryStateBinding.Default
            });
        }

        public ICommand AddAliasCommand { get; private set; }
        public ICommand DeleteAliasCommand { get; private set; }

        void InitAliasController()
        {
            // Предикат для проверки валидности ввода
            Predicate<string> aliasValidator = (alias) => Executable.IsValidAliasName(alias) 
                && aliasSetVM.Items.All(i => i.Text.Trim().ToLower() != alias.Trim().ToLower());
            // Скармливаем предикат в конструктор
            aliasSetVM = new AliasControllerViewModel(aliasValidator)
            {
                Key = "ExtraAliasSet",
                IsSelectable = false
            };
            aliasContentControl.Content = aliasSetVM;

            this.AddAliasCommand = new DelegateCommand(() =>
            {
                AddAlias(aliasSetVM.Input, new List<Entry>());
            });

            this.DeleteAliasCommand = new DelegateCommand(() =>
            {
                this.ResetAttachmentPanels();

                int selectedIndex = aliasSetVM.GetFirstSelectedIndex();
                aliasSetVM.Items.RemoveAt(selectedIndex);

                if (aliasSetVM.Items.Count > 0)
                {
                    UpdateAttachmentPanels();

                    // Так же для удобства сделаем фокус на первом элементе панели, если такой есть
                    if (this.solidAttachmentsVM.Items.Count > 0)
                    {
                        string firstEntry = (string)(this.solidAttachmentsVM.Items[0].Tag);

                        this.GetController(firstEntry).Focus();
                    }

                    // Если в панели еще остались алиасы, то обновим конфиг
                    this.AddEntry(aliasSetVM.Key, false);
                }
                else
                {
                    // Если удалили последнюю кнопку, то удалим 
                    this.RemoveEntry(aliasSetVM.Key);
                    this.SetStateAndUpdateUI(EntryStateBinding.InvalidState);
                }
            });

            void AddAlias(string name, List<Entry> attachedEntries)
            {
                ItemViewModel item = new ItemViewModel()
                {
                    Text = name.Trim(),
                    Tag = new List<Entry>()
                };

                aliasSetVM.Items.Add(item);

                if (this.StateBinding == EntryStateBinding.InvalidState)
                    this.SetStateAndUpdateUI(EntryStateBinding.Alias);

                this.AddEntry(aliasSetVM.Key, false);
                UpdateAttachmentPanels();
            }

            this.entryV2Controllers.Add(new EntryControllerV2()
            {
                AttachedViewModel = aliasSetVM,
                Generate = () =>
                {
                    List<ParametrizedEntry<Entry[]>> aliases =
                    new List<ParametrizedEntry<Entry[]>>();

                    CommandCollection dependencies = new CommandCollection();

                    foreach (ItemViewModel aliaselement in aliasSetVM.Items)
                    {
                        string aliasName = aliaselement.Text.ToString();
                        List<Entry> attachedEntries = (List<Entry>)aliaselement.Tag;

                        // Выпишем все зависимости, которые есть для текущего элемента
                        foreach (Entry entry in attachedEntries)
                            foreach (Executable dependency in entry.Dependencies)
                                dependencies.Add(dependency);

                        ParametrizedEntry<Entry[]> aliasEntry = new ParametrizedEntry<Entry[]>()
                        {
                            PrimaryKey = "ExtraAlias",
                            Cmd = new SingleCmd(aliasName),
                            IsMetaScript = false,
                            Type = EntryType.Dynamic,
                            Arg = attachedEntries.ToArray()
                        };

                        AliasCmd alias = new AliasCmd(
                            aliaselement.Text,
                            attachedEntries.Select(e => e.Cmd));

                        aliases.Add(aliasEntry);
                        dependencies.Add(alias);
                    }

                    // сформируем итоговый элемент конфига
                    return new ParametrizedEntry<Entry[]>()
                    {
                        PrimaryKey = aliasSetVM.Key,
                        Cmd = null,
                        IsMetaScript = false,
                        Type = EntryType.Dynamic,
                        Arg = aliases.ToArray(),
                        Dependencies = dependencies
                    };
                },
                UpdateUI = (entry) =>
                {
                    ParametrizedEntry<Entry[]> extendedEntry = (ParametrizedEntry<Entry[]>)entry;

                    Entry[] aliases = extendedEntry.Arg;

                    foreach (Entry alias in aliases)
                    {
                        AddAlias(alias.Cmd.ToString(), (alias as ParametrizedEntry<Entry[]>).Arg.ToList());
                    }
                },
                Restore = () =>
                {
                    this.ResetAttachmentPanels();
                    aliasSetVM.Items.Clear();
                },
                HandleState = (state) => { }
            });
        }
        #endregion

        #region Framework
        void ColorizeKeyboard()
        {
            // сбрасываем цвета перед обновлением
            foreach (Button key in this.kb)
            {
                key.ClearValue(ButtonBase.BackgroundProperty);
                key.ClearValue(ButtonBase.ForegroundProperty);
            }

            SolidColorBrush keyInSequenceBackground = (SolidColorBrush)this.FindResource("SecondaryAccentBrush");
            SolidColorBrush keyInSequenceForeground = (SolidColorBrush)this.FindResource("SecondaryAccentForegroundBrush");
            
            SolidColorBrush firstKeyBackground = (SolidColorBrush)this.FindResource("PrimaryHueMidBrush");
            SolidColorBrush firstKeyForeground = (SolidColorBrush)this.FindResource("PrimaryHueMidForegroundBrush");

            SolidColorBrush secondKeyBackground = (SolidColorBrush)this.FindResource("PrimaryHueDarkBrush");
            SolidColorBrush secondKeyForeground = (SolidColorBrush)this.FindResource("PrimaryHueDarkForegroundBrush");

            // Все элементы конфига
            var allEntries = this.cfgManager.Entries;

            // Закрасим первым цветом все кнопки, которые 1-е в последовательности
            allEntries.ToList()
            .ForEach(pair =>
            {
                Button button = this.kb.GetButtonByName(pair.Key.Keys[0]);
                button.Background = firstKeyBackground;
                button.Foreground = firstKeyForeground;
            });

            // Если в текущей последовательности 1 кнопка - закрасим вторым цветом все кнопки, 
            // которые связаны с текущей и являются вторыми в последовательности
            if (currentKeySequence != null && this.currentKeySequence.Keys.Length == 1)
            {
                allEntries.Where(p => p.Key.Keys.Length == 2 && p.Key.Keys[0] == currentKeySequence[0]).ToList()
               .ForEach(pair =>
               {
                   Button button = this.kb.GetButtonByName(pair.Key.Keys[1]);
                   button.Background = secondKeyBackground;
                   button.Foreground = secondKeyForeground;
               });
            }

            // Теперь выделим акцентным цветом все кнопки в текущей последовательности
            if (currentKeySequence != null)
            {
                Button seqKey1 = this.kb.GetButtonByName(currentKeySequence[0]);
                seqKey1.Background = keyInSequenceBackground;
                seqKey1.Foreground = keyInSequenceForeground;
                if (currentKeySequence.Keys.Length == 2)
                {
                    Button seqKey2 = this.kb.GetButtonByName(currentKeySequence[1]);
                    seqKey2.Background = keyInSequenceBackground;
                    seqKey2.Foreground = keyInSequenceForeground;
                }
            }
        }

        string Localize(string primaryKey)
        {
            string result = Res.ResourceManager.GetString(primaryKey);
            return result ?? primaryKey;
        }

        string GeneratePrefix()
        {
            System.Text.StringBuilder prefixBuilder = new System.Text.StringBuilder();
            
            switch (this.StateBinding)
            {
                case EntryStateBinding.KeyDown:
                    {
                        prefixBuilder.Append($"{this.currentKeySequence.ToString()}_");
                        prefixBuilder.Append("down");
                        break;
                    }
                case EntryStateBinding.KeyUp:
                    {
                        prefixBuilder.Append($"{this.currentKeySequence.ToString()}_");
                        prefixBuilder.Append("up");
                        break;
                    }
                case EntryStateBinding.Default:
                    {
                        prefixBuilder.Append("default");
                        break;
                    }
                case EntryStateBinding.Alias:
                    {
                        string aliasName = (aliasSetVM.GetSelectedItem().Text.ToString());
                        prefixBuilder.Append($"{aliasName}");
                        break;
                    }
                default:
                    throw new Exception($"Попытка генерации префикса при состоянии {this.StateBinding}");
            }

            return prefixBuilder.ToString();
        }

        /// <summary>
        /// Обработчик нажатия чекбоксов для добавления/удаления элементов конфига
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void HandleEntryClick(object sender, RoutedEventArgs args)
        {
            CheckBox checkbox = (CheckBox)sender;
            string entryKey = (string)checkbox.Tag;

            this.HandleEntryClick(entryKey);
        }

        void HandleEntryClick(string entryKey)
        {
            // Получим обработчика и 
            EntryControllerV2 entryController = this.GetController(entryKey);
            EntryViewModel entryVM = entryController.AttachedViewModel;
            Entry entry = (Entry)entryController.Generate();

            if (entryVM.IsChecked)
                this.AddEntry(entry);
            else
                this.RemoveEntry(entry);

            // Обновим панели
            this.UpdateAttachmentPanels();
        }

        void SetStateAndUpdateUI(EntryStateBinding newState)
        {
            this.StateBinding = newState;

            foreach (var controller in this.entryV2Controllers)
                controller.HandleState(this.StateBinding);

            keyDownAttachmentsVM.IsSelected = newState == EntryStateBinding.KeyDown;
            keyUpAttachmentsVM.IsSelected = newState == EntryStateBinding.KeyUp;
            solidAttachmentsVM.IsSelected = newState != EntryStateBinding.InvalidState;
        }

        BindEntry ConvertToBindEntry(Entry entry)
        {
            BindEntry bindEntry = (BindEntry)entry;

            Executable onKeyDown, onKeyUp;

            if (entry.IsMetaScript)
            {
                onKeyDown = new SingleCmd($"+{entry.Cmd}");
                onKeyUp = new SingleCmd($"-{entry.Cmd}");
            }
            else
            {
                bool isKeyDownBinding = this.StateBinding == EntryStateBinding.KeyDown;

                onKeyDown = isKeyDownBinding ? entry.Cmd : null;
                onKeyUp = isKeyDownBinding ? null : entry.Cmd;
            }

            bindEntry.OnKeyDown = onKeyDown;
            bindEntry.OnKeyRelease = onKeyUp;

            return bindEntry;
        }

        void AddEntry(string cfgEntryKey, bool abortIfNotUser)
        {
            EntryControllerV2 controller = this.GetController(cfgEntryKey);

            // Если сказано, что отмена, если добавление идет не из-за действий пользователя
            // То значит гарантированно AttachedCheckbox не может быть равен null
            if (abortIfNotUser && !controller.AttachedViewModel.IsChecked) return;

            Entry generatedEntry = (Entry)controller.Generate();
            this.AddEntry(generatedEntry);
        }

        void RemoveEntry(string cfgEntryKey)
        {
            Entry generatedEntry = (Entry)this.GetController(cfgEntryKey).Generate();
            this.RemoveEntry(generatedEntry);
        }

        void AddEntry(Entry entry)
        {
            switch (this.StateBinding)
            {
                case EntryStateBinding.KeyDown:
                case EntryStateBinding.KeyUp:
                    {
                        BindEntry bindEntry = this.ConvertToBindEntry(entry);
                        this.cfgManager.AddEntry(this.currentKeySequence, bindEntry);
                        break;
                    }
                case EntryStateBinding.Default:
                    {
                        this.cfgManager.AddEntry(entry);
                        break;
                    }
                case EntryStateBinding.Alias:
                    {
                        // Проверяем, что добавляется не основной узел со всеми алиасами
                        if (entry.PrimaryKey != aliasSetVM.Key)
                        {
                            // Добавляем текущий элемент к коллекции, привязанной к выбранной кнопке
                            ItemViewModel selectedItem = aliasSetVM.GetSelectedItem();
                            List<Entry> attachedToAlias = (List<Entry>)(selectedItem.Tag);

                            attachedToAlias.Add(entry);
                            selectedItem.Tag = attachedToAlias;

                            // И вызываем обработчика пользовательских алиасов
                            Entry aliasSetEntry = (Entry)this.GetController(aliasSetVM.Key).Generate();
                            this.cfgManager.AddEntry(aliasSetEntry);
                        }
                        else
                        {
                            // Если это основной узел, то добавим его напрямую в конфиг
                            this.cfgManager.AddEntry(entry);
                        }
                        break;
                    }
                default: throw new Exception($"Состояние {this.StateBinding} при добавлении элемента");
            }
        }

        void RemoveEntry(Entry entry)
        {
            switch (this.StateBinding)
            {
                case EntryStateBinding.KeyDown:
                case EntryStateBinding.KeyUp:
                    {
                        BindEntry bindEntry = this.ConvertToBindEntry(entry);
                        this.cfgManager.RemoveEntry(this.currentKeySequence, bindEntry);
                        break;
                    }
                case EntryStateBinding.Default:
                    {
                        this.cfgManager.RemoveEntry(entry);
                        break;
                    }
                case EntryStateBinding.Alias:
                    {
                        if (entry.PrimaryKey != aliasSetVM.Key)
                        {
                            // Добавляем текущий элемент к коллекции, привязанной к выбранной кнопке
                            ItemViewModel selectedItem = aliasSetVM.GetSelectedItem();
                            List<Entry> attachedToAlias = ((List<Entry>)selectedItem.Tag)
                                .Where(e => e.PrimaryKey != entry.PrimaryKey).ToList();
                            
                            selectedItem.Tag = attachedToAlias;

                            // Напрямую обновим узел в менеджере
                            Entry aliasSetEntry = (Entry)this.GetController(aliasSetVM.Key).Generate();
                            this.cfgManager.AddEntry(aliasSetEntry);
                        }
                        else
                        {
                            // Если удаляем основной узел со всеми алиасами, то напрямую стираем его из менеджера
                            this.cfgManager.RemoveEntry(entry);
                        }
                        break;
                    }
                default: throw new Exception($"Состояние {this.StateBinding} при попытке удалить элемент");

            }
        }

        EntryControllerV2 GetController(string cmd)
        {
            return this.entryV2Controllers.FirstOrDefault(c => c.AttachedViewModel.Key == cmd);
        }

        void ResetAttachmentPanels()
        {
            // Получим предыдущие элементы и сбросим связанные с ними элементы интерфейса
            // Для этого объединим коллекции элементов из всех панелей
            List<AttachmentsViewModel> attachmentVMs = new List<AttachmentsViewModel>()
            {
                this.keyDownAttachmentsVM,
                this.keyUpAttachmentsVM,
                this.solidAttachmentsVM
            };

            IEnumerable<string> shownEntryKeys = attachmentVMs.SelectMany(vm => vm.Items.Select(i => i.Tag)).Cast<string>();

            foreach (string entryKey in shownEntryKeys)
            {
                EntryControllerV2 controller = this.GetController(entryKey);
                // Метод, отвечающий непосредственно за сброс состояния интерфейса
                controller.Restore();
            }

            // Очистим панели
            attachmentVMs.ForEach(vm => vm.Items.Clear());
        }

        /// <summary>
        /// Метод для обновления панелей с привязанными к сочетанию клавиш элементами конфига
        /// </summary>
        void UpdateAttachmentPanels()
        {
            // Очистим панели и сбросим настройки интерфейса
            ResetAttachmentPanels();
            
            // Локальный метод для добавления нового элемента в заданную панель
            void AddAttachment(string entryKey, AttachmentsViewModel attachmentsVM)
            {
                ItemViewModel item = new ItemViewModel()
                {
                    Text = Localize(entryKey),
                    Tag = entryKey
                };
                attachmentsVM.Items.Add(item);
            }

            // Согласно текущему состоянию выберем элементы
            if (this.StateBinding == EntryStateBinding.KeyDown || this.StateBinding == EntryStateBinding.KeyUp)
            {
                if (!this.cfgManager.Entries.TryGetValue(this.currentKeySequence, out List<BindEntry> attachedEntries)) return;

                /*
                 * Мета-скрипты привязываются только к нажатию кнопок (хотя отжатие обработано тоже)
                 * В других случаях может быть обработано либо нажатие, либо отжатие кнопки
                 */

                // Теперь заполним панели новыми элементами
                attachedEntries.ForEach(entry =>
                {
                    // Добавим в нужную панель
                    if (entry.OnKeyDown != null)
                        AddAttachment(entry.PrimaryKey, keyDownAttachmentsVM);
                    else
                        AddAttachment(entry.PrimaryKey, keyUpAttachmentsVM);

                    // Обновим интерфейс согласно элементам, привязанным к текущему состоянию
                    attachedEntries.Where(e => this.IsEntryAttachedToCurrentState(e))
                        .ToList().ForEach(e => this.GetController(e.PrimaryKey).UpdateUI(e));
                });
            }
            else if (this.StateBinding == EntryStateBinding.Default)
            {
                // Получаем все элементы по умолчанию, которые должны быть отображены в панели
                List<Entry> attachedEntries = this.cfgManager.DefaultEntries
                    .Where(e => this.GetController(e.PrimaryKey).AttachedViewModel.IsSelectable).ToList();

                // Теперь заполним панели новыми элементами
                attachedEntries.ForEach(entry =>
                {
                    AddAttachment(entry.PrimaryKey, solidAttachmentsVM);
                    // Обновим интерфейс согласно элементам, привязанным к текущему состоянию
                    this.GetController(entry.PrimaryKey).UpdateUI(entry);
                });
            }
            else if (this.StateBinding == EntryStateBinding.Alias)
            {
                int selectedIndex = this.aliasSetVM.GetFirstSelectedIndex();

                if (selectedIndex == -1) return;

                // Узнаем какие элементы привязаны к текущей команде
                List<Entry> attachedEntries = (List<Entry>)(this.aliasSetVM.Items[selectedIndex].Tag);

                attachedEntries.ForEach(entry =>
                {
                    AddAttachment(entry.PrimaryKey, solidAttachmentsVM);
                    // Обновим интерфейс согласно элементам, привязанным к текущему состоянию
                    this.GetController(entry.PrimaryKey).UpdateUI(entry);
                });
            }
            else { } // InvalidState
        }
        
        /// <summary>
        /// Обработчик нажатия на привязанный элемент из конфига
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        void HandleAttachedEntryClick(object obj, RoutedEventArgs args)
        {
            // Сбросим поиск, чтобы вывелись все элементы
            searchCmdBox.Text = string.Empty;

            // Узнаем за привязку к какому типу отвечает нажатая кнопка
            FrameworkElement element = (FrameworkElement)obj;

            string entryKey = (string) element.Tag;

            // Получим обработчика и переведем фокус на нужный элемент
            this.GetController(entryKey).Focus();
        }
        
        private void GenerateConfig(object sender, RoutedEventArgs e)
        {
            if (this.CfgName.Length == 0)
                this.CfgName = (string)CfgNameProperty.DefaultMetadata.DefaultValue;

            string resultCfgPath = Path.Combine(GetTargetFolder(), $"{this.CfgName}.cfg");

            this.cfgManager.GenerateCfg(resultCfgPath);
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{resultCfgPath}\"");
        }

        bool IsEntryAttachedToCurrentState(BindEntry entry)
        {
            if (this.StateBinding == EntryStateBinding.Default
                || this.StateBinding == EntryStateBinding.Alias
                || this.StateBinding == EntryStateBinding.InvalidState)
                throw new Exception(); // TODO: УБРАТЬ

            // Если сбрасываем привязанные к нажатию элементы, и у элемента определено действие на нажатие, то подходит
            if (this.StateBinding == EntryStateBinding.KeyDown && entry.OnKeyDown != null)
                return true;
            // Если привязанные к отжатию и у нас обрабатывается только отжатие, то подходит
            else if (this.StateBinding == EntryStateBinding.KeyUp && entry.OnKeyRelease != null && entry.OnKeyDown == null)
                return true;
            else
                return false;
        }

        void GenerateRandomCrosshairs(int count)
        {
            string prefix = GeneratePrefix();
            Random rnd = new Random();

            string GenerateRandomCmd(string cmd, double from, double to, bool asInteger)
            {
                double randomValue;

                if (!asInteger)
                    randomValue = from + rnd.NextDouble() * (to - from);
                else
                    randomValue = from + rnd.NextDouble() * (to - from + 1);

                string formatted = Executable.FormatNumber(randomValue, asInteger);
                return $"{cmd} {formatted}";
            };

            string firstCrosshair = null;

            for (int i = 0; i < count; i++)
            {
                string crosshairPath = $@"{GetTargetFolder()}\{prefix}_ch{i + 1}.cfg";
                if (firstCrosshair == null) firstCrosshair = crosshairPath;

                string[] lines = new string[]
                {
                    GenerateRandomCmd("cl_crosshair_drawoutline", 0, 1, true),
                    GenerateRandomCmd("cl_crosshair_outlinethickness", 1, 2, true),
                    GenerateRandomCmd("cl_crosshair_sniper_width", 1, 5, true),
                    GenerateRandomCmd("cl_crosshairalpha", 200, 255, true),
                    GenerateRandomCmd("cl_crosshairdot", 0, 1, true),
                    GenerateRandomCmd("cl_crosshairgap", 0, 1, true),
                    GenerateRandomCmd("cl_crosshair_t", 0, 1, true),
                    GenerateRandomCmd("cl_crosshairsize", -5, 5, true),
                    GenerateRandomCmd("cl_crosshairstyle", 0, 5, true),
                    GenerateRandomCmd("cl_crosshairthickness", 1, 3, true)
                };

                if (File.Exists(crosshairPath)) File.Delete(crosshairPath);
                File.WriteAllLines(crosshairPath, lines);
            }
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{firstCrosshair}\"");
        }

        void UpdateCfgManager()
        {
            // Сбросим все настройки от прошлого конфига
            foreach (EntryControllerV2 controller in this.entryV2Controllers)
                controller.Restore();

            // Зададим привязку к дефолтному состоянию
            keyboardAliasCombobox.SelectedIndex = 0;
            //this.StateBinding = EntryStateBinding.Default;

            foreach (Entry entry in this.cfgManager.DefaultEntries)
                this.GetController(entry.PrimaryKey).UpdateUI(entry);

            this.UpdateAttachmentPanels();
            this.ColorizeKeyboard();
        }

        string GetTargetFolder()
        {
            string cfgPath = this.CsgoCfgPath;

            if (cfgPath.Length > 0 && Directory.Exists(cfgPath))
            {
                return cfgPath;
            }
            else
            {
                return Directory.GetCurrentDirectory();
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
        #endregion
    }
}
