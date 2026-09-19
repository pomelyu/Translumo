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

        public ObservableCollection<string> AvailableModels
        {
            get => _availableModels;
            private set => SetProperty(ref _availableModels, value);
        }

        private ObservableCollection<string> _availableModels = new ObservableCollection<string>();

        public string ModelsStatus
        {
            get => _modelsStatus;
            private set => SetProperty(ref _modelsStatus, value);
        }

        private string _modelsStatus;

        public bool IsRefreshingModels
        {
            get => _isRefreshingModels;
            private set
            {
                SetProperty(ref _isRefreshingModels, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _isRefreshingModels;

        public bool IsCustomProvider => LlmProviderDescriptor.Get(Model.Provider).RequiresServerUrl;

        public ICommand RestoreDefaultPromptCommand => new RelayCommand(OnRestoreDefaultPrompt);

        public ICommand RefreshModelsCommand => new RelayCommand(OnRefreshModels, () => !IsRefreshingModels);

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
                    if (!IsCustomProvider)
                    {
                        Model.Model = LlmProviderDescriptor.Get(Model.Provider).PresetModels[0];
                    }
                    else
                    {
                        Model.Model = string.Empty;
                    }
                    RefreshAvailableModels();
                    break;
                case nameof(Model.ApiKey):
                    RefreshAvailableModels();
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
            // Invalidate requests even when the new settings cannot yet be queried.
            ++_modelsRequestVersion;
            IsRefreshingModels = false;
            ModelsStatus = string.Empty;
            var selectedModel = Model.Model;
            AvailableModels = new ObservableCollection<string>(string.IsNullOrWhiteSpace(selectedModel)
                ? Array.Empty<string>() : new[] { selectedModel });
            Model.Model = selectedModel;

            if (IsCustomProvider ? !string.IsNullOrWhiteSpace(Model.ServerUrl) : !string.IsNullOrWhiteSpace(Model.ApiKey))
                FetchServerModelsAsync();
        }

        private async void FetchServerModelsAsync()
        {
            var requestVersion = ++_modelsRequestVersion;
            IsRefreshingModels = true;
            ModelsStatus = "Loading models...";
            try
            {
                var models = await LlmModelsClient.GetAvailableModelsAsync(Model.Provider, Model.ServerUrl, Model.ApiKey);
                if (requestVersion != _modelsRequestVersion)
                    return;

                // Replacing the collection avoids transient selection changes while clearing it.
                var selectedModel = Model.Model;
                AvailableModels = new ObservableCollection<string>(models);
                Model.Model = string.IsNullOrWhiteSpace(selectedModel) ? models.FirstOrDefault() ?? string.Empty : selectedModel;
                ModelsStatus = models.Count == 0 ? "No models returned." : $"Loaded {models.Count} models.";
            }
            catch (Exception ex)
            {
                if (requestVersion == _modelsRequestVersion)
                    ModelsStatus = ex is InvalidOperationException ? ex.Message : "Unable to retrieve models. Check your connection and settings.";
            }
            finally
            {
                if (requestVersion == _modelsRequestVersion)
                    IsRefreshingModels = false;
            }
        }

        private void OnRefreshModels()
        {
            FetchServerModelsAsync();
        }

        private void OnRestoreDefaultPrompt()
        {
            Model.SystemPromptTemplate = LlmTranslationConfiguration.DEFAULT_SYSTEM_PROMPT;
        }
    }
}
