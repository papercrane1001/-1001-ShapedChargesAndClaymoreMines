using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using UnityEngine;

namespace ShapedCharges
{
    public static class MyExtensions
    {
  //      public static void StartExplosion(this Explosion expl, IntVec3 vec, SoundDef explosionSound, List<Thing> ignoredThings)
  //      {
		//	if (!base.Spawned)
		//	{
		//		Log.Error("Called StartExplosion() on unspawned thing.");
		//		return;
		//	}
		//	this.startTick = Find.TickManager.TicksGame;
		//	this.ignoredThings = ignoredThings;
		//	this.cellsToAffect.Clear();
		//	this.damagedThings.Clear();
		//	this.addedCellsAffectedOnlyByDamage.Clear();
		//	this.cellsToAffect.AddRange(this.damType.Worker.ExplosionCellsToHit(this));
		//	if (this.applyDamageToExplosionCellsNeighbors)
		//	{
		//		this.AddCellsNeighbors(this.cellsToAffect);
		//	}
		//	this.damType.Worker.ExplosionStart(this, this.cellsToAffect);
		//	this.PlayExplosionSound(explosionSound);
		//	FleckMaker.WaterSplash(base.Position.ToVector3Shifted(), base.Map, this.radius * 6f, 20f);
		//	this.cellsToAffect.Sort((IntVec3 a, IntVec3 b) => this.GetCellAffectTick(b).CompareTo(this.GetCellAffectTick(a)));
		//	RegionTraverser.BreadthFirstTraverse(base.Position, base.Map, (Region from, Region to) => true, delegate (Region x)
		//	{
		//		List<Thing> allThings = x.ListerThings.AllThings;
		//		for (int i = allThings.Count - 1; i >= 0; i--)
		//		{
		//			if (allThings[i].Spawned)
		//			{
		//				allThings[i].Notify_Explosion(this);
		//			}
		//		}
		//		return false;
		//	}, 25, RegionType.Set_Passable);
		//}
    }
}
