﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using RimWorld;
using Verse;
using UnityEngine;

namespace ShapedCharges
{
    public class Building_ShapedCharge : Building
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action
            {
                defaultLabel = "CommandDetonateLabel".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate"),
                defaultDesc = "CommandDetonateDesc".Translate(),
                action = Command_Detonate
            };
        }

        protected void Command_Detonate()
        {
            GetComp<CompDirectedExplosive>().Detonaate(new IntVec3((int)this.Rotation.AsVector2.x, 0, (int)this.Rotation.AsVector2.y), Map);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            var map = Map;
            base.Destroy(mode);

            if (mode != DestroyMode.KillFinalize)
            {
                return;
            }
        }
    }
}
