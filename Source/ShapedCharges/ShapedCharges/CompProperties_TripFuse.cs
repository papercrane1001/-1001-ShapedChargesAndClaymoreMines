using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace ShapedCharges
{
    public class CompProperties_TripFuse : CompProperties
    {
        public CompProperties_TripFuse()
        {
            this.compClass = typeof(CompTripFuse);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
			foreach (string text in base.ConfigErrors(parentDef))
			{
				yield return text;
			}
			IEnumerator<string> enumerator = null;
			if (parentDef.tickerType != TickerType.Normal)
			{
				yield return string.Concat(new object[]
				{
					"CompTripFuse needs tickerType ",
					TickerType.Rare,
					" or faster, has ",
					parentDef.tickerType
				});
			}
			if (parentDef.CompDefFor<CompDirectedExplosive>() == null)
			{
				yield return "CompTripFuse requires a CompDirectedExplosive";
			}
			yield break;
			yield break;
		}

		public ThingDef target;

		// Token: 0x0400223F RID: 8767
		public float radius;
	}
}
