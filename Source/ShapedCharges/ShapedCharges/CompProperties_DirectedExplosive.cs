﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace ShapedCharges
{
    public class CompProperties_DirectedExplosive : CompProperties_Explosive
    {
        public CompProperties_DirectedExplosive()
        {
            this.compClass = typeof(CompDirectedExplosive);
            //this.explosiveDamageType = DamageDefOf.Bomb;
            //this.damageAmountBase = 100;
        }
    }
}
