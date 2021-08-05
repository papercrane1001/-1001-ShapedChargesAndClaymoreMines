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
    public class CompTripFuse : ThingComp
    {
        public CompProperties_TripFuse Props
        {
            get
            {
                return (CompProperties_TripFuse)this.props;
            }
        }

        public override void CompTick()
        {
            //base.CompTick();
            if(Find.TickManager.TicksGame % 100 == 0)
            {
                this.CompTickRare();
            }
        }

        public override void CompTickRare()
        {
            //Check if anything is in front of this.parent.Position using this.parent.Rotation.AsVector2
            //iv2.GetEdifice(map) != null
            //iv2.GetFistPawn(map) != null
            IntVec3 vec = this.parent.Position;

            IntVec3 orientation = new IntVec3((int)this.parent.Rotation.AsVector2.x, 0, (int)this.parent.Rotation.AsVector2.y);
            vec += orientation;
            int rangeCounter = 0;
            while(rangeCounter++ < Props.radius && vec.GetEdifice(this.parent.Map) == null)
            {
                if(vec.GetFirstPawn(this.parent.Map) != null)
                {
                    ((CompDirectedExplosive)this.parent.GetComp<CompDirectedExplosive>()).Detonaate(orientation, this.parent.Map);
                    break;
                }
                vec += orientation;
            }
            

            

        }
    }
}
