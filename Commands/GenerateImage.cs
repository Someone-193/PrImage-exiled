using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;
using PrImage.API;

namespace PrImage
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GenerateImageCommand : ICommand
    {
        public string Command => "generateimage";
        public string[] Aliases => new string[] { "gri" };
        public string Description => "Generates primitives using the given image file.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            float maxWidth = 4f;
            float maxHeight = 4f;
            float duration = 30f;
            int targetWidth = 32;
            int targetHeight = 32;

            if (arguments.Count > 0)
            {
                string imagePath = arguments.At(0);

                if (arguments.Count > 1)
                {
                    // maxWidth, maxHeight
                    if (arguments.Count > 1)
                        float.TryParse(arguments.At(1), out maxWidth);
                    if (arguments.Count > 2)
                        float.TryParse(arguments.At(2), out maxHeight);
                    if (arguments.Count > 3)
                        float.TryParse(arguments.At(3), out duration);
                    if (arguments.Count > 4)
                        int.TryParse(arguments.At(4), out targetWidth);
                    if (arguments.Count > 5)
                        int.TryParse(arguments.At(5), out targetHeight);
                }

                ImageRendererAPI.RenderImage(player, imagePath, maxWidth, maxHeight, 2f, duration, targetWidth, targetHeight);
            }
            else
            {
                response = "No image path specified.";
                return false;
            }

            response = "Command was successfully executed.";
            return true;
        }
    }
}
