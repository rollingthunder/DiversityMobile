﻿namespace DiversityPhone.Model
{
    public static class AppSettingsMixin
    {
        public static UserCredentials ToCreds(this AppSettings settings)
        {
            if (settings == null)
            {
                return null;
            }
            else
            {
                return new UserCredentials()
                {
                    AgentName = settings.AgentName,
                    AgentURI = settings.AgentURI,
                    LoginName = settings.UserName,
                    Password = settings.Password,
                    ProjectID = settings.CurrentProject,
                    Repository = settings.HomeDBName,
                };
            }
        }
    }

    public class AppSettings
    {
        public AppSettings()
        {
            UseGPS = true;
            SaveMultimediaExternally = true;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string AgentName { get; set; }
        public string AgentURI { get; set; }
        public bool UseGPS { get; set; }

        public bool SaveMultimediaExternally { get; set; }

        public string HomeDBName { get; set; }

        public int CurrentProject { get; set; }
        public string CurrentProjectName { get; set; }
        public int? CurrentSeriesID { get; set; }

        public AppSettings Clone()
        {
            return (AppSettings)this.MemberwiseClone();
        }

    }


}
