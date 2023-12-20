// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using FGear;

using UnityEngine.UI;

public class Effects : MonoBehaviour
{
    [System.Serializable]
    class SurfaceType
    {
        public PhysicMaterial Material = null;
        public float Friction = 1f;
        public float Roughness = 0f;
        public AudioClip SkidClip = null;
        [System.NonSerialized]
        public AudioSource SkidSound;
    }

    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    SurfaceType[] SurfaceList;
    [SerializeField]
    bool ShowTireSmoke = true;
    [SerializeField]
    bool ShowSkidmark = true;
    [SerializeField]
    bool TerrainBasedSurface = false;
    [SerializeField]
    bool ShowGripGUI = false;

    //sfx
    [SerializeField]
    AudioClip EngineHighClip;
    [SerializeField]
    AudioClip EngineLowClip;
    [SerializeField]
    AudioClip CrashClip;

    AudioSource mEngineHigh;
    AudioSource mEngineLow;
    AudioSource mCrashSound;
    AudioSource mNitroSound;
    AudioSource mMuzzleSound;

    //brake light effect
    [SerializeField]
    Renderer[] BrakeLightRenderers;
    [SerializeField]
    int BrakeLightMaterialIndex;
    [SerializeField]
    Color BrakeLightColor = Color.black;

    //sfx volume etc.
    [SerializeField]
    float BaseVolume = 1f;
    [SerializeField]
    float EngineVolume = 0.3f;
    [SerializeField]
    float EngineBasePitch = 1f;
    [SerializeField]
    float EnginePitchScale = 1f;
    [SerializeField]
    float SkidVolume = 0.2f;
    [SerializeField]
    float CrashVolume = 0.2f;
    [SerializeField]
    float LongSlipThreshold = 0.2f;
    [SerializeField]
    float LatSlipThreshold = 0.2f;

    //nitro effect
    [SerializeField]
    bool NitroActive = false;
    [SerializeField]
    float NitroPower = 2f;
    [SerializeField]
    float NitroRestoreSpeed = 0.1f;
    [SerializeField]
    float NitroConsumeSpeed = 0.3f;
    [SerializeField]
    AudioClip NitroClip;
    [SerializeField]
    float NitroVolume = 0.3f;
    private Button nitroButton;

    //muzzle effects
    [SerializeField]
    Transform Muzzles;
    [SerializeField]
    AudioClip MuzzleClip;
    [SerializeField]
    float MuzzleVolume = 0.1f;
    [SerializeField]
    float MuzzlePitch = 0.2f;
    [SerializeField]
    float MuzzleInterval = 0.1f;
    [SerializeField]
    float MuzzleLimit = 0.1f;

    //wheels
    Wheel mFLW, mFRW, mRLW, mRRW;

    //surface ids
    int mFLSurface, mFRSurface, mRLSurface, mRRSurface;

    //effects
    Skidmark mSmarkFL, mSmarkFR, mSmarkRL, mSmarkRR;
    ParticleSystem mSmokeFL, mSmokeFR, mSmokeRL, mSmokeRR;
    int mSkidFL, mSkidFR, mSkidRL, mSkidRR;
    int mSkidCount;
    Material[] mBrakeMaterials;
    Color mParticleDefaultColor = new Color(1f, 1f, 1f, 0.5f);
    Color mParticleSoilColor = new Color(0.35f, 0.15f, 0.1f, 0.5f);

    //muzzle helpers
    float mLastRpm = 0f;
    float mLastTime = 0f;

    //nitro helpers
    float mCurrentNitro = 1f;
    bool mNitroActive = false;

    //surface friction ui
    Rect mWindowRect = new Rect(Screen.width - 130, 180, 125, 140);
    int mWinID;

