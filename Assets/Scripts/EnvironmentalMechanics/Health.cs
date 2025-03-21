using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    #region Inspector Properties
    [SerializeField]
    [Tooltip("The current health.")]
    public float Hitpoints = 10;

    [SerializeField]
    [Tooltip("The maximum hitpoints this can regenerate to.")]
    public float MaximumHitpoints = 10;

    [SerializeField]
    [Tooltip("Whether or not to set this object to inactive on death. See also: the 'Died' event in this class.")]
    public bool SetInactiveOnDeath = true;

    [SerializeField]
    [Tooltip("Whether or not health should regen passively.")]
    public bool PassivelyRegenerates = false;

    [SerializeField]
    [Tooltip("The amount of health to regenerate per second when passive regeneration is enabled.")]
    public float RegenerationRate = 1;

    // TODO add options for regeneration to be discrete or continuous (currently continuous)
    #endregion

    #region Events
    public event Action ReachedFullHealth;
    public event Action<float> TookDamage;
    public event Action Died;
    #endregion

    #region Computed Properties
    bool IsDead => Hitpoints <= 0;
    #endregion

    public void DoDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        Hitpoints = Mathf.Max(Hitpoints - damage, 0);
        TookDamage?.Invoke(damage);

        if (IsDead)
        {
            Died?.Invoke();
        }

        if (IsDead && SetInactiveOnDeath)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (PassivelyRegenerates)
        {
            float oldHp = Hitpoints;
            Hitpoints = Mathf.Min(MaximumHitpoints, Hitpoints + RegenerationRate * Time.deltaTime);

            if (oldHp < Hitpoints && Hitpoints >= MaximumHitpoints)
            {
                ReachedFullHealth?.Invoke();
            }
        }
    }
}
