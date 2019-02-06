﻿using Engine.Exceptions;
using Engine.Model.Client;
using Engine.Model.Common.Entities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using UI.Infrastructure;
using Keys = System.Windows.Forms.Keys;

namespace UI.ViewModel
{
  public class AudioTabViewModel : SettingsTabViewModel
  {
    private const string NameKey = "settingsTabCategory-audio";
    private const string PressTheKeyKey = "settingsTab-audio-pressTheKey";

    #region fields
    private IList<string> _outputDevices;
    private IList<string> _inputDevices;
    private IList<AudioQuality> _inputConfigs;
    private int _selectedInputIndex;
    private int _selectedOutputIndex;
    private int _selectedConfigIndex;
    private string _selectButtonName;
    private volatile bool _selectingKey;
    #endregion

    #region properties
    public IList<string> OutputDevices
    {
      get { return _outputDevices; }
      set { SetValue(value, nameof(OutputDevices), v => _outputDevices = v); }
    }

    public IList<string> InputDevices
    {
      get { return _inputDevices; }
      set { SetValue(value, nameof(InputDevices), v => _inputDevices = v); }
    }

    public IList<AudioQuality> InputConfigs
    {
      get { return _inputConfigs; }
      set { SetValue(value, nameof(InputConfigs), v => _inputConfigs = v); }
    }

    public int SelectedInputIndex
    {
      get { return _selectedInputIndex; }
      set { SetValue(value, nameof(SelectedInputIndex), v => _selectedInputIndex = v); }
    }

    public int SelectedConfigIndex
    {
      get { return _selectedConfigIndex; }
      set { SetValue(value, nameof(SelectedConfigIndex), v => _selectedConfigIndex = v); }
    }

    public int SelectedOutputIndex
    {
      get { return _selectedOutputIndex; }
      set { SetValue(value, nameof(SelectedOutputIndex), v => _selectedOutputIndex = v); }
    }

    public string SelectButtonName
    {
      get { return _selectButtonName; }
      set { SetValue(value, nameof(SelectButtonName), v => _selectButtonName = v); }
    }
    #endregion

    #region commands

    public ICommand SelectKeyCommand { get; private set; }

    #endregion

    public AudioTabViewModel()
      : base(NameKey, SettingsTabCategory.Audio)
    {
      OutputDevices = ClientModel.Player.Devices;
      InputDevices = ClientModel.Recorder.Devices;
      InputConfigs = new[]
      {
        new AudioQuality(1, 8, 22050),
        new AudioQuality(1, 16, 22050),
        new AudioQuality(1, 8, 44100),
        new AudioQuality(1, 16, 44100)
      };

      SelectedOutputIndex = OutputDevices.IndexOf(Settings.Current.OutputAudioDevice);
      SelectedInputIndex = InputDevices.IndexOf(Settings.Current.InputAudioDevice);
      SelectedConfigIndex = InputConfigs.IndexOf(new AudioQuality(1, Settings.Current.Bits, Settings.Current.Frequency));

      if (SelectedOutputIndex == -1)
        SelectedOutputIndex = 0;

      if (SelectedInputIndex == -1)
        SelectedInputIndex = 0;

      if (SelectedConfigIndex == -1)
        SelectedConfigIndex = 0;

      SelectButtonName = Settings.Current.RecorderKey.ToString();

      SelectKeyCommand = new Command(SelectKey);
    }

    private void SelectKey(object obj)
    {
      if (_selectingKey == true)
        return;

      _selectingKey = true;
      SelectButtonName = Localizer.Instance.Localize(PressTheKeyKey);
      KeyBoard.KeyDown += KeyDown;
    }

    private void KeyDown(Keys key)
    {
      if (!_selectingKey)
      {
        KeyBoard.KeyDown -= KeyDown;
        return;
      }

      _selectingKey = false;
      SelectButtonName = key.ToString();
    }

    public override void SaveSettings()
    {
      if (InputConfigs.Count > 0 && SelectedConfigIndex >= 0 && SelectedConfigIndex < InputConfigs.Count)
      {
        AudioQuality selected = InputConfigs[SelectedConfigIndex];
        Settings.Current.Frequency = selected.Frequency;
        Settings.Current.Bits = selected.Bits;
      }

      if (OutputDevices.Count > 0 && SelectedOutputIndex >= 0 && SelectedOutputIndex < OutputDevices.Count)
        Settings.Current.OutputAudioDevice = OutputDevices[SelectedOutputIndex];

      if (InputDevices.Count > 0 && SelectedInputIndex >= 0 && SelectedInputIndex < InputDevices.Count)
        Settings.Current.InputAudioDevice = InputDevices[_selectedInputIndex];

      if (!string.Equals(Localizer.Instance.Localize(PressTheKeyKey), SelectButtonName))
        Settings.Current.RecorderKey = (Keys)Enum.Parse(typeof(Keys), SelectButtonName);

      try
      {
        ClientModel.Recorder.SetOptions(Settings.Current.InputAudioDevice, InputConfigs[SelectedConfigIndex]);
        ClientModel.Player.SetOptions(Settings.Current.OutputAudioDevice);
      }
      catch (ModelException me)
      {
        ClientModel.Player.Dispose();
        ClientModel.Recorder.Dispose();

        if (me.Code != ErrorCode.AudioNotEnabled)
          throw;
        else
        {
          var msg = Localizer.Instance.Localize(MainViewModel.AudioInitializationFailedKey);
          MessageBox.Show(msg, MainViewModel.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
      }
    }
  }
}
