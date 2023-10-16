using Angor.Client.Services;
using Angor.Shared.Models;
using Blazored.SessionStorage;

namespace Angor.Client.Storage;

public class LocalSessionStorage : ISessionStorage
{
    private ISyncSessionStorageService _sessionStorageService;

    private const string NostrStreamSubscriptions = "subscriptions";

    public LocalSessionStorage(ISyncSessionStorageService sessionStorageService)
    {
        _sessionStorageService = sessionStorageService;
    }

    public void StoreProjectInfo(ProjectInfo project)
    {
        _sessionStorageService.SetItem(project.ProjectIdentifier,project);
    }

    public void AddProjectToSubscribedList(string nostrPubKey)
    {
        var list = _sessionStorageService.GetItem<List<string>>(NostrStreamSubscriptions) ?? new List<string>();

        list.Add(nostrPubKey);

        _sessionStorageService.SetItem(NostrStreamSubscriptions, list);
    }

    public bool IsProjectInSubscribedList(string nostrPubKey)
    {
        var list = _sessionStorageService.GetItem<List<string>>(NostrStreamSubscriptions) ?? new List<string>();

        return list.Contains(nostrPubKey);
    }

    public ProjectInfo? GetProjectById(string projectId)
    {
        return _sessionStorageService.GetItem<ProjectInfo>(projectId);
    }
}