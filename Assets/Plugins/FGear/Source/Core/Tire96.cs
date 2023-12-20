// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;

namespace FGear
{
    [CreateAssetMenu(fileName = "Tire96Default", menuName = "FGear/Tire96", order = 2)]
    public class Tire96 : Tire
    {
        //pacejka 96 longitudinal
        [SerializeField]
        float PCX1 = 1.685f,
              PDX1 = 1.210f, PDX2 = -0.037f,
              PEX1 = 0.344f, PEX2 = 0.095f, PEX3 = -0.020f, PEX4 = 0f,
              PKX1 = 21.51f, PKX2 = -0.163f, PKX3 = 0.245f,
              PHX1 = -0.002f, PHX2 = 0.002f,
              PVX1 = 0f, PVX2 = 0f;

        //pacejka 96 lateral
        [SerializeField]
        float PCY1 = 1.193f,
              PDY1 = -0.990f, PDY2 = 0.145f, PDY3 = -11.23f,
              PEY1 = -1.003f, PEY2 = -0.537f, PEY3 = -0.083f, PEY4 = -4.787f,
              PKY1 = -14.95f, PKY2 = 2.130f, PKY3 = -0.028f,
              PHY1 = 0.003f, PHY2 = -0.001f, PHY3 = 0.075f,
              PVY1 = 0.045f, PVY2 = -0.024f, PVY3 = -0.532f, PVY4 = 0.039f;

        //pacejka 96 combined longitudinal
        [SerializeField]
        float RBX1 = 12.35f, RBX2 = -10.77f,
              RCX1 = 1.092f,
              REX1 = 0f, REX2 = 0f,
              RHX1 = 0.007f;

        //pacejka 96 combined lateral
        [SerializeField]
        float RBY1 = 6.461f, RBY2 = 4.196f, RBY3 = -0.015f,
              RCY1 = 1.081f,
              REY1 = 0f, REY2 = 0f,
              RHY1 = 0.009f,
              RVY1 = 0.053f, RVY2 = -0.073f, RVY3 = 0.517f, RVY4 = 35.44f, RVY5 = 1.9f, RVY6 = -10.71f;

        [SerializeField]
        float mNominalLoad = 5000f;

        protected override void init()
        {
            base.init();
            mCombineMode = ForceCombineMode.SUM;
        }

        float pacejkaFx(float slipRatio, float Fz, float dFz)
        {
            float ux = PDX1 + PDX2 * dFz;
            float Dx = ux * Fz;
            float Cx = PCX1;
            float Kxk = Fz * (PKX1 + PKX2 * dFz) * Mathf.Exp(PKX3 * dFz);
            float Bx = Kxk / (Cx * Dx + Mathf.Epsilon);
            float Shx = PHX1 + PHX2 * dFz;
            float Kx = slipRatio + Shx;
            float Ex = (PEX1 + PEX2 * dFz + PEX3 * dFz * dFz) * (1.0f - PEX4 * Math.Sign(Kx)); //signum
            float Svx = Fz * (PVX1 + PVX2 * dFz);
            float Fx0 = Dx * Mathf.Sin(Cx * Mathf.Atan(Bx * Kx - Ex * (Bx * Kx - Mathf.Atan(Bx * Kx)))) + Svx;
            return Fx0;
        }

        float pacejkaFy(float slipAngle, float camber, float Fz, float dFz, out float Dyk)
        {
            float Fz0 = mNominalLoad;
            float uy = (PDY1 + PDY2 * dFz) * (1.0f - PDY3 * camber * camber);
            float Dy = uy * Fz;
            float Cy = PCY1;
            float Kya = PKY1 * Fz0 * Mathf.Sin(2.0f * Mathf.Atan(Fz / (PKY2 * Fz0)));
            float Ky = Kya * (1.0f - PKY3 * camber * camber);
            float By = Ky / (Cy * Dy + Mathf.Epsilon);
            float Shy = (PHY1 + PHY2 * dFz) + (PHY3 * camber);
            float Ay = slipAngle + Shy;
            float Ey = (PEY1 + PEY2 * dFz) * (1.0f - (PEY3 + PEY4 * camber) * Math.Sign(Ay)); //signum
            float Svy = Fz * ((PVY1 + PVY2 * dFz) + (PVY3 + PVY4 * dFz) * camber);
            float Fy0 = Dy * Mathf.Sin(Cy * Mathf.Atan(By * Ay - Ey * (By * Ay - Mathf.Atan(By * Ay)))) + Svy;

            //for PacejkaSvy
            Dyk = Dy;

            return Fy0;
        }

