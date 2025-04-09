using Exiled.API.Interfaces;

namespace PrImage
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool useColorQuantization { get; set; } = false;
    }
}
