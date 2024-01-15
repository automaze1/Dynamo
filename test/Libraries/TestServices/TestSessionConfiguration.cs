using System.Configuration;
using System.IO;
using System.Reflection;

using System.Collections.Generic;
using System;

namespace TestServices
{
    /// <summary>
    /// The TestSessionConfiguration class is responsible for 
    /// reading the TestServices configuration file, if provided,
    /// to provide data for configuring a test session from a location
    /// other than Dynamo's core directory.
    /// </summary>
    public class TestSessionConfiguration
    {
        private const string CONFIG_FILE_NAME = "TestServices.dll.config";
        private  List<Version> supportedLibGVersions = new List<Version>
                {
                    new Version(224,4,0),
                    new Version(224,0,1),
                    new Version(223,0,1),
                    new Version(222,0,0),
                    new Version(221,0,0)
                };

        public string DynamoCorePath { get; private set; }
        /// <summary>
        /// The requested libG library version as a full system.version string.
        /// If the key is not present in the config file a default value will be selected.
        /// </summary>
        public Version RequestedLibraryVersion2 { get; private set; }
  
        /// <summary>
        /// This constructor does not read configuration from a config file, the configuration properties are
        /// set directly by the parameters passed to this constructor. 
        /// It can be used by test fixtures that know which libG version should be loaded.
        /// </summary>
        /// <param name="dynamoCoreDirectory"></param>
        /// <param name="requestedVersion"></param>
        public TestSessionConfiguration(string dynamoCoreDirectory, Version requestedVersion)
        {
            DynamoCorePath = dynamoCoreDirectory;
            RequestedLibraryVersion2 = requestedVersion;
        }

        public TestSessionConfiguration()
            : this(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)){}

        /// <summary>
        /// A TestSessionConfiguration specifies test session configuration properties
        /// for a test.
        /// </summary>
        /// <param name="dynamoCoreDirectory">The location of Dynamo's core directory. If the config file
        /// contains a value for DynamoBasePath, then this parameter will be ignored.</param>
        /// <param name="configFileDirectory">The location of the directory containing the TestServices.dll.config file.
        /// If this parameter is null, the value of dynamoCoreDirectory will be used.</param>
        public TestSessionConfiguration(string dynamoCoreDirectory, string configFileDirectory = null)
        {
            string configPath;
            if (configFileDirectory == null || !Directory.Exists(configFileDirectory))
            {
                configPath = Path.Combine(dynamoCoreDirectory, CONFIG_FILE_NAME);
            }
            else
            {
                configPath = Path.Combine(configFileDirectory, CONFIG_FILE_NAME);
            }

            // If a config file cannot be found, fall back to 
            // using the executing assembly's directory for core
            // and version 219 for the shape manager.
            if (!File.Exists(configPath))
            {
                DynamoCorePath = dynamoCoreDirectory;

                return;
            }

            // Adjust the config file map because the config file
            // might not always be in the same directory as the dll.
            var map = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            var dir = GetAppSetting(config, "DynamoBasePath");
            DynamoCorePath = string.IsNullOrEmpty(dir) ? dynamoCoreDirectory : dir;
        }

        private static string GetAppSetting(Configuration config, string key)
        {
            var element = config.AppSettings.Settings[key];
            if (element == null) return string.Empty;

            var value = element.Value;
            return !string.IsNullOrEmpty(value) ? value : string.Empty;
        }
    }
}