    void Start()
    {
        mWinID = Utility.winIDs++;

        mFLW = Vehicle.getAxle(0).getLeftWheel();
        mFRW = Vehicle.getAxle(0).getRightWheel();
        mRLW = Vehicle.getAxle(1).getLeftWheel();
        mRRW = Vehicle.getAxle(1).getRightWheel();

        createVisualEffects();
        createSoundEffects();

        //try to get material for brake lights
        if (BrakeLightRenderers != null && BrakeLightRenderers.Length > 0)
        {
            mBrakeMaterials = new Material[BrakeLightRenderers.Length];
            for (int i = 0; i < BrakeLightRenderers.Length; i++)
            {
                List<Material> mats = new List<Material>();
                BrakeLightRenderers[i].GetMaterials(mats);
                mBrakeMaterials[i] = mats[BrakeLightMaterialIndex];
            }
        }

        string canvasName = "MobileUICanvas";
        GameObject mobileUI = GameObject.Find(canvasName);
        // Проверить, что объект MobileUICanvas найден
        if (mobileUI != null)
        {
            // Найти компонент кнопки nitroButton внутри MobileUICanvas
            Button nitroButton = Utility.findChild(mobileUI, "nitroButton").GetComponent<Button>();

            // Проверить, что кнопка найдена
            if (nitroButton != null)
            {
                // Добавим обработчики событий для кнопки "NitroButton"
                nitroButton.onClick.AddListener(OnNitroButtonClick);
                //nitroButton.onClick.AddListener(OnNitroButtonRelease);
            }
            else
            {
                Debug.LogError("Компонент кнопки nitroButton не найден внутри MobileUICanvas.");
            }
        }
        else
        {
            Debug.Log("Объект MobileUICanvas не найден.");
        }
    }
    private void OnNitroButtonClick()
    {
        // Обработчик нажатия на кнопку Nitro
        Debug.Log("Нажата кнопка Nitro");

        // Установить mNitroActive в true при нажатии кнопки
        mNitroActive = true;
    }

    private void OnNitroButtonRelease()
    {
        // Обработчик отпускания кнопки Nitro
        Debug.Log("Отпущена кнопка Nitro");

        // Установить mNitroActive в false при отпускании кнопки
        mNitroActive = false;
    }

    void createVisualEffects()
    {
        GameObject effects = GameObject.Find("Effects");
        if (effects == null)
        {
            GameObject seed = Resources.Load("Effects") as GameObject;
            if (seed != null)
            {
                effects = GameObject.Instantiate(seed);
                effects.name = "Effects";
            }
        }

        if (effects == null) return;

        //get skidmark objects
        GameObject smarks = Utility.findChild(effects, "Skidmarks");
        mSmarkFL = Utility.findChild(smarks, "m1").GetComponent<Skidmark>();
        mSmarkFR = Utility.findChild(smarks, "m2").GetComponent<Skidmark>();
        mSmarkRL = Utility.findChild(smarks, "m3").GetComponent<Skidmark>();
        mSmarkRR = Utility.findChild(smarks, "m4").GetComponent<Skidmark>();

        //create particle objects for eacj wheel
        GameObject particleObj = GameObject.Instantiate(Utility.findChild(effects, "Smoke1"));
        mSmokeFL = particleObj.GetComponent<ParticleSystem>();
        mSmokeFL.transform.parent = mFLW.getWheelTransform();
        mSmokeFL.transform.localPosition = Vector3.zero;
        mSmokeFL.Play();

        particleObj = GameObject.Instantiate(Utility.findChild(effects, "Smoke2"));
        mSmokeFR = particleObj.GetComponent<ParticleSystem>();
        mSmokeFR.transform.parent = mFRW.getWheelTransform();
        mSmokeFR.transform.localPosition = Vector3.zero;
        mSmokeFR.Play();

        particleObj = GameObject.Instantiate(Utility.findChild(effects, "Smoke3"));
        mSmokeRL = particleObj.GetComponent<ParticleSystem>();
        mSmokeRL.transform.parent = mRLW.getWheelTransform();
        mSmokeRL.transform.localPosition = Vector3.zero;
        mSmokeRL.Play();

        particleObj = GameObject.Instantiate(Utility.findChild(effects, "Smoke4"));
        mSmokeRR = particleObj.GetComponent<ParticleSystem>();
        mSmokeRR.transform.parent = mRRW.getWheelTransform();
        mSmokeRR.transform.localPosition = Vector3.zero;
        mSmokeRR.Play();

        //no smoke emission initially
        ParticleSystem.EmissionModule emission;
        emission = mSmokeFL.emission; emission.enabled = false;
        emission = mSmokeFR.emission; emission.enabled = false;
        emission = mSmokeRL.emission; emission.enabled = false;
        emission = mSmokeRR.emission; emission.enabled = false;

        //hide muzzles initially
        if (Muzzles != null) Muzzles.gameObject.SetActive(false);
    }

