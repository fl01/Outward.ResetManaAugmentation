using System.Collections;
using BepInEx;
using UnityEngine;

namespace Outward.FastGatherOutOfCombat
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ResetManaAugmentationMod : BaseUnityPlugin
    {
        public const string GUID = "fl01.reset-mana-augmentation";
        public const string NAME = "Reset mana augmentation";
        public const string VERSION = "1.0.0";
        public const int ItemId = -24000;

        internal void Awake()
        {
            base.StartCoroutine(ConfigurePotion());
        }

        private IEnumerator ConfigurePotion()
        {
            while (!ResourcesPrefabManager.Instance.Loaded)
            {
                yield return new WaitForSeconds(1f);
            }

            var item = ResourcesPrefabManager.Instance.GetItemPrefab(ItemId);
            if (item != null)
            {
                item.ClearEffects();
                item.AddEffect<AutoKnock>();
                item.AddEffect<CorruptionEffect>();
                item.AddEffect<ResetMana>();
            }
        }
    }

    public class ResetMana : Effect
    {
        public override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            _affectedCharacter.Stats.m_manaPoint = 0;
            _affectedCharacter.Stats.RefreshVitalMaxStat();
            _affectedCharacter.Stats.m_maxManaStat.Update();

            for (int i = 0; i < ItemManager.Instance.LearntSkillOnManaUnlock.Length; i++)
            {
                var skill = ItemManager.Instance.LearntSkillOnManaUnlock[i];
                _affectedCharacter.Inventory.SkillKnowledge.RemoveItem(skill.ItemID);
                ItemManager.Instance.DestroyItem(skill.UID);
                _affectedCharacter.Inventory.NotifyItemRemoved(skill, 1, false);
            }
        }
    }

    public class CorruptionEffect : Effect
    {
        public override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            _affectedCharacter.PlayerStats.AffectCorruptionLevel(1000f, false);
        }
    }

    public static class Extensions
    {
        public static T AddEffect<T>(this Item item) where T : Effect
        {
            return item.transform.Find("Effects").GetOrAddComponent<T>();
        }

        public static Item ClearEffects(this Item item)
        {
            Effect[] components = item.transform.Find("Effects").GetComponents<Effect>();
            for (int i = 0; i < components.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(components[i]);
            }
            return item;
        }
    }
}
