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
        const string extraAliasSetEntryKey = "ExtraAliasSet";

        Dictionary<string, EntryController> entryControllers = new Dictionary<string, EntryController>();

        public EntryStateBinding StateBinding
        {
            get => (EntryStateBinding) GetValue(StateBindingProperty);
            set
            {
                SetValue(StateBindingProperty, value);
                // Т.к. привязка задается только в коде, то тут же обновим интерфейс. TODO: Remove
                this.entryControllers.Values.ToList()
                    .ForEach(entry => entry.HandleState(this.StateBinding));
            }   
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

            InitActionTab();
            InitBuyTab();
            InitGameSettingsTab();
            InitAliasController();
            InitExtra();

            // Проверим, что в словаре нет одинаковых ключей в разном регистре
            // Для этого сгруппируем все ключи, переведя их в нижний регистр,
            // и найдем группу, где больше 1 элемента
            var duplicatedKeyGroup = this.entryControllers.GroupBy(pair => pair.Key.ToLower())
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatedKeyGroup != null) throw new Exception($"Duplicate key: {duplicatedKeyGroup.Key}");

            // Зададим привязку по умолчанию
            this.StateBinding = EntryStateBinding.Default;
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
                this.StateBinding = EntryStateBinding.KeyDown;

                // Отредактируем текст у панелей
                this.keyDownPanelLabel.Text = string.Format(Res.KeyDown1_Format, currentKeySequence[0].ToUpper());
                this.keyReleasePanelLabel.Text = string.Format(Res.KeyUp1_Format, currentKeySequence[0].ToUpper());
            }
            else if (currentKeySequence.Keys.Length == 1)
            {
                // Иначе в последовательности уже есть 1 кнопка и надо добавить вторую
                // Проверяем, что выбрана не та же кнопка
                if (currentKeySequence[0] == key) return;

                currentKeySequence = new KeySequence(currentKeySequence[0], key);
                this.StateBinding = EntryStateBinding.KeyDown;

                string key1Upper = currentKeySequence[0].ToUpper();
                string key2Upper = currentKeySequence[1].ToUpper();

                this.keyDownPanelLabel.Text = string.Format(Res.KeyDown2_Format, key2Upper, key1Upper);
                this.keyReleasePanelLabel.Text = string.Format(Res.KeyUp2_Format, key2Upper, key1Upper);
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

            Border border = (Border)sender;
            bool isKeyDownBinding = ((FrameworkElement)sender).Tag as string == EntryStateBinding.KeyDown.ToString();

            this.StateBinding = isKeyDownBinding ? EntryStateBinding.KeyDown : EntryStateBinding.KeyUp;

            UpdateAttachmentPanels();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)e.Source;
            ComboBoxItem selectedItem = (ComboBoxItem)cbox.SelectedItem;

            if (selectedItem.Name == iKeyboard.Name)
            {
                // При выборе клавиатуры по умолчанию не выбрана последовательность
                this.StateBinding = EntryStateBinding.InvalidState;
            }
            else
            {
                EntryStateBinding selectedState = selectedItem.Name == iDefault.Name ?
                    EntryStateBinding.Default :
                    EntryStateBinding.Alias;

                solidAttachmentPanelLabel.Text = selectedState == EntryStateBinding.Default ?
                    Res.CommandsByDefault_Hint :
                    Res.CommandsInAlias_Hint;

                this.currentKeySequence = null;
                this.ColorizeKeyboard();

                if (selectedState == EntryStateBinding.Alias)
                {
                    // И при этом если ни одной команды не создано, то задаем неверное состояние
                    selectedState =
                        aliasPanel.Tag == null ?
                        EntryStateBinding.InvalidState :
                        EntryStateBinding.Alias;
                }

                this.StateBinding = selectedState;
            }

            UpdateAttachmentPanels();
        }

        private void AddAliasButton_Click(object sender, RoutedEventArgs e)
        {
            string aliasName = this.newAliasNameTextbox.Text;
            AddAliasButton(aliasName, new List<Entry>());
            // Пусть в конфиге всё равно будет объявлен алиас, хоть он и пустой
            this.AddEntry(extraAliasSetEntryKey, false);
        }

        private void DeleteAliasButton_Click(object sender, RoutedEventArgs e)
        {
            this.ResetAttachmentPanels();

            FrameworkElement targetElement = aliasPanel.Tag as FrameworkElement;
            // Отвяжем настройки, привязанные к текущей кнопке
            targetElement.Tag = null;
            // Уберем привязку к тегу панели алиасов
            BindingOperations.ClearAllBindings(targetElement);
            aliasPanel.Children.Remove(targetElement);

            if (aliasPanel.Children.Count > 0)
            {
                // Если в панели еще есть элементы, то задаем новый тег
                // и обновляем панели под новый алиас
                aliasPanel.Tag = aliasPanel.Children[0];
                UpdateAttachmentPanels();

                // Так же для удобства сделаем фокус на первом элементе панели, если такой есть
                if (this.solidAttachmentsPanel.Children.Count > 0)
                {
                    string firstEntry =
                    (string)(this.solidAttachmentsPanel.Children[0] as FrameworkElement).Tag;

                    this.entryControllers[firstEntry].Focus();
                }

                // Если в панели еще остались алиасы, то обновим конфиг
                this.AddEntry(extraAliasSetEntryKey, false);
            }
            else
            {
                // Если удалили последнюю кнопку, то удалим 
                this.RemoveEntry(extraAliasSetEntryKey);
                aliasPanel.Tag = null;
                this.StateBinding = EntryStateBinding.InvalidState;
            }
        }

        private void CommandNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            string text = box.Text.Trim();

            // Отключаем кнопку добавления новой команды
            addCmdButton.IsEnabled = false;

            // И включаем только если задана новая команда и её до этого не было
            // TODO: Проверка имени регуляркой
            if (text.Length == 0)
                return;
            else if (customCmdPanel.Children.OfType<ButtonBase>().Any(b => b.Content.ToString() == text))
                return;

            addCmdButton.IsEnabled = true;
        }
        
        private void GenerateRandomCrosshairsButton_Click(object sender, RoutedEventArgs e)
        {
            int count = (int)cycleChSlider.Value;
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
        
        private void ResetSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            this.currentKeySequence = null;
            this.StateBinding = EntryStateBinding.Default;

            this.ColorizeKeyboard();
            this.UpdateAttachmentPanels();
        }

        private void SearchCmdTextbox(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            string input = textbox.Text.Trim().ToLower();

            UIElementCollection elements = settingsTabPanel.Children;

            // Выводим все элементы, если ничего не ищем
            if (input.Length == 0)
            {
                foreach (FrameworkElement element in elements)
                    element.Visibility = Visibility.Visible;
                addUnknownCmdButton.Visibility = Visibility.Hidden;
            }
            else
            {
                int foundCount = 0;

                foreach (FrameworkElement element in elements)
                {
                    if (element.Tag != null)
                    {
                        string entryKey = (string)element.Tag;
                        if (entryKey.ToLower().Contains(input))
                        {
                            element.Visibility = Visibility.Visible;
                            foundCount++;
                        }
                        else
                        {
                            element.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                        element.Visibility = Visibility.Collapsed;
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
        #endregion

        #region Filling UI with config entry managers
        void InitActionTab()
        {
            //// Локальный метод для подготовки и настройки нового чекбокса-контроллера
            CheckBox PrepareAction(string cmd, bool isMeta)
            {
                CheckBox checkbox = new CheckBox
                {
                    Content = Localize(cmd),
                    Tag = cmd
                };

                // переводим команду в нижний регистр для удобства восприятия
                cmd = cmd.ToLower();

                string tooltip = isMeta ? $"+{cmd}" : $"{cmd}";
                TextBlock tooltipBlock = new TextBlock();
                tooltipBlock.Inlines.Add(tooltip);
                checkbox.ToolTip = tooltipBlock;

                checkbox.Click += HandleEntryClick;
                actionsPanel.Children.Add(checkbox);
                return checkbox;
            }

            // Локальный метод для добавления действий
            void AddAction(string cmd, bool isMeta)
            {
                CheckBox checkbox = PrepareAction(cmd, isMeta);

                EntryController entryUiBinding = new EntryController
                {
                    AttachedCheckbox = checkbox,
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
                    UpdateUI = (entry) => checkbox.IsChecked = true,
                    Focus = () =>
                    {
                        actionTabButton.IsChecked = true;
                        checkbox.Focus();
                    },
                    Restore = () => checkbox.IsChecked = false,
                    HandleState = (state) =>
                    {
                        checkbox.IsEnabled =
                            state != EntryStateBinding.InvalidState
                            && (!isMeta || state == EntryStateBinding.KeyDown);
                    }                    
                };
                entryControllers.Add(cmd, entryUiBinding);
            };

            // Метод для добавления новой категории.
            void AddActionGroupSeparator(string text)
            {
                TextBlock block = new TextBlock();
                Inline bold = new Bold(new Run(text));
                block.Inlines.Add(bold);

                Border border = new Border
                {
                    Child = block,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                actionsPanel.Children.Add(border);
            };

            AddActionGroupSeparator(Res.CategoryCommonActions);
            AddAction("attack", true);
            AddAction("attack2", true);
            AddAction("reload", true);
            AddAction("drop", false);
            AddAction("use", true);
            AddAction("showscores", true);

            AddActionGroupSeparator(Res.CategoryMovement);
            AddAction("forward", true);
            AddAction("back", true);
            AddAction("moveleft", true);
            AddAction("moveright", true);
            AddAction("jump", true);
            AddAction("duck", true);
            AddAction("speed", true);

            AddActionGroupSeparator(Res.CategoryEquipment);
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

            AddActionGroupSeparator(Res.CategoryBuy);
            AddAction("buymenu", false);
            AddAction("autobuy", false);
            AddAction("rebuy", false);
            
            AddActionGroupSeparator(Res.CategoryCommunication);
            AddAction("voicerecord", true);
            AddAction("radio1", false);
            AddAction("radio2", false);
            AddAction("radio3", false);
            AddAction("messagemode2", false);
            AddAction("messagemode", false);

            AddActionGroupSeparator(Res.CategoryWarmup);
            AddAction("god", false);
            AddAction("noclip", false);
            AddAction("impulse 101", false);
            AddAction("mp_warmup_start", false);
            AddAction("mp_warmup_end", false);
            AddAction("mp_swapteams", false);
            AddAction("bot_add_t", false);
            AddAction("bot_add_ct", false);
            AddAction("bot_place", false);

            AddActionGroupSeparator(Res.CategoryOther);
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
            AddAction("quit", false);

            AddActionGroupSeparator(Res.CategoryDemo);
            AddAction("demo_resume", false);
            AddAction("demo_togglepause", false);

            AddActionGroupSeparator(Res.CategoryBonusScripts);

            // jumpthrow script
            const string jumpthrowEntryKey = "Jumpthrow";
            CheckBox jumpthrowCheckbox = PrepareAction(jumpthrowEntryKey, true);

            EntryController jumpthrowBinding = new EntryController()
            {
                AttachedCheckbox = jumpthrowCheckbox,
                Focus = () =>
                {
                    actionTabButton.IsChecked = true;
                    jumpthrowCheckbox.Focus();
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
                UpdateUI = (entry) => jumpthrowCheckbox.IsChecked = true,
                Restore = () => jumpthrowCheckbox.IsChecked = false,
                HandleState = (state) => jumpthrowCheckbox.IsEnabled = state == EntryStateBinding.KeyDown
            };
            this.entryControllers.Add(jumpthrowEntryKey, jumpthrowBinding);

            // DisplayDamageOn
            const string displayDamageOnEntryKey = "DisplayDamage_On";
            CheckBox displayDamageOnCheckbox = PrepareAction(displayDamageOnEntryKey, false);

            this.entryControllers.Add(displayDamageOnEntryKey, new EntryController()
                {
                    AttachedCheckbox = displayDamageOnCheckbox,
                    Focus = () =>
                    {
                        actionTabButton.IsChecked = true;
                        displayDamageOnCheckbox.Focus();
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
                    UpdateUI = (entry) => displayDamageOnCheckbox.IsChecked = true,
                    Restore = () => displayDamageOnCheckbox.IsChecked = false,
                    HandleState = (state) => displayDamageOnCheckbox.IsEnabled = state != EntryStateBinding.InvalidState
                });

            // DisplayDamageOff
            const string displayDamageOffEntryKey = "DisplayDamage_Off";
            CheckBox displayDamageOffCheckbox = PrepareAction(displayDamageOffEntryKey, false);

            this.entryControllers.Add(displayDamageOffEntryKey, new EntryController()
                {
                    AttachedCheckbox = displayDamageOffCheckbox,
                    Focus = () =>
                    {
                        actionTabButton.IsChecked = true;
                        displayDamageOffCheckbox.Focus();
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
                    UpdateUI = (entry) => displayDamageOffCheckbox.IsChecked = true,
                    Restore = () => displayDamageOffCheckbox.IsChecked = false,
                    HandleState = (state) => displayDamageOffCheckbox.IsEnabled = state != EntryStateBinding.InvalidState
                });
        }

        void InitBuyTab()
        {
            const string buyScenarioEntryKey = "BuyScenario";

            // Добавляем главный чекбокс
            CheckBox mainCheckbox = new CheckBox
            {
                Content = Localize(buyScenarioEntryKey),
                Tag = buyScenarioEntryKey
            };
            mainCheckbox.Click += HandleEntryClick;
            buyTabStackPanel.Children.Add(mainCheckbox);

            // Панель на которой будут располагаться все элементы для закупки
            WrapPanel buyPanel = new WrapPanel();
            buyTabStackPanel.Children.Add(buyPanel);

            // Свяжем свойство активности с чекбоксом
            Binding enabledBinding = new Binding("IsChecked")
            {
                Source = mainCheckbox
            };
            buyPanel.SetBinding(WrapPanel.IsEnabledProperty, enabledBinding);


            // Локальный метод для получения всех чекбоксов с оружием
            List<CheckBox> GetWeaponCheckboxes()
            {
                return buyPanel.Children.OfType<StackPanel>()
                .SelectMany(s => s.Children.OfType<CheckBox>()).ToList();
            };

            // Обработчик интерфейса настроек закупки
            EntryController buyEntryBinding = new EntryController()
            {
                AttachedCheckbox = mainCheckbox,
                Focus = () => buyTabButton.IsChecked = true,
                UpdateUI = (entry) =>
                {
                    IParametrizedEntry<string[]> extendedEntry = (IParametrizedEntry<string[]>)entry;
                    string[] weapons = extendedEntry.Arg;

                    // зададим состояние чекбоксов согласно аргументам
                    GetWeaponCheckboxes().ForEach(c => c.IsChecked = weapons.Contains(((string)c.Tag)));
                    // не забываем про главный чекбокс
                    mainCheckbox.IsChecked = true;
                },
                Generate = () =>
                {
                    string[] weaponsToBuy = GetWeaponCheckboxes()
                    .Where(c => (bool)c.IsChecked).Select(c => (string)c.Tag).ToArray();

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
                    mainCheckbox.IsChecked = false;
                    GetWeaponCheckboxes().ForEach(c => c.IsChecked = false);
                },
                HandleState = (state) => mainCheckbox.IsEnabled = state != EntryStateBinding.InvalidState
            };
            // Добавляем обработчика
            this.entryControllers.Add(buyScenarioEntryKey, buyEntryBinding);

            StackPanel currentPanel = null;

            void AddWeapon(string weaponId, string localizedName)
            {
                CheckBox weaponCheckbox = new CheckBox
                {
                    Content = localizedName,
                    Tag = weaponId
                };

                // При нажатии на чекбокс оружия искусственно вызовем событие обработки нажатия на главный чекбокс
                weaponCheckbox.Click += (_, __) =>
                {
                    this.AddEntry(buyScenarioEntryKey, false);
                };

                currentPanel.Children.Add(weaponCheckbox);
            };

            // Метод для добавления новой категории. Определяет новый stackpanel и создает текстовую метку
            void AddGroupSeparator(string text)
            {
                currentPanel = new StackPanel();
                buyPanel.Children.Add(currentPanel);

                TextBlock block = new TextBlock();
                Inline bold = new Bold(new Run(text));
                block.Inlines.Add(bold);

                Border border = new Border
                {
                    Child = block,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                currentPanel.Children.Add(border);
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
            double rowHeight = 30;

            Tuple<TextBlock, Grid, Button, CheckBox> PrepareNewRow(string cmd, bool needToggle)
            {
                Grid rowGrid = new Grid
                {
                    Height = rowHeight
                };

                rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) });
                rowGrid.Tag = cmd;

                // Текст с результирующей командой
                TextBlock resultCmd = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                rowGrid.Children.Add(resultCmd);
                Grid.SetColumn(resultCmd, 0);

                // Сетка с управляющими элементами
                Grid controlsAndToggleButtonGrid = new Grid();
                rowGrid.Children.Add(controlsAndToggleButtonGrid);
                Grid.SetColumn(controlsAndToggleButtonGrid, 1);

                Grid controlsGrid = new Grid();
                controlsAndToggleButtonGrid.Children.Add(controlsGrid);

                // Определяем нужна ли кнопка для циклических аргументов
                Button toggleButton = null;

                if (needToggle)
                {
                    controlsAndToggleButtonGrid.ColumnDefinitions.Add(
                        new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    controlsAndToggleButtonGrid.ColumnDefinitions.Add(
                        new ColumnDefinition() { Width = new GridLength(-1, GridUnitType.Auto) });

                    toggleButton = new Button
                    {
                        Style = (Style)this.FindResource("MaterialDesignFlatButton"),
                        Content = "⇄"
                    };

                    controlsAndToggleButtonGrid.Children.Add(toggleButton);
                    Grid.SetColumn(toggleButton, 1);
                }

                // Колонка с чекбоксом
                CheckBox checkbox = new CheckBox
                {
                    Tag = cmd,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                rowGrid.Children.Add(checkbox);
                Grid.SetColumn(checkbox, 2);
                checkbox.Click += HandleEntryClick;

                // Привяжем доступность регулирующих элементов к значению чекбокса
                Binding checkedBinding = new Binding("IsChecked")
                {
                    Source = checkbox
                };
                controlsAndToggleButtonGrid.SetBinding(Grid.IsEnabledProperty, checkedBinding);
                // А так же привяжем отдельно наш ToggleTool, т.к. он находится в общем гриде

                settingsTabPanel.Children.Add(rowGrid);

                return new Tuple<TextBlock, Grid, Button, CheckBox>(resultCmd, controlsGrid, toggleButton, checkbox);
            };

            
            void AddIntervalCmdController(string cmd, double from, double to, double step, double defaultValue)
            {
                var tuple = PrepareNewRow(cmd, true);
                TextBlock resultCmdBlock = tuple.Item1;
                Grid sliderGrid = tuple.Item2;
                Button toggleButton = tuple.Item3;
                CheckBox checkbox = tuple.Item4;

                bool isInteger = from % 1 == 0 && to % 1 == 0 && step % 1 == 0;

                // Колонка с ползунком
                sliderGrid.ColumnDefinitions.Add(new ColumnDefinition());
                sliderGrid.ColumnDefinitions.Add(new ColumnDefinition());
                sliderGrid.ColumnDefinitions.Add(new ColumnDefinition());
                sliderGrid.ColumnDefinitions[0].Width = new GridLength(-1, GridUnitType.Auto);
                sliderGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                sliderGrid.ColumnDefinitions[2].Width = new GridLength(-1, GridUnitType.Auto);

                sliderGrid.MaxHeight = rowHeight;
                Slider slider = new Slider
                {
                    Margin = new Thickness(3, 0, 3, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Minimum = from,
                    Maximum = to
                };

                sliderGrid.Children.Add(slider);
                Grid.SetColumn(slider, 1);

                Border minBorder = new Border();
                //minBorder.Width = 20;
                TextBlock minText = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                //minText.FontSize = 12;
                minText.Inlines.Add(Executable.FormatNumber(from, from % 1 == 0));
                minBorder.Child = minText;
                sliderGrid.Children.Add(minBorder);

                Border maxBorder = new Border();
                //maxBorder.Width = 20;
                TextBlock maxText = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                //maxText.FontSize = 11;
                maxText.Inlines.Add(Executable.FormatNumber(to, to % 1 == 0));
                maxBorder.Child = maxText;
                sliderGrid.Children.Add(maxBorder);
                Grid.SetColumn(maxBorder, 2);

                toggleButton.Click += (_, __) =>
                {
                    ToggleWindow toggleWindow = new ToggleWindow(isInteger, from, to);
                    if ((bool)toggleWindow.ShowDialog())
                    {
                        double[] values = toggleWindow.GeneratedArg.Split(' ').Select(value =>
                        {
                            Executable.TryParseDouble(value, out double parsedValue);
                            return parsedValue;
                        }).ToArray();

                        resultCmdBlock.Text = Executable.GenerateToggleCmd(cmd, values, isInteger).ToString();
                        // Сохраним аргумент в теге
                        resultCmdBlock.Tag = values;

                        //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                        this.AddEntry(cmd, true);
                    }
                    else
                    {

                    }
                };

                slider.IsSnapToTickEnabled = true;
                slider.TickFrequency = step;

                slider.ValueChanged += (obj, args) =>
                {
                    double value = args.NewValue;
                    string formatted = Executable.FormatNumber(args.NewValue, isInteger);
                    Executable.TryParseDouble(formatted, out double fixedValue);
                    fixedValue = isInteger ? ((int)fixedValue) : fixedValue;

                    resultCmdBlock.Text = new SingleCmd($"{cmd} {formatted}").ToString();
                    resultCmdBlock.Tag = fixedValue;

                    //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                    this.AddEntry(cmd, true);
                };

                // обработчик интерфейса
                EntryController entryBinding = new EntryController()
                {
                    AttachedCheckbox = checkbox,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        checkbox.Focus();
                    },
                    Restore = () =>
                    {
                        // Сперва сбрасываем чекбокс, это важно
                        checkbox.IsChecked = false;
                        slider.Value = defaultValue;
                        resultCmdBlock.Tag = defaultValue;
                    },
                    Generate = () =>
                    {
                        if (resultCmdBlock.Tag is double)
                        {
                            return new ParametrizedEntry<double>()
                            {
                                PrimaryKey = cmd,
                                Cmd = new SingleCmd(resultCmdBlock.Text),
                                IsMetaScript = false,
                                Type = EntryType.Dynamic,
                                Arg = (double)resultCmdBlock.Tag
                            };
                        }
                        else
                        {
                            return new ParametrizedEntry<double[]>()
                            {
                                PrimaryKey = cmd,
                                Cmd = new SingleCmd(resultCmdBlock.Text),
                                IsMetaScript = false,
                                Type = EntryType.Dynamic,
                                Arg = (double[])resultCmdBlock.Tag
                            };
                        }
                    },
                    UpdateUI = (entry) =>
                    {
                        checkbox.IsChecked = true;
                        if (entry is IParametrizedEntry<double>)
                        {
                            IParametrizedEntry<double> extendedEntry = (IParametrizedEntry<double>)entry;
                            slider.Value = extendedEntry.Arg;
                            resultCmdBlock.Tag = extendedEntry.Arg;
                        }
                        else
                        {
                            IParametrizedEntry<double[]> extendedEntry = (IParametrizedEntry<double[]>)entry;
                            resultCmdBlock.Text = Executable.GenerateToggleCmd(
                                cmd, extendedEntry.Arg, isInteger).ToString();
                            resultCmdBlock.Tag = extendedEntry.Arg;
                        }
                    },
                    HandleState = (state) => checkbox.IsEnabled = state != EntryStateBinding.InvalidState
                };
                this.entryControllers.Add(cmd, entryBinding);

                // Задаем начальное значение и тут же подключаем обработчика интерфейса
                slider.Value = defaultValue;
            };
            
            void AddComboboxCmdController(string cmd, string[] names, int defaultIndex, bool isIntegerArg, int baseIndex = 0)
            {
                var tuple = PrepareNewRow(cmd, true);
                TextBlock resultCmdBlock = tuple.Item1;
                Grid comboboxGrid = tuple.Item2;
                Button toggleButton = tuple.Item3;
                CheckBox checkbox = tuple.Item4;

                // Если аргумент - число, то создадим сетку с 2-мя кнопками
                //Grid comboboxGrid = new Grid();
                comboboxGrid.ColumnDefinitions.Add(new ColumnDefinition());

                ComboBox combobox = new ComboBox
                {
                    MaxHeight = rowHeight
                };
                comboboxGrid.Children.Add(combobox);
                ComboBoxAssist.SetClassicMode(combobox, true);

                // Если надо предусмотреть функцию toggle, то расширяем сетку и добавляем кнопку
                if (isIntegerArg)
                {
                    toggleButton.Click += (_, __) =>
                    {
                        ToggleWindow toggleWindow = new ToggleWindow(true, 0, names.Length - 1);
                        if ((bool)toggleWindow.ShowDialog())
                        {
                            int[] values = toggleWindow.GeneratedArg.Split(' ').Select(n => int.Parse(n)).ToArray();
                            resultCmdBlock.Text = Executable.GenerateToggleCmd(cmd, values).ToString();
                            // Сохраним аргумент в теге
                            resultCmdBlock.Tag = values;

                            //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                            this.AddEntry(cmd, true);
                        }
                        else
                        {

                        }
                    };
                }

                // Зададим элементы комбобокса
                names.ToList().ForEach(name => combobox.Items.Add(name));

                // Создадим обработчика пораньше, т.к. он понадобится уже при задании начального индекса комбобокса
                EntryController entryBinding = new EntryController()
                {
                    AttachedCheckbox = checkbox,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        checkbox.Focus();
                    },
                    Restore = () =>
                    {
                        // Сначала сбрасываем чекбокс, ибо дальше мы с ним сверяемся
                        checkbox.IsChecked = false;
                        // искусственно сбрасываем выделенный элемент
                        combobox.SelectedIndex = -1; 
                        // и гарантированно вызываем обработчик SelectedIndexChanged
                        combobox.SelectedIndex = defaultIndex; 
                    },
                    Generate = () =>
                    {
                        SingleCmd resultCmd = new SingleCmd(resultCmdBlock.Text);

                        if (resultCmdBlock.Tag is int)
                        {
                            return new ParametrizedEntry<int>()
                            {
                                PrimaryKey = cmd,
                                Type = EntryType.Dynamic,
                                IsMetaScript = false,
                                Cmd = resultCmd,
                                Arg = (int)resultCmdBlock.Tag
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
                                Arg = (int[])resultCmdBlock.Tag
                            };
                        }
                    },
                    UpdateUI = (entry) =>
                    {
                        checkbox.IsChecked = true;

                        if (entry is IParametrizedEntry<int>)
                        {
                            IParametrizedEntry<int> extendedEntry = (IParametrizedEntry<int>)entry;
                            combobox.SelectedIndex = extendedEntry.Arg;
                            resultCmdBlock.Tag = extendedEntry.Arg;
                        }
                        else
                        {
                            IParametrizedEntry<int[]> extendedEntry = (IParametrizedEntry<int[]>)entry;
                            resultCmdBlock.Text = Executable.GenerateToggleCmd(cmd, extendedEntry.Arg).ToString();
                            resultCmdBlock.Tag = extendedEntry.Arg;
                        }
                    },
                    HandleState = (state) => checkbox.IsEnabled = state != EntryStateBinding.InvalidState
                };
                // Добавим его в словарь
                this.entryControllers.Add(cmd, entryBinding);

                combobox.SelectionChanged += (obj, args) =>
                {
                    if (combobox.SelectedIndex == -1) return;

                    string resultCmdStr;
                    if (isIntegerArg)
                        resultCmdStr = $"{cmd} {combobox.SelectedIndex + baseIndex}";
                    else
                        resultCmdStr = $"{cmd} {combobox.SelectedItem}";

                    resultCmdBlock.Text = new SingleCmd(resultCmdStr).ToString();
                    resultCmdBlock.Tag = combobox.SelectedIndex;
                    //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                    this.AddEntry(cmd, true);
                };
                
                // Команда по умолчанию обновится, т.к. уже есть обработчик
                combobox.SelectedIndex = defaultIndex;                
            };

            void AddTextboxNumberCmdController(string cmd, double defaultValue, bool asInteger)
            {
                var tuple = PrepareNewRow(cmd, true);
                TextBlock resultCmdBlock = tuple.Item1;
                Grid textboxGrid = tuple.Item2;
                Button toggleButton = tuple.Item3;
                CheckBox checkbox = tuple.Item4;
                string fixedDefaultStrValue = Executable.FormatNumber(defaultValue, asInteger);
                double fixedDefaultValue = double.Parse(fixedDefaultStrValue, CultureInfo.InvariantCulture);

                //Grid textboxGrid = new Grid();

                TextBox textbox = new TextBox
                {
                    MaxHeight = rowHeight
                };
                textboxGrid.Children.Add(textbox);

                toggleButton.Click += (_, __) =>
                {
                    ToggleWindow toggleWindow = new ToggleWindow(asInteger, double.MinValue, double.MaxValue);
                    if ((bool)toggleWindow.ShowDialog())
                    {
                        double[] values = toggleWindow.GeneratedArg.Split(' ').Select(value =>
                        {
                            Executable.TryParseDouble(value, out double parsedValue);
                            return parsedValue;
                        }).ToArray();

                        resultCmdBlock.Text = Executable.GenerateToggleCmd(cmd, values, asInteger).ToString();
                        // Сохраним аргумент в теге
                        resultCmdBlock.Tag = values;

                        //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                        this.AddEntry(cmd, true);
                    }
                    else
                    {

                    }
                };

                textbox.TextChanged += (obj, args) =>
                {
                    if (!Executable.TryParseDouble(textbox.Text.Trim(), out double fixedValue))
                        return;
                    // Обрезаем дробную часть, если необходимо
                    fixedValue = asInteger? (int)fixedValue: fixedValue;

                    // сохраним последнее верное значение в тег текстового блока
                    resultCmdBlock.Tag = fixedValue;

                    string formatted = Executable.FormatNumber(fixedValue, asInteger);
                    resultCmdBlock.Text = new SingleCmd($"{cmd} {formatted}").ToString();

                    //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                    AddEntry(cmd, true);
                };

                EntryController entryBinding = new EntryController()
                {
                    AttachedCheckbox = checkbox,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        checkbox.Focus();
                    },
                    Restore = () =>
                    {
                        checkbox.IsChecked = false;
                        textbox.Text = fixedDefaultStrValue;
                        resultCmdBlock.Tag = fixedDefaultValue;
                    },
                    Generate = () =>
                    {
                        SingleCmd generatedCmd = new SingleCmd(resultCmdBlock.Text);

                        if (resultCmdBlock.Tag is double)
                        {
                            return new ParametrizedEntry<double>()
                            {
                                PrimaryKey = cmd,
                                Cmd = generatedCmd,
                                Type = EntryType.Dynamic,
                                IsMetaScript = false,
                                Arg = (double)resultCmdBlock.Tag // Подтягиваем аргумент из тега
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
                                Arg = (double[])resultCmdBlock.Tag // Подтягиваем аргумент из тега
                            };
                        }
                    },
                    UpdateUI = (entry) =>
                    {
                        checkbox.IsChecked = true;

                        if (entry is IParametrizedEntry<double>)
                        {
                            IParametrizedEntry<double> extendedEntry = (IParametrizedEntry<double>)entry;
                            textbox.Text = Executable.FormatNumber(extendedEntry.Arg, asInteger);
                            resultCmdBlock.Tag = extendedEntry.Arg;
                        }
                        else
                        {
                            IParametrizedEntry<double[]> extendedEntry = (IParametrizedEntry<double[]>)entry;
                            double[] values = extendedEntry.Arg;

                            resultCmdBlock.Text = Executable.GenerateToggleCmd(cmd, values, asInteger).ToString();
                            resultCmdBlock.Tag = extendedEntry.Arg;                            
                        }                        
                    },
                    HandleState = (state) => checkbox.IsEnabled = state != EntryStateBinding.InvalidState
                };
                this.entryControllers.Add(cmd, entryBinding);

                // Начальное значение
                textbox.Text = fixedDefaultStrValue;
            };

            void AddTextboxStringCmdController(string cmd, string defaultValue)
            {
                var tuple = PrepareNewRow(cmd, false);
                TextBlock resultCmdBlock = tuple.Item1;
                Grid textBoxGrid = tuple.Item2;
                CheckBox checkbox = tuple.Item4;

                TextBox textbox = new TextBox
                {
                    MaxHeight = rowHeight
                };

                textBoxGrid.Children.Add(textbox);

                textbox.TextChanged += (obj, args) =>
                {
                    string text = textbox.Text;
                    bool wrap = text.Contains(" ");
                    
                    // Обернем в команду только название команды, т.к. аргументы в нижнем регистре не нужны
                    resultCmdBlock.Text = $"{new SingleCmd(cmd)} {(wrap?"\"":"")}{text}{(wrap ? "\"" : "")}";

                    //if ((bool)checkbox.IsChecked) // Добавляем в конфиг только если это сделал сам пользователь
                    AddEntry(cmd, true);
                };

                EntryController entryBinding = new EntryController()
                {
                    AttachedCheckbox = checkbox,
                    Focus = () =>
                    {
                        gameSettingsTabButton.IsChecked = true;
                        checkbox.Focus();
                    },
                    Restore = () =>
                    {
                        checkbox.IsChecked = false;
                        textbox.Text = defaultValue;
                    },
                    Generate = () =>
                    {
                        SingleCmd generatedCmd = new SingleCmd(resultCmdBlock.Text, false);

                        return new ParametrizedEntry<string>()
                        {
                            PrimaryKey = cmd,
                            Cmd = generatedCmd,
                            Type = EntryType.Dynamic,
                            IsMetaScript = false,
                            Arg = (string)textbox.Text // Подтягиваем аргумент из тега
                        };
                    },
                    UpdateUI = (entry) =>
                    {
                        checkbox.IsChecked = true;
                        IParametrizedEntry<string> extendedEntry = (IParametrizedEntry<string>)entry;
                        textbox.Text = extendedEntry.Arg;
                    },
                    HandleState = (state) => checkbox.IsEnabled = state != EntryStateBinding.InvalidState
                };
                this.entryControllers.Add(cmd, entryBinding);

                // Начальное значение
                textbox.Text = defaultValue;
            };

            void AddGroupHeader(string text)
            {
                TextBlock block = new TextBlock();
                block.Inlines.Add(new Bold(new Run(text)));
                block.HorizontalAlignment = HorizontalAlignment.Center;
                block.VerticalAlignment = VerticalAlignment.Center;

                if (settingsTabPanel.Children.Count != 0)
                    block.Margin = new Thickness(0, 5, 0, 0);

                settingsTabPanel.Children.Add(block);
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
            
            AddGroupHeader(Res.CategoryBots);
            AddComboboxCmdController("bot_stop", toggleStrings, 0, true);
            AddComboboxCmdController("bot_mimic", toggleStrings, 0, true);
            AddComboboxCmdController("bot_crouch", toggleStrings, 0, true);
            AddIntervalCmdController("bot_mimic_yaw_offset", 0, 180, 5, 0);

            // текстовые аргументы
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
        }

        void InitExtra()
        {
            // --- custom command execution ---
            const string execCustomCmdsEntryKey = "ExecCustomCmds";
            this.customCmdHeaderCheckbox.Click += HandleEntryClick;
            this.customCmdHeaderCheckbox.Tag = execCustomCmdsEntryKey;

            // Зададим действия по клику на кнопки в этом методе, чтобы ключ нигде кроме как здесь не упоминался
            // Повесим действие по добавлению новой команды на кнопку
            addCmdButton.Click += (sender, args) =>
            {
                ButtonBase cmdElement = new Chip
                {
                    Style = (Style)this.Resources["BubbleButton"],
                    Content = cmdTextbox.Text.Trim()
                };
                customCmdPanel.Children.Add(cmdElement);
                customCmdPanel.Tag = cmdElement;

                cmdElement.Click += (_, __) => customCmdPanel.Tag = cmdElement;

                Binding tagBinding = new Binding("Tag")
                {
                    Source = customCmdPanel,
                    Converter = new TagToFontWeightConverter(),
                    ConverterParameter = cmdElement
                };
                cmdElement.SetBinding(ButtonBase.FontWeightProperty, tagBinding);

                AddEntry(execCustomCmdsEntryKey, false);
            };
            // Обработчик нажатия на кнопку добавления неизвестной программе команды
            addUnknownCmdButton.Click += (sender, args) =>
            {
                // Перейдем в клатке с соответствующим контроллером
                this.entryControllers[execCustomCmdsEntryKey].Focus();
                //this.entryControllers[execCustomCmdsEntryKey].

                // Добавим указанную пользователем команду в контроллер ExecCustomCmds
                string cmd = searchCmdBox.Text.Trim();

                // Сделаем контроллер активным, искусственно нажав кнопку, если он не активен.
                // Т.к. искусственный вызов ClickEvent не чекбоксе не меняет его состояния,
                // переключим его вручную и вызовем нужный метод для добавления в конфиг элемента
                customCmdHeaderCheckbox.IsChecked = true;
                HandleEntryClick(customCmdHeaderCheckbox, new RoutedEventArgs(CheckBox.ClickEvent));
                
                cmdTextbox.Text = cmd;
                addCmdButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                cmdTextbox.Text = string.Empty;
            };

            // А так же повесим действие на кнопку удаления команды
            deleteCmdButton.Click += (sender, args) =>
            {
                ButtonBase targetButton = customCmdPanel.Tag as ButtonBase;
                BindingOperations.ClearAllBindings(targetButton);
                customCmdPanel.Children.Remove(targetButton);

                if (customCmdPanel.Children.Count > 0)
                {
                    customCmdPanel.Tag = customCmdPanel.Children[0];
                    this.AddEntry(execCustomCmdsEntryKey, false);
                }
                else
                {
                    customCmdPanel.Tag = null;
                }
            };

            this.entryControllers.Add(execCustomCmdsEntryKey, new EntryController()
            {
                AttachedCheckbox = customCmdHeaderCheckbox,
                Focus = () => extraTabButton.IsChecked = true,
                HandleState = (state) => customCmdHeaderCheckbox.IsEnabled = state != EntryStateBinding.InvalidState,
                Restore = () =>
                {
                    this.ClearPanel_s(customCmdPanel);

                    cmdTextbox.Text = string.Empty;
                    customCmdHeaderCheckbox.IsChecked = false;
                },
                Generate = () =>
                {
                    // Получим все указанные пользователем команды
                    string[] cmds = customCmdPanel.Children.OfType<ButtonBase>()
                        .Select(b => b.Content.ToString()).ToArray();

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
                    customCmdHeaderCheckbox.IsChecked = true;

                    IParametrizedEntry<string[]> extendedEntry = (IParametrizedEntry<string[]>)entry;
                    string[] cmds = extendedEntry.Arg;

                    this.ClearPanel_s(customCmdPanel);

                    foreach (string cmd in cmds)
                    {
                        cmdTextbox.Text = cmd;
                        addCmdButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                }
            });

            // cycle crosshairs
            const string cycleCrosshairEntryKey = "CycleCrosshair";

            this.cycleChHeaderCheckbox.Click += HandleEntryClick;
            this.cycleChHeaderCheckbox.Tag = cycleCrosshairEntryKey;
            this.entryControllers.Add(cycleCrosshairEntryKey, new EntryController()
            {
                AttachedCheckbox = cycleChHeaderCheckbox,
                Focus = () =>
                {
                    extraTabButton.IsChecked = true;
                    cycleChHeaderCheckbox.Focus();
                },
                Generate = () =>
                {
                    int crosshairCount = (int)cycleChSlider.Value;
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
                    cycleChHeaderCheckbox.IsChecked = true;
                    int crosshairCount = (entry as IParametrizedEntry<int>).Arg;

                    cycleChSlider.Value = crosshairCount;
                },
                Restore = () =>
                {
                    cycleChHeaderCheckbox.IsChecked = false;
                    cycleChSlider.Value = cycleChSlider.Minimum;
                },
                HandleState = (state) => cycleChHeaderCheckbox.IsEnabled =
                    state != EntryStateBinding.Default && state != EntryStateBinding.InvalidState
            });

            cycleChSlider.ValueChanged += (_, __) =>
            {
                //if ((bool)cycleChHeaderCheckbox.IsChecked == true)
                this.AddEntry(cycleCrosshairEntryKey, true);
            };


            // volume regulator
            const string volumeRegulatorEntryKey = "VolumeRegulator";
            this.volumeRegulatorCheckbox.Click += HandleEntryClick;
            this.volumeRegulatorCheckbox.Tag = volumeRegulatorEntryKey;

            void volumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                if (volumeStepSlider == null || maxVolumeSlider == null || minVolumeSlider == null)
                    return;

                Slider slider = (Slider)sender;

                // Нижняя граница изменилась
                if (slider.Name == minVolumeSlider.Name)
                {
                    maxVolumeSlider.Minimum = slider.Value + 0.01;
                }
                else if (slider.Name == maxVolumeSlider.Name)
                {
                    // Иначе верхняя граница изменилась
                    minVolumeSlider.Maximum = slider.Value - 0.01;
                }
                else
                {
                    // Иначе изменился шаг
                }

                // Определим дельту
                double delta = maxVolumeSlider.Value - minVolumeSlider.Value;
                volumeStepSlider.Maximum = delta;

                // Обновим регулировщик в конфиге, если изменение было сделано пользователем
                //if ((bool)volumeRegulatorCheckbox.IsChecked)
                this.AddEntry(volumeRegulatorEntryKey, true);
            }
            minVolumeSlider.ValueChanged += volumeSliderValueChanged;
            maxVolumeSlider.ValueChanged += volumeSliderValueChanged;
            volumeStepSlider.ValueChanged += volumeSliderValueChanged;
            volumeDirectionCombobox.SelectionChanged += (_, __) =>
            {
                // Обновим регулировщик в конфиге, если изменение было сделано пользователем
                //if ((bool)volumeRegulatorCheckbox.IsChecked)
                this.AddEntry(volumeRegulatorEntryKey, true);
            };

            this.entryControllers.Add(volumeRegulatorEntryKey, new EntryController()
            {
                AttachedCheckbox = volumeRegulatorCheckbox,
                Focus = () =>
                {
                    extraTabButton.IsChecked = true;
                    volumeRegulatorCheckbox.Focus();
                },
                Generate = () =>
                {
                    double minVolume = Math.Round(minVolumeSlider.Value, 2);
                    double maxVolume = Math.Round(maxVolumeSlider.Value, 2);
                    double volumeStep = Math.Round(volumeStepSlider.Value, 2);
                    volumeStep = volumeStep == 0 ? 0.01 : volumeStep;
                    bool volumeUp = volumeDirectionCombobox.SelectedIndex == 1;

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
                    volumeRegulatorCheckbox.IsChecked = true;
                    double[] args = ((IParametrizedEntry<double[]>)entry).Arg;

                    minVolumeSlider.Value = args[0];
                    maxVolumeSlider.Value = args[1];
                    volumeStepSlider.Value = args[2];
                    volumeDirectionCombobox.SelectedIndex = entry.Cmd.ToString() == "volume_up"? 1 : 0;
                },
                Restore = () =>
                {
                    volumeRegulatorCheckbox.IsChecked = false;
                },
                HandleState = (state) => volumeRegulatorCheckbox.IsEnabled =
                    state != EntryStateBinding.InvalidState && state != EntryStateBinding.Default
            });
        }

        void InitAliasController()
        {
            const string extraAliasEntryKey = "ExtraAlias";

            this.entryControllers.Add(extraAliasSetEntryKey, new EntryController()
            {
                Generate = () =>
                {
                    List<ParametrizedEntry<Entry[]>> aliases =
                    new List<ParametrizedEntry<Entry[]>>();

                    CommandCollection dependencies = new CommandCollection();

                    foreach (ContentControl aliaselement in aliasPanel.Children.OfType<ContentControl>())
                    {
                        string aliasName = aliaselement.Content.ToString();
                        List<Entry> attachedEntries = (List<Entry>)aliaselement.Tag;

                        // Выпишем все зависимости, которые есть для текущего элемента
                        foreach (Entry entry in attachedEntries)
                            foreach (Executable dependency in entry.Dependencies)
                                dependencies.Add(dependency);

                        ParametrizedEntry<Entry[]> aliasEntry = new ParametrizedEntry<Entry[]>()
                        {
                            PrimaryKey = extraAliasEntryKey,
                            Cmd = new SingleCmd(aliasName),
                            IsMetaScript = false,
                            Type = EntryType.Dynamic,
                            Arg = attachedEntries.ToArray()
                        };

                        AliasCmd alias = new AliasCmd(
                            aliaselement.Content.ToString(),
                            attachedEntries.Select(e => e.Cmd));

                        aliases.Add(aliasEntry);
                        dependencies.Add(alias);
                    }

                    // сформируем итоговый элемент конфига
                    return new ParametrizedEntry<Entry[]>()
                    {
                        PrimaryKey = extraAliasSetEntryKey,
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
                        AddAliasButton(
                            alias.Cmd.ToString(),
                            (alias as ParametrizedEntry<Entry[]>).Arg.ToList());
                    }
                },
                Restore = () =>
                {
                    this.ResetAttachmentPanels();
                    this.ClearPanel_s(aliasPanel);
                }
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
                        string aliasName = (aliasPanel.Tag as ContentControl).Content.ToString();
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

            // Получим обработчика и 
            EntryController entryBinding = this.entryControllers[entryKey];
            Entry entry = (Entry)entryBinding.Generate();

            if ((bool)checkbox.IsChecked)
                this.AddEntry(entry);
            else
                this.RemoveEntry(entry);

            // Обновим панели
            this.UpdateAttachmentPanels();
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
            EntryController controller = this.entryControllers[cfgEntryKey];

            // Если сказано, что отмена, если добавление идет не из-за действий пользователя
            // То значит гарантированно AttachedCheckbox не может быть равен null
            if (abortIfNotUser && controller.AttachedCheckbox.IsChecked == false) return;

            Entry generatedEntry = (Entry)controller.Generate();
            this.AddEntry(generatedEntry);
        }

        void RemoveEntry(string cfgEntryKey)
        {
            Entry generatedEntry = (Entry)this.entryControllers[cfgEntryKey].Generate();
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
                        if (entry.PrimaryKey != extraAliasSetEntryKey)
                        {
                            // Добавляем текущий элемент к коллекции, привязанной к выбранной кнопке
                            FrameworkElement targetElement = aliasPanel.Tag as FrameworkElement;
                            List<Entry> attachedToAlias = targetElement.Tag as List<Entry>;
                            targetElement.Tag = attachedToAlias
                                .Where(attachedEntry => attachedEntry.PrimaryKey != entry.PrimaryKey)
                                .Concat(new Entry[] { entry }).ToList();

                            // И вызываем обработчика пользовательских алиасов
                            Entry aliasSetEntry = (Entry)this.entryControllers[extraAliasSetEntryKey].Generate();
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
                        if (entry.PrimaryKey != extraAliasSetEntryKey)
                        {
                            // Добавляем текущий элемент к коллекции, привязанной к выбранной кнопке
                            FrameworkElement targetElement = aliasPanel.Tag as FrameworkElement;
                            List<Entry> attachedToAlias = targetElement.Tag as List<Entry>;
                            targetElement.Tag = attachedToAlias
                                .Where(attachedEntry => attachedEntry.PrimaryKey != entry.PrimaryKey)
                                .ToList();

                            // Напрямую обновим узел в менеджере
                            Entry aliasSetEntry = (Entry)this.entryControllers[extraAliasSetEntryKey].Generate();
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

        void ResetAttachmentPanels()
        {
            // Получим предыдущие элементы и сбросим связанные с ними элементы интерфейса
            // Для этого объединим коллекции элементов из всех панелей
            List<WrapPanel> attachmentPanels = new List<WrapPanel>()
            {
                onKeyDownAttachmentsPanel,
                onKeyReleaseAttachmentsPanel,
                solidAttachmentsPanel
            };

            IEnumerable<FrameworkElement> mergedElements = attachmentPanels.SelectMany(p => p.Children.Cast<FrameworkElement>());

            foreach (FrameworkElement element in mergedElements)
            {
                EntryController entryBinding = this.entryControllers[(string)element.Tag];
                // Метод, отвечающий непосредственно за сброс состояния интерфейса
                entryBinding.Restore();
            }

            // Очистим панели
            attachmentPanels.ForEach(p => p.Children.Clear());
        }

        /// <summary>
        /// Метод для обновления панелей с привязанными к сочетанию клавиш элементами конфига
        /// </summary>
        void UpdateAttachmentPanels()
        {
            // Очистим панели и сбросим настройки интерфейса
            ResetAttachmentPanels();

            void AddAttachment(string entryKey, WrapPanel panel)
            {
                // Создадим новую кнопку и зададим нужный стиль

                ButtonBase chip = new Chip
                {
                    Content = Localize(entryKey) //CurrentLocale[cfgEntry]
                };
                chip.Click += HandleAttachedEntryClick;
                chip.Style = (Style)this.Resources["BubbleButton"];
                chip.Tag = entryKey;
                chip.FontSize = 13;
                chip.Height = 17;
                
                // Добавим в нужную панель
                panel.Children.Add(chip);
            };

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
                        AddAttachment(entry.PrimaryKey, onKeyDownAttachmentsPanel);
                    else
                        AddAttachment(entry.PrimaryKey, onKeyReleaseAttachmentsPanel);

                    // Обновим интерфейс согласно элементам, привязанным к текущему состоянию
                    attachedEntries.Where(e => this.IsEntryAttachedToCurrentState(e))
                        .ToList().ForEach(e => this.entryControllers[e.PrimaryKey].UpdateUI(e));
                });
            }
            else if (this.StateBinding == EntryStateBinding.Default)
            {
                // Получаем все элементы по умолчанию, которые должны быть отображены в панели
                List<Entry> attachedEntries = this.cfgManager.DefaultEntries
                    .Where(e => this.entryControllers[e.PrimaryKey].AttachedCheckbox != null).ToList();

                // Теперь заполним панели новыми элементами
                attachedEntries.ForEach(entry =>
                {
                    AddAttachment(entry.PrimaryKey, solidAttachmentsPanel);
                    // Обновим интерфейс согласно элементам, привязанным к текущему состоянию
                    this.entryControllers[entry.PrimaryKey].UpdateUI(entry);
                });
            }
            else if (this.StateBinding == EntryStateBinding.Alias)
            {
                if (this.aliasPanel.Tag == null) return;

                // Узнаем какие элементы привязаны к текущей команде
                List<Entry> attachedEntries = (List<Entry>)(this.aliasPanel.Tag as FrameworkElement).Tag;

                attachedEntries.ForEach(entry =>
                {
                    AddAttachment(entry.PrimaryKey, solidAttachmentsPanel);
                    // Обновим интерфейс согласно элементам, привязанным к текущему состоянию
                    this.entryControllers[entry.PrimaryKey].UpdateUI(entry);
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
            if (element.Tag == null) return; // TODO: УДАЛИТЬ
            string entryKey = (string) element.Tag;

            // Получим обработчика и переведем фокус на нужный элемент
            this.entryControllers[entryKey].Focus();
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

        void AddAliasButton(string name, List<Entry> attachedEntries)
        {
            if (aliasPanel.Children.OfType<ButtonBase>().Any(b => b.Content.ToString() == name))
                return;

            Chip chip = new Chip
            {
                Content = name,
                Tag = attachedEntries,
                Style = (Style)this.Resources["BubbleButton"]
            };

            chip.Click += (_, __) =>
            {
                // При нажатии на кнопку задаем в теге 
                // панели какая выбрана в данный момент
                aliasPanel.Tag = chip;

                // Очистим панели и заполним их согласно выбранному алиасу
                UpdateAttachmentPanels();
            };

            aliasPanel.Children.Add(chip);
            aliasPanel.Tag = chip;

            if (this.StateBinding == EntryStateBinding.InvalidState)
                this.StateBinding = EntryStateBinding.Alias;

            // Программно настроим привязку 
            Binding binding = new Binding("Tag")
            {
                Source = aliasPanel,
                Converter = new TagToFontWeightConverter(),
                ConverterParameter = chip
            };
            chip.SetBinding(ButtonBase.FontWeightProperty, binding);

            // Искусственно генерируем клик на новую кнопку, чтобы перестроить интерфейс
            chip.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        void ClearPanel_s(WrapPanel panel)
        {
            foreach (FrameworkElement element in panel.Children)
                BindingOperations.ClearAllBindings(element);

            panel.Children.Clear();
            panel.Tag = null;
        }

        void UpdateCfgManager()
        {
            // Сбросим все настройки от прошлого конфига
            foreach (EntryController binding in this.entryControllers.Values)
                binding.Restore();

            // Зададим привязку к дефолтному состоянию
            this.StateBinding = EntryStateBinding.Default;

            foreach (Entry entry in this.cfgManager.DefaultEntries)
                this.entryControllers[entry.PrimaryKey].UpdateUI(entry);

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