    void createSoundEffects()
    {
        if (EngineHighClip != null)
        {
            mEngineHigh = gameObject.AddComponent<AudioSource>();
            mEngineHigh.spatialBlend = 1f; mEngineHigh.dopplerLevel = 0f;
            mEngineHigh.volume = 0f; mEngineHigh.pitch = 1f;
            mEngineHigh.minDistance = 5f; mEngineHigh.maxDistance = 500f;
            mEngineHigh.clip = EngineHighClip; mEngineHigh.loop = true;
            mEngineHigh.Play();
        }

        if (EngineLowClip != null)
        {
            mEngineLow = gameObject.AddComponent<AudioSource>();
            mEngineLow.spatialBlend = 1f; mEngineLow.dopplerLevel = 0f;
            mEngineLow.volume = 0f; mEngineLow.pitch = 1f;
            mEngineLow.minDistance = 5f; mEngineLow.maxDistance = 500f;
            mEngineLow.clip = EngineLowClip; mEngineLow.loop = true;
            mEngineLow.Play();
        }

        for (int i = 0; i < SurfaceList.Length; i++)
        {
            SurfaceType st = SurfaceList[i];
            if (st.SkidClip != null)
            {
                st.SkidSound = gameObject.AddComponent<AudioSource>();
                st.SkidSound.spatialBlend = 1f; st.SkidSound.dopplerLevel = 0f;
                st.SkidSound.volume = BaseVolume * SkidVolume; st.SkidSound.pitch = 1f;
                st.SkidSound.minDistance = 5f; st.SkidSound.maxDistance = 500f;
                st.SkidSound.clip = st.SkidClip; st.SkidSound.loop = true;
            }
        }

        if (CrashClip != null)
        {
            mCrashSound = gameObject.AddComponent<AudioSource>();
            mCrashSound.spatialBlend = 1f; mCrashSound.dopplerLevel = 0f;
            mCrashSound.volume = BaseVolume; mCrashSound.pitch = 1f;
            mCrashSound.minDistance = 5f; mCrashSound.maxDistance = 500f;
            mCrashSound.clip = CrashClip; mCrashSound.loop = false;
        }

        if (MuzzleClip != null)
        {
            mMuzzleSound = gameObject.AddComponent<AudioSource>();
            mMuzzleSound.spatialBlend = 1f; mMuzzleSound.dopplerLevel = 1f;
            mMuzzleSound.minDistance = 5f; mMuzzleSound.maxDistance = 500f;
            mMuzzleSound.clip = MuzzleClip; mMuzzleSound.loop = false;
            mMuzzleSound.Stop();
        }

        if (NitroClip != null)
        {
            mNitroSound = gameObject.AddComponent<AudioSource>();
            mNitroSound.spatialBlend = 1f; mNitroSound.dopplerLevel = 0f;
            mNitroSound.volume = BaseVolume * NitroVolume; mNitroSound.pitch = 1.5f;
            mNitroSound.minDistance = 5f; mNitroSound.maxDistance = 500f;
            mNitroSound.clip = NitroClip; mNitroSound.loop = true;
            mNitroSound.Stop();
        }
    }

    public void setVolume(float f)
    {
        BaseVolume = f;

        for (int i = 0; i < SurfaceList.Length; i++)
        {
            SurfaceType st = SurfaceList[i];
            if (st.SkidSound != null) st.SkidSound.volume = BaseVolume * SkidVolume;
        }

        if (mNitroSound != null) mNitroSound.volume = BaseVolume * NitroVolume;
    }

