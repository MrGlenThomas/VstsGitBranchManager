﻿using System;

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
    }
}
