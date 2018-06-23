using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TerrariaApi;

namespace TerrariaDeathCounter
{
    [ApiVersion(2, 1)]
    class TerrariaDeathCounterPlugin : TerrariaPlugin
    {
		public override string Name
		{
			get
			{
				return "Death Recorder";
			}
		}

		public override Version Version
		{
			get
			{
				return new Version(1, 0);
			}
		}
		
		public override string Author
		{
			get
			{
				return "Discoveri";
			}
		}

		public override string Description
		{
			get
			{
				return "Records and reports player deaths from each source.";
			}
		}

        public TerrariaDeathCounterPlugin(Main game)
            : base(game)
        {

        }

		public override void Initialize()
        {
			
        }
    }
}