    public void setShowUI(bool show)
    {
        ShowGripGUI = show;
    }

    void Update()
    {
        if (TerrainBasedSurface) terrainSurfaceUpdate();
        else physicsSurfaceUpdate();
        effectUpdate();
    }

    void FixedUpdate()
    {
        muzzleUpdate();
        nitroUpdate();
    }

    //check each wheels hit surface physics material
    //change wheel frciton acc. to surface friction value
    //change vehicle body drag acc. to surface roughness value
    //store surface id for each wheel to use in effectUpdate
    void physicsSurfaceUpdate()
    {
        Vehicle.getBody().drag = 0f;

        mFLW.setFrictionFactor(1.0f);
        mFRW.setFrictionFactor(1.0f);
        mRLW.setFrictionFactor(1.0f);
        mRRW.setFrictionFactor(1.0f);

        if (SurfaceList == null || SurfaceList.Length == 0) return;

        PhysicMaterial flwMat = null;
        PhysicMaterial frwMat = null;
        PhysicMaterial rlwMat = null;
        PhysicMaterial rrwMat = null;

        //get physic materials from hit objects
        RaycastHit hit = mFLW.getRayHit();
        if (hit.collider != null) flwMat = hit.collider.sharedMaterial;
        hit = mFRW.getRayHit();
        if (hit.collider != null) frwMat = hit.collider.sharedMaterial;
        hit = mRLW.getRayHit();
        if (hit.collider != null) rlwMat = hit.collider.sharedMaterial;
        hit = mRRW.getRayHit();
        if (hit.collider != null) rrwMat = hit.collider.sharedMaterial;

        //iterate surface list
        //if wheel mat is matched to a surface then set surface id, friction and drag values
        for (int j = 0; j < SurfaceList.Length; j++)
        {
            SurfaceType st = SurfaceList[j];
            if (st.Material == flwMat)
            {
                mFLSurface = j;
                mFLW.setFrictionFactor(st.Friction);
                Vehicle.getBody().drag = Mathf.Max(Vehicle.getBody().drag, st.Roughness);
                ParticleSystem.MainModule ma = mSmokeFL.main;
                ma.startColor = j == 1 ? mParticleSoilColor : mParticleDefaultColor;
            }
            if (st.Material == frwMat)
            {
                mFRSurface = j;
                mFRW.setFrictionFactor(st.Friction);
                Vehicle.getBody().drag = Mathf.Max(Vehicle.getBody().drag, st.Roughness);
                ParticleSystem.MainModule ma = mSmokeFR.main;
                ma.startColor = j == 1 ? mParticleSoilColor : mParticleDefaultColor;
            }
            if (st.Material == rlwMat)
            {
                mRLSurface = j;
                mRLW.setFrictionFactor(st.Friction);
                Vehicle.getBody().drag = Mathf.Max(Vehicle.getBody().drag, st.Roughness);
                ParticleSystem.MainModule ma = mSmokeRL.main;
                ma.startColor = j == 1 ? mParticleSoilColor : mParticleDefaultColor;
            }
            if (st.Material == rrwMat)
            {
                mRRSurface = j;
                mRRW.setFrictionFactor(st.Friction);
                Vehicle.getBody().drag = Mathf.Max(Vehicle.getBody().drag, st.Roughness);
                ParticleSystem.MainModule ma = mSmokeRR.main;
                ma.startColor = j == 1 ? mParticleSoilColor : mParticleDefaultColor;
            }
        }
    }

    float[] getTerrainTextureWeights(Wheel w)
    {
        RaycastHit hit = w.getRayHit();
        int alphaW = Terrain.activeTerrain.terrainData.alphamapWidth;
        int alphaH = Terrain.activeTerrain.terrainData.alphamapHeight;
        int tx = (int)(hit.textureCoord.x * alphaW);
        int ty = (int)(hit.textureCoord.y * alphaH);
        float[,,] maps = Terrain.activeTerrain.terrainData.GetAlphamaps(tx, ty, 1, 1);
        float[] ret = new float[maps.GetLength(2)];
        System.Buffer.BlockCopy(maps, 0, ret, 0, ret.Length * 4);
        return ret;
    }

