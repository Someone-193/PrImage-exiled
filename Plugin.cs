using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;

namespace PrImage
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;
        public override string Name => "PrImage";
        public override string Author => "Lilin";
        public override Version Version => new Version(0, 3, 0);
        public override void OnEnabled()
        {
            Instance = this;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            base.OnDisabled();
        }
    }
}
