# VSTS Git Branch Manager
Application for managing Git branches in Visual Studio Team Services

Enables generating branch names automatically based on work item attributes and automatically setting source branch based on work item type.
The VSTS web application does not support creating branches from work items located in different VSTS projects, but this is achieved through the VSTS API.
Branches created can also be linked to selected work items.

## Configuration
Configure application settings in App.config appSettings

#### AccessToken.
Generate an access token from your VSTS account security. The access token will restrict API data to what the related account can access.

#### CollectionUri.
The URL of your VSTS system. e.g. https://mycompany.visualstudio.com/

#### BranchNameFormatString
A format string for generating branch names from work items.
Supported tokens:
  *{workItemId}
  *{workItemTitle}
  *{workItemType}
  *{workItemAssignedTo}
  
#### MaxBranchNameLength
The maximum length for generated branch names. -1 = infinite length.

#### BugWorkItemDefaultBranchGroup
The default branch group to use for Bug work items when creating new branches. By default a Bug work item is assumed to be a hotfix.

#### BugWorkItemDefaultSourceBranch
The default source branch to use for Bug work items. By default the master branch is used.
