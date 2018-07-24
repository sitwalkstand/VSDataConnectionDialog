using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Principal;

namespace VSDataConnectionDialog
{
    public partial class VSDataConnectionDialog : Form
    {
        public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }

        internal class Properties : INotifyPropertyChanged
        {
            internal class Tags
            {
                public static readonly string DataSource = "DataSource";
                public static readonly string InitialCatalog = "InitialCatalog";
                public static readonly string InitialCatalogValid = "InitialCatalogValid";
                public static readonly string UserName = "UserName";
                public static readonly string Password = "Password";
                public static readonly string IntegratedSecurity = "IntegratedSecurity";
                public static readonly string DataSourceValid = "DataSourceValid";
                public static readonly string UserNameEnabled = "UserNameEnabled";
                public static readonly string WindowsAuthentication = "Windows Authentication";
                public static readonly string SQLServerAuthentication = "SQL Server Authentication";
                public static readonly string AuthenticationMode = "AuthenticationMode";
                public static readonly string TestingEnabled = "TestingEnabled";
                public static readonly string TestResult = "TestResult";
            };

            public event PropertyChangedEventHandler PropertyChanged;

            public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }

            public string DataSource
            {
                get { return ConnectionStringBuilder.DataSource; }
                set
                {
                    ConnectionStringBuilder.DataSource = value;
                    NotifyPropertyChanged(Tags.DataSource);
                    NotifyPropertyChanged(Tags.DataSourceValid);
                    NotifyPropertyChanged(Tags.TestingEnabled);
                    TestResult = String.Empty;
                }
            }

            public string InitialCatalog
            {
                get { return ConnectionStringBuilder.InitialCatalog; }
                set
                {
                    ConnectionStringBuilder.InitialCatalog = value;
                    NotifyPropertyChanged(Tags.InitialCatalog);
                    NotifyPropertyChanged(Tags.InitialCatalogValid);
                    NotifyPropertyChanged(Tags.TestingEnabled);
                    TestResult = String.Empty;

                }
            }
            public bool DataSourceValid
            {
                get { return !string.IsNullOrEmpty(DataSource); }
            }

            public bool InitialCatalogValid
            {
                get { return !string.IsNullOrEmpty(InitialCatalog); }
            }

            public bool IntegratedSecurity
            {
                get { return ConnectionStringBuilder.IntegratedSecurity; }
                set
                {
                    if (value != ConnectionStringBuilder.IntegratedSecurity)
                    {
                        ConnectionStringBuilder.IntegratedSecurity = value;
                        NotifyPropertyChanged(Tags.IntegratedSecurity);
                        NotifyPropertyChanged(Tags.UserNameEnabled);
                        NotifyPropertyChanged(Tags.AuthenticationMode);
                        TestResult = String.Empty;
                    }
                }
            }

            public string UserName
            {
                get { return IntegratedSecurity ? WindowsUserName : ConnectionStringBuilder.UserID; }
                set
                {
                    ConnectionStringBuilder.UserID = value;
                    NotifyPropertyChanged(Tags.UserName);
                    TestResult = String.Empty;
                }
            }

            public string Password
            {
                get { return IntegratedSecurity ? String.Empty : ConnectionStringBuilder.Password; }
                set
                {
                    ConnectionStringBuilder.Password = value;
                    NotifyPropertyChanged(Tags.Password);
                    TestResult = String.Empty;
                }
            }

            public bool UserNameEnabled
            {
                get { return !IntegratedSecurity; }
            }

            public string AuthenticationMode
            {
                get { return IntegratedSecurity ? Tags.WindowsAuthentication : Tags.SQLServerAuthentication; }
                set
                {
                    IntegratedSecurity = (value == Tags.WindowsAuthentication);
                }
            }

            private bool _isTesting = false;

            public bool IsTesting
            {
                get { return _isTesting; }
                set
                {
                    _isTesting = value;
                    NotifyPropertyChanged(Tags.TestingEnabled);
                }
            }

            public bool TestingEnabled
            {
                get { return DataSourceValid && !IsTesting; }
            }

            private string _testResult = "";

            public string TestResult
            {
                get { return _testResult; }
                set
                {
                    _testResult = value;
                    NotifyPropertyChanged(Tags.TestResult);
                }
            }

            internal string WindowsUserName { get; set; }

            internal void NotifyPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            public Properties(SqlConnectionStringBuilder scsb)
            {
                ConnectionStringBuilder = scsb;
                WindowsUserName = WindowsIdentity.GetCurrent().Name;
            }
        }

        private Properties FormProperties { get; set; }

        public VSDataConnectionDialog(SqlConnectionStringBuilder connectionStringBuilder)
        {
            InitializeComponent();

            authenticationMode.Items.Add(Properties.Tags.WindowsAuthentication);
            authenticationMode.Items.Add(Properties.Tags.SQLServerAuthentication);

            ConnectionStringBuilder = connectionStringBuilder ?? new SqlConnectionStringBuilder()
            {
                IntegratedSecurity = true,
                DataSource = "."
            };

            FormProperties = new Properties(ConnectionStringBuilder);

            serverName.DataBindings.Add("Text", FormProperties, Properties.Tags.DataSource, false, DataSourceUpdateMode.OnPropertyChanged);
            initialCatalogName.DataBindings.Add("Text", FormProperties, Properties.Tags.InitialCatalog, false, DataSourceUpdateMode.OnPropertyChanged);
            userName.DataBindings.Add("Text", FormProperties, Properties.Tags.UserName);
            userPassword.DataBindings.Add("Text", FormProperties, Properties.Tags.Password);

            authenticationMode.DataBindings.Add("Text", FormProperties, Properties.Tags.AuthenticationMode, false, DataSourceUpdateMode.OnPropertyChanged);

            userName.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);
            userPassword.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);
            lblUserName.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);
            lblPassword.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);

            btnConnect.DataBindings.Add("Enabled", FormProperties, Properties.Tags.DataSourceValid);
            btnTest.DataBindings.Add("Enabled", FormProperties, Properties.Tags.TestingEnabled);

            lblStatus.DataBindings.Add("Text", FormProperties, Properties.Tags.TestResult);
        }

        internal void TestConnection()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    FormProperties.IsTesting = true;
                    FormProperties.TestResult = "Testing...";
                    conn.Open();
                    FormProperties.TestResult = "Success!";
                }
                catch (Exception ex)
                {
                    FormProperties.TestResult = ex.Message;
                }
                finally
                {
                    FormProperties.IsTesting = false;
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            TestConnection();
        }
    }
}
