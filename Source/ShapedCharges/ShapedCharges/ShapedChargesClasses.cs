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
        public override void StartExplosion(SoundDef explosionSound, List<Thing> ignoredThings)
        {
            
        }
    }

    public class DirectedExplosion_DamageWorker : DamageWorker
    {
        public virtual IEnumerable<IntVec3> DirectionalExplosionCellsToHit(IntVec3 vector, IntVec3 center, Map map, float radius,
            IntVec3? needLOSToCell1 = null, IntVec3? needLOSToCell2 = null)
        {
            List<IntVec3> myOpenCells = new List<IntVec3>(); //note: openCells is private
            List<IntVec3> myadjWallCells = new List<IntVec3>();
            //IntVec3 test = new IntVec3(0, 0, 0);
            //test.RotatedBy(new Rot4())
            List<IntVec3> pattern = TriangularPattern(radius, vector);
            for(int i = 0; i < pattern.Count; ++i)
            {
                IntVec3 intVec = center + pattern[i];
                if(intVec.InBounds(map) && GenSight.LineOfSight(center, intVec, map, true, null, 0, 0))
                {
                    if(needLOSToCell1 != null || needLOSToCell2 != null)
                    {
                        bool flag = needLOSToCell1 != null && GenSight.LineOfSight(needLOSToCell1.Value, intVec, map, false, null, 0, 0);
                        bool flag2 = needLOSToCell2 != null && GenSight.LineOfSight(needLOSToCell2.Value, intVec, map, false, null, 0, 0);
                        if (!flag && !flag2)
                        {
                            goto IL_B1;
                        }
                    }
                    myOpenCells.Add(intVec);
                }
            IL_B1:;
            }

            for(int i = 0; i < myOpenCells.Count; ++i)
            {
                IntVec3 intVec = myOpenCells[i];
                if (intVec.Walkable(map))
                {
                    for(int j = 0; j < 4; j++)
                    {
                        IntVec3 iv2 = intVec + GenAdj.CardinalDirections[j];
                        if(iv2.InHorDistOf(center, radius) && iv2.InBounds(map) && !iv2.Standable(map) && iv2.GetEdifice(map) != null &&
                            !myOpenCells.Contains(iv2) && !myadjWallCells.Contains(iv2))
                        {
                            myadjWallCells.Add(iv2);
                        }
                    }
                }
            }

            return myOpenCells.Concat(myadjWallCells);
        }



        public virtual IntVec3 XZTranspose(IntVec3 vecIn)
        {
            return new IntVec3(vecIn.z, vecIn.y, vecIn.x);
        }
        public virtual List<IntVec3> TriangularPattern(float radius, IntVec3 vec)
        {
            List<IntVec3> pattern = new List<IntVec3>();
            for(int i = 0; i < radius; ++i)
            {
                for(int j = 0; j < i + 1; ++j)
                {
                    pattern.Add(new IntVec3(i * vec.x + j * vec.z, 0, i * vec.z + j * vec.x));
                    pattern.Add(new IntVec3(i * vec.x - j * vec.z, 0, i * vec.z - j * vec.x));
                }
            }
            return pattern;
        }
    }
}
