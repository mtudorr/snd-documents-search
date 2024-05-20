using System;
using Constructs;

namespace SearchService.Deploy;

public class Configuration
{
    public Configuration(Node node)
    {
        Env = node.TryGetContext("env")?.ToString();
        
        if (Env == null)
        {
            throw new Exception("The environment name was not specified");
        }
    }
    
    public string Env { get; }
}