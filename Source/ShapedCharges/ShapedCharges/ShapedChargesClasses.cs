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
    //public class CompProperties_DirectedExplosive : CompProperties_Explosive
    //{
    //    //public override 
    //}
    public class Projectile_DirectedExplosive : Projectile_Explosive
    {
        protected virtual void Explode(IntVec3 vector)
        {
            //base.Explode();
            Map map = base.Map;
            this.Destroy(DestroyMode.Vanish);
            if (this.def.projectile.explosionEffect != null)
            {
                Effecter effecter = this.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map, false), new TargetInfo(base.Position, map, false));
                effecter.Cleanup();
            }
            IntVec3 position = base.Position;
            Map map2 = map;
            float explosionRadius = this.def.projectile.explosionRadius;
            DamageDef damageDef = this.def.projectile.damageDef;
            Thing launcher = this.launcher;
            int damageAmount = base.DamageAmount;
            float armorPenetration = base.ArmorPenetration;
            SoundDef soundExplode = this.def.projectile.soundExplode;
            ThingDef equipmentDef = this.equipmentDef;
            ThingDef def = this.def;
            Thing thing = this.intendedTarget.Thing;
            ThingDef postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = this.def.projectile.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = this.def.projectile.postExplosionSpawnThingCount;
            ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
            float preExplosionSpawnChance = this.def.projectile.preExplosionSpawnChance;
            int preExplosionSpawnThingCount = this.def.projectile.preExplosionSpawnThingCount;
            GenDirectedExplosion.DoExplosion(vector,
                position, map2, explosionRadius, damageDef, launcher, damageAmount, armorPenetration, soundExplode, equipmentDef, 
                def, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, 
                this.def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, 
                preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff, 
                new float?(this.origin.AngleToFlat(this.destination)), null);
        }
    }

    public class GenDirectedExplosion
    {
        public static void DoExplosion(IntVec3 vector, IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, 
            float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, 
            Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, 
            int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, 
            ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, 
            float chanceToStartFire = 0f, bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null)
        {
            if (map == null)
            {
                Log.Warning("Tried to do explosion in a null map.");
                return;
            }
            if (damAmount < 0)
            {
                damAmount = damType.defaultDamage;
                armorPenetration = damType.defaultArmorPenetration;
                if (damAmount < 0)
                {
                    Log.ErrorOnce("Attempted to trigger an explosion without defined damage", 91094882);
                    damAmount = 1;
                }
            }
            if (armorPenetration < 0f)
            {
                armorPenetration = (float)damAmount * 0.015f;
            }

            MyExplosion explosion = (MyExplosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map, WipeMode.Vanish);
            //MyExplosion myExplosion = ()
            
            IntVec3? needLOSToCell = null;
            IntVec3? needLOSToCell2 = null;
            if (direction != null)
            {
                CalculateNeededLOSToCells(center, map, direction.Value, out needLOSToCell, out needLOSToCell2);
            }
            explosion.radius = radius;
            explosion.damType = damType;
            explosion.instigator = instigator;
            explosion.damAmount = damAmount;
            explosion.armorPenetration = armorPenetration;
            explosion.weapon = weapon;
            explosion.projectile = projectile;
            explosion.intendedTarget = intendedTarget;
            explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
            explosion.preExplosionSpawnChance = preExplosionSpawnChance;
            explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
            explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
            explosion.postExplosionSpawnChance = postExplosionSpawnChance;
            explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
            explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
            explosion.chanceToStartFire = chanceToStartFire;
            explosion.damageFalloff = damageFalloff;
            explosion.needLOSToCell1 = needLOSToCell;
            explosion.needLOSToCell2 = needLOSToCell2;
            explosion.StartExplosion(vector, explosionSound, ignoredThings);
        }

        public static void CalculateNeededLOSToCells(IntVec3 position, Map map, float direction, out IntVec3? needLOSToCell1, out IntVec3? needLOSToCell2)
        {
            needLOSToCell1 = null;
            needLOSToCell2 = null;
            if (position.CanBeSeenOverFast(map))
            {
                return;
            }
            direction = GenMath.PositiveMod(direction, 360f);
            IntVec3 intVec = position;
            intVec.z++;
            IntVec3 intVec2 = position;
            intVec2.z--;
            IntVec3 intVec3 = position;
            intVec3.x--;
            IntVec3 intVec4 = position;
            intVec4.x++;
            if (direction < 90f)
            {
                if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = new IntVec3?(intVec3);
                }
                if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = new IntVec3?(intVec);
                    return;
                }
            }
            else if (direction < 180f)
            {
                if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = new IntVec3?(intVec);
                }
                if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = new IntVec3?(intVec4);
                    return;
                }
            }
            else if (direction < 270f)
            {
                if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = new IntVec3?(intVec4);
                }
                if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = new IntVec3?(intVec2);
                    return;
                }
            }
            else
            {
                if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = new IntVec3?(intVec2);
                }
                if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = new IntVec3?(intVec3);
                }
            }
        }
    }

    //public class DirectedExplosion : Explosion
    //{
    //    public virtual void StartExplosion(IntVec3 vector, SoundDef explosionSound, List<Thing> ignoredThings)
    //    {
    //        base.StartExplosion(explosionSound, ignoredThings);
    //    }
    //}

    public class DirectedExplosion_DamageWorker : DamageWorker
    {
        public static IEnumerable<IntVec3> DirectionalExplosionCellsToHit(IntVec3 vector, IntVec3 center, Map map, float radius,
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
        public static List<IntVec3> TriangularPattern(float radius, IntVec3 vec)
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
