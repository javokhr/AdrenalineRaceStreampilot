// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;

namespace FGear
{
    [CreateAssetMenu(fileName = "TireSimpleDefault", menuName = "FGear/TireSimple", order = 1)]
    public class TireSimple : Tire
    {
        //simplified pacejka longitudinal
        [SerializeField]
        float Lnb = 15.0f, Lnc = 2.0f, Lnd = 1.0f, Lne = 0.95f;

        //simplified pacejka lateral
        [SerializeField]
        float Ltb = 5.0f, Ltc = 2.0f, Ltd = 1.0f, Lte = 0.9f;

        protected override void init()
        {
            base.init();
            mCombineMode = ForceCombineMode.GRIP;
        }

        public override void calculate(float load, float slipRatio, float slipAngle, float camber, float pressure, float radius, float Vx)
        {
            mLongitudinalForce = load * (Lnd * Mathf.Sin(Lnc * Mathf.Atan(Lnb * (1 - Lne) * slipRatio + Lne * Mathf.Atan(Lnb * slipRatio))));
            mLateralForce = load * (Ltd * Mathf.Sin(Ltc * Mathf.Atan(Ltb * (1 - Lte) * slipAngle + Lte * Mathf.Atan(Ltb * slipAngle))));
        }
    }
}