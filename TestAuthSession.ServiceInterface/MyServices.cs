using ServiceStack;
using TestAuthSession.ServiceModel;

namespace TestAuthSession.ServiceInterface
{
    
    public class MyServices : Service
    {
        public object Any(Hello request)
        {
            var session = GetSession();
            return new HelloResponse { Result = $"Hello, {request.Name}!" };
        }
    }

    [Authenticate]
    public class MyAuthService : Service
    {
        public object Any(HelloAuth request)
        {
            var session = GetSession();
            return new HelloResponse { Result = $"Hello, {request.Name}! - UserAuthId:{session.UserAuthId}" };
        }
    }
}

