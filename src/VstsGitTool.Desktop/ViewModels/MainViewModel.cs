using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using VstsGitTool.Desktop.Command;
using VstsGitTool.Desktop.Configuration;
using VstsGitTool.VstsClient;
using VstsGitTool.VstsClient.Model;

namespace VstsGitTool.Desktop.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly VstsClientFactory _vstsClientFactory;
        private IEnumerable<VstsQuery> _queries;
        private VstsQuery _selectedQuery;
        private VstsWorkItem[] _workItems;
        private VstsWorkItem _selectedWorkItem;
        private VstsGitRepository[] _repositories;
        private VstsGitRepository _selectedRepository;
        private VstsGitBranch[] _branches;
        private VstsGitBranch _selectedBranch;
        private IEnumerable<VstsProject> _projects;
        private VstsProject _selectedRepositoriesProject;
        private VstsProject _selectedWorkItemsProject;
        private bool _isBusy;
        private CreateBranchViewModel _createBranchViewModel;
        private ConfirmationViewModel _confirmationViewModel;
        private ErrorViewModel _errorViewModel;

        public ICommand RefreshQueriesCommand { get; set; }
        public ICommand RefreshWorkItemsCommand { get; set; }
        public ICommand RefreshRepositoriesCommand { get; set; }
        public ICommand CreateBranchCommand { get; set; }
        public ICommand CancelCreateBranchCommand { get; set; }
        public ICommand ConfirmCreateBranchCommand { get; set; }
        public ICommand DeleteBranchCommand { get; set; }
        public ICommand LinkBranchToWorkItemCommand { get; set; }
        public ICommand RefreshBranchesCommand { get; set; }
        public ICommand ConfirmCommand { get; set; }
        public ICommand CloseErrorCommand { get; set; }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<VstsProject> Projects
        {
            get => _projects;
            private set
            {
                _projects = value;
                OnPropertyChanged();
            }
        }

        public VstsProject SelectedRepositoriesProject
        {
            get => _selectedRepositoriesProject;
            set
            {
                _selectedRepositoriesProject = value;
                OnPropertyChanged();
                if (value != null) UserConfiguration.DefaultRepositoriesProjectId = value.Id;
                RefreshRepos(null);
            }
        }

        public VstsProject SelectedWorkItemsProject
        {
            get => _selectedWorkItemsProject;
            set
            {
                _selectedWorkItemsProject = value;
                OnPropertyChanged();
                if (value != null) UserConfiguration.DefaultWorkItemsProjectId = value.Id;
                RefreshQueries(null);
            }
        }

        public IEnumerable<VstsQuery> Queries
        {
            get => _queries;
            private set
            {
                _queries = value;
                OnPropertyChanged();
            }
        }

        public VstsQuery SelectedQuery
        {
            get => _selectedQuery;
            set
            {
                _selectedQuery = value;
                OnPropertyChanged();

                RefreshWorkItems(null);
            }
        }

        public VstsWorkItem[] WorkItems
        {
            get => _workItems;
            set
            {
                _workItems = value;
                OnPropertyChanged();
            } 
        }

        public VstsWorkItem SelectedWorkItem
        {
            get => _selectedWorkItem;
            set
            {
                _selectedWorkItem = value;
                OnPropertyChanged();
            }
        }

        public VstsGitRepository[] Repositories
        {
            get => _repositories;
            set
            {
                _repositories = value;
                OnPropertyChanged();
            }
        }

        public VstsGitRepository SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                _selectedRepository = value;
                OnPropertyChanged();

                RefreshBranches(null);
            }
        }

        public VstsGitBranch[] Branches
        {
            get => _branches;
            set
            {
                _branches = value;
                OnPropertyChanged();
            }
        }

        public VstsGitBranch SelectedBranch
        {
            get => _selectedBranch;
            set
            {
                _selectedBranch = value;
                OnPropertyChanged();
            }
        }

        public CreateBranchViewModel CreateBranchViewModel
        {
            get => _createBranchViewModel;
            private set
            {
                _createBranchViewModel = value;
                OnPropertyChanged();
            }
        }

        public ConfirmationViewModel ConfirmationViewModel
        {
            get => _confirmationViewModel;
            private set
            {
                _confirmationViewModel = value;
                OnPropertyChanged();
            }
        }

        public ErrorViewModel ErrorViewModel
        {
            get => _errorViewModel;
            private set
            {
                _errorViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _vstsClientFactory = new VstsClientFactory(VstsConfiguration.CollectionUri, VstsConfiguration.AccessToken);

            RefreshQueriesCommand = new RelayCommand(RefreshQueries, o => SelectedWorkItemsProject != null);
            RefreshWorkItemsCommand = new RelayCommand(RefreshWorkItems, CanRefreshWorkItems);
            RefreshRepositoriesCommand = new RelayCommand(RefreshRepos, CanRefreshRepositories);
            CreateBranchCommand = new RelayCommand(CreateBranch, CanCreatebranch);
            DeleteBranchCommand = new RelayCommand(DeleteBranch, CanDeleteBranch);
            RefreshBranchesCommand = new RelayCommand(RefreshBranches, CanRefreshBranches);
            CancelCreateBranchCommand = new RelayCommand(o => CreateBranchViewModel = null, o => CreateBranchViewModel != null);
	        ConfirmCreateBranchCommand = new RelayCommand(PerformCreateBranch,
		        o => CreateBranchViewModel != null && !string.IsNullOrWhiteSpace(CreateBranchViewModel.BranchName) &&
		             CreateBranchViewModel.BasedOnBranch != null);
            LinkBranchToWorkItemCommand = new RelayCommand(LinkBranchToWorkItem, CanLinkBranchToWorkItem);
            ConfirmCommand = new RelayCommand(ActionConfirmation);
            CloseErrorCommand = new RelayCommand(o => ErrorViewModel = null, o => ErrorViewModel != null);

            RefreshProjects();
            RefreshQueriesAsync().ContinueWith(t => SelectDefaultQuery());
        }

        private void ActionConfirmation(object parameter)
        {
            var confirmationResult = parameter is ConfirmationResult result ? result : ConfirmationResult.Cancel;

            ConfirmationViewModel.PerformAction(confirmationResult);
        }

	    private void LinkBranchToWorkItem(object obj)
	    {
		    var selectedBranch = SelectedBranch;
		    var selectedRepository = SelectedRepository;
		    var selectedRepositoriesProject = SelectedRepositoriesProject;
		    var selectedWorkItem = SelectedWorkItem;
		    var selectedWorkItemsProject = SelectedWorkItemsProject;

		    var message =
			    $"Are you sure you want to link the branch '{selectedBranch.Name}' in repository '{selectedRepository.Name}' to work item {selectedWorkItem.Id} '{selectedWorkItem.Title}'?";

		    var confirmationViewModel = new ConfirmationViewModel("Link a branch to a work item", message,
			    ConfirmationButtons.YesNo,
			    confirmationResult =>
			    {
				    if (confirmationResult == ConfirmationResult.Yes)
				    {
					    PerformLinkBranchToWorkItem(selectedBranch, selectedRepository, selectedRepositoriesProject, selectedWorkItem,
						    selectedWorkItemsProject);
				    }
				    ConfirmationViewModel = null;
			    });

		    ConfirmationViewModel = confirmationViewModel;
	    }

	    private async void PerformLinkBranchToWorkItem(VstsGitBranch branch, VstsGitRepository repository,
		    VstsProject repositoriesProject, VstsWorkItem workItem, VstsProject workItemsProject)
	    {
		    await LinkBranchToWorkItemAsync(branch.Name, repository.Id, repositoriesProject.Id,
			    workItem.Id.Value, workItemsProject.Name);
	    }

	    private bool CanLinkBranchToWorkItem(object obj)
        {
            return SelectedBranch != null && SelectedRepository != null && SelectedRepositoriesProject != null &&
                   SelectedWorkItem != null && SelectedWorkItemsProject != null;
        }

        private async void RefreshProjects()
        {
            try
            {
                IsBusy = true;

                using (var projectsClient = _vstsClientFactory.CreateProjectClient())
                {
                    Projects = await projectsClient.GetProjects();
                }

                var defaultWorkItemsProjectId = UserConfiguration.DefaultWorkItemsProjectId;

                if (defaultWorkItemsProjectId != Guid.Empty)
                {
                    var defaultWorkItemsProject = Projects?.FirstOrDefault(p => p.Id.Equals(defaultWorkItemsProjectId));
                    if (defaultWorkItemsProject != null)
                    {
                        SelectedWorkItemsProject = defaultWorkItemsProject;
                    }
                }

                if (SelectedWorkItemsProject == null) SelectedWorkItemsProject = Projects?.FirstOrDefault();

                var defaultRepositoriesProjectId = UserConfiguration.DefaultRepositoriesProjectId;

                if (defaultRepositoriesProjectId != Guid.Empty)
                {
                    var defaultRepositoriesProject =
                        Projects?.FirstOrDefault(p => p.Id.Equals(defaultRepositoriesProjectId));
                    if (defaultRepositoriesProject != null)
                    {
                        SelectedRepositoriesProject = defaultRepositoriesProject;
                    }
                }

                if (SelectedRepositoriesProject == null) SelectedRepositoriesProject = Projects?.FirstOrDefault();
            }
            catch (Exception e)
            {
                ErrorViewModel = new ErrorViewModel(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void RefreshQueries(object obj)
        {
            await RefreshQueriesAsync();
        }

        private async Task RefreshQueriesAsync()
        {
            try
            {
                IsBusy = true;

                var selectedProject = SelectedWorkItemsProject;

                if (selectedProject == null)
                {
                    Queries = null;
                    IsBusy = false;
                    return;
                }

                using (var workItemClient = _vstsClientFactory.CreateWorkItemClient(selectedProject.Name))
                {
                    Queries = await workItemClient.GetQueries();
                }

            }
            catch (Exception e)
            {
                ErrorViewModel = new ErrorViewModel(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SelectDefaultQuery()
        {
            if (Queries != null)
            {
                var myQueriesFolder =
                    Queries.SingleOrDefault(q => q.Name.Equals("My Queries") && (q.IsFolder ?? false));

                var committedInSprintQuery =
                    myQueriesFolder?.Children?.SingleOrDefault(q =>
                        q.Name.Equals("Committed In Sprint") && !(q.IsFolder ?? false));

                if (committedInSprintQuery != null)
                {
                    SelectedQuery = committedInSprintQuery;
                }
            }
        }

        private async void RefreshWorkItems(object obj)
        {
            try
            {
                IsBusy = true;

                var selectedProject = SelectedWorkItemsProject;
                var selectedQuery = SelectedQuery;

                if (selectedProject == null)
                {
                    Queries = null;
                    WorkItems = null;
                    IsBusy = false;
                    return;
                }

                if (selectedQuery == null || selectedQuery.IsFolder == true)
                {
                    WorkItems = null;
                    IsBusy = false;
                    return;
                }

                using (var workItemClient = _vstsClientFactory.CreateWorkItemClient(selectedProject.Name))
                {
                    WorkItems = await workItemClient.GetWorkItems(selectedQuery.Id);
                }
            }
            catch (Exception e)
            {
                ErrorViewModel = new ErrorViewModel(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRefreshWorkItems(object obj)
        {
            return SelectedQuery != null && !(SelectedQuery.IsFolder ?? false);
        }

        private async void RefreshRepos(object obj)
        {
            try
            {
                IsBusy = true;

                var project = SelectedRepositoriesProject;

                if (project == null)
                {
                    Repositories = null;
                    return;
                }

                using (var gitClient = _vstsClientFactory.CreateGitClient(project.Name))
                {
                    Repositories = await gitClient.GetRepos();
                }
            }
            catch (Exception e)
            {
                ErrorViewModel = new ErrorViewModel(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRefreshRepositories(object obj)
        {
            return SelectedRepositoriesProject != null;
        }

        private async void RefreshBranches(object obj)
        {
            try
            {
                IsBusy = true;

                var selectedProject = SelectedRepositoriesProject;
                var selectedRepository = SelectedRepository;

                if (selectedProject == null || selectedRepository == null)
                {
                    Branches = null;
                    IsBusy = false;
                    return;
                }

                using (var gitClient = _vstsClientFactory.CreateGitClient(selectedProject.Name))
                {
                    Branches = await gitClient.GetBranches(selectedRepository.Id);
                }

            }
            catch (Exception e)
            {
                ErrorViewModel = new ErrorViewModel(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRefreshBranches(object obj)
        {
            return SelectedRepository != null;
        }

        private void CreateBranch(object param)
        {
			var createBranchViewModel =
                new CreateBranchViewModel(Branches, SelectedWorkItem, SelectedRepository);

            CreateBranchViewModel = createBranchViewModel;
        }

        private async void PerformCreateBranch(object param)
        {
            try
            {
                IsBusy = true;

                var selectedWorkItemProject = SelectedWorkItemsProject;
                var selectedRepoProject = SelectedRepositoriesProject;
                var workItem = CreateBranchViewModel.WorkItem;
                var repository = SelectedRepository;

                if (selectedWorkItemProject == null)
                {
                    IsBusy = false;
                    throw new Exception("No work item project selected");
                }

                if (selectedRepoProject == null)
                {
                    IsBusy = false;
                    throw new Exception("No repository project selected");
                }

                if (repository == null)
                {
                    IsBusy = false;
                    throw new Exception("No repository selected");
                }

                using (var gitClient = _vstsClientFactory.CreateGitClient(selectedRepoProject.Name))
                {
                    await gitClient.CreateBranch(CreateBranchViewModel.FullBranchName,
                        CreateBranchViewModel.BasedOnBranch.Name, repository.Id);
                }

                RefreshBranches(null);

                if (CreateBranchViewModel.LinkToWorkItem && workItem != null)
                {
                    await LinkBranchToWorkItemAsync(CreateBranchViewModel.FullBranchName, repository.Id,
                        selectedRepoProject.Id,
                        workItem.Id.Value, selectedWorkItemProject.Name);
                }

                CreateBranchViewModel = null;
            }
            catch (Exception e)
            {
                ErrorViewModel = new ErrorViewModel(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LinkBranchToWorkItemAsync(string branchName, Guid repositoryId, Guid repositoryProjectId,
            int workItemId, string workItemProjectName)
        {
            using (var workItemClient = _vstsClientFactory.CreateWorkItemClient(workItemProjectName))
            {
                await workItemClient.AddBranchLink(workItemId, repositoryProjectId, repositoryId,
                    branchName.Replace("refs/heads/", string.Empty));
            }

            RefreshWorkItems(null);
        }

        private void DeleteBranch(object obj)
        {
            var selectedBranch = SelectedBranch;
            var selectedRepository = SelectedRepository;
            var selectedRepositoriesProject = SelectedRepositoriesProject;

            var message =
                $"Are you sure you want to delete the branch '{selectedBranch.Name}' in repository '{selectedRepository.Name}' on the remote?";

            var confirmationViewModel = new ConfirmationViewModel("Delete a branch", message, ConfirmationButtons.YesNo,
                confirmationResult =>
                {
                    if (confirmationResult == ConfirmationResult.Yes)
                    {
                        PerformDeleteBranchAsync(selectedBranch, selectedRepository, selectedRepositoriesProject);
                    }
                    ConfirmationViewModel = null;
                });

            ConfirmationViewModel = confirmationViewModel;
        }

        private async void PerformDeleteBranchAsync(VstsGitBranch branch, VstsGitRepository repository, VstsProject repoProject)
        {
            IsBusy = true;

            var branchName = branch.Name;

            if (repository == null)
            {
                IsBusy = false;
                throw new Exception("No repository selected");
            }

            using (var gitClient = _vstsClientFactory.CreateGitClient(repoProject.Name))
            {
                await gitClient.DeleteBranch(branchName, repository.Id);
            }

            RefreshBranches(null);
            RefreshWorkItems(null);

            IsBusy = false;
        }

        private bool CanDeleteBranch(object obj)
        {
            return SelectedBranch != null;
        }

        private bool CanCreatebranch(object obj)
        {
            return SelectedRepository != null;
        }

        private void CreatePullRequest()
        {
            
        }
    }
}
