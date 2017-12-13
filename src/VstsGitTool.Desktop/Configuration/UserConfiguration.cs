using System;

namespace VstsGitTool.Desktop.Configuration
{
	using System.Configuration;

	public class UserConfiguration
    {
        public static Guid DefaultWorkItemsProjectId
        {
            get => Properties.Settings.Default.DefaultWorkItemsProjectId;
            set
            {
                Properties.Settings.Default.DefaultWorkItemsProjectId = value;
                Properties.Settings.Default.Save();
            }
        }

        public static Guid DefaultRepositoriesProjectId
        {
            get => Properties.Settings.Default.DefaultRepositoriesProjectId;
            set
            {
                Properties.Settings.Default.DefaultRepositoriesProjectId = value;
                Properties.Settings.Default.Save();
            }
        }

	    public static string BranchNameFormatString => ConfigurationManager.AppSettings["BranchNameFormatString"];

	    public static int MaxBranchNameLength
	    {
		    get
		    {
				var settingValue = ConfigurationManager.AppSettings["MaxBranchNameLength"];

			    var maxBranchNameLength = -1;

			    int.TryParse(settingValue, out maxBranchNameLength);

			    return maxBranchNameLength;
		    }
	    }

	    public static string BugWorkItemDefaultBranchGroup => ConfigurationManager.AppSettings["BugWorkItemDefaultBranchGroup"];

	    public static string BugWorkItemDefaultSourceBranch => ConfigurationManager.AppSettings["BugWorkItemDefaultSourceBranch"];
	}
}
