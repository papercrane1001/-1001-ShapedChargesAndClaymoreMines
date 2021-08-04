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
	public class CompDirectedExplosive : CompExplosive
	{
		//protected override void Detonate()
		public void Detonaate(Map map, bool ignoreUnspawned = false)
		{
			Log.Message("Point5");
			if (!ignoreUnspawned && !this.parent.SpawnedOrAnyParentSpawned)
			{
				return;
			}
			CompProperties_DirectedExplosive props = (CompProperties_DirectedExplosive)this.Props;
			float num = this.ExplosiveRadius();
			if (props.explosiveExpandPerFuel > 0f && this.parent.GetComp<CompRefuelable>() != null)
			{
				this.parent.GetComp<CompRefuelable>().ConsumeFuel(this.parent.GetComp<CompRefuelable>().Fuel);
			}
			if (props.destroyThingOnExplosionSize <= num && !this.parent.Destroyed)
			{
				this.destroyedThroughDetonation = true;
				this.parent.Kill(null, null);
			}
			//this.EndWickSustainer();
			if (this.wickSoundSustainer != null)
			{
				this.wickSoundSustainer.End();
				this.wickSoundSustainer = null;
			}

			this.wickStarted = false;
			if (map == null)
			{
				Log.Warning("Tried to detonate CompExplosive in a null map.");
				return;
			}
			if (props.explosionEffect != null)
			{
				Effecter effecter = props.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(this.parent.PositionHeld, map, false), new TargetInfo(this.parent.PositionHeld, map, false));
				effecter.Cleanup();
			}
			Thing parent;
			//if (this.instigator != null && (!this.instigator.HostileTo(this.parent.Faction) || this.parent.Faction == Faction.OfPlayer))
			//{
			//	parent = this.instigator;
			//}
			//else
			//{
			parent = this.parent;
			//}
			GenDirectedExplosion.DoExplosion(new IntVec3(1, 0, 0), this.parent.PositionHeld, map, num, props.explosiveDamageType, parent,
				props.damageAmountBase, props.armorPenetrationBase, props.explosionSound, null, null, null, props.postExplosionSpawnThingDef,
				props.postExplosionSpawnChance, props.postExplosionSpawnThingCount, props.applyDamageToExplosionCellsNeighbors,
				props.preExplosionSpawnThingDef, props.preExplosionSpawnChance, props.preExplosionSpawnThingCount, props.chanceToStartFire,
				props.damageFalloff, null, null);
		}
	}
}
