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
using System.Windows.Input;
using ConfigMaker.Mvvm.ViewModels;
using ConfigMaker.Mvvm;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel MainVM => this.DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.MainVM.SaveCommand.Execute(null);
        }

        private void AddCmdButton_Click(object sender, RoutedEventArgs e)
        {
            //this.MainVM.AddCmdCommand.Execute(null); // TODO: Реализовать команду в CustomCmdViewModel
        }

        private void DeleteCmdButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DeleteCmdCommand.Execute(null);
        }

        private void GenerateRandomCrosshairsButton_Click(object sender, RoutedEventArgs e)
        {
            //this.GenerateCrosshairsCommand.Execute(null);
        }

        private void AddAliasButton_Click(object sender, RoutedEventArgs e)
        {
            //this.GenerateCrosshairsCommand.Execute(null);
        }

        private void DeleteAliasButton_Click(object sender, RoutedEventArgs e)
        {
            //this.GenerateCrosshairsCommand.Execute(null);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ComboBox cbox = (ComboBox)e.Source;
            //ComboBoxItem selectedItem = (ComboBoxItem)cbox.SelectedItem;

            //if (selectedItem.Name == iKeyboard.Name)
            //{
            //    // При выборе клавиатуры по умолчанию не выбрана последовательность
            //    this.SetStateAndUpdateUI(EntryStateBinding.InvalidState);
            //}
            //else
            //{
            //    if (solidAttachmentsVM == null) return;

            //    EntryStateBinding selectedState = selectedItem.Name == iDefault.Name ?
            //        EntryStateBinding.Default :
            //        EntryStateBinding.Alias;

            //    solidAttachmentsVM.Hint = selectedState == EntryStateBinding.Default ?
            //        Res.CommandsByDefault_Hint :
            //        Res.CommandsInAlias_Hint;

            //    this.currentKeySequence = null;
            //    this.ColorizeKeyboard();

            //    if (selectedState == EntryStateBinding.Alias)
            //    {
            //        // И при этом если ни одной команды не создано, то задаем неверное состояние
            //        selectedState =
            //            this.aliasSetVM.Items.Count == 0 ?
            //            EntryStateBinding.InvalidState :
            //            EntryStateBinding.Alias;
            //    }

            //    this.SetStateAndUpdateUI(selectedState);
            //}

            //UpdateAttachmentPanels();
        }

        private void GenerateConfig(object sender, RoutedEventArgs e)
        {
            //if (this.CfgName.Length == 0)
            //    this.CfgName = (string)CfgNameProperty.DefaultMetadata.DefaultValue;

            //string resultCfgPath = Path.Combine(GetTargetFolder(), $"{this.CfgName}.cfg");

            //this.cfgManager.GenerateCfg(resultCfgPath);
            //System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{resultCfgPath}\"");
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchCmdTextbox(object sender, TextChangedEventArgs e)
        {
            //TextBox textbox = (TextBox)sender;
            //string input = textbox.Text.Trim().ToLower();

            //DynamicEntryViewModel[] elements = ((IEnumerable<BindableBase>)gameSettingsItemsControl.ItemsSource)
            //    .Where(item => item is DynamicEntryViewModel).Cast<DynamicEntryViewModel>().ToArray();

            //// Выводим все элементы, если ничего не ищем
            //if (input.Length == 0)
            //{
            //    foreach (DynamicEntryViewModel element in elements)
            //        element.IsVisible = true;
            //    addUnknownCmdButton.Visibility = Visibility.Hidden;
            //}
            //else
            //{
            //    int foundCount = 0;

            //    foreach (DynamicEntryViewModel element in elements)
            //    {
            //        if (element.Key != null)
            //        {
            //            string entryKey = element.Key;
            //            if (entryKey.ToLower().Contains(input))
            //            {
            //                element.IsVisible = true;
            //                foundCount++;
            //            }
            //            else
            //            {
            //                element.IsVisible = false;
            //            }
            //        }
            //        else
            //            element.IsVisible = false;
            //    }

            //    // Если ничего не выведено - предалагем добавить
            //    if (foundCount == 0)
            //    {
            //        addUnknownCmdButton.Content = string.Format(Res.UnknownCommandExecution_Format, input);
            //        addUnknownCmdButton.Visibility = Visibility.Visible;
            //    }
            //    else
            //        addUnknownCmdButton.Visibility = Visibility.Hidden;
            //}
        }

        private void kb_OnKeyboardKeyDown(object sender, VirtualKeyboard.KeyboardClickRoutedEvtArgs e)
        {
            this.MainVM.ClickButton(e.Key, e.SpecialKeyFlags);
            this.ColorizeKeyboard();
        }

        //private void KeyboardKeyDownHandler(object sender, VirtualKeyboard.KeyboardClickRoutedEvtArgs args)
        //{
        //    // Определим новую последовательность
        //    string key = args.Key.ToLower();

        //    // Убедимся, что в комбобоксе выделен нужный элемент
        //    this.iKeyboard.IsSelected = true;

        //    if (currentKeySequence == null || currentKeySequence.Keys.Length == 2 ||
        //        currentKeySequence.Keys.Length == 1 && !args.SpecialKeyFlags.HasFlag(VirtualKeyboard.SpecialKey.Shift))
        //    {
        //        // Теперь создаем новую последовательность с 1 клавишей
        //        currentKeySequence = new KeySequence(key);
        //        this.SetStateAndUpdateUI(EntryStateBinding.KeyDown);

        //        // Отредактируем текст у панелей
        //        this.keyDownAttachmentsVM.Hint = string.Format(Res.KeyDown1_Format, currentKeySequence[0].ToUpper());
        //        this.keyUpAttachmentsVM.Hint = string.Format(Res.KeyUp1_Format, currentKeySequence[0].ToUpper());
        //    }
        //    else if (currentKeySequence.Keys.Length == 1)
        //    {
        //        // Иначе в последовательности уже есть 1 кнопка и надо добавить вторую
        //        // Проверяем, что выбрана не та же кнопка
        //        if (currentKeySequence[0] == key) return;

        //        currentKeySequence = new KeySequence(currentKeySequence[0], key);
        //        this.SetStateAndUpdateUI(EntryStateBinding.KeyDown);

        //        string key1Upper = currentKeySequence[0].ToUpper();
        //        string key2Upper = currentKeySequence[1].ToUpper();

        //        this.keyDownAttachmentsVM.Hint = string.Format(Res.KeyDown2_Format, key2Upper, key1Upper);
        //        this.keyUpAttachmentsVM.Hint = string.Format(Res.KeyUp2_Format, key2Upper, key1Upper);
        //    }

        //    ColorizeKeyboard();

        //    // Обновляем интерфейс под новую последовательность
        //    this.UpdateAttachmentPanels();
        //}



        //#region Proxy methods
        //private void CmdInputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //this.HandleCmdNameCommand.Execute(null);
        //}


        //public ICommand AddCmdCommand { get; private set; }
        //public ICommand DeleteCmdCommand { get; private set; }
        //public ICommand GenerateCrosshairsCommand { get; private set; }

        //void InitExtra()
        //{
        //    // --- custom command execution ---
        //    const string execCustomCmdsEntryKey = "ExecCustomCmds";

        //    CustomCmdControllerViewModel customCmdVM = new CustomCmdControllerViewModel((cmd) => cmd.Trim().Length > 0)
        //    {
        //        Content = this.Localize(execCustomCmdsEntryKey),
        //        Key = execCustomCmdsEntryKey
        //    };
        //    extraItemsControl.Items.Add(customCmdVM);

        //    // Зададим действия по клику на кнопки в этом методе, чтобы ключ нигде кроме как здесь не упоминался
        //    // Повесим действие по добавлению новой команды на кнопку
        //    this.AddCmdCommand = new DelegateCommand(() =>
        //    {
        //        customCmdVM.Items.Add(new ItemViewModel() { Text = customCmdVM.Input });
        //        this.AddEntry(execCustomCmdsEntryKey, false);
        //    });
        //    // Обработчик нажатия на кнопку добавления неизвестной программе команды
        //    addUnknownCmdButton.Click += (sender, args) =>
        //    {
        //        // Перейдем в клатке с соответствующим контроллером
        //        this.GetController(execCustomCmdsEntryKey).Focus();
        //        //this.entryControllers[execCustomCmdsEntryKey].
        //        customCmdVM.IsChecked = true;

        //        // Добавим указанную пользователем команду в контроллер ExecCustomCmds
        //        string cmd = searchCmdBox.Text.Trim();
        //        customCmdVM.Input = cmd;

        //        // И выполним команду по добавлению
        //        this.AddCmdCommand.Execute(null);

        //    };

        //    // А так же повесим действие на кнопку удаления команды
        //    this.DeleteCmdCommand = new DelegateCommand(() =>
        //    {
        //        int firstSelectedIndex = customCmdVM.Items
        //            .IndexOf(customCmdVM.Items.First(b => b.IsSelected));

        //        customCmdVM.Items.RemoveAt(firstSelectedIndex);
        //        AddEntry(execCustomCmdsEntryKey, false);
        //    });

        //    this.entryV2Controllers.Add(new EntryController()
        //    {
        //        //Model = customCmdVM,
        //        Focus = () => extraTabButton.IsChecked = true,
        //        HandleState = (state) => customCmdVM.IsEnabled = state != EntryStateBinding.InvalidState,
        //        Restore = () =>
        //        {
        //            customCmdVM.Items.Clear();

        //            customCmdVM.Input = string.Empty;
        //            customCmdVM.UpdateIsChecked(false);
        //        },
        //        Generate = () =>
        //        {
        //            // Получим все указанные пользователем команды
        //            string[] cmds = customCmdVM.Items.Select(b => b.Text).ToArray();

        //            SingleCmd cmd = null;
        //            CommandCollection dependencies = null;

        //            // Если указана только одна команда - просто выписываем её в конфиг напрямую
        //            if (cmds.Length == 1)
        //            {
        //                cmd = new SingleCmd(cmds[0].Trim());
        //                dependencies = new CommandCollection();
        //            }
        //            else
        //            {
        //                // Иначе генерируем специальный алиас и привязываемся к нему
        //                string aliasName = $"{GeneratePrefix()}_exec";
        //                AliasCmd execCmdsAlias = new AliasCmd(aliasName, cmds.Select(strCmd => new SingleCmd(strCmd)));

        //                cmd = new SingleCmd(aliasName);
        //                dependencies = new CommandCollection(execCmdsAlias);
        //            }

        //            return new ParametrizedEntry<string[]>()
        //            {
        //                PrimaryKey = execCustomCmdsEntryKey,
        //                Cmd = cmd,
        //                IsMetaScript = false,
        //                Type = EntryType.Dynamic,
        //                Dependencies = dependencies,
        //                Arg = cmds
        //            };
        //        },
        //        UpdateUI = (entry) =>
        //        {
        //            customCmdVM.UpdateIsChecked(true);

        //            IParametrizedEntry<string[]> extendedEntry = (IParametrizedEntry<string[]>)entry;
        //            string[] cmds = extendedEntry.Arg;

        //            customCmdVM.Items.Clear();

        //            foreach (string cmd in cmds)
        //            {
        //                customCmdVM.Input = cmd;
        //                this.AddCmdCommand.Execute(null);
        //            }
        //        }
        //    });

        //    // cycle crosshairs
        //    const string cycleCrosshairEntryKey = "CycleCrosshair";

        //    CycleCrosshairViewModel cycleChVM = new CycleCrosshairViewModel()
        //    {
        //        Content = Localize(cycleCrosshairEntryKey),
        //        Key = cycleCrosshairEntryKey
        //    };
        //    extraItemsControl.Items.Add(cycleChVM);

        //    this.GenerateCrosshairsCommand = new DelegateCommand(() =>
        //    {
        //        GenerateRandomCrosshairs(cycleChVM.CrosshairCount);
        //    });

        //    this.entryV2Controllers.Add(new EntryController()
        //    {
        //        //Model = cycleChVM,
        //        Focus = () =>
        //        {
        //            extraTabButton.IsChecked = true;
        //            cycleChVM.IsFocused = true;
        //        },
        //        Generate = () =>
        //        {
        //            int crosshairCount = cycleChVM.CrosshairCount;
        //            string prefix = GeneratePrefix();
        //            string scriptName = $"{prefix}_crosshairLoop";

        //            // Зададим имена итерациям
        //            string[] iterationNames = new string[crosshairCount];

        //            for (int i = 0; i < crosshairCount; i++)
        //                iterationNames[i] = $"{scriptName}{i + 1}";

        //            List<CommandCollection> iterations = new List<CommandCollection>();

        //            for (int i = 0; i < crosshairCount; i++)
        //            {
        //                CommandCollection currentIteration = new CommandCollection()
        //                {
        //                    new SingleCmd($"exec {prefix}_ch{i + 1}"),
        //                    new SingleCmd($"echo \"Crosshair {i + 1} loaded\"")
        //                };
        //                iterations.Add(currentIteration);
        //            }

        //            CycleCmd crosshairLoop = new CycleCmd(scriptName, iterations, iterationNames);

        //            // Задаем начальную команду для алиаса
        //            CommandCollection dependencies = new CommandCollection();

        //            // И добавим в конец все итерации нашего цикла
        //            foreach (Executable iteration in crosshairLoop)
        //                dependencies.Add(iteration);

        //            return new ParametrizedEntry<int>()
        //            {
        //                PrimaryKey = cycleCrosshairEntryKey,
        //                Cmd = new SingleCmd(scriptName),
        //                Type = EntryType.Dynamic,
        //                IsMetaScript = false,
        //                Arg = crosshairCount,
        //                Dependencies = dependencies
        //            };
        //        },
        //        UpdateUI = (entry) =>
        //        {
        //            cycleChVM.UpdateIsChecked(true);
        //            int crosshairCount = (entry as IParametrizedEntry<int>).Arg;

        //            cycleChVM.CrosshairCount = crosshairCount;
        //        },
        //        Restore = () =>
        //        {
        //            cycleChVM.UpdateIsChecked(false);
        //            cycleChVM.CrosshairCount = cycleChVM.MinimumCount;
        //        },
        //        HandleState = (state) => cycleChVM.IsEnabled =
        //            state != EntryStateBinding.Default && state != EntryStateBinding.InvalidState
        //    });

        //    cycleChVM.PropertyChanged += (_, arg) =>
        //    {
        //        //if ((bool)cycleChHeaderCheckbox.IsChecked == true)
        //        if (arg.PropertyName == nameof(CycleCrosshairViewModel.CrosshairCount))
        //            this.AddEntry(cycleCrosshairEntryKey, true);
        //    };


        //    // volume regulator
        //    const string volumeRegulatorEntryKey = "VolumeRegulator";

        //    VolumeRegulatorControllerViewModel volumeVM = new VolumeRegulatorControllerViewModel()
        //    {
        //        Content = Localize(volumeRegulatorEntryKey),
        //        Key = volumeRegulatorEntryKey
        //    };
        //    extraItemsControl.Items.Add(volumeVM);

        //    volumeVM.PropertyChanged += (_, arg) =>
        //    {
        //        string prop = arg.PropertyName;

        //        if (prop == nameof(VolumeRegulatorControllerViewModel.Mode))
        //        {
        //            this.AddEntry(volumeRegulatorEntryKey, true);
        //            return;
        //        }
        //        else if (prop == nameof(VolumeRegulatorControllerViewModel.From))
        //        {
        //            volumeVM.ToMinimum = volumeVM.From + 0.01;
        //        }
        //        else if (prop == nameof(VolumeRegulatorControllerViewModel.To))
        //        {
        //            volumeVM.FromMaximum = volumeVM.To - 0.01;
        //        }
        //        else if (prop == nameof(VolumeRegulatorControllerViewModel.Step))
        //        {

        //        }

        //        // Определим дельту
        //        double delta = volumeVM.To - volumeVM.From;
        //        volumeVM.StepMaximum = delta;

        //        // Обновим регулировщик в конфиге, если изменение было сделано пользователем
        //        this.AddEntry(volumeRegulatorEntryKey, true);
        //    };

        //    this.entryV2Controllers.Add(new EntryController()
        //    {
        //        //Model = volumeVM,
        //        Focus = () =>
        //        {
        //            extraTabButton.IsChecked = true;
        //            volumeVM.IsFocused = true;
        //        },
        //        Generate = () =>
        //        {
        //            double minVolume = Math.Round(volumeVM.From, 2);
        //            double maxVolume = Math.Round(volumeVM.To, 2);
        //            double volumeStep = Math.Round(volumeVM.Step, 2);
        //            volumeStep = volumeStep == 0 ? 0.01 : volumeStep;
        //            bool volumeUp = volumeVM.Mode == 1;

        //            // Определяем промежуточные значения от максимума к минимуму
        //            List<double> volumeValues = new List<double>();

        //            double currentValue = maxVolume;

        //            while (currentValue >= minVolume)
        //            {
        //                volumeValues.Add(currentValue);
        //                currentValue -= volumeStep;
        //                string formatted = Executable.FormatNumber(currentValue, false);
        //                Executable.TryParseDouble(formatted, out currentValue);
        //            }
        //            // Если минимальное значение не захватилось, то добавим его вручную
        //            if (volumeValues.Last() != minVolume)
        //                volumeValues.Add(minVolume);

        //            // Теперь упорядочим по возрастанию
        //            volumeValues.Reverse();

        //            // Создаем цикл
        //            string volumeUpCmd = "volume_up";
        //            string volumeDownCmd = "volume_down";

        //            SingleCmd[] iterationNames = volumeValues
        //                .Select(v => new SingleCmd($"volume_{Executable.FormatNumber(v, false)}")).ToArray();

        //            CommandCollection dependencies = new CommandCollection();

        //            for (int i = 0; i < volumeValues.Count; i++)
        //            {
        //                double value = volumeValues[i];
        //                string formattedValue = Executable.FormatNumber(value, false);

        //                CommandCollection iterationCmds = new CommandCollection();

        //                // Задаем звук на текущей итерации с комментарием в консоль
        //                SingleCmd volumeCmd = new SingleCmd($"volume {formattedValue}");
        //                iterationCmds.Add(volumeCmd);
        //                iterationCmds.Add(new SingleCmd($"echo {volumeCmd.ToString()}"));

        //                if (i == 0)
        //                {
        //                    iterationCmds.Add(
        //                        new AliasCmd(volumeDownCmd, new SingleCmd("echo Volume: Min")));
        //                    iterationCmds.Add(
        //                        new AliasCmd(volumeUpCmd, iterationNames[i + 1]));
        //                }
        //                else if (i == volumeValues.Count - 1)
        //                {
        //                    iterationCmds.Add(
        //                        new AliasCmd(volumeUpCmd, new SingleCmd("echo Volume: Max")));
        //                    iterationCmds.Add(
        //                        new AliasCmd(volumeDownCmd, iterationNames[i - 1]));
        //                }
        //                else
        //                {
        //                    iterationCmds.Add(
        //                        new AliasCmd(volumeDownCmd, iterationNames[i - 1]));
        //                    iterationCmds.Add(
        //                        new AliasCmd(volumeUpCmd, iterationNames[i + 1]));
        //                }

        //                // Добавим зависимость
        //                dependencies.Add(new AliasCmd(iterationNames[i].ToString(), iterationCmds));
        //            }

        //            // По умолчанию будет задано минимальное значение звука
        //            dependencies.Add(iterationNames[0]);

        //            return new ParametrizedEntry<double[]>()
        //            {
        //                PrimaryKey = volumeRegulatorEntryKey,
        //                Cmd = volumeUp ? new SingleCmd(volumeUpCmd) : new SingleCmd(volumeDownCmd),
        //                Type = EntryType.Semistatic,
        //                IsMetaScript = false,
        //                Dependencies = dependencies,
        //                Arg = new double[] { minVolume, maxVolume, volumeStep }
        //            };
        //        },
        //        UpdateUI = (entry) =>
        //        {
        //            volumeVM.UpdateIsChecked(true);
        //            double[] args = ((IParametrizedEntry<double[]>)entry).Arg;

        //            volumeVM.From = args[0];
        //            volumeVM.To = args[1];
        //            volumeVM.Step = args[2];
        //            volumeVM.Mode = entry.Cmd.ToString() == "volume_up" ? 1 : 0;
        //        },
        //        Restore = () =>
        //        {
        //            volumeVM.UpdateIsChecked(false);
        //            volumeVM.Mode = 0;
        //        },
        //        HandleState = (state) => volumeVM.IsEnabled =
        //            state != EntryStateBinding.InvalidState && state != EntryStateBinding.Default
        //    });
        //}

        //public ICommand AddAliasCommand { get; private set; }
        //public ICommand DeleteAliasCommand { get; private set; }

        //void InitAliasController()
        //{
        //    // Предикат для проверки валидности ввода
        //    Predicate<string> aliasValidator = (alias) => Executable.IsValidAliasName(alias) 
        //        && aliasSetVM.Items.All(i => i.Text.Trim().ToLower() != alias.Trim().ToLower());
        //    // Скармливаем предикат в конструктор
        //    aliasSetVM = new AliasControllerViewModel(aliasValidator)
        //    {
        //        Key = "ExtraAliasSet",
        //        IsSelectable = false
        //    };
        //    aliasContentControl.Content = aliasSetVM;

        //    this.AddAliasCommand = new DelegateCommand(() =>
        //    {
        //        AddAlias(aliasSetVM.Input, new List<Entry>());
        //    });

        //    this.DeleteAliasCommand = new DelegateCommand(() =>
        //    {
        //        this.ResetAttachmentPanels();

        //        int selectedIndex = aliasSetVM.GetFirstSelectedIndex();
        //        aliasSetVM.Items.RemoveAt(selectedIndex);

        //        if (aliasSetVM.Items.Count > 0)
        //        {
        //            UpdateAttachmentPanels();

        //            // Так же для удобства сделаем фокус на первом элементе панели, если такой есть
        //            if (this.solidAttachmentsVM.Items.Count > 0)
        //            {
        //                string firstEntry = (string)(this.solidAttachmentsVM.Items[0].Tag);

        //                this.GetController(firstEntry).Focus();
        //            }

        //            // Если в панели еще остались алиасы, то обновим конфиг
        //            this.AddEntry(aliasSetVM.Key, false);
        //        }
        //        else
        //        {
        //            // Если удалили последнюю кнопку, то удалим 
        //            this.RemoveEntry(aliasSetVM.Key);
        //            this.SetStateAndUpdateUI(EntryStateBinding.InvalidState);
        //        }
        //    });

        //    void AddAlias(string name, List<Entry> attachedEntries)
        //    {
        //        ItemViewModel item = new ItemViewModel()
        //        {
        //            Text = name.Trim(),
        //            Tag = new List<Entry>()
        //        };

        //        aliasSetVM.Items.Add(item);

        //        if (this.StateBinding == EntryStateBinding.InvalidState)
        //            this.SetStateAndUpdateUI(EntryStateBinding.Alias);

        //        this.AddEntry(aliasSetVM.Key, false);
        //        UpdateAttachmentPanels();
        //    }

        //    this.entryV2Controllers.Add(new EntryController()
        //    {
        //        //Model = aliasSetVM,
        //        Generate = () =>
        //        {
        //            List<ParametrizedEntry<Entry[]>> aliases =
        //            new List<ParametrizedEntry<Entry[]>>();

        //            CommandCollection dependencies = new CommandCollection();

        //            foreach (ItemViewModel aliaselement in aliasSetVM.Items)
        //            {
        //                string aliasName = aliaselement.Text.ToString();
        //                List<Entry> attachedEntries = (List<Entry>)aliaselement.Tag;

        //                // Выпишем все зависимости, которые есть для текущего элемента
        //                foreach (Entry entry in attachedEntries)
        //                    foreach (Executable dependency in entry.Dependencies)
        //                        dependencies.Add(dependency);

        //                ParametrizedEntry<Entry[]> aliasEntry = new ParametrizedEntry<Entry[]>()
        //                {
        //                    PrimaryKey = "ExtraAlias",
        //                    Cmd = new SingleCmd(aliasName),
        //                    IsMetaScript = false,
        //                    Type = EntryType.Dynamic,
        //                    Arg = attachedEntries.ToArray()
        //                };

        //                AliasCmd alias = new AliasCmd(
        //                    aliaselement.Text,
        //                    attachedEntries.Select(e => e.Cmd));

        //                aliases.Add(aliasEntry);
        //                dependencies.Add(alias);
        //            }

        //            // сформируем итоговый элемент конфига
        //            return new ParametrizedEntry<Entry[]>()
        //            {
        //                PrimaryKey = aliasSetVM.Key,
        //                Cmd = null,
        //                IsMetaScript = false,
        //                Type = EntryType.Dynamic,
        //                Arg = aliases.ToArray(),
        //                Dependencies = dependencies
        //            };
        //        },
        //        UpdateUI = (entry) =>
        //        {
        //            ParametrizedEntry<Entry[]> extendedEntry = (ParametrizedEntry<Entry[]>)entry;

        //            Entry[] aliases = extendedEntry.Arg;

        //            foreach (Entry alias in aliases)
        //            {
        //                AddAlias(alias.Cmd.ToString(), (alias as ParametrizedEntry<Entry[]>).Arg.ToList());
        //            }
        //        },
        //        Restore = () =>
        //        {
        //            this.ResetAttachmentPanels();
        //            aliasSetVM.Items.Clear();
        //        },
        //        HandleState = (state) => { }
        //    });
        //}
        //#endregion

        //#region Framework
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
            KeySequence currentKeySequence = this.MainVM.KeySequence;
            var allEntries = this.MainVM.BoundEntries;

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
            if (currentKeySequence != null && currentKeySequence.Keys.Length == 1)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
