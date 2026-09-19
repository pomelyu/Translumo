using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public bool IsCustomProvider => LlmProviderDescriptor.Get(Model.Provider).RequiresServerUrl;

        public ICommand RestoreDefaultPromptCommand => new RelayCommand(OnRestoreDefaultPrompt);

        public ICommand RefreshModelsCommand => new RelayCommand(OnRefreshModels);

        private int _modelsRequestVersion;

        public LlmSettingsViewModel(LlmTranslationConfiguration configuration)
        {
            this.Model = configuration;
            this.Model.PropertyChanged += ModelOnPropertyChanged;

            RefreshAvailableModels();
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Model.Provider):
                    OnPropertyChanged(nameof(IsCustomProvider));
                    RefreshAvailableModels();
                    if (!IsCustomProvider)
                    {
                        Model.Model = LlmProviderDescriptor.Get(Model.Provider).PresetModels[0];
                    }
                    break;
                case nameof(Model.ServerUrl):
                    if (IsCustomProvider)
                    {
                        RefreshAvailableModels();
                    }
                    break;
            }
        }

        private void RefreshAvailableModels()
        {
            if (IsCustomProvider)
            {
                FetchServerModelsAsync();
                return;
            }

            AvailableModels.Clear();
            foreach (var model in LlmProviderDescriptor.Get(Model.Provider).PresetModels)
            {
                AvailableModels.Add(model);
            }
        }

        private async void FetchServerModelsAsync()
        {
            var requestVersion = ++_modelsRequestVersion;
            var models = await LlmModelsClient.GetAvailableModelsAsync(Model.ServerUrl, Model.ApiKey);
            if (requestVersion != _modelsRequestVersion || !IsCustomProvider)
            {
                return;
            }

            AvailableModels.Clear();
            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }

            if (models.Any() && (string.IsNullOrWhiteSpace(Model.Model) || !models.Contains(Model.Model)))
            {
                Model.Model = models[0];
            }
        }

        private void OnRefreshModels()
        {
            RefreshAvailableModels();
        }

        private void OnRestoreDefaultPrompt()
        {
            Model.SystemPromptTemplate = LlmTranslationConfiguration.DEFAULT_SYSTEM_PROMPT;
        }
    }
}
