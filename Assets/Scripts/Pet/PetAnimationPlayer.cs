using System.Collections;
using EnjoyJob.Battle.Skills;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EnjoyJob.Battle
{
    // 挂在亚比对象上，负责按 PetCtrl 里的动画路径播放 .anim。
    // 战斗流程只等待这个脚本播放完，不关心具体动画文件放在哪里。
    [RequireComponent(typeof(PetCtrl))]
    public sealed class PetAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float missingAnimationWaitSeconds = 0.15f;

        private PetCtrl petCtrl;
        private PlayableGraph currentGraph;

        private void Awake()
        {
            petCtrl = GetComponent<PetCtrl>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            PlayIdle();
        }

        private void OnDisable()
        {
            StopCurrentGraph();
        }

        public IEnumerator PlayAttackRoutine(SkillAttackType attackType)
        {
            PetAnimationPaths paths = GetAnimationPaths();
            string path = ChooseAttackPath(paths, attackType);
            yield return PlayOnceAndReturnIdleRoutine(path);
        }

        public IEnumerator PlayHurtRoutine()
        {
            PetAnimationPaths paths = GetAnimationPaths();
            yield return PlayOnceAndReturnIdleRoutine(paths != null ? paths.hurt : null);
        }

        public void PlayIdle()
        {
            PetAnimationPaths paths = GetAnimationPaths();
            PlayLoopAtPath(paths != null ? paths.idle : null);
        }

        private IEnumerator PlayOnceAndReturnIdleRoutine(string resourcePath)
        {
            if (animator == null)
            {
                yield return WaitMissingAnimation();
                PlayIdle();
                yield break;
            }

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                yield return WaitMissingAnimation();
                PlayIdle();
                yield break;
            }

            AnimationClip clip = Resources.Load<AnimationClip>(resourcePath);
            if (clip == null)
            {
                Debug.LogWarning($"Animation clip not found in Resources: {resourcePath}", this);
                yield return WaitMissingAnimation();
                PlayIdle();
                yield break;
            }

            StopCurrentGraph();
            PlayClip(clip);

            float duration = Mathf.Max(0f, clip.length);
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            StopCurrentGraph();
            PlayIdle();
        }

        private void PlayLoopAtPath(string resourcePath)
        {
            if (animator == null || string.IsNullOrWhiteSpace(resourcePath))
            {
                StopCurrentGraph();
                return;
            }

            AnimationClip clip = Resources.Load<AnimationClip>(resourcePath);
            if (clip == null)
            {
                Debug.LogWarning($"Idle animation clip not found in Resources: {resourcePath}", this);
                StopCurrentGraph();
                return;
            }

            StopCurrentGraph();
            PlayClip(clip);
        }

        private void PlayClip(AnimationClip clip)
        {
            currentGraph = PlayableGraph.Create($"{name}_{clip.name}");
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(currentGraph, "Animation", animator);
            AnimationClipPlayable playable = AnimationClipPlayable.Create(currentGraph, clip);
            playable.SetApplyFootIK(false);
            playable.SetApplyPlayableIK(false);
            output.SetSourcePlayable(playable);
            currentGraph.Play();
        }

        private PetAnimationPaths GetAnimationPaths()
        {
            if (petCtrl == null)
            {
                petCtrl = GetComponent<PetCtrl>();
            }

            return petCtrl != null ? petCtrl.AnimationPaths : null;
        }

        private static string ChooseAttackPath(PetAnimationPaths paths, SkillAttackType attackType)
        {
            if (paths == null)
            {
                return null;
            }

            switch (attackType)
            {
                case SkillAttackType.AttributeAttack:
                    return FirstNotEmpty(paths.attributeAttack1, paths.attributeAttack2, paths.normalAttack1, paths.specialAttack1);
                case SkillAttackType.SpecialAttack:
                    return FirstNotEmpty(paths.specialAttack1, paths.specialAttack2, paths.normalAttack1, paths.attributeAttack1);
                case SkillAttackType.NormalAttack:
                default:
                    return FirstNotEmpty(paths.normalAttack1, paths.normalAttack2, paths.specialAttack1, paths.attributeAttack1);
            }
        }

        private static string FirstNotEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private IEnumerator WaitMissingAnimation()
        {
            if (missingAnimationWaitSeconds > 0f)
            {
                yield return new WaitForSeconds(missingAnimationWaitSeconds);
            }
        }

        private void StopCurrentGraph()
        {
            if (currentGraph.IsValid())
            {
                currentGraph.Destroy();
            }
        }
    }
}
