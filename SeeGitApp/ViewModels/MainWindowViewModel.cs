﻿using System;
using System.IO;
using System.Windows.Threading;

namespace SeeGit
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        private RepositoryGraph _graph;

        private IRepositoryGraphBuilder _graphBuilder;
        // Tree, LinLog, KK, ISOM, EfficientSugiyama, FR, CompoundFDP, BoundedFR, Circular

        private string _layoutAlgorithmType = "Tree";
        private readonly Dispatcher _uiDispatcher;
        private readonly Func<string, IRepositoryGraphBuilder> _graphBuilderThunk;
        private IDisposable _repositoryCreationWatcher;

        public MainWindowViewModel(Dispatcher uiDispatcher, Func<string, IRepositoryGraphBuilder> graphBuilderThunk)
        {
            _uiDispatcher = uiDispatcher;
            _graphBuilderThunk = graphBuilderThunk ?? (path => new RepositoryGraphBuilder(path));
        }

        public string LayoutAlgorithmType
        {
            get { return _layoutAlgorithmType; }
            set
            {
                _layoutAlgorithmType = value;
                RaisePropertyChanged(() => LayoutAlgorithmType);
            }
        }

        public RepositoryGraph Graph
        {
            get { return _graph; }
            set
            {
                _graph = value;
                LayoutAlgorithmType = _graph.LayoutAlgorithmType;
                RaisePropertyChanged(() => Graph);
            }
        }

        public string RepositoryPath { get; private set; }

        public void MonitorRepository(string repositoryWorkingPath)
        {
            if (repositoryWorkingPath == null) return;

            // If the user has previously selected a directory for monitoring lets cancel that subscription
            var repositoryCreationWatcher = _repositoryCreationWatcher;
            if (repositoryCreationWatcher != null)
            {
                repositoryCreationWatcher.Dispose();
                _repositoryCreationWatcher = null;
            }

            string gitPath = repositoryWorkingPath.NormalizeGitRepositoryPath();
            if (!Directory.Exists(gitPath))
            {
                MonitorForRepositoryCreation(repositoryWorkingPath);
                return;
            }

            _graphBuilder = _graphBuilderThunk(gitPath);
            Graph = _graphBuilder.Graph();
            LayoutAlgorithmType = "Tree";

            MonitorForRepositoryChanges(gitPath);
        }

        private void MonitorForRepositoryCreation(string repositoryWorkingPath)
        {
            _repositoryCreationWatcher = ModelExtensions.CreateGitRepositoryCreationObservable(repositoryWorkingPath)
                .Subscribe(_ => _uiDispatcher.Invoke(new Action(() => MonitorRepository(repositoryWorkingPath))));
        }

        private void MonitorForRepositoryChanges(string gitRepositoryPath)
        {
            ModelExtensions.CreateGitRepositoryChangesObservable(gitRepositoryPath)
                .Subscribe(_ => _uiDispatcher.Invoke(new Action(Refresh)));
        }

        public void Refresh()
        {
            Graph = _graphBuilder.Graph();
        }
    }
}