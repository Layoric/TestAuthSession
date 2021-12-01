using ServiceStack;

namespace TestAuthSession.ServiceModel
{
    [Route("/hello")]
    [Route("/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }
    
    [Route("/secure/hello")]
    [Route("/secure/hello/{Name}")]
    public class HelloAuth : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }
}


