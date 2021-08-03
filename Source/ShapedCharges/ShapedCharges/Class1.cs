using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

namespace ShapedCharges
{
    public class DirectedExplosion : Explosion
    {

    }

    public class DirectedExplosion_DamageWorker : DamageWorker
    {
        public override IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, Map map, float radius, 
            IntVec3? needLOSToCell1 = null, IntVec3? needLOSToCell2 = null)
        {
            //return base.ExplosionCellsToHit(center, map, radius, needLOSToCell1, needLOSToCell2);
            //DamageWorker.openCells.Clear();
            List<IntVec3> myOpenCells = new List<IntVec3>(); //note: openCells is private
            List<IntVec3> myadjWallCells = new List<IntVec3>();

            int num = GenRadial.NumCellsInRadius(radius);


        }
    }
}
