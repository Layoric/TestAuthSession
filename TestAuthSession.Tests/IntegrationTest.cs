using Funq;
using ServiceStack;
using NUnit.Framework;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.FluentValidation;
using ServiceStack.OrmLite;
using TestAuthSession.ServiceInterface;
using TestAuthSession.ServiceModel;

namespace TestAuthSession.Tests
{
    public class IntegrationTest
    {
        const string BaseUri = "http://localhost:2000/";
        private readonly ServiceStackHost appHost;

        class AppHost : AppSelfHostBase
        {
            public AppHost() : base(nameof(IntegrationTest), typeof(MyServices).Assembly) { }

            public override void Configure(Container container)
            {
                container.AddSingleton<IDbConnectionFactory>(new OrmLiteConnectionFactory(
                    ":memory:",
                    SqliteDialect.Provider));
                
                container.AddSingleton<IAuthRepository>(c =>
                    new OrmLiteAuthRepository<AppUser, UserAuthDetails>(c.Resolve<IDbConnectionFactory>()) {
                        UseDistinctRoleTables = true
                    }); 
                
                var AppSettings = this.AppSettings;
                this.Plugins.Add(new AuthFeature(
                    new IAuthProvider[] {
                        new CredentialsAuthProvider(AppSettings),     /* Sign In with Username / Password credentials */
                        new FacebookAuthProvider(AppSettings),        /* Create App https://developers.facebook.com/apps */
                        new GoogleAuthProvider(AppSettings),          /* Create App https://console.developers.google.com/apis/credentials */
                        new MicrosoftGraphAuthProvider(AppSettings),  /* Create App https://apps.dev.microsoft.com */
                    }));

                this.Plugins.Add(new RegistrationFeature()); //Enable /register Service

                //override the default registration validation with your own custom implementation
                this.RegisterAs<CustomRegistrationValidator, IValidator<Register>>();
                
                var authRepo = this.Resolve<IAuthRepository>();
                authRepo.InitSchema();

                CreateUser(authRepo, "admin@example.com", "Admin User", "test1234", roles: new[] { RoleNames.Admin });
            }
            
            // Add initial Users to the configured Auth Repository
            public void CreateUser(IAuthRepository authRepo, string email, string name, string password, string[] roles)
            {
                if (authRepo.GetUserAuthByUserName(email) == null)
                {
                    var newAdmin = new AppUser { Email = email, DisplayName = name };
                    var user = authRepo.CreateUserAuth(newAdmin, password);
                    authRepo.AssignRoles(user, roles);
                }
            }
        }

        public IntegrationTest()
        {
            appHost = new AppHost()
                .Init()
                .Start(BaseUri);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => appHost.Dispose();

        public IServiceClient CreateClient() => new JsonServiceClient(BaseUri);

        [Test]
        public void Can_call_Hello_Service()
        {
            var client = CreateClient();

            var response = client.Get(new Hello { Name = "World" });

            Assert.That(response.Result, Is.EqualTo("Hello, World!"));
        }

        [Test]
        public void Can_call_Hello_Auth_Service()
        {
            var client = CreateClient();
            
            var authResponse = client.Post(new Authenticate
            {
                UserName = "admin@example.com",
                Password = "test1234"
            });
            
            Assert.That(authResponse.UserId, Is.Not.Null);
            Assert.That(authResponse.UserId, Is.EqualTo(1));

            var response = client.Get(new HelloAuth { Name = "World" });

            Assert.That(response.Result, Is.EqualTo("Hello, World! - UserAuthId:1"));
        }
    }
}