    void terrainSurfaceUpdate()
    {
        Vehicle.getBody().drag = 0f;

        mFLW.setFrictionFactor(1.0f);
        mFRW.setFrictionFactor(1.0f);
        mRLW.setFrictionFactor(1.0f);
        mRRW.setFrictionFactor(1.0f);

        if (SurfaceList == null || SurfaceList.Length == 0) return;
        int surfaceCt = SurfaceList.Length;

        float[] flWeights = getTerrainTextureWeights(mFLW);
        float[] frWeights = getTerrainTextureWeights(mFRW);
        float[] rlWeights = getTerrainTextureWeights(mRLW);
        float[] rrWeights = getTerrainTextureWeights(mRRW);

        //skip if any wheel is out of terrain
        if (flWeights.Length != surfaceCt || flWeights.Length != surfaceCt ||
            flWeights.Length != surfaceCt || flWeights.Length != surfaceCt) return;

        float flFriction = 0f;
        float frFriction = 0f;
        float rlFriction = 0f;
        float rrFriction = 0f;

        for (int j = 0; j < surfaceCt; j++)
        {
            SurfaceType st = SurfaceList[j];
            flFriction += flWeights[j] * st.Friction;
            frFriction += frWeights[j] * st.Friction;
            rlFriction += rlWeights[j] * st.Friction;
            rrFriction += rrWeights[j] * st.Friction;
        }

        mFLW.setFrictionFactor(flFriction);
        mFRW.setFrictionFactor(frFriction);
        mRLW.setFrictionFactor(rlFriction);
        mRRW.setFrictionFactor(rrFriction);
    }

