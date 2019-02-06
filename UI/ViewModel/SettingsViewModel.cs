﻿using System.Windows.Input;
using UI.Infrastructure;
using UI.View;

namespace UI.ViewModel
{
  public class SettingsViewModel : BaseViewModel
  {
    #region fields
    private SettingsView _window;
    private SettingsTabViewModel _selectedTab;
    #endregion

    #region properties
    public SettingsTabViewModel[] Tabs { get; private set; }

    public SettingsTabViewModel SelectedTab
    {
      get { return _selectedTab; }
      set { SetValue(value, nameof(SelectedTab), v => _selectedTab = v); }
    }
    #endregion

    #region commands
    public ICommand CloseSettingsCommand { get; private set; }
    #endregion

    #region constructors
    public SettingsViewModel(SettingsView view)
      : base(null, false)
    {
      _window = view;
      CloseSettingsCommand = new Command(CloseSettings);

      Tabs = new SettingsTabViewModel[] 
      {
        new ClientTabViewModel(),
        new ServerTabViewModel(),
        new AudioTabViewModel(),
        //new PluginSettingTabViewModel()
      };

      SelectedTab = Tabs[0];
    }
    #endregion

    #region methods
    private void CloseSettings(object obj)
    {
      foreach (var tab in Tabs)
      {
        tab.SaveSettings();
        tab.Dispose();
      }

      _window.Close();
    }
    #endregion
  }
}
