using System;
using System.Collections.Generic;
using EnjoyJob.Battle;
using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    // 技能效果执行器。
    // 它根据 SkillEffectScrpit 里写的类名找到对应技能脚本，并调用 Execute。
    public sealed class SkillEffectExecutor
    {
        private readonly Dictionary<string, SkillEffectScript> effectCache = new Dictionary<string, SkillEffectScript>();

        public bool TryExecute(SkillDefinition skill, PetCtrl user, PetCtrl target)
        {
            if (skill == null || string.IsNullOrWhiteSpace(skill.SkillEffectScrpit))
            {
                return false;
            }

            SkillEffectScript effectScript = GetOrCreateEffect(skill.SkillEffectScrpit);
            if (effectScript == null)
            {
                Debug.LogError($"Skill effect script not found: {skill.SkillEffectScrpit}");
                return false;
            }

            int damage = BattleDamageCalculator.CalculateDamage(user, target, skill);
            effectScript.Execute(new SkillEffectContext(skill, user, target, damage));
            return true;
        }

        private SkillEffectScript GetOrCreateEffect(string scriptName)
        {
            if (effectCache.TryGetValue(scriptName, out SkillEffectScript cachedEffect))
            {
                return cachedEffect;
            }

            Type effectType = FindEffectType(scriptName);
            if (effectType == null || !typeof(SkillEffectScript).IsAssignableFrom(effectType))
            {
                return null;
            }

            SkillEffectScript effectScript = (SkillEffectScript)Activator.CreateInstance(effectType);
            effectCache.Add(scriptName, effectScript);
            return effectScript;
        }

        private Type FindEffectType(string scriptName)
        {
            foreach (Type type in typeof(SkillEffectScript).Assembly.GetTypes())
            {
                if (type.Name == scriptName)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