    //update skid effectss, engine sounds etc.
    void effectUpdate()
    {
        //longitudinal slips
        float lngSlipFL = Mathf.Abs(mFLW.getSlipRatio()) * Mathf.Min(1f, Mathf.Abs(mFLW.getLongitudinalSlip()));
        float lngSlipFR = Mathf.Abs(mFRW.getSlipRatio()) * Mathf.Min(1f, Mathf.Abs(mFRW.getLongitudinalSlip()));
        float lngSlipRL = Mathf.Abs(mRLW.getSlipRatio()) * Mathf.Min(1f, Mathf.Abs(mRLW.getLongitudinalSlip()));
        float lngSlipRR = Mathf.Abs(mRRW.getSlipRatio()) * Mathf.Min(1f, Mathf.Abs(mRRW.getLongitudinalSlip()));

        //lateral slips
        float latSlipFL = Mathf.Abs(mFLW.getSlipAngle()) * Mathf.Min(1f, Mathf.Abs(mFLW.getLateralSlip()));
        float latSlipFR = Mathf.Abs(mFRW.getSlipAngle()) * Mathf.Min(1f, Mathf.Abs(mFRW.getLateralSlip()));
        float latSlipRL = Mathf.Abs(mRLW.getSlipAngle()) * Mathf.Min(1f, Mathf.Abs(mRLW.getLateralSlip()));
        float latSlipRR = Mathf.Abs(mRRW.getSlipAngle()) * Mathf.Min(1f, Mathf.Abs(mRRW.getLateralSlip()));

        //skid marks
        if (ShowSkidmark)
        {
            //check conditions
            bool caseFL = mFLW.hasContact() && (lngSlipFL > LongSlipThreshold || latSlipFL > LatSlipThreshold);
            bool caseFR = mFRW.hasContact() && (lngSlipFR > LongSlipThreshold || latSlipFR > LatSlipThreshold);
            bool caseRL = mRLW.hasContact() && (lngSlipRL > LongSlipThreshold || latSlipRL > LatSlipThreshold);
            bool caseRR = mRRW.hasContact() && (lngSlipRR > LongSlipThreshold || latSlipRR > LatSlipThreshold);

            Vector3 add = mFLW.getRadius() * Vector3.down;
            if (caseFL) mSkidFL = mSmarkFL.AddSkidMark(mFLW.getWheelPosition() + add, Vector3.up, mFLW.getSuspensionCompressRatio(), mSkidFL); else mSkidFL = -1;
            if (caseFR) mSkidFR = mSmarkFR.AddSkidMark(mFRW.getWheelPosition() + add, Vector3.up, mFRW.getSuspensionCompressRatio(), mSkidFR); else mSkidFR = -1;
            if (caseRL) mSkidRL = mSmarkRL.AddSkidMark(mRLW.getWheelPosition() + add, Vector3.up, mRLW.getSuspensionCompressRatio(), mSkidRL); else mSkidRL = -1;
            if (caseRR) mSkidRR = mSmarkRR.AddSkidMark(mRRW.getWheelPosition() + add, Vector3.up, mRRW.getSuspensionCompressRatio(), mSkidRR); else mSkidRR = -1;
            if (mSkidFL != -1 || mSkidFR != -1 || mSkidRL != -1 || mSkidRR != -1) mSkidCount++;
            else mSkidCount = 0;
        }

        //show particle with skidmarks
        ParticleSystem.EmissionModule emission;
        emission = mSmokeFL.emission; emission.enabled = ShowTireSmoke && mSkidFL != -1;
        emission = mSmokeFR.emission; emission.enabled = ShowTireSmoke && mSkidFR != -1;
        emission = mSmokeRL.emission; emission.enabled = ShowTireSmoke && mSkidRL != -1;
        emission = mSmokeRR.emission; emission.enabled = ShowTireSmoke && mSkidRR != -1;

        //play skid sound acc. to max slip value and dominant surface id
        if (mSkidCount > 0 && SurfaceList.Length > 0)
        {
            //find surface index and play skid snd
            int surfaceIndex = Mathf.RoundToInt(0.25f * (mFLSurface + mFRSurface + mRLSurface + mRRSurface));
            SurfaceType st = SurfaceList[surfaceIndex];
            if (st.SkidSound != null && !st.SkidSound.isPlaying) st.SkidSound.Play();

            //set pitch values
            float[] vals = { lngSlipFL, lngSlipFR, lngSlipRL, lngSlipRR, latSlipFL, latSlipFR, latSlipRL, latSlipRR };
            float mslip = Mathf.Max(vals);
            for (int i = 0; i < SurfaceList.Length; i++)
            {
                SurfaceType ist = SurfaceList[i];
                if (ist.SkidSound != null && ist.SkidSound.isPlaying)
                {
                    ist.SkidSound.pitch = Mathf.Min(0.75f, Mathf.Max(0.4f, 0.1f + mslip));
                    //stop playing others
                    if (ist != st) ist.SkidSound.Pause();
                }
            }
        }
        //stop all surface snd
        else
        {
            for (int i = 0; i < SurfaceList.Length; i++)
            {
                SurfaceType st = SurfaceList[i];
                if (st.SkidSound != null) st.SkidSound.Stop();
            }
        }

        //set engine sound pitch/volme acc. to throttle & rpm
        if (mEngineHigh != null)
        {
            float throttle = Vehicle.getEngine().getThrottle();
            float vol = BaseVolume * EngineVolume * (0.75f + 0.25f * throttle);
            mEngineHigh.volume = Vehicle.getEngine().isRunning() ? vol : 0.0f;
            mEngineHigh.pitch = EngineBasePitch + (EnginePitchScale * Vehicle.getEngine().getRpmRatio());
        }
        if (mEngineLow != null)
        {
            float throttle = Vehicle.getEngine().getThrottle();
            float vol = BaseVolume * EngineVolume * (1f - 0.25f * throttle);
            mEngineLow.volume = Vehicle.getEngine().isRunning() ? vol : 0.0f;
            mEngineLow.pitch = EngineBasePitch + (EnginePitchScale * Vehicle.getEngine().getRpmRatio());
        }

        //brake lights effect
        if (mBrakeMaterials != null)
        {
            Color lampColor = Vehicle.getAxle(0).getLeftWheel().getBraking() > 0f ? BrakeLightColor : Color.black;
            for (int i = 0; i < mBrakeMaterials.Length; i++)
            {
                mBrakeMaterials[i].SetColor("_EmissionColor", lampColor);
            }
        }
    }

