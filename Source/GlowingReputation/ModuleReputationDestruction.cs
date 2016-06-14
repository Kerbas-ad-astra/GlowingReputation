using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GlowingReputation
{
    public class ModuleReputationDestruction:PartModule
    {
        [KSPField(isPersistant = false)]
        public bool SafeUntilFirstActivation = true;

        [KSPField(isPersistant = false)]
        public float BaseReputationHit = 15f;

        [KSPField(isPersistant = true)]
        public bool Unsafe = false;

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                GameEvents.onPartDestroyed.Add(new EventData<Part>.OnEvent(OnPartDestroyed));
            }
        }


        public void Update()
        {
            if (SafeUntilFirstActivation && Unsafe)
            {
                EvaluateSafety();
            }
        }

        public void OnDestroy()
        {
            GameEvents.onPartDestroyed.Remove(new EventData<Part>.OnEvent(OnPartDestroyed));
        }

        protected void OnPartDestroyed(Part p)
        {
            if (SafeUntilFirstActivation && !Unsafe)
              return;

            float repLoss = Utils.GetReputationScale(p.vessel.mainBody,
                Vector3.Distance(p.vessel.mainBody.position, p.partTransform.position) - p.vessel.mainBody.Radius) * BaseReputationHit;
            Reputation.Instance.AddReputation(repLoss, TransactionReasons.VesselLoss);
        }

        protected void EvaluateSafety()
        {
            // ModuleResourceConverter (ie fission reactor)
            ModuleResourceConverter converter = this.GetComponent<ModuleResourceConverter>();
            List<ModuleEnginesFX> engines = this.GetComponents<ModuleEnginesFX>();

            if (converter != null)
            {
              if (converter.ModuleIsActive())
              {
                Unsafe = true;
              }
            }

            // ModuleEnginesFX (ie nuclear engine)
            foreach (ModuleEnginesFX engine in engines)
            {
              if (!engine.Shutdown())
              {
                Usafe = true;
              }
            }
        }
    }
}