        float pacejkaGx(float slipRatio, float slipAngle, float dFz)
        {
            float Bx = RBX1 * Mathf.Cos(Mathf.Atan(RBX2 * slipRatio));
            float Cx = RCX1;
            float Shx = RHX1;
            float As = slipAngle + Shx;
            float Ex = REX1 + REX2 * dFz;
            float G0 = Mathf.Cos(Cx * Mathf.Atan(Bx * Shx - Ex * (Bx * Shx - Mathf.Atan(Bx * Shx))));
            float Gx = Mathf.Cos(Cx * Mathf.Atan(Bx * As - Ex * (Bx * As - Mathf.Atan(Bx * As)))) / G0;
            return Gx;
        }

        float pacejkaGy(float slipRatio, float slipAngle, float dFz)
        {
            float By = RBY1 * Mathf.Cos(Mathf.Atan(RBY2 * (slipAngle - RBY3)));
            float Cy = RCY1;
            float Shy = RHY1;
            float Ks = slipRatio + Shy;
            float Ey = REY1 + REY2 * dFz;
            float G0 = Mathf.Cos(Cy * Mathf.Atan(By * Shy - Ey * (By * Shy - Mathf.Atan(By * Shy))));
            float Gy = Mathf.Cos(Cy * Mathf.Atan(By * Ks - Ey * (By * Ks - Mathf.Atan(By * Ks)))) / G0;
            return Gy;
        }

        float pacejkaSvyk(float slipRatio, float slipAngle, float camber, float dFz, float Dyk)
        {
            float Dvyk = Dyk * (RVY1 + RVY2 * dFz + RVY3 * camber) * Mathf.Cos(Mathf.Atan(RVY4 * slipAngle));
            float Svyk = Dvyk * Mathf.Sin(RVY5 * Mathf.Atan(RVY6 * slipRatio));
            return Svyk;
        }

        public override void calculate(float load, float slipRatio, float slipAngle, float camber, float pressure, float radius, float Vx)
        {
            if (load <= 0f)
            {
                mLongitudinalForce = mLateralForce = 0f;
                return;
            }

            //symmetry signs
            float lngSign = 1.0f;
            float latSign = 1.0f;
            if (mForceSymmetry)
            {
                if (slipRatio < 0.0f)
                {
                    slipRatio *= -1.0f;
                    lngSign = -1.0f;
                }
                else if (slipRatio == 0.0f) lngSign = 0.0f;
                if (slipAngle < 0.0f)
                {
                    slipAngle *= -1.0f;
                    latSign = -1.0f;
                }
                else if (slipAngle == 0.0f) latSign = 0.0f;
            }

            slipAngle *= -1f; //sign convention
            slipAngle = Mathf.Tan(slipAngle);
            camber = Mathf.Sin(camber);

            float dFz = (load - mNominalLoad) / mNominalLoad;

            //longitudinal
            float Fx0 = pacejkaFx(slipRatio, load, dFz);

            //lateral
            float Dyk;
            float Fy0 = pacejkaFy(slipAngle, camber, load, dFz, out Dyk);

            //combined
            float Gx = pacejkaGx(slipRatio, slipAngle, dFz);
            float Gy = pacejkaGy(slipRatio, slipAngle, dFz);
            float Svyk = pacejkaSvyk(slipRatio, slipAngle, camber, dFz, Dyk);

            //sum up
            mLongitudinalForce = Gx * Fx0 * lngSign;
            mLateralForce = (Gy * Fy0 + Svyk) * latSign;
        }
    }
}