using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Translumo.Translation.Configuration;
using Translumo.Translation.Llm;
using Translumo.Utils;
using RelayCommand = Translumo.MVVM.Common.RelayCommand;

namespace Translumo.MVVM.ViewModels
{
    public sealed class LlmSettingsViewModel : BindableBase
    {
        public LlmTranslationConfiguration Model { get; set; }

        public LlmProviders[] AvailableProviders { get; } = (LlmProviders[])Enum.GetValues(typeof(LlmProviders));

        public ObservableCollection<string> AvailableModels { get; } = new ObservableCollection<string>();

        public ICommand RestoreDefaultPromptCommand => new RelayCommand(OnRestoreDefaultPrompt);

        public LlmSettingsViewModel(LlmTranslationConfiguration configuration)
        {
            this.Model = configuration;
            this.Model.PropertyChanged += ModelOnPropertyChanged;

            RefreshAvailableModels();
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.Provider))
            {
                RefreshAvailableModels();
                Model.Model = LlmProviderDescriptor.Get(Model.Provider).PresetModels[0];
            }
        }

        private void RefreshAvailableModels()
        {
            AvailableModels.Clear();
            foreach (var model in LlmProviderDescriptor.Get(Model.Provider).PresetModels)
            {
                AvailableModels.Add(model);
            }
        }

        private void OnRestoreDefaultPrompt()
        {
            Model.SystemPromptTemplate = LlmTranslationConfiguration.DEFAULT_SYSTEM_PROMPT;
        }
    }
}
