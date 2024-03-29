﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace ShapedCharges
{
	public class MyExplosion : Explosion
	{
        // Token: 0x060017D1 RID: 6097 RVA: 0x0008EE30 File Offset: 0x0008D030
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.cellsToAffect = SimplePool<List<IntVec3>>.Get();
                this.cellsToAffect.Clear();
                this.damagedThings = SimplePool<List<Thing>>.Get();
                this.damagedThings.Clear();
                this.addedCellsAffectedOnlyByDamage = SimplePool<HashSet<IntVec3>>.Get();
                this.addedCellsAffectedOnlyByDamage.Clear();
            }
        }

        // Token: 0x060017D2 RID: 6098 RVA: 0x0008EE8C File Offset: 0x0008D08C
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            this.cellsToAffect.Clear();
            SimplePool<List<IntVec3>>.Return(this.cellsToAffect);
            this.cellsToAffect = null;
            this.damagedThings.Clear();
            SimplePool<List<Thing>>.Return(this.damagedThings);
            this.damagedThings = null;
            this.addedCellsAffectedOnlyByDamage.Clear();
            SimplePool<HashSet<IntVec3>>.Return(this.addedCellsAffectedOnlyByDamage);
            this.addedCellsAffectedOnlyByDamage = null;
        }

        // Token: 0x060017D3 RID: 6099 RVA: 0x0008EEF8 File Offset: 0x0008D0F8
        public virtual void StartExplosion(IntVec3 vector, SoundDef explosionSound, List<Thing> ignoredThings)
        {
            if (!base.Spawned)
            {
                Log.Error("Called StartExplosion() on unspawned thing.");
                return;
            }
            this.startTick = Find.TickManager.TicksGame;
            this.ignoredThings = ignoredThings;
            this.cellsToAffect.Clear();
            this.damagedThings.Clear();
            this.addedCellsAffectedOnlyByDamage.Clear();
            this.cellsToAffect.AddRange(
                DirectedExplosion_DamageWorker.DirectionalExplosionCellsToHit(vector, Position, Map, radius, needLOSToCell1, needLOSToCell2));
            
            if (this.applyDamageToExplosionCellsNeighbors)
            {
                this.AddCellsNeighbors(this.cellsToAffect);
            }


            this.damType.Worker.ExplosionStart(this, this.cellsToAffect);


            this.PlayExplosionSound(explosionSound);
            FleckMaker.WaterSplash(base.Position.ToVector3Shifted(), base.Map, this.radius * 6f, 20f);
            this.cellsToAffect.Sort((IntVec3 a, IntVec3 b) => this.GetCellAffectTick(b).CompareTo(this.GetCellAffectTick(a)));
            RegionTraverser.BreadthFirstTraverse(base.Position, base.Map, (Region from, Region to) => true, delegate (Region x)
            {
                List<Thing> allThings = x.ListerThings.AllThings;
                for (int i = allThings.Count - 1; i >= 0; i--)
                {
                    if (allThings[i].Spawned)
                    {
                        allThings[i].Notify_Explosion(this);
                    }
                }
                return false;
            }, 25, RegionType.Set_Passable);
        }

        // Token: 0x060017D4 RID: 6100 RVA: 0x0008F020 File Offset: 0x0008D220
        public override void Tick()
        {
            int ticksGame = Find.TickManager.TicksGame;
            int num = this.cellsToAffect.Count - 1;
            while (num >= 0 && ticksGame >= this.GetCellAffectTick(this.cellsToAffect[num]))
            {
                try
                {
                    this.AffectCell(this.cellsToAffect[num]);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Explosion could not affect cell ",
                        this.cellsToAffect[num],
                        ": ",
                        ex
                    }));
                }
                this.cellsToAffect.RemoveAt(num);
                num--;
            }
            if (!this.cellsToAffect.Any<IntVec3>())
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }

        // Token: 0x060017D5 RID: 6101 RVA: 0x0008F0E4 File Offset: 0x0008D2E4
        public int GetDamageAmountAt(IntVec3 c)
        {
            if (!this.damageFalloff)
            {
                return this.damAmount;
            }
            float t = c.DistanceTo(base.Position) / this.radius;
            return Mathf.Max(GenMath.RoundRandom(Mathf.Lerp((float)this.damAmount, (float)this.damAmount * 0.2f, t)), 1);
        }

        // Token: 0x060017D6 RID: 6102 RVA: 0x0008F13C File Offset: 0x0008D33C
        public float GetArmorPenetrationAt(IntVec3 c)
        {
            if (!this.damageFalloff)
            {
                return this.armorPenetration;
            }
            float t = c.DistanceTo(base.Position) / this.radius;
            return Mathf.Lerp(this.armorPenetration, this.armorPenetration * 0.2f, t);
        }

        // Token: 0x060017D7 RID: 6103 RVA: 0x0008F184 File Offset: 0x0008D384
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.radius, "radius", 0f, false);
            Scribe_Defs.Look<DamageDef>(ref this.damType, "damType");
            Scribe_Values.Look<int>(ref this.damAmount, "damAmount", 0, false);
            Scribe_Values.Look<float>(ref this.armorPenetration, "armorPenetration", 0f, false);
            Scribe_References.Look<Thing>(ref this.instigator, "instigator", false);
            Scribe_Defs.Look<ThingDef>(ref this.weapon, "weapon");
            Scribe_Defs.Look<ThingDef>(ref this.projectile, "projectile");
            Scribe_References.Look<Thing>(ref this.intendedTarget, "intendedTarget", false);
            Scribe_Values.Look<bool>(ref this.applyDamageToExplosionCellsNeighbors, "applyDamageToExplosionCellsNeighbors", false, false);
            Scribe_Defs.Look<ThingDef>(ref this.preExplosionSpawnThingDef, "preExplosionSpawnThingDef");
            Scribe_Values.Look<float>(ref this.preExplosionSpawnChance, "preExplosionSpawnChance", 0f, false);
            Scribe_Values.Look<int>(ref this.preExplosionSpawnThingCount, "preExplosionSpawnThingCount", 1, false);
            Scribe_Defs.Look<ThingDef>(ref this.postExplosionSpawnThingDef, "postExplosionSpawnThingDef");
            Scribe_Values.Look<float>(ref this.postExplosionSpawnChance, "postExplosionSpawnChance", 0f, false);
            Scribe_Values.Look<int>(ref this.postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1, false);
            Scribe_Values.Look<float>(ref this.chanceToStartFire, "chanceToStartFire", 0f, false);
            Scribe_Values.Look<bool>(ref this.damageFalloff, "dealMoreDamageAtCenter", false, false);
            Scribe_Values.Look<IntVec3?>(ref this.needLOSToCell1, "needLOSToCell1", null, false);
            Scribe_Values.Look<IntVec3?>(ref this.needLOSToCell2, "needLOSToCell2", null, false);
            Scribe_Values.Look<int>(ref this.startTick, "startTick", 0, false);
            Scribe_Collections.Look<IntVec3>(ref this.cellsToAffect, "cellsToAffect", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<Thing>(ref this.damagedThings, "damagedThings", LookMode.Reference, Array.Empty<object>());
            Scribe_Collections.Look<Thing>(ref this.ignoredThings, "ignoredThings", LookMode.Reference, Array.Empty<object>());
            Scribe_Collections.Look<IntVec3>(ref this.addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.damagedThings != null)
                {
                    this.damagedThings.RemoveAll((Thing x) => x == null);
                }
                if (this.ignoredThings != null)
                {
                    this.ignoredThings.RemoveAll((Thing x) => x == null);
                }
            }
        }

        // Token: 0x060017D8 RID: 6104 RVA: 0x0008F3D8 File Offset: 0x0008D5D8
        private int GetCellAffectTick(IntVec3 cell)
        {
            return this.startTick + (int)((cell - base.Position).LengthHorizontal * 1.5f);
        }

        // Token: 0x060017D9 RID: 6105 RVA: 0x0008F408 File Offset: 0x0008D608
        private void AffectCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return;
            }
            bool flag = this.ShouldCellBeAffectedOnlyByDamage(c);
            if (!flag && Rand.Chance(this.preExplosionSpawnChance) && c.Walkable(base.Map))
            {
                this.TrySpawnExplosionThing(this.preExplosionSpawnThingDef, c, this.preExplosionSpawnThingCount);
            }
            //Log.Message(damagedThings.Count.ToString());
            //Log.Message(((Explosion)this).GetDamageAmountAt(Position).ToString());
            //Log.Message((this).GetDamageAmountAt(Position).ToString());





            //Explosion tempExplosion = new Explosion() { Position = this.Position, radius = this.radius,
            //damAmount = this.damAmount, armorPenetration = this.armorPenetration};
            Explosion tempExplosion = (Explosion)this;
            tempExplosion.damAmount = this.damAmount;
            tempExplosion.armorPenetration = this.armorPenetration;

            this.damType.Worker.ExplosionAffectCell(tempExplosion, c, this.damagedThings, this.ignoredThings, !flag);
            
            
            
            
            
            
            //Log.Message(damagedThings.Count.ToString());
            if (!flag && Rand.Chance(this.postExplosionSpawnChance) && c.Walkable(base.Map))
            {
                this.TrySpawnExplosionThing(this.postExplosionSpawnThingDef, c, this.postExplosionSpawnThingCount);
            }
            float num = this.chanceToStartFire;
            if (this.damageFalloff)
            {
                num *= Mathf.Lerp(1f, 0.2f, c.DistanceTo(base.Position) / this.radius);
            }
            if (Rand.Chance(num))
            {
                FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
            }
        }

        // Token: 0x060017DA RID: 6106 RVA: 0x0008F508 File Offset: 0x0008D708
        private void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 c, int count)
        {
            if (thingDef == null)
            {
                return;
            }
            if (thingDef.IsFilth)
            {
                FilthMaker.TryMakeFilth(c, base.Map, thingDef, count, FilthSourceFlags.None);
                return;
            }
            Thing thing = ThingMaker.MakeThing(thingDef, null);
            thing.stackCount = count;
            GenSpawn.Spawn(thing, c, base.Map, WipeMode.Vanish);
        }

        // Token: 0x060017DB RID: 6107 RVA: 0x0008F544 File Offset: 0x0008D744
        private void PlayExplosionSound(SoundDef explosionSound)
        {
            bool flag;
            if (Prefs.DevMode)
            {
                flag = (explosionSound != null);
            }
            else
            {
                flag = !explosionSound.NullOrUndefined();
            }
            if (flag)
            {
                explosionSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                return;
            }
            this.damType.soundExplosion.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        }

        //Token: 0x060017DC RID: 6108 RVA: 0x0008F5B4 File Offset: 0x0008D7B4
        private void AddCellsNeighbors(List<IntVec3> cells)
        {
            List<IntVec3> tmp = new List<IntVec3>();
            this.addedCellsAffectedOnlyByDamage.Clear();
            for (int i = 0; i < cells.Count; i++)
            {
                tmp.Add(cells[i]);
            }
            for (int j = 0; j < cells.Count; j++)
            {
                if (cells[j].Walkable(base.Map))
                {
                    for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
                    {
                        IntVec3 intVec = cells[j] + GenAdj.AdjacentCells[k];
                        if (intVec.InBounds(base.Map))
                        {
                            this.addedCellsAffectedOnlyByDamage.Add(intVec);
                        }
                    }
                }
            }
            cells.Clear();
            foreach (IntVec3 item in tmp)
            {
                cells.Add(item);
            }
            tmp.Clear();
        }

        // Token: 0x060017DD RID: 6109 RVA: 0x0008F6C8 File Offset: 0x0008D8C8
        private bool ShouldCellBeAffectedOnlyByDamage(IntVec3 c)
        {
            return this.applyDamageToExplosionCellsNeighbors && this.addedCellsAffectedOnlyByDamage.Contains(c);
        }

        // Token: 0x04001064 RID: 4196
        public float radius;

        // Token: 0x04001065 RID: 4197
        public DamageDef damType;

        // Token: 0x04001066 RID: 4198
        public int damAmount;

        // Token: 0x04001067 RID: 4199
        public float armorPenetration;

        // Token: 0x04001068 RID: 4200
        public Thing instigator;

        // Token: 0x04001069 RID: 4201
        public ThingDef weapon;

        // Token: 0x0400106A RID: 4202
        public ThingDef projectile;

        // Token: 0x0400106B RID: 4203
        public Thing intendedTarget;

        // Token: 0x0400106C RID: 4204
        public bool applyDamageToExplosionCellsNeighbors;

        // Token: 0x0400106D RID: 4205
        public ThingDef preExplosionSpawnThingDef;

        // Token: 0x0400106E RID: 4206
        public float preExplosionSpawnChance;

        // Token: 0x0400106F RID: 4207
        public int preExplosionSpawnThingCount = 1;

        // Token: 0x04001070 RID: 4208
        public ThingDef postExplosionSpawnThingDef;

        // Token: 0x04001071 RID: 4209
        public float postExplosionSpawnChance;

        // Token: 0x04001072 RID: 4210
        public int postExplosionSpawnThingCount = 1;

        // Token: 0x04001073 RID: 4211
        public float chanceToStartFire;

        // Token: 0x04001074 RID: 4212
        public bool damageFalloff;

        // Token: 0x04001075 RID: 4213
        public IntVec3? needLOSToCell1;

        // Token: 0x04001076 RID: 4214
        public IntVec3? needLOSToCell2;

        // Token: 0x04001077 RID: 4215
        private int startTick;

        // Token: 0x04001078 RID: 4216
        private List<IntVec3> cellsToAffect;

        // Token: 0x04001079 RID: 4217
        private List<Thing> damagedThings;

        // Token: 0x0400107A RID: 4218
        private List<Thing> ignoredThings;

        // Token: 0x0400107B RID: 4219
        private HashSet<IntVec3> addedCellsAffectedOnlyByDamage;

        // Token: 0x0400107C RID: 4220
        private const float DamageFactorAtEdge = 0.2f;

        // Token: 0x0400107D RID: 4221
        private static HashSet<IntVec3> tmpCells = new HashSet<IntVec3>();
    }
}