    //show muzzle object & play sound if the conditions are met
    void muzzleUpdate()
    {
        if (!Vehicle.getEngine().isRunning()) return;

        float rpm = Vehicle.getEngine().getRpmRatio();
        float delta = rpm - mLastRpm;
        if (delta == 0f) return;
        float speed = delta / Time.fixedDeltaTime;
        mLastRpm = rpm;

        bool trigger = speed < -MuzzleLimit;
        bool throttle = Vehicle.getEngine().getThrottle() < 0.9f;
        bool gear = Vehicle.getTransmission().isChanging() && Vehicle.getTransmission().getCurGear() != 0;
        bool limit = Vehicle.getEngine().isLimiterOn();
        trigger &= throttle;
        trigger |= gear;
        trigger |= limit;

        if (mLastTime <= 0f)
        {
            if (Muzzles != null)
            {
                Muzzles.gameObject.SetActive(trigger);
            }
            if (trigger && mMuzzleSound != null)
            {
                mMuzzleSound.volume = BaseVolume * MuzzleVolume * Random.Range(1f, 2f);
                mMuzzleSound.pitch = MuzzlePitch * Random.Range(1f, 2f);
                mMuzzleSound.Play();
            }
            mLastTime = Random.Range(0.75f * MuzzleInterval, 1.25f * MuzzleInterval);
        }
        else if (mLastTime > 0f) mLastTime -= Time.fixedDeltaTime;
    }

    public void nitroUpdate()
    {
        if (!NitroActive) return;

        float scale = 1f;
        mNitroActive |= Input.GetKeyDown(KeyCode.N);
        mNitroActive &= !Input.GetKeyUp(KeyCode.N);


        if (mNitroActive)
        {
            if (mCurrentNitro > 0f)
            {
                mCurrentNitro -= NitroConsumeSpeed * Time.fixedDeltaTime;
                if (mCurrentNitro <= 0f)
                {
                    mCurrentNitro = 0f;
                    mNitroActive = false;
                }
                scale = NitroPower;
            }
        }
        else if (mCurrentNitro < 1f)
        {
            mCurrentNitro += NitroRestoreSpeed * Time.fixedDeltaTime;
            if (mCurrentNitro >= 1f)
            {
                mCurrentNitro = 1f;
            }
        }

        if (Muzzles != null)
        {
            bool isActive = Muzzles.gameObject.activeSelf;
            if (!isActive) Muzzles.gameObject.SetActive(scale > 1f);
        }

        if (scale > 1f)
        {
            if (mNitroSound != null && !mNitroSound.isPlaying) mNitroSound.Play();
        }
        else if (mNitroSound != null && mNitroSound.isPlaying) mNitroSound.Stop();

        Vehicle.getEngine().setTorqueScale(scale);
    }

    //play crash sound upon impact
    void OnCollisionEnter(Collision collision)
    {
        if (mCrashSound == null) return;

        float magnitude = collision.relativeVelocity.magnitude;
        if (magnitude > 1f && !mCrashSound.isPlaying)
        {
            mCrashSound.volume = 0.1f * magnitude * BaseVolume * CrashVolume;
            mCrashSound.pitch = Random.Range(0.5f, 1.25f);
            mCrashSound.Play();
        }
    }

    void OnGUI()
    {
        if (ShowGripGUI) mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Grip");
    }

    void uiWindowFunction(int windowID)
    {
        GUI.Label(new Rect(10, 20, 100, 20), "FL-Wheel:%" + (int)(mFLW.getFrictionFactor() * 100));
        GUI.Label(new Rect(10, 50, 100, 20), "FR-Wheel:%" + (int)(mFRW.getFrictionFactor() * 100));
        GUI.Label(new Rect(10, 80, 100, 20), "RL-Wheel:%" + (int)(mRLW.getFrictionFactor() * 100));
        GUI.Label(new Rect(10, 110, 100, 20), "RR-Wheel:%" + (int)(mRRW.getFrictionFactor() * 100));
    }
